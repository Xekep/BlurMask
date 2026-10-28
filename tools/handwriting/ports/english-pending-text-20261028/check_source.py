"""Read-only exact source authentication. Never applies a patch to production."""
from pathlib import Path
import argparse,hashlib,json
PACKET=Path(__file__).resolve().parent
PINS_SHA="9e75ac97ae23a08fce4b6709225a63ae96cbdf3d6373e0b8d429a368d52e59ca"
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def verify(root,pins):
 root=root.resolve();actual={p.name:p for p in root.glob('*.cs')}
 if set(actual)!=set(pins):raise ValueError(f"Wrong exact source extent: {len(actual)} files, expected {len(pins)}; missing={sorted(set(pins)-set(actual))}; extra={sorted(set(actual)-set(pins))}")
 for name,digest in pins.items():
  if sha(actual[name])!=digest:raise ValueError('Unknown source hash: '+name)
 return len(actual)
def main():
 a=argparse.ArgumentParser(description=__doc__);a.add_argument('--baseline950',type=Path);a.add_argument('--candidate950',type=Path);a.add_argument('--production935',type=Path);v=a.parse_args()
 if sha(PACKET/'source-pins.json')!=PINS_SHA:raise ValueError('Source pin manifest changed')
 d=json.loads((PACKET/'source-pins.json').read_text())
 assert len(d['base950'])==len(d['candidate950'])==950 and len(d['production935'])==935 and len(d['changedSix'])==6
 for name in d['changedSix']:
  if sha(PACKET/'source'/name)!=d['candidate950'][name] or sha(PACKET/'preimage'/name)!=d['base950'][name]:raise ValueError('Packet patch/preimage changed: '+name)
 assert {n for n in d['base950'] if d['base950'][n]!=d['candidate950'][n]}==set(d['changedSix'])
 assert d['base950']['LegacyEnglishCaseNode0Ledger.cs']==d['candidate950']['LegacyEnglishCaseNode0Ledger.cs']
 result={'packetSixAndPreimages':'PASS','capsLedgerUnchanged':True}
 for label,root,key in [('baseline950',v.baseline950,'base950'),('candidate950',v.candidate950,'candidate950'),('production935',v.production935,'production935')]:
  if root is not None:
   result[label]=verify(root,d[key])
   if sha(root/'InkRecognition.csproj')!=d['projectHashes']['base950' if label=='baseline950' else label]:raise ValueError('Project metadata changed: '+label)
 print(json.dumps(result))
if __name__=='__main__':main()
