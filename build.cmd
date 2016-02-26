@echo off

"tools\nuget\nuget.exe" "install" "FAKE.Core" "-OutputDirectory" "tools" "-ExcludeVersion" "-version" "4.4.2"

:Build

SET TARGET="Default"

IF NOT [%1]==[] (set TARGET="%1")

IF %TARGET%=="BuildApp" (
    "tools\FAKE.core\tools\Fake.exe" "build.fsx" "target=BuildApp" "buildMode=Release" "architecture=x86" "runtime=clr" "runtimeVersion=1.0.0-rc1-update1"
    "tools\FAKE.core\tools\Fake.exe" "build.fsx" "target=BuildApp" "buildMode=Release" "architecture=x86" "runtime=coreclr" "runtimeVersion=1.0.0-rc1-update1"
) ELSE (
    "tools\FAKE.core\tools\Fake.exe" "build.fsx" "target=%TARGET%" "buildMode=Release" "architecture=x86" "runtime=clr" "runtimeVersion=1.0.0-rc1-update1"
)

exit /b %errorlevel%