<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmBOIRtgs
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
        Me.pnlTop = New System.Windows.Forms.Panel()
        Me.lblBankCaption = New System.Windows.Forms.Label()
        Me.cmbBank = New System.Windows.Forms.ComboBox()
        Me.lblSearch = New System.Windows.Forms.Label()
        Me.txtSearch = New System.Windows.Forms.TextBox()
        Me.btnRefresh = New System.Windows.Forms.Button()
        Me.pnlLeft = New System.Windows.Forms.Panel()
        Me.lblVouchers = New System.Windows.Forms.Label()
        Me.dgVouchers = New System.Windows.Forms.DataGridView()
        Me.btnShow = New System.Windows.Forms.Button()
        Me.grpSlip = New System.Windows.Forms.GroupBox()
        Me.pnlBottom = New System.Windows.Forms.Panel()
        Me.lblOffX = New System.Windows.Forms.Label()
        Me.nudOffsetX = New System.Windows.Forms.NumericUpDown()
        Me.lblOffY = New System.Windows.Forms.Label()
        Me.nudOffsetY = New System.Windows.Forms.NumericUpDown()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.btnPreview = New System.Windows.Forms.Button()
        Me.btnPrint = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.picPreview = New System.Windows.Forms.PictureBox()
        Me.lblRemitter = New System.Windows.Forms.Label()
        Me.txtRemitter = New System.Windows.Forms.TextBox()
        Me.lblRemitterAc = New System.Windows.Forms.Label()
        Me.txtRemitterAc = New System.Windows.Forms.TextBox()
        Me.lblOurBranch = New System.Windows.Forms.Label()
        Me.txtOurBranch = New System.Windows.Forms.TextBox()
        Me.lblFrom = New System.Windows.Forms.Label()
        Me.txtFrom = New System.Windows.Forms.TextBox()
        Me.lblRemAddress = New System.Windows.Forms.Label()
        Me.txtRemAddress = New System.Windows.Forms.TextBox()
        Me.lblMobile = New System.Windows.Forms.Label()
        Me.txtMobile = New System.Windows.Forms.TextBox()
        Me.lblSignatory = New System.Windows.Forms.Label()
        Me.txtSignatory = New System.Windows.Forms.TextBox()
        Me.lblDate = New System.Windows.Forms.Label()
        Me.dtpDate = New System.Windows.Forms.DateTimePicker()
        Me.lblParty = New System.Windows.Forms.Label()
        Me.cmbParty = New System.Windows.Forms.ComboBox()
        Me.lblPartyBank = New System.Windows.Forms.Label()
        Me.cmbPartyBank = New System.Windows.Forms.ComboBox()
        Me.lblName = New System.Windows.Forms.Label()
        Me.txtName = New System.Windows.Forms.TextBox()
        Me.lblBenAc = New System.Windows.Forms.Label()
        Me.txtBenAc = New System.Windows.Forms.TextBox()
        Me.lblIfsc = New System.Windows.Forms.Label()
        Me.txtIfsc = New System.Windows.Forms.TextBox()
        Me.lblBenBank = New System.Windows.Forms.Label()
        Me.txtBenBank = New System.Windows.Forms.TextBox()
        Me.lblBenBranch = New System.Windows.Forms.Label()
        Me.txtBenBranch = New System.Windows.Forms.TextBox()
        Me.lblChqNo = New System.Windows.Forms.Label()
        Me.txtChqNo = New System.Windows.Forms.TextBox()
        Me.lblAmount = New System.Windows.Forms.Label()
        Me.txtAmount = New System.Windows.Forms.TextBox()
        Me.lblWords = New System.Windows.Forms.Label()
        Me.txtWords = New System.Windows.Forms.TextBox()
        Me.pnlTop.SuspendLayout()
        Me.pnlLeft.SuspendLayout()
        Me.grpSlip.SuspendLayout()
        Me.pnlBottom.SuspendLayout()
        CType(Me.dgVouchers, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudOffsetX, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nudOffsetY, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.picPreview, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pnlTop
        '
        Me.pnlTop.Controls.AddRange(New System.Windows.Forms.Control() {Me.lblBankCaption, Me.cmbBank, Me.lblSearch, Me.txtSearch, Me.btnRefresh})
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Location = New System.Drawing.Point(0, 0)
        Me.pnlTop.Size = New System.Drawing.Size(1280, 46)
        Me.pnlTop.Name = "pnlTop"
        Me.pnlTop.TabIndex = 0
        Me.pnlTop.Padding = New System.Windows.Forms.Padding(8)
        '
        'lblBankCaption
        '
        Me.lblBankCaption.AutoSize = True
        Me.lblBankCaption.Location = New System.Drawing.Point(12, 15)
        Me.lblBankCaption.Name = "lblBankCaption"
        Me.lblBankCaption.Text = "Bank Account (BOI):"
        '
        'cmbBank
        '
        Me.cmbBank.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbBank.FormattingEnabled = True
        Me.cmbBank.Location = New System.Drawing.Point(140, 11)
        Me.cmbBank.Size = New System.Drawing.Size(320, 24)
        Me.cmbBank.Name = "cmbBank"
        Me.cmbBank.TabIndex = 1
        '
        'lblSearch
        '
        Me.lblSearch.AutoSize = True
        Me.lblSearch.Location = New System.Drawing.Point(480, 15)
        Me.lblSearch.Name = "lblSearch"
        Me.lblSearch.Text = "Search:"
        '
        'txtSearch
        '
        Me.txtSearch.Location = New System.Drawing.Point(532, 11)
        Me.txtSearch.Size = New System.Drawing.Size(220, 24)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.TabIndex = 2
        '
        'btnRefresh
        '
        Me.btnRefresh.Location = New System.Drawing.Point(762, 9)
        Me.btnRefresh.Size = New System.Drawing.Size(90, 28)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.TabIndex = 3
        Me.btnRefresh.Text = "&Refresh"
        Me.btnRefresh.UseVisualStyleBackColor = True
        '
        'pnlLeft
        '
        Me.pnlLeft.Controls.AddRange(New System.Windows.Forms.Control() {Me.lblVouchers, Me.dgVouchers, Me.btnShow, Me.grpSlip})
        Me.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left
        Me.pnlLeft.Location = New System.Drawing.Point(0, 46)
        Me.pnlLeft.Size = New System.Drawing.Size(500, 660)
        Me.pnlLeft.Name = "pnlLeft"
        Me.pnlLeft.TabIndex = 1
        Me.pnlLeft.AutoScroll = True
        '
        'lblVouchers
        '
        Me.lblVouchers.AutoSize = True
        Me.lblVouchers.Location = New System.Drawing.Point(10, 6)
        Me.lblVouchers.Name = "lblVouchers"
        Me.lblVouchers.Text = "Bank vouchers (double-click or Enter to load):"
        '
        'dgVouchers
        '
        Me.dgVouchers.AllowUserToAddRows = False
        Me.dgVouchers.AllowUserToDeleteRows = False
        Me.dgVouchers.ReadOnly = True
        Me.dgVouchers.RowHeadersVisible = False
        Me.dgVouchers.MultiSelect = False
        Me.dgVouchers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgVouchers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgVouchers.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgVouchers.Location = New System.Drawing.Point(10, 26)
        Me.dgVouchers.Size = New System.Drawing.Size(460, 170)
        Me.dgVouchers.Name = "dgVouchers"
        Me.dgVouchers.TabIndex = 0
        '
        'btnShow
        '
        Me.btnShow.Location = New System.Drawing.Point(370, 200)
        Me.btnShow.Size = New System.Drawing.Size(100, 28)
        Me.btnShow.Name = "btnShow"
        Me.btnShow.TabIndex = 1
        Me.btnShow.Text = "&Show"
        Me.btnShow.UseVisualStyleBackColor = True
        '
        'grpSlip
        '
        Me.grpSlip.Controls.AddRange(New System.Windows.Forms.Control() {Me.lblRemitter, Me.txtRemitter, Me.lblRemitterAc, Me.txtRemitterAc, Me.lblOurBranch, Me.txtOurBranch, Me.lblFrom, Me.txtFrom, Me.lblRemAddress, Me.txtRemAddress, Me.lblMobile, Me.txtMobile, Me.lblSignatory, Me.txtSignatory, Me.lblDate, Me.dtpDate, Me.lblParty, Me.cmbParty, Me.lblPartyBank, Me.cmbPartyBank, Me.lblName, Me.txtName, Me.lblBenAc, Me.txtBenAc, Me.lblIfsc, Me.txtIfsc, Me.lblBenBank, Me.txtBenBank, Me.lblBenBranch, Me.txtBenBranch, Me.lblChqNo, Me.txtChqNo, Me.lblAmount, Me.txtAmount, Me.lblWords, Me.txtWords})
        Me.grpSlip.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grpSlip.Location = New System.Drawing.Point(10, 232)
        Me.grpSlip.Size = New System.Drawing.Size(460, 559)
        Me.grpSlip.Name = "grpSlip"
        Me.grpSlip.TabIndex = 2
        Me.grpSlip.Text = "Slip details (you can edit before printing)"
        '
        'lblRemitter
        '
        Me.lblRemitter.AutoSize = False
        Me.lblRemitter.Location = New System.Drawing.Point(10, 24)
        Me.lblRemitter.Size = New System.Drawing.Size(150, 20)
        Me.lblRemitter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblRemitter.Name = "lblRemitter"
        Me.lblRemitter.Text = "Applicant (our A/c title):"
        '
        'txtRemitter
        '
        Me.txtRemitter.Location = New System.Drawing.Point(165, 22)
        Me.txtRemitter.Size = New System.Drawing.Size(282, 24)
        Me.txtRemitter.Name = "txtRemitter"
        Me.txtRemitter.TabIndex = 0
        '
        'lblRemitterAc
        '
        Me.lblRemitterAc.AutoSize = False
        Me.lblRemitterAc.Location = New System.Drawing.Point(10, 51)
        Me.lblRemitterAc.Size = New System.Drawing.Size(150, 20)
        Me.lblRemitterAc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblRemitterAc.Name = "lblRemitterAc"
        Me.lblRemitterAc.Text = "Our BOI A/c No.:"
        '
        'txtRemitterAc
        '
        Me.txtRemitterAc.Location = New System.Drawing.Point(165, 49)
        Me.txtRemitterAc.Size = New System.Drawing.Size(282, 24)
        Me.txtRemitterAc.Name = "txtRemitterAc"
        Me.txtRemitterAc.TabIndex = 1
        '
        'lblOurBranch
        '
        Me.lblOurBranch.AutoSize = False
        Me.lblOurBranch.Location = New System.Drawing.Point(10, 78)
        Me.lblOurBranch.Size = New System.Drawing.Size(150, 20)
        Me.lblOurBranch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblOurBranch.Name = "lblOurBranch"
        Me.lblOurBranch.Text = "Our BOI Branch:"
        '
        'txtOurBranch
        '
        Me.txtOurBranch.Location = New System.Drawing.Point(165, 76)
        Me.txtOurBranch.Size = New System.Drawing.Size(282, 24)
        Me.txtOurBranch.Name = "txtOurBranch"
        Me.txtOurBranch.TabIndex = 2
        '
        'lblFrom
        '
        Me.lblFrom.AutoSize = False
        Me.lblFrom.Location = New System.Drawing.Point(10, 105)
        Me.lblFrom.Size = New System.Drawing.Size(150, 20)
        Me.lblFrom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblFrom.Name = "lblFrom"
        Me.lblFrom.Text = "From (counterfoil):"
        '
        'txtFrom
        '
        Me.txtFrom.Location = New System.Drawing.Point(165, 103)
        Me.txtFrom.Size = New System.Drawing.Size(282, 24)
        Me.txtFrom.Name = "txtFrom"
        Me.txtFrom.TabIndex = 3
        '
        'lblRemAddress
        '
        Me.lblRemAddress.AutoSize = False
        Me.lblRemAddress.Location = New System.Drawing.Point(10, 132)
        Me.lblRemAddress.Size = New System.Drawing.Size(150, 20)
        Me.lblRemAddress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblRemAddress.Name = "lblRemAddress"
        Me.lblRemAddress.Text = "Our Address:"
        '
        'txtRemAddress
        '
        Me.txtRemAddress.Location = New System.Drawing.Point(165, 130)
        Me.txtRemAddress.Size = New System.Drawing.Size(282, 44)
        Me.txtRemAddress.Name = "txtRemAddress"
        Me.txtRemAddress.TabIndex = 4
        Me.txtRemAddress.Multiline = True
        '
        'lblMobile
        '
        Me.lblMobile.AutoSize = False
        Me.lblMobile.Location = New System.Drawing.Point(10, 179)
        Me.lblMobile.Size = New System.Drawing.Size(150, 20)
        Me.lblMobile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblMobile.Name = "lblMobile"
        Me.lblMobile.Text = "Tel/Mob. No.:"
        '
        'txtMobile
        '
        Me.txtMobile.Location = New System.Drawing.Point(165, 177)
        Me.txtMobile.Size = New System.Drawing.Size(282, 24)
        Me.txtMobile.Name = "txtMobile"
        Me.txtMobile.TabIndex = 5
        Me.txtMobile.MaxLength = 15
        '
        'lblSignatory
        '
        Me.lblSignatory.AutoSize = False
        Me.lblSignatory.Location = New System.Drawing.Point(10, 206)
        Me.lblSignatory.Size = New System.Drawing.Size(150, 20)
        Me.lblSignatory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblSignatory.Name = "lblSignatory"
        Me.lblSignatory.Text = "Signatory:"
        '
        'txtSignatory
        '
        Me.txtSignatory.Location = New System.Drawing.Point(165, 204)
        Me.txtSignatory.Size = New System.Drawing.Size(282, 24)
        Me.txtSignatory.Name = "txtSignatory"
        Me.txtSignatory.TabIndex = 6
        '
        'lblDate
        '
        Me.lblDate.AutoSize = False
        Me.lblDate.Location = New System.Drawing.Point(10, 233)
        Me.lblDate.Size = New System.Drawing.Size(150, 20)
        Me.lblDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblDate.Name = "lblDate"
        Me.lblDate.Text = "Date:"
        '
        'dtpDate
        '
        Me.dtpDate.Location = New System.Drawing.Point(165, 231)
        Me.dtpDate.Size = New System.Drawing.Size(282, 24)
        Me.dtpDate.Name = "dtpDate"
        Me.dtpDate.TabIndex = 7
        Me.dtpDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dtpDate.CustomFormat = "dd/MM/yyyy"
        '
        'lblParty
        '
        Me.lblParty.AutoSize = False
        Me.lblParty.Location = New System.Drawing.Point(10, 260)
        Me.lblParty.Size = New System.Drawing.Size(150, 20)
        Me.lblParty.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblParty.Name = "lblParty"
        Me.lblParty.Text = "Beneficiary Party:"
        '
        'cmbParty
        '
        Me.cmbParty.Location = New System.Drawing.Point(165, 258)
        Me.cmbParty.Size = New System.Drawing.Size(282, 24)
        Me.cmbParty.Name = "cmbParty"
        Me.cmbParty.TabIndex = 8
        Me.cmbParty.FormattingEnabled = True
        '
        'lblPartyBank
        '
        Me.lblPartyBank.AutoSize = False
        Me.lblPartyBank.Location = New System.Drawing.Point(10, 287)
        Me.lblPartyBank.Size = New System.Drawing.Size(150, 20)
        Me.lblPartyBank.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblPartyBank.Name = "lblPartyBank"
        Me.lblPartyBank.Text = "Party Bank A/c:"
        '
        'cmbPartyBank
        '
        Me.cmbPartyBank.Location = New System.Drawing.Point(165, 285)
        Me.cmbPartyBank.Size = New System.Drawing.Size(282, 24)
        Me.cmbPartyBank.Name = "cmbPartyBank"
        Me.cmbPartyBank.TabIndex = 9
        Me.cmbPartyBank.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPartyBank.FormattingEnabled = True
        '
        'lblName
        '
        Me.lblName.AutoSize = False
        Me.lblName.Location = New System.Drawing.Point(10, 314)
        Me.lblName.Size = New System.Drawing.Size(150, 20)
        Me.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblName.Name = "lblName"
        Me.lblName.Text = "Beneficiary Name:"
        '
        'txtName
        '
        Me.txtName.Location = New System.Drawing.Point(165, 312)
        Me.txtName.Size = New System.Drawing.Size(282, 24)
        Me.txtName.Name = "txtName"
        Me.txtName.TabIndex = 10
        '
        'lblBenAc
        '
        Me.lblBenAc.AutoSize = False
        Me.lblBenAc.Location = New System.Drawing.Point(10, 341)
        Me.lblBenAc.Size = New System.Drawing.Size(150, 20)
        Me.lblBenAc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblBenAc.Name = "lblBenAc"
        Me.lblBenAc.Text = "Beneficiary A/c No.:"
        '
        'txtBenAc
        '
        Me.txtBenAc.Location = New System.Drawing.Point(165, 339)
        Me.txtBenAc.Size = New System.Drawing.Size(282, 24)
        Me.txtBenAc.Name = "txtBenAc"
        Me.txtBenAc.TabIndex = 11
        '
        'lblIfsc
        '
        Me.lblIfsc.AutoSize = False
        Me.lblIfsc.Location = New System.Drawing.Point(10, 368)
        Me.lblIfsc.Size = New System.Drawing.Size(150, 20)
        Me.lblIfsc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblIfsc.Name = "lblIfsc"
        Me.lblIfsc.Text = "IFSC Code:"
        '
        'txtIfsc
        '
        Me.txtIfsc.Location = New System.Drawing.Point(165, 366)
        Me.txtIfsc.Size = New System.Drawing.Size(282, 24)
        Me.txtIfsc.Name = "txtIfsc"
        Me.txtIfsc.TabIndex = 12
        Me.txtIfsc.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtIfsc.MaxLength = 11
        '
        'lblBenBank
        '
        Me.lblBenBank.AutoSize = False
        Me.lblBenBank.Location = New System.Drawing.Point(10, 395)
        Me.lblBenBank.Size = New System.Drawing.Size(150, 20)
        Me.lblBenBank.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblBenBank.Name = "lblBenBank"
        Me.lblBenBank.Text = "Bank:"
        '
        'txtBenBank
        '
        Me.txtBenBank.Location = New System.Drawing.Point(165, 393)
        Me.txtBenBank.Size = New System.Drawing.Size(282, 24)
        Me.txtBenBank.Name = "txtBenBank"
        Me.txtBenBank.TabIndex = 13
        '
        'lblBenBranch
        '
        Me.lblBenBranch.AutoSize = False
        Me.lblBenBranch.Location = New System.Drawing.Point(10, 422)
        Me.lblBenBranch.Size = New System.Drawing.Size(150, 20)
        Me.lblBenBranch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblBenBranch.Name = "lblBenBranch"
        Me.lblBenBranch.Text = "Branch:"
        '
        'txtBenBranch
        '
        Me.txtBenBranch.Location = New System.Drawing.Point(165, 420)
        Me.txtBenBranch.Size = New System.Drawing.Size(282, 24)
        Me.txtBenBranch.Name = "txtBenBranch"
        Me.txtBenBranch.TabIndex = 14
        '
        'lblChqNo
        '
        Me.lblChqNo.AutoSize = False
        Me.lblChqNo.Location = New System.Drawing.Point(10, 449)
        Me.lblChqNo.Size = New System.Drawing.Size(150, 20)
        Me.lblChqNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblChqNo.Name = "lblChqNo"
        Me.lblChqNo.Text = "Cheque No.:"
        '
        'txtChqNo
        '
        Me.txtChqNo.Location = New System.Drawing.Point(165, 447)
        Me.txtChqNo.Size = New System.Drawing.Size(282, 24)
        Me.txtChqNo.Name = "txtChqNo"
        Me.txtChqNo.TabIndex = 15
        '
        'lblAmount
        '
        Me.lblAmount.AutoSize = False
        Me.lblAmount.Location = New System.Drawing.Point(10, 476)
        Me.lblAmount.Size = New System.Drawing.Size(150, 20)
        Me.lblAmount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblAmount.Name = "lblAmount"
        Me.lblAmount.Text = "Amount Rs.:"
        '
        'txtAmount
        '
        Me.txtAmount.Location = New System.Drawing.Point(165, 474)
        Me.txtAmount.Size = New System.Drawing.Size(282, 24)
        Me.txtAmount.Name = "txtAmount"
        Me.txtAmount.TabIndex = 16
        Me.txtAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'lblWords
        '
        Me.lblWords.AutoSize = False
        Me.lblWords.Location = New System.Drawing.Point(10, 503)
        Me.lblWords.Size = New System.Drawing.Size(150, 20)
        Me.lblWords.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblWords.Name = "lblWords"
        Me.lblWords.Text = "Rupees (in words):"
        '
        'txtWords
        '
        Me.txtWords.Location = New System.Drawing.Point(165, 501)
        Me.txtWords.Size = New System.Drawing.Size(282, 44)
        Me.txtWords.Name = "txtWords"
        Me.txtWords.TabIndex = 17
        Me.txtWords.Multiline = True
        '
        'pnlBottom
        '
        Me.pnlBottom.Controls.AddRange(New System.Windows.Forms.Control() {Me.lblOffX, Me.nudOffsetX, Me.lblOffY, Me.nudOffsetY, Me.lblStatus, Me.btnPreview, Me.btnPrint, Me.btnClose})
        Me.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlBottom.Location = New System.Drawing.Point(0, 706)
        Me.pnlBottom.Size = New System.Drawing.Size(1280, 50)
        Me.pnlBottom.Name = "pnlBottom"
        Me.pnlBottom.TabIndex = 2
        '
        'lblOffX
        '
        Me.lblOffX.AutoSize = True
        Me.lblOffX.Location = New System.Drawing.Point(10, 17)
        Me.lblOffX.Name = "lblOffX"
        Me.lblOffX.Text = "Printer offset X (mm):"
        '
        'nudOffsetX
        '
        Me.nudOffsetX.DecimalPlaces = 1
        Me.nudOffsetX.Increment = New Decimal(New Integer() {5, 0, 0, 65536})
        Me.nudOffsetX.Maximum = New Decimal(New Integer() {60, 0, 0, 0})
        Me.nudOffsetX.Minimum = New Decimal(New Integer() {60, 0, 0, -2147483648})
        Me.nudOffsetX.Location = New System.Drawing.Point(132, 14)
        Me.nudOffsetX.Size = New System.Drawing.Size(64, 24)
        Me.nudOffsetX.Name = "nudOffsetX"
        Me.nudOffsetX.TabIndex = 0
        '
        'nudOffsetY
        '
        Me.nudOffsetY.DecimalPlaces = 1
        Me.nudOffsetY.Increment = New Decimal(New Integer() {5, 0, 0, 65536})
        Me.nudOffsetY.Maximum = New Decimal(New Integer() {60, 0, 0, 0})
        Me.nudOffsetY.Minimum = New Decimal(New Integer() {60, 0, 0, -2147483648})
        Me.nudOffsetY.Location = New System.Drawing.Point(290, 14)
        Me.nudOffsetY.Size = New System.Drawing.Size(64, 24)
        Me.nudOffsetY.Name = "nudOffsetY"
        Me.nudOffsetY.TabIndex = 1
        '
        'lblOffY
        '
        Me.lblOffY.AutoSize = True
        Me.lblOffY.Location = New System.Drawing.Point(240, 17)
        Me.lblOffY.Name = "lblOffY"
        Me.lblOffY.Text = "Y (mm):"
        '
        'lblStatus
        '
        Me.lblStatus.AutoEllipsis = True
        Me.lblStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatus.Location = New System.Drawing.Point(380, 17)
        Me.lblStatus.Size = New System.Drawing.Size(550, 20)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Text = "Pick a voucher and press Show."
        '
        'btnPreview
        '
        Me.btnPreview.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPreview.Location = New System.Drawing.Point(940, 10)
        Me.btnPreview.Size = New System.Drawing.Size(104, 30)
        Me.btnPreview.Name = "btnPreview"
        Me.btnPreview.TabIndex = 3
        Me.btnPreview.Text = "Preview"
        Me.btnPreview.UseVisualStyleBackColor = True
        '
        'btnPrint
        '
        Me.btnPrint.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPrint.Location = New System.Drawing.Point(1050, 10)
        Me.btnPrint.Size = New System.Drawing.Size(104, 30)
        Me.btnPrint.Name = "btnPrint"
        Me.btnPrint.TabIndex = 4
        Me.btnPrint.Text = "&Print"
        Me.btnPrint.UseVisualStyleBackColor = True
        '
        'btnClose
        '
        Me.btnClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnClose.Location = New System.Drawing.Point(1160, 10)
        Me.btnClose.Size = New System.Drawing.Size(104, 30)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.TabIndex = 5
        Me.btnClose.Text = "&Close"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'picPreview
        '
        Me.picPreview.BackColor = System.Drawing.Color.FromArgb(CType(CType(226, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(236, Byte), Integer))
        Me.picPreview.Dock = System.Windows.Forms.DockStyle.Fill
        Me.picPreview.Location = New System.Drawing.Point(500, 46)
        Me.picPreview.Size = New System.Drawing.Size(780, 660)
        Me.picPreview.Name = "picPreview"
        Me.picPreview.TabStop = False
        '
        'frmBOIRtgs
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1280, 756)
        Me.Controls.Add(Me.picPreview)
        Me.Controls.Add(Me.pnlLeft)
        Me.Controls.Add(Me.pnlBottom)
        Me.Controls.Add(Me.pnlTop)
        Me.MinimumSize = New System.Drawing.Size(1000, 700)
        Me.Name = "frmBOIRtgs"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Bank of India RTGS / Star Insta Remit"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.CancelButton = Me.btnClose
        CType(Me.dgVouchers, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudOffsetX, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nudOffsetY, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.picPreview, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        Me.pnlLeft.ResumeLayout(False)
        Me.pnlLeft.PerformLayout()
        Me.grpSlip.ResumeLayout(False)
        Me.grpSlip.PerformLayout()
        Me.pnlBottom.ResumeLayout(False)
        Me.pnlBottom.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents pnlTop As System.Windows.Forms.Panel
    Friend WithEvents lblBankCaption As System.Windows.Forms.Label
    Friend WithEvents cmbBank As System.Windows.Forms.ComboBox
    Friend WithEvents lblSearch As System.Windows.Forms.Label
    Friend WithEvents txtSearch As System.Windows.Forms.TextBox
    Friend WithEvents btnRefresh As System.Windows.Forms.Button
    Friend WithEvents pnlLeft As System.Windows.Forms.Panel
    Friend WithEvents lblVouchers As System.Windows.Forms.Label
    Friend WithEvents dgVouchers As System.Windows.Forms.DataGridView
    Friend WithEvents btnShow As System.Windows.Forms.Button
    Friend WithEvents grpSlip As System.Windows.Forms.GroupBox
    Friend WithEvents pnlBottom As System.Windows.Forms.Panel
    Friend WithEvents lblOffX As System.Windows.Forms.Label
    Friend WithEvents nudOffsetX As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblOffY As System.Windows.Forms.Label
    Friend WithEvents nudOffsetY As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents btnPreview As System.Windows.Forms.Button
    Friend WithEvents btnPrint As System.Windows.Forms.Button
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents picPreview As System.Windows.Forms.PictureBox
    Friend WithEvents lblRemitter As System.Windows.Forms.Label
    Friend WithEvents txtRemitter As System.Windows.Forms.TextBox
    Friend WithEvents lblRemitterAc As System.Windows.Forms.Label
    Friend WithEvents txtRemitterAc As System.Windows.Forms.TextBox
    Friend WithEvents lblOurBranch As System.Windows.Forms.Label
    Friend WithEvents txtOurBranch As System.Windows.Forms.TextBox
    Friend WithEvents lblFrom As System.Windows.Forms.Label
    Friend WithEvents txtFrom As System.Windows.Forms.TextBox
    Friend WithEvents lblRemAddress As System.Windows.Forms.Label
    Friend WithEvents txtRemAddress As System.Windows.Forms.TextBox
    Friend WithEvents lblMobile As System.Windows.Forms.Label
    Friend WithEvents txtMobile As System.Windows.Forms.TextBox
    Friend WithEvents lblSignatory As System.Windows.Forms.Label
    Friend WithEvents txtSignatory As System.Windows.Forms.TextBox
    Friend WithEvents lblDate As System.Windows.Forms.Label
    Friend WithEvents dtpDate As System.Windows.Forms.DateTimePicker
    Friend WithEvents lblParty As System.Windows.Forms.Label
    Friend WithEvents cmbParty As System.Windows.Forms.ComboBox
    Friend WithEvents lblPartyBank As System.Windows.Forms.Label
    Friend WithEvents cmbPartyBank As System.Windows.Forms.ComboBox
    Friend WithEvents lblName As System.Windows.Forms.Label
    Friend WithEvents txtName As System.Windows.Forms.TextBox
    Friend WithEvents lblBenAc As System.Windows.Forms.Label
    Friend WithEvents txtBenAc As System.Windows.Forms.TextBox
    Friend WithEvents lblIfsc As System.Windows.Forms.Label
    Friend WithEvents txtIfsc As System.Windows.Forms.TextBox
    Friend WithEvents lblBenBank As System.Windows.Forms.Label
    Friend WithEvents txtBenBank As System.Windows.Forms.TextBox
    Friend WithEvents lblBenBranch As System.Windows.Forms.Label
    Friend WithEvents txtBenBranch As System.Windows.Forms.TextBox
    Friend WithEvents lblChqNo As System.Windows.Forms.Label
    Friend WithEvents txtChqNo As System.Windows.Forms.TextBox
    Friend WithEvents lblAmount As System.Windows.Forms.Label
    Friend WithEvents txtAmount As System.Windows.Forms.TextBox
    Friend WithEvents lblWords As System.Windows.Forms.Label
    Friend WithEvents txtWords As System.Windows.Forms.TextBox
End Class
