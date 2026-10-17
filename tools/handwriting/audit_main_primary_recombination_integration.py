"""Read-only native2144F0 ->20E6A0 ->ordering and fresh-input binding pins.

The behavioral regression probes compare full actual DIRECT/CASE/global rows.
This audit never loads vendor DLLs or supplies a missing secondary anchor.
"""
import hashlib
import json
import struct
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

if len(sys.argv) != 2:
    raise SystemExit('report.json')
output = Path(sys.argv[1])
output.parent.mkdir(parents=True, exist_ok=True)
dependencies = []
for name in ['audit_main_primary_word_recombination.py', 'audit_fresh_case_main_source.py',
             'audit_fresh_case_dictionary.py']:
    report = output.with_name(output.stem + '-' + name.removeprefix('audit_').removesuffix('.py') + '.json')
    subprocess.run([sys.executable, str(ROOT / 'tools/handwriting' / name), str(report)], check=True)
    dependencies.append(str(report))
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md = Cs(CS_ARCH_X86, CS_MODE_64)
instructions = {i.address: (i.mnemonic, i.op_str) for i in md.disasm(
    b.data[b.off(0x1802144f0):b.off(0x180215ef0)], 0x1802144f0)}
pins = [
    (0x180215250, '498b4110'),
    (0x18021525a, '48c1e107'),
    (0x180215261, '66ffc3'),
    (0x180215264, '89540158'),       #actual initial ordinal58 before dispatcher
    (0x180215279, 'e82294ffff'),     #20E6A0 before ordering, not after materialization
    (0x18021527e, '84c0'),
    (0x180215280, '0f8499f4ffff'),
    (0x1802152c4, '4183fd01'),       #sort only count>1; dispatcher still ran
    (0x18020c358, '668983e4240000'),
    (0x18020c369, '668983e6240000'),
    (0x18020c374, '668983e8240000'),
    (0x18020c380, '668983ea240000'),
    (0x18020baba, '33f6'),          #NEW20BAA0 zero register
    (0x18020bb6f, '89b308230000'),  #permission2309 starts zero
    (0x18020bdfe, 'c7830026000000000002'), #cap2601/2602 zero;2603=2 distinct
    (0x18020da91, '4438a8c4000000'), #fresh C4zero skips28F/count-cap activation
    (0x18020da98, '7454'),
    (0x18020dae7, 'c6870126000001'),
    (0x18020dc00, '4438af01260000'),
    (0x18020dc07, '7422'),
    (0x18020dc2b, '4532c0'),
    (0x18020dc2e, '44888702260000'), #then count2602zero, not percent guessing
    (0x18020c800, '4c3b8200230000'), #same no-associated0 skips permission rewrite
    (0x18020c807, '0f848d000000'),
]
for address, value in pins:
    assert b.data[b.off(address):b.off(address) + len(value) // 2].hex() == value, hex(address)
assert instructions[0x180215279] == ('call', '0x18020e6a0')
#The AL stored header's eighth word belongs at runtime1A. Its first seven
#words land at08/0A/0C/0E/10/12/14;24E4/6/8 therefore use serialized8/A/C.
engine = Binary('MyScriptEngine.dll')
assert hashlib.sha256(engine.data).hexdigest() == 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
engine_pins = [(0x180053f57, '4c8d4308'), (0x180053f71, '4c8d430a'),
    (0x180053f9d, '4c8d430c'), (0x180053fc9, '4c8d430e'),
    (0x180053ff5, '4c8d4310'), (0x180054021, '4c8d4312'),
    (0x18005404d, '4c8d4314'), (0x180054079, '488d731a')]
for address, value in engine_pins:
    assert engine.data[engine.off(address):engine.off(address) + len(value) // 2].hex() == value
#Selected main247 has no283 extension hooks that could install1ECCB0/28F.
ak = (ROOT / 'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest() == '7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
extension_count = 189734 + 8 + struct.unpack_from('<I', ak, 189738)[0]
assert struct.unpack_from('<I', ak, extension_count)[0] == 0
assert 8 + struct.unpack_from('<I', ak, 4)[0] - extension_count == 190
managed = (ROOT / 'src/InkRecognition/LegacyOwnedMainDirectWordPass.cs').read_text()
body = managed.split('private LegacyNativeWordAlternativeOrderingResult OrderPrimary(', 1)[1].split('private bool WithinWordLength', 1)[0]
assert body.index('AssignInitial(') < body.index('LegacyNativePrimaryWordRecombination.Apply(') < body.index('LegacyNativeWordAlternativeOrdering.Prepare(')
assert 'LegacyNativeRootWordAlternativeProducer.Create(' not in body
assert 'LegacyNativeInitialParentWordAlternativeProducer.Create(' not in body
managed_pins = [
    'source.CopyRows()', 'produced.HeapStorage128.Span', 'owner.Categories.Categories',
    'primaryPool = produced.PrimaryStorage40.ToArray(); secondaryPool = produced.SecondaryStorage40.ToArray();',
    'createPrimaryRecombinationOperations = owner.GlobalTransitions!.CreateOperations;',
    'createPrimaryRecombinationOperations = geometry.CreateOperations;',
    'needsGeneralRequest ? primaryRecombinationOperations ??= createPrimaryRecombinationOperations() : null',
    'PrimaryRecombinationRequestResidue, node',
    'U16(alphabetHeader, 8), U16(alphabetHeader, 10)', 'U16(alphabetHeader, 12), Label("\\ufffd")',
    'Label("\\u0003")', 'preparation.Sources.SubwordSourceIndex22ec',
]
for value in managed_pins:
    assert value in managed, value
report = dict(textPins=len(pins), engineHeaderPins=len(engine_pins), managedOwnerPins=len(managed_pins),
    dependencies=dependencies, nativeOrder='assign true58 ->20E6A0 ->ordering ->quota ->source row materialization',
    ownership='same actual source graph/categories/contexts/descriptors and current full23F0 stored88/pools; same global mutable transition owner',
    freshPolicy='sealed new knowledge without properties11/12,C4 or extension hooks; no-associated2300 remains0; identity2360, cap2601/2602=0, permission2309=0',
    enabledProperty='only existing named preconstruction secondary-count2; no other live policy mutation enabled',
    sourceExtensionCount=dict(offset=extension_count, count=0),
    residue='allocated only for a genuine general-higher row; unused80 shape zeros do not stand in for observed native stack bytes',
    scope='current actual regression inputs have only root/primary predecessor branches; higher branches remain prepared-kernel evidence',
    noVendorExecution=True)
output.write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
print(f'PASS native order/{len(pins)} Text+{len(engine_pins)} Engine pins/{len(managed_pins)} owner pins; static reads only.')
