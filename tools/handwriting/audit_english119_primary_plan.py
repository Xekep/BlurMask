"""Read-only FIRST119 primary/ancestor/1941B0 ownership plan. Static PE only."""
from pathlib import Path
import hashlib,json,runpy,struct,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);saved=sys.argv
sys.argv=[str(Path(__file__).with_name('audit_english119_cache_plan.py')),str(target.with_suffix('.cache.json'))]
g=runpy.run_path(sys.argv[0]);sys.argv=saved;b=g['b'];f=g['f'];root=g['root'];md=Cs(CS_ARCH_X86,CS_MODE_64)
sys.argv=[str(Path(__file__).with_name('audit_first_protocol_ownership.py')),'--output',str(target.with_suffix('.protocol.json'))]
runpy.run_path(sys.argv[0]);sys.argv=saved
regions=[(0x180150d00,0x180151c10,'e7f15c59744a669d190bdb6b54ff90241d53df333f51a4a8885612ccb599f1e9'),
 (0x1801a6980,0x1801a6e70,'f800765cefd5dfd32a841e31153eb786b449c5ad309bb86d82ea228b981f88bc'),
 (0x18018d920,0x18018db40,'9acbabc3d24fda474edc777e1d4dd49f47b91794cff6d69f0cadde7fc2095982'),
 (0x18016a130,0x18016a280,'2dcfc82d3b237c5afa5ab00d042be0637eabbe5533d2bbacd20de71f2e07eca7'),
 (0x1801941b0,0x180195520,'8dbf55ad280b0857313412cec6b5721b964d8f9f62397b42e643fdd46cc47169'),
 (0x180195c40,0x180196110,'30e60dabb96d698938070a462dd57b28748638a4cb5fb11eed079d10ebd5b246'),
 (0x1801ee3b0,0x1801ef070,'ad0a304c4877b880a25b3c2fa88bf64d194442b65aaf0344c1a34f0e7ed87c48'),
 (0x1800103e0,0x180010830,'f662baacc0e1d01add637540904a3c7e120978fffbf8683b12b7debad87aa426'),
 (0x1801eddb0,0x1801edde0,'5731aa8620e0dbe5ec1f5a58ce47fa3b034eccc700ceab40395d0fd33b1f94a9')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins={0x1801514af:('mov','edx, 0x23f'),0x1801514fb:('mov','rbx, qword ptr [rbx + 0x60]'),
 0x180151519:('call','rdi'),0x180151525:('setne','byte ptr [rbp - 0x44]'),0x180151593:('lea','r8, [rbp - 0x49]'),
 0x1801a6db9:('movzx','eax, byte ptr [rbp + 5]'),0x1801a6dbd:('mov','byte ptr [rsp + 0x31], al'),
 0x1801a6df2:('lea','r8, [rsp + 0x30]'),0x1801a6dfd:('mov','r9, qword ptr [rax + 0x18]'),
 0x18018d971:('movzx','eax, byte ptr [r8 + 1]'),0x18018d976:('mov','byte ptr [rdx + 0x210], al'),
 0x18016a25d:('mov','dword ptr [rdx + 0x9c], 0xbf800000'),0x180168bf7:('movss','xmm0, dword ptr [rax + 0x9c]'),
 0x180194211:('cmp','byte ptr [r14 + 0x210], 0'),0x180194229:('movaps','xmm6, xmm0'),
 0x180194257:('mov','r8, qword ptr [rax + 0x58]'),0x180194268:('movss','xmm0, dword ptr [r14 + 0x6c]'),
 0x180195f58:('cmp','byte ptr [rdi + 0x10c], al'),0x180195de2:('mov','r8d, r14d'),
 0x180195f20:('cvttss2si','eax, xmm0'),0x180195f24:('shl','eax, 0xd'),
 0x1801ee5fa:('cmp','r8w, word ptr [rdi + 0xe6]'),0x1801ee604:('cmp','byte ptr [rdi + 0xe0], 0'),
 0x1801ee62b:('movzx','ecx, word ptr [rdx + rsi + 0xe]'),0x1801ee632:('shr','ecx, 1'),0x1801ee634:('and','ecx, 7'),
 0x1801ee8ec:('mov','byte ptr [rbp - 0x34], 5'),0x1801ee8f9:('mov','ecx, 6')}
for a,p in pins.items():
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==p,(hex(a),i.mnemonic,i.op_str,p)
assert b.qwords(0x1802e1ff0+0x10,1)[0]==0x1801a6980 and b.qwords(0x1802e1490+0x18,1)[0]==0x18018d920
assert b.qwords(0x1802df340+0x58,1)[0]==0x180168bf0
assert 'cVar6 = FUN_18016a130()'in f['180163970']and'*(undefined8 **)(param_2 + 0x120) = puVar9'in f['180163970']
assert 'local_37 = param_3[5]'in f['1801a6980']and'*(undefined1 *)(param_2 + 0x210) = *(undefined1 *)(param_3 + 1)'in f['18018d920']
assert 'param_2 + 0x134'in f['18018d920']and'*(undefined1 *)(param_2 + 0x242) = 0'in f['18018d920']
assert 'iVar6 = (**(code **)(*plVar1 + 0x68))'in f['1801edde0']and'if (iVar6 == 0)'in f['1801edde0']and'LAB_1801eddb0'in f['1801edde0']
outer=f['1801ef070'];inner=f['1801ee3b0'];sampler=f['1801941b0'];kind5=f['180195c40'].split("case '\\x05':",1)[1].split('break;',1)[0]
assert'0x188'in kind5 and'iVar10 + 1,0'in kind5 and'0x10 + (ulonglong)*(ushort *)(param_3 + 3) * 0xb8'in kind5
assert'*(longlong *)(param_3 + 0x70)'in inner and'*(longlong *)(param_3 + 0x78)'in inner
walk=inner[inner.index('while( true )',inner.index('local_ba8 = local_b88 * 0x28')):inner.index('local_ca0 = local_c36')]
assert walk.index('iVar11 == -1')<walk.index('uVar23 = (ulonglong)*(byte *)')
assert'(iVar20 - (*(ushort *)(lVar16 + 0xe + lVar1) >> 1 & 7)) + -1'in walk
for x in('uStack_bfa','uStack_bf8','uStack_be2','iStack_c04 = iVar20','iVar20 = local_bb0'):assert x in inner
assert'local_2e0 = 0'in outer and'local_318 = 0xffff'in outer and'*(longlong *)(local_1d8 + 0x50)'in outer
model=next(x for x in g['g']['models']if x['path']=='en-US/en_US-ak-cur.lite.res');d=(root/'models'/model['path']).read_bytes()
processors=[x for x in model['first']['children']if x['destination'].startswith('parametersC0.58[')]
assert[x['kind']for x in processors]==[0x24a,0x24c,0x265]
samplerNode=next(x for x in model['first']['children']if x['destination']=='247.130');assert samplerNode['version']==0x108
at=samplerNode['offset']+65;v,n=struct.unpack_from('<HH',d,at);assert(v,n)==(0x201,217);at+=4
def ids():
 global at
 n=d[at];at=(at+4)&~3;v=struct.unpack_from('<'+'i'*n,d,at);at+=n*4;return list(v)
first=ids();fm=list(struct.unpack_from('<220b',d,at));at+=220;geometry=ids();sm=list(struct.unpack_from('<220b',d,at));at+=220;em=list(struct.unpack_from('<220b',d,at));at+=220+13*len(geometry)
tail=at;assert(first,len(geometry),tail)==([0,2,3,1,4,6,5],24,18124)
assert all(a==-1 or first[a]<=5 for a in fm[:217])and(first[fm[217]],first[fm[218]],first[fm[219]])==(6,5,5)
assert(sm[217],em[217])==(7,-1)and sm[182:185]==[-1]*3 and em[182:185]==[-1]*3
assert(struct.unpack_from('<f',d,tail+33)[0],d[tail+189],d[tail+8])==(-.5,1,1)
assert d[18338:18340]==b'\0\0'and struct.unpack_from('<BBHH',d,16796)==(0,0,5,65535)
report=dict(readOnly=True,vendorExecution=False,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins={hex(a):' '.join(p)for a,p in pins.items()},
 firstFlag='150D00 type23F(container60) -> graph packet5 ->1A6980 sampler packet1 ->18D920210. Same NEW20E count0/controller23F construction as audited first protocol owner; no external/reused/serialized driver.',
 sourceGap='163970 creates secondary with16A130;16A25D F32−1 at9C ->218V58/168BF0 ->1941B0 negative branch6C. Actual first source has only24A/24C/265, excluding spacing writers174530/175350; no supplied source/property changes.',
 categories='AL217;24 prototype indices,42 composites. Raw first-map blank217 index5 resolvesVALUE6; synthetic218/219 index6 resolvesVALUE5. All ordinary217 AL first-ID values <=5. Class query takes prototype; kind2 matrix uses resolved FirstId, and blank has no kind2.',
 blank='1941B0 cache10=maxX due10C1, prefixMaxX, height,70−.5,negative108;195C40 kind5:graph108sameordinary0, thenwhole-start checks current and nextnode arc0, addcache10 andcomparelimit.118 then140; pair index7 actualblankgeometry.',
 ancestors='SkipE2/E6 andwhenE0falseEA/EC/EE. Stop when nextnode−1 BEFORE reading previous-alternative255. Original acoustic F0/F8 parent preserved;118/140 use separate ancestor. E8xor checks original parent; callback prior label comes from ancestor.',
 ordinary='Same-descriptor unsuppressed type0. FFFDxor kind0 rejects/kind2 matrix bypass; first6 unchanged. Pair195B30 retains negative offsets; translation truncates before8192shift. New owner must enforce all scope rather than silently drop rows.',
 next='Extract unchanged numeric existing BlankPrimary.RunCore into internal sharedkernel, new ENpolicy/transition/primary owning adapters, actualCandidates.SourceRanges internal getter. Public oldRU API/captures remain unchanged; no B0/B8/terminal/WORD claim.')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print(f'PASS{len(regions)}nativeblocks/{len(pins)}pins +source9C/first210/categoricalblank/retainedroot ancestry/zero-trait resolver; read-only owning primary plan')
