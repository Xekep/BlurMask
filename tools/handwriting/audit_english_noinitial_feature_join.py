"""Read-only FF/FF->pen-up->217/199/boundaries/activation join proof."""
from pathlib import Path
import hashlib,json,math,runpy,struct,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
repo=Path(__file__).resolve().parents[2];target=Path(sys.argv[1]);saved=sys.argv[:]
sys.argv=[str(repo/'tools/handwriting/audit_noinitial_first_feature_plan.py'),str(target.with_suffix('.source-plan.json'))]
g=runpy.run_path(sys.argv[0]);sys.argv=saved;b=g['b'];f=g['f'];md=Cs(CS_ARCH_X86,CS_MODE_64)
regions=[
(0x1801b4bd0,0x1801b4e40,'75140a4eb57c2e4d73f78836afa42875462619b93ab270a4aec15053fcb19c97'),
(0x1801b4e40,0x1801b5860,'743553d84b6797f26db91154402d0f600bb12dab7abca4f248420d5ec93ba520'),
(0x1801b49a0,0x1801b4ae0,'fce268a7e4541a00713daa8499aa17b9649367ecec64954b9499e52956e36d81'),
(0x1801b7f70,0x1801b8320,'a759c06886585940b707b9487dd42bdc845b33b000dca2552c051cf13c32a7bd'),
(0x1801a7970,0x1801a79e0,'392e952a126a72fcfab4c9f54e39cc839ab6826ec773e616c6f6de8d58f2e4f7'),
(0x180007070,0x180007470,'adcd037204321b3c371e5f5bc4093096c3a6a003d37f4df76ee0f40b31eb9fc7'),
(0x180007470,0x180007b20,'ec952e3beea4c668f0f6d522d49af5b7a5112006a785a2083f7aa11ac9a34b6c'),
(0x18000a9f0,0x18000ac70,'cd1dbbce1d57e3c638a4836b12f6cc3cc3f73619b43fa6231e4224b6f48f4191'),
(0x180249e30,0x180249e80,'26e14d958660ae32ac93e4ffb474453015a9c6e331b83c698a8411a2656ca5de'),
(0x1800ebb60,0x1800ebc30,'642edba8985655efe07b80ee438cd106f082349b69caad7e3e4e0f9010832568')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins={
0x1801b0794:('xor','r14d, r14d'),0x1801b085a:('mov','word ptr [rsi + 0x1a4], r14w'),
0x1801b0862:('mov','byte ptr [rsi + 0x1a6], r14b'),0x1801b0f84:('mov','byte ptr [r13 + 0x248], 1'),
0x1801b4c17:('cmp','word ptr [r8 - 8], 0'),0x1801b4c1f:('movsx','ecx, word ptr [r8]'),
0x1801b4c23:('mov','eax, 0x55555556'),0x1801b4c28:('imul','ecx'),
0x1801b5512:('cmp','byte ptr [r13 + 8], 0xff'),0x1801b5519:('cmp','byte ptr [r13 + 9], 0xff'),
0x1801b5540:('mulss','xmm0, xmm1'),0x1801b5544:('divss','xmm0, xmm10'),
0x1801b5553:('cvttss2si','edi, xmm0'),0x1801b555d:('subss','xmm0, xmm15'),
0x1801b5562:('mulss','xmm0, xmm1'),0x1801b5566:('divss','xmm0, xmm10'),
0x1801b5578:('cvttss2si','eax, xmm0'),
0x1801b804d:('mov','r10, qword ptr [rax + 0x90]'),0x1801b8054:('call','r10'),
0x1801b8075:('cmp','dword ptr [r9 + 4], 0'),
0x1801b80c6:('cmp','dword ptr [r9 + r10*4 + 8], edx'),
0x1801a7990:('movups','xmm0, xmmword ptr [rcx + rax + 8]'),
0x1801a79ba:('movups','xmm0, xmmword ptr [rcx + rax + 0x20]'),
0x180249e44:('lea','rdx, [rip + 0x17245]')}
for a,expected in pins.items():
 i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==expected
assert b.qwords(0x1802e1ff0+0x90,1)[0]==0x1801a7970
assert b.qwords(0x1802ba2c0+0x90,1)[0]==0x1800ebb60
for text in('FUN_1801b49a0','FUN_1801b4bd0','param_2 + 8) == -1','param_2 + 9) == -1'):assert text in f['1801b4e40']
for text in('sVar11 = 0x7fff','psVar8[-4] != 0','iVar5 / 3','FUN_1801b4ae0'):assert text in f['1801b4bd0']
for text in('FUN_180007070','FUN_180007470','FUN_18000a9f0','FUN_180008290','FUN_180008770'):assert text in f['18000aef0']
assert 'iVar21 == 0' in f['180007470']and'iVar19 == 0' in f['180007470']and'param_2 + 0xd0) = 1' in f['180007470']
for text in('param_2[6] * 7','param_2[10]','param_2[2] & 0x20'):assert text in f['18000a9f0']
hidden=struct.unpack_from('<48i',b.data,b.off(0x180260fd0));output=struct.unpack_from('<128i',b.data,b.off(0x180261090))
assert hidden==tuple(math.floor(1024*math.tanh(3*i/47))for i in range(48))
assert output==tuple([math.floor(1024/(1+math.exp(-8*i/127)))for i in range(127)]+[1024])
en=g['en'];data=g['ed'];acoustic=en['firstAcoustic'];offset=acoustic['offset'];profile=acoustic['profiles'][0]['offset']
assert struct.unpack_from('<BBh',data,offset+16)==(255,255,100)
assert struct.unpack_from('<BHI5H7f',data,profile)==(0,0x1011,0x3f,30,16,8,10,0,0.,0.,0.,0.,0.,0.,0.)
report=dict(staticOnly=True,noVendorExecution=True,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],
 pins=[dict(address=hex(a),instruction=' '.join(v))for a,v in pins.items()],
 actualFirst=dict(source=en['first']['offset'],acoustic=offset,flags='FF/FF',normalizationTarget=100,trajectoryGeometry=217,rasterGeometry=199,inputs=[219,201],penUpLimit=10),
 boundaryOrder='graphV90 returns stored ranges unchanged;1B7F70 reads stored first/last;normalization1B4E40 sorts separately via1B49A0. Never reorder boundary vector merely to reuse normalizer order.',
 activationReconstruction=dict(hiddenEntries=48,hiddenSha256=hashlib.sha256(struct.pack('<48i',*hidden)).hexdigest(),outputEntries=128,outputSha256=hashlib.sha256(struct.pack('<128i',*output)).hexdigest(),
 semantics='Finite mathematical reconstruction agrees with every pinned constant entry;not a claim that the native runtime constructs the constants by these formulas.'),
 nextScope='NEW prepared-range same-full-preprocessing owner;normalizedshorts/penup10/217+199+storedboundary2;source-boundactivationfactory.No actualgraph/cut/sourceclassifier until297/108 producer.')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS10newPEranges/24pins/2bindings/rawFF100/profile217199/cap10/storedBoundary/48+128nativeactivationentries')
