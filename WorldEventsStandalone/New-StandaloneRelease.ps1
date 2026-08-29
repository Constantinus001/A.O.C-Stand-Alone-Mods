[CmdletBinding()]
param(
    [string]$BannerlordDir
)

$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Verify.ps1')
& (Join-Path $PSScriptRoot 'Build.ps1') -BannerlordDir $BannerlordDir

$artifacts = Join-Path $PSScriptRoot 'artifacts'
$stage = Join-Path $artifacts 'stage'
$stageModule = Join-Path $stage 'WorldEventsStandalone'
New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
if (Test-Path -LiteralPath $stage) {
    Remove-Item -LiteralPath $stage -Recurse -Force
}
New-Item -ItemType Directory -Path $stage -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Module\WorldEventsStandalone') -Destination $stage -Recurse

$buildOutput = Join-Path $PSScriptRoot 'Source\bin\Release\net472'
Copy-Item -LiteralPath (Join-Path $buildOutput 'WorldEventsStandalone.v1.0.9.dll') -Destination (Join-Path $stageModule 'bin\Win64_Shipping_Client\WorldEventsStandalone.v1.0.9.dll') -Force
$builtPdb = Join-Path $buildOutput 'WorldEventsStandalone.v1.0.9.pdb'
if (Test-Path -LiteralPath $builtPdb) {
    Copy-Item -LiteralPath $builtPdb -Destination (Join-Path $stageModule 'bin\Win64_Shipping_Client\WorldEventsStandalone.v1.0.9.pdb') -Force
}

& (Join-Path $PSScriptRoot 'Verify.ps1') -ModuleRoot $stageModule -SkipRecordedHashes

$zip = Join-Path $artifacts 'WorldEventsStandalone-v1.0.9.zip'
if (Test-Path -LiteralPath $zip) {
    Remove-Item -LiteralPath $zip -Force
}
Compress-Archive -LiteralPath $stageModule -DestinationPath $zip -CompressionLevel Optimal
Write-Host "Release created: $zip"
