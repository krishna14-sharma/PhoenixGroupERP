Option Strict On
Option Explicit On

Imports System.Data.SqlClient

''' <summary>
''' Small, fast, parameterised data-access helper.
''' Use it for new code instead of building SQL with string concatenation:
''' parameters are safe against SQL injection, let SQL Server re-use query
''' plans (faster) and avoid date-format problems.
'''
''' Example:
'''   Dim t = Db.GetTable("select * from Usertab4 where uname=@u", Db.P("@u", txtuname.Text))
'''   Dim n = CInt(Db.Scalar("select count(*) from Vtype where session=@s", Db.P("@s", GMod.Session)))
''' </summary>
Public NotInheritable Class Db

    Private Sub New()
    End Sub

    Public Shared Function P(ByVal name As String, ByVal value As Object) As SqlParameter
        Return New SqlParameter(name, If(value, DBNull.Value))
    End Function

    Private Shared Function NewCommand(ByVal conn As SqlConnection, ByVal sql As String, ByVal params As SqlParameter()) As SqlCommand
        Dim cmd As New SqlCommand(sql, conn)
        cmd.CommandTimeout = GMod.QueryTimeoutSeconds
        If params IsNot Nothing Then
            For Each prm As SqlParameter In params
                If prm IsNot Nothing Then cmd.Parameters.Add(prm)
            Next
        End If
        Return cmd
    End Function

    ''' <summary>Runs a SELECT and returns a new DataTable.</summary>
    Public Shared Function GetTable(ByVal sql As String, ByVal ParamArray params As SqlParameter()) As DataTable
        Dim t As New DataTable()
        Dim qStart As Long = Global.System.Diagnostics.Stopwatch.GetTimestamp()
        Try
            Using conn As New SqlConnection(GMod.Connstr)
                Using cmd As SqlCommand = NewCommand(conn, sql, params)
                    conn.Open()
                    Using rdr As SqlDataReader = cmd.ExecuteReader()
                        t.Load(rdr)
                    End Using
                End Using
            End Using
            Return t
        Finally
            FormTimings.RecordQuery(Global.System.Diagnostics.Stopwatch.GetTimestamp() - qStart)
        End Try
    End Function

    ''' <summary>Runs a SELECT and stores the result in GMod.ds (same contract as GMod.DataSetRet).</summary>
    Public Shared Function Fill(ByVal tableName As String, ByVal sql As String, ByVal ParamArray params As SqlParameter()) As DataTable
        Dim fresh As DataTable = GetTable(sql, params)
        fresh.TableName = tableName
        If GMod.ds.Tables.Contains(tableName) Then GMod.ds.Tables.Remove(tableName)
        GMod.ds.Tables.Add(fresh)
        Return fresh
    End Function

    ''' <summary>First column of the first row (Nothing when there are no rows).</summary>
    Public Shared Function Scalar(ByVal sql As String, ByVal ParamArray params As SqlParameter()) As Object
        Dim qStart As Long = Global.System.Diagnostics.Stopwatch.GetTimestamp()
        Try
            Using conn As New SqlConnection(GMod.Connstr)
                Using cmd As SqlCommand = NewCommand(conn, sql, params)
                    conn.Open()
                    Dim v As Object = cmd.ExecuteScalar()
                    If v Is DBNull.Value Then Return Nothing
                    Return v
                End Using
            End Using
        Finally
            FormTimings.RecordQuery(Global.System.Diagnostics.Stopwatch.GetTimestamp() - qStart)
        End Try
    End Function

    ''' <summary>INSERT / UPDATE / DELETE on its own pooled connection. Returns rows affected.</summary>
    Public Shared Function Execute(ByVal sql As String, ByVal ParamArray params As SqlParameter()) As Integer
        Dim qStart As Long = Global.System.Diagnostics.Stopwatch.GetTimestamp()
        Try
            Using conn As New SqlConnection(GMod.Connstr)
                Using cmd As SqlCommand = NewCommand(conn, sql, params)
                    conn.Open()
                    Return cmd.ExecuteNonQuery()
                End Using
            End Using
        Finally
            FormTimings.RecordQuery(Global.System.Diagnostics.Stopwatch.GetTimestamp() - qStart)
        End Try
    End Function

    '---------------------------------------------------------------------
    ' Server clock. Many screens ask SQL Server for getdate(); this keeps
    ' the offset in memory so the UI (clock, status bar) needs no round-trip.
    '---------------------------------------------------------------------
    ' (one implementation for the whole application: GMod.ServerNow, also used by DataSetRet)
    Public Shared Function ServerNow() As DateTime
        Try
            Return GMod.ServerNow()
        Catch
            Return DateTime.Now
        End Try
    End Function

End Class
