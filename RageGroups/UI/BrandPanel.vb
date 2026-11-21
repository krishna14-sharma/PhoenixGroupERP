Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text

''' <summary>
''' Brand side panel used by the login screen (and any splash / about box):
''' deep-blue gradient, soft decorative rings, white logo tile, product name,
''' tag line and footer. Everything is painted - no images to scale, crisp at any DPI.
''' The theme engine leaves it alone (Tag = "NoTheme").
''' </summary>
<ToolboxItem(True)>
Public Class BrandPanel
    Inherits Panel

    Private _logo As Image
    Private _title As String = "Phoenix Group ERP"
    Private _tagLine As String = "Accounts  " & ChrW(183) & "  Inventory  " & ChrW(183) & "  Payroll  " & ChrW(183) & "  MIS"
    Private _footer As String = ChrW(169) & " Phoenix Group"

    Public Sub New()
        MyBase.New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or
                    ControlStyles.ResizeRedraw Or ControlStyles.UserPaint, True)
        Me.Tag = ThemeManager.SkipTag
        Me.BackColor = AppTheme.PrimaryDark
    End Sub

    <Category("Brand")>
    Public Property Logo As Image
        Get
            Return _logo
        End Get
        Set(ByVal value As Image)
            _logo = value
            Invalidate()
        End Set
    End Property

    <Category("Brand"), DefaultValue("Phoenix Group ERP")>
    Public Property Title As String
        Get
            Return _title
        End Get
        Set(ByVal value As String)
            _title = value
            Invalidate()
        End Set
    End Property

    <Category("Brand")>
    Public Property TagLine As String
        Get
            Return _tagLine
        End Get
        Set(ByVal value As String)
            _tagLine = value
            Invalidate()
        End Set
    End Property

    <Category("Brand")>
    Public Property Footer As String
        Get
            Return _footer
        End Get
        Set(ByVal value As String)
            _footer = value
            Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        Dim r As Rectangle = Me.ClientRectangle
        If r.Width <= 0 OrElse r.Height <= 0 Then Return
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

        ' background gradient
        Using lg As New LinearGradientBrush(r, AppTheme.PrimaryHover, AppTheme.PrimaryDark, 65.0F)
            g.FillRectangle(lg, r)
        End Using

        ' decorative rings (very subtle)
        Using p1 As New Pen(Color.FromArgb(28, Color.White), 2.0F), p2 As New Pen(Color.FromArgb(18, Color.White), 22.0F)
            g.DrawEllipse(p2, r.Width - 170, -120, 340, 340)
            g.DrawEllipse(p1, -140, r.Height - 220, 320, 320)
            g.DrawEllipse(p1, -100, r.Height - 180, 240, 240)
        End Using
        ' accent stripe
        Using b As New SolidBrush(AppTheme.Accent)
            g.FillRectangle(b, 40, 64, 36, 4)
        End Using

        Dim x As Integer = 40
        Dim y As Integer = 92

        ' logo tile
        If _logo IsNot Nothing Then
            Dim pad As Integer = 14
            Dim maxW As Integer = Math.Min(r.Width - 2 * x, 260)
            Dim scale As Double = Math.Min(1.0, (maxW - 2 * pad) / CDbl(_logo.Width))
            Dim lw As Integer = CInt(_logo.Width * scale)
            Dim lh As Integer = CInt(_logo.Height * scale)
            Dim tile As New Rectangle(x, y, lw + 2 * pad, lh + 2 * pad)
            Using path As GraphicsPath = ModernToolStripRenderer.RoundedRect(tile, 8), wb As New SolidBrush(Color.White)
                g.FillPath(wb, path)
            End Using
            g.InterpolationMode = InterpolationMode.HighQualityBicubic
            g.DrawImage(_logo, New Rectangle(tile.X + pad, tile.Y + pad, lw, lh))
            y = tile.Bottom + 28
        End If

        ' title + tag line
        Dim textW As Integer = r.Width - 2 * x
        Using fTitle As Font = New Font(AppTheme.FontFamilySemibold, 20.0F, FontStyle.Regular, GraphicsUnit.Point),
              fTag As Font = New Font(AppTheme.FontFamilyName, 9.75F, FontStyle.Regular, GraphicsUnit.Point),
              fFoot As Font = New Font(AppTheme.FontFamilyName, 8.25F, FontStyle.Regular, GraphicsUnit.Point)
            Dim titleSize As Size = TextRenderer.MeasureText(g, _title, fTitle, New Size(textW, Integer.MaxValue), TextFormatFlags.WordBreak)
            TextRenderer.DrawText(g, _title, fTitle, New Rectangle(x, y, textW, titleSize.Height), Color.White,
                                  TextFormatFlags.WordBreak Or TextFormatFlags.Left)
            y += titleSize.Height + 8
            TextRenderer.DrawText(g, _tagLine, fTag, New Rectangle(x, y, textW, 40), Color.FromArgb(200, 222, 240),
                                  TextFormatFlags.WordBreak Or TextFormatFlags.Left)

            If Not String.IsNullOrEmpty(_footer) Then
                TextRenderer.DrawText(g, _footer, fFoot, New Rectangle(x, r.Height - 44, textW, 20), Color.FromArgb(170, 196, 222),
                                      TextFormatFlags.Left Or TextFormatFlags.VerticalCenter)
            End If
        End Using
    End Sub

End Class
