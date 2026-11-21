Option Strict On
Option Explicit On

Imports System.Collections.Generic
Imports System.Drawing

''' <summary>
''' Turns the existing main window (frmMDI) into the modern shell:
'''  * home dashboard instead of the stretched background picture,
'''  * Ctrl+K / F3 command search (also: just start typing on the dashboard),
'''  * search box and user menu on the right of the menu bar,
'''  * clean status bar (server date | company | FY | user | version).
''' Nothing in frmMDI's menu logic changes - the shell only reads the menu.
''' </summary>
Public NotInheritable Class MdiShell

    Private ReadOnly _form As Form
    Private ReadOnly _menu As MenuStrip
    Private ReadOnly _status As StatusStrip
    Private ReadOnly _commands As List(Of MenuCommand)
    Private _dashboard As DashboardView
    Private _changeSession As MethodInvoker

    Private Shared ReadOnly Shells As New Dictionary(Of Form, MdiShell)()

    Private Sub New(ByVal f As Form, ByVal menu As MenuStrip, ByVal status As StatusStrip)
        _form = f
        _menu = menu
        _status = status
        _commands = MenuCatalog.Collect(menu)
    End Sub

    ''' <summary>
    ''' Call once at the end of the main window's Load (after role rights were applied).
    ''' <paramref name="legacyBackground"/> = old decoration controls to hide (background picture, logo, labels).
    ''' </summary>
    Public Shared Sub Attach(ByVal f As Form, ByVal menu As MenuStrip, ByVal status As StatusStrip, ByVal ParamArray legacyBackground() As Control)
        If f Is Nothing OrElse Not AppTheme.Enabled OrElse Shells.ContainsKey(f) Then Return
        Dim shell As New MdiShell(f, menu, status)
        Shells(f) = shell
        Try
            shell.Build(legacyBackground)
        Catch ex As Exception
            ' the shell is cosmetic: never stop the application because of it
            Global.System.Diagnostics.Debug.WriteLine("MdiShell: " & ex.ToString())
            For Each c As Control In legacyBackground
                If c IsNot Nothing Then c.Visible = True
            Next
        End Try
    End Sub

    ''' <summary>
    ''' Registers the main window's Change Session routine: offered in the user menu (for every role)
    ''' and on the dashboard's financial-year chip.
    ''' </summary>
    Public Shared Sub SetChangeSession(ByVal f As Form, ByVal action As MethodInvoker)
        Dim shell As MdiShell = Nothing
        If f Is Nothing OrElse Not Shells.TryGetValue(f, shell) Then Return
        shell._changeSession = action
        If shell._dashboard IsNot Nothing Then shell._dashboard.SessionChipClickable = (action IsNot Nothing)
        shell.AddChangeSessionToUserMenu()
    End Sub

    Private _userMenu As ToolStripMenuItem

    Private Sub AddChangeSessionToUserMenu()
        If _userMenu Is Nothing OrElse _changeSession Is Nothing Then Return
        ' replace the menu-bound entry (Tools may be disabled for the role) with a direct one
        For i As Integer = _userMenu.DropDownItems.Count - 1 To 0 Step -1
            If _userMenu.DropDownItems(i).Text = "Change session" Then _userMenu.DropDownItems.RemoveAt(i)   ' menu-bound copy
        Next
        Dim item As New ToolStripMenuItem("Change company / session" & ChrW(8230))
        AddHandler item.Click, Sub(s, e) _form.BeginInvoke(_changeSession)
        _userMenu.DropDownItems.Insert(Math.Min(2, _userMenu.DropDownItems.Count), item)
        ThemeManager.StyleStrip(_menu)
    End Sub

    ''' <summary>Opens the command search for the given main window.</summary>
    Public Shared Sub ShowSearch(ByVal f As Form, Optional ByVal initialText As String = "")
        Dim shell As MdiShell = Nothing
        If f IsNot Nothing AndAlso Shells.TryGetValue(f, shell) Then
            CommandPalette.ShowFor(f, shell._commands, initialText)
        End If
    End Sub

    Private Sub Build(ByVal legacyBackground() As Control)
        _form.SuspendLayout()
        Try
            For Each c As Control In legacyBackground
                If c IsNot Nothing Then c.Visible = False
            Next
            _form.BackgroundImage = Nothing
            For Each c As Control In _form.Controls
                If TypeOf c Is MdiClient Then c.BackColor = AppTheme.Surface
            Next

            _dashboard = New DashboardView(_commands)
            AddHandler _dashboard.SearchRequested, Sub(s, e) ShowSearch(_form)
            AddHandler _dashboard.SessionRequested, Sub(s, e) If _changeSession IsNot Nothing Then _form.BeginInvoke(_changeSession)
            _form.Controls.Add(_dashboard)
            ' docked last = fills the space left between menu bar and status bar
            _dashboard.BringToFront()

            AddMenuBarExtras()
            StyleStatusBar()

            Warmup.Start()   ' preload report libraries in the background

            _form.KeyPreview = True
            AddHandler _form.KeyDown, AddressOf Form_KeyDown
            AddHandler _form.KeyPress, AddressOf Form_KeyPress
            AddHandler _form.Activated, Sub(s, e) _dashboard.RefreshFrequentIfNeeded()
            AddHandler _form.FormClosed, Sub(s, e) Shells.Remove(_form)
        Finally
            _form.ResumeLayout(True)
        End Try
    End Sub

    '---------------------------------------------------------------------
    ' keyboard
    '---------------------------------------------------------------------
    Private Sub Form_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If (e.Control AndAlso e.KeyCode = Keys.K) OrElse (e.KeyCode = Keys.F3 AndAlso e.Modifiers = Keys.None) Then
            e.Handled = True
            e.SuppressKeyPress = True
            ShowSearch(_form)
        End If
    End Sub

    ' typing a letter while the dashboard is in front starts a search with that letter
    Private Sub Form_KeyPress(ByVal sender As Object, ByVal e As KeyPressEventArgs)
        If Not Char.IsLetterOrDigit(e.KeyChar) Then Return
        Dim active As Control = _form.ActiveControl
        If active IsNot Nothing AndAlso Not (active Is _dashboard OrElse _dashboard.Contains(active)) Then Return
        e.Handled = True
        ShowSearch(_form, e.KeyChar.ToString())
    End Sub

    '---------------------------------------------------------------------
    ' menu bar: search + user menu, right aligned
    '---------------------------------------------------------------------
    Private Function FindCommand(ByVal itemName As String) As MenuCommand
        For Each c As MenuCommand In _commands
            If String.Equals(c.Item.Name, itemName, StringComparison.OrdinalIgnoreCase) Then Return c
        Next
        Return Nothing
    End Function

    Private Sub AddMenuBarExtras()
        If _menu Is Nothing Then Return

        Dim user As New ToolStripMenuItem(If(String.IsNullOrEmpty(GMod.username), "User", GMod.username))
        user.Alignment = ToolStripItemAlignment.Right
        _userMenu = user
        user.Tag = MenuCatalog.ShellTag
        user.ToolTipText = "Signed in as " & GMod.username & If(String.IsNullOrEmpty(GMod.role), "", " (" & GMod.role & ")")
        Dim info As New ToolStripMenuItem(GMod.Cmpname & "  " & ChrW(183) & "  session " & GMod.Session) With {.Enabled = False}
        user.DropDownItems.Add(info)
        user.DropDownItems.Add(New ToolStripSeparator())
        For Each pair As String() In New String()() {
                New String() {"ChangeOwnPasswordToolStripMenuItem", "Change my password"},
                New String() {"ChangeSessionToolStripMenuItem", "Change session"},
                New String() {"LogOffToolStripMenuItem", "Log off"},
                New String() {"ExitToolStripMenuItem", "Exit"}}
            Dim cmd As MenuCommand = FindCommand(pair(0))
            If cmd Is Nothing OrElse Not cmd.IsUsable Then Continue For
            Dim proxy As New ToolStripMenuItem(pair(1))
            AddHandler proxy.Click, Sub(s, e) _form.BeginInvoke(New MethodInvoker(AddressOf cmd.Run))
            user.DropDownItems.Add(proxy)
        Next

        Dim search As New ToolStripMenuItem("Search" & ChrW(8230) & "   Ctrl+K")
        search.Alignment = ToolStripItemAlignment.Right
        search.Tag = MenuCatalog.ShellTag
        search.ToolTipText = "Find any screen or report (Ctrl+K or F3)"
        AddHandler search.Click, Sub(s, e) _form.BeginInvoke(New MethodInvoker(Sub() ShowSearch(_form)))

        user.DropDownItems.Add(New ToolStripSeparator())
        Dim timings As New ToolStripMenuItem("Screen open times" & ChrW(8230))
        timings.ToolTipText = "How long each screen takes to open - helps find slow screens"
        AddHandler timings.Click, Sub(s, e) FormTimings.ShowSummary(_form)
        user.DropDownItems.Add(timings)

        ' right-aligned items are laid out right-to-left: add user first so it ends up outermost
        _menu.Items.Add(user)
        _menu.Items.Add(search)
        ThemeManager.StyleStrip(_menu)
    End Sub

    '---------------------------------------------------------------------
    ' status bar
    '---------------------------------------------------------------------
    Private Sub StyleStatusBar()
        If _status Is Nothing Then Return
        _status.SuspendLayout()
        Try
            ThemeManager.StyleStrip(_status)
            _status.BackColor = AppTheme.Card   ' designer colour (PaleTurquoise) must not show through
            _status.Font = ThemeManager.GetFont(9.0F, FontStyle.Regular)
            _status.Padding = New Padding(Dpi.S(10), 0, Dpi.S(14), 0)
            _status.SizingGrip = False

            Dim warn As Boolean = GMod.PerviousSession
            Dim fore As Color = If(warn, AppTheme.Danger, AppTheme.TextMuted)
            For Each it As ToolStripItem In _status.Items
                it.Font = ThemeManager.GetFont(9.0F, If(warn, FontStyle.Bold, FontStyle.Regular))
                it.ForeColor = fore
                Dim sl As ToolStripStatusLabel = TryCast(it, ToolStripStatusLabel)
                If sl IsNot Nothing Then
                    sl.BorderSides = ToolStripStatusLabelBorderSides.None
                    sl.BorderStyle = Border3DStyle.Flat
                End If
            Next

            ' the legacy second label repeats the window title - replace with clean facts
            If _status.Items.Count >= 2 Then
                _status.Items(1).Text = GMod.Cmpname & "   " & ChrW(183) & "   session " & GMod.Session &
                                        "   " & ChrW(183) & "   " & GMod.username & If(String.IsNullOrEmpty(GMod.role), "", " (" & GMod.role & ")")
            End If

            Dim spring As New ToolStripStatusLabel() With {.Spring = True}
            Dim hint As New ToolStripStatusLabel("Ctrl+K  search menus") With {.ForeColor = AppTheme.TextMuted, .Font = ThemeManager.GetFont(8.25F, FontStyle.Regular)}
            Dim ver As New ToolStripStatusLabel("v" & Application.ProductVersion) With {.ForeColor = AppTheme.TextMuted, .Font = ThemeManager.GetFont(8.25F, FontStyle.Regular)}
            _status.Items.Add(spring)
            _status.Items.Add(hint)
            _status.Items.Add(ver)
        Finally
            _status.ResumeLayout(True)
        End Try
    End Sub

End Class
