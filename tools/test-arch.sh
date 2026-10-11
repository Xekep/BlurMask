#!/usr/bin/env bash
# Run only as root in a clean Arch container/runner, never on a user's installation.
set -euo pipefail
package="$(realpath "${1:?Pass an Arch package}")"
[[ "$EUID" == 0 ]] || { echo 'Use a clean root-owned Arch container.' >&2; exit 2; }
[[ "$(pacman -Qp "$package" | cut -d' ' -f1)" == blurmask ]]
if pacman -Q blurmask &>/dev/null; then echo 'BlurMask is already installed.' >&2; exit 2; fi
if [[ -e /opt/blurmask || -e /etc/xdg/autostart/blurmask.desktop ]]; then echo 'Existing BlurMask files found.' >&2; exit 2; fi
cleanup() { pacman -R --noconfirm blurmask; }
trap cleanup EXIT
pacman -U --noconfirm "$package"
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
while IFS= read -r -d '' elf; do
  dependencies="$(ldd "$elf")"
  printf '%s\n%s\n' "$elf" "$dependencies"
  if [[ "$dependencies" == *'not found'* ]]; then exit 1; fi
done < <(find /opt/blurmask -type f \( -name BlurMask -o -name icas -o -name '*.so' \) -print0)
useradd --create-home blurmask-test
runuser -u blurmask-test -- /usr/bin/blurmask --math-solver-test
runuser -u blurmask-test -- /usr/bin/blurmask --ai-pen-test
runuser -u blurmask-test -- /usr/bin/blurmask --overlay-platform-test
runuser -u blurmask-test -- xvfb-run -a /usr/bin/blurmask --smoke-test --minimized
ui_results="${BLURMASK_UI_RESULTS:-/tmp/arch-ui-results}"
install -d -o blurmask-test -g blurmask-test "$ui_results"
ui_script="$(dirname "$(realpath "$0")")/test-linux-ui.sh"
runuser -u blurmask-test -- bash "$ui_script" /usr/bin/blurmask "$ui_results/default"
runuser -u blurmask-test -- bash "$ui_script" /usr/bin/blurmask "$ui_results/software" --software-rendering
original_hash="$(sha256sum /opt/blurmask/BlurMask)"
printf 'outdated payload\n' > /opt/blurmask/BlurMask
pacman -U --noconfirm "$package"
[[ "$(sha256sum /opt/blurmask/BlurMask)" == "$original_hash" ]]
runuser -u blurmask-test -- /usr/bin/blurmask --math-solver-test
pacman -Qkk blurmask
cleanup
trap - EXIT
for path in /opt/blurmask /usr/bin/blurmask /etc/xdg/autostart/blurmask.desktop /usr/share/applications/blurmask.desktop /usr/share/icons/hicolor/scalable/apps/blurmask.svg /usr/share/licenses/blurmask; do
  test ! -e "$path"
done
echo 'Arch install, dependencies, real UI input/pixels, reinstall and removal checks passed.'
