"""Prepared numeric inputs only. No inference, labels or actual owner claim."""
from pathlib import Path
import ast,hashlib,json,struct,sys
D=Path(__file__).resolve().parent
if len(sys.argv)!=3:raise SystemExit('REPOSITORY FRESH_FIXTURES_JSON')
R=Path(sys.argv[1])
ak=(R/'models/en-US/en_US-ak-cur.lite.res').read_bytes();assert hashlib.sha256(ak).hexdigest()=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
g={'struct':struct};s=D/'reference/old_raw_compact_oracle.py';t=ast.parse(s.read_text(encoding='utf-8-sig'));exec(compile(ast.Module(body=[n for n in t.body if isinstance(n,ast.FunctionDef)and n.name in ['u16','u32','i32','read_alphabet']],type_ignores=[]),str(s),'exec'),g)
al=g['read_alphabet'](ak);ids={s:i for i,s in al['mapping'].items()};abc=[ids[x]for x in 'abc'];assert all(x<217 for x in abc)
p=182477;assert struct.unpack_from('<HH',ak,p)==(0x201,217);p+=4
n=ak[p];p=(p+4)&~3;first=list(struct.unpack_from('<'+'i'*n,ak,p));p+=n*4;fm=struct.unpack_from('<220b',ak,p);p+=220
n=ak[p];p=(p+4)&~3;ge=list(struct.unpack_from('<'+'i'*n,ak,p));p+=n*4;start=struct.unpack_from('<220b',ak,p);p+=220;end=struct.unpack_from('<220b',ak,p);zero=first.index(0);pairs=[]
for f,a,z in zip(fm,start,end):
 for fi,gi in ((f,a),(f,z),(zero,a),(zero,z)):
  if fi!=-1 and gi!=-1 and(first[fi],gi)not in pairs:pairs.append((first[fi],gi))
categories=[pairs.index((first[fm[x]],start[x]))for x in abc]
def w(b,o,v,n=4):b[o:o+n]=(v&((1<<(n*8))-1)).to_bytes(n,'little')
cases=[]
for k in range(64):
 nodes=[]
 for node in range(2):
  rows=bytearray([167])*(156*88);pool=bytearray([167])*(125*80)
  for o in range(156):w(rows,o*88+16,0x4000000);rows[o*88+6]=0
  for o in range(250):w(pool,o*40,0x4000000);pool[o*40+27]=0
  for o in [0,1,125]:
   at=o*88;suffix=o%2;ctx=(1+suffix)if node==0 else(3 if suffix==0 or k&8 else 4)
   fallback=bool(k&1)if o==0 else bool(k&2)
   tag=(1<<32)|(int(fallback)<<41)|(17+node*10+o)
   w(rows,at,tag,8);w(rows,at+8,ctx,8);w(rows,at+16,1000+o*2+node*40);w(rows,at+20,7)
   w(rows,at+24,node,2);rows[at+26]=categories[suffix];rows[at+27]=0;w(rows,at+28,0,2)
   w(rows,at+30,0 if o<125 else 125,2);rows[at+32]=0;rows[at+33]=0
   w(rows,at+34,0xa7a7,2);w(rows,at+36,abc[suffix],2);rows[at+38]=1;rows[at+39]=0
   w(rows,at+40,8<<40,8);w(rows,at+48,-1);w(rows,at+52,11);w(rows,at+56,node);w(rows,at+60,o,2);w(rows,at+62,65535,2);w(rows,at+80,-1)
   rows[at+6]=2;rows[at+7]=0
   if o>=125:continue
   for a in range(1+(1 if k&4 else 0)):
    pa=o*80+a*40;ctx2=ctx if a==0 or k&16 else(2 if node==0 else 4)
    w(pool,pa,100+o*10+a);w(pool,pa+4,7);w(pool,pa+8,ctx2,8);w(pool,pa+16,ctx2,8)
    w(pool,pa+24,0,2);pool[pa+26]=0;pool[pa+27]=128|a;w(pool,pa+28,0xa7a7,2);pool[pa+30]=0;pool[pa+31]=32 if node==0 else 0
  nodes.append(dict(SourceEnd=126,RetainedEnd=126,Rows=rows.hex(),Primary=pool.hex()))
 costs=[-100 if k&32 else 100,200,400]
 cases.append(dict(Id=k,Nodes=nodes,ContextPaths=[[abc[0]],[abc[1]],[abc[0],abc[1]],[abc[0],abc[2]],[abc[2]]],
  Candidates=[list(zip(abc,costs)),sorted(zip(abc,list(reversed(costs))),key=lambda x:x[1])],Scales=[1.,1.6817928552627563],
  Replies=[bool((k>>4)&1),bool((k>>5)&1)],Penalty=512 if k%3 else -512,Headers=[0,0xc0e]))
# Scalar budget controls include exact/one-short ceilings; no live-owner scope.
budgets=[dict(Nodes=n,Arcs=a,Records=r)for n in [2,3,49]for a in [1,4096]for r in [1,100,1048576]]
q=Path(sys.argv[2]);assert not q.exists();q.write_text(json.dumps(dict(Scope='PREPARED_MARKED207CB0_ONLY',Cases=cases,Budgets=budgets),separators=(',',':')),encoding='utf-8')
print(hashlib.sha256(q.read_bytes()).hexdigest())
