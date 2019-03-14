# $ErrorActionPreference = "Stop"

Write-Host "Start of Codecov.ps1"

# Save the current location.
$CurrentDir = $(Get-Location).Path;
Write-Host "CurrentDir: " $CurrentDir

# Get location of powershell file
Write-Host "PSScriptRoot: " $PSScriptRoot

# we know this script is located in the .scripts\ folder of the root.
$RootDir = [IO.Path]::GetFullPath( (join-path $PSScriptRoot "..\") )
Write-Host "ROOT: " $RootDir

$nugetToolsDir = "C:\NuGetTools"
$codecovExe = ""
Get-ChildItem -Recurse ($nugetToolsDir) | Where-Object {$_.Name -like "codecov.exe"} | % { $codecovExe = $_.FullName};

if (! ( Test-Path $codecovExe )) 
{
	throw "Could not upload the coverage results because codecov.exe wasn't found."
}

pushd
cd ..
$coverageResults = Get-ChildItem -Recurse | Where-Object{$_.Name -like "coverageInOpencoverFormat.xml" } | % { '"{0}"' -f $_.FullName }; # access $_.DirectoryName for the directory.
popd

$allCoverageResults = [String]::Join(" ", $coverageResults);

iex "$codecovExe -f $allCoverageResults"
	
if ($LastExitCode -ne 0) {
	throw "Could not upload the coverage file."
}
