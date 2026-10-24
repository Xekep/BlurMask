"""Historical whole AL/header + independently declared native storage DATA receipt."""
from pathlib import Path
import json,hashlib,copy,sys
BASE="3611e7a4f2b150b8735adae05e3f38c404177133a20165d3dd2165675950396e"
def unique(pairs):
 d={}
 for k,v in pairs:
  assert k not in d,("duplicate",k)
  d[k]=v
 return d
def strict(a,e,p="root"):
 assert type(a)is type(e),(p,type(a),type(e))
 if type(e)is dict:
  assert a.keys()==e.keys(),(p,"keys")
  for k in e:strict(a[k],e[k],p+"/"+k)
 elif type(e)is list:
  assert len(a)==len(e),(p,"count")
  for i,(x,y)in enumerate(zip(a,e)):strict(x,y,p+"/"+str(i))
 else:assert a==e,(p,a,e)
def expected(raw):
 assert hashlib.sha256(raw).hexdigest()==BASE
 e=json.loads(raw.decode("utf-8"),object_pairs_hook=unique)
 assert len(e["cases"])==14 and len(e["guards"])==93
 assert sum(r["data"]["Header"]["Positive1390Count"]>0 for r in e["cases"])==1
 e["StorageSchema"]="first276-storage-allocation-v1"
 e["StorageCases"]=[]
 for r in e["cases"]:
  data=dict(Version=1,Alphabet=copy.deepcopy(r["data"]),InputArrayLength=288,HiddenArrayLength=208,OutputArrays=4,OutputArrayLength=258,NativeObjectBytes=200,NativeScratchBytes=5135,TypedScratchBytes=5120,ModelPayloadBytes=168946,ModelCopyBytes=337908,ModelSourceUnits=13267216,AdditionalStoragePayloadBytes=5307,AdditionalStorageCopyBytes=5315,AdditionalStorageSourceUnits=114406,HasInitializedFeatures=False,HasPreparedReceiver=False,HasCandidateAuthority=False)
  e["StorageCases"].append(dict(name=r["name"],data=data))
 return e
def controls(e):
 cases=[]
 for i in range(14):
  for key,value in e["StorageCases"][i]["data"].items():
   if type(value)in(int,bool):
    a=copy.deepcopy(e);a["StorageCases"][i]["data"][key]=value+1 if type(value)is int else not value;cases.append(a)
  a=copy.deepcopy(e);a["StorageCases"][i]["data"]["InputArrayLength"]=True;cases.append(a)
  a=copy.deepcopy(e);a["cases"][i]["data"]["Header"]["WorkWords"]+=1;cases.append(a)
  a=copy.deepcopy(e);a["StorageCases"][i]["data"]["Alphabet"]["OpaqueRequestBytes"]=0;cases.append(a)
 for key in e:
  a=copy.deepcopy(e);del a[key];cases.append(a)
 for a in cases:
  try:strict(a,e)
  except AssertionError:pass
  else:raise AssertionError("corruption accepted")
 return len(cases)
if __name__=="__main__":
 assert len(sys.argv)==3
 e=expected(Path(sys.argv[1]).read_bytes())
 if sys.argv[2]!="--selftest":
  raw=Path(sys.argv[2]).read_bytes();assert len(raw)<=131072
  strict(json.loads(raw.decode("utf-8"),object_pairs_hook=unique),e)
 print(json.dumps(dict(PASS=True,controls=controls(e),cases=14,oldGuards=93,positive1390=1,profileExecutionsByAuditor=0,owningSourceExecutionsByAuditor=0,NN=0)))
