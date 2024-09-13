#!/bin/sh

epoch_time=$(date +%s)

# Build the project
dotnet pack src -c Release -o packages -p:PackageVersion=1.0.0-test-$epoch_time

# Change directory to the test project
cd tests/fixtures/valid
# Update the package reference to the latest version
dotnet add Simple.fsproj \
    package EasyBuild.PackageReleaseNotes.Tasks \
    --source ../../../packages \
    --version 1.0.0-test-$epoch_time
# Pack the project for testing
dotnet pack Simple.fsproj
