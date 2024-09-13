module Tests.Setup

open Fixie
open System
open System.Collections.Generic
open System.Reflection
open SimpleExec
open BlackFox.CommandLine

let testPackageVersion =
    let suffix = DateTimeOffset.Now.ToUnixTimeSeconds()

    $"0.0.1-test-{suffix}"

[<AttributeUsage(AttributeTargets.Method)>]
type TestAttribute() =
    inherit Attribute()

type TestModuleDiscovery() =
    interface IDiscovery with
        member _.TestClasses(concreteClasses: IEnumerable<Type>) =
            concreteClasses
            |> Seq.filter (fun cls ->
                cls.GetMembers() |> Seq.exists (fun m -> m.Has<TestAttribute>())
            )

        member _.TestMethods(publicMethods: IEnumerable<MethodInfo>) =
            publicMethods |> Seq.filter (fun x -> x.Has<TestAttribute>() && x.IsStatic)

type TestProject() =
    interface ITestProject with
        member _.Configure(configuration: TestConfiguration, environment: TestEnvironment) =
            configuration.Conventions.Add<TestModuleDiscovery, TestProject>()

    interface IExecution with

        member _.Run(testSuite: TestSuite) =
            task {
                for testClass in testSuite.TestClasses do
                    for test in testClass.Tests do
                        // Create a package to be used in the tests
                        // I didn't find a way to test the MSBuild tasks execution using MSBuild only
                        // So each fsproj, will use a package reference to the package created here
                        Command.Run(
                            "dotnet",
                            CmdLine.empty
                            |> CmdLine.appendPrefix "pack" "src"
                            |> CmdLine.appendPrefix "-c" "Release"
                            |> CmdLine.appendPrefix "-o" "packages"
                            |> CmdLine.appendRaw $"-p:PackageVersion=%s{testPackageVersion}"
                            |> CmdLine.toString,
                            workingDirectory = Workspace.Workspace.``..``.``.``
                        )

                        let! _ = test.Run()
                        ()
            }
