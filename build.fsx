#r @"tools/FAKE.Core/tools/FakeLib.dll"

open Fake
open System
open System.Collections.Generic
open System.Text

let mutable DnxHome = "[unknown]"

let authors = ["GitHub"]

let projectName = "Scientist.Net"
let projectDescription = "A library for carefully refactoring critical paths"
let projectSummary = projectDescription

//Directories
let packagingRoot = "./packaging/"
let packagingDir = packagingRoot @@ "scientist.net"
let buildDir = "./src/Scientist/bin"
let testBuildDir = "./test/Scientise.Test/bin"

//let releaseNotes =
//    ReadFile "ReleaseNotes.md"
//    |> ReleaseNotesHelper.parseReleaseNotes

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

let GetHomeDirectory =
    let result = Run currentDirectory "cmd" "/c \"echo %USERPROFILE%\""
    result.Messages.[0]

let GetDnvmHome =
    let homeDirectory =
        if buildServer = BuildServer.AppVeyor
            then "C:\\Program Files\\Microsoft DNX\\Dnvm\\"
            else (GetHomeDirectory + "\\.dnx\\bin\\")

    homeDirectory

let GetDnxHome =
    let homeDirectory = GetHomeDirectory
    homeDirectory + "\\.dnx\\runtimes\\dnx-clr-win-x86.1.0.0-rc1-update1\\bin\\"

let UpdateProjectJson projectJson =
    let fullJsonPath = (__SOURCE_DIRECTORY__ + projectJson)
    let backupJsonPath = (fullJsonPath + ".bak")

    CopyFile backupJsonPath fullJsonPath

    let tempReleaseNotes = "Temporary release notes\\nWith new line"
    RegexReplaceInFileWithEncoding "\"releaseNotes\": \"\"," ("\"releaseNotes\": \"" + tempReleaseNotes +  "\",") Encoding.UTF8 fullJsonPath

let RestoreProjectJson projectJson =
    let fullJsonPath = (__SOURCE_DIRECTORY__ + projectJson)
    let backupJsonPath = (fullJsonPath + ".bak")

    DeleteFile fullJsonPath
    CopyFile fullJsonPath backupJsonPath
    DeleteFile backupJsonPath

let SetDnxBuildVersion =
    setProcessEnvironVar "DNX_BUILD_VERSION" (environVarOrDefault "APPVEYOR_BUILD_NUMBER" "local")

//Targets

Target "Clean" (fun _ ->
    CleanDirs [packagingDir; packagingRoot; "artifacts"; buildDir; testBuildDir]
)

Target "SetupBuild" (fun _ ->
    DnxHome <- GetDnxHome

    SetDnxBuildVersion

    let dnvmHome = GetDnvmHome
    Run currentDirectory (dnvmHome + "dnvm.cmd") "install 1.0.0-rc1-update1 -r clr -a x86" |> ignore
    Run currentDirectory (dnvmHome + "dnvm.cmd") "use 1.0.0-rc1-update1 -r clr -a x86" |> ignore
    
    Run currentDirectory (DnxHome + "dnu.cmd") "restore" |> ignore
)

Target "BuildApp" (fun _ ->
    Run currentDirectory (DnxHome + "dnu.cmd") "build .\\src\\Scientist\\ --configuration Release" |> ignore
    Run currentDirectory (DnxHome + "dnu.cmd") "build .\\test\\Scientist.Test\\ --configuration Release" |> ignore
)

Target "CreatePackages" (fun _ ->
    let scientistJsonPath = "/src/Scientist/project.json"

    UpdateProjectJson scientistJsonPath

    Run currentDirectory (DnxHome + "dnu.cmd") ("pack .\\src\\Scientist\\ --configuration Release --out " + packagingDir) |> ignore

    RestoreProjectJson scientistJsonPath
)

Target "RunTests" (fun _ ->
    Run currentDirectory (DnxHome + "dnx.exe") "-p .\\test\\Scientist.Test\\ test" |> ignore
)

Target "Default" DoNothing

"Clean"
    ==> "SetupBuild"

"SetupBuild"
    ==> "BuildApp"

"SetupBuild"
    ==> "RunTests"

"SetupBuild"
    ==> "CreatePackages"

"RunTests"
    ==> "Default"

RunTargetOrDefault "Default"