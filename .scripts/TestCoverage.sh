#!/bin/sh

# Get absolute path this script is in and use this path as a base for all other (relatve) filenames.
# !! Please make sure there are no spaces inside the path !!
# Source: https://stackoverflow.com/questions/242538/unix-shell-script-find-out-which-directory-the-script-file-resides
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
ROOT_PATH=$(cd ${SCRIPTPATH}/../; pwd)

cd ${ROOT_PATH}

TMP_LCOV=${ROOT_PATH}/single_coverage_results.info
MERGED_LCOV=${ROOT_PATH}/coverage_results.info
touch $MERGED_LCOV

TEST_PROJECTS=$(find . -type f -name *Test.csproj)

# this for loop only works if the path doesn't contain any spaces!
for TEST_PROJECT in $TEST_PROJECTS
do
	echo Testing project: ${TEST_PROJECT}
	
	dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=$TMP_LCOV /p:configuration=Release $TEST_PROJECT
	
	if [ -f "$TMP_LCOV" ]
	then
		cat ${TMP_LCOV}
		echo Upload coverage results to coverall
		cat ${TMP_LCOV} | ./node_modules/coveralls/bin/coveralls.js
	
		echo Coverage file exists.. merge into final merge results.
		cat ${TMP_LCOV} >> ${MERGED_LCOV}
		echo '\n' >> ${MERGED_LCOV}
		rm $TMP_LCOV
	fi
done

# echo Upload coverage results to coverall
# cat ${MERGED_LCOV} | ./node_modules/coveralls/bin/coveralls.js