# EasyBuild.PackageReleaseNotes.Tasks

[![NuGet](https://img.shields.io/nuget/v/EasyBuild.PackageReleaseNotes.Tasks.svg)](https://www.nuget.org/packages/EasyBuild.PackageReleaseNotes.Tasks)

[![Sponsors badge link](https://img.shields.io/badge/Sponsors_this_project-EA4AAA?style=for-the-badge)](https://mangelmaxime.github.io/sponsors/)

This project provides MSBuild tasks to automate setting the following properties based on the content of a CHANGELOG file:

- `Version` - The version of the latest release
- `PackageVersion` - The version of the latest release
- `PackageReleaseNotes` - The body of the latest release

## Usage

1. Install `EasyBuild.PackageReleaseNotes.Tasks` from NuGet

    ```bash
    dotnet add package EasyBuild.PackageReleaseNotes.Tasks
    ```

2. If your CHANGELOG file is not named `CHANGELOG.md`, and beside your project file, you can set the `ChangelogFile` property in your project file.

    ```xml
    <PropertyGroup>
        <ChangelogFile>path/to/CHANGELOG.md</ChangelogFile>
    </PropertyGroup>
    ```

    💡 You can use the MSBuild property `$(MSBuildThisFileDirectory)` to get the directory of the project file.

    ```xml
    <PropertyGroup>
        <!-- This will use an absolute path -->
        <ChangelogFile>$(MSBuildThisFileDirectory)path/to/CHANGELOG.md</ChangelogFile>
    </PropertyGroup>
    ```

## What is the latest release?

The first version at the top of the file which is not `Unreleased` is considered the latest release.

```md
## [Unreleased]

## [1.0.0] - 2021-01-01

## [0.1.0] - 2020-01-01
```

In this example, `1.0.0` is the latest release.

### Supported format

The tool is on purpose flexible on how a version is detected. It allows it to work with various formats like KeepAChangeLog, conventional commits based changelog.

It supports the following formats:

- `## 1.0.0 - 2021-01-01`
- `## [1.0.0] - 2021-01-01`
- `## v1.0.0 - 2021-01-01`
- `## [v1.0.0] - 2021-01-01`
- `## 1.0.0`
- `## [1.0.0]`
- `## v1.0.0`
- `## [v1.0.0]`

<details>
<summary>Regex used to match the version</summary>

```text
^                    # Start of the string
##                   # Match literal "##"
\s                   # Match a space (whitespace character)
\[?                  # Optionally match an opening bracket '['
v?                   # Optionally match a literal 'v' (for version)
(?<version>          # Start a named capture group for "version"
  [\w\d.-]+          # Match one or more word characters (letters, digits), dots, or hyphens
  \.                 # Match a literal dot (.)
  [\w\d.-]+          # Match one or more word characters (letters, digits), dots, or hyphens again
  [a-zA-Z0-9]        # Match a single alphanumeric character (ensures no trailing dot/hyphen)
)                    # End the "version" capture group
\]?                  # Optionally match a closing bracket ']'
\s-\s                # Match a literal space, hyphen, and space " - "
(?<date>             # Start a named capture group for "date"
  \d{4}              # Match exactly 4 digits (year)
  -                  # Match a literal hyphen "-"
  \d{2}              # Match exactly 2 digits (month)
  -                  # Match a literal hyphen "-"
  \d{2}              # Match exactly 2 digits (day)
)?                   # The "date" group is optional (match 0 or 1 times)
$                    # End of the string
```

</details>

## MSBuild properties

The task exposes the following property:

- `CurrentRelease.Version` - The version of the latest release (without the leading `v`)
- `CurrentRelease.Date` - The date of the latest release
- `CurrentRelease.Body` - The body of the latest release

> [!NOTE]
> `CurrentRelease` name is used in the sense that this is the release that is currently being processed.

## Aknowledgements

This project has been inspired by [Ionide.KeepAChangeLog](https://github.com/ionide/KeepAChangelog). If you are using KeepAChangeLog format, you need a deeper integration with your CHANGELOG file, like accessing the different sections of a release, you should take a look at this project.
