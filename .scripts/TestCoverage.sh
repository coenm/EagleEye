#!/bin/bash

# Get absolute path this script is in and use this path as a base for all other (relatve) filenames.
# !! Please make sure there are no spaces inside the path !!
# Source: https://stackoverflow.com/questions/242538/unix-shell-script-find-out-which-directory-the-script-file-resides
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
ROOT_PATH=$(cd ${SCRIPTPATH}/../; pwd)

cd ${ROOT_PATH}

# exclude the Testhelper project:  [TestHelper]*
# exclude all tests projects:      [*.Test]EagleEye.*
COVERLET_EXCLUDE_FILTER=[TestHelper]*,[*.Test]EagleEye.*,[xunit.*]*
COVERLET_INCLUDE_FILTER=[*]EagleEye.*,[EagleEye*]*
COVERLET_EXCLUDE_ATTRIBUTE=DebuggerNonUserCodeAttribute

# Get all test projects.
TEST_PROJECTS=$(find . -type f -name *Test.csproj)

# Test coverage filename
SINGLE_COVERAGE_FILENAME=coverageInOpencoverFormat.xml

# Store the exitcode for each test run so we can exit if one of the test projects has at least one failing test.
EXIT_CODE=0

# This for loop only works if the path doesn't contain any spaces! (TODO?)
for TEST_PROJECT in $TEST_PROJECTS
do
	echo Testing project: ${TEST_PROJECT}
	dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./$SINGLE_COVERAGE_FILENAME /p:Exclude=\"$COVERLET_EXCLUDE_FILTER\" /p:Include=\"$COVERLET_INCLUDE_FILTER\" /p:ExcludeByAttribute=\"$COVERLET_EXCLUDE_ATTRIBUTE_ESCAPED\" /p:ExcludeByFile=\"*\*Designer.cs\" /p:configuration=Release $TEST_PROJECT
	
	LAST_EXIT_CODE=$?
	if [ "$LAST_EXIT_CODE" -ne "0" ]; then
		echo "At least one test failed. Setting exitcode to make sure this script will fail."
		EXIT_CODE=$LAST_EXIT_CODE
  	fi
done

if [ "$EXIT_CODE" -ne "0" ]; then
    echo "One of the test projects failed. Stop the script. "
    exit ${EXIT_CODE}
fi

# Find all coverage result files and format them to be used in csmacnz.Coveralls
COVERAGE_RESULTS=$(find . -type f -name $SINGLE_COVERAGE_FILENAME -printf 'opencover=%p;')

# Join all found result files to one string.
COVERAGE_RESULT_STRING=$( printf "%s" "${COVERAGE_RESULTS[@]}" ) 

# Remove last seperation (semicolon character)
COVERAGE_RESULT_STRING=${COVERAGE_RESULT_STRING::-1}
echo COVERAGE_RESULT_STRING : "${COVERAGE_RESULT_STRING}"

# Upload to Coveralls
echo Upload coverage results to coverall
csmacnz.Coveralls --multiple --input $COVERAGE_RESULT_STRING --useRelativePaths --basePath $ROOT_PATH
