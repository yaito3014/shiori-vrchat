<#
.SYNOPSIS
  Adds this package to a dev project created by shiori/Tools~/New-DevProject.ps1, as a file:
  reference next to the core, and lists it in "testables" so its tests run with the core's.

.PARAMETER ProjectPath
  The dev project. Default: <repo parent>/shiori-dev/<Stream>.

.PARAMETER Stream
  Unity stream folder under shiori-dev, e.g. 2022.3 or 6000.6. Ignored when ProjectPath is given.

.EXAMPLE
  pwsh Tools~/Add-ToDevProject.ps1 -Stream 2022.3
  pwsh Tools~/Add-ToDevProject.ps1 -ProjectPath ../shiori-dev/2022.3-batch
#>
param(
    [string]$ProjectPath = "",
    [string]$Stream = ""
)

$ErrorActionPreference = "Stop"

$repo = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if ($ProjectPath -eq "") {
    if ($Stream -eq "") { throw "Give -ProjectPath or -Stream" }
    $ProjectPath = Join-Path (Join-Path (Split-Path $repo -Parent) "shiori-dev") $Stream
}
if (-not (Test-Path $ProjectPath)) { throw "Dev project not found: $ProjectPath" }
$ProjectPath = (Resolve-Path $ProjectPath).Path
$packagesDir = Join-Path $ProjectPath "Packages"
$manifestPath = Join-Path $packagesDir "manifest.json"
if (-not (Test-Path $manifestPath)) { throw "manifest.json not found in $packagesDir" }

$packageName = "com.yaito3014.shiori.vrchat"
$relative = [System.IO.Path]::GetRelativePath($packagesDir, $repo)
$reference = "file:" + ($relative -replace "\\", "/")

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
if (-not ($manifest.PSObject.Properties.Name -contains "dependencies")) {
    $manifest | Add-Member -NotePropertyName dependencies -NotePropertyValue ([pscustomobject]@{})
}
if ($manifest.dependencies.PSObject.Properties.Name -contains $packageName) {
    $manifest.dependencies.$packageName = $reference
} else {
    $manifest.dependencies | Add-Member -NotePropertyName $packageName -NotePropertyValue $reference
}

$testables = @()
if ($manifest.PSObject.Properties.Name -contains "testables") { $testables = @($manifest.testables) }
if ($testables -notcontains $packageName) { $testables += $packageName }
if ($manifest.PSObject.Properties.Name -contains "testables") {
    $manifest.testables = $testables
} else {
    $manifest | Add-Member -NotePropertyName testables -NotePropertyValue $testables
}

$json = ($manifest | ConvertTo-Json -Depth 5).Replace("`r`n", "`n")
[IO.File]::WriteAllText($manifestPath, $json + "`n", [Text.UTF8Encoding]::new($false))

Write-Host "Added $packageName to $manifestPath"
Write-Host "Package reference:   $reference"
