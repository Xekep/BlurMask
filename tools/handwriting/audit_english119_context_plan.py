"""Selected249 A0/A2/A4 ownership and context reuse proof. PE static only."""
from pathlib import Path
import ast,hashlib,json,runpy,struct,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);saved=sys.argv;root=Path(__file__).resolve().parents[2]
sys.argv=[str(Path(__file__).with_name('audit_english119_primary_plan.py')),str(target.with_suffix('.primary.json'))]
g=runpy.run_path(sys.argv[0]);sys.argv=saved;b=g['b'];f=g['f'];md=Cs(CS_ARCH_X86,CS_MODE_64)
regions=[
 (0x180155d30,0x180155d96,'231ba5ca71c4ce58e812a6f0553509bbe9ffe16387ceff7feea906989475ed1f'),
 (0x180155ef0,0x180156080,'9a7c6d40a6bd0cfb16f91acad3413ca1ac47cd5c2eb572219aadb67f36711d2f'),
 (0x180158de0,0x180158dfe,'756b08b9b02a5ede6f6434ce59e985c42eb087bd8622e695a8cdc8bb43307ff4'),
 (0x180151269,0x180151345,'5fed1a3c573c491b82dbd2e8d634c302c1cb65f88d3a742d882810b8298d83d6'),
 (0x180156630,0x180156950,'b0de9f7ab32a212f4b5529cfc40966c48891fab98192051a8aa18c03bf236325'),
 (0x1801ef240,0x1801ef33c,'7d2ba66c4720b4488ee6553e2ab54fca2557e7b68bfb7352baa45b19a9cbe243')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins={
 0x180155d43:('mov','qword ptr [rdx + 0x1d8], rax'),
 0x180155f35:('mov','qword ptr [rdx + 0x1d8], rax'),
 0x180155ea7:('movups','xmmword ptr [rbx + 0x1d0], xmm0'),
 0x180158de0:('mov','rax, qword ptr [rdx + 0x1e0]'),
 0x180158df0:('movups','xmm0, xmmword ptr [r8]'),
 0x180158df6:('movups','xmmword ptr [rdx + 0x1d0], xmm0'),
 0x180151277:('mov','edx, 0x249'),
 0x180151287:('mov','r8, qword ptr [rax + 0x30]'),
 0x180151295:('mov','r8, qword ptr [rax]'),
 0x18015129b:('mov','rbx, rax'),
 0x18015129e:('test','rax, rax'),
 0x1801512a1:('je','0x18015119d'),
 0x1801512bf:('mov','r8, qword ptr [rcx + 0x18]'),
 0x1801512c9:('mov','qword ptr [rbp - 0x11], rax'),
 0x180151308:('mov','qword ptr [rbp - 0x51], rbx'),
 0x18015130f:('mov','r8, qword ptr [rax + 0x20]'),
 0x180151334:('lea','r8, [rbp - 0x59]'),
 0x180151338:('mov','rdx, rbx'),
 0x18015133e:('mov','r9, qword ptr [rax + 8]'),
 0x180151342:('call','r9'),
 0x180156650:('mov','r15, qword ptr [rdx + 0x1d8]'),
 0x1801568f2:('mov','edx, 1'),
 0x1801568f7:('mov','ecx, 0x14'),
 0x1801568fc:('test','r15, r15'),
 0x1801568ff:('cmovne','cx, dx'),
 0x180156903:('mov','byte ptr [rax + 0x11], 0'),
 0x18015690a:('mov','word ptr [rax + 0x18], dx'),
 0x180156911:('mov','word ptr [rax + 0x16], dx'),
 0x180156918:('mov','word ptr [rax + 0x14], cx'),
 0x1801ef2bc:('cmovne','di, word ptr [rcx + 0x14]'),
 0x1801ef2c5:('cmovne','si, word ptr [rcx + 0x16]'),
 0x1801ef2ce:('cmovne','r14w, word ptr [rcx + 0x18]'),
 0x1801ef312:('mov','word ptr [rbx + 0xa0], di'),
 0x1801ef31d:('mov','word ptr [rbx + 0xa4], r14w'),
 0x1801ef32c:('mov','word ptr [rbx + 0xa2], si'),
 0x1801ef337:('mov','byte ptr [rbx + 0x88], cl')}
for a,p in pins.items():
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==p,(hex(a),i.mnemonic,i.op_str)
assert b.qwords(0x1802dee00+0x20,1)[0]==0x1802deee0
assert b.qwords(0x1802deee0,2)==(0x180158de0,0x180158df0)
outer=f['180150d00'];start=outer.index('local_a8 = *(undefined8 *)(param_2 + 0x58)');scope=outer[start:outer.index('LAB_1801511a1:',start)]
for text in ('0x249','lVar13 = (*(code *)*puVar12)','if (lVar13 != 0)','uStack_b0 = lVar13','&local_b8','(**(code **)(lVar13 + 8))'):assert text in scope
#Actual source IDs are not interchangeable: these are the original selected249
#resource objects before any clone, each wrapping its own nonempty247 profile.
wirePath=root/'tools/LegacySource247Wire.Probe/audit_output.py'
wireTree=ast.parse(wirePath.read_text(encoding='utf-8-sig'));wireNames={}
exec(compile(ast.Module(body=[n for n in wireTree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))],type_ignores=[]),str(wirePath),'exec'),wireNames)
actual=[];manifest={x['path']:x['sha256']for x in json.loads((root/'models/manifest.json').read_bytes())['resources']}
for name,offset,expectedEnd in [('ru-RU/ru_RU-ak-cur.lite.res',6104,187937),('en-US/en_US-ak-cur.lite.res',8827,182191)]:
 d=(root/'models'/name).read_bytes();assert hashlib.sha256(d).hexdigest()==manifest[name]
 magic,size,kind,flags,version=struct.unpack_from('<4sIIHH',d,offset)
 assert(magic,kind,flags,version)==(b'\x90\xfe\x01\x01',0x249,0,0x100)and offset+size+8==expectedEnd
 #The selected249 body holds its complete247 resource, followed only by
 #the249 score-exponent F32; parse that actual nested envelope independently.
 _,rawSource=wireNames['source'](d[offset+16:expectedEnd-4]);assert rawSource['SourceKind']==0x135
 actual.append(dict(path=name,selected249=offset,end=expectedEnd,original247Body=offset+16,sourceKind=0x135,sha256=hashlib.sha256(d[offset:expectedEnd]).hexdigest()))
controls=[dict(original247Present=present,container14=1 if present else 20,container16=1,container18=1)for present in(False,True)]
report=dict(staticOnly=True,vendorExecution=False,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins={hex(a):' '.join(p)for a,p in pins.items()},actual=actual,preparedBranchControls=controls,
 selected='150D00 finds selected249, obtains its NONNULL original247 through158DE0, clones processor list, puts original247 at pair+8, calls registered158DF0 on selected clone. Thus child1D8 is NONNULL before156630 for both actual resources.',
 caps='156630 container14=(1D8?1:20),16=1,18=1,alias11=0. Native1EF070 copies14/16/18 intoA0/A2/A4 and11 into88. FreshC4zero is separately composed from primary policy ownership proof.',
 boundary='Standalone constructor/resource-instantiation WITHOUT150D00 selected-source binding has1D8zero andA0=20; the typed selected frontend policy must not be described as all249 objects.',
 next='Reuse generic A8 with actual ranked119 probabilities and shared byte-identical blank-context numeric kernel. New typed EN owner must retain actual primary/source/geometry/cache. No caller supplied masks/costs/flags; no terminal273/text claim.')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(f'PASS{len(regions)} pinned ranges/{len(pins)} instructions; both actual RU/EN selected249 original247->paired1D8->A0/A2/A4=1; separate prepared unbound20 branch.')
