module EasyBuild.PackageReleaseNotes.Tasks.Log

(*
    Log module allowing to define log data for different errors.

    It makes it easier to keep track of the error codes, because everything is in one place.

    Notes:

    - EPT in the error codes stands for EasyBuild PackageReleaseNotes Task
                                        ^         ^                   ^
                                        E         P                   T
*)

type LogData =
    {
        ErrorCode: string
        HelpKeyword: string
        Message: string
        MessageArgs: obj array
    }

let changelogFileNotFound (filePath: string) =
    {
        ErrorCode = "EPT0001"
        HelpKeyword = "Missing Changelog file"
        Message = "The Changelog file {0} was not found."
        MessageArgs = [| box filePath |]
    }

let lastVersionNotFound (filePath: string) (error: LastVersionFinder.Errors) =
    {
        ErrorCode = "EPT0002"
        HelpKeyword = "Last version not found"
        Message = "Could not find the last version in the Changelog file {0}. Error: {1}"
        MessageArgs = [| box filePath; box (error.ToText()) |]
    }
