# Gestor de Expedientes

Versión actual: **1.03**

Aplicación de escritorio local para Windows 11 que reúne cuatro tipos de documento en un único PDF, siempre en este orden:

1. Orden de servicio
2. Orden de trabajo
3. Cotización (uno o varios archivos)
4. Reporte de mantenimiento

## Stack

- C# y .NET 10 LTS
- WinUI 3 / Windows App SDK 2.2
- CommunityToolkit.Mvvm
- SQLite y Entity Framework Core 10
- PDFsharp 6.2
- `Windows.Devices.Scanners` sobre controladores WIA 2.0
- xUnit

## Estructura

```text
src/
├── DocumentManager.Core/            Modelos, reglas y contratos
├── DocumentManager.Infrastructure/  EF Core, SQLite, PDF y archivos
└── DocumentManager.WinUI/           XAML, ViewModels y servicios Windows
tests/
├── DocumentManager.Tests/           Pruebas de negocio e integración
└── DocumentManager.WindowsCodeCheck/Validación cruzada del adaptador WIA
scripts/
├── publish-win-x64.ps1              Prueba y genera el EXE en Windows
└── verify-portable.sh               Verificación portable en Linux
```

## Almacenamiento

De forma predeterminada, la aplicación crea:

```text
Documentos/GestorExpedientes/
├── Database/app.db
├── Temp/
├── Expedientes/
└── settings.json
```

SQLite solo almacena la fecha, los dos folios y la ruta final. Los PDF nunca se guardan como BLOB. La carpeta de salida de expedientes se puede cambiar desde Configuración; la base de datos y los temporales permanecen en la ubicación local predecible.

El número económico se captura al crear el expediente y se utiliza, junto con el folio de la orden de servicio, para generar nombres como:

```text
REPORTE MANTENIMIENTO EXTERNO123 OS-5812.pdf
```

El folio interno mostrado es una vista previa. La secuencia de SQLite solamente avanza cuando el PDF ya fue generado y el registro se guarda correctamente.

La sección Cotización permite seleccionar varios PDF a la vez, agregar más archivos o anexar escaneos adicionales. Todas las cotizaciones se incorporan consecutivamente, conservando su orden de selección, antes del reporte de mantenimiento.

## Abrir en Visual Studio

Requisitos en Windows 11:

1. Visual Studio 2026 con la carga de trabajo **Desarrollo de aplicaciones de Windows**.
2. SDK .NET 10.
3. Windows SDK 10.0.26100 o posterior.
4. Modo de desarrollador de Windows habilitado para depuración.

Abre `DocumentManager.slnx`, establece `DocumentManager.WinUI` como proyecto de inicio y selecciona `x64`.

## Generar el EXE

Desde PowerShell en Windows:

```powershell
.\scripts\publish-win-x64.ps1
```

Salida esperada:

```text
artifacts/win-x64/GestorExpedientes.exe
```

Es una publicación WinUI 3 sin MSIX, autocontenida y de archivo único. En el primer arranque, Windows extrae internamente sus dependencias a una carpeta temporal, que es el comportamiento soportado por Windows App SDK para esta modalidad.

## Escáneres

La aplicación enumera equipos instalados con controlador WIA, permite escoger equipo, cama plana o alimentador, y solicita PDF, PNG, JPEG o DIB en ese orden de preferencia según lo que admita el dispositivo. El resultado se normaliza a PDF y se asigna automáticamente a la tarjeta desde la que comenzó el escaneo.

Los equipos que solo exponen TWAIN y no tienen controlador WIA no aparecerán. `IScannerService` mantiene esta dependencia aislada para poder añadir posteriormente un adaptador TWAIN específico sin cambiar ViewModels, PDF ni persistencia.

## Verificación en Linux

WinUI no puede compilarse por completo en Linux porque su compilador XAML es un ejecutable Windows. Sí pueden compilarse y probarse el dominio, EF Core, SQLite, PDFsharp, los ViewModels y las firmas de `Windows.Devices.Scanners`:

```bash
DOTNET_COMMAND=/ruta/a/dotnet ./scripts/verify-portable.sh
```

La compilación, ejecución visual, prueba con escáner físico y publicación del `.exe` deben realizarse finalmente en Windows 11.
