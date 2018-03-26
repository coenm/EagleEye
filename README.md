# EagleEye
quick .net core app to remove already processed images.

# The original Problem
When I import photo's from my/a phone, or camera, via email or something else, I want to know which of those new photo's I've already seen and processed before.

# The easy solution
Just build an index containing hashes (sha256) of all photos and movies already processed. Before reviewing new data, calculate each single hash and remove the file if it has a known value.

# The more complicated solution
The easy solution just find 100% matches. When an original photo is send over using Whatsapp (or something similar) it is scaled to save bandwidth. In such case it would be nice to detect similarity. The more complicated solution is to not only compare by file hash but also by comparing raw images, exif data etc. etc.

# This project
Originally, this project is a quick (commandline) tool to build an index with filename and hash for first and easy comparison. 

#  Experimenting with frameworks, tooling and patterns - Check
- [x]  [GitFlow](http://nvie.com/posts/a-successful-git-branching-model/);
- [x] [GitVersion](https://gitversion.readthedocs.io/en/latest/) for automatic versioning;
- [x] dotnet core / standard;
- [ ] DI: SimpleInjector;
- [x] Test frameworks: xUnit; FakeItEasy;
- [ ] Misc: NLog;
- [x] CI: AppVeyor (windows CI); Travis (Linux CI);
- [x] Coverage: OpenCover (windows); [Coverlet](https://github.com/tonerdo/coverlet/) (cross platform);
- [x] Coverage Report: [CodeCov](https://codecov.io); [Coveralls](https://coveralls.io)
- [ ] Docker;
- [ ] Lucene.NET;
- [ ] SonarQube




# The badges
| Branch | Status |
| :--- | :--- |
| Develop | [![Build status](https://ci.appveyor.com/api/projects/status/ner6290e44akpvuw/branch/develop?svg=true)](https://ci.appveyor.com/project/coenm/eagleeye/branch/develop) [![Build Status](https://travis-ci.org/coenm/EagleEye.svg?branch=develop)](https://travis-ci.org/coenm/EagleEye) [![Coverage](https://codecov.io/gh/coenm/eagleeye/branch/develop/graph/badge.svg)](https://codecov.io/gh/coenm/eagleeye/branch/develop) |


# Download
```
git clone https://github.com/coenm/EagleEye.git
git submodule update --init --recursive
```