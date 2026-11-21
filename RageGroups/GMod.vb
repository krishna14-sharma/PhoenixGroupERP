Imports System.Data.SqlClient
Module GMod
    Public SessionStartDate As DateTime
    Public SessionEndDate As DateTime
    Public SessionCurrentDate As DateTime
    Public PerviousSession As Boolean = False
    Public EntryStatus As Integer
    Public PrevSession As String

    Public tdsflag As Boolean = False
    Public tdsdcode As String = ""
    Public usrname As String = ""
    Public othcmp As Double = 0
    Public staff1 As Integer = 0
    Public Connstr As String = ""
    Public userid As Integer = 0
    Public username As String = ""
    Public role As String = ""
    Public status As String = ""
    Public Cmpid As String = ""
    Public Cmpname As String = ""
    Public Session As String = ""
    Public InventoryFound As Boolean = False
    Public nxtCR As Int64
    Public setDate As DateTime
    Public nofd As Integer
    Public isUnAuth As Integer
    ''' <summary>Set by Change Session: the main window closes and the login re-opens the app in this session.</summary>
    Public SwitchToSession As String = Nothing
    ''' <summary>Company (Cmp_id) to open together with <see cref="SwitchToSession"/>.</summary>
    Public SwitchToCompany As String = Nothing
    Public setUpdateOpening As Integer
    'For DataBases
    ' The shared connection used by the save/transaction code all over the application.
    ' It is now self-healing: if a screen closed it (e.g. frmDMPosting) or the network /
    ' SQL Server dropped it, it is transparently re-opened on next use instead of making
    ' every later save fail until the application is restarted.
    Private WithEvents _sqlConn As New SqlConnection()
    Private _sqlConnShutdown As Boolean = False
    Public ReadOnly Property SqlConn As SqlConnection
        Get
            EnsureSharedConnection()
            Return _sqlConn
        End Get
    End Property

    Private Sub EnsureSharedConnection()
        If _sqlConnShutdown Then Return
        Try
            If _sqlConn.State = ConnectionState.Broken Then _sqlConn.Close()
            If _sqlConn.State = ConnectionState.Closed Then
                If String.IsNullOrEmpty(_sqlConn.ConnectionString) AndAlso Not String.IsNullOrEmpty(Connstr) Then
                    _sqlConn.ConnectionString = Connstr
                End If
                If Not String.IsNullOrEmpty(_sqlConn.ConnectionString) Then _sqlConn.Open()
            End If
        Catch
            ' the caller gets the real error when it uses the connection
        End Try
    End Sub

    ''' <summary>
    ''' Opens the shared connection with the current <see cref="Connstr"/>.
    ''' Unlike the self-healing getter, this throws the real error (used by the login screen).
    ''' </summary>
    Public Sub OpenSharedConnection()
        _sqlConnShutdown = False
        If _sqlConn.State = ConnectionState.Open AndAlso String.Equals(_sqlConn.ConnectionString, Connstr, StringComparison.Ordinal) Then Return
        If _sqlConn.State <> ConnectionState.Closed Then _sqlConn.Close()
        _sqlConn.ConnectionString = Connstr
        _sqlConn.Open()
        ResetServerClock()
    End Sub

    ''' <summary>Doubles single quotes so a value can be embedded in a T-SQL string literal.</summary>
    Public Function Q(ByVal value As String) As String
        If value Is Nothing Then Return ""
        Return value.Replace("'", "''")
    End Function

    ''' <summary>Call once when the application exits (closes the shared connection for good).</summary>
    Public Sub ShutdownConnection()
        _sqlConnShutdown = True
        Try
            _sqlConn.Close()
            _sqlConn.Dispose()
        Catch
        End Try
    End Sub

    ''' <summary>Timeout (seconds) for read queries - large reports no longer stop at the 30 s default.</summary>
    Public QueryTimeoutSeconds As Integer = 600

    Public ds As New DataSet
    Public Dept As Integer
    Public _CASHCOUNTER As Integer = 0
    Public cashcount As Integer = 0
    Public az As Integer = 0
    'For tables 
    Public ACC_HEAD As String
    Public VENTRY As String
    Public CHQ_ALLOT As String
    Public CHQ_ISSUED As String
    Public BANK_STATE As String
    Public BANK_STATE_OPEN As String
    Public CHQBOOK As String
    Public INVENTORY As String
    'For multiple chequ entry
    Public Chq_no As String
    Public ReciptId As String
    Public no_of_cheque As String
    Public ramdom As Boolean = False
    Public favourof As String
    Public damount As Double
    Public chq_date As DateTime
    Public VouNo As String
    Public Vtype As String
    'Area Code 
    Public AreaCode As String = "JB"

    'Cr 
    Public CrVouNo As String
    Public CrVouType As String

    Public vou_type As String = ""
    Public vou_no As String = ""
    Public setModifyFlag As Boolean = False

    'VoucherTax
    Public VoucherTax As String = ""
    'For Debit Note 
    Public BillNo As String = ""
    Public party_name As String = ""
    Public Billdate As Date
    ''' <summary>
    ''' Runs a SELECT and stores the result in GMod.ds under <paramref name="tablename"/>.
    ''' Optimised version:
    '''  * one round-trip with a forward-only reader (no intermediate copies);
    '''  * if the table already exists with the same columns it is refilled in place,
    '''    so controls bound to it refresh automatically (same as before);
    '''  * if the columns differ, the table is replaced. Previously old columns were kept
    '''    and piled up, so Rows(0)(0) could point at a stale, empty column;
    '''  * multiple result sets keep the old naming (name, name1, name2 ...).
    ''' </summary>
    Public Sub DataSetRet(ByVal sql As String, ByVal tablename As String)
        ' Fast path: "select getdate()" is issued ~80 times across the screens.
        ' It is answered from a cached server-clock offset (no network round-trip).
        If IsPlainGetDate(sql) Then
            Try
                FillServerDate(tablename)
                Return
            Catch
                ' fall through to the normal query
            End Try
        End If
        Dim qStart As Long = Global.System.Diagnostics.Stopwatch.GetTimestamp()
        Try
            Using conn As New SqlConnection(Connstr)
                Using cmd As New SqlCommand(sql, conn)
                    cmd.CommandTimeout = QueryTimeoutSeconds
                    conn.Open()
                    Using rdr As SqlDataReader = cmd.ExecuteReader()
                        Dim index As Integer = 0
                        Do
                            Dim name As String = If(index = 0, tablename, tablename & index.ToString())
                            LoadIntoDataSet(rdr, name)
                            index += 1
                        Loop While Not rdr.IsClosed AndAlso index < 50
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ' same as before: a failed query leaves an EMPTY table, never stale rows
            Try
                If ds.Tables.Contains(tablename) Then ds.Tables(tablename).Clear()
            Catch
            End Try
            MessageBox.Show("Error: " & ex.Message, "Phoenix Group ERP", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            FormTimings.RecordQuery(Global.System.Diagnostics.Stopwatch.GetTimestamp() - qStart)
        End Try
    End Sub

    Private Sub LoadIntoDataSet(ByVal rdr As SqlDataReader, ByVal name As String)
        Dim existing As DataTable = Nothing
        If ds.Tables.Contains(name) Then existing = ds.Tables(name)

        If rdr.FieldCount = 0 Then
            ' statement returned no rows-set (UPDATE / EXEC run through DataSetRet)
            If existing IsNot Nothing Then existing.Clear()
            If Not rdr.NextResult() Then rdr.Close()
            Return
        End If

        If existing IsNot Nothing AndAlso SameColumns(existing, rdr) Then
            existing.BeginLoadData()
            existing.Clear()
            existing.Load(rdr)
            existing.EndLoadData()
            Return
        End If

        Dim fresh As New DataTable(name)
        fresh.BeginLoadData()
        fresh.Load(rdr)
        fresh.EndLoadData()
        If existing IsNot Nothing Then
            Try
                ds.Tables.Remove(existing)
            Catch
                ' the table takes part in a relation - fall back to the legacy refill
                existing.Clear()
                existing.Merge(fresh, False, MissingSchemaAction.Add)
                Return
            End Try
        End If
        ds.Tables.Add(fresh)
    End Sub

    Private Function SameColumns(ByVal t As DataTable, ByVal rdr As SqlDataReader) As Boolean
        If t.Columns.Count <> rdr.FieldCount Then Return False
        For i As Integer = 0 To rdr.FieldCount - 1
            Dim col As DataColumn = t.Columns(i)
            If Not String.Equals(col.ColumnName, rdr.GetName(i), StringComparison.OrdinalIgnoreCase) Then Return False
            If col.DataType IsNot rdr.GetFieldType(i) Then Return False
        Next
        Return True
    End Function

    '=====================================================================
    ' Server clock cache
    '=====================================================================
    Private _serverOffsetTicks As Long
    Private _serverOffsetReadAt As DateTime = DateTime.MinValue
    Private ReadOnly _clockLock As New Object()
    ''' <summary>How long the measured server/PC clock difference is trusted.</summary>
    Private ReadOnly ServerClockRefresh As TimeSpan = TimeSpan.FromMinutes(10)

    Private Function IsPlainGetDate(ByVal sql As String) As Boolean
        If sql Is Nothing OrElse sql.Length > 40 Then Return False
        Dim s As String = System.Text.RegularExpressions.Regex.Replace(sql, "\s+", " ").Trim().TrimEnd(";"c).Trim()
        Return String.Equals(s, "select getdate()", StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' Current SQL Server date/time. The server is asked at most once every
    ''' 10 minutes; in between, the value is derived from the local clock plus
    ''' the measured offset (accurate to a few milliseconds).
    ''' </summary>
    Public Function ServerNow() As DateTime
        SyncLock _clockLock
            Dim localNow As DateTime = DateTime.Now
            If _serverOffsetReadAt = DateTime.MinValue OrElse (localNow - _serverOffsetReadAt).Duration() > ServerClockRefresh Then
                Using conn As New SqlConnection(Connstr)
                    Using cmd As New SqlCommand("select getdate()", conn)
                        cmd.CommandTimeout = 30
                        conn.Open()
                        Dim t0 As DateTime = DateTime.Now
                        Dim server As DateTime = CDate(cmd.ExecuteScalar())
                        Dim t1 As DateTime = DateTime.Now
                        Dim localMid As DateTime = t0.AddTicks((t1 - t0).Ticks \ 2)
                        _serverOffsetTicks = (server - localMid).Ticks
                        _serverOffsetReadAt = t1
                    End Using
                End Using
                localNow = DateTime.Now
            End If
            ' SQL Server datetime has ~3 ms precision; keep milliseconds only
            Dim v As DateTime = localNow.AddTicks(_serverOffsetTicks)
            Return New DateTime(v.Year, v.Month, v.Day, v.Hour, v.Minute, v.Second, v.Millisecond)
        End SyncLock
    End Function

    ''' <summary>Forces the next ServerNow() to re-read the server clock.</summary>
    Public Sub ResetServerClock()
        SyncLock _clockLock
            _serverOffsetReadAt = DateTime.MinValue
        End SyncLock
    End Sub

    Private Sub FillServerDate(ByVal tablename As String)
        Dim stamp As DateTime = ServerNow()
        Dim t As DataTable = Nothing
        If ds.Tables.Contains(tablename) Then t = ds.Tables(tablename)
        If t IsNot Nothing AndAlso t.Columns.Count = 1 AndAlso t.Columns(0).DataType Is GetType(DateTime) Then
            t.Clear()
        Else
            If t IsNot Nothing Then
                Try
                    ds.Tables.Remove(t)
                Catch
                    t.Clear()
                    t.Rows.Add(t.NewRow())
                    t.Rows(0)(0) = stamp
                    Return
                End Try
            End If
            t = New DataTable(tablename)
            t.Columns.Add("Column1", GetType(DateTime))   ' same column name SqlClient gives an unnamed column
            ds.Tables.Add(t)
        End If
        t.Rows.Add(stamp)
        t.AcceptChanges()
    End Sub

    ' Parameterised queries for new code: see Data\Db.vb (Db.GetTable / Db.Scalar / Db.Execute).

    Public Function SqlExecuteNonQuery(ByVal sql As String) As String

        'If GMod.SqlConn.State = ConnectionState.Closed Then
        '    GMod.SqlConn.Open()
        'End If
        Dim retval As String = ""
        Dim qStart As Long = Global.System.Diagnostics.Stopwatch.GetTimestamp()
        Try
            Using sqlcmd As New SqlCommand(sql, SqlConn)   ' SqlConn re-opens itself if it was dropped
                sqlcmd.CommandTimeout = 3000
                sqlcmd.ExecuteNonQuery()
            End Using
            retval = "SUCCESS"
        Catch ex As Exception
            retval = ex.Message
        Finally
            FormTimings.RecordQuery(Global.System.Diagnostics.Stopwatch.GetTimestamp() - qStart)
        End Try
        'If GMod.SqlConn.State = ConnectionState.Open Then
        '    GMod.SqlConn.Close()
        'End If
        Return retval

    End Function
    Public Function Getsession(ByVal statusdate As DateTime) As String
        Dim year, x As Integer
        Dim yearstrsuff, yearstrpfx As String
        If statusdate.Month > 3 Then
            year = statusdate.Year
            x = year + 1
            yearstrsuff = year.ToString.Substring(2, 2)
            yearstrpfx = x.ToString.Substring(2, 2)
            Return yearstrsuff & yearstrpfx
        Else
            year = statusdate.Year - 1
            x = year + 1
            yearstrsuff = year.ToString.Substring(2, 2)
            yearstrpfx = x.ToString.Substring(2, 2)
            Return yearstrsuff & yearstrpfx
        End If
    End Function

    Public Function GetPrevsiousSession(ByVal statusdate As DateTime) As String
        Dim year, x As Integer
        Dim yearstrsuff, yearstrpfx As String
        If statusdate.Month > 3 Then
            year = statusdate.Year
            x = year + 1
            yearstrsuff = year.ToString.Substring(2, 2) - 1
            yearstrpfx = x.ToString.Substring(2, 2) - 1
            Return yearstrsuff & yearstrpfx
        Else
            year = statusdate.Year - 1
            x = year + 1
            yearstrsuff = year.ToString.Substring(2, 2) - 1
            yearstrpfx = x.ToString.Substring(2, 2) - 1
            Return yearstrsuff & yearstrpfx
        End If
    End Function

    Public Function EncryptionStr(ByVal strval As String, ByVal isencrypt As Boolean) As String
        If String.IsNullOrEmpty(strval) Then Return ""
        Dim addval As Integer = If(isencrypt, 11, -11)
        Dim sb As New System.Text.StringBuilder(strval.Length)
        For i As Integer = 0 To strval.Length - 1
            sb.Append(Chr(Asc(strval.Chars(i)) + addval))
        Next
        Return sb.ToString()
    End Function
    Public sp1(3) As String
    Function spliterbook(ByVal st As String)
        Try
            Dim word() As String
            word = st.Split(" ")
            sp1(0) = ""
            sp1(1) = ""
            sp1(2) = ""
            Dim i As Integer
            For i = 0 To word.Length - 1
                'earlier it was 55
                If sp1(0).Length < 50 Then
                    sp1(0) = sp1(0) & word(i) & " "
                Else
                    sp1(1) = sp1(1) & word(i) & " "
                End If
            Next
            word = sp1(1).Split(" ")
            sp1(1) = ""
            For i = 0 To word.Length - 1
                If sp1(1).Length < 50 Then
                    sp1(1) = sp1(1) & word(i) & " "
                Else
                    sp1(2) = sp1(2) & word(i) & " "
                End If
            Next
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Function

    Public Sub Fill_Log(ByVal cmp_id As String, ByVal vou_no As String, ByVal vou_type As String, ByVal vou_date As Date, ByVal Modfy_date As Date, ByVal session As String, ByVal TYPE As String, ByVal username As String)
        'Cmp_id, vou_no, vou_type, vou_date, Modfy_date, session, usename
        Dim InsertLogSql As String
        InsertLogSql = " insert into Log_Table (Cmp_id, vou_no, vou_type, vou_date, Modfy_date, session,TYPE, usename) values ("
        InsertLogSql &= "'" & cmp_id & "',"
        InsertLogSql &= "'" & vou_no & "',"
        InsertLogSql &= "'" & vou_type & "',"
        InsertLogSql &= "'" & vou_date & "',"
        InsertLogSql &= "'" & Modfy_date & "',"
        InsertLogSql &= "'" & session & "',"
        InsertLogSql &= "'" & TYPE & "',"
        InsertLogSql &= "'" & username & "')"
        'MsgBox(InsertLogSql)
        GMod.SqlExecuteNonQuery(InsertLogSql)
    End Sub
    Public Sub Fill_Log_Head(ByVal cmp_id As String, ByVal acc_code As String, ByVal acc_head As String, ByVal Modfy_date As Date, ByVal session As String, ByVal TYPE As String, ByVal username As String)
        'cmp_id, acc_code, acc_head, Modfy_date, session, TYPE, usename
        Dim InsertLogSql As String
        InsertLogSql = " insert into dbo.Log_Head_Table (cmp_id, acc_code, acc_head, Modfy_date, session, TYPE, usename) values ("
        InsertLogSql &= "'" & cmp_id & "',"
        InsertLogSql &= "'" & acc_code & "',"
        InsertLogSql &= "'" & acc_head & "',"
        InsertLogSql &= "'" & Modfy_date & "',"
        InsertLogSql &= "'" & session & "',"
        InsertLogSql &= "'" & TYPE & "',"
        InsertLogSql &= "'" & username & "')"
        'MsgBox(InsertLogSql)
        GMod.SqlExecuteNonQuery(InsertLogSql)
    End Sub
    Public Function LockDataCheck(ByVal vou_no As String, ByVal locksess As String, ByVal vou_type As String) As Boolean
        Dim sqls As String
        sqls = "select vou_date from " & GMod.VENTRY & " where vou_no='" & vou_no & "' and vou_type='" & vou_type & "'"
        GMod.DataSetRet(sqls, "VD")
        If GMod.ds.Tables("VD").Rows.Count > 0 Then
            'sqls = "select max(vou_date) from " & GMod.VENTRY & " where vou_type='" & vou_type & "'"
            sqls = "select getdate()"
            GMod.DataSetRet(sqls, "LD")
            'MsgBox(DateDiff(DateInterval.Day, CDate(GMod.ds.Tables("LD").Rows(0)(0)), CDate(GMod.ds.Tables("VD").Rows(0)(0))))
            If Math.Abs(DateDiff(DateInterval.Day, CDate(GMod.ds.Tables("LD").Rows(0)(0)), CDate(GMod.ds.Tables("VD").Rows(0)(0)))) = 0 Then
                Return False
            Else
                'MsgBox(CDate(GMod.ds.Tables("LD").Rows(0)(0)).AddDays(-10))
                Return True
            End If
        Else
            MsgBox("Voucher doen't exists")
        End If
    End Function
    Public Function LockDataChecks(ByVal vou_no As String, ByVal locksess As String, ByVal vou_type As String) As Boolean
        Dim sqls As String
        sqls = "select billdate from printdata where vou_no='" & vou_no & "' and vou_type='" & vou_type & "' and cmp_id='" & GMod.Cmpid & "' and session='" & GMod.Session & "'"
        GMod.DataSetRet(sqls, "VD")
        If GMod.ds.Tables("VD").Rows.Count > 0 Then
            sqls = "select max(billdate) from printdata  where vou_type='" & vou_type & "' and cmp_id='" & GMod.Cmpid & "'"
            GMod.DataSetRet(sqls, "LD")
            'MsgBox(DateDiff(DateInterval.Day, CDate(GMod.ds.Tables("LD").Rows(0)(0)), CDate(GMod.ds.Tables("VD").Rows(0)(0))))
            If Math.Abs(DateDiff(DateInterval.Day, CDate(GMod.ds.Tables("LD").Rows(0)(0)), CDate(GMod.ds.Tables("VD").Rows(0)(0)))) > 90 Then
                Return False
            Else
                'MsgBox(CDate(GMod.ds.Tables("LD").Rows(0)(0)).AddDays(-10))
                Return True
            End If
        Else
            MsgBox("Voucher doen't exists")
        End If
    End Function
    
End Module
