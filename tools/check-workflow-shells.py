"""Parse explicitly Bash workflow blocks without executing their commands.

Requires PyYAML and Bash. GitHub expressions are placeholders at this stage;
this checks the shell syntax of the checked-in template, not expression values.
"""
import argparse
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys

import yaml


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--bash', type=Path, help='Explicit Bash executable; Windows defaults to Git Bash')
    parser.add_argument('workflows', nargs='+', type=Path)
    args = parser.parse_args()
    bash = str(args.bash) if args.bash else None
    if bash is None and os.name == 'nt':
        #System32/bash.exe may be a WSL launcher, not a usable local shell.
        git = shutil.which('git')
        if git:
            bash = next((str(parent / suffix)
                         for parent in list(Path(git).parents)[:3]
                         for suffix in ('bin/bash.exe', 'usr/bin/bash.exe')
                         if (parent / suffix).is_file()), None)
    elif bash is None:
        bash = shutil.which('bash')
    if bash is None:
        parser.error('Bash is required for the syntax check')
    count = 0
    failed = False
    for path in args.workflows:
        workflow = yaml.safe_load(path.read_text(encoding='utf-8-sig'))
        for job_id, job in workflow['jobs'].items():
            for step in job.get('steps', []):
                if step.get('shell', '').split(' ', 1)[0] != 'bash' or 'run' not in step:
                    continue
                source = re.sub(r'\$\{\{.*?\}\}', '__github_expression__', step['run'], flags=re.S)
                result = subprocess.run([bash, '-n'], input=source.encode('utf-8'), capture_output=True)
                count += 1
                if result.returncode:
                    failed = True
                    print(f'{path}: {job_id}: {step.get("name", "unnamed Bash step")}', file=sys.stderr)
                    print((result.stderr + result.stdout).decode('utf-8', errors='replace'), file=sys.stderr)
    if failed:
        return 1
    print(f'PASS syntax of {count} explicit Bash workflow blocks; no workflow commands executed')
    return 0


if __name__ == '__main__':
    sys.exit(main())
