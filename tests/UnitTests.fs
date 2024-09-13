module Tests.UnitTests

open Tests.Setup
open Moq
open Microsoft.Build.Framework
open EasyBuild.PackageReleaseNotes.Tasks
open Workspace
open Faqt

type TestContext =
    {
        BuildEngine: Mock<IBuildEngine>
        Errors: ResizeArray<BuildErrorEventArgs>
    }

    member this.PrintErrors() =
        this.Errors |> Seq.iter (fun error -> printfn "Error: %s" error.Message)

let private setupBuildEngine () =
    let context =
        {
            BuildEngine = Mock<IBuildEngine>()
            Errors = ResizeArray<BuildErrorEventArgs>()
        }

    context.BuildEngine
        .Setup(fun engine -> engine.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
        .Callback(fun (args: BuildErrorEventArgs) -> context.Errors.Add(args))
    |> ignore

    context

[<Test>]
let ``task fails when changelog file does not exist`` () =
    let context = setupBuildEngine ()

    let myTask = GetCurrentReleaseTask(ChangelogFile = "ThisFileDoesNotExist.md")
    myTask.BuildEngine <- context.BuildEngine.Object

    let success = myTask.Execute()

    success.Should().BeFalse() |> ignore
    context.Errors.Count.Should().Be(1) |> ignore
    context.Errors.[0].Code.Should().Be("EPT0001") |> ignore

[<Test>]
let ``task succeeds when changelog file exists (relative path + ConventionalCommits)`` () =
    let context = setupBuildEngine ()

    // When running tests, the working directory is where the dll is located
    let myTask =
        GetCurrentReleaseTask(ChangelogFile = "./../../../fixtures/CHANGELOG_ConventionalCommits.md")

    myTask.BuildEngine <- context.BuildEngine.Object

    let success = myTask.Execute()

    printfn "Errors: %A" context.Errors

    context.PrintErrors()

    success.Should().BeTrue() |> ignore
    context.Errors.Count.Should().Be(0) |> ignore

    myTask.CurrentRelease.GetMetadata("Version").Should().Be("0.10.0") |> ignore
    myTask.CurrentRelease.GetMetadata("Date").Should().Be("") |> ignore

    myTask.CurrentRelease
        .GetMetadata("Body")
        .Should()
        .Be(
            """### 🚀 Features

* Feature 1

### 🐞 Bug Fixes

* Bug fix 1
* Bug fix 2"""
        )
    |> ignore

[<Test>]
let ``task succeeds when changelog file exists (absolute path + KeepAChangeLog format)`` () =
    let context = setupBuildEngine ()

    // When running tests, the working directory is where the dll is located
    let myTask =
        GetCurrentReleaseTask(ChangelogFile = Workspace.fixtures.``CHANGELOG_KeepAChangelog.md``)

    myTask.BuildEngine <- context.BuildEngine.Object

    let success = myTask.Execute()

    context.PrintErrors()

    success.Should().BeTrue() |> ignore
    context.Errors.Count.Should().Be(0) |> ignore

    myTask.CurrentRelease.GetMetadata("Version").Should().Be("0.1.0") |> ignore
    myTask.CurrentRelease.GetMetadata("Date").Should().Be("2022-01-13") |> ignore

    myTask.CurrentRelease
        .GetMetadata("Body")
        .Should()
        .Be(
            """### Added

- Created the package

### Changed

- Updated the package"""
        )
    |> ignore

[<Test>]
let ``task fails no version is detected`` () =
    let context = setupBuildEngine ()

    let myTask =
        GetCurrentReleaseTask(ChangelogFile = Workspace.fixtures.``CHANGELOG_empty.md``)

    myTask.BuildEngine <- context.BuildEngine.Object

    let success = myTask.Execute()

    success.Should().BeFalse() |> ignore
    context.Errors.Count.Should().Be(1) |> ignore
    context.Errors.[0].Code.Should().Be("EPT0002") |> ignore

    context.Errors.[0].Message
        .Should()
        .Be(
            "Could not find the last version in the Changelog file /Users/mmangel/Workspaces/Github/easybuild-org/EasyBuild.PackageReleaseNotes.Tasks/tests/fixtures/CHANGELOG_empty.md. Error: No version found"
        )
    |> ignore
