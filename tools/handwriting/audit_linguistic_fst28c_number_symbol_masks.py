from pathlib import Path
import ast,struct,json,hashlib
import argparse,copy,runpy
p=argparse.ArgumentParser();p.add_argument('--native-repo',required=True,type=Path);p.add_argument('--models',required=True,type=Path);p.add_argument('--output',required=True,type=Path);a=p.parse_args()
R=Path(__file__).resolve().parents[2];ak=(a.models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();lk=(a.models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();pos=189734+20
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
pins=json.loads((R/'tools/handwriting/linguistic-fst28c-number-symbol-masks-pins.json').read_text())
native=(a.native_repo/'research/myscript/sdk/build/x64/MyScriptText.dll').read_bytes()
assert hashlib.sha256(native).hexdigest()==pins['nativeSha256']
pe=struct.unpack_from('<I',native,60)[0];assert native[pe:pe+4]==b'PE\0\0';o=pe+24;base=struct.unpack_from('<Q',native,o+24)[0];n=struct.unpack_from('<H',native,pe+6)[0];z=struct.unpack_from('<H',native,pe+20)[0];sections=[struct.unpack_from('<IIII',native,o+z+i*40+8)for i in range(n)]
def native_bytes(address,count):
 rva=address-base
 for vs,r,sz,at in sections:
  if r<=rva and count<=r+sz-rva:return native[at+rva-r:at+rva-r+count]
 raise AssertionError('Unbacked native span')
binding=runpy.run_path(str(R/'tools/handwriting/audit_legacy_linguistic_mask_binding.py'))
binding_report=binding['audit'](native,ak)
for row in pins['regions']+pins['spans']:
 raw=native_bytes(int(row['start'],16),row['bytes']);assert hashlib.sha256(raw).hexdigest()==row['sha256']
 if 'hex'in row:assert raw.hex()==row['hex']


def get(fmt,n=1):
 global pos
 z=struct.calcsize('<'+fmt)*n;assert 0<=n<=1048576 and pos+z<=572573;v=struct.unpack_from('<'+fmt*n,ak,pos);pos+=z;return v[0]if n==1 else list(v)
def skip(n):get('B',n)
def flag():
 x=get('B');assert x in [0,1];return x
def network():
 global pos
 magic,version,length=get('H'),get('H'),get('I');assert(magic,version)==(0xfea0,0x106);skip(length)
def shorts(copies):
 n=get('H');assert n<=512;skip(2*n*copies)
assert flag()==1;network();skip(3*4+2+2+4+1);selector=get('B');skip(2);assert flag()==1;get('H');skip(9*4);assert get('B')==1;assert flag()==0;skip(2+4+5*2+7*4);shorts(1);shorts(2);shorts(2);skip(2+2+4+4+2+2);assert flag()==0
if flag():shorts(2)
for slot in range(7):
 if flag():shorts(2)
 first,second,mode=flag(),flag(),get('B');assert mode<=2
 if mode==0:get('f')
 for n in range(3):
  if flag():network()
prefix=pos;skip(1+4+1+8);lists=get('H')
for _ in range(lists):n=get('H');skip(n*2)
skip(6*4);assert flag()==0;skip(8);assert flag()==0;group_count=get('h');assert 0<=group_count<=512
ends=get('H',group_count+1) if group_count else [0];symbols=get('H',ends[-1]) if ends[-1] else [];weight=get('f') if group_count else 0;assert ends[0]==0 and all(x<=y for x,y in zip(ends,ends[1:]))and all(x<192 for x in symbols);groups=[symbols[ends[i]:ends[i+1]]for i in range(group_count)]
parser=next(n for n in ast.parse((R/'tools/LegacyFst28CPrimaryMap.Probe/audit_output.py').read_text()).body if isinstance(n,ast.FunctionDef)and n.name=='alphabet');ns={'struct':struct};exec(compile(ast.Module(body=[parser],type_ignores=[]),'originalAL','exec'),ns)
cn,ce,source,_=ns['alphabet'](lk,299110);an,ae,acoustic,reverse=ns['alphabet'](ak,99);assert(cn,ce,an,ae)==(16,299573,192,5233);lookup={v:k for k,v in acoustic.items()};preserved=[65533 if source[i]==b'\0'else 65531 if source[i]==b' 'else lookup.get(source[i],65535)for i in range(16)];seed={i for i in preserved if i<192};primary=set(seed);secondary=set(range(192))
for g in groups:
 if weight>0 and primary.intersection(g):primary.update(g)
for text in [b'-',b'\xc2\xad']:
 if text in lookup:primary.add(lookup[text])
virtual={i for i,v in enumerate(reverse)if v==65535};primary-=virtual;secondary-=virtual;header=struct.unpack_from('<8H',ak,99)
for mask in [primary,secondary]:
 dash=[b'-',b'_',b'\xf3\xb0\x80\x82',b'\xf3\xb0\x80\x83',b'\xe3\x83\xbc',b'\xe4\xb8\x80']
 if header[1]!=65535:
  if any(lookup[t]in mask for t in dash if t in lookup):mask.add(header[1])
  else:mask.discard(header[1])
 if header[2]!=65535:
  if lookup.get(b'\xf3\xb0\x80\x81')in mask:mask.add(header[2])
  else:mask.discard(header[2])
 for s in [3,0,4,5,6]:
  if header[s]!=65535:mask.add(header[s])
sha=lambda x:hashlib.sha256(x).hexdigest();j={'scope':'Prepared numerical reference derived from exact number source; no runtime owner/query/D0 authority','prefixEnd':prefix,'knowledgeEnd':pos,'knowledgeCount':group_count,'knowledgeEntries':len(symbols),'weight':weight,'groupEnds':ends,'groups':groups,'preserved':preserved,'seed':[int(i in seed)for i in range(192)],'primary':[int(i in primary)for i in range(192)],'secondary':[int(i in secondary)for i in range(192)],'seedCount':len(seed),'primaryCount':len(primary),'secondaryCount':len(secondary),'seedSha256':sha(bytes(int(i in seed)for i in range(192))),'primarySha256':sha(bytes(int(i in primary)for i in range(192))),'secondarySha256':sha(bytes(int(i in secondary)for i in range(192))),'selector':selector}
assert(prefix,pos,group_count,len(symbols),selector)==(566330,566382,0,0,2)
# Independently authenticate script tables and rule out contextual/wide additions.
pub=json.loads((a.models/'text-unicode/legacy-text-publication-data.json').read_bytes());h=hashlib.sha256()
def hi(v):h.update(struct.pack('<i',v))
for name in ['scriptRanges','controlRanges','extendRanges','prependRanges','joinerRanges']:
 rows=pub[name];hi(len(rows))
 for row in rows:
  for v in row:hi(v)
hi(len(pub['literalRows']))
for row in pub['literalRows']:
 raw=bytes.fromhex(row[0]);hi(len(raw));h.update(raw)
 for v in row[1:]:hi(v)
assert h.hexdigest()==pub['tableSha256']=='dd431ff067120a92b87cb35cb4d34253516740e4e0e9fedf841f8d258317edcc'
def script(cp):return next((k for lo,hi,k in pub['scriptRanges']if lo<=cp<=hi),0)
arabic=[];wide=[]
for label,raw in acoustic.items():
 text=raw.decode('utf8');last=1;eligible=bool(text)
 for ch in text:
  cp=ord(ch)
  if 0x660<=cp<0x66a or 0x6f0<=cp<0x6fa:eligible=False;break
  value=script(cp)
  if value!=1:last=value
  if last!=5:eligible=False;break
 if eligible:arabic.append(label)
 if len(text)==1:
  cp=ord(text);narrow=cp-0xfee0 if 0xff01<=cp<=0xff5e else {0x3000:0x20,0xff5f:0x2985,0xff60:0x2986,0xffe0:0xa2,0xffe1:0xa3,0xffe2:0xac,0xffe3:0xaf,0xffe4:0xa6,0xffe5:0xa5,0xffe6:0x20a9}.get(cp)
  if narrow is not None and chr(narrow).encode('utf8')in lookup:wide.append([lookup[chr(narrow).encode('utf8')],label])
assert not arabic and not wide
assert(j['seedCount'],j['primaryCount'],j['secondaryCount'])==(14,21,192)
# Explicit V68 scalar reference: preserved copy each call, only bit0; no secondary mutation.
def refresh(permission):
 assert len(permission)==192 and all(0<=b<=255 for b in permission)
 return [code if code>=192 or permission[code]&1 else 65535 for code in preserved]
uniform=[refresh([b]*192)for b in range(256)]
for b,row in enumerate(uniform):assert row==[code if code>=192 or b%2 else 65535 for code in preserved]
onehot=[refresh([int(i==bit)for i in range(192)])for bit in range(192)]
for bit,row in enumerate(onehot):assert row==[code if code>=192 or code==bit else 65535 for code in preserved]
# Restore witness: a zero-permission prior call must not permanently discard ordinary labels.
assert refresh([0]*192)!=refresh([1]*192)==preserved
secondary_before=[65533]+[65535]*10+[65531]+[65535]*4
assert secondary_before==[65533]+[65535]*10+[65531]+[65535]*4
# Negative semantic alternatives are discriminated by the actual source.
assert refresh([2]*192)!=preserved # nonzero is NOT the native bit test
assert refresh([0]*192)[0]==65533 and refresh([0]*192)[11]==65531
assert sorted(seed)==list(range(10))+[138,139,141,185]
negatives={'nonzeroPermissionInsteadOfBit0':refresh([2]*192)!=preserved,'noRestoreAfterRefusal':refresh([0]*192)!=preserved,'specialFilteredAsAcoustic':refresh([0]*192)[0]==65533,'spaceFilteredAsAcoustic':refresh([0]*192)[11]==65531,'secondaryFromPrimary':secondary_before!=refresh([1]*192),'unrestoredMSeed':j['primary']!=j['seed']}
assert all(negatives.values())
j.update(scope='STATIC raw-derived actual direct number source symbols and fresh-mask numerical closure; no NN/D0/query authority',nativeBodyRegions=len(pins['regions']),nativeSpanChecks=len(pins['spans']),arabicCandidateLabels=arabic,widePairs=wide,scriptTableSha256=pub['tableSha256'],sourceEnumeratedLabels=list(range(16)),mappedSourceSymbols=preserved,refreshUniformCases=256,refreshOneHotCases=192,refreshUniformSha256=sha(struct.pack('<'+'H'*(256*16),*sum(uniform,[]))),refreshOneHotSha256=sha(struct.pack('<'+'H'*(192*16),*sum(onehot,[]))),secondaryUnchanged=secondary_before,negativeSemanticWitnesses=negatives,nativeExecuted=False,priorBindingClasses=len(binding_report['concrete_classes']))
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(j,indent=2)+'\n');print(json.dumps({k:v for k,v in j.items()if k not in ['groups','seed','primary','secondary']}))
