@ECHO OFF

SETLOCAL

SET CACHED_NUGET=%LocalAppData%\NuGet\NuGet.exe
SET SOLUTION_PATH=%~dp0src\CodeFormatter.sln
SET BUILD_TOOLS_PATH="%ProgramFiles(x86)%\MSBuild\12.0\bin\MSBuild.exe"
SET FAKE_PATH=".\bin\tools"

IF NOT EXIST %BUILD_TOOLS_PATH% (
  echo In order to build or run this tool you need either Visual Studio 2015 or
  echo Microsoft Build Tools 2015 tools installed.
  echo.
  echo Visit this page to download either:
  echo.
  echo http://www.visualstudio.com/en-us/downloads/visual-studio-2015-downloads-vs
  echo.
  goto :eof
)

echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

echo Downloading latest version of Fake.exe...
%CACHED_NUGET% "install" "FAKE" "-OutputDirectory" "%FAKE_PATH%" "-ExcludeVersion" "-Prerelease"
%CACHED_NUGET% "install" "FSharp.Formatting.CommandTool" "-OutputDirectory" "%FAKE_PATH%" "-ExcludeVersion" "-Prerelease"
%CACHED_NUGET% "install" "SourceLink.Fake" "-OutputDirectory" "%FAKE_PATH%" "-ExcludeVersion"

SET TARGET="Build"

IF NOT [%1]==[] (set TARGET="%1")

"%FAKE_PATH%\FAKE\tools\Fake.exe" "build.fsx" "target=%TARGET%" applicationName=Elders.Pandora