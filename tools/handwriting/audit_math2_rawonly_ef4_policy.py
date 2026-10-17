"""Actual cold raw-only EF4 scalar prerequisites; vendor binaries are data only."""
from pathlib import Path
import hashlib,json,struct,subprocess,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
out=Path(sys.argv[1]).resolve();out.parent.mkdir(parents=True,exist_ok=True)
b=Binary('MyScriptDocument.dll');assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions=[(6442612656,
  6442612800,
  '4379afdb74b7d3a8f2c39de533454a84aec09b82420e03859cba5caa54cfebe8'),
 (6442734688,
  6442736528,
  '8a9d69ac15350ad940b5871b5753d679ea833dafffe8eba010d8699cb47ff908'),
 (6442732224,
  6442733488,
  'f7cbc430d27bd819ff2a0d4346def61ff3939f7a8c238cd7f5ede03c104e4265'),
 (6442733760,
  6442734688,
  '155f71ec1b5fea2831e30c94fa71dfcd3d3c8753d41bfe22dd27df8b9eb51693'),
 (6442670576,
  6442671184,
  '44a3ff6d114956fe746eb35b3d80f0e3867f6094fc8d8f0da18c353ef1ce6b48'),
 (6442681872,
  6442689120,
  '180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a'),
 (6442720960,
  6442721536,
  '71dbd4cfc33c068a2ffcba976a6541d1bffc13ef9b62491840957fe51d4e49b6')]
pins=[(6442612729, 'xor', 'ecx, ecx'),
 (6442612742, 'mov', 'qword ptr [rdi + 0x48], rcx'),
 (6442612746, 'mov', 'dword ptr [rdi + 0x40], ecx'),
 (6442612777, 'mov', 'dword ptr [rdi + 0x38], 0xffffffff'),
 (6442612784, 'mov', 'word ptr [rdi + 0x3c], 0x101'),
 (6442735043, 'mov', 'qword ptr [rdi + 0xb4], 0x3dcccccd'),
 (6442732707, 'movss', 'dword ptr [rsi + 0x4c], xmm0'),
 (6442733806, 'mov', 'qword ptr [rsp + 0x48], r8'),
 (6442734376, 'mov', 'rdx, qword ptr [rsp + 0x48]'),
 (6442734490, 'mov', 'edx, dword ptr [rdx + 8]'),
 (6442734493, 'mov', 'dword ptr [rdi + 0xbc], edx'),
 (6442670818, 'movups', 'xmm1, xmmword ptr [rbp + 0x30]'),
 (6442670822, 'movups', 'xmmword ptr [rdi + 0x30], xmm1'),
 (6442670826, 'movups', 'xmm0, xmmword ptr [rbp + 0x40]'),
 (6442670830, 'movups', 'xmmword ptr [rdi + 0x40], xmm0'),
 (6442687181, 'cmp', 'qword ptr [rax + 0xc0], 0'),
 (6442687189, 'je', '0x180039afc'),
 (6442687191, 'cmp', 'byte ptr [rax + 0xb6], 0'),
 (6442687198, 'jne', '0x180039afc'),
 (6442687209, 'call', '0x180038170'),
 (6442687228, 'cmp', 'dword ptr [r13 + 0x5c], 0'),
 (6442687233, 'jle', '0x180039b23'),
 (6442687248, 'call', '0x180038340')]
regions.append((0x180036f20,0x180037430,'c4b2044a8fa4bcc00c9ede6610d678daa4f22e379032a38944c9181678559b85'))
pins.extend([(0x180036fa8,'cmp','dword ptr [rbx + 0x10], 0'),
             (0x180036fac,'je','0x1800373c8')])
ins={}
for a,z,h in regions:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 ins.update({i.address:(i.mnemonic,i.op_str)for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(raw,a)})
for a,m,o in pins:assert ins[a]==(m,o),(hex(a),ins.get(a))
metadata_path=out.with_name(out.stem+'-models.json')
subprocess.run([sys.executable,str(ROOT/'tools/handwriting/audit_math2_beam_width_producers.py'),str(metadata_path)],check=True)
metadata=json.loads(metadata_path.read_bytes());configs=[row['config']for row in metadata['models']]
assert len(configs)==3 and all('seg_cost_weight'not in config for config in configs)
assert configs[2]['seg_type']=='sigmoid'
# Ordered model metadata callbacks cannot overwrite the explicit reset here.
alpha_bits=0x3dcccccd;alpha=struct.unpack('<f',struct.pack('<I',alpha_bits))[0]
packet=bytearray(0x108);eed=bytearray(0x240);eee=bytearray(0xa0);request=bytearray(24)
struct.pack_into('<I',request,0,0xffffffff);struct.pack_into('<H',request,4,0x101)
request[6]=1;struct.pack_into('<I',request,8,0)
struct.pack_into('<I',eed,0xb4,alpha_bits);struct.pack_into('<I',eed,0xbc,struct.unpack_from('<I',request,8)[0])
packet[:]=eed[0x70:0x178];eee[0x10:0x60]=packet[:0x50]
assert struct.unpack_from('<I',eee,0x54)[0]==alpha_bits
assert struct.unpack_from('<I',eee,0x5c)[0]==0 and request[4]==1
report=dict(regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],instructionPins=len(pins),
 alphaBits=f'{alpha_bits:08X}',alpha=alpha,alphaProducer='0455C3 reset EED.B4 +no seg_cost_weight in actual ordered3model configs ->packet44 ->EEE54',
 optionalCount=0,optionalProducer='02780A EEB40=0 ->request+8 ->04539A/39D EED.BC ->packet4C ->EEE5C;039AFC/039B01 skips038340',
 correctionByte=request[4],metadata=configs,
 constraints='The exact library cold bundle constructs only3model owners; no fourth constraint model is installed. General configured/warm ownership remains excluded.',
 emptySource='Library raw-only input type excludes glyph/existing-expression objects; NOT a proof that arbitrary native fresh EEB selection is C25-free',
 emptyRecord='036FA8/036FAC skips primaryCount0 before reconstruction/EF4; Score and empty output retained, including nonfinite Score bits',
 scope='Successful unmodified fresh EEB request scalar proof +explicit library raw-only source entry; no runtime decoder or EF4 executed',vendorExecution=False)
out.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(regions)} regions/{len(pins)} pins; actual cold alpha0.1 +optionalcount0 +3model metadata; static only')
