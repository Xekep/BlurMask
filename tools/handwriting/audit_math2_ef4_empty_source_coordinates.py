"""Static EF4 byte-one existing-source provenance and distinct coordinate planes.
No vendor execution or inference. Prepared controls are data-only and audited separately.
"""
from pathlib import Path
import hashlib,json,runpy,struct,subprocess,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
out=Path(sys.argv[1]).resolve();out.parent.mkdir(parents=True,exist_ok=True)
b=Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions=[(6442741568, 6442742784, '9dcab04d7fc764a7c76629a6a285da0b8af58719dc3ca8b91b4d4f208af64832'),
 (6442744912, 6442747312, '7176ded3572da44ea4d47c139eb64fd44af6d8552f45df66c5dbaaa1a357a49b'),
 (6442689120, 6442693152, '43b6395c140ccc4975bcc1dde81378e5df1745dd5bdd1cbdb88a79007780f19d'),
 (6442681872, 6442689120, '180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a'),
 (6442676000, 6442677296, 'c4b2044a8fa4bcc00c9ede6610d678daa4f22e379032a38944c9181678559b85'),
 (6442721536, 6442723834, '5759f6dc6cc730baa03b865f1ba5a75a659b75a47801ef520f5fa8041d726599'),
 (6442717760, 6442718816, 'a05eaf260d8cba544af148a65ed157ec81db6790af15008fae9416209de8d9df'),
 (6442718816, 6442719792, '269bf16800f6bafbe1f7236773a0d6955e82f7b61846ff7d1986d2089d32d2fd'),
 (6442713392, 6442713520, 'b5e4352527498709b371970215708d494d676015c8d6f6962af601009a88c811'),
 (6442713520, 6442713712, '41f82fed120475fadb8d721a57c1529359e57fbdb57e1435ebfbbf0b77a9b88b'),
 (6442720496, 6442720960, '58885c0c050e8841686ded2be8584433c4f0cd819621d12efb9ad9ae720cfebe'),
 (6442734688, 6442736528, '8a9d69ac15350ad940b5871b5753d679ea833dafffe8eba010d8699cb47ff908'),
 (6442733760, 6442734688, '155f71ec1b5fea2831e30c94fa71dfcd3d3c8753d41bfe22dd27df8b9eb51693'),
 (6442670576, 6442671184, '44a3ff6d114956fe746eb35b3d80f0e3867f6094fc8d8f0da18c353ef1ce6b48'),
 (6442747312, 6442749424, '91497ec8275aecfb226bb87b82da6e1bc7f44006007877bd7148643bb9e632cf')]
pins=[(6442741602, 'mov', 'ebp, dword ptr [r8 + 0x18]'),
 (6442741609, 'mov', 'rsi, qword ptr [r8 + 0x10]'),
 (6442741619, 'mov', 'dword ptr [rax], 0'),
 (6442741625, 'test', 'ebp, ebp'),
 (6442741627, 'je', '0x180046fb6'),
 (6442741637, 'cmp', 'dword ptr [rsi + rdx*8 + 0xc], 4'),
 (6442741642, 'jne', '0x180046fb0'),
 (6442741673, 'cmp', 'eax, 0xc25'),
 (6442741678, 'je', '0x180046fc7'),
 (6442741684, 'jb', '0x180046f80'),
 (6442741686, 'mov', 'al, 1'),
 (6442741702, 'ret', ''),
 (6442745914, 'xor', 'eax, eax'),
 (6442745916, 'lea', 'r9, [rbp - 0x80]'),
 (6442745920, 'mov', 'qword ptr [rbp - 0x80], rax'),
 (6442745927, 'mov', 'qword ptr [rbp - 0x78], rax'),
 (6442745947, 'call', '0x180046f40'),
 (6442746202, 'lea', 'rax, [rbp - 0x80]'),
 (6442746211, 'mov', 'qword ptr [rsp + 0x20], rax'),
 (6442746216, 'call', 'r10'),
 (6442689135, 'lea', 'rbp, [rax - 0x448]'),
 (6442689246, 'mov', 'rax, qword ptr [rbp + 0x470]'),
 (6442689266, 'mov', 'qword ptr [rbp - 0x48], rax'),
 (6442690411, 'mov', 'rax, qword ptr [rbp - 0x48]'),
 (6442690420, 'mov', 'qword ptr [rsp + 0x30], rax'),
 (6442690441, 'call', '0x180038610'),
 (6442690826, 'mov', 'rax, qword ptr [rbp - 0x48]'),
 (6442690830, 'mov', 'qword ptr [rsp + 0x30], rax'),
 (6442690848, 'call', '0x180038610'),
 (6442687431, 'mov', 'r8, qword ptr [rbp + 0x80]'),
 (6442687438, 'mov', 'qword ptr [rsp + 0x30], r15'),
 (6442687443, 'mov', 'qword ptr [rsp + 0x28], rcx'),
 (6442687451, 'mov', 'r9, qword ptr [r9 + 0xf8]'),
 (6442687458, 'mov', 'qword ptr [rsp + 0x20], rdi'),
 (6442687463, 'call', '0x180036f20'),
 (6442676013, 'lea', 'rbp, [rsp - 0xd8]'),
 (6442676045, 'mov', 'rax, qword ptr [rbp + 0x148]'),
 (6442676057, 'mov', 'qword ptr [rbp - 0x60], rax'),
 (6442676091, 'mov', 'qword ptr [rbp - 0x58], r8'),
 (6442677000, 'movzx', 'edi, word ptr [rsi + 0x20]'),
 (6442677021, 'movss', 'xmm1, dword ptr [rsi + 0x54]'),
 (6442677026, 'lea', 'rcx, [rsi + 0x68]'),
 (6442677030, 'lea', 'r9, [rbp - 0x50]'),
 (6442677034, 'movzx', 'r8d, di'),
 (6442677045, 'mov', 'qword ptr [rsp + 0x48], rax'),
 (6442677050, 'movzx', 'eax, byte ptr [rsi + 0x51]'),
 (6442677054, 'mov', 'byte ptr [rsp + 0x40], al'),
 (6442677058, 'mov', 'rax, qword ptr [rbp + 0x150]'),
 (6442677065, 'mov', 'qword ptr [rsp + 0x38], rax'),
 (6442677070, 'mov', 'rax, qword ptr [rbp - 0x58]'),
 (6442677074, 'mov', 'qword ptr [rsp + 0x30], rax'),
 (6442677079, 'mov', 'qword ptr [rsp + 0x28], rcx'),
 (6442677087, 'mov', 'qword ptr [rsp + 0x20], rbx'),
 (6442677092, 'call', 'rdx'),
 (6442721659, 'mov', 'qword ptr [rbp - 0x38], r9'),
 (6442721669, 'mov', 'rax, qword ptr [r9 + 0x18]'),
 (6442721677, 'mov', 'rax, qword ptr [r9 + 0x10]'),
 (6442721716, 'movzx', 'ebx, word ptr [rcx + r13*2]'),
 (6442721753, 'test', 'al, al'),
 (6442721759, 'cmove', 'rcx, r15'),
 (6442721828, 'mov', 'qword ptr [rsp + 0x28], r15'),
 (6442721836, 'mov', 'qword ptr [rsp + 0x20], r8'),
 (6442721844, 'call', '0x18003f350'),
 (6442721996, 'mov', 'rax, qword ptr [r14 + 8]'),
 (6442722044, 'call', 'r9'),
 (6442722049, 'je', '0x18004234e'),
 (6442722051, 'mov', 'rax, qword ptr [rbp - 0x38]'),
 (6442722059, 'add', 'rdx, qword ptr [rax + 0x68]'),
 (6442722085, 'call', '0x1801ac820'),
 (6442722102, 'mov', 'dword ptr [rcx], r13d'),
 (6442722133, 'add', 'qword ptr [rbp - 0x68], rax'),
 (6442722248, 'movzx', 'eax, word ptr [rax + rdx*2]'),
 (6442722252, 'mov', 'word ptr [rcx + rdx*4], ax'),
 (6442722260, 'mov', 'byte ptr [rax + rdx*4 + 3], 1'),
 (6442722269, 'mov', 'byte ptr [rax + rdx*4 + 2], dil'),
 (6442722310, 'cmp', 'byte ptr [rbp + 0x2a0], 0'),
 (6442722327, 'mov', 'qword ptr [rbp - 0x68], rdi'),
 (6442722339, 'mov', 'qword ptr [rbp - 0x58], rdi'),
 (6442722351, 'jne', '0x180042473'),
 (6442722419, 'mov', 'rax, qword ptr [rbp - 0x70]'),
 (6442722423, 'cmp', 'dword ptr [rax + 8], 0'),
 (6442722427, 'jne', '0x1800424bb'),
 (6442722438, 'mov', 'byte ptr [rsp + 0x28], 0'),
 (6442722460, 'xor', 'edx, edx'),
 (6442722465, 'call', '0x180041020'),
 (6442723204, 'mov', 'rdx, qword ptr [rbp - 0x70]'),
 (6442723208, 'lea', 'rax, [rbp - 0x68]'),
 (6442723217, 'lea', 'r15, [r14 + 0x50]'),
 (6442723231, 'mov', 'word ptr [rsp + 0x30], ax'),
 (6442723236, 'lea', 'r8, [rbp + 0xf0]'),
 (6442723248, 'mov', 'qword ptr [rsp + 0x28], r15'),
 (6442723256, 'mov', 'qword ptr [rsp + 0x20], rax'),
 (6442723261, 'call', '0x180041240'),
 (6442723274, 'lea', 'r9, [rbp - 0x68]'),
 (6442723287, 'call', '0x180041ec0'),
 (6442723304, 'lea', 'r9, [rbp - 0x58]'),
 (6442723322, 'mov', 'r8, r15'),
 (6442723325, 'mov', 'rax, qword ptr [rbp - 0x40]'),
 (6442723332, 'mov', 'qword ptr [rsp + 0x20], rax'),
 (6442723337, 'call', '0x180041660'),
 (6442723363, 'mov', 'r13, qword ptr [rbp - 0x58]'),
 (6442723367, 'mov', 'ebx, eax'),
 (6442723379, 'cmp', 'byte ptr [rbx + r13], 0'),
 (6442723384, 'je', '0x18004289e'),
 (6442723500, 'cmp', 'byte ptr [rcx + r13], 0'),
 (6442723505, 'je', '0x1800428c0'),
 (6442723507, 'mov', 'rax, qword ptr [rbp - 0x68]'),
 (6442723511, 'cmp', 'byte ptr [rcx + rax], 0'),
 (6442723515, 'jne', '0x18004291d'),
 (6442723536, 'movzx', 'r8d, word ptr [rax + 0xa]'),
 (6442723541, 'call', '0x180041ba0'),
 (6442723567, 'mov', 'r8, qword ptr [rbp - 0x38]'),
 (6442723581, 'lea', 'r9, [rbp - 0x68]'),
 (6442723596, 'call', '0x180041cf0'),
 (6442723613, 'xor', 'bl, bl'),
 (6442717829, 'mov', 'r9d, dword ptr [rsi + 8]'),
 (6442717833, 'call', '0x1800401b0'),
 (6442717919, 'cmp', 'dword ptr [r8 + r13*4], -1'),
 (6442717924, 'jne', '0x18004160d'),
 (6442718406, 'mov', 'word ptr [rsp + 0xb8], ax'),
 (6442718414, 'mov', 'word ptr [rsp + 0xba], 0x100'),
 (6442718700, 'mov', 'byte ptr [r8], 1'),
 (6442713556, 'mov', 'ebp, r9d'),
 (6442713563, 'lea', 'edx, [rax + rbp]'),
 (6442713663, 'mov', 'r8, rbp'),
 (6442713666, 'xor', 'edx, edx'),
 (6442713668, 'call', '0x1801ac460'),
 (6442713679, 'mov', 'dword ptr [rdi], eax'),
 (6442719627, 'cmp', 'byte ptr [rdx + rax], 0'),
 (6442719631, 'jne', '0x180041996'),
 (6442719633, 'mov', 'dword ptr [rdi + rdx*4], esi'),
 (6442719636, 'inc', 'esi'),
 (6442719667, 'cmp', 'eax, -1'),
 (6442719672, 'mov', 'ecx, dword ptr [rdi + rax*4]'),
 (6442719675, 'mov', 'dword ptr [rdx], ecx'),
 (6442720524, 'mov', 'edi, dword ptr [r9 + 8]'),
 (6442720656, 'mov', 'dword ptr [r9], edx'),
 (6442720662, 'movzx', 'ecx, byte ptr [r8]'),
 (6442720670, 'test', 'cl, cl'),
 (6442720676, 'cmovne', 'eax, edx'),
 (6442720725, 'movsxd', 'rax, dword ptr [rsi]'),
 (6442720731, 'je', '0x180041e15'),
 (6442720736, 'inc', 'ebx'),
 (6442720741, 'cmp', 'byte ptr [rcx + rax], 0'),
 (6442720745, 'jne', '0x180041e15'),
 (6442720747, 'movsxd', 'rcx, dword ptr [r15 + rcx*4]'),
 (6442720754, 'imul', 'rax, qword ptr [r13 + 0x78]'),
 (6442720759, 'imul', 'rcx, qword ptr [r13 + 0x70]'),
 (6442747814, 'lea', 'r8, [rsi + 0x1e2]'),
 (6442747821, 'mov', 'r9d, 2'),
 (6442747827, 'mov', 'qword ptr [rsp + 0x20], 1'),
 (6442747843, 'call', 'r10'),
 (6442747935, 'movzx', 'r13d, word ptr [rsi + 0x1e2]'),
 (6442735644, 'movzx', 'esi, word ptr [rdi + 0x1e2]'),
 (6442735660, 'call', '0x180022d30'),
 (6442735670, 'mov', 'word ptr [rdi + 0x1e4], ax'),
 (6442735682, 'mov', 'word ptr [rdi + 0x1e4], si'),
 (6442735689, 'inc', 'si'),
 (6442735712, 'cmp', 'si, ax'),
 (6442735715, 'jae', '0x18004587c'),
 (6442735737, 'movzx', 'esi, ax'),
 (6442735763, 'call', '0x180022d30'),
 (6442735768, 'mov', 'word ptr [rdi + 0x1e8], ax'),
 (6442735778, 'jne', '0x1800458ae'),
 (6442735780, 'mov', 'word ptr [rdi + 0x1e8], si'),
 (6442734414, 'movzx', 'eax, word ptr [rdi + 0x1e8]'),
 (6442734421, 'mov', 'qword ptr [rdi + 0x80], rax'),
 (6442670795, 'movups', 'xmm0, xmmword ptr [rbp]'),
 (6442670799, 'movups', 'xmmword ptr [rdi], xmm0'),
 (6442670802, 'movups', 'xmm1, xmmword ptr [rbp + 0x10]'),
 (6442670806, 'movups', 'xmmword ptr [rdi + 0x10], xmm1'),
 (6442670810, 'movups', 'xmm0, xmmword ptr [rbp + 0x20]'),
 (6442670814, 'movups', 'xmmword ptr [rdi + 0x20], xmm0')]
instructions={}
for a,z,digest in regions:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==digest
 instructions.update({i.address:(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,a)})
for a,m,o in pins:assert instructions[a]==(m,o),(hex(a),instructions.get(a))
bindings=[(0x1801fd0d8+0x20,0x180047c50),(0x1801ed930+0x28,0x18003a260)]
for a,v in bindings:assert b.qwords(a,1)==(v,)
for name in ['cold_correction_request','observable_token_gate']:
 p=out.with_name(out.stem+'-'+name+'.json')
 subprocess.run([sys.executable,str(ROOT/f'tools/handwriting/audit_math2_{name}.py'),str(p)],check=True)
gate=json.loads(out.with_name(out.stem+'-observable_token_gate.json').read_bytes())
assert gate['ef3Table'][5]==65535 and gate['sourceAlphabetCount']==299
reader=runpy.run_path(str(ROOT/'tools/export-ink-math-model.py'))
raw=(ROOT/'models/math2/math2-unwrapped.res').read_bytes()
assert hashlib.sha256(raw).hexdigest()=='2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
assert reader['header'](raw)[:3]==(0xeed,0,0x103)
_,_,_,end=reader['header'](raw,90);r=reader['Reader'](raw,106,end)
fields=r.read('H',8);n=r.read('H')
for inc in [32,64,1]:
 table=r.read('H',n);n=next(v+inc for v in reversed(table)if v!=65535)
spans=r.read('H',n+1);entries=[r.read('II')for _ in range(spans[-1]+1)];utf8=r.bytes(entries[-1][0])
labels={label&65535:utf8[offset:entries[j+1][0]].decode('utf8')for j,(offset,label)in enumerate(entries[:-1])}
r.read('H',fields[-1]);bits=r.read('B')
if bits:r.bytes((fields[-1]+7)//8)
triplets=r.read('H');r.read('H',triplets*3);extra=r.read('I')if triplets else 0
for flag in [1,2,4]:
 if extra&flag:r.read('B'if triplets<128 else'H',fields[-1])
r.read('B');assert r.pos==end
serialized_count=struct.unpack_from('<H',raw,end)[0]
assert serialized_count==297 and fields[-1]==299 and '[ink]'not in labels.values() and '\ufffd'not in labels.values()
# EED1E2 starts at serialized297. Missing [ink] takes297 then increments;
# max(counter,AL299) precedes missing FFFD -> replacement299.
replacement=max(serialized_count+1,fields[-1]);assert replacement==299
marker_address=0x18004588a+0x1d4c9a
assert b.data[b.off(marker_address):b.off(marker_address)+4]==b'\xef\xbf\xbd\0'
ink_address=0x18004581c+0x1d4db4
assert b.data[b.off(ink_address):b.off(ink_address)+6]==b'[ink]\0'
observable=set(gate['observableIds'])
controls=[]
for control in range(256):
 count=2+control%9;segments=1+control%7
 primary=[([2,3,262,268,0,276,296][(control+i*3)%7])for i in range(count)]
 primary[-1]=2;visible=[i for i,label in enumerate(primary)if label in observable]
 matrix=[[((i*5+j*3+control)%11)/16 for j in range(segments)]for i in range(count)]
 for j in range(segments):matrix[visible[(control+j)%len(visible)]][j]=1.0
 filtered=[matrix[i][:]for i in visible]
 assignments=[]
 for j in range(segments):
  best=-3.4028234663852886e38;winner=-1
  for k,row in enumerate(filtered):
   if row[j]>best:best=row[j];winner=k
  assignments.append(visible[winner])
 # Every segment is a single-segment stroke: no041660 boundary suppression.
 use=[assignments.count(i)for i in range(count)]
 deleted=[int(use[i]==0 and primary[i]in observable)for i in range(count)]
 old_to_new=[];next_index=0
 for d in deleted:
  old_to_new.append(-1 if d else next_index)
  if not d:next_index+=1
 after=[old_to_new[i]for i in assignments]
 initial=[0]*count
 tail_rows=after[:] # literal041CF0(initialzeros, ORIGINAL matrix), not retained-row projection
 retained=[i for i,d in enumerate(deleted)if not d]
 controls.append(dict(id=control,primary=primary,segments=segments,matrix=matrix,observableRows=visible,
  filtered=filtered,assignmentsBefore=assignments,initialMask=initial,deletionMask=deleted,
  oldToNew=old_to_new,assignmentsAfter=after,retainedOriginalRows=retained,
  tailRows=tail_rows,tailValues=[matrix[i][j]for j,i in enumerate(tail_rows)]))
truth=[dict(initial=i,deleted=d,success=not(d!=0 and i!=0))for i in [0,1,2,255]for d in [0,1,2,255]]
source_cases=[]
for kind in [0,1,4,7]:
 for type_id in [0xc25,0xc1a,0xc66,0]:
  match=kind==4 and type_id==0xc25
  source_cases.append(dict(kind=kind,typeId=type_id,entersC38=match,emptyFastReturn=not match))
report=dict(scope='Static native byte1 branch plus explicit prepared coordinate cases; no actual C25-free native source ownership claimed',
 regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],instructionPins=len(pins),bindings=len(bindings),
 sourceRoute='047C50 zeroexistingvector ->046F40 scan ->EEE03A260.arg5 ->038610.arg7 ->036F20.arg3 ->EF4.arg7',
 nativeDefaultEmptyExistingSourceProved=False,missingProducer='Native selected-item source has no kind4/C25; fresh EEB/C66 features alone do not establish this',
 replacementLabel=replacement,separatorLabel=gate['ef3Table'][5],serializedCounter=serialized_count,
 abi=dict(arg4='original activated tensor Nprimary x Ssegments',arg5='actual completed168 record',arg6='sameEF3 owner',
  arg7='source-owned existing fourbyte-unit vector',arg8='forwarded opaque input scalar; do not synthesize or conflate with strokeEnds',
  arg9='fresh unmodified EEB byte1',arg10='actual strk_ends tensor or explicit null from original input packet'),
 planes=dict(original='rbp-38 original activated tensor',observable='rbp+F0 local copy and rsp+68 original-row map',
  units='completed50/58 mutable fourbyte units',assignments='rsp+58 persegment units;041660 remaps',
  initialMask='rbp-68 zerooriginal/oneinserted;used in final041CF0',deletionMask='rbp-58 used to prune units/aux; distinct from scoremask'),
 guard='postDeletion!=0 AND initialMask!=0 ->04291D false; originalzero removals allowed',
 controls=controls,preservationTruth=truth,sourceCases=source_cases,vendorExecution=False,runtimeChanges=False)
out.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(regions)} regions/{len(pins)} pins/{len(bindings)} bindings;256 prepared coordinate cases; native-default empty source remains UNPROVED')
