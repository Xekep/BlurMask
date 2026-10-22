"""Static direct-number source admission proof; raw bytes only, no vendor loading."""
from pathlib import Path
import argparse,ast,hashlib,json,struct,runpy
p=argparse.ArgumentParser();p.add_argument('--native-repo',required=True,type=Path);p.add_argument('--models',required=True,type=Path);p.add_argument('--output',required=True,type=Path);a=p.parse_args()
R=a.native_repo;models=a.models;W=Path(__file__).resolve().parents[2]
pins=json.loads((W/'tools/handwriting/linguistic-fst28c-direct-number-bootstrap-pins.json').read_text())
class Binary:
 def __init__(self,name):
  self.data=(R/'research/myscript/sdk/build/x64'/name).read_bytes();pe=struct.unpack_from('<I',self.data,60)[0];assert self.data[pe:pe+4]==b'PE\0\0';o=pe+24;self.base=struct.unpack_from('<Q',self.data,o+24)[0];n=struct.unpack_from('<H',self.data,pe+6)[0];z=struct.unpack_from('<H',self.data,pe+20)[0];self.sections=[struct.unpack_from('<IIII',self.data,o+z+i*40+8)for i in range(n)]
 def off(self,va):
  rva=va-self.base
  for vs,r,sz,at in self.sections:
   if r<=rva<r+sz:return at+rva-r
  raise AssertionError('Unbacked pin')
for module in pins['modules']:
 binary=Binary(module['name']);assert hashlib.sha256(binary.data).hexdigest()==module['sha256']
 for row in module['regions']+module['spans']:
  at=binary.off(int(row['start'],16));assert hashlib.sha256(binary.data[at:at+row['bytes']]).hexdigest()==row['sha256']
sha=lambda x:hashlib.sha256(x).hexdigest();raw=(models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes()
s=(W/'tools/LegacyFst28CPrimaryMap.Probe/audit_output.py').read_text();fn=next(n for n in ast.parse(s).body if isinstance(n,ast.FunctionDef)and n.name=='alphabet');fn.body[-1]=ast.parse('return count,pos,tokens,reverse,triplets,flags if triplets else 0').body[0];ns={'struct':struct};exec(compile(ast.fix_missing_locations(ast.Module(body=[fn],type_ignores=[])),'original-al','exec'),ns);count,end,tokens,reverse,triplets,maps=ns['alphabet'](raw,299110)
assert(count,end,triplets,maps)==(16,299573,0,0)
c=(models/'text-unicode/legacy-case-data.json').read_bytes();case=json.loads(c)
ordered=['alternatingRanges','lowerRanges','lowerSingles','upperRanges','upperSingles','titleSingles','specialRows','sequenceScalars'];flat=[]
for k in ordered:
 flat.append(len(case[k]))
 for row in case[k]:flat.extend(row if isinstance(row,list)else[row])
assert sha(struct.pack('<'+'i'*len(flat),*flat))=='5e99b64e4e7c2893a50e60552eb5c6a11894e466e18b8c4945071b5699916c04'
cps=sorted({x for v in tokens.values()for x in v});assert all(cp<128 for cp in cps)
def inrange(rows,cp):return next((r for r in rows if r[0]<=cp<=r[1]),None)
def present(key,cp):return any(r[0]==cp for r in case[key])
def alternatives(cp):
 special=sum(r[0]==cp for r in case['specialRows']);lo=inrange(case['alternatingRanges'],cp);up=inrange(case['alternatingRanges'],cp-1)
 lower=(cp-lo[0])%2==0 if lo else bool(inrange(case['lowerRanges'],cp)or present('lowerSingles',cp))
 upper=(cp-up[0])%2==1 if up else bool(inrange(case['upperRanges'],cp)or present('upperSingles',cp))
 return special+int(lower or upper)
assert all(alternatives(cp)==0 for cp in cps)
e=Binary('MyScriptEngine.dll');scripts=[struct.unpack_from('<III',e.data,e.off(0x1801fb6d0)+i*12)for i in range(716)];assert all(a<=z for a,z,k in scripts)and all(scripts[i][1]<scripts[i+1][0]for i in range(715));sc=lambda cp:next((k for a,z,k in scripts if a<=cp<=z),0)
assert all(sc(cp)!=5 and not(0x660<=cp<=0x669 or 0x6f0<=cp<=0x6f9)for cp in cps)
costs=runpy.run_path(str(W/'tools/handwriting/reference_fst23c_outgoing.py'))['actual_main135_costs']();negative=[]
for state in range(1,98):
 tag=struct.unpack_from('<b',raw,299588+state*8+5)[0]
 if costs[tag+128]<0:negative.append(state)
assert not negative
source_flags=struct.unpack_from('<I',raw,299096)[0];source_weight60=struct.unpack_from('<I',raw,299092)[0];assert(source_flags,source_weight60)==(257,0)
name_raw=raw[299024:299080];key=(0x77191214).to_bytes(4,'little');name=bytes(v^key[i%4]for i,v in enumerate(name_raw)).split(b'\0')[0].decode('ascii');assert name=='ru_RU-lk-text.number'
# Actual dotted .number is absent from23E0C0's recognized suffix cases.
category_bits=0;header=1|(int(bool(negative))<<1)|category_bits;assert header==1
j={'scope':'STATIC exact number child direct-fresh singleton proposal, not fullroot selection/default','rootSha256':sha(raw),'alphabetPayloadSha256':sha(raw[299110:299573]),'sourceFlags68':source_flags,'acousticKind':309,'skipMain135Bit7':bool((source_flags>>7)&1),'caseTriplets':triplets,'storedCaseMapFlags':maps,'codepoints':[{'value':cp,'caseAlternatives':alternatives(cp),'nativeScript':sc(cp)}for cp in cps],'derivedCaseV58':False,'derivedContextualV68':False,'sourceV58True':True,'sourceV60NegativeCost':False,'sourceWeight60':source_weight60,'sourceWeight64':65536,'decodedSourceName':name,'numberNameCategoryBits':category_bits,'derived28CHeader':header,'directFreshSelectedCount':1,'directFreshSelectedOrdinal':0,'directFreshAggregateNamespace':1,'fullrootNamespaceClaimed':False,'nativeCaseNumericTableSHA256':case['tableSha256'],'scriptTableSHA256':sha(e.data[e.off(0x1801fb6d0):e.off(0x1801fb6d0)+716*12]),'queryExecuted':False}
j['nativeBodyRegions']=sum(len(m['regions'])for m in pins['modules']);j['nativeSpanChecks']=sum(len(m['spans'])for m in pins['modules']);a.output.write_text(json.dumps(j,indent=2)+'\n');print(json.dumps(j,indent=2))
