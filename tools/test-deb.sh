#!/usr/bin/env bash
set -euo pipefail
package="$(realpath "${1:?Pass a deb package}")"
[[ "$(dpkg-deb -f "$package" Package)" == novapen ]]
if dpkg-query -W novapen &>/dev/null; then echo 'Use a clean runner: NovaPen is already installed.' >&2; exit 2; fi
cleanup() { sudo apt-get purge -y novapen; }
trap cleanup EXIT
sudo apt-get install -y "$package"
test -x /opt/novapen/NovaPen
test -x /opt/novapen/runtimes/linux-x64/solver/icas
test -s /opt/novapen/runtimes/linux-x64/solver/COPYING
/usr/bin/novapen --math-solver-test
test -L /usr/bin/novapen
desktop-file-validate /usr/share/applications/novapen.desktop
desktop-file-validate /etc/xdg/autostart/novapen.desktop
grep -Fx 'Exec=/opt/novapen/NovaPen' /usr/share/applications/novapen.desktop
grep -Fx 'Exec=/opt/novapen/NovaPen --minimized' /etc/xdg/autostart/novapen.desktop
xvfb-run -a /usr/bin/novapen --smoke-test --minimized
sudo apt-get install --reinstall -y "$package"
test -x /opt/novapen/NovaPen
/usr/bin/novapen --math-solver-test
cleanup
trap - EXIT
test ! -e /opt/novapen/NovaPen
test ! -e /etc/xdg/autostart/novapen.desktop
echo 'Debian install, launch, reinstall and removal checks passed.'
