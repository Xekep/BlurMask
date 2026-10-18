"""Read-only next global217670/liveV50 contract; no vendor execution."""
import base64,hashlib,json,runpy,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)not in(2,3):raise SystemExit('report.json [actual-postV68-diagnostic.json]')
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
target=Path(sys.argv[1]);target.parent.mkdir(parents=True,exist_ok=True);saved=sys.argv;components=[]
for name in ('audit_main_ordinary_glyph_refresh.py','audit_main_glyph_refresh_scoring.py','audit_main_glyph_utf8_emission.py','audit_glyph_alternative_selection.py','audit_legacy_text_glyph_caller.py'):
 out=target.with_name(target.stem+'.'+name[6:-3]+'.json');path=Path(__file__).with_name(name);sys.argv=[str(path)]+(['--output',str(out)]if name=='audit_legacy_text_glyph_caller.py'else[str(out)]);runpy.run_path(str(path),run_name='__main__');components.append(dict(audit=name,report=str(out),sha256=hashlib.sha256(out.read_bytes()).hexdigest()))
sys.argv=saved
regions=[(0x18013c6e0,0x18013c840,'6c2600caf2d7b6b388aedfe41f0be739896e5adbb72a5413464c07f006f2b176'),(0x180217670,0x180217db0,'8321a0ed9770f4e6f4ff036643ff8d1490ad6989dd616084b739dd5a2c0b176b')]
ins={};md=Cs(CS_ARCH_X86,CS_MODE_64)
for a,z,digest in regions:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==digest;ins.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)})
pins={0x18013c74d:('mov','rdx, qword ptr [rbp + 0x48]'),0x18013c760:('call','r9'),0x18013c773:('test','word ptr [rax + 0x18], r14w'),0x18013c77a:('mov','rdx, qword ptr [rbp + 0x10]'),0x18013c786:('mov','r10, qword ptr [rax + 0x90]'),0x18013c79b:('call','r10'),0x18013c7a3:('or','word ptr [rbx + 0x18], r14w'),0x18013c7bb:('test','cl, cl'),0x18013c7d6:('mov','r10, qword ptr [rax + 0x60]'),0x18013c7e4:('call','r10'),0x18013c80d:('mov','dword ptr [rdx], eax'),0x180217960:('mov','rcx, qword ptr [rbx + 0x2460]'),0x180217973:('movzx','edx, byte ptr [rcx + r14*8 + 0x20]'),0x180217979:('test','dl, 0x20'),0x18021798a:('cmp','qword ptr [rcx + r14*8 + 8], rax'),0x180217995:('test','dl, 7'),0x1802179b1:('sub','r9d, r8d')}
for a,value in pins.items():assert ins[a]==value,(hex(a),ins.get(a))
assert b.qwords(0x1802dc740+0x40,1)[0]==0x18013c6e0
assert b.qwords(0x1802dc680+0x60,1)[0]==0x18013a830
observation=None
if len(sys.argv)==3:
 path=Path(sys.argv[2]);d=json.loads(path.read_bytes());assert len(d)==3;owners=matches=0
 for source in d:
  assert source['negative']==source['categoryFailures']==0 and source['nnBefore']==source['nnAfter']
  assert source['references']==source['positive']==len(source['glyphs'])
  for glyph in source['glyphs']:
   owners+=1;initial=base64.b64decode(glyph['selected40']);descriptor=struct.unpack_from('<i',initial)[0]
   assert glyph['arc']>=0 and glyph['baseKey']==glyph['key'] and len(base64.b64decode(glyph['source26']))%26==0
   for row in glyph['matchingCompact']:
    compact=base64.b64decode(row['compact40']);request=base64.b64decode(row['request16']);symbol=struct.unpack_from('<H',compact,28)[0]
    assert len(compact)==40 and len(request)==16 and compact[32]&32 and compact[32]&7==0
    assert struct.unpack_from('<Q',compact,8)[0]==glyph['baseKey']
    assert struct.unpack_from('<H',request,6)[0]==65535 and struct.unpack_from('<H',request,8)[0]==symbol and symbol<192
    assert row['ordinal']!=descriptor,'The selected-only pool shortcut would accidentally satisfy this fixture'
    matches+=1
 assert(owners,matches)==(5,10)
 observation=dict(path=str(path),sha256=hashlib.sha256(path.read_bytes()).hexdigest(),actualOwners=owners,matchingCompactRows=matches,selectedDescriptorIsNotMatchingCompactDescriptor=True,extraNN=0,scope='E-only Managed read-only post-V68 observation against frozen global12 DLL. No native refresh pool mutation or emitted result was claimed; no expected text.')
report=dict(vendorExecution=False,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins={hex(a):' '.join(v)for a,v in pins.items()},bindings=2,components=components,observation=observation,contract='same live global postV68 scope/session/pool/compact2460-count2470-starts2478 +Associations.Metadata/SourceOverlay, originalLast selects acoustic group; primary live acoustic AL remains separate from empty TOP18 control2A and TOP20/30 alternate vectors',failure='13C6E0 sets owner4000 after callback including false; failed first call skips selection. A bounded portable owner may poison failed refresh explicitly, never declare full native failed-refresh replay.',next='NEW typed global ordinary refresh +liveV50 emitter, shared internal compact mutation kernel after exact extraction, then same preNN3E glyph alternative caller. No old result/AL coercion, imported2A, source-ready CHAR/WORD/TEXT or acceptance.')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print('PASS2 complete regions/17 pins/2 bindings +5 refresh/scoring/emission/selection/caller compositions; bounded global next contract only')
