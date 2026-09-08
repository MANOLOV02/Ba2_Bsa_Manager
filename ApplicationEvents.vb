Imports Microsoft.VisualBasic.ApplicationServices

Namespace My
    ' The following events are available for MyApplication:
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.

    ' **NEW** ApplyApplicationDefaults: Raised when the application queries default values to be set for the application.

    ' Example:
    ' Private Sub MyApplication_ApplyApplicationDefaults(sender As Object, e As ApplyApplicationDefaultsEventArgs) Handles Me.ApplyApplicationDefaults
    '
    '   ' Setting the application-wide default Font:
    '   e.Font = New Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular)
    '
    '   ' Setting the HighDpiMode for the Application:
    '   e.HighDpiMode = HighDpiMode.PerMonitorV2
    '
    '   ' If a splash dialog is used, this sets the minimum display time:
    '   e.MinimumSplashScreenDisplayTime = 4000
    ' End Sub

    Partial Friend Class MyApplication

        ''' <summary>⛔ ACÁ NO SE TOCA NADA DE NINGÚN DLL PROPIO: este cuerpo corre antes de que exista la
        ''' ventana principal y el JIT resuelve sus referencias antes de la primera línea. Lo único que hay es
        ''' el chequeo de instalación, que se compila DENTRO de este exe (fuente linkeada, ver el .vbproj).
        ''' <para>Este exe se lleva <c>BSA_BA2_Library_DLL</c>, que es de OTRO repo y se versiona por separado
        ''' — y que además comparte con Nif Explorer. Actualizar una sola de las dos herramientas es
        ''' exactamente la instalación mezclada que esto ataja. Ver <c>Shared\VersionGate.vb</c>.</para></summary>
        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
            If Not VersionGate.VerificarInstalacion() Then
                Environment.ExitCode = 1
                e.Cancel = True
            End If
        End Sub

    End Class
End Namespace
