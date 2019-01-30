# How to build

* Pull this repo.
* Pull the [ProcessMemory](https://github.com/Squirrelies/ProcessMemory) repo.
* Pull the [DoubleBuffered](https://github.com/Squirrelies/DoubleBuffered) repo.
* Open the RE2REmakeSRT.sln, edit the properties for RE2REmakeSRT and remove the post-build events for code signing.
* ...
* Success.

Notes:

.NET Framework v4.7.2 SDK required, by default, to build this project. You could reduce the .NET Framework version in the projects and the code will likely still work.
