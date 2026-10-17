"""Compose existing static PE contracts for the configured source/TOP bridge.
No vendor code is loaded. The only accepted main property changes short18;
negative source export, C21 insertion and TOP.V50 emission keep their existing
native contracts and distinct ordinary/protocol boundaries.
"""
import hashlib
import json
import subprocess
import sys
from pathlib import Path
root = Path(__file__).resolve().parents[2]
out = Path(sys.argv[1])
out.parent.mkdir(parents=True, exist_ok=True)
contracts = [
    'audit_main_configured_alternative_assembly.py',
    'audit_main_span_glyph_refresh.py',
    'audit_main_text_source_positions.py',
    'audit_main_glyph_utf8_emission.py',
]
reports = []
for name in contracts:
    target = out.with_name(out.stem+'-'+name.removeprefix('audit_').removesuffix('.py')+'.json')
    subprocess.run([sys.executable, str(root/'tools/handwriting'/name), str(target)], check=True, cwd=root)
    reports.append(dict(contract=name, scriptSha256=hashlib.sha256((root/'tools/handwriting'/name).read_bytes()).hexdigest(),
                        report=json.loads(target.read_bytes())))
checks = {
    'src/InkRecognition/LegacyOwnedMainSpanGlyphSource.cs': [
        'PrepareConfigured(LegacyOwnedMainCandidateSource candidate',
        'CandidateSource?.Validate(token)',
        'SourceFormat22f9!=0',
        'controller.Configuration.Mode22f6!=0',
        'ownership.SelectedOwner.ConfidenceOwner.Graph'],
    'src/InkRecognition/LegacyOwnedMainSpanGlyphRefresh.cs': [
        'ConditionalWeakTable<LegacyOwnedMainConfiguredAlternative,',
        'ReferenceEquals(existing.CandidateSource!.SourceOverlay,candidate.SourceOverlay)',
        'ReferenceEquals(existing.CandidateSource.Ownership,candidate.Ownership)',
        'CandidateSource?.Validate(token)',
        'pool.CopyOwnerNode(canonicalSpanKey,token)'],
    'src/InkRecognition/LegacyOwnedRawInkDocument.cs': [
        'ProjectSpanCandidate(LegacyOwnedMainCandidateSource actualCandidate',
        'ReferenceEquals(sourceCandidate.CandidateIdentity, actualCandidate.CandidateIdentity)',
        'ReferenceEquals(sourceCandidate.SourceOverlay, actualCandidate.SourceOverlay)',
        'Project(map, references, false, [originalRange], token)'],
    'src/InkRecognition/LegacyFreshMainGlyphUtf8Emission.cs': [
        'PrepareConfigured(LegacyOwnedMainCandidateSource candidate',
        'CandidateSource?.Validate(token)',
        'ControlSymbolCount => 0', 'AlternateOffsetCount => 0',
        'TopOwner.GlyphPool.CopyRecord(glyph.Reference, token)'],
}
for name, terms in checks.items():
    text = (root/name).read_text()
    for term in terms:
        assert term in text, (name, term)
report = dict(dependencies=reports, managedOwnershipPins=sum(map(len,checks.values())), noVendorExecution=True,
    scope='actual configured2 SAME selected overlay/candidate and TOP; one fresh ordinary source descriptor',
    notImplemented=['configured higher-source namespace sharing', 'imported/reused text and alternate vectors',
                    'full 024F30 string wrapper or accepted text'])
out.write_text(json.dumps(report,indent=2)+'\n')
print(f'PASS four independent static PE contracts/{report["managedOwnershipPins"]} ownership pins; no vendor execution.')
