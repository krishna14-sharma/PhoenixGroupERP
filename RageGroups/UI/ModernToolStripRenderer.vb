Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Drawing.Drawing2D

''' <summary>
''' Flat, professional renderer for MenuStrip / ToolStrip / StatusStrip / ContextMenuStrip.
''' Top-level main menu = dark brand bar, drop-downs = clean white cards.
''' Brushes and pens are cached, so painting large menus stays cheap.
''' </summary>
Public Class ModernToolStripRenderer
    Inherits ToolStripProfessionalRenderer

    Private Shared ReadOnly HoverOnDark As Color = AppTheme.PrimaryHover
    Private Shared ReadOnly PressedOnDark As Color = AppTheme.PrimaryDark
    Private Shared ReadOnly PreviousSessionText As Color = Color.FromArgb(255, 210, 122)

    Public Sub New()
        MyBase.New(New ModernColorTable())
        Me.RoundedEdges = False
    End Sub

    Private Shared Function IsTopLevelMenu(ByVal ts As ToolStrip) As Boolean
        Return TypeOf ts Is MenuStrip
    End Function

    Protected Overrides Sub OnRenderToolStripBackground(ByVal e As ToolStripRenderEventArgs)
        Dim g As Graphics = e.Graphics
        If IsTopLevelMenu(e.ToolStrip) Then
            Using b As New SolidBrush(AppTheme.Primary)
                g.FillRectangle(b, e.AffectedBounds)
            End Using
        ElseIf TypeOf e.ToolStrip Is ToolStripDropDown Then
            Using b As New SolidBrush(AppTheme.Card)
                g.FillRectangle(b, e.AffectedBounds)
            End Using
        ElseIf TypeOf e.ToolStrip Is StatusStrip Then
            Using b As New SolidBrush(AppTheme.Card)
                g.FillRectangle(b, e.AffectedBounds)
            End Using
            Using p As New Pen(AppTheme.Border)
                g.DrawLine(p, 0, 0, e.ToolStrip.Width, 0)
            End Using
        Else
            Using b As New SolidBrush(AppTheme.Card)
                g.FillRectangle(b, e.AffectedBounds)
            End Using
        End If
    End Sub

    Protected Overrides Sub OnRenderToolStripBorder(ByVal e As ToolStripRenderEventArgs)
        If TypeOf e.ToolStrip Is ToolStripDropDown Then
            Dim r As New Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1)
            Using p As New Pen(AppTheme.BorderStrong)
                e.Graphics.DrawRectangle(p, r)
            End Using
        ElseIf Not (IsTopLevelMenu(e.ToolStrip) OrElse TypeOf e.ToolStrip Is StatusStrip) Then
            Using p As New Pen(AppTheme.Border)
                e.Graphics.DrawLine(p, 0, e.ToolStrip.Height - 1, e.ToolStrip.Width, e.ToolStrip.Height - 1)
            End Using
        End If
    End Sub

    Protected Overrides Sub OnRenderMenuItemBackground(ByVal e As ToolStripItemRenderEventArgs)
        Dim item As ToolStripItem = e.Item
        Dim g As Graphics = e.Graphics
        Dim rect As New Rectangle(Point.Empty, item.Size)

        If IsTopLevelMenu(e.ToolStrip) Then
            If item.Pressed Then
                Using b As New SolidBrush(PressedOnDark)
                    g.FillRectangle(b, rect)
                End Using
                Using b As New SolidBrush(AppTheme.Accent)
                    g.FillRectangle(b, 0, rect.Height - 3, rect.Width, 3)
                End Using
            ElseIf item.Selected AndAlso item.Enabled Then
                Using b As New SolidBrush(HoverOnDark)
                    g.FillRectangle(b, rect)
                End Using
            End If
            Return
        End If

        If (item.Selected OrElse item.Pressed) AndAlso item.Enabled Then
            Dim r As New Rectangle(3, 1, rect.Width - 6, rect.Height - 2)
            g.SmoothingMode = SmoothingMode.AntiAlias
            Using path As GraphicsPath = RoundedRect(r, 3)
                Using b As New SolidBrush(AppTheme.PrimaryLight)
                    g.FillPath(b, path)
                End Using
            End Using
            g.SmoothingMode = SmoothingMode.Default
        End If
    End Sub

    Protected Overrides Sub OnRenderButtonBackground(ByVal e As ToolStripItemRenderEventArgs)
        Dim item As ToolStripItem = e.Item
        Dim rect As New Rectangle(Point.Empty, item.Size)
        Dim btn As ToolStripButton = TryCast(item, ToolStripButton)
        If item.Pressed OrElse (btn IsNot Nothing AndAlso btn.Checked) Then
            Using b As New SolidBrush(AppTheme.Pressed)
                e.Graphics.FillRectangle(b, rect)
            End Using
        ElseIf item.Selected Then
            Using b As New SolidBrush(AppTheme.Hover)
                e.Graphics.FillRectangle(b, rect)
            End Using
        End If
    End Sub

    Protected Overrides Sub OnRenderItemText(ByVal e As ToolStripItemTextRenderEventArgs)
        If IsTopLevelMenu(e.ToolStrip) Then
            If Not e.Item.Enabled Then
                e.TextColor = Color.FromArgb(143, 163, 184)
            ElseIf GMod.PerviousSession Then
                e.TextColor = PreviousSessionText
            Else
                e.TextColor = AppTheme.TextOnDark
            End If
        ElseIf TypeOf e.ToolStrip Is StatusStrip Then
            ' status labels keep their own colour when one has been set on purpose
            If Not e.Item.Enabled Then e.TextColor = AppTheme.TextDisabled
        Else
            e.TextColor = If(e.Item.Enabled, AppTheme.Text, AppTheme.TextDisabled)
        End If
        MyBase.OnRenderItemText(e)
    End Sub

    Protected Overrides Sub OnRenderArrow(ByVal e As ToolStripArrowRenderEventArgs)
        If e.Item IsNot Nothing AndAlso IsTopLevelMenu(e.Item.Owner) Then
            e.ArrowColor = AppTheme.TextOnDark
        Else
            e.ArrowColor = If(e.Item Is Nothing OrElse e.Item.Enabled, AppTheme.TextMuted, AppTheme.TextDisabled)
        End If
        MyBase.OnRenderArrow(e)
    End Sub

    Protected Overrides Sub OnRenderSeparator(ByVal e As ToolStripSeparatorRenderEventArgs)
        Dim r As Rectangle = e.Item.ContentRectangle
        Using p As New Pen(AppTheme.Border)
            If e.Vertical Then
                Dim x As Integer = r.Width \ 2
                e.Graphics.DrawLine(p, x, r.Top + 4, x, r.Bottom - 4)
            Else
                Dim y As Integer = e.Item.Height \ 2
                e.Graphics.DrawLine(p, 30, y, e.Item.Width - 6, y)
            End If
        End Using
    End Sub

    Protected Overrides Sub OnRenderImageMargin(ByVal e As ToolStripRenderEventArgs)
        ' clean: no grey gutter
        Using b As New SolidBrush(AppTheme.Card)
            e.Graphics.FillRectangle(b, e.AffectedBounds)
        End Using
    End Sub

    Protected Overrides Sub OnRenderItemCheck(ByVal e As ToolStripItemImageRenderEventArgs)
        Dim r As New Rectangle(e.ImageRectangle.X - 2, e.ImageRectangle.Y - 2, e.ImageRectangle.Width + 4, e.ImageRectangle.Height + 4)
        Using b As New SolidBrush(AppTheme.Pressed)
            e.Graphics.FillRectangle(b, r)
        End Using
        MyBase.OnRenderItemCheck(e)
    End Sub

    Protected Overrides Sub OnRenderStatusStripSizingGrip(ByVal e As ToolStripRenderEventArgs)
        ' no grip - cleaner status bar
    End Sub

    Friend Shared Function RoundedRect(ByVal r As Rectangle, ByVal radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim d As Integer = radius * 2
        If d <= 0 Then
            path.AddRectangle(r)
            Return path
        End If
        path.AddArc(r.X, r.Y, d, d, 180, 90)
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90)
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90)
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
        Return path
    End Function

End Class

''' <summary>Colour table used by the professional renderer for anything we do not paint ourselves.</summary>
Public Class ModernColorTable
    Inherits ProfessionalColorTable

    Public Sub New()
        Me.UseSystemColors = False
    End Sub

    Public Overrides ReadOnly Property MenuBorder As Color
        Get
            Return AppTheme.BorderStrong
        End Get
    End Property
    Public Overrides ReadOnly Property MenuItemBorder As Color
        Get
            Return Color.Transparent
        End Get
    End Property
    Public Overrides ReadOnly Property MenuItemSelected As Color
        Get
            Return AppTheme.PrimaryLight
        End Get
    End Property
    Public Overrides ReadOnly Property ToolStripDropDownBackground As Color
        Get
            Return AppTheme.Card
        End Get
    End Property
    Public Overrides ReadOnly Property ImageMarginGradientBegin As Color
        Get
            Return AppTheme.Card
        End Get
    End Property
    Public Overrides ReadOnly Property ImageMarginGradientMiddle As Color
        Get
            Return AppTheme.Card
        End Get
    End Property
    Public Overrides ReadOnly Property ImageMarginGradientEnd As Color
        Get
            Return AppTheme.Card
        End Get
    End Property
    Public Overrides ReadOnly Property SeparatorDark As Color
        Get
            Return AppTheme.Border
        End Get
    End Property
    Public Overrides ReadOnly Property SeparatorLight As Color
        Get
            Return AppTheme.Card
        End Get
    End Property
    Public Overrides ReadOnly Property ButtonSelectedBorder As Color
        Get
            Return AppTheme.Border
        End Get
    End Property
    Public Overrides ReadOnly Property ToolStripBorder As Color
        Get
            Return AppTheme.Border
        End Get
    End Property
    Public Overrides ReadOnly Property ToolStripGradientBegin As Color
        Get
            Return AppTheme.Card
        End Get
    End Property
    Public Overrides ReadOnly Property ToolStripGradientMiddle As Color
        Get
            Return AppTheme.Card
        End Get
    End Property
    Public Overrides ReadOnly Property ToolStripGradientEnd As Color
        Get
            Return AppTheme.Card
        End Get
    End Property
    Public Overrides ReadOnly Property StatusStripGradientBegin As Color
        Get
            Return AppTheme.Card
        End Get
    End Property
    Public Overrides ReadOnly Property StatusStripGradientEnd As Color
        Get
            Return AppTheme.Card
        End Get
    End Property
    Public Overrides ReadOnly Property MenuStripGradientBegin As Color
        Get
            Return AppTheme.Primary
        End Get
    End Property
    Public Overrides ReadOnly Property MenuStripGradientEnd As Color
        Get
            Return AppTheme.Primary
        End Get
    End Property
End Class
