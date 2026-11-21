Option Strict On
Option Explicit On

Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text

'==========================================================================
' Home dashboard shown inside the main window (replaces the old stretched
' background picture). Built entirely from the main menu, so it follows the
' role rights applied at login without any extra configuration.
'==========================================================================

''' <summary>Pixel scaling for controls created in code (designer controls are auto-scaled by WinForms).</summary>
Friend NotInheritable Class Dpi
    Private Sub New()
    End Sub
    Private Shared _factor As Single = 0
    Public Shared ReadOnly Property Factor As Single
        Get
            If _factor = 0 Then
                Try
                    Using g As Graphics = Graphics.FromHwnd(IntPtr.Zero)
                        _factor = Math.Max(1.0F, g.DpiX / 96.0F)
                    End Using
                Catch
                    _factor = 1.0F
                End Try
            End If
            Return _factor
        End Get
    End Property
    Public Shared Function S(ByVal px As Integer) As Integer
        Return CInt(Math.Round(px * Factor))
    End Function
End Class

''' <summary>Accent colours used to tell the modules apart.</summary>
Friend NotInheritable Class ModuleColors
    Private Sub New()
    End Sub
    Private Shared ReadOnly Palette As Color() = {
        Color.FromArgb(31, 78, 121), Color.FromArgb(0, 128, 128), Color.FromArgb(224, 123, 57),
        Color.FromArgb(111, 66, 193), Color.FromArgb(46, 125, 50), Color.FromArgb(192, 57, 43),
        Color.FromArgb(48, 63, 159), Color.FromArgb(183, 121, 31), Color.FromArgb(84, 110, 122)}
    Private Shared ReadOnly Assigned As New Dictionary(Of ToolStripItem, Color)()
    Public Shared Function ForModule(ByVal top As ToolStripItem) As Color
        Dim c As Color
        If top Is Nothing Then Return AppTheme.Primary
        If Assigned.TryGetValue(top, c) Then Return c
        c = Palette(Assigned.Count Mod Palette.Length)
        Assigned(top) = c
        Return c
    End Function
End Class

Friend NotInheritable Class Paint2D
    Private Sub New()
    End Sub
    Public Shared Function Rounded(ByVal r As Rectangle, ByVal radius As Integer) As GraphicsPath
        Return ModernToolStripRenderer.RoundedRect(r, radius)
    End Function
    Public Shared Function Initials(ByVal title As String) As String
        Dim words() As String = title.Split(New Char() {" "c, "/"c, "-"c, "("c}, StringSplitOptions.RemoveEmptyEntries)
        Dim sb As New System.Text.StringBuilder()
        For Each w As String In words
            If sb.Length >= 2 Then Exit For
            If Char.IsLetterOrDigit(w(0)) Then sb.Append(Char.ToUpperInvariant(w(0)))
        Next
        If sb.Length = 0 AndAlso title.Length > 0 Then sb.Append(Char.ToUpperInvariant(title(0)))
        Return sb.ToString()
    End Function
End Class

'--------------------------------------------------------------------------
' Header band: greeting, company, session chips, clock and search pill
'--------------------------------------------------------------------------
Public Class DashboardHeader
    Inherits Control

    Public Event SearchRequested As EventHandler
    Public Event SessionRequested As EventHandler

    Private _searchRect As Rectangle
    Private _sessionChipRect As Rectangle
    Private _sessionHot As Boolean
    Public Property SessionChipClickable As Boolean
    Private _searchHot As Boolean
    Private _lastMinute As Integer = -1
    Private ReadOnly _clock As New System.Windows.Forms.Timer() With {.Interval = 1000}

    Public Sub New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or
                    ControlStyles.ResizeRedraw Or ControlStyles.UserPaint, True)
        Me.Height = Dpi.S(150)
        Me.Tag = ThemeManager.SkipTag
        AddHandler _clock.Tick, AddressOf Clock_Tick
        _clock.Start()
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then _clock.Dispose()
        MyBase.Dispose(disposing)
    End Sub

    Private Shared Function CurrentTime() As DateTime
        Try
            Return GMod.ServerNow()
        Catch
            Return DateTime.Now
        End Try
    End Function

    Private Sub Clock_Tick(ByVal sender As Object, ByVal e As EventArgs)
        Dim m As Integer = DateTime.Now.Minute
        If m <> _lastMinute Then
            _lastMinute = m
            Invalidate()
        End If
    End Sub

    Private Shared Function FinancialYearText() As String
        Try
            If GMod.SessionStartDate.Year > 1900 Then
                Return "FY " & GMod.SessionStartDate.Year.ToString() & "-" & (GMod.SessionStartDate.Year + 1).ToString().Substring(2)
            End If
        Catch
        End Try
        Return "Session " & GMod.Session
    End Function

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        Dim r As Rectangle = Me.ClientRectangle
        If r.Width <= 0 OrElse r.Height <= 0 Then Return
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

        Using lg As New LinearGradientBrush(r, AppTheme.Primary, AppTheme.PrimaryDark, LinearGradientMode.Horizontal)
            g.FillRectangle(lg, r)
        End Using
        Using ring As New Pen(Color.FromArgb(16, Color.White), Dpi.S(26))
            g.DrawEllipse(ring, r.Right - Dpi.S(420), -Dpi.S(160), Dpi.S(360), Dpi.S(360))
        End Using

        Dim stamp As DateTime = CurrentTime()
        Dim x As Integer = Dpi.S(32)
        Dim greeting As String = If(stamp.Hour < 12, "Good morning", If(stamp.Hour < 17, "Good afternoon", "Good evening"))
        If Not String.IsNullOrEmpty(GMod.username) Then greeting &= ", " & GMod.username

        Using fSmall As New Font(AppTheme.FontFamilyName, 10.0F), fBig As New Font(AppTheme.FontFamilySemibold, 19.0F),
              fChip As New Font(AppTheme.FontFamilyName, 8.25F), fClock As New Font(AppTheme.FontFamilyName, 24.0F, FontStyle.Regular),
              fSearch As New Font(AppTheme.FontFamilyName, 9.5F)

            ' right column: clock + date + search pill
            Dim rightW As Integer = Dpi.S(330)
            Dim rx As Integer = r.Right - rightW - Dpi.S(28)
            TextRenderer.DrawText(g, stamp.ToString("hh:mm tt"), fClock, New Rectangle(rx, Dpi.S(16), rightW, Dpi.S(44)), Color.White,
                                  TextFormatFlags.Right Or TextFormatFlags.VerticalCenter)
            TextRenderer.DrawText(g, stamp.ToString("dddd, dd MMMM yyyy"), fSmall, New Rectangle(rx, Dpi.S(58), rightW, Dpi.S(20)),
                                  Color.FromArgb(200, 222, 240), TextFormatFlags.Right Or TextFormatFlags.VerticalCenter)
            _searchRect = New Rectangle(rx, Dpi.S(92), rightW, Dpi.S(36))
            Using path As GraphicsPath = Paint2D.Rounded(_searchRect, Dpi.S(18)),
                  sb As New SolidBrush(If(_searchHot, Color.FromArgb(255, 255, 255), Color.FromArgb(235, 255, 255, 255)))
                g.FillPath(sb, path)
            End Using
            Using p As New Pen(AppTheme.TextMuted, 1.8F)
                Dim cx As Integer = _searchRect.Left + Dpi.S(16)
                Dim cy As Integer = _searchRect.Top + Dpi.S(11)
                g.DrawEllipse(p, cx, cy, Dpi.S(11), Dpi.S(11))
                g.DrawLine(p, cx + Dpi.S(10), cy + Dpi.S(10), cx + Dpi.S(15), cy + Dpi.S(15))
            End Using
            Dim textRect As New Rectangle(_searchRect.Left + Dpi.S(40), _searchRect.Top, _searchRect.Width - Dpi.S(52), _searchRect.Height)
            TextRenderer.DrawText(g, "Search menus and reports" & ChrW(8230), fSearch, textRect, AppTheme.TextMuted,
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
            TextRenderer.DrawText(g, "Ctrl+K", fChip, textRect, AppTheme.TextMuted, TextFormatFlags.Right Or TextFormatFlags.VerticalCenter)

            ' left column: greeting, company, chips
            Dim leftW As Integer = Math.Max(Dpi.S(200), rx - x - Dpi.S(24))
            TextRenderer.DrawText(g, greeting, fSmall, New Rectangle(x, Dpi.S(20), leftW, Dpi.S(22)), Color.FromArgb(200, 222, 240),
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
            Dim company As String = If(String.IsNullOrEmpty(GMod.Cmpname), "Phoenix Group ERP", GMod.Cmpname)
            TextRenderer.DrawText(g, company, fBig, New Rectangle(x, Dpi.S(42), leftW, Dpi.S(40)), Color.White,
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)

            Dim chips As New List(Of String)()
            chips.Add(FinancialYearText())
            If Not String.IsNullOrEmpty(GMod.Cmpid) Then chips.Add("Company code  " & GMod.Cmpid)
            If Not String.IsNullOrEmpty(GMod.role) Then chips.Add("Role  " & GMod.role)
            chips.Add("Server date  " & stamp.ToString("dd-MMM-yyyy"))
            Dim cxp As Integer = x
            Dim cyp As Integer = Dpi.S(98)
            _sessionChipRect = Rectangle.Empty
            For i As Integer = 0 To chips.Count - 1
                Dim sz As Size = TextRenderer.MeasureText(g, chips(i), fChip, Size.Empty, TextFormatFlags.NoPadding Or TextFormatFlags.NoPrefix)
                Dim chip As New Rectangle(cxp, cyp, sz.Width + Dpi.S(22), Dpi.S(26))
                If chip.Right > x + leftW Then Exit For
                Dim warn As Boolean = (i = 0 AndAlso GMod.PerviousSession)
                Dim hotChip As Boolean = (i = 0 AndAlso _sessionHot AndAlso SessionChipClickable)
                If i = 0 Then _sessionChipRect = chip
                Using path As GraphicsPath = Paint2D.Rounded(chip, Dpi.S(13)),
                      cb As New SolidBrush(If(warn, AppTheme.WarningBanner, Color.FromArgb(If(hotChip, 80, 38), 255, 255, 255)))
                    g.FillPath(cb, path)
                End Using
                TextRenderer.DrawText(g, chips(i), fChip, chip, If(warn, AppTheme.Text, Color.White),
                                      TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
                cxp = chip.Right + Dpi.S(8)
            Next
        End Using
    End Sub

    Protected Overrides Sub OnMouseMove(ByVal e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Dim hot As Boolean = _searchRect.Contains(e.Location)
        Dim sHot As Boolean = SessionChipClickable AndAlso _sessionChipRect.Contains(e.Location)
        If hot <> _searchHot OrElse sHot <> _sessionHot Then
            _searchHot = hot
            _sessionHot = sHot
            Me.Cursor = If(hot OrElse sHot, Cursors.Hand, Cursors.Default)
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseLeave(ByVal e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _searchHot OrElse _sessionHot Then
            _searchHot = False
            _sessionHot = False
            Me.Cursor = Cursors.Default
            Invalidate(_searchRect)
        End If
    End Sub

    Protected Overrides Sub OnMouseClick(ByVal e As MouseEventArgs)
        MyBase.OnMouseClick(e)
        If _searchRect.Contains(e.Location) Then RaiseEvent SearchRequested(Me, EventArgs.Empty)
        If SessionChipClickable AndAlso _sessionChipRect.Contains(e.Location) Then RaiseEvent SessionRequested(Me, EventArgs.Empty)
    End Sub
End Class

'--------------------------------------------------------------------------
' Section caption ("Frequently used", "Modules")
'--------------------------------------------------------------------------
Friend Class SectionTitle
    Inherits Control

    Private ReadOnly _sub As String

    Public Sub New(ByVal title As String, ByVal subtitle As String)
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or ControlStyles.UserPaint, True)
        Me.Text = title
        _sub = subtitle
        Me.Height = Dpi.S(48)
        Me.Margin = New Padding(Dpi.S(6), Dpi.S(14), Dpi.S(6), Dpi.S(2))
        Me.BackColor = AppTheme.Surface
    End Sub

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        e.Graphics.Clear(Me.BackColor)
        Using f1 As New Font(AppTheme.FontFamilySemibold, 12.0F), f2 As New Font(AppTheme.FontFamilyName, 8.75F)
            Dim sz As Size = TextRenderer.MeasureText(e.Graphics, Me.Text, f1)
            TextRenderer.DrawText(e.Graphics, Me.Text, f1, New Point(0, Dpi.S(4)), AppTheme.Text, TextFormatFlags.NoPrefix)
            TextRenderer.DrawText(e.Graphics, _sub, f2, New Rectangle(sz.Width + Dpi.S(6), Dpi.S(4), Me.Width, sz.Height),
                                  AppTheme.TextMuted, TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
        End Using
        Using p As New Pen(AppTheme.Border)
            e.Graphics.DrawLine(p, 0, Me.Height - 2, Me.Width, Me.Height - 2)
        End Using
    End Sub
End Class

'--------------------------------------------------------------------------
' Quick-launch tile
'--------------------------------------------------------------------------
Friend Class ActionTile
    Inherits Control

    Private ReadOnly _cmd As MenuCommand
    Private _hot As Boolean

    Public Sub New(ByVal cmd As MenuCommand)
        _cmd = cmd
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                    ControlStyles.UserPaint Or ControlStyles.Selectable, True)
        Me.Size = New Size(Dpi.S(212), Dpi.S(92))
        Me.Margin = New Padding(Dpi.S(6))
        Me.Cursor = Cursors.Hand
        Me.Text = cmd.Title
        Me.BackColor = AppTheme.Surface
        Me.TabStop = True
    End Sub

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.Clear(Me.BackColor)
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim r As New Rectangle(0, 0, Me.Width - 1, Me.Height - 1)
        Dim accent As Color = ModuleColors.ForModule(_cmd.TopLevel)
        Using path As GraphicsPath = Paint2D.Rounded(r, Dpi.S(8)),
              b As New SolidBrush(If(_hot, AppTheme.Hover, AppTheme.Card)),
              p As New Pen(If(_hot OrElse Me.Focused, accent, AppTheme.Border))
            g.FillPath(b, path)
            g.DrawPath(p, path)
        End Using
        ' badge with initials
        Dim badge As New Rectangle(Dpi.S(14), Dpi.S(16), Dpi.S(34), Dpi.S(34))
        Using path As GraphicsPath = Paint2D.Rounded(badge, Dpi.S(8)), bb As New SolidBrush(AppTheme.Blend(accent, Color.White, 0.86))
            g.FillPath(bb, path)
        End Using
        Using fBadge As New Font(AppTheme.FontFamilySemibold, 10.0F), fTitle As New Font(AppTheme.FontFamilySemibold, 9.75F),
              fPath As New Font(AppTheme.FontFamilyName, 8.0F)
            TextRenderer.DrawText(g, Paint2D.Initials(_cmd.Title), fBadge, badge, accent,
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
            Dim tx As Integer = badge.Right + Dpi.S(12)
            TextRenderer.DrawText(g, _cmd.Title, fTitle, New Rectangle(tx, Dpi.S(12), Me.Width - tx - Dpi.S(10), Dpi.S(42)), AppTheme.Text,
                                  TextFormatFlags.WordBreak Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
            Dim where As String = If(_cmd.Path.Length = 0, MenuCatalog.CleanText(_cmd.TopLevel.Text), _cmd.Path)
            TextRenderer.DrawText(g, where, fPath, New Rectangle(Dpi.S(14), Me.Height - Dpi.S(28), Me.Width - Dpi.S(24), Dpi.S(18)),
                                  AppTheme.TextMuted, TextFormatFlags.Left Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
        End Using
    End Sub

    Protected Overrides Sub OnMouseEnter(ByVal e As EventArgs)
        MyBase.OnMouseEnter(e)
        _hot = True
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseLeave(ByVal e As EventArgs)
        MyBase.OnMouseLeave(e)
        _hot = False
        Invalidate()
    End Sub

    Protected Overrides Sub OnGotFocus(ByVal e As EventArgs)
        MyBase.OnGotFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnLostFocus(ByVal e As EventArgs)
        MyBase.OnLostFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnClick(ByVal e As EventArgs)
        MyBase.OnClick(e)
        Dim f As Form = Me.FindForm()
        If f IsNot Nothing Then f.BeginInvoke(New MethodInvoker(AddressOf _cmd.Run)) Else _cmd.Run()
    End Sub

    Protected Overrides Function IsInputKey(ByVal keyData As Keys) As Boolean
        If keyData = Keys.Enter OrElse keyData = Keys.Space Then Return True
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnKeyDown(ByVal e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        If e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.Space Then OnClick(EventArgs.Empty)
    End Sub
End Class

'--------------------------------------------------------------------------
' Module card: one per top-level menu, listing its first commands
'--------------------------------------------------------------------------
Friend Class ModuleCard
    Inherits Control

    Private Const MaxRows As Integer = 7
    Private ReadOnly _top As ToolStripMenuItem
    Private ReadOnly _rows As New List(Of MenuCommand)()
    Private ReadOnly _total As Integer
    Private _hotRow As Integer = -2       ' -1 = "all commands" row
    Private ReadOnly _headerH As Integer = Dpi.S(58)
    Private ReadOnly _rowH As Integer = Dpi.S(30)

    Public Sub New(ByVal top As ToolStripMenuItem, ByVal commands As List(Of MenuCommand))
        _top = top
        _total = commands.Count
        For Each c As MenuCommand In commands
            If _rows.Count >= MaxRows Then Exit For
            _rows.Add(c)
        Next
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or ControlStyles.UserPaint, True)
        Me.Width = Dpi.S(304)
        Dim rowsShown As Integer = _rows.Count + If(_total > _rows.Count, 1, 0)
        Me.Height = _headerH + rowsShown * _rowH + Dpi.S(12)
        Me.Margin = New Padding(Dpi.S(6), Dpi.S(6), Dpi.S(6), Dpi.S(10))
        Me.BackColor = AppTheme.Surface
        Me.Text = MenuCatalog.CleanText(top.Text)
    End Sub

    Private Function RowRect(ByVal i As Integer) As Rectangle
        Return New Rectangle(Dpi.S(8), _headerH + i * _rowH, Me.Width - Dpi.S(17), _rowH)
    End Function

    Private Function HitRow(ByVal pt As Point) As Integer
        For i As Integer = 0 To _rows.Count - 1
            If RowRect(i).Contains(pt) Then Return i
        Next
        If _total > _rows.Count AndAlso RowRect(_rows.Count).Contains(pt) Then Return -1
        Return -2
    End Function

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.Clear(Me.BackColor)
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim accent As Color = ModuleColors.ForModule(_top)
        Dim r As New Rectangle(0, 0, Me.Width - 1, Me.Height - 1)
        Using path As GraphicsPath = Paint2D.Rounded(r, Dpi.S(8)), b As New SolidBrush(AppTheme.Card), p As New Pen(AppTheme.Border)
            g.FillPath(b, path)
            g.DrawPath(p, path)
        End Using
        ' top accent line
        Using ab As New SolidBrush(accent)
            g.FillRectangle(ab, Dpi.S(8), 0, Me.Width - Dpi.S(17), Dpi.S(3))
        End Using

        Using fHead As New Font(AppTheme.FontFamilySemibold, 11.0F), fCount As New Font(AppTheme.FontFamilyName, 8.25F),
              fRow As New Font(AppTheme.FontFamilyName, 9.25F), fBadge As New Font(AppTheme.FontFamilySemibold, 10.0F)
            Dim badge As New Rectangle(Dpi.S(14), Dpi.S(14), Dpi.S(32), Dpi.S(32))
            Using path As GraphicsPath = Paint2D.Rounded(badge, Dpi.S(8)), bb As New SolidBrush(accent)
                g.FillPath(bb, path)
            End Using
            TextRenderer.DrawText(g, Paint2D.Initials(Me.Text), fBadge, badge, Color.White,
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
            Dim tx As Integer = badge.Right + Dpi.S(12)
            TextRenderer.DrawText(g, Me.Text, fHead, New Rectangle(tx, Dpi.S(11), Me.Width - tx - Dpi.S(10), Dpi.S(22)), AppTheme.Text,
                                  TextFormatFlags.Left Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
            TextRenderer.DrawText(g, _total.ToString() & If(_total = 1, " command", " commands"), fCount,
                                  New Rectangle(tx, Dpi.S(31), Me.Width - tx - Dpi.S(10), Dpi.S(18)), AppTheme.TextMuted,
                                  TextFormatFlags.Left Or TextFormatFlags.NoPrefix)
            Using sep As New Pen(AppTheme.GridLine)
                g.DrawLine(sep, Dpi.S(14), _headerH - Dpi.S(4), Me.Width - Dpi.S(15), _headerH - Dpi.S(4))
            End Using

            For i As Integer = 0 To _rows.Count - 1
                DrawRow(g, RowRect(i), _rows(i).Title, _hotRow = i, fRow, AppTheme.Text, accent)
            Next
            If _total > _rows.Count Then
                DrawRow(g, RowRect(_rows.Count), "All " & _total.ToString() & " commands  " & ChrW(8250), _hotRow = -1, fRow, accent, accent)
            End If
        End Using
    End Sub

    Private Sub DrawRow(ByVal g As Graphics, ByVal rr As Rectangle, ByVal text As String, ByVal hot As Boolean, ByVal f As Font, ByVal fore As Color, ByVal accent As Color)
        If hot Then
            Using path As GraphicsPath = Paint2D.Rounded(rr, Dpi.S(5)), hb As New SolidBrush(AppTheme.Hover)
                g.FillPath(hb, path)
            End Using
        End If
        Dim dot As New Rectangle(rr.Left + Dpi.S(8), rr.Top + rr.Height \ 2 - Dpi.S(2), Dpi.S(5), Dpi.S(5))
        Using db As New SolidBrush(If(hot, accent, AppTheme.BorderStrong))
            g.FillEllipse(db, dot)
        End Using
        TextRenderer.DrawText(g, text, f, New Rectangle(rr.Left + Dpi.S(22), rr.Top, rr.Width - Dpi.S(28), rr.Height),
                              If(hot, AppTheme.Primary, fore), TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
    End Sub

    Protected Overrides Sub OnMouseMove(ByVal e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Dim h As Integer = HitRow(e.Location)
        If h <> _hotRow Then
            _hotRow = h
            Me.Cursor = If(h = -2, Cursors.Default, Cursors.Hand)
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseLeave(ByVal e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hotRow <> -2 Then
            _hotRow = -2
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseClick(ByVal e As MouseEventArgs)
        MyBase.OnMouseClick(e)
        If e.Button <> MouseButtons.Left Then Return
        Dim h As Integer = HitRow(e.Location)
        Dim f As Form = Me.FindForm()
        If h >= 0 Then
            Dim cmd As MenuCommand = _rows(h)
            If f IsNot Nothing Then f.BeginInvoke(New MethodInvoker(AddressOf cmd.Run)) Else cmd.Run()
        ElseIf h = -1 Then
            ' open the real menu so every command of the module is one click away
            _top.ShowDropDown()
        End If
    End Sub
End Class

'--------------------------------------------------------------------------
' The dashboard itself
'--------------------------------------------------------------------------
Public Class DashboardView
    Inherits Panel

    Private ReadOnly _commands As List(Of MenuCommand)
    Private ReadOnly _header As New DashboardHeader()
    Private ReadOnly _body As New FlowLayoutPanel()
    Private _banner As Label
    Private _frequentTitle As SectionTitle
    Private _modulesTitle As SectionTitle
    Private ReadOnly _tiles As New List(Of Control)()
    Private _frequentDirty As Boolean

    Public Event SearchRequested As EventHandler
    Public Event SessionRequested As EventHandler

    ''' <summary>When True the financial-year chip in the header opens Change Session.</summary>
    Public Property SessionChipClickable As Boolean
        Get
            Return _header.SessionChipClickable
        End Get
        Set(ByVal value As Boolean)
            _header.SessionChipClickable = value
            _header.Invalidate()
        End Set
    End Property

    Public Sub New(ByVal commands As List(Of MenuCommand))
        _commands = commands
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
        Me.Tag = ThemeManager.SkipTag
        Me.BackColor = AppTheme.Surface
        Me.Dock = DockStyle.Fill

        _header.Dock = DockStyle.Top
        AddHandler _header.SearchRequested, Sub(s, e) RaiseEvent SearchRequested(Me, EventArgs.Empty)
        AddHandler _header.SessionRequested, Sub(s, e) RaiseEvent SessionRequested(Me, EventArgs.Empty)

        _body.Dock = DockStyle.Fill
        _body.AutoScroll = True
        _body.FlowDirection = FlowDirection.LeftToRight
        _body.WrapContents = True
        _body.Padding = New Padding(Dpi.S(26), Dpi.S(8), Dpi.S(26), Dpi.S(26))
        _body.BackColor = AppTheme.Surface
        ThemeManager.EnableDoubleBuffer(_body)
        AddHandler _body.Resize, Sub(s, e) SizeTitles()

        If GMod.PerviousSession Then
            _banner = New Label()
            _banner.Dock = DockStyle.Top
            _banner.Height = Dpi.S(38)
            _banner.BackColor = AppTheme.WarningBanner
            _banner.ForeColor = AppTheme.Text
            _banner.Font = ThemeManager.GetFont(9.75F, FontStyle.Bold)
            _banner.Padding = New Padding(Dpi.S(30), 0, 0, 0)
            _banner.TextAlign = ContentAlignment.MiddleLeft
            _banner.Text = ChrW(9888) & "   You are working in a PREVIOUS financial year (session " & GMod.Session &
                           "). Entries you save will be posted to that year."
        End If

        Me.Controls.Add(_body)
        If _banner IsNot Nothing Then Me.Controls.Add(_banner)
        Me.Controls.Add(_header)

        BuildBody()
        AddHandler MenuCatalog.CommandUsed, AddressOf Catalog_CommandUsed
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then RemoveHandler MenuCatalog.CommandUsed, AddressOf Catalog_CommandUsed
        MyBase.Dispose(disposing)
    End Sub

    Private Sub Catalog_CommandUsed(ByVal sender As Object, ByVal e As EventArgs)
        ' a screen is about to open; refresh the tiles when the user comes back
        _frequentDirty = True
    End Sub

    Protected Overrides Sub OnVisibleChanged(ByVal e As EventArgs)
        MyBase.OnVisibleChanged(e)
        RefreshFrequentIfNeeded()
    End Sub

    ''' <summary>Called by the shell when the main window is activated again.</summary>
    Public Sub RefreshFrequentIfNeeded()
        If Not _frequentDirty OrElse Not Me.Visible Then Return
        _frequentDirty = False
        RebuildFrequent()
    End Sub

    Private Sub BuildBody()
        _body.SuspendLayout()
        Try
            _frequentTitle = New SectionTitle("Frequently used", "  your most opened screens - learns as you work")
            _body.Controls.Add(_frequentTitle)
            _body.SetFlowBreak(_frequentTitle, True)
            AddFrequentTiles()

            _modulesTitle = New SectionTitle("Modules", "  everything your role can open")
            _body.Controls.Add(_modulesTitle)
            _body.SetFlowBreak(_modulesTitle, True)

            ' group usable commands by top-level menu, keeping menu order
            Dim order As New List(Of ToolStripMenuItem)()
            Dim groups As New Dictionary(Of ToolStripMenuItem, List(Of MenuCommand))()
            For Each c As MenuCommand In _commands
                If Not c.IsUsable Then Continue For
                Dim lst As List(Of MenuCommand) = Nothing
                If Not groups.TryGetValue(c.TopLevel, lst) Then
                    lst = New List(Of MenuCommand)()
                    groups(c.TopLevel) = lst
                    order.Add(c.TopLevel)
                End If
                lst.Add(c)
            Next
            For Each top As ToolStripMenuItem In order
                _body.Controls.Add(New ModuleCard(top, groups(top)))
            Next
            SizeTitles()
        Finally
            _body.ResumeLayout(True)
        End Try
    End Sub

    Private Sub AddFrequentTiles()
        Dim insertAt As Integer = _body.Controls.GetChildIndex(_frequentTitle) + 1
        For Each c As MenuCommand In MenuCatalog.Frequent(_commands, 8)
            Dim t As New ActionTile(c)
            _body.Controls.Add(t)
            _body.Controls.SetChildIndex(t, insertAt)
            insertAt += 1
            _tiles.Add(t)
        Next
        If _tiles.Count > 0 Then _body.SetFlowBreak(_tiles(_tiles.Count - 1), True)
    End Sub

    Private Sub RebuildFrequent()
        _body.SuspendLayout()
        Try
            For Each t As Control In _tiles
                _body.Controls.Remove(t)
                t.Dispose()
            Next
            _tiles.Clear()
            AddFrequentTiles()
        Finally
            _body.ResumeLayout(True)
        End Try
    End Sub

    Private Sub SizeTitles()
        ' leave room for the vertical scroll bar so the titles never force a horizontal one
        Dim w As Integer = _body.ClientSize.Width - _body.Padding.Horizontal - Dpi.S(12) - SystemInformation.VerticalScrollBarWidth - Dpi.S(4)
        If w < Dpi.S(200) Then Return
        If _frequentTitle IsNot Nothing Then _frequentTitle.Width = w
        If _modulesTitle IsNot Nothing Then _modulesTitle.Width = w
    End Sub

End Class
