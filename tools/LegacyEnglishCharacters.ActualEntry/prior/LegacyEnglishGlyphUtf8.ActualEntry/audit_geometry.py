"""DATA-only fixed genuine input geometry replay. No NN or live issuer construction."""
import argparse,base64,copy,hashlib,json,struct,types
from pathlib import Path
HERE=Path(__file__).resolve().parent
SCOPE='ACTUAL_OWNING_ENGLISH_GLYPH_SOURCE_GEOMETRY_DATA_NO_CHAR_TEXT_ADMISSION'
BASELINE='5688b05c9c19e805c2e830c3faa5324ddc39e47161df96905e57639c00eeb53f'
RAW='d1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99'
def sha(b):return hashlib.sha256(b).hexdigest()
def readpin(p,h):
 b=p.read_bytes();assert sha(b)==h;return json.loads(b)
def module(p,name):
 m=types.ModuleType(name);m.__file__=str(p);exec(compile(p.read_bytes().decode('utf-8-sig'),str(p),'exec'),m.__dict__);return m

def load(baseline,top,source,compact,raw,models):
 for rel,h in json.loads((HERE/'geometry-auditor-pins.json').read_text()).items():assert sha((HERE/rel).read_bytes())==h
 b=readpin(baseline,BASELINE);r=readpin(raw,RAW);assert type(r)is list and len(r)==141
 g=module(HERE/'audit_actual.py','frozen_glyph_auditor')
 e=g.load(top,source,compact,raw,models);want=g.expected(e);g.audit(b,e,want)
 p=module(HERE.parent/'LegacyEnglishGlyphGeometry.Probe/audit_output.py','independent_geometry_scalars')
 return b,e,want,g,p,r[4]

def decode26(text):
 b=base64.b64decode(text,validate=True);assert len(b)>0 and len(b)%26==0;n=len(b)//26
 return [list(struct.unpack_from('<6i',b,i*24))+[b[n*24+i*2],b[n*24+i*2+1]]for i in range(n)]
def flat(pairs):return [v for r in pairs for v in r]
def expected_geometry(b,e,want,g,p,raw):
 assert type(raw['Raw'])is list and len(raw['Raw'])>0
 counts=[len(s)for s in raw['Raw']];assert counts==[3]and b['OriginalIndex']==4 and b['RawPoints']==3
 root=[[p.pack(0,0,3,counts[0]),p.pack(len(counts)-1,counts[-1]-1,5,counts[-1])]]
 #Word storage is already independently checked by v5 raw-source chronology,
 #not newly accepted from a purported correct geometry channel.
 word=p.project(counts,decode26(e['source']['SourceCompletion']['Source']['Storage26']))
 #The one fixed original fixture has one reference; do not generalize lazy4000.
 choices=want['GlyphChoices'];assert len(choices)==1
 oldpool={n['Key']:base64.b64decode(n['Records'],validate=True)for n in e['top']['SourceCompletion']['Raw']['Owners']}
 out=[]
 for c in choices:
  ref=c['Reference'];key=g.normalkey(ref);ordinal=ref>>40&65535;original=oldpool[key][ordinal*40:(ordinal+1)*40];assert len(original)==40 and original[4]&7==0
  symbol=struct.unpack_from('<H',original,8)[0];joiner=symbol==e['s']['labels'].get('\u200d',65535)
  projected=p.project(counts,decode26(c['Source26']));clipped=p.scope['intersect'](projected,word)
  out.append(dict(Reference=ref,OriginalDescriptor40=base64.b64encode(original).decode(),OriginalJoiner=joiner,ProjectedPairs=flat(projected),WordClippedPairs=flat(clipped)))
 return dict(OriginalStrokePointCounts=counts,OriginalRoot=flat(root),WordClip=flat(word),Glyphs=out)

def audit(x,b,want,g):
 assert type(x)is dict and set(x)==set(b)|{'Geometry','CHARCreated'}and x['Scope']==SCOPE
 assert x['CHARCreated']is False
 #Compare the ENTIRE baseline envelope, not a fake Scope projection supplied
 #to an old validator or a numerical channel accepted because another passes.
 for k,v in b.items():
  if k!='Scope':g.strict(x[k],v)
 g.strict(x['Geometry'],want)
 return dict(GeometryGlyphReferences=len(want['Glyphs']),OriginalObjects=len(want['OriginalStrokePointCounts']),WordClipRanges=len(want['WordClip'])//2,ProjectedRanges=sum(len(v['ProjectedPairs'])//2 for v in want['Glyphs']),ClippedRanges=sum(len(v['WordClippedPairs'])//2 for v in want['Glyphs']))

def negatives(x,b,want,g):
 tests=[('source mode',lambda y:y['Geometry']['OriginalRoot'].__setitem__(0,7)),('bool position',lambda y:y['Geometry']['OriginalRoot'].__setitem__(0,True)),('point count bool',lambda y:y['Geometry']['OriginalStrokePointCounts'].__setitem__(0,True)),('point count wrong',lambda y:y['Geometry']['OriginalStrokePointCounts'].__setitem__(0,4)),('WORDclip missing',lambda y:y['Geometry'].update(WordClip=[])),('projected missing',lambda y:y['Geometry']['Glyphs'][0].update(ProjectedPairs=[])),('clipped missing',lambda y:y['Geometry']['Glyphs'][0].update(WordClippedPairs=[])),('original descriptor changed',lambda y:y['Geometry']['Glyphs'][0].update(OriginalDescriptor40=base64.b64encode(bytes(40)).decode())),('original joiner changed',lambda y:y['Geometry']['Glyphs'][0].update(OriginalJoiner=True)),('reference changed',lambda y:y['Geometry']['Glyphs'][0].update(Reference=0)),('glyph omitted',lambda y:y['Geometry'].update(Glyphs=[])),('glyph extra',lambda y:y['Geometry']['Glyphs'].append(copy.deepcopy(y['Geometry']['Glyphs'][0]))),('CHAR numeric',lambda y:y.update(CHARCreated=0)),('CHAR invented',lambda y:y.update(CHARCreated=True)),('scope old',lambda y:y.update(Scope=b['Scope'])),('geometry omitted',lambda y:y.pop('Geometry')),('extra result',lambda y:y.update(Fake=True)),('metrics changed',lambda y:y.update(Metrics={})),('Vector10 changed',lambda y:y.update(Vector10=[])),('TOP changed',lambda y:y['Top'].update(SelectedWordOrdinal=True)),('Source changed',lambda y:y['SourceCompletion']['Source'].update(Storage26='')),('glyph choice changed',lambda y:y['GlyphChoices'][0].update(Selected4='')),('glyph UTF8 changed',lambda y:y.update(GlyphUtf8=[])),('metadata float',lambda y:y.update(OriginalIndex=4.0))]
 refused=[]
 for name,change in tests:
  y=copy.deepcopy(x);change(y)
  try:audit(y,b,want,g)
  except (AssertionError,KeyError,TypeError,ValueError):refused.append(name);continue
  raise AssertionError('Tamper accepted '+name)
 return refused

def main():
 a=argparse.ArgumentParser();a.add_argument('capture',type=Path);a.add_argument('baseline',type=Path);a.add_argument('prior_top',type=Path);a.add_argument('prior_source',type=Path);a.add_argument('prior_compact',type=Path);a.add_argument('raw',type=Path);a.add_argument('models',type=Path);a.add_argument('report',type=Path);a.add_argument('--tamper',action='store_true');a=a.parse_args()
 assert a.capture.stat().st_size<=32*1024*1024
 b,e,w,g,p,r=load(a.baseline,a.prior_top,a.prior_source,a.prior_compact,a.raw,a.models);expected=expected_geometry(b,e,w,g,p,r);x=json.loads(a.capture.read_bytes());report=audit(x,b,expected,g)
 report.update(Status='PASS_SCOPED_OWNING_GEOMETRY_DATA',WholeBaselineMatched=True,ActualSha256=sha(a.capture.read_bytes()),CHARCreated=False,NNEvaluated=False,NotVerified=['live source issuer/epoch/mutation gates are SOURCE reviewed; detached data cannot prove them','full native13C6E0 lazy4000/repeated-reference caching','CHAR/sourceprocessing/interner/WORD/TEXT/document/app/quality/admission','general externalC22/cached/partial/prior/root identity; this is exact library raw-parent full-domain adaptation'])
 if a.tamper:report['TamperNames']=negatives(x,b,expected,g);report['TamperRefusals']=len(report['TamperNames'])
 a.report.write_text(json.dumps(report,indent=2));print(json.dumps(report))
if __name__=='__main__':main()
