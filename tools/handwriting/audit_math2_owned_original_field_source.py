"""Static native dependencies and narrow owning pre-NN Math field joins."""
from pathlib import Path
import hashlib, json, subprocess, sys

if len(sys.argv) != 2: raise SystemExit('report.json')
root = Path(__file__).resolve().parents[2]; target = Path(sys.argv[1])
native = target.with_suffix('.native.json')
subprocess.run([sys.executable, str(root/'tools/handwriting/audit_math2_original_field_source.py'), str(native)], check=True)
proof = json.loads(native.read_text()); assert proof['staticOnly'] and not proof['vendorExecution']
assert len(proof['regions']) == 31 and len(proof['bindings']) == 29
checks = {
 'src/InkRecognition.Math/Math2Mode0OriginalMathFieldSource.cs': [
  'private readonly InkOwner originalInk, selectedInk;', 'Dictionary<long, RegistryEntry>',
  'internal object C23Identity => c23Identity;', 'Math2Mode0RawSourcePositions.ValidateDocument(view.Document, token)',
  'if (payload > budget)', 'new(tag, "INPUT", 102, originalInk, originalC22)',
  'selectedInk.Registry.Add(entry.Id, new(entry.Id, entry.Name, entry.Category, selectedInk, selectedC22))',
  'selectedInk.Next = originalInk.Next;', 'c23 = []; c23Storage = null;',
  'selectedInk.Registry.TryGetValue(id, out resolved)', 'ReferenceEquals(resolved.C22.Owner, selectedInk)',
  'ReferenceEquals(c23Storage, incomingStorage) && c23.Length == resolved.C22.Ranges.Length',
  'Intersect(c23, resolved.C22.Ranges, token)', 'c23.SequenceEqual(domain)',
  'CurrentView.ExportIdentityTransforms(c23, token)', 'OriginalNameOwner { get; } = new()',
  'SelectedNameOwner { get; } = new()', 'private Binding? activeBinding;',
  'claimed = true;', 'ReferenceEquals(activeBinding, binding)', 'catch { failed = true; throw; }'],
 'src/InkRecognition.Math/Math2Mode0RawSourcePositions.cs': [
  'LegacyOwnedRawInkDocument actualDocument, CancellationToken token,',
  'ReferenceEquals(actualDocument, actualField.Document)',
  'MathPreprocessor.ProcessWithSourcePositions(actualDocument, token)',
  'prepared.Positions.PayloadReservation + actualField.AdditionalPayloadReservation'],
 'src/InkRecognition.Math/Math2Mode0RawOnlySource.cs': [
  'PrepareColdWithSelectedMathField(', 'private readonly Math2Mode0OriginalMathFieldSource.Binding? selectedField;',
  'field = actualField.ClaimForInference(token)', 'PrepareFeatures(field.Document, token, field)',
  'field?.Validate(field.Owner, token)', 'using var source = runtime.Tensor',
  'actualField!.SourcePrepared(field)', 'actualField!.Fail(field)', 'selectedField?.ValidatePrepared()'],
 'src/InkRecognition.Math/Math2Mode0SelectedMathFieldCopy.cs': [
  'private readonly Math2Mode0OriginalMathFieldSource.Binding binding;',
  'private Math2Mode0SelectedMathFieldCopy(', 'WithSelectedMathFieldForCopy(actualField, binding =>',
  'Math2Mode0FirstResultCopy.Create(actualOutput, token)', 'ReferenceEquals(actualCopy, Copy)',
  'binding.Validate(actualField, token)', 'Copy.ValidateForOuterCopy(token)',
  'Output.WithSourceAttachment(this, () =>', 'ValidateForCopy(Output, token)',
  'Output.Owner.Ef4.Source.ReadSelectedMathField(actualField)', 'if (result is IDisposable disposable) disposable.Dispose()']}
sources = {}; pins = 0
for path, expressions in checks.items():
    text = (root/path).read_text(encoding='utf-8-sig'); sources[path] = text
    for expression in expressions: assert expression in text, (path, expression); pins += 1
    assert 'LegacyLibraryOriginalTextFieldBootstrap' not in text
raw = sources['src/InkRecognition.Math/Math2Mode0RawOnlySource.cs']
assert raw.index('if (upper >') < raw.index('field = actualField.ClaimForInference') < raw.index('PrepareFeatures(field.Document') < raw.index('field?.Validate(field.Owner') < raw.index('using var source = runtime.Tensor')
context = sources['src/InkRecognition.Math/Math2Mode0OriginalMathFieldSource.cs']
assert context.index('if (payload > budget)') < context.index('var owner = new Math2Mode0OriginalMathFieldSource')
assert context.index('selectedInk.Registry.TryGetValue') < context.index('Intersect(c23, resolved.C22.Ranges') < context.index('c23.SequenceEqual(domain)')
#A context/binding owns source metadata only. No reverse reference can keep a
#disposed recognition run, decoder, Step or SourceBoundOutput alive.
assert 'Math2Mode0RawOnlySource' not in context and 'Ort' not in context and 'SourceBoundOutput' not in context
receipt = sources['src/InkRecognition.Math/Math2Mode0SelectedMathFieldCopy.cs']
assert 'readonly Math2Mode0SourceBoundOutput' not in receipt and 'readonly Math2Mode0RawOnlySource' not in receipt
report = dict(staticOnly=True, vendorExecution=False, nativeReport=str(native), nativeRegions=31, nativePins=194,
              nativeBindings=29, consumptionPins=pins, orderedJoins=4,
              runtimeSha256={p:hashlib.sha256((root/p).read_bytes()).hexdigest() for p in checks},
              scope='Actual pre-NN one-field Math INPUT102/full ordinary C23; exact field/selected/C23/Document binding, produced scoped detached-copy receipt. No old text owner, CBB, CB8, DOM publication or acceptance.')
target.write_text(json.dumps(report, indent=2)+'\n', encoding='utf8')
print(f'PASS31 native regions/194pins/29bindings +{pins} consumption pins/4ordered joins; vendor execution false')
