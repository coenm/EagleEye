$ErrorActionPreference = "Stop"

# Save the current location.
$CurrentDir = $(Get-Location).Path;
Write-Host "CurrentDir: " $CurrentDir

# Get location of powershell file
Write-Host "PSScriptRoot: " $PSScriptRoot

# we know this script is located in the .scripts\ folder of the root.
$RootDir = [IO.Path]::GetFullPath( (join-path $PSScriptRoot "..\") )
Write-Host "ROOT: " $RootDir

# Expected OpenCover location appveyor.
$dotcoverExe = 'C:\Users\appveyor\AppData\Local\JetBrains\Installations\dotCover11\dotCover.exe'
# Search for opencover in the chocolatery directory.
#Get-ChildItem -Recurse ('C:\Users\appveyor\AppData\Local\JetBrains') | Where-Object {$_.Name -like "dotCover.exe"} | % { Write-Host "Found DotCover: " $_.FullName};
#Get-ChildItem -Recurse ('C:\ProgramData\chocolatey') | Where-Object {$_.Name -like "dotCover.exe"} | % { $dotcoverExe = $_.FullName};

# "C:\Users\coen\AppData\Local\JetBrains\Installations\dotCover11\dotCover.exe" analyse /TargetExecutable="C:\Program Files\dotnet\dotnet.exe" /TargetArguments="test C:\Repositories\EagleEye\EagleEye.sln" /Filters=+:type=EagleEye.*;-:module=*.Test; /Output="D:/output.xml" /ReportType="XML"

$dotnetExe = 'C:\Program Files\dotnet\dotnet.exe'

$outputOpenCoverXmlFile = 'C:\projects\eagleeye\coverage-dotnet.xml'
$outputOpenCoverXmlFile = (join-path $RootDir "coverage-dotnet.xml")
$slnFile = (join-path $RootDir "EagleEye.sln")

Write-Host "Location dotcover.exe: " $dotcoverExe
Write-Host "Location dotnet.exe: " $dotnetExe
Write-Host "Location xml coverage result: " $outputOpenCoverXmlFile

$dotnetTestArgs = '-c Debug --no-build --logger:trx' # ;LogFileName=' + $outputTrxFile
$opencoverFilter = "+[*]EagleEye.* -[*.Test]EagleEye.*"
$dotcoverFilter = "+:type=EagleEye.*;-:module=*.Test;"

# pushd
# cd ..
$testProjectLocations = Get-ChildItem -Recurse | Where-Object{$_.Name -like "*Test.csproj" } | % { $_.FullName }; # access $_.DirectoryName for the directory.
# popd


$command = $dotcoverExe + ' analyse /TargetExecutable="' + $dotnetExe + '" /TargetArguments="test ' + $slnFile + ' '+ $dotnetTestArgs + '" "/Output=' + $outputOpenCoverXmlFile + '" /Filters="' +  $dotcoverFilter + '"'
		
Write-Output $command
		
iex $command

# Try
# {
# 	ForEach ($testProjectLocation in $testProjectLocations)
# 	{
# 		Write-Host "Run tests for project " (Resolve-Path $testProjectLocation).Path;
# 
# # "C:\Users\coen\AppData\Local\JetBrains\Installations\dotCover11\dotCover.exe" analyse /TargetExecutable="C:\Program Files\dotnet\dotnet.exe" /TargetArguments="test C:\Repositories\EagleEye\EagleEye.sln" /Filters=+:type=EagleEye.*;-:module=*.Test; /Output="D:/output.html" /ReportType="HTML"  
# 		
# 		$command = $dotcoverExe + ' analyse /TargetExecutable="' + $dotnetExe + '" /TargetArguments="test ' + $testProjectLocation + ' '+ $dotnetTestArgs + '" "/Output=' + $outputOpenCoverXmlFile + '" /Filters="' +  $dotcoverFilter + '"'
# 		
# 		Write-Output $command
# 		
# 		iex $command
# 		
# 		Write-Host "Command finished, ready for the next one"
# 	}
# 
# 	# Either display or publish the results
# 	If ($env:CI -eq 'True')
# 	{
# 		Write-Output "Running from CI"
# #		$command = (Get-ChildItem ($env:USERPROFILE + '\.nuget\packages\coveralls.io'))[0].FullName + '\tools\coveralls.net.exe' + ' --opencover "' + $outputFile + '" --full-sources'
# #		Write-Output $command
# #		iex $command
# 	}
# 	Else
# 	{
# 		Write-Output "Running local"
# #		$command = (Get-ChildItem ($env:USERPROFILE + '\.nuget\packages\ReportGenerator'))[0].FullName + '\tools\ReportGenerator.exe -reports:"' + $outputFile + '" -targetdir:"' + $outputPath + '"'
# #		Write-Output $command
# #		iex $command
# #		cd $outputPath
# #		./index.htm
# 	}
# }
# Finally
# {
# 	Write-Output "Finally"
# 	popd
# }