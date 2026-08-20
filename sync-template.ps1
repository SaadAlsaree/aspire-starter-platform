<#
.SYNOPSIS
    Refreshes the packaged templates from the live solution so what ships is always the code
    that CI actually builds and tests.

    templates/aspire-platform  the whole solution
    templates/aspire-service   the Simble service on its own

.PARAMETER Check
    Fails instead of writing when a template copy is out of date. Used by CI.
#>
param(
    [switch]$Check
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$excludeDirs = @("bin", "obj", ".vs", "TestResults", "Logs")

function Test-Excluded {
    param([string]$Relative)

    foreach ($segment in ($Relative -split '[\\/]')) {
        if ($excludeDirs -contains $segment) { return $true }
    }
    return $Relative -like "*.user"
}

function Get-PlatformFiles {
    $files = New-Object System.Collections.Generic.List[object]

    foreach ($name in @("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props",
            "nuget.config", "global.json", ".editorconfig", ".gitignore")) {
        $path = Join-Path $root $name
        if (Test-Path $path) {
            $files.Add([pscustomobject]@{ Source = $path; Relative = $name })
        }
    }

    $treeRoots = @("src", ".github") |
        ForEach-Object { Join-Path $root $_ } |
        Where-Object { Test-Path $_ }

    Get-ChildItem $treeRoots -Recurse -File -Force | ForEach-Object {
        $relative = $_.FullName.Substring($root.Length).TrimStart('\', '/')
        if (-not (Test-Excluded $relative)) {
            $files.Add([pscustomobject]@{ Source = $_.FullName; Relative = $relative })
        }
    }

    $sln = Join-Path $root "aspire-starter-platform.slnx"
    if (Test-Path $sln) {
        $files.Add([pscustomobject]@{ Source = $sln; Relative = "AspirePlatform.slnx" })
    }

    return $files
}

function Get-ServiceFiles {
    $files = New-Object System.Collections.Generic.List[object]
    $serviceRoot = Join-Path $root "src/Services/Simble"

    Get-ChildItem $serviceRoot -Recurse -File -Force | ForEach-Object {
        $relative = $_.FullName.Substring($serviceRoot.Length).TrimStart('\', '/')
        if (-not (Test-Excluded $relative)) {
            $files.Add([pscustomobject]@{ Source = $_.FullName; Relative = $relative })
        }
    }

    return $files
}

function Sync-Template {
    param(
        [string]$Name,
        [object[]]$Files
    )

    $target = Join-Path $root "templates/$Name"
    $stale = New-Object System.Collections.Generic.List[string]

    if (-not $Check) {
        if (Test-Path $target) {
            Get-ChildItem $target -Force |
                Where-Object { $_.Name -ne ".template.config" } |
                Remove-Item -Recurse -Force
        }
        New-Item -ItemType Directory -Force -Path $target | Out-Null
    }

    foreach ($file in $Files) {
        $destination = Join-Path $target $file.Relative

        if ($Check) {
            if (-not (Test-Path $destination)) {
                $stale.Add("$Name missing: $($file.Relative)")
                continue
            }
            if ((Get-FileHash $file.Source -Algorithm SHA256).Hash -ne (Get-FileHash $destination -Algorithm SHA256).Hash) {
                $stale.Add("$Name changed: $($file.Relative)")
            }
            continue
        }

        $parent = Split-Path $destination -Parent
        if (-not (Test-Path $parent)) {
            New-Item -ItemType Directory -Force -Path $parent | Out-Null
        }
        Copy-Item $file.Source $destination -Force
    }

    return $stale
}

$problems = @(Sync-Template -Name "aspire-platform" -Files (Get-PlatformFiles)) +
            @(Sync-Template -Name "aspire-service" -Files (Get-ServiceFiles))

if ($Check) {
    if ($problems.Count -gt 0) {
        $problems | ForEach-Object { Write-Host $_ -ForegroundColor Yellow }
        Write-Error "Packaged templates are out of date. Run ./sync-template.ps1"
    }
    Write-Host "Template copies are up to date." -ForegroundColor Green
    exit 0
}

Write-Host "Synced templates/aspire-platform and templates/aspire-service." -ForegroundColor Green
