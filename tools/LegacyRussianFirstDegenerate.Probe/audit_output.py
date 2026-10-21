"""Independent scalar exact assignment + NumPy int8 NN oracle; no target labels."""
import ast,base64,hashlib,json,math,struct,sys
from pathlib import Path
import numpy as np
ROOT=Path(__file__).resolve().parents[2]
def defs(p):
 t=ast.parse(p.read_text());t.body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];s={"__file__":str(p)};exec(compile(t,str(p),"exec"),s);return s
nn=defs(ROOT/'tools/LegacyFreshMainAcoustic.Probe/audit_output.py');cost=defs(ROOT/'tools/LegacyCostEnsemble.Probe/reference_controls.py')['reference']
model=Path(sys.argv[1]);p=json.loads(Path(sys.argv[3]).read_text());b=(model/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();profile=json.loads((model/'ru-RU/legacy-engine-profile.json').read_text());actual=Path(sys.argv[2]).read_bytes();src=json.loads(actual)
assert hashlib.sha256(b).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert p['actualInputSha256']==hashlib.sha256(actual).hexdigest() and p['expectedLabelsRead'] is False
assert p['sourceFingerprint'].lower()==hashlib.sha256(b[6120:187933]).hexdigest()
assert len(p['rows'])==4 and p['checks']==95
stats=[1024,-768,0,0,-768,1024,-1024];t=[0]*217;t[6:36]=[1024]*30;t[210:217]=stats;r=stats+[-1024]*64+[1024]*128
for row,old in zip(p['rows'],src['samples']):
 d=old['diagnostic'];xy=[list(map(int,p.split(','))) for p in d['normalizedShorts'].split('|')];ctx=list(map(int,d['context'].split(',')));ends=list(map(int,d['boundaries'].split(',')))
 assert row['id']==old['id'] and row['points']==xy and all(p==xy[0] for p in xy)
 assert row['context']==ctx and row['boundaries']==ends
 assert row['trajectory']==t and row['raster']==r
 ti=t+ctx+ends;ri=r+ctx+ends
 assert row['trajectoryInput']==ti and row['rasterInput']==ri
 prob=[nn['exp_prob'](nn['network'](b,at,vec,profile['activations']['hidden'])) for at,vec in [(19195,ti),(105197,ri)]]
 assert row['trajectoryProbabilities']==prob[0] and row['rasterProbabilities']==prob[1]
 slots=[dict(probabilities=x,weight=1) for x in prob]+[None]*5
 assert row['integerCosts']==cost(slots,1,[])
 mask=list(base64.b64decode(row['mask']));assert mask==[int(i%3!=0) for i in range(180)]
 assert row['maskedCosts']==cost(slots,1,mask)
print('PASS4 actual normalized candidates:1664 complete native basic shorts/1904 prepared shorts/1440 NN probabilities/1440 masked+unmasked costs;95 guards; no targets.')
