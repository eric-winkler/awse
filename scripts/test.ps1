$ErrorActionPreference = "Stop"
Set-StrictMode -Version "Latest"

try {
    $srcPath = Join-Path -Path $PSScriptRoot -ChildPath "..\src"
    $testsToRun = Get-ChildItem -Path $srcPath -Recurse -File -Filter *Tests.csproj

    $testFailure = $false
    foreach ($testToRun in $testsToRun) {
        dotnet test $testToRun.FullName
        
        if ($LastExitCode -ne 0) {
            $testFailure = $true
        }
    }
    
    if ($testFailure) {
        exit 1
    }
}
catch {
    Write-Error $_ -ErrorAction Continue
    exit 1
}