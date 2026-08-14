$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$artifactDirectory = Join-Path $repositoryRoot "artifacts\win-x64"
$testProject = Join-Path $repositoryRoot "tests\DocumentManager.Tests\DocumentManager.Tests.csproj"
$applicationProject = Join-Path $repositoryRoot "src\DocumentManager.WinUI\DocumentManager.WinUI.csproj"

Push-Location $repositoryRoot
try {
    dotnet --info
    dotnet restore $testProject
    dotnet test $testProject --configuration Release --no-restore

    dotnet restore $applicationProject --runtime win-x64
    dotnet publish $applicationProject `
        --configuration Release `
        --runtime win-x64 `
        --self-contained true `
        --no-restore `
        --output $artifactDirectory `
        -p:PublishSingleFile=true `
        -p:WindowsAppSDKSelfContained=true `
        -p:IncludeAllContentForSelfExtract=true `
        -p:PublishTrimmed=false

    $executable = Join-Path $artifactDirectory "GestorExpedientes.exe"
    if (-not (Test-Path $executable)) {
        throw "La publicación terminó sin producir $executable"
    }

    Write-Host "Publicación terminada: $executable" -ForegroundColor Green
}
finally {
    Pop-Location
}

