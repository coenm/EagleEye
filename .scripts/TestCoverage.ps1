$ErrorActionPreference = "Stop"

# md -Force $outputLocation | Out-Null
#$outputPath = (Resolve-Path $outputLocation).Path
#$outputFile = Join-Path $outputPath -childpath 'coverage.xml'

# OpenCover location appveyor.
$opencoverExe = 'C:\ProgramData\chocolatey\lib\opencover.portable\tools\OpenCover.Console.exe'
# (Get-ChildItem ($env:USERPROFILE + '\.nuget\packages\OpenCover'))[0].FullName + '\tools\OpenCover.Console.exe'

$dotnetExe = 'dotnet.exe'


$outputTrxFile = 'C:\testrun.trx'
$outputOpenCoverXmlFile = 'C:\coverage-dotnet.xml'

$dotnetTestArgs = '-c Debug --no-build --logger:trx;LogFileName=' + $outputTrxFile

$filter = "+[*]EagleEye.* -[*.Test]EagleEye.*"

# Get-ChildItem | Get-Member # this gets you everything

pushd
cd ..
# $testProjectLocations = Get-ChildItem -Recurse | Where-Object{$_.Name -like "*Test.csproj" } | % { Write-Host $_.DirectoryName};
$testProjectLocations = Get-ChildItem -Recurse | Where-Object{$_.Name -like "*Test.csproj" } | % { $_.FullName };
popd

Try
{
	ForEach ($testProjectLocation in $testProjectLocations)
	{
		Write-Host "found csproj file: " (Resolve-Path $testProjectLocation).Path;
	
		$command = $opencoverExe + ' -threshold:1 -register:user -oldStyle -mergebyhash -mergeoutput -target:"' + $dotnetExe + '" -targetargs:"test ' + $testProjectLocation + ' '+ $dotnetTestArgs + '" "-output:' + $outputOpenCoverXmlFile + '" -returntargetcode "-excludebyattribute:System.Diagnostics.DebuggerNonUserCodeAttribute" "-filter:+[*]EagleEye.* -[*.Test]EagleEye.*"'
		
		# (Debug) command to run:
		# Write-Output $command
		
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