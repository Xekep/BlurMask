"""Static-only FIRST119 fresh ensemble/mask/ordered-group/cache ownership."""
from pathlib import Path
import hashlib,json,runpy,struct,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
target=Path(sys.argv[1]);saved=sys.argv;sys.argv=[str(Path(__file__).with_name('audit_english_acoustic119_plan.py')),str(target.with_suffix('.inventory.json'))]
g=runpy.run_path(sys.argv[0]);sys.argv=saved;b=g['b'];f=g['f'];root=g['ROOT'];md=Cs(CS_ARCH_X86,CS_MODE_64)
from inspect_text_features import Binary,functions,parse_alphabet
engine=Binary('MyScriptEngine.dll');ef=functions('MyScriptEngine.dll')
assert hashlib.sha256(engine.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
regions=[
(0x1801b0330,0x1801b0550,'bb91230d307e312a4c7fb0b409b2ec1232acf3c3a71097e391f9b2982af95ff5'),
(0x1801b9e20,0x1801ba4a0,'66e191e9bc1fd5ecb3e9aa370d828290c697e6abf11d9809511d22f7acdfe187'),
(0x1801bb020,0x1801bbf80,'5d85f282169b03e2dd6d554bccea42e00580a01c20dc4126b574180ce1b7bd01'),
(0x1801b8850,0x1801b8eb0,'fed7df45c3131e060754bc4bf831c4714d73e6c65d35077e4308125c7188b663'),
(0x1801b8eb0,0x1801b95a0,'6bbeb8a0e8ebd6eded955a4d6e4ed52ea2b31608506e954f3c97cee108670fa3'),
(0x1801d60d0,0x1801d6230,'1aebcb9992ec1b108cca4f20118f0d9b7ce2f6c644b96fe7fb265e21bae24971')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins={0x1801b04c4:('mov','dword ptr [rbx + 0x1b8], esi'),0x1801b04ca:('mov','qword ptr [rbx + 0x1bc], 0x3f800000'),
0x180151ba0:('mov','eax, dword ptr [rdx + 0x30]'),0x180151b98:('mov','dword ptr [rdx + 0x30], r8d'),
0x1801bbc50:('mov','rax, qword ptr [rdi + 0xe0]'),0x1801bbc64:('jbe','0x1801bbdcb'),
0x1801bbca3:('cmp','byte ptr [rax + r13], 0'),0x1801bbcc8:('call','0x18024e970'),
0x1801bbcd0:('cvtsd2ss','xmm1, xmm0'),0x1801bbcda:('maxss','xmm1, xmm6'),
0x1801bbd36:('mov','eax, dword ptr [rsi + rbp*4]'),0x1801bbd53:('call','0x18024e970'),
0x1801bbd5b:('cvtsd2ss','xmm2, xmm0'),0x1801bbd76:('subss','xmm1, xmm0'),
0x1801bbd7a:('mulss','xmm0, xmm6'),0x1801bbd7e:('mulss','xmm1, xmm2'),0x1801bbd82:('addss','xmm1, xmm0'),
0x1801bbd8d:('call','0x18024f110'),0x1801bbdae:('cvttsd2si','eax, xmm0'),0x1801bbdb2:('mov','dword ptr [rsi + rbp*4], eax'),
0x1801bbdbc:('inc','bx'),0x1801bbdd2:('inc','r15w')}
for a,p in pins.items():i=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(i.mnemonic,i.op_str)==p
terms={
'1801b9e20':['if (fVar20 == 2.0)','*piVar11 >> 1','param_2 + 0x1bc','param_2 + 0xc'],
'1801d60d0':['param_3 + 0x48','~uVar2 & (uVar4 | 0x21703)','param_2 + 0x1ec'],
'1801bb020':['(uint)param_6 & 0x10','if ((uint)param_6 == 0)','*(short *)(param_2 + 200)','param_2 + 0xd0','param_2 + 0xd8','param_2 + 0xe0'],
'1801be930':['FUN_1801bb020','FUN_1801bbf80','FUN_1801bc900','FUN_1801bc680'],
'1801b0330':['*(undefined2 *)(param_2 + 0x270) = 1','*(undefined8 *)(param_2 + 0x2b8) = 0']}
for a,values in terms.items():
 for value in values:assert value in f[a],(a,value)
text=f['1801be930'];ordinary=text.index('FUN_1801bb020');assert ordinary<text.index('FUN_1801bbf80',ordinary)<text.index('FUN_1801bc900')<text.index('FUN_1801bc680')
for a in('1801be930','1801bf0a0'):assert 'param_2 + 0x1b0' in f[a]and'FUN_1801b7440'in f[a]
model=next(m for m in g['models']if m['path']=='en-US/en_US-ak-cur.lite.res');a=model['firstAcoustic'];c=a['cache'];d=(root/'models'/model['path']).read_bytes()
assert a['multiplier']==1 and a['initial']is None and a['dynamic']is None
assert[(s['slot'],s['mode'])for s in a['profiles'][0]['slots']if s['networks']]==[(0,2),(1,2)]
assert [s['networks'][0]['outputs']for s in a['profiles'][0]['slots'][:2]]==[182,182]
assert c['C8']==8 and c['D0']==[0,2,4,7,9,11,13,15,17]and all(x==struct.unpack('<f',struct.pack('<f',.99))[0]for x in c['E0'])
assert(c['B8'],c['100'],c['108'],c['128'],c['148/150present'],c['80'])==(0,0,0,0,0,0)
assert(c['E8'],c['EC'],c['EE'])==(85173,1,10)
assert struct.unpack_from('<8H',d,8942)==(181,178,65535,180,184,182,183,217)
metadata=model['first']['end']-186;mode=13;mask=struct.unpack_from('<I',d,metadata+mode*4)[0];flags=0x21703&~mask
assert flags==0x21700 and flags&16==0 and d[metadata+128]>mode
alphabet=parse_alphabet(d,8942);assert alphabet['label_count']==217
alphabetHash=hashlib.sha256(d[8942:alphabet['end']]).hexdigest()
assert alphabetHash=='5554e287937d2d8682c46eb7d85a701b15f1b86afb45f55228dbc8c1ebf7a1ff'
#053F20 DOES call0573B0. Absence of a direct+88 load is not an
#absence proof: pin the real builder's early return and its actual script inputs.
assert 'FUN_1800573b0(param_1,param_2,uVar8)'in ef['180053f20']
assert '*(undefined8 *)(param_2 + 0x88) = 0;'in ef['180052260']
builder=ef['1800573b0'];assert builder.index('if (uVar6 == 0)')<builder.index('param_2[0x11] = lVar10')
scan=ef['18004e280'];assert 'iVar4 != 5'in scan and'iVar2 - 0x660U < 10'in scan and'iVar2 - 0x6f0U < 10'in scan
scripts=[struct.unpack_from('<III',engine.data,engine.off(0x1801fb6d0)+i*12)for i in range(716)]
codepoints={ord(ch)for text in alphabet['mapping'].values()for ch in text}
assert all(next((s for a,z,s in scripts if a<=cp<=z),0)!=5 and not(0x660<=cp<=0x669 or 0x6f0<=cp<=0x6f9)for cp in codepoints)
wide={0x3000,*range(0xff01,0xff61),*range(0xffe0,0xffe7)};assert not(codepoints&wide)
assert b.qwords(0x1802e79d0+0xa8,1)[0]==0
assert b.qwords(0x1802de610+0x20,1)[0]==0x180151b90 and b.qwords(0x1802de610+0x28,1)[0]==0x180151ba0
assert '*(undefined4 *)(param_2 + 0x30) = 0'in f['1801503f0']
assert '*(undefined8 *)(param_2 + 0x38) = 0'in f['1801503f0']
assert '*(undefined8 *)(param_2 + 0x2b8) = 0'in f['1801c0cf0']
assert b.qwords(0x1802de610+0x30,1)[0]==0x180151bb0 and b.qwords(0x1802e4180+0x30,1)[0]==0x1801c0cf0
assert '*(undefined2 *)(param_2 + 0x270) = 1'in f['1801b0330'] and'param_2 + 0x271'in f['1801b0760']
assert'*(undefined2 *)(param_2 + 0xf4) = 0'in f['18000c860']
assert'param_2[0x28] = 0'in f['18016a130']and'param_2[0x29] = 0'in f['18016a130']
assert b.qwords(0x1802df340+0xd8,1)[0]==0x1801691c0
assert engine.data[engine.off(0x180052640):engine.off(0x18005264d)].hex()=='33c0483982880000000f95c0c3'
assert engine.data[engine.off(0x1800522e6):engine.off(0x1800522ed)].hex()=='48898f88000000'
assert '*(undefined4 *)(param_2 + 0x18) = 0x20dffff'in f['18000c860']
engineRegions=[(0x180052260,0x180052450,'4690de6571532c18c2aa01d267a1e9af1a99138d0b6ed7e8c23163ee81af08be'),(0x180053f20,0x1800548d0,'7f9c99b34bc63ab96949cbe6b70dec74505bade2ad2c0efd37a8b263685ce2ad')]
for lo,hi,h in engineRegions:assert hashlib.sha256(engine.data[engine.off(lo):engine.off(hi)]).hexdigest()==h
report=dict(noVendorExecution=True,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],pins={hex(a):' '.join(v)for a,v in pins.items()},terms=terms,
 actual=dict(source=model['first']['offset'],acoustic=a['offset'],width=182,alphabet=217,freshWeights=[1,1,0,0,0,0,0],flags=hex(flags),prune=True,cache=c),
 ordered='1BB020 two-pass max then sequential current-cost reads/writes per member; duplicate/overlapping groups must not be flattened. Runs after primitives/dot and before1BBF80; double EXP/log with FLOAT argument/result/mix boundaries.',
 sourceMask=dict(alphabetSha256=alphabetHash,engineRegions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in engineRegions],codepoints=len(codepoints),scriptRanges=716,derivedArabic88=False,widePairs=0,controller23fA8=None,restriction='1503F0 zero38 ->150D00 ->151BB0/1C0CF0 NULL2B8',working='270word1/271false; second-only active networks; absent148/150 bounds',penalty='16A130 zero140/148; sourceD8 false; ctorF4word0 includesF5'),
 remaining='Fresh noexternal/raw source binding only. Native053F20 calls0573B0 but actual script scan returns before allocating88; runtime binds this exact immutable AL hash. Masks run virtual/header reconstruction. Source controller/word scheduling, accepted confidence and dynamic/initial119 remain separate.')
target.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS8pinnedranges/22exactinstructions/actual182x2 freshensemble/eightorderedgroups/716script noArabic scan/knowledgeflags0x21700/prunetrue; static only')
