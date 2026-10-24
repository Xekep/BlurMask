"""PE-only complete fragmented lazy-dispatch bodies and pure state controls. No target load."""
from pathlib import Path
import argparse,hashlib,json,struct
import pefile,capstone
SHA='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
REGIONS=[(0x1399f0,0x139c90,'38b7a8eec0dd21114b6f9b6d551d7f0aef91f8eb3868256fae7accdad1f9860a',[(0x1399f0,0x139a76),(0x139a76,0x139c75),(0x139c75,0x139c81)]),(0x13a670,0x13a830,'3dbaed44e2537888bf1132ab1033a800f4458d4c5a60a90384d97a72169cce57',[(0x13a670,0x13a82d)]),(0x13c6e0,0x13c840,'6c2600caf2d7b6b388aedfe41f0be739896e5adbb72a5413464c07f006f2b176',[(0x13c6e0,0x13c6e9),(0x13c6e9,0x13c6f5),(0x13c6f5,0x13c7bb),(0x13c7bb,0x13c7ff),(0x13c7ff,0x13c839)])]
PINS={0x139a30:('xor','edi, edi'),0x139a36:('mov','dword ptr [rax + 0x18], edi'),0x139b2e:('mov','dword ptr [rax + 0x18], edi'),0x13c773:('test','word ptr [rax + 0x18], r14w'),0x13c778:('jne','0x18013c7ac'),0x13c771:('mov','cl, 1'),0x13c79b:('call','r10'),0x13c79e:('test','al, al'),0x13c7a0:('setne','cl'),0x13c7a3:('or','word ptr [rbx + 0x18], r14w'),0x13c7bd:('je','0x18013c7ef'),0x13c7e4:('call','r10'),0x13a78f:('mov','r8d, 0xffffc003'),0x13a804:('and','cx, r8w'),0x13a80b:('mov','word ptr [rsi], cx')}
def audit(repo):
 raw=(repo/'research/myscript/sdk/build/x64/MyScriptText.dll').read_bytes();assert hashlib.sha256(raw).hexdigest()==SHA
 p=pefile.PE(data=raw);base=p.OPTIONAL_HEADER.ImageBase;assert base==0x180000000
 dis=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);ins={};regions=[]
 for a,z,h,fragments in REGIONS:
  b=p.get_data(a,z-a);assert hashlib.sha256(b).hexdigest()==h
  actual=[(e.struct.BeginAddress,e.struct.EndAddress)for e in p.DIRECTORY_ENTRY_EXCEPTION if a<=e.struct.BeginAddress<z];assert actual==fragments
  assert all(fragments[i][1]==fragments[i+1][0]for i in range(len(fragments)-1))
  rows=list(dis.disasm(b,base+a));assert sum(i.size for i in rows)==len(b)
  for i in rows:
   ins[i.address-base]=(i.mnemonic,i.op_str)
   if i.mnemonic.startswith('j'):
    assert i.op_str.startswith('0x') and a<=int(i.op_str,16)-base<fragments[-1][1]
  assert ins[fragments[-1][1]-1][0]=='ret'
  regions.append(dict(Start=hex(a),BodyEnd=hex(fragments[-1][1]),PinnedEndIncludingPadding=hex(z),Sha256=h,Fragments=actual,Instructions=len(rows)))
 for a,w in PINS.items():assert ins[a]==w,(hex(a),ins[a],w)
 return regions

def transition(ready,exists=True,callback=True,result=True,selection=False,select_result=True,throws=False):
 # Literal normal-return control flow; managed poisoning is not native false.
 calls=0
 if not exists:return ready,-1,calls
 ok=True
 if not ready:
  if callback:
   calls=1
   if throws:raise RuntimeError('Injected')
   ok=result
  ready=True
 if selection and ok:ok=select_result
 return ready,(0 if ok else -1),calls

def controls():
 count=0
 for exists in [False,True]:
  for ready in [False,True]:
   for callback in [False,True]:
    for result in [False,True]:
     for selection in [False,True]:
      for selected in [False,True]:
       state,rc,n=transition(ready,exists,callback,result,selection,selected)
       expected_calls=int(exists and not ready and callback);assert n==expected_calls
       expected_ok=exists and (ready or not callback or result) and (not selection or selected)
       assert(rc==0)==expected_ok and state==(ready or exists);count+=1
 # A normal false marks ready; a second dispatch skips callback and succeeds.
 r,c,n=transition(False,result=False);assert(r,c,n)==(True,-1,1)
 assert transition(r,result=False)==(True,0,0)
 # Independent canonical owner state, no cache keyed by marker or array index.
 keys=[0x1000001,0x1000001|(7<<40),0x2000001];seen=set();hits=[]
 for k in keys:
  canonical=(k&0xffffff)|(k&0xffff000000)|(k&0x100000000000000);hits.append(canonical not in seen);seen.add(canonical)
 assert hits==[True,False,True]
 throws=False
 try:transition(False,throws=True)
 except RuntimeError:throws=True
 assert throws
 # Exact16-bit append-update instructions preserve4000/8000/AVL bits.
 appends=0
 for n in range(2040):
  for flags in [0,0x4000,0x8000,0xc000]:
   for low in range(4):
    old=n*4|flags|low;dx=((old&0xfffc)<<2)&65535
    dx=((dx-65536 if dx&0x8000 else dx)>>2)+4;dx&=65535
    new=dx^((dx^old)&0xc003)
    assert new==((n+1)*4|flags|low);appends+=1
 return dict(StateCases=count,OwnerAliasTrace=hits,AppendFlagCases=appends,NormalFalseThenReady=True,ThrowIsNotFalse=True)

def main():
 a=argparse.ArgumentParser();a.add_argument('repo',type=Path);a.add_argument('report',type=Path);a=a.parse_args()
 report=dict(Status='PASS_BOUNDED_NATIVE_LAZY_DISPATCH',Regions=audit(a.repo),Pins=len(PINS),Pure=controls(),VendorLoaded=False,NN=False,Limitations=['Native callback normal false marks4000; thrown callback is distinct, no exception-success theorem','Allocator nonnull and valid fresh node access are success-domain premises; no allocator callback freshness inferred','This PE-only proof does not execute the owning dispatcher; the one-reference actual fixture does not prove aliases','No CHAR/sourceprocessing/owning integration supplied by this proof'])
 a.report.write_bytes(json.dumps(report,indent=2).encode());print(json.dumps({'Status':report['Status'],'Pins':len(PINS),'Pure':report['Pure']}))
if __name__=='__main__':main()
