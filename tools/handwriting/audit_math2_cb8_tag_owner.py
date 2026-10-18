"""Static math CB8 tag/counter, resolution and stale-selection lifecycle.
Vendor PE bytes only; no runtime acceptance or vendor execution.
"""
from pathlib import Path
import hashlib,json,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll');ink=Binary('MyScriptInk.dll')
assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
assert hashlib.sha256(ink.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
regions=[(6442657360, 6442657792, '04eadc8218f4edbcf10315e469a53f5781786d8802d61ccd8bc6c25e9636979b'), (6442657792, 6442658416, 'b04d0ab1d59c0c6be350c414145aa25444d1b357ed70cc765d9914c1e2583528'), (6442658816, 6442659904, '8fd84883852e07d04a4d4288f076721c7c98e78025c5e0c02016d8d71fc9f32e'), (6442659904, 6442660112, '608d044b6d18c6be9a3524b070593ec1eb66ba7b31fb30c101306e723beba1e1'), (6442661216, 6442661520, '7baac6a3f7811cc82933d1a3ce5f314b6c4f299956ee7ab1cc01d077b0b25929'), (6442661568, 6442661856, '583a02a1f7995dad7f1142365cb989e42a9b97f8bf9b6135d0ee848ee484f92f'), (6442661856, 6442662000, '17a0a3c0ca3052d4c01f1834b92a4e66399ca9825738d21a9b52fbad29aa37db'), (6442662000, 6442662176, '4b509e61e31593961be43ddf693dc312542d4b2198b6fd62526c85d3be7892f8'), (6442662176, 6442662352, '5260e43254949a0ed242c37bd7a1837e475afdbbbbeb7c7f48da44070dae50ee'), (6442662352, 6442662512, 'b02591eaa8ba2f75c40a9f8e2361e88b9c4e4af343e592a627fc7d6fdf086e04')]
pins={6442658839: ('cmp', 'r9, 0x28'), 6442658945: ('mov', 'dword ptr [rdx + 0x30], eax'), 6442659214: ('mov', 'qword ptr [r14 + 0x28], rax'), 6442659304: ('mov', 'qword ptr [r14 + 0x48], rax'), 6442659384: ('mov', 'qword ptr [rcx + 0x38], rbp'), 6442659451: ('mov', 'qword ptr [rax + 0x28], rbp'), 6442659455: ('mov', 'qword ptr [rax + 0x30], rbp'), 6442661262: ('call', '0x180032650'), 6442661286: ('mov', 'qword ptr [rax], rdx'), 6442661293: ('inc', 'qword ptr [rcx]'), 6442661300: ('mov', 'qword ptr [rax + 8], 0xffffffffffffffff'), 6442661308: ('mov', 'qword ptr [rax + 0x10], rbx'), 6442661324: ('mov', 'edx, 0xc22'), 6442661351: ('mov', 'byte ptr [rsp + 0x34], 0'), 6442661393: ('mov', 'r9d, 0x18'), 6442661412: ('mov', 'qword ptr [rsi + 0x18], rax'), 6442661432: ('mov', 'qword ptr [rcx + 8], rsi'), 6442661436: ('test', 'rax, rax'), 6442661441: ('mov', 'rax, 0xffffffffffffffff'), 6442661503: ('mov', 'rax, qword ptr [rsi]'), 6442661621: ('call', '0x180031e20'), 6442661634: ('jns', '0x1800337d7'), 6442661653: ('neg', 'rax'), 6442661656: ('cmp', 'qword ptr [rbx], rax'), 6442661712: ('test', 'rax, rax'), 6442661717: ('mov', 'rdx, qword ptr [rdi + 0x10]'), 6442661724: ('mov', 'r9, qword ptr [rbx + 0x18]'), 6442661728: ('mov', 'r8, qword ptr [rbx + 0x10]'), 6442661735: ('mov', 'r10, qword ptr [rax + 0xe0]'), 6442661742: ('mov', 'eax, dword ptr [rdi + 0x30]'), 6442661780: ('call', 'r10'), 6442661786: ('mov', 'qword ptr [rbx + 8], rax'), 6442661821: ('mov', 'byte ptr [rsp + 0x20], 4'), 6442661829: ('mov', 'r10, qword ptr [rax + 0x100]'), 6442661836: ('call', 'r10'), 6442662387: ('mov', 'esi, dword ptr [rax + 0x30]'), 6442662398: ('mov', 'rax, qword ptr [r10 + 0x28]'), 6442662402: ('mov', 'edx, dword ptr [r10 + 0x40]'), 6442662406: ('mov', 'r8, qword ptr [rax + rbx*8]'), 6442662420: ('cmp', 'r8, qword ptr [r10 + rax*8]'), 6442662437: ('mov', 'rdx, qword ptr [rdi + 0x10]'), 6442662447: ('mov', 'r9, qword ptr [rax + 0xe8]'), 6442662454: ('call', 'r9')}
md=Cs(CS_ARCH_X86,CS_MODE_64);decoded={}
for a,z,h in regions:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 decoded.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)})
for a,want in pins.items():assert decoded[a]==want,(hex(a),decoded.get(a),want)
bindings=[]
for binary,table,slot,target in [(b,0x1801ed890,0x18,0x1801ed8d8),(b,0x1801ed8d8,8,0x1801ed830),(b,0x1801ed8d8,0x10,0x1801ed910),(b,0x1801ed910,0x10,0x180032c00),(b,0x1801ed830,0x10,0x180033040),(b,0x1801ed830,0x28,0x180033560),(b,0x1801ed830,0x40,0x1800337e0),(b,0x1801ed830,0x48,0x180033920),(b,0x1801ed830,0x50,0x1800339d0),(ink,0x1800e5050,0xe0,0x18006be30),(ink,0x1800e5050,0xe8,0x180069630),(ink,0x1800e5050,0x128,0x18006c880),(ink,0x1800e5050,0x130,0x18006c8f0)]:
 assert binary.qwords(table+slot,1)==(target,);bindings.append(dict(dll=binary.name,table=hex(table),slot=hex(slot),target=hex(target)))
# Literal initialization, not inferred from the first actual allocated tag.
i=list(md.disasm(b.data[b.off(0x180032dec):b.off(0x180032dff)],0x180032dec))
assert any(x.mnemonic=='mov'and x.op_str=='qword ptr [rax], 2'for x in i)
report=dict(staticOnly=True,vendorExecution=False,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins={hex(a):' '.join(w)for a,w in pins.items()},bindings=bindings,
 contract='CB8 local temporary IDs start2; consumed before range allocation, linked failure prefixes retained. Negative DOM placeholders resolve once to actual selected Ink IDs; descriptor category102 is not tag/raw-object ID. Ordered old/new vectors retain duplicates; delete only old values absent from new.',
 boundary='Actual math C23/selected field/temporary DOM owners and source-rich registration remain required. No caller-created name/range booleans or text-kind4 target substitute is authorized.')
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(regions)} CB8 regions/{len(pins)} pins/{len(bindings)} bindings/start2; static only, no accepted formula')
