"""Static-only repeated2144 gate,207CB0 cursor and same-source epoch proof."""
import json,subprocess,sys,tempfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
if len(sys.argv)!=2:raise SystemExit('output.json')
output=Path(sys.argv[1]).resolve()
with tempfile.TemporaryDirectory(dir=output.parent,prefix='repeat-live-static-') as directory:
 previous=Path(directory)/'live.json'
 subprocess.run([sys.executable,str(ROOT/'tools/handwriting/audit_main_global_live_publication.py'),str(previous)],check=True)
 report=json.loads(previous.read_bytes())
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary()
pins=[
 (0x180214826,'4181fc50c30000'), #actual native source-node ceiling50000.
 (0x18021482d,'7e27'),
 (0x1802085b5,'453bec'),       #source cursor compared to actual requested end.
 (0x1802085b8,'0f8f41340000'), #completed interval goes to sentinel/cursor writes.
 (0x18020b953,'448b6d88'),
 (0x18020b957,'41ffc5'),       #increment source cursor once for each node.
 (0x18020b95d,'44896d88'),
 (0x18020b969,'0f8f90000000'),
 (0x18020b9ff,'498b8e78240000'),
 (0x18020ba09,'418b8670240000'),
 (0x18020ba17,'890491'),       #2478[end+1]=retained global2470.
 (0x18020ba1a,'418d4dff'),
 (0x18020ba25,'4589ae90240000'),#2490=end+1, used by next2144 gate.
 (0x18020ba2c,'890c90'),       #2488[end+1]=end.
 (0x180215dcb,'038e90240000'), #same persistent2490 read every loop, no cycle counter.
 (0x180215dde,'e8cd1effff'),
 (0x180215e92,'41ffd2'),       #same sourceV80 follows publication.
 (0x180215e95,'ffc7'),         #next logical node.
]
for address,raw in pins:assert b.data[b.off(address):b.off(address)+len(raw)//2].hex()==raw,hex(address)
def gates(count):
 cursor=0;result=[]
 for node in range(count):
  if node+1<count and 120+cursor<=node+1:result.append([cursor,node]);cursor=node+1
 return result,cursor
for count in (1,119,120,121,239,240,241,244,359,360,361,364,479,480,481,484,50000):
 actual,cursor=gates(count);assert len(actual)==(count-1)//120 and cursor==len(actual)*120
 assert all(a==120*i and z==120*(i+1)-1 for i,(a,z) in enumerate(actual))
report.update(repeatedPins=len(pins),nativeNodeCeiling=50000,
 actualRuntime='Named budgeted repeated owner; same graph/dictionary/persistent290/pool. Old public one-cycle APIs stay strict.',
 portableBounds='Max logicalnodes, cumulative requested controller payload, fixed compact pool bytes; not RSS. No guessed physical ring aliases.',
 arithmeticBoundaryCounts=[1,119,120,121,239,240,241,244,359,360,361,364,479,480,481,484,50000],
 excluded=['associated/customV58','physical1FA300 ring aliases','guaranteed winner or accepted text','handwriting quality'])
output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(f'PASS repeated LIVE {len(pins)} added exact pins/17 gate boundaries; retained same-source proof.')
