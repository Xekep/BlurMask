"""Static C79 producer/validator and same-owner046AE0 attachment audit.

Reads vendor PE data only. Does not load or execute vendor libraries.
"""
from pathlib import Path
import hashlib
import json
import subprocess
import sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
report = Path(sys.argv[1]).resolve()
dependencies = []
for name in ['source_position_transport', 'source_mapping_producer']:
    output = report.with_suffix('.' + name + '.json')
    subprocess.run([sys.executable, '-B', str(ROOT / ('tools/handwriting/audit_math2_' + name + '.py')),
                    str(output)], check=True)
    dependencies.append(str(output))

b = Binary('MyScriptInk.dll')
regions = [
    (0x18000f7e0, 0x18000fb30, '7f5a8dcb5788d3703542586f02936b05cea55583ca9f11e528ee3f19d29c09cb'),
    (0x180018346, 0x18001838d, '83dcfb4db0ff9d9ecde6785bf87db1c006319277b71dcb008b349ee83543b5f6'),
    (0x18000ee90, 0x18000ef70, 'ca92eeca8a0a2c1149600b09c341463d9a8fbace5402a82d6e92b49b0d202350'),
    (0x180016b10, 0x180016d80, 'b54fc7d8f5cd30eaba86043718a26372f02642140e6d36ebf0460a30acdf59e1'),
    (0x1800184af, 0x1800184f3, '9eb87b9f88ac2427bc99aa1f8559b577571da96804295c63eced1ac063517010'),
]
pins = [
    (0x18000ef56, 'mov', 'byte ptr [rsi + 0xb0], 1'),
    (0x180016b6c, 'lea', 'r9, [r14 + 0x10]'),
    (0x180016b70, 'mov', 'dword ptr [rsp + 0x20], 1'),
    (0x180016b78, 'lea', 'r8, [rip + 0xe0051]'),
    (0x180016b85, 'mov', 'r10, qword ptr [rax + 0x38]'),
    (0x1800184b7, 'cmp', 'dword ptr [rax + 0x10], 1'),
    (0x1800184bb, 'jge', '0x1800184c4'),
    (0x1800184bd, 'mov', 'byte ptr [rbx + 0xb0], 0'),
    (0x1800184c8, 'mov', 'edx, 0xc79'),
    (0x1800184ea, 'mov', 'r9, qword ptr [rax + 0x18]'),
    (0x18000f984, 'cmp', 'dl, r9b'),
    (0x18000f989, 'cmp', 'dl, 0xff'),
    (0x18000f98e, 'cmp', 'r9b, 0xff'),
    (0x18000f994, 'cmp', 'cl, bpl'),
    (0x18000f99d, 'cmp', 'al, 7'),
    (0x18000f9ff, 'cmp', 'rax, rcx'),
    (0x18000fa02, 'jb', '0x18000fa12'),
    (0x18000fa04, 'cmp', 'byte ptr [rdi + 0xb0], 0'),
    (0x18000fa0b, 'jne', '0x18000fa18'),
    (0x18000fa92, 'shr', 'r10, 0x20'),
    (0x18000faac, 'cmp', 'r10d, dword ptr [rsi + rcx*4]'),
    (0x18000facf, 'jle', '0x18000fad8'),
    (0x180018349, 'mov', 'eax, dword ptr [rsp + 0x40]'),
    (0x18001834d, 'test', 'eax, eax'),
    (0x18001834f, 'je', '0x18001838d'),
    (0x180018355, 'lea', 'r8d, [rax - 1]'),
    (0x18001835e, 'lea', 'ecx, [rbx - 1]'),
    (0x180018361, 'cmp', 'ebx, 0x8000'),
    (0x180018367, 'mov', 'eax, 0x7fff'),
    (0x18001836c, 'cmovne', 'ax, cx'),
    (0x180018372, 'shl', 'rcx, 0x10'),
    (0x18001837c, 'shl', 'rcx, 0x10'),
    (0x180018380, 'or', 'rcx, 0xff05'),
    (0x180018387, 'mov', 'qword ptr [rdx + r8*8], rcx'),
]
decoder = Cs(CS_ARCH_X86, CS_MODE_64)
instructions = {}
for start, end, expected in regions:
    raw = b.data[b.off(start):b.off(start) + end - start]
    assert hashlib.sha256(raw).hexdigest() == expected, hex(start)
    instructions.update({i.address: (i.mnemonic, i.op_str) for i in decoder.disasm(raw, start)})
for address, mnemonic, operands in pins:
    assert instructions[address] == (mnemonic, operands), (hex(address), instructions.get(address))
assert b.data[b.off(0x1800f6bd0):b.off(0x1800f6bd0) + 8] == b'version\0'

# Full validator is pinned above. Its fraction-byte comparisons are equality
# and FF-special overlap predicates; there is no fraction<200 validity test.
# Packed ascending/source membership are still required. This is NOT a port
# of general nonempty-cache/C25 validation.
checks = {
    'src/InkRecognition.Math/MathPreprocessor.cs': [
        'ProcessCore(strokes, null, default).Features',
        'LegacyMath2MeanXOrder.CopySourceOrder(means)',
        'positions.AppendCoordinate(sourceInput, point, fraction)',
        'positions?.CompleteStroke(i)',
        'positions?.Finish(features, token)',
    ],
    'src/InkRecognition.Math/Math2Mode0RawSourcePositions.cs': [
        'WeakReference<MathFeatures>', 'ReferenceEquals(prepared, actual)',
        'Math2Mode0SourcePositionCoordinates.Compose(left, right, fraction)',
        'result[^1] = prior[priorEnds[input] - 1]',
        'LegacyNativeInkPositionSnapshot.CreateProcessedForMath(document, fields, ends, order, token)',
        'rawPoints * 8 + fields.Length * 8L + order.Length * 128L + 32',
    ],
    'src/InkRecognition/LegacyNativeInkPositionSnapshot.cs': [
        'internal static LegacyNativeInkPositionSnapshot CreateProcessedForMath(',
        'position.SourceIndex != (uint)source',
        '(position.Fraction > 200 && position.Fraction != 255)',
        '(point != begin && position.Value <= previous)',
    ],
    'src/InkRecognition.Math/Math2Mode0RawOnlySource.cs': [
        'PrepareColdWithSourcePositions(', 'PrepareWithPreparedSearchAndSourcePositions(',
        'positions.ValidateFeatures(features)', 'using var source = runtime.Tensor(features.Values, features.Frames, 1, 26)',
        'using var ends = runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length)',
        'private readonly Math2Mode0RawSourcePositions? sourcePositions',
    ],
    'src/InkRecognition.Math/Math2Mode0SourceBoundOutput.cs': [
        'Output = output; this.positions = positions',
        'records[at] = row.RecordIdentity',
        'positions.ReadRange(previous, run.EndSegmentExclusive, token)',
        'ranges.Union(attached[at][run.PrimaryIndex], interval, token)',
        'previous = run.EndSegmentExclusive',
        'ReferenceEquals(actual, Output)', 'ReferenceEquals(sourceAttachment, candidate)',
        'row.ToArray()', 'Output.Owner.Ef4.Source.Loop.FailRawAttentionHistory()',
    ],
}
hashes = {}
for name, expressions in checks.items():
    raw = (ROOT / name).read_bytes()
    text = raw.decode('utf-8-sig')
    for expression in expressions:
        assert expression in text, (name, expression)
    hashes[name] = hashlib.sha256(raw).hexdigest()
text = (ROOT / 'src/InkRecognition.Math/Math2Mode0SourceBoundOutput.cs').read_text(encoding='utf-8-sig')
assert text.index('if (payload > budget)') < text.index('sourceAttachmentClaimed = true') < text.index('sourceAttachment = new(')
assert text.index('if (run.PrimaryIndex >= 0)') < text.index('ranges.Union(') < text.index('previous = run.EndSegmentExclusive')
assert 'RunStep(' not in text and 'MathPreprocessor.Process(' not in text
result = dict(scope='library-owned raw-only same-feature C79 and actual046AE0 attachment; no SDK page/DOM/acceptance',
              regions=[dict(start=hex(a), end=hex(z), sha256=h) for a, z, h in regions],
              pins=len(pins), dependencies=dependencies, runtimeHashes=hashes,
              fraction200='Stored without carry; validator has no numeric fraction cap; packed ordering/source checks remain',
              sourceLifetime='Weak feature reference authenticates only initial binding; private immutable position/document owner remains')
report.parent.mkdir(parents=True, exist_ok=True)
report.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
print(f'PASS{len(regions)} additional PE regions/{len(pins)} pins/{len(checks)} runtime ownership chains; dependency audits; no vendor execution')
