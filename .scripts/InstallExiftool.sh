#!/bin/sh
# sudo apt-get install perl

wget https://www.sno.phy.queensu.ca/~phil/exiftool/Image-ExifTool-${EXIFTOOL_VERSION}.tar.gz
gzip -dc Image-ExifTool-${EXIFTOOL_VERSION}.tar.gz | tar -xf -
cd Image-ExifTool-${EXIFTOOL_VERSION}
perl Makefile.PL
# make test
sudo make install
pwd