@echo off

"tools\nuget\nuget.exe" "install" "FAKE.Core" "-OutputDirectory" "tools" "-ExcludeVersion" "-version" "4.4.2"

"tools\FAKE.core\tools\Fake.exe" "build.fsx" "target=BuildApp" "buildMode=Release"

exit /b %errorlevel%