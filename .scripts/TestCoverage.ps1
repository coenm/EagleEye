$ErrorActionPreference = "Stop"

# Save the current location.
$CurrentDir = $(Get-Location).Path;
Write-Output 'CurrentDir: ' + $CurrentDir

# Get location of powershell file
Write-Output 'PSScriptRoot: ' +$PSScriptRoot

# Expected OpenCover location appveyor.
$opencoverExe = 'C:\ProgramData\chocolatey\bin\OpenCover.Console.exe'
# Search for opencover in the chocolatery directory.
Get-ChildItem -Recurse ('C:\ProgramData\chocolatey\bin') | Where-Object {$_.Name -like "OpenCover.Console.exe"} | % { $opencoverExe = $_.FullName};

$dotnetExe = 'dotnet.exe'

Write-Host "Location opencover.exe: " $opencoverExe
Write-Host "Location dotnet.exe: " $dotnetExe

$outputTrxFile = 'C:\projects\eagleeye\testrun.trx'
$outputOpenCoverXmlFile = 'C:\projects\eagleeye\coverage-dotnet.xml'

$dotnetTestArgs = '-c Debug --no-build --logger:trx' # ;LogFileName=' + $outputTrxFile

$filter = "+[*]EagleEye.* -[*.Test]EagleEye.*"
# -filter:"+[*]EagleEye.* -[*.Test]EagleEye.*"
# Get-ChildItem | Get-Member # this gets you everything

pushd
cd ..
$testProjectLocations = Get-ChildItem -Recurse | Where-Object{$_.Name -like "*Test.csproj" } | % { $_.FullName }; # access $_.DirectoryName for the directory.
popd

Try
{
	ForEach ($testProjectLocation in $testProjectLocations)
	{
		Write-Host "found csproj file: " (Resolve-Path $testProjectLocation).Path;
	
		#$command = $opencoverExe + ' -threshold:1 -register:user -oldStyle -mergebyhash -mergeoutput -target:"' + $dotnetExe + '" -targetargs:"test ' + $testProjectLocation + ' '+ $dotnetTestArgs + '" "-output:' + $outputOpenCoverXmlFile + '" -returntargetcode "-excludebyattribute:System.Diagnostics.DebuggerNonUserCodeAttribute" "-filter:+[*]EagleEye.* -[*.Test]EagleEye.*"'
		$command = $opencoverExe + ' -threshold:1 -register:user -oldStyle -mergebyhash -mergeoutput -target:"' + $dotnetExe + '" -targetargs:"test ' + $testProjectLocation + ' '+ $dotnetTestArgs + '" "-output:' + $outputOpenCoverXmlFile + '" -returntargetcode "-excludebyattribute:System.Diagnostics.DebuggerNonUserCodeAttribute" "-filter:' +  $filter + '"'
		
		# (Debug) command to run:
		Write-Output $command
		
		iex $command
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
	popd
}