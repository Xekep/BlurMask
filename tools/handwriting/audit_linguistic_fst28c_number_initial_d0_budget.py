import argparse,hashlib,json,pathlib
p=argparse.ArgumentParser();p.add_argument('--repo',required=True);p.add_argument('--output',required=True);a=p.parse_args()
s=json.loads(pathlib.Path(__file__).with_name('linguistic-fst28c-number-initial-d0-budget-pins.json').read_text());r=pathlib.Path(a.repo)
restored=[]
for name,h in s['sourcePins'].items():
 data=(r/name).read_bytes().replace(b'\r\n',b'\n');actual=hashlib.sha256(data).hexdigest()
 if pathlib.Path(name).name in ('LegacyPreparedGlyphClassifier.cs','LegacyDynamicAcousticWeights.cs','LegacyDirectionalGlyphClassifier.cs'):
  import runpy
  helper=r/'tools/LegacyConfiguredTwoAltAugmentation.Probe/source_preimage.py'
  assert hashlib.sha256(helper.read_bytes().replace(b'\r\n',b'\n')).hexdigest()=='41677ef5434f5292f8ffb706403b8ef83c5dead91ed6ef882accb1e09c309709'
  data=runpy.run_path(str(helper))['restore'](pathlib.Path(name).name,data,h)
 if pathlib.Path(name).name=='LegacyOrdinaryGraphSourceRanges.cs':
  import runpy
  helper=r/'tools/LegacyEnglishCharacters.Probe/source_preimage.py'
  assert hashlib.sha256(helper.read_bytes().replace(b'\r\n',b'\n')).hexdigest()=='3839cc530b618f397eb1757f02455f124053561f3ab8f1f77a46d391e4b76d12'
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
