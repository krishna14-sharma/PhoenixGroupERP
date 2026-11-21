Option Strict On
Option Explicit On

Imports System.Collections.Generic
Imports System.Drawing

''' <summary>
''' "Change company / session" dialog. Lists the companies assigned to the
''' signed-in user (Cmp_assignment) and the financial years he may open
''' (SessionMaster); returns the chosen combination.
''' </summary>
Public Class SessionPicker
    Inherits ThemedForm

    Private Class CompanyItem
        Public Id As String
        Public Name As String
    End Class

    Private ReadOnly _companies As New List(Of CompanyItem)()
    Private ReadOnly _sessions As New List(Of String)()
    Private ReadOnly _cmpList As New ListBox()
    Private ReadOnly _sesList As New ListBox()
    Private ReadOnly _ok As New Button()
    Private ReadOnly _cancel As New Button()
    Private ReadOnly _hint As New Label()

    ''' <summary>"2526" -> "FY 2025-26"</summary>
    Public Shared Function Describe(ByVal session As String) As String
        Dim n As Integer
        If session IsNot Nothing AndAlso session.Length = 4 AndAlso Integer.TryParse(session, n) Then
            Return "FY 20" & session.Substring(0, 2) & "-" & session.Substring(2, 2)
        End If
        Return "Session " & session
    End Function

    ''' <summary>Shows the dialog. Returns True and the chosen company id / session, or False when cancelled.</summary>
    Public Shared Function Pick(ByVal owner As IWin32Window, ByRef companyId As String, ByRef session As String) As Boolean
        Dim companies As New List(Of CompanyItem)()
        Dim sessions As New List(Of String)()
        Try
            Dim tc As DataTable = Db.GetTable("select cam.Cmp_id, cmp.Cmpname from Cmp_assignment cam " &
                                              "inner join Company cmp on cam.Cmp_id = cmp.Cmp_id where cam.Uname = @u order by cmp.Cmpname",
                                              Db.P("@u", GMod.username))
            For Each r As DataRow In tc.Rows
                Dim id As String = Convert.ToString(r(0)).Trim()
                If id.Length = 0 Then Continue For
                Dim exists As Boolean = False
                For Each c As CompanyItem In companies
                    If String.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase) Then exists = True : Exit For
                Next
                If Not exists Then companies.Add(New CompanyItem() With {.Id = id, .Name = Convert.ToString(r(1)).Trim()})
            Next
            Dim ts As DataTable = Db.GetTable("select session from SessionMaster where uname = @u order by SessionId desc", Db.P("@u", GMod.username))
            For Each r As DataRow In ts.Rows
                Dim s As String = Convert.ToString(r(0)).Trim()
                If s.Length > 0 AndAlso Not sessions.Contains(s) Then sessions.Add(s)
            Next
        Catch ex As Exception
            MessageBox.Show(owner, "Could not read your companies / sessions:" & Environment.NewLine & ex.Message,
                            "Change company / session", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End Try
        If companies.Count <= 1 AndAlso sessions.Count <= 1 Then
            MessageBox.Show(owner, "You are assigned to only one company and one session." & Environment.NewLine &
                            "Ask the administrator to assign more (Master Entries > Assign Company to User, Tools > Session Permission To User).",
                            "Change company / session", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        Using dlg As New SessionPicker(companies, sessions)
            If dlg.ShowDialog(owner) = DialogResult.OK Then
                companyId = dlg._companies(dlg._cmpList.SelectedIndex).Id
                session = dlg._sessions(dlg._sesList.SelectedIndex)
                Return True
            End If
        End Using
        Return False
    End Function

    Private Sub New(ByVal companies As List(Of CompanyItem), ByVal sessions As List(Of String))
        _companies.AddRange(companies)
        _sessions.AddRange(sessions)
        Me.Text = "Change company / session"
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ClientSize = New Size(Dpi.S(700), Dpi.S(440))
        Me.Font = ThemeManager.GetFont(9.0F, FontStyle.Regular)

        Dim title As New Label() With {.Text = "Open another company or financial year", .AutoSize = False,
            .Font = ThemeManager.GetFont(12.0F, FontStyle.Bold), .Location = New Point(Dpi.S(20), Dpi.S(16)), .Size = New Size(Dpi.S(660), Dpi.S(26))}
        Dim current As New Label() With {.Text = "Open now: " & GMod.Cmpname & "  " & ChrW(183) & "  " & Describe(GMod.Session) &
            ".   Your open screens will close.", .AutoSize = False, .ForeColor = AppTheme.TextMuted,
            .Location = New Point(Dpi.S(20), Dpi.S(44)), .Size = New Size(Dpi.S(660), Dpi.S(20))}
        Dim capC As New Label() With {.Text = "Company", .AutoSize = False, .Font = ThemeManager.GetFont(9.0F, FontStyle.Bold),
            .Location = New Point(Dpi.S(20), Dpi.S(76)), .Size = New Size(Dpi.S(380), Dpi.S(18))}
        Dim capS As New Label() With {.Text = "Financial year", .AutoSize = False, .Font = ThemeManager.GetFont(9.0F, FontStyle.Bold),
            .Location = New Point(Dpi.S(416), Dpi.S(76)), .Size = New Size(Dpi.S(264), Dpi.S(18))}

        SetupList(_cmpList, New Point(Dpi.S(20), Dpi.S(98)), New Size(Dpi.S(380), Dpi.S(270)))
        For Each c As CompanyItem In _companies
            _cmpList.Items.Add(c.Name)
        Next
        SetupList(_sesList, New Point(Dpi.S(416), Dpi.S(98)), New Size(Dpi.S(264), Dpi.S(270)))
        For Each s As String In _sessions
            _sesList.Items.Add(s)
        Next
        AddHandler _cmpList.DrawItem, AddressOf List_DrawItem
        AddHandler _sesList.DrawItem, AddressOf List_DrawItem
        AddHandler _cmpList.SelectedIndexChanged, Sub(s, e) UpdateState()
        AddHandler _sesList.SelectedIndexChanged, Sub(s, e) UpdateState()
        AddHandler _cmpList.DoubleClick, Sub(s, e) If _ok.Enabled Then Me.DialogResult = DialogResult.OK
        AddHandler _sesList.DoubleClick, Sub(s, e) If _ok.Enabled Then Me.DialogResult = DialogResult.OK

        _hint.AutoSize = False
        _hint.ForeColor = AppTheme.TextMuted
        _hint.Location = New Point(Dpi.S(20), Dpi.S(390))
        _hint.Size = New Size(Dpi.S(420), Dpi.S(32))
        _hint.TextAlign = ContentAlignment.MiddleLeft

        _ok.Text = "Open"
        _ok.Size = New Size(Dpi.S(110), Dpi.S(32))
        _ok.Location = New Point(Dpi.S(450), Dpi.S(390))
        _ok.DialogResult = DialogResult.OK
        _cancel.Text = "Cancel"
        _cancel.Size = New Size(Dpi.S(110), Dpi.S(32))
        _cancel.Location = New Point(Dpi.S(570), Dpi.S(390))
        _cancel.DialogResult = DialogResult.Cancel

        Me.Controls.AddRange(New Control() {title, current, capC, capS, _cmpList, _sesList, _hint, _ok, _cancel})
        Me.AcceptButton = _ok
        Me.CancelButton = _cancel

        ' start on the company / year that is open now
        _cmpList.SelectedIndex = Math.Max(0, IndexOfCompany(GMod.Cmpid))
        _sesList.SelectedIndex = Math.Max(0, _sessions.FindIndex(Function(x) String.Equals(x, GMod.Session, StringComparison.OrdinalIgnoreCase)))
        UpdateState()
    End Sub

    Private Sub SetupList(ByVal lb As ListBox, ByVal location As Point, ByVal size As Size)
        lb.Location = location
        lb.Size = size
        lb.IntegralHeight = False
        lb.Font = ThemeManager.GetFont(10.5F, FontStyle.Regular)
        lb.DrawMode = DrawMode.OwnerDrawFixed
        lb.ItemHeight = Dpi.S(32)
    End Sub

    Private Function IndexOfCompany(ByVal id As String) As Integer
        For i As Integer = 0 To _companies.Count - 1
            If String.Equals(_companies(i).Id, id, StringComparison.OrdinalIgnoreCase) Then Return i
        Next
        Return -1
    End Function

    Private Function IsCurrentCompany(ByVal i As Integer) As Boolean
        Return String.Equals(_companies(i).Id, GMod.Cmpid, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function IsCurrentSession(ByVal i As Integer) As Boolean
        Return String.Equals(_sessions(i), GMod.Session, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub UpdateState()
        Dim ok As Boolean = _cmpList.SelectedIndex >= 0 AndAlso _sesList.SelectedIndex >= 0 AndAlso
                            Not (IsCurrentCompany(_cmpList.SelectedIndex) AndAlso IsCurrentSession(_sesList.SelectedIndex))
        _ok.Enabled = ok
        If _cmpList.SelectedIndex >= 0 AndAlso _sesList.SelectedIndex >= 0 Then
            _hint.Text = If(ok, "Will open: " & _companies(_cmpList.SelectedIndex).Name & "  " & ChrW(183) & "  " & Describe(_sessions(_sesList.SelectedIndex)),
                            "This company and year are already open.")
        End If
    End Sub

    Private Sub List_DrawItem(ByVal sender As Object, ByVal e As DrawItemEventArgs)
        If e.Index < 0 Then Return
        Dim lb As ListBox = DirectCast(sender, ListBox)
        Dim isCmp As Boolean = lb Is _cmpList
        Dim selected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected
        Dim current As Boolean = If(isCmp, IsCurrentCompany(e.Index), IsCurrentSession(e.Index))
        Using b As New SolidBrush(If(selected, AppTheme.Selection, AppTheme.Card))
            e.Graphics.FillRectangle(b, e.Bounds)
        End Using
        Dim r As New Rectangle(e.Bounds.Left + Dpi.S(10), e.Bounds.Top, e.Bounds.Width - Dpi.S(20), e.Bounds.Height)
        Dim text As String = If(isCmp, _companies(e.Index).Name, Describe(_sessions(e.Index)) & "  (" & _sessions(e.Index) & ")")
        Dim tagW As Integer = If(current, Dpi.S(60), 0)
        TextRenderer.DrawText(e.Graphics, text, lb.Font, New Rectangle(r.Left, r.Top, r.Width - tagW, r.Height),
                              If(selected, AppTheme.SelectionText, AppTheme.Text),
                              TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
        If current Then
            TextRenderer.DrawText(e.Graphics, "open now", ThemeManager.GetFont(8.25F, FontStyle.Regular), r, AppTheme.TextMuted,
                                  TextFormatFlags.Right Or TextFormatFlags.VerticalCenter)
        End If
    End Sub

End Class
