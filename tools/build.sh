#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

RID="${1:-linux-x64}"
VERSION="${2:-}"
publish_args=(publish ./src/NovaPen/NovaPen.csproj -c Release -r "$RID" --self-contained true -p:PublishAot=true)
if [[ -n "$VERSION" ]]; then
  if [[ ! "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+(\.[0-9]+)?(-[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?$ ]]; then
    echo "Invalid application version: $VERSION" >&2; exit 2
  fi
  publish_args+=("-p:Version=$VERSION")
fi
case "$RID" in
  win-x64|win-arm64|linux-x64|linux-arm64|osx-x64|osx-arm64) ;;
  *) echo "Unsupported RID: $RID" >&2; exit 2 ;;
esac

case "$(uname -s)" in
  Linux)  PREFIX="linux-" ;;
  Darwin) PREFIX="osx-" ;;
  MINGW*|MSYS*|CYGWIN*) PREFIX="win-" ;;
  *) echo "Unsupported host OS: $(uname -s)" >&2; exit 2 ;;
esac

if [[ "$RID" != "$PREFIX"* ]]; then
  echo "Native AOT does not support cross-OS publishing. Host prefix: $PREFIX, requested RID: $RID" >&2
  exit 2
fi

echo "Publishing NovaPen (.NET 11 Native AOT) for $RID..."
dotnet "${publish_args[@]}"

case "$RID" in
  linux-x64|osx-x64|osx-arm64)
    python3 tools/fetch-ink-math-runtime.py "$RID" --output "src/NovaPen/bin/Release/net11.0/$RID/publish" ;;
  *) echo "Formula runtime is not packaged for $RID; handwritten ink and text recognition remain available." ;;
esac

case "$RID" in
  linux-x64|osx-x64|osx-arm64)
    python3 tools/package-experimental-math-model.py --output "src/NovaPen/bin/Release/net11.0/$RID/publish" --rid "$RID"
    solver_dir=".artifacts/giac-runtime/$RID"
    bash tools/prepare-giac-runtime.sh "$RID" "$solver_dir" .artifacts/installers
    install -d "src/NovaPen/bin/Release/net11.0/$RID/publish/runtimes/$RID/solver"
    cp -a "$solver_dir/." "src/NovaPen/bin/Release/net11.0/$RID/publish/runtimes/$RID/solver/" ;;
  *) echo "Bundled Giac is not available for $RID." ;;
esac

echo "Published to: src/NovaPen/bin/Release/net11.0/$RID/publish/"
