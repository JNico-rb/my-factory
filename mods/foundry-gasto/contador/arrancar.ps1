<#
.SYNOPSIS
  Arranca el contador en segundo plano, suelto de quien lo lanza: sigue en marcha aunque se cierre
  Claude Code, para que las aplicaciones que apuntan a él no se queden sin servicio.
#>
param(
  [string]$Dir = (Join-Path $HOME '.claude\foundry-gasto')
)
$ErrorActionPreference = 'Stop'
$pwsh = (Get-Process -Id $PID).Path
$script = Join-Path $PSScriptRoot 'contador.ps1'
Start-Process -FilePath $pwsh -WindowStyle Hidden -ArgumentList @(
  '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', "`"$script`"", '-Dir', "`"$Dir`""
)
