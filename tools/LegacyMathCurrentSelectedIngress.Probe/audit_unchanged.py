from pathlib import Path
import json,sys,hashlib
def first_differences(actual,expected,limit=8):
 differences=[]
 def value(v):
  if isinstance(v,str)and len(v)>160:return v[:160]+'...'
  if isinstance(v,(dict,list)):return {'type':type(v).__name__,'count':len(v)}
  return v
 def add(path,a,b):
  if len(differences)<limit:differences.append(dict(Path=path,Actual=value(a),Expected=value(b)))
 def walk(a,b,path):
  if len(differences)>=limit:return
  if type(a)is not type(b):add(path,a,b)
  elif isinstance(a,dict):
   for key in sorted(set(a)|set(b)):
    if len(differences)>=limit:break
    if key not in a:add(path+'.'+key,'<missing>',b[key])
    elif key not in b:add(path+'.'+key,a[key],'<missing>')
    else:walk(a[key],b[key],path+'.'+key)
  elif isinstance(a,list):
   if len(a)!=len(b):add(path+'.length',len(a),len(b))
   for index,(x,y)in enumerate(zip(a,b)):
    if len(differences)>=limit:break
    walk(x,y,path+'['+str(index)+']')
  elif a!=b:add(path,a,b)
 walk(actual,expected,'$');return differences

cap=json.loads(Path(sys.argv[1]).read_bytes());old=json.loads(Path(sys.argv[2]).read_bytes());assert len(cap['Unchanged'])==len(old['Actual'])==6
for a,b in zip(cap['Unchanged'],old['Actual']):
 for key in ['Width','Reversed','Prior','Prepared','Finished','NewNn']:
  if a[key]!=b[key]:
   print(json.dumps(dict(Event='UNCHANGED_EXACT_MISMATCH',Width=a['Width'],Reversed=a['Reversed'],Field=key,CaptureSHA256=hashlib.sha256(Path(sys.argv[1]).read_bytes()).hexdigest(),ReferenceSHA256=hashlib.sha256(Path(sys.argv[2]).read_bytes()).hexdigest(),FirstDifferences=first_differences(a[key],b[key])),ensure_ascii=True),file=sys.stderr,flush=True)
  assert a[key]==b[key],(a['Width'],a['Reversed'],key)
print('PASS6 exact unchanged prior/C23/NEWNN snapshots vs authenticated old6 reference; new current route separate')
