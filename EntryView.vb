Option Strict On

Imports BSA_BA2_Library_DLL.BethesdaArchive.Core

''' <summary>Fila mostrada en la grilla — una entrada del archive abierto o agregada por el usuario.
''' <para>⛔ QUE HAY EN <see cref="Data"/>, porque de eso cuelgan dos defectos que ya costaron archivos:
''' para una entrada DX10 (BA2 de texturas de FO4) <b>Data es el payload SIN cabecera DDS</b> — el mismo
''' contrato que <c>Dx10Importer.FromDdsBytes</c>, que es quien la produce, tanto al ABRIR un archive
''' (MainForm.OpenArchiveInNewTab) como al INSERTAR un .dds suelto (MainForm.AddFiles). La cabecera se
''' reconstruye desde la metadata (<see cref="DxgiFormat"/>, <see cref="Width"/>, <see cref="Height"/>,
''' <see cref="MipCount"/>, <see cref="IsCubemap"/>) con <c>Dx10Importer.ToDdsBytes</c>. Para todo lo
''' demas —BSA, BA2 GNRL, y un .dds que vive dentro de un GNRL— Data es el archivo COMPLETO y esos
''' campos de metadata quedan en 0.</para>
''' <para>O sea que <c>Width &gt; 0 AndAlso MipCount &gt; 0</c> es lo que distingue "payload despojado"
''' de "archivo completo". No es una convencion inventada aca: es la que ya imponen el importador y el
''' writer de BA2 DX10.</para>
''' <para>⛔ VIVE FUERA DEL FORM a proposito. Era una clase <c>Private</c> anidada en
''' <c>Mainform_form</c>, y por eso la logica de guardado y de extraccion —que opera sobre estas filas—
''' no podia salir del Form ni ser medida por un gate. Ver <see cref="GuardadoDeArchive"/>.</para></summary>
Friend NotInheritable Class EntryView
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
    ''' <summary>The DX10 record's tile byte, kept on save (VirtualEntry.TileMode; user decision r19c-05). 8 for a new row.</summary>
    Public Property TileMode As Byte = 8
    ' BSA
    Public Property PreferCompress As Boolean

    ''' <summary>THE row of an archive entry, as the Manager builds it when it opens an archive (MainForm.OpenArchiveInNewTab; and
    ''' Tools\Ba2ManagerSaveGate H4 calls this same function): a .dds of a BA2 DX10 -&gt; the stripped payload and the metadata of
    ''' Dx10Importer.FromDdsBytes, plus the record's tile byte, kept on save (r19c-05; 8 when the entry has no DX10 record); any
    ''' other entry -&gt; the whole file, opaque, compressed on save when the format allows.</summary>
    ''' <param name="dx10Texture">The entry is a .dds of a Fallout 4 BA2 DX10 (the tab's kind and the file's extension).</param>
    Friend Shared Function FromArchiveEntry(it As ArchiveEntry, bytes As Byte(), index As Integer, dx10Texture As Boolean) As EntryView
        If Not dx10Texture Then
            Return New EntryView With {.Index = index, .Directory = it.Directory, .FileName = it.FileName, .Data = bytes, .PreferCompress = True}
        End If
        Dim ve = Dx10Importer.FromDdsBytes(bytes, PathUtil.JoinDirFile(it.Directory, it.FileName))
#If DEBUG Then
        ' Ba2ManagerSaveGate H4's directed mutant (the BA2 library's switch, Ba2GateMutants): the row forgets the record's tile byte.
        If Ba2GateMutants.IsOn("mgr-entry-tile8") Then
            Return New EntryView With {.Index = index, .Directory = ve.Directory, .FileName = ve.FileName, .Data = ve.Data, .DxgiFormat = ve.DxgiFormat,
                .Width = ve.Width, .Height = ve.Height, .MipCount = ve.MipCount, .Faces = ve.Faces, .IsCubemap = ve.IsCubemap, .TileMode = 8}
        End If
#End If
        Return New EntryView With {
            .Index = index, .Directory = ve.Directory, .FileName = ve.FileName, .Data = ve.Data,
            .DxgiFormat = ve.DxgiFormat, .Width = ve.Width, .Height = ve.Height,
            .MipCount = ve.MipCount, .Faces = ve.Faces, .IsCubemap = ve.IsCubemap,
            .TileMode = If(it.Dx10Record.HasValue, it.Dx10Record.Value.TileMode, CByte(8))
        }
    End Function
End Class
