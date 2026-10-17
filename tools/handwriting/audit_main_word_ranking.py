"""Static13D360 numeric ranking, exact sort bodies and category quota offsets."""
import hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[
 (0x18013ccc0,0x18013ce00,'25c91c0eec985c2674428dc09afe86c1e3394f7b31cd2aaa9e9295c627184dd7'),
 (0x18013ce00,0x18013ce60,'7cefa207caf3e5f8ac9d683118bdc5e282192999897074df5f3acbddb13ea010'),
 (0x18013ce60,0x18013cf70,'441e0db4a8406e0f1e5b9d5898a46207ed8c3d86e7e736f8e6dfaecca6b1c391'),
 (0x18013d360,0x18013e7f0,'f6218b8b0df1f308e0b434c7cb48d9aa408e2563fbd0ba1e9e6d16f6b35ccc51')]
instructions={};md=Cs(CS_ARCH_X86,CS_MODE_64)
for a,z,h in regions:
    raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
    instructions.update({i.address:(i.mnemonic,i.op_str)for i in md.disasm(raw,a)})
pins={
 0x18013dc1f:('call','r10'), #actual276.V60, separate from279 alternative equality
 0x18013e1b3:('lea','rax, [rbx + 0x10]'),
 0x18013e1c0:('cmp','byte ptr [rax + 1], 0'), #removed17
 0x18013e1ca:('cmp','byte ptr [rax], 0'), #protected16
 0x18013e1d1:('cmp','byte ptr [rax - 1], 0'), #category15, not selected13
 0x18013e1d7:('inc','ecx'),
 0x18013e26b:('call','0x18013ccc0'),
 0x18013e347:('call','0x18013ccc0'),
 0x18013e46b:('call','0x18013ce00'),
 0x18013e662:('call','0x18013ce60')}
for a,expected in pins.items():assert instructions[a]==expected,(hex(a),instructions.get(a))
Path(sys.argv[1]).write_text(json.dumps(dict(regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],
 pins=len(pins),scope='Prepared finite unsentinelized WORD ranking only. Actual span mutation/comparison/source completion remain separate.',
 vendorExecution=False,topV68Completed=False),indent=2)+'\n')
print('PASS4 complete ranking/sort regions/',len(pins),'static pins; category15 quota and native unstable tie order')
