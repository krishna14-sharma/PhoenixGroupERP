''' <summary>
''' Kept for backward compatibility: any code that still does
''' "strip.Renderer = New CustomRenderer" now gets the modern renderer
''' (previous-session menus are still highlighted in amber).
''' </summary>
Public Class CustomRenderer
    Inherits ModernToolStripRenderer
End Class
