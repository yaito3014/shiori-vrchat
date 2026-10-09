<#
.SYNOPSIS
  Adds this package to a dev project created by shiori/Tools~/New-DevProject.ps1 and lists it in
  "testables" so its tests run with the core's.

.PARAMETER ProjectPath
  The dev project. Default: <repo parent>/shiori-dev/<Stream>.

.PARAMETER Stream
  Unity stream folder under shiori-dev, e.g. 2022.3 or 6000.6. Ignored when ProjectPath is given.

.PARAMETER Embed
  Copy the tracked package files into <project>/Packages/com.yaito3014.shiori.vrchat instead of
  adding a "file:" reference. Used by CI, where the project lives inside the repository and the
  core is embedded the same way (see New-DevProject.ps1 -Embed).

.EXAMPLE
  pwsh Tools~/Add-ToDevProject.ps1 -Stream 2022.3
  pwsh Tools~/Add-ToDevProject.ps1 -ProjectPath ci-project~/2022.3.22f1 -Embed
#>
param(
    [string]$ProjectPath = "",
    [string]$Stream = "",
    [switch]$Embed
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

$packageName = (Get-Content (Join-Path $repo "package.json") -Raw | ConvertFrom-Json).name
$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
if (-not ($manifest.PSObject.Properties.Name -contains "dependencies")) {
    $manifest | Add-Member -NotePropertyName dependencies -NotePropertyValue ([pscustomobject]@{})
}

if ($Embed) {
    $target = Join-Path $packagesDir $packageName
    New-Item -ItemType Directory -Force $target | Out-Null
    $files = & git -C $repo ls-files
    if ($LASTEXITCODE -ne 0 -or -not $files) { throw "git ls-files failed; the embed option needs a git checkout" }
    $count = 0
    foreach ($relative in $files) {
        if ($relative -like ".github/*" -or $relative -like "Tools~/*") { continue }
        $src = Join-Path $repo $relative
        if (-not (Test-Path $src -PathType Leaf)) { continue }
        $dst = Join-Path $target $relative
        New-Item -ItemType Directory -Force (Split-Path $dst -Parent) | Out-Null
        Copy-Item $src $dst
        $count++
    }
    # An embedded package must not also be a dependency entry.
    if ($manifest.dependencies.PSObject.Properties.Name -contains $packageName) {
        $manifest.dependencies.PSObject.Properties.Remove($packageName)
    }
    $reference = "embedded ($count files copied to Packages/$packageName)"
} else {
    $relative = [System.IO.Path]::GetRelativePath($packagesDir, $repo)
    $reference = "file:" + ($relative -replace "\\", "/")
    if ($manifest.dependencies.PSObject.Properties.Name -contains $packageName) {
        $manifest.dependencies.$packageName = $reference
    } else {
        $manifest.dependencies | Add-Member -NotePropertyName $packageName -NotePropertyValue $reference
    }
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
