' WriteOptionsConfig.vb
Option Strict On
Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization

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

    Public Sub Save(path As String)
        Dim opt As New JsonSerializerOptions With {.WriteIndented = True}
        Dim json = JsonSerializer.Serialize(Me, opt)
        File.WriteAllText(path, json)
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
        ' ZlibPreset: "Default" | "Fastest" | "Optimal"
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

