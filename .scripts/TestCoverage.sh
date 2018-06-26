#!/bin/sh

# Get absolute path this script is in and use this path as a base for all other (relatve) filenames.
# !! Please make sure there are no spaces inside the path !!
# Source: https://stackoverflow.com/questions/242538/unix-shell-script-find-out-which-directory-the-script-file-resides
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
ROOT_PATH=$(cd ${SCRIPTPATH}/../; pwd)

cd ${ROOT_PATH}

#remove extension from filename as coverlet will add the extension
TMP_LCOV=${ROOT_PATH}/single_coverage_results
TMP_LCOV_EXT=${TMP_LCOV}.info


MERGED_LCOV=${ROOT_PATH}/coverage_results.info

# exclude the Testhelper project:  [TestHelper]*
# exclude all tests projects:      [*.Test]EagleEye.*
COVERLET_EXCLUDE_FILTER=[TestHelper]*,[*.Test]EagleEye.*


TEST_PROJECTS=$(find . -type f -name *Test.csproj)

# this for loop only works if the path doesn't contain any spaces!
for TEST_PROJECT in $TEST_PROJECTS
do
	echo Testing project: ${TEST_PROJECT}
	
	echo dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=$TMP_LCOV /p:Exclude=\"$COVERLET_EXCLUDE_FILTER\" /p:configuration=Release $TEST_PROJECT
	dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=$TMP_LCOV /p:Exclude=\"$COVERLET_EXCLUDE_FILTER\" /p:configuration=Release $TEST_PROJECT
	
	if [ -f "$TMP_LCOV_EXT" ]
	then
		#cat ${TMP_LCOV_EXT}
		#echo Upload coverage results to coverall
		#cat ${TMP_LCOV_EXT} | ./node_modules/coveralls/bin/coveralls.js
	
		echo Coverage file exists.. merge into final merge results.
		
		if [ -f "$MERGED_LCOV" ]
		then
			echo '\n' >> ${MERGED_LCOV}
			
		else
			touch $MERGED_LCOV
		fi
		
		cat ${TMP_LCOV_EXT} >> ${MERGED_LCOV}
		#echo '\n' >> ${MERGED_LCOV}
		rm $TMP_LCOV_EXT
	fi
done

echo Upload coverage results to coverall
# cat ${MERGED_LCOV} | ./node_modules/coveralls/bin/coveralls.js
cat ${MERGED_LCOV}