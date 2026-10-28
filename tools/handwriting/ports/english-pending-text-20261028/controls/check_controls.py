"""Independent strict audit of numeric visits and native-prefix/cancellation controls."""
from pathlib import Path
import argparse,json
P=Path(__file__).resolve().parent
def strict(a,b):
 if type(a)!=type(b):return False
 if isinstance(a,dict):return set(a)==set(b) and all(strict(a[k],b[k]) for k in a)
 if isinstance(a,list):return len(a)==len(b) and all(strict(x,y) for x,y in zip(a,b))
 return a==b
def read(p,array=False):
 s=p.read_text(encoding='utf-8-sig');return json.loads(s) if array else [json.loads(x) for x in s.splitlines() if x.strip()]
a=argparse.ArgumentParser(description=__doc__);a.add_argument('charges',type=Path);a.add_argument('reuse',type=Path);a.add_argument('cancellation',type=Path);v=a.parse_args()
for expected,actual,array in [('expected-charges.json',v.charges,True),('expected-reuse.jsonl',v.reuse,False),('expected-cancellation.jsonl',v.cancellation,False)]:
 x=read(P/expected,array);y=read(actual)
 if not strict(x,y):raise ValueError('Independent capture mismatch: '+expected)
 print(expected+': PASS '+str(len(x)))
