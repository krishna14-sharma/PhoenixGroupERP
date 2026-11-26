Option Strict On
Option Explicit On
Option Infer On

Imports System.Data
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Printing
Imports System.Windows.Forms
Imports System.Linq

''' <summary>
''' Bank of India RTGS application ("MM RTGS" format: counterfoil on the left, application on the right).
''' The whole format - table, wording and values - is printed on plain A4 landscape paper.
''' Pick the bank voucher; the beneficiary's bank, branch, IFSC and account number come from
''' Party Bank Details (partyBankDetials). Our own details (account no., address, mobile, signatory,
''' BOI branch) are typed once and remembered on this PC.
''' </summary>
Public Class frmBOIRtgs

    ' ---- table size (mm) ----
    Private Const TableW As Single = 272.0F
    Private Const TableH As Single = 181.0F

    ' columns (mm from the table's left edge)
    Private Const LSplit As Single = 34.0F          ' counterfoil: label | value
    Private Const LRight As Single = 98.5F          ' counterfoil right edge
    Private Const GapMid As Single = 102.6F
    Private Const RLeft As Single = 106.8F          ' application left edge
    Private Const RSplit As Single = 171.0F         ' application: label | value

    Private _loading As Boolean
    Private _bankTable As DataTable
    Private _vouchers As DataTable
    Private _parties As DataTable
    Private _partyBanks As DataTable
    Private ReadOnly _doc As New PrintDocument()
    Private ReadOnly _settings As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _previewTimer As New Timer() With {.Interval = 300}

    Public Sub New()
        InitializeComponent()
        AddHandler _doc.PrintPage, AddressOf Doc_PrintPage
        AddHandler _previewTimer.Tick, AddressOf PreviewTimer_Tick
        Try
            _doc.DefaultPageSettings.Landscape = True
            _doc.DefaultPageSettings.Margins = New Margins(0, 0, 0, 0)
            For Each ps As PaperSize In _doc.PrinterSettings.PaperSizes
                If ps.Kind = PaperKind.A4 Then _doc.DefaultPageSettings.PaperSize = ps : Exit For
            Next
        Catch
            ' no printer installed
        End Try
    End Sub

    ' =====================================================================================
    ' Load
    ' =====================================================================================
    Private Sub frmBOIRtgs_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        _loading = True
        Try
            LoadSettings()
            dtpDate.Value = GMod.ServerNow().Date

            _bankTable = Db.GetTable("select account_code, account_head_name from " & GMod.ACC_HEAD &
                                     " where Group_name='BANK' order by account_head_name")
            cmbBank.DisplayMember = "account_head_name"
            cmbBank.ValueMember = "account_code"
            cmbBank.DataSource = _bankTable

            ' default: last bank used, else the first account that looks like Bank of India
            Dim lastBank As String = GetSetting("Bank", "")
            Dim pick As Integer = -1
            For i As Integer = 0 To _bankTable.Rows.Count - 1
                Dim code As String = _bankTable.Rows(i)("account_code").ToString()
                Dim nm As String = _bankTable.Rows(i)("account_head_name").ToString().ToUpperInvariant()
                If lastBank <> "" AndAlso code = lastBank Then pick = i : Exit For
                If pick < 0 AndAlso (nm.Contains("BANK OF INDIA") OrElse nm.Contains("BOI")) Then pick = i
            Next
            If pick >= 0 Then cmbBank.SelectedIndex = pick

            nudOffsetX.Value = ClampDec(GetSetting("OffsetX", "0"), nudOffsetX)
            nudOffsetY.Value = ClampDec(GetSetting("OffsetY", "0"), nudOffsetY)
            txtMobile.Text = GetSetting("Mobile", "")
            txtRemAddress.Text = GetSetting("Address", "").Replace("|", Environment.NewLine)
            txtSignatory.Text = GetSetting("Signatory", "")
            txtFrom.Text = GetSetting("From", GMod.Cmpname)
        Catch ex As Exception
            MessageBox.Show("Could not load bank accounts: " & ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            _loading = False
        End Try

        LoadParties()
        ApplyRemitterDefaults()
        LoadVouchers()
        RefreshPreview()
    End Sub

    Private Sub cmbBank_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbBank.SelectedIndexChanged
        If _loading Then Return
        ApplyRemitterDefaults()
        LoadVouchers()
    End Sub

    Private Function SelectedBankCode() As String
        If cmbBank.SelectedValue Is Nothing Then Return ""
        Return cmbBank.SelectedValue.ToString()
    End Function

    ''' <summary>
    ''' Our (debit) account for "As per details below by debiting my / our account":
    '''   1. Party Bank Details saved against the bank account head (partyCode = bank head code),
    '''   2. the number typed here before (remembered per bank account),
    '''   3. the account number written in the bank head's name, e.g. "BANK OF INDIA CC A/C 940520110000028".
    ''' </summary>
    Private Sub ApplyRemitterDefaults()
        Dim code As String = SelectedBankCode()
        txtRemitter.Text = GetSetting("Title_" & code, GMod.Cmpname)
        txtOurBranch.Text = GetSetting("Branch_" & code, "")
        Dim acNo As String = ""
        If code <> "" Then
            Try
                Dim t As DataTable = Db.GetTable("select accNumber, branch from partyBankDetials where partyCode=@p " &
                                                 "order by case when Cmp_id=@c then 0 else 1 end", Db.P("@p", code), Db.P("@c", GMod.Cmpid))
                If t.Rows.Count > 0 Then
                    acNo = t.Rows(0)("accNumber").ToString().Trim()
                    If txtOurBranch.Text = "" Then txtOurBranch.Text = t.Rows(0)("branch").ToString().Trim()
                End If
            Catch
            End Try
        End If
        If acNo = "" Then acNo = GetSetting("AcNo_" & code, "")
        If acNo = "" Then acNo = AccountNoFromName(cmbBank.Text)
        txtRemitterAc.Text = acNo
        If acNo = "" AndAlso code <> "" Then
            lblStatus.Text = "Enter our " & cmbBank.Text & " account number once in 'Our BOI A/c No.' - it will be remembered."
        End If
    End Sub

    ''' <summary>Longest run of 9+ digits in the account head name (bank heads often carry the A/c number).</summary>
    Private Shared Function AccountNoFromName(ByVal name As String) As String
        Dim best As String = ""
        For Each m As Global.System.Text.RegularExpressions.Match In Global.System.Text.RegularExpressions.Regex.Matches(If(name, ""), "\d{9,20}")
            If m.Value.Length > best.Length Then best = m.Value
        Next
        Return best
    End Function

    Private Sub btnRefresh_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnRefresh.Click
        LoadVouchers()
    End Sub

    Private Sub LoadVouchers()
        Dim code As String = SelectedBankCode()
        If code = "" Then
            dgVouchers.DataSource = Nothing
            Return
        End If
        Try
            Cursor = Cursors.WaitCursor
            _vouchers = Db.GetTable(
                "select vouno as [Vou No], Chq_date as [Date], Chq_no as [Chq No], amount as [Amount], favourof as [Favour Of], vou_type " &
                "from chq_issue where session=@s and Cmp_id=@c and code=@b " &
                "order by Chq_date desc, vouno desc",
                Db.P("@s", GMod.Session), Db.P("@c", GMod.Cmpid), Db.P("@b", code))
            dgVouchers.DataSource = _vouchers
            If dgVouchers.Columns.Contains("vou_type") Then dgVouchers.Columns("vou_type").Visible = False
            If dgVouchers.Columns.Contains("Amount") Then
                dgVouchers.Columns("Amount").DefaultCellStyle.Format = "N2"
                dgVouchers.Columns("Amount").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            End If
            If dgVouchers.Columns.Contains("Date") Then dgVouchers.Columns("Date").DefaultCellStyle.Format = "dd/MM/yyyy"
            If dgVouchers.Columns.Contains("Favour Of") Then dgVouchers.Columns("Favour Of").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            ApplyFilter()
            lblStatus.Text = _vouchers.Rows.Count.ToString() & " bank voucher(s) for " & cmbBank.Text
        Catch ex As Exception
            MessageBox.Show("Could not load vouchers: " & ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub txtSearch_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtSearch.TextChanged
        ApplyFilter()
    End Sub

    Private Sub ApplyFilter()
        If _vouchers Is Nothing Then Return
        Dim s As String = txtSearch.Text.Trim().Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]")
        If s = "" Then
            _vouchers.DefaultView.RowFilter = ""
        Else
            _vouchers.DefaultView.RowFilter = "Convert([Vou No], 'System.String') like '%" & s & "%' or Convert([Chq No], 'System.String') like '%" & s &
                                              "%' or [Favour Of] like '%" & s & "%' or Convert([Amount], 'System.String') like '%" & s & "%'"
        End If
    End Sub

    Private Sub dgVouchers_CellDoubleClick(ByVal sender As Object, ByVal e As DataGridViewCellEventArgs) Handles dgVouchers.CellDoubleClick
        If e.RowIndex >= 0 Then ShowVoucher()
    End Sub

    Private Sub dgVouchers_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles dgVouchers.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.Handled = True
            ShowVoucher()
        End If
    End Sub

    Private Sub btnShow_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnShow.Click
        ShowVoucher()
    End Sub

    ' =====================================================================================
    ' Fill from the selected voucher
    ' =====================================================================================
    Private Sub ShowVoucher()
        If dgVouchers.CurrentRow Is Nothing Then
            MessageBox.Show("Select a bank voucher first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Dim drv As DataRowView = TryCast(dgVouchers.CurrentRow.DataBoundItem, DataRowView)
        If drv Is Nothing Then Return

        Dim vouNo As String = drv("Vou No").ToString()
        Dim amount As Decimal = ToDec(drv("Amount"))
        Dim bankCode As String = SelectedBankCode()
        Dim vouType As String = drv("vou_type").ToString().Trim()
        If vouType = "" Then vouType = "BANK"

        Try
            Cursor = Cursors.WaitCursor
            ClearBeneficiary()
            txtAmount.Text = amount.ToString("0.00")
            txtChqNo.Text = drv("Chq No").ToString().Trim()
            If Not IsDBNull(drv("Date")) Then
                Dim d As DateTime
                If DateTime.TryParse(drv("Date").ToString(), d) Then dtpDate.Value = d.Date
            End If
            txtWords.Text = AmountInWords(amount)

            Dim party As String = FindVoucherParty(vouNo, vouType, bankCode, amount, drv("Favour Of").ToString().Trim())
            If party = "" Then
                txtName.Text = drv("Favour Of").ToString().Trim()
                lblStatus.Text = "Voucher " & vouNo & ": party not found - pick the Beneficiary Party to load its bank details."
            Else
                SelectParty(party)
                If txtBenAc.Text = "" Then
                    lblStatus.Text = "Voucher " & vouNo & ": no Party Bank Details for " & txtName.Text & " - add them in Party Account first."
                Else
                    lblStatus.Text = "Voucher " & vouNo & " loaded - check the details and press Print."
                End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            Cursor = Cursors.Default
        End Try
        RefreshPreview()
    End Sub

    ''' <summary>
    ''' Party paid by the voucher: its debit line in dummy_ventry (unposted) or the posted ledger table,
    ''' preferring the line whose amount equals the cheque; last resort the "favour of" name.
    ''' </summary>
    Private Function FindVoucherParty(ByVal vouNo As String, ByVal vouType As String, ByVal bankCode As String,
                                      ByVal amount As Decimal, ByVal favourOf As String) As String
        Dim queries As String() = {
            "select Acc_head_code from dummy_ventry where Vou_no=@v and Vou_type=@t and cmp_id=@c and session=@s and dramt > 0 and Acc_head_code<>@b order by case when dramt=@amt then 0 else 1 end",
            "select Acc_head_code from dummy_ventry where Vou_no=@v and cmp_id=@c and session=@s and dramt > 0 and Acc_head_code<>@b order by case when dramt=@amt then 0 else 1 end",
            "select Acc_head_code from " & GMod.VENTRY & " where convert(varchar(50), Vou_no)=@v and Vou_type=@t and dramt > 0 and Acc_head_code<>@b order by case when dramt=@amt then 0 else 1 end"}
        For Each q As String In queries
            Try
                Dim t As DataTable = Db.GetTable(q, Db.P("@v", vouNo), Db.P("@t", vouType), Db.P("@c", GMod.Cmpid), Db.P("@s", GMod.Session),
                                                 Db.P("@b", bankCode), Db.P("@amt", amount))
                If t.Rows.Count > 0 Then Return t.Rows(0)(0).ToString().Trim()
            Catch
                ' not available in this database - try the next source
            End Try
        Next
        If favourOf <> "" AndAlso _parties IsNot Nothing Then
            For Each r As DataRow In _parties.Rows
                If String.Equals(r("account_head_name").ToString().Trim(), favourOf, StringComparison.OrdinalIgnoreCase) Then Return r("account_code").ToString().Trim()
            Next
        End If
        Return ""
    End Function

    Private Sub ClearBeneficiary()
        For Each t As TextBox In New TextBox() {txtName, txtBenAc, txtIfsc, txtBenBank, txtBenBranch, txtChqNo, txtAmount, txtWords}
            t.Text = ""
        Next
        cmbParty.SelectedIndex = -1
        cmbPartyBank.DataSource = Nothing
    End Sub

    ' =====================================================================================
    ' Beneficiary party and its bank account (Party Bank Details)
    ' =====================================================================================
    Private Sub LoadParties()
        Try
            _parties = Db.GetTable("select account_code, account_head_name from " & GMod.ACC_HEAD &
                                   " where account_code in (select partyCode from partyBankDetials) or Group_name='PARTY' order by account_head_name")
            cmbParty.DisplayMember = "account_head_name"
            cmbParty.ValueMember = "account_code"
            cmbParty.DataSource = _parties
            cmbParty.SelectedIndex = -1
            cmbParty.AutoCompleteMode = AutoCompleteMode.SuggestAppend
            cmbParty.AutoCompleteSource = AutoCompleteSource.ListItems
        Catch ex As Exception
            lblStatus.Text = "Could not load parties: " & ex.Message
        End Try
    End Sub

    Private Sub SelectParty(ByVal code As String)
        If _parties IsNot Nothing Then
            For i As Integer = 0 To _parties.Rows.Count - 1
                If _parties.Rows(i)("account_code").ToString().Trim() = code Then
                    cmbParty.SelectedIndex = i
                    LoadPartyBanks(code, _parties.Rows(i)("account_head_name").ToString().Trim())
                    Return
                End If
            Next
        End If
        Dim nm As Object = Nothing
        Try
            nm = Db.Scalar("select account_head_name from " & GMod.ACC_HEAD & " where account_code=@p", Db.P("@p", code))
        Catch
        End Try
        LoadPartyBanks(code, If(nm Is Nothing OrElse IsDBNull(nm), "", nm.ToString().Trim()))
    End Sub

    Private Sub cmbParty_SelectionChangeCommitted(ByVal sender As Object, ByVal e As EventArgs) Handles cmbParty.SelectionChangeCommitted
        PartyPicked()
    End Sub

    Private Sub cmbParty_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles cmbParty.Leave
        Dim i As Integer = cmbParty.FindStringExact(cmbParty.Text)
        If i >= 0 AndAlso i <> cmbParty.SelectedIndex Then
            cmbParty.SelectedIndex = i
            PartyPicked()
        End If
    End Sub

    Private Sub PartyPicked()
        Dim drv As DataRowView = TryCast(cmbParty.SelectedItem, DataRowView)
        If drv Is Nothing Then Return
        LoadPartyBanks(drv("account_code").ToString().Trim(), drv("account_head_name").ToString().Trim())
    End Sub

    ''' <summary>Loads the party's account(s) from partyBankDetials; a party may have more than one.</summary>
    Private Sub LoadPartyBanks(ByVal code As String, ByVal partyName As String)
        If partyName <> "" Then txtName.Text = partyName
        Try
            _partyBanks = Db.GetTable("select bankName, accNumber, ifscCode, branch, " &
                                      "isnull(bankName,'') + '  -  ' + isnull(accNumber,'') as display " &
                                      "from partyBankDetials where partyCode=@p order by case when Cmp_id=@c then 0 else 1 end",
                                      Db.P("@p", code), Db.P("@c", GMod.Cmpid))
        Catch ex As Exception
            _partyBanks = Nothing
            lblStatus.Text = "Could not read Party Bank Details: " & ex.Message
        End Try
        cmbPartyBank.DataSource = Nothing
        If _partyBanks Is Nothing OrElse _partyBanks.Rows.Count = 0 Then
            For Each t As TextBox In New TextBox() {txtBenAc, txtIfsc, txtBenBank, txtBenBranch}
                t.Text = ""
            Next
            lblStatus.Text = "No Party Bank Details for " & txtName.Text & " - add them in Party Account."
            RefreshPreview()
            Return
        End If
        cmbPartyBank.DisplayMember = "display"
        cmbPartyBank.DataSource = _partyBanks
        cmbPartyBank.SelectedIndex = 0
        ApplyPartyBank()
    End Sub

    Private Sub cmbPartyBank_SelectionChangeCommitted(ByVal sender As Object, ByVal e As EventArgs) Handles cmbPartyBank.SelectionChangeCommitted
        ApplyPartyBank()
    End Sub

    Private Sub ApplyPartyBank()
        Dim drv As DataRowView = TryCast(cmbPartyBank.SelectedItem, DataRowView)
        If drv Is Nothing Then Return
        txtBenBank.Text = drv("bankName").ToString().Trim()
        txtBenAc.Text = drv("accNumber").ToString().Trim()
        txtIfsc.Text = drv("ifscCode").ToString().Trim().ToUpperInvariant()
        txtBenBranch.Text = drv("branch").ToString().Trim()
        RefreshPreview()
    End Sub

    ' =====================================================================================
    ' Amount helpers
    ' =====================================================================================
    Private Sub txtAmount_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles txtAmount.Leave
        Dim a As Decimal
        If Decimal.TryParse(txtAmount.Text, a) Then
            txtAmount.Text = a.ToString("0.00")
            txtWords.Text = AmountInWords(a)
        End If
    End Sub

    ''' <summary>"Rupees Seventy Five Lakh Only" using the same converter as the HDFC slip.</summary>
    Private Shared Function AmountInWords(ByVal amount As Decimal) As String
        If amount <= 0 Then Return ""
        Dim rupees As Decimal = Decimal.Truncate(amount)
        Dim paise As Integer = CInt(Math.Round((amount - rupees) * 100D))
        Dim cls As New Module1.Num2Words
        Dim s As String = cls.NUMMM(rupees.ToString("0")).Trim()
        If paise > 0 Then s &= " and " & cls.NUMMM(paise.ToString()).Trim() & " Paise"
        s = System.Text.RegularExpressions.Regex.Replace(s, "\s+", " ").Trim()
        If Not s.StartsWith("Rupees", StringComparison.OrdinalIgnoreCase) Then s = "Rupees " & s
        Return s & " Only"
    End Function

    ''' <summary>Indian grouping: 7500000 -> 75,00,000.00</summary>
    Private Shared Function IndianAmount(ByVal a As Decimal) As String
        Dim s As String = Math.Abs(a).ToString("0.00", Global.System.Globalization.CultureInfo.InvariantCulture)
        Dim intPart As String = s.Substring(0, s.Length - 3), dec As String = s.Substring(s.Length - 3)
        If intPart.Length > 3 Then
            Dim last3 As String = intPart.Substring(intPart.Length - 3)
            Dim rest As String = intPart.Substring(0, intPart.Length - 3)
            Dim parts As New List(Of String)
            While rest.Length > 2
                parts.Insert(0, rest.Substring(rest.Length - 2))
                rest = rest.Substring(0, rest.Length - 2)
            End While
            If rest.Length > 0 Then parts.Insert(0, rest)
            intPart = String.Join(",", parts.ToArray()) & "," & last3
        End If
        Return If(a < 0, "-", "") & intPart & dec
    End Function

    ''' <summary>IFSC printed as on the bank's sample: "HDFC 0001282".</summary>
    Private Shared Function IfscDisplay(ByVal s As String) As String
        s = If(s, "").Replace(" ", "").Trim().ToUpperInvariant()
        If s.Length = 11 Then Return s.Substring(0, 4) & " " & s.Substring(4)
        Return s
    End Function

    ' =====================================================================================
    ' Preview / print
    ' =====================================================================================
    Private Sub AnyField_Changed(ByVal sender As Object, ByVal e As EventArgs) Handles _
        txtName.TextChanged, txtBenAc.TextChanged, txtIfsc.TextChanged, txtBenBank.TextChanged, txtBenBranch.TextChanged,
        txtChqNo.TextChanged, txtAmount.TextChanged, txtWords.TextChanged, txtRemitter.TextChanged, txtRemitterAc.TextChanged,
        txtMobile.TextChanged, txtRemAddress.TextChanged, txtSignatory.TextChanged, txtOurBranch.TextChanged, txtFrom.TextChanged,
        dtpDate.ValueChanged, nudOffsetX.ValueChanged, nudOffsetY.ValueChanged
        If _loading Then Return
        _previewTimer.Stop()
        _previewTimer.Start()
    End Sub

    Private Sub PreviewTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        _previewTimer.Stop()
        RefreshPreview()
    End Sub

    Private Sub RefreshPreview()
        picPreview.Invalidate()
    End Sub

    Private Sub picPreview_Resize(ByVal sender As Object, ByVal e As EventArgs) Handles picPreview.Resize
        picPreview.Invalidate()
    End Sub

    ''' <summary>On-screen preview of the A4 landscape page (needs no printer).</summary>
    Private Sub picPreview_Paint(ByVal sender As Object, ByVal e As PaintEventArgs) Handles picPreview.Paint
        Dim g As Graphics = e.Graphics
        Dim area As Rectangle = picPreview.ClientRectangle
        area.Inflate(-14, -14)
        If area.Width < 50 OrElse area.Height < 50 Then Return
        Const pageW As Single = 297.0F, pageH As Single = 210.0F
        Dim k As Single = Math.Min(area.Width / pageW, area.Height / pageH)
        Dim w As Single = pageW * k, h As Single = pageH * k
        Dim x0 As Single = area.Left + (area.Width - w) / 2.0F, y0 As Single = area.Top + (area.Height - h) / 2.0F
        Using shadow As New SolidBrush(Color.FromArgb(60, 0, 0, 0))
            g.FillRectangle(shadow, x0 + 4, y0 + 4, w, h)
        End Using
        g.FillRectangle(Brushes.White, x0, y0, w, h)
        RenderTo(g, k, x0 + (pageW - TableW) / 2.0F * k, y0 + (pageH - TableH) / 2.0F * k)
    End Sub

    ''' <summary>Draws the form in pixel units: k = pixels per mm, (x0, y0) = the table's top-left corner.</summary>
    Friend Sub RenderTo(ByVal g As Graphics, ByVal k As Single, ByVal x0 As Single, ByVal y0 As Single)
        g.PageUnit = GraphicsUnit.Pixel
        _k = k : _ox = x0 : _oy = y0 : _fontUnit = GraphicsUnit.Pixel
        RenderForm(g)
    End Sub

    Private Sub btnPrint_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnPrint.Click
        If Not ReadyToPrint() Then Return
        Using dlg As New PrintDialog()
            dlg.Document = _doc
            dlg.UseEXDialog = True
            If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
        End Using
        SaveSettings()
        Try
            _doc.DocumentName = "BOI RTGS " & txtName.Text
            _doc.Print()
            lblStatus.Text = "Sent to printer: " & txtName.Text & "  Rs. " & txtAmount.Text
        Catch ex As Exception
            MessageBox.Show("Printing failed: " & ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub btnPreview_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnPreview.Click
        If Not ReadyToPrint() Then Return
        Using dlg As New PrintPreviewDialog()
            dlg.Document = _doc
            dlg.WindowState = FormWindowState.Maximized
            dlg.ShowDialog(Me)
        End Using
    End Sub

    Private Function ReadyToPrint() As Boolean
        Dim missing As New List(Of String)
        If txtName.Text.Trim() = "" Then missing.Add("Beneficiary name")
        If txtBenAc.Text.Trim() = "" Then missing.Add("Beneficiary A/c No.")
        If txtIfsc.Text.Trim() = "" Then missing.Add("IFSC code")
        Dim a As Decimal
        If Not Decimal.TryParse(txtAmount.Text, a) OrElse a <= 0 Then missing.Add("Amount")
        If missing.Count > 0 Then
            MessageBox.Show("Please fill: " & String.Join(", ", missing.ToArray()), Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        If txtIfsc.Text.Replace(" ", "").Trim().Length <> 11 Then
            If MessageBox.Show("IFSC code should be 11 characters. Print anyway?", Text,
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return False
        End If
        Return True
    End Function

    Private Sub btnClose_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnClose.Click
        Close()
    End Sub

    Private Sub frmBOIRtgs_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles Me.FormClosing
        SaveSettings()
        _previewTimer.Stop()
    End Sub

    ' ---------------------------------- drawing ----------------------------------------
    ' All positions are in mm from the table's top-left corner and go through X()/Y(), so the same
    ' code serves the printer (millimetres) and the on-screen preview (pixels).
    Private _k As Single = 1.0F
    Private _ox As Single, _oy As Single
    Private _fontUnit As GraphicsUnit = GraphicsUnit.Millimeter

    Private Function X(ByVal mm As Single) As Single
        Return _ox + mm * _k
    End Function

    Private Function Y(ByVal mm As Single) As Single
        Return _oy + mm * _k
    End Function

    Private Function MakeFont(ByVal family As String, ByVal pt As Single, ByVal style As FontStyle) As Font
        Return New Font(family, Math.Max(0.5F, pt * 25.4F / 72.0F * _k), style, _fontUnit)
    End Function

    Private Sub Doc_PrintPage(ByVal sender As Object, ByVal e As PrintPageEventArgs)
        Dim g As Graphics = e.Graphics
        g.PageUnit = GraphicsUnit.Millimeter
        _fontUnit = GraphicsUnit.Millimeter
        ' centre the table on the paper (origin of the printer graphics = printable area)
        Dim pageW As Single = e.PageBounds.Width * 0.254F, pageH As Single = e.PageBounds.Height * 0.254F
        Dim hmX As Single = e.PageSettings.HardMarginX * 0.254F, hmY As Single = e.PageSettings.HardMarginY * 0.254F
        _k = Math.Min(1.0F, Math.Min((pageW - 16.0F) / TableW, (pageH - 16.0F) / TableH))
        If _k <= 0.1F Then _k = 1.0F
        _ox = (pageW - TableW * _k) / 2.0F - hmX + CSng(nudOffsetX.Value)
        _oy = (pageH - TableH * _k) / 2.0F - hmY + CSng(nudOffsetY.Value)
        RenderForm(g)
        e.HasMorePages = False
    End Sub

    ''' <summary>The complete RTGS format: table, wording and values.</summary>
    Private Sub RenderForm(ByVal g As Graphics)
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = Global.System.Drawing.Text.TextRenderingHint.AntiAliasGridFit

        Const serif As String = "Times New Roman"
        Dim dateDots As String = dtpDate.Value.ToString("dd.MM.yyyy")
        Dim amt As Decimal
        Decimal.TryParse(txtAmount.Text, amt)
        Dim amtText As String = If(amt > 0, IndianAmount(amt), "")
        Dim ifsc As String = IfscDisplay(txtIfsc.Text)
        Dim benName As String = txtName.Text.Trim().ToUpperInvariant()
        Dim benBank As String = txtBenBank.Text.Trim().ToUpperInvariant()
        Dim benBranch As String = txtBenBranch.Text.Trim().ToUpperInvariant()
        Dim benAc As String = txtBenAc.Text.Trim()
        Dim words As String = txtWords.Text.Trim()
        Dim chq As String = txtChqNo.Text.Trim()
        Dim applicant As String = txtRemitter.Text.Trim().ToUpperInvariant()

        Using pen As New Pen(Color.Black, 0.3F * _k),
              lbl As Font = MakeFont(serif, 11.0F, FontStyle.Regular),
              lblB As Font = MakeFont(serif, 11.0F, FontStyle.Bold),
              val As Font = MakeFont(serif, 11.5F, FontStyle.Bold),
              valR As Font = MakeFont(serif, 11.0F, FontStyle.Regular),
              big As Font = MakeFont(serif, 13.0F, FontStyle.Bold),
              italic As Font = MakeFont(serif, 11.5F, FontStyle.Bold Or FontStyle.Italic),
              head As Font = MakeFont("Arial", 14.0F, FontStyle.Bold),
              sans As Font = MakeFont("Arial", 9.5F, FontStyle.Regular),
              sansB As Font = MakeFont("Arial", 9.5F, FontStyle.Bold),
              shade As New SolidBrush(Color.FromArgb(205, 205, 205)),
              shadeDark As New SolidBrush(Color.FromArgb(170, 170, 170)),
              ink As New SolidBrush(Color.Black)

            Dim Hl = Sub(x1 As Single, x2 As Single, y1 As Single)
                         g.DrawLine(pen, X(x1), Y(y1), X(x2), Y(y1))
                     End Sub
            Dim Vl = Sub(x1 As Single, y1 As Single, y2 As Single)
                         g.DrawLine(pen, X(x1), Y(y1), X(x1), Y(y2))
                     End Sub
            Dim Fill = Sub(b As Brush, x1 As Single, y1 As Single, x2 As Single, y2 As Single)
                           g.FillRectangle(b, X(x1), Y(y1), X(x2) - X(x1), Y(y2) - Y(y1))
                       End Sub
            ' text in a cell: align 0 left / 1 centre / 2 right; top=True puts it at the top of the cell
            Dim Cell = Sub(s As String, f As Font, x1 As Single, y1 As Single, x2 As Single, y2 As Single, align As Integer, top As Boolean)
                           DrawCell(g, s, f, ink, x1, y1, x2, y2, align, top)
                       End Sub

            ' ---------------- shading (drawn first, lines on top) ----------------
            Fill(shade, LSplit, 111.5F, LRight, 117.5F)                 ' counterfoil cheque no.
            Fill(shadeDark, LSplit, 122.4F, LRight, 124.4F)             ' counterfoil amount underline band
            Fill(shade, RSplit, 15.4F, TableW, 21.1F)                   ' remit the sum of
            Fill(shade, RSplit, 111.3F, TableW, 116.0F)                 ' cheque no.
            Fill(shadeDark, RSplit, 116.0F, TableW, 122.3F)             ' amount

            ' ---------------- frame ----------------
            g.DrawRectangle(pen, X(0), Y(0), X(LRight) - X(0), Y(TableH) - Y(0))
            g.DrawRectangle(pen, X(RLeft), Y(0), X(TableW) - X(RLeft), Y(TableH) - Y(0))
            Vl(GapMid, 0, TableH)

            ' ================= counterfoil (left) =================
            Dim lRows As Single() = {10.0F, 17.7F, 23.3F, 40.8F, 54.7F, 64.5F, 76.3F, 93.2F, 100.3F, 111.5F, 117.5F, 122.4F, 124.4F, 149.0F, 154.3F, 160.0F, 165.2F, 172.4F}
            For Each yy As Single In lRows
                Hl(0, LRight, yy)
            Next
            Hl(LRight, GapMid, 17.7F) : Hl(LRight, GapMid, 122.4F) : Hl(LRight, GapMid, 124.4F)
            Vl(LSplit, 17.7F, 23.3F)
            Vl(LSplit, 54.7F, 149.0F)
            Vl(LSplit, 154.3F, TableH)

            Cell("BANK OF INDIA", head, 1.5F, 0, LRight, 10.0F, 0, False)
            Cell(txtOurBranch.Text.Trim(), lblB, 1.5F, 10.0F, LRight, 17.7F, 0, False)
            Cell("DATE:-", lbl, 0, 17.7F, LSplit, 23.3F, 1, False)
            Cell(dateDots, sans, LSplit, 17.7F, LRight, 23.3F, 1, False)
            DrawFrom(g, ink, lbl, big, txtFrom.Text.Trim(), 0, 23.3F, LRight, 40.8F)
            Cell("By  Cheque /Transfer/Cash for RTGS", italic, 0, 40.8F, LRight, 47.0F, 1, False)
            Cell("On Bank", lbl, 0.8F, 54.7F, LSplit, 64.5F, 0, True)
            Cell(benBank, val, LSplit + 1, 54.7F, LRight, 64.5F, 0, True)
            Cell("Branch", lbl, 0.8F, 64.5F, LSplit, 76.3F, 0, True)
            Cell(benBranch, valR, LSplit + 1, 64.5F, LRight, 76.3F, 0, True)
            Cell("In Favour Of:-", lbl, 0.8F, 76.3F, LSplit, 93.2F, 0, True)
            Cell(benName, val, LSplit + 1, 76.3F, LRight, 93.2F, 0, True)
            Cell("IFSC Code", lbl, 0.8F, 93.2F, LSplit, 100.3F, 0, False)
            Cell(ifsc, val, LSplit + 1, 93.2F, LRight, 100.3F, 0, False)
            Cell("Account Number:-", lbl, 0.8F, 100.3F, LSplit, 111.5F, 0, False)
            Cell(benAc, val, LSplit + 0.5F, 100.3F, LRight, 111.5F, 0, False)
            Cell("CHEQUE NO.", lbl, 0.8F, 111.5F, LSplit, 117.5F, 0, False)
            Cell(chq, val, LSplit + 1, 111.5F, LRight, 117.5F, 0, False)
            Cell("Amount Rs.:", lbl, 0, 117.5F, LSplit - 0.5F, 122.4F, 2, False)
            Cell(amtText, val, LSplit, 117.5F, LRight - 0.5F, 122.4F, 2, False)
            Cell("Rupees In Words :-", lbl, 0.8F, 124.4F, LSplit, 131.0F, 0, True)
            Cell(words, val, LSplit + 0.5F, 124.4F, LRight, 149.0F, 0, True)

            ' ================= application (right) =================
            Dim rRows As Single() = {9.0F, 15.4F, 21.1F, 32.5F, 38.5F, 53.6F, 64.2F, 75.5F, 92.1F, 99.4F, 111.3F, 116.0F, 122.3F, 124.1F, 129.9F, 140.7F, 146.5F, 151.8F, 157.8F}
            For Each yy As Single In rRows
                Hl(RLeft, TableW, yy)
            Next
            Hl(RLeft, TableW, 163.8F) : Hl(RLeft, TableW, 170.6F)
            Hl(GapMid, RLeft, 32.5F) : Hl(GapMid, RLeft, 122.3F) : Hl(GapMid, RLeft, 124.1F) : Hl(GapMid, RLeft, 129.9F)
            Hl(GapMid, RLeft, 140.7F) : Hl(GapMid, RLeft, 146.5F)
            Vl(RSplit, 0, TableH)

            Cell("BANK OF INDIA", head, RLeft + 1, 0, RSplit, 9.0F, 0, False)
            Cell("DATE:- " & dateDots, lblB, RSplit, 0, TableW, 9.0F, 1, False)
            Cell(txtOurBranch.Text.Trim(), lblB, RLeft + 1, 9.0F, RSplit, 15.4F, 0, False)
            Cell("Please remit the sum of  Rs.:", lbl, RLeft + 1, 15.4F, RSplit, 21.1F, 0, False)
            Cell(amtText, val, RSplit + 2, 15.4F, TableW, 21.1F, 0, False)
            Cell("Rupees In Words :-", lblB, RLeft + 1, 21.1F, RSplit, 32.5F, 0, True)
            Cell(words, valR, RSplit + 1.5F, 21.1F, TableW - 1, 32.5F, 0, True)
            Cell("As per details below by debiting my /", lbl, RLeft + 1, 32.5F, RSplit, 38.5F, 0, False)
            Cell(txtRemitterAc.Text.Trim(), val, RSplit, 32.5F, TableW, 38.5F, 1, False)
            Cell("Name of the beneficiary", lbl, RLeft + 1, 38.5F, RSplit, 53.6F, 0, True)
            Cell(benName, big, RSplit + 1, 38.5F, TableW - 1, 53.6F, 0, True)
            Cell("Destination Bank's name &", lbl, RLeft + 1, 53.6F, RSplit, 64.2F, 0, True)
            Cell(benBank, val, RSplit + 1, 53.6F, TableW - 1, 64.2F, 0, False)
            Cell("BRANCH.", lbl, RLeft + 1, 64.2F, RSplit, 75.5F, 0, True)
            Cell(benBranch, val, RSplit + 1, 64.2F, TableW - 1, 75.5F, 0, False)
            Cell("IFSC Code", lbl, RLeft + 1, 75.5F, RSplit, 92.1F, 0, False)
            Cell(ifsc, val, RSplit + 1, 75.5F, TableW - 1, 92.1F, 0, False)
            Cell("Account No.", lbl, RLeft + 1, 92.1F, RSplit, 99.4F, 0, False)
            Cell(benAc, val, RSplit + 1, 92.1F, TableW - 1, 99.4F, 0, False)
            Cell("Amount (in words)", lbl, RLeft + 1, 99.4F, RSplit, 111.3F, 0, False)
            Cell(words, val, RSplit + 1, 99.4F, TableW - 1, 111.3F, 0, False)
            Cell("CHEQUE NO.", lblB, RLeft + 1, 111.3F, RSplit, 116.0F, 0, False)
            Cell(chq, val, RSplit + 1, 111.3F, TableW - 1, 116.0F, 0, False)
            Cell("Amount Rs.:", lbl, RLeft, 116.0F, RSplit - 0.8F, 122.3F, 2, False)
            Cell(amtText, big, RSplit + 2, 116.0F, TableW - 1, 122.3F, 0, False)

            Cell("Name of Applicant   :", lbl, RLeft + 1, 124.1F, RSplit, 129.9F, 0, False)
            Cell(applicant, val, RSplit + 1, 124.1F, TableW - 1, 129.9F, 0, False)
            Cell("Address  :", lbl, RLeft + 1, 129.9F, RSplit, 140.7F, 0, True)
            Cell(txtRemAddress.Text.Trim().ToUpperInvariant(), val, RSplit + 1, 129.9F, TableW - 1, 140.7F, 0, True)
            Cell("Tel/Mob. No            :", lbl, RLeft + 1, 140.7F, RSplit, 146.5F, 0, False)
            If txtMobile.Text.Trim() <> "" Then
                Using ul As Font = MakeFont(serif, 11.5F, FontStyle.Underline)
                    Cell(txtMobile.Text.Trim(), ul, RSplit + 1, 140.7F, TableW - 1, 146.5F, 0, False)
                End Using
            End If
            Cell("Date of transfer/cash  ........................", sans, RLeft + 1, 146.5F, RSplit, 151.8F, 0, False)
            Cell("Amount Rs : " & If(amt > 0, amtText & "/-", ""), sansB, RLeft + 2, 151.8F, RSplit, 157.8F, 0, False)
            Cell("Scroll No.  ............................", sans, RLeft + 1, 157.8F, RSplit, 163.8F, 0, False)
            Cell("UTR No.   ............................", sans, RLeft + 1, 163.8F, RSplit, 170.6F, 0, False)

            Cell(applicant, val, RSplit, 157.8F, TableW, 163.8F, 1, False)
            Cell(txtSignatory.Text.Trim().ToUpperInvariant(), valR, RSplit, 163.8F, TableW, 170.6F, 1, False)
            Cell(If(txtMobile.Text.Trim() = "", "", "MOBILE NO. " & txtMobile.Text.Trim()), valR, RSplit, 170.6F, TableW, TableH, 1, False)
        End Using
    End Sub

    ''' <summary>"From ......Phoenix Poultry, Jabalpur" - the remitter in a larger bold face.</summary>
    Private Sub DrawFrom(ByVal g As Graphics, ByVal ink As Brush, ByVal lbl As Font, ByVal big As Font, ByVal who As String,
                         ByVal x1 As Single, ByVal y1 As Single, ByVal x2 As Single, ByVal y2 As Single)
        Const prefix As String = "From ......"
        Dim w1 As Single = g.MeasureString(prefix, lbl).Width
        Dim w2 As Single = If(who = "", 0.0F, g.MeasureString(who, big).Width)
        Dim avail As Single = X(x2) - X(x1) - 4.0F * _k
        Dim f As Font = big
        Try
            If w1 + w2 > avail AndAlso w2 > 0 Then
                f = New Font(big.FontFamily, big.Size * Math.Max(0.55F, (avail - w1) / w2), big.Style, big.Unit)
                w2 = g.MeasureString(who, f).Width
            End If
            Dim left As Single = X(x1) + (X(x2) - X(x1) - w1 - w2) / 2.0F
            Dim cy As Single = (Y(y1) + Y(y2)) / 2.0F
            Dim h1 As Single = g.MeasureString(prefix, lbl).Height
            g.DrawString(prefix, lbl, ink, left, cy - h1 / 2.0F)
            If who <> "" Then
                Dim h2 As Single = g.MeasureString(who, f).Height
                g.DrawString(who, f, ink, left + w1, cy - h2 / 2.0F)
            End If
        Finally
            If f IsNot big Then f.Dispose()
        End Try
    End Sub

    ''' <summary>Word-wrapped text inside a cell, shrunk if it does not fit.</summary>
    Private Sub DrawCell(ByVal g As Graphics, ByVal s As String, ByVal f As Font, ByVal b As Brush,
                         ByVal x1 As Single, ByVal y1 As Single, ByVal x2 As Single, ByVal y2 As Single,
                         ByVal align As Integer, ByVal top As Boolean)
        s = If(s, "").Trim()
        If s = "" Then Return
        Dim pad As Single = 0.6F * _k
        Dim r As New RectangleF(X(x1) + pad, Y(y1) + pad * 0.5F, X(x2) - X(x1) - 2 * pad, Y(y2) - Y(y1) - pad)
        If r.Width <= 1 OrElse r.Height <= 1 Then Return
        Using sf As New StringFormat()
            sf.Alignment = If(align = 1, StringAlignment.Center, If(align = 2, StringAlignment.Far, StringAlignment.Near))
            sf.LineAlignment = If(top, StringAlignment.Near, StringAlignment.Center)
            sf.Trimming = StringTrimming.None
            Dim useFont As Font = f
            Dim size As Single = f.Size
            Try
                While size > f.Size * 0.55F
                    Dim m As SizeF = g.MeasureString(s, useFont, CInt(Math.Max(1, r.Width)))
                    Dim oneLine As Boolean = g.MeasureString(s, useFont).Width <= r.Width
                    If m.Height <= r.Height + 0.5F AndAlso (oneLine OrElse r.Height >= useFont.GetHeight(g) * 1.8F) Then Exit While
                    size *= 0.93F
                    If useFont IsNot f Then useFont.Dispose()
                    useFont = New Font(f.FontFamily, size, f.Style, f.Unit)
                End While
                g.DrawString(s, useFont, b, r, sf)
            Finally
                If useFont IsNot f Then useFont.Dispose()
            End Try
        End Using
    End Sub

    ' =====================================================================================
    ' Local settings (our details, printer offsets) - per company, on this PC
    ' =====================================================================================
    Private Function SettingsPath() As String
        Dim dir As String = Global.System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PhoenixGroupERP")
        Return Global.System.IO.Path.Combine(dir, "BOIRtgs_" & If(GMod.Cmpid, "").Trim() & ".ini")
    End Function

    Private Sub LoadSettings()
        _settings.Clear()
        Try
            Dim p As String = SettingsPath()
            If Not Global.System.IO.File.Exists(p) Then Return
            For Each line As String In Global.System.IO.File.ReadAllLines(p, Global.System.Text.Encoding.UTF8)
                Dim k As Integer = line.IndexOf("="c)
                If k > 0 Then _settings(line.Substring(0, k)) = line.Substring(k + 1)
            Next
        Catch
            ' settings are a convenience only
        End Try
    End Sub

    Private Sub SaveSettings()
        Try
            Dim code As String = SelectedBankCode()
            _settings("Bank") = code
            If code <> "" Then
                _settings("Title_" & code) = txtRemitter.Text.Trim()
                _settings("AcNo_" & code) = txtRemitterAc.Text.Trim()
                _settings("Branch_" & code) = txtOurBranch.Text.Trim()
            End If
            _settings("Mobile") = txtMobile.Text.Trim()
            _settings("Address") = txtRemAddress.Text.Trim().Replace(ChrW(13) & ChrW(10), "|").Replace(ChrW(10), "|").Replace(ChrW(13), "|")
            _settings("Signatory") = txtSignatory.Text.Trim()
            _settings("From") = txtFrom.Text.Trim()
            _settings("OffsetX") = nudOffsetX.Value.ToString(Global.System.Globalization.CultureInfo.InvariantCulture)
            _settings("OffsetY") = nudOffsetY.Value.ToString(Global.System.Globalization.CultureInfo.InvariantCulture)
            Dim p As String = SettingsPath()
            Global.System.IO.Directory.CreateDirectory(Global.System.IO.Path.GetDirectoryName(p))
            Global.System.IO.File.WriteAllLines(p, _settings.Select(Function(kv) kv.Key & "=" & kv.Value.Replace(ChrW(13), " "c).Replace(ChrW(10), " "c)).ToArray(),
                                                Global.System.Text.Encoding.UTF8)
        Catch
            ' ignore - read-only profile etc.
        End Try
    End Sub

    Private Function GetSetting(ByVal key As String, ByVal def As String) As String
        Dim v As String = Nothing
        If _settings.TryGetValue(key, v) AndAlso v IsNot Nothing Then Return v
        Return def
    End Function

    Private Shared Function ClampDec(ByVal s As String, ByVal n As NumericUpDown) As Decimal
        Dim d As Decimal
        If Not Decimal.TryParse(s, Global.System.Globalization.NumberStyles.Number, Global.System.Globalization.CultureInfo.InvariantCulture, d) Then d = 0
        Return Math.Max(n.Minimum, Math.Min(n.Maximum, d))
    End Function

    Private Shared Function ToDec(ByVal o As Object) As Decimal
        If o Is Nothing OrElse IsDBNull(o) Then Return 0
        Dim d As Decimal
        Decimal.TryParse(o.ToString(), d)
        Return d
    End Function

End Class
