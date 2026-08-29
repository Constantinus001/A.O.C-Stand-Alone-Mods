[CmdletBinding()]
param(
    [string]$ModuleRoot = (Join-Path $PSScriptRoot 'Module\WorldEventsStandalone'),
    [switch]$SkipRecordedHashes
)

$ErrorActionPreference = 'Stop'
$required = @(
    'SubModule.xml',
    'Assets\GauntletUI\ui_world_calendar_1_tex.tpac',
    'AssetSources\GauntletUI\ui_world_calendar_1.png',
    'bin\Win64_Shipping_Client\WorldEventsStandalone.v1.0.9.dll',
    'GUI\WorldEventsStandaloneSpriteData.xml',
    'GUI\Prefabs\WorldEventsStandalone\WorldEventsStandalone.xml'
)

foreach ($relativePath in $required) {
    $path = Join-Path $ModuleRoot $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing required World Events file: $relativePath"
    }
}

$manifest = Get-Content -LiteralPath (Join-Path $ModuleRoot 'SubModule.xml') -Raw
foreach ($value in @('Id value="WorldEventsStandalone"', 'Version value="v1.0.9"', 'DLLName value="WorldEventsStandalone.v1.0.9.dll"', 'TwelveMonthCalendar.WorldEventsStandaloneSubModule')) {
    if (-not $manifest.Contains($value)) {
        throw "SubModule.xml does not contain the required value: $value"
    }
}

$provinceCount = @(Get-ChildItem -LiteralPath (Join-Path $ModuleRoot 'GUI\SpriteParts') -Recurse -File -Filter 'strategic_province_*.png').Count
if ($provinceCount -lt 133) {
    throw "Strategic-map province artwork is incomplete: found $provinceCount, expected at least 133."
}

$sourceRoot = Join-Path $PSScriptRoot 'Source'
$sourceCount = @(
    Get-ChildItem -LiteralPath $sourceRoot -Recurse -File -Filter '*.cs' |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }
).Count
if ($sourceCount -lt 120) {
    throw "Standalone source tree is incomplete: found $sourceCount C# files, expected at least 120."
}

if (-not $SkipRecordedHashes) {
    $checksumFile = Join-Path $PSScriptRoot 'ModuleChecksums.sha256'
    foreach ($line in Get-Content -LiteralPath $checksumFile) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $parts = $line -split '  ', 2
        $actual = (Get-FileHash -LiteralPath (Join-Path $ModuleRoot ($parts[1] -replace '/', '\')) -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -ne $parts[0]) {
            throw "Runtime hash mismatch: $($parts[1])"
        }
    }
}

Write-Host "World Events Standalone verification passed ($provinceCount province images, $sourceCount C# files)."
