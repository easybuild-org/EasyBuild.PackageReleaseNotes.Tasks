namespace Tests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open SimpleExec
open BlackFox.CommandLine
open Workspace
open Faqt
open System.Threading.Tasks

module Utils =

    let getPackageProperties projectName =
        Command.ReadAsync(
            "dotnet",
            CmdLine.empty
            |> CmdLine.appendPrefix "pack" projectName
            |> CmdLine.appendPrefix "-c" "Release"
            |> CmdLine.appendRaw "--getProperty:Version"
            |> CmdLine.appendRaw "--getProperty:PackageVersion"
            |> CmdLine.appendRaw "--getProperty:PackageReleaseNotes"
            |> CmdLine.toString,
            workingDirectory = Workspace.fixtures.``.``
        )

[<TestClass>]
type IntegrationTests() =

    member val testPackageVersion = null with get, set

    member this.AddPackageReference(projectName: string) =
        let suffix = projectName.Replace(".fsproj", "")

        this.testPackageVersion <- $"0.0.1-test-{suffix}"

        // Create a package to be used in the tests
        // I didn't find a way to test the MSBuild tasks execution using MSBuild only
        // So each fsproj, will use a package reference to the package created here
        Command.Run(
            "dotnet",
            CmdLine.empty
            |> CmdLine.appendPrefix "pack" "src"
            |> CmdLine.appendPrefix "-c" "Release"
            |> CmdLine.appendPrefix "-o" VirtualWorkspace.packages.``.``
            |> CmdLine.appendRaw $"-p:PackageVersion=%s{this.testPackageVersion}"
            |> CmdLine.toString,
            workingDirectory = Workspace.``..``.``.``
        )

        Command.Run(
            "dotnet",
            CmdLine.empty
            |> CmdLine.appendPrefix "add" projectName
            |> CmdLine.appendPrefix "package" "EasyBuild.PackageReleaseNotes.Tasks"
            // |> CmdLine.appendPrefix "--source" VirtualWorkspace.packages.``.``
            |> CmdLine.appendPrefix "--version" this.testPackageVersion
            |> CmdLine.toString,
            workingDirectory = Workspace.fixtures.``.``
        )

    [<TestMethod>]
    member this.``works for absolute path with conventional commits changelog``() : Task =
        task {
            let projectName = "WorksForAbsolutePathWithConventionalCommitsChangelog.fsproj"

            this.AddPackageReference projectName

            let! struct (stdout, _) = Utils.getPackageProperties projectName

            stdout
                .Should()
                .Be(
                    """{
  "Properties": {
    "Version": "0.10.0",
    "PackageVersion": "0.10.0",
    "PackageReleaseNotes": "### \uD83D\uDE80 Features\n\n* Feature 1\n\n### \uD83D\uDC1E Bug Fixes\n\n* Bug fix 1\n* Bug fix 2"
  }
}
"""
                )
            |> ignore
        }

    [<TestMethod>]
    member this.``works for relative path with conventional commits changelog``() : Task =
        task {
            let projectName = "WorksForRelativePathWithConventionalCommitsChangelog.fsproj"

            this.AddPackageReference projectName

            let! struct (stdout, _) = Utils.getPackageProperties projectName

            stdout
                .Should()
                .Be(
                    """{
  "Properties": {
    "Version": "0.10.0",
    "PackageVersion": "0.10.0",
    "PackageReleaseNotes": "### \uD83D\uDE80 Features\n\n* Feature 1\n\n### \uD83D\uDC1E Bug Fixes\n\n* Bug fix 1\n* Bug fix 2"
  }
}
"""
                )
            |> ignore
        }

    [<TestMethod>]
    member this.``works for absolute path with keep a changelog``() : Task =
        task {
            let projectName = "WorksForAbsolutePathWithKeepAChangelog.fsproj"

            this.AddPackageReference projectName

            let! struct (stdout, _) = Utils.getPackageProperties projectName

            stdout
                .Should()
                .Be(
                    """{
  "Properties": {
    "Version": "0.1.0",
    "PackageVersion": "0.1.0",
    "PackageReleaseNotes": "### Added\n\n- Created the package\n\n### Changed\n\n- Updated the package"
  }
}
"""
                )
            |> ignore
        }

    [<TestMethod>]
    member this.``works for relative path with keep a changelog``() : Task =
        task {
            let projectName = "WorksForRelativePathWithKeepAChangelog.fsproj"

            this.AddPackageReference projectName

            let! struct (stdout, _) = Utils.getPackageProperties projectName

            stdout
                .Should()
                .Be(
                    """{
  "Properties": {
    "Version": "0.1.0",
    "PackageVersion": "0.1.0",
    "PackageReleaseNotes": "### Added\n\n- Created the package\n\n### Changed\n\n- Updated the package"
  }
}
"""
                )
            |> ignore
        }

    [<TestMethod>]
    member this.``works with default CHANGELOG.md if no changelog is specified``() : Task =
        task {
            let projectName = "DefaultToChangelogIfNotSpecified.fsproj"

            this.AddPackageReference projectName

            let! struct (stdout, _) = Utils.getPackageProperties projectName

            stdout
                .Should()
                .Be(
                    """{
  "Properties": {
    "Version": "10.0.99",
    "PackageVersion": "10.0.99",
    "PackageReleaseNotes": "### \uD83D\uDE80 Features\n\n- Feature 1"
  }
}
"""
                )
            |> ignore
        }

    [<TestMethod>]
    member this.``works for changelog with no version yet``() : Task =
        task {

            let projectName = "WorksForChangelogWithNoVersion.fsproj"

            this.AddPackageReference projectName

            let! struct (stdout, _) = Utils.getPackageProperties projectName

            stdout
                .Should()
                .Be(
                    """{
  "Properties": {
    "Version": "0.0.0",
    "PackageVersion": "0.0.0",
    "PackageReleaseNotes": ""
  }
}
"""
                )
            |> ignore
        }
