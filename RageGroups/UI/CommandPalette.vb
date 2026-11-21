Option Strict On
Option Explicit On

Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D

''' <summary>
''' Ctrl+K / F3 "search everything" box: type a few letters of any menu command
''' (e.g. "led", "trial add", "sale reg") and press Enter to open it.
''' Respects role rights - disabled / hidden menu items never appear.
''' </summary>
Public Class CommandPalette
    Inherits Form

    Private ReadOnly _all As List(Of MenuCommand)
    Private ReadOnly _search As New TextBox()
    Private ReadOnly _list As New ListBox()
    Private ReadOnly _hint As New Label()
    Private _chosen As MenuCommand

    Private Shared ReadOnly TitleFont As Font = New Font(AppTheme.FontFamilyName, 10.5F, FontStyle.Regular)
    Private Shared ReadOnly PathFont As Font = New Font(AppTheme.FontFamilyName, 8.25F, FontStyle.Regular)

    Public Sub New(ByVal commands As List(Of MenuCommand))
        _all = commands
        Me.FormBorderStyle = FormBorderStyle.None
        Me.ShowInTaskbar = False
        Me.StartPosition = FormStartPosition.Manual
        Me.KeyPreview = True
        Me.BackColor = AppTheme.BorderStrong          ' 1px frame shows through the padding
        Me.Padding = New Padding(1)
        Me.Size = New Size(660, 448)
        Me.Font = ThemeManager.GetFont(9.0F, FontStyle.Regular)
        Me.Tag = ThemeManager.SkipTag

        Dim inner As New Panel() With {.Dock = DockStyle.Fill, .BackColor = AppTheme.Card, .Padding = New Padding(14, 12, 14, 8)}

        Dim searchHost As New Panel() With {.Dock = DockStyle.Top, .Height = 40, .BackColor = AppTheme.SurfaceAlt, .Padding = New Padding(36, 9, 10, 6)}
        AddHandler searchHost.Paint, AddressOf SearchHost_Paint
        _search.BorderStyle = BorderStyle.None
        _search.BackColor = AppTheme.SurfaceAlt
        _search.ForeColor = AppTheme.Text
        _search.Font = ThemeManager.GetFont(12.0F, FontStyle.Regular)
        _search.Dock = DockStyle.Fill
        searchHost.Controls.Add(_search)

        Dim gap As New Panel() With {.Dock = DockStyle.Top, .Height = 8, .BackColor = AppTheme.Card}

        _list.Dock = DockStyle.Fill
        _list.BorderStyle = BorderStyle.None
        _list.DrawMode = DrawMode.OwnerDrawFixed
        _list.ItemHeight = 42
        _list.IntegralHeight = False
        _list.BackColor = AppTheme.Card
        ThemeManager.EnableDoubleBuffer(_list)

        _hint.Dock = DockStyle.Bottom
        _hint.Height = 24
        _hint.ForeColor = AppTheme.TextMuted
        _hint.TextAlign = ContentAlignment.MiddleLeft
        _hint.Font = ThemeManager.GetFont(8.25F, FontStyle.Regular)
        _hint.Text = ChrW(8593) & ChrW(8595) & " move     Enter  open     Esc  close"

        inner.Controls.Add(_list)
        inner.Controls.Add(_hint)
        inner.Controls.Add(gap)
        inner.Controls.Add(searchHost)
        Me.Controls.Add(inner)

        AddHandler _search.TextChanged, Sub(s, e) Refilter()
        AddHandler _list.DrawItem, AddressOf List_DrawItem
        AddHandler _list.DoubleClick, Sub(s, e) Choose()
        AddHandler _list.MouseMove, AddressOf List_MouseMove
        Refilter()
    End Sub

    ''' <summary>Shows the palette over <paramref name="owner"/>; runs the chosen command after it closes.</summary>
    Public Shared Sub ShowFor(ByVal owner As Form, ByVal commands As List(Of MenuCommand), Optional ByVal initialText As String = "")
        Dim chosen As MenuCommand = Nothing
        Using p As New CommandPalette(commands)
            Dim area As Rectangle = owner.RectangleToScreen(owner.ClientRectangle)
            p.Location = New Point(area.Left + (area.Width - p.Width) \ 2, area.Top + Math.Min(110, Math.Max(20, area.Height \ 8)))
            If Not String.IsNullOrEmpty(initialText) Then
                p._search.Text = initialText
                p._search.SelectionStart = initialText.Length
            End If
            p.ShowDialog(owner)
            chosen = p._chosen
        End Using
        ' run after the palette is gone, so the screen it opens gets focus normally
        If chosen IsNot Nothing Then owner.BeginInvoke(New MethodInvoker(AddressOf chosen.Run))
    End Sub

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ClassStyle = cp.ClassStyle Or &H20000      ' CS_DROPSHADOW
            Return cp
        End Get
    End Property

    Protected Overrides Sub OnShown(ByVal e As EventArgs)
        MyBase.OnShown(e)
        _search.Focus()
    End Sub

    Protected Overrides Sub OnDeactivate(ByVal e As EventArgs)
        MyBase.OnDeactivate(e)
        If Me.Visible AndAlso _chosen Is Nothing Then Me.Close()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, ByVal keyData As Keys) As Boolean
        Select Case keyData
            Case Keys.Escape
                Me.Close()
                Return True
            Case Keys.Enter
                Choose()
                Return True
            Case Keys.Down
                MoveSelection(1)
                Return True
            Case Keys.Up
                MoveSelection(-1)
                Return True
            Case Keys.PageDown
                MoveSelection(8)
                Return True
            Case Keys.PageUp
                MoveSelection(-8)
                Return True
        End Select
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Private Sub MoveSelection(ByVal delta As Integer)
        If _list.Items.Count = 0 Then Return
        _list.SelectedIndex = Math.Max(0, Math.Min(_list.Items.Count - 1, _list.SelectedIndex + delta))
    End Sub

    Private Sub Choose()
        Dim c As MenuCommand = TryCast(_list.SelectedItem, MenuCommand)
        If c Is Nothing Then Return
        _chosen = c
        Me.Close()
    End Sub

    Private Sub Refilter()
        Dim found As List(Of MenuCommand) = MenuCatalog.Search(_all, _search.Text, 200)
        _list.BeginUpdate()
        Try
            _list.Items.Clear()
            For Each c As MenuCommand In found
                _list.Items.Add(c)
            Next
            If _list.Items.Count > 0 Then _list.SelectedIndex = 0
        Finally
            _list.EndUpdate()
        End Try
        If found.Count = 0 Then
            _hint.Text = "No matching menu command.     Esc  close"
        Else
            _hint.Text = found.Count.ToString() & If(found.Count = 1, " command", " commands") &
                         "        " & ChrW(8593) & ChrW(8595) & " move     Enter  open     Esc  close"
        End If
    End Sub

    Private Sub List_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        Dim i As Integer = _list.IndexFromPoint(e.Location)
        If i >= 0 AndAlso i <> _list.SelectedIndex Then _list.SelectedIndex = i
    End Sub

    Private Sub SearchHost_Paint(ByVal sender As Object, ByVal e As PaintEventArgs)
        ' magnifier glyph
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        Using p As New Pen(AppTheme.TextMuted, 2.0F)
            g.DrawEllipse(p, 12, 12, 12, 12)
            g.DrawLine(p, 22, 22, 28, 28)
        End Using
    End Sub

    Private Sub List_DrawItem(ByVal sender As Object, ByVal e As DrawItemEventArgs)
        If e.Index < 0 Then Return
        Dim c As MenuCommand = DirectCast(_list.Items(e.Index), MenuCommand)
        Dim g As Graphics = e.Graphics
        Dim selected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected
        Dim r As Rectangle = e.Bounds
        Using bg As New SolidBrush(If(selected, AppTheme.Selection, AppTheme.Card))
            g.FillRectangle(bg, r)
        End Using
        If selected Then
            Using ab As New SolidBrush(AppTheme.Primary)
                g.FillRectangle(ab, r.Left, r.Top + 6, 3, r.Height - 12)
            End Using
        End If
        Dim textLeft As Integer = r.Left + 14
        TextRenderer.DrawText(g, c.Title, TitleFont, New Rectangle(textLeft, r.Top + 4, r.Width - 150, 20),
                              If(selected, AppTheme.SelectionText, AppTheme.Text), TextFormatFlags.Left Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
        Dim path As String = If(c.Path.Length = 0, "Main menu", c.Path)
        TextRenderer.DrawText(g, path, PathFont, New Rectangle(textLeft, r.Top + 23, r.Width - 150, 16),
                              AppTheme.TextMuted, TextFormatFlags.Left Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
        Dim used As Integer = MenuCatalog.UsageCount(c)
        If used > 0 Then
            TextRenderer.DrawText(g, "used " & used.ToString() & "x", PathFont, New Rectangle(r.Right - 130, r.Top, 118, r.Height),
                                  AppTheme.TextMuted, TextFormatFlags.Right Or TextFormatFlags.VerticalCenter)
        End If
    End Sub

End Class
