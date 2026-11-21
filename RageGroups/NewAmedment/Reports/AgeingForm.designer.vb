<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AgeingForm
    Inherits ThemedForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            If disposing Then overdueFont.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle7 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.printDoc = New System.Drawing.Printing.PrintDocument()
        Me.filterBar = New System.Windows.Forms.FlowLayoutPanel()
        Me.lblSessionCaption = New System.Windows.Forms.Label()
        Me.dtpAsOn = New System.Windows.Forms.DateTimePicker()
        Me.btnLoad = New System.Windows.Forms.Button()
        Me.lblSearchCaption = New System.Windows.Forms.Label()
        Me.txtSearch = New System.Windows.Forms.TextBox()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.btnPrint = New System.Windows.Forms.Button()
        Me.summaryBar = New System.Windows.Forms.FlowLayoutPanel()
        Me.pnlParties = New System.Windows.Forms.Panel()
        Me.lblPartiesTitle = New System.Windows.Forms.Label()
        Me.lblParties = New System.Windows.Forms.Label()
        Me.pnl0To30 = New System.Windows.Forms.Panel()
        Me.lbl0To30Title = New System.Windows.Forms.Label()
        Me.lbl0To30 = New System.Windows.Forms.Label()
        Me.pnl31To60 = New System.Windows.Forms.Panel()
        Me.lbl31To60Title = New System.Windows.Forms.Label()
        Me.lbl31To60 = New System.Windows.Forms.Label()
        Me.pnlAbove60 = New System.Windows.Forms.Panel()
        Me.lblAbove60Title = New System.Windows.Forms.Label()
        Me.lblAbove60 = New System.Windows.Forms.Label()
        Me.pnlTotal = New System.Windows.Forms.Panel()
        Me.lblTotalTitle = New System.Windows.Forms.Label()
        Me.lblTotal = New System.Windows.Forms.Label()
        Me.grid = New System.Windows.Forms.DataGridView()
        Me.colPartyCode = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPartyName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colProduct = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colAmt0To30 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colAmt31To60 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colAmtAbove60 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTotalDue = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colOldestDays = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.status = New System.Windows.Forms.StatusStrip()
        Me.lblStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.filterBar.SuspendLayout()
        Me.summaryBar.SuspendLayout()
        Me.pnlParties.SuspendLayout()
        Me.pnl0To30.SuspendLayout()
        Me.pnl31To60.SuspendLayout()
        Me.pnlAbove60.SuspendLayout()
        Me.pnlTotal.SuspendLayout()
        CType(Me.grid, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.status.SuspendLayout()
        Me.SuspendLayout()
        '
        'printDoc
        '
        Me.printDoc.DocumentName = "Party Ageing Report"
        '
        'filterBar
        '
        Me.filterBar.AutoSize = True
        Me.filterBar.Controls.Add(Me.lblSessionCaption)
        Me.filterBar.Controls.Add(Me.dtpAsOn)
        Me.filterBar.Controls.Add(Me.btnLoad)
        Me.filterBar.Controls.Add(Me.lblSearchCaption)
        Me.filterBar.Controls.Add(Me.txtSearch)
        Me.filterBar.Controls.Add(Me.btnExport)
        Me.filterBar.Controls.Add(Me.btnPrint)
        Me.filterBar.Dock = System.Windows.Forms.DockStyle.Top
        Me.filterBar.Location = New System.Drawing.Point(0, 0)
        Me.filterBar.Name = "filterBar"
        Me.filterBar.Padding = New System.Windows.Forms.Padding(8, 8, 8, 4)
        Me.filterBar.Size = New System.Drawing.Size(1164, 45)
        Me.filterBar.TabIndex = 0
        '
        'lblSessionCaption
        '
        Me.lblSessionCaption.AutoSize = True
        Me.lblSessionCaption.Location = New System.Drawing.Point(16, 15)
        Me.lblSessionCaption.Margin = New System.Windows.Forms.Padding(8, 7, 3, 0)
        Me.lblSessionCaption.Name = "lblSessionCaption"
        Me.lblSessionCaption.Size = New System.Drawing.Size(52, 17)
        Me.lblSessionCaption.TabIndex = 0
        Me.lblSessionCaption.Text = "As on"
        '
        'dtpAsOn
        '
        Me.dtpAsOn.CustomFormat = "dd-MMM-yyyy"
        Me.dtpAsOn.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dtpAsOn.Location = New System.Drawing.Point(74, 11)
        Me.dtpAsOn.Name = "dtpAsOn"
        Me.dtpAsOn.Size = New System.Drawing.Size(120, 24)
        Me.dtpAsOn.TabIndex = 1
        '
        'btnLoad
        '
        Me.btnLoad.AutoSize = True
        Me.btnLoad.Location = New System.Drawing.Point(421, 11)
        Me.btnLoad.Name = "btnLoad"
        Me.btnLoad.Size = New System.Drawing.Size(75, 27)
        Me.btnLoad.TabIndex = 4
        Me.btnLoad.Text = "Load"
        Me.btnLoad.UseVisualStyleBackColor = True
        '
        'lblSearchCaption
        '
        Me.lblSearchCaption.AutoSize = True
        Me.lblSearchCaption.Location = New System.Drawing.Point(507, 15)
        Me.lblSearchCaption.Margin = New System.Windows.Forms.Padding(8, 7, 3, 0)
        Me.lblSearchCaption.Name = "lblSearchCaption"
        Me.lblSearchCaption.Size = New System.Drawing.Size(81, 17)
        Me.lblSearchCaption.TabIndex = 5
        Me.lblSearchCaption.Text = "Search party"
        '
        'txtSearch
        '
        Me.txtSearch.Location = New System.Drawing.Point(594, 11)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(200, 24)
        Me.txtSearch.TabIndex = 6
        '
        'btnExport
        '
        Me.btnExport.AutoSize = True
        Me.btnExport.Enabled = False
        Me.btnExport.Location = New System.Drawing.Point(976, 11)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(85, 27)
        Me.btnExport.TabIndex = 8
        Me.btnExport.Text = "Export CSV"
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'btnPrint
        '
        Me.btnPrint.AutoSize = True
        Me.btnPrint.Enabled = False
        Me.btnPrint.Location = New System.Drawing.Point(1067, 11)
        Me.btnPrint.Name = "btnPrint"
        Me.btnPrint.Size = New System.Drawing.Size(75, 27)
        Me.btnPrint.TabIndex = 9
        Me.btnPrint.Text = "Print"
        Me.btnPrint.UseVisualStyleBackColor = True
        '
        'summaryBar
        '
        Me.summaryBar.AutoSize = True
        Me.summaryBar.Controls.Add(Me.pnlParties)
        Me.summaryBar.Controls.Add(Me.pnl0To30)
        Me.summaryBar.Controls.Add(Me.pnl31To60)
        Me.summaryBar.Controls.Add(Me.pnlAbove60)
        Me.summaryBar.Controls.Add(Me.pnlTotal)
        Me.summaryBar.Dock = System.Windows.Forms.DockStyle.Top
        Me.summaryBar.Location = New System.Drawing.Point(0, 45)
        Me.summaryBar.Name = "summaryBar"
        Me.summaryBar.Padding = New System.Windows.Forms.Padding(8, 2, 8, 6)
        Me.summaryBar.Size = New System.Drawing.Size(1164, 76)
        Me.summaryBar.TabIndex = 1
        '
        'pnlParties
        '
        Me.pnlParties.BackColor = System.Drawing.Color.White
        Me.pnlParties.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlParties.Controls.Add(Me.lblPartiesTitle)
        Me.pnlParties.Controls.Add(Me.lblParties)
        Me.pnlParties.Location = New System.Drawing.Point(11, 5)
        Me.pnlParties.Name = "pnlParties"
        Me.pnlParties.Size = New System.Drawing.Size(195, 62)
        Me.pnlParties.TabIndex = 0
        '
        'lblPartiesTitle
        '
        Me.lblPartiesTitle.AutoSize = True
        Me.lblPartiesTitle.ForeColor = System.Drawing.Color.DimGray
        Me.lblPartiesTitle.Location = New System.Drawing.Point(8, 6)
        Me.lblPartiesTitle.Name = "lblPartiesTitle"
        Me.lblPartiesTitle.Size = New System.Drawing.Size(47, 17)
        Me.lblPartiesTitle.TabIndex = 0
        Me.lblPartiesTitle.Text = "Parties"
        '
        'lblParties
        '
        Me.lblParties.AutoSize = True
        Me.lblParties.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold)
        Me.lblParties.ForeColor = System.Drawing.Color.FromArgb(CType(CType(55, Byte), Integer), CType(CType(65, Byte), Integer), CType(CType(81, Byte), Integer))
        Me.lblParties.Location = New System.Drawing.Point(7, 28)
        Me.lblParties.Name = "lblParties"
        Me.lblParties.Size = New System.Drawing.Size(22, 25)
        Me.lblParties.TabIndex = 1
        Me.lblParties.Text = "0"
        '
        'pnl0To30
        '
        Me.pnl0To30.BackColor = System.Drawing.Color.White
        Me.pnl0To30.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnl0To30.Controls.Add(Me.lbl0To30Title)
        Me.pnl0To30.Controls.Add(Me.lbl0To30)
        Me.pnl0To30.Location = New System.Drawing.Point(212, 5)
        Me.pnl0To30.Name = "pnl0To30"
        Me.pnl0To30.Size = New System.Drawing.Size(195, 62)
        Me.pnl0To30.TabIndex = 1
        '
        'lbl0To30Title
        '
        Me.lbl0To30Title.AutoSize = True
        Me.lbl0To30Title.ForeColor = System.Drawing.Color.DimGray
        Me.lbl0To30Title.Location = New System.Drawing.Point(8, 6)
        Me.lbl0To30Title.Name = "lbl0To30Title"
        Me.lbl0To30Title.Size = New System.Drawing.Size(65, 17)
        Me.lbl0To30Title.TabIndex = 0
        Me.lbl0To30Title.Text = "0-30 days"
        '
        'lbl0To30
        '
        Me.lbl0To30.AutoSize = True
        Me.lbl0To30.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold)
        Me.lbl0To30.ForeColor = System.Drawing.Color.FromArgb(CType(CType(21, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(61, Byte), Integer))
        Me.lbl0To30.Location = New System.Drawing.Point(7, 28)
        Me.lbl0To30.Name = "lbl0To30"
        Me.lbl0To30.Size = New System.Drawing.Size(22, 25)
        Me.lbl0To30.TabIndex = 1
        Me.lbl0To30.Text = "0"
        '
        'pnl31To60
        '
        Me.pnl31To60.BackColor = System.Drawing.Color.White
        Me.pnl31To60.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnl31To60.Controls.Add(Me.lbl31To60Title)
        Me.pnl31To60.Controls.Add(Me.lbl31To60)
        Me.pnl31To60.Location = New System.Drawing.Point(413, 5)
        Me.pnl31To60.Name = "pnl31To60"
        Me.pnl31To60.Size = New System.Drawing.Size(195, 62)
        Me.pnl31To60.TabIndex = 2
        '
        'lbl31To60Title
        '
        Me.lbl31To60Title.AutoSize = True
        Me.lbl31To60Title.ForeColor = System.Drawing.Color.DimGray
        Me.lbl31To60Title.Location = New System.Drawing.Point(8, 6)
        Me.lbl31To60Title.Name = "lbl31To60Title"
        Me.lbl31To60Title.Size = New System.Drawing.Size(72, 17)
        Me.lbl31To60Title.TabIndex = 0
        Me.lbl31To60Title.Text = "31-60 days"
        '
        'lbl31To60
        '
        Me.lbl31To60.AutoSize = True
        Me.lbl31To60.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold)
        Me.lbl31To60.ForeColor = System.Drawing.Color.FromArgb(CType(CType(180, Byte), Integer), CType(CType(83, Byte), Integer), CType(CType(9, Byte), Integer))
        Me.lbl31To60.Location = New System.Drawing.Point(7, 28)
        Me.lbl31To60.Name = "lbl31To60"
        Me.lbl31To60.Size = New System.Drawing.Size(22, 25)
        Me.lbl31To60.TabIndex = 1
        Me.lbl31To60.Text = "0"
        '
        'pnlAbove60
        '
        Me.pnlAbove60.BackColor = System.Drawing.Color.White
        Me.pnlAbove60.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlAbove60.Controls.Add(Me.lblAbove60Title)
        Me.pnlAbove60.Controls.Add(Me.lblAbove60)
        Me.pnlAbove60.Location = New System.Drawing.Point(614, 5)
        Me.pnlAbove60.Name = "pnlAbove60"
        Me.pnlAbove60.Size = New System.Drawing.Size(195, 62)
        Me.pnlAbove60.TabIndex = 3
        '
        'lblAbove60Title
        '
        Me.lblAbove60Title.AutoSize = True
        Me.lblAbove60Title.ForeColor = System.Drawing.Color.DimGray
        Me.lblAbove60Title.Location = New System.Drawing.Point(8, 6)
        Me.lblAbove60Title.Name = "lblAbove60Title"
        Me.lblAbove60Title.Size = New System.Drawing.Size(94, 17)
        Me.lblAbove60Title.TabIndex = 0
        Me.lblAbove60Title.Text = "Above 60 days"
        '
        'lblAbove60
        '
        Me.lblAbove60.AutoSize = True
        Me.lblAbove60.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold)
        Me.lblAbove60.ForeColor = System.Drawing.Color.FromArgb(CType(CType(185, Byte), Integer), CType(CType(28, Byte), Integer), CType(CType(28, Byte), Integer))
        Me.lblAbove60.Location = New System.Drawing.Point(7, 28)
        Me.lblAbove60.Name = "lblAbove60"
        Me.lblAbove60.Size = New System.Drawing.Size(22, 25)
        Me.lblAbove60.TabIndex = 1
        Me.lblAbove60.Text = "0"
        '
        'pnlTotal
        '
        Me.pnlTotal.BackColor = System.Drawing.Color.White
        Me.pnlTotal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlTotal.Controls.Add(Me.lblTotalTitle)
        Me.pnlTotal.Controls.Add(Me.lblTotal)
        Me.pnlTotal.Location = New System.Drawing.Point(815, 5)
        Me.pnlTotal.Name = "pnlTotal"
        Me.pnlTotal.Size = New System.Drawing.Size(195, 62)
        Me.pnlTotal.TabIndex = 4
        '
        'lblTotalTitle
        '
        Me.lblTotalTitle.AutoSize = True
        Me.lblTotalTitle.ForeColor = System.Drawing.Color.DimGray
        Me.lblTotalTitle.Location = New System.Drawing.Point(8, 6)
        Me.lblTotalTitle.Name = "lblTotalTitle"
        Me.lblTotalTitle.Size = New System.Drawing.Size(62, 17)
        Me.lblTotalTitle.TabIndex = 0
        Me.lblTotalTitle.Text = "Total due"
        '
        'lblTotal
        '
        Me.lblTotal.AutoSize = True
        Me.lblTotal.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold)
        Me.lblTotal.ForeColor = System.Drawing.Color.FromArgb(CType(CType(29, Byte), Integer), CType(CType(78, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.lblTotal.Location = New System.Drawing.Point(7, 28)
        Me.lblTotal.Name = "lblTotal"
        Me.lblTotal.Size = New System.Drawing.Size(22, 25)
        Me.lblTotal.TabIndex = 1
        Me.lblTotal.Text = "0"
        '
        'grid
        '
        Me.grid.AllowUserToAddRows = False
        Me.grid.AllowUserToDeleteRows = False
        Me.grid.AllowUserToResizeRows = False
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(CType(CType(245, Byte), Integer), CType(CType(247, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.grid.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        Me.grid.BackgroundColor = System.Drawing.Color.White
        DataGridViewCellStyle2.Font = New System.Drawing.Font("Segoe UI", 9.5!)
        Me.grid.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle2
        Me.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grid.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colPartyCode, Me.colPartyName, Me.colProduct, Me.colAmt0To30, Me.colAmt31To60, Me.colAmtAbove60, Me.colTotalDue, Me.colOldestDays})
        Me.grid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grid.EnableHeadersVisualStyles = False
        Me.grid.Location = New System.Drawing.Point(0, 121)
        Me.grid.MultiSelect = False
        Me.grid.Name = "grid"
        Me.grid.ReadOnly = True
        Me.grid.RowHeadersVisible = False
        Me.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.grid.Size = New System.Drawing.Size(1164, 498)
        Me.grid.TabIndex = 2
        '
        'colPartyCode
        '
        Me.colPartyCode.DataPropertyName = "PartyCode"
        Me.colPartyCode.HeaderText = "Code"
        Me.colPartyCode.Name = "colPartyCode"
        Me.colPartyCode.ReadOnly = True
        Me.colPartyCode.Width = 80
        '
        'colPartyName
        '
        Me.colPartyName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colPartyName.DataPropertyName = "PartyName"
        Me.colPartyName.HeaderText = "Party"
        Me.colPartyName.Name = "colPartyName"
        Me.colPartyName.ReadOnly = True
        '
        'colProduct
        '
        Me.colProduct.DataPropertyName = "Product"
        Me.colProduct.HeaderText = "Product"
        Me.colProduct.Name = "colProduct"
        Me.colProduct.ReadOnly = True
        Me.colProduct.Width = 150
        '
        'colAmt0To30
        '
        Me.colAmt0To30.DataPropertyName = "Amt0To30"
        DataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle3.Format = "N2"
        Me.colAmt0To30.DefaultCellStyle = DataGridViewCellStyle3
        Me.colAmt0To30.HeaderText = "0-30 days"
        Me.colAmt0To30.Name = "colAmt0To30"
        Me.colAmt0To30.ReadOnly = True
        Me.colAmt0To30.Width = 125
        '
        'colAmt31To60
        '
        Me.colAmt31To60.DataPropertyName = "Amt31To60"
        DataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle4.Format = "N2"
        Me.colAmt31To60.DefaultCellStyle = DataGridViewCellStyle4
        Me.colAmt31To60.HeaderText = "31-60 days"
        Me.colAmt31To60.Name = "colAmt31To60"
        Me.colAmt31To60.ReadOnly = True
        Me.colAmt31To60.Width = 125
        '
        'colAmtAbove60
        '
        Me.colAmtAbove60.DataPropertyName = "AmtAbove60"
        DataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle5.Format = "N2"
        Me.colAmtAbove60.DefaultCellStyle = DataGridViewCellStyle5
        Me.colAmtAbove60.HeaderText = "Above 60 days"
        Me.colAmtAbove60.Name = "colAmtAbove60"
        Me.colAmtAbove60.ReadOnly = True
        Me.colAmtAbove60.Width = 125
        '
        'colTotalDue
        '
        Me.colTotalDue.DataPropertyName = "TotalDue"
        DataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle6.Format = "N2"
        Me.colTotalDue.DefaultCellStyle = DataGridViewCellStyle6
        Me.colTotalDue.HeaderText = "Total due"
        Me.colTotalDue.Name = "colTotalDue"
        Me.colTotalDue.ReadOnly = True
        Me.colTotalDue.Width = 125
        '
        'colOldestDays
        '
        Me.colOldestDays.DataPropertyName = "OldestDays"
        DataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight
        Me.colOldestDays.DefaultCellStyle = DataGridViewCellStyle7
        Me.colOldestDays.HeaderText = "Oldest 60+ (days)"
        Me.colOldestDays.Name = "colOldestDays"
        Me.colOldestDays.ReadOnly = True
        Me.colOldestDays.Width = 125
        '
        'status
        '
        Me.status.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lblStatus})
        Me.status.Location = New System.Drawing.Point(0, 619)
        Me.status.Name = "status"
        Me.status.Size = New System.Drawing.Size(1164, 22)
        Me.status.TabIndex = 3
        '
        'lblStatus
        '
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(272, 17)
        Me.lblStatus.Text = "Choose the As on date, then click Load."
        '
        'AgeingForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ClientSize = New System.Drawing.Size(1164, 641)
        Me.Controls.Add(Me.grid)
        Me.Controls.Add(Me.summaryBar)
        Me.Controls.Add(Me.filterBar)
        Me.Controls.Add(Me.status)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.5!)
        Me.MinimumSize = New System.Drawing.Size(960, 560)
        Me.Name = "AgeingForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Party Ageing Report"
        Me.filterBar.ResumeLayout(False)
        Me.filterBar.PerformLayout()
        Me.summaryBar.ResumeLayout(False)
        Me.pnlParties.ResumeLayout(False)
        Me.pnlParties.PerformLayout()
        Me.pnl0To30.ResumeLayout(False)
        Me.pnl0To30.PerformLayout()
        Me.pnl31To60.ResumeLayout(False)
        Me.pnl31To60.PerformLayout()
        Me.pnlAbove60.ResumeLayout(False)
        Me.pnlAbove60.PerformLayout()
        Me.pnlTotal.ResumeLayout(False)
        Me.pnlTotal.PerformLayout()
        CType(Me.grid, System.ComponentModel.ISupportInitialize).EndInit()
        Me.status.ResumeLayout(False)
        Me.status.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents printDoc As System.Drawing.Printing.PrintDocument
    Friend WithEvents filterBar As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents summaryBar As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents dtpAsOn As System.Windows.Forms.DateTimePicker
    Friend WithEvents txtSearch As System.Windows.Forms.TextBox
    Friend WithEvents btnLoad As System.Windows.Forms.Button
    Friend WithEvents btnExport As System.Windows.Forms.Button
    Friend WithEvents btnPrint As System.Windows.Forms.Button
    Friend WithEvents grid As System.Windows.Forms.DataGridView
    Friend WithEvents status As System.Windows.Forms.StatusStrip
    Friend WithEvents lblStatus As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents lblSessionCaption As System.Windows.Forms.Label
    Friend WithEvents lblSearchCaption As System.Windows.Forms.Label
    Friend WithEvents pnlParties As System.Windows.Forms.Panel
    Friend WithEvents lblPartiesTitle As System.Windows.Forms.Label
    Friend WithEvents lblParties As System.Windows.Forms.Label
    Friend WithEvents pnl0To30 As System.Windows.Forms.Panel
    Friend WithEvents lbl0To30Title As System.Windows.Forms.Label
    Friend WithEvents lbl0To30 As System.Windows.Forms.Label
    Friend WithEvents pnl31To60 As System.Windows.Forms.Panel
    Friend WithEvents lbl31To60Title As System.Windows.Forms.Label
    Friend WithEvents lbl31To60 As System.Windows.Forms.Label
    Friend WithEvents pnlAbove60 As System.Windows.Forms.Panel
    Friend WithEvents lblAbove60Title As System.Windows.Forms.Label
    Friend WithEvents lblAbove60 As System.Windows.Forms.Label
    Friend WithEvents pnlTotal As System.Windows.Forms.Panel
    Friend WithEvents lblTotalTitle As System.Windows.Forms.Label
    Friend WithEvents lblTotal As System.Windows.Forms.Label
    Friend WithEvents colPartyCode As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colPartyName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colProduct As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colAmt0To30 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colAmt31To60 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colAmtAbove60 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colTotalDue As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colOldestDays As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
