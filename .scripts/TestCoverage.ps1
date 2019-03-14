if ($isWindows) {

	# Save the current location.
	$CurrentDir = $(Get-Location).Path;
	Write-Host "CurrentDir: " $CurrentDir

	# Get location of powershell file
	Write-Host "PSScriptRoot: " $PSScriptRoot

	# we know this script is located in the .scripts\ folder of the root.
	$RootDir = [IO.Path]::GetFullPath( (join-path $PSScriptRoot "..\") )
	Write-Host "ROOT: " $RootDir

	$outputOpenCoverXmlFile = (join-path $RootDir "coverage-dotnet.xml")

	# Should be release of debug (set by AppVeyor)
	$configuration = $env:CONFIGURATION

	Write-Host "(Environment) Configuration:" $configuration 
	Write-Host "Location xml coverage result: " $outputOpenCoverXmlFile

	#remove extension from filename as coverlet will add the extension
	$TMP_LCOV = (join-path $RootDir "single_coverage_results")
	$TMP_LCOV_EXT = (join-path $RootDir "single_coverage_results.info")


	$MERGED_LCOV = (join-path $RootDir "coverage_results.info")
	  
	# exclude the Testhelper project:  [TestHelper]*
	# exclude all tests projects:      [*.Test]EagleEye.*
	$COVERLET_EXCLUDE_FILTER = "[TestHelper]*,[*.Test]EagleEye.*,[xunit.*]*"
	$COVERLET_INCLUDE_FILTER = "[*]EagleEye.*,[EagleEye*]*"
	$COVERLET_EXCLUDE_ATTRIBUTE = "DebuggerNonUserCodeAttribute"
	
	
	# see https://github.com/tonerdo/coverlet/blob/master/README.md
	$COVERLET_EXCLUDE_FILTER_ESCAPED = $COVERLET_EXCLUDE_FILTER -replace ",", "%2c"
	$COVERLET_INCLUDE_FILTER_ESCAPED = $COVERLET_INCLUDE_FILTER -replace ",", "%2c"
	$COVERLET_EXCLUDE_ATTRIBUTE_ESCAPED = $COVERLET_EXCLUDE_ATTRIBUTE -replace ",", "%2c"

	pushd
	cd ..
	$testProjectLocations = Get-ChildItem -Recurse | Where-Object{$_.Name -like "*Test.csproj" } | % { $_.FullName }; # access $_.DirectoryName for the directory.
	popd

	$ExitCode = 0

	ForEach ($testProjectLocation in $testProjectLocations)
	{
		Write-Host "Test project: " (Resolve-Path $testProjectLocation).Path;
		
		$command = "dotnet.exe test "`
            + "/p:CollectCoverage=true "`
            + "/p:Exclude=""${COVERLET_EXCLUDE_FILTER_ESCAPED}"" "`
            + "/p:Include=""${COVERLET_INCLUDE_FILTER_ESCAPED}"" "`
            + "/p:ExcludeByAttribute=""${COVERLET_EXCLUDE_ATTRIBUTE_ESCAPED}"" "`
            + "/p:ExcludeByFile=""*\*Designer.cs"" "`
            + "/p:CoverletOutputFormat=opencover "`
            + "/p:CoverletOutput=./coverageInOpencoverFormat.xml "`
            + "/p:configuration=Release "`
            + " ""${testProjectLocation}"""
		
		Write-Output $command
		iex $command

		if ($LastExitCode -ne 0) {
			Write-Host  "At least one test failed. Setting exitcode to make sure this script will fail."
			$ExitCode = $LastExitCode
		}
	}	
	
	if ($ExitCode -ne 0) {
		throw "Done but with failing tests.."
	}
}
Write-Output "End of TestCoverage.ps1"
