#r @"tools/FAKE.Core/tools/FakeLib.dll"

open Fake
open System
open System.Collections.Generic
open System.Text

let buildMode = getBuildParamOrDefault "buildMode" "Release"

let versionRegex = "(<VersionPrefix>)([^\"]+)(</VersionPrefix>)"

//Directories
let packagingRoot = __SOURCE_DIRECTORY__ + "/packaging/"
let packagingDir =  packagingRoot + "scientist.net"
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

let dotnetExe = "dotnet.exe"

let UpdateProject csprojPath =
    let fullCsprojPath = (__SOURCE_DIRECTORY__ + csprojPath)
   
    let tempReleaseNotes = toLines releaseNotes.Notes
    RegexReplaceInFileWithEncoding "<PackageReleaseNotes>.*</PackageReleaseNotes>" ("<PackageReleaseNotes>" + tempReleaseNotes +  "</PackageReleaseNotes>") Encoding.UTF8 fullCsprojPath
    RegexReplaceInFileWithEncoding versionRegex ("${1}" + (releaseNotes.NugetVersion) + "${3}") Encoding.UTF8 fullCsprojPath

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
    let csprojPath = "/src/Scientist/Scientist.csproj"

    UpdateProject csprojPath

    Run currentDirectory dotnetExe ("pack .\\src\\Scientist\\ --configuration " + buildMode + " --output " + packagingDir) |> ignore
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