"""Full cold EE4 parser/formatter state provenance. Vendor PE reads only.

Complete helpers, call handoff and semantic initialization; no runtime port claim.
"""
import hashlib
import json
from pathlib import Path
import runpy
import struct
import sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

if len(sys.argv)!=2:
    raise SystemExit('report.json')
root=Path(__file__).resolve().parents[2]
out=Path(sys.argv[1]).resolve();out.parent.mkdir(parents=True,exist_ok=True)
old=sys.argv[:]
try:
    sys.argv=[str(root/'tools/handwriting/audit_math2_output_route.py'),str(out.with_suffix('.route.json'))]
    runpy.run_path(sys.argv[0],run_name='__main__')
finally:
    sys.argv=old
route=json.loads(out.with_suffix('.route.json').read_bytes())
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary('MyScriptDocument.dll')
regions=[{'start': '0x1800234f0',
  'end': '0x1800236b0',
  'sha256': 'b9b5f04ab8b834ef3aedf30f3b4f46bb51c90c70f4ecb83712416fbb262e1e56',
  'calls': [{'address': '0x180023549', 'target': '0x180023330'},
            {'address': '0x1800235c5', 'target': '0x180023460'},
            {'address': '0x1800235f9', 'target': '0x180023090'},
            {'address': '0x180023623', 'target': '0x180023330'},
            {'address': '0x180023633', 'target': '0x180023460'},
            {'address': '0x18002364b', 'target': '0x180022f30'}]},
 {'start': '0x180023d60',
  'end': '0x180023e40',
  'sha256': 'cd14e9e8e3d1d80fb328fc5a9872c5d4da30021f3254fd5f5e4971bc963ca214',
  'calls': [{'address': '0x180023dc5', 'target': 'r8'},
            {'address': '0x180023df4', 'target': 'r8'},
            {'address': '0x180023e1d', 'target': '0x1801ac820'}]},
 {'start': '0x180023e40',
  'end': '0x180023fa0',
  'sha256': '1533c0dc5b8848761a3781ea45587578051a6d8601cf78e75da44c54d6814c36',
  'calls': []},
 {'start': '0x180023fa0',
  'end': '0x1800241b0',
  'sha256': '9eb5105fcb770e8544300d9aae97f63fb3609b011f068f33a591f7975a3851f5',
  'calls': [{'address': '0x180023fca', 'target': '0x1800234f0'},
            {'address': '0x180024003', 'target': '0x180023e40'},
            {'address': '0x180024034', 'target': 'r10'},
            {'address': '0x1800240c3', 'target': 'r10'},
            {'address': '0x180024137', 'target': '0x1801ac460'}]},
 {'start': '0x1800241b0',
  'end': '0x180024310',
  'sha256': 'd06fc5cfc892b1205755aa12eb9c89f32172dee2af6816478348401d882c7bd1',
  'calls': []},
 {'start': '0x180024310',
  'end': '0x180024780',
  'sha256': '1e1df192dacad7e7407101801f4776634d570c6d87ec86a290dd27ec25c8c020',
  'calls': [{'address': '0x1800243e0', 'target': '0x1800241b0'},
            {'address': '0x18002443e', 'target': '0x1800241b0'},
            {'address': '0x180024525', 'target': 'r10'}]},
 {'start': '0x180024780',
  'end': '0x180024900',
  'sha256': 'e485c58d52b4990cb57d9b5f3f200bb9fe4fd3adfcfb97b87c666bf299b7dae1',
  'calls': [{'address': '0x180024821', 'target': '0x180023fa0'},
            {'address': '0x180024890', 'target': '0x1800234f0'},
            {'address': '0x1800248ac', 'target': '0x180023e40'},
            {'address': '0x1800248e4', 'target': '0x180024310'}]},
 {'start': '0x180024900',
  'end': '0x180024a20',
  'sha256': 'be9b084d441e8bf8a42f2f5186f3a0fe601df1becd34ca7511d1a91d99814573',
  'calls': [{'address': '0x180024989', 'target': '0x180023d60'}]},
 {'start': '0x180024a20',
  'end': '0x180024c60',
  'sha256': '4938a9eccc50c7ec805a439cbe61f0c559a56064c1a3f9c29af9bcbca1aef5ae',
  'calls': [{'address': '0x180024a9b', 'target': '0x180024780'},
            {'address': '0x180024ab8', 'target': '0x180024900'},
            {'address': '0x180024adb', 'target': '0x180023d60'},
            {'address': '0x180024b58', 'target': 'r8'},
            {'address': '0x180024b70', 'target': 'r10'},
            {'address': '0x180024bb4', 'target': 'r10'},
            {'address': '0x180024bcd', 'target': 'r8'},
            {'address': '0x180024bf5', 'target': 'r8'},
            {'address': '0x180024c17', 'target': 'r8'},
            {'address': '0x180024c24', 'target': '0x180022e70'}]},
 {'start': '0x180024c60',
  'end': '0x180024ec0',
  'sha256': '70a3ad83739ffab2e9e89113425ef9bf76dc0af3a951f447753c24a13b4bf9d5',
  'calls': [{'address': '0x180024da4', 'target': '0x180022df0'},
            {'address': '0x180024deb', 'target': '0x1801ac820'},
            {'address': '0x180024e82', 'target': '0x180022df0'}]},
 {'start': '0x180024ec0',
  'end': '0x1800251e0',
  'sha256': '35b88f501ff932f0ec7dfc9ab7f42dbc4cb95f2d721136557ddef38620b6de28',
  'calls': [{'address': '0x1800250ed', 'target': '0x180024310'},
            {'address': '0x180025133', 'target': '0x180024c60'},
            {'address': '0x18002515a', 'target': '0x180022df0'},
            {'address': '0x180025193', 'target': '0x180024900'}]},
 {'start': '0x1800251e0',
  'end': '0x180025980',
  'sha256': '438532fa5596a3d1295e55e80261ec8f5945d4ef8bf0496419eb306a6177d52a',
  'calls': [{'address': '0x18002530b', 'target': '0x180022df0'},
            {'address': '0x180025380', 'target': '0x180022df0'},
            {'address': '0x180025431', 'target': 'r10'},
            {'address': '0x180025479', 'target': '0x180022df0'},
            {'address': '0x18002559f', 'target': 'r10'},
            {'address': '0x1800255fc', 'target': '0x180022df0'},
            {'address': '0x1800256c7', 'target': 'r10'},
            {'address': '0x180025720', 'target': '0x180022df0'},
            {'address': '0x1800257cb', 'target': 'r10'},
            {'address': '0x180025894', 'target': 'r10'},
            {'address': '0x1800258fa', 'target': '0x180022df0'},
            {'address': '0x18002595a', 'target': 'r8'}]},
 {'start': '0x180025980',
  'end': '0x180026010',
  'sha256': '779faa001fcd60bd611a83e94aa4bf914c0c9a7058c3e23e0fde644ee9f6ecfc',
  'calls': [{'address': '0x180025a79', 'target': 'r10'},
            {'address': '0x180025ae7', 'target': '0x180024780'},
            {'address': '0x180025b53', 'target': '0x180024ec0'},
            {'address': '0x180025bc4', 'target': '0x180023d60'},
            {'address': '0x180025c67', 'target': '0x180022df0'},
            {'address': '0x180025d5e', 'target': '0x180023d60'},
            {'address': '0x180025da4', 'target': '0x180024c60'},
            {'address': '0x180025db9', 'target': '0x180024900'},
            {'address': '0x180025de9', 'target': '0x180024900'},
            {'address': '0x180025e50', 'target': '0x180024ec0'},
            {'address': '0x180025eae', 'target': '0x180023d60'},
            {'address': '0x180025ef6', 'target': 'r8'},
            {'address': '0x180025f1a', 'target': 'r8'},
            {'address': '0x180025f33', 'target': 'r8'},
            {'address': '0x180025f69', 'target': '0x1800251e0'},
            {'address': '0x180025f88', 'target': '0x180022e70'},
            {'address': '0x180025fb9', 'target': 'r9'},
            {'address': '0x180025fd2', 'target': 'r9'},
            {'address': '0x180025fe3', 'target': '0x180024a20'}]}]
pins=[(6442595975, 'mov', 'qword ptr [rdi], rax'),
 (6442595978, 'mov', 'dword ptr [rdi], 0'),
 (6442595984, 'mov', 'word ptr [rdi + 4], 0xffff'),
 (6442598492, 'xorps', 'xmm0, xmm0'),
 (6442598495, 'movups', 'xmmword ptr [rdx], xmm0'),
 (6442598498, 'movups', 'xmmword ptr [rdx + 0x10], xmm0'),
 (6442598502, 'movups', 'xmmword ptr [rdx + 0x20], xmm0'),
 (6442598506, 'movups', 'xmmword ptr [rdx + 0x30], xmm0'),
 (6442598510, 'inc', 'dword ptr [r13 + 0x10]'),
 (6442598552, 'mov', 'byte ptr [rbx + rdi], al'),
 (6442598555, 'mov', 'byte ptr [rbx + rdi + 1], r14b'),
 (6442598560, 'mov', 'qword ptr [rbx + rdi + 0x18], r12'),
 (6442598636, 'mov', 'word ptr [rax], r13w'),
 (6442598716, 'cmp', 'esi, 7'),
 (6442598733, 'mov', 'qword ptr [rbx + rdi + 0x2c], 0xffffffffffffffff'),
 (6442598742, 'cmp', 'esi, 0xe'),
 (6442598745, 'mov', 'byte ptr [rbx + rdi + 0x23], al'),
 (6442598762, 'cmp', 'esi, 0xf'),
 (6442598765, 'mov', 'byte ptr [rbx + rdi + 0x26], al'),
 (6442598777, 'mov', 'word ptr [rbx + rdi + 0x34], ax'),
 (6442598782, 'mov', 'byte ptr [rbx + rdi + 0x38], cl'),
 (6442599098, 'movsx', 'ecx, byte ptr [r8 + 1]'),
 (6442599106, 'cmp', 'dl, 4'),
 (6442599124, 'cmp', 'word ptr [r10 + rax*2 + 0x14], r9w'),
 (6442599146, 'mov', 'byte ptr [rax], dl'),
 (6442599153, 'cmp', 'eax, ecx'),
 (6442599423, 'mov', 'dword ptr [r14 + 0x1c], 1'),
 (6442599444, 'mov', 'r8, qword ptr [r14 + 8]'),
 (6442599464, 'add', 'r8, -0x40'),
 (6442599478, 'add', 'r8, rcx'),
 (6442599491, 'dec', 'esi'),
 (6442599783, 'mov', 'word ptr [rdx], r8w'),
 (6442599787, 'inc', 'dword ptr [rbx + 0x10]'),
 (6442599804, 'cmp', 'ecx, 9'),
 (6442600260, 'mov', 'dword ptr [r14 + 0x1c], 4'),
 (6442601963, 'call', '0x1801ac820'),
 (6442605395, 'call', '0x180024ec0'),
 (6442606160, 'call', '0x180024ec0'),
 (6442606224, 'mov', 'word ptr [r15 + rax*4], 0xffff'),
 (6442606441, 'call', '0x1800251e0'),
 (6442606472, 'call', '0x180022e70'),
 (6442606481, 'mov', 'qword ptr [r14 + 0xf8], rax'),
 (6442606491, 'mov', 'qword ptr [r14 + 0x100], rax'),
 (6442606498, 'mov', 'qword ptr [r14 + 0x108], rax'),
 (6442606563, 'call', '0x180024a20')]
ins={}
for region in regions:
    start,end=int(region['start'],16),int(region['end'],16)
    raw=b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(raw).hexdigest()==region['sha256']
    decoded={i.address:(i.mnemonic,i.op_str) for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,start)}
    actual=[dict(address=hex(address),target=op) for address,(mnemonic,op) in decoded.items() if mnemonic=='call']
    assert actual==region['calls']
    ins.update(decoded)
for address,mnemonic,operands in pins:
    assert ins[address]==(mnemonic,operands),(hex(address),ins.get(address))
# Semantic frame lanes only: AST/position-vector pointers are actual owners in
# native code and are deliberately absent from these zero/sentinel facts.
frames=[]
for kind in range(3,17):
    frames.append(dict(kind=kind,partCount=1 if kind<9 else 2 if kind<14 else 3,
        step=0,presence=[0,0,0],fence=kind==7,table=kind==14,vertical=kind==15,
        seenPairs=[-1,-1,-1,-1],maximumColumns=-1,cell=[0,0],mismatch=0,lastVerticalSeparator=0))
#023E40 is literal AST-kind presence, not EF3's observable-label classifier.
presence=[]
for kind in range(2,17):
    for prior in (0,1):
        nested=[[0,1,0],[1,0,0],[0,0,prior]];steps=[0,1,2]
        top=nested[-1][steps[-1]]!=0 or kind in (2,3)
        nested[-1][steps[-1]]=int(top)
        if top:
            for frame,step in zip(nested[:-1],steps[:-1]):frame[step]=1
        assert all(nested[i][step]==int(top) for i,step in enumerate(steps))
        presence.append(dict(kind=kind,prior=prior,after=nested))
reader=runpy.run_path(str(root/'tools/export-ink-math-model.py'))
raw=(root/'models/math2/math2-unwrapped.res').read_bytes();end=reader['header'](raw,90)[3]
r=reader['Reader'](raw,106,end);r.read('H',8);n=r.read('H')
for increment in (32,64,1):
    table=r.read('H',n);n=next(v+increment for v in table[::-1] if v!=65535)
spans=r.read('H',n+1);entries=[r.read('II') for _ in range(spans[-1]+1)]
strings=r.bytes(entries[-1][0]);labels={strings[o:entries[i+1][0]].decode():v&65535 for i,(o,v) in enumerate(entries[:-1])}
delimiterAddresses=[0x18020ca70,0x18020d584,0x18020d588,0x18020d58c,0x18020caa8,0x18020ca9c,0x18020ca74,0x18020d590,0x18020d594,0x18020d598,0x18020caac,0x18020caa0,0x18020d59c,0x18020d5a0]
delimiters=[]
for address in delimiterAddresses:
    offset=b.off(address);name=b.data[offset:b.data.index(0,offset)].decode()
    delimiters.append(dict(address=hex(address),text=name,label=labels.get(name)))
assert delimiters[-2]['text']=='|' and delimiters[-2]['label'] is not None
assert [r['type'] for r in route['rules']]==[3,9,10,4,5,11,12,13,16,6,14,15,7,8]
report=dict(regions=regions,fieldPins=len(pins),callPins=sum(len(r['calls']) for r in regions),dependency=route,
 frameBytes=64,frameSemanticControls=frames,presenceControls=presence,delimiters=delimiters,
 missingPresenceExceptions=[kind for kind in range(3,17) if kind==9 or kind<=15 and (0xc408>>kind)&1],
 pipeline=['023FA0 open','023E40 ancestor presence','0241B0 match step','024310 closer/ancestor search','024780 token','024900 pop','024EC0 recovery','024C60 table-padding','0251E0 full32 compaction/U16 map','024A20 final validation'],
 scope='Same raw-only cold AL; formatter mode0 and null external eligibility map. Complete general EE4 parser native semantics inventoried, not runtime implemented.',
 noRuntimeEdits=True,vendorExecution=False)
out.write_text(json.dumps(report,indent=2,ensure_ascii=True)+'\n',encoding='utf8')
print(f"PASS{len(regions)} EE4 regions/{len(pins)} field pins/{sum(len(r['calls']) for r in regions)} call pins/14rules/14delimiter literals; static only")
