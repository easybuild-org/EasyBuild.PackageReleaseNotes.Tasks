module Tests.LastVersionFinderTests

open Tests.Setup
// open Shouldly
open EasyBuild.PackageReleaseNotes.Tasks
open System
open System.IO
open Workspace
open Semver
open Faqt

[<Test>]
let ``should return an Error if no version is found`` () =
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

// I don't know how to pass an invalid version that pass the regex but not SemVer parsing
// [<Test>]
// let ``should return an Error if the version is invalid`` () =
//     let result =
//         LastVersionFinder.tryFindLastVersion "## [this is not a valid version]"

//     result
//         .Should()
//         .BeError()
//         .WhoseValue.Should(())
//         .Be(LastVersionFinder.InvalidVersionFormat "[this is not a valid version]")
//     |> ignore

[<Test>]
let ``should works for `## version - date` format`` () =
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

[<Test>]
let ``should works for `## version` format`` () =
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

[<Test>]
let ``should works for `## version - date` format with body`` () =
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
                Body = """Body line 1

* Change 1

    Indented change 1 description"""
            }
            : LastVersionFinder.Version
        )

[<Test>]
let ``should works for `## [version] - date` format`` () =
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

[<Test>]
let ``should works for `## [version]` format`` () =
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

[<Test>]
let ``should works for `## [version] - date` format with body`` () =
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
                Body = """Body line 1

* Change 1

    Indented change 1 description"""
                }
                : LastVersionFinder.Version
            )
