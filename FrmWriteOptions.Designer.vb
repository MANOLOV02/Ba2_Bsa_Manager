' FrmWriteOptions.Designer.vb
Option Strict On
Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmWriteOptions
    Inherits Form

    Private components As System.ComponentModel.IContainer

    Friend WithEvents Tab As TabControl
    Friend WithEvents TabDx As TabPage
    Friend WithEvents TabGn As TabPage
    Friend WithEvents TabBsa As TabPage
    Friend WithEvents cboDxVer As ComboBox
    Friend WithEvents cboDxComp As ComboBox
    Friend WithEvents chkDxStr As CheckBox
    Friend WithEvents cboDxZlib As ComboBox
    Friend WithEvents cboGnVer As ComboBox
    Friend WithEvents cboGnComp As ComboBox
    Friend WithEvents chkGnStr As CheckBox
    Friend WithEvents cboGnZlib As ComboBox
    Friend WithEvents chkBsaDir As CheckBox
    Friend WithEvents chkBsaFile As CheckBox
    Friend WithEvents chkBsaEmbed As CheckBox
    Friend WithEvents chkBsaGlobal As CheckBox
    Friend WithEvents btnOK As Button
    Friend WithEvents btnCancel As Button
    Friend WithEvents lblDxVer As Label
    Friend WithEvents lblDxComp As Label
    Friend WithEvents lblDxZlib As Label
    Friend WithEvents lblGnVer As Label
    Friend WithEvents lblGnComp As Label
    Friend WithEvents lblGnZlib As Label

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Tab = New TabControl()
        TabDx = New TabPage()
        lblDxVer = New Label()
        cboDxVer = New ComboBox()
        lblDxComp = New Label()
        cboDxComp = New ComboBox()
        lblDxZlib = New Label()
        cboDxZlib = New ComboBox()
        chkDxStr = New CheckBox()
        TabGn = New TabPage()
        lblGnVer = New Label()
        cboGnVer = New ComboBox()
        lblGnComp = New Label()
        cboGnComp = New ComboBox()
        lblGnZlib = New Label()
        cboGnZlib = New ComboBox()
        chkGnStr = New CheckBox()
        TabBsa = New TabPage()
        chkBsaDir = New CheckBox()
        chkBsaFile = New CheckBox()
        chkBsaEmbed = New CheckBox()
        chkBsaGlobal = New CheckBox()
        btnOK = New Button()
        btnCancel = New Button()
        Tab.SuspendLayout()
        TabDx.SuspendLayout()
        TabGn.SuspendLayout()
        TabBsa.SuspendLayout()
        SuspendLayout()
        ' 
        ' Tab
        ' 
        Tab.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Tab.Controls.Add(TabDx)
        Tab.Controls.Add(TabGn)
        Tab.Controls.Add(TabBsa)
        Tab.Location = New Point(8, 8)
        Tab.Name = "Tab"
        Tab.SelectedIndex = 0
        Tab.Size = New Size(544, 304)
        Tab.TabIndex = 0
        ' 
        ' TabDx
        ' 
        TabDx.Controls.Add(lblDxVer)
        TabDx.Controls.Add(cboDxVer)
        TabDx.Controls.Add(lblDxComp)
        TabDx.Controls.Add(cboDxComp)
        TabDx.Controls.Add(lblDxZlib)
        TabDx.Controls.Add(cboDxZlib)
        TabDx.Controls.Add(chkDxStr)
        TabDx.Location = New Point(4, 34)
        TabDx.Name = "TabDx"
        TabDx.Padding = New Padding(8)
        TabDx.Size = New Size(536, 266)
        TabDx.TabIndex = 0
        TabDx.Text = "BA2 — DX10"
        ' 
        ' lblDxVer
        ' 
        lblDxVer.AutoSize = True
        lblDxVer.Location = New Point(12, 16)
        lblDxVer.Name = "lblDxVer"
        lblDxVer.Size = New Size(74, 25)
        lblDxVer.TabIndex = 0
        lblDxVer.Text = "Version:"
        ' 
        ' cboDxVer
        ' 
        cboDxVer.DropDownStyle = ComboBoxStyle.DropDownList
        cboDxVer.Items.AddRange(New Object() {"1", "2", "3", "7", "8"})
        cboDxVer.Location = New Point(120, 12)
        cboDxVer.Name = "cboDxVer"
        cboDxVer.Size = New Size(120, 33)
        cboDxVer.TabIndex = 1
        ' 
        ' lblDxComp
        ' 
        lblDxComp.AutoSize = True
        lblDxComp.Location = New Point(270, 16)
        lblDxComp.Name = "lblDxComp"
        lblDxComp.Size = New Size(121, 25)
        lblDxComp.TabIndex = 2
        lblDxComp.Text = "Compression:"
        ' 
        ' cboDxComp
        ' 
        cboDxComp.DropDownStyle = ComboBoxStyle.DropDownList
        cboDxComp.Items.AddRange(New Object() {"ZIP (zlib)", "LZ4 (raw)"})
        cboDxComp.Location = New Point(405, 12)
        cboDxComp.Name = "cboDxComp"
        cboDxComp.Size = New Size(120, 33)
        cboDxComp.TabIndex = 3
        ' 
        ' lblDxZlib
        ' 
        lblDxZlib.AutoSize = True
        lblDxZlib.Location = New Point(12, 54)
        lblDxZlib.Name = "lblDxZlib"
        lblDxZlib.Size = New Size(99, 25)
        lblDxZlib.TabIndex = 4
        lblDxZlib.Text = "Zlib preset:"
        ' 
        ' cboDxZlib
        ' 
        cboDxZlib.DropDownStyle = ComboBoxStyle.DropDownList
        cboDxZlib.Items.AddRange(New Object() {"Default", "Fastest", "Maximum"})
        cboDxZlib.Location = New Point(120, 50)
        cboDxZlib.Name = "cboDxZlib"
        cboDxZlib.Size = New Size(120, 33)
        cboDxZlib.TabIndex = 5
        ' 
        ' chkDxStr
        ' 
        chkDxStr.AutoSize = True
        chkDxStr.Location = New Point(12, 92)
        chkDxStr.Name = "chkDxStr"
        chkDxStr.Size = New Size(197, 29)
        chkDxStr.TabIndex = 6
        chkDxStr.Text = "Include strings table"
        ' 
        ' TabGn
        ' 
        TabGn.Controls.Add(lblGnVer)
        TabGn.Controls.Add(cboGnVer)
        TabGn.Controls.Add(lblGnComp)
        TabGn.Controls.Add(cboGnComp)
        TabGn.Controls.Add(lblGnZlib)
        TabGn.Controls.Add(cboGnZlib)
        TabGn.Controls.Add(chkGnStr)
        TabGn.Location = New Point(4, 34)
        TabGn.Name = "TabGn"
        TabGn.Padding = New Padding(8)
        TabGn.Size = New Size(536, 266)
        TabGn.TabIndex = 1
        TabGn.Text = "BA2 — GNRL"
        ' 
        ' lblGnVer
        ' 
        lblGnVer.AutoSize = True
        lblGnVer.Location = New Point(12, 16)
        lblGnVer.Name = "lblGnVer"
        lblGnVer.Size = New Size(74, 25)
        lblGnVer.TabIndex = 0
        lblGnVer.Text = "Version:"
        ' 
        ' cboGnVer
        ' 
        cboGnVer.DropDownStyle = ComboBoxStyle.DropDownList
        cboGnVer.Items.AddRange(New Object() {"1", "2", "3", "7", "8"})
        cboGnVer.Location = New Point(120, 12)
        cboGnVer.Name = "cboGnVer"
        cboGnVer.Size = New Size(120, 33)
        cboGnVer.TabIndex = 1
        ' 
        ' lblGnComp
        ' 
        lblGnComp.AutoSize = True
        lblGnComp.Location = New Point(270, 16)
        lblGnComp.Name = "lblGnComp"
        lblGnComp.Size = New Size(121, 25)
        lblGnComp.TabIndex = 2
        lblGnComp.Text = "Compression:"
        ' 
        ' cboGnComp
        ' 
        cboGnComp.DropDownStyle = ComboBoxStyle.DropDownList
        cboGnComp.Items.AddRange(New Object() {"ZIP (zlib)", "LZ4 (raw)"})
        cboGnComp.Location = New Point(405, 12)
        cboGnComp.Name = "cboGnComp"
        cboGnComp.Size = New Size(120, 33)
        cboGnComp.TabIndex = 3
        ' 
        ' lblGnZlib
        ' 
        lblGnZlib.AutoSize = True
        lblGnZlib.Location = New Point(12, 54)
        lblGnZlib.Name = "lblGnZlib"
        lblGnZlib.Size = New Size(99, 25)
        lblGnZlib.TabIndex = 4
        lblGnZlib.Text = "Zlib preset:"
        ' 
        ' cboGnZlib
        ' 
        cboGnZlib.DropDownStyle = ComboBoxStyle.DropDownList
        cboGnZlib.Items.AddRange(New Object() {"Default", "Fastest", "Maximum"})
        cboGnZlib.Location = New Point(120, 50)
        cboGnZlib.Name = "cboGnZlib"
        cboGnZlib.Size = New Size(120, 33)
        cboGnZlib.TabIndex = 5
        ' 
        ' chkGnStr
        ' 
        chkGnStr.AutoSize = True
        chkGnStr.Location = New Point(12, 92)
        chkGnStr.Name = "chkGnStr"
        chkGnStr.Size = New Size(197, 29)
        chkGnStr.TabIndex = 6
        chkGnStr.Text = "Include strings table"
        ' 
        ' TabBsa
        ' 
        TabBsa.Controls.Add(chkBsaDir)
        TabBsa.Controls.Add(chkBsaFile)
        TabBsa.Controls.Add(chkBsaEmbed)
        TabBsa.Controls.Add(chkBsaGlobal)
        TabBsa.Location = New Point(4, 34)
        TabBsa.Name = "TabBsa"
        TabBsa.Padding = New Padding(8)
        TabBsa.Size = New Size(536, 266)
        TabBsa.TabIndex = 2
        TabBsa.Text = "BSA — SSE"
        ' 
        ' chkBsaDir
        ' 
        chkBsaDir.AutoSize = True
        chkBsaDir.Location = New Point(12, 16)
        chkBsaDir.Name = "chkBsaDir"
        chkBsaDir.Size = New Size(227, 29)
        chkBsaDir.TabIndex = 0
        chkBsaDir.Text = "Include directory names"
        ' 
        ' chkBsaFile
        ' 
        chkBsaFile.AutoSize = True
        chkBsaFile.Location = New Point(12, 44)
        chkBsaFile.Name = "chkBsaFile"
        chkBsaFile.Size = New Size(180, 29)
        chkBsaFile.TabIndex = 1
        chkBsaFile.Text = "Include file names"
        ' 
        ' chkBsaEmbed
        ' 
        chkBsaEmbed.AutoSize = True
        chkBsaEmbed.Location = New Point(12, 72)
        chkBsaEmbed.Name = "chkBsaEmbed"
        chkBsaEmbed.Size = New Size(295, 29)
        chkBsaEmbed.TabIndex = 2
        chkBsaEmbed.Text = "Embed names in data (BSTRING)"
        ' 
        ' chkBsaGlobal
        ' 
        chkBsaGlobal.AutoSize = True
        chkBsaGlobal.Location = New Point(12, 100)
        chkBsaGlobal.Name = "chkBsaGlobal"
        chkBsaGlobal.Size = New Size(240, 29)
        chkBsaGlobal.TabIndex = 3
        chkBsaGlobal.Text = "Global compression (LZ4)"
        ' 
        ' btnOK
        ' 
        btnOK.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnOK.DialogResult = DialogResult.OK
        btnOK.Location = New Point(368, 320)
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(84, 35)
        btnOK.TabIndex = 1
        btnOK.Text = "OK"
        ' 
        ' btnCancel
        ' 
        btnCancel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Location = New Point(468, 320)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(84, 35)
        btnCancel.TabIndex = 2
        btnCancel.Text = "Cancel"
        ' 
        ' FrmWriteOptions
        ' 
        ClientSize = New Size(560, 360)
        Controls.Add(Tab)
        Controls.Add(btnOK)
        Controls.Add(btnCancel)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "FrmWriteOptions"
        StartPosition = FormStartPosition.CenterParent
        Text = "Write Options"
        Tab.ResumeLayout(False)
        TabDx.ResumeLayout(False)
        TabDx.PerformLayout()
        TabGn.ResumeLayout(False)
        TabGn.PerformLayout()
        TabBsa.ResumeLayout(False)
        TabBsa.PerformLayout()
        ResumeLayout(False)
    End Sub
End Class

