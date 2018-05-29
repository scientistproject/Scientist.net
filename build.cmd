@echo off

"tools\nuget\nuget.exe" "install" "FAKE.Core" "-OutputDirectory" "tools" "-ExcludeVersion" "-version" "4.4.2"

:Build

SET TARGET="Default"

IF NOT [%1]==[] (set TARGET="%1")

"tools\FAKE.core\tools\Fake.exe" "build.fsx" "target=%TARGET%" "buildMode=Release"

exit /b %errorlevel%