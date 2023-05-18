#!/bin/bash

# How to use
# In a terminal, run the following from the solution directory
# 1. ./scripts/publish_nuget_package.sh <version-name>

PACKAGE_VERSION=$1
ACCESS_TOKEN=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

dotnet pack ./Kava/Kava.csproj -p:Version="${PACKAGE_VERSION}" -c Release

dotnet nuget push ./Kava/bin/Release/*.nupkg --api-key $ACCESS_TOKEN --source "https://nuget.pkg.github.com/Kava-Up-LLC/index.json"