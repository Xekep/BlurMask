#!/usr/bin/env bash
set -euo pipefail
package="$(realpath "${1:?Pass a deb package}")"
[[ "$(dpkg-deb -f "$package" Package)" == blurmask ]]
if dpkg-query -W blurmask &>/dev/null; then echo 'Use a clean runner: BlurMask is already installed.' >&2; exit 2; fi
cleanup() { sudo apt-get purge -y blurmask; }
trap cleanup EXIT
sudo apt-get install -y "$package"
test -x /opt/blurmask/BlurMask
test -L /usr/bin/blurmask
desktop-file-validate /usr/share/applications/blurmask.desktop
desktop-file-validate /etc/xdg/autostart/blurmask.desktop
grep -Fx 'Exec=/opt/blurmask/BlurMask' /usr/share/applications/blurmask.desktop
grep -Fx 'Exec=/opt/blurmask/BlurMask --minimized' /etc/xdg/autostart/blurmask.desktop
xvfb-run -a /usr/bin/blurmask --smoke-test --minimized
sudo apt-get install --reinstall -y "$package"
test -x /opt/blurmask/BlurMask
cleanup
trap - EXIT
test ! -e /opt/blurmask/BlurMask
test ! -e /etc/xdg/autostart/blurmask.desktop
echo 'Debian install, launch, reinstall and removal checks passed.'
