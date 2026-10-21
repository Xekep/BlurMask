"""Portable static PE body/leaf/dispatch/frame ABI audit. Never loads SDK DLLs."""
from pathlib import Path
import argparse,json,hashlib,struct
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();proof=json.loads(Path(__file__).with_name('english-full-ctc-frame-pins.json').read_bytes());sha=lambda b:hashlib.sha256(b).hexdigest();counts={'registeredFunctions':0,'fullBodyRegions':0,'closedUnregisteredLeaves':0,'virtualBindings':0,'instructionPins':0,'focusedDirectCalls':0}
for name,entry in proof['binaries'].items():
 d=(a.sdk/name).read_bytes();assert sha(d)==entry['sha256'];pe=struct.unpack_from('<I',d,60)[0];assert d[pe:pe+4]==b'PE\0\0';n=struct.unpack_from('<H',d,pe+6)[0];opt=pe+24;assert struct.unpack_from('<H',d,opt)[0]==0x20b;base=struct.unpack_from('<Q',d,opt+24)[0];at=opt+struct.unpack_from('<H',d,pe+20)[0];sections=[]
 for i in range(n):q=at+i*40;size,rv,rawsize,raw=struct.unpack_from('<IIII',d,q+8);sections.append((rv,rawsize,raw))
 def read(at,n):
  rv=int(at,16)-base
  for start,size,raw in sections:
   if start<=rv and rv+n<=start+size:return d[raw+rv-start:raw+rv-start+n]
  raise AssertionError((name,at,n))
 functions=set()
 for v in entry['regions']:
  lo,hi=int(v['start'],16),int(v['endExclusive'],16);assert hi-lo==v['bytes'];assert sha(read(v['start'],v['bytes']))==v['sha256'];functions.add(v['function']);counts['fullBodyRegions']+=1
 counts['registeredFunctions']+=len(functions)
 for v in entry['unregisteredClosedLeaves']:
  expected=bytes.fromhex(v['bytes']);assert len(expected)==int(v['endExclusive'],16)-int(v['start'],16);assert read(v['start'],len(expected))==expected and sha(expected)==v['sha256']and expected[-1]==0xc3
  assert all(int(v['start'],16)<=int(x,16)<int(v['endExclusive'],16)for x in v['internalBranchTargets']);counts['closedUnregisteredLeaves']+=1
 for v in entry['bindings']:assert struct.unpack('<Q',read(v['at'],8))[0]==int(v['target'],16);counts['virtualBindings']+=1
 for v in entry['instructions']:expected=bytes.fromhex(v['bytes']);assert read(v['at'],len(expected))==expected;counts['instructionPins']+=1
 for v in entry['focusedDirectCalls']:
  expected=bytes.fromhex(v['bytes']);assert read(v['at'],len(expected))==expected and len(expected)==5 and expected[0]==0xe8 and int(v['at'],16)+5+struct.unpack_from('<i',expected,1)[0]==int(v['target'],16);counts['focusedDirectCalls']+=1
result={'status':'PASS','scope':'Actual full FIRST1011/219guide/21E input dependency only; no feature inference/readiness claim',**counts,'nativeExecuted':False};a.output.write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result))
