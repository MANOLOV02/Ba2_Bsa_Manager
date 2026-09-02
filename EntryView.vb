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
    ' BSA
    Public Property PreferCompress As Boolean
End Class
