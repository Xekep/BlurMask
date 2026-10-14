"""Fetch immutable official comparison data, verifying each SHA before use."""
import argparse
import hashlib
import time
import urllib.request
from pathlib import Path

FILES = {
    'UnicodeData.txt': '68dfc414d28257b9b5d6ddbb8b466c768c00ebdf6cbf7784364a9b6cad55ee8f',
    'DerivedNormalizationProps.txt': '02d8e12cea7c61a9f3cc5fbf8eaceadf8da23fe83d60c64cf647088830b810cc',
}


def fetch(folder):
    folder.mkdir(parents=True, exist_ok=True)
    for name, expected in FILES.items():
        target = folder / name
        if target.is_file() and hashlib.sha256(target.read_bytes()).hexdigest() == expected:
            continue
        for attempt in range(3):
            try:
                with urllib.request.urlopen('https://www.unicode.org/Public/9.0.0/ucd/' + name, timeout=30) as response:
                    data = response.read(16 * 1024 * 1024 + 1)
                if len(data) > 16 * 1024 * 1024 or hashlib.sha256(data).hexdigest() != expected:
                    raise ValueError('Unexpected pinned Unicode data: ' + name)
                temporary = target.with_suffix('.download')
                temporary.write_bytes(data)
                temporary.replace(target)
                break
            except (OSError, ValueError):
                if attempt == 2:
                    raise
                time.sleep(1)
    print('PASS two official Unicode9 reference files, verified SHA256.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('output', type=Path)
    fetch(parser.parse_args().output)
