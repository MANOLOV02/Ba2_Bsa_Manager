Option Strict On

Imports System.IO
Imports BSA_BA2_Library_DLL
Imports BSA_BA2_Library_DLL.BethesdaArchive.Core

''' <summary>Lo que Ba2_Bsa_Manager hace con los BYTES al guardar y al extraer, fuera del Form.
''' <para>⛔ POR QUE ESTA CLASE EXISTE. Todo esto vivia dentro de <c>Mainform_form</c>, mezclado con los
''' <c>MessageBox</c> de confirmacion, y por eso NO SE PODIA MEDIR: un gate no puede instanciar un Form
''' ni contestarle un dialogo. Tres defectos bloqueantes vivieron ahi sin que nada se pusiera rojo. La
''' UI (elegir la ruta, preguntar si se sobrescribe, pintar el progreso) se queda en el Form; los bytes
''' se deciden aca, y los mide <c>Tools\Ba2ManagerSaveGate</c>.</para>
''' <para>El progreso entra como <c>Action</c> y se engancha al evento del writer ACA, con un handler
''' <b>Shared</b>: el evento <c>Writed</c> es Shared (raiz estatica) y un handler de instancia captura el
''' Form, asi que sin desenganchar el pack siguiente dispara tambien al handler zombie. El
''' <c>Try/Finally</c> que lo saca es obligatorio y esta en las tres escrituras.</para></summary>
Friend NotInheritable Class GuardadoDeArchive

    Private Sub New()
    End Sub

    ' =================================================================================================
    ' El puente del progreso. Un campo Shared + un handler Shared en vez de `AddressOf` sobre un metodo
    ' de instancia del Form: asi el evento Shared del writer nunca enraiza al Form, y lo que se limpia
    ' en el Finally es una referencia a la Action, no la suscripcion de un objeto vivo.
    ' No es reentrante — no hace falta: el guardado corre en el hilo de UI, uno por vez.
    ' =================================================================================================
    Private Shared _progreso As Action

    Private Shared Sub AlEscribirUno()
        Dim p = _progreso
        If p IsNot Nothing Then p()
    End Sub

    ' =================================================================================================
    ' STAGING — de las filas de la grilla a las entradas que consume el writer.
    ' =================================================================================================

    ''' <summary>Las entradas DX10 de un guardado de texturas.
    ''' <para>⛔ LA FILA VIVA NO SE TOCA, Y ESE ES EL PUNTO. Acá se ESCRIBIA sobre la fila el resultado de
    ''' la re-importacion (<c>e.Data = ve2.Data</c>, y el payload de <c>Dx10Importer</c> va SIN cabecera
    ''' DDS por contrato) antes de que el guardado hubiera escrito un solo byte. Si una fila POSTERIOR
    ''' tiraba —un .dds corrupto entre veinte buenos—, las anteriores quedaban en la pestaña VIVA con el
    ''' Data recortado y <c>Dirty=True</c>, y el Extract de rescate escribia esos bytes crudos: .dds
    ''' ilegibles. Mutar el modelo antes de confirmar la escritura es el defecto; la conversion es LOCAL
    ''' y lo unico que sale de aca es la lista de entradas.</para>
    ''' <para>Lo que se perdio al no cachear la metadata en la fila: nada medible. El parseo es SOLO de
    ''' cabecera (<c>GetMetadataFromDDSMemory</c>, sin decodificar un pixel) y ademas casi nunca corre —
    ''' las filas de una pestaña DX10 ya vienen con metadata, tanto al abrir el archive como al insertar
    ''' un .dds suelto. Este camino es para el .dds que estaba adentro de un BA2 GNRL.</para></summary>
    Friend Shared Function EntradasDx10(filas As IEnumerable(Of EntryView)) As List(Of VirtualEntry)
        Dim ves As New List(Of VirtualEntry)()
        For Each e In filas
            ves.Add(EntradaDx10(e))
        Next
        Return ves
    End Function

    ''' <summary>La entrada DX10 de UNA fila, sin tocarla. Ver <see cref="EntradasDx10"/>.</summary>
    Friend Shared Function EntradaDx10(e As EntryView) As VirtualEntry
        ' Ya trae metadata: la fila se usa tal cual (su Data ya es el payload despojado).
        If e.Width > 0 AndAlso e.Height > 0 AndAlso e.MipCount > 0 AndAlso e.DxgiFormat >= 0 AndAlso e.Data IsNot Nothing Then
            Return New VirtualEntry With {
                .Directory = e.Directory, .FileName = e.FileName, .Data = e.Data,
                .DxgiFormat = e.DxgiFormat, .Width = e.Width, .Height = e.Height,
                .MipCount = e.MipCount, .Faces = If(e.IsCubemap, 6, Math.Max(1, e.Faces)), .IsCubemap = e.IsCubemap
            }
        End If

        ' Falta metadata (vino solo con bytes): se parsea el DDS ACA, en una entrada nueva. La ruta la
        ' pone la FILA, no el parseo: es la que el usuario puede haber renombrado en la grilla.
        Dim ve2 = Dx10Importer.FromDdsBytes(e.Data, e.FullPath)
        If ve2.Width <= 0 OrElse ve2.Height <= 0 OrElse ve2.MipCount <= 0 OrElse ve2.DxgiFormat < 0 Then
            Throw New InvalidDataException($"Missing DX10 metadada in '{e.FullPath}'.")
        End If
        Return New VirtualEntry With {
            .Directory = e.Directory, .FileName = e.FileName, .Data = ve2.Data,
            .DxgiFormat = ve2.DxgiFormat, .Width = ve2.Width, .Height = ve2.Height,
            .MipCount = ve2.MipCount, .Faces = If(ve2.IsCubemap, 6, Math.Max(1, ve2.Faces)), .IsCubemap = ve2.IsCubemap
        }
    End Function

    ''' <summary>Las entradas de un BA2 GNRL: el archivo completo, opaco.</summary>
    Friend Shared Function EntradasGnrl(filas As IEnumerable(Of EntryView)) As List(Of VirtualEntry)
        Dim ves As New List(Of VirtualEntry)()
        For Each e In filas
            ves.Add(New VirtualEntry With {.Directory = e.Directory, .FileName = e.FileName, .Data = e.Data})
        Next
        Return ves
    End Function

    ''' <summary>Las entradas de un BSA (Skyrim SE): archivo completo y compresion por entrada.</summary>
    Friend Shared Function EntradasBsa(filas As IEnumerable(Of EntryView)) As List(Of VirtualEntry)
        Dim ves As New List(Of VirtualEntry)()
        For Each e In filas
            ves.Add(New VirtualEntry With {.Directory = e.Directory, .FileName = e.FileName, .Data = e.Data, .PreferCompress = True})
        Next
        Return ves
    End Function

    ' =================================================================================================
    ' ESCRITURA
    ' =================================================================================================

    ''' <summary>⛔ LAS TRES ESCRITURAS VAN POR <c>EscrituraEnElLugar.GuardarConCopia</c>, Y ESTO NO ES
    ''' UNA PREFERENCIA. Abrian con <c>FileMode.Create</c>, que es CREATE_ALWAYS: <b>trunca AL ABRIR</b>.
    ''' Cualquier excepcion a mitad —disco lleno, el archivo tomado, un .dds corrupto— dejaba el archive
    ''' ANTERIOR del usuario en 0 bytes, sin copia y sin nada para volver. Y el caso probable no es
    ''' teorico: el split de FO4 escribe DOS .ba2 seguidos, asi que un disco que se llena en el segundo
    ''' se llevaba puestos los dos.
    ''' <para><c>GuardarConCopia</c> deja primero una copia VERIFICADA de lo que va a pisar, no escribe si
    ''' no la pudo dejar («SIN RED NO SE TRUNCA»), restaura el destino si la escritura muere despues de
    ''' empezar, y borra la copia al salir bien. Un archive es dato del usuario que la app NO puede
    ''' regenerar —adentro puede haber lo unico que queda de una textura editada—, asi que es el camino
    ''' con red y no el de <c>Escribir</c>.</para>
    ''' <para>⛔ CONTRATO DEL CUERPO: escribe en el stream y NO LO CIERRA (el dueño del stream es
    ''' <c>EscrituraEnElLugar</c>: lo abre, lo trunca, lo sincroniza y lo cierra). Los tres writers de la
    ''' libreria escriben directo sobre el Stream que reciben y no lo envuelven, asi que cumplen; si
    ''' alguno lo envolviera en un <c>BinaryWriter</c> sin <c>leaveOpen:=True</c>, la guarda
    ''' <c>ContratoDelCuerpoException</c> lo dice con nombre y archivo en vez de un ObjectDisposed
    ''' criptico.</para>
    ''' <para>⛔ LO QUE CUESTA, MEDIDO Y NO ARGUMENTADO. La red no es gratis y un archive no es un .esp:
    ''' antes de escribir se COPIA el archive anterior entero y la copia se sincroniza. Medido en este
    ''' arbol sobre un archivo de 300 MB (disco D:, temp): <c>File.Copy</c> 117 ms + <c>Flush(True)</c>
    ''' de la copia 2.059 ms = <b>~2,2 s por cada 300 MB del archive ANTERIOR</b>; y el archive NUEVO
    ''' ahora tambien se sincroniza (mismo orden: ~2 s por 300 MB), cosa que con el <c>FileMode.Create</c>
    ''' pelado no pasaba. Transitoriamente hace falta el DOBLE de espacio en disco. Sobre un BA2 de 2 GB
    ''' eso es decenas de segundos por guardado. Es el precio de que un corte no deje al usuario sin el
    ''' archive; si algun dia no se puede pagar, lo que corresponde NO es volver a truncar sin red sino el
    ''' protocolo `.new`+sello del packer (<c>ArchivePackager</c>, gate <c>Tools\PackRecuperacionGate</c>),
    ''' que da recuperacion sin copiar el anterior. Queda dicho, no elegido: los bytes los decide el
    ''' usuario.</para>
    ''' <para>⛔ ESTAS TRES son la puerta de UN archive. El guardado que escribe DOS —el split de FO4— NO
    ''' las encadena: va por <see cref="GuardarFo4Split"/>, que usa el LOTE, porque encadenarlas es
    ''' transaccional por archivo y deja cosechas mixtas. Ver ahi el por que.</para></summary>
    ''' <summary>El cuerpo de escritura de un BA2 DX10. Es un <c>Action(Of Stream)</c> porque lo consumen
    ''' las DOS puertas: <see cref="EscribirDx10"/> (un archive solo) y <see cref="GuardarFo4Split"/> (una
    ''' etapa del lote). Una sola forma de escribir, no dos.</summary>
    Private Shared Function CuerpoDx10(ves As List(Of VirtualEntry), opts As Ba2WriterDX10.Options,
                                       progreso As Action) As Action(Of Stream)
        Return Sub(fs)
                   _progreso = progreso
                   AddHandler Ba2WriterDX10.Writed, AddressOf AlEscribirUno
                   Try
                       Ba2WriterDX10.Write(fs, ves, opts)
                   Finally
                       RemoveHandler Ba2WriterDX10.Writed, AddressOf AlEscribirUno
                       _progreso = Nothing
                   End Try
               End Sub
    End Function

    Private Shared Function CuerpoGnrl(ves As List(Of VirtualEntry), opts As Ba2WriterGNRL.Options,
                                       progreso As Action) As Action(Of Stream)
        Return Sub(fs)
                   _progreso = progreso
                   AddHandler Ba2WriterGNRL.Writed, AddressOf AlEscribirUno
                   Try
                       Ba2WriterGNRL.Write(fs, ves, opts)
                   Finally
                       RemoveHandler Ba2WriterGNRL.Writed, AddressOf AlEscribirUno
                       _progreso = Nothing
                   End Try
               End Sub
    End Function

    Private Shared Function CuerpoBsa(ves As List(Of VirtualEntry), opts As BsaWriter.Options,
                                      progreso As Action) As Action(Of Stream)
        Return Sub(fs)
                   _progreso = progreso
                   AddHandler BsaWriter.Writed, AddressOf AlEscribirUno
                   Try
                       BsaWriter.Write(fs, ves, opts)
                   Finally
                       RemoveHandler BsaWriter.Writed, AddressOf AlEscribirUno
                       _progreso = Nothing
                   End Try
               End Sub
    End Function

    Friend Shared Sub EscribirDx10(destino As String, ves As List(Of VirtualEntry),
                                   opts As Ba2WriterDX10.Options, progreso As Action)
        EscrituraEnElLugar.GuardarConCopia(destino, CuerpoDx10(ves, opts, progreso))
    End Sub

    Friend Shared Sub EscribirGnrl(destino As String, ves As List(Of VirtualEntry),
                                   opts As Ba2WriterGNRL.Options, progreso As Action)
        EscrituraEnElLugar.GuardarConCopia(destino, CuerpoGnrl(ves, opts, progreso))
    End Sub

    Friend Shared Sub EscribirBsa(destino As String, ves As List(Of VirtualEntry),
                                  opts As BsaWriter.Options, progreso As Action)
        EscrituraEnElLugar.GuardarConCopia(destino, CuerpoBsa(ves, opts, progreso))
    End Sub

    ''' <summary>El guardado FO4 que se PARTE en dos archives: BA2 no deja mezclar texturas (.dds) con
    ''' archivos generales, asi que UNA accion del usuario escribe DOS.
    ''' <para>⛔ LOS DOS SON UNA SOLA UNIDAD, Y POR ESO VAN EN UN LOTE. Encadenar dos
    ''' <c>GuardarConCopia</c> es transaccional POR ARCHIVO: un fallo en la segunda etapa deja la primera
    ''' escrita y CONFIRMADA (su copia ya se borro), o sea el usuario con <b>`- Textures.ba2` nuevo y
    ''' `- Main.ba2` viejo</b> — dos archives validos de distinta cosecha, y el `Main` referencia mallas
    ''' cuyas texturas ya no son las que estan en el `Textures`. Es el mismo daño que el docstring de
    ''' <c>EscrituraEnElLugar.NuevoLote</c> documenta para el proyecto de Wardrobe Manager («un .osp
    ''' apuntando a un .osd nuevo con un .nif viejo») y para el que existe el lote: las copias no se borran
    ''' al exito de cada etapa sino al <c>Confirmar</c>, asi que un fallo en la etapa 2 devuelve las DOS.</para>
    ''' <para>⚠️ LO QUE EL LOTE NO DA, y su docstring lo dice: NO es atomico. Cubre la EXCEPCION, no el
    ''' corte de luz. Si el proceso muere entre etapas, los archives quedan a medias y las copias
    ''' `.npcm.prev` quedan EN DISCO al lado — que es mejor que no quedar, pero nadie las restaura solo.
    ''' Quien quiera cerrar ese hueco tiene <c>EscrituraEnElLugar.CopiasPendientes</c>, que las NOMBRA
    ''' cuando el usuario vuelve a abrir; hoy la UI no lo consume. Queda dicho, no escondido.</para>
    ''' <para>El STAGING de las dos listas se hace ANTES de abrir el lote: un .dds corrupto tiene que hacer
    ''' fallar el guardado sin que se haya tocado un solo archive (ver H2 y el caso H1h del gate).</para></summary>
    Friend Shared Sub GuardarFo4Split(texPath As String, genPath As String,
                                      filasTex As IEnumerable(Of EntryView), filasGen As IEnumerable(Of EntryView),
                                      optDx As Ba2WriterDX10.Options, optGnrl As Ba2WriterGNRL.Options,
                                      progreso As Action)
        Dim vesTex = EntradasDx10(filasTex)
        Dim vesGen = EntradasGnrl(filasGen)

        ' El `Using` cubre el caso que `Guardar` no cubre: que el llamador aborte entre etapas. Si el lote
        ' no se confirmo, el Dispose deshace lo escrito (y NUNCA tira: taparia la causa original).
        Using lote = EscrituraEnElLugar.NuevoLote()
            lote.Guardar(texPath, CuerpoDx10(vesTex, optDx, progreso))
            lote.Guardar(genPath, CuerpoGnrl(vesGen, optGnrl, progreso))
            lote.Confirmar()
        End Using
    End Sub

    ' =================================================================================================
    ' EXTRACCION
    ' =================================================================================================

    ''' <summary>Los bytes que se escriben al disco al extraer una fila.
    ''' <para>⛔ UNA FILA DX10 NO ES UN ARCHIVO: es el payload SIN cabecera DDS. El BA2 DX10 no guarda la
    ''' cabecera —la reconstruye el reader desde la metadata al abrir— y la app le quita la que trae el
    ''' reader para poder volver a empaquetar. Escribir ese Data crudo, que es lo que se hacia, producia
    ''' un .dds sin cabecera en CADA extraccion de CADA BA2 DX10: ilegible para el juego y para cualquier
    ''' visor, y reportado como "Extracted N file(s)". Medido contra el Fallout 4 instalado: -128 B
    ''' exactos por archivo y `GetDdsMetadata` devolviendo Loaded=False.</para>
    ''' <para>⛔ LO QUE DECIDE ES LA METADATA, NO LA EXTENSION. Un .dds que vive adentro de un BA2 GNRL o
    ''' de un BSA se guarda COMPLETO y su fila tiene Width/MipCount en 0: a ese hay que escribirlo
    ''' VERBATIM, porque anteponerle una segunda cabecera lo destruye. Es el mismo criterio que usan el
    ''' importador y el writer. Testigos H3c/H3d de <c>Tools\Ba2ManagerSaveGate</c>.</para>
    ''' <para>La reconstruccion sale de <c>Dx10Importer.ToDdsBytes</c>, que llama al MISMO encode que usa
    ''' el reader: abrir y extraer da byte a byte lo que da <c>BethesdaReader.ExtractToMemory</c>.</para></summary>
    Friend Shared Function BytesParaExtraer(fila As EntryView) As Byte()
        If fila Is Nothing Then Return Nothing
        If fila.Data Is Nothing Then Return Nothing
        If fila.Width > 0 AndAlso fila.MipCount > 0 Then
            Return Dx10Importer.ToDdsBytes(fila.Data, fila.DxgiFormat, fila.Width, fila.Height,
                                           fila.MipCount, fila.IsCubemap)
        End If
        Return fila.Data
    End Function

    ''' <summary>Extrae filas a <paramref name="baseDir"/> respetando su ruta relativa. Devuelve cuantas
    ''' escribio. Siempre desde <c>EntryView.Data</c> en memoria (nunca releyendo del archive en disco):
    ''' refleja ediciones no guardadas y evita I/O redundante.
    ''' <para>⛔ EL SUELTO SE ESCRIBE POR <c>EscrituraEnElLugar.Escribir</c>, NO CON
    ''' <c>File.WriteAllBytes</c>. `WriteAllBytes` pide CREATE_ALWAYS, y CREATE_ALWAYS sobre un archivo
    ''' OCULTO da ACCESS_DENIED — el atributo lo dejan OneDrive y los desempaquetadores, asi que el caso es
    ''' comun: extraer una textura a la carpeta donde el usuario ya la tenia fallaba y cortaba la
    ''' extraccion ahi, con las filas siguientes sin escribir. Medido en el caso P0 de
    ''' <c>Tools\Ba2ManagerSaveGate</c>, que remide la tabla en cada corrida en vez de dejarla escrita en
    ''' un comentario que envejece; es la misma migracion que la libreria ya hizo en su desempaquetado a
    ''' sueltos (<c>Tools\UnpackSueltosGate</c>, U0/U1).</para>
    ''' <para>Es <c>Escribir</c> y NO <c>GuardarConCopia</c> porque lo extraido es salida REGENERABLE: sale
    ''' del archive que sigue abierto en la pestaña, y son cientos de archivos por corrida. La copia con
    ''' red es para el dato que la app no puede rehacer.</para>
    ''' <para>⚠️ NO CUBIERTO Y DICHO: el destino de SOLO LECTURA sigue fallando — no lo escribe NINGUNA de
    ''' las dos primitivas (misma fila P0, misma no-cobertura que declara UnpackSueltosGate). Lo que si
    ''' esta garantizado es que el archivo del usuario queda INTACTO: <c>EscribirNucleo</c> falla al ABRIR,
    ''' antes de truncar (caso P2).</para></summary>
    Friend Shared Function ExtraerAlDisco(baseDir As String, filas As IEnumerable(Of EntryView),
                                          progreso As Action(Of Integer, Integer)) As Integer
        Dim count As Integer = 0
        Dim max As Integer = filas.Count()
        Dim val As Integer = 0
        For Each ev In filas
            val += 1
            If progreso IsNot Nothing Then progreso(val, max)
            If ev Is Nothing Then Continue For
            Dim rel As String = PathUtil.JoinDirFile(ev.Directory, ev.FileName)
            Dim bytes As Byte() = BytesParaExtraer(ev)
            If bytes Is Nothing Then Continue For

            Dim relOs As String = rel.Replace(InCorrect_Path_separator, Path.DirectorySeparatorChar).Replace(Correct_Path_separator, Path.DirectorySeparatorChar)
            Dim outPath As String = Path.Combine(baseDir, relOs)
            ' La carpeta la crea EscribirNucleo (Directory.CreateDirectory del destino), asi que no se
            ' repite aca: dos lugares creando el mismo arbol es como se desincronizan.
            EscrituraEnElLugar.Escribir(outPath, Sub(fs) fs.Write(bytes, 0, bytes.Length))
            count += 1
        Next
        Return count
    End Function

End Class
