"""Static contracts and managed ownership/order pins for configured choices.
Vendor binaries are read as data only. This is not a full024F30 wrapper proof.
"""
import hashlib
import json
import subprocess
import sys
from pathlib import Path
root = Path(__file__).resolve().parents[2]
out = Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True)
contracts = [
    ('audit_main_configured_span_emission.py', False),
    ('audit_legacy_text_glyph_caller.py', True),
]
reports=[]
for name, named in contracts:
    target=out.with_name(out.stem+'-'+name.removeprefix('audit_').removesuffix('.py')+'.json')
    command=[sys.executable,str(root/'tools/handwriting'/name)]
    if named:command+=['--output']
    subprocess.run(command+[str(target)],check=True,cwd=root)
    reports.append(dict(contract=name,scriptSha256=hashlib.sha256((root/'tools/handwriting'/name).read_bytes()).hexdigest(),report=json.loads(target.read_bytes())))
#The committed classifier tables are checked directly against the pinned PE;
#this bounded audit needs no private Script9 download or inferred Unicode data.
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
import struct
engine=Binary('MyScriptEngine.dll');text=Binary()
assert hashlib.sha256(engine.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
assert hashlib.sha256(text.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
data=json.loads((root/'models/text-unicode/legacy-text-publication-data.json').read_bytes())
for i,row in enumerate(data['scriptRanges']):
    assert list(struct.unpack_from('<iii',engine.data,engine.off(0x1801fb6d0)+i*12))==row
for i,row in enumerate(data['literalRows']):
    at=text.off(0x18030e160)+i*32
    pointer,*fields=struct.unpack_from('<Q6i',text.data,at)
    off=text.off(pointer);raw=text.data[off:text.data.index(b'\0',off)]
    assert row==[raw.hex(),*fields[:5]]
assert len(data['scriptRanges'])==716 and len(data['literalRows'])==120
s=(root/'src/InkRecognition/LegacyFreshMainTextGlyphChoices.cs').read_text()
terms=[
    'PrepareConfigured(ReadOnlySpan<byte> resource,',
    'LegacyOwnedMainOrdinaryGlyphRefresh.PrepareConfigured(resource, candidate, table, token)',
    'LegacyOwnedMainSpanGlyphRefresh.PrepareConfigured(candidate, token)',
    'LegacyFreshMainGlyphUtf8Emission.PrepareConfigured(candidate, normalization, circles, token)',
    'Configured choices have no count1 first candidate.',
    'CandidateSource?.Validate(token)',
    'document.ProjectCandidate(selected,refreshed.Source,token)',
    'document.ProjectSpanCandidate(selected,refreshed.Source,token)',
    'LegacyNativeTextGlyphCaller.Produce(input.GlyphOwner,Emitter.PrimaryAlphabet,',
    'NormalizeOwnerKey(input.Reference),false,',
]
for term in terms:assert term in s,term
refresh=s[s.index('private LegacyPreparedMainTextWordSources RefreshWordSourcesCore'):s.index('internal LegacyPreparedMainTextGlyphSelection PrepareGlyph')]
assert 'foreach(ulong reference in word.Word.FlatReferences)'in refresh
assert 'PrepareGlyph('not in refresh and 'Produce('not in refresh
publish=s[s.index('public IReadOnlyList<LegacyOwnedMainTextWordChoices> Publish'):s.index('public void Dispose()')]
assert publish.index('RefreshWordSources(')<publish.index('PrepareGlyph(')<publish.index('PublishFreshGlyph(')
begin=s[s.index('internal void BeginPublication'):s.index('private void CheckWord')]
assert begin.index('token.ThrowIfCancellationRequested()')<begin.index('Interlocked.CompareExchange')
assert begin.index('allocatedRecordStorage40.Length')<begin.index('Interlocked.CompareExchange')
report=dict(contracts=reports,ownershipPins=len(terms),orderedStages=['refresh every actual WORD reference','select glyph alternatives','TOP.V50 emission/0213A0','021000 classify/intern'],noVendorExecution=True,rawScriptRows=716,rawLiteralRows=120,
    scope='genuine configured candidate/private local string owner; not full024F30 interning/reuse or accepted text')
out.write_text(json.dumps(report,indent=2)+'\n')
print(f'PASS two static contracts/716 script rows/120 literal rows/{len(terms)} managed ownership pins/native staged kernel ordering; no vendor execution.')
