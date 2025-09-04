' FrmWriteOptions.vb
Option Strict On
Imports System.Windows.Forms
Imports BSA_BA2_Library_DLL.BethesdaArchive.Core

Partial Class FrmWriteOptions
    Private ReadOnly _cfg As AppConfig

    Public Sub New(cfg As AppConfig, Optional showDx10 As Boolean = True, Optional showGnrl As Boolean = True, Optional showBsa As Boolean = True)
        InitializeComponent()
        _cfg = cfg

        ' Mostrar/ocultar pestañas según corresponda
        If Not showDx10 Then Tab.TabPages.Remove(TabDx)
        If Not showGnrl Then Tab.TabPages.Remove(TabGn)
        If Not showBsa Then Tab.TabPages.Remove(TabBsa)

        ' Cargar config en UI
        If showDx10 Then LoadBa2ToUi(_cfg.Ba2Dx, isDx10:=True)
        If showGnrl Then LoadBa2ToUi(_cfg.Ba2Gnrl, isDx10:=False)
        If showBsa Then LoadBsaToUi(_cfg.Bsa)

        ' Reglas de habilitación
        AddHandler cboDxVer.SelectedIndexChanged, Sub() UpdateBa2Enablement(isDx10:=True)
        AddHandler cboGnVer.SelectedIndexChanged, Sub() UpdateBa2Enablement(isDx10:=False)
        AddHandler cboDxComp.SelectedIndexChanged, Sub() UpdateBa2Enablement(isDx10:=True)
        AddHandler cboGnComp.SelectedIndexChanged, Sub() UpdateBa2Enablement(isDx10:=False)

        UpdateBa2Enablement(isDx10:=True)
        UpdateBa2Enablement(isDx10:=False)
    End Sub

    Private Sub LoadBa2ToUi(g As AppConfig.Ba2Group, isDx10 As Boolean)
        Dim cboVer = If(isDx10, cboDxVer, cboGnVer)
        Dim cboComp = If(isDx10, cboDxComp, cboGnComp)
        Dim chkStr = If(isDx10, chkDxStr, chkGnStr)
        Dim cboZ = If(isDx10, cboDxZlib, cboGnZlib)

        cboVer.SelectedItem = g.Version.ToString()
        cboComp.SelectedItem = If(String.Equals(g.Compression, "lz4", StringComparison.OrdinalIgnoreCase), "LZ4 (raw)", "ZIP (zlib)")
        chkStr.Checked = g.IncludeStrings
        cboZ.SelectedItem = If(String.IsNullOrEmpty(g.ZlibPreset), "Default", g.ZlibPreset)
    End Sub

    Private Sub LoadBsaToUi(g As AppConfig.BsaGroup)
        chkBsaDir.Checked = g.UseDirectoryStrings
        chkBsaFile.Checked = g.UseFileStrings
        chkBsaEmbed.Checked = g.EmbedNames
        chkBsaGlobal.Checked = g.GlobalCompressed
    End Sub

    Private Sub UpdateBa2Enablement(isDx10 As Boolean)
        Dim cboVer = If(isDx10, cboDxVer, cboGnVer)
        Dim cboComp = If(isDx10, cboDxComp, cboGnComp)
        Dim cboZ = If(isDx10, cboDxZlib, cboGnZlib)

        Dim ver As Integer
        Integer.TryParse(CStr(cboVer.SelectedItem), ver)
        Dim isV3 As Boolean = (ver = 3)

        ' En v3 se puede elegir LZ4 o ZIP; en el resto, solo ZIP
        cboComp.Enabled = isV3
        If Not isV3 Then cboComp.SelectedItem = "ZIP (zlib)"

        ' Zlib preset solo aplica si se usa ZIP
        cboZ.Enabled = (CStr(cboComp.SelectedItem) = "ZIP (zlib)")
    End Sub

    ' Devuelve True si el usuario confirmó y aplicó los cambios a cfg
    Public Shared Function EditOptions(owner As IWin32Window, cfg As AppConfig, showDx10 As Boolean, showGnrl As Boolean, showBsa As Boolean) As Boolean
        Using f As New FrmWriteOptions(cfg, showDx10, showGnrl, showBsa)
            If f.ShowDialog(owner) <> DialogResult.OK Then Return False
            f.ApplyToConfig()
            Return True
        End Using
    End Function

    Private Sub ApplyToConfig()
        If Tab.TabPages.Contains(TabDx) Then
            _cfg.Ba2Dx.Version = Integer.Parse(CStr(cboDxVer.SelectedItem))
            _cfg.Ba2Dx.Compression = If(CStr(cboDxComp.SelectedItem) = "LZ4 (raw)", "lz4", "zip")
            _cfg.Ba2Dx.IncludeStrings = chkDxStr.Checked
            _cfg.Ba2Dx.ZlibPreset = CStr(cboDxZlib.SelectedItem)
        End If
        If Tab.TabPages.Contains(TabGn) Then
            _cfg.Ba2Gnrl.Version = Integer.Parse(CStr(cboGnVer.SelectedItem))
            _cfg.Ba2Gnrl.Compression = If(CStr(cboGnComp.SelectedItem) = "LZ4 (raw)", "lz4", "zip")
            _cfg.Ba2Gnrl.IncludeStrings = chkGnStr.Checked
            _cfg.Ba2Gnrl.ZlibPreset = CStr(cboGnZlib.SelectedItem)
        End If
        If Tab.TabPages.Contains(TabBsa) Then
            _cfg.Bsa.UseDirectoryStrings = chkBsaDir.Checked
            _cfg.Bsa.UseFileStrings = chkBsaFile.Checked
            _cfg.Bsa.EmbedNames = chkBsaEmbed.Checked
            _cfg.Bsa.GlobalCompressed = chkBsaGlobal.Checked
        End If
    End Sub
End Class
