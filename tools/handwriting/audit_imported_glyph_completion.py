"""Pinned glyph-local default-helper ordering; static PE only, no DLL execution."""
from pathlib import Path
import hashlib,json,subprocess,sys
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
t=Binary();e=Binary('MyScriptEngine.dll');md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
pinsT={6443747663: ('mov', 'r14, r9'), 6443747763: ('call', '0x18013c840'), 6443747771: ('lea', 'r9, [rsi + 0x20]'), 6443747812: ('mov', 'r10, qword ptr [rax + 0x68]'), 6443747816: ('call', 'r10'), 6443738585: ('cmp', 'r14d, r13d'), 6443738588: ('jb', '0x18013a183'), 6443738601: ('mov', 'edx, 5'), 6443738610: ('mov', 'rax, qword ptr [rcx + 0x68]'), 6443738614: ('mov', 'rbx, qword ptr [rax + 0xe0]'), 6443738631: ('call', 'r8'), 6443738634: ('mov', 'r8, rax'), 6443738637: ('xor', 'r9d, r9d'), 6443738640: ('mov', 'rdx, rdi'), 6443738646: ('call', 'rbx'), 6443738650: ('je', '0x18013a642'), 6443738665: ('call', '0x180015d30'), 6443738684: ('mov', 'byte ptr [rdx], al')}
pinsE={6442656976: ('mov', 'qword ptr [rcx + 0x68], rax'), 6442657246: ('mov', 'qword ptr [rcx + 0x70], rax'), 6442750401: ('cmp', 'dword ptr [rdx + 8], 0'), 6442750530: ('call', '0x180048190'), 6442750550: ('call', '0x18004aa10'), 6442754802: ('test', 'r15b, r15b'), 6442760948: ('cmp', 'edx, 0xd'), 6442760963: ('mov', 'rax, qword ptr [rcx + rax*8]')}
regions=[]
for b,lo,hi,pins in [(t,0x18013c920,0x18013ca33,pinsT),(t,0x18013a0b0,0x18013a661,pinsT),(e,0x180031910,0x180033110,pinsE),(e,0x1800491a0,0x18004a480,pinsE),(e,0x18004baf0,0x18004bb0b,pinsE)]:
 data=b.data[b.off(lo):b.off(hi)];ins={i.address:(i.mnemonic,i.op_str) for i in md.disasm(data,lo)}
 for a,v in pins.items():
  if lo<=a<hi:assert ins[a]==v,(hex(a),ins.get(a),v)
 regions.append(dict(binary=b.name,start=hex(lo),end=hex(hi),sha256=hashlib.sha256(data).hexdigest()))
assert e.qwords(0x180252188,1)[0]==0x1801dd740 and e.qwords(0x1801dd740+32,1)[0]==0x1801dd7a0 and e.qwords(0x1801dd7a0+0xe0,1)[0]==0x1800491a0
assert e.qwords(0x180252190,1)[0]==0x1801dd990 and e.qwords(0x1801dd990+32,1)[0]==0x1801dd968 and e.qwords(0x1801dd968,1)[0]==0x18004baf0
assert e.qwords(0x1801dd900+5*8,1)[0]==0x18020fe10 and e.qwords(0x18020fe10,1)[0]==5 and e.qwords(0x18020fe10+16,1)[0]==0x180094500
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
sub=Path(str(p)+'.circled.json');subprocess.run([sys.executable,str(root/'tools/handwriting/audit_legacy_circled_writeback.py'),'--output',str(sub)],check=True,stdout=subprocess.DEVNULL)
report=dict(noVendorExecution=True,textPins=[dict(address=hex(a),instruction=' '.join(v)) for a,v in pinsT.items()],enginePins=[dict(address=hex(a),instruction=' '.join(v)) for a,v in pinsE.items()],bindings=9,regions=regions,circledDependency=json.loads(sub.read_bytes()),scope='ONEglyph all tokens->rawbytes->ONE0491A0 helper->storageNUL;WORDconcatenationafterperglyphcompletion')
p.write_text(json.dumps(report,indent=2)+'\n');print('PASS26exactpins/9bindings/5regions/defaultcircleddependency; glyph-local completion')
