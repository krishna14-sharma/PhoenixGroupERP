Option Strict On
Option Explicit On

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Threading

''' <summary>
''' Measures how long every screen takes to open (from "New" until it is visible)
''' and how many database queries it ran while opening. One line per opening is
''' appended to %LOCALAPPDATA%\PhoenixGroupERP\logs\screen-open-times-yyyyMM.csv,
''' so the slowest screens can be found from real use and fixed one by one.
''' Switch off with  &lt;add key="UiLogScreenTimes" value="false"/&gt;  in app.config.
''' </summary>
Public NotInheritable Class FormTimings

    Private Sub New()
    End Sub

    Private Shared _queryCount As Integer
    Private Shared _queryTicks As Long
    Private Shared _enabled As Integer = -1

    Public Shared ReadOnly Property Enabled As Boolean
        Get
            If _enabled = -1 Then
                Dim isOn As Boolean = True
                Try
                    Dim v As String = Global.System.Configuration.ConfigurationManager.AppSettings("UiLogScreenTimes")
                    If v IsNot Nothing AndAlso v.Trim().Equals("false", StringComparison.OrdinalIgnoreCase) Then isOn = False
                Catch
                End Try
                _enabled = If(isOn, 1, 0)
            End If
            Return _enabled = 1
        End Get
    End Property

    ''' <summary>Called by GMod.DataSetRet / SqlExecuteNonQuery / Db after each database call.</summary>
    Public Shared Sub RecordQuery(ByVal elapsedStopwatchTicks As Long)
        Interlocked.Increment(_queryCount)
        Interlocked.Add(_queryTicks, elapsedStopwatchTicks)
    End Sub

    Public Shared ReadOnly Property QueryCount As Integer
        Get
            Return Thread.VolatileRead(_queryCount)
        End Get
    End Property

    Public Shared ReadOnly Property QueryTicks As Long
        Get
            Return Interlocked.Read(_queryTicks)
        End Get
    End Property

    Public Shared Function TicksToMs(ByVal ticks As Long) As Long
        Return CLng(ticks * 1000.0 / Stopwatch.Frequency)
    End Function

    Public Shared ReadOnly Property LogFolder As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PhoenixGroupERP", "logs")
        End Get
    End Property

    Private Shared ReadOnly Property LogFile As String
        Get
            Return Path.Combine(LogFolder, "screen-open-times-" & DateTime.Now.ToString("yyyyMM") & ".csv")
        End Get
    End Property

    Private Shared ReadOnly WriteLock As New Object()

    Private Shared Function Csv(ByVal s As String) As String
        If s Is Nothing Then Return ""
        Return """" & s.Replace("""", """""").Replace(ControlCharsNewLine, " ") & """"
    End Function
    Private Shared ReadOnly ControlCharsNewLine As String = Environment.NewLine

    ''' <summary>Appends one measurement (never throws).</summary>
    Public Shared Sub Write(ByVal f As Form, ByVal totalMs As Long, ByVal loadMs As Long, ByVal queries As Integer, ByVal queryMs As Long)
        If Not Enabled Then Return
        Try
            SyncLock WriteLock
                Directory.CreateDirectory(LogFolder)
                Dim file As String = LogFile
                Dim sb As New StringBuilder()
                If Not Global.System.IO.File.Exists(file) Then sb.AppendLine("time,screen,title,total_ms,load_ms,queries,query_ms,user,company,session")
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append(","c)
                sb.Append(Csv(f.GetType().Name)).Append(","c).Append(Csv(f.Text)).Append(","c)
                sb.Append(totalMs).Append(","c).Append(loadMs).Append(","c).Append(queries).Append(","c).Append(queryMs).Append(","c)
                sb.Append(Csv(GMod.username)).Append(","c).Append(Csv(GMod.Cmpid)).Append(","c).Append(Csv(GMod.Session)).AppendLine()
                Global.System.IO.File.AppendAllText(file, sb.ToString())
            End SyncLock
        Catch
        End Try
    End Sub

    '---------------------------------------------------------------------
    ' Summary window (user menu > Screen open times...)
    '---------------------------------------------------------------------
    Private Class Stat
        Public Screen As String
        Public Title As String
        Public Count As Integer
        Public TotalMs As Long
        Public MaxMs As Long
        Public Queries As Long
    End Class

    Public Shared Sub ShowSummary(ByVal owner As IWin32Window)
        Dim stats As New Dictionary(Of String, Stat)(StringComparer.OrdinalIgnoreCase)
        Try
            If Directory.Exists(LogFolder) Then
                For Each file As String In Directory.GetFiles(LogFolder, "screen-open-times-*.csv")
                    For Each line As String In Global.System.IO.File.ReadAllLines(file)
                        Dim p As List(Of String) = SplitCsv(line)
                        Dim total As Long, q As Integer
                        If p.Count < 7 OrElse Not Long.TryParse(p(3), total) Then Continue For
                        Integer.TryParse(p(5), q)
                        Dim s As Stat = Nothing
                        If Not stats.TryGetValue(p(1), s) Then
                            s = New Stat() With {.Screen = p(1), .Title = p(2)}
                            stats(p(1)) = s
                        End If
                        s.Count += 1
                        s.TotalMs += total
                        s.MaxMs = Math.Max(s.MaxMs, total)
                        s.Queries += q
                    Next
                Next
            End If
        Catch
        End Try

        Dim list As New List(Of Stat)(stats.Values)
        list.Sort(Function(a, b) (b.TotalMs \ Math.Max(1, b.Count)).CompareTo(a.TotalMs \ Math.Max(1, a.Count)))

        Using dlg As New ThemedForm()
            dlg.Text = "Screen open times (slowest first)"
            dlg.StartPosition = FormStartPosition.CenterParent
            dlg.ShowInTaskbar = False
            dlg.MinimizeBox = False
            dlg.ClientSize = New Size(Dpi.S(860), Dpi.S(520))
            Dim grid As New DataGridView() With {.Dock = DockStyle.Fill, .ReadOnly = True, .AllowUserToAddRows = False,
                .AllowUserToDeleteRows = False, .RowHeadersVisible = False, .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill}
            grid.Columns.Add("screen", "Screen")
            grid.Columns.Add("title", "Title")
            grid.Columns.Add("opened", "Times opened")
            grid.Columns.Add("avg", "Average (sec)")
            grid.Columns.Add("max", "Slowest (sec)")
            grid.Columns.Add("q", "Queries per open")
            grid.Columns(1).FillWeight = 220
            For Each s As Stat In list
                grid.Rows.Add(s.Screen, s.Title, s.Count, (s.TotalMs / Math.Max(1, s.Count) / 1000.0).ToString("0.00"),
                              (s.MaxMs / 1000.0).ToString("0.00"), (s.Queries / Math.Max(1, s.Count)).ToString("0.0"))
            Next
            ' give the panel its final width BEFORE adding right-anchored buttons (else they drift off-screen)
            Dim bottom As New Panel() With {.Width = dlg.ClientSize.Width, .Height = Dpi.S(52), .Dock = DockStyle.Bottom}
            Dim info As New Label() With {.AutoSize = False, .Location = New Point(Dpi.S(12), Dpi.S(8)), .Size = New Size(Dpi.S(560), Dpi.S(36)),
                .Text = If(list.Count = 0, "No measurements yet - open a few screens and look again.",
                           "Send the CSV files from the log folder to your developer to speed up the slowest screens.")}
            Dim openFolder As New Button() With {.Text = "Open log folder", .Size = New Size(Dpi.S(140), Dpi.S(32)),
                .Location = New Point(Dpi.S(590), Dpi.S(10)), .Anchor = AnchorStyles.Right Or AnchorStyles.Top}
            AddHandler openFolder.Click, Sub(s2, e2)
                                             Try
                                                 Directory.CreateDirectory(LogFolder)
                                                 Process.Start("explorer.exe", """" & LogFolder & """")
                                             Catch
                                             End Try
                                         End Sub
            Dim close As New Button() With {.Text = "Close", .Size = New Size(Dpi.S(100), Dpi.S(32)),
                .Location = New Point(Dpi.S(742), Dpi.S(10)), .Anchor = AnchorStyles.Right Or AnchorStyles.Top, .DialogResult = DialogResult.OK}
            bottom.Controls.AddRange(New Control() {info, openFolder, close})
            dlg.Controls.Add(grid)
            dlg.Controls.Add(bottom)
            dlg.CancelButton = close
            dlg.ShowDialog(owner)
        End Using
    End Sub

    Private Shared Function SplitCsv(ByVal line As String) As List(Of String)
        Dim result As New List(Of String)()
        Dim sb As New StringBuilder()
        Dim q As Boolean = False
        Dim i As Integer = 0
        While i < line.Length
            Dim c As Char = line(i)
            If q Then
                If c = """"c Then
                    If i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                        sb.Append(""""c) : i += 1
                    Else
                        q = False
                    End If
                Else
                    sb.Append(c)
                End If
            ElseIf c = """"c Then
                q = True
            ElseIf c = ","c Then
                result.Add(sb.ToString()) : sb.Length = 0
            Else
                sb.Append(c)
            End If
            i += 1
        End While
        result.Add(sb.ToString())
        Return result
    End Function

End Class
