# CompanionLibrary
Simple library to read battle scribe data files in C#

###### Building
To help deploying the library files in the .csproj files there is a post build event:
"$(SolutionDir)DeployHelper.exe" -path="C:\\Projects\\" -lib="$(TargetDir)$(ProjectName).dll" -deploy="true" -debug="true" -tools="false"
This has a hard coded path so if it fails to build you can remove it or change it to an applicable path.