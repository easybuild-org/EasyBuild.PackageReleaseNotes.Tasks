namespace Tests

open Microsoft.VisualStudio.TestTools.UnitTesting
open EasyBuild.PackageReleaseNotes.Tasks
open System
open System.IO
open Workspace
open Semver
open Faqt

[<TestClass>]
type LastVersionFinderTests() =

    [<TestMethod>]
    member this.``should return an Error if no version is found``() =
        let result = LastVersionFinder.tryFindLastVersion ""

        result
            .Should()
            .BeError()
            .WhoseValue.Should(())
            .Be(LastVersionFinder.Errors.NoVersionFound)
        |> ignore

        let result =
            LastVersionFinder.tryFindLastVersion
                "# Changelog

Changelog description

"

        result
            .Should()
            .BeError()
            .WhoseValue.Should(())
            .Be(LastVersionFinder.Errors.NoVersionFound)
        |> ignore

    [<TestMethod>]
    member this.``UNRELEASED should be skipped``() =
        let result =
            LastVersionFinder.tryFindLastVersion
                "# Changelog

Changelog description

## [Unreleased]

## [1.0.0] - 2021-10-10
"

        result
            .Should()
            .BeOk()
            .WhoseValue.Should(())
            .Be(
                {
                    Version = SemVersion(1, 0, 0)
                    Date = Some(DateTime(2021, 10, 10))
                    Body = ""
                }
                : LastVersionFinder.Version
            )
        |> ignore

    [<TestMethod>]
    member this.``should return an Error if not a valid version``() =
        let result =
            LastVersionFinder.tryFindLastVersion
                "# Changelog

## [something] - 2022-01-13
"

        result
            .Should()
            .BeError()
            .WhoseValue.Should(())
            .Be(LastVersionFinder.Errors.InvalidVersionFormat "something")
        |> ignore

        let result =
            LastVersionFinder.tryFindLastVersion
                "# Changelog

## this-is-not-a-valid-version
"

        result
            .Should()
            .BeError()
            .WhoseValue.Should(())
            .Be(LastVersionFinder.Errors.InvalidVersionFormat "this-is-not-a-valid-version")
        |> ignore

    [<TestMethod>]
    member this.``should works for `## version - date` format``() =
        let result = LastVersionFinder.tryFindLastVersion "## 1.0.0 - 2021-10-10"

        result
            .Should()
            .BeOk()
            .WhoseValue.Should(())
            .Be(
                {
                    Version = SemVersion(1, 0, 0)
                    Date = Some(DateTime(2021, 10, 10))
                    Body = ""
                }
                : LastVersionFinder.Version
            )
        |> ignore

    [<TestMethod>]
    member this.``should works for `## version` format``() =
        let result = LastVersionFinder.tryFindLastVersion "## 1.0.0"

        result
            .Should()
            .BeOk()
            .WhoseValue.Should(())
            .Be(
                {
                    Version = SemVersion(1, 0, 0)
                    Date = None
                    Body = ""
                }
                : LastVersionFinder.Version
            )
        |> ignore

    [<TestMethod>]
    member this.``should works for `## version - date` format with body``() =
        let result =
            LastVersionFinder.tryFindLastVersion
                """## 1.0.0 - 2021-10-10

Body line 1

* Change 1

    Indented change 1 description

        """

        result
            .Should()
            .BeOk()
            .WhoseValue.Should(())
            .Be(
                {
                    Version = SemVersion(1, 0, 0)
                    Date = Some(DateTime(2021, 10, 10))
                    Body =
                        """Body line 1

* Change 1

    Indented change 1 description"""
                }
                : LastVersionFinder.Version
            )
        |> ignore

    [<TestMethod>]
    member this.``should works for `## [version] - date` format``() =
        let result = LastVersionFinder.tryFindLastVersion "## [1.0.0] - 2021-10-10"

        result
            .Should()
            .BeOk()
            .WhoseValue.Should(())
            .Be(
                {
                    Version = SemVersion(1, 0, 0)
                    Date = Some(DateTime(2021, 10, 10))
                    Body = ""
                }
                : LastVersionFinder.Version
            )
        |> ignore

    [<TestMethod>]
    member this.``should works for `## [version]` format``() =
        let result = LastVersionFinder.tryFindLastVersion "## [1.0.0]"

        result
            .Should()
            .BeOk()
            .WhoseValue.Should(())
            .Be(
                {
                    Version = SemVersion(1, 0, 0)
                    Date = None
                    Body = ""
                }
                : LastVersionFinder.Version
            )
        |> ignore

    [<TestMethod>]
    member this.``should works for `## [version] - date` format with body``() =
        let result =
            LastVersionFinder.tryFindLastVersion
                """## [1.0.0] - 2021-10-10

Body line 1

* Change 1

    Indented change 1 description

        """

        result
            .Should()
            .BeOk()
            .WhoseValue.Should(())
            .Be(
                {
                    Version = SemVersion(1, 0, 0)
                    Date = Some(DateTime(2021, 10, 10))
                    Body =
                        """Body line 1

* Change 1

    Indented change 1 description"""
                }
                : LastVersionFinder.Version
            )
        |> ignore

// I don't know how to pass an invalid version that pass the regex but not SemVer parsing
// [<TestMethod>]
// member this.let ``should return an Error if the version is invalid`` () =
//     let result =
//         LastVersionFinder.tryFindLastVersion "## [this is not a valid version]"

//     result
//         .Should()
//         .BeError()
//         .WhoseValue.Should(())
//         .Be(LastVersionFinder.InvalidVersionFormat "[this is not a valid version]")
//     |> ignore
