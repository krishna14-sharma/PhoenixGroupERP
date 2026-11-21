Option Strict On
Option Explicit On

''' <summary>
''' Fast filling of unbound DataGridViews.
''' The classic pattern
'''     For i ... : grid.Rows.Add() : grid(0, i).Value = ... : grid(1, i).Value = ... : Next
''' makes the grid re-validate, re-size and re-paint after every single cell, which
''' takes seconds for a few thousand rows. GridFill builds all rows in memory and
''' adds them with ONE Rows.AddRange call while auto-sizing is paused.
''' The grid ends up with exactly the same rows and values.
''' </summary>
Public NotInheritable Class GridFill

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Replaces the rows of <paramref name="grid"/> with one row per DataRow of <paramref name="table"/>.
    ''' <paramref name="rowValues"/> returns the cell values for a row, in column order.
    ''' </summary>
    Public Shared Sub Fill(ByVal grid As DataGridView, ByVal table As DataTable, ByVal rowValues As Func(Of DataRow, Object()))
        If grid Is Nothing Then Return
        Dim count As Integer = If(table Is Nothing, 0, table.Rows.Count)

        Dim oldRowsMode As DataGridViewAutoSizeRowsMode = grid.AutoSizeRowsMode
        Dim oldColsMode As DataGridViewAutoSizeColumnsMode = grid.AutoSizeColumnsMode
        Dim oldHeaderMode As DataGridViewRowHeadersWidthSizeMode = grid.RowHeadersWidthSizeMode
        grid.SuspendLayout()
        Try
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            If oldHeaderMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders OrElse
               oldHeaderMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders Then
                grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing
            End If

            grid.Rows.Clear()
            If count > 0 Then
                Dim rows(count - 1) As DataGridViewRow
                For i As Integer = 0 To count - 1
                    Dim r As New DataGridViewRow()
                    r.CreateCells(grid, rowValues(table.Rows(i)))
                    rows(i) = r
                Next
                grid.Rows.AddRange(rows)
            End If
        Finally
            grid.AutoSizeColumnsMode = oldColsMode
            grid.AutoSizeRowsMode = oldRowsMode
            grid.RowHeadersWidthSizeMode = oldHeaderMode
            grid.ResumeLayout()
        End Try
    End Sub

End Class
