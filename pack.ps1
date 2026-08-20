<#
.SYNOPSIS
    Syncs, packs, and optionally installs or publishes AspirePlatform.Templates.

.PARAMETER SkipSync
    Skip refreshing templates/aspire-platform from the live solution.

.PARAMETER InstallLocal
    Install the produced package into the local dotnet new template engine.

.PARAMETER Smoke
    Instantiate the solution template into a temporary folder and build it.

.PARAMETER Push
    Publish the package to NuGet.org. Requires -ApiKey.

.PARAMETER ApiKey
    NuGet API key used with -Push.
#>
param(
    [switch]$SkipSync,
    [switch]$InstallLocal,
    [switch]$Smoke,
    [switch]$Push,
    [string]$ApiKey
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$project = Join-Path $root "templates/AspirePlatform.Templates.csproj"
$output = Join-Path $root "templates/bin/Release"

if (-not $SkipSync) {
    Write-Host "==> Syncing template content from the live solution..." -ForegroundColor Cyan
    & (Join-Path $root "sync-template.ps1")
}

Write-Host "==> Cleaning previous packages..." -ForegroundColor Cyan
if (Test-Path $output) {
    Remove-Item (Join-Path $output "*.nupkg") -Force -ErrorAction SilentlyContinue
}

Write-Host "==> Packing AspirePlatform.Templates..." -ForegroundColor Cyan
dotnet pack $project -c Release
if ($LASTEXITCODE -ne 0) { Write-Error "dotnet pack failed." }

$nupkg = Get-ChildItem (Join-Path $output "*.nupkg") | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($null -eq $nupkg) { Write-Error "No package was produced." }

Write-Host "==> Package created: $($nupkg.FullName)" -ForegroundColor Green

if ($InstallLocal -or $Smoke) {
    Write-Host "==> Installing into the local template engine..." -ForegroundColor Cyan

    # Installing a .nupkg by path adds a registration rather than replacing one, and duplicate
    # registrations make the template ambiguous, so clear them out first.
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        for ($attempt = 0; $attempt -lt 10; $attempt++) {
            $installed = dotnet new uninstall 2>&1 | Select-String -SimpleMatch "AspirePlatform.Templates"
            if (-not $installed) { break }
            dotnet new uninstall AspirePlatform.Templates | Out-Null
        }

        dotnet new install $nupkg.FullName
        if ($LASTEXITCODE -ne 0) { throw "dotnet new install failed." }
    }
    finally {
        $ErrorActionPreference = $previousPreference
    }
}

if ($Smoke) {
    $smokeRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("aspire-template-smoke-" + [guid]::NewGuid().ToString("N").Substring(0, 8))
    Write-Host "==> Smoke testing into $smokeRoot" -ForegroundColor Cyan
    New-Item -ItemType Directory -Force -Path $smokeRoot | Out-Null

    Push-Location $smokeRoot
    # dotnet writes progress to stderr, which Windows PowerShell turns into terminating errors.
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        dotnet new aspire-platform -n Acme
        if ($LASTEXITCODE -ne 0) { throw "Template instantiation failed." }

        dotnet build (Join-Path $smokeRoot "Acme/Acme.slnx")
        if ($LASTEXITCODE -ne 0) { throw "Generated solution failed to build." }

        Write-Host "==> Smoke test passed." -ForegroundColor Green
    }
    finally {
        $ErrorActionPreference = $previousPreference
        Pop-Location
    }
}

if ($Push) {
    if ([string]::IsNullOrWhiteSpace($ApiKey)) {
        Write-Error "ApiKey is required when -Push is specified."
    }
    Write-Host "==> Publishing $($nupkg.Name) to NuGet.org..." -ForegroundColor Cyan
    dotnet nuget push $nupkg.FullName --api-key $ApiKey --source https://api.nuget.org/v3/index.json --skip-duplicate
}
