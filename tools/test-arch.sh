#!/usr/bin/env bash
# Run only as root in a clean Arch container/runner, never on a user's installation.
set -euo pipefail
package="$(realpath "${1:?Pass an Arch package}")"
[[ "$EUID" == 0 ]] || { echo 'Use a clean root-owned Arch container.' >&2; exit 2; }
[[ "$(pacman -Qp "$package" | cut -d' ' -f1)" == novapen ]]
if pacman -Q novapen &>/dev/null; then echo 'NovaPen is already installed.' >&2; exit 2; fi
if [[ -e /opt/novapen || -e /etc/xdg/autostart/novapen.desktop ]]; then echo 'Existing NovaPen files found.' >&2; exit 2; fi
cleanup() { pacman -R --noconfirm novapen; }
trap cleanup EXIT
pacman -U --noconfirm "$package"
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
while IFS= read -r -d '' elf; do
  dependencies="$(ldd "$elf")"
  printf '%s\n%s\n' "$elf" "$dependencies"
  if [[ "$dependencies" == *'not found'* ]]; then exit 1; fi
done < <(find /opt/novapen -type f \( -name NovaPen -o -name icas -o -name '*.so' \) -print0)
useradd --create-home novapen-test
runuser -u novapen-test -- /usr/bin/novapen --math-solver-test
runuser -u novapen-test -- /usr/bin/novapen --ai-pen-test
runuser -u novapen-test -- /usr/bin/novapen --overlay-platform-test
runuser -u novapen-test -- xvfb-run -a /usr/bin/novapen --smoke-test --minimized
ui_results="${BLURMASK_UI_RESULTS:-/tmp/arch-ui-results}"
install -d -o novapen-test -g novapen-test "$ui_results"
ui_script="$(dirname "$(realpath "$0")")/test-linux-ui.sh"
runuser -u novapen-test -- bash "$ui_script" /usr/bin/novapen "$ui_results/default"
runuser -u novapen-test -- bash "$ui_script" /usr/bin/novapen "$ui_results/software" --software-rendering
original_hash="$(sha256sum /opt/novapen/NovaPen)"
printf 'outdated payload\n' > /opt/novapen/NovaPen
pacman -U --noconfirm "$package"
[[ "$(sha256sum /opt/novapen/NovaPen)" == "$original_hash" ]]
runuser -u novapen-test -- /usr/bin/novapen --math-solver-test
pacman -Qkk novapen
cleanup
trap - EXIT
for path in /opt/novapen /usr/bin/novapen /etc/xdg/autostart/novapen.desktop /usr/share/applications/novapen.desktop /usr/share/icons/hicolor/scalable/apps/novapen.svg /usr/share/licenses/novapen; do
  test ! -e "$path"
done
echo 'Arch install, dependencies, real UI input/pixels, reinstall and removal checks passed.'
