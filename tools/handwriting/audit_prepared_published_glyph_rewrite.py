"""Prepared21F320 source/category/ordered mutation proof; static PE only."""
from pathlib import Path
import hashlib,json,subprocess,sys
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,parse_alphabet
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pins={
0x18000eb05:('movzx','eax, byte ptr [r8]'),0x18000eb09:('mov','byte ptr [rdx + 0xf4], al'),
0x18000eb12:('mov','byte ptr [rdx + 0xf4], 0'),
0x180152057:('mov','r10, qword ptr [rax + 0x68]'),0x18015206c:('mov','r10, qword ptr [r8 + 0x88]'),
0x1801a78f6:('movzx','eax, byte ptr [rax + 7]'),0x1801a7912:('movzx','eax, byte ptr [rax + 7]'),
0x180151bdc:('mov','r8, qword ptr [rax + 0x170]'),0x1801acf30:('mov','rax, qword ptr [rdx + 0x188]'),
0x18021f733:('mov','word ptr [rax + 8], r15w'),0x18021f749:('mov','r8, qword ptr [rax + 0x48]'),
0x18021f811:('mov','r8, qword ptr [rax + 0x10]'),0x18021f82a:('movsx','rdx, byte ptr [rax + rcx]'),
0x18021f833:('cmp','dword ptr [rax + rdx*4], 1'),
0x18021f8e3:('mov','word ptr [rax + 8], si'),0x18021f8fc:('xor','r13b, r13b'),
0x18021f8ff:('mov','word ptr [rax + 8], si'),0x18021fba4:('mov','word ptr [rdx + 8], cx'),
0x18021fbbd:('mov','word ptr [rdx + 8], ax'),0x18021facb:('cmp','ax, r10w'),
0x18021f2ab:('mov','dword ptr [r10], eax'),0x18021f2d9:('lea','rcx, [rsp + 0x70]'),
0x180196607:('mov','eax, 0x106'),0x180196610:('cmp','r15w, ax'),
0x180196614:('jb','0x180196689'),0x180196901:('call','0x180010d80'),
}
for a,v in pins.items():
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==v,(hex(a),i.mnemonic,i.op_str,v)
bindings=[(0x1802de610+0xb8,0x180152040),(0x1802de610+0x48,0x180151bd0),
 (0x1802e1ff0+0x88,0x1801a78d0),(0x1802e1ff0+0x170,0x1801acf30)]
for a,v in bindings:assert b.qwords(a,1)[0]==v
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);deps=[]
for name in ['audit_contextual_quote_rules.py','audit_main_result_glyph_normalization.py','audit_word_quote_normalization.py']:
 out=Path(str(p)+'.'+name+'.json');subprocess.run([sys.executable,str(root/'tools/handwriting'/name),str(out)],check=True,stdout=subprocess.DEVNULL);deps.append(json.loads(out.read_bytes()))
resources=[]
samplers={'en-US/en_US-ak-cur.lite.res':182412,'en-US/en_US-ak-cur.res':374182,'en-US/en_US-ak-superimposed.lite.res':207622,'en-US/en_US-ak-superimposed.res':207622,'ru-RU/ru_RU-ak-cur.lite.res':188157}
manifest={r['path']:r['sha256'] for r in json.loads((root/'models/manifest.json').read_bytes())['resources']}
for name,sampler in samplers.items():
 data=(root/'models'/name).read_bytes();sha=hashlib.sha256(data).hexdigest();assert sha==manifest[name]
 alphabet=parse_alphabet(data,99);inverse={text:label for label,text in alphabet['mapping'].items()}
 assert data[sampler:sampler+4]==bytes.fromhex('90fe0101') and int.from_bytes(data[sampler+8:sampler+12],'little')==0x297
 assert data[sampler+64]==1 and data[sampler+65:sampler+69]==(b'\x01\x02\xd9\0' if name.startswith('en-US') else b'\x01\x02\xc0\0')
 resources.append(dict(path=name,sha256=sha,alphabetOffset=99,count=alphabet['label_count'],sampler=sampler,profile=sampler+65,labels={f'U+{ord(x):04X}':inverse.get(x) for x in ["'",'"','\u201c','\u201d','\u2018','\u2019','\u201e']}))
report=dict(noVendorExecution=True,pins=[dict(address=hex(a),instruction=' '.join(v)) for a,v in pins.items()],bindings=[[hex(a),hex(v)] for a,v in bindings],dependencies=deps,resources=resources,
 scope='INTERNAL PREPARED WORD driver only; actual AL/category slices, explicit source-kind/currentTOP2A; F4 enabled configured owner unported. Kind3 no-write rejected BEFORE scalar scratch use; no version rewrite. Field24BE carry controls are separate prepared supplied-controller-field cases, never actual AL lookup.')
p.write_text(json.dumps(report,indent=2)+'\n');print(f'PASS {len(pins)} exactpins/4bindings/5actual resource slices/3 static dependencies; prepared driver only')
