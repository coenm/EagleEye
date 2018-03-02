#!/bin/sh
# sudo apt-get install perl

EXIFTOOL_VERSION_READ=`cat ../EXIFTOOL_VERSION`
echo "Read exiftool version: ${EXIFTOOL_VERSION_READ}"

wget https://www.sno.phy.queensu.ca/~phil/exiftool/Image-ExifTool-${EXIFTOOL_VERSION}.tar.gz
gzip -dc Image-ExifTool-${EXIFTOOL_VERSION}.tar.gz | tar -xf -
cd Image-ExifTool-${EXIFTOOL_VERSION}
perl Makefile.PL
# make test
sudo make install
pwd