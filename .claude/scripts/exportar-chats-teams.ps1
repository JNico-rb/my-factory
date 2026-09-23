<#
.SYNOPSIS
Exports the Teams chats where the teacher participates to transcripts for the consejos-profesora agent.

.DESCRIPTION
Reads the chats with Microsoft Graph: delegated permission Chat.Read, you sign in yourself in the browser and no
token is stored. Writes one notes/teams/chat-<id>.md per chat, with the authors pseudonymized before
Claude reads them: PROFESORA (whoever matches -Autora), YO (you), P1..Pn (everyone else), APP (bots).
Mentions and emails come out masked; of shared files, only the name.
Each file's name comes from the chat id: it is stable across exports.

Requirement, once:  Install-Module Microsoft.Graph.Authentication -Scope CurrentUser
If the login asks for administrator approval, your organization does not allow Chat.Read: paste the chats by hand
into notes/teams/ (plan B, see .claude/agents/consejos-profesora.md).

.EXAMPLE
powershell -ExecutionPolicy Bypass -File .claude\scripts\exportar-chats-teams.ps1 -Autora "First Last"
#>
param(
    # Part of the teacher's name as it appears in Teams.
    [string]$Autora,
    [string]$Salida = (Join-Path $PSScriptRoot '..\..\notes\teams')
)

$ErrorActionPreference = 'Stop'

function Get-FechaLocal($Valor) {
    if ($Valor -is [datetime]) { return $Valor.ToLocalTime() }
    ([datetimeoffset]::Parse([string]$Valor, [cultureinfo]::InvariantCulture)).LocalDateTime
}

function ConvertTo-Texto([string]$Html) {
    # HTML of a Teams message -> text: links keep their URL and code its block.
    if (-not $Html) { return '' }
    $nl = "`n"
    $fence = '```'
    $t = $Html -replace '(?is)<at\b[^>]*>.*?</at>', '@[mention]'
    $t = $t -replace '(?is)<emoji\b[^>]*\balt="([^"]*)"[^>]*>(\s*</emoji>)?', '$1'
    $t = $t -replace '(?is)<img\b[^>]*>', '[image]'
    $t = $t -replace '(?is)<attachment\b[^>]*>\s*</attachment>', ''
    $t = [regex]::Replace($t, '(?is)<a\b[^>]*\bhref="([^"]*)"[^>]*>(.*?)</a>', [Text.RegularExpressions.MatchEvaluator]{
        param($m)
        $url = [Net.WebUtility]::HtmlDecode($m.Groups[1].Value)
        $txt = [Net.WebUtility]::HtmlDecode(($m.Groups[2].Value -replace '<[^>]+>', '')).Trim()
        if (-not $txt -or $txt -eq $url) { $url } else { "$txt ($url)" }
    })
    $t = $t -replace '(?is)<(codeblock|pre)\b[^>]*>\s*(<code\b[^>]*>)?', "$nl$fence$nl"
    $t = $t -replace '(?is)(</code>\s*)?</(codeblock|pre)>', "$nl$fence$nl"
    $t = $t -replace '(?i)</?code\b[^>]*>', '`'
    $t = $t -replace '(?i)<br\s*/?>', $nl
    $t = $t -replace '(?i)</(p|div|li|h[1-6]|blockquote|tr)>', $nl
    $t = $t -replace '(?i)<li\b[^>]*>', '- '
    $t = $t -replace '<[^>]+>', ''
    $t = [Net.WebUtility]::HtmlDecode($t) -replace [char]0x00A0, ' '
    $t = $t -replace '[\w.+-]+@[\w-]+(\.[\w-]+)+', '[email]'
    $t = $t -replace '[ \t]+\r?\n', $nl
    $t = $t -replace '(\r?\n){3,}', "$nl$nl"
    $t.Trim()
}

function Format-Transcripcion {
    # Graph messages (chatMessage) -> chronological Markdown with pseudonymized authors.
    param([object[]]$Mensajes, [string]$Autora, [string]$MiId, [string]$Titulo)

    $seudonimos = @{}   # user id -> PROFESORA / YO / P<n>
    $coinciden = @{}    # names that match -Autora: if there is more than one, -Autora is ambiguous

    function Resolver($Identidad) {
        if (-not $Identidad) { return '?' }
        if ($Identidad.application) { return 'APP' }
        $u = $Identidad.user
        if (-not $u) { return '?' }
        $clave = if ($u.id) { $u.id } else { "nombre:$($u.displayName)" }
        if (-not $seudonimos.ContainsKey($clave)) {
            $seudonimos[$clave] =
                if ($u.id -and $u.id -eq $MiId) { 'YO' }
                elseif ($Autora -and $u.displayName -like "*$Autora*") { $coinciden[$u.displayName] = $true; 'PROFESORA' }
                else { 'P' + (@($seudonimos.Values | Where-Object { $_ -match '^P\d+$' }).Count + 1) }
        }
        $seudonimos[$clave]
    }

    $validos = @($Mensajes |
        Where-Object { $_.messageType -eq 'message' -and -not $_.deletedDateTime } |
        Sort-Object { Get-FechaLocal $_.createdDateTime })

    $bloques = New-Object System.Collections.Generic.List[string]
    foreach ($m in $validos) {
        $citas = @(); $archivos = @()
        foreach ($a in @($m.attachments)) {
            if (-not $a) { continue }
            switch ($a.contentType) {
                'reference' { $archivos += "[shared file: $($a.name)]" }
                'messageReference' {
                    $ref = $a.content | ConvertFrom-Json
                    $previa = (ConvertTo-Texto $ref.messagePreview) -replace '\s+', ' '
                    $citas += "[in reply to $(Resolver $ref.messageSender): `"$previa`"]"
                }
                default { $archivos += "[attachment: $($a.contentType)]" }
            }
        }
        $texto = ConvertTo-Texto $m.body.content
        if (-not $texto -and -not $archivos) { continue }
        $cabecera = '### {0:yyyy-MM-dd HH:mm} - {1}' -f (Get-FechaLocal $m.createdDateTime), (Resolver $m.from)
        $bloques.Add((@($cabecera) + $citas + @($texto) + $archivos | Where-Object { $_ }) -join "`n")
    }

    $rango = if ($validos) {
        '{0:yyyy-MM-dd} -> {1:yyyy-MM-dd}' -f (Get-FechaLocal $validos[0].createdDateTime), (Get-FechaLocal $validos[-1].createdDateTime)
    } else { 'no messages' }
    $cabeceraArchivo = @(
        "# $Titulo`: $($bloques.Count) messages, $rango"
        "Pseudonymized authors: PROFESORA = the teacher, YO = the exporter, P1..Pn = everyone else, APP = bots. Exported on $(Get-Date -Format yyyy-MM-dd)."
    ) -join "`n"

    [pscustomobject]@{
        Texto    = (@($cabeceraArchivo) + $bloques) -join "`n`n"
        Mensajes = $bloques.Count
        Autoras  = @($coinciden.Keys)
    }
}

# Dot-sourced (tests): only the functions.
if ($MyInvocation.InvocationName -eq '.') { return }

if (-not $Autora) { throw 'Missing -Autora: part of the teacher''s name as it appears in Teams.' }
if (-not (Get-Module -ListAvailable Microsoft.Graph.Authentication)) {
    throw 'Missing the Graph module. Install it once: Install-Module Microsoft.Graph.Authentication -Scope CurrentUser'
}
Import-Module Microsoft.Graph.Authentication
try { Connect-MgGraph -Scopes 'Chat.Read', 'User.Read' -NoWelcome }
catch { throw "Could not sign in: $($_.Exception.Message). If it asks for administrator approval, use plan B: paste the chats by hand into notes/teams/." }

function Get-Coleccion([string]$Uri) {
    # Follows @odata.nextLink to the end of the collection.
    while ($Uri) {
        $r = Invoke-MgGraphRequest -Method GET -Uri $Uri -OutputType PSObject
        $r.value
        $Uri = $r.'@odata.nextLink'
    }
}

$miId = (Invoke-MgGraphRequest -Method GET -Uri 'v1.0/me?$select=id' -OutputType PSObject).id
$chats = @(Get-Coleccion 'v1.0/me/chats?$expand=members&$top=50' |
    Where-Object { $_.members | Where-Object { $_.displayName -like "*$Autora*" } } |
    Sort-Object { Get-FechaLocal $_.lastUpdatedDateTime } -Descending)
if (-not $chats) { throw "None of your chats has a member matching '$Autora'." }

for ($i = 0; $i -lt $chats.Count; $i++) {
    $c = $chats[$i]
    $titulo = if ($c.topic) { $c.topic } else { '(untitled)' }
    Write-Host ('[{0}] {1} | {2} | {3} members | updated {4:yyyy-MM-dd}' -f ($i + 1), $c.chatType, $titulo, @($c.members).Count, (Get-FechaLocal $c.lastUpdatedDateTime))
}
$eleccion = Read-Host 'Numbers of the chats to export, comma-separated (Enter = all)'
$elegidos = if (-not $eleccion) { $chats } else {
    foreach ($parte in $eleccion -split ',') {
        $k = [int]$parte.Trim()
        if ($k -lt 1 -or $k -gt $chats.Count) { throw "Number not in the list: $k" }
        $chats[$k - 1]
    }
}

$Salida = [IO.Path]::GetFullPath($ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Salida))
New-Item -ItemType Directory -Force -Path $Salida | Out-Null
$utf8 = New-Object System.Text.UTF8Encoding $false
$sha = [Security.Cryptography.SHA256]::Create()
$autoras = @{}

foreach ($c in $elegidos) {
    $hash = -join ($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($c.id))[0..2] | ForEach-Object { $_.ToString('x2') })
    $archivo = "chat-$hash.md"
    $mensajes = @(Get-Coleccion "v1.0/chats/$($c.id)/messages?`$top=50")
    $r = Format-Transcripcion -Mensajes $mensajes -Autora $Autora -MiId $miId -Titulo "$archivo ($($c.chatType))"
    [IO.File]::WriteAllText((Join-Path $Salida $archivo), $r.Texto, $utf8)
    $r.Autoras | ForEach-Object { $autoras[$_] = $true }
    $titulo = if ($c.topic) { $c.topic } else { '(untitled)' }
    Write-Host "$archivo <- $titulo : $($r.Mensajes) messages"
}

if ($autoras.Count -gt 1) {
    Write-Warning "-Autora matches several people ($($autoras.Keys -join ', ')): all of them come out as PROFESORA. Repeat with a more exact name."
}
if ($autoras.Count -eq 0) {
    Write-Warning "No one matching '$Autora' has written in the chosen chats: there are no PROFESORA messages."
}
Disconnect-MgGraph | Out-Null
Write-Host "Done in $Salida. Now ask Claude: 'extract the teacher's tips'."
