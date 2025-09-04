Option Strict On
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports BSA_BA2_Library_DLL.BethesdaArchive.Core


Partial Class Mainform_form
    Private ReadOnly _configPath As String = IO.Path.Combine(AppContext.BaseDirectory, "config.json")
    Private _config As AppConfig = AppConfig.Load(_configPath)
    ' ========= tipos =========

    Private Enum GameKindUI
        FO4_BA2 = 0
        SSE_BSA = 1
    End Enum

    Private Enum Fo4Ba2Type
        Unknown = 0
        DX10 = 1
        GNRL = 2
    End Enum

    ' Contexto por pestaña
    Private NotInheritable Class TabContext
        Public Property GameKind As GameKindUI
        Public Property Fo4Type As Fo4Ba2Type = Fo4Ba2Type.Unknown
        Public Property SourcePath As String
        Public Property GameRoot As String
        Public Property Entries As BindingList(Of EntryView) = New BindingList(Of EntryView)()
        Public Property Filtered As SortableBindingList = New SortableBindingList
        Public Property LastFilter As String = ""
        Public Property LastDirFilter As String = Nothing

        Public Property Dirty As Boolean
        ' === NUEVO: referencias UI de esta pestaña ===
        Public Property Grid As DataGridView
        Public Property Tree As TreeView
        Public Property SelectedDir As String = Nothing
        ' SelectedDir semantics:
        '   Nothing  => "All" (mostrar todo)
        '   ""       => "(root)" (solo archivos en raíz, sin subdirectorio)
        '   "a\b\c"  => ese directorio y sus subdirectorios
    End Class

    ' Fila mostrada en la grilla
    Private NotInheritable Class EntryView
        Public Property Index As Integer
        Public Property Directory As String
        Public Property FileName As String
        Public ReadOnly Property FullPath As String
            Get
                If String.IsNullOrEmpty(Directory) Then Return FileName
                Return Directory.TrimEnd(InCorrect_Path_separator, Correct_Path_separator) & Correct_Path_separator & FileName
            End Get
        End Property
        Public Property Data As Byte()
        ' DX10 metadata
        Public Property DxgiFormat As Integer
        Public Property Width As Integer
        Public Property Height As Integer
        Public Property MipCount As Integer
        Public Property Faces As Integer
        Public Property IsCubemap As Boolean
        ' BSA
        Public Property PreferCompress As Boolean
    End Class

    ' ========= ctor =========
    ' Ordenamiento para la grilla sin “desaparecer” filas.
    ' Requiere: Imports System.ComponentModel
    Private NotInheritable Class SortableBindingList
        Inherits BindingList(Of EntryView)

        Private _isSorted As Boolean
        Private _sortProperty As PropertyDescriptor
        Private _sortDirection As ListSortDirection = ListSortDirection.Ascending

        ' ==== soporte IBindingList ====
        Protected Overrides ReadOnly Property SupportsSortingCore As Boolean
            Get
                Return True
            End Get
        End Property

        Protected Overrides ReadOnly Property IsSortedCore As Boolean
            Get
                Return _isSorted
            End Get
        End Property

        Protected Overrides ReadOnly Property SortPropertyCore As PropertyDescriptor
            Get
                Return _sortProperty
            End Get
        End Property

        Protected Overrides ReadOnly Property SortDirectionCore As ListSortDirection
            Get
                Return _sortDirection
            End Get
        End Property

        Protected Overrides Sub RemoveSortCore()
            _isSorted = False
            _sortProperty = Nothing
            _sortDirection = ListSortDirection.Ascending
            OnListChanged(New ListChangedEventArgs(ListChangedType.Reset, -1))
        End Sub

        Protected Overrides Sub ApplySortCore(prop As PropertyDescriptor, direction As ListSortDirection)
            If prop Is Nothing Then Throw New ArgumentNullException(NameOf(prop))

            Dim list = TryCast(Me.Items, List(Of EntryView))
            If list Is Nothing OrElse list.Count <= 1 Then
                _isSorted = False
                _sortProperty = prop
                _sortDirection = direction
                OnListChanged(New ListChangedEventArgs(ListChangedType.Reset, -1))
                Return
            End If

            ' Comparador por PropertyDescriptor (null-safe y con IComparable)
            Dim cmp As Comparison(Of EntryView) =
            Function(a As EntryView, b As EntryView)
                Dim va = prop.GetValue(a)
                Dim vb = prop.GetValue(b)
                If va Is vb Then Return 0
                If va Is Nothing Then Return -1
                If vb Is Nothing Then Return 1
                Dim ca = TryCast(va, IComparable)
                If ca IsNot Nothing Then
                    Return ca.CompareTo(vb)
                End If
                ' Fallback: comparar como texto
                Return String.Compare(va.ToString(), vb?.ToString(), StringComparison.CurrentCulture)
            End Function

            If direction = ListSortDirection.Ascending Then
                list.Sort(cmp)
            Else
                list.Sort(Function(a, b) -cmp(a, b))
            End If

            _isSorted = True
            _sortProperty = prop
            _sortDirection = direction

            ' Un único reset (no borrar/insertar ítems)
            OnListChanged(New ListChangedEventArgs(ListChangedType.Reset, -1))
        End Sub
    End Class
    Public Sub New()
        InitializeComponent()
        Me.AutoScaleMode = AutoScaleMode.Dpi
        Me.AutoScaleDimensions = New SizeF(96.0F, 96.0F)

        Me.DoubleBuffered = True
        statusText.Text = "Open a file or create new one to start."

        ' menú
        AddHandler miAbrir.Click, AddressOf miAbrir_Click
        AddHandler miGuardar.Click, AddressOf miGuardar_Click
        AddHandler miGuardarComo.Click, AddressOf miGuardarComo_Click
        AddHandler BtnSave.Click, AddressOf miGuardar_Click
        AddHandler btnSaveAs.Click, AddressOf miGuardarComo_Click
        AddHandler miCerrar.Click, AddressOf miCerrar_Click
        AddHandler btnclosetab.Click, AddressOf miCerrar_Click
        AddHandler miSalir.Click, Sub(sender As Object, e As EventArgs) Me.Close()

        ' nuevos del menú Archivo
        AddHandler miCrearBSA.Click, AddressOf miCrearBSA_Click
        AddHandler miCrearBA2.Click, AddressOf miCrearBA2_Click
        AddHandler miCrearBA2Tex.Click, AddressOf miCrearBA2Tex_Click

        ' toolstrip
        AddHandler btnBrowseRoot.Click, AddressOf btnBrowseRoot_Click
        AddHandler btnInsert.Click, AddressOf btnInsert_Click
        AddHandler btnRemove.Click, AddressOf btnRemove_Click
        AddHandler Btnrename.Click, AddressOf btnRename_Click
        AddHandler btnExtract.Click, AddressOf btnExtract_Click
        AddHandler btnExtractAll.Click, AddressOf btnExtractAll_Click
        AddHandler txtFilter.TextChanged, AddressOf txtFilter_TextChanged
        AddHandler BtnInsertFolder.Click, AddressOf btnInsertFolder_Click
        AddHandler tab.SelectedIndexChanged, AddressOf tab_changed
        AddHandler miWriteOptions.Click, AddressOf miWriteOptions_Click
        AddHandler Me.FormClosing, AddressOf Mainform_form_FormClosing

        UpdateButtonsForGame()
        AdjustToolStripForDpi()
        AdjustTabsForDpi()
    End Sub
    Private Sub miWriteOptions_Click(sender As Object, e As EventArgs)
        Dim ctx = CurrentContext()
        Dim showDx As Boolean = (ctx Is Nothing) OrElse (ctx.GameKind = GameKindUI.FO4_BA2 AndAlso (ctx.Fo4Type = Fo4Ba2Type.GNRL Or ctx.Fo4Type = Fo4Ba2Type.Unknown))
        Dim showGn As Boolean = (ctx Is Nothing) OrElse (ctx.GameKind = GameKindUI.FO4_BA2 AndAlso (ctx.Fo4Type = Fo4Ba2Type.DX10 Or ctx.Fo4Type = Fo4Ba2Type.Unknown))
        Dim showBsa As Boolean = (ctx Is Nothing) OrElse (ctx.GameKind = GameKindUI.SSE_BSA)

        FrmWriteOptions.EditOptions(Me, _config, showDx, showGn, showBsa)
    End Sub

    Private Sub Mainform_form_FormClosing(sender As Object, e As FormClosingEventArgs)
        Try
            _config.Save(_configPath)
        Catch
            ' swallow – no bloquear cierre por error de disco
        End Try
    End Sub

    Private Function MakeDxOptionsFromConfig() As Ba2WriterDX10.Options
        Dim g = _config.Ba2Dx
        Dim o As New Ba2WriterDX10.Options With {
        .Encoding = Encoding.UTF8,
        .Version = CUInt(Math.Max(1, g.Version)),
        .IncludeStrings = g.IncludeStrings,
        .ZlibPreset = ParseZlibPreset(g.ZlibPreset),
        .CompressionFormat = If(g.Version = 3 AndAlso String.Equals(g.Compression, "lz4", StringComparison.OrdinalIgnoreCase),
                                Ba2WriterCommon.CompressionFormat.Lz4,
                                Ba2WriterCommon.CompressionFormat.Zip)
    }
        Return o
    End Function

    Private Function MakeGnrlOptionsFromConfig() As Ba2WriterGNRL.Options
        Dim g = _config.Ba2Gnrl
        Dim o As New Ba2WriterGNRL.Options With {
        .Encoding = Encoding.UTF8,
        .Version = CUInt(Math.Max(1, g.Version)),
        .IncludeStrings = g.IncludeStrings,
        .ZlibPreset = ParseZlibPreset(g.ZlibPreset),
        .CompressionFormat = If(g.Version = 3 AndAlso String.Equals(g.Compression, "lz4", StringComparison.OrdinalIgnoreCase),
                                Ba2WriterCommon.CompressionFormat.Lz4,
                                Ba2WriterCommon.CompressionFormat.Zip)
    }
        Return o
    End Function

    Private Function MakeBsaOptionsFromConfig() As BsaWriter.Options
        Dim g = _config.Bsa
        Return New BsaWriter.Options With {
        .Encoding = Encoding.UTF8,
        .UseDirectoryStrings = g.UseDirectoryStrings,
        .UseFileStrings = g.UseFileStrings,
        .EmbedNames = g.EmbedNames,
        .GlobalCompressed = g.GlobalCompressed
    }
    End Function

    Private Shared Function ParseZlibPreset(name As String) As Ba2WriterCommon.ZlibPreset
        Select Case (If(name, "Default")).Trim()
            Case "Fastest" : Return Ba2WriterCommon.ZlibPreset.Fast
            Case "Maximum" : Return Ba2WriterCommon.ZlibPreset.Max
            Case Else : Return Ba2WriterCommon.ZlibPreset.Default
        End Select
    End Function

    ' ========= UI pintura (tabs con X) =========
    Private Sub AdjustToolStripForDpi()
        Dim scale As Single = Me.DeviceDpi / 96.0F

        toolStrip.ImageScalingSize = New Size(CInt(20 * scale), CInt(20 * scale))
        toolStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow
        toolStrip.Stretch = True
        toolStrip.AutoSize = True

        For Each it As ToolStripItem In toolStrip.Items
            it.Margin = New Padding(CInt(2 * scale), CInt(2 * scale), CInt(2 * scale), CInt(2 * scale))
            it.Padding = New Padding(CInt(2 * scale), CInt(2 * scale), CInt(2 * scale), CInt(2 * scale))
            If TypeOf it Is ToolStripSeparator Then Continue For

            it.AutoSize = False
            Dim h As Integer = CInt(26 * scale)

            If TypeOf it Is ToolStripButton Then
                it.DisplayStyle = ToolStripItemDisplayStyle.Text
                it.Size = New Size(Math.Max(it.Width, 100), h)
            ElseIf TypeOf it Is ToolStripLabel Then
                it.Size = New Size(Math.Max(it.Width, 60), h)
            ElseIf TypeOf it Is ToolStripTextBox Then
                Dim tb = DirectCast(it, ToolStripTextBox)
                tb.AutoSize = False
                tb.Size = New Size(Math.Max(tb.Width, 240), h)
                tb.Control.Margin = Padding.Empty
                tb.Control.Padding = Padding.Empty
            ElseIf TypeOf it Is ToolStripComboBox Then
                Dim cb = DirectCast(it, ToolStripComboBox)
                cb.AutoSize = False
                cb.Size = New Size(Math.Max(cb.Width, 160), h)
                cb.DropDownStyle = ComboBoxStyle.DropDownList
                cb.ComboBox.IntegralHeight = False
                cb.ComboBox.ItemHeight = Math.Max(CInt(16 * scale), cb.ComboBox.ItemHeight)
            End If
        Next

        toolStrip.PerformLayout()
    End Sub

    Private Sub AdjustTabsForDpi()
        Dim scale As Single = Me.DeviceDpi / 96.0F
        tab.SizeMode = TabSizeMode.Normal
        tab.DrawMode = TabDrawMode.Normal
        tab.ItemSize = New Size(CInt(140 * scale), CInt(28 * scale))
        tab.Padding = New Drawing.Point(CInt(18 * scale), CInt(4 * scale))
        tab.PerformLayout()
        tab.Invalidate()
    End Sub

    Private Sub Mainform_form_DpiChanged(sender As Object, e As DpiChangedEventArgs) Handles Me.DpiChanged
        AdjustToolStripForDpi()
        AdjustTabsForDpi()
    End Sub

    Private Sub tab_changed(sender As Object, e As EventArgs)
        Dim ctx = CurrentContext()
        If Not IsNothing(ctx) Then
            If ctx.LastFilter <> txtFilter.Text Then
                ApplyFilter(ctx, txtFilter.Text)
            End If
        End If
        UpdateButtonsForGame()
    End Sub

    ' ========= helpers UI =========

    Private Sub cmbGame_SelectedIndexChanged(sender As Object, e As EventArgs)
        UpdateButtonsForGame()
    End Sub

    Private Sub btnBrowseRoot_Click(sender As Object, e As EventArgs)
        Dim ctx = CurrentContext()
        If IsNothing(ctx) Then Exit Sub
        Using d As New FolderBrowserDialog()
            d.Description = "Select 'Data' directory"
            d.UseDescriptionForTitle = True
            If d.ShowDialog(Me) = DialogResult.OK Then
                ctx.GameRoot = d.SelectedPath
                txtDataRoot.Text = d.SelectedPath
            End If
        End Using
    End Sub

    Private Sub txtFilter_TextChanged(sender As Object, e As EventArgs)
        Dim ctx = CurrentContext() : If ctx Is Nothing Then Return
        ApplyFilter(ctx, txtFilter.Text)
    End Sub

    Private Sub UpdateButtonsForGame()
        Dim ctx = CurrentContext()
        If ctx Is Nothing Then
            btnInsert.Enabled = False
            btnRemove.Enabled = False
            Btnrename.Enabled = False
            btnExtract.Enabled = False
            btnExtractAll.Enabled = False
            BtnSave.Enabled = False
            btnSaveAs.Enabled = False
            miGuardar.Enabled = False
            miGuardarComo.Enabled = False
            btnclosetab.Enabled = False
            miCerrar.Enabled = False
            BtnInsertFolder.Enabled = False
            txtDataRoot.Text = ""
            SetStatus(Nothing, Nothing)
            Return
        End If
        SetStatus(Nothing, ctx)
        btnclosetab.Enabled = True
        miCerrar.Enabled = True
        ' Insertar: sólo si hay archivo/pestaña abierta
        btnInsert.Enabled = True
        BtnInsertFolder.Enabled = True
        txtDataRoot.Text = ctx.GameRoot
        Dim grid = CurrentGrid()
        Dim haveSel As Boolean = (grid IsNot Nothing AndAlso grid.SelectedRows.Count > 0)
        btnRemove.Enabled = haveSel
        Btnrename.Enabled = (grid IsNot Nothing AndAlso grid.SelectedRows.Count = 1)
        BtnSave.Enabled = ctx.Dirty And ctx.SourcePath <> "" AndAlso ctx.Entries.Any
        miGuardar.Enabled = ctx.Dirty And ctx.SourcePath <> "" AndAlso ctx.Entries.Any
        miGuardarComo.Enabled = ctx.Entries.Any
        btnSaveAs.Enabled = ctx.Entries.Any
        btnExtract.Enabled = haveSel
        btnExtractAll.Enabled = ctx.Entries IsNot Nothing AndAlso ctx.Entries.Count > 0
    End Sub

    Private Function CurrentContext() As TabContext
        Dim tp = tab.SelectedTab
        If tp Is Nothing Then Return Nothing
        Return TryCast(tp.Tag, TabContext)
    End Function

    Private Function CurrentGrid() As DataGridView
        Dim ctx = CurrentContext()
        If ctx Is Nothing Then Return Nothing
        Return ctx.Grid
    End Function
    Private Sub SetStatus(msg As String, ctx As TabContext)
        status.SuspendLayout()
        If Not IsNothing(msg) Then statusText.Text = msg
        If IsNothing(ctx) Then
            statusfileb.Text = "(None)"
            Dim tipo As String = "Unknown"
            statustypeb.Text = tipo
            Statusfilteredb.Text = "0 / 0"
        Else
            statusfileb.Text = IO.Path.GetFileName(ctx.SourcePath)
            Dim tipo As String = "Unknown"
            Select Case ctx.GameKind
                Case GameKindUI.FO4_BA2
                    Select Case ctx.Fo4Type
                        Case Fo4Ba2Type.DX10
                            tipo = "FO4 BA2 (Textures)"
                        Case Fo4Ba2Type.GNRL
                            tipo = "FO4 BA2 (General)"
                        Case Fo4Ba2Type.Unknown
                            tipo = "FO4 BA2 (Unknown)"
                    End Select
                Case GameKindUI.SSE_BSA
                    tipo = "SSE BSA"
            End Select
            statustypeb.Text = tipo
            Statusfilteredb.Text = String.Format("{0:N0}", ctx.Filtered.Count) + " / " + String.Format("{0:N0}", ctx.Entries.Count)
        End If
        status.ResumeLayout()
    End Sub

    ' ========= Archivo: abrir / guardar / guardar como / cerrar =========
    Private Sub Progreso(Valor As Double, Maximo As Double)
        ToolStripProgressBar1.Value = CInt(Math.Clamp(Valor / Maximo * 100, 0, 100))
    End Sub
    Private Sub miAbrir_Click(sender As Object, e As EventArgs)
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Fallout 4 / Skyrim Archives (*.ba2;*.bsa)|*.ba2;*.bsa|all (*.*)|*.*"
            ofd.Multiselect = True
            If ofd.ShowDialog(Me) <> DialogResult.OK Then Return
            For Each p In ofd.FileNames
                OpenArchiveInNewTab(p)
            Next
        End Using
    End Sub

    Private Sub miGuardar_Click(sender As Object, e As EventArgs)
        Dim ctx = CurrentContext()
        If ctx Is Nothing Then Return
        If String.IsNullOrWhiteSpace(ctx.SourcePath) Then
            miGuardarComo_Click(sender, e) : Return
        End If
        SaveContextToPath(ctx, ctx.SourcePath)
    End Sub

    Private Sub miGuardarComo_Click(sender As Object, e As EventArgs)
        Dim ctx = CurrentContext() : If ctx Is Nothing Then Return
        Using sfd As New SaveFileDialog()
            If ctx.GameKind = GameKindUI.FO4_BA2 Then
                sfd.Filter = "Fallout 4 BA2 (*.ba2)|*.ba2|all (*.*)|*.*"
            Else
                sfd.Filter = "Skyrim SE BSA (*.bsa)|*.bsa|all (*.*)|*.*"
            End If
            If sfd.ShowDialog(Me) <> DialogResult.OK Then Return
            ctx.SourcePath = sfd.FileName
            SaveContextToPath(ctx, ctx.SourcePath)
            Dim tp = tab.SelectedTab
            If tp IsNot Nothing Then tp.Text = Path.GetFileName(ctx.SourcePath)
        End Using
    End Sub

    Private Sub miCerrar_Click(sender As Object, e As EventArgs)
        Dim idx = tab.SelectedIndex
        If idx >= 0 Then CloseTabAt(idx)
    End Sub

    Private Sub CloseTabAt(index As Integer)
        If index < 0 OrElse index >= tab.TabPages.Count Then Return
        Dim tp = tab.TabPages(index)
        Dim ctx = TryCast(tp.Tag, TabContext)
        If ctx IsNot Nothing AndAlso ctx.Dirty Then
            Dim ans = MessageBox.Show(Me, "There is unsaved changes. ¿Save before close?", "Close tab", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            If ans = DialogResult.Cancel Then Return
            If ans = DialogResult.Yes Then
                miGuardar_Click(Me, EventArgs.Empty)
            End If
        End If
        tab.TabPages.RemoveAt(index)
        SetStatus("Closed tab.", ctx)
        UpdateButtonsForGame()
    End Sub

    ' ========= lectura / creación de pestañas =========

    Private Sub OpenArchiveInNewTab(path As String)
        Dim ctx As TabContext = Nothing
        Try
            Dim kind As GameKindUI
            Dim fo4Type As Fo4Ba2Type = Fo4Ba2Type.Unknown

            If IO.Path.GetExtension(path).Equals(".ba2", StringComparison.OrdinalIgnoreCase) Then
                kind = GameKindUI.FO4_BA2
                fo4Type = DetectFo4Ba2Type(path)
            Else
                kind = GameKindUI.SSE_BSA
            End If

            ctx = CreateEmptyTab(kind, path)
            ctx.Fo4Type = fo4Type

            Using fs As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim reader As New BethesdaReader(fs, Encoding.UTF8)
                Dim idx As Integer = 0
                Dim max = reader.EntriesFiles.Count
                Dim val = 0
                For Each it In reader.EntriesFiles
                    val += 1
                    Progreso(val, max)
                    Dim bytes = reader.ExtractToMemory(it.Index) ' ← lector
                    If kind = GameKindUI.FO4_BA2 AndAlso fo4Type = Fo4Ba2Type.DX10 AndAlso it.FileName.EndsWith(".dds", StringComparison.OrdinalIgnoreCase) Then
                        Dim rel As String = PathUtil.JoinDirFile(it.Directory, it.FileName)
                        Dim ve = Dx10Importer.FromDdsBytes(bytes, rel)
                        Dim ev As New EntryView With {
                            .Index = idx, .Directory = ve.Directory, .FileName = ve.FileName, .Data = ve.Data,
                            .DxgiFormat = ve.DxgiFormat, .Width = ve.Width, .Height = ve.Height,
                            .MipCount = ve.MipCount, .Faces = ve.Faces, .IsCubemap = ve.IsCubemap
                        }
                        ctx.Entries.Add(ev)
                    Else
                        Dim ev As New EntryView With {.Index = idx, .Directory = it.Directory, .FileName = it.FileName, .Data = bytes, .PreferCompress = True}
                        ctx.Entries.Add(ev)
                    End If
                    idx += 1
                Next

            End Using
        Catch ex As Exception
            MsgBox("Error reading file:" + vbCrLf + ex.Message, vbCritical, "Error")
        Finally
            Progreso(0, 100)
            ReindexEntries(ctx)
            RefreshGrid(ctx)
            RebuildDirectoryTree(ctx) ' << NUEVO
            SetStatus("Opened", ctx)
            UpdateButtonsForGame()
        End Try

    End Sub

    Private Function CreateEmptyTab(kind As GameKindUI, sourcePath As String) As TabContext
        Dim ctx As New TabContext() With {.GameKind = kind, .SourcePath = sourcePath, .GameRoot = IO.Path.GetDirectoryName(sourcePath)}
        Dim tp As New TabPage(If(String.IsNullOrEmpty(sourcePath), "(New)", Path.GetFileName(sourcePath))) With {.Tag = ctx}

        ' === Split izquierda (Tree) / derecha (Grid) ===
        Dim split As New SplitContainer() With {
    .Dock = DockStyle.Fill,
    .Orientation = Orientation.Vertical,
    .FixedPanel = FixedPanel.Panel1,
    .Panel1MinSize = 160,
    .SplitterWidth = 6
}
        ' Ajustes DPI + ancho preferido del panel izquierdo
        Dim scale As Single = Me.DeviceDpi / 96.0F
        split.Panel1MinSize = CInt(220 * scale)           ' mínimo real
        split.SplitterWidth = CInt(6 * scale)

        ' === TreeView de directorios ===
        Dim tv As New TreeView() With {
    .Dock = DockStyle.Fill,
    .HideSelection = False,
    .ShowLines = True,
    .HotTracking = False,
    .FullRowSelect = True
}
        ' Hacer los nodos menos “finos”
        tv.ItemHeight = Math.Max(tv.ItemHeight, CInt(18 * scale))
        tv.Indent = Math.Max(tv.Indent, CInt(16 * scale))

        AddHandler tv.AfterSelect, Sub(sender As Object, e As TreeViewEventArgs)
                                       Dim sel = TryCast(e.Node.Tag, String) ' Nothing = All, "" = root, "a\b"
                                       ctx.SelectedDir = sel
                                       ApplyFilter(ctx, txtFilter.Text)
                                   End Sub

        ' === DataGridView (estética ya aplicada) ===
        Dim grid As New DataGridView() With {
        .Dock = DockStyle.Fill,
        .ReadOnly = True,
        .AllowUserToAddRows = False,
        .AllowUserToDeleteRows = False,
        .AllowUserToResizeRows = False,
        .AllowUserToOrderColumns = True,
        .AllowUserToResizeColumns = True,
        .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        .MultiSelect = True,
        .AutoGenerateColumns = False,
        .BorderStyle = BorderStyle.None,
        .BackgroundColor = Drawing.SystemColors.Window,
        .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        .RowHeadersVisible = True,
        .CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
        .EnableHeadersVisualStyles = False
    }
        ' Estilos
        grid.RowHeadersWidth = 30
        grid.ColumnHeadersDefaultCellStyle.BackColor = Drawing.SystemColors.ControlDark
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Drawing.Color.White
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Drawing.SystemColors.ControlDark
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Drawing.Color.White
        grid.ColumnHeadersDefaultCellStyle.Font = New Drawing.Font(grid.Font, Drawing.FontStyle.Bold)
        grid.AlternatingRowsDefaultCellStyle.BackColor = Drawing.Color.FromArgb(245, 245, 245)
        grid.DefaultCellStyle.SelectionBackColor = Drawing.Color.FromArgb(204, 232, 255)
        grid.DefaultCellStyle.SelectionForeColor = Drawing.Color.Black
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False
        grid.GridColor = Drawing.SystemColors.ControlLight

        ' Columnas
        grid.Columns.Add(New DataGridViewTextBoxColumn() With {
        .HeaderText = "#", .DataPropertyName = "Index",
        .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, .FillWeight = 8
    })
        grid.Columns.Add(New DataGridViewTextBoxColumn() With {
        .HeaderText = "FileName", .DataPropertyName = "FileName",
        .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, .FillWeight = 35
    })
        grid.Columns.Add(New DataGridViewTextBoxColumn() With {
        .HeaderText = "Directory", .DataPropertyName = "Directory",
        .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, .FillWeight = 30
    })
        grid.Columns.Add(New DataGridViewTextBoxColumn() With {
        .HeaderText = "Path", .DataPropertyName = "FullPath",
        .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, .FillWeight = 27
    })

        For Each c As DataGridViewColumn In grid.Columns
            c.SortMode = DataGridViewColumnSortMode.Automatic
        Next

        AddHandler grid.SelectionChanged, Sub(sender As Object, e As EventArgs)
                                              UpdateButtonsForGame()
                                          End Sub

        ' Ensamblar UI
        split.Panel1.Controls.Add(tv)
        split.Panel2.Controls.Add(grid)
        tp.Controls.Add(split)

        tp.ImageIndex = 6
        tab.TabPages.Add(tp)
        tab.SelectedTab = tp

        ' Enlazar
        grid.DataSource = ctx.Filtered
        ctx.Grid = grid
        ctx.Tree = tv
        ctx.SelectedDir = Nothing ' "All" por defecto

        ' Formatos finales
        Dim prefer = CInt(tp.ClientSize.Width * 0.2F)
        Dim minW = CInt(180 * scale)
        Dim maxW = CInt(520 * scale)
        split.SplitterDistance = Math.Max(minW, Math.Min(prefer, maxW))

        ' Cargar raíz del árbol vacía
        RebuildDirectoryTree(ctx)

        Return ctx
    End Function



    Private Function CurrentTree() As TreeView
        Dim ctx = CurrentContext()
        If ctx Is Nothing Then Return Nothing
        Return ctx.Tree
    End Function

    ' ========= Treeview =========
    ' Construye TreeView a partir de ctx.Entries
    Private Sub RebuildDirectoryTree(ctx As TabContext)
        If ctx Is Nothing OrElse ctx.Tree Is Nothing Then Return
        Dim tv = ctx.Tree

        tv.BeginUpdate()
        Try
            tv.Nodes.Clear()

            ' Nodo raíz "All" (Tag = Nothing)
            Dim nAll = tv.Nodes.Add("All")
            nAll.Tag = Nothing
            nAll.NodeFont = New Drawing.Font(tv.Font, Drawing.FontStyle.Bold)
            nAll.ToolTipText = "Show all"

            ' ¿Hay archivos en la raíz?
            Dim hasRootFiles As Boolean = ctx.Entries.Any(Function(ev) String.IsNullOrEmpty(ev.Directory))

            ' Crear "(root)" solo si hay archivos en raíz
            Dim nRoot As TreeNode = Nothing
            If hasRootFiles Then
                nRoot = nAll.Nodes.Add("(root)")
                nRoot.Tag = ""
                nRoot.ToolTipText = "(root) – files without directory"
            End If

            ' Recopilar directorios únicos (no vacíos)
            Dim sep As Char = Correct_Path_separator
            Dim dirs = ctx.Entries.
            Select(Function(ev) If(ev.Directory, "")).
            Where(Function(d) Not String.IsNullOrEmpty(d)).
            Distinct(StringComparer.OrdinalIgnoreCase).
            OrderBy(Function(d) d, StringComparer.OrdinalIgnoreCase).
            ToList()

            ' Insertar jerarquía
            For Each Dire In dirs
                AddDirPath(nAll, Dire, sep)
            Next

            nAll.Expand()

            ' Selección según filtro actual (coherente si ya no hay root)
            Dim toSelect As TreeNode = nAll
            If ctx.SelectedDir Is Nothing Then
                toSelect = nAll
            ElseIf ctx.SelectedDir = "" Then
                If hasRootFiles AndAlso nRoot IsNot Nothing Then
                    toSelect = nRoot
                Else
                    ' ya no hay archivos en raíz: volver a All
                    ctx.SelectedDir = Nothing
                    toSelect = nAll
                End If
            Else
                Dim found = FindNodeByTag(nAll, ctx.SelectedDir)
                toSelect = If(found, nAll)
            End If

            tv.SelectedNode = toSelect

        Finally
            tv.EndUpdate()
        End Try
    End Sub


    ' Inserta nodos por cada segmento del path "a\b\c"; Tag del último = path completo
    Private Sub AddDirPath(root As TreeNode, path As String, sep As Char)
        Dim parts = path.Split(New Char() {sep}, StringSplitOptions.RemoveEmptyEntries)
        Dim cur = root
        Dim acc As New List(Of String)(parts.Length)
        For Each p In parts
            acc.Add(p)
            Dim accumPath = String.Join(sep, acc)
            Dim nextNode As TreeNode = Nothing
            For Each ch In cur.Nodes
                Dim tn = TryCast(ch, TreeNode)
                If tn IsNot Nothing AndAlso String.Equals(CStr(tn.Tag), accumPath, StringComparison.OrdinalIgnoreCase) Then
                    nextNode = tn : Exit For
                End If
                ' También permitir coincidencia por texto si Tag no está aún
                If tn IsNot Nothing AndAlso tn.Tag Is Nothing AndAlso String.Equals(tn.Text, p, StringComparison.OrdinalIgnoreCase) Then
                    nextNode = tn : Exit For
                End If
            Next
            If nextNode Is Nothing Then
                nextNode = cur.Nodes.Add(p)
                nextNode.Tag = accumPath
            End If
            cur = nextNode
        Next
    End Sub

    Private Function FindNodeByTag(root As TreeNode, tagValue As String) As TreeNode
        If String.Equals(CStr(root.Tag), tagValue, StringComparison.OrdinalIgnoreCase) Then Return root
        For Each ch As TreeNode In root.Nodes
            Dim f = FindNodeByTag(ch, tagValue)
            If f IsNot Nothing Then Return f
        Next
        Return Nothing
    End Function

    ' ========= filtro / refresco =========

    Private Sub RefreshGrid(ctx As TabContext)
        ApplyFilter(ctx, txtFilter.Text, force:=True)
    End Sub

    Private Sub ApplyFilter(ctx As TabContext, term As String, Optional force As Boolean = False)
        ' Evitar trabajo si no cambió texto ni directorio, salvo 'force'
        Dim sameText As Boolean = String.Equals(ctx.LastFilter, term, StringComparison.OrdinalIgnoreCase)
        Dim sameDir As Boolean = (ctx.LastDirFilter Is Nothing AndAlso ctx.SelectedDir Is Nothing) OrElse (ctx.LastDirFilter IsNot Nothing AndAlso ctx.SelectedDir IsNot Nothing AndAlso
            ctx.LastDirFilter.Equals(ctx.SelectedDir, StringComparison.OrdinalIgnoreCase))
        If Not force AndAlso sameText AndAlso sameDir Then Return

        ctx.Filtered.Clear()

        Dim q = If(term, "").Trim()
        Dim sel As String = ctx.SelectedDir ' Nothing=All, ""=root, "a\b..."
        Dim hasText As Boolean = (q.Length > 0)

        For Each e In ctx.Entries
            ' --- Filtro por directorio ---
            Dim dirOk As Boolean = True
            If sel Is Nothing Then
                dirOk = True ' All
            ElseIf sel = "" Then
                dirOk = String.IsNullOrEmpty(e.Directory) ' solo raíz
            Else
                If String.IsNullOrEmpty(e.Directory) Then
                    dirOk = False
                ElseIf e.Directory.Equals(sel, StringComparison.OrdinalIgnoreCase) Then
                    dirOk = True
                Else
                    ' Subdirectorios
                    Dim prefix = sel & Correct_Path_separator
                    dirOk = e.Directory.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                End If
            End If

            If Not dirOk Then Continue For

            ' --- Filtro por texto ---
            If hasText Then
                If e.FullPath.IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0 Then Continue For
            End If

            ctx.Filtered.Add(e)
        Next
        ctx.LastDirFilter = ctx.SelectedDir
        ctx.LastFilter = term
        SetStatus("Filtered", ctx)
    End Sub
    ' ========= insertar / eliminar / renombrar =========

    Private Sub btnInsert_Click(sender As Object, e As EventArgs)
        Dim ctx = CurrentContext()
        If ctx Is Nothing Then Exit Sub

        Using ofd As New OpenFileDialog()
            ofd.Title = "Select files to insert"
            ofd.Multiselect = True
            ofd.Filter = "Todos (*.*)|*.*"
            If ofd.ShowDialog(Me) <> DialogResult.OK Then Return
            AddFiles(ofd.FileNames, ctx)
        End Using
    End Sub
    Private Sub btnInsertFolder_Click(sender As Object, e As EventArgs)
        Dim ctx = CurrentContext()
        If ctx Is Nothing Then Exit Sub

        Using ofd As New FolderBrowserDialog()
            ofd.UseDescriptionForTitle = True
            ofd.Description = "Select folder to insert"
            If ofd.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim files = IO.Directory.EnumerateFiles(ofd.SelectedPath, "*.*", SearchOption.AllDirectories).ToArray
            AddFiles(files, ctx)
        End Using
    End Sub

    Private Sub AddFiles(files As String(), ctx As TabContext)
        Dim dataRoot = txtDataRoot.Text
        Dim added As Integer = 0
        Dim max = files.Count
        Dim val = 0
        If max > 0 Then
            Dim relTest = PathUtil.MakeRelativeUnderDataRoot(files(0), dataRoot)
            If relTest = files(0) Then
                MsgBox("The files and folders must be inside the data root path.", vbCritical, "Error")
            Else
                Try
                    For Each abs In files
                        Dim rel = PathUtil.MakeRelativeUnderDataRoot(abs, dataRoot)
                        Dim dir As String = "", fileName As String = ""
                        PathUtil.SplitDirFile(rel, dir, fileName)
                        Dim bytes = System.IO.File.ReadAllBytes(abs)
                        Dim isDds = IsDdsFilename(fileName)
                        val += 1
                        Progreso(val, max)
                        If ctx.GameKind = GameKindUI.SSE_BSA Then
                            ' BSA: archivo común
                            Dim ve As New VirtualEntry With {.Directory = If(dir, ""), .FileName = fileName, .Data = bytes, .PreferCompress = True}
                            AddOrReplaceEntry(ctx, ve)
                            added += 1
                        Else
                            ' FO4 BA2
                            If ctx.Fo4Type = Fo4Ba2Type.Unknown Then
                                ctx.Fo4Type = If(isDds, Fo4Ba2Type.DX10, Fo4Ba2Type.GNRL)
                                UpdateButtonsForGame()
                            End If

                            If ctx.Fo4Type = Fo4Ba2Type.DX10 Then
                                If Not isDds Then
                                    If MessageBox.Show(Me, $"This BA2 is DX10 (textures). {fileName} was ignored", "FO4 DX10", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) = DialogResult.Cancel Then
                                        Exit For
                                    End If
                                    Continue For
                                End If
                                Dim relPath = If(String.IsNullOrEmpty(dir), fileName, dir & Correct_Path_separator & fileName)
                                Dim veTex = Dx10Importer.FromDdsBytes(bytes, relPath)
                                AddOrReplaceEntry(ctx, veTex)
                                added += 1
                            Else
                                ' GNRL
                                If isDds Then
                                    If MessageBox.Show(Me, $"This BA2 es GNRL (general).{fileName} was ignored", "FO4 GNRL", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) = DialogResult.Cancel Then
                                        Exit For
                                    End If
                                    Continue For
                                End If
                                Dim ve As New VirtualEntry With {.Directory = If(dir, ""), .FileName = fileName, .Data = bytes}
                                AddOrReplaceEntry(ctx, ve)
                                added += 1
                            End If
                        End If
                    Next
                Catch ex As Exception
                    MsgBox("Error inserting archives:" + vbCrLf + ex.Message, vbCritical, "Error")
                End Try
            End If
        End If


        Progreso(0, 100)
        If added > 0 Then
            ctx.Dirty = True
            RefreshGrid(ctx)
            RebuildDirectoryTree(ctx) ' << NUEVO
            SetStatus($"Inserted {added} archive(s).", ctx)
            UpdateButtonsForGame()
        Else
            SetStatus("No archive(s) inserted.", ctx)
        End If
    End Sub
    Private Sub btnRemove_Click(sender As Object, e As EventArgs)
        Dim ctx = CurrentContext() : If ctx Is Nothing Then Return
        Dim grid = CurrentGrid() : If grid Is Nothing OrElse grid.SelectedRows.Count = 0 Then Return
        If MessageBox.Show(Me, "¿Remove selected elements?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return

        Dim toRemove As New List(Of EntryView)
        For Each r As DataGridViewRow In grid.SelectedRows
            Dim ev = TryCast(r.DataBoundItem, EntryView)
            If ev IsNot Nothing Then toRemove.Add(ev)
        Next
        Dim max = toRemove.Count
        Dim val = 0
        For Each ev In toRemove
            val += 1
            Progreso(val, max)
            ctx.Entries.Remove(ev)
        Next

        ReindexEntries(ctx)
        Progreso(0, 100)
        ctx.Dirty = True
        RefreshGrid(ctx)
        RebuildDirectoryTree(ctx) ' << NUEVO

        SetStatus($"Deleted {val} archive(s).", ctx)
        UpdateButtonsForGame()
    End Sub
    Private Function IsValidFileName(name As String) As Boolean
        If String.IsNullOrWhiteSpace(name) Then Return False

        ' 1. Contener caracteres inválidos
        Dim invalidChars = Path.GetInvalidFileNameChars()
        If name.IndexOfAny(invalidChars) >= 0 Then Return False

        ' 2. Longitud máxima
        If name.Length > 255 Then Return False

        ' 3. Reservados en Windows
        Dim reservedNames As String() = {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    }

        Dim baseName = Path.GetFileNameWithoutExtension(name).ToUpperInvariant()
        If reservedNames.Contains(baseName) Then Return False

        Return True
    End Function
    Private Function IsValidDirName(name As String) As Boolean
        Return IsValidFileName(name.Replace(Correct_Path_separator, ""))
    End Function
    Private Sub btnRename_Click(sender As Object, e As EventArgs)
        Dim ctx = CurrentContext() : If ctx Is Nothing Then Return
        Dim grid = CurrentGrid() : If grid Is Nothing OrElse grid.SelectedRows.Count <> 1 Then Return
        Dim ev = TryCast(grid.SelectedRows(0).DataBoundItem, EntryView) : If ev Is Nothing Then Return

        Dim newPath As String = Interaction.InputBox("New name and relative path (use '" + Correct_Path_separator + "'): ", "Rename", ev.FullPath)
        If String.IsNullOrWhiteSpace(newPath) OrElse newPath.Equals(ev.FullPath, StringComparison.Ordinal) Then Return

        Dim dir As String = ""
        Dim file As String = newPath
        Dim p = newPath.LastIndexOf(Correct_Path_separator)
        If p >= 0 Then
            dir = If(p = 0, "", newPath.Substring(0, p))
            file = newPath.Substring(p + 1)
        End If
        If Not IsValidFileName(file) OrElse (dir <> "" AndAlso Not IsValidDirName(dir)) Then
            MessageBox.Show(Me, "Invalid name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        ev.Directory = dir
        ev.FileName = file
        ReindexEntries(ctx)
        ctx.Dirty = True
        RefreshGrid(ctx)
        RebuildDirectoryTree(ctx) ' << NUEVO
        SetStatus("Renamed 1 archive.", ctx)
        UpdateButtonsForGame()
    End Sub

    ' ========= Add/Replace + Reindex =========

    Private Sub AddOrReplaceEntry(ctx As TabContext, ve As VirtualEntry)
        If ctx Is Nothing OrElse ve Is Nothing Then Return
        Dim match = ctx.Entries.FirstOrDefault(Function(x) x.Directory.Equals(ve.Directory, StringComparison.OrdinalIgnoreCase) AndAlso x.FileName.Equals(ve.FileName, StringComparison.OrdinalIgnoreCase))
        If match IsNot Nothing Then
            ' Reemplazar datos y metadatos
            match.Directory = ve.Directory
            match.FileName = ve.FileName
            match.Data = ve.Data
            match.PreferCompress = ve.PreferCompress
            match.DxgiFormat = ve.DxgiFormat
            match.Width = ve.Width
            match.Height = ve.Height
            match.MipCount = ve.MipCount
            match.Faces = ve.Faces
            match.IsCubemap = ve.IsCubemap
        Else
            ' Agregar nuevo
            Dim ev As New EntryView With {
                .Index = ctx.Entries.Count,
                .Directory = ve.Directory,
                .FileName = ve.FileName,
                .Data = ve.Data,
                .PreferCompress = ve.PreferCompress,
                .DxgiFormat = ve.DxgiFormat,
                .Width = ve.Width,
                .Height = ve.Height,
                .MipCount = ve.MipCount,
                .Faces = ve.Faces,
                .IsCubemap = ve.IsCubemap
            }
            ctx.Entries.Add(ev)
        End If
        ReindexEntries(ctx)
        ctx.Dirty = True
    End Sub

    Private Sub ReindexEntries(ctx As TabContext)
        For i = 0 To ctx.Entries.Count - 1
            ctx.Entries(i).Index = i
        Next
    End Sub

    ' ========= guardado =========

    Private Shared Function IsDdsFilename(name As String) As Boolean
        Return Not String.IsNullOrEmpty(name) AndAlso name.EndsWith(".dds", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Sub SuggestFo4Paths(baseOut As String, hasTex As Boolean, hasGen As Boolean, ByRef texPath As String, ByRef genPath As String)
        Dim base As String = baseOut
        If base.EndsWith(".ba2", StringComparison.OrdinalIgnoreCase) Then base = base.Substring(0, base.Length - 4)
        If base.EndsWith(" - Textures", StringComparison.OrdinalIgnoreCase) Then base = base.Substring(0, base.Length - " - Textures".Length)
        If base.EndsWith(" - Main", StringComparison.OrdinalIgnoreCase) Then base = base.Substring(0, base.Length - " - Main".Length)

        texPath = If(hasTex, base & " - Textures.ba2", Nothing)
        genPath = If(hasGen, base & " - Main.ba2", Nothing)
        If hasTex AndAlso Not hasGen Then texPath = If(Path.GetExtension(baseOut).Length > 0, baseOut, base & ".ba2")
        If hasGen AndAlso Not hasTex Then genPath = If(Path.GetExtension(baseOut).Length > 0, baseOut, base & ".ba2")
    End Sub

    Private Max_Writed As Double = 100
    Private Count_Writed As Double = 0
    Public Sub Writed()
        Count_Writed += 1
        Progreso(Count_Writed, Max_Writed)
    End Sub
    Private Sub SaveContextToPath(ctx As TabContext, outPath As String)
        Try
            If ctx Is Nothing Then Throw New InvalidOperationException("No active context.")

            If ctx.GameKind = GameKindUI.FO4_BA2 Then
                ' FO4 BA2: decidir tipo por contenido real
                Dim texRows = ctx.Entries.Where(Function(e) IsDdsFilename(e.FileName)).ToList()
                Dim genRows = ctx.Entries.Where(Function(e) Not IsDdsFilename(e.FileName)).ToList()

                If texRows.Count = 0 AndAlso genRows.Count = 0 Then
                    Throw New InvalidOperationException("No elements to save.")
                End If

                If texRows.Count > 0 AndAlso genRows.Count = 0 Then
                    ' === Solo texturas → DX10 ===
                    If System.IO.File.Exists(outPath) Then
                        If MessageBox.Show(Me, $"Archive already exist: {outPath}" & Environment.NewLine & "¿Overwrite?",
                               "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
                    End If

                    Dim ves As New List(Of VirtualEntry)(texRows.Count)
                    For Each e In texRows
                        ' Asegurar metadata DX10 (si vino solo con bytes, intentamos parsear DDS)
                        If (e.Width <= 0 OrElse e.Height <= 0 OrElse e.MipCount <= 0 OrElse e.DxgiFormat < 0 OrElse e.Data Is Nothing) Then
                            Dim ve2 = Dx10Importer.FromDdsBytes(e.Data, e.FullPath)
                            e.DxgiFormat = ve2.DxgiFormat : e.Width = ve2.Width : e.Height = ve2.Height
                            e.MipCount = ve2.MipCount : e.Faces = ve2.Faces : e.IsCubemap = ve2.IsCubemap : e.Data = ve2.Data
                        End If
                        If e.Width <= 0 OrElse e.Height <= 0 OrElse e.MipCount <= 0 OrElse e.DxgiFormat < 0 Then
                            Throw New InvalidDataException($"Missing DX10 metadada in '{e.FullPath}'.")
                        End If
                        ves.Add(New VirtualEntry With {
              .Directory = e.Directory, .FileName = e.FileName, .Data = e.Data,
              .DxgiFormat = e.DxgiFormat, .Width = e.Width, .Height = e.Height,
              .MipCount = e.MipCount, .Faces = If(e.IsCubemap, 6, Math.Max(1, e.Faces)), .IsCubemap = e.IsCubemap
            })
                    Next
                    Dim optDX As Ba2WriterDX10.Options = MakeDxOptionsFromConfig()
                    Using fs As New FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None)
                        Max_Writed = ctx.Entries.Count
                        Count_Writed = 0
                        AddHandler Ba2WriterDX10.Writed, AddressOf Writed
                        Ba2WriterDX10.Write(fs, ves, optDX)
                        RemoveHandler Ba2WriterDX10.Writed, AddressOf Writed
                        Progreso(0, 100)
                    End Using
                    ctx.Dirty = False
                    SetStatus("Saved (DX10)", ctx)
                    UpdateButtonsForGame()
                    Return
                End If

                If genRows.Count > 0 AndAlso texRows.Count = 0 Then
                    ' === Solo generales → GNRL ===
                    If System.IO.File.Exists(outPath) Then
                        If MessageBox.Show(Me, $"Archive already exist: {outPath}" & Environment.NewLine & "¿Overwrite?",
                               "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
                    End If
                    Dim vesG As New List(Of VirtualEntry)(genRows.Count)
                    For Each e In genRows
                        vesG.Add(New VirtualEntry With {.Directory = e.Directory, .FileName = e.FileName, .Data = e.Data})
                    Next
                    Dim optG As Ba2WriterGNRL.Options = MakeGnrlOptionsFromConfig()
                    Using fs As New FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None)
                        Max_Writed = ctx.Entries.Count
                        Count_Writed = 0
                        AddHandler Ba2WriterGNRL.Writed, AddressOf Writed
                        Ba2WriterGNRL.Write(fs, vesG, optG)
                        RemoveHandler Ba2WriterGNRL.Writed, AddressOf Writed
                        Progreso(0, 100)
                    End Using
                    ctx.Dirty = False
                    SetStatus("Saved (GNRL)", ctx)
                    UpdateButtonsForGame()
                    Return
                End If

                ' === Mezcla (dds + no-dds) → dividir ===
                Dim choice = MessageBox.Show(Me,
          "This set contains texturess (.dds) y general archives." & Environment.NewLine &
          "BA2 format does not allow to mix them in a single file." & Environment.NewLine & Environment.NewLine &
          "¿Save to two files?: '... - Textures.ba2' y '... - Main.ba2'?",
          "Fallout 4 — Split file", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information)
                If choice <> DialogResult.Yes Then
                    Throw New InvalidOperationException("Save canceled (DX10/GNRL Mix).")
                End If

                Dim texPath As String = Nothing, genPath As String = Nothing
                SuggestFo4Paths(outPath, True, True, texPath, genPath)

                If System.IO.File.Exists(texPath) Then
                    If MessageBox.Show(Me, $"Archive already exist: {texPath}" & Environment.NewLine & "¿Overwrite?",
                             "Confirm (Textures)", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
                End If
                If System.IO.File.Exists(genPath) Then
                If MessageBox.Show(Me, $"Archive already exist: {genPath}" & Environment.NewLine & "¿Overwrite?",
                             "Confirmar (General)", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
            End If

            ' 1) DX10
            Dim vesTex As New List(Of VirtualEntry)(texRows.Count)
                For Each e In texRows
                    If (e.Width <= 0 OrElse e.Height <= 0 OrElse e.MipCount <= 0 OrElse e.DxgiFormat < 0 OrElse e.Data Is Nothing) Then
                        Dim ve2 = Dx10Importer.FromDdsBytes(e.Data, e.FullPath)
                        e.DxgiFormat = ve2.DxgiFormat : e.Width = ve2.Width : e.Height = ve2.Height
                        e.MipCount = ve2.MipCount : e.Faces = ve2.Faces : e.IsCubemap = ve2.IsCubemap : e.Data = ve2.Data
                    End If
                    If e.Width <= 0 OrElse e.Height <= 0 OrElse e.MipCount <= 0 OrElse e.DxgiFormat < 0 Then
                        Throw New InvalidDataException($"Missing DX10 metadada in '{e.FullPath}'.")
                    End If
                    vesTex.Add(New VirtualEntry With {
            .Directory = e.Directory, .FileName = e.FileName, .Data = e.Data,
            .DxgiFormat = e.DxgiFormat, .Width = e.Width, .Height = e.Height,
            .MipCount = e.MipCount, .Faces = If(e.IsCubemap, 6, Math.Max(1, e.Faces)), .IsCubemap = e.IsCubemap
          })
                Next
                Dim optDX2 As Ba2WriterDX10.Options = MakeDxOptionsFromConfig()
                Using fs As New FileStream(texPath, FileMode.Create, FileAccess.Write, FileShare.None)
                    Max_Writed = ctx.Entries.Count
                    Count_Writed = 0
                    AddHandler Ba2WriterDX10.Writed, AddressOf Writed
                    Ba2WriterDX10.Write(fs, vesTex, optDX2)
                    RemoveHandler Ba2WriterDX10.Writed, AddressOf Writed
                    Progreso(0, 100)
                End Using

                ' 2) GNRL
                Dim vesGen As New List(Of VirtualEntry)(genRows.Count)
                For Each e In genRows
                    vesGen.Add(New VirtualEntry With {.Directory = e.Directory, .FileName = e.FileName, .Data = e.Data})
                Next
                Dim optG2 As Ba2WriterGNRL.Options = MakeGnrlOptionsFromConfig()
                Using fs As New FileStream(genPath, FileMode.Create, FileAccess.Write, FileShare.None)
                    Max_Writed = ctx.Entries.Count
                    Count_Writed = 0
                    AddHandler Ba2WriterGNRL.Writed, AddressOf Writed
                    Ba2WriterGNRL.Write(fs, vesGen, optG2)
                    RemoveHandler Ba2WriterGNRL.Writed, AddressOf Writed
                    Progreso(0, 100)
                End Using

                ctx.Dirty = False
                SetStatus("Saved (Both)", ctx)
                UpdateButtonsForGame()
                Return

            Else
                ' ===== Skyrim SE (BSA) =====
                Dim ves As New List(Of VirtualEntry)(ctx.Entries.Count)
                For Each e In ctx.Entries
                    ves.Add(New VirtualEntry With {.Directory = e.Directory, .FileName = e.FileName, .Data = e.Data, .PreferCompress = True})
                Next

                If System.IO.File.Exists(outPath) Then
                    If MessageBox.Show(Me, $"Archive already exist: {outPath}" & Environment.NewLine & "¿Overwrite?",
                             "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
                End If

                Dim opt As BsaWriter.Options = MakeBsaOptionsFromConfig()
                Using fs As New FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None)
                    Max_Writed = ctx.Entries.Count
                    Count_Writed = 0
                    AddHandler BsaWriter.Writed, AddressOf Writed
                    BsaWriter.Write(fs, ves, opt)
                    RemoveHandler BsaWriter.Writed, AddressOf Writed
                    Progreso(0, 100)
                End Using

                ctx.Dirty = False
                SetStatus("Saved (BSA)", ctx)
                UpdateButtonsForGame()
            End If

        Catch ex As Exception
            MessageBox.Show(Me, "Error on saving: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ========= util =========

    Private Function DetectFo4Ba2Type(path As String) As Fo4Ba2Type
        Using fs As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            If fs.Length < 12 Then Return Fo4Ba2Type.Unknown
            Using br As New BinaryReader(fs, Encoding.ASCII, leaveOpen:=True)
                Dim magic = Encoding.ASCII.GetString(br.ReadBytes(4))
                If magic <> "BTDX" Then Return Fo4Ba2Type.Unknown
                Dim _ver = br.ReadUInt32()
                Dim typeStr = Encoding.ASCII.GetString(br.ReadBytes(4))
                If typeStr = "DX10" Then Return Fo4Ba2Type.DX10
                If typeStr = "GNRL" Then Return Fo4Ba2Type.GNRL
                Return Fo4Ba2Type.Unknown
            End Using
        End Using
    End Function

    ' ========= nuevos handlers: crear archivos =========
    Private Sub miCrearBSA_Click(sender As Object, e As EventArgs)
        Dim ctx = CreateEmptyTab(GameKindUI.SSE_BSA, Nothing)
        RebuildDirectoryTree(ctx) ' << NUEVO
        UpdateButtonsForGame()
        SetStatus("New BSA", ctx)
    End Sub

    Private Sub miCrearBA2_Click(sender As Object, e As EventArgs)
        Dim ctx = CreateEmptyTab(GameKindUI.FO4_BA2, Nothing)
        RebuildDirectoryTree(ctx) ' << NUEVO
        ctx.Fo4Type = Fo4Ba2Type.GNRL
        UpdateButtonsForGame()
        SetStatus("New BA2 (GNRL)", ctx)
    End Sub

    Private Sub miCrearBA2Tex_Click(sender As Object, e As EventArgs)
        Dim ctx = CreateEmptyTab(GameKindUI.FO4_BA2, Nothing)
        RebuildDirectoryTree(ctx) ' << NUEVO
        ctx.Fo4Type = Fo4Ba2Type.DX10
        UpdateButtonsForGame()
        SetStatus("New BA2 (DX10)", ctx)
    End Sub

    ' ========= extracción (usa BethesdaReader.ExtractToMemory) =========

    Private Sub btnExtract_Click(sender As Object, e As EventArgs)
        Dim ctx = CurrentContext() : If ctx Is Nothing Then Return
        Dim grid = CurrentGrid() : If grid Is Nothing OrElse grid.SelectedRows.Count = 0 Then Return
        Using fbd As New FolderBrowserDialog()
            fbd.Description = "Selecciona target directory to extract selected archive(s)"
            fbd.UseDescriptionForTitle = True
            If fbd.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim list As New List(Of EntryView)
            For Each r As DataGridViewRow In grid.SelectedRows
                Dim ev = TryCast(r.DataBoundItem, EntryView)
                If ev IsNot Nothing Then list.Add(ev)
            Next
            Dim n = ExtractEntries(fbd.SelectedPath, ctx, list)
            SetStatus($"Extracted {n} file(s).", ctx)
        End Using
    End Sub

    Private Sub btnExtractAll_Click(sender As Object, e As EventArgs)
        Dim ctx = CurrentContext() : If ctx Is Nothing Then Return
        If ctx.Entries Is Nothing OrElse ctx.Entries.Count = 0 Then Return
        Using fbd As New FolderBrowserDialog()
            fbd.Description = "Selecciona target directory to extract ALL archive(s)"
            fbd.UseDescriptionForTitle = True
            If fbd.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim n = ExtractEntries(fbd.SelectedPath, ctx, ctx.Entries)
            SetStatus($"Extracted {n} file(s) (all).", ctx)
        End Using
    End Sub

    ''' <summary>
    ''' Extrae entries releyendo del archivo en disco con BethesdaReader.ExtractToMemory.
    ''' Si SourcePath no existe o un path no se encuentra en el archivo, cae a EntryView.Data.
    ''' </summary>
    Private Function ExtractEntries(baseDir As String, ctx As TabContext, entries As IEnumerable(Of EntryView)) As Integer
        If String.IsNullOrWhiteSpace(baseDir) OrElse ctx Is Nothing OrElse entries Is Nothing Then Return 0
        Dim count As Integer = 0
        Try
            Dim max = entries.Count
            Dim val = 0
            For Each ev In entries
                val += 1
                Progreso(val, max)
                If ev Is Nothing Then Continue For
                Dim rel As String = PathUtil.JoinDirFile(ev.Directory, ev.FileName)
                ' Extraer siempre desde memoria: refleja ediciones no guardadas y evita I/O redundante
                Dim bytes As Byte() = ev.Data
                If bytes Is Nothing Then Continue For

                Dim relOs As String = rel.Replace(InCorrect_Path_separator, Path.DirectorySeparatorChar).Replace(Correct_Path_separator, Path.DirectorySeparatorChar)
                Dim outPath As String = Path.Combine(baseDir, relOs)
                Dim outDir As String = Path.GetDirectoryName(outPath)
                If Not String.IsNullOrEmpty(outDir) AndAlso Not Directory.Exists(outDir) Then Directory.CreateDirectory(outDir)
                File.WriteAllBytes(outPath, bytes)
                count += 1
            Next
        Catch ex As Exception
            MsgBox("Error extracting archive:" + vbCrLf + ex.Message, vbCritical, "Error")
        Finally
            Progreso(0, 100)
        End Try

        Return count
    End Function


End Class







