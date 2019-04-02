<h1 align="center">EagleEye</h1>
<div align="center">

[![Build status](https://ci.appveyor.com/api/projects/status/ner6290e44akpvuw/branch/develop?svg=true)](https://ci.appveyor.com/project/coenm/eagleeye/branch/develop) [![Coverage](https://codecov.io/gh/coenm/eagleeye/branch/develop/graph/badge.svg)](https://codecov.io/gh/coenm/eagleeye/branch/develop) [![Coverage](https://coveralls.io/repos/github/coenm/EagleEye/badge.svg?branch=develop)](https://coveralls.io/github/coenm/EagleEye?branch=develop) 

</div>
quick .net core app to remove already processed images.

# The original Problem
When I import photo's from my/a phone, or camera, via email or something else, I want to know which of those new photo's I've already seen and processed before.

# The easy solution
Just build an index containing hashes (sha256) of all photos and movies already processed. Before reviewing new data, calculate each single hash and remove the file if it has a known value.

# The more complicated solution
The easy solution just find 100% matches. When an original photo is send over using Whatsapp (or something similar) it is scaled to save bandwidth. In such case it would be nice to detect similarity. The more complicated solution is to not only compare by file hash but also by comparing raw images, exif data etc. etc.

# This project
Originally, this project is a quick (commandline) tool to build an index with filename and hash for first and easy comparison. Now, this project is trying to make photo's (and movies) searchable based on their metadata.


#  Experimenting with frameworks, tooling and patterns

- [x] [GitFlow](http://nvie.com/posts/a-successful-git-branching-model/);
- [x] [GitVersion](https://gitversion.readthedocs.io/en/latest/) for automatic versioning; 
- [x] dotnet core / standard;
- [x] DI: [SimpleInjector](https://simpleinjector.org/);
- [x] Test frameworks: [xUnit](https://github.com/xunit), [FakeItEasy](https://fakeiteasy.github.io/), [FluentAssertions](https://fluentassertions.com/);
- [x] [Jetbrains Annotations](https://www.jetbrains.com/help/resharper/Code_Analysis__Code_Annotations.html), ~~Maybe use [Code Contract](https://docs.microsoft.com/en-us/dotnet/framework/debug-trace-profile/code-contracts)?~~
- [x] CI: [AppVeyor](https://www.appveyor.com/) (windows CI and Ubuntu); ~~[Travis](https://travis-ci.org/) (Linux CI);~~
- [x] Coverage: ~~[OpenCover](https://github.com/OpenCover/opencover) (windows);~~ [Coverlet](https://github.com/tonerdo/coverlet/) (cross platform);
- [x] Coverage Report: [CodeCov](https://codecov.io), [Coveralls](https://coveralls.io);
- [ ] Docker;
- [ ] [SonarQube](https://about.sonarcloud.io/);
- [x] CQRS ES (using CQRSlite);
- [x] Entity framework;
- [x] Fody;
- [ ] Misc: NLog, Humanizer, Lucene.NET, NetMq (ZeroMq).
- [ ] Azure DevOps;

# Checkout and build
```
git clone https://github.com/coenm/EagleEye.git
git submodule update --init --recursive
dotnet restore
dotnet build
```
