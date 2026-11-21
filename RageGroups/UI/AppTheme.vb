Option Strict On
Option Explicit On

Imports System.Configuration
Imports System.Drawing

''' <summary>
''' Central design tokens for the Phoenix Group ERP user interface.
''' Every colour / font used by the modern look lives here so the whole
''' application can be re-branded from one place.
''' </summary>
Public NotInheritable Class AppTheme

    Private Sub New()
    End Sub

    '---------------------------------------------------------------------
    ' Brand
    '---------------------------------------------------------------------
    Public Shared ReadOnly Primary As Color = Color.FromArgb(31, 78, 121)          ' deep corporate blue
    Public Shared ReadOnly PrimaryDark As Color = Color.FromArgb(21, 52, 82)
    Public Shared ReadOnly PrimaryHover As Color = Color.FromArgb(42, 96, 145)
    Public Shared ReadOnly PrimaryLight As Color = Color.FromArgb(232, 240, 249)
    Public Shared ReadOnly Accent As Color = Color.FromArgb(224, 123, 57)           ' phoenix orange

    '---------------------------------------------------------------------
    ' Neutrals
    '---------------------------------------------------------------------
    Public Shared ReadOnly Surface As Color = Color.FromArgb(244, 246, 249)          ' window background
    Public Shared ReadOnly Card As Color = Color.White                              ' panels / inputs
    Public Shared ReadOnly SurfaceAlt As Color = Color.FromArgb(236, 240, 245)
    Public Shared ReadOnly Border As Color = Color.FromArgb(213, 219, 227)
    Public Shared ReadOnly BorderStrong As Color = Color.FromArgb(184, 194, 207)
    Public Shared ReadOnly Text As Color = Color.FromArgb(31, 41, 51)
    Public Shared ReadOnly TextMuted As Color = Color.FromArgb(107, 119, 133)
    Public Shared ReadOnly TextDisabled As Color = Color.FromArgb(160, 168, 179)
    Public Shared ReadOnly TextOnDark As Color = Color.White

    '---------------------------------------------------------------------
    ' States
    '---------------------------------------------------------------------
    Public Shared ReadOnly Hover As Color = Color.FromArgb(234, 241, 250)
    Public Shared ReadOnly Pressed As Color = Color.FromArgb(214, 228, 243)
    Public Shared ReadOnly Selection As Color = Color.FromArgb(207, 225, 245)
    Public Shared ReadOnly SelectionText As Color = Color.FromArgb(15, 42, 68)
    Public Shared ReadOnly InputFocus As Color = Color.FromArgb(255, 249, 230)
    Public Shared ReadOnly InputReadOnly As Color = Color.FromArgb(241, 244, 248)
    Public Shared ReadOnly Highlight As Color = Color.FromArgb(255, 244, 214)        ' replaces loud "attention" colours

    '---------------------------------------------------------------------
    ' Semantic text colours (legacy red / green / blue labels are mapped to these)
    '---------------------------------------------------------------------
    Public Shared ReadOnly Danger As Color = Color.FromArgb(192, 57, 43)
    Public Shared ReadOnly Success As Color = Color.FromArgb(46, 125, 50)
    Public Shared ReadOnly Warning As Color = Color.FromArgb(183, 121, 31)
    Public Shared ReadOnly Info As Color = Primary
    Public Shared ReadOnly WarningBanner As Color = Color.FromArgb(255, 236, 179)

    '---------------------------------------------------------------------
    ' Grid
    '---------------------------------------------------------------------
    Public Shared ReadOnly GridHeader As Color = Color.FromArgb(237, 241, 246)
    Public Shared ReadOnly GridLine As Color = Color.FromArgb(225, 230, 237)
    Public Shared ReadOnly GridAlternate As Color = Color.FromArgb(248, 250, 252)

    '---------------------------------------------------------------------
    ' Typography
    '---------------------------------------------------------------------
    Public Const FontFamilyName As String = "Segoe UI"
    Public Const FontFamilySemibold As String = "Segoe UI Semibold"

    Private Shared _uiFont As Font
    Public Shared ReadOnly Property UiFont As Font
        Get
            If _uiFont Is Nothing Then _uiFont = New Font(FontFamilyName, 9.0F, FontStyle.Regular)
            Return _uiFont
        End Get
    End Property

    '---------------------------------------------------------------------
    ' Switches (app.config  <appSettings>)
    '   UiTheme       = Modern | Classic       (Classic = original look)
    '   UiModernFonts = true | false           (false keeps original fonts)
    '---------------------------------------------------------------------
    Private Shared _loaded As Boolean
    Private Shared _enabled As Boolean = True
    Private Shared _modernFonts As Boolean = True

    Public Shared ReadOnly Property Enabled As Boolean
        Get
            EnsureLoaded()
            Return _enabled
        End Get
    End Property

    Public Shared ReadOnly Property ModernFonts As Boolean
        Get
            EnsureLoaded()
            Return _modernFonts
        End Get
    End Property

    ''' <summary>
    ''' Background of combo boxes (darker than text boxes so drop-downs stand out).
    ''' Override in app.config:  &lt;add key="UiComboBack" value="#CFDDED"/&gt;  (or "207,221,237").
    ''' </summary>
    Public Shared ReadOnly Property ComboBack As Color
        Get
            EnsureLoaded()
            Return _comboBack
        End Get
    End Property
    Private Shared _comboBack As Color = Color.FromArgb(207, 221, 237)

    Private Shared Function ParseColor(ByVal s As String, ByVal fallback As Color) As Color
        If String.IsNullOrEmpty(s) Then Return fallback
        Try
            Dim parts() As String = s.Split(","c)
            If parts.Length = 3 Then Return Color.FromArgb(CInt(parts(0).Trim()), CInt(parts(1).Trim()), CInt(parts(2).Trim()))
            Return ColorTranslator.FromHtml(s.Trim())
        Catch
            Return fallback
        End Try
    End Function

    Private Shared Sub EnsureLoaded()
        If _loaded Then Return
        _loaded = True
        Try
            _comboBack = ParseColor(ConfigurationManager.AppSettings("UiComboBack"), _comboBack)
            Dim t As String = ConfigurationManager.AppSettings("UiTheme")
            If t IsNot Nothing AndAlso t.Trim().Equals("Classic", StringComparison.OrdinalIgnoreCase) Then _enabled = False
            Dim f As String = ConfigurationManager.AppSettings("UiModernFonts")
            If f IsNot Nothing AndAlso f.Trim().Equals("false", StringComparison.OrdinalIgnoreCase) Then _modernFonts = False
        Catch
            ' configuration problems must never stop the application
        End Try
    End Sub

    '---------------------------------------------------------------------
    ' Colour helpers
    '---------------------------------------------------------------------
    ''' <summary>Relative luminance 0 (black) .. 1 (white).</summary>
    Public Shared Function Luminance(ByVal c As Color) As Double
        Return (0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B) / 255.0
    End Function

    Public Shared Function IsDark(ByVal c As Color) As Boolean
        Return c.A > 0 AndAlso Luminance(c) < 0.45
    End Function

    ''' <summary>HSV saturation 0..1.</summary>
    Public Shared Function Saturation(ByVal c As Color) As Double
        Dim mx As Integer = Math.Max(c.R, Math.Max(c.G, c.B))
        Dim mn As Integer = Math.Min(c.R, Math.Min(c.G, c.B))
        If mx = 0 Then Return 0
        Return (mx - mn) / CDbl(mx)
    End Function

    Public Shared Function Blend(ByVal a As Color, ByVal b As Color, ByVal amount As Double) As Color
        Dim r As Integer = CInt(a.R + (CInt(b.R) - a.R) * amount)
        Dim g As Integer = CInt(a.G + (CInt(b.G) - a.G) * amount)
        Dim bl As Integer = CInt(a.B + (CInt(b.B) - a.B) * amount)
        Return Color.FromArgb(r, g, bl)
    End Function

    ''' <summary>
    ''' Maps a legacy foreground colour onto the palette while keeping its meaning
    ''' (red stays "danger", green stays "success", blue stays "primary"...).
    ''' </summary>
    Public Shared Function MapForeground(ByVal original As Color, ByVal background As Color) As Color
        If IsDark(background) Then Return TextOnDark
        If original.A = 0 Then Return Text
        If Luminance(original) > 0.72 Then Return Text           ' white text that sat on a coloured bar
        If Saturation(original) < 0.3 Then Return Text           ' black / grey / navy-ish greys
        Dim h As Single = original.GetHue()
        If h < 18 OrElse h >= 330 Then Return Danger
        If h < 75 Then Return Warning
        If h < 170 Then Return Success
        Return Primary                                          ' blues, teals, purples
    End Function

End Class
