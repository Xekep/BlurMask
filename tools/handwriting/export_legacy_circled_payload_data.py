"""Original bounded static CFG constraint extraction. Reads pinned native comparisons; never loads or executes vendor code.
Each accepted path must constrain every payload byte to one literal. Unsupported opcodes/ambiguous domains abort.
"""
import sys,copy,struct,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
import capstone
from capstone.x86 import *
b=Binary('MyScriptEngine.dll');cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);cs.detail=True
STARTS={3:0x180049474,6:0x180049fb3,12:0x18004a18b,15:0x18004a20c}
END=0x18004a285;FAIL=0x18004a2c8
cache={}
def ins(pc):
 if pc not in cache:cache[pc]=next(cs.disasm(b.data[b.off(pc):b.off(pc)+15],pc,count=1))
 return cache[pc]
def rootreg(name):
 if name in ['rax','eax','ax','al']:return 'a'
 if name in ['rcx','ecx','cx','cl']:return 'c'
 if name in ['rdx','edx','dx','dl']:return 'd'
 if name in ['r8','r8d','r8w','r8b']:return '8'
 raise ValueError(name)
def evalop(o,i,regs,n):
 if o.type==X86_OP_IMM:return ('const',o.imm)
 if o.type==X86_OP_REG:return regs.get(rootreg(i.reg_name(o.reg)),('const',-1))
 if o.type==X86_OP_MEM:
  base=i.reg_name(o.mem.base);idx=o.mem.disp-(4 if base=='rsi' else 0)
  if base not in ['rdi','rsi']:raise ValueError(('mem',base,idx))
  return ('byte',idx,False)
 raise ValueError(o.type)
def integer(v,x,width):
 if v[0]=='const':return v[1]&((1<<(width*8))-1)
 y=x
 if v[2] and x>=128:y=x-256
 return y&((1<<(width*8))-1)
def extract(n,start):
 stack=[(start,{'8':('const',-1)},[set(range(256)) for _ in range(n)],None,-1)];results=[];visits=0
 while stack:
  pc,regs,ds,cmp,out=stack.pop();visits+=1
  if visits>100000:raise ValueError('budget')
  if pc==FAIL:continue
  if pc==END:
   if out<0:raise ValueError('negative output')
   if any(len(x)!=1 for x in ds):raise ValueError(('unbound accepted',n,hex(out),[len(x) for x in ds]))
   results.append((bytes(next(iter(x)) for x in ds),out));continue
  i=ins(pc);op=i.mnemonic;os=i.operands;nextpc=pc+i.size
  if op.startswith('nop'):stack.append((nextpc,regs,ds,cmp,out));continue
  if op=='jmp':stack.append((os[0].imm,regs,ds,cmp,out));continue
  if op in ['mov','movzx','movsx']:
   v=evalop(os[1],i,regs,n)
   if v[0]=='byte' and op=='movsx':v=(v[0],v[1],True)
   if os[0].type==X86_OP_REG:
    regs=regs.copy();regs[rootreg(i.reg_name(os[0].reg))]=v
   elif os[0].type==X86_OP_MEM and i.reg_name(os[0].mem.base)=='rsp' and os[0].mem.disp==0x30:
    if v[0]!='const':raise ValueError(('symbol output',hex(pc),v))
    out=v[1]
   else:raise ValueError(('store',hex(pc),i.op_str))
   stack.append((nextpc,regs,ds,cmp,out));continue
  if op=='cmp':
   left=evalop(os[0],i,regs,n);right=evalop(os[1],i,regs,n);cmp=(left,right,os[0].size);stack.append((nextpc,regs,ds,cmp,out));continue
  if op in ['je','jne','ja','jb','jae','jbe']:
   left,right,width=cmp
   if right[0]!='const':
    if len(ds[right[1]])==1:right=('const',next(iter(ds[right[1]])))
    else:raise ValueError(('cmp',hex(pc),cmp,[len(x) for x in ds]))
   idx=left[1] if left[0]=='byte' else None
   possibles=ds[idx] if idx is not None and 0<=idx<n else ({[0xf3,0xb0,0x80,0x81][idx-n]} if idx is not None and n<=idx<n+4 else {0})
   matched=set()
   for x in possibles:
    l,r=integer(left,x,width),integer(right,x,width)
    take={'je':l==r,'jne':l!=r,'ja':l>r,'jb':l<r,'jae':l>=r,'jbe':l<=r}[op]
    if take:matched.add(x)
   for vals,dest in [(matched,os[0].imm),(set(possibles)-matched,nextpc)]:
    if not vals:continue
    ds2=[set(x) for x in ds]
    if idx is not None and 0<=idx<n:ds2[idx]=vals
    stack.append((dest,regs,ds2,cmp,out))
   continue
  raise ValueError(('instruction',hex(pc),op,i.op_str))
 return results,visits
def main():
 import argparse,hashlib
 p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
 if hashlib.sha256(b.data).hexdigest()!='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c':raise ValueError('Pinned source SHA differs')
 starts=struct.unpack_from('<15I',b.data,b.off(0x18004a494))
 for n,start in STARTS.items():
  if 0x180000000+starts[n-1]!=start:raise ValueError('Circle switch target changed')
 rows=[];counts={}
 for n,start in STARTS.items():
  accepted,visits=extract(n,start);counts[str(n)]=dict(paths=len(accepted),visits=visits)
  rows += [(raw.hex(),cp) for raw,cp in accepted]
 rows.sort();assert len(rows)==158 and len({h for h,cp in rows})==158 and len({cp for h,cp in rows})==127
 numeric=struct.pack('<i',len(rows))
 for h,cp in rows:
  raw=bytes.fromhex(h);numeric+=struct.pack('<i',len(raw))+raw+struct.pack('<i',cp)
 result=dict(format='legacy-native-circled-payload-data-v1',engineSha256=hashlib.sha256(b.data).hexdigest(),
             tableSha256=hashlib.sha256(numeric).hexdigest(),literalPayloads=rows)
 output=Path(a.output);output.parent.mkdir(parents=True,exist_ok=True);output.write_bytes((json.dumps(result,separators=(',',':'))+'\n').encode())
 print(json.dumps(dict(paths=counts,bytes=output.stat().st_size,sha256=hashlib.sha256(output.read_bytes()).hexdigest(),tableSha256=result['tableSha256'])))
if __name__=='__main__':main()
