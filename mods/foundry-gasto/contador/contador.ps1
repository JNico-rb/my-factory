<#
.SYNOPSIS
  El contador personal de foundry-gasto: vigila qué procesos de este equipo llaman al recurso de
  Azure AI Foundry, estima lo que gastan y, al llegar a tu límite, los para. Se apaga solo cuando el
  mod se desactiva o se desinstala. Lo arranca el mod; a mano:
  pwsh -NoProfile -ExecutionPolicy Bypass -File .\contador.ps1
#>
param(
  # Carpeta de datos: configuración (contador.json), estado (estado.json) y el libro de gasto.
  [string]$Dir = (Join-Path $HOME '.claude\foundry-gasto')
)
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Force -Path $Dir | Out-Null
try {
  Add-Type -Path (Join-Path $PSScriptRoot 'Contador.cs')
  [Contador]::Run($Dir, (Split-Path $PSScriptRoot -Parent))
}
catch {
  Set-Content -Path (Join-Path $Dir 'contador-error.txt') -Value "$(Get-Date -Format o) $($_.Exception.Message)" -Encoding utf8NoBOM
  throw
}
