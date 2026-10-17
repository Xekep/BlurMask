"""Read-only204290 language dispatch/source70/context-heap/E8 static evidence.

Vendor code is parsed as PE bytes; no vendor library is loaded or executed.
Behavioral tests distinguish real first-only passes from prepared later history.
"""
import hashlib
import json
import struct
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

if len(sys.argv) != 2:
    raise SystemExit('report.json')
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
start, end = 0x180204290, 0x180206720
block_hash = hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest()
assert block_hash == '2a63768874cfd7ba65467869488c603fe8681710aebd25d16825dcf5c078d5d6'
md = Cs(CS_ARCH_X86, CS_MODE_64)
instructions = {i.address: (i.mnemonic, i.op_str) for i in md.disasm(b.data[b.off(start):b.off(end)], start)}
pins = [
    (0x180204d7b, '4d8b4530'), (0x180204d7f, '488d8d50010000'),
    (0x180204d86, 'c644242800'), (0x180204d8b, '4c8bce'),
    (0x180204d91, '664489642420'), (0x180204d9a, '4c8b5070'),
    (0x180204d9e, '41ffd2'), (0x180204da5, '0f1038'),
    (0x18020509f, '4180bdf622000000'), (0x1802050ad, '8b01'),
    (0x1802050af, '83e01c'), (0x1802050b2, '83f808'),
    (0x1802050ce, '4a635c0038'), (0x1802050d3, '420fb744003c'),
    (0x1802050d9, '4c6bc058'), (0x1802050ff, '410fbe4020'),
    (0x180205107, '2bd8'), (0x18020510f, '448d7bff'),
    (0x18020512d, '4c8b90e8000000'),
    (0x1802051e1, '4183ffff'), (0x1802051e5, '0f8543040000'),
    (0x18020563d, 'bb0a000000'), (0x180205646, '448945f8'),
    (0x18020564a, '8b4c0104'), (0x18020564e, 'f3480f2ac1'),
    (0x180205653, 'f3410f59c2'), (0x180205658, 'f3480f2cc0'),
    (0x18020565d, '3bc3'), (0x18020565f, '0f47d8'),
    (0x180205826, '4533c9'), (0x180205844, 'e847deffff'),
    (0x18020586b, '8b4df8'), (0x180205880, '8bc1'),
    (0x180205882, 'ffc9'), (0x180205887, '85c0'),
    (0x180205889, '0f8491fcffff'), (0x180205896, '48c1e104'),
    (0x1802058a8, '440fb7590c'),
    #E8 returns primary group's descriptor line, not descriptor/physical index.
    (0x180152297, '4c8b5068'), (0x1801522a1, '83f8ff'),
    (0x1801522b0, '4c8b4878'), (0x1801522be, '440fb7c0'),
    (0x1801522c6, '4c8b4810'), (0x1801522cd, '8b4010'),
    (0x1801a7944, '410fb70401'), (0x180168ada, '486bc058'),
    (0x180168ade, '48034118'),
]
for address, value in pins:
    assert b.data[b.off(address):b.off(address) + len(value) // 2].hex() == value, hex(address)
assert instructions[0x1802051e5] == ('jne', '0x18020562e')
assert instructions[0x180205844] == ('call', '0x180203690')
assert b.data[b.off(0x1802ed0e4):b.off(0x1802ed0e4) + 4].hex() == 'cdcccc3d'
assert b.qwords(0x1802de610 + 0xe8, 1)[0] == 0x180152280
assert b.qwords(0x1802e1ff0 + 0x78, 1)[0] == 0x1801a7920
assert b.qwords(0x1802df340 + 0x10, 1)[0] == 0x180168ad0

managed = (ROOT / 'src/InkRecognition/LegacyOwnedMainDirectWordPass.cs').read_text()
dispatch = managed.split('private void RunLanguage(', 1)[1].split('private int NodeMinimum', 1)[0]
assert dispatch.index('LanguagePreviousNode(') < dispatch.index('if (previous == -1)') < dispatch.index('RunLaterLanguageQuery(')
later = managed.split('private int RunLaterLanguageQuery(', 1)[1].split('private bool WithinWordLength(', 1)[0]
assert later.index('CaptureActualTerminal(') < later.index('LegacyNativeLanguageContextRecombination.Collect(') < later.index('ProcessActualQueryWithPreparedTerminal(')
owner_pins = [
    'I32(current, 56)', 'U16(current, 60)', 'anchorOrdinal >= source.SourceEnd23f0',
    'unchecked(anchorNode - (sbyte)anchor[32])', 'source.CopyRows()',
    'router.GetPrimaryHeap(actualCurrent88[4]).Capacity',
    'new(false, 0, false, true, 0, configuration.PrimaryArcLimit230a, checked((ushort)previous.Count))',
    'primaryRecombinationOperations ??= createPrimaryRecombinationOperations()',
    'global.ResolveGroupSource(arcs[ordinal].GroupId, token).Primary.LineId',
    'source.Source.GetSourceLineIdForArc(node, ordinal)', 'line.Owner.SourceLineId',
    'LanguageContextRequestResidue, endingNode',
]
for value in owner_pins:
    assert value in managed, value
prepared = (ROOT / 'src/InkRecognition/LegacyNativeLaterDictionaryWordCompletion.PreparedTerminal.cs').read_text()
assert 'preparedTerminals.Contains(captured)' in prepared and 'preparedTerminals.Remove(captured)' in prepared
assert 'for (int index = contextResult.Candidates16.Length / 16 - 1; index >= 0; index--)' in prepared
assert 'caseSpan.ReadTerminal(tag, symbol, cancellationToken)' in prepared
assert 'validateSourceLease(cancellationToken)' in prepared
core = (ROOT / 'src/InkRecognition/LegacyNativeLaterDictionaryWordCompletion.cs').read_text()
assert 'preparedTerminal16 is not null' in core
output = Path(sys.argv[1])
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps(dict(function=dict(start=hex(start), endExclusive=hex(end), sha256=block_hash),
    textPins=len(pins), vtableTargets=3, ownerPins=len(owner_pins),
    order='actual source70(fulltag,symbol,boundary0) -> fresh context heap203690 -> reverse16 consumption, no second70',
    branch='anchor38/3C signed20 resolves first; previous==-1 first, otherwise later',
    capacity='actual target uint32 heap logical capacity converted via int64/single/MULSS/CVTTSS2SI, unsigned max10',
    scope='fresh nontracing C00/no-associated/ec0/F5false; real current fixtures exercise first-only; prepared history is not inferred later output',
    vendorExecution=False), indent=2) + '\n')
print(f'PASS {len(pins)} PE pins/3 vtable targets/{len(owner_pins)} owning-source pins; no vendor execution')
