Option Strict On

Partial Class Mainform_form
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    '== Menú ==
    Friend WithEvents menu As MenuStrip
    Friend WithEvents mnuArchivo As ToolStripMenuItem
    Friend WithEvents miAbrir As ToolStripMenuItem
    Friend WithEvents miGuardar As ToolStripMenuItem
    Friend WithEvents miGuardarComo As ToolStripMenuItem
    Friend WithEvents miCerrar As ToolStripMenuItem
    Friend WithEvents miSalir As ToolStripMenuItem

    '== ToolStrip ==
    Friend WithEvents toolStrip As ToolStrip
    Friend WithEvents lblDataRoot As ToolStripLabel
    Friend WithEvents txtDataRoot As ToolStripTextBox
    Friend WithEvents btnBrowseRoot As ToolStripButton
    Friend WithEvents sepA As ToolStripSeparator
    Friend WithEvents lblFilter As ToolStripLabel
    Friend WithEvents txtFilter As ToolStripTextBox
    '== Centro ==
    Friend WithEvents tab As TabControl

    '== Status ==
    Friend WithEvents status As StatusStrip
    Friend WithEvents statusText As ToolStripStatusLabel

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Mainform_form))
        menu = New MenuStrip()
        mnuArchivo = New ToolStripMenuItem()
        miAbrir = New ToolStripMenuItem()
        NewToolStripMenuItem = New ToolStripMenuItem()
        miCrearBSA = New ToolStripMenuItem()
        miCrearBA2 = New ToolStripMenuItem()
        miCrearBA2Tex = New ToolStripMenuItem()
        miGuardar = New ToolStripMenuItem()
        miGuardarComo = New ToolStripMenuItem()
        miCerrar = New ToolStripMenuItem()
        miSalir = New ToolStripMenuItem()
        toolStrip = New ToolStrip()
        lblDataRoot = New ToolStripLabel()
        txtDataRoot = New ToolStripTextBox()
        btnBrowseRoot = New ToolStripButton()
        sepA = New ToolStripSeparator()
        lblFilter = New ToolStripLabel()
        txtFilter = New ToolStripTextBox()
        tab = New TabControl()
        ImageList1 = New ImageList(components)
        status = New StatusStrip()
        statusfile = New ToolStripStatusLabel()
        statusfileb = New ToolStripStatusLabel()
        statustype = New ToolStripStatusLabel()
        statustypeb = New ToolStripStatusLabel()
        statusfiltered = New ToolStripStatusLabel()
        Statusfilteredb = New ToolStripStatusLabel()
        ToolStripProgressBar1 = New ToolStripProgressBar()
        statusText = New ToolStripStatusLabel()
        TableLayoutPanel1 = New TableLayoutPanel()
        Btnrename = New Button()
        btnExtract = New Button()
        btnExtractAll = New Button()
        BtnSave = New Button()
        btnSaveAs = New Button()
        btnclosetab = New Button()
        btnInsert = New Button()
        BtnInsertFolder = New Button()
        btnRemove = New Button()
        SplitContainer1 = New SplitContainer()
        miWriteOptions = New ToolStripMenuItem()
        menu.SuspendLayout()
        toolStrip.SuspendLayout()
        status.SuspendLayout()
        TableLayoutPanel1.SuspendLayout()
        CType(SplitContainer1, ComponentModel.ISupportInitialize).BeginInit()
        SplitContainer1.Panel1.SuspendLayout()
        SplitContainer1.Panel2.SuspendLayout()
        SplitContainer1.SuspendLayout()
        SuspendLayout()
        ' 
        ' menu
        ' 
        menu.BackColor = Color.Transparent
        menu.ImageScalingSize = New Size(24, 24)
        menu.Items.AddRange(New ToolStripItem() {mnuArchivo})
        menu.Location = New Point(6, 6)
        menu.Name = "menu"
        menu.Size = New Size(1607, 33)
        menu.TabIndex = 2
        ' 
        ' mnuArchivo
        ' 
        mnuArchivo.DropDownItems.AddRange(New ToolStripItem() {miAbrir, NewToolStripMenuItem, miGuardar, miGuardarComo, miCerrar, miWriteOptions, miSalir})
        mnuArchivo.Name = "mnuArchivo"
        mnuArchivo.Size = New Size(62, 29)
        mnuArchivo.Text = "&Files"
        ' 
        ' miAbrir
        ' 
        miAbrir.Name = "miAbrir"
        miAbrir.ShortcutKeys = Keys.Control Or Keys.O
        miAbrir.Size = New Size(349, 34)
        miAbrir.Text = "&Open..."
        ' 
        ' NewToolStripMenuItem
        ' 
        NewToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {miCrearBSA, miCrearBA2, miCrearBA2Tex})
        NewToolStripMenuItem.Name = "NewToolStripMenuItem"
        NewToolStripMenuItem.Size = New Size(349, 34)
        NewToolStripMenuItem.Text = "&New"
        ' 
        ' miCrearBSA
        ' 
        miCrearBSA.Name = "miCrearBSA"
        miCrearBSA.Size = New Size(262, 34)
        miCrearBSA.Text = "SSE &BSA"
        ' 
        ' miCrearBA2
        ' 
        miCrearBA2.Name = "miCrearBA2"
        miCrearBA2.Size = New Size(262, 34)
        miCrearBA2.Text = "FO4 BA2 (&General)"
        ' 
        ' miCrearBA2Tex
        ' 
        miCrearBA2Tex.Name = "miCrearBA2Tex"
        miCrearBA2Tex.Size = New Size(262, 34)
        miCrearBA2Tex.Text = "FO4 BA2 (&Textures)"
        ' 
        ' miGuardar
        ' 
        miGuardar.Name = "miGuardar"
        miGuardar.ShortcutKeys = Keys.Control Or Keys.S
        miGuardar.Size = New Size(349, 34)
        miGuardar.Text = "&Save"
        ' 
        ' miGuardarComo
        ' 
        miGuardarComo.Name = "miGuardarComo"
        miGuardarComo.ShortcutKeys = Keys.Control Or Keys.Shift Or Keys.S
        miGuardarComo.Size = New Size(349, 34)
        miGuardarComo.Text = "Save &as..."
        ' 
        ' miCerrar
        ' 
        miCerrar.Name = "miCerrar"
        miCerrar.ShortcutKeys = Keys.Control Or Keys.W
        miCerrar.Size = New Size(349, 34)
        miCerrar.Text = "&Close tab"
        ' 
        ' miSalir
        ' 
        miSalir.Name = "miSalir"
        miSalir.ShortcutKeys = Keys.Alt Or Keys.F4
        miSalir.Size = New Size(349, 34)
        miSalir.Text = "&Exit"
        ' 
        ' toolStrip
        ' 
        toolStrip.BackColor = Color.Transparent
        toolStrip.GripStyle = ToolStripGripStyle.Hidden
        toolStrip.ImageScalingSize = New Size(24, 24)
        toolStrip.Items.AddRange(New ToolStripItem() {lblDataRoot, txtDataRoot, btnBrowseRoot, sepA, lblFilter, txtFilter})
        toolStrip.Location = New Point(6, 39)
        toolStrip.Name = "toolStrip"
        toolStrip.Size = New Size(1607, 34)
        toolStrip.TabIndex = 1
        ' 
        ' lblDataRoot
        ' 
        lblDataRoot.Name = "lblDataRoot"
        lblDataRoot.Size = New Size(92, 29)
        lblDataRoot.Text = "Data root:"
        ' 
        ' txtDataRoot
        ' 
        txtDataRoot.Name = "txtDataRoot"
        txtDataRoot.Size = New Size(400, 34)
        ' 
        ' btnBrowseRoot
        ' 
        btnBrowseRoot.Name = "btnBrowseRoot"
        btnBrowseRoot.Size = New Size(76, 29)
        btnBrowseRoot.Text = "Change"
        ' 
        ' sepA
        ' 
        sepA.Name = "sepA"
        sepA.Size = New Size(6, 34)
        ' 
        ' lblFilter
        ' 
        lblFilter.Name = "lblFilter"
        lblFilter.Size = New Size(54, 29)
        lblFilter.Text = "Filter:"
        ' 
        ' txtFilter
        ' 
        txtFilter.Name = "txtFilter"
        txtFilter.Size = New Size(400, 34)
        ' 
        ' tab
        ' 
        tab.Dock = DockStyle.Fill
        tab.ImageList = ImageList1
        tab.Location = New Point(0, 0)
        tab.Name = "tab"
        tab.SelectedIndex = 0
        tab.Size = New Size(1607, 556)
        tab.TabIndex = 0
        ' 
        ' ImageList1
        ' 
        ImageList1.ColorDepth = ColorDepth.Depth32Bit
        ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), ImageListStreamer)
        ImageList1.TransparentColor = Color.Transparent
        ImageList1.Images.SetKeyName(0, "filesave.ico")
        ImageList1.Images.SetKeyName(1, "filesaveas.ico")
        ImageList1.Images.SetKeyName(2, "filter.ico")
        ImageList1.Images.SetKeyName(3, "mail_delete.ico")
        ImageList1.Images.SetKeyName(4, "reload.ico")
        ImageList1.Images.SetKeyName(5, "filenew.ico")
        ImageList1.Images.SetKeyName(6, "fileopen.ico")
        ImageList1.Images.SetKeyName(7, "folder_new.ico")
        ImageList1.Images.SetKeyName(8, "view_right_p.ico")
        ImageList1.Images.SetKeyName(9, "fileclose.ico")
        ImageList1.Images.SetKeyName(10, "folder_sent_mail.ico")
        ImageList1.Images.SetKeyName(11, "forward.ico")
        ' 
        ' status
        ' 
        status.BackColor = SystemColors.Control
        status.ImageScalingSize = New Size(24, 24)
        status.Items.AddRange(New ToolStripItem() {statusfile, statusfileb, statustype, statustypeb, statusfiltered, Statusfilteredb, ToolStripProgressBar1, statusText})
        status.Location = New Point(6, 756)
        status.Name = "status"
        status.Size = New Size(1607, 32)
        status.TabIndex = 3
        ' 
        ' statusfile
        ' 
        statusfile.Name = "statusfile"
        statusfile.Size = New Size(42, 25)
        statusfile.Text = "File:"
        ' 
        ' statusfileb
        ' 
        statusfileb.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        statusfileb.Name = "statusfileb"
        statusfileb.Size = New Size(72, 25)
        statusfileb.Text = "(None)"
        ' 
        ' statustype
        ' 
        statustype.Name = "statustype"
        statustype.Size = New Size(53, 25)
        statustype.Text = "Type:"
        ' 
        ' statustypeb
        ' 
        statustypeb.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        statustypeb.Name = "statustypeb"
        statustypeb.Size = New Size(93, 25)
        statustypeb.Text = "Unknown"
        ' 
        ' statusfiltered
        ' 
        statusfiltered.Name = "statusfiltered"
        statusfiltered.Size = New Size(87, 25)
        statusfiltered.Text = "Elements:"
        ' 
        ' Statusfilteredb
        ' 
        Statusfilteredb.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        Statusfilteredb.Name = "Statusfilteredb"
        Statusfilteredb.Size = New Size(50, 25)
        Statusfilteredb.Text = "0 / 0"
        ' 
        ' ToolStripProgressBar1
        ' 
        ToolStripProgressBar1.Name = "ToolStripProgressBar1"
        ToolStripProgressBar1.Size = New Size(200, 24)
        ' 
        ' statusText
        ' 
        statusText.BackColor = SystemColors.Control
        statusText.ForeColor = SystemColors.Highlight
        statusText.Name = "statusText"
        statusText.Size = New Size(53, 25)
        statusText.Text = "Listo."
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.ColumnCount = 3
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33333F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.3333359F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.3333359F))
        TableLayoutPanel1.Controls.Add(Btnrename, 2, 0)
        TableLayoutPanel1.Controls.Add(btnExtract, 1, 0)
        TableLayoutPanel1.Controls.Add(btnExtractAll, 0, 0)
        TableLayoutPanel1.Controls.Add(BtnSave, 0, 2)
        TableLayoutPanel1.Controls.Add(btnSaveAs, 1, 2)
        TableLayoutPanel1.Controls.Add(btnclosetab, 2, 2)
        TableLayoutPanel1.Controls.Add(btnInsert, 0, 1)
        TableLayoutPanel1.Controls.Add(BtnInsertFolder, 1, 1)
        TableLayoutPanel1.Controls.Add(btnRemove, 2, 1)
        TableLayoutPanel1.Dock = DockStyle.Fill
        TableLayoutPanel1.Location = New Point(0, 0)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 3
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 33.3333321F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 33.3333321F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 33.3333321F))
        TableLayoutPanel1.Size = New Size(1607, 123)
        TableLayoutPanel1.TabIndex = 4
        ' 
        ' Btnrename
        ' 
        Btnrename.Dock = DockStyle.Fill
        Btnrename.ImageIndex = 4
        Btnrename.ImageList = ImageList1
        Btnrename.Location = New Point(1073, 3)
        Btnrename.Name = "Btnrename"
        Btnrename.Size = New Size(531, 35)
        Btnrename.TabIndex = 8
        Btnrename.Text = "Rename"
        Btnrename.TextAlign = ContentAlignment.MiddleRight
        Btnrename.TextImageRelation = TextImageRelation.ImageBeforeText
        Btnrename.UseVisualStyleBackColor = True
        ' 
        ' btnExtract
        ' 
        btnExtract.Dock = DockStyle.Fill
        btnExtract.ImageKey = "forward.ico"
        btnExtract.ImageList = ImageList1
        btnExtract.Location = New Point(538, 3)
        btnExtract.Name = "btnExtract"
        btnExtract.Size = New Size(529, 35)
        btnExtract.TabIndex = 1
        btnExtract.Text = "Extract selected"
        btnExtract.TextAlign = ContentAlignment.MiddleRight
        btnExtract.TextImageRelation = TextImageRelation.ImageBeforeText
        btnExtract.UseVisualStyleBackColor = True
        ' 
        ' btnExtractAll
        ' 
        btnExtractAll.Dock = DockStyle.Fill
        btnExtractAll.ImageKey = "folder_sent_mail.ico"
        btnExtractAll.ImageList = ImageList1
        btnExtractAll.Location = New Point(3, 3)
        btnExtractAll.Name = "btnExtractAll"
        btnExtractAll.Size = New Size(529, 35)
        btnExtractAll.TabIndex = 0
        btnExtractAll.Text = "Extract all"
        btnExtractAll.TextAlign = ContentAlignment.MiddleRight
        btnExtractAll.TextImageRelation = TextImageRelation.ImageBeforeText
        btnExtractAll.UseVisualStyleBackColor = True
        ' 
        ' BtnSave
        ' 
        BtnSave.Dock = DockStyle.Fill
        BtnSave.ImageKey = "filesave.ico"
        BtnSave.ImageList = ImageList1
        BtnSave.Location = New Point(3, 85)
        BtnSave.Name = "BtnSave"
        BtnSave.Size = New Size(529, 35)
        BtnSave.TabIndex = 2
        BtnSave.Text = "Save"
        BtnSave.TextAlign = ContentAlignment.MiddleRight
        BtnSave.TextImageRelation = TextImageRelation.ImageBeforeText
        BtnSave.UseVisualStyleBackColor = True
        ' 
        ' btnSaveAs
        ' 
        btnSaveAs.Dock = DockStyle.Fill
        btnSaveAs.ImageKey = "filesaveas.ico"
        btnSaveAs.ImageList = ImageList1
        btnSaveAs.Location = New Point(538, 85)
        btnSaveAs.Name = "btnSaveAs"
        btnSaveAs.Size = New Size(529, 35)
        btnSaveAs.TabIndex = 3
        btnSaveAs.Text = "Save as.."
        btnSaveAs.TextAlign = ContentAlignment.MiddleRight
        btnSaveAs.TextImageRelation = TextImageRelation.ImageBeforeText
        btnSaveAs.UseVisualStyleBackColor = True
        ' 
        ' btnclosetab
        ' 
        btnclosetab.Dock = DockStyle.Fill
        btnclosetab.ImageKey = "fileclose.ico"
        btnclosetab.ImageList = ImageList1
        btnclosetab.Location = New Point(1073, 85)
        btnclosetab.Name = "btnclosetab"
        btnclosetab.Size = New Size(531, 35)
        btnclosetab.TabIndex = 4
        btnclosetab.Text = "Close"
        btnclosetab.TextAlign = ContentAlignment.MiddleRight
        btnclosetab.TextImageRelation = TextImageRelation.ImageBeforeText
        btnclosetab.UseVisualStyleBackColor = True
        ' 
        ' btnInsert
        ' 
        btnInsert.Dock = DockStyle.Fill
        btnInsert.ImageKey = "view_right_p.ico"
        btnInsert.ImageList = ImageList1
        btnInsert.Location = New Point(3, 44)
        btnInsert.Name = "btnInsert"
        btnInsert.Size = New Size(529, 35)
        btnInsert.TabIndex = 5
        btnInsert.Text = "Insert file(s)"
        btnInsert.TextAlign = ContentAlignment.MiddleRight
        btnInsert.TextImageRelation = TextImageRelation.ImageBeforeText
        btnInsert.UseVisualStyleBackColor = True
        ' 
        ' BtnInsertFolder
        ' 
        BtnInsertFolder.Dock = DockStyle.Fill
        BtnInsertFolder.ImageKey = "folder_new.ico"
        BtnInsertFolder.ImageList = ImageList1
        BtnInsertFolder.Location = New Point(538, 44)
        BtnInsertFolder.Name = "BtnInsertFolder"
        BtnInsertFolder.Size = New Size(529, 35)
        BtnInsertFolder.TabIndex = 6
        BtnInsertFolder.Text = "Insert folder"
        BtnInsertFolder.TextAlign = ContentAlignment.MiddleRight
        BtnInsertFolder.TextImageRelation = TextImageRelation.ImageBeforeText
        BtnInsertFolder.UseVisualStyleBackColor = True
        ' 
        ' btnRemove
        ' 
        btnRemove.Dock = DockStyle.Fill
        btnRemove.ImageIndex = 3
        btnRemove.ImageList = ImageList1
        btnRemove.Location = New Point(1073, 44)
        btnRemove.Name = "btnRemove"
        btnRemove.Size = New Size(531, 35)
        btnRemove.TabIndex = 7
        btnRemove.Text = "Delete selected"
        btnRemove.TextAlign = ContentAlignment.MiddleRight
        btnRemove.TextImageRelation = TextImageRelation.ImageBeforeText
        btnRemove.UseVisualStyleBackColor = True
        ' 
        ' SplitContainer1
        ' 
        SplitContainer1.Dock = DockStyle.Fill
        SplitContainer1.FixedPanel = FixedPanel.Panel2
        SplitContainer1.Location = New Point(6, 73)
        SplitContainer1.Name = "SplitContainer1"
        SplitContainer1.Orientation = Orientation.Horizontal
        ' 
        ' SplitContainer1.Panel1
        ' 
        SplitContainer1.Panel1.Controls.Add(tab)
        ' 
        ' SplitContainer1.Panel2
        ' 
        SplitContainer1.Panel2.Controls.Add(TableLayoutPanel1)
        SplitContainer1.Size = New Size(1607, 683)
        SplitContainer1.SplitterDistance = 556
        SplitContainer1.TabIndex = 5
        ' 
        ' miWriteOptions
        ' 
        miWriteOptions.Name = "miWriteOptions"
        miWriteOptions.Size = New Size(349, 34)
        miWriteOptions.Text = "&Write Options"
        ' 
        ' Mainform_form
        ' 
        BackColor = Color.FromArgb(CByte(250), CByte(250), CByte(252))
        ClientSize = New Size(1619, 794)
        Controls.Add(SplitContainer1)
        Controls.Add(toolStrip)
        Controls.Add(menu)
        Controls.Add(status)
        Font = New Font("Segoe UI", 9F)
        MainMenuStrip = menu
        MinimumSize = New Size(1250, 850)
        Name = "Mainform_form"
        Padding = New Padding(6)
        StartPosition = FormStartPosition.CenterScreen
        Text = "Bethesda BSA (SSE) / BA2 (FO4) Manager"
        WindowState = FormWindowState.Maximized
        menu.ResumeLayout(False)
        menu.PerformLayout()
        toolStrip.ResumeLayout(False)
        toolStrip.PerformLayout()
        status.ResumeLayout(False)
        status.PerformLayout()
        TableLayoutPanel1.ResumeLayout(False)
        SplitContainer1.Panel1.ResumeLayout(False)
        SplitContainer1.Panel2.ResumeLayout(False)
        CType(SplitContainer1, ComponentModel.ISupportInitialize).EndInit()
        SplitContainer1.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents btnExtractAll As Button
    Friend WithEvents btnExtract As Button
    Friend WithEvents BtnSave As Button
    Friend WithEvents btnSaveAs As Button
    Friend WithEvents btnclosetab As Button
    Friend WithEvents btnInsert As Button
    Friend WithEvents BtnInsertFolder As Button
    Friend WithEvents btnRemove As Button
    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents Btnrename As Button
    Friend WithEvents ToolStripProgressBar1 As ToolStripProgressBar
    Friend WithEvents statusfile As ToolStripStatusLabel
    Friend WithEvents statusfiltered As ToolStripStatusLabel
    Friend WithEvents statustype As ToolStripStatusLabel
    Friend WithEvents NewToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents miCrearBSA As ToolStripMenuItem
    Friend WithEvents miCrearBA2 As ToolStripMenuItem
    Friend WithEvents miCrearBA2Tex As ToolStripMenuItem
    Friend WithEvents statusfileb As ToolStripStatusLabel
    Friend WithEvents statustypeb As ToolStripStatusLabel
    Friend WithEvents Statusfilteredb As ToolStripStatusLabel
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents miWriteOptions As ToolStripMenuItem

End Class





