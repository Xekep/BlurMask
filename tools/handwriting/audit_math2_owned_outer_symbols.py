"""Read-only native outer-copy dependencies plus portable owner/claim wiring."""
from pathlib import Path
import hashlib
import json
import subprocess
import sys

if len(sys.argv) != 2:
    raise SystemExit('report.json')
root = Path(__file__).resolve().parents[2]
report = Path(sys.argv[1]).resolve()
native = report.with_suffix('.native.json')
subprocess.run([sys.executable, '-B', str(root / 'tools/handwriting/audit_math2_outer_symbol_owner.py'), str(native)], check=True)
checks = {
    'src/InkRecognition.Math/Math2Mode0FirstResultCopy.OuterView.cs': [
        'lock (gate)', 'ValidateForOuterCopy(token)', 'rows.Length != sourceRanges.Length',
        'return action(rows, sourceRanges, reservation)', 'ObjectDisposedException.ThrowIf(released, this)',
        '!complete || failed || library is null || library.IsDisposed',
    ],
    'src/InkRecognition.Math/Math2Mode0OuterSymbolVector.cs': [
        'CreateSingle(Math2Mode0FirstResultCopy actualFirstCopy', 'source.WithSymbolRowsForOuterCopy(',
        'new Math2Mode0OuterSymbolVector(source.Document, planned)',
        'sourceReservation + 96 + rows.LongLength * 72 + rangeCount * 16 + strings + peak',
        'target.library = new(1); target.library.InitializeEmptyPool()',
        'target.cells[i] = new(); target.initialized++', 'target.count = rows.Length',
        'cell.Flags = row.Flags; cell.Metadata = row.Metadata',
        'target.library.InternOutput(row.Text.ReadBytes(), token)',
        'cell.Alternatives[0] = new(text, row.ByteCount, row.ScalarBits)',
        'cell.AlternativeCount = 1; target.alternativesWritten++',
        'cell.Ranges = sourceRanges[i].ToArray()', 'target.rangesWritten += sourceRanges[i].Length',
        'target.failed = true', 'if (observer is null) target.Dispose()',
        'source.ValidateForOuterCopy(token)', 'ObjectDisposedException.ThrowIf(target.released, target)',
        'ReferenceEquals(actualDocument, Document)', 'cell.Ranges.ToArray()',
        'released = true; cells = []; initialized = count = 0; library?.Release()',
    ],
}
hashes = {}
for name, expressions in checks.items():
    data = (root / name).read_bytes(); text = data.decode('utf-8-sig')
    for expression in expressions:
        assert expression in text, (name, expression)
    hashes[name] = hashlib.sha256(data).hexdigest()
text = (root / 'src/InkRecognition.Math/Math2Mode0OuterSymbolVector.cs').read_text(encoding='utf-8-sig')
assert text.index('if (planned > budget)') < text.index('var target = new Math2Mode0OuterSymbolVector(') < text.index('target.library = new(1)')
assert text.index('target.cells[i] = new()') < text.index('target.count = rows.Length') < text.index('cell.Flags = row.Flags')
assert text.index('target.library.InternOutput(') < text.index('cell.AlternativeCount = 1') < text.index('cell.Ranges = sourceRanges[i].ToArray()') < text.index('target.copiedRows++')
assert 'RunStep(' not in text and 'CreateMulti' not in text and 'MATH_SYMBOL' not in text
assert 'private readonly Math2Mode0FirstResultCopy' not in text
first = (root / 'src/InkRecognition.Math/Math2Mode0FirstResultCopy.cs').read_text(encoding='utf-8-sig')
assert 'internal sealed partial class Math2Mode0FirstResultCopy' in first
assert first.index('target.secondary = source.Source.CopySecondary()') < first.index('target.frontier = source.Source.Frontier.Value')
report.parent.mkdir(parents=True, exist_ok=True)
report.write_text(json.dumps(dict(scope='ONE actual first-copy owner -> independent outer3E/ranges; no multi-C23/CB8/DOM',
    nativeReport=str(native), runtimeHashes=hashes, runtimeChains=len(checks),
    budget='Conservative source-chain plus NEW pool/rows/ranges/strings/peak, payload-only; metadata/CLR excluded',
    lifetime='Complete source gate during copy; no source retained by successful target; same Document immutable domain'), indent=2) + '\n', encoding='utf-8')
print(f'PASS native8regions69pins dependencies/{len(checks)} runtime owner chains; count-before-fill/new-pool/ranges/cancel ordering')
