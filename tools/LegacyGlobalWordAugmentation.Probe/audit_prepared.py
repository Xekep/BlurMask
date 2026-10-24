"""Independent full-byte prepared checks. Native/reference data, no production import/NN/targets."""
import base64,copy,hashlib,json,math,struct,sys
from pathlib import Path
f=lambda v:struct.unpack('<f',struct.pack('<f',v))[0]
def word(head,cost,trial,duplicate=False):
 b=bytearray(120);struct.pack_into('<Q',b,8,head)
 score=f(f(-5)*cost)
 if duplicate:score=f(score-f(.00001))
 struct.pack_into('<ffff',b,16,score,1,cost,f(trial/8192))
 struct.pack_into('<ii',b,40,1,1);struct.pack_into('<i',b,60,-1);struct.pack_into('<i',b,84,-1)
 return b

def expected_toy_rows():
 rows=[]
 for i in [2,1,0]:
  b=bytearray(72);parent=i-1;previous=i+1 if i<2 else -1
  struct.pack_into('<iH',b,0,i,0);struct.pack_into('<i',b,8,i);struct.pack_into('<iH',b,12,parent,0 if parent>=0 else 65535);struct.pack_into('<i',b,20,parent)
  struct.pack_into('<iH',b,24,parent,0 if parent>=0 else 65535);struct.pack_into('<i',b,32,parent);struct.pack_into('<iH',b,36,previous,0 if previous>=0 else 65535)
  struct.pack_into('<QHH',b,48,i,i+1,i+1);b[62]=255;rows.append(b)
 return b''.join(rows)

def check(historical,c):
 assert c['NNCalls']==0 and c['ActualOwnerEntryExercised'] is False and c['PreparedCases']==128 and c['HistoricalResults']==12198 and c['IneligibleNoInspect'] is True
 assert len(c['Historical'])==len(historical)==7
 for source,rows in zip(historical,c['Historical'],strict=True):
  expected=[]
  for entry in source['walks']:
   for emit in [False,True]:
    e=entry['emit' if emit else 'inspect'];expected.append(dict(Index=entry['index'],Emit=emit,Status=e['status'],Minimum=e['minimum'],Parent=e['parent'],Source=e['source'],Flags=e['wordFlags'],Rows=e['rows']))
  assert rows==expected
 assert len(c['Prepared'])==128 and [x['Trial'] for x in c['Prepared']]==list(range(128))
 for trial,x in enumerate(c['Prepared']):
  assert x['SelectedOrdinal']==0 and x['PayloadCount']==2 and len(x['BeforeMass'])==len(x['AfterMass'])==2
  c0=f(f(.1)+f(trial/1000));c1=f(f(.2)+f(trial/1000));a=word(0,c0,trial);b=word(1<<40,c1,trial,True)
  assert list(map(base64.b64decode,x['BeforeMass']))==[a,b]
  diff=f(struct.unpack_from('<f',b,16)[0]-struct.unpack_from('<f',a,16)[0]);mass=f(math.exp(diff));first=f(1/f(1+mass));second=f(mass*first)
  struct.pack_into('<ff',a,16,first,1);struct.pack_into('<ff',b,16,second,1)
  assert list(map(base64.b64decode,x['AfterMass']))==[a,b]
 t=c['EligibleFlow'];assert t['SelectedCount']==1 and t['Inspections']==1 and t['Created']==3 and t['Head']==2
 assert base64.b64decode(t['Rows72'])==expected_toy_rows()
 compact=bytearray(120)
 for i in range(3):
  struct.pack_into('<QiiiH',compact,i*40+8,i,i-1,(i+1)*100,(i+1)*50,i+1);compact[i*40+32]=8 if i==2 else 0;compact[i*40+33]=255;struct.pack_into('<i',compact,i*40+36,-1)
 assert base64.b64decode(t['Compact'])==compact
 b=bytearray(120);struct.pack_into('<Q',b,8,2);struct.pack_into('<ffff',b,16,f(-5*f(300/24576)),1,f(300/24576),f(150/8192));struct.pack_into('<ii',b,40,3,3);struct.pack_into('<i',b,60,-1);struct.pack_into('<i',b,84,-1)
 assert base64.b64decode(t['Word120'])==b
 assert len(c['Source26'])==5 and [x['Count'] for x in c['Source26']]==[0,1,2,125,1024]
 for row in c['Source26']:
  n=row['Count'];expected=b''.join(struct.pack('<iiiiii',i+100,-i-3,i*7,i+200,-i-9,i*11) for i in range(n))+bytes(v&255 for i in range(n) for v in [i*3,i*5])
  assert base64.b64decode(row['Bytes'])==expected
 seed=bytearray(120);seed[44:48]=bytes([0xa5])*4;seed[52]=0xa6;struct.pack_into('<Q',seed,8,(1<<64)-1);struct.pack_into('<ff',seed,16,1,1);struct.pack_into('<I',seed,48,65534);struct.pack_into('<i',seed,60,-1);struct.pack_into('<i',seed,84,-1)
 assert base64.b64decode(c['WorkingReuse120'])==seed

h=Path(sys.argv[1]).read_bytes();assert hashlib.sha256(h).hexdigest()=='a94ddc61d1bbbb0548b76464b63919cb1ca5a0fd9d97f7a6cd34234324a69640';historical=json.loads(h);raw=Path(sys.argv[2]).read_bytes();capture=json.loads(raw);check(historical,capture)
negative=0
for field,where in [('Word120','EligibleFlow'),('Rows72','EligibleFlow'),('BeforeMass','Prepared'),('AfterMass','Prepared')]:
 t=copy.deepcopy(capture)
 if where=='EligibleFlow':v=bytearray(base64.b64decode(t[where][field]));v[-1]^=1;t[where][field]=base64.b64encode(v).decode()
 else:v=bytearray(base64.b64decode(t[where][0][field][0]));v[-1]^=1;t[where][0][field][0]=base64.b64encode(v).decode()
 try:check(historical,t)
 except AssertionError:negative+=1
 else:raise AssertionError('tamper accepted')
for kind in ['omitted','duplicate','historical','NN']:
 t=copy.deepcopy(capture)
 if kind=='omitted':t['Prepared'].pop()
 elif kind=='duplicate':t['Prepared'][1]=t['Prepared'][0]
 elif kind=='historical':t['Historical'][0][0]['Source']^=1
 else:t['NNCalls']=1
 try:check(historical,t)
 except AssertionError:negative+=1
 else:raise AssertionError('tamper accepted')
for field in ['Source26','WorkingReuse120']:
 t=copy.deepcopy(capture)
 if field=='Source26':v=bytearray(base64.b64decode(t[field][-1]['Bytes']));v[-1]^=1;t[field][-1]['Bytes']=base64.b64encode(v).decode()
 else:v=bytearray(base64.b64decode(t[field]));v[52]^=1;t[field]=base64.b64encode(v).decode()
 try:check(historical,t)
 except AssertionError:negative+=1
 else:raise AssertionError('tamper accepted')
print(json.dumps(dict(status='PASS_DATA_ONLY',historicalResults=12198,preparedCases=128,syntheticEligibleFlow=1,source26Cases=5,workingReuseCycles=125,ineligibleNoInspect=True,full120ByteImages=128*4+1,negativeControls=negative,NNCalls=0,actualOwnerEntryExercised=False,captureSha256=hashlib.sha256(raw).hexdigest()),indent=2))
