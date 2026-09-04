#!/usr/bin/env bash
set -euo pipefail

RID="${1:-linux-x64}"
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

echo "Publishing BlurMask (.NET 11 Native AOT) for $RID..."
dotnet publish ./BlurMask.csproj \
  -c Release \
  -r "$RID" \
  --self-contained true \
  -p:PublishAot=true

echo "Published to: bin/Release/net11.0/$RID/publish/"
