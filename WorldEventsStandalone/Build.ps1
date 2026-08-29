[CmdletBinding()]
param(
    [string]$BannerlordDir
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$projectFile = Join-Path $projectRoot 'Source\WorldEventsStandalone.v1.0.9.csproj'

if ([string]::IsNullOrWhiteSpace($BannerlordDir)) {
    $candidates = @(
        $env:BANNERLORD_DIR,
        (Join-Path $env:ProgramFiles 'Steam\steamapps\common\Mount & Blade II Bannerlord'),
        (Join-Path ${env:ProgramFiles(x86)} 'Steam\steamapps\common\Mount & Blade II Bannerlord')
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    $BannerlordDir = $candidates |
        Where-Object { Test-Path -LiteralPath (Join-Path $_ 'bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.dll') } |
        Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($BannerlordDir)) {
    throw 'Bannerlord was not found. Pass -BannerlordDir or set BANNERLORD_DIR.'
}

dotnet build $projectFile -c Release --nologo "-p:BannerlordDir=$BannerlordDir"
if ($LASTEXITCODE -ne 0) {
    throw "World Events Standalone build failed with exit code $LASTEXITCODE."
}

$output = Join-Path $projectRoot 'Source\bin\Release\net472'
Write-Host "Build succeeded: $output"
