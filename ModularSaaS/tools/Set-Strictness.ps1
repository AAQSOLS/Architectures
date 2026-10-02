<#
.SYNOPSIS
  Switch the analyzer strictness of the whole solution.
  Migration: .editorconfig severities `error` -> `warning`, TreatWarningsAsErrors=false (legacy code still builds).
  Strict   : .editorconfig == .editorconfig.strict as delivered, TreatWarningsAsErrors=true.
.EXAMPLE
  pwsh tools/Set-Strictness.ps1 -Mode Migration
  pwsh tools/Set-Strictness.ps1 -Mode Strict
#>
param([Parameter(Mandatory)][ValidateSet('Migration', 'Strict')][string]$Mode)

$root   = Split-Path -Parent $PSScriptRoot
$strict = Join-Path $root '.editorconfig.strict'
$target = Join-Path $root '.editorconfig'
if (-not (Test-Path $strict)) { throw ".editorconfig.strict not found at $strict" }

$lines = Get-Content $strict | ForEach-Object {
    if ($Mode -eq 'Migration' -and $_ -notmatch '^\s*#') {
        $_ -replace '(=\s*)error\s*$', '${1}warning' -replace ':error\s*$', ':warning'
    } else { $_ }
}
[System.IO.File]::WriteAllLines($target, $lines, (New-Object System.Text.UTF8Encoding($false)))

$props = Join-Path $root 'Directory.Build.props'
if (Test-Path $props) {
    $value = if ($Mode -eq 'Strict') { 'true' } else { 'false' }
    $xml = Get-Content $props -Raw
    if ($xml -match '<TreatWarningsAsErrors>') {
        $xml = $xml -replace '<TreatWarningsAsErrors>.*?</TreatWarningsAsErrors>', "<TreatWarningsAsErrors>$value</TreatWarningsAsErrors>"
        [System.IO.File]::WriteAllText($props, $xml, (New-Object System.Text.UTF8Encoding($false)))
    }
}
Write-Host "Strictness set to $Mode"
