"""DATA comparison to precompile FAKE mathematical expectations only."""
import argparse,base64,copy,hashlib,json
from pathlib import Path
def sha(b):return hashlib.sha256(b).hexdigest()
def verify(fixture,got,fixture_sha,library_sha):
 assert got['Version']==fixture['Version']==1 and got['FixtureSha256']==fixture_sha and got['LibrarySha256']==library_sha
 assert got['Scope']=='FAKE_PROBABILITY_ONLY; actual compiled converter; no models/weights/NN'
 for group in['MixedRows','GenericRows']:
  assert len(got[group])==len(fixture[group])
  for c,r in zip(fixture[group],got[group]):
   assert r==dict(Name=c['Name'],InputUnchanged=True,**c['Expected']),('independent fake probability',group,c['Name'])
 assert got['SigmoidRows']==[dict(Raw=c['Raw'],Q15=c['Expected'])for c in fixture['SigmoidRows']]
def controls(fixture,got,fixture_sha,library_sha):
 result=[]
 def corrupt(name,edit):
  bad=copy.deepcopy(got);edit(bad);assert bad!=got
  try:verify(fixture,bad,fixture_sha,library_sha)
  except (AssertionError,KeyError,IndexError):result.append(name)
  else:raise AssertionError('Corruption accepted: '+name)
 for group in['MixedRows','GenericRows']:
  for i,row in enumerate(got[group]):
   if row['Exception']=='none':
    corrupt(f'{group}{i}.Q15',lambda x,i=i,group=group:x[group][i]['Q15'].__setitem__(0,x[group][i]['Q15'][0]^1))
   else:corrupt(f'{group}{i}.exception',lambda x,i=i,group=group:x[group][i].__setitem__('Exception','none'))
  corrupt(group+'.inputMutation',lambda x,group=group:x[group][0].__setitem__('InputUnchanged',False))
 for i,row in enumerate(got['SigmoidRows']):corrupt(f'sigmoid{i}',lambda x,i=i:x['SigmoidRows'][i].__setitem__('Q15',x['SigmoidRows'][i]['Q15']^1))
 corrupt('fixtureIdentity',lambda x:x.__setitem__('FixtureSha256','0'*64))
 corrupt('libraryIdentity',lambda x:x.__setitem__('LibrarySha256','0'*64))
 return result
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('fixture');p.add_argument('capture');p.add_argument('--reviewed-library-sha256',required=True);p.add_argument('--report');a=p.parse_args()
 library_sha=a.reviewed_library_sha256.lower();assert len(library_sha)==64 and all(c in'0123456789abcdef'for c in library_sha)
 raw=Path(a.fixture).read_bytes();fixture=json.loads(raw);got=json.loads(Path(a.capture).read_bytes());verify(fixture,got,sha(raw),library_sha)
 negative=controls(fixture,got,sha(raw),library_sha)
 report=dict(scope='FAKE_PROBABILITY_ONLY; rounded-exp assumption explicit; no real weights/raw vectors/models/NN',status='MATCH_FAKE_EXPECTATIONS_ONLY',
  fixtureSha256=sha(raw),librarySha256=library_sha,mixed=len(got['MixedRows']),generic=len(got['GenericRows']),sigmoid=len(got['SigmoidRows']),corruptionsRejected=len(negative),controls=negative)
 output=json.dumps(report,indent=2)
 if a.report:Path(a.report).write_text(output+'\n')
 print(output)
