"""Opt-in pinned PE/resource facts. No vendor DLL execution or resource activation."""
import argparse
import hashlib
import json
import struct
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary, parse_alphabet

parser = argparse.ArgumentParser()
parser.add_argument('--output', required=True)
args = parser.parse_args()
text = Binary('MyScriptText.dll')
engine = Binary('MyScriptEngine.dll')
assert hashlib.sha256(text.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(engine.data).hexdigest() == 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'

def read(binary, address, count):
    offset = binary.off(address)
    return binary.data[offset:offset + count]

# Header2E8B28 points to runtime table and separate initializer table.
assert text.qwords(0x1802e8b28 + 8, 2) == (0x1802e8bb0, 0x1802e8c40)
assert text.qwords(0x1802e8c40, 1)[0] == 0x180235f30
constructor = bytes.fromhex('488d4210c64208004885c0740e0f57c00f11000f1140100f11402033c0c742180000803f48894240488942484889425048894258894260b001c3')
assert read(text, 0x180235f30, len(constructor)) == constructor
assert text.qwords(0x1802e8bb0 + 0x20, 2) == (0x180236820, 0x180236830)
assert read(text, 0x180236820, 7) == bytes.fromhex('8b4258ffc0c3cc')
assert read(text, 0x180236830, 31) == bytes.fromhex('4c8bca4585c07505488d4210c3418d40ff488d1440498b4150488b44d010c3')

data = (ROOT / 'models/ru-RU/ru_RU-lk-text.lite.res').read_bytes()
assert hashlib.sha256(data).hexdigest() == 'd26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
alphabet = parse_alphabet(data, 607074)
assert alphabet['label_count'] == 139 and len(alphabet['triplets']) == 0
assert struct.unpack_from('<IIf', data, 606964 + 92) == (0, 1, 1.0)
name = bytes(c ^ (0x14, 0x12, 0x19, 0x77)[i % 4] for i, c in enumerate(data[606964 + 24:606964 + 80])).split(b'\0')[0]
assert name == b'ru_RU-lk-text.lite.lex'
report = dict(staticOnly=True, implicitSource='235F30 header0/weight24=0/weight28=1;236820 count+1/236830 index0',
              lexicalCount=139, relatedTriplets=0, sourceFlags=1, runtimeScale=1.0, identifier=name.decode(),
              scope='Prepared dictionary/header facts only. Complete240/290 factory, refresh and recognition accuracy excluded.')
Path(args.output).write_text(json.dumps(report, indent=2), encoding='utf8')
print('PASS pinned implicit-source constructor/getters and actual Russian lexical prefix; no vendor execution')
