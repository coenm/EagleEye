#!/bin/sh
# sudo apt-get install perl

# Get absolute path this script is in and use this path as a base for all other (relatve) filenames.
# !! Please make sure there are no spaces inside the path !!
# Source: https://stackoverflow.com/questions/242538/unix-shell-script-find-out-which-directory-the-script-file-resides
# 2017-12-07
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
echo "scriptpath: $SCRIPTPATH"
EXIFTOOL_VERSION_READ=$(cat "${SCRIPTPATH}/EXIFTOOL_VERSION")
echo "Read exiftool version: ${EXIFTOOL_VERSION_READ}"

EXIFTOOL_VERSION_READ2=`cat ${SCRIPTPATH}/EXIFTOOL_VERSION`
echo "Read exiftool version2: ${EXIFTOOL_VERSION_READ2}"


wget https://www.sno.phy.queensu.ca/~phil/exiftool/Image-ExifTool-${EXIFTOOL_VERSION}.tar.gz
gzip -dc Image-ExifTool-${EXIFTOOL_VERSION}.tar.gz | tar -xf -
cd Image-ExifTool-${EXIFTOOL_VERSION}
perl Makefile.PL
# make test
sudo make install
pwd