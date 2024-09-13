namespace EasyBuild.PackageReleaseNotes.Tasks

open Microsoft.Build.Utilities
open Microsoft.Build.Framework
open System.IO
open FsToolkit.ErrorHandling

module Result =

    let toBool =
        function
        | Ok _ -> true
        | Error _ -> false

type GetCurrentReleaseTask() =
    inherit Task()

    [<Required>]
    member val ChangelogFile: string = null with get, set

    [<Output>]
    // member val CurrentRelease: ITaskItem = TaskItem() with get, set
    member val CurrentRelease: ITaskItem = null with get, set

    override this.Execute() : bool =
        // failwith "Not implemented"
        let file = this.ChangelogFile |> FileInfo

        // Using result CE to make code easier to read by avoiding nested if statements
        result {
            do! this.CheckFileExists file
            let! lastVersion = this.FintLastVersion file

            this.StoreLastVersion lastVersion

            // Done
            return true
        }
        |> Result.toBool

    member this.StoreLastVersion (version: LastVersionFinder.Version) =
        this.CurrentRelease <- TaskItem()
        this.CurrentRelease.ItemSpec <- version.Version.ToString()
        this.CurrentRelease.SetMetadata("Version", version.Version.ToString())
        this.CurrentRelease.SetMetadata("Date",
            match version.Date with
            | Some date -> date.ToString("yyyy-MM-dd")
            | None -> ""
        )
        this.CurrentRelease.SetMetadata("Body", version.Body)

    member this.CheckFileExists(fileInfo: FileInfo) =
        if fileInfo.Exists then
            Ok()
        else
            this.LogError(Log.changelogFileNotFound fileInfo.FullName)
            Error()

    member this.FintLastVersion(fileInfo: FileInfo) : Result<LastVersionFinder.Version, unit> =
        let changelogContent = File.ReadAllText(fileInfo.FullName)

        match LastVersionFinder.tryFindLastVersion changelogContent with
        | Ok version -> Ok version
        | Error error ->
            this.LogError(Log.lastVersionNotFound fileInfo.FullName error)
            Error()

    /// <summary>
    /// Helper method to log an error with the given log data.
    /// </summary>
    /// <param name="logData"></param>
    member this.LogError(logData: Log.LogData) =
        this.Log.LogError(
            "CHANGELOG",
            logData.ErrorCode,
            logData.HelpKeyword,
            this.BuildEngine.ProjectFileOfTaskNode,
            this.BuildEngine.LineNumberOfTaskNode,
            this.BuildEngine.ColumnNumberOfTaskNode,
            this.BuildEngine.LineNumberOfTaskNode,
            this.BuildEngine.ColumnNumberOfTaskNode,
            logData.Message,
            logData.MessageArgs
        )
