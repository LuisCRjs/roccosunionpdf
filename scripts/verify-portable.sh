#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
dotnet_command="${DOTNET_COMMAND:-dotnet}"

cd "$repository_root"
"$dotnet_command" restore tests/DocumentManager.Tests/DocumentManager.Tests.csproj
"$dotnet_command" build tests/DocumentManager.Tests/DocumentManager.Tests.csproj --configuration Release --no-restore --maxcpucount:1
"$dotnet_command" test tests/DocumentManager.Tests/DocumentManager.Tests.csproj --configuration Release --no-restore --no-build
"$dotnet_command" restore tests/DocumentManager.WindowsCodeCheck/DocumentManager.WindowsCodeCheck.csproj --runtime win-x64
"$dotnet_command" build tests/DocumentManager.WindowsCodeCheck/DocumentManager.WindowsCodeCheck.csproj --configuration Release --runtime win-x64 --no-restore --maxcpucount:1

