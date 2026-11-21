Option Strict On
Option Explicit On

Imports System.Collections.Generic
Imports System.Drawing
Imports System.Reflection
Imports System.Runtime.CompilerServices

''' <summary>
''' Applies the modern look to any form at runtime, without touching the
''' 250+ designer files one by one.
'''
''' Rules (all colours come from <see cref="AppTheme"/>):
'''  * Form / panels      : legacy pastel backgrounds -> neutral surface / white cards,
'''                         dark bands (title bars)  -> brand blue with white text.
'''  * Labels             : inherit the background; red/green/blue meaning is kept.
'''  * Inputs             : white, flat single border, soft focus highlight;
'''                         loud "attention" colours become a soft amber highlight.
'''  * Buttons            : flat; Save/Login/OK/Show... become primary (blue),
'''                         Delete/Remove become danger (red outline).
'''  * DataGridView       : flat header, light grid lines, zebra rows,
'''                         double-buffered for fast scrolling.
'''  * Menus / tool strips: <see cref="ModernToolStripRenderer"/>.
'''  * Legacy fonts (MS Sans Serif, Verdana, Tahoma, Arial...) -> Segoe UI,
'''    same size and style so layouts do not move.
'''
''' Opt-out: set a form's or control's Tag to "NoTheme", or put
''' &lt;add key="UiTheme" value="Classic"/&gt; in app.config.
''' </summary>
Public NotInheritable Class ThemeManager

    Private Sub New()
    End Sub

    Public Const SkipTag As String = "NoTheme"

    Private Shared ReadOnly LegacyFonts As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "Microsoft Sans Serif", "MS Sans Serif", "Tahoma", "Verdana", "Arial", "Arial Narrow",
        "Times New Roman", "Book Antiqua", "Trebuchet MS", "Calibri", "Cambria", "Comic Sans MS",
        "Century Gothic", "Georgia", "Garamond", "Palatino Linotype", "Bookman Old Style",
        "Lucida Sans", "Lucida Sans Unicode", "Franklin Gothic Medium", "Segoe UI"}

    Private Shared ReadOnly PrimaryWords As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "save", "login", "log in", "sign in", "ok", "submit", "update", "show", "view", "generate", "process",
        "post", "post selected", "preview", "display", "go", "apply", "authorize", "authorise", "transfer", "import", "save & print", "save and print"}

    Private Shared ReadOnly DangerWords As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "delete", "remove", "del", "delete voucher", "delete entry"}

    Private Shared ReadOnly FontCache As New Dictionary(Of String, Font)(StringComparer.Ordinal)
    Private Shared ReadOnly FontLock As New Object()
    Private Shared ReadOnly Hooked As New ConditionalWeakTable(Of Control, Object)()
    Private Shared ReadOnly DoubleBufferedProperty As PropertyInfo =
        GetType(Control).GetProperty("DoubleBuffered", BindingFlags.Instance Or BindingFlags.NonPublic)

    Private Shared _renderer As ModernToolStripRenderer
    Public Shared ReadOnly Property Renderer As ModernToolStripRenderer
        Get
            If _renderer Is Nothing Then _renderer = New ModernToolStripRenderer()
            Return _renderer
        End Get
    End Property

    Private Structure Original
        Public Back As Color
        Public Fore As Color
    End Structure

    '=====================================================================
    ' Public API
    '=====================================================================

    ''' <summary>Themes a complete form (called automatically by ThemedForm.OnLoad).</summary>
    Public Shared Sub ApplyTheme(ByVal f As Form)
        If f Is Nothing OrElse Not AppTheme.Enabled OrElse IsSkipped(f) Then Return

        ' 1. snapshot colours BEFORE we change anything (ambient colours would otherwise leak)
        Dim snap As New Dictionary(Of Control, Original)()
        Dim formOriginal As Color = f.BackColor
        For Each c As Control In f.Controls
            Snapshot(c, snap)
        Next

        f.SuspendLayout()
        Try
            If f.BackgroundImage Is Nothing AndAlso f.TransparencyKey.IsEmpty AndAlso Not f.IsMdiContainer Then
                f.BackColor = AppTheme.Surface
            End If
            f.ForeColor = AppTheme.Text
            If f.ContextMenuStrip IsNot Nothing Then StyleStrip(f.ContextMenuStrip)

            Dim resolved As Color = f.BackColor
            For Each c As Control In f.Controls
                ApplyControl(c, formOriginal, resolved, snap)
            Next
            HookControlAdded(f)
        Finally
            f.ResumeLayout(True)
        End Try
    End Sub

    ''' <summary>Themes a single control tree (e.g. a control created at runtime).</summary>
    Public Shared Sub ApplyTheme(ByVal c As Control)
        If c Is Nothing OrElse Not AppTheme.Enabled Then Return
        Dim parentBack As Color = If(c.Parent IsNot Nothing, c.Parent.BackColor, AppTheme.Surface)
        Dim snap As New Dictionary(Of Control, Original)()
        Snapshot(c, snap)
        ApplyControl(c, parentBack, parentBack, snap)
    End Sub

    ''' <summary>Returns the Segoe UI equivalent of a legacy font (cached, never disposed).</summary>
    Public Shared Function MapFont(ByVal f As Font) As Font
        If f Is Nothing OrElse Not AppTheme.ModernFonts Then Return f
        If Not LegacyFonts.Contains(f.Name) Then Return f
        Return GetFont(f.Size, f.Style, f.Unit)
    End Function

    Public Shared Function GetFont(ByVal size As Single, ByVal style As FontStyle, Optional ByVal unit As GraphicsUnit = GraphicsUnit.Point) As Font
        Dim key As String = size.ToString(Global.System.Globalization.CultureInfo.InvariantCulture) & "|" & CInt(style).ToString() & "|" & CInt(unit).ToString()
        SyncLock FontLock
            Dim cached As Font = Nothing
            If FontCache.TryGetValue(key, cached) Then Return cached
            cached = New Font(AppTheme.FontFamilyName, size, style, unit)
            FontCache(key) = cached
            Return cached
        End SyncLock
    End Function

    ''' <summary>Turns on double buffering for any control (big win for grids and list views).</summary>
    Public Shared Sub EnableDoubleBuffer(ByVal c As Control)
        Try
            If DoubleBufferedProperty IsNot Nothing Then DoubleBufferedProperty.SetValue(c, True, Nothing)
        Catch
        End Try
    End Sub

    Public Shared Sub StyleStrip(ByVal ts As ToolStrip)
        If ts Is Nothing Then Return
        ts.RenderMode = ToolStripRenderMode.ManagerRenderMode
        ts.Renderer = Renderer
        If TypeOf ts Is MenuStrip Then
            ts.Padding = New Padding(6, 3, 0, 3)
        End If
        If AppTheme.ModernFonts Then ts.Font = MapFont(ts.Font)
        StyleStripItems(ts.Items, TypeOf ts Is MenuStrip)
    End Sub

    '=====================================================================
    ' Internals
    '=====================================================================

    Private Shared Function IsSkipped(ByVal c As Control) As Boolean
        Dim t As String = TryCast(c.Tag, String)
        Return t IsNot Nothing AndAlso t.IndexOf(SkipTag, StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    Private Shared Function IsForeignControl(ByVal c As Control) As Boolean
        ' third-party viewers style themselves - never walk into them
        Dim n As String = c.GetType().FullName
        Return n.StartsWith("CrystalDecisions.", StringComparison.Ordinal)
    End Function

    Private Shared Sub Snapshot(ByVal c As Control, ByVal snap As Dictionary(Of Control, Original))
        If snap.ContainsKey(c) Then Return
        Dim o As Original
        o.Back = c.BackColor
        o.Fore = c.ForeColor
        snap(c) = o
        If IsForeignControl(c) Then Return
        For Each child As Control In c.Controls
            Snapshot(child, snap)
        Next
    End Sub

    Private Shared Function GetOriginal(ByVal c As Control, ByVal snap As Dictionary(Of Control, Original)) As Original
        Dim o As Original
        If snap.TryGetValue(c, o) Then Return o
        o.Back = c.BackColor
        o.Fore = c.ForeColor
        Return o
    End Function

    Private Shared Function SameColor(ByVal a As Color, ByVal b As Color) As Boolean
        Return a.ToArgb() = b.ToArgb()
    End Function

    Private Shared Sub ApplyControl(ByVal c As Control, ByVal parentOriginal As Color, ByVal parentResolved As Color,
                                    ByVal snap As Dictionary(Of Control, Original))
        If IsSkipped(c) OrElse IsForeignControl(c) Then Return
        Dim o As Original = GetOriginal(c, snap)
        Dim resolved As Color = parentResolved

        ' Fonts: never on ContainerControls (UserControl, SplitContainer...) because
        ' a font change there triggers AutoScale and would move the layout.
        If AppTheme.ModernFonts AndAlso Not TypeOf c Is ContainerControl Then
            Dim nf As Font = MapFont(c.Font)
            If nf IsNot c.Font Then c.Font = nf
        End If

        If c.ContextMenuStrip IsNot Nothing Then StyleStrip(c.ContextMenuStrip)

        If TypeOf c Is DataGridView Then
            StyleGrid(DirectCast(c, DataGridView))
            Return
        ElseIf TypeOf c Is ToolStrip Then
            StyleStrip(DirectCast(c, ToolStrip))
            Return
        ElseIf TypeOf c Is ButtonBase AndAlso Not (TypeOf c Is CheckBox OrElse TypeOf c Is RadioButton) Then
            StyleButton(DirectCast(c, ButtonBase))
            Return
        ElseIf TypeOf c Is TextBoxBase Then
            StyleTextBox(DirectCast(c, TextBoxBase), o, parentResolved)
            Return
        ElseIf TypeOf c Is ComboBox Then
            StyleCombo(DirectCast(c, ComboBox), o)
            Return
        ElseIf TypeOf c Is ListBox OrElse TypeOf c Is ListView OrElse TypeOf c Is TreeView Then
            StyleList(c, o)
            Return
        ElseIf TypeOf c Is UpDownBase Then
            Dim ud As UpDownBase = DirectCast(c, UpDownBase)
            ' keep Fixed3D: with visual styles Windows draws it as the native thin grey border (blue on focus); FixedSingle would be black
            ud.BackColor = If(AppTheme.IsDark(o.Back), AppTheme.Card, If(AppTheme.Saturation(o.Back) > 0.6, AppTheme.Highlight, AppTheme.Card))
            ud.ForeColor = AppTheme.MapForeground(o.Fore, ud.BackColor)
            HookFocus(ud)
            Return
        ElseIf TypeOf c Is DateTimePicker OrElse TypeOf c Is PictureBox OrElse TypeOf c Is ProgressBar OrElse TypeOf c Is ScrollBar Then
            Return
        ElseIf TypeOf c Is Label Then
            StyleLabel(DirectCast(c, Label), o, parentOriginal, parentResolved)
            Return
        ElseIf TypeOf c Is CheckBox OrElse TypeOf c Is RadioButton Then
            StyleCheck(DirectCast(c, ButtonBase), o, parentOriginal, parentResolved)
            Return
        ElseIf TypeOf c Is TabControl Then
            Dim tc As TabControl = DirectCast(c, TabControl)
            For Each page As TabPage In tc.TabPages
                Dim po As Original = GetOriginal(page, snap)
                page.UseVisualStyleBackColor = False
                page.BackColor = If(AppTheme.IsDark(po.Back), AppTheme.Primary, AppTheme.Card)
                page.ForeColor = AppTheme.MapForeground(po.Fore, page.BackColor)
                If AppTheme.ModernFonts Then page.Font = MapFont(page.Font)
                For Each child As Control In page.Controls
                    ApplyControl(child, po.Back, page.BackColor, snap)
                Next
                HookControlAdded(page)
            Next
            Return
        ElseIf TypeOf c Is GroupBox Then
            Dim gb As GroupBox = DirectCast(c, GroupBox)
            If AppTheme.IsDark(o.Back) AndAlso Not SameColor(o.Back, parentOriginal) Then
                gb.BackColor = AppTheme.Primary
            Else
                gb.BackColor = parentResolved
            End If
            resolved = gb.BackColor
            gb.ForeColor = If(AppTheme.IsDark(resolved), AppTheme.TextOnDark, AppTheme.Primary)
        Else
            ' Panels, SplitterPanels, FlowLayout/TableLayout panels, user controls...
            resolved = ResolveContainerBack(o.Back, parentOriginal, parentResolved)
            If c.BackgroundImage Is Nothing Then
                If resolved.A = 0 Then
                    resolved = parentResolved
                Else
                    c.BackColor = resolved
                End If
            Else
                resolved = c.BackColor
            End If
            c.ForeColor = AppTheme.MapForeground(o.Fore, resolved)
            Dim pnl As Panel = TryCast(c, Panel)
            If pnl IsNot Nothing AndAlso pnl.BorderStyle = BorderStyle.Fixed3D Then pnl.BorderStyle = BorderStyle.FixedSingle
        End If

        For Each child As Control In c.Controls
            ApplyControl(child, o.Back, resolved, snap)
        Next
        HookControlAdded(c)
    End Sub

    Private Shared Function ResolveContainerBack(ByVal original As Color, ByVal parentOriginal As Color, ByVal parentResolved As Color) As Color
        If original.A = 0 Then Return Color.Transparent
        If SameColor(original, parentOriginal) OrElse SameColor(original, parentResolved) Then Return parentResolved
        If AppTheme.IsDark(original) Then Return AppTheme.Primary
        If SameColor(parentResolved, AppTheme.Card) Then Return AppTheme.Surface
        Return AppTheme.Card
    End Function

    Private Shared Sub StyleLabel(ByVal lbl As Label, ByVal o As Original, ByVal parentOriginal As Color, ByVal parentResolved As Color)
        Dim bg As Color
        If o.Back.A = 0 Then
            bg = parentResolved                         ' stays transparent
        ElseIf SameColor(o.Back, parentOriginal) OrElse SameColor(o.Back, parentResolved) Then
            lbl.BackColor = parentResolved
            bg = parentResolved
        ElseIf AppTheme.IsDark(o.Back) Then
            lbl.BackColor = AppTheme.Primary               ' title / header band
            bg = AppTheme.Primary
        ElseIf AppTheme.Saturation(o.Back) > 0.55 Then
            lbl.BackColor = AppTheme.Highlight             ' "look at me" boxes (totals, balances)
            bg = AppTheme.Highlight
        Else
            lbl.BackColor = parentResolved
            bg = parentResolved
        End If
        If AppTheme.IsDark(parentResolved) AndAlso o.Back.A = 0 Then bg = parentResolved
        lbl.ForeColor = AppTheme.MapForeground(o.Fore, bg)
        If lbl.BorderStyle = BorderStyle.Fixed3D Then lbl.BorderStyle = BorderStyle.FixedSingle

        Dim link As LinkLabel = TryCast(lbl, LinkLabel)
        If link IsNot Nothing Then
            link.LinkColor = If(AppTheme.IsDark(bg), AppTheme.TextOnDark, AppTheme.Primary)
            link.ActiveLinkColor = AppTheme.Accent
            link.VisitedLinkColor = link.LinkColor
        End If
    End Sub

    Private Shared Sub StyleCheck(ByVal b As ButtonBase, ByVal o As Original, ByVal parentOriginal As Color, ByVal parentResolved As Color)
        Dim bg As Color = parentResolved
        If o.Back.A > 0 AndAlso AppTheme.IsDark(o.Back) AndAlso Not SameColor(o.Back, parentOriginal) Then
            bg = AppTheme.Primary
        End If
        If o.Back.A > 0 Then b.BackColor = bg
        b.ForeColor = AppTheme.MapForeground(o.Fore, bg)

        Dim isButtonLike As Boolean =
            (TypeOf b Is CheckBox AndAlso DirectCast(b, CheckBox).Appearance = Appearance.Button) OrElse
            (TypeOf b Is RadioButton AndAlso DirectCast(b, RadioButton).Appearance = Appearance.Button)
        If isButtonLike Then
            b.FlatStyle = FlatStyle.Flat
            b.FlatAppearance.BorderColor = AppTheme.BorderStrong
            b.FlatAppearance.CheckedBackColor = AppTheme.Selection
            b.FlatAppearance.MouseOverBackColor = AppTheme.Hover
            b.BackColor = AppTheme.Card
            b.ForeColor = AppTheme.Text
        End If
    End Sub

    Private Shared Function NormalizedText(ByVal s As String) As String
        If s Is Nothing Then Return String.Empty
        Return s.Replace("&&", ChrW(1)).Replace("&", "").Replace(ChrW(1), "&").Trim().TrimEnd("."c, ":"c).Trim()
    End Function

    Private Shared Sub StyleButton(ByVal b As ButtonBase)
        If b.BackgroundImage IsNot Nothing Then Return   ' picture buttons keep their artwork

        Dim txt As String = NormalizedText(b.Text)
        Dim isPrimary As Boolean = PrimaryWords.Contains(txt)
        Dim isDanger As Boolean = DangerWords.Contains(txt)
        Dim f As Form = b.FindForm()
        If f IsNot Nothing AndAlso f.AcceptButton Is TryCast(b, IButtonControl) AndAlso Not isDanger Then isPrimary = True

        b.FlatStyle = FlatStyle.Flat
        Dim btn As Button = TryCast(b, Button)
        If btn IsNot Nothing Then btn.UseVisualStyleBackColor = False
        b.FlatAppearance.BorderSize = 1
        b.Cursor = Cursors.Hand

        If isPrimary Then
            b.BackColor = AppTheme.Primary
            b.ForeColor = AppTheme.TextOnDark
            b.FlatAppearance.BorderColor = AppTheme.Primary
            b.FlatAppearance.MouseOverBackColor = AppTheme.PrimaryHover
            b.FlatAppearance.MouseDownBackColor = AppTheme.PrimaryDark
        ElseIf isDanger Then
            b.BackColor = AppTheme.Card
            b.ForeColor = AppTheme.Danger
            b.FlatAppearance.BorderColor = AppTheme.Blend(AppTheme.Danger, Color.White, 0.45)
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(253, 236, 234)
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(249, 213, 209)
        Else
            b.BackColor = AppTheme.Card
            b.ForeColor = AppTheme.Text
            b.FlatAppearance.BorderColor = AppTheme.BorderStrong
            b.FlatAppearance.MouseOverBackColor = AppTheme.Hover
            b.FlatAppearance.MouseDownBackColor = AppTheme.Pressed
        End If
    End Sub

    Private Shared Sub StyleTextBox(ByVal tb As TextBoxBase, ByVal o As Original, ByVal parentResolved As Color)
        If tb.BorderStyle = BorderStyle.None Then
            ' borderless text boxes are used as labels / display areas
            tb.BackColor = If(AppTheme.IsDark(o.Back), AppTheme.Primary, parentResolved)
            tb.ForeColor = AppTheme.MapForeground(o.Fore, tb.BackColor)
            Return
        End If
        ' keep Fixed3D: with visual styles Windows draws it as the native thin grey border (blue on focus); FixedSingle would be black

        If tb.ReadOnly OrElse Not tb.Enabled Then
            tb.BackColor = If(AppTheme.Saturation(o.Back) > 0.6 AndAlso Not AppTheme.IsDark(o.Back), AppTheme.Highlight, AppTheme.InputReadOnly)
        ElseIf AppTheme.Saturation(o.Back) > 0.6 AndAlso Not AppTheme.IsDark(o.Back) Then
            tb.BackColor = AppTheme.Highlight
        Else
            tb.BackColor = AppTheme.Card
        End If
        tb.ForeColor = AppTheme.MapForeground(o.Fore, tb.BackColor)
        HookFocus(tb)
    End Sub

    Private Shared Sub StyleCombo(ByVal cb As ComboBox, ByVal o As Original)
        If cb.DropDownStyle <> ComboBoxStyle.Simple Then cb.FlatStyle = FlatStyle.Flat
        If AppTheme.Saturation(o.Back) > 0.6 AndAlso Not AppTheme.IsDark(o.Back) Then
            cb.BackColor = AppTheme.Highlight
        Else
            cb.BackColor = AppTheme.ComboBack      ' darker tint so drop-downs stand out from text boxes
        End If
        cb.ForeColor = AppTheme.MapForeground(o.Fore, cb.BackColor)
        HookFocus(cb)
    End Sub

    Private Shared Sub StyleList(ByVal c As Control, ByVal o As Original)
        c.BackColor = If(AppTheme.Saturation(o.Back) > 0.6 AndAlso Not AppTheme.IsDark(o.Back), AppTheme.Highlight, AppTheme.Card)
        c.ForeColor = AppTheme.MapForeground(o.Fore, c.BackColor)
        Dim lb As ListBox = TryCast(c, ListBox)
        ' keep Fixed3D: with visual styles Windows draws it as the native thin grey border (blue on focus); FixedSingle would be black
        Dim lv As ListView = TryCast(c, ListView)
        If lv IsNot Nothing Then
            EnableDoubleBuffer(lv)
        End If
        Dim tv As TreeView = TryCast(c, TreeView)
        If tv IsNot Nothing Then
            tv.LineColor = AppTheme.BorderStrong
        End If
    End Sub

    Private Shared Sub StyleGrid(ByVal g As DataGridView)
        EnableDoubleBuffer(g)

        g.EnableHeadersVisualStyles = False
        g.BackgroundColor = AppTheme.Card
        If g.BorderStyle = BorderStyle.Fixed3D Then g.BorderStyle = BorderStyle.FixedSingle
        g.GridColor = AppTheme.GridLine
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
        g.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single

        ' header
        Dim hs As DataGridViewCellStyle = g.ColumnHeadersDefaultCellStyle
        hs.BackColor = AppTheme.GridHeader
        hs.ForeColor = AppTheme.Text
        hs.SelectionBackColor = AppTheme.GridHeader
        hs.SelectionForeColor = AppTheme.Text
        Dim headerFont As Font = If(hs.Font, g.Font)
        If AppTheme.ModernFonts AndAlso headerFont IsNot Nothing AndAlso LegacyFonts.Contains(headerFont.Name) Then
            hs.Font = GetFont(headerFont.Size, headerFont.Style Or FontStyle.Bold, headerFont.Unit)
        End If
        If g.ColumnHeadersHeightSizeMode <> DataGridViewColumnHeadersHeightSizeMode.AutoSize AndAlso g.ColumnHeadersHeight < 28 Then
            g.ColumnHeadersHeight = 28
        End If

        ' row headers
        Dim rh As DataGridViewCellStyle = g.RowHeadersDefaultCellStyle
        rh.BackColor = AppTheme.GridHeader
        rh.ForeColor = AppTheme.TextMuted
        rh.SelectionBackColor = AppTheme.Selection
        rh.SelectionForeColor = AppTheme.SelectionText

        ' cells
        Dim ds As DataGridViewCellStyle = g.DefaultCellStyle
        If ds.BackColor.IsEmpty OrElse Not AppTheme.IsDark(ds.BackColor) Then ds.BackColor = AppTheme.Card
        ds.ForeColor = AppTheme.MapForeground(ds.ForeColor, ds.BackColor)
        ds.SelectionBackColor = AppTheme.Selection
        ds.SelectionForeColor = AppTheme.SelectionText
        If AppTheme.ModernFonts Then
            If ds.Font IsNot Nothing Then ds.Font = MapFont(ds.Font)
            If g.RowsDefaultCellStyle.Font IsNot Nothing Then g.RowsDefaultCellStyle.Font = MapFont(g.RowsDefaultCellStyle.Font)
        End If
        If Not g.RowsDefaultCellStyle.BackColor.IsEmpty AndAlso Not AppTheme.IsDark(g.RowsDefaultCellStyle.BackColor) Then
            g.RowsDefaultCellStyle.BackColor = AppTheme.Card
        End If
        If Not g.RowsDefaultCellStyle.SelectionBackColor.IsEmpty Then
            g.RowsDefaultCellStyle.SelectionBackColor = AppTheme.Selection
            g.RowsDefaultCellStyle.SelectionForeColor = AppTheme.SelectionText
        End If

        ' columns: keep intentional column colours, but modernise fonts and selection
        Dim columnHasColour As Boolean = False
        For Each col As DataGridViewColumn In g.Columns
            Dim cs As DataGridViewCellStyle = col.DefaultCellStyle
            If cs Is Nothing Then Continue For
            If AppTheme.ModernFonts AndAlso cs.Font IsNot Nothing Then cs.Font = MapFont(cs.Font)
            If Not cs.BackColor.IsEmpty AndAlso Not SameColor(cs.BackColor, Color.White) Then
                columnHasColour = True
                If AppTheme.Saturation(cs.BackColor) > 0.35 AndAlso Not AppTheme.IsDark(cs.BackColor) Then cs.BackColor = AppTheme.Highlight
            End If
            If Not cs.SelectionBackColor.IsEmpty Then
                cs.SelectionBackColor = AppTheme.Selection
                cs.SelectionForeColor = AppTheme.SelectionText
            End If
            If col.HeaderCell.HasStyle AndAlso col.HeaderCell.Style.Font IsNot Nothing AndAlso AppTheme.ModernFonts Then
                col.HeaderCell.Style.Font = MapFont(col.HeaderCell.Style.Font)
            End If
        Next

        ' zebra rows (only when columns don't carry their own colours - those would be hidden)
        If Not columnHasColour Then
            g.AlternatingRowsDefaultCellStyle.BackColor = AppTheme.GridAlternate
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = AppTheme.Selection
            g.AlternatingRowsDefaultCellStyle.SelectionForeColor = AppTheme.SelectionText
        ElseIf Not g.AlternatingRowsDefaultCellStyle.BackColor.IsEmpty Then
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.Empty
        End If

        ' comfortable row height for the new font
        Dim cellFont As Font = If(ds.Font, g.Font)
        If cellFont IsNot Nothing Then
            Dim wanted As Integer = Math.Max(24, cellFont.Height + 8)
            If g.RowTemplate.Height < wanted AndAlso g.Rows.Count = 0 Then g.RowTemplate.Height = wanted
        End If
    End Sub

    Private Shared Sub StyleStripItems(ByVal items As ToolStripItemCollection, ByVal topLevel As Boolean)
        For Each it As ToolStripItem In items
            If AppTheme.ModernFonts AndAlso it.Font IsNot Nothing AndAlso LegacyFonts.Contains(it.Font.Name) Then
                it.Font = MapFont(it.Font)
            End If
            Dim mi As ToolStripMenuItem = TryCast(it, ToolStripMenuItem)
            If mi IsNot Nothing Then
                If topLevel Then
                    mi.Padding = New Padding(10, 2, 10, 2)
                Else
                    mi.Padding = New Padding(0, 1, 0, 1)
                End If
                If mi.HasDropDownItems Then
                    mi.DropDown.Renderer = Renderer
                    StyleStripItems(mi.DropDownItems, False)
                End If
            End If
        Next
    End Sub

    '---------------------------------------------------------------------
    ' focus highlight for inputs: only touches the colour we set ourselves,
    ' so existing GotFocus/LostFocus colour code in the forms keeps working.
    '---------------------------------------------------------------------
    Private Shared Sub HookFocus(ByVal c As Control)
        Dim dummy As Object = Nothing
        If Hooked.TryGetValue(c, dummy) Then Return
        Hooked.Add(c, Nothing)
        AddHandler c.Enter, AddressOf Input_Enter
        AddHandler c.Leave, AddressOf Input_Leave
    End Sub

    Private Shared Sub Input_Enter(ByVal sender As Object, ByVal e As EventArgs)
        Dim c As Control = DirectCast(sender, Control)
        If SameColor(c.BackColor, AppTheme.Card) OrElse SameColor(c.BackColor, AppTheme.ComboBack) Then c.BackColor = AppTheme.InputFocus
    End Sub

    Private Shared Sub Input_Leave(ByVal sender As Object, ByVal e As EventArgs)
        Dim c As Control = DirectCast(sender, Control)
        If SameColor(c.BackColor, AppTheme.InputFocus) Then c.BackColor = If(TypeOf c Is ComboBox, AppTheme.ComboBack, AppTheme.Card)
    End Sub

    '---------------------------------------------------------------------
    ' controls created at runtime (Controls.Add in code) get themed too
    '---------------------------------------------------------------------
    Private Shared ReadOnly ContainerHooks As New ConditionalWeakTable(Of Control, Object)()

    Private Shared Sub HookControlAdded(ByVal c As Control)
        Dim dummy As Object = Nothing
        If ContainerHooks.TryGetValue(c, dummy) Then Return
        ContainerHooks.Add(c, Nothing)
        AddHandler c.ControlAdded, AddressOf Container_ControlAdded
    End Sub

    Private Shared Sub Container_ControlAdded(ByVal sender As Object, ByVal e As ControlEventArgs)
        Try
            Dim parent As Control = DirectCast(sender, Control)
            Dim snap As New Dictionary(Of Control, Original)()
            Snapshot(e.Control, snap)
            ApplyControl(e.Control, parent.BackColor, parent.BackColor, snap)
        Catch
            ' theming must never break a screen
        End Try
    End Sub

End Class
