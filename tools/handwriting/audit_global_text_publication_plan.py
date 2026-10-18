"""Read-only live-global publication owner/order checkpoint. No vendor execution.
An optional E-only reachability capture records actual compiler-owned outputs;
it does not make prepared mode0 projection a completed mode2/V68 producer.
"""
from pathlib import Path
import base64,hashlib,json,runpy,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)not in(2,3):raise SystemExit('report.json [actual-reachability.json]')
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
target=Path(sys.argv[1]);target.parent.mkdir(parents=True,exist_ok=True);saved=sys.argv;components=[]
for name in('audit_main_result_source_lifecycle.py','audit_imported_word_bytes.py','audit_text_processing_context_preparation.py'):
 path=Path(__file__).with_name(name);p=target.with_name(target.stem+'.'+name.removeprefix('audit_').removesuffix('.py')+'.json');sys.argv=[str(path),str(p)];runpy.run_path(str(path));components.append(dict(path=str(p),sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
sys.argv=saved
#These regions include split hot/cold fragments. They are explicit decoded
#byte intervals, not an assumption that the first .pdata row is a whole body.
regions=[(0x1800283b0,0x180029988,'98cfecc93d4297827f4eda1d72425251f32fb26a9b165f8bc059dc54ce48bf22'),
 (0x180024f30,0x180025937,'704616d97def59b653a6d47f6d975772e84eaea6aa35877da97cf2482a45ebcd'),
 (0x1800213a0,0x180021756,'2e7688d356b39493bfee74d2ac6857a006c2c72a3f0b9fe9d33f926be0a26a40'),
 (0x180021ab0,0x180021d07,'4f9b4df67b8dc91040e7eede1b8bf772d4d512e0568a10e0fe78df963fa0b1ed'),
 (0x1801a9ae0,0x1801aa462,'da7ada9c2f364bfcdd9464d9f603b76a3298e6884f065b1ba254c5e03b6f338d')]
md=Cs(CS_ARCH_X86,CS_MODE_64);instructions={}
for a,z,h in regions:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 instructions.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)})
pins={0x1800283df:('mov','rax, qword ptr [r8 + 0x10]'),0x1800283fc:('mov','r15, qword ptr [rax + 0x30]'),
 0x180028412:('call','0x180032550'),0x180028431:('mov','rdx, qword ptr [rcx + 0x10]'),
 0x180028435:('movsx','ecx, word ptr [rdx + 0x60]'),0x1800284c7:('mov','rax, qword ptr [rcx + 0x10]'),
 0x1800284cb:('mov','rdi, qword ptr [rax + 8]'),0x1800284cf:('test','rdi, rdi'),
 0x1800284f9:('mov','r8, qword ptr [rax + 0x250]'),0x180028500:('call','r8'),
 0x18002901d:('call','0x180024f30'),0x1800292b1:('call','0x180024f30'),
 0x180029411:('call','0x180021ab0'),0x180029647:('call','0x180025e40'),
 0x180025078:('mov','rdx, qword ptr [rax + 0x10]'),0x180025084:('mov','rdx, qword ptr [rdx + 0x48]'),
 0x180025090:('call','r9'),0x180021495:('mov','rdx, qword ptr [rax + 0x48]'),
 0x1800214da:('mov','rbx, qword ptr [rax + 0x40]'),0x180021505:('mov','r8, rbx'),
 0x180021508:('mov','r10, qword ptr [rax + 0x50]'),0x180021515:('call','r10'),
 0x180021afa:('mov','r8, qword ptr [rax + 0x250]'),0x180021b13:('mov','r8d, 0x218'),
 0x180021b62:('cmp','al, 3'),0x180021bec:('add','dword ptr [r14 + 0x100], ebx'),
 0x180021c23:('mov','rax, qword ptr [r13 + 0x100]'),
 0x1801aa0f6:('cmp','dword ptr [rbp + 0x6f], 2'),0x1801aa12a:('mov','eax, dword ptr [rsi + 0x208]'),
 0x1801aa135:('je','0x1801aa249'),0x1801aa147:('cmp','word ptr [rax + 2], cx')}
for a,expected in pins.items():assert instructions[a]==expected,(hex(a),instructions.get(a),expected)
for a,expected in {0x180219604:('mov','dword ptr [rsp + 0x20], 2'),0x180219624:('imul','r8, qword ptr [rsi + 0x30], 0x1a'),0x18021966e:('mov','dword ptr [rsp + 0x20], 2')}.items():
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==expected
bindings={0x180275550+0x18:0x1800283b0,0x180275550+0x28:0x18002d8c0,0x180275550+0x30:0x180031880,
 0x1802df340+0x100:0x1801692b0,0x1802e1ff0+0x120:0x1801a9ae0,0x1802dc740+0x68:0x18013e7f0}
for a,value in bindings.items():assert b.qwords(a,1)[0]==value
source=f['1801a9ae0']
assert source.count('param_5')==5 and source.count('param_5 == 2')==1 and '*(uint *)(param_2 + 0x208)'in source
live=f['1800283b0'];assert '*(longlong **)(*(longlong *)(local_138 + 0x10) + 8)'in live and 'FUN_180021ab0'in live and 'FUN_180024f30'in live
reachability=None
if len(sys.argv)==3:
 p=Path(sys.argv[2]);d=json.loads(p.read_bytes());assert d['ExpectedRead']is False and len(d['Sources'])==9
 total=0;nn=0
 for ordinal,s in enumerate(d['Sources']):
  raw=base64.b64decode(s['Compact'],validate=True);assert s['Index']==ordinal and len(raw)==40*s['CompactCount']
  positions=[i for i in range(s['CompactCount'])if raw[i*40+32]&8];assert positions==s['Bit8']==[]
  assert s['HasWinner']and s['CallbacksCompleted']and(s['Descriptors'],s['PendingSteps'],s['ContextRecords'])==(1,0,0)
  assert s['NNBefore']==s['NNAfter']and len(s['Words'])==1
  for word in s['Words']:
   assert word['First']>=0 and word['Last']>=word['First']and len(base64.b64decode(word['Storage120']))==120
   assert word['ExistingMode0Projection']and all(r[0]==r[3]==0 and r[1]<=r[4]and r[2]>=0 and r[5]>=0 for r in word['ExistingMode0Projection'])
   assert all(((r>>24)&65535)<32768 for r in word['References'])
  total+=s['CompactCount'];nn+=s['NNBefore']
 assert(total,nn)==(22440,868)
 reachability=dict(path=str(p),sha256=hashlib.sha256(p.read_bytes()).hexdigest(),actualSources=9,compactRows=total,bit8=0,NN=nn,
  scope='E-only Managed read-only observation on exact4a533ec+frozen14 library; no targets. Existing mode0 projection/count-fill was inspected, NOT completed nativeV68 or a produced mode2 owner.')
report=dict(staticOnly=True,vendorExecution=False,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins={hex(a):' '.join(v)for a,v in pins.items()},extraCountFillPins=3,bindings={hex(a):hex(v)for a,v in bindings.items()},components=components,reachability=reachability,
 ownership='The SAME OriginalSelected binding/Document/CurrentView/C22/tag/registry ->new2DA/shared3E before NN. Live original global TOP/pool/graph/source used forV68/217670/213A0/024F30. Postimport2A copied pool and05BAC0 byte route remain distinct.',
 order='Branch from completed RAW global callbacks, before import. Capture/quote/F4 ->actualV68 source count/fill/rank/275score/checkbox ->live glyph refresh/selection ->CHAR/WORD/TEXT ->source lines/postprocessors. Do not mutate original pool after imported snapshot validation.',
 minimumNext='New typed pre-NN global publication scope plus actualglobal candidate/source prefix and bounded no-bit8 V68 owner. Use same selected binding, exact model/Document, own global graph/context120/WORD span; refuse pending/reverse/higher/unported branches explicitly. Full source-rich document glue follows that owner; no oldconfigured2 coercion.',
 confidence='Native candidate24.field0, glyphvector10/header1C, WORDcost and normalizedTOPmass remain separate source-bound channels. No calibrated acceptance probability or application threshold mapping is established.')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print(f'PASS5 live source regions/{len(pins)+3}pins/6bindings +3compositions; postimport/live distinction and bounded global publication plan; no runtime change')
