"""Read-only evaluator arithmetic/caller proof; vendor code is never executed."""
from pathlib import Path
import hashlib,json,math,struct,sys
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();f=functions();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[
(0x1800e6780,0x1800e69f0,'ea7333f0fcd764680c6bfa0899e03eb1b32ecc245092ac5bb5f7632d13d48505'),
(0x1800e7480,0x1800e7560,'b186eb9dd1225e34f16e69a09a2b39723b3e5fb80b0abbc6ebce14f259a013c2'),
(0x1800e82c0,0x1800e8430,'3a4eaca2025f8de4a2c2565768ff208fbe74cda6f1a461a694ccda8921733637'),
(0x1800e9360,0x1800e9610,'e3bc3ace2c06ab5af464d70ceb9ea30d14c68daa5cf70aafae2493bd098703b5'),
(0x1801b86f0,0x1801b8850,'c46dd5a9a2e2d33ed7b0fb31ce4b36749d9a55624d62c2ebf70a730e6df485f8'),
(0x180249de0,0x180249e30,'795826afb221332c9852feaa595a1a96be4b9ebb3b103186632292e19d7280e8'),
(0x180249db0,0x180249de0,'85d43931d03422eda42ab26bfcd3fe08ce393b1fb63b85a49c24985c7c87b096'),
(0x1800ef270,0x1800ef430,'ed1a50db39e7095096bef436c39a36cba259b6354fc6da15356599d33dba56c5'),
(0x1801b75b0,0x1801b7f70,'8c46659918980aba36dc16fd5a663fab9069b039ab6572271a2e53c28962dcd3'),
(0x1800ee900,0x1800ee930,'cf8c40a6fc760e81606f17863ff9d316ba331f00efa0e50d52a7c3bf83e6b957'),
(0x1800ef430,0x1800ef480,'dcc81415dd0b9d38f8e66033fc6fff17ea0920357b89360b22d6f65d10be97ee'),
(0x1800ef5a0,0x1800ef632,'296e2ab29567fa08ee2d0f2e4b3c7214fb1662c3faed37521da3d3247571ec23'),
(0x1800ef6b0,0x1800ef73a,'0a99e907ce557cabc456d35b3c33f7e3118bfc85289cd3d6b5aff4a90155b3e3'),
(0x1800eaf90,0x1800eb130,'cacfbeeb2117e522424d7133560af820bc17057f0ab217d071eda0270eb22d21'),
(0x1800eb6c0,0x1800eb880,'43bc8b448b01f9776098f88808be884f03ab79a3f0072fb55b6c1cc47364a7ec')]
regions.append((0x1801b0760,0x1801b3ce0,'184c45caa2b952f13cea9fa20752a92c2693abe563bbf306b7c3e2a700a2992a'))
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins={0x1800ef6ec:('pmaddwd','xmm0, xmmword ptr [rcx - 0x20]'),0x1800ef6ff:('paddd','xmm0, xmm3'),
0x1800ef715:('phaddd','xmm2, xmm2'),0x1800ef726:('imul','eax, ecx'),
0x1800ef5dc:('pmaddwd','xmm0, xmmword ptr [rcx - 0x20]'),0x1800ef61e:('imul','eax, ecx')}
for a,expected in pins.items():
 ins=next(md.disasm(b.data[b.off(a):b.off(a)+16],a));assert(ins.mnemonic,ins.op_str)==expected
assert b.qwords(0x1802ba2c0+0x50,1)[0]==0x1800eaf90 and b.qwords(0x1802ba2c0+0x68,1)[0]==0x1800eb6c0
assert 'FUN_1800e82c0' in f['1800eaf90']and'FUN_1800e9360' in f['1800eb6c0']
for phrase in('param_23','param_2 + 0xb0','* 2','sVar2 + -4','param_2 + 0xbc'):assert phrase in f['1800e6780']
for phrase in('FUN_180249de0','param_2 + 0xbc','1 <<'):assert phrase in f['1800e7480']or phrase in f['1800eaf90']
for phrase in('param_2 + 0x160) + 0xe','sVar1 == -1','param_4','*param_3 + 0x50','*param_3 + 0x68'):assert phrase in f['1801b86f0']
for phrase in('param_7 + 0xa0','param_7 + 0xa1','param_7 + 0xe0','param_7 + 0xe1','param_2 + 0x268'):assert phrase in f['1801b75b0']
for phrase in('param_2[0x4b] == 0','lVar15 + 0xb8 + lVar27','lVar31 + 0xa0','lVar15 + 0xb9 + lVar27',
 'lVar15 + 0xe8 + lVar27','lVar31 + 0xe0','lVar15 + 0xe9 + lVar27'):assert phrase in f['1801b0760']
hidden=b.data[b.off(0x180260fd0):b.off(0x180260fd0)+192];h=hashlib.sha256(hidden).hexdigest()
assert h=='db88702e24d4ac761fb778dca068606627c3a8b25e1ff614410c963fa627e26f'
profile=json.loads((ROOT/'models/ru-RU/legacy-engine-profile.json').read_bytes())
assert tuple(profile['activations']['hidden'])==struct.unpack('<48i',hidden)
exps=struct.unpack_from('<352I',b.data,b.off(0x180260a50));assert exps==tuple(int(25*math.exp(-4+22*i/351))for i in range(352))
engine=Binary('MyScriptEngine.dll');engineStart=0x180053f20;engineEnd=0x180054080
assert hashlib.sha256(engine.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
assert hashlib.sha256(engine.data[engine.off(engineStart):engine.off(engineEnd)]).hexdigest()=='54eae8fb5bb67e77ea98fefdb16596fa7729c6e1fd7119669fa4393e6713a756'
enginePins={0x180053f57:('lea','r8, [rbx + 8]'),0x180053f71:('lea','r8, [rbx + 0xa]'),
 0x180053f9d:('lea','r8, [rbx + 0xc]'),0x180053fc9:('lea','r8, [rbx + 0xe]')}
for a,expected in enginePins.items():
 ins=next(md.disasm(engine.data[engine.off(a):engine.off(a)+16],a));assert(ins.mnemonic,ins.op_str)==expected
for name in('en-US/en_US-ak-cur.lite.res','en-US/en_US-ak-cur.res'):
 data=(ROOT/'models'/name).read_bytes();assert struct.unpack_from('<H',data,8942+6)[0]==180
report=dict(noVendorExecution=True,regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],
 pins=[dict(address=hex(a),instruction=' '.join(v))for a,v in pins.items()],hiddenLutSha256=h,
 hiddenEntries=48,expEntries=352,actualSourceSpecial180=True,
 engineHeaderRegion=dict(start=hex(engineStart),end=hex(engineEnd),sha256='54eae8fb5bb67e77ea98fefdb16596fa7729c6e1fd7119669fa4393e6713a756'),
 enginePins=[dict(address=hex(a),instruction=' '.join(v))for a,v in enginePins.items()],
 noInitialBoundary='1B75B0 optional context append gates A0/E0 are false in actual119 slots; no borrowed28-channel context. Actual24C/source feature producer and activation construction factory remain unjoined.',
 scope='Prepared full vectors/activation object only; no masks, aliases, confidence, native features or controller.')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print('PASS16Text+1Engine ranges/6SIMD+4ALpins/2dispatchbindings/native48hidden+352EXP entries/special180 caller/noInitial append boundary')
