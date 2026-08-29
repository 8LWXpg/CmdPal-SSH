param(
	[switch]$skipBuild,
	[switch]$skipBump,
	[switch]$skipSign
)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot/../SSH

if (-not $skipBump) {
	$version = Read-Host -Prompt 'New tag'
	(Get-Content ./Package.appxmanifest -Raw) `
		-replace '(Identity[^>]+?)Version="[\d\.]+"', "`$1Version=`"$version.0`"" |
		Out-File ./Package.appxmanifest -NoNewline

	git add ..
	git commit -m 'bump'
	git tag "v$version"
}

if (-not $skipBuild) {
	Remove-Item ./AppPackages, ./out -Recurse -Force -ErrorAction Ignore
	New-Item ./out -ItemType Directory | Out-Null

	foreach ($platform in 'x64', 'ARM64') {
		dotnet build --configuration Release -p:Platform=$platform -p:AppxPackageDir="AppPackages\$platform\"
	}

	$msix = Get-ChildItem ./AppPackages -Recurse -Filter *.msix
	Copy-Item $msix ./out -Force

	$parts = $msix[0].Name -split '_'
	$bundle = "$($parts[0])_$($parts[1]).msixbundle"

	Push-Location
	Import-Module 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\Microsoft.VisualStudio.DevShell.dll'
	Enter-VsDevShell e87fa5ce | Out-Null
	Pop-Location

	makeappx bundle /d ./out/ /p $bundle
	Move-Item $bundle ./out
}

if (-not $skipSign) {
	winapp sign ./out/$bundle E:/cert/cert.pfx
}

Pop-Location
