#!/usr/bin/env python3
# SPDX-License-Identifier: GPL-3.0-or-later
"""Original portable CLI controls, identical assertions to the Windows probe."""
import subprocess
import sys

FIXTURES = (
    ('latex(-123456789)', '-123456789'),
    ('latex(-1/2)', r'-\frac{1}{2}'),
    ('latex(solve(x^2-1=0,x))', r'\left[-1,1\right]'),
    ('latex(solve(x=x,x))', r'\left[x\right]'),
    ('latex(solve([x+y=3,x-y=1],[x,y]))', r'\left[\left[2,1\right]\right]'),
    ('latex([[1,2],[3,4]]^(-1))', r'\left[\begin{array}{cc}-2&1\\\frac{3}{2}&-\frac{1}{2}\end{array}\right]'),
    ('latex([[1,2],[3,4]])', r'\left[\begin{array}{cc}1&2\\3&4\end{array}\right]'),
    ('latex(integrate(x^2,x,10,1))', '-333'),
)

def main():
    if len(sys.argv) != 2:
        raise SystemExit('Usage: test-transport.py /absolute/path/to/icas')
    for expression, expected in FIXTURES:
        result = subprocess.run([sys.argv[1], '--sage'], input=expression + '\n',
                                capture_output=True, text=True, timeout=30, check=True)
        quoted = [line for line in result.stdout.splitlines()
                  if line.startswith('"') and line.endswith('"')]
        if quoted != ['"' + expected + '"']:
            raise RuntimeError(f'Transport differs for {expression}: {result.stdout!r}')
    print(f'PASS Giac single-line transport: {len(FIXTURES)} negative, root-list, identity-list and matrix controls.')

if __name__ == '__main__':
    main()
