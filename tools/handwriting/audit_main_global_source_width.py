"""Static-only closure of global TOP.AC/B0 and explicit scratch/WORD residue."""
import hashlib, json, subprocess, sys
from pathlib import Path

root = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root / 'research/myscript/scripts'))
from inspect_text_features import Binary, functions

if len(sys.argv) != 2:
    raise SystemExit('report.json')
out = Path(sys.argv[1])
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pins = [(0x180227776, '4d39a600230000410f44c8'),
        (0x1802277b9, '4c896424484c89642440'),
        (0x1802219d0, 'f30f1186b0000000'),
        (0x1802219da, 'c786b000000000000000')]
for address, raw in pins:
    assert b.data[b.off(address):b.off(address) + len(raw)//2].hex() == raw
f = functions()
caller = f['1802173e0']
#TOP.AC/B0 are transported unchanged, while the label cursor is explicitly256.
#The stack label buffer itself is not initialized by this declaration/call.
assert 'local_260 = 0x100;' in caller
assert 'local_2b0 = *(undefined4 *)(param_3 + 0xac);' in caller.lower()
assert 'local_2a8 = *(undefined4 *)(param_3 + 0xb0);' in caller.lower()
assert 'local_2c0 = local_248;' in caller.lower()
deps = []
for script in ('audit_main_source_width.py', 'audit_main_word_publication.py', 'audit_main_owned_flush.py'):
    path = out.with_name(out.stem + '-' + script.removeprefix('audit_').removesuffix('.py') + '.json')
    subprocess.run([sys.executable, str(root / 'tools/handwriting' / script), str(path)], check=True, cwd=root)
    deps.append(dict(script=script, report=path.name, sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
out.write_text(json.dumps(dict(pins=len(pins), dependencies=deps,
    scope='one fresh no-associated source135, nontrace ordinary mode; actual flush plan; current global controller remains partial',
    topAC='2271F0 no-associated ->2',
    topB0='221840 initializes sum/count,2188F0 reverse flush widths use float subtract then double accumulate; positive mean to float, else explicitzero',
    residue='2173E0 cursor256 explicitly initialized, labels256 uninitialized; WORD120 bytes44..53 remain allocator residue, not TOP scalars',
    limits=['complete controller/213C00 soft-hyphen join', 'WORD120 allocator/pool and full218560 publication', 'pending source70 events not reached by actual8 inputs'],
    noVendorExecution=True), indent=2) + '\n', encoding='utf-8')
print('PASS global TOP.AC/B0 4 pins + width/publication/WORD-seed dependency audits; static only')
