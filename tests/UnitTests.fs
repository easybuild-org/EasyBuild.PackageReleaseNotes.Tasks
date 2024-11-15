namespace Tests

open Microsoft.VisualStudio.TestTools.UnitTesting
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

[<TestClass>]
type UnitTests() =

    member val context = Unchecked.defaultof<TestContext> with get, set

    [<TestInitialize>]
    member this.Initialize() =
        this.context <-
            {
                BuildEngine = Mock<IBuildEngine>()
                Errors = ResizeArray<BuildErrorEventArgs>()
            }

        this.context.BuildEngine
            .Setup(fun engine -> engine.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
            .Callback(fun (args: BuildErrorEventArgs) -> this.context.Errors.Add(args))
        |> ignore

    [<TestMethod>]
    member this.``task fails when changelog file does not exist``() =
        let myTask = GetCurrentReleaseTask(ChangelogFile = "ThisFileDoesNotExist.md")
        myTask.BuildEngine <- this.context.BuildEngine.Object

        let success = myTask.Execute()

        success.Should().BeFalse() |> ignore
        this.context.Errors.Count.Should().Be(1) |> ignore
        this.context.Errors.[0].Code.Should().Be("EPT0001") |> ignore

    [<TestMethod>]
    member this.``task succeeds when changelog file exists (relative path + ConventionalCommits)``
        ()
        =
        // When running tests, the working directory is where the dll is located
        let myTask =
            GetCurrentReleaseTask(
                ChangelogFile = "./../../../fixtures/CHANGELOG_ConventionalCommits.md"
            )

        myTask.BuildEngine <- this.context.BuildEngine.Object

        let success = myTask.Execute()

        this.context.PrintErrors()

        success.Should().BeTrue() |> ignore
        this.context.Errors.Count.Should().Be(0) |> ignore

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

    [<TestMethod>]
    member this.``task succeeds when changelog file exists (absolute path + KeepAChangeLog format)``
        ()
        =
        // When running tests, the working directory is where the dll is located
        let myTask =
            GetCurrentReleaseTask(
                ChangelogFile = Workspace.fixtures.``CHANGELOG_KeepAChangelog.md``
            )

        myTask.BuildEngine <- this.context.BuildEngine.Object

        let success = myTask.Execute()

        this.context.PrintErrors()

        success.Should().BeTrue() |> ignore
        this.context.Errors.Count.Should().Be(0) |> ignore

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

    [<TestMethod>]
    member this.``task fails no version is detected``() =
        let myTask =
            GetCurrentReleaseTask(ChangelogFile = Workspace.fixtures.``CHANGELOG_invalid.md``)

        myTask.BuildEngine <- this.context.BuildEngine.Object

        let success = myTask.Execute()

        success.Should().BeFalse() |> ignore
        this.context.Errors.Count.Should().Be(1) |> ignore
        this.context.Errors.[0].Code.Should().Be("EPT0002") |> ignore

        this.context.Errors.[0].Message
            .Should()
            .Contain("Could not find the last version in the Changelog file")
        |> ignore
