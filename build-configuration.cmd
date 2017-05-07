@echo off

%FAKE% %NYX% "target=clean" -st
%FAKE% %NYX% "target=RestoreNugetPackages" -st

IF NOT [%1]==[] (set RELEASE_NUGETKEY="%1")

SET SUMMARY="Pandora aims to externalize the application configuration."
SET DESCRIPTION="Pandora aims to externalize the application configuration."

%FAKE% %NYX% appName=Elders.Pandora appSummary=%SUMMARY% appDescription=%DESCRIPTION% nugetPackageName=Pandora nugetkey=%RELEASE_NUGETKEY%
