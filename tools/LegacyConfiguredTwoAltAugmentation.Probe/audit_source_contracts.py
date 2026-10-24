"""Exact reviewed source/default-null contracts; no target/model/native execution."""
from pathlib import Path
import argparse,hashlib,importlib.util,json
p=argparse.ArgumentParser(description=__doc__);p.add_argument('repo',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
root=a.repo.resolve();tool=Path(__file__).resolve().parent;out=a.output.resolve();assert not out.exists()
provenance=json.loads((tool/'provenance.json').read_text());contracts=json.loads((tool/'source_contracts.json').read_text())
sp=importlib.util.spec_from_file_location('configured_source_preimage',tool/'source_preimage.py');m=importlib.util.module_from_spec(sp);sp.loader.exec_module(m)
h=lambda b:hashlib.sha256(b).hexdigest()
rows=[];negative=[]
for rel,expected in provenance['sourceLF_SHA256'].items():
    data=(root/rel).read_bytes().replace(b'\r\n',b'\n');assert h(data)==expected,rel
    name=Path(rel).name
    if name in contracts['allowedExpected']:
        # Current3a default restoration is the first accepted target; the
        # distinct pre-pointlike historical target is never a default theorem.
        target=contracts['allowedExpected'][name][0]
        restored=m.restore(name,data,target);assert h(restored)==target
        assert m.restore(name,data.replace(b'\n',b'\r\n'),target)==restored
        for label,source,want in [('source-byte',data+b' ',target),('unknown-expected',data,'0'*64)]:
            try:m.restore(name,source,want)
            except AssertionError:negative.append(name+':'+label)
            else:raise AssertionError(('accepted source change',name,label))
        rows.append(dict(path=rel,currentLF_SHA256=expected,restoredCurrent3aSHA256=target))
assert len(rows)==13 and len(negative)==26
name='LegacyDirectionalGlyphClassifier.cs';historical='374e84422ad49bf0b2f970960cc940032d2c246ec9bd03b96d9d397603fda57e'
current='c3da6b4f675c335911da5c6c08c3f8ce80c7bef9c59a9432966eb86511bea0cd'
data=(root/'src/InkRecognition'/name).read_bytes();assert h(m.restore(name,data,current))==current
assert h(m.restore(name,data,historical))==historical
try:m.restore('Unknown.cs',data,current)
except AssertionError:negative.append('unknown-filename')
else:raise AssertionError('unregistered filename')
aug=(root/'src/InkRecognition/LegacyFreshMainConfiguredWordAugmentation.cs').read_text(encoding='utf-8-sig')
assert '=>PrepareCore(resource,actual,table,normalization,circles,callerResidue,token,null);' in aug
assert 'if(raw is null)return LegacyNativeMainWordEquality.Compare(' in aug
assert aug.count('LegacyNativeMainWordEquality.CompareWithReadObserver(')==1
assert aug.count('LegacyPreparedMainWordRanking.Rank(')==1 and aug.count('LegacyPreparedMainSpanScorePublication.Normalize(')==1
no=(root/'src/InkRecognition/LegacyFreshMainConfiguredNoCandidateSourceCompletion.cs').read_text(encoding='utf-8-sig')
assert 'Enabled219580 candidate augmentation cannot be omitted.' in no
assert no.index('LegacyConfiguredSourceCompletionClaim.Acquire(')<no.index('CopyActiveAlternatives()')
raw=(root/'src/InkRecognition/LegacyFreshMainConfiguredWordAugmentation.Diagnostic.cs').read_text(encoding='utf-8-sig')
assert 'private readonly RawCollector? raw;' in raw and 'phase!=2' in raw
assert 'actual.debit.WriteRawTerminalDiagnostic(actual,writer,token);' in raw
assert 'fixedVisits=192+1024+512+4096;' in raw
assert 'SetValue(' not in raw and 'GetField(' not in raw
# Explicit test-only positive-route successor; public GLOBAL/SourceBatch is untouched.
oldprobe=(root/'tools/LegacyMainDocumentInputTransforms.Probe/Program.cs').read_bytes().replace(b'\r\n',b'\n')
assert h(oldprobe)=="c9f74d03b31aac3f05b7e7f004e876287aac4e42fc36a152d14a581bbf7a57b8"
probe=oldprobe.decode('utf-8-sig')
assert 'LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(' in probe
assert 'Enabled219580 candidate augmentation cannot be omitted.' in probe
assert 'LegacyFreshMainConfiguredWordAugmentation.Prepare(ak,configured,table,norm,circles,residue);completion.Run();' in probe
assert 'PrepareRawDiagnostic(' not in probe
assert probe.index('if(!bit8Refused)')<probe.index('LegacyFreshMainConfiguredWordAugmentation.Prepare(')
out.write_text(json.dumps(dict(scope='All19 exact reviewed sources and13 full source restorations; source proof, not a new semantic/runtime/NN test',sourceFiles=19,defaultSourceRestorations=rows,sourceNegatives=negative,historicalPointlikeSource=dict(current=current,historical=historical,currentNumericOrBudgetEquivalenceClaimed=False),publicBroadProbeUnchanged=False,explicitAugmentedPositiveEntry=True,publicGlobalRouteUnchanged=True,rawOptInOnly=True,targetExecutions=0),indent=2)+'\n',encoding='utf-8')
print('PASS19source/13strictrestorations/27source negatives; historical374 distinct;0target')
