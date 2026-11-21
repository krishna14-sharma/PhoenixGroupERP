Option Strict On
Option Explicit On

Imports System.Collections.Generic
Imports System.IO
Imports System.Text

''' <summary>One clickable command from the main menu (a leaf menu item).</summary>
Public NotInheritable Class MenuCommand
    Public ReadOnly Item As ToolStripMenuItem
    Public ReadOnly Title As String
    Public ReadOnly Path As String          ' e.g. "General Report > Bank"
    Public ReadOnly TopLevel As ToolStripMenuItem
    Friend ReadOnly SearchText As String     ' lower-case "path title"

    Public Sub New(ByVal item As ToolStripMenuItem, ByVal title As String, ByVal path As String, ByVal topLevel As ToolStripMenuItem)
        Me.Item = item
        Me.Title = title
        Me.Path = path
        Me.TopLevel = topLevel
        Me.SearchText = (path & " " & title).ToLowerInvariant()
    End Sub

    ''' <summary>True when the user may run it right now (role rights are applied by switching items off).</summary>
    Public ReadOnly Property IsUsable As Boolean
        Get
            Dim it As ToolStripItem = Item
            While it IsNot Nothing
                If Not it.Available OrElse Not it.Enabled Then Return False
                it = it.OwnerItem
            End While
            Return True
        End Get
    End Property

    Public Sub Run()
        If Not IsUsable Then Return
        MenuCatalog.RecordUse(Me)
        Item.PerformClick()
    End Sub
End Class

''' <summary>
''' Reads the main MenuStrip into a flat list of commands (used by the dashboard
''' and the Ctrl+K search) and remembers how often each one is used, per Windows user.
''' </summary>
Public NotInheritable Class MenuCatalog

    Private Sub New()
    End Sub

    Private Shared ReadOnly _usage As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    Private Shared _usageLoaded As Boolean
    Private Shared _hooked As New HashSet(Of ToolStripMenuItem)()

    ''' <summary>Raised after a command was used (dashboard refreshes its "frequently used" row).</summary>
    Public Shared Event CommandUsed As EventHandler

    ''' <summary>"&amp;Day Book" -> "Day Book", "P &amp;&amp; L" -> "P &amp; L".</summary>
    Public Shared Function CleanText(ByVal s As String) As String
        If s Is Nothing Then Return String.Empty
        Dim t As String = s.Replace("&&", ChrW(1)).Replace("&", "").Replace(ChrW(1), "&")
        t = t.Replace("....", "").Replace("...", "")
        Dim sb As New StringBuilder(t.Length)
        Dim lastSpace As Boolean = False
        For Each ch As Char In t
            If Char.IsWhiteSpace(ch) Then
                If Not lastSpace Then sb.Append(" "c)
                lastSpace = True
            Else
                sb.Append(ch)
                lastSpace = False
            End If
        Next
        Return sb.ToString().Trim()
    End Function

    ''' <summary>All leaf commands of the menu, in menu order (includes unusable ones; check IsUsable).</summary>
    Public Shared Function Collect(ByVal menu As MenuStrip) As List(Of MenuCommand)
        Dim result As New List(Of MenuCommand)()
        If menu Is Nothing Then Return result
        For Each top As ToolStripItem In menu.Items
            Dim tmi As ToolStripMenuItem = TryCast(top, ToolStripMenuItem)
            If tmi Is Nothing OrElse tmi.Tag Is ShellTag Then Continue For
            Walk(tmi, CleanText(tmi.Text), tmi, result)
        Next
        Return result
    End Function

    ''' <summary>Marker for the items the shell itself adds to the menu bar (excluded from the catalog).</summary>
    Friend Shared ReadOnly ShellTag As New Object()

    Private Shared Sub Walk(ByVal parent As ToolStripMenuItem, ByVal path As String, ByVal top As ToolStripMenuItem, ByVal result As List(Of MenuCommand))
        If Not parent.HasDropDownItems Then
            result.Add(New MenuCommand(parent, CleanText(parent.Text), If(parent Is top, "", path), top))
            Hook(parent)
            Return
        End If
        For Each child As ToolStripItem In parent.DropDownItems
            Dim mi As ToolStripMenuItem = TryCast(child, ToolStripMenuItem)
            If mi Is Nothing Then Continue For
            Dim title As String = CleanText(mi.Text)
            If title.Length = 0 OrElse title = "?" Then Continue For
            If mi.HasDropDownItems Then
                Walk(mi, path & "  " & ChrW(8250) & "  " & title, top, result)
            Else
                result.Add(New MenuCommand(mi, title, path, top))
                Hook(mi)
            End If
        Next
    End Sub

    ' every click on a menu item (from the menu bar too) counts as "use"
    Private Shared Sub Hook(ByVal mi As ToolStripMenuItem)
        If _hooked.Contains(mi) Then Return
        _hooked.Add(mi)
        AddHandler mi.Click, AddressOf Item_Click
    End Sub

    Private Shared _suppressNextClick As ToolStripMenuItem
    Private Shared Sub Item_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim mi As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        If mi Is Nothing Then Return
        If mi Is _suppressNextClick Then
            _suppressNextClick = Nothing
            Return
        End If
        Increment(mi.Name)
    End Sub

    Friend Shared Sub RecordUse(ByVal cmd As MenuCommand)
        Increment(cmd.Item.Name)
        _suppressNextClick = cmd.Item   ' PerformClick will raise Click once more - don't count twice
    End Sub

    Private Shared Sub Increment(ByVal key As String)
        If String.IsNullOrEmpty(key) Then Return
        EnsureUsageLoaded()
        Dim n As Integer = 0
        _usage.TryGetValue(key, n)
        _usage(key) = n + 1
        SaveUsage()
        RaiseEvent CommandUsed(Nothing, EventArgs.Empty)
    End Sub

    Public Shared Function UsageCount(ByVal cmd As MenuCommand) As Integer
        EnsureUsageLoaded()
        Dim n As Integer = 0
        _usage.TryGetValue(cmd.Item.Name, n)
        Return n
    End Function

    ''' <summary>Commands shown on a brand-new installation, until real usage takes over.</summary>
    Private Shared ReadOnly DefaultFavourites As String() = {
        "LedgerToolStripMenuItem", "ToolStripMenuItem28", "TrialToolStripMenuItem", "ToolStripMenuItem21",
        "VoucherSToolStripMenuItem", "PLBalanceSheetToolStripMenuItem", "ToolStripMenuItem34", "BankStatementToolStripMenuItem1"}

    ''' <summary>Most used usable commands (falls back to sensible defaults).</summary>
    Public Shared Function Frequent(ByVal all As List(Of MenuCommand), ByVal count As Integer) As List(Of MenuCommand)
        EnsureUsageLoaded()
        Dim usable As New List(Of MenuCommand)()
        For Each c As MenuCommand In all
            If c.IsUsable Then usable.Add(c)
        Next
        Dim used As New List(Of MenuCommand)()
        For Each c As MenuCommand In usable
            If UsageCount(c) > 0 Then used.Add(c)
        Next
        used.Sort(Function(a, b) UsageCount(b).CompareTo(UsageCount(a)))
        Dim result As New List(Of MenuCommand)()
        For Each c As MenuCommand In used
            If result.Count >= count Then Exit For
            result.Add(c)
        Next
        If result.Count < count Then
            For Each name As String In DefaultFavourites
                If result.Count >= count Then Exit For
                For Each c As MenuCommand In usable
                    If String.Equals(c.Item.Name, name, StringComparison.OrdinalIgnoreCase) AndAlso Not result.Contains(c) Then
                        result.Add(c)
                        Exit For
                    End If
                Next
            Next
        End If
        Return result
    End Function

    ''' <summary>
    ''' Ranks commands for a search query: every word must appear; title matches beat
    ''' path matches; prefix matches beat substrings; frequent commands float up.
    ''' </summary>
    Public Shared Function Search(ByVal all As List(Of MenuCommand), ByVal query As String, ByVal max As Integer) As List(Of MenuCommand)
        Dim words() As String = If(query, "").ToLowerInvariant().Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries)
        Dim scored As New List(Of KeyValuePair(Of Integer, MenuCommand))()
        For Each c As MenuCommand In all
            If Not c.IsUsable Then Continue For
            Dim score As Integer = 0
            Dim ok As Boolean = True
            Dim titleLower As String = c.Title.ToLowerInvariant()
            For Each w As String In words
                Dim ti As Integer = titleLower.IndexOf(w, StringComparison.Ordinal)
                If ti >= 0 Then
                    score += If(ti = 0, 40, If(titleLower(ti - 1) = " "c, 30, 15))
                ElseIf c.SearchText.IndexOf(w, StringComparison.Ordinal) >= 0 Then
                    score += 5
                Else
                    ok = False
                    Exit For
                End If
            Next
            If Not ok Then Continue For
            score += Math.Min(UsageCount(c), 50)
            scored.Add(New KeyValuePair(Of Integer, MenuCommand)(score, c))
        Next
        If words.Length > 0 Then
            scored.Sort(Function(a, b)
                            Dim d As Integer = b.Key.CompareTo(a.Key)
                            If d <> 0 Then Return d
                            Return String.Compare(a.Value.Title, b.Value.Title, StringComparison.OrdinalIgnoreCase)
                        End Function)
        End If
        Dim result As New List(Of MenuCommand)()
        For Each kv As KeyValuePair(Of Integer, MenuCommand) In scored
            If result.Count >= max Then Exit For
            result.Add(kv.Value)
        Next
        Return result
    End Function

    '---------------------------------------------------------------------
    ' persistence: %LOCALAPPDATA%\PhoenixGroupERP\menu-usage.txt  (name<TAB>count)
    '---------------------------------------------------------------------
    Private Shared ReadOnly Property UsageFile As String
        Get
            Dim dir As String = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PhoenixGroupERP")
            Return System.IO.Path.Combine(dir, "menu-usage.txt")
        End Get
    End Property

    Private Shared Sub EnsureUsageLoaded()
        If _usageLoaded Then Return
        _usageLoaded = True
        Try
            If Not File.Exists(UsageFile) Then Return
            For Each line As String In File.ReadAllLines(UsageFile)
                Dim parts() As String = line.Split(ChrW(9))
                Dim n As Integer
                If parts.Length = 2 AndAlso Integer.TryParse(parts(1), n) Then _usage(parts(0)) = n
            Next
        Catch
            ' usage statistics are a convenience only
        End Try
    End Sub

    Private Shared Sub SaveUsage()
        Try
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(UsageFile))
            Dim sb As New StringBuilder()
            For Each kv As KeyValuePair(Of String, Integer) In _usage
                sb.Append(kv.Key).Append(ChrW(9)).Append(kv.Value).AppendLine()
            Next
            File.WriteAllText(UsageFile, sb.ToString())
        Catch
        End Try
    End Sub

End Class
