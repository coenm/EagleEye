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
$opencoverExe = 'C:\ProgramData\chocolatey\bin\OpenCover.Console.exe'
# Search for opencover in the chocolatery directory.
Get-ChildItem -Recurse ('C:\ProgramData\chocolatey\bin') | Where-Object {$_.Name -like "OpenCover.Console.exe"} | % { $opencoverExe = $_.FullName};

$dotnetExe = 'dotnet.exe'

$outputOpenCoverXmlFile = 'C:\projects\eagleeye\coverage-dotnet.xml'
$outputOpenCoverXmlFile = (join-path $RootDir "coverage-dotnet.xml")

Write-Host "Location opencover.exe: " $opencoverExe
Write-Host "Location dotnet.exe: " $dotnetExe
Write-Host "Location xml coverage result: " $outputOpenCoverXmlFile

$dotnetTestArgs = '-c Debug --no-build --filter Category!=ExifTool --logger:trx' # ;LogFileName=' + $outputTrxFile
$opencoverFilter = "+[*]EagleEye.* -[*.Test]EagleEye.*"

pushd
cd ..
$testProjectLocations = Get-ChildItem -Recurse | Where-Object{$_.Name -like "*Test.csproj" } | % { $_.FullName }; # access $_.DirectoryName for the directory.
popd




Try
{
	Write-Host "----------------------------";

	ForEach ($testProjectLocation in $testProjectLocations)
	{
		Write-Host "Run tests without coverage for project " (Resolve-Path $testProjectLocation).Path;
		dotnet test $testProjectLocation 
	}
	
	Write-Host "----------------------------";
		
	
	ForEach ($testProjectLocation in $testProjectLocations)
	{
		Write-Host "Run tests for project " (Resolve-Path $testProjectLocation).Path;

		$command = $opencoverExe + ' -threshold:1 -register:user -oldStyle -mergebyhash -mergeoutput -target:"' + $dotnetExe + '" -targetargs:"test ' + $testProjectLocation + ' '+ $dotnetTestArgs + '" "-output:' + $outputOpenCoverXmlFile + '" -returntargetcode "-excludebyattribute:System.Diagnostics.DebuggerNonUserCodeAttribute" "-filter:' +  $opencoverFilter + '"'
		
		Write-Output $command
		
		iex $command
		
		Write-Host "Command finished, ready for the next one"
	}

	# Either display or publish the results
	If ($env:CI -eq 'True')
	{
		Write-Output "Running from CI"
#		$command = (Get-ChildItem ($env:USERPROFILE + '\.nuget\packages\coveralls.io'))[0].FullName + '\tools\coveralls.net.exe' + ' --opencover "' + $outputFile + '" --full-sources'
#		Write-Output $command
#		iex $command
	}
	Else
	{
		Write-Output "Running local"
#		$command = (Get-ChildItem ($env:USERPROFILE + '\.nuget\packages\ReportGenerator'))[0].FullName + '\tools\ReportGenerator.exe -reports:"' + $outputFile + '" -targetdir:"' + $outputPath + '"'
#		Write-Output $command
#		iex $command
#		cd $outputPath
#		./index.htm
	}
}
Finally
{
	Write-Output "Finally"
	popd
}