"""Independent raw immutable-model DATA oracle; no target/profile/source calls."""
from pathlib import Path
import hashlib,json,sys,copy,struct
SHA="8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1"
def expected(raw):
 assert len(raw)==3685881 and hashlib.sha256(raw).hexdigest()==SHA
 # Signed scalar return -> explicit wire reconstruction is independently raw-derived.
 first=raw[8736:123936];second=raw[123936:177392]
 assert b"".join(struct.pack("<h",x[0])for x in struct.iter_unpack("<h",first))==first
 assert bytes(x[0]&255 for x in struct.iter_unpack("b",second))==second
 digests=[hashlib.sha256(raw[a:b]).hexdigest()for a,b in [(8736,123936),(123936,177392),(177392,177650)]]
 guards=[dict(Name="prepare-pre-cancel",Type="OperationCanceledException",Parameter=None,TokenMatched=True),dict(Name="empty-resource",Type="ArgumentException",Parameter="resource",TokenMatched=False)]
 for b in range(3):
  for prefix in ["negative-index-","past-end-index-"]:guards.append(dict(Name=prefix+str(b),Type="ArgumentOutOfRangeException",Parameter="index",TokenMatched=False))
 guards += [dict(Name="negative-bank",Type="ArgumentOutOfRangeException",Parameter="bank",TokenMatched=False),dict(Name="past-end-bank",Type="ArgumentOutOfRangeException",Parameter="bank",TokenMatched=False),dict(Name="scalar-pre-cancel",Type="OperationCanceledException",Parameter=None,TokenMatched=True),dict(Name="wiped-resource-hash",Type="InvalidDataException",Parameter=None,TokenMatched=False)]
 return dict(version=1,resourceSHA=SHA,resourceBytes=3685881,profileSuccesses=1,profileHashRefusals=1,callerOverwriteByte=165,sameProfileAcrossPasses=True,before=digests,after=digests.copy(),scalarCalls=222637,wireBytes=337828,knownLoopArrayPayloadBytes=1360,callerOverwriteBytes=3685881,maximumCaptureBytes=8192,calibrationPayloadBytes=4772,hasCandidateAuthority=False,guards=guards,geometryCalls=0,sourceCalls=0,firstCalls=0,NN=0)
def strict(a,e,path="root"):
 assert type(a)is type(e),(path,type(a),type(e))
 if type(e)is dict:
  assert a.keys()==e.keys(),(path,"keys")
  for k in e:strict(a[k],e[k],path+"/"+k)
 elif type(e)is list:
  assert len(a)==len(e),(path,"length")
  for i,(x,y)in enumerate(zip(a,e)):strict(x,y,path+"/"+str(i))
 else:assert a==e,(path,a,e)
def controls(e):
 cases=[]
 for field in ["before","after"]:
  for i in range(3):
   a=copy.deepcopy(e);a[field][i]="0"*64;cases.append(a)
 for k in e:
  a=copy.deepcopy(e);del a[k];cases.append(a)
 for k in ["scalarCalls","wireBytes","profileSuccesses","sourceCalls"]:
  a=copy.deepcopy(e);a[k]=True;cases.append(a)
 for i in range(12):
  a=copy.deepcopy(e);a["guards"][i]["Type"]="Exception";cases.append(a)
 a=copy.deepcopy(e);a["unexpected"]=0;cases.append(a)
 for a in cases:
  try:strict(a,e)
  except AssertionError:pass
  else:raise AssertionError("corruption accepted")
 return len(cases)
def unique(pairs):
 d={}
 for k,v in pairs:
  assert k not in d,("duplicate-key",k)
  d[k]=v
 return d
if __name__=="__main__":
 assert len(sys.argv)in(3,4)
 e=expected(Path(sys.argv[1]).read_bytes())
 if sys.argv[2]=="--selftest":print(json.dumps(dict(PASS=True,controls=controls(e),targetCalls=0)))
 else:
  raw=Path(sys.argv[2]).read_bytes();assert len(raw)<=8192
  strict(json.loads(raw.decode("utf-8"),object_pairs_hook=unique),e);print(json.dumps(dict(PASS=True,controls=controls(e))))
