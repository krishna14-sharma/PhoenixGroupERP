Option Strict On
Option Explicit On

Imports System.Collections.Generic
Imports System.Reflection
Imports System.Threading

''' <summary>
''' Background warm-up started once the main window is open.
''' The first report screen used to stall for several seconds while Windows loaded the
''' large Crystal Reports libraries from disk. They are now loaded on a low-priority
''' background thread while the user is still on the dashboard. Nothing is shown,
''' nothing is changed - if anything fails, the screen simply loads them itself as before.
''' </summary>
Public NotInheritable Class Warmup

    Private Sub New()
    End Sub

    Private Shared _started As Integer = 0

    Public Shared Sub Start()
        If Interlocked.Exchange(_started, 1) = 1 Then Return
        Try
            Dim t As New Thread(AddressOf Run)
            t.IsBackground = True
            t.Priority = ThreadPriority.BelowNormal
            t.Name = "Phoenix warm-up"
            t.Start()
        Catch
        End Try
    End Sub

    Private Shared Sub Run()
        Try
            Thread.Sleep(1500)          ' let the main window finish painting first
            Dim entry As Assembly = Assembly.GetEntryAssembly()
            If entry Is Nothing Then Return
            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim queue As New Queue(Of AssemblyName)(entry.GetReferencedAssemblies())
            While queue.Count > 0
                Dim n As AssemblyName = queue.Dequeue()
                If Not IsReportLibrary(n.Name) OrElse Not seen.Add(n.FullName) Then Continue While
                Try
                    Dim a As Assembly = Assembly.Load(n)
                    For Each r As AssemblyName In a.GetReferencedAssemblies()
                        queue.Enqueue(r)
                    Next
                Catch
                    ' not installed on this PC / cannot load: the report screen will report it as before
                End Try
            End While
        Catch
        End Try
    End Sub

    Private Shared Function IsReportLibrary(ByVal name As String) As Boolean
        Return name IsNot Nothing AndAlso
               (name.StartsWith("CrystalDecisions", StringComparison.OrdinalIgnoreCase) OrElse
                name.StartsWith("BusinessObjects", StringComparison.OrdinalIgnoreCase))
    End Function

End Class
