#r @"tools/FAKE.Core/tools/FakeLib.dll"

open Fake
open System
open System.Collections.Generic

let mutable DnxHome = "[unknown]"

let authors = ["GitHub"]

let projectName = "Scientist.Net"
let projectDescription = "A library for carefully refactoring critical paths"
let projectSummary = projectDescription

//let releaseNotes =
//    ReadFile "ReleaseNotes.md"
//    |> ReleaseNotesHelper.parseReleaseNotes
    
//trace releaseNotes.AssemblyVersion

let Exec command args =
    let result = Shell.Exec(command, args)
    if result <> 0 then failwithf "%s exited with error %d" command result 
 
Target "SetupRuntime" (fun _ ->
    Exec (__SOURCE_DIRECTORY__ + "\\tools\\dnvm\\dnvm.cmd") "install 1.0.0-rc1-update1 -r clr -a x86"
)
 
Target "BuildApp" (fun _ ->
    Exec "dnu.cmd" "build --configuration Release"
)

"SetupRuntime" ==> "BuildApp"

RunTargetOrDefault "BuildApp"