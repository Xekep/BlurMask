import argparse,hashlib,json,pathlib
def restore_ordinary_chronology_source(name,data,expected):
 # This auditor evaluates the historical D0 reservation, not the corrected
 # ordinary-factor runtime. Admit only the two complete reviewed successors.
 rows={
  'LegacySourcePointChronology.cs':('2e7bf68330e2c2ad9f90ff33f8172f09ad1675b5e04f031686e05c7ac5e22143','31cf1a0e24f38a0bb0836d9adc91e8bd5259e37345d134b11ea950e09ffa8e2e',
   b'        // Fresh native219 secondary16A130 initializes source+128 to200.\n        // Retain fractional265 map ticks through the later metadata lookup.\n        return new(counts,keys,Enumerable.Range(0,total).ToArray(),[],200);',
   b'        return new(counts,keys,Enumerable.Range(0,total).ToArray(),[],1);'),
  'LegacyOrdinarySourceInk.cs':('34bdda6179be10f2f6de2213b0cf6c230e6e48c81ab818859a4dcaacddcb8708','36abe3c77a43a16072fb005bb2fd1eae7331978a08d48e74efacfc8c0d9456e8',
   b'Enumerable.Range(0,keys.Count).ToArray(),sourceFactor:200);',b'Enumerable.Range(0,keys.Count).ToArray());')}
 current,historical,after,before=rows[name]
 assert expected==historical,('Unreviewed historical target',name,expected)
 # Exact private-meter inverses precede the independent factor-unit inverse.
 # The corrected4ace body keeps explicit200 until it reaches unmetered34bdda;
 # the prior d443 checkpoint maps separately to the old factor1 body36abe3.
 # Neither source body is authority for the historical budget or live runtime.
 if name=='LegacyOrdinarySourceInk.cs':
  meter_sources={
   '4acecbd2dfb380ea9995cccc57776ea346a710736fffad4016fb525006d79d3d':'34bdda6179be10f2f6de2213b0cf6c230e6e48c81ab818859a4dcaacddcb8708',
   'd4439824067a79d1563c1feb9d6000f582ddc1c39cb447363c7fc0d3e0a9bcaf':'36abe3c77a43a16072fb005bb2fd1eae7331978a08d48e74efacfc8c0d9456e8'}
  digest=hashlib.sha256(data).hexdigest()
  if digest in meter_sources:
   metered=b'        CancellationToken cancellationToken=default)=>ResolveEndingMetadataCore(actualPreparedIdentity,cancellationToken,null);\n    internal LegacySourcePointIdentity ResolveEndingMetadataMetered(LegacySourcePointIdentity actualPreparedIdentity,\n        CancellationToken cancellationToken,LegacyOrdinarySourceTraversalMeter meter)\n        =>ResolveEndingMetadataCore(actualPreparedIdentity,cancellationToken,meter);\n    private LegacySourcePointIdentity ResolveEndingMetadataCore(LegacySourcePointIdentity actualPreparedIdentity,\n        CancellationToken cancellationToken,LegacyOrdinarySourceTraversalMeter? meter)\n    {\n        cancellationToken.ThrowIfCancellationRequested();meter?.Before(256);\n        if(originalMetadataOwner is{} owner)return owner.ResolveEndingMetadataCore(actualPreparedIdentity,cancellationToken,meter);'
   original=b'        CancellationToken cancellationToken=default)\n    {\n        cancellationToken.ThrowIfCancellationRequested();\n        if(originalMetadataOwner is{} owner)return owner.ResolveEndingMetadata(actualPreparedIdentity,cancellationToken);'
   assert data.count(metered)==1
   data=data.replace(metered,original)
   assert hashlib.sha256(data).hexdigest()==meter_sources[digest],('Wrong private-meter inverse',name)
 if hashlib.sha256(data).hexdigest()==historical:return data
 assert hashlib.sha256(data).hexdigest()==current,('Unreviewed chronology successor',name)
 assert data.count(after)==1
 restored=data.replace(after,before)
 assert hashlib.sha256(restored).hexdigest()==historical
 return restored
p=argparse.ArgumentParser();p.add_argument('--repo',required=True);p.add_argument('--output',required=True);a=p.parse_args()
s=json.loads(pathlib.Path(__file__).with_name('linguistic-fst28c-number-initial-d0-budget-pins.json').read_text());r=pathlib.Path(a.repo)
restored=[]
for name,h in s['sourcePins'].items():
 data=(r/name).read_bytes().replace(b'\r\n',b'\n');actual=hashlib.sha256(data).hexdigest()
 if pathlib.Path(name).name in ('LegacySourcePointChronology.cs','LegacyOrdinarySourceInk.cs'):
  data=restore_ordinary_chronology_source(pathlib.Path(name).name,data,h)
 if pathlib.Path(name).name in ('LegacyPreparedGlyphClassifier.cs','LegacyDynamicAcousticWeights.cs','LegacyDirectionalGlyphClassifier.cs'):
  import runpy
  helper=r/'tools/LegacyConfiguredTwoAltAugmentation.Probe/source_preimage.py'
  assert hashlib.sha256(helper.read_bytes().replace(b'\r\n',b'\n')).hexdigest()=='7f8e7fb615d340a9731fc44910d68e6f0086072bec78d408378acc48b83c689c'
  data=runpy.run_path(str(helper))['restore'](pathlib.Path(name).name,data,h)
 if pathlib.Path(name).name=='LegacyOrdinaryGraphSourceRanges.cs':
  import runpy
  helper=r/'tools/LegacyEnglishCharacters.Probe/source_preimage.py'
  assert hashlib.sha256(helper.read_bytes().replace(b'\r\n',b'\n')).hexdigest()=='f76cc8d09e11210fc529078a4fdf612be5052a26a72c8e4d78c3880da6af5e54'
  data=runpy.run_path(str(helper))['restore'](pathlib.Path(name).name,data,h)
 assert hashlib.sha256(data).hexdigest()==h,name
 if actual!=h:restored.append(dict(path=name,currentSHA256=actual,historicalSHA256=h))
v=s['limits'];f=s['fixed'];N,G,A,S,D,P0,P,R,T,C,L=(v[k] for k in ['N','G','A','S','D','P0','P','R','T','C','L'])
# Every named resource pass is additive, never paid from the feature term.
fixed=4*f['AK']+f['AK']+2*f['Networks']+3*f['OrdinaryNode']+4*f['AL']+4*f['Calibration']+4*f['Confidence']+4*f['Activation']+65536
pre=N+G+A+32*(P0+P)+64*S+64*D+4*D*S+R*(T+8*8)+4096
context=C*(5828+32768+256*min(P,4096)+16*S+16*D+2048)
selected=T*(368720+64*L*L+64*4096*4096+32768+8*D+16*C+64*L+64*S+4096)
work=fixed+pre+context+selected
b0=f['AK']+2*f['Networks']+4*f['AL']+65536
logical=b0+512*(P0+P)+128*D+256*S+16*(N+G+A)+1024*C+327680*T+4096
assert s['requiredCalls']['AKCleanupClear']==1
assert fixed>=5*f['AK']+2*f['Networks']+3*f['OrdinaryNode']
assert s['requiredCalls']['AKSha256']==3 and s['requiredCalls']['AKCopy']==1
assert s['requiredCalls']['GlyphNetworkLoad']+s['requiredCalls']['ContextNetworkLoad']==5
assert P<=4096 and L<=255
assert all(0<=x<2**63 for x in [fixed,pre,context,selected,work,logical])
# Negative old envelope: removing either repeated full-resource SHA changes W by exactly AK.
assert work-f['AK'] != work and work-2*f['AK'] == fixed-2*f['AK']+pre+context+selected
out={'sourceFilesVerified':len(s['sourcePins']),'limits':v,'logicalByteMaximum':logical,'workMaximum':work,'workTerms':dict(fixed=fixed,preflightAndSource=pre,context=context,selected=selected),'requiredCalls':s['requiredCalls'],'meaning':'protocol/cardinality reservation, not allocator/RSS/instruction bound','runtimeApproved':False,'historicalSourceRestorations':restored,'currentPointlikeNumericOrBudgetEquivalenceClaimed':False}
pathlib.Path(a.output).write_text(json.dumps(out,indent=2)+'\n');print(json.dumps(out))
