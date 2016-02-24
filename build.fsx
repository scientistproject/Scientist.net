#r @"tools/FAKE.Core/tools/FakeLib.dll"

open Fake
open System

let authors = ["GitHub"]

let projectName = "Scientist.Net"
let projectDescription = "A library for carefully refactoring critical paths"
let projectSummary = projectDescription

//let releaseNotes =
//    ReadFile "ReleaseNotes.md"
//    |> ReleaseNotesHelper.parseReleaseNotes
    
trace releaseNotes.AssemblyVersion
    
Target "BuildApp" (fun _ ->
    Exec "dnu" "build --configuration Release"
)

RunTargetOrDefault "BuildApp"