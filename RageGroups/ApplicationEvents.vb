Imports System.IO
Imports System.Text

Namespace My

    ' Application-level events:
    '  * Startup            : modern rendering defaults.
    '  * UnhandledException : any error nobody caught is written to a log file and
    '                         shown in a friendly message - the application keeps running
    '                         instead of closing and losing the user's unsaved work.
    '  * Shutdown           : closes the shared SQL connection cleanly.
    Partial Friend Class MyApplication

        Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            ' ToolStrips (menus, status bar) use the modern renderer everywhere by default
            Try
                If AppTheme.Enabled Then ToolStripManager.Renderer = ThemeManager.Renderer
            Catch
            End Try
        End Sub

        Private Sub MyApplication_Shutdown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shutdown
            Try
                GMod.ShutdownConnection()
            Catch
            End Try
        End Sub

        Private Sub MyApplication_UnhandledException(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            Dim logPath As String = WriteErrorLog(e.Exception)
            Dim msg As New StringBuilder()
            msg.AppendLine("Something went wrong on this screen:")
            msg.AppendLine()
            msg.AppendLine(e.Exception.Message)
            msg.AppendLine()
            If TypeOf e.Exception Is System.Data.SqlClient.SqlException Then
                msg.AppendLine("If this keeps happening, check the network connection to the database server.")
            End If
            msg.AppendLine("The application will keep running. Please check that your last entry was saved.")
            If logPath IsNot Nothing Then
                msg.AppendLine()
                msg.Append("Details were saved to: ").Append(logPath)
            End If
            Try
                MessageBox.Show(msg.ToString(), "Phoenix Group ERP", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Catch
            End Try
            e.ExitApplication = False
        End Sub

        ''' <summary>Appends the error to %LOCALAPPDATA%\PhoenixGroupERP\logs\errors-yyyyMM.log; returns the path (or Nothing).</summary>
        Friend Shared Function WriteErrorLog(ByVal ex As Exception) As String
            Try
                Dim dir As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PhoenixGroupERP", "logs")
                Directory.CreateDirectory(dir)
                Dim file As String = Path.Combine(dir, "errors-" & DateTime.Now.ToString("yyyyMM") & ".log")
                Dim sb As New StringBuilder()
                sb.AppendLine(New String("-"c, 80))
                sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "   user=" & GMod.username & "   company=" & GMod.Cmpid & "   session=" & GMod.Session)
                Dim active As Form = Form.ActiveForm
                If active IsNot Nothing Then sb.AppendLine("screen: " & active.Name & " (" & active.Text & ")")
                sb.AppendLine(ex.ToString())
                Global.System.IO.File.AppendAllText(file, sb.ToString())
                Return file
            Catch
                Return Nothing
            End Try
        End Function

    End Class

End Namespace
