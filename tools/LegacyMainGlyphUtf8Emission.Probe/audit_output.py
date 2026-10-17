"""Independent label/run and actual empty-owner decisions; no expected OCR text."""
import argparse
import base64
import importlib.util
import json
import struct
from pathlib import Path

root = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('raw_alphabet_oracle',
    root / 'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
oracle = importlib.util.module_from_spec(spec)
spec.loader.exec_module(oracle)

p = argparse.ArgumentParser(description=__doc__)
p.add_argument('acoustic', type=Path)
p.add_argument('capture', type=Path)
a = p.parse_args()
raw_acoustic = a.acoustic.read_bytes()
alphabet = oracle.read_alphabet(raw_acoustic)
label_count = struct.unpack_from('<H', raw_acoustic, 113)[0]
synthetic = {65532: '\u00a0', 65531: ' ', 65529: '\n', 65526: '\U000f0000'}
synthetic.update({65523-i: chr(0xf0010+i) for i in range(9)})


def expected(flags, symbol):
    kind = flags & 7
    if kind == 4:
        return None  #NONNULL native empty alternate object always fails.
    count = ((symbol >> 2) & 8191) if kind in (1, 3) else 1
    if not count:
        return b''
    if kind == 3:
        if symbol & 3 == 1:
            return None  #Unwritten native token label is explicitly unsupported.
        return '\ufffd'.encode() * count  #Lookup-only empty2A givesFFFF.
    if kind == 1:
        symbol = ((symbol & 3) - 7) & 65535
    elif kind not in (0, 2):
        symbol = 65535
    if symbol < label_count and symbol not in alphabet:
        return None  #Actual primaryAL virtual30 failed; no invented replacement.
    text = alphabet.get(symbol, synthetic.get(symbol, '\ufffd'))
    #Pinned AL symbols contain no complete paired circled marker payload. The
    #native helper discards its canonical scratch absent a successful pair.
    assert not ('\U000f0000' in text and '\U000f0001' in text)
    return text.encode() * count


actual = prepared = refused = 0
for source in json.loads(a.capture.read_text())['inputs']:
    for row in source['actual']:
        raw = base64.b64decode(row['row40'])
        flags, symbol = struct.unpack_from('<H', raw, 4)[0], struct.unpack_from('<H', raw, 8)[0]
        check = expected(flags, symbol)
        assert row.get('refused', False) == (check is None)
        if check is not None:
            assert base64.b64decode(row['utf8']) == check
        actual += 1
    for row in source.get('preparedDescriptors', []):
        check = expected(row['flags'], row['symbol'])
        assert row.get('refused', False) == (check is None)
        if check is not None:
            assert base64.b64decode(row['utf8']) == check
        else:
            refused += 1
        prepared += 1
print(f'PASS {actual} genuine pool rows/{prepared} prepared descriptors/{refused} native-empty or undefined refusals: independent raw AK label/run oracle, no OCR target strings.')
