param(
	[switch]$skipBuild
)

Push-Location $PSScriptRoot

$version = Read-Host -Prompt 'New tag'

if (-not $skipBuild) {
	dotnet build -c Release -p:GenerateAppxPackageOnBuild=true -p:Platform=x64
	dotnet build -c Release -p:GenerateAppxPackageOnBuild=true -p:Platform=arm64
	Remove-Item ./out/* -ea Ignore
	mkdir ./out -ea ig
	(Get-ChildItem -r *.msix -Exclude Microsoft.WindowsAppRuntime.*.msix).FullName | ForEach-Object { Copy-Item $_ ./out/. -Force }
}

(Get-Content ./app.manifest -Raw) -replace `
	'<assemblyIdentity version="[\d\.]+" name="ProcessKiller.app"/>', `
	"<assemblyIdentity version=`"$version.0`" name=`"ProcessKiller.app`"/>" `
| Out-File .\app.manifest -NoNewline
(Get-Content ./Package.appxmanifest -Raw) -replace `
	'Publisher="CN=8LWXpg"
    Version="[\d\.]+" />', `
	"Publisher=`"CN=8LWXpg`"
    Version=`"$version.0`" />" `
| Out-File ./Package.appxmanifest -NoNewline

git add ..
git commit -m 'bump'
git tag "v$version"

Pop-Location

