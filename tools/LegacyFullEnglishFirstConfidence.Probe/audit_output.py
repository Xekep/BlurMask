"""Independent full-FIRST confidence oracle, composed with the complete A raw
feature/NN/cache audit. No SDK execution or expected target text."""
from pathlib import Path
import argparse,ast,base64,struct,hashlib,json,math
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--profile',type=Path);p.add_argument('--capture',type=Path,required=True);p.add_argument('--confidence',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
def defs(path):
 ns={'struct':struct,'base64':base64,'math':math};nodes=[n for n in ast.parse(path.read_text(encoding='utf-8-sig')).body if isinstance(n,(ast.FunctionDef,ast.ClassDef))];exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),ns);return ns
wire=defs(a.repo/'tools/LegacyFirstAcoustic119Wire.Probe/audit_output.py');d=(a.models/'en-US/en_US-ak-cur.res').read_bytes();assert hashlib.sha256(d).hexdigest()=='355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'
_,_,_,children=wire['source'](d,0);selected=next(c for c in children if c[3]==0x249);start=selected[1]+16;end,kind,count,children=wire['source'](d,start);assert(start,end,kind,count)==(8843,373957,0x135,217)
header,labels=defs(a.repo/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py')['labels'](d[start:end]);assert header==[181,178,65535,180,184,182,183,217]
#Independently derive the small tail and actual slot packets from raw model bytes.
#No generated report or C#capture supplies model fields or family count authority.
network=defs(a.repo/'tools/LegacyPreparedNetwork107.Probe/audit_output.py');nets=network['first_networks'](d);assert[x[0]for x in nets]==[0,1,2,3]
_,akstart,akend,akkind,akversion=next(c for c in children if c[0]=='140');assert(akstart,akend,akkind,akversion)==(18695,373766,0x21d,0x119)
slots=[]
for index,p0,p1 in nets:
 assert d[p0-6:p0]==bytes([0,0,1,2,0,1])and d[p1]==0
 magic,version,size,architecture,nkind,cols,hidden,z1,z2,outputs,trailing=struct.unpack_from('<HHIBHIIIIIH',d,p0)
 assert(magic,version,size,architecture,z1,z2,trailing)==(0xfea0,0x107,p1-p0-8,0x12,0,0,10)
 q=p0+33;matrices=[]
 for inputs,rows in[(cols,hidden-1),(hidden,outputs-1)]:
  q=(q+15)//16*16;stride=(inputs+15)//16*16;scales=q+stride*rows;assert d[scales]==1 and all(d[scales+1:scales+rows+1])
  matrices.append(dict(input=inputs,rows=rows,stride=stride,weightsOffset=q,scalesOffset=scales,weightsSha256=hashlib.sha256(d[q:scales]).hexdigest(),scalesSha256=hashlib.sha256(d[scales:scales+rows+1]).hexdigest()));q=scales+rows+1
 assert q==p1
 slots.append(dict(index=index,first=0,second=1,mode=2,weight=None,networks=[dict(offset=p0,end=p1,version=version,position=1,kind=nkind,input=cols,hidden=hidden-1,output=outputs-1,matrices=matrices)]))
q=nets[-1][2]+1
for index in range(4,7):
 conversion,first,second,mode=struct.unpack_from('<4B',d,q);weight=struct.unpack_from('<f',d,q+4)[0]
 assert conversion==mode==0 and weight==0 and d[q+8:q+11]==bytes(3)
 slots.append(dict(index=index,first=first,second=second,mode=mode,weight=weight,networks=[]));q+=11
r=wire['Acoustic'](d,q,akend)
r.u('78');r.f('7C');r.u('80');r.f('88');r.f('84')
for i in range(r.u('90',2)):r.lists(f'98[{i}].count',f'98[{i}]',1,'21D')
for name in('A4','A8','AC','B0','B4'):r.f(name)
r.u('B8');r.f('BC');r.f('C0');ng=r.u('C8',2);assert ng==8
ends0=struct.unpack('<9H',r.take('21D','D0',2,9));r.take('21D','D8',2,ends0[-1]);[r.f(f'E0[{i}]')for i in range(ng)]
r.u('E8',4);r.u('EC',2);r.u('EE',2)
for name in('F0','F4','F8'):r.u(name,4)
r.u('FC');r.f('100');r.u('108',4);r.u('128',4);r.u('148/150.present');r.f('120');r.f('124');assert r.p==akend
fields={x['Destination']:base64.b64decode(x['Raw'])for x in r.fields}
assert tuple(int.from_bytes(fields[n],'little')for n in('78','80','B8','108','128','148/150.present'))==(0,0,0,0,0,0)
assert struct.unpack('<f',fields['100'])[0]==0
if a.profile is not None:
 assert hashlib.sha256(a.profile.read_bytes()).hexdigest()=='c5808e37ba7deda45da6b7ceb1315d29ad8882abfef386f24cabac8ea7532822'
 profile=json.loads(a.profile.read_bytes())['models'][0];assert profile['resourceSha256']==hashlib.sha256(d).hexdigest()and profile['slots']==slots
 for x in profile['fields']:
  raw=base64.b64decode(x['Raw']);assert raw==d[x['Offset']:x['Offset']+len(raw)]
  if x['Owner']=='21D'and x['Destination']in fields:assert raw==fields[x['Destination']]
threshold=int.from_bytes(fields['E8'],'little');minimum=int.from_bytes(fields['EC'],'little');limit=int.from_bytes(fields['EE'],'little');assert(threshold,minimum,limit)==(85173,1,10)
assert int.from_bytes(fields['108'],'little')==int.from_bytes(fields['128'],'little')==0
assert all(s['mode']==2 and len(s['networks'])==1 for s in slots[:4])and all(not s['networks']for s in slots[4:])
ends=struct.unpack('<9H',fields['D0']);members=struct.unpack('<17H',fields['D8']);aliases={}
for i in range(8):
 g=members[ends[i]:ends[i+1]];alpha=struct.unpack('<f',fields[f'E0[{i}]'])[0]
 if alpha>0 and any(s>=182 for s in g):
  target=next((s for s in g if s<182),65535)
  if target!=65535:
   for s in g:
    if s>=182:aliases.setdefault(s,target)
assert not aliases
for text in('-','\uff0d','_','\uff3f','\u00ad','\u30fc','\u4e00','\U000f0002','\U000f0003'):
 if text in labels and not(text=='\u30fc'and labels[text]<182):aliases.setdefault(labels[text],header[1])
aliases[218]=labels["'"];aliases[219]=labels['"']
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
bits=lambda x:struct.pack('<f',x)
def cal(p):return f(1/(1+math.exp(-f(p*f(1/32768))*.575)))
enclosure=defs(a.repo/'tools/LegacySourceEnclosure.Probe/reference_controls.py')['evaluate'];enclosure.__globals__.update(f=f,f32=f)
capture=json.loads(a.capture.read_bytes());actual=json.loads(a.confidence.read_bytes());assert not actual['TargetsRead']
assert len(actual['Calibration'])==len(actual['PreparedStreaming'])==32769
wrong_slope=0;wrong_association=0
for p in range(32769):
 expected=cal(p);assert bits(expected)==bits(actual['Calibration'][p]);wrong_slope+=bits(expected)!=bits(f(1/(1+math.exp(-f(p*f(1/32768))*f(.575)))))
 ps=[p,32768-p,p//2,(p*17)%32769];v=[cal(x)for x in ps];total=f(v[1]+v[0])
 for x in v[2:]+[0.,0.,0.,0.]:total=f(total+x)
 assert bits(min(1.,f(total/4)))==bits(actual['PreparedStreaming'][p])
 wrong_association+=bits(f(total/4))!=bits(f(f(f(v[1]+v[0])+f(v[2]+v[3]))/4))
assert wrong_slope>0 and wrong_association>0 and actual['Guards']==464 and actual['Cancellations']==10
assert len(capture['Actual'])==len(actual['Actual']);groups=values=closed=partial=positive_partial=wrong_two=0
for arow,brow in zip(capture['Actual'],actual['Actual']):
 assert arow['Name']==brow['Name']
 if arow['Status']!='AcceptedAcoustic':assert brow['Status']=='Refused';continue
 assert brow['Status']=='AcceptedConfidence' and arow['NetworkCount']==brow['NetworkCount']
 assert len(arow['Children'])==len(brow['Children'])
 for achild,bchild in zip(arow['Children'],brow['Children']):
  assert achild['Descriptor']==bchild['Descriptor']and len(achild['Groups'])==len(bchild['Groups'])
  for ag,bg in zip(achild['Groups'],bchild['Groups']):
   assert ag['Group']==bg['Group'];candidates=ag['Candidates'];assert len(candidates)==len(bg['Values'])<=10
   physical=ag['Stored'][0][0];stored=enclosure(achild['Raw'],physical);assert abs(stored-bg['StoredProbabilityInspection'])<3e-6
   if ag['Probability']is not None:assert bits(ag['Probability'])==bits(bg['StoredProbabilityInspection'])
   partial+=not ag['Whole'];positive_partial+=not ag['Whole']and stored>0
   probabilities=[n['Probabilities']for n in ag['Networks']]
   for candidate,value in zip(candidates,bg['Values']):
    symbol=candidate[0]
    if symbol==labels['\U000f0001']:expected=bg['StoredProbabilityInspection'];closed+=1
    else:
     output=aliases.get(symbol,symbol);assert output<182;v=[cal(p[output])for p in probabilities];total=f(v[1]+v[0])
     for x in v[2:]+[0.,0.,0.,0.]:total=f(total+x)
     expected=min(1.,f(total/4));wrong_two+=bits(expected)!=bits(f(f(v[1]+v[0])/2))
    assert bits(value)==bits(expected),(arow['Name'],ag['Group'],symbol,value,expected);values+=1
   groups+=1
assert groups==actual['Groups']==capture['Groups']and values==actual['Values']and actual['Accepted']==capture['Accepted']and actual['Refused']==capture['Refused']
report=dict(status='PASS',scope='Actual full-FIRST4family scalar confidence; not word acceptance',acousticSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),confidenceSha256=hashlib.sha256(a.confidence.read_bytes()).hexdigest(),actual=len(actual['Actual']),accepted=actual['Accepted'],refused=actual['Refused'],groups=groups,values=values,closed=closed,partialStoredInspections=partial,positivePartialInspections=positive_partial,wrongTwoFamilyDiscriminators=wrong_two,wrongFloatSlopeDiscriminators=wrong_slope,wrongAssociationDiscriminators=wrong_association,exhaustiveCalibration=32769,preparedStreaming=32769,guards=actual['Guards'],cancellations=actual['Cancellations'],targetsRead=False,vendorExecuted=False)
a.output.write_text(json.dumps(report,indent=2));print(json.dumps(report))
