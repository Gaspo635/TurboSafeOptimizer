# TurboSafe Optimizer

Optimizador de Windows orientado a gaming, diagnóstico y cambios reversibles.

## Regla principal
**NO MEJORÓ = NO SE QUEDA.**

TurboSafe mide el estado antes de un cambio, aplica cambios conservadores y reversibles, vuelve a medir y conserva el cambio solo si el resultado no empeora el perfil seleccionado.

## Estado
- v0.4.0 — base nativa de Windows + CI de compilación.
- El instalador final debe probarse en Windows real antes de considerarse estable.
- No desactiva Defender, Firewall, UAC ni mecanismos de seguridad para obtener rendimiento.

## Compilar localmente
Requiere .NET 8 SDK y Windows:

```powershell
dotnet publish TurboSafeOptimizer/TurboSafeOptimizer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

El ejecutable se genera en `TurboSafeOptimizer/bin/Release/net8.0-windows/win-x64/publish/`.

## Arquitectura
Escanear → Medir → Respaldar → Cambiar → Medir → Conservar/Revertir.

La primera versión nativa es deliberadamente conservadora: inventario y diagnóstico antes de habilitar cambios automáticos.
