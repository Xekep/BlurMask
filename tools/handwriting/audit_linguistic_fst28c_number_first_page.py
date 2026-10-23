"""STATIC byte/raw-model first-normal-page proof, never actual query authority."""
import argparse,ast,hashlib,json,runpy,struct,sys,math
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--native-repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--d0-capture',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();repo=Path(__file__).resolve().parents[2];sha=lambda b:hashlib.sha256(b).hexdigest()
pins=json.loads(Path(__file__).with_name('linguistic-fst28c-number-first-page-pins.json').read_bytes())
def pe_reader(b):
 pe=struct.unpack_from('<I',b,60)[0];o=pe+24;sz=struct.unpack_from('<H',b,pe+20)[0];sections=[struct.unpack_from('<IIII',b,o+sz+i*40+8)for i in range(struct.unpack_from('<H',b,pe+6)[0])]
 def read(va,n):
  r=va-0x180000000;s,v,l,pos=next(x for x in sections if x[1]<=r<x[1]+x[2]);assert r+n<=v+l;return b[pos+r-v:pos+r-v+n]
 return read
native=(a.native_repo/'research/myscript/sdk/build/x64/MyScriptText.dll').read_bytes();assert sha(native)==pins['nativeSha256'];read=pe_reader(native)
for row in pins['regions']:assert sha(read(int(row['start'],16),row['bytes']))==row['sha256']
for row in pins['spans']:assert read(int(row['start'],16),row['bytes']).hex()==row['hex']and sha(bytes.fromhex(row['hex']))==row['sha256']
for row in pins['runtimeReviewedLF']+pins['existingIndependentDefinitionsLF']:assert sha((repo/row['path']).read_bytes().replace(b'\r\n',b'\n'))==row['sha256LF'],row['path']
e=pins['engineTypeAPI'];eb=(a.native_repo/'research/myscript/sdk/build/x64'/e['binary']).read_bytes();assert sha(eb)==e['sha256'];er=pe_reader(eb)
for row in e['tables']:assert er(int(row['at'],16),len(bytes.fromhex(row['bytes']))).hex()==row['bytes']
# Exact mode0 primary and namespace dispatcher leaves; slot18 is CLEANUP.
assert read(0x18023fff0,8).hex()=='4533c9e9f8f6ffff'and read(0x180236b30,8).hex()=='4533c9e9d8fdffff'
assert struct.unpack('<Q',read(0x1802e9288,8))[0]==0x18023f4f0
assert struct.unpack('<Q',read(0x1802e91b0+16,8))[0]==0x18023dfe0 and struct.unpack('<Q',read(0x1802e91b0+24,8))[0]==0
# Validate genuine upstreamNN/cache permissions rather than trust a capture winner/hash.
old=sys.argv;sys.argv=[str(repo/'tools/LegacyFst28CNumberInitialD0.Probe/audit_output.py'),str(a.models),str(a.d0_capture)]
try:runpy.run_path(sys.argv[0],run_name='__main__')
finally:sys.argv=old
d0=json.loads(a.d0_capture.read_bytes());assert len(d0['cases'])==4

def defs(path,names):
 ns={'struct':struct};nodes=[n for n in ast.parse(path.read_text(encoding='utf-8-sig')).body if isinstance(n,(ast.FunctionDef,ast.ClassDef))and n.name in names];assert len(nodes)==len(names);exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),ns);return ns
alphabet=defs(repo/'tools/LegacyFst28CPrimaryMap.Probe/audit_output.py',{'alphabet'})['alphabet'];parse=defs(repo/'tools/LegacyFst13AOutgoing.Probe/audit_output.py',{'parse'})['parse']
lk=(a.models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();ak=(a.models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert sha(lk)=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'and sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
sn,se,source,reverse=alphabet(lk,299110);an,ae,acoustic,_=alphabet(ak,99);assert(sn,se,an,ae)==(16,299573,192,5233)
lookup={v:k for k,v in acoustic.items()};preserved=[65535 if reverse[i]==65535 else 65533 if source[i]==b'\0'else 65531 if source[i]==b' 'else lookup.get(source[i],65535)for i in range(sn)]
keys=[v for v in preserved if v not in(65533,65535)];aux=[-1 if v in(65533,65535)else keys.index(v)for v in preserved];assert len(keys)==15 and aux[0]==-1 and aux[1]==0
assert lookup.get('\u200c'.encode(),65535)==65535 and lookup.get('\u200d'.encode(),65535)==184 and 184 not in preserved
name=bytes(lk[299000+24+i]^[0x14,0x12,0x19,0x77][i%4]for i in range(56)).split(b'\0',1)[0];assert name==b'ru_RU-lk-text.number'and name.rsplit(b'.',1)[1]==b'number'and len(b'number')==len(b'bullet')and b'number'!=b'bullet'
states,words,descriptors=parse(lk[299576:301580],299576);costs=runpy.run_path(str(repo/'tools/handwriting/reference_fst23c_outgoing.py'))['actual_main135_costs']();assert sha(struct.pack('<256i',*costs))=='6db9bc77ac744b8bb25b71d42d15f2ef088d9273b660b2950cb9bca788406ba3'
assert all(costs[r[2]+128]>=0 for r in descriptors[1:98])and costs[128]==0 and costs[179]==3454
assert max(descriptors[i+1][0]-descriptors[i][0]for i in range(98))==12

def raw_rows(state):
 return [[t,descriptors[t][1],costs[descriptors[t][2]+128],descriptors[t][2],descriptors[t][3]]for t in words[descriptors[state][0]:descriptors[state+1][0]]]
assert raw_rows(0)==[[97,0,3454,51,0]]and [r[:2]for r in raw_rows(97)]==[[65,0],[69,14],[70,15]]
digit_targets=words[descriptors[65][0]:descriptors[66][0]];assert [descriptors[t][1]for t in digit_targets]==list(range(1,11))
assert all(descriptors[t][3]==1 for t in digit_targets)and descriptors[69][3]==descriptors[70][3]==0
leaf_targets=digit_targets+[69,70];assert len(leaf_targets)==12 and all(costs[descriptors[t][2]+128]==0 for t in leaf_targets)
literal=runpy.run_path(str(Path(__file__).with_name('reference_fst28c_number_first_page.py')))['first_page']
# Independent flattening follows the RAW two-epsilon closure, not returned walker rows.
def flat(current):
 result=[]
 for target in leaf_targets:
  off,label,signed,byte7=descriptors[target];code=current[label]
  if code==65535:continue
  assert 0<=code<192
  result.append([target|(1<<32)|((byte7&1)<<41),aux[label],3454,code,0,0,0])
 return result
cases=[]
for i,row in enumerate(d0['cases']):
 current=row['primary'];assert current==[v if v>=192 or row['permissions'][v]&1 else 65535 for v in preserved]
 trace=literal(raw_rows,current,aux);assert trace['records']==flat(current)
 assert(trace['backendCalls'],trace['sourceV80Fills'],trace['sourceRowsCopied'],trace['sourceNextCursor'],trace['outerDepthUsed'],trace['contextNodesAdded'])==(6,3,14,3,3,0)
 assert [c['state']for c in trace['calls']]==[0,97,65,65,97,0]
 assert all(c['returned']==0 for c in trace['calls'][3:])
 assert [c['physical']for c in trace['cursorFinal']]==[1,3,10]
 assert trace['calls'][3]['skip']==trace['cursorFinal'][2]['accepted'] and trace['calls'][4]['skip']==trace['cursorFinal'][1]['accepted']
 trace['case']=i;trace['currentPrimary']=current;cases.append(trace)
assert [v['visible']for v in cases]==[len(flat(r['primary']))for r in d0['cases']]
# All4096 current permission combinations are STATIC synthetic diagnostics only.
# They never confer typed D0/request authority or tune model masks.
distribution=[0]*13;total_visible=0;total_backend=0;total_source=0
for bits in range(1<<12):
 current=preserved.copy()
 for k,label in enumerate(list(range(1,11))+[14,15]):
  if not bits>>k&1:current[label]=65535
 result=literal(raw_rows,current,aux);expected=flat(current);assert result['records']==expected and result['visible']==bits.bit_count()
 assert result['backendCalls']==6 and result['sourceRowsCopied']==14 and result['outerDepthUsed']==3
 distribution[result['visible']]+=1;total_visible+=result['visible'];total_backend+=result['backendCalls'];total_source+=result['sourceV80Fills']
assert distribution==[math.comb(12,i)for i in range(13)]and(total_visible,total_backend,total_source)==(24576,24576,12288)
negative={
 'root-is-not-visible':all(all(r[3]!=65533 for r in c['records'])for c in cases),
 'root97-not-costtag51':raw_rows(0)[0][0]!=raw_rows(0)[0][3],
 'costtable-not-signedtag':all(all(r[2]==3454 and r[2]!=51 for r in c['records'])for c in cases),
 'namespace-not-rootordinal5':all(all((r[0]>>32)&255==1 for r in c['records'])for c in cases),
 'tagbyte5-not-casebyte23':any((r[0]>>40)&255!=r[5]for c in cases for r in c['records']),
 'initial-rootbit40-not-propagated':all(all((r[0]>>40)&1==0 for r in c['records'])for c in cases),
 'physical-not-accepted':any(c['cursorFinal'][2]['physical']!=c['cursorFinal'][2]['accepted']for c in cases),
 'sourceaux-zero-slot-not-null':aux[1]==0 and aux[0]==-1,
 'nonnull-aux-payload-may-be-null':len(keys)==15,
 'no-prefixguard-on-bit43-alone':not(0<0 or (0&0x81)==1),
 'current-map-not-preserved':any(c['visible']!=12 for c in cases),
 'constructor-root-cache-not-reused':all(c['sourceV80Fills']==3 for c in cases),
}
assert all(negative.values())
# Exact NEXT proposal bounds; no runtime yet. Structured typed-row cache includes
# byte7 metadata to avoid re-querying/interpreting raw descriptor bytes for V70.
B=8*(80+16+128*32)+8*(80+12*(16+2))+12*16+128*32+6*64+3*64+16*(2+8)+8*32+80+1024
W=B+16*32+8*80+3*(12*16+64)+6*(128*64+128)+8*(128*64+64)+128*32+6*64+512
report={'scope':'STATIC predicted first normal page only; not issued C#query/numberOCR','regions':len(pins['regions']),'spans':len(pins['spans']),'runtimeLF':len(pins['runtimeReviewedLF']),'sourceName':name.decode(),'sourceCurrentPrimaryCases':cases,'diagnostic4096':{'distribution':distribution,'visibleRows':total_visible,'backendCalls':total_backend,'sourceFills':total_source},'staticCounterexamples':negative,'sourceAuxCount':15,'liveContextNodes':18,'noContextAllocations':True,'proposedLogicalBytes':B,'proposedWork':W,'runtimeApproved':False,'vendorExecuted':False,'d0CaptureSha256':sha(a.d0_capture.read_bytes())}
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(report,indent=2)+'\n');print('PASS STATIC',len(pins['regions']),'regions',len(pins['spans']),'spans; firstpage predictions',[c['visible']for c in cases],'; all4096 masks/24576 visible; B/W',B,W)
