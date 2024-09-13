module Tests.IntegrationTests

open Tests.Setup
open SimpleExec
open BlackFox.CommandLine
open Workspace
open Faqt

[<Test>]
let ``works for 'ci' type with default config`` () =
    task {
        let projectName = "Simple.fsproj"

        Command.Run(
            "dotnet",
            CmdLine.empty
            |> CmdLine.appendPrefix "add" projectName
            |> CmdLine.appendPrefix "package" "EasyBuild.PackageReleaseNotes.Tasks"
            |> CmdLine.appendPrefix "--source" Workspace.``..``.packages.``.``
            |> CmdLine.appendPrefix "--version" testPackageVersion
            |> CmdLine.toString,
            workingDirectory = Workspace.fixtures.valid.``.``
        )

        let! struct (stdout, _) =
            Command.ReadAsync(
                "dotnet",
                CmdLine.empty
                |> CmdLine.appendPrefix "pack" projectName
                |> CmdLine.appendPrefix "-c" "Release"
                |> CmdLine.appendRaw "--getProperty:Version"
                |> CmdLine.appendRaw "--getProperty:PackageVersion"
                |> CmdLine.appendRaw "--getProperty:PackageReleaseNotes"
                |> CmdLine.toString,
                workingDirectory = Workspace.fixtures.valid.``.``
            )

        stdout.Should().Be("dwdw") |> ignore
    }

//     let buildEngine = Mock<IBuildEngine>()
//     let errors = ResizeArray<BuildErrorEventArgs>()

//     buildEngine
//         .Setup(fun x -> x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
//         .Callback(fun args -> errors.Add(args))
//     |> ignore

//     let item = Mock<ITaskItem>()
//     item.Setup(fun x -> x.GetMetadata("MaximeTest")).Returns("test") |> ignore

//     let myTask = ParseChangelog(ChangelogFile = "MyChangelog.md")
//     myTask.BuildEngine <- buildEngine.Object

//     let success = myTask.Execute()

//     success.ShouldBeTrue()
