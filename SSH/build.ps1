param(
    [switch]$skipBuild
)

Push-Location $PSScriptRoot

$version = Read-Host -Prompt 'New tag'

(Get-Content ./app.manifest -Raw) -replace `
	   '<assemblyIdentity version="[\d\.]+"', `
	   "<assemblyIdentity version=`"$version.0`"" `
| Out-File .\app.manifest -NoNewline
(Get-Content ./Package.appxmanifest -Raw) -replace `
	   '    Version="[\d\.]+" />', `
	   "    Version=`"$version.0`" />" `
| Out-File ./Package.appxmanifest -NoNewline
(Get-Content ./SSH.csproj -Raw) -replace `
    '<Version>[\d\.]+</Version>', `
    "<Version>$version</Version>" `
| Out-File ./SSH.csproj -NoNewline

if (-not $skipBuild) {
    dotnet build -c Release -p:GenerateAppxPackageOnBuild=true -p:Platform=x64
    dotnet build -c Release -p:GenerateAppxPackageOnBuild=true -p:Platform=arm64
    Remove-Item ./out/* -ea Ignore
    mkdir ./out -ea ig
    (Get-ChildItem -r *.msix -Exclude Microsoft.WindowsAppRuntime.*.msix).FullName | ForEach-Object { Copy-Item $_ ./out/. -Force }
}

git add ..
git commit -m 'bump'
git tag "v$version"

Pop-Location

