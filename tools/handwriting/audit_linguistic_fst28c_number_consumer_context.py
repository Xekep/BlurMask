"""STATIC number1FD240 context prerequisite. No native/new graft execution.
--native-repo REPO --models MODELS --page-capture PAGE --d0-capture D0 --output REPORT
"""
import argparse,ast,hashlib,json,runpy,struct,sys
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--native-repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--page-capture',type=Path,required=True);p.add_argument('--d0-capture',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();repo=Path(__file__).resolve().parents[2];sha=lambda b:hashlib.sha256(b).hexdigest()
pins=json.loads(Path(__file__).with_name('linguistic-fst28c-number-consumer-context-pins.json').read_bytes());b=(a.native_repo/'research/myscript/sdk/build/x64/MyScriptText.dll').read_bytes();assert sha(b)==pins['nativeSHA256']
pe=struct.unpack_from('<I',b,60)[0];o=pe+24;sz=struct.unpack_from('<H',b,pe+20)[0];sections=[struct.unpack_from('<IIII',b,o+sz+i*40+8)for i in range(struct.unpack_from('<H',b,pe+6)[0])]
def read(va,n):
 r=va-0x180000000;s,v,l,pos=next(x for x in sections if x[1]<=r<x[1]+x[2]);assert r+n<=v+l;return b[pos+r-v:pos+r-v+n]
for row in pins['regions']:assert sha(read(int(row['start'],16),row['bytes']))==row['sha256']
for row in pins['spans']:assert read(int(row['start'],16),row['bytes']).hex()==row['hex']and sha(bytes.fromhex(row['hex']))==row['sha256']
for row in pins['sourceMetadataLeaves']:assert read(int(row['start'],16),len(bytes.fromhex(row['bytes']))).hex()==row['bytes']
for row in pins['sourceMetadataSlots']:assert read(int(row['at'],16),8).hex()==row['bytes']
for row in pins['runtimeLF']:assert sha((repo/row['path']).read_bytes().replace(b'\r\n',b'\n'))==row['sha256LF']
assert read(0x180236820,6).hex()=='8b4258ffc0c3'
assert struct.unpack('<Q',read(0x1802e8bd0,8))[0]==0x180236820 and struct.unpack('<Q',read(0x1802e8bd8,8))[0]==0x180236830
# Independent raw page/NN proof; no graft or new factory runs.
old=sys.argv;sys.argv=[str(repo/'tools/LegacyFst28CNumberInitialPage.Probe/audit_output.py'),str(a.models),str(a.page_capture),str(a.d0_capture)]
try:upstream=runpy.run_path(sys.argv[0],run_name='__main__')
finally:sys.argv=old
page=json.loads(a.page_capture.read_bytes());assert len(page['cases'])==4

def defs(path,names):
 ns={'struct':struct};nodes=[n for n in ast.parse(path.read_text(encoding='utf-8-sig')).body if isinstance(n,(ast.FunctionDef,ast.ClassDef))and n.name in names];assert len(nodes)==len(names);exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),ns);return ns
alphabet=defs(repo/'tools/LegacyFst28CPrimaryMap.Probe/audit_output.py',{'alphabet'})['alphabet']
avl=defs(repo/'tools/LegacySymbolContext.Probe/reference_controls.py',{'Node','height','update','left','right','add','case'})
lk=(a.models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(a.models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();sn,se,source,reverse=alphabet(lk,299110);an,ae,acoustic,_=alphabet(ak,99);assert(sn,se,an,ae)==(16,299573,192,5233)
lookup={v:k for k,v in acoustic.items()};preserved=[65535 if reverse[i]==65535 else 65533 if source[i]==b'\0'else 65531 if source[i]==b' 'else lookup.get(source[i],65535)for i in range(sn)]
keys=[v for v in preserved if v not in(65533,65535)];prefix=[65531,65529,65531,65532,65530];initialAllocation=list(dict.fromkeys(keys+prefix));assert len(initialAllocation)==18
fffa=initialAllocation.index(65530);initialTree=avl['case'](keys+prefix);initialByKey={r[0]:r for r in initialTree['nodes']};initialSlots={key:i for i,key in enumerate(initialAllocation)}
assert fffa==17
# Same fixed native13A.V58=true binding from the inherited initialization
# certificate; negative-cost bit rederived from ALL actual raw target descriptors.
negativeCosts=any(upstream['costs'][d[2]+128]<0 for d in upstream['desc'][1:98])
sourceHeader=1|(int(negativeCosts)<<1)
assert sourceHeader==1
predictions=[]
for i,row in enumerate(page['cases']):
 symbols=[r[3]for r in row['records']];assert len(symbols)<=12 and len(set(symbols))==len(symbols)and all(0<=s<192 for s in symbols)
 assert all(r[4:]==[0,0,0]and(r[0]>>32)&255==1 and r[1]>=0 for r in row['records'])
 childTree=avl['case'](symbols);childByKey={r[0]:r for r in childTree['nodes']};childSlots={key:18+k for k,key in enumerate(symbols)}
 nodes=[]
 for k,key in enumerate(initialAllocation):
  _,left,right,balance=initialByKey[key]
  nodes.append([k,key,-1 if left<0 else initialSlots[left],-1 if right<0 else initialSlots[right],balance,-1,(-1 if not symbols else childSlots[childTree['root']])if k==fffa else -1,0])
 for key in symbols:
  _,left,right,balance=childByKey[key];nodes.append([childSlots[key],key,-1 if left<0 else childSlots[left],-1 if right<0 else childSlots[right],balance,fffa,-1,0])
 assert len(nodes)==18+len(symbols)<=30 and all(n[7]==0 for n in nodes)
 grafts=[[k,r[1],childSlots[r[3]],r[3],fffa]for k,r in enumerate(row['records'])]
 assert len(grafts)==8 and len(nodes)==26
 # Existing source nodes retain NULL parent. The FFFA child is NEW, not a
 # replacement source ID or copied historical page context.
 assert all(nodes[g[1]][5]==-1 and nodes[g[2]][5]==fffa and g[1]!=g[2]for g in grafts)
 predictions.append({'case':i,'grafts':grafts,'activeNodes':nodes,'initialNodeCount':18,'newChildren':len(symbols),'activeNodeCount':len(nodes),'sourceNamespace':1,'numberRootOrdinal':5,'sourceHeader':sourceHeader,'sourceKind40':(sourceHeader>>9)&15,'sourceAuxCount':15,'historicalPageUnchanged':True})
# Numeric controls only; no typed D0/pool/page authority granted.
prepared=[]
for symbols in [[],list(range(12)),list(reversed(range(12))),[0],list(range(8))]:
 child=avl['case'](symbols);assert len(child['nodes'])==len(symbols);needed=18+len(symbols);assert needed<=30<102
 prepared.append({'symbols':symbols,'neededNodes':needed,'old18CanAllocateAll':needed<=18,'reserved30CanAllocateAll':needed<=30})
assert prepared[0]['old18CanAllocateAll']and not prepared[1]['old18CanAllocateAll']
B=8954+42288+12*64+12*56+30*64+1024
W=22202+165168+4384+12*30*64+30*30*8+12*64+4096
assert(B,W)==(55626,226858)==(pins['proposedByteBound'],pins['proposedWorkBound'])
negative={'historic18insufficient':all(v['activeNodeCount']>18 for v in predictions),'rootOrdinalIsNotNamespace':all(v['numberRootOrdinal']!=v['sourceNamespace']for v in predictions),'EnglishHeaderNotNumber':all(v['sourceHeader']!=0xc0e and v['sourceKind40']==0 for v in predictions),'sourceIDsNotCloned':all(all(g[1]!=g[2]for g in v['grafts'])for v in predictions)}
assert all(negative.values())
report={'scope':'STATIC actual1FD240 context-graft prerequisite ONLY; not executed','regions':len(pins['regions']),'spans':len(pins['spans']),'metadataLeaves':len(pins['sourceMetadataLeaves']),'metadataSlots':len(pins['sourceMetadataSlots']),'runtimeLF':len(pins['runtimeLF']),'predictions':predictions,'preparedOnlyContextControls':prepared,'staticDiscriminators':negative,'excludedClaims':['query-parent authority','source-history inference','full1FD240 acceptance'],'proposedBytes':B,'proposedWork':W,'nativePageBytes':4080,'nativeNodeBytes':40,'nativePageSlots':102,'runtimeApproved':False,'vendorExecuted':False,'pageCaptureSHA256':sha(a.page_capture.read_bytes())}
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(report,indent=2)+'\n');print('PASS STATIC',len(pins['regions']),'native regions/3metadata leaves/2slots/7runtimeLF;4predictions8children/26nodes; B/W',B,W,'; no graft execution or history authority')
