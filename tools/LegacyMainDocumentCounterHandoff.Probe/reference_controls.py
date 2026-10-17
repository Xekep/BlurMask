"""Native0298AA equality ->07AAF0 operation7 append; no command execution."""
import json,random,sys,struct,base64
from pathlib import Path
r=random.Random(0x7aaf0);rows=[]
for n in range(1024):
 old=r.randrange(-(1<<63),1<<63);new=old if n%4==0 else r.randrange(-(1<<63),1<<63)
 failed=bool(n&1);prefix=r.randrange(5);append=old!=new and not failed
 body=base64.b64encode(struct.pack('<qq',new,old)+bytes(40)).decode()if append else None
 rows.append(dict(old=old,new=new,allocationFailure=failed,prefix=prefix,succeeded=old==new or not failed,body56=body,commandCount=prefix+append))
Path(sys.argv[1]).write_bytes((json.dumps(dict(cases=rows),separators=(',',':'))+'\n').encode())
print('Wrote1024 independent pending counter append/failure controls')
