Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Drawing.Printing
Imports System.IO
Imports System.Text
Imports System.Windows.Forms

' Party ageing report: PARTY-group accounts with a Cr balance (amount payable), all voucher types,
' totals equal to the General Ledger balance.
' Add these files to a Windows Forms project targeting .NET Framework 2.0 or later.
' Keep AgeingForm.vb, AgeingForm.Designer.vb and AgeingForm.resx together in the project.
' Set AgeingForm as the startup form or open it with: New AgeingForm().Show()
Public Class AgeingForm

    ' Change this to your SQL Server connection string.
    Friend Shared ReadOnly Property ConnectionString As String
        Get
            Return GMod.Connstr
        End Get
    End Property
       
    Private Const QueryTimeoutSeconds As Integer = 300

    Private ReadOnly overdueFont As New Font("Segoe UI", 9.5F, FontStyle.Bold)
    Private view As DataView
    Private printRowIndex As Integer

    Public Sub New()
        InitializeComponent()
        printDoc.DefaultPageSettings.Landscape = True
    End Sub

    ' ---------- Data ----------
    '
    ' How the report works (matches the General Ledger):
    '  * Balance of each party = opening (Dr - Cr) in the account master
    '    + all debits - all credits up to the "As on" date, ALL voucher types,
    '    with the same exclusions as the ledger (BANKREC, Pay_mode '-', users starting with '$').
    '  * Only accounts of the PARTY group with a CREDIT balance (we owe them) are shown.
    '  * Ageing: the Cr balance is matched against the party's most recent credit entries
    '    (their bills), newest first, until it is used up (first-in, first-out): payments
    '    settle the oldest bills first. Whatever is left after the session's credits is the
    '    brought-forward opening balance, dated on the first day of the session.
    '    So 0-30 + 31-60 + 60+ = Total due = ledger Cr balance.
    '  * Age is counted from the BILL DATE written in the narration by the purchase screens
    '    ("BILL NO 123 DT.05/Oct/2026"); if a narration has no readable bill date, the
    '    voucher date is used. Bills are matched newest bill date first.

    Private Const PartyGroup As String = "PARTY"
    Private billsByParty As New Dictionary(Of String, DataTable)(StringComparer.OrdinalIgnoreCase)
    Private reportAsOn As Date
    Private pendingAsOn As Date

    Private Class AgeingResult
        Public Summary As DataTable
        Public Bills As Dictionary(Of String, DataTable)
    End Class

    Private Sub AgeingForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Dim today As Date = DateTime.Today
        Try
            today = GMod.ServerNow().Date
        Catch
        End Try
        ' the ledger tables are per session: default to today, or the session's last day for an old session
        If GMod.SessionEndDate > Date.MinValue AndAlso today > GMod.SessionEndDate.Date Then today = GMod.SessionEndDate.Date
        dtpAsOn.Value = today
        Text = "Party Ageing Report  -  " & GMod.Cmpname & "  [session " & GMod.Session & "]"

    End Sub

    Private Shared ReadOnly BillDatePattern As New Global.System.Text.RegularExpressions.Regex(
        "(?<![A-Z])D(?:A?TE?D?|T)\s*[.:\-]?\s*(\d{1,2})\s*[/\-. ]\s*([A-Z]{3,9}|\d{1,2})\.?\s*[/\-. ]\s*(\d{4}|\d{2})(?!\d)",
        Global.System.Text.RegularExpressions.RegexOptions.IgnoreCase Or Global.System.Text.RegularExpressions.RegexOptions.Compiled)

    ' Reads the bill date from a narration such as "BILL NO 123 DT.05/Oct/2026", "DT.05-Oct-26",
    ' "DT 05/10/2026" or "DATE: 5.10.26". Returns Nothing when there is no plausible date.
    Friend Shared Function BillDateFromNarration(narration As String, voucherDate As Date, asOn As Date) As Date?
        If String.IsNullOrEmpty(narration) Then Return Nothing
        Dim m As Global.System.Text.RegularExpressions.Match = BillDatePattern.Match(narration)
        While m.Success
            Dim day As Integer = Integer.Parse(m.Groups(1).Value)
            Dim monthText As String = m.Groups(2).Value
            Dim month As Integer = 0
            If Char.IsDigit(monthText(0)) Then
                month = Integer.Parse(monthText)
            Else
                Dim names As String() = {"JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"}
                month = Array.IndexOf(names, monthText.Substring(0, 3).ToUpperInvariant()) + 1
            End If
            Dim year As Integer = Integer.Parse(m.Groups(3).Value)
            If year < 100 Then year += 2000
            If month >= 1 AndAlso month <= 12 AndAlso day >= 1 AndAlso year >= 1990 AndAlso year <= 2100 AndAlso day <= Date.DaysInMonth(year, month) Then
                Dim d As New Date(year, month, day)
                ' plausible: not after the report date and not more than 3 years before the voucher
                If d <= asOn.Date AndAlso d >= voucherDate.Date.AddYears(-3) Then Return d
            End If
            m = m.NextMatch()
        End While
        Return Nothing
    End Function

    Private Shared Function RunAgeing(asOn As Date) As AgeingResult
        Dim filters As String = " AND v.vou_type <> 'BANKREC' AND v.Pay_mode <> '-' AND LEFT(v.Uname, 1) <> '$' AND v.vou_date < @next "
        Dim acc As String = GMod.ACC_HEAD
        Dim ven As String = GMod.VENTRY
        Dim sql As String =
            "SET NOCOUNT ON; " &
            "DECLARE @next datetime; SET @next = DATEADD(day, 1, @asOn); " &
            "IF OBJECT_ID('tempdb..#bal') IS NOT NULL DROP TABLE #bal; " &
            "SELECT h.account_code, h.account_head_name, h.group_name, " &
            "  CAST(ISNULL(h.opening_cr, 0) - ISNULL(h.opening_dr, 0) - ISNULL(t.net, 0) AS decimal(18, 2)) AS balance " &
            "INTO #bal FROM " & acc & " h " &
            "LEFT JOIN (SELECT v.Acc_head_code, SUM(v.dramt - v.cramt) AS net FROM " & ven & " v WHERE 1 = 1 " & filters &
            "           GROUP BY v.Acc_head_code) t ON t.Acc_head_code = h.account_code " &
            "WHERE h.group_name = @grp " &
            "  AND ISNULL(h.opening_cr, 0) - ISNULL(h.opening_dr, 0) - ISNULL(t.net, 0) > 0; " &
            "SELECT account_code, account_head_name, group_name, balance FROM #bal; " &
            "SELECT v.Acc_head_code, v.vou_date, v.vou_type, v.vou_no, v.Narration, v.cramt AS amount FROM " & ven & " v " &
            "  INNER JOIN #bal b ON b.account_code = v.Acc_head_code WHERE v.cramt > 0 " & filters &
            "  ORDER BY v.Acc_head_code, v.vou_date DESC, v.Entry_id DESC; " &
            "WITH lastbill AS (SELECT v.Acc_head_code, v.vou_no, v.vou_type, " &
            "    ROW_NUMBER() OVER (PARTITION BY v.Acc_head_code ORDER BY v.vou_date DESC, v.Entry_id DESC) AS rn " &
            "  FROM " & ven & " v INNER JOIN #bal b ON b.account_code = v.Acc_head_code WHERE v.cramt > 0 " & filters & "), " &
            " contra AS (SELECT l.Acc_head_code AS party, c.Acc_head, " &
            "    ROW_NUMBER() OVER (PARTITION BY l.Acc_head_code ORDER BY c.dramt DESC) AS rn " &
            "  FROM lastbill l INNER JOIN " & ven & " c ON c.vou_no = l.vou_no AND c.vou_type = l.vou_type " &
            "    AND c.dramt > 0 AND c.Acc_head_code <> l.Acc_head_code WHERE l.rn = 1) " &
            "SELECT party, Acc_head FROM contra WHERE rn = 1; " &
            "DROP TABLE #bal;"

        Dim data As New DataSet()
        Using connection As New SqlConnection(GMod.Connstr)
            Using command As New SqlCommand(sql, connection)
                command.CommandTimeout = QueryTimeoutSeconds
                command.Parameters.Add("@asOn", SqlDbType.DateTime).Value = asOn.Date
                command.Parameters.Add("@grp", SqlDbType.VarChar, 40).Value = PartyGroup
                Using adapter As New SqlDataAdapter(command)
                    adapter.Fill(data)
                End Using
            End Using
        End Using

        Dim balances As DataTable = data.Tables(0)
        Dim credits As DataTable = data.Tables(1)
        Dim products As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each row As DataRow In data.Tables(2).Rows
            products(Convert.ToString(row(0)).Trim()) = Convert.ToString(row(1)).Trim()
        Next

        ' credit entries (bills) per party, already newest first
        Dim billsByCode As New Dictionary(Of String, List(Of DataRow))(StringComparer.OrdinalIgnoreCase)
        For Each row As DataRow In credits.Rows
            Dim code As String = Convert.ToString(row("Acc_head_code")).Trim()
            Dim list As List(Of DataRow) = Nothing
            If Not billsByCode.TryGetValue(code, list) Then
                list = New List(Of DataRow)()
                billsByCode(code) = list
            End If
            list.Add(row)
        Next

        Dim summary As New DataTable()
        summary.Columns.Add("PartyCode", GetType(String))
        summary.Columns.Add("PartyName", GetType(String))
        summary.Columns.Add("Product", GetType(String))
        summary.Columns.Add("Amt0To30", GetType(Decimal))
        summary.Columns.Add("Amt31To60", GetType(Decimal))
        summary.Columns.Add("AmtAbove60", GetType(Decimal))
        summary.Columns.Add("TotalDue", GetType(Decimal))
        summary.Columns.Add("OldestDays", GetType(Integer))

        Dim bills As New Dictionary(Of String, DataTable)(StringComparer.OrdinalIgnoreCase)
        Dim openingDate As Date = If(GMod.SessionStartDate > Date.MinValue, GMod.SessionStartDate.Date, asOn.Date)

        For Each party As DataRow In balances.Rows
            Dim code As String = Convert.ToString(party("account_code")).Trim()
            Dim balance As Decimal = Convert.ToDecimal(party("balance"))
            Dim remaining As Decimal = balance
            Dim b0 As Decimal = 0D, b31 As Decimal = 0D, b60 As Decimal = 0D
            Dim oldest As Integer = -1

            Dim detail As New DataTable()
            detail.Columns.Add("Bill date", GetType(Date))
            detail.Columns.Add("Voucher date", GetType(Date))
            detail.Columns.Add("Voucher type", GetType(String))
            detail.Columns.Add("Voucher no", GetType(String))
            detail.Columns.Add("Narration", GetType(String))
            detail.Columns.Add("Bill amount", GetType(Decimal))
            detail.Columns.Add("Pending", GetType(Decimal))
            detail.Columns.Add("Age (days)", GetType(Integer))

            Dim list As List(Of DataRow) = Nothing
            If billsByCode.TryGetValue(code, list) Then
                ' bill date from the narration (else the voucher date); newest bill first
                Dim entries As New List(Of KeyValuePair(Of Date, DataRow))(list.Count)
                For Each d As DataRow In list
                    Dim vouDate As Date = Convert.ToDateTime(d("vou_date")).Date
                    Dim fromText As Date? = BillDateFromNarration(Convert.ToString(d("Narration")), vouDate, asOn)
                    entries.Add(New KeyValuePair(Of Date, DataRow)(If(fromText.HasValue, fromText.Value, vouDate), d))
                Next
                ' stable sort: equal bill dates keep the voucher order (newest voucher first)
                Dim order As New List(Of Integer)(entries.Count)
                For i As Integer = 0 To entries.Count - 1
                    order.Add(i)
                Next
                order.Sort(Function(x, y)
                               Dim c As Integer = entries(y).Key.CompareTo(entries(x).Key)
                               Return If(c <> 0, c, x.CompareTo(y))
                           End Function)
                For Each idx As Integer In order
                    If remaining <= 0D Then Exit For
                    Dim d As DataRow = entries(idx).Value
                    Dim billDate As Date = entries(idx).Key
                    Dim amount As Decimal = Convert.ToDecimal(d("amount"))
                    Dim portion As Decimal = Math.Min(remaining, amount)
                    Dim age As Integer = Math.Max(0, (asOn.Date - billDate).Days)
                    AddToBucket(age, portion, b0, b31, b60, oldest)
                    detail.Rows.Add(billDate, Convert.ToDateTime(d("vou_date")).Date, Convert.ToString(d("vou_type")),
                                    Convert.ToString(d("vou_no")), Convert.ToString(d("Narration")), amount, portion, age)
                    remaining -= portion
                Next
            End If
            If remaining > 0D Then
                ' not covered by this session's bills: the brought-forward opening balance
                Dim age As Integer = Math.Max(0, (asOn.Date - openingDate).Days)
                AddToBucket(age, remaining, b0, b31, b60, oldest)
                detail.Rows.Add(openingDate, openingDate, "OPENING", "-", "Balance brought forward from previous year", remaining, remaining, age)
            End If

            Dim product As String = Nothing
            If Not products.TryGetValue(code, product) OrElse String.IsNullOrEmpty(product) Then
                product = Convert.ToString(party("group_name"))
            End If
            ' same grouping the old pau_ageing procedure used
            Select Case product.Trim().ToUpperInvariant()
                Case "FEED MEDICINE", "PARIYAT MEDICINE", "BROILER MEDICINE", "LAYER MEDICINE", "GOURHA MEDICINE"
                    product = "MEDICINE"
            End Select
            summary.Rows.Add(code, Convert.ToString(party("account_head_name")), product, b0, b31, b60, balance,
                             If(oldest > 60, CObj(oldest), DBNull.Value))
            bills(code) = detail
        Next

        Return New AgeingResult() With {.Summary = summary, .Bills = bills}
    End Function

    Private Shared Sub AddToBucket(age As Integer, amount As Decimal, ByRef b0 As Decimal, ByRef b31 As Decimal,
                                   ByRef b60 As Decimal, ByRef oldest As Integer)
        If age <= 30 Then
            b0 += amount
        ElseIf age <= 60 Then
            b31 += amount
        Else
            b60 += amount
        End If
        If age > oldest Then oldest = age
    End Sub

    Private Sub BtnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click
        btnLoad.Enabled = False
        SetResultActions(False)
        UseWaitCursor = True
        lblStatus.Text = "Calculating balances and ageing..."
        pendingAsOn = dtpAsOn.Value.Date

        Dim worker As New BackgroundWorker()
        AddHandler worker.DoWork, AddressOf ReportWorker_DoWork
        AddHandler worker.RunWorkerCompleted, AddressOf ReportWorker_Completed
        worker.RunWorkerAsync(pendingAsOn)
    End Sub

    Private Sub ReportWorker_DoWork(sender As Object, e As DoWorkEventArgs)
        e.Result = RunAgeing(DirectCast(e.Argument, Date))
    End Sub

    Private Sub ReportWorker_Completed(sender As Object, e As RunWorkerCompletedEventArgs)
        DirectCast(sender, BackgroundWorker).Dispose()
        If IsDisposed Then Return

        UseWaitCursor = False
        btnLoad.Enabled = True

        If e.Error IsNot Nothing Then
            ' Keep the buttons usable for an earlier report that is still on screen.
            SetResultActions(view IsNot Nothing)
            lblStatus.Text = "The report could not be loaded."
            MessageBox.Show(Me, e.Error.Message, "Report failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        Dim result As AgeingResult = DirectCast(e.Result, AgeingResult)
        billsByParty = result.Bills
        view = New DataView(result.Summary)
        view.Sort = "TotalDue DESC, PartyName ASC"
        reportAsOn = pendingAsOn
        grid.DataSource = view
        ApplyFilter()
        SetResultActions(True)
    End Sub

    Private Sub SetResultActions(enabled As Boolean)
        btnExport.Enabled = enabled
        btnPrint.Enabled = enabled
    End Sub

    ' ---------- Filtering and totals ----------

    Private Sub FilterChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        ApplyFilter()
    End Sub

    Private Sub ApplyFilter()
        If view Is Nothing Then Return

        Dim conditions As New List(Of String)()

        Dim search As String = txtSearch.Text.Trim()
        If search.Length > 0 Then
            Dim pattern As String = EscapeLike(search)
            conditions.Add(String.Format("(PartyName LIKE '%{0}%' OR PartyCode LIKE '%{0}%')", pattern))
        End If

        view.RowFilter = String.Join(" AND ", conditions.ToArray())
        UpdateSummary()
    End Sub

    Private Shared Function EscapeLike(value As String) As String
        Dim result As New StringBuilder()
        For Each character As Char In value
            Select Case character
                Case "'"c
                    result.Append("''")
                Case "["c, "]"c, "*"c, "%"c
                    result.Append("[").Append(character).Append("]")
                Case Else
                    result.Append(character)
            End Select
        Next
        Return result.ToString()
    End Function

    Private Function SumOf(columnName As String) As Decimal
        Dim total As Decimal = 0D
        For Each row As DataRowView In view
            If Not Convert.IsDBNull(row(columnName)) Then total += Convert.ToDecimal(row(columnName))
        Next
        Return total
    End Function

    Private Sub UpdateSummary()
        lblParties.Text = view.Count.ToString("N0")
        lbl0To30.Text = SumOf("Amt0To30").ToString("N2")
        lbl31To60.Text = SumOf("Amt31To60").ToString("N2")
        lblAbove60.Text = SumOf("AmtAbove60").ToString("N2")
        lblTotal.Text = SumOf("TotalDue").ToString("N2")
        lblStatus.Text = String.Format("{0:N0} parties shown. Double-click a party to see its unpaid bills.", view.Count)
    End Sub

    ' ---------- Grid ----------

    Private Sub Grid_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles grid.CellFormatting
        If e.RowIndex < 0 OrElse grid.Columns(e.ColumnIndex) IsNot colAmtAbove60 Then Return
        If e.Value Is Nothing OrElse Convert.IsDBNull(e.Value) Then Return

        If Convert.ToDecimal(e.Value) > 0D Then
            e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28)
            e.CellStyle.Font = overdueFont
        End If
    End Sub

    Private Sub Grid_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles grid.CellDoubleClick
        If e.RowIndex < 0 Then Return

        Dim row As DataRowView = TryCast(grid.Rows(e.RowIndex).DataBoundItem, DataRowView)
        If row Is Nothing Then Return

        Dim partyBills As DataTable = Nothing
        If Not billsByParty.TryGetValue(Convert.ToString(row("PartyCode")), partyBills) Then Return
        Using details As New BillDetailForm(Convert.ToString(row("PartyName")), partyBills)
            details.ShowDialog(Me)
        End Using
    End Sub

    ' ---------- Export ----------

    Private Sub BtnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        Using dialog As New SaveFileDialog()
            dialog.Filter = "CSV file (*.csv)|*.csv"
            dialog.FileName = String.Format("Party_Ageing_{0:yyyyMMdd}.csv", DateTime.Today)
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return

            Try
                WriteCsv(dialog.FileName)
                lblStatus.Text = "Exported to " & dialog.FileName
            Catch ex As Exception
                MessageBox.Show(Me, ex.Message, "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    Private Sub WriteCsv(path As String)
        Dim columns As String() = {"PartyCode", "PartyName", "Product", "Amt0To30", "Amt31To60",
                                   "AmtAbove60", "TotalDue", "OldestDays"}
        Dim headers As String() = {"Code", "Party", "Product", "0-30 days", "31-60 days",
                                   "Above 60 days", "Total due", "Oldest 60+ (days)"}

        Using writer As New StreamWriter(path, False, New UTF8Encoding(True))
            Dim cells(headers.Length - 1) As String
            For index As Integer = 0 To headers.Length - 1
                cells(index) = CsvValue(headers(index))
            Next
            writer.WriteLine(String.Join(",", cells))

            For Each row As DataRowView In view
                For index As Integer = 0 To columns.Length - 1
                    cells(index) = CsvValue(Convert.ToString(row(columns(index))))
                Next
                writer.WriteLine(String.Join(",", cells))
            Next
        End Using
    End Sub

    Private Shared Function CsvValue(value As String) As String
        ' Stops Excel treating names that start with these characters as formulas.
        If value.Length > 0 AndAlso "=+-@".IndexOf(value(0)) >= 0 Then
            Dim number As Decimal
            If Not Decimal.TryParse(value, number) Then value = "'" & value
        End If
        Return """" & value.Replace("""", """""") & """"
    End Function

    ' ---------- Print ----------

    Private Sub BtnPrint_Click(sender As Object, e As EventArgs) Handles btnPrint.Click
        If view.Count = 0 Then
            MessageBox.Show(Me, "There are no rows to print.", "Party Ageing",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Using preview As New PrintPreviewDialog()
            preview.Document = printDoc
            preview.Width = 1000
            preview.Height = 700
            preview.ShowDialog(Me)
        End Using
    End Sub

    Private Sub PrintDoc_BeginPrint(sender As Object, e As PrintEventArgs) Handles printDoc.BeginPrint
        printRowIndex = 0
    End Sub

    Private Sub PrintDoc_PrintPage(sender As Object, e As PrintPageEventArgs) Handles printDoc.PrintPage
        Dim columns As String() = {"PartyName", "Amt0To30", "Amt31To60", "AmtAbove60", "TotalDue"}
        Dim headers As String() = {"Party", "0-30 days", "31-60 days", "Above 60 days", "Total due"}
        Dim widthShares As Single() = {0.36F, 0.16F, 0.16F, 0.16F, 0.16F}

        Dim area As Rectangle = e.MarginBounds
        Dim y As Single = area.Top

        Using titleFont As New Font("Segoe UI", 14.0F, FontStyle.Bold),
              bodyFont As New Font("Segoe UI", 9.0F),
              boldFont As New Font("Segoe UI", 9.0F, FontStyle.Bold)

            e.Graphics.DrawString("Party Ageing Report", titleFont, Brushes.Black, area.Left, y)
            y += titleFont.GetHeight(e.Graphics) + 2
            e.Graphics.DrawString(String.Format("{0}    As on: {1:dd-MMM-yyyy}    Group: PARTY    Cr balances (payable), all voucher types",
                                                GMod.Cmpname, reportAsOn),
                                  bodyFont, Brushes.DimGray, area.Left, y)
            y += bodyFont.GetHeight(e.Graphics) + 10

            Dim rowHeight As Single = bodyFont.GetHeight(e.Graphics) + 6
            DrawPrintRow(e.Graphics, headers, widthShares, area, y, rowHeight, boldFont)
            y += rowHeight
            e.Graphics.DrawLine(Pens.Black, area.Left, y, area.Right, y)

            ' Keep room for the total on the last page.
            Dim lastRowY As Single = area.Bottom - (rowHeight * 2)
            While printRowIndex < view.Count AndAlso y <= lastRowY
                Dim row As DataRowView = view(printRowIndex)
                Dim values(columns.Length - 1) As String
                values(0) = Convert.ToString(row(columns(0)))
                For index As Integer = 1 To columns.Length - 1
                    values(index) = FormatAmount(row(columns(index)))
                Next
                DrawPrintRow(e.Graphics, values, widthShares, area, y, rowHeight, bodyFont)
                y += rowHeight
                printRowIndex += 1
            End While

            If printRowIndex < view.Count Then
                e.HasMorePages = True
            Else
                e.Graphics.DrawLine(Pens.Black, area.Left, y, area.Right, y)
                Dim totals As String() = {"Total", SumOf(columns(1)).ToString("N2"), SumOf(columns(2)).ToString("N2"),
                                          SumOf(columns(3)).ToString("N2"), SumOf(columns(4)).ToString("N2")}
                DrawPrintRow(e.Graphics, totals, widthShares, area, y + 2, rowHeight, boldFont)
                e.HasMorePages = False
            End If
        End Using
    End Sub

    Private Shared Function FormatAmount(value As Object) As String
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return "0.00"
        Return Convert.ToDecimal(value).ToString("N2")
    End Function

    Private Shared Sub DrawPrintRow(g As Graphics, values As String(), shares As Single(),
                                    area As Rectangle, y As Single, height As Single, font As Font)
        Dim x As Single = area.Left
        For index As Integer = 0 To values.Length - 1
            Dim width As Single = area.Width * shares(index)
            Using format As New StringFormat()
                format.Alignment = If(index = 0, StringAlignment.Near, StringAlignment.Far)
                format.Trimming = StringTrimming.EllipsisCharacter
                format.FormatFlags = StringFormatFlags.NoWrap
                g.DrawString(values(index), font, Brushes.Black, New RectangleF(x, y, width, height), format)
            End Using
            x += width
        Next
    End Sub
End Class

' Shows the bills that make up a party's balance (newest first), as matched by the ageing.
Public Class BillDetailForm
    Inherits ThemedForm

    Private ReadOnly grid As New DataGridView()

    Public Sub New(partyName As String, bills As DataTable)
        Text = "Pending bills - " & partyName
        Font = New Font("Segoe UI", 9.5F)
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(980, 480)

        grid.Dock = DockStyle.Fill
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.RowHeadersVisible = False
        grid.BackgroundColor = Color.White
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        Controls.Add(grid)

        grid.DataSource = bills
        AddHandler Load, Sub(s, e)
                             grid.Columns("Bill date").DefaultCellStyle.Format = "dd-MMM-yyyy"
                             grid.Columns("Voucher date").DefaultCellStyle.Format = "dd-MMM-yyyy"
                             grid.Columns("Bill amount").DefaultCellStyle.Format = "N2"
                             grid.Columns("Pending").DefaultCellStyle.Format = "N2"
                             grid.Columns("Narration").FillWeight = 260
                             For Each name As String In New String() {"Bill amount", "Pending", "Age (days)"}
                                 grid.Columns(name).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                             Next
                         End Sub
    End Sub
End Class
