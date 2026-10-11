#!/usr/bin/env bash
# Root only in a disposable Void/glibc container; GUI runs as a dedicated normal user.
set -euo pipefail
package="$(realpath "${1:?Pass a Void x86_64 XBPS package}")"
[[ "$EUID" == 0 ]] || { echo 'Use a clean root-owned Void container.' >&2; exit 2; }
[[ "$(xbps-uhelper arch)" == x86_64 && "$(getconf GNU_LIBC_VERSION)" == glibc* ]]
[[ "$(basename "$package")" =~ ^blurmask-[0-9].*\.x86_64\.xbps$ ]]
if xbps-query blurmask &>/dev/null; then echo 'BlurMask is already installed.' >&2; exit 2; fi
for path in /opt/blurmask /usr/bin/blurmask /etc/xdg/autostart/blurmask.desktop; do
  [[ ! -e "$path" ]] || { echo "Existing BlurMask path: $path" >&2; exit 2; }
done
repo="$(mktemp -d)"
installed=0
cleanup() {
  local result=$?
  trap - EXIT
  if [[ "$installed" == 1 ]]; then xbps-remove -y blurmask || result=1; fi
  rm -rf -- "$repo"
  exit "$result"
}
trap cleanup EXIT
cp "$package" "$repo/"
xbps-rindex -a "$repo/$(basename "$package")"
xbps-query -R --repository="$repo" -p architecture blurmask | grep -Fx x86_64
xbps-query -R --repository="$repo" -p license blurmask | grep -Fx 'MIT, GPL-3.0-or-later'
xbps-install -y --repository="$repo" blurmask
installed=1
test -x /opt/blurmask/BlurMask
test -x /opt/blurmask/runtimes/linux-x64/solver/icas
test -s /opt/blurmask/runtimes/linux-x64/solver/COPYING
test "$(readlink /usr/bin/blurmask)" == /opt/blurmask/BlurMask
test -s /usr/share/icons/hicolor/scalable/apps/blurmask.svg
test -s /usr/share/licenses/blurmask/LICENSE
desktop-file-validate /usr/share/applications/blurmask.desktop
desktop-file-validate /etc/xdg/autostart/blurmask.desktop
grep -Fx 'Exec=/opt/blurmask/BlurMask' /usr/share/applications/blurmask.desktop
grep -Fx 'Exec=/opt/blurmask/BlurMask --minimized' /etc/xdg/autostart/blurmask.desktop
(cd /opt/blurmask; sha256sum --check /usr/share/doc/blurmask/payload.sha256)
for license in THIRD-PARTY-NOTICES.txt LICENSE-FONT-Inter.txt LICENSE-FONT-Manrope.txt \
  LICENSE-FONT-SourceSerif4.txt LICENSE-FONT-STIXTwoMath.txt LICENSE-FONT-STIXTwoText.txt LICENSE-FONT-STIXTwoTextItalic.txt \
  runtimes/linux-x64/native/LICENSE runtimes/linux-x64/native/ThirdPartyNotices.txt runtimes/linux-x64/native/runtime-provenance.json; do
  test -s "/opt/blurmask/$license"
done
test -s /opt/blurmask/runtimes/linux-x64/native/libonnxruntime.so
while IFS= read -r -d '' elf; do
  dependencies="$(ldd "$elf")"
  printf '%s\n%s\n' "$elf" "$dependencies"
  [[ "$dependencies" != *'not found'* ]]
done < <(find /opt/blurmask -type f \( -name BlurMask -o -name icas -o -name '*.so' -o -name '*.so.*' \) -print0)
useradd --create-home blurmask-test
runuser -u blurmask-test -- /usr/bin/blurmask --math-solver-test
runuser -u blurmask-test -- /usr/bin/blurmask --ai-pen-test
runuser -u blurmask-test -- /usr/bin/blurmask --overlay-platform-test
# This explicitly checks the autostart mode leaves the application windows unmapped.
runuser -u blurmask-test -- xvfb-run -a /usr/bin/blurmask --smoke-test --minimized
ui_results="${BLURMASK_UI_RESULTS:-/tmp/void-ui-results}"
install -d -o blurmask-test -g blurmask-test "$ui_results"
runuser -u blurmask-test -- python3 - "$ui_results" <<'PY'
import ctypes as c, hashlib, json, sys
from pathlib import Path
native = Path('/opt/blurmask/runtimes/linux-x64/native')
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
runuser -u blurmask-test -- bash "$ui_script" /usr/bin/blurmask "$ui_results/default"
runuser -u blurmask-test -- bash "$ui_script" /usr/bin/blurmask "$ui_results/software" --software-rendering
original_hash="$(sha256sum /opt/blurmask/BlurMask)"
printf 'outdated payload\n' > /opt/blurmask/BlurMask
xbps-install -yf --repository="$repo" blurmask
[[ "$(sha256sum /opt/blurmask/BlurMask)" == "$original_hash" ]]
runuser -u blurmask-test -- /usr/bin/blurmask --math-solver-test
(cd /opt/blurmask; sha256sum --check /usr/share/doc/blurmask/payload.sha256)
xbps-pkgdb --checks=files,dependencies blurmask
xbps-remove -y blurmask
installed=0
for path in /opt/blurmask /usr/bin/blurmask /etc/xdg/autostart/blurmask.desktop /usr/share/applications/blurmask.desktop /usr/share/icons/hicolor/scalable/apps/blurmask.svg /usr/share/licenses/blurmask /usr/share/doc/blurmask; do
  test ! -e "$path"
done
echo 'Void glibc install, native dependencies/licenses, pointer/pixels, reinstall and removal checks passed.'
