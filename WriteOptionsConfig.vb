' WriteOptionsConfig.vb
Option Strict On
Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports BSA_BA2_Library_DLL

Public NotInheritable Class AppConfig
    Public Property Ba2Dx As Ba2Group = Ba2Group.Defaults()   ' FO4 BA2 DX10
    Public Property Ba2Gnrl As Ba2Group = Ba2Group.Defaults() ' FO4 BA2 GNRL
    Public Property Bsa As BsaGroup = BsaGroup.Defaults()     ' SSE BSA

    <JsonIgnore>
    Public ReadOnly Property IsLoaded As Boolean

    Public Sub New()
    End Sub

    <JsonConstructor>
    Public Sub New(ba2Dx As Ba2Group, ba2Gnrl As Ba2Group, bsa As BsaGroup)
        Me.Ba2Dx = If(ba2Dx, Ba2Group.Defaults())
        Me.Ba2Gnrl = If(ba2Gnrl, Ba2Group.Defaults())
        Me.Bsa = If(bsa, BsaGroup.Defaults())
    End Sub

    Public Shared Function Load(path As String) As AppConfig
        Try
            If Not File.Exists(path) Then Return DefaultsAll()
            Dim json = File.ReadAllText(path)
            Dim opt As New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True, .ReadCommentHandling = JsonCommentHandling.Skip, .AllowTrailingCommas = True}
            Dim cfg = JsonSerializer.Deserialize(Of AppConfig)(json, opt)
            If cfg Is Nothing Then Return DefaultsAll()
            Return cfg
        Catch
            Return DefaultsAll()
        End Try
    End Function

    ''' <summary>⛔ VA POR <c>EscrituraEnElLugar.Escribir</c>, NO POR <c>File.WriteAllText</c>.
    ''' `WriteAllText` pide CREATE_ALWAYS y CREATE_ALWAYS sobre un archivo OCULTO da ACCESS_DENIED — y este
    ''' archivo vive al lado del .exe, o sea justo donde OneDrive y los desempaquetadores dejan el
    ''' atributo. El guardado se tragaba el error (MainForm lo llama dentro de un Catch mudo) y las
    ''' opciones del usuario dejaban de persistir EN SILENCIO. Medido en el caso P0 de
    ''' <c>Tools\Ba2ManagerSaveGate</c>; testigo P3.
    ''' <para>Es <c>Escribir</c> y NO <c>GuardarConCopia</c> a proposito: el config es salida REGENERABLE
    ''' —<see cref="Load"/> devuelve <see cref="DefaultsAll"/> ante cualquier fallo—, asi que no le
    ''' corresponde la red con copia. Testigo P4: no deja ningun `.npcm.prev` al lado.</para>
    ''' <para>Los bytes NO cambian: se escribe UTF-8 SIN BOM, que es exactamente lo que dejaba
    ''' <c>File.WriteAllText</c> (su default es <c>UTF8Encoding(False)</c>). Un BOM acá lo rompería para
    ''' cualquier lector estricto de JSON. Verificado en P4.</para>
    ''' <para>⛔ CONTRATO DEL CUERPO: se escriben los bytes directo al stream y no se lo envuelve en un
    ''' <c>StreamWriter</c> — envolverlo sin <c>leaveOpen:=True</c> cerraria el FileStream del que
    ''' <c>EscrituraEnElLugar</c> es dueño, y la guarda <c>ContratoDelCuerpoException</c> lo cazaria.</para></summary>
    Public Sub Save(path As String)
        Dim opt As New JsonSerializerOptions With {.WriteIndented = True}
        Dim json = JsonSerializer.Serialize(Me, opt)
        Dim bytes = New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False).GetBytes(json)
        EscrituraEnElLugar.Escribir(path, Sub(fs) fs.Write(bytes, 0, bytes.Length))
    End Sub

    Public Shared Function DefaultsAll() As AppConfig
        Return New AppConfig With {
            .Ba2Dx = Ba2Group.Defaults(),
            .Ba2Gnrl = Ba2Group.Defaults(),
            .Bsa = BsaGroup.Defaults()
        }
    End Function

    ' ====== Grupos ======
    Public NotInheritable Class Ba2Group
        ' Version: 1,2,3,7,8 (default 8)
        Public Property Version As Integer = 8
        ' Compression: "zip" | "lz4"   (lz4 solo se usa si Version=3)
        Public Property Compression As String = "zip"
        ' Strings table
        Public Property IncludeStrings As Boolean = True
        ' ZlibPreset: "Default" | "Fastest" | "Maximum"
        Public Property ZlibPreset As String = "Default"

        Public Shared Function Defaults() As Ba2Group
            Return New Ba2Group()
        End Function
    End Class

    Public NotInheritable Class BsaGroup
        Public Property UseDirectoryStrings As Boolean = True
        Public Property UseFileStrings As Boolean = True
        Public Property EmbedNames As Boolean = True
        Public Property GlobalCompressed As Boolean = True

        Public Shared Function Defaults() As BsaGroup
            Return New BsaGroup()
        End Function
    End Class
End Class

