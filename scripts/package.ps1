$srcPath = Join-Path -Path $PSScriptRoot -ChildPath "..\src"

$projectFilesToPack = Get-ChildItem -Path $srcPath -Recurse -File -Filter *.csproj
$projectFilesToPack | Where-Object {$_.FullName -notlike "*Tests*"} | ForEach-Object {dotnet pack $_.FullName}