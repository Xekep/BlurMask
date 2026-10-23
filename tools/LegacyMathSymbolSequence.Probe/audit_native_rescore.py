"""Static original bytes only: fixed resource and Math00FD40/00F3C0 rescore pins."""
from pathlib import Path
import hashlib,json,struct,sys
def sha(b):return hashlib.sha256(b).hexdigest()
def main():
    if len(sys.argv)!=3:raise SystemExit('NATIVE_REPO REPORT')
    r=Path(sys.argv[1]);b=(r/'research/myscript/sdk/build/x64/MyScriptMath.dll').read_bytes()
    assert sha(b)=='d81b9cb071efac9d1300da1c6e8be24db2fbb12793ed1adefba38ffd0f80a8fb'
    sections=next(x for x in json.loads((r/'research/myscript/output/binary-inventory.json').read_text()) if x['file'].endswith('MyScriptMath.dll'))['sections']
    def off(va):
        v=va-0x180000000
        return next(s['raw_offset']+v-s['rva'] for s in sections if s['rva']<=v<s['rva']+s['raw_size'])
    pins=json.loads(Path(__file__).with_name('native-region-pins.json').read_text())
    for p in pins:
        x=b[off(int(p['start'],16)):off(int(p['end'],16))]
        assert len(x)==p['bytes'] and sha(x)==p['sha256']
    for va,raw in [(0x18009d350,'00008000'),(0x18009d450,'50acaec2'),(0x18009d3ac,'0000803f')]:
        assert b[off(va):off(va)+4]==bytes.fromhex(raw)
    # Stored integer ID meanings and actual F32 operand chronology.
    instructions={0x18000f43d:'8d48ff',0x18000f440:'83c0fe',0x18000f606:'f30f100490',
      0x18000f610:'f3410f59442e14',0x18000f617:'440f2fc0',0x18000f61b:'7605',
      0x18000f622:'e8d9db0700',0x18000f62b:'f30f5cf0',0x18000f675:'f30f5ef0',
      0x180010058:'f30f104714',0x18001005d:'f30f58c6',0x180010061:'f30f114714',
      0x180010073:'f30f5e4714'}
    for va,hexbytes in instructions.items():
        expected=bytes.fromhex(hexbytes);assert b[off(va):off(va)+len(expected)]==expected
    raw=(r/'models/math/math-ak.res').read_bytes()
    assert sha(raw)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
    assert struct.unpack_from('<IffII',raw,180058)==(0,struct.unpack('<f',struct.pack('<f',.14))[0],1.5,2,71)
    for at in [592004,1004804]:
        m=struct.unpack_from('<290i',raw,at);assert min(m)==0 and max(m)==239 and m.count(239)==51 and 240 not in m
    report=dict(moduleSha256=sha(b),completeCallerRegions=len(pins),instructionSpans=len(instructions),constantSpans=3,
      boundaryId=240,ignoreId=239,ignoreMappedOrdinalCount=51,recordCount=2,
      arithmetic='F32 ignore multiply, log/floor, ordered subtract across records, final divide by2',
      nativeLogEmulated=False,vendorExecuted=False)
    Path(sys.argv[2]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print('PASS2callerregions/13ASM/3constants/2maps; no vendor execution')
if __name__=='__main__':main()
