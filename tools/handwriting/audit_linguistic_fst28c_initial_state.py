"""STATIC target special/range fields and fresh same-knowledge C4 lineage.
Never loads vendor binaries or grants cursor/history/recognition authority.
"""
import argparse,ast,hashlib,json,runpy,struct,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
from capstone.x86_const import X86_OP_MEM,X86_OP_IMM,X86_REG_RDX
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);p.add_argument('--actual-outgoing-capture',type=Path,required=True);a=p.parse_args()
sha=lambda b:hashlib.sha256(b).hexdigest();root=Path(__file__).resolve().parents[2]
old=json.loads(Path(__file__).with_name('linguistic-fst28c-primary-map-pins.json').read_bytes());new=json.loads(Path(__file__).with_name('linguistic-fst28c-special-secondary-pins.json').read_bytes())
initial=json.loads(Path(__file__).with_name('linguistic-fst28c-initial-state-pins.json').read_bytes())
readers={};functions=set();regions=0;spans=0
for package in[old,new,initial]:
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

# Exact inherited reader sequence:13A202 ->139301 ->138100 ->133100.
# Decode only the authenticated stored name. No licensing/activation semantics
# or acceptance result is implemented by this diagnostic.
child=299000;position=child+16
version139,version138=struct.unpack_from('<HH',lk,position);position+=4
assert(version139,version138)==(0x301,0x100)
raw10=struct.unpack_from('<I',lk,position)[0];position+=4
raw_name=lk[position:position+56];position+=56
sentinel=struct.unpack_from('<i',lk,position)[0];position+=4
if sentinel==-1:raw4c=struct.unpack_from('<I',lk,position)[0];position+=4
else:raw4c=sentinel&0xffffffff
raw50=struct.unpack_from('<I',lk,position)[0];position+=4
key=bytes.fromhex('14121977');decoded=bytes(v^key[i%4]for i,v in enumerate(raw_name))if sentinel==-1 else raw_name
name=decoded.split(b'\0',1)[0];assert position==299092 and name==b'ru_RU-lk-text.number'
#Actual23E0C0 dot-suffix branch: number is NOT the no-dot mul-lk-number
#shortcut, nor any recognized dot-suffix in this concrete body.
suffix=name.rsplit(b'.',1)[-1]
assert b'.'in name and suffix not in[b'ool_lower',b'ool_upper',b'ool_latin',b'ool_any',b'email',b'uri',b'bullet',b'emoticon',b'emoji']
assert q(0x1802badf0+0x78)==0x18010bab0 and q(0x1802badf0+0x70)==0x18010ba90 and q(0x1802badf0+0x60)==0x18010ba30
for typ,descriptor,table in[(0x133,0x1802e54b0,0x1802e54f8),(0x138,0x1802bb240,0x1802bb2c0),(0x139,0x1802e5c30,0x1802e5d40)]:
 metadata=q(descriptor+24);assert q(descriptor+8)&0xffffffff==typ and q(metadata+8)==table
assert struct.unpack('<Q',readers['MyScriptEngine.dll'](0x1801db830+0x28,8))[0]==0x180037ef0
costs=runpy.run_path(str(Path(__file__).with_name('reference_fst23c_outgoing.py')))['actual_main135_costs']()
firsts=[struct.unpack_from('<I',lk,299588+i*8)[0]for i in range(99)]
capacity=struct.unpack_from('<I',lk,300380)[0];degrees=[b-a for a,b in zip(firsts,firsts[1:])]
assert capacity==12 and degrees[0]==1 and max(degrees)==capacity
# REUSE original independent outgoing parse, including narrow raw4/5/6
# ->runtime label/byte6/byte7. Raw7 is opaque padding, NOT raw5.
outgoing_oracle=Path(__file__).resolve().parents[1]/'LegacyFst13AOutgoing.Probe/audit_output.py'
original=ast.parse(outgoing_oracle.read_text(encoding='utf8'));raw_namespace={'struct':struct}
exec(compile(ast.Module(body=[n for n in original.body if isinstance(n,ast.FunctionDef)and n.name=='parse'],type_ignores=[]),'original-independent-outgoing-parse','exec'),raw_namespace)
parsed_states,parsed_words,parsed_descriptors=raw_namespace['parse'](lk[299576:301580],299576)
wire_report=a.output.with_name(a.output.stem+'-original-wire.json');saved=sys.argv[:]
try:
 sys.argv=['audit_linguistic_fst23c_wire.py','--sdk',str(a.sdk),'--models',str(a.models),'--output',str(wire_report)]
 runpy.run_path(str(Path(__file__).with_name('audit_linguistic_fst23c_wire.py')),run_name='__main__')
finally:sys.argv=saved
wire_proof=json.loads(wire_report.read_bytes())
for parsed,wire in zip(parsed_descriptors,wire_proof['descriptors']):
 raw_tag=wire['runtimeByte6'];signed=raw_tag if raw_tag<128 else raw_tag-256
 assert parsed==(wire['field0'],wire['runtimeLabel16'],signed,wire['runtimeByte7'])

assert parsed_states==98
source_has_negative_cost=any(costs[d[2]+128]<0 for d in parsed_descriptors[1:98])
assert not source_has_negative_cost
slots=[];source_slots=[]
for label,code in enumerate(primary):
 if code in(65535,65533):source_slots.append(None);continue
 slot=next((i for i,row in enumerate(slots)if row['key']==code),None)
 if slot is None:slot=len(slots);slots.append(dict(key=code,insertedAtSourceLabel=label,historyValue=None,secondaryValue=None))
 source_slots.append(slot)
assert len(slots)==15 and source_slots==[None]+list(range(15))
assert b'\xe2\x80\x8d'not in source['tokens'].values()
root_rows=[]
for ordinal,i in enumerate(range(firsts[0],firsts[1])):
 target=parsed_words[i];_,label,signed,flag=parsed_descriptors[target]
 root_rows.append(dict(ordinal=ordinal,target=target,sourceLabel=label,primary=primary[label],secondary=fresh[label],cost=costs[signed+128],targetSignedByte6=signed,targetByte7=flag,auxiliarySlot=source_slots[label]))
assert root_rows==[dict(ordinal=0,target=97,sourceLabel=0,primary=65533,secondary=65533,cost=3454,targetSignedByte6=51,targetByte7=0,auxiliarySlot=None)]
md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True
reset_writes=[]
for ins in md.disasm(t(0x18023ded0,267),0x18023ded0):
 if ins.mnemonic=='mov'and len(ins.operands)==2 and ins.operands[0].type==X86_OP_MEM and ins.operands[1].type==X86_OP_IMM:
  destination,value=ins.operands
  assert destination.mem.base==X86_REG_RDX and destination.mem.index==0
  reset_writes.append((destination.mem.disp,destination.size,value.imm&((1<<(destination.size*8))-1)))
expected_reset=[]
for i in range(8):
 base=0xc8+i*0x50;expected_reset.extend([(base,4,0xffffffff),(base+0x38,8,0xffffffffffffffff),(base+0x48,8,0xffffffffffffffff)])
expected_reset.append((0x348,4,0))
assert reset_writes==expected_reset
actual_capture=json.loads(a.actual_outgoing_capture.read_bytes())
actual_source=next(row for row in actual_capture['sources']if row['name']=='actual-number')
assert actual_source['actualAuthority']and actual_source['rootReceipt']and actual_source['childReceipt']
assert actual_source['costs']==costs and actual_source['states'][0]['rows']==[[97,0,3454,51,0]]
assert actual_source['states'][0]['rows']==[[r['target'],r['sourceLabel'],r['cost'],r['targetSignedByte6'],r['targetByte7']]for r in root_rows]
cursors=[dict(index=i,base=0xc8+i*0x50,state=0 if i==0 else -1,savedContext0=-1,savedContext1=-1,count=1 if i==0 else 0,physicalOffset=0 if i==0 else -1,acceptedOffset=0 if i==0 else -1)for i in range(8)]
#Negative independent controls distinguish the three concepts and preserve
#nonempty auxiliary structure despite all fresh history values beingNULL.
assert capacity!=degrees[0] and slots and all(s['historyValue']is None for s in slots)
assert len({c['base']for c in cursors})==8 and cursors[-1]['base']+0x50==0x348
report.update(status='PASS STATIC actual initial15aux/8cursor/state0prefix/name closure; no runtime query/history',initialFullFunctions=len(initial['modules'][0]['regions']),composedUniqueFunctions=len(functions),composedRegionChecks=regions,dataAndLeafSpanChecks=spans,
 sourceName=dict(decodedASCII=name.decode('ascii'),rawOffset=299024,rawBytes=56,sentinel=sentinel,raw10=raw10,raw4C=raw4c,raw50=raw50,opaqueAcceptanceNotImplemented=True,namePolicyBits=0),
 allocationCapacity=capacity,initialState=0,initialDegree=degrees[0],maxObservedDegree=max(degrees),sourceV60HasNegativeCost=source_has_negative_cost,sourceV58=True,sourceZWJLabel=65535,initialCallback='180240010',initialAlternative=-1,nextCursor=0,decodedResetStores=reset_writes,cursors=cursors,auxiliarySlots=slots,sourceLabelToAuxiliarySlot=source_slots,auxiliaryIsEmpty=False,rootOutgoing=root_rows,
 runtimeImplemented=False,historyPayloadAvailable=False,query28CReady=False,supersedesIncorrectRaw5PaddingInterpretation=True,actualTypedSourceRootRow=actual_source['states'][0]['rows'],actualOutgoingCaptureSHA256=sha(a.actual_outgoing_capture.read_bytes()),originalWireReportSHA256=sha(wire_report.read_bytes()),rootDescriptorRawHex=wire_proof['descriptors'][97]['rawHex'],rawProjection=dict(labelOffset=4,runtimeByte6FromRawOffset=5,runtimeByte7FromRawOffset=6,opaquePaddingOffset=7))
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='ascii')
print(f'PASS STATIC{len(functions)} unique full functions/{regions} regions/{spans} spans;15nonnull aux keyslots/8cursor reset states/state0degree1 vs capacity12; actualdecoded name; noquery/history runtime')
