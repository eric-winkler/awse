$srcPath = Join-Path -Path $PSScriptRoot -ChildPath "..\src"

$slnFiles = Get-ChildItem -Path $srcPath -Recurse -File -Filter *.sln
$slnFiles | ForEach-Object {dotnet build $_.FullName}