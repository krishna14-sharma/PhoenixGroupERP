Imports System.IO
Imports System.Diagnostics
Imports System.Data.SqlClient
Imports System.Runtime.InteropServices
Public Class frmlogin

    '----------------------------------------------------------------------
    ' Window chrome for the borderless sign-in window
    '----------------------------------------------------------------------
    <DllImport("user32.dll")> _
    Private Shared Function ReleaseCapture() As Boolean
    End Function
    <DllImport("user32.dll")> _
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function
    Private Const WM_NCLBUTTONDOWN As Integer = &HA1
    Private Const HTCAPTION As Integer = 2
    Private Const CS_DROPSHADOW As Integer = &H20000

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW
            Return cp
        End Get
    End Property

    ' drag the window by its brand panel or the empty card area
    Private Sub DragWindow_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles pnlBrand.MouseDown, pnlCard.MouseDown, lblTitle.MouseDown, lblSubtitle.MouseDown
        If e.Button = MouseButtons.Left Then
            Try
                ReleaseCapture()
                SendMessage(Me.Handle, WM_NCLBUTTONDOWN, New IntPtr(HTCAPTION), IntPtr.Zero)
            Catch
            End Try
        End If
    End Sub

    Private Sub lblClose_Click(ByVal sender As Object, ByVal e As EventArgs) Handles lblClose.Click
        Application.Exit()
    End Sub

    Private Sub lblClose_MouseEnter(ByVal sender As Object, ByVal e As EventArgs) Handles lblClose.MouseEnter
        lblClose.BackColor = Color.FromArgb(232, 17, 35)
        lblClose.ForeColor = Color.White
    End Sub

    Private Sub lblClose_MouseLeave(ByVal sender As Object, ByVal e As EventArgs) Handles lblClose.MouseLeave
        lblClose.BackColor = pnlCard.BackColor
        lblClose.ForeColor = AppTheme.TextMuted
    End Sub

    ' Shows a short status / error line under the buttons instead of a pop-up.
    Private Sub ShowStatus(ByVal text As String, Optional ByVal isError As Boolean = False)
        lblFooter.Text = text
        lblFooter.ForeColor = If(isError, AppTheme.Danger, AppTheme.TextMuted)
    End Sub

    Private Sub ShowServerInfo()
        Try
            Dim b As New SqlConnectionStringBuilder(GMod.Connstr)
            ShowStatus("Server: " & b.DataSource & "   " & ChrW(183) & "   Database: " & b.InitialCatalog)
        Catch
            ShowStatus("Connected")
        End Try
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnexit.Click
        Application.Exit()
    End Sub
    Private Sub frmlogin_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        GMod.ds.Tables.Clear()
    End Sub
    Public Sub FillSession()
        Dim sqlgetate As String
        sqlgetate = "select getdate()"
        GMod.DataSetRet(sqlgetate, "gdate")
        dtCurrDate.Value = CDate(GMod.ds.Tables("gdate").Rows(0)(0))
        'Dim i As Integer
        Dim start As Integer = 8
        'dtCurrDate.Value = "4/1/08"
        'For i = 0 To 50
        'dtCurrDate.Value = dtCurrDate.Value.AddYears(i)
        'cmbSession.Items.Add(GMod.Getsession(dtCurrDate.Value))
        'Next
    End Sub
    Public Sub createBatchFile()
        Dim sw As StreamWriter
        sw = File.CreateText("printfile.bat")
        sw.WriteLine("@echo off")
        sw.WriteLine("pause")
        sw.WriteLine("type %* > prn")
        sw.WriteLine("type %* >>b.txt")
        sw.WriteLine("exit")
        sw.Close()
    End Sub
    Private Sub frmlogin_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Dim procList() As Process = Process.GetProcesses()
        'Dim i As Integer
        'For i = 0 To procList.Length - 1
        '    Dim strProcName As String = procList(i).ProcessName
        '    Dim iProcID As Integer = procList(i).Id
        '    'MsgBox(strProcName)
        '    If strProcName = "PhoenixGroups" Then
        '        'MsgBox ("Progra")
        '        Me.Close()
        '    End If
        'Next
        createBatchFile()
        lblClose.ForeColor = AppTheme.TextMuted
        Try
            ' Config_ERP.txt holds the (obfuscated) connection string; the last line wins, as before.
            Dim configPath As String = Nothing
            If System.IO.File.Exists("Config_ERP.txt") Then
                configPath = "Config_ERP.txt"
            ElseIf System.IO.File.Exists("c:\Config_ERP.txt") Then
                configPath = "c:\Config_ERP.txt"
            End If
            If configPath Is Nothing Then
                frmServerdetection.ShowDialog()
                Exit Sub
            End If
            ' read exactly like the original StreamReader: UTF-8 (frmServerdetection writes the file in UTF-8)
            For Each line As String In File.ReadAllLines(configPath, System.Text.Encoding.UTF8)
                GMod.Connstr = EncryptionStr(line, False)
            Next
            ' FIX: the shared connection opens itself on first use; calling .Open() on it
            ' again threw "connection already open" and sent every user to the server screen.
            GMod.OpenSharedConnection()
        Catch ex As Exception
            MessageBox.Show("Could not connect to the database server:" & Environment.NewLine & Environment.NewLine & ex.Message & Environment.NewLine & Environment.NewLine & "Please check the server settings on the next screen.", "Phoenix Group ERP", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            frmServerdetection.ShowDialog()
            Exit Sub
        End Try
        FillSession()
        ShowServerInfo()
        Me.ActiveControl = txtuname

    End Sub

    Private Sub btnlogin_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnlogin.Click
        ' quick validation with an inline message instead of an error further down
        If txtuname.Text.Trim() = "" Then
            ShowStatus("Please enter your user name.", True) : txtuname.Focus() : Exit Sub
        End If
        If cmbcmpname.SelectedIndex < 0 Then
            ShowStatus("Please select a company.", True) : cmbcmpname.Focus() : Exit Sub
        End If
        If txtpwd.Text = "" Then
            ShowStatus("Please enter your password.", True) : txtpwd.Focus() : Exit Sub
        End If
        If cmbSession.Text = "" Then
            ShowStatus("Please select the financial year (session).", True) : cmbSession.Focus() : Exit Sub
        End If
        ShowStatus("Signing in" & ChrW(8230))
        Me.Cursor = Cursors.WaitCursor
        Me.Update()
        Try
            'GMod.DataSetRet("select nofd from nofd", "nofd")
            'GMod.nofd = GMod.ds.Tables("nofd").Rows(0)(0)
            GMod.Session = cmbSession.Text

            If GMod.Session = "" Then
                Me.Close()
            Else
                GMod.Session = cmbSession.Text
                GMod.DataSetRet("select nofd from nofd_session2 where session ='" & GMod.Q(GMod.Session) & "'", "nofd")
                GMod.nofd = GMod.ds.Tables("nofd").Rows(0)(0)
                'MsgBox(GMod.nofd)

                GMod.DataSetRet("select entry_status,noofdays from SessionMaster where Uname ='" & GMod.Q(Trim(txtuname.Text)) & "' and session ='" & GMod.Q(GMod.Session) & "'", "entry_status")
                GMod.EntryStatus = CInt(GMod.ds.Tables("entry_status").Rows(0)(0))
                GMod.nofd = CInt(GMod.ds.Tables("entry_status").Rows(0)(1))


            End If
            ' GMod.nofd = 365
            'Setting Session Start and End Date
            GMod.SessionStartDate = CDate("4/1/" & GMod.Session.Substring(0, 2))
            GMod.SessionEndDate = CDate("3/31/" & GMod.Session.Substring(2, 2))
            ' MessageBox.Show(GMod.SessionStartDate + "  " + GMod.SessionEndDate)
            GMod.DataSetRet("select getdate()", "getserverdate")
            'MsgBox(GMod.ds.Tables("getserverdate").Rows(0)(0))
            'MessageBox.Show(GMod.Getsession(CDate(GMod.ds.Tables("getserverdate").Rows(0)(0))))
            If GMod.Getsession(CDate(GMod.ds.Tables("getserverdate").Rows(0)(0))) = GMod.Session Then
                GMod.SessionCurrentDate = CDate(GMod.ds.Tables("getserverdate").Rows(0)(0))
                GMod.PerviousSession = False
                GMod.PrevSession = GMod.GetPrevsiousSession(CDate(GMod.ds.Tables("getserverdate").Rows(0)(0)))
            Else
                GMod.SessionCurrentDate = GMod.SessionEndDate
                GMod.PerviousSession = True
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            Application.Exit()
        End Try

        Dim rl As String
        Try
            Dim sqllogin As String
            ' SECURITY FIX: parameterised - a password such as  ' or '1'='1  no longer logs in.
            Dim userRow As DataTable = Db.GetTable("select role,dept,isnull(staff,0) staff , isnull(uo,0) uo , isnull(isUnAuth,0) isUnAuth  from Usertab4 where uname=@u and cast(Pwd as varchar(50)) = @p", Db.P("@u", txtuname.Text), Db.P("@p", txtpwd.Text))
            userRow.TableName = "isexists"
            If GMod.ds.Tables.Contains("isexists") Then GMod.ds.Tables.Remove("isexists")
            GMod.ds.Tables.Add(userRow)
            If GMod.ds.Tables("isexists").Rows.Count > 0 Then

                rl = GMod.ds.Tables("isexists").Rows(0)(0)
                ' company id comes from the selected row (works even though the id combo is hidden)
                Dim cmpRow As DataRowView = TryCast(cmbcmpname.SelectedItem, DataRowView)
                GMod.Cmpid = If(cmpRow IsNot Nothing, cmpRow("Cmp_id").ToString(), cmbcmpid.Text)
                GMod.Cmpname = cmbcmpname.Text
                GMod.role = GMod.ds.Tables("isexists").Rows(0)(0).ToString()
                GMod.username = txtuname.Text
                GMod.Dept = Val(GMod.ds.Tables("isexists").Rows(0)(1).ToString())
                ' GMod.Session = GMod.Getsession(dtCurrDate.Value)
                GMod.Session = cmbSession.Text
                'MsgBox(GMod.Session)
                GMod.staff1 = GMod.ds.Tables("isexists").Rows(0)("staff")
                GMod.setUpdateOpening = GMod.ds.Tables("isexists").Rows(0)("uo")
                GMod.isUnAuth = GMod.ds.Tables("isexists").Rows(0)("isUnAuth")
                Try
                    'getting ip address
                    Dim comname As String
                    Dim ipad As String
                    comname = System.Net.Dns.GetHostName()
                    ipad = System.Net.Dns.GetHostAddresses(comname)(0).ToString

                    'Inserting into User Status
                    sqllogin = "insert into UserStatus (uname, ipadd, login, status) values ("
                    sqllogin &= "'" & GMod.username & "',"
                    sqllogin &= "'" & ipad & "',"
                    sqllogin &= "'" & Now & "',"
                    sqllogin &= "'1')"
                    GMod.SqlExecuteNonQuery(sqllogin)
                Catch ex As Exception
                    Application.Exit()

                End Try


                Try

                    'Checking for already login
                    GMod.DataSetRet("select getdate()", "getserverdate")
                    sqllogin = "select login from UserStatus where uname='" & GMod.username & "'"
                    GMod.DataSetRet(sqllogin, "chck")
                    Dim n As Integer
                    n = DateDiff(DateInterval.Minute, GMod.ds.Tables("getserverdate").Rows(0)(0), GMod.ds.Tables("chck").Rows(0)(0))
                    'MsgBox(n)
                    'MsgBox(GMod.ds.Tables("getserverdate").Rows(0)(0))
                    'MsgBox(GMod.ds.Tables("chck").Rows(0)(0))

                    If Math.Abs(n) > 0 Then
                        GMod.DataSetRet("select getdate()", "serverdate")
                        Dim sql As String
                        sql = "update UserStatus set login='" & CDate(GMod.ds.Tables("serverdate").Rows(0)(0)) & "' where uname='" & GMod.username & "'"
                        GMod.SqlExecuteNonQuery(sql)
                    Else
                        MsgBox("User Already Loged In!!,Please wait", MsgBoxStyle.Information)
                        Exit Sub
                    End If
                Catch ex As Exception
                    MsgBox(ex.Message)
                    Application.Exit()
                End Try

                'getting table name
                GettingTableName()
                Dim frmmdiobj As New frmMDI
                If rl = "OPERATOR" Then

                    If GMod.Dept = 0 Then
                        'sale 
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = True
                        frmmdiobj.SaleToolStripMenuItem2.Enabled = True
                        frmmdiobj.SaleToolStripMenuItem3.Enabled = True
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = True

                        frmmdiobj.PurchaseToolStripMenuItem1.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem2.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem3.Enabled = False

                        frmmdiobj.BankToolStripMenuItem.Enabled = False
                        frmmdiobj.PaymnetOthersToolStripMenuItem.Enabled = False
                        '
                        frmmdiobj.ExpensesToolStripMenuItem.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem1.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem2.Enabled = False

                        frmmdiobj.CashCounterToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseRegisterToolStripMenuItem.Enabled = False
                        frmmdiobj.SaleProductWiseToolStripMenuItem.Enabled = True
                        frmmdiobj.FinishedProductLedgerToolStripMenuItem.Enabled = True


                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = True
                        frmmdiobj.CrDebitToolStripMenuItem.Enabled = True
                        frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = True
                        frmmdiobj.OtherSaleToolStripMenuItem.Enabled = True
                        frmmdiobj.ReceiptToolStripMenuItem1.Enabled = True
                    ElseIf GMod.Dept = 1 Then
                        'purchase

                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem2.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem3.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False


                        frmmdiobj.PurchaseToolStripMenuItem1.Enabled = True
                        frmmdiobj.PurchaseToolStripMenuItem2.Enabled = True
                        frmmdiobj.PurchaseToolStripMenuItem3.Enabled = True
                        frmmdiobj.TDSEntryToolStripMenuItem.Enabled = True

                        frmmdiobj.BankToolStripMenuItem.Enabled = False
                        frmmdiobj.PaymnetOthersToolStripMenuItem.Enabled = False


                        frmmdiobj.ExpensesToolStripMenuItem.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem1.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem2.Enabled = False

                        frmmdiobj.CashCounterToolStripMenuItem.Enabled = False

                        frmmdiobj.JournalPurchaseToolStripMenuItem.Enabled = True
                        frmmdiobj.JournalPurchaseRegisterToolStripMenuItem.Enabled = True
                        frmmdiobj.PurchaseSummaryToolStripMenuItem.Visible = True



                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = False
                        frmmdiobj.CrDebitToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherSaleToolStripMenuItem.Enabled = False
                        frmmdiobj.ReceiptToolStripMenuItem1.Enabled = False

                    ElseIf GMod.Dept = 3 Then
                        'bank
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem2.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem3.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False


                        frmmdiobj.PurchaseToolStripMenuItem1.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem2.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem3.Enabled = False

                        frmmdiobj.BankToolStripMenuItem.Enabled = True
                        frmmdiobj.PaymnetOthersToolStripMenuItem.Enabled = True


                        frmmdiobj.ExpensesToolStripMenuItem.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem1.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem2.Enabled = False

                        frmmdiobj.CashCounterToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseRegisterToolStripMenuItem.Enabled = False


                        frmmdiobj.TdsEntryToolStripMenuItem2.Visible = True


                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = False
                        frmmdiobj.CrDebitToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherSaleToolStripMenuItem.Enabled = False
                        frmmdiobj.ReceiptToolStripMenuItem1.Enabled = False

                    ElseIf GMod.Dept = 2 Then
                        'expenses
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem2.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem3.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False


                        frmmdiobj.PurchaseToolStripMenuItem1.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem2.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem3.Enabled = False

                        frmmdiobj.BankToolStripMenuItem.Enabled = False
                        frmmdiobj.PaymnetOthersToolStripMenuItem.Enabled = False


                        frmmdiobj.ExpensesToolStripMenuItem.Enabled = True
                        frmmdiobj.ExpensesToolStripMenuItem1.Enabled = True
                        frmmdiobj.ExpensesToolStripMenuItem2.Enabled = True
                        frmmdiobj.TDSEntryToolStripMenuItem1.Enabled = True


                        frmmdiobj.CashCounterToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseRegisterToolStripMenuItem.Enabled = False


                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = False
                        frmmdiobj.CrDebitToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = False


                    ElseIf GMod.Dept = 4 Then
                        'cash counter

                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem2.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem3.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False


                        frmmdiobj.PurchaseToolStripMenuItem1.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem2.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem3.Enabled = False

                        frmmdiobj.BankToolStripMenuItem.Enabled = False
                        frmmdiobj.PaymnetOthersToolStripMenuItem.Enabled = False


                        frmmdiobj.ExpensesToolStripMenuItem.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem1.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem2.Enabled = False

                        frmmdiobj.CashCounterToolStripMenuItem.Enabled = True
                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = True
                        frmmdiobj.JournalPurchaseToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseRegisterToolStripMenuItem.Enabled = False


                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = False
                        frmmdiobj.CrDebitToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherSaleToolStripMenuItem.Enabled = False
                        frmmdiobj.ReceiptToolStripMenuItem1.Enabled = True
                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = True

                    End If
                    'frmmdiobj.MasterEntriesToolStripMenuItem.Enabled = False
                    frmmdiobj.ChequeToolStripMenuItem.Enabled = False
                    frmmdiobj.tstrpassgincmp.Enabled = False
                    frmmdiobj.tstrpcmpinfo.Enabled = False
                    frmmdiobj.tstrpusermgt.Enabled = False
                    frmmdiobj.MisReporToolStripMenuItem.Enabled = False
                    'frmmdiobj.ToolStripMenuItem19.Enabled = False
                    frmmdiobj.transfer.Enabled = False
                    frmmdiobj.ToolStripMenuItem19.Enabled = False
                ElseIf rl = "VIEWER" Then
                    frmmdiobj.MasterEntriesToolStripMenuItem.Enabled = False
                    frmmdiobj.ChequeToolStripMenuItem.Enabled = False
                    frmmdiobj.TranseectionToolStripMenuItem.Enabled = False
                    frmmdiobj.DisplayPrintToolStripMenuItem.Enabled = False
                    frmmdiobj.ToolsToolStripMenuItem.Enabled = False
                    frmmdiobj.MisReporToolStripMenuItem.Enabled = False
                    frmmdiobj.VoucherSToolStripMenuItem.Enabled = True

                ElseIf rl = "LIMITED" Then

                    frmmdiobj.MasterEntriesToolStripMenuItem.Enabled = False
                    frmmdiobj.ChequeToolStripMenuItem.Enabled = False
                    frmmdiobj.TranseectionToolStripMenuItem.Enabled = False
                    frmmdiobj.DisplayPrintToolStripMenuItem.Enabled = False
                    frmmdiobj.ToolsToolStripMenuItem.Enabled = False
                    frmmdiobj.TrialToolStripMenuItem.Enabled = True
                    frmmdiobj.ToolStripMenuItem21.Enabled = False
                    frmmdiobj.MisReporToolStripMenuItem.Enabled = False
                    frmmdiobj.ToolStripMenuItem28.Enabled = False
                    frmmdiobj.ToolStripMenuItem29.Enabled = False

                ElseIf rl = "VIEWER LEVEL-1" Then

                    frmmdiobj.MasterEntriesToolStripMenuItem.Enabled = True
                    frmmdiobj.ChequeToolStripMenuItem.Enabled = False
                    frmmdiobj.TranseectionToolStripMenuItem.Enabled = False
                    frmmdiobj.ToolsToolStripMenuItem.Enabled = False
                    frmmdiobj.MisReporToolStripMenuItem.Enabled = False


                    frmmdiobj.ReceiptToolStripMenuItem.Enabled = True
                    frmmdiobj.CrDebitToolStripMenuItem.Enabled = True
                    frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = True
                    frmmdiobj.PurchaseSummaryToolStripMenuItem.Visible = True

                    If GMod.Dept = 0 Then
                        'sale 
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = True
                        frmmdiobj.SaleToolStripMenuItem2.Enabled = True
                        frmmdiobj.SaleToolStripMenuItem3.Enabled = True
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = True

                        frmmdiobj.PurchaseToolStripMenuItem1.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem2.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem3.Enabled = False

                        frmmdiobj.BankToolStripMenuItem.Enabled = False
                        frmmdiobj.PaymnetOthersToolStripMenuItem.Enabled = False


                        frmmdiobj.ExpensesToolStripMenuItem.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem1.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem2.Enabled = False

                        frmmdiobj.CashCounterToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseRegisterToolStripMenuItem.Enabled = False
                        frmmdiobj.SaleProductWiseToolStripMenuItem.Enabled = True
                        frmmdiobj.FinishedProductLedgerToolStripMenuItem.Enabled = True


                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = True
                        frmmdiobj.CrDebitToolStripMenuItem.Enabled = True
                        frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = True
                        frmmdiobj.OtherSaleToolStripMenuItem.Enabled = True
                        frmmdiobj.ReceiptToolStripMenuItem1.Enabled = True
                    ElseIf GMod.Dept = 1 Then
                        'purchase

                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem2.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem3.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False


                        frmmdiobj.PurchaseToolStripMenuItem1.Enabled = True
                        frmmdiobj.PurchaseToolStripMenuItem2.Enabled = True
                        frmmdiobj.PurchaseToolStripMenuItem3.Enabled = True
                        frmmdiobj.TDSEntryToolStripMenuItem.Enabled = True

                        frmmdiobj.BankToolStripMenuItem.Enabled = False
                        frmmdiobj.PaymnetOthersToolStripMenuItem.Enabled = False


                        frmmdiobj.ExpensesToolStripMenuItem.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem1.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem2.Enabled = False

                        frmmdiobj.CashCounterToolStripMenuItem.Enabled = False

                        frmmdiobj.JournalPurchaseToolStripMenuItem.Enabled = True
                        frmmdiobj.JournalPurchaseRegisterToolStripMenuItem.Enabled = True
                        frmmdiobj.PurchaseSummaryToolStripMenuItem.Visible = True

                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = False
                        frmmdiobj.CrDebitToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherSaleToolStripMenuItem.Enabled = False
                        frmmdiobj.ReceiptToolStripMenuItem1.Enabled = False

                    ElseIf GMod.Dept = 3 Then
                        'bank
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem2.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem3.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False


                        frmmdiobj.PurchaseToolStripMenuItem1.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem2.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem3.Enabled = False

                        frmmdiobj.BankToolStripMenuItem.Enabled = True
                        frmmdiobj.PaymnetOthersToolStripMenuItem.Enabled = True

                        frmmdiobj.ExpensesToolStripMenuItem.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem1.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem2.Enabled = False

                        frmmdiobj.CashCounterToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseRegisterToolStripMenuItem.Enabled = False


                        frmmdiobj.TdsEntryToolStripMenuItem2.Visible = True

                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = False
                        frmmdiobj.CrDebitToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherSaleToolStripMenuItem.Enabled = False
                        frmmdiobj.ReceiptToolStripMenuItem1.Enabled = False

                    ElseIf GMod.Dept = 2 Then
                        'expenses

                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem2.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem3.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False


                        frmmdiobj.PurchaseToolStripMenuItem1.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem2.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem3.Enabled = False

                        frmmdiobj.BankToolStripMenuItem.Enabled = False
                        frmmdiobj.PaymnetOthersToolStripMenuItem.Enabled = False


                        frmmdiobj.ExpensesToolStripMenuItem.Enabled = True
                        frmmdiobj.ExpensesToolStripMenuItem1.Enabled = True
                        frmmdiobj.ExpensesToolStripMenuItem2.Enabled = True
                        frmmdiobj.TDSEntryToolStripMenuItem1.Enabled = True


                        frmmdiobj.CashCounterToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseRegisterToolStripMenuItem.Enabled = False

                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = False
                        frmmdiobj.CrDebitToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherSaleToolStripMenuItem.Enabled = False
                        frmmdiobj.ReceiptToolStripMenuItem1.Enabled = False


                    ElseIf GMod.Dept = 4 Then
                        'cash counter

                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem2.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem3.Enabled = False
                        frmmdiobj.SaleToolStripMenuItem1.Enabled = False


                        frmmdiobj.PurchaseToolStripMenuItem1.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem2.Enabled = False
                        frmmdiobj.PurchaseToolStripMenuItem3.Enabled = False

                        frmmdiobj.BankToolStripMenuItem.Enabled = False
                        frmmdiobj.PaymnetOthersToolStripMenuItem.Enabled = False


                        frmmdiobj.ExpensesToolStripMenuItem.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem1.Enabled = False
                        frmmdiobj.ExpensesToolStripMenuItem2.Enabled = False

                        frmmdiobj.CashCounterToolStripMenuItem.Enabled = True
                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = True
                        frmmdiobj.JournalPurchaseToolStripMenuItem.Enabled = False
                        frmmdiobj.JournalPurchaseRegisterToolStripMenuItem.Enabled = False

                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = False
                        frmmdiobj.CrDebitToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = False
                        frmmdiobj.OtherSaleToolStripMenuItem.Enabled = False
                        frmmdiobj.ReceiptToolStripMenuItem1.Enabled = True
                        frmmdiobj.ReceiptToolStripMenuItem.Enabled = True
                    End If
                    frmmdiobj.ChqLayoutToolStripMenuItem.Visible = False


                ElseIf rl = "MIS" Then
                ElseIf rl = "ADMIN" Then
                    frmmdiobj.ChqLayoutToolStripMenuItem.Visible = True
                    frmmdiobj.FinishedProductLedgerToolStripMenuItem.Enabled = True
                    CreateTables() 'Creating table Only Administrator Righit
                    frmmdiobj.PurchaseSummaryToolStripMenuItem.Visible = True
                    frmmdiobj.TdsEntryToolStripMenuItem2.Visible = True


                    frmmdiobj.ReceiptToolStripMenuItem.Enabled = True
                    frmmdiobj.CrDebitToolStripMenuItem.Enabled = True
                    frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = True


                    frmmdiobj.ReceiptToolStripMenuItem.Enabled = True
                    frmmdiobj.CrDebitToolStripMenuItem.Enabled = True
                    frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = True

                    frmmdiobj.ReceiptToolStripMenuItem.Enabled = True
                    frmmdiobj.CrDebitToolStripMenuItem.Enabled = True
                    frmmdiobj.OtherDetectionVoucherEntryToolStripMenuItem.Enabled = True
                    frmmdiobj.OtherSaleToolStripMenuItem.Enabled = True
                    frmmdiobj.ReceiptToolStripMenuItem1.Enabled = True
                End If
                frmmdiobj.Text = GMod.Cmpname
                Me.Cursor = Cursors.Default
                Me.Hide()
                frmmdiobj.ShowDialog()
                ' Change Session: the main window closed itself and asked for another year
                If GMod.SwitchToSession IsNot Nothing Then
                    Dim target As String = GMod.SwitchToSession
                    Dim targetCompany As String = If(GMod.SwitchToCompany, GMod.Cmpid)
                    GMod.SwitchToSession = Nothing
                    GMod.SwitchToCompany = Nothing
                    frmmdiobj.Dispose()
                    Me.BeginInvoke(New MethodInvoker(Sub() ReLoginToSession(targetCompany, target)))
                End If
            Else
                Me.Cursor = Cursors.Default
                ShowStatus("User name or password is incorrect.", True)
                txtpwd.Text = ""
                txtpwd.Focus()
            End If
        Catch ex As Exception
            Me.Cursor = Cursors.Default
            MsgBox(ex.Message)
            Me.Hide()
            frmServerdetection.ShowDialog()
        End Try
        Me.Cursor = Cursors.Default


    End Sub

    Private Sub txtuname_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtuname.KeyDown
        If e.KeyCode = Keys.Enter Then cmbcmpname.Focus()
    End Sub
    ''' <summary>Signs the same user in again to another company and/or session (used by Change company / session).</summary>
    Private Sub ReLoginToSession(ByVal targetCompany As String, ByVal target As String)
        Try
            ' refresh the companies assigned to this user and select the requested one
            Dim sqlcmp As String = "select cam.Cmp_id,Cmpname from Cmp_assignment cam " &
                                   "inner join Company cmp  on cam.Cmp_id=cmp.Cmp_id where Uname='" & GMod.Q(txtuname.Text) & "'"
            GMod.DataSetRet(sqlcmp, "usercompany")
            cmbcmpname.DataSource = GMod.ds.Tables("usercompany")
            cmbcmpname.DisplayMember = "Cmpname"
            cmbcmpid.DataSource = GMod.ds.Tables("usercompany")
            cmbcmpid.DisplayMember = "Cmp_id"
            Dim cmpIndex As Integer = -1
            For r As Integer = 0 To GMod.ds.Tables("usercompany").Rows.Count - 1
                If String.Equals(GMod.ds.Tables("usercompany").Rows(r)("Cmp_id").ToString().Trim(), targetCompany, StringComparison.OrdinalIgnoreCase) Then
                    cmpIndex = r
                    Exit For
                End If
            Next
            If cmpIndex < 0 Then
                Me.Show()
                ShowStatus("Company " & targetCompany & " is not assigned to this user.", True)
                Exit Sub
            End If
            cmbcmpname.SelectedIndex = cmpIndex
            ' refresh the session list (screens may have cleared GMod.ds meanwhile)
            GMod.DataSetRet("select * from SessionMaster  where uname='" & GMod.Q(txtuname.Text) & "' order by SessionId desc", "SessionMaster")
            cmbSession.DataSource = GMod.ds.Tables("SessionMaster")
            cmbSession.DisplayMember = "session"
            Dim i As Integer = cmbSession.FindStringExact(target)
            Me.Show()
            If i < 0 Then
                ShowStatus("Session " & target & " is not available for this user.", True)
                Exit Sub
            End If
            cmbSession.SelectedIndex = i
            ShowStatus("Opening " & cmbcmpname.Text & "  " & ChrW(183) & "  session " & target & ChrW(8230))
            Me.Update()
            btnlogin_Click(btnlogin, EventArgs.Empty)
        Catch ex As Exception
            Me.Show()
            ShowStatus(ex.Message, True)
        End Try
    End Sub

    Private _lastUserLookedUp As String = Nothing
    Private Sub txtuname_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtuname.Leave
        ' nothing typed (e.g. the user clicked Exit) or same user as last time: no need to query again
        If txtuname.Text.Trim() = "" Then Exit Sub
        If String.Equals(_lastUserLookedUp, txtuname.Text, StringComparison.OrdinalIgnoreCase) AndAlso cmbcmpname.Items.Count > 0 Then Exit Sub
        _lastUserLookedUp = txtuname.Text
        Try
            Dim sqlselect As String
            sqlselect = "select top 1 uname from Usertab4 where uname='" & GMod.Q(txtuname.Text) & "'"
            GMod.DataSetRet(sqlselect, "checkuser")
            If GMod.ds.Tables("checkuser").Rows.Count > 0 Then
                sqlselect = "select cam.Cmp_id,Cmpname from Cmp_assignment cam "
                sqlselect &= "inner join Company cmp  on cam.Cmp_id=cmp.Cmp_id where Uname='" & GMod.Q(txtuname.Text) & "'"
                GMod.DataSetRet(sqlselect, "usercompany")
                cmbcmpname.DataSource = GMod.ds.Tables("usercompany")
                cmbcmpname.DisplayMember = "Cmpname"
                cmbcmpid.DataSource = GMod.ds.Tables("usercompany")
                cmbcmpid.DisplayMember = "Cmp_id"
                ShowServerInfo()
            Else
                _lastUserLookedUp = Nothing
                ShowStatus("User name '" & txtuname.Text & "' was not found.", True)
                txtuname.SelectAll()
                txtuname.Focus()
            End If
        Catch ex As Exception
            frmServerdetection.Show()
            Me.Hide()
        End Try

        'select * from SessionMaster order by id desc
        GMod.DataSetRet("select * from SessionMaster  where uname='" & GMod.Q(txtuname.Text) & "' order by SessionId desc", "SessionMaster")
        cmbSession.DataSource = GMod.ds.Tables("SessionMaster")
        cmbSession.DisplayMember = "session"



    End Sub
    Dim tablename As String, createtablesql As String, sqlresult As String
    Private Sub GettingTableName()
        tablename = "ACC_HEAD" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.ACC_HEAD = tablename

        tablename = "CHQBOOK" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.CHQBOOK = tablename

        tablename = "VENTRY" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.VENTRY = tablename

        tablename = "CHQ_ALLOT" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.CHQ_ALLOT = tablename

        tablename = "CHQ_ISSUED" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.CHQ_ISSUED = tablename

        tablename = "BANK_STATE" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.BANK_STATE = tablename

        tablename = "BANK_STATE_OPEN" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.BANK_STATE_OPEN = tablename

        'For inventory 
        tablename = "INVINFO" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.INVENTORY = tablename


    End Sub

    Private Sub CreateTables() 'Creating Tables Only ADMIN right
        'in this tha tables are created according to cmoid and session 
        'we are braking the table on the basis of company and session 

        'Greating table for account head
        ' Note: We break the table on the basis of  company and session and the initial table name will be stored in the database.
        'Eg: Company Name – Raga Finvest Pvt Ltd. (Code: RAFI)
        'Session 2007-2008 : 0708 
        'For GroupName  GroupInfo(Code)
        'Then table name will be :- GroupInfoRAFI0708


        tablename = "ACC_HEAD" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.DataSetRet("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' and TABLE_NAME='" & tablename & "'", "tableaccount")
        GMod.ACC_HEAD = tablename
        If GMod.ds.Tables("tableaccount").Rows.Count = 0 Then
            createtablesql = "CREATE TABLE " & tablename & "(" _
                             & "[Cmp_id] [varchar](5)  NOT NULL DEFAULT ('-') ," _
                             & "[account_code] [varchar](18)  NOT NULL DEFAULT ('-') primary key," _
                             & "[account_head_name] [varchar](50)  NOT NULL DEFAULT ('-')," _
                             & "[group_name] [varchar](40)  NOT NULL DEFAULT ('-') ," _
                             & "[sub_group_name] [varchar](60)  NOT NULL DEFAULT ('-') ," _
                             & "[credit_days] [varchar](50)  NOT NULL DEFAULT ('-')," _
                             & "[credit_limit] [varchar](50) NOT NULL DEFAULT ('-')," _
                             & "[opening_dr] [numeric](18, 2) NOT NULL DEFAULT ((0))," _
                             & "[opening_cr] [numeric](18, 2) NOT NULL   DEFAULT ((0))," _
                             & "[account_type] [varchar](50) NOT NULL DEFAULT ('-')," _
                             & "[address] [varchar](100) NOT NULL  DEFAULT ('-'), " _
                             & "[city] [varchar](30) NOT NULL DEFAULT ('-')," _
                             & "[state] [varchar](30) NOT NULL  DEFAULT ('-')," _
                             & "[phone] [varchar](20) NOT NULL DEFAULT ('-')," _
                             & "[pan_no] [varchar](20)  NOT NULL DEFAULT ('-')," _
                             & "[rate_of_interest] [varchar](50) NOT NULL  DEFAULT ('-')," _
                             & "[interest_rule_id] [varchar](50)  DEFAULT ('-')," _
                             & "[Area_code] [Varchar](2)  DEFAULT ('-')," _
                             & "[remark1] [varchar](100) NOT NULL DEFAULT ('-')," _
                             & "[remark2] [varchar](100) NOT NULL DEFAULT ('-')," _
                             & "[remark3] [varchar](100) NOT NULL  DEFAULT ('-') ) "
            sqlresult = GMod.SqlExecuteNonQuery(createtablesql)
        End If
        tablename = "CHQBOOK" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.DataSetRet("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' and TABLE_NAME='" & tablename & "'", "tableaccount")
        GMod.CHQBOOK = tablename
        If GMod.ds.Tables("tableaccount").Rows.Count = 0 Then
            createtablesql = "CREATE TABLE " & tablename & "(" _
                                & "[Cmp_id] [varchar](5)  NOT NULL DEFAULT ('-') ," _
                                & "[Uname]  [varchar](15)  NOT NULL DEFAULT ('-') ," _
                                & "[Entry_id]  [numeric](8)  NOT NULL DEFAULT ((0)) ," _
                                & "[Entry_date]  [Datetime] NOT NULL ," _
                                & "[Acc_head_code] [varchar](18)  NOT NULL DEFAULT ('-') ," _
                                & "[Acc_head] [varchar](50)  NOT NULL DEFAULT ('-') ," _
                                & "[Book_no] [varchar](30)  NOT NULL DEFAULT ('-') ," _
                                & "[Cheque_no_from] [varchar](15) NOT NULL  DEFAULT ('-')," _
                                & "[Cheque_no_to] [varchar](15) NOT NULL  DEFAULT ('-')," _
                                & "[isavailable] [varchar](1) NOT NULL DEFAULT ('-'))"
            'sqlresult = GMod.SqlExecuteNonQuery(createtablesql)
        End If

        tablename = "VENTRY" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.DataSetRet("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' and TABLE_NAME='" & tablename & "'", "tableaccount")
        GMod.VENTRY = tablename
        If GMod.ds.Tables("tableaccount").Rows.Count = 0 Then
            createtablesql = "CREATE TABLE " & tablename & "(" _
                                & "[Cmp_id] [varchar](5)  NOT NULL DEFAULT ('-') ," _
                                & "[Uname]  [varchar](15)  NOT NULL DEFAULT ('-') ," _
                                & "[Entry_id]  [numeric](18,2)  NOT NULL DEFAULT ((0)) ," _
                                & "[Vou_no] [varchar](12)  NOT NULL DEFAULT ('-') ," _
                                & "[Vou_type] [varchar](20)  NOT NULL DEFAULT ('-')," _
                                & "[Vou_date] [Datetime] NOT NULL ," _
                                & "[Acc_head_code] [varchar](18)  NOT NULL DEFAULT ('-') ," _
                                & "[Acc_head] [varchar](50)  NOT NULL DEFAULT ('-') ," _
                                & "[dramt] [numeric](18, 2)  NOT NULL DEFAULT ((0))," _
                                & "[cramt] [numeric](18, 2) NOT NULL DEFAULT ((0))," _
                                & "[Pay_mode] [varchar](20) NOT NULL DEFAULT ('-')," _
                                & "[Cheque_no] [varchar](15) NOT NULL  DEFAULT ('-'), " _
                                & "[Narration] [varchar](180) NOT NULL DEFAULT ('-')," _
                                & "[Group_name] [varchar](40) NOT NULL  DEFAULT ('-')," _
                                & "[Sub_group_name] [varchar](60) NOT NULL DEFAULT ('-')," _
                                & "[Ch_issue_date] [Datetime]  NULL," _
                                & "[Ch_date] [Datetime]  NULL, " _
                                & "[id] [bigint] IDENTITY(1,1) NOT NULL," _
                                & "[VoucherTax] [varchar](50) NULL," _
                                & "[TaxPer] [numeric](18, 2) NULL," _
                                & "[TaxType] [varchar](50) NULL, " _
                                & "[WinOut] [varchar](50) NULL ," _
                                & "[exp_date] [Datetime]  NULL, " _
                                & "[item_name] [varchar](50)  NULL) "
            sqlresult = GMod.SqlExecuteNonQuery(createtablesql)
        End If
        'For Tigger
        createtablesql = "create TRIGGER trdel_" & GMod.Cmpid & "_" & GMod.Session & " ON " & GMod.VENTRY & " for DELETE,update AS " _
        & " BEGIN " _
        & " declare @update_userid varchar(20) " _
        & " select @update_userid=Uname from inserted " _
        & " insert into ventrytrgg (Cmp_id, Uname, Entry_id, " _
        & "Vou_no, Vou_type, Vou_date, Acc_head_code, Acc_head, dramt,  " _
        & "cramt, Pay_mode, Cheque_no, Narration, Group_name, Sub_group_name, " _
        & "Ch_issue_date, Ch_date, Update_date, Un) " _
        & "select Cmp_id, Uname, Entry_id, " _
        & "Vou_no, Vou_type, Vou_date, Acc_head_code, Acc_head, dramt, " _
        & "cramt, Pay_mode, Cheque_no, Narration, Group_name, Sub_group_name, " _
        & "Ch_issue_date, Ch_date ,getdate(),@update_userid from deleted " _
        & " End "
        sqlresult = GMod.SqlExecuteNonQuery(createtablesql)


        tablename = "CHQ_ALLOT" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.DataSetRet("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' and TABLE_NAME='" & tablename & "'", "tableaccount")
        GMod.CHQ_ALLOT = tablename
        If GMod.ds.Tables("tableaccount").Rows.Count = 0 Then
            createtablesql = "CREATE TABLE " & tablename & "(" _
                                            & "[Cmp_id] [varchar](5)  NOT NULL DEFAULT ('-') ," _
                                            & "[U_name] [varchar](15)  NOT NULL DEFAULT ('-') ," _
                                            & "[Bank_acc_head] [varchar](50)  NOT NULL DEFAULT ('-')," _
                                            & "[Book_No] [varchar](10)  NOT NULL DEFAULT ('-')," _
                                            & "[Ch_no_from] [varchar](15)  NOT NULL DEFAULT ('-') ," _
                                            & "[Ch_no_to] [varchar](15) NOT NULL  DEFAULT ('-'), " _
                                            & "[Ch_allot_date] [Datetime] NOT NULL  , " _
                                            & "[last_che] [varchar](15) NOT NULL  DEFAULT ('-') ," _
                                            & "[Chq_remain] [int] NOT NULL  primary key(U_name,Ch_no_from,Ch_no_to))"

            'sqlresult = GMod.SqlExecuteNonQuery(createtablesql)
        End If
        tablename = "CHQ_ISSUED" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.DataSetRet("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' and TABLE_NAME='" & tablename & "'", "tableaccount")
        GMod.CHQ_ISSUED = tablename
        If GMod.ds.Tables("tableaccount").Rows.Count = 0 Then
            createtablesql = "CREATE TABLE " & tablename & "(" _
                                 & "[Cmp_id] [varchar](5)  NOT NULL DEFAULT ('-') ," _
                                 & "[Receipt_id] [varchar](12)  NOT NULL DEFAULT ('-') ," _
                                 & "[Issue_date] [Datetime]  NOT NULL ," _
                                 & "[Chq_date] [Datetime]  NOT NULL ," _
                                 & "[Cheque_no] [varchar](15)  NOT NULL DEFAULT ('-') ," _
                                 & "[BankAcc_code] [Varchar](18)  NOT NULL DEFAULT ('-')," _
                                 & "[Amount] [numeric](18, 2) NOT NULL DEFAULT ((0))," _
                                 & "[Dd_Charge_per] [numeric](6, 2) NOT NULL DEFAULT ((0))," _
                                 & "[Dd_charge] [numeric](10, 2) NOT NULL   DEFAULT ((0))," _
                                 & "[Service_tax] [numeric](10, 2) NOT NULL  DEFAULT ((0))," _
                                 & "[Receipt_amt] [numeric](18, 2)  DEFAULT ((0))," _
                                 & "[party_code] [varchar](8) NOT NULL  DEFAULT ('-')," _
                                 & "[party_name] [varchar](50) NOT NULL  DEFAULT ('-')," _
                                 & "[final_date] [datetime]," _
                                 & "[favourof] [varchar](100)," _
                                 & "[vouno] [varchar](12)," _
                                 & "[stop_bounce] [varchar](1) NOT NULL  DEFAULT ('F') ,  " _
                                 & "[Rect_type] [varchar](10) NOT NULL  DEFAULT ('-') ,  " _
                                 & "[Remark] [varchar](50) NOT NULL  DEFAULT ('-') ,  " _
                                 & "[IsPrinted] [varchar](1) NOT NULL  DEFAULT ('N')  primary key (Cheque_no,BankAcc_code)) "
            'sqlresult = GMod.SqlExecuteNonQuery(createtablesql)
        End If
        'Creating bank statement table
        tablename = "BANK_STATE" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.DataSetRet("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' and TABLE_NAME='" & tablename & "'", "tableaccount")
        GMod.BANK_STATE = tablename
        If GMod.ds.Tables("tableaccount").Rows.Count = 0 Then
            createtablesql = "CREATE TABLE " & tablename & "(" _
                                 & "[Cmp_id] [varchar](5)  NOT NULL DEFAULT ('-')," _
                                 & "[Entryid] [bigint] null," _
                                 & "[bank_acc_code] [varchar](8)  NOT NULL DEFAULT ('-')," _
                                 & "[bankedate] [datetime] NULL," _
                                 & "[paytype] [varchar](15) NULL," _
                                 & "[detail] [varchar](50) NULL," _
                                 & "[chddno] [varchar](15) NULL," _
                                 & "[dramt] [numeric](18, 2) NULL," _
                                 & "[cramt] [numeric](18, 2) NULL ," _
                                 & "[IssueDate] [Datetime] )"
            'sqlresult = GMod.SqlExecuteNonQuery(createtablesql)
        End If
        tablename = "BANK_STATE_OPEN" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.DataSetRet("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' and TABLE_NAME='" & tablename & "'", "tableaccount")
        GMod.BANK_STATE_OPEN = tablename
        If GMod.ds.Tables("tableaccount").Rows.Count = 0 Then
            createtablesql = "CREATE TABLE " & tablename & "(" _
                                 & "[bank_acc_code] [varchar](8)  NOT NULL DEFAULT ('-')," _
                                 & "[dramt] [numeric](18, 2) NULL," _
                                 & "[cramt] [numeric](18, 2) NULL )"
            'sqlresult = GMod.SqlExecuteNonQuery(createtablesql)
        End If
        'For inventory 
        tablename = "INVINFO" & "_" & GMod.Cmpid & "_" & GMod.Session
        GMod.DataSetRet("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' and TABLE_NAME='" & tablename & "'", "tableaccount")
        GMod.INVENTORY = tablename
        If GMod.ds.Tables("tableaccount").Rows.Count = 0 Then
            createtablesql = "CREATE TABLE " & tablename & "(" _
                                & "[Cmp_id] [varchar](5)  NOT NULL DEFAULT ('-') ," _
                                & "[Uname]  [varchar](15)  NOT NULL DEFAULT ('-') ," _
                                & "[Vou_no] [varchar](12)  NOT NULL DEFAULT ('-') ," _
                                & "[Vou_type] [varchar](20)  NOT NULL DEFAULT ('-')," _
                                & "[Vou_date] [Datetime] NOT NULL ," _
                                & "[Acc_head_code] [varchar](18)  NOT NULL DEFAULT ('-') ," _
                                & "[Acc_head] [varchar](50)  NOT NULL DEFAULT ('-') ," _
                                & "[ItemName] [varchar](30) NOT NULL DEFAULT ('-')," _
                                & "[Qty] [numeric](18,2) NOT NULL  DEFAULT ((0)), " _
                                & "[QtyNos] [numeric](18,2) NOT NULL  DEFAULT ((0)), " _
                                & "[Unit] [varchar](10) NOT NULL DEFAULT ('-')," _
                                & "[Rate] [numeric](18,2) NOT NULL  DEFAULT ((0)), " _
                                & "[Amount] [numeric](18,2) NOT NULL  DEFAULT ((0)), " _
                                & "[Free_Qty] [numeric](6,2) NOT NULL  DEFAULT ((0)), " _
                                & "[BillType] [varchar](10)  NULL," _
                                & "[BillNo] [varchar](10)  NULL," _
                                & "[BillDate] [Datetime]  NULL ," _
                                & "[AreaCode] [varchar](2)  NULL ," _
                                & "[Free_Per] [numeric](6,2)  NULL, " _
                                & "[Hatch_date] [Datetime]  NULL )"

            'sqlresult = GMod.SqlExecuteNonQuery(createtablesql)
        End If
        GMod.ds.Tables("tableaccount").Dispose()
        CreateTableDayBook()
    End Sub
    Private Sub txtpwd_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtpwd.KeyDown
        If e.KeyCode = Keys.Enter Then cmbSession.Focus()
    End Sub
    Private Sub cmbcmpname_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles cmbcmpname.KeyDown
        If e.KeyCode = Keys.Enter Then txtpwd.Focus()
    End Sub

    Public Sub CreateTableDayBook()
        GMod.DataSetRet("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' and TABLE_NAME='Daybook'", "tableaccountbook")
        If GMod.ds.Tables("tableaccountbook").Rows.Count = 0 Then
            Dim daybook As String
            daybook = "CREATE TABLE [Daybook](" _
            & " [cmpid] [varchar](4), " _
            & "[Acc_code] [varchar](18)," _
            & "[Acc_name] [varchar](40), " _
            & "[narration] [varchar](500)," _
            & "[Group_name] [varchar](40), " _
            & "[Sub_Group] [varchar](40)," _
            & "[Vtype] [varchar](30)," _
            & "[Vouno] [varchar](18)," _
            & "[dramt] [numeric](18, 2) NULL," _
            & "[cramt] [numeric](18, 2) NULL," _
            & "[Cheque No] [varchar](100)," _
            & "[Vdate] [datetime] NULL," _
            & "[entryid] [int] NULL, " _
            & "[Uname] [varchar](50)  NULL, " _
            & "[id] [bigint] IDENTITY(1,1) NOT NULL )"
            GMod.SqlExecuteNonQuery(daybook)
        End If
        GMod.ds.Tables("tableaccountbook").Dispose()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim CLS As New Module1.Num2Words
        MsgBox(CLS.NUMMM(InputBox("Nm")))
    End Sub

    Private Sub cmbSession_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles cmbSession.KeyDown
        If e.KeyCode = Keys.Enter Then btnlogin_Click(sender, e)
    End Sub

    Private Sub cmbSession_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbSession.Leave
        If cmbSession.Text = "" Then
            MsgBox("Please Select session ")
            cmbSession.Focus()
            btnlogin.Enabled = False
        Else
            btnlogin.Enabled = True
        End If
    End Sub
    Private Sub cmbcmpname_Leave(sender As Object, e As EventArgs) Handles cmbcmpname.Leave
        If cmbSession.Items.Count > 0 AndAlso cmbSession.SelectedIndex < 0 Then cmbSession.SelectedIndex = 0
    End Sub

   
    Private Sub txtuname_TextChanged(sender As Object, e As EventArgs) Handles txtuname.TextChanged

    End Sub
End Class
