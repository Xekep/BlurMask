"""EN119 rich FIRST273/WORD actual factory/assignment/interior proof. Static only."""
from pathlib import Path
import ast,hashlib,json,runpy,struct,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);target.parent.mkdir(parents=True,exist_ok=True);saved=sys.argv;components=[]
for name,flag in [('audit_english119_terminal_plan.py',False),('audit_noinitial_first_feature_plan.py',False),('audit_main_global_result_ownership.py',False),('audit_first_interior_delimiter.py',True),('audit_first_selected_glyph_publication.py',True),('audit_first_plain_text.py',True)]:
 path=Path(__file__).with_name(name);out=target.with_name(target.stem+'.'+name.removeprefix('audit_').removesuffix('.py')+'.json');sys.argv=[str(path)]+(['--output',str(out)]if flag else[str(out)])
 g=runpy.run_path(str(path));components.append(dict(auditor=name,report=str(out),sha256=hashlib.sha256(out.read_bytes()).hexdigest()))
 if name=='audit_english119_terminal_plan.py':b=g['b'];f=g['f'];root=g['root']
sys.argv=saved;md=Cs(CS_ARCH_X86,CS_MODE_64)
regions=[(0x18016a130,0x18016a280,'2dcfc82d3b237c5afa5ab00d042be0637eabbe5533d2bbacd20de71f2e07eca7'),
 (0x18016e850,0x18016eb90,'6262b8fcda933597d1dafe89aa162c9cf1aabdf94f91faf8d57a66792b5561c4'),
 (0x1801ad9d0,0x1801adf3c,'17d6207dc2da9d75335230bdea5f8117e13e37628a0e48052f805ac5444bb31f')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins={0x18016a130:('xor','eax, eax'),0x18016a17c:('mov','qword ptr [rdx + 0x80], rax'),
 0x18016e895:('xor','r12d, r12d'),0x18016e8d0:('and','ax, 0xf'),0x18016e8d6:('cmp','ax, 6'),
 0x18016e8e0:('cmp','dword ptr [rdi + 0x10], r12d'),0x18016e8e4:('jge','0x18016eb3f'),
 0x18016e8ea:('mov','ecx, dword ptr [rbx + 0x80]'),0x18016e8f6:('inc','ecx'),
 0x18016e972:('inc','dword ptr [rbx + 0x80]'),0x18016eab9:('mov','eax, dword ptr [rbx + 0x80]'),
 0x18016eabf:('dec','eax'),0x18016eac1:('mov','dword ptr [rdi + 0x10], eax'),
 0x1801565a4:('mov','byte ptr [rax + 0x6c], 1'),
 0x1801ef1b6:('mov','r8, qword ptr [rax + 0x68]'),0x1801ef1bd:('mov','byte ptr [rbx + 0xf0], al'),
 0x180155710:('mov','rdx, qword ptr [rdx + 0x70]'),0x180155717:('mov','r9, qword ptr [rax + 0x1c8]'),
 0x18005a400:('xor','al, al'),0x18005a402:('ret',''),
 0x1801ada3f:('lea','r8, [rbx + 9]'),
 0x1801f4f26:('cmp','byte ptr [rax + 0x6c], r12b'),0x1801f4f2a:('jne','0x1801f5248')}
for a,p in pins.items():
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==p,(hex(a),i.mnemonic,i.op_str,p)
assert b.qwords(0x1802de610+0x280,1)[0]==0x180155710 and b.qwords(0x1802e1ff0+0x1c8,1)[0]==0x18005a400
assert b.qwords(0x1802e5a20+0x68,1)[0]==0x1801d62a0
assert 'param_2 + 0xb9,1'in f['1801d4950']
reader=f['1801ad9d0'];assert'param_2 + 9,1,1'in reader and'param_2 + 0x159,1,1'in reader
frame=f['18016e850'];assert frame.index('*(int *)(param_3 + 0x80) =')<frame.index('*(int *)(puVar9 + 8) =')<frame.index('FUN_18016e570')
assert 'puVar9 + 0x12'in frame and'puVar9 + 0x14'in frame
#Actual EN source-owned ordered processors/graph/flags. The runtime will use
#typed child identities/fields, never these audit-only absolute offsets.
wirePath=root/'tools/LegacySource247Wire.Probe/audit_output.py';tree=ast.parse(wirePath.read_text(encoding='utf-8-sig'));w={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))],type_ignores=[]),str(wirePath),'exec'),w)
raw=(root/'models/en-US/en_US-ak-cur.lite.res').read_bytes();r,source=w['source'](raw[8843:182187])
fields={(x['owner'],x['destination']):raw[8843+x['offset']:8843+x['offset']+x['size']*x['count']]for x in r.fields}
assert source['SourceKind']==0x135 and fields['247','B9']==b'\0'
children=[c for c in r.children if c['destination'].startswith('parametersC0.58[')];assert[c['Kind']for c in children]==[0x24a,0x24c,0x265]
assert all(fields['parametersC0',n]==b'\0'for n in('20','21','legacyThirdPresence','22'))
graph=next(c for c in r.children if c['destination']=='247.138');gb=raw[8843+graph['Offset']:8843+graph['End']]
assert graph['Version']==0x10a and len(gb)==355 and gb[17]==1 and gb[350]==0
#Decoded immutable raw resource graph offsets:8/9; twelve 6-float matrices;
#12C/12D/12E;10float fields130..154;158/159/15C. No opaque skip claims.
at=16;layout={}
for dest,size in [('08',1),('09',1)]+[(f'{0xc+i*24:X}',24)for i in range(6)]+[(f'{0x9c+i*24:X}',24)for i in range(6)]+[('12C',1),('12D',1),('12E',1)]+[(f'{v:X}',4)for v in range(0x130,0x158,4)]+[('158',1),('159',1),('15C',4)]:
 layout[dest]=dict(offset=at,bytes=gb[at:at+size].hex());at+=size
assert at==len(gb) and layout['09']==dict(offset=17,bytes='01')and layout['159']==dict(offset=350,bytes='00')
word=f['1801f42d0'];literal=word.index("*(char *)(*(longlong *)(param_2 + 0x18) + 0x6c)");replacement=word.index('uVar35 == *(ushort *)(param_2 + 0xe8)',literal)
assert literal<replacement and'goto LAB_1801f5250'in word[literal:replacement]
report=dict(staticOnly=True,vendorExecution=False,components=components,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins={hex(a):' '.join(p)for a,p in pins.items()},
 source=dict(offset=8843,end=182187,sha256=hashlib.sha256(raw[8843:182187]).hexdigest(),orderedProcessors=[hex(c['Kind'])for c in children],B9=0,graphLayout=layout),
 assignedLine='Fresh16A130 zeroes guide vector count80/cap84. One ordinary raw descriptor has10=-1, no earlier processor.16E850 appends first guide, increments count0->1, writes descriptor10=count-1=0 at16EAC1, then169750/16E570 update same guide/XY.24C/265 preserve assigned ID. RawDescriptor remains preassignment-1.',
 factory='Same selected249/119 owner only;156200 fresh6C=1;driver220zero ->27341->27410 capture0;freshC4false;actualB9 via247V68 ->FIRSTF0. SourceB9true/trace/reused/externallyconfigured shapes remain unsupported.',
 interior='Same pruned source graph/actual cutXY/category VALUE maps; assigned line0 excludes both-unassigned conjunction, graph09=1 excludes cased adjustment, actual graph159=0 excludes extra assigned fallback. No caller delimiter/zero fact. First1F42 request kind1,policy2C0,gap30zero,capacity256; no metadata so endpoints zero-span, interior at most one explicitFFFB write.',
 symbol='Outer99false skipsE2/E4 allocation/classification/confidence/boundarypublication but retains predecessor walk.6Ctrue bypasses alias/FFFD dispatch. Other literal acoustic/synthetic IDs retained, includingF0003;13AF30 and native category mismatch/publication flags applied. No UTF8 conversion/guessed letter.',
 next='NEW typed ENfactory/interior/literal/TEXT with explicit immutable WORD120/glyph40 allocation residue. Shared INTERNAL mechanical TEXT loop +tiny literal/interior kernels; oldRU wrappers preserve exact resource setup/residue conventions/APIs/goldens. Public EN result must retain private actual pool and expose detached snapshots only.')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print(f'PASS3 new regions/{len(pins)}pins +6compositions/actualENgraph typed-layout/24Acount0->line0/factory/symbol/interior proof; no vendor execution')
