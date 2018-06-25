#!/bin/sh

# Get absolute path this script is in and use this path as a base for all other (relatve) filenames.
# !! Please make sure there are no spaces inside the path !!
# Source: https://stackoverflow.com/questions/242538/unix-shell-script-find-out-which-directory-the-script-file-resides
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
ROOT_PATH=$(cd ${SCRIPTPATH}/../; pwd)


# Source: https://sedders123.me/2017/07/28/install-gitversion-on-ubuntu/
sudo apt-get install mono-complete
sudo apt-get install libcurl3

wget https://github.com/GitTools/GitVersion/releases/download/v4.0.0-beta.12/GitVersion_4.0.0-beta0012.zip
unzip GitVersion_4.0.0-beta0012.zip -d ${SCRIPTPATH}/GitVersion

mono ${SCRIPTPATH}/GitVersion/GitVersion.exe