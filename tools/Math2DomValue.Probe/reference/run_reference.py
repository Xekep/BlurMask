"""Build the pinned independent C reference and check frozen Decimal tuples.

Windows uses the installed x64 MSVC/Windows SDK directly (no shell setup).
Linux/macOS use cc. No network, vendor runtime or application process is used.
"""
import argparse
import hashlib
import os
import shutil
import sys
import time
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
sys.path.insert(0, str(ROOT / 'tools/handwriting'))
from run_port_checks import execute, write_json

SHA = 'ef79323711f162150ec377774320049f10733bd9bbdddcd4f1583e8c6ec25249'


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def compiler_command(output, environment):
    binary = output / ('reference.exe' if os.name == 'nt' else 'reference')
    if os.name != 'nt':
        compiler = shutil.which('cc')
        if not compiler:
            raise RuntimeError('A C99 compiler is required for the independent reference.')
        return [compiler, '-O2', '-std=c99', '-fno-fast-math', '-ffp-contract=off',
                str(HERE / 'driver.c'), '-lm', '-o', str(binary)], Path(compiler), binary
    programs = Path(os.environ.get('ProgramFiles', 'C:/Program Files'))
    candidates = sorted(programs.glob('Microsoft Visual Studio/*/*/VC/Tools/MSVC/*/bin/Hostx64/x64/cl.exe'))
    if not candidates:
        raise RuntimeError('Installed x64 MSVC tools are required for the independent reference.')
    compiler = candidates[-1]
    vc = compiler.parents[3]
    sdk = Path(os.environ.get('ProgramFiles(x86)', 'C:/Program Files (x86)')) / 'Windows Kits/10'
    versions = sorted((p.name for p in (sdk / 'Include').iterdir()
                       if (p / 'ucrt/stdio.h').is_file() and (sdk / 'Lib' / p.name / 'ucrt/x64').is_dir()),
                      key=lambda name: tuple(map(int, name.split('.'))))
    if not versions:
        raise RuntimeError('A complete Windows UCRT SDK is required.')
    version = versions[-1]
    environment['PATH'] = str(compiler.parent) + os.pathsep + environment.get('PATH', '')
    command = [str(compiler), '/nologo', '/O2', '/fp:precise', '/MT', '/TC', '/D_CRT_SECURE_NO_WARNINGS',
               '/I' + str(vc / 'include'), '/I' + str(sdk / 'Include' / version / 'ucrt'),
               str(HERE / 'driver.c'), '/Fe:' + str(binary), '/Fo:' + str(output / 'reference.obj'), '/link',
               '/LIBPATH:' + str(vc / 'lib/x64'), '/LIBPATH:' + str(sdk / 'Lib' / version / 'ucrt/x64'),
               '/LIBPATH:' + str(sdk / 'Lib' / version / 'um/x64')]
    return command, compiler, binary


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('controls', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    controls, output = args.controls.resolve(), args.output.resolve()
    output.mkdir(parents=True, exist_ok=False)
    report = dict(status='running', vendorExecution=False, steps=[])
    environment = os.environ.copy()
    environment['PYTHONDONTWRITEBYTECODE'] = '1'
    environment.pop('PYTHONOPTIMIZE', None)
    pinned = [HERE / 'dtoa.c', HERE / 'driver.c', controls, controls.with_suffix('.tsv')]
    pins = {str(p): digest(p) for p in pinned}
    report['inputPins'] = pins
    try:
        if pins[str(HERE / 'dtoa.c')] != SHA:
            raise ValueError('The Netlib source must match the reviewed SHA256.')
        command, compiler, binary = compiler_command(output, environment)
        report['compiler'] = dict(path=str(compiler), sha256=digest(compiler))
        for name, argv in [('build', command), ('mode2-mode6', [str(binary), str(controls.with_suffix('.tsv')), str(output / 'tuples.tsv')]),
                           ('decimal-reference', [sys.executable, '-B', str(HERE.parent / 'finite_controls.py'),
                                                  '--check-reference', str(controls), str(output / 'tuples.tsv')])]:
            started = time.monotonic()
            code, timed_out = execute(argv, ROOT, environment, output / (name + '.log'), 120)
            report['steps'].append(dict(name=name, exitCode=code, timedOut=timed_out,
                                        seconds=time.monotonic() - started))
            if code or timed_out:
                raise RuntimeError(f'Independent reference {name} failed; see {output / (name + ".log")}')
        if any(digest(Path(path)) != sha for path, sha in pins.items()):
            raise RuntimeError('Reference inputs changed during verification.')
        report.update(status='passed', executableSha256=digest(binary), tuplesSha256=digest(output / 'tuples.tsv'))
        print((output / 'decimal-reference.log').read_text().strip())
    except BaseException as error:
        report.update(status='failed', error=repr(error))
        raise
    finally:
        write_json(output / 'report.json', report)


if __name__ == '__main__':
    main()
