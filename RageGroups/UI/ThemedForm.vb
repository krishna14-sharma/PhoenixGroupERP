Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Drawing

''' <summary>
''' Base class for every screen in Phoenix Group ERP.
''' Applies the modern theme right before the form's own Load handler runs,
''' so any colour the form sets in code at runtime still wins.
''' It is designer-safe: nothing happens inside the Visual Studio designer.
''' </summary>
Public Class ThemedForm
    Inherits System.Windows.Forms.Form

    Private _themed As Boolean

    ' screen-open timing (see FormTimings): taken when the form object is created
    Private ReadOnly _createdAt As Long = Global.System.Diagnostics.Stopwatch.GetTimestamp()
    Private ReadOnly _queriesAtCreate As Integer = FormTimings.QueryCount
    Private ReadOnly _queryTicksAtCreate As Long = FormTimings.QueryTicks
    Private _loadTicks As Long
    Private _timed As Boolean

    Public Sub New()
        MyBase.New()
        ' reduces flicker when large screens are painted / resized
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
    End Sub

    Private ReadOnly Property IsInDesigner As Boolean
        Get
            Return LicenseManager.UsageMode = LicenseUsageMode.Designtime OrElse Me.DesignMode
        End Get
    End Property

    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        If Not _themed AndAlso Not IsInDesigner Then
            _themed = True
            Try
                ThemeManager.ApplyTheme(Me)
            Catch ex As Exception
                ' never block a screen because of styling
                Global.System.Diagnostics.Debug.WriteLine("Theme error on " & Me.Name & ": " & ex.Message)
            End Try
        End If
        Dim t0 As Long = Global.System.Diagnostics.Stopwatch.GetTimestamp()
        MyBase.OnLoad(e)
        _loadTicks = Global.System.Diagnostics.Stopwatch.GetTimestamp() - t0
    End Sub

    Protected Overrides Sub OnShown(ByVal e As EventArgs)
        MyBase.OnShown(e)
        If _timed OrElse IsInDesigner Then Return
        ' the shell's own helper dialogs are not business screens
        If Me.GetType() Is GetType(ThemedForm) OrElse TypeOf Me Is SessionPicker Then Return
        _timed = True
        Try
            Dim total As Long = FormTimings.TicksToMs(Global.System.Diagnostics.Stopwatch.GetTimestamp() - _createdAt)
            FormTimings.Write(Me, total, FormTimings.TicksToMs(_loadTicks), FormTimings.QueryCount - _queriesAtCreate,
                              FormTimings.TicksToMs(FormTimings.QueryTicks - _queryTicksAtCreate))
        Catch
        End Try
    End Sub

    ''' <summary>Borderless pop-ups (look-ups, search lists) get a crisp 1px frame.</summary>
    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        MyBase.OnPaint(e)
        If AppTheme.Enabled AndAlso Me.FormBorderStyle = FormBorderStyle.None AndAlso Not IsInDesigner Then
            Using p As New Pen(AppTheme.BorderStrong)
                e.Graphics.DrawRectangle(p, 0, 0, Me.ClientSize.Width - 1, Me.ClientSize.Height - 1)
            End Using
        End If
    End Sub

    Protected Overrides Sub OnResize(ByVal e As EventArgs)
        MyBase.OnResize(e)
        If Me.FormBorderStyle = FormBorderStyle.None Then Me.Invalidate(False)
    End Sub

End Class
