Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic
Imports System.Text

' Feed Transfer - Post To Accounts
'
' Lists every Inter Unit Transfer bill of the feed factory that is not yet posted in the
' accounts. The user ticks the bills and posts them all at once. Each bill is posted in its
' own transaction (one voucher per bill, ALL lines of the bill), with the same entries as the
' old POST button:
'     Dr  party of the farm (ACC_HEAD.remark3 = ToFarm)          amount of the line
'     Cr  **TFPA0014  BEEJAPURI - FEED TRANSFER TO PARIYAT UNIT   amount of the line
'     + one OtherSaleData row per line (BillNo = IU number), which marks the bill as posted.
Public Class frmFeedTransfer

    Private Const ConStrFeed As String = "Data Source=192.168.0.130,3759;Initial Catalog=phxfeed_2324;User ID=sa;Password=Ph@hoenix#g;Connect Timeout=600;Encrypt=False;TrustServerCertificate=True;"
    ' session code of the transfer bills in the feed database (calendar year there)
    Private Const FeedSession As String = "2026"
    Private Const TransferHeadCode As String = "**TFPA0014"
    Private Const TransferHeadName As String = "BEEJAPURI - FEED TRANSFER TO PARIYAT UNIT"

    Private ReadOnly tablename As String = "VENTRY" & "_" & "PHOE" & "_" & GMod.Session
    Private ReadOnly tablenameacc As String = "ACC_HEAD" & "_" & "PHOE" & "_" & GMod.Session

    Private bills As DataTable          ' one row per pending IU bill (grid on top)
    Private billView As DataView
    Private lines As DataTable          ' all lines of all pending bills
    Private partyByFarm As New Dictionary(Of String, DataRow)(StringComparer.OrdinalIgnoreCase)
    Private updatingSelection As Boolean

    '=====================================================================
    ' Opening
    '=====================================================================
    Private Sub frmFeedTransfer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' date set to as per session (same rule as before)
        dtpPostingDate.Value = GMod.SessionCurrentDate
        dtpPostingDate.MinDate = CDate(GMod.SessionCurrentDate).AddDays(-Val(GMod.nofd))
        dtpPostingDate.MaxDate = GMod.SessionCurrentDate

        Dim sql As String = "select vtype from Vtype where cmp_id='PHOE'  and session='" & GMod.Session & "' and vtype like '%INTER%'"
        GMod.DataSetRet(sql, "CRVT")
        cmbVoucherType.DataSource = GMod.ds.Tables("CRVT")
        cmbVoucherType.DisplayMember = "vtype"
        Dim i As Integer = cmbVoucherType.FindStringExact("INTER TRANSFER")
        If i >= 0 Then cmbVoucherType.SelectedIndex = i
    End Sub

    Private Sub frmFeedTransfer_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        LoadPending()
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadPending()
    End Sub

    '=====================================================================
    ' Pending bills
    '=====================================================================
    Private Sub LoadPending()
        Cursor = Cursors.WaitCursor
        lblSelected.Text = "Loading pending bills" & ChrW(8230)
        lblSelected.Refresh()
        Try
            ' 1. all lines of transfer bills that are not yet in the accounts
            Dim sql As String =
                "SELECT a.Vou_no, a.Billdate, a.formulaname, a.OutQty, a.OutQtyNos, a.Rate, a.Amount, a.ToFarm " &
                "FROM OtherSaleData a " &
                "WHERE a.Session = @feedSession AND a.Vou_type = 'Inter Unit Transfer' " &
                "AND NOT EXISTS (SELECT 1 FROM [PhxGroupERP].dbo.OtherSaleData b " &
                "                WHERE b.Session = @session AND b.Vou_type LIKE '%INTER%' AND b.BillNo = a.Vou_no) " &
                "ORDER BY a.Billdate, a.Vou_no"
            lines = New DataTable()
            Using da As New SqlDataAdapter(sql, ConStrFeed)
                da.SelectCommand.Parameters.AddWithValue("@feedSession", FeedSession)
                da.SelectCommand.Parameters.AddWithValue("@session", GMod.Session)
                da.SelectCommand.CommandTimeout = 300
                da.Fill(lines)
            End Using

            ' 2. party account of every farm (ACC_HEAD.remark3 holds the farm name)
            partyByFarm.Clear()
            Dim heads As DataTable = Db.GetTable("select account_code, account_head_name, group_name, sub_group_name, remark3 from " & tablenameacc &
                                                 " where isnull(remark3,'') <> ''")
            For Each h As DataRow In heads.Rows
                Dim farm As String = Convert.ToString(h("remark3")).Trim()
                If farm.Length > 0 AndAlso Not partyByFarm.ContainsKey(farm) Then partyByFarm(farm) = h
            Next

            BuildBills()
        Catch ex As Exception
            MessageBox.Show(Me, "Could not load the pending transfer bills:" & Environment.NewLine & ex.Message,
                            "Feed Transfer", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            lblSelected.Text = "Could not load the pending bills."
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub BuildBills()
        bills = New DataTable()
        bills.Columns.Add("Post", GetType(Boolean))
        bills.Columns.Add("IU No", GetType(String))
        bills.Columns.Add("Bill date", GetType(Date))
        bills.Columns.Add("To farm", GetType(String))
        bills.Columns.Add("Party account", GetType(String))
        bills.Columns.Add("Product", GetType(String))
        bills.Columns.Add("Bags", GetType(Decimal))
        bills.Columns.Add("Qty kg", GetType(Decimal))
        bills.Columns.Add("Amount", GetType(Decimal))
        bills.Columns.Add("Lines", GetType(Integer))
        bills.Columns.Add("Status", GetType(String))
        bills.PrimaryKey = New DataColumn() {bills.Columns("IU No")}

        For Each l As DataRow In lines.Rows
            Dim iu As String = Convert.ToString(l("Vou_no")).Trim()
            Dim b As DataRow = bills.Rows.Find(iu)
            Dim farm As String = Convert.ToString(l("ToFarm")).Trim()
            Dim product As String = Convert.ToString(l("formulaname")).Trim()
            If b Is Nothing Then
                b = bills.NewRow()
                b("Post") = False
                b("IU No") = iu
                b("Bill date") = If(IsDBNull(l("Billdate")), CObj(DBNull.Value), CObj(Convert.ToDateTime(l("Billdate")).Date))
                b("To farm") = farm
                b("Party account") = ""
                b("Product") = product
                b("Bags") = 0D
                b("Qty kg") = 0D
                b("Amount") = 0D
                b("Lines") = 0
                b("Status") = "Ready"
                bills.Rows.Add(b)
            Else
                If Not String.Equals(Convert.ToString(b("Product")), product, StringComparison.OrdinalIgnoreCase) AndAlso
                   Not Convert.ToString(b("Product")).EndsWith(" +more") Then
                    b("Product") = Convert.ToString(b("Product")) & " +more"
                End If
                If Not String.Equals(Convert.ToString(b("To farm")), farm, StringComparison.OrdinalIgnoreCase) AndAlso
                   Not Convert.ToString(b("To farm")).EndsWith(" +more") Then
                    b("To farm") = Convert.ToString(b("To farm")) & " +more"
                End If
            End If
            b("Bags") = CDec(b("Bags")) + ToDec(l("OutQtyNos"))
            b("Qty kg") = CDec(b("Qty kg")) + ToDec(l("OutQty"))
            b("Amount") = CDec(b("Amount")) + ToDec(l("Amount"))
            b("Lines") = CInt(b("Lines")) + 1

            ' every line must have a party account and an amount
            Dim party As DataRow = Nothing
            If Not partyByFarm.TryGetValue(farm, party) Then
                b("Status") = "No party account for farm '" & farm & "'"
            ElseIf Convert.ToString(b("Party account")).Length = 0 Then
                b("Party account") = Convert.ToString(party("account_head_name"))
            End If
            If ToDec(l("Amount")) <= 0D AndAlso CStr(b("Status")) = "Ready" Then b("Status") = "Line without amount"
        Next

        billView = New DataView(bills)
        dgPending.DataSource = billView
        SetupPendingGrid()
        ApplySearch()
        UpdateSelection()
        ShowLines()
    End Sub

    Private Shared Function ToDec(v As Object) As Decimal
        If v Is Nothing OrElse IsDBNull(v) Then Return 0D
        Dim d As Decimal
        If Decimal.TryParse(Convert.ToString(v), d) Then Return d
        Return 0D
    End Function

    Private Sub SetupPendingGrid()
        With dgPending
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            For Each c As DataGridViewColumn In .Columns
                c.ReadOnly = (c.Name <> "Post")
                c.SortMode = DataGridViewColumnSortMode.Automatic
            Next
            .Columns("Post").FillWeight = 40
            .Columns("IU No").FillWeight = 70
            .Columns("Bill date").FillWeight = 75
            .Columns("Bill date").DefaultCellStyle.Format = "dd-MMM-yyyy"
            .Columns("To farm").FillWeight = 120
            .Columns("Party account").FillWeight = 170
            .Columns("Product").FillWeight = 150
            .Columns("Lines").FillWeight = 40
            .Columns("Status").FillWeight = 150
            For Each n As String In New String() {"Bags", "Qty kg", "Amount"}
                .Columns(n).DefaultCellStyle.Format = If(n = "Amount", "N2", "N3")
                .Columns(n).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                .Columns(n).FillWeight = 75
            Next
            .Columns("Lines").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End With
    End Sub

    Private Sub dgPending_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgPending.CellFormatting
        If e.RowIndex < 0 OrElse dgPending.Columns(e.ColumnIndex).Name <> "Status" Then Return
        Dim s As String = Convert.ToString(e.Value)
        e.CellStyle.ForeColor = If(s = "Ready", Color.FromArgb(46, 125, 50), Color.FromArgb(192, 57, 43))
    End Sub

    '=====================================================================
    ' Search, selection and the lines of the current bill
    '=====================================================================
    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        ApplySearch()
    End Sub

    Private Sub ApplySearch()
        If billView Is Nothing Then Return
        Dim t As String = txtSearch.Text.Trim().Replace("'", "''").Replace("[", "[[]").Replace("*", "[*]").Replace("%", "[%]")
        If t.Length = 0 Then
            billView.RowFilter = ""
        Else
            billView.RowFilter = String.Format("[IU No] LIKE '%{0}%' OR [To farm] LIKE '%{0}%' OR [Party account] LIKE '%{0}%' OR Product LIKE '%{0}%'", t)
        End If
        UpdateSelection()
    End Sub

    ' commit the tick at once so the totals update while clicking
    Private Sub dgPending_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs) Handles dgPending.CurrentCellDirtyStateChanged
        If dgPending.IsCurrentCellDirty Then dgPending.CommitEdit(DataGridViewDataErrorContexts.Commit)
    End Sub

    Private Sub dgPending_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgPending.CellValueChanged
        If updatingSelection OrElse e.RowIndex < 0 OrElse dgPending.Columns(e.ColumnIndex).Name <> "Post" Then Return
        Dim r As DataRowView = TryCast(dgPending.Rows(e.RowIndex).DataBoundItem, DataRowView)
        If r Is Nothing Then Return
        If CBool(r("Post")) AndAlso CStr(r("Status")) <> "Ready" Then
            updatingSelection = True
            r("Post") = False
            updatingSelection = False
            MessageBox.Show(Me, "Bill " & CStr(r("IU No")) & " cannot be posted: " & CStr(r("Status")) & ".",
                            "Feed Transfer", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
        UpdateSelection()
    End Sub

    Private Sub chkSelectAll_CheckedChanged(sender As Object, e As EventArgs) Handles chkSelectAll.CheckedChanged
        If updatingSelection OrElse billView Is Nothing Then Return
        updatingSelection = True
        Try
            For Each r As DataRowView In billView
                r("Post") = chkSelectAll.Checked AndAlso CStr(r("Status")) = "Ready"
            Next
        Finally
            updatingSelection = False
        End Try
        UpdateSelection()
    End Sub

    Private Sub UpdateSelection()
        If bills Is Nothing Then Return
        Dim n As Integer = 0
        Dim total As Decimal = 0D
        For Each r As DataRow In bills.Rows
            If CBool(r("Post")) Then
                n += 1
                total += CDec(r("Amount"))
            End If
        Next
        Dim shown As Integer = If(billView Is Nothing, bills.Rows.Count, billView.Count)
        lblSelected.Text = If(n = 0, "No bills selected", n.ToString() & If(n = 1, " bill", " bills") & " selected   " & ChrW(183) & "   Rs. " & total.ToString("N2")) &
                           "        (" & bills.Rows.Count.ToString() & " pending" & If(shown <> bills.Rows.Count, ", " & shown.ToString() & " shown", "") & ")"
        btnPostSelected.Enabled = n > 0
        btnPostSelected.Text = If(n = 0, "Post selected", "Post selected (" & n.ToString() & ")")
    End Sub

    Private Sub dgPending_SelectionChanged(sender As Object, e As EventArgs) Handles dgPending.SelectionChanged
        ShowLines()
    End Sub

    Private Sub ShowLines()
        If lines Is Nothing Then Return
        Dim iu As String = ""
        If dgPending.CurrentRow IsNot Nothing Then
            Dim r As DataRowView = TryCast(dgPending.CurrentRow.DataBoundItem, DataRowView)
            If r IsNot Nothing Then iu = CStr(r("IU No"))
        End If
        Dim v As New DataView(lines)
        v.RowFilter = "Trim(Vou_no) = '" & iu.Replace("'", "''") & "'"
        dgIUTRFList.DataSource = v
        lblLines.Text = If(iu.Length = 0, "Lines of the selected bill", "Lines of bill " & iu)
        For Each c As DataGridViewColumn In dgIUTRFList.Columns
            If c.Name = "Billdate" Then c.DefaultCellStyle.Format = "dd-MMM-yyyy"
            If c.Name = "Amount" OrElse c.Name = "Rate" Then c.DefaultCellStyle.Format = "N2"
        Next
        dgIUTRFList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    End Sub

    '=====================================================================
    ' Posting
    '=====================================================================
    Private Sub btnPostSelected_Click(sender As Object, e As EventArgs) Handles btnPostSelected.Click
        Dim selected As New List(Of String)()
        Dim total As Decimal = 0D
        For Each r As DataRow In bills.Rows
            If CBool(r("Post")) AndAlso CStr(r("Status")) = "Ready" Then
                selected.Add(CStr(r("IU No")))
                total += CDec(r("Amount"))
            End If
        Next
        If selected.Count = 0 Then Return
        If cmbVoucherType.Text.Trim().Length = 0 Then
            MessageBox.Show(Me, "Please select the voucher type.", "Feed Transfer", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If MessageBox.Show(Me, "Post " & selected.Count.ToString() & If(selected.Count = 1, " bill", " bills") &
                           " for Rs. " & total.ToString("N2") & Environment.NewLine &
                           "as " & cmbVoucherType.Text & " dated " & dtpPostingDate.Value.ToString("dd-MMM-yyyy") & "?",
                           "Post feed transfers", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return

        Dim posted As New StringBuilder()
        Dim failed As New StringBuilder()
        Dim okCount As Integer = 0
        Cursor = Cursors.WaitCursor
        btnPostSelected.Enabled = False
        Try
            For i As Integer = 0 To selected.Count - 1
                lblSelected.Text = "Posting " & (i + 1).ToString() & " of " & selected.Count.ToString() & ":  " & selected(i) & ChrW(8230)
                lblSelected.Refresh()
                Try
                    Dim vouNo As Integer = PostBill(selected(i))
                    okCount += 1
                    posted.AppendLine("   " & selected(i) & "  ->  voucher " & vouNo.ToString())
                Catch ex As Exception
                    failed.AppendLine("   " & selected(i) & ":  " & ex.Message)
                End Try
            Next
        Finally
            Cursor = Cursors.Default
        End Try

        Dim msg As New StringBuilder()
        msg.AppendLine(okCount.ToString() & " of " & selected.Count.ToString() & " bills posted.")
        If posted.Length > 0 Then msg.AppendLine().AppendLine("Posted:").Append(posted.ToString())
        If failed.Length > 0 Then msg.AppendLine().AppendLine("Not posted (nothing was saved for these):").Append(failed.ToString())
        MessageBox.Show(Me, msg.ToString(), "Feed Transfer", MessageBoxButtons.OK,
                        If(failed.Length > 0, MessageBoxIcon.Warning, MessageBoxIcon.Information))
        chkSelectAll.Checked = False
        LoadPending()
    End Sub

    ''' <summary>Posts one IU bill (all its lines) as one voucher in its own transaction; returns the voucher number.</summary>
    Private Function PostBill(iu As String) As Integer
        Dim billLines As DataRow() = lines.Select("Trim(Vou_no) = '" & iu.Replace("'", "''") & "'")
        If billLines.Length = 0 Then Throw New Exception("bill lines not found - click Refresh")
        Dim vouType As String = cmbVoucherType.Text
        Dim postDate As Date = dtpPostingDate.Value.Date

        Using conn As New SqlConnection(GMod.Connstr)
            conn.Open()
            Using tran As SqlTransaction = conn.BeginTransaction()
                Try
                    ' already posted (by someone else since the list was loaded)?
                    Using chk As New SqlCommand("SELECT COUNT(*) FROM OtherSaleData WITH (UPDLOCK, HOLDLOCK) " &
                                                "WHERE Session = @session AND Vou_type LIKE '%INTER%' AND BillNo = @iu", conn, tran)
                        chk.Parameters.AddWithValue("@session", GMod.Session)
                        chk.Parameters.AddWithValue("@iu", iu)
                        If Convert.ToInt32(chk.ExecuteScalar()) > 0 Then Throw New Exception("already posted")
                    End Using

                    ' next voucher number (locked)
                    Dim vouNo As Integer
                    Using cmdVou As New SqlCommand("SELECT ISNULL(MAX(CAST(vou_no AS NUMERIC(18,0))),0) + 1 FROM " & tablename &
                                                   " WITH (UPDLOCK, HOLDLOCK) WHERE vou_type = @vt", conn, tran)
                        cmdVou.Parameters.AddWithValue("@vt", vouType)
                        vouNo = Convert.ToInt32(cmdVou.ExecuteScalar())
                    End Using

                    For Each l As DataRow In billLines
                        Dim farm As String = Convert.ToString(l("ToFarm")).Trim()
                        Dim party As DataRow = Nothing
                        If Not partyByFarm.TryGetValue(farm, party) Then Throw New Exception("no party account for farm '" & farm & "'")
                        Dim amount As Decimal = ToDec(l("Amount"))
                        Dim billDate As Date = If(IsDBNull(l("Billdate")), postDate, Convert.ToDateTime(l("Billdate")).Date)
                        Dim narration As String = "BEING " & Convert.ToString(l("formulaname")) &
                                                  " No.of Bags " & Convert.ToString(l("OutQtyNos")) &
                                                  " Qty kg " & Convert.ToString(l("OutQty")) &
                                                  " @ " & Convert.ToString(l("Rate")) &
                                                  " TRANSFER FROM BEEJAPURI FEED FACTORY TO " & farm &
                                                  " Inv No. " & iu &
                                                  " DATED " & billDate.ToString("dd/MMM/yyyy")

                        ' Dr party
                        InsertVentry(conn, tran, vouNo, vouType, postDate, Convert.ToString(party("account_code")),
                                     Convert.ToString(party("account_head_name")), amount, 0D, narration,
                                     Convert.ToString(party("group_name")), Convert.ToString(party("sub_group_name")))

                        ' OtherSaleData (also marks the bill as posted)
                        Using cmd As New SqlCommand(
                            "INSERT INTO OtherSaleData ([Vou_type],[Vou_no],[AccCode],[AccName],[Station],[ProductName],[OutQty]," &
                            "[Rate],[Amount],[OutQtyNos],[BillNo],[BillDate],[InQty],[InQtyNos],[Cmp_id],[Session],[mrktrate],[authr]," &
                            "[Prdunit],[Packing],[Insurance],[Discount],[CrHead],[tcs_per],[tcs_amt],cgstp,cgsta) VALUES (" &
                            "@vt, @vno, @acc, @accname, 'COMMON', @prd, @qty, @rate, @amt, @bags, @iu, @billdate, 0, 0, @cmp, @session," &
                            " 0, '-', '-', 0, 0, 0, @crhead, 0, 0, 0, 0)", conn, tran)
                            cmd.Parameters.AddWithValue("@vt", vouType)
                            cmd.Parameters.AddWithValue("@vno", vouNo.ToString())
                            cmd.Parameters.AddWithValue("@acc", Convert.ToString(party("account_code")))
                            cmd.Parameters.AddWithValue("@accname", Convert.ToString(party("account_head_name")))
                            cmd.Parameters.AddWithValue("@prd", Convert.ToString(l("formulaname")))
                            cmd.Parameters.AddWithValue("@qty", ToDec(l("OutQty")))
                            cmd.Parameters.AddWithValue("@rate", ToDec(l("Rate")))
                            cmd.Parameters.AddWithValue("@amt", amount)
                            cmd.Parameters.AddWithValue("@bags", ToDec(l("OutQtyNos")))
                            cmd.Parameters.AddWithValue("@iu", iu)
                            cmd.Parameters.AddWithValue("@billdate", billDate)
                            cmd.Parameters.AddWithValue("@cmp", GMod.Cmpid)
                            cmd.Parameters.AddWithValue("@session", GMod.Session)
                            cmd.Parameters.AddWithValue("@crhead", TransferHeadName)
                            cmd.ExecuteNonQuery()
                        End Using

                        ' Cr feed transfer head
                        InsertVentry(conn, tran, vouNo, vouType, postDate, TransferHeadCode, TransferHeadName, 0D, amount,
                                     narration, "TRANSFER", "PARIYAT UNIT")
                    Next

                    tran.Commit()
                    Return vouNo
                Catch
                    Try
                        tran.Rollback()
                    Catch
                    End Try
                    Throw
                End Try
            End Using
        End Using
    End Function

    Private Sub InsertVentry(conn As SqlConnection, tran As SqlTransaction, vouNo As Integer, vouType As String, vouDate As Date,
                             accCode As String, accName As String, dr As Decimal, cr As Decimal, narration As String,
                             groupName As String, subGroup As String)
        Using cmd As New SqlCommand(
            "INSERT INTO " & tablename & " (Cmp_id, Uname, Entry_id, Vou_no, Vou_type, Vou_date, Acc_head_code, Acc_head," &
            " dramt, cramt, Pay_mode, Cheque_no, Narration, Group_name, Sub_group_name, entry_date) VALUES (" &
            "@cmp, @user, 0, @vno, @vt, @vdate, @acc, @accname, @dr, @cr, '-', '-', @narr, @grp, @sub, @edate)", conn, tran)
            cmd.Parameters.AddWithValue("@cmp", GMod.Cmpid)
            cmd.Parameters.AddWithValue("@user", GMod.username)
            cmd.Parameters.AddWithValue("@vno", vouNo.ToString())
            cmd.Parameters.AddWithValue("@vt", vouType)
            cmd.Parameters.AddWithValue("@vdate", vouDate)
            cmd.Parameters.AddWithValue("@acc", accCode)
            cmd.Parameters.AddWithValue("@accname", accName)
            cmd.Parameters.AddWithValue("@dr", dr)
            cmd.Parameters.AddWithValue("@cr", cr)
            cmd.Parameters.AddWithValue("@narr", narration)
            cmd.Parameters.AddWithValue("@grp", groupName)
            cmd.Parameters.AddWithValue("@sub", subGroup)
            cmd.Parameters.AddWithValue("@edate", DateTime.Now.Date)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

End Class
