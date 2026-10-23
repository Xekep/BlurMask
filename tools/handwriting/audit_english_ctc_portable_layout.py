"""Compare the existing portable pack's byte layout with conditional native binding.

This checks matrix/vector storage, not recurrent gate mathematics or decoding.
No vendor code or neural inference executes.
"""
from pathlib import Path
import argparse,hashlib,json,runpy,struct,sys

p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--sdk',type=Path,required=True)
p.add_argument('--models',type=Path,required=True)
p.add_argument('--portable',type=Path,required=True)
p.add_argument('--output',type=Path,required=True)
args=p.parse_args()
repo=Path(__file__).resolve().parents[2]
old=sys.argv
sys.argv=[str(repo/'tools/handwriting/audit_english_ctc_graph_blob_dataflow.py'),'--sdk',str(args.sdk),'--models',str(args.models),'--output',str(args.output.with_name(args.output.stem+'-native-layout.json'))]
try:up=runpy.run_path(sys.argv[0],run_name='__main__')
finally:sys.argv=old
Address=up['Address'];native=up['GraphBinder'](8,True,True);native.run()
packed=args.portable.read_bytes()
assert hashlib.sha256(packed).hexdigest()=='39418d35708502db6520fa22781614c30c7256efdb9981bf6bbf9b1b0c4f8d5b'
assert packed[:8]==b'IKRPACK1' and struct.unpack_from('<I',packed,8)[0]==1
body=packed[48:]
assert struct.unpack_from('<I',packed,12)[0]==len(body) and hashlib.sha256(body).digest()==packed[16:48]
position=0
def take(count):
 global position
 assert 0<=count<=len(body)-position
 value=body[position:position+count];position+=count;return value
def integer():return struct.unpack('<i',take(4))[0]
def text():return take(integer()).decode('utf-8')
assert [integer()for _ in range(5)]==[13,182,181,3,181]
assert text()=='en-US'
take(26*4+4);minimum=integer();assert 1<=minimum<=8 and integer()==0
assert [[integer()for _ in range(3)]for _ in range(3)]==[[127,1,2]]*3
assert integer()==2
labels=[text()for _ in range(181)]
assert integer()==67
tensors=[]
for _ in range(67):tensors.append((text(),[integer()for _ in range(7)]))
length=integer();payload=take(length);assert position==len(body) and length==2007776
resource=(args.models/'en-US/en_US-ak-cur.res').read_bytes()
assert payload==resource[1774464:1774464+length]

def expected_layout():
 expected=[]
 for layer in (1,2,3):
  for group,role in [('input','i'),('recurrent','r')]:
   name=role+str(layer)
   for direction,side in enumerate(('forward','backward')):
    for ordinal,gate in enumerate(('g','i','f','o')):
     base=direction*448+ordinal*64
     start=native.memory[Address(name,base),8].offset
     end=native.memory[Address(name,base+8),8].offset
     scales=native.memory[Address(name,base+48),8].offset
     stride=native.memory[Address(name,base+56),4]
     assert end-start==127*stride*2 and scales-end==128*8
     expected.append((f'layer{layer}.{side}.{group}.{gate}',[1,start,scales+32-start,127,stride,13 if layer==1 and role=='i'else 254 if role=='i'else 127,2]))
    if role=='r':
     for ordinal,gate in enumerate(('i','f','o')):
      base=direction*448+256+ordinal*64
      start=native.memory[Address(name,base),8].offset
      stride=native.memory[Address(name,base+56),4]
      expected.append((f'layer{layer}.{side}.recurrent.p{gate}',[2,start,stride*4,127,1,0,1]))
 start=native.memory[Address('out'),8].offset
 stride=native.memory[Address('out',56),4]
 scales=native.memory[Address('out',48),8].offset
 expected.append(('output',[1,start,scales+32-start,182,stride,254,2]))
 return expected
expected=expected_layout()
assert tensors==expected,'Every portable tensor descriptor must match native storage order and offsets'
for (name,row),(nextname,nextrow)in zip(tensors,tensors[1:]):assert row[1]+row[2]==nextrow[1],(name,nextname)
assert tensors[0][1][1]==0 and tensors[-1][1][1]+tensors[-1][1][2]==length
report=dict(status='PASS67 portable tensor storage descriptors and complete payload match conditional native binding',tensorCount=67,payloadBytes=length,portableSha256=hashlib.sha256(packed).hexdigest(),gateMathematicsProven=False,callerAllocatorOriginsProven=False,wholeEngineProven=False,vendorExecuted=False,neuralInferenceExecuted=False,runtimePredicateApproved=False)
args.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
