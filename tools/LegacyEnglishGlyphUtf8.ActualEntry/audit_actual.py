"""DATA-only fixed-input EN ordinary refresh/selection/Latin-glyph UTF8 replay.
No target/NN/vendor execution. Native live owner and call counts remain source evidence.
"""
import argparse,base64,copy,hashlib,json,struct,types
from pathlib import Path
HERE=Path(__file__).resolve().parent
SCOPE='ACTUAL_OWNING_ENGLISH_ORDINARY_GLYPH_REFRESH_SELECTION_UTF8_DATA_NO_TEXT_ADMISSION'
TOP='e21f7b9633e82c7b1ae42f5f9abea3e705d65d867fb10b2f52b516c5ad40bd77'
SOURCE='04a55b4c2f1c435cbab421ad10c75f1476fbbfe6dc4d82858aac0b5c946d7846'
PUBLICATION='6daddbc9ce3d1d3d2e9e179ce008db60887f2c6a13c52ab8bb47dcb215890673'
NORMALIZATION='3cc1d91269959a13b5700b7463d3db7442b83b593ba70435a94665d6531226f2'
IMP=0x4000000

def sha(v):return hashlib.sha256(v).hexdigest()
def integer(v,lo=0,hi=(1<<64)-1):
 assert type(v)is int and lo<=v<=hi
 return v
def decoded(v):
 assert type(v)is str
 return base64.b64decode(v,validate=True)
def encoded(v):return base64.b64encode(v).decode()
def strict(a,b):
 assert type(a)is type(b)
 if type(a)is dict:
  assert set(a)==set(b)
  for k in b:strict(a[k],b[k])
 elif type(a)is list:
  assert len(a)==len(b)
  for x,y in zip(a,b,strict=True):strict(x,y)
 else:assert a==b

def readpin(path,pin):
 raw=path.read_bytes();assert sha(raw)==pin
 return json.loads(raw)
def module(path,name):
 m=types.ModuleType(name);m.__file__=str(path);exec(compile(path.read_bytes().decode('utf-8-sig'),str(path),'exec'),m.__dict__);return m

def upstream():
 for rel,pin in json.loads((HERE/'dependency-pins.json').read_text()).items():assert sha((HERE/rel).read_bytes())==pin
 return module(HERE/'upstream/v5/audit_actual.py','pinned_source_v5')

def load(top,source,compact,rawinput,models):
 oldtop=readpin(top,TOP);oldsource=readpin(source,SOURCE);u=upstream()
 s=u.setup(compact,rawinput,models/'en-US/en_US-ak-cur.lite.res');prior_report=u.audit(oldsource,s)
 strict(oldtop['SourceCompletion'],oldsource['SourceCompletion']);strict(oldtop['Vector10'],oldsource['Vector10']);strict(oldtop['Metrics'],oldsource['Metrics'])
 conf=u.confidence(oldsource,s);p=readpin(models/'text-unicode/legacy-text-publication-data.json',PUBLICATION)
 n=readpin(models/'text-unicode/legacy-normalization-data.json',NORMALIZATION)
 assert n['format']=='legacy-native-unicode-normalization-data-v1'and len(n['combiningRanges'])==517
 return dict(top=oldtop,source=oldsource,s=s,u=u,confidence=conf,publication=p,normalization=n,prior_report=prior_report)

def normalkey(reference):return reference&0x00ffffff|reference&0x000000ffff000000|reference&0x0100000000000000

def source26(rows):
 out=bytearray(len(rows)*26)
 for i,row in enumerate(rows):
  a=row['First'];b=row['Last'];struct.pack_into('<6i',out,i*24,a['First'],a['Second'],a['Third'],b['First'],b['Second'],b['Third'])
  out[len(rows)*24+i*2]=row['FirstResidual'];out[len(rows)*24+i*2+1]=row['LastResidual']
 return bytes(out)

def selection(rows,marker,flags,key):
 m=module(HERE/'upstream/selection/reference_controls.py','pinned_native_selection')
 control=dict(key=key,packed=(len(rows)<<2)|(0x8000 if flags else 0),owner=encoded(b''.join(rows)),marker=marker,initialCapacity=0,allocation=encoded(bytes((len(rows)*3//2+2)*4)))
 result=m.oracle(control);assert result['succeeded']
 raw=decoded(result['rows']);return list(struct.unpack('<'+'i'*(len(raw)//4),raw)),result['marker']

def expected(e):
 u=e['u'];s=e['s'];c=s['c'];compact=s['b'];old=e['source']['SourceCompletion'];graph=c['Graph']
 words=old['Raw']['Published275'];selected=e['top']['Top']['SelectedWordOrdinal'];word=next(w for w in words if w['Index']==selected)
 refs=word['FlatReferences'];assert 1<=len(refs)<=128
 pool={};flags={};order=[]
 for node in old['Raw']['Owners']:
  key=integer(node['Key']);assert key==normalkey(key)and key not in pool
  raw=decoded(node['Records']);packed=integer(node['PackedCount'],0,65535);assert (packed>>2)&2047==len(raw)//40 and len(raw)%40==0
  pool[key]=[bytearray(raw[i:i+40])for i in range(0,len(raw),40)];flags[key]=bool(packed&0x8000);order.append(key)
 headers=struct.unpack_from('<8H',s['ak'],99);scores={v['Group']:v for v in c['Scores']};separations=old['Source']['GroupSeparations'];progress=[]
 replacement=s['labels']['\ufffd'];closed=s['closed'];bypass={closed,*[s['labels'].get(t,65535)for t in['\U000f0004','\U000f0008','\U000f0002','\U000f0003']],*headers[4:7]}
 for reference in refs:
  integer(reference);key=normalkey(reference);assert key in pool and not key&(1<<56)
  node=key&0xffffff;arc=(key>>24)&65535;assert node<len(graph)and arc<len(graph[node])
  actual=graph[node][arc];group=actual['Group'];oldscore=scores[group];assert len(oldscore['Symbols'])>0
  confidence=e['confidence'][group];firstbits=u.bits(confidence.get(oldscore['Symbols'][0],0))&0xffffffff
  proof=u.source_inputs(old['Source']['SourceInputs'],actual['Start'],actual['End']);ranges=source26(proof['References']);assert ranges
  costs={a:b for a,b in reversed(list(zip(oldscore['Symbols'],oldscore['Costs'],strict=True)))};visited=0;first=c['Starts'][node];last=c['Starts'][node+1]
  for index in range(first,last):
   row=compact[index*40:(index+1)*40];traits=row[32]
   if not traits&0x20 or traits&7 or u.qq(row,8)!=key:continue
   descriptor=index-first;ordinal=next((i for i,v in enumerate(pool[key])if u.ii(v,0)==descriptor),None)if not flags[key]else None
   if ordinal is None:
    assert len(pool[key])<2040
    v=bytearray(40);u.wi(v,0,descriptor);u.wh(v,8,65535);u.wf(v,12,1);u.wf(v,16,-1);u.wq(v,32,(1<<64)-1);pool[key].append(v);ordinal=len(pool[key])-1
   v=pool[key][ordinal];symbol=u.hh(row,28)
   assert symbol<217 and symbol not in(replacement,closed,headers[0]),'Reached replacement/closed/blank lacks this independent scalar input scope'
   #Actual admitted ordinary identity conversion; no wide/Arabic/macro stand-in.
   u.wh(v,4,(u.hh(v,4)&0xfff8));u.wh(v,8,symbol);u.wi(v,16,u.bits(confidence.get(symbol,0)))
   cost=costs.get(symbol,IMP)
   if cost<IMP and symbol not in bypass:cost=(cost+separations[group]+2**31)%2**32-2**31
   u.wi(v,20,cost);u.wf(v,12,u.f(row[33]/255));u.wh(v,4,(u.hh(v,4)&0xfff7)|(8 if traits&0x40 else 0));visited+=1
  progress.append((key,ranges,firstbits,visited))
 choices=[];texts=[];literal={bytes.fromhex(v[0]):v[1]for v in e['publication']['literalRows']}
 for reference,(key,ranges,firstbits,visited)in zip(refs,progress,strict=True):
  marker=(reference>>40)&65535;marker=marker if marker<32768 else marker-65536;marker&=0xffffffff
  chosen,newmarker=selection(pool[key],marker,flags[key],key);selected4=b''.join(struct.pack('<i',v)for v in chosen);full=b''.join(pool[key])
  choices.append(dict(Reference=reference,Source26=encoded(ranges),FirstScoreBits=firstbits,Refreshed=visited,Owner40=encoded(full),Selected4=encoded(selected4),Marker=newmarker))
  utf8=[];published=[]
  for ordinal in chosen:
   row=pool[key][ordinal];symbol=u.hh(row,8);assert u.hh(row,4)&7==0 and symbol!=headers[3]
   text=s['al']['mapping'][symbol];assert len(text)==1 and('A'<=text<='Z'or'a'<=text<='z'),'Auditor currently supports genuine single ASCII Latin glyphs only'
   cp=ord(text);ccc=next((value for a,z,value in e['normalization']['combiningRanges']if a<=cp<=z),-1);assert ccc==0
   script=next((v for a,z,v in e['publication']['scriptRanges']if a<=cp<=z),0);assert script==2
   raw=text.encode('utf-8');value=u.hh(row,6)&0x3ff
   if literal.get(raw,-1)>=0:value|=0x10000
   published.append(value);utf8.append(encoded(raw))
  texts.append(dict(Reference=reference,Flags=published,Utf8=utf8,RetainedSelection4=encoded(selected4),Marker=newmarker if newmarker<2**31 else newmarker-2**32))
 owners=[dict(Key=key,PackedCount=(len(pool[key])<<2)|(0x8000 if flags[key]else 0),Records=encoded(b''.join(pool[key])))for key in order]
 return dict(Owners=owners,GlyphChoices=choices,GlyphUtf8=texts)

def validate_projection(x,want):
 assert set(x)==set(want);strict(x,want)
 return {'Owner40Bytes':sum(len(decoded(v['Records']))for v in want['Owners']),'Selection4Bytes':sum(len(decoded(v['Selected4']))for v in want['GlyphChoices']),'Source26Bytes':sum(len(decoded(v['Source26']))for v in want['GlyphChoices']),'Utf8Bytes':sum(len(decoded(v))for g in want['GlyphUtf8']for v in g['Utf8']),'References':len(want['GlyphChoices'])}

def audit(x,e,want,scope=SCOPE):
 assert scope in(SCOPE,'PREPARED_GLYPH_ENVELOPE_NOT_ACTUAL')
 old=e['top'];assert set(x)==set(old)|{'GlyphChoices','GlyphUtf8'}and x['Scope']==scope
 for k,v in old.items():
  if k not in('Scope','SourceCompletion'):strict(x[k],v)
 raw=x['SourceCompletion']['Raw'];prior=e['source']['SourceCompletion'];assert set(x['SourceCompletion'])=={'Raw','Source'}
 strict(x['SourceCompletion']['Source'],prior['Source']);assert set(raw)==set(prior['Raw'])
 for k,v in prior['Raw'].items():
  if k!='Owners':strict(raw[k],v)
 return validate_projection(dict(Owners=raw['Owners'],GlyphChoices=x['GlyphChoices'],GlyphUtf8=x['GlyphUtf8']),want)

def main():
 p=argparse.ArgumentParser();p.add_argument('capture',type=Path);p.add_argument('prior_top',type=Path);p.add_argument('prior_source',type=Path);p.add_argument('prior_compact',type=Path);p.add_argument('input',type=Path);p.add_argument('models',type=Path);p.add_argument('report',type=Path);p.add_argument('--tamper',action='store_true');a=p.parse_args()
 assert a.capture.stat().st_size<=32*1024*1024
 e=load(a.prior_top,a.prior_source,a.prior_compact,a.input,a.models);want=expected(e);x=json.loads(a.capture.read_text());report=audit(x,e,want)
 report.update(Status='PASS_SCOPED_REFRESH_SELECTION_ASCII_GLYPH_DATA',ActualSha256=sha(a.capture.read_bytes()),NativeExecuted=False,NNEvaluated=False,WholeTEXTAdmission=False,NotIndependentlyVerified=['NN outputs and geometry Separation derivations are unchanged pinned observed inputs, scalar calibration/cost composition replayed.','Count/fill invocation multiplicity and private live-stage/epoch identity are source reviewed, not inferable from detached DATA.','Request category/prototype argument byte not exported; same pinned zero Separation vector makes accepted ordinary numerical cost independent of valid category index.','Non-ASCII/multi-scalar/circled/alternate/control/HeaderE glyph UTF8 and blank/closed/replacement scalar requests explicitly refuse this auditor scope.','Source geometry/CHAR/dedup/full TEXT/document/readiness/resource-budget/quality acceptance remain unverified.'])
 if a.tamper:
  m=module(HERE/'test_projection.py','projection_tamper')
  report['TamperRefusals']=m.negatives(want,validate_projection)
 a.report.write_text(json.dumps(report,indent=2));print(json.dumps(report))
if __name__=='__main__':main()
