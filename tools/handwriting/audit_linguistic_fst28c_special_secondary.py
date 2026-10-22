"""STATIC target special/range fields and fresh same-knowledge C4 lineage.
Never loads vendor binaries or grants cursor/history/recognition authority.
"""
import argparse,ast,hashlib,json,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
sha=lambda b:hashlib.sha256(b).hexdigest();root=Path(__file__).resolve().parents[2]
old=json.loads(Path(__file__).with_name('linguistic-fst28c-primary-map-pins.json').read_bytes());new=json.loads(Path(__file__).with_name('linguistic-fst28c-special-secondary-pins.json').read_bytes())
readers={};functions=set();regions=0;spans=0
for package in[old,new]:
 for module in package['modules']:
  raw=(a.sdk/module['binary']).read_bytes();assert sha(raw)==module['sha256'];pe=struct.unpack_from('<I',raw,60)[0];op=pe+24;base=struct.unpack_from('<Q',raw,op+24)[0];at=op+struct.unpack_from('<H',raw,pe+20)[0];sections=[]
  assert raw[pe:pe+4]==b'PE\0\0'and struct.unpack_from('<H',raw,op)[0]==0x20b
  for i in range(struct.unpack_from('<H',raw,pe+6)[0]):_,v,n,f=struct.unpack_from('<IIII',raw,at+i*40+8);sections.append((v,n,f))
  def make_reader(raw,base,sections):
   def read(at,n):
    rva=at-base
    for v,size,f in sections:
     if v<=rva and 0<=n<=size-(rva-v):return raw[f+rva-v:f+rva-v+n]
    raise AssertionError(('PE range',hex(at),n))
   return read
  read=make_reader(raw,base,sections);readers[module['binary']]=read
  for row in module['regions']:
   start=int(row['start'],16);end=int(row['endExclusive'],16);assert end-start==row['bytes']and sha(read(start,end-start))==row['sha256'];functions.add((module['binary'],row['function']));regions+=1
  for row in module['tables']:value=bytes.fromhex(row['bytes']);assert read(int(row['at'],16),len(value))==value;spans+=1
t=readers['MyScriptText.dll'];q=lambda at:struct.unpack('<Q',t(at,8))[0]
assert q(0x180274aa0+24)==0x180274b68 and q(0x180274b68+16)==0x180275220 and q(0x180275220)==0x18000c860
assert[q(0x180274af0+at)for at in[16,24]]==[0x18000caa0,0x18000cab0]
assert q(0x1802e7ab8+8)==0x1802e7cb0 and q(0x1802e7cb0+24)==0x18020c1c0
assert q(0x1802e8f40+16)==0x1802e8f78 and q(0x1802e8f78+24)==0x18014fa70
assert q(0x1802e8cf0+16)==0x1802e8d88 and q(0x1802e8d88+16)==0x180237110
assert struct.unpack('<Q',readers['MyScriptEngine.dll'](0x1801db830+32,8))[0]==0x180037e90
assert t(0x18014fa70,7)==bytes.fromhex('4c894208b001c3')
assert q(0x1802e8f40+8)==0x1802e8fa0 and q(0x1802e8fa0+16)==0x18023bcf0
assert q(0x1802e8cf0+8)==0x1802e8c60 and q(0x1802e8c60+16)==0x180237380
assert q(0x1802e8b28+8)==0x1802e8bb0 and q(0x1802e8bb0+16)==0x180236490
assert q(0x1802e9270+16)==0x18023e380 and q(0x1802badf0+0x58)==0x18004a2c0
lk=(a.models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(a.models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert sha(lk)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'and sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
tree=ast.parse(Path(__file__).with_name('audit_linguistic_fst28c_primary_map.py').read_text(encoding='utf8'));namespace={'struct':struct}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef)and n.name=='alphabet'],type_ignores=[]),'raw-v103-alphabet','exec'),namespace)
source=namespace['alphabet'](lk,299110);acoustic=namespace['alphabet'](ak,99)
assert(source['count'],source['end'],acoustic['count'],acoustic['end'])==(16,299573,192,5233)
lookup={v:k for k,v in acoustic['tokens'].items()};fields={};lookup_fields=[('A0',0x1802eb360,4),('A8',0x1802eb368,4),('AA',0x1802eb370,4),('AC',0x1802eb378,4),('AE',0x1802eb380,4),('B0',0x1802eb388,3),('B2',0x1802ecbec,2),('B4',0x1802e9bc0,2)]
for slot,address,length in lookup_fields:fields[slot]=lookup.get(t(address,length),65535)
#Engine053F20 reads raw H fields0..6 into runtime+8,+A,+C,+E,+10,+12,+14.
for slot,index in[('A2',4),('A4',5),('A6',6)]:fields[slot]=acoustic['header'][index]
range_checks=[]
def contiguous(table,characters):
 base=table.get(characters[0].encode(),65535);checks=0
 for i,char in enumerate(characters[1:],1):
  checks+=1
  if table.get(char.encode(),65535)!=base+i:return 65535,checks
 return base,checks
for slot,chars in[('B6','0123456789'),('B8','abcdefghijklmnopqrstuvwxyz'),('BA','ABCDEFGHIJKLMNOPQRSTUVWXYZ')]:
 base,checked=contiguous(lookup,chars);fields[slot]=base
 changed=dict(lookup);changed[chars[-1].encode()]=65535
 refused,_=contiguous(changed,chars);assert refused==65535
 range_checks.append(dict(slot=slot,base=base,adjacentChecks=checked,missingFinalSymbolRefuses=True))
primary=[]
for label in range(16):
 token=source['tokens'][label];primary.append(65535 if source['reverse'][label]==65535 else 65533 if token==b'\0'else 65531 if token==b' 'else lookup.get(token,65535))
def secondary(c4):
 return[code if code>=192 and(not c4 or((code+7)&0xfffd)!=0)else 65535 for code in primary]
fresh=secondary(False);counterfactual=secondary(True);assert fresh[0]==65533 and fresh[11]==65531 and all(value==65535 for i,value in enumerate(fresh)if i not in[0,11])
assert[i for i,(x,y)in enumerate(zip(fresh,counterfactual))if x!=y]==[11]
report=dict(status='PASS STATIC14targetfields/3rangeguards/freshC4lineage',newCompleteFunctions=sum(len(m['regions'])for m in new['modules']),composedUniqueFunctions=len(functions),composedRegionChecks=regions,dataAndLeafSpanChecks=spans,targetFields=dict(sorted(fields.items())),targetFieldRawSHA256=sha(struct.pack('<14H',*(fields[format(i,'02X')]for i in range(0xa0,0xbc,2)))),rangeChecks=range_checks,secondaryFreshC4Zero=fresh,secondaryRawSHA256=sha(struct.pack('<16H',*fresh)),counterfactualC4OneOnlyChangedSourceLabel=11,actualC4Producer='00C860 clears knowledge+C4;00CAA0 frontend+8 same knowledge;20C1C0 binds controller+20 same frontend;20D8F0 passes frontend through237380/23BCF0/236490/23E380;237110 via Engine037E90 actual28E initlist.V18=14FA70 stores actual inner+8;no property events',freshKnowledgeReceiptImplemented=False,sourceV58=True,auxiliaryAssumedEmpty=False,cursorsOrHistoryImplemented=False,nativeExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n');print(f'PASS STATIC {len(functions)} composedunique functions/{regions} regions/{spans} spans;14 target fields/59 adjacency checks/3 refusal witnesses; samefreshknowledgeC4=0, noquery/history')
