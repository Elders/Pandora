#I @"./bin/tools/FAKE/tools/"
#r @"./bin/tools/FAKE/tools/FakeLib.dll"
#load @"./bin/tools/SourceLink.Fake/tools/SourceLink.fsx"

open Fake
open Fake.Git
open Fake.FSharpFormatting
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System
open System.IO

type System.String with member x.contains (comp:System.StringComparison) str = x.IndexOf(str,comp) >= 0        
let excludePaths (pathsToExclude : string list) (path: string) = pathsToExclude |> List.exists (path.contains StringComparison.OrdinalIgnoreCase)|> not

let applicationName = getBuildParamOrDefault "applicationName" ""
let applicationProjFile = @".\src\" @@ applicationName @@ applicationName + @".csproj"
let buildDir  = @"./bin/Release" @@ applicationName
let release = LoadReleaseNotes "RELEASE_NOTES.md"

let projectName = "Pandora"
let projectSummary = "Elders.Pandora"
let projectDescription = "Elders.Pandora"
let projectAuthors = ["Nikolai Mynkow"; "Simeon Dimov";]

let packages = ["Pandora", projectDescription]
let nuget = environVar "CACHED_NUGET"
let nugetArtifactsDir = "./bin/nuget"
let nugetDependencies = ["Newtonsoft.Json", "6.0.6";]
let nugetDependenciesFlat, _ = nugetDependencies |> List.unzip
let excludeNugetDependencies = excludePaths nugetDependenciesFlat

Target "Clean" (fun _ -> CleanDirs [buildDir])

Target "AssemblyInfo" (fun _ ->
    let assInfoFile = @"./src/" + applicationName + "/Properties/AssemblyInfo.cs"
    CreateCSharpAssemblyInfo assInfoFile
           [Attribute.Title "Elders.Pandora"
            Attribute.Description "Elders.Pandora"
            Attribute.Product "Elders.Pandora"
            Attribute.Version release.AssemblyVersion
            Attribute.InformationalVersion release.AssemblyVersion
            Attribute.FileVersion release.AssemblyVersion]
)

Target "Build" (fun _ ->
    Console.WriteLine applicationProjFile
    !! applicationProjFile
        |> MSBuildRelease buildDir "Build"
        |> Log "Build-Output: "
)

Target "RestorePackages" (fun _ ->
    !! "./**/packages.config"
    |> Seq.iter (RestorePackage (fun p -> 
        { p with 
            ToolPath = nuget 
            OutputPath = "./src/packages" }))
)

Target "CreateNuGet" (fun _ ->
    for package,description in packages do
    
        let nugetToolsDir = nugetArtifactsDir @@ "lib" @@ "net45-full"
        CleanDir nugetToolsDir

        match package with
        | p when p = projectName ->
            CopyDir nugetToolsDir buildDir excludeNugetDependencies
        | _ -> ()
        
        let nuspecFile = package + ".nuspec"
        NuGet (fun p ->
            {p with
                Authors = projectAuthors
                Project = package
                Description = description
                Version = release.NugetVersion
                Summary = projectSummary
                ReleaseNotes = release.Notes |> toLines
                Dependencies = nugetDependencies
                AccessKey = getBuildParamOrDefault "nugetkey" ""
                Publish = hasBuildParam "nugetkey"
                ToolPath = nuget
                OutputPath = nugetArtifactsDir
                WorkingDir = nugetArtifactsDir }) nuspecFile
)

Target "Release" (fun _ ->
    StageAll ""
    let notes = String.concat "; " release.Notes
    Commit "" (sprintf "%s" notes)
    Branches.push ""

    Branches.tag "" release.NugetVersion
    Branches.pushTag "" "origin" release.NugetVersion
)

// Dependencies
"Clean"
    ==> "RestorePackages"
    ==> "AssemblyInfo"
    ==> "Build"
    ==> "CreateNuGet"
    ==> "Release"
 
// start build
RunParameterTargetOrDefault "target" "Build"