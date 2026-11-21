<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmFeedTransfer
    Inherits ThemedForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.lblHint = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.dtpPostingDate = New System.Windows.Forms.DateTimePicker()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cmbVoucherType = New System.Windows.Forms.ComboBox()
        Me.lblSearch = New System.Windows.Forms.Label()
        Me.txtSearch = New System.Windows.Forms.TextBox()
        Me.btnRefresh = New System.Windows.Forms.Button()
        Me.chkSelectAll = New System.Windows.Forms.CheckBox()
        Me.dgPending = New System.Windows.Forms.DataGridView()
        Me.lblLines = New System.Windows.Forms.Label()
        Me.dgIUTRFList = New System.Windows.Forms.DataGridView()
        Me.lblSelected = New System.Windows.Forms.Label()
        Me.btnPostSelected = New System.Windows.Forms.Button()
        CType(Me.dgPending, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgIUTRFList, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI Semibold", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitle.Location = New System.Drawing.Point(14, 10)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(354, 30)
        Me.lblTitle.TabIndex = 0
        Me.lblTitle.Text = "Feed Transfer - Post To Accounts"
        '
        'lblHint
        '
        Me.lblHint.AutoSize = True
        Me.lblHint.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHint.ForeColor = System.Drawing.Color.DimGray
        Me.lblHint.Location = New System.Drawing.Point(17, 42)
        Me.lblHint.Name = "lblHint"
        Me.lblHint.Size = New System.Drawing.Size(498, 15)
        Me.lblHint.TabIndex = 1
        Me.lblHint.Text = "All inter-unit transfer bills not yet posted. Tick the bills to post, check the date and click Post selected."
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(16, 74)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(81, 17)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Posting date"
        '
        'dtpPostingDate
        '
        Me.dtpPostingDate.CustomFormat = "dd-MMM-yyyy"
        Me.dtpPostingDate.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.dtpPostingDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dtpPostingDate.Location = New System.Drawing.Point(103, 70)
        Me.dtpPostingDate.Name = "dtpPostingDate"
        Me.dtpPostingDate.Size = New System.Drawing.Size(124, 25)
        Me.dtpPostingDate.TabIndex = 3
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(243, 74)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(83, 17)
        Me.Label4.TabIndex = 4
        Me.Label4.Text = "Voucher type"
        '
        'cmbVoucherType
        '
        Me.cmbVoucherType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbVoucherType.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbVoucherType.FormattingEnabled = True
        Me.cmbVoucherType.Location = New System.Drawing.Point(332, 70)
        Me.cmbVoucherType.Name = "cmbVoucherType"
        Me.cmbVoucherType.Size = New System.Drawing.Size(190, 25)
        Me.cmbVoucherType.TabIndex = 5
        '
        'lblSearch
        '
        Me.lblSearch.AutoSize = True
        Me.lblSearch.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSearch.Location = New System.Drawing.Point(540, 74)
        Me.lblSearch.Name = "lblSearch"
        Me.lblSearch.Size = New System.Drawing.Size(47, 17)
        Me.lblSearch.TabIndex = 6
        Me.lblSearch.Text = "Search"
        '
        'txtSearch
        '
        Me.txtSearch.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSearch.Location = New System.Drawing.Point(593, 70)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(200, 25)
        Me.txtSearch.TabIndex = 7
        '
        'btnRefresh
        '
        Me.btnRefresh.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnRefresh.Location = New System.Drawing.Point(805, 68)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(90, 29)
        Me.btnRefresh.TabIndex = 8
        Me.btnRefresh.Text = "Refresh"
        Me.btnRefresh.UseVisualStyleBackColor = True
        '
        'chkSelectAll
        '
        Me.chkSelectAll.AutoSize = True
        Me.chkSelectAll.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkSelectAll.Location = New System.Drawing.Point(18, 106)
        Me.chkSelectAll.Name = "chkSelectAll"
        Me.chkSelectAll.Size = New System.Drawing.Size(173, 21)
        Me.chkSelectAll.TabIndex = 9
        Me.chkSelectAll.Text = "Select all bills ready to post"
        Me.chkSelectAll.UseVisualStyleBackColor = True
        '
        'dgPending
        '
        Me.dgPending.AllowUserToAddRows = False
        Me.dgPending.AllowUserToDeleteRows = False
        Me.dgPending.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgPending.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgPending.Location = New System.Drawing.Point(16, 132)
        Me.dgPending.MultiSelect = False
        Me.dgPending.Name = "dgPending"
        Me.dgPending.RowHeadersVisible = False
        Me.dgPending.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgPending.Size = New System.Drawing.Size(1068, 300)
        Me.dgPending.TabIndex = 10
        '
        'lblLines
        '
        Me.lblLines.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblLines.AutoSize = True
        Me.lblLines.Font = New System.Drawing.Font("Segoe UI Semibold", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLines.Location = New System.Drawing.Point(15, 440)
        Me.lblLines.Name = "lblLines"
        Me.lblLines.Size = New System.Drawing.Size(158, 17)
        Me.lblLines.TabIndex = 11
        Me.lblLines.Text = "Lines of the selected bill"
        '
        'dgIUTRFList
        '
        Me.dgIUTRFList.AllowUserToAddRows = False
        Me.dgIUTRFList.AllowUserToDeleteRows = False
        Me.dgIUTRFList.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgIUTRFList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgIUTRFList.Location = New System.Drawing.Point(16, 460)
        Me.dgIUTRFList.Name = "dgIUTRFList"
        Me.dgIUTRFList.ReadOnly = True
        Me.dgIUTRFList.RowHeadersVisible = False
        Me.dgIUTRFList.Size = New System.Drawing.Size(1068, 120)
        Me.dgIUTRFList.TabIndex = 12
        '
        'lblSelected
        '
        Me.lblSelected.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblSelected.AutoSize = True
        Me.lblSelected.Font = New System.Drawing.Font("Segoe UI Semibold", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSelected.Location = New System.Drawing.Point(15, 598)
        Me.lblSelected.Name = "lblSelected"
        Me.lblSelected.Size = New System.Drawing.Size(132, 19)
        Me.lblSelected.TabIndex = 13
        Me.lblSelected.Text = "No bills selected"
        '
        'btnPostSelected
        '
        Me.btnPostSelected.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPostSelected.Enabled = False
        Me.btnPostSelected.Font = New System.Drawing.Font("Segoe UI Semibold", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnPostSelected.Location = New System.Drawing.Point(924, 590)
        Me.btnPostSelected.Name = "btnPostSelected"
        Me.btnPostSelected.Size = New System.Drawing.Size(160, 36)
        Me.btnPostSelected.TabIndex = 14
        Me.btnPostSelected.Text = "Post selected"
        Me.btnPostSelected.UseVisualStyleBackColor = True
        '
        'frmFeedTransfer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1100, 640)
        Me.Controls.Add(Me.lblTitle)
        Me.Controls.Add(Me.lblHint)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.dtpPostingDate)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.cmbVoucherType)
        Me.Controls.Add(Me.lblSearch)
        Me.Controls.Add(Me.txtSearch)
        Me.Controls.Add(Me.btnRefresh)
        Me.Controls.Add(Me.chkSelectAll)
        Me.Controls.Add(Me.dgPending)
        Me.Controls.Add(Me.lblLines)
        Me.Controls.Add(Me.dgIUTRFList)
        Me.Controls.Add(Me.lblSelected)
        Me.Controls.Add(Me.btnPostSelected)
        Me.MinimumSize = New System.Drawing.Size(900, 520)
        Me.Name = "frmFeedTransfer"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Feed Transfer - Post To Accounts"
        CType(Me.dgPending, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgIUTRFList, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents lblHint As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents dtpPostingDate As System.Windows.Forms.DateTimePicker
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents cmbVoucherType As System.Windows.Forms.ComboBox
    Friend WithEvents lblSearch As System.Windows.Forms.Label
    Friend WithEvents txtSearch As System.Windows.Forms.TextBox
    Friend WithEvents btnRefresh As System.Windows.Forms.Button
    Friend WithEvents chkSelectAll As System.Windows.Forms.CheckBox
    Friend WithEvents dgPending As System.Windows.Forms.DataGridView
    Friend WithEvents lblLines As System.Windows.Forms.Label
    Friend WithEvents dgIUTRFList As System.Windows.Forms.DataGridView
    Friend WithEvents lblSelected As System.Windows.Forms.Label
    Friend WithEvents btnPostSelected As System.Windows.Forms.Button
End Class
