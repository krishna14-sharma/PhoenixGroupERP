<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmlogin
    Inherits ThemedForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.pnlBrand = New Phoenix_GroupERP.BrandPanel()
        Me.pnlCard = New System.Windows.Forms.Panel()
        Me.lblClose = New System.Windows.Forms.Label()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.lblSubtitle = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtuname = New System.Windows.Forms.TextBox()
        Me.lblCompany = New System.Windows.Forms.Label()
        Me.cmbcmpname = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtpwd = New System.Windows.Forms.TextBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.cmbSession = New System.Windows.Forms.ComboBox()
        Me.btnlogin = New System.Windows.Forms.Button()
        Me.btnexit = New System.Windows.Forms.Button()
        Me.lblFooter = New System.Windows.Forms.Label()
        Me.cmbcmpid = New System.Windows.Forms.ComboBox()
        Me.dtCurrDate = New System.Windows.Forms.DateTimePicker()
        Me.pnlCard.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlBrand
        '
        Me.pnlBrand.Dock = System.Windows.Forms.DockStyle.Left
        Me.pnlBrand.Location = New System.Drawing.Point(0, 0)
        Me.pnlBrand.Logo = Global.Phoenix_GroupERP.My.Resources.Resources.phoenix_logo
        Me.pnlBrand.Name = "pnlBrand"
        Me.pnlBrand.Size = New System.Drawing.Size(340, 540)
        Me.pnlBrand.TabIndex = 20
        Me.pnlBrand.Title = "Phoenix Group ERP"
        '
        'pnlCard
        '
        Me.pnlCard.BackColor = System.Drawing.Color.White
        Me.pnlCard.Controls.Add(Me.lblClose)
        Me.pnlCard.Controls.Add(Me.lblTitle)
        Me.pnlCard.Controls.Add(Me.lblSubtitle)
        Me.pnlCard.Controls.Add(Me.Label2)
        Me.pnlCard.Controls.Add(Me.txtuname)
        Me.pnlCard.Controls.Add(Me.lblCompany)
        Me.pnlCard.Controls.Add(Me.cmbcmpname)
        Me.pnlCard.Controls.Add(Me.Label3)
        Me.pnlCard.Controls.Add(Me.txtpwd)
        Me.pnlCard.Controls.Add(Me.Label8)
        Me.pnlCard.Controls.Add(Me.cmbSession)
        Me.pnlCard.Controls.Add(Me.btnlogin)
        Me.pnlCard.Controls.Add(Me.btnexit)
        Me.pnlCard.Controls.Add(Me.lblFooter)
        Me.pnlCard.Controls.Add(Me.cmbcmpid)
        Me.pnlCard.Controls.Add(Me.dtCurrDate)
        Me.pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlCard.Location = New System.Drawing.Point(340, 0)
        Me.pnlCard.Name = "pnlCard"
        Me.pnlCard.Size = New System.Drawing.Size(540, 540)
        Me.pnlCard.TabIndex = 0
        '
        'lblClose
        '
        Me.lblClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblClose.Cursor = System.Windows.Forms.Cursors.Hand
        Me.lblClose.Font = New System.Drawing.Font("Segoe UI", 15.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblClose.ForeColor = System.Drawing.Color.Gray
        Me.lblClose.Location = New System.Drawing.Point(498, 6)
        Me.lblClose.Name = "lblClose"
        Me.lblClose.Size = New System.Drawing.Size(36, 36)
        Me.lblClose.TabIndex = 30
        Me.lblClose.Text = ChrW(215)
        Me.lblClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI Semibold", 20.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitle.Location = New System.Drawing.Point(66, 50)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(98, 37)
        Me.lblTitle.TabIndex = 21
        Me.lblTitle.Text = "Sign in"
        '
        'lblSubtitle
        '
        Me.lblSubtitle.AutoSize = True
        Me.lblSubtitle.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSubtitle.ForeColor = System.Drawing.Color.DimGray
        Me.lblSubtitle.Location = New System.Drawing.Point(69, 92)
        Me.lblSubtitle.Name = "lblSubtitle"
        Me.lblSubtitle.Size = New System.Drawing.Size(305, 17)
        Me.lblSubtitle.TabIndex = 22
        Me.lblSubtitle.Text = "Enter your credentials to open the company books."
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Segoe UI Semibold", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(69, 136)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(66, 15)
        Me.Label2.TabIndex = 23
        Me.Label2.Text = "User name"
        '
        'txtuname
        '
        Me.txtuname.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal
        Me.txtuname.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtuname.Location = New System.Drawing.Point(70, 156)
        Me.txtuname.MaxLength = 15
        Me.txtuname.Name = "txtuname"
        Me.txtuname.Size = New System.Drawing.Size(400, 27)
        Me.txtuname.TabIndex = 0
        '
        'lblCompany
        '
        Me.lblCompany.AutoSize = True
        Me.lblCompany.Font = New System.Drawing.Font("Segoe UI Semibold", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCompany.Location = New System.Drawing.Point(69, 198)
        Me.lblCompany.Name = "lblCompany"
        Me.lblCompany.Size = New System.Drawing.Size(58, 15)
        Me.lblCompany.TabIndex = 24
        Me.lblCompany.Text = "Company"
        '
        'cmbcmpname
        '
        Me.cmbcmpname.DropDownHeight = 320
        Me.cmbcmpname.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbcmpname.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbcmpname.FormattingEnabled = True
        Me.cmbcmpname.IntegralHeight = False
        Me.cmbcmpname.Location = New System.Drawing.Point(70, 218)
        Me.cmbcmpname.MaxDropDownItems = 16
        Me.cmbcmpname.Name = "cmbcmpname"
        Me.cmbcmpname.Size = New System.Drawing.Size(400, 28)
        Me.cmbcmpname.TabIndex = 1
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Segoe UI Semibold", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(69, 262)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(57, 15)
        Me.Label3.TabIndex = 25
        Me.Label3.Text = "Password"
        '
        'txtpwd
        '
        Me.txtpwd.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtpwd.Location = New System.Drawing.Point(70, 282)
        Me.txtpwd.Name = "txtpwd"
        Me.txtpwd.Size = New System.Drawing.Size(400, 27)
        Me.txtpwd.TabIndex = 2
        Me.txtpwd.UseSystemPasswordChar = True
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Segoe UI Semibold", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(69, 324)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(141, 15)
        Me.Label8.TabIndex = 26
        Me.Label8.Text = "Financial year (session)"
        '
        'cmbSession
        '
        Me.cmbSession.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbSession.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbSession.FormattingEnabled = True
        Me.cmbSession.Location = New System.Drawing.Point(70, 344)
        Me.cmbSession.Name = "cmbSession"
        Me.cmbSession.Size = New System.Drawing.Size(400, 28)
        Me.cmbSession.TabIndex = 3
        '
        'btnlogin
        '
        Me.btnlogin.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnlogin.Font = New System.Drawing.Font("Segoe UI Semibold", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnlogin.Location = New System.Drawing.Point(70, 398)
        Me.btnlogin.Name = "btnlogin"
        Me.btnlogin.Size = New System.Drawing.Size(400, 42)
        Me.btnlogin.TabIndex = 4
        Me.btnlogin.Text = "Sign &in"
        Me.btnlogin.UseVisualStyleBackColor = False
        '
        'btnexit
        '
        Me.btnexit.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnexit.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnexit.Location = New System.Drawing.Point(70, 448)
        Me.btnexit.Name = "btnexit"
        Me.btnexit.Size = New System.Drawing.Size(400, 36)
        Me.btnexit.TabIndex = 5
        Me.btnexit.Text = "E&xit"
        Me.btnexit.UseVisualStyleBackColor = False
        '
        'lblFooter
        '
        Me.lblFooter.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblFooter.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFooter.ForeColor = System.Drawing.Color.Gray
        Me.lblFooter.Location = New System.Drawing.Point(69, 504)
        Me.lblFooter.Name = "lblFooter"
        Me.lblFooter.Size = New System.Drawing.Size(402, 18)
        Me.lblFooter.TabIndex = 27
        Me.lblFooter.Text = "Connecting to server" & ChrW(8230)
        Me.lblFooter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'cmbcmpid
        '
        Me.cmbcmpid.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbcmpid.FormattingEnabled = True
        Me.cmbcmpid.Location = New System.Drawing.Point(-400, -400)
        Me.cmbcmpid.Name = "cmbcmpid"
        Me.cmbcmpid.Size = New System.Drawing.Size(120, 21)
        Me.cmbcmpid.TabIndex = 40
        Me.cmbcmpid.TabStop = False
        '
        'dtCurrDate
        '
        Me.dtCurrDate.CustomFormat = "dd/MMM/yy"
        Me.dtCurrDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dtCurrDate.Location = New System.Drawing.Point(-400, -400)
        Me.dtCurrDate.Name = "dtCurrDate"
        Me.dtCurrDate.Size = New System.Drawing.Size(90, 20)
        Me.dtCurrDate.TabIndex = 41
        Me.dtCurrDate.TabStop = False
        '
        'frmlogin
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(880, 540)
        Me.Controls.Add(Me.pnlCard)
        Me.Controls.Add(Me.pnlBrand)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = False
        Me.Name = "frmlogin"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Phoenix Group ERP - Sign in"
        Me.pnlCard.ResumeLayout(False)
        Me.pnlCard.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents pnlBrand As Phoenix_GroupERP.BrandPanel
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents lblClose As System.Windows.Forms.Label
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents lblSubtitle As System.Windows.Forms.Label
    Friend WithEvents lblCompany As System.Windows.Forms.Label
    Friend WithEvents lblFooter As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtpwd As System.Windows.Forms.TextBox
    Friend WithEvents btnlogin As System.Windows.Forms.Button
    Friend WithEvents btnexit As System.Windows.Forms.Button
    Friend WithEvents cmbcmpname As System.Windows.Forms.ComboBox
    Friend WithEvents txtuname As System.Windows.Forms.TextBox
    Friend WithEvents cmbcmpid As System.Windows.Forms.ComboBox
    Friend WithEvents dtCurrDate As System.Windows.Forms.DateTimePicker
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents cmbSession As System.Windows.Forms.ComboBox

End Class
