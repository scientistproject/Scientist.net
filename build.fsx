#r @"tools/FAKE.Core/tools/FakeLib.dll"

open Fake
open System
open System.Collections.Generic
open System.Text

let architecture = getBuildParamOrDefault "architecture" "x86"
let runtime = getBuildParamOrDefault "runtime" "clr"
let runtimeVersion = getBuildParamOrDefault "runtimeVersion" "1.0.0-rc1-update1"
let buildMode = getBuildParamOrDefault "buildMode" "Release"

let versionRegex = "(\"version\": \")([^\"]+)(\")"

//Directories
let packagingRoot = "./packaging/"
let packagingDir = packagingRoot @@ "scientist.net"
let buildDir = "./src/Scientist/bin"
let testBuildDir = "./test/Scientise.Test/bin"

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

let dotnetHome = "C:\\Program Files\\dotnet\\"
let dotnetExe = dotnetHome + "dotnet.exe"

let UpdateProjectJson projectJson =
    let fullJsonPath = (__SOURCE_DIRECTORY__ + projectJson)
    let backupJsonPath = (fullJsonPath + ".bak")

    CopyFile backupJsonPath fullJsonPath
    
    let tempReleaseNotes = toLines releaseNotes.Notes
    RegexReplaceInFileWithEncoding "\"releaseNotes\": \"\"," ("\"releaseNotes\": \"" + tempReleaseNotes +  "\",") Encoding.UTF8 fullJsonPath
    RegexReplaceInFileWithEncoding versionRegex ("${1}" + (releaseNotes.NugetVersion) + "${3}") Encoding.UTF8 fullJsonPath

let RestoreProjectJson projectJson =
    let fullJsonPath = (__SOURCE_DIRECTORY__ + projectJson)
    let backupJsonPath = (fullJsonPath + ".bak")

    DeleteFile fullJsonPath
    CopyFile fullJsonPath backupJsonPath
    DeleteFile backupJsonPath

let SetBuildVersion =
    setProcessEnvironVar "DOTNET_BUILD_VERSION" (environVarOrDefault "APPVEYOR_BUILD_NUMBER" "local")

//Targets
Target "Clean" (fun _ ->
    CleanDirs [packagingDir; packagingRoot; "artifacts"; buildDir; testBuildDir]
)

Target "SetupBuild" (fun _ ->
    SetBuildVersion
    
    Run currentDirectory dotnetExe "restore" |> ignore
)

Target "BuildApp" (fun _ ->
    Run currentDirectory dotnetExe ("build .\\src\\Scientist\\ --configuration " + buildMode) |> ignore
    Run currentDirectory dotnetExe ("build .\\test\\Scientist.Test\\ --configuration " + buildMode + "") |> ignore
)

Target "CreatePackages" (fun _ ->
    let scientistJsonPath = "/src/Scientist/project.json"

    UpdateProjectJson scientistJsonPath

    Run currentDirectory dotnetExe ("pack .\\src\\Scientist\\ --configuration " + buildMode + " --out " + packagingDir) |> ignore

    RestoreProjectJson scientistJsonPath
)

Target "RunTests" (fun _ ->
    Run currentDirectory dotnetExe "-p .\\test\\Scientist.Test\\ test" |> ignore
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