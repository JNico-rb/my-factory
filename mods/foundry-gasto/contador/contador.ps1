<#
.SYNOPSIS
  El contador personal de foundry-gasto: un paso intermedio local delante del recurso de Azure AI
  Foundry. Mide lo que gastan las llamadas que pasan por él y las rechaza al llegar a tu límite.
  Lo arranca el mod solo; a mano: pwsh -NoProfile -ExecutionPolicy Bypass -File .\contador.ps1
#>
param(
  # Carpeta de datos: configuración (contador.json), estado (estado.json) y el libro de gasto.
  [string]$Dir = (Join-Path $HOME '.claude\foundry-gasto')
)
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Force -Path $Dir | Out-Null
try {
  Add-Type -Path (Join-Path $PSScriptRoot 'Contador.cs')
  [Contador]::Run($Dir)
}
catch {
  Set-Content -Path (Join-Path $Dir 'contador-error.txt') -Value "$(Get-Date -Format o) $($_.Exception.Message)" -Encoding utf8NoBOM
  throw
}
