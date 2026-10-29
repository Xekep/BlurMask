"""Restore five exact admitted sources for the original bootstrap SOURCE check.

This does not establish runtime equivalence of the later periodic publication.
The original contract table, its old-body inverses and fixture pins stay intact.
"""
import hashlib
import json
from pathlib import Path

TABLE_SHA = 'a9b4c9ac32d923bb16f6d942b69e5c7d4996a5826f0b8d69f2ad6760488a3dca'


def digest(data):
    return hashlib.sha256(data).hexdigest()


def restore(relative, data, historical):
    raw = Path(__file__).with_name('bootstrap-successor-preimages.json').read_bytes()
    if digest(raw) != TABLE_SHA:
        raise ValueError('Bootstrap successor inverse table changed')
    rows = json.loads(raw)['Sources']
    matches = [row for row in rows if row['Relative'] == relative]
    if len(matches) != 1 or matches[0]['HistoricalLF'] != historical:
        raise ValueError('Unreviewed bootstrap source or historical endpoint')
    row = matches[0]
    data = data.replace(b'\r\n', b'\n')
    if digest(data) == historical:
        return data
    if digest(data) != row['CurrentLF']:
        raise ValueError('Unreviewed bootstrap successor source')
    for start, end, old, new in reversed(row['Reverse']):
        if data[start:end] != bytes.fromhex(old):
            raise ValueError('Bootstrap inverse slice mismatch')
        data = data[:start] + bytes.fromhex(new) + data[end:]
    if digest(data) != historical:
        raise ValueError('Bootstrap historical endpoint mismatch')
    return data
