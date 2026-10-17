"""Static2271F0 final207CB0-before-2257C0 ordering, not vendor execution."""
import hashlib
import json
import subprocess
import sys
import tempfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
if len(sys.argv)!=2: raise SystemExit('output.json')
output=Path(sys.argv[1]).resolve()
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
b=Binary()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pins=[
(0x180227271,'4c63aa10230000'), #R13=actual2310 source count, signed.
(0x18022727c,'44896d90'),
(0x1802272e8,'4539ae90240000'), #compare actual2490 with source count.
(0x1802272ef,'7d17'),           #signed>= skips when already complete.
(0x1802272f1,'458d45ff'),       #ending=actualCount-1, not winner row22.
(0x1802272fb,'e8b009feff'),     #207CB0.
(0x180227300,'84c0'),
(0x180227302,'0f84ec080000'),   #failure aborts before selection.
(0x180227603,'e8b8e1ffff'),     #later2257C0.
]
for address,expected in pins:
    assert b.data[b.off(address):b.off(address)+len(expected)//2].hex()==expected,hex(address)
md=Cs(CS_ARCH_X86,CS_MODE_64)
calls=[(i.address,i.op_str) for i in md.disasm(b.data[b.off(0x1802271f0):b.off(0x180227700)],0x1802271f0)
       if i.mnemonic=='call' and i.op_str in ('0x180207cb0','0x1802257c0')]
assert calls==[(0x1802272fb,'0x180207cb0'),(0x180227603,'0x1802257c0')]
with tempfile.TemporaryDirectory(prefix='global-final-static-',dir=output.parent) as temporary:
    report=Path(temporary)/'selector.json'
    subprocess.run([sys.executable,str(ROOT/'tools/handwriting/audit_main_terminal_selection.py'),str(report)],check=True)
    selector=json.loads(report.read_bytes())
output.write_text(json.dumps(dict(pins=len(pins),orderedCalls=[hex(a) for a,_ in calls],selector=selector,
    scope='Typed completed global LIVE pool -> final207CB0 -> actual published88 numeric selection/noWinner. No273/WORD/text or allocationfailure-order claim.'),indent=2)+'\n',encoding='utf-8')
print(f'PASS{len(pins)} finalpool/source2310/failure-order pins and 207CB0-before-2257C0 call order; selector proof retained.')
