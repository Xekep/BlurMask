#!/usr/bin/env bash
# Root only in a disposable Void/glibc container; GUI runs as a dedicated normal user.
set -euo pipefail
package="$(realpath "${1:?Pass a Void x86_64 XBPS package}")"
[[ "$EUID" == 0 ]] || { echo 'Use a clean root-owned Void container.' >&2; exit 2; }
[[ "$(xbps-uhelper arch)" == x86_64 && "$(getconf GNU_LIBC_VERSION)" == glibc* ]]
[[ "$(basename "$package")" =~ ^novapen-[0-9].*\.x86_64\.xbps$ ]]
if xbps-query novapen &>/dev/null; then echo 'NovaPen is already installed.' >&2; exit 2; fi
for path in /opt/novapen /usr/bin/novapen /etc/xdg/autostart/novapen.desktop; do
  [[ ! -e "$path" ]] || { echo "Existing NovaPen path: $path" >&2; exit 2; }
done
repo="$(mktemp -d)"
installed=0
cleanup() {
  local result=$?
  trap - EXIT
  if [[ "$installed" == 1 ]]; then xbps-remove -y novapen || result=1; fi
  rm -rf -- "$repo"
  exit "$result"
}
trap cleanup EXIT
cp "$package" "$repo/"
xbps-rindex -a "$repo/$(basename "$package")"
xbps-query -R --repository="$repo" -p architecture novapen | grep -Fx x86_64
xbps-query -R --repository="$repo" -p license novapen | grep -Fx 'MIT, GPL-3.0-or-later'
xbps-install -y --repository="$repo" novapen
installed=1
test -x /opt/novapen/NovaPen
test -x /opt/novapen/runtimes/linux-x64/solver/icas
test -s /opt/novapen/runtimes/linux-x64/solver/COPYING
test "$(readlink /usr/bin/novapen)" == /opt/novapen/NovaPen
test -s /usr/share/icons/hicolor/scalable/apps/novapen.svg
test -s /usr/share/licenses/novapen/LICENSE
desktop-file-validate /usr/share/applications/novapen.desktop
desktop-file-validate /etc/xdg/autostart/novapen.desktop
grep -Fx 'Exec=/opt/novapen/NovaPen' /usr/share/applications/novapen.desktop
grep -Fx 'Exec=/opt/novapen/NovaPen --minimized' /etc/xdg/autostart/novapen.desktop
(cd /opt/novapen; sha256sum --check /usr/share/doc/novapen/payload.sha256)
for license in THIRD-PARTY-NOTICES.txt LICENSE-FONT-Inter.txt LICENSE-FONT-Manrope.txt \
  LICENSE-FONT-SourceSerif4.txt LICENSE-FONT-STIXTwoMath.txt LICENSE-FONT-STIXTwoText.txt LICENSE-FONT-STIXTwoTextItalic.txt \
  runtimes/linux-x64/native/LICENSE runtimes/linux-x64/native/ThirdPartyNotices.txt runtimes/linux-x64/native/runtime-provenance.json; do
  test -s "/opt/novapen/$license"
done
test -s /opt/novapen/runtimes/linux-x64/native/libonnxruntime.so
while IFS= read -r -d '' elf; do
  dependencies="$(ldd "$elf")"
  printf '%s\n%s\n' "$elf" "$dependencies"
  [[ "$dependencies" != *'not found'* ]]
done < <(find /opt/novapen -type f \( -name NovaPen -o -name icas -o -name '*.so' -o -name '*.so.*' \) -print0)
useradd --create-home novapen-test
runuser -u novapen-test -- /usr/bin/novapen --math-solver-test
runuser -u novapen-test -- /usr/bin/novapen --ai-pen-test
runuser -u novapen-test -- /usr/bin/novapen --overlay-platform-test
# This explicitly checks the autostart mode leaves the application windows unmapped.
runuser -u novapen-test -- xvfb-run -a /usr/bin/novapen --smoke-test --minimized
ui_results="${BLURMASK_UI_RESULTS:-/tmp/void-ui-results}"
install -d -o novapen-test -g novapen-test "$ui_results"
runuser -u novapen-test -- python3 - "$ui_results" <<'PY'
import ctypes as c, hashlib, json, sys
from pathlib import Path
native = Path('/opt/novapen/runtimes/linux-x64/native')
manifest = json.loads((native / 'runtime-provenance.json').read_text())
for name, entry in manifest['files'].items():
    data = (native / name).read_bytes()
    assert len(data) == entry['bytes'] and hashlib.sha256(data).hexdigest() == entry['sha256'], name
# Public ONNX Runtime C API: two function pointers in OrtApiBase, no model needed.
class ApiBase(c.Structure):
    _fields_ = [('get_api', c.CFUNCTYPE(c.c_void_p, c.c_uint32)),
                ('get_version', c.CFUNCTYPE(c.c_char_p))]
library = c.CDLL(str(native / 'libonnxruntime.so'))
library.OrtGetApiBase.restype = c.POINTER(ApiBase)
library.OrtGetApiBase.argtypes = []
base = library.OrtGetApiBase()
assert base and base.contents.get_api(23), 'ONNX Runtime C API23 unavailable on Void'
version = base.contents.get_version().decode('ascii')
assert version == manifest['version'], (version, manifest['version'])
report = dict(version=version, c_api=23, runtime_hashes_verified=True, native_library_loaded=True,
              limitation='C API load/version check; no proprietary model inference.')
Path(sys.argv[1], 'formula-runtime.json').write_text(json.dumps(report, indent=2) + '\n')
print(json.dumps(report))
PY
ui_script="$(dirname "$(realpath "$0")")/test-linux-ui.sh"
runuser -u novapen-test -- bash "$ui_script" /usr/bin/novapen "$ui_results/default"
runuser -u novapen-test -- bash "$ui_script" /usr/bin/novapen "$ui_results/software" --software-rendering
original_hash="$(sha256sum /opt/novapen/NovaPen)"
printf 'outdated payload\n' > /opt/novapen/NovaPen
xbps-install -yf --repository="$repo" novapen
[[ "$(sha256sum /opt/novapen/NovaPen)" == "$original_hash" ]]
runuser -u novapen-test -- /usr/bin/novapen --math-solver-test
(cd /opt/novapen; sha256sum --check /usr/share/doc/novapen/payload.sha256)
xbps-pkgdb --checks=files,dependencies novapen
xbps-remove -y novapen
installed=0
for path in /opt/novapen /usr/bin/novapen /etc/xdg/autostart/novapen.desktop /usr/share/applications/novapen.desktop /usr/share/icons/hicolor/scalable/apps/novapen.svg /usr/share/licenses/novapen /usr/share/doc/novapen; do
  test ! -e "$path"
done
echo 'Void glibc install, native dependencies/licenses, pointer/pixels, reinstall and removal checks passed.'
