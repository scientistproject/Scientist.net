#r @"tools/FAKE.Core/tools/FakeLib.dll"

open Fake
open Fake.FileHelper
open System
open System.Collections.Generic
open System.Net;
open System.Text

let architecture = getBuildParamOrDefault "architecture" "x86"
let runtime = getBuildParamOrDefault "runtime" "clr"
let runtimeVersion = getBuildParamOrDefault "runtimeVersion" "1.0.0-rc1-update1"
let buildMode = getBuildParamOrDefault "buildMode" "Release"

let versionRegex = "(<VersionPrefix>)([^\"]+)(</VersionPrefix>)"

//Directories
let packagingRoot = "./packaging/"
let packagingDir = packagingRoot @@ "scientist.net"
let buildDir = "./src/Scientist/bin"
let testBuildDir = "./test/Scientist.Test/bin"

let releaseNotes =
    ReadFile "ReleaseNotes.md"
    |> ReleaseNotesHelper.parseReleaseNotes

//Helper functions
let Run workingDirectory fileName args =
    let errors = new List<string>()
    let messages = new List<string>()
    let timout = TimeSpan.MaxValue

    let error msg =
        traceError msg
        errors.Add msg

    let message msg =
        traceImportant msg
        messages.Add msg

    let code = 
        ExecProcessWithLambdas (fun info ->
            info.FileName <- fileName
            info.WorkingDirectory <- workingDirectory
            info.Arguments <- args
        ) timout true error message

    ProcessResult.New code messages errors

let dotnetHome = ".\\tools\dotnet\\"
let dotnetExe = dotnetHome + "dotnet.exe"
let dotnetInstall = dotnetHome + "dotnet-install.ps1"
let dotnetInstallPath = "https://raw.githubusercontent.com/dotnet/cli/rel/1.0.0/scripts/obtain/dotnet-install.ps1"
let powershell = "powershell.exe"

let UpdateProject csprojPath nuspecPath =
    let fullCsprojPath = (__SOURCE_DIRECTORY__ + csprojPath)
    let fullNuspecPath = (__SOURCE_DIRECTORY__ + csprojPath)
    let backupCsprojPath = (fullCsprojPath + ".bak")
    let backupNuspecPath = (fullNuspecPath + ".bak")

    CopyFile backupCsprojPath fullCsprojPath
    CopyFile backupNuspecPath fullNuspecPath
    
    let tempReleaseNotes = toLines releaseNotes.Notes
    RegexReplaceInFileWithEncoding "<releaseNotes></releaseNotes>" ("<releaseNotes>" + tempReleaseNotes +  "</releaseNotes>") Encoding.UTF8 fullCsprojPath

    RegexReplaceInFileWithEncoding versionRegex ("${1}" + (releaseNotes.NugetVersion) + "${3}") Encoding.UTF8 fullNuspecPath

let RestoreProject csprojPath nuspecPath =
    let fullCsprojPath = (__SOURCE_DIRECTORY__ + csprojPath)
    let fullNuspecPath = (__SOURCE_DIRECTORY__ + csprojPath)
    let backupCsprojPath = (fullCsprojPath + ".bak")
    let backupNuspecPath = (fullNuspecPath + ".bak")
    
    DeleteFile fullCsprojPath
    CopyFile fullCsprojPath backupCsprojPath
    DeleteFile backupCsprojPath
    
    DeleteFile fullNuspecPath
    CopyFile fullNuspecPath backupNuspecPath
    DeleteFile backupNuspecPath

let SetBuildVersion =
    setProcessEnvironVar "DOTNET_BUILD_VERSION" (environVarOrDefault "APPVEYOR_BUILD_NUMBER" "local")

//Targets
Target "Clean" (fun _ ->
    CleanDirs [packagingDir; packagingRoot; "artifacts"; buildDir; testBuildDir]
)

Target "SetupBuild" (fun _ ->
    SetBuildVersion
    
    if not (fileExists dotnetExe) then 
        CreateDir dotnetHome
        
        let wc = new WebClient()
        wc.DownloadFile(dotnetInstallPath, dotnetInstall)
        
        Run currentDirectory powershell ("-file " + dotnetInstall + " -InstallDir .\\tools\\dotnet\\ -Version 1.0.0") |> ignore

    Run currentDirectory dotnetExe "restore" |> ignore
)

Target "BuildApp" (fun _ ->
    Run currentDirectory dotnetExe ("build .\\src\\Scientist\\ --configuration " + buildMode) |> ignore
    Run currentDirectory dotnetExe ("build .\\test\\Scientist.Test\\ --configuration " + buildMode + "") |> ignore
)

Target "CreatePackages" (fun _ ->
    let csprojPath = "/src/Scientist/Scientist.csproj"
    let nuspecPath = "/src/Scientist/scientist.nuspec"

    UpdateProject csprojPath nuspecPath

    Run currentDirectory dotnetExe ("pack .\\src\\Scientist\\ --configuration " + buildMode + " --output " + packagingDir) |> ignore

    RestoreProject csprojPath nuspecPath
)

Target "RunTests" (fun _ ->
 
    let result =
        Run currentDirectory dotnetExe "test .\\test\\Scientist.Test\\Scientist.Test.csproj"

    if result.ExitCode <> 0 then
        failwith "Unit tests failed"
    ()
)

Target "Default" DoNothing

//Dependencies
"Clean"
    ==> "SetupBuild"
    ==> "BuildApp"
    
"Clean"
    ==> "SetupBuild"
    ==> "CreatePackages"
    
"SetupBuild"
    ==> "RunTests"

"RunTests"
    ==> "Default"

RunTargetOrDefault "Default"