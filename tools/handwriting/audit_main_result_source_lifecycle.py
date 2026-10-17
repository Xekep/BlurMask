"""Static normal getter -> TOP.V68 -> source export/postprocessing lifecycle.

Pinned vendor images are read only. This does not execute the SDK or establish
that the portable first-candidate adapter has completed these later stages.
"""
import hashlib,importlib.util,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[
 (0x18000f460,0x18000f710,'ca5c9b83755099b0d2ef4445b8b8aba0250e58d8bc4d6b18c742ecdaec4d6554'),
 (0x18013e7f0,0x18013e8e0,'3870dd68823fc06c08805d38baa2641fd5f60a5205607ab85107cf2e8b0ed66d'),
 (0x180219580,0x180219f80,'9825287ed9aabcefcad22ff893fad2eb058e8be88a54e7614f82990a2e5a7f64'),
 (0x18013d360,0x18013e7f0,'f6218b8b0df1f308e0b434c7cb48d9aa408e2563fbd0ba1e9e6d16f6b35ccc51'),
 (0x18013d000,0x18013d360,'e0cd67a9ed8d8276cc0c5bf910356e4951557d38dca148482e40e0207c9b47a8')]
instructions={};md=Cs(CS_ARCH_X86,CS_MODE_64)
for a,z,digest in regions:
    raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==digest
    instructions.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)})
pins={
 0x18000f643:('mov','qword ptr [rsp + 0x28], r13'),
 0x18000f654:('mov','r10, qword ptr [rax + 0x80]'),0x18000f65b:('call','r10'),
 0x18000f664:('test','al, al'),0x18000f666:('je','0x18000f5e0'),
 0x18000f670:('mov','edx, 0x273'),0x18000f69d:('mov','r8, qword ptr [rax + 0x68]'),
 0x18000f6a5:('call','r8'),0x18000f6aa:('je','0x18000f5e0'),
 0x18013e84e:('test','word ptr [rdi + 0x10], r13w'),
 0x18013e853:('jne','0x18013e88b'),0x18013e85d:('mov','r10, qword ptr [rcx + 0x88]'),
 0x18013e86d:('mov','r9, rbx'),0x18013e879:('call','r10'),
 0x18013e87e:('je','0x18013e884'),0x18013e886:('or','word ptr [rdi + 0x10], r13w'),
 0x18013e88b:('mov','rbx, qword ptr [rbx + 0x20]'),
 0x18013e8be:('call','0x18013d360'),0x18013e8c5:('je','0x18013e899'),
 0x18013e8cd:('call','0x18013d000'),0x18013e8d4:('je','0x18013e899')}
for a,expected in pins.items():assert instructions[a]==expected,(hex(a),instructions.get(a))
assert any(m=='mov'and op=='r13d, 0x2000'for m,op in instructions.values())
assert b.qwords(0x1802dc740+0x68,1)==(0x18013e7f0,)
assert b.qwords(0x1802e7cb0+0x80,3)==(0x1802271f0,0x180219580,0x180217670)
spec=importlib.util.spec_from_file_location('source_al',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py');module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
ak=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
alphabet=module.read_alphabet(ak)
for address,raw in [(0x1802eb6f4,b'\xe2\x98\x90'),(0x1802eb6f8,b'\xe2\x98\x91'),(0x1802eb6fc,b'\xe2\x98\x92')]:
    assert b.data[b.off(address):b.off(address)+4]==raw+b'\0'
    assert raw.decode('utf8')not in alphabet.values()
Path(sys.argv[1]).write_text(json.dumps(dict(regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],
 pins=len(pins),bindings=4,checkboxLiteralsAbsentFromPinnedRU=True,
 scope='Normal getter invokes TOP.V68 after successful controller.V80, independently of its external argument. V68 exports each unrefreshed span then runs both postprocessors; bit2000 written even after callback failure.',
 portableV68Completed=False,vendorExecution=False),indent=2)+'\n')
print('PASS5 result/source lifecycle regions,',len(pins),'pins,4 bindings and3 absent RU checkbox literals; no SDK execution')
