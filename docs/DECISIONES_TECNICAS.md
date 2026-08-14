# Decisiones técnicas del MVP

## Dependencias fijadas

| Paquete | Versión | Uso |
|---|---:|---|
| Microsoft.WindowsAppSDK | 2.2.0 | WinUI 3 y runtime de escritorio |
| Microsoft.Windows.SDK.BuildTools | 10.0.26100.8249 | Toolchain Windows SDK |
| CommunityToolkit.Mvvm | 8.4.2 | ObservableObject y RelayCommand |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.10 | Persistencia local |
| SQLitePCLRaw.bundle_e_sqlite3 | 2.1.12 | SQLite nativo corregido |
| PDFsharp | 6.2.4 | Validación, conversión y combinación |

`SQLitePCLRaw.bundle_e_sqlite3` se fija explícitamente en 2.1.12 porque la versión transitiva 2.1.11 reporta una vulnerabilidad de severidad alta.

## Folio interno

`FolioSequences` contiene una única secuencia. La reserva ejecuta:

```sql
UPDATE FolioSequences
SET LastValue = LastValue + 1
WHERE Id = 1
RETURNING LastValue;
```

La sentencia corre dentro de una transacción serializable. No depende del número de registros ni reutiliza folios eliminados. Una reserva abandonada puede dejar un salto, lo cual es preferible a duplicar un identificador. `ServiceRecords.InternalFolio` también tiene un índice único como defensa final.

## Escritura de PDF

PDFsharp escribe primero a un archivo parcial con nombre único. Solo después de guardar correctamente se mueve al nombre final. El PDF final nunca se sobrescribe ni se elimina automáticamente. Los temporales propiedad de la aplicación se eliminan después de guardar el registro; también se limpian al cambiar o quitar un escaneo durante la sesión.

## Límites de la validación en Fedora

- El compilador XAML de WinUI (`XamlCompiler.exe`) solo se ejecuta en Windows.
- WIA requiere Windows y un dispositivo/driver físico.
- La modalidad EXE único se produce con `dotnet publish` en Windows.
- En Fedora se verifican Core, Infrastructure, ViewModels, XML bien formado y compilación de las APIs de escáner contra el contrato Windows 10.0.26100.

