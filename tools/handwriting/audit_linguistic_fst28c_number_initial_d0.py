"""Static native/AK initial-number D0 prerequisites; no SDK loading/execution."""
from pathlib import Path
import argparse,hashlib,json,struct,math,ast
p=argparse.ArgumentParser();p.add_argument('--native-repo',required=True,type=Path);p.add_argument('--models',required=True,type=Path);p.add_argument('--output',required=True,type=Path);a=p.parse_args();W=Path(__file__).resolve().parents[2];sha=lambda b:hashlib.sha256(b).hexdigest()
b=(a.native_repo/'research/myscript/sdk/build/x64/MyScriptText.dll').read_bytes();pins=json.loads((W/'tools/handwriting/linguistic-fst28c-number-initial-d0-pins.json').read_text());assert sha(b)==pins['nativeSha256'];pe=struct.unpack_from('<I',b,60)[0];assert b[pe:pe+4]==b'PE\0\0';op=pe+24;base=struct.unpack_from('<Q',b,op+24)[0];z=struct.unpack_from('<H',b,pe+20)[0];sections=[struct.unpack_from('<IIII',b,op+z+i*40+8)for i in range(struct.unpack_from('<H',b,pe+6)[0])]
def native(va,n):
 rva=va-base
 for vs,r,size,at in sections:
  if r<=rva and n<=r+size-rva:return b[at+rva-r:at+rva-r+n]
 raise AssertionError('Unbacked pin')
for r in pins['regions']+pins['spans']:
 raw=native(int(r['start'],16),r['bytes']);assert sha(raw)==r['sha256']
 if 'hex'in r:assert raw.hex()==r['hex']
def q(va):return struct.unpack('<Q',native(va,8))[0]
assert q(0x1802e3c80+24)==0x1802e3c40 and q(0x1802e3c40+16)==0x1802e3b68
assert(q(0x1802e3b68),q(0x1802e3b68+8))==(0x1801b0330,0x1801b0560)
assert q(0x1802e8b60+32)==0x1802e8b00 and q(0x1802e8b00)==0x180236020
assert(q(0x1802e8bb0+0x10),q(0x1802e8bb0+0x68),q(0x1802e8c60+0x68))==(0x180236490,0x180236ca0,0x18023bd60)
assert native(0x18023bd60,14)==bytes.fromhex('488b5208488b024c8b486849ffe1')
assert q(0x1802de8a0+24)==0x1802de5d8 and q(0x1802de5d8+8)==0x1802de610
assert q(0x1802de610+0x248)==0x1800ca630 and native(0x1800ca630,5)==bytes.fromhex('488b4270c3')
assert q(0x1802de610+0x280)==0x180155710 # explicit distinct-slot negative witness
assert q(0x1802e1d40+24)==0x1802e1d88 and q(0x1802e1d88+8)==0x1802e1ff0 and q(0x1802e1ff0+0x180)==0x1801acf50
assert native(0x1801acf50,20)==bytes.fromhex('488b9280010000488b024c8b908800000049ffe2')
assert q(0x1802e5a20+0x88)==0x1801d6320 and native(0x1801d6320,8)==bytes.fromhex('488b8230010000c3')
assert q(0x1802e15e0+24)==0x1802e1030 and q(0x1802e1030+8)==0x1802e1490 and q(0x1802e1490+0x88)==0x18004a2c0
assert native(0x18004a2c0,3)==bytes.fromhex('b001c3')
assert(q(0x1802de610+0xd0),q(0x1802e4180+0x48),q(0x1802e4180+0x50),q(0x1802e9270+0x68))==(0x1801520e0,0x1801c01b0,0x1801c0f60,0x180241eb0)
hidden=[math.floor(1024*math.tanh(3*i/47))for i in range(48)];output=[math.floor(1024/(1+math.exp(-8*i/127)))for i in range(127)]+[1024]
assert struct.unpack('<48i',native(0x180260fd0,192))==tuple(hidden)
assert struct.unpack('<128i',native(0x180261090,512))==tuple(output)
e=(a.native_repo/'research/myscript/sdk/build/x64/MyScriptEngine.dll').read_bytes();ep=pins['engineHeaderReader'];assert sha(e)==ep['nativeSha256']
ee=struct.unpack_from('<I',e,60)[0];eo=ee+24;ebase=struct.unpack_from('<Q',e,eo+24)[0];ez=struct.unpack_from('<H',e,ee+20)[0];esections=[struct.unpack_from('<IIII',e,eo+ez+i*40+8)for i in range(struct.unpack_from('<H',e,ee+6)[0])]
for row in ep['regions']:
 rva=int(row['start'],16)-ebase
 region=next((e[at+rva-r:at+rva-r+row['bytes']]for vs,r,n,at in esections if r<=rva and row['bytes']<=r+n-rva),None);assert region is not None and sha(region)==row['sha256']
ak=(a.models/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();lk=(a.models/'ru-RU/ru_RU-lk-text.lite.res').read_bytes();assert sha(ak)=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
networks=[];total=0;dots=0
for start,end in [(189755,195746),(195895,283690),(283697,367892),(367899,467094),(467101,566296)]:
 magic,version,length=struct.unpack_from('<HHI',ak,start);assert(magic,version,start+8+length)==(0xfea0,0x106,end)
 assert ak[start+8]==0x12;kind,inputs,hiddenWidth,zero1,zero2,outputs,scale=struct.unpack_from('<HIIIIIH',ak,start+9);assert(zero1,zero2,scale)==(0,0,10)
 cursor=start+33;matrices=[]
 for columns,rows in [(inputs,hiddenWidth-1),(hiddenWidth,outputs-1)]:
  weightAt=cursor;scaleAt=cursor+columns*rows;assert ak[scaleAt]==1 and all(ak[scaleAt+1:scaleAt+rows+1]);cursor=scaleAt+rows+1
  matrices.append(dict(columns=columns,rows=rows,weightsOffset=weightAt,weightBytes=columns*rows,weightsSha256=sha(ak[weightAt:scaleAt]),scalesOffset=scaleAt,scalesSha256=sha(ak[scaleAt:cursor])))
 assert cursor==end
 total+=end-start;networks.append(dict(start=start,end=end,kind=kind,inputWidth=inputs,hiddenWidth=hiddenWidth-1,outputWidth=outputs-1,sha256=sha(ak[start:end]),matrices=matrices,dotProducts=sum(m['columns']*m['rows']for m in matrices)))
assert [n['inputWidth']for n in networks]==[30,256,238,313,313]
assert [n['outputWidth']for n in networks]==[28,180,180,180,180] and total==376371
contextDots=networks[0]['dotProducts'];glyphDots=sum(n['dotProducts']for n in networks[1:]);assert(contextDots,glyphDots)==(5828,368720)
assert struct.unpack_from('<IHH',ak,566408)==(1,192,256)
calibration=ak[566416:572560];assert sha(calibration)=='c6566cb776435b35bd6159959c192f527dd40422d3601e21453892a24e80729d'
centers=[];slopes=[]
for family in range(4):
 pairs=[struct.unpack_from('<ff',calibration,(family*192+i)*8)for i in range(180)];centers.append([v[0]for v in pairs]);slopes.append([v[1]for v in pairs]);assert all(math.isfinite(c)and math.isfinite(s)and s>0 for c,s in pairs)
profile=json.loads((a.models/'ru-RU/legacy-engine-profile.json').read_text());assert profile['activations']=={'hidden':hidden,'output':output}
for key,rows in [('centers',centers),('slopes',slopes)]:
 for f in range(4):assert struct.pack('<180f',*rows[f])==struct.pack('<180f',*profile[key][f])
# Raw decoded source number map, not supplied lexical IDs.
fn=next(n for n in ast.parse((W/'tools/LegacyFst28CPrimaryMap.Probe/audit_output.py').read_text()).body if isinstance(n,ast.FunctionDef)and n.name=='alphabet');ns={'struct':struct};exec(compile(ast.Module(body=[fn],type_ignores=[]),'original-al','exec'),ns)
sn,se,source,_=ns['alphabet'](lk,299110);an,ae,acoustic,_=ns['alphabet'](ak,99);assert(sn,se,an,ae)==(16,299573,192,5233);lookup={v:k for k,v in acoustic.items()};preserved=[65533 if source[i]==b'\0'else 65531 if source[i]==b' 'else lookup.get(source[i],65535)for i in range(16)]
header=struct.unpack_from('<8H',ak,99);assert header[3]==178 and header[3]!=65535;reserved=list(header[4:7])+[lookup.get(v,65535)for v in [b'\xf3\xb0\x80\x80',b'\xe2\x80\x8c',b'\xe2\x80\x8d']]
# Independently scoped PREPARED numerical controls for the future genuine producer.
# These rows are NOT raw-model NN results or authority-bearing D0 receipts.
IMP=0x4000000;controls=[]
for mode in range(6):
 rows=[[([IMP,40000,39999,0,IMP-1,40001][(i+mode)%6])for i in range(195)]for _ in range(mode%3+1)]
 permission=[0]*192
 for code in reserved:
  if code!=65535:permission[code]=3
 for row in rows:
  for i,cost in enumerate(row[:192]):permission[i]|=(3 if cost<IMP else 0)|(4 if cost<40000 else 0)
 eligibility=[int(any(row[i]<IMP for row in rows))for i in range(195)]
 mapped=[code if code>=192 or permission[code]&1 else 65535 for code in preserved]
 assert mapped[0]==65533 and mapped[11]==65531 and all(v in [0,3,4,7]for v in permission)
 controls.append(dict(mode=mode,rows=len(rows),permissionsSha256=sha(bytes(permission)),eligibilitySha256=sha(bytes(eligibility)),refreshedMap=mapped))
# Proposed bounded initial-only facade, library limits not native constants.
maxima=dict(nodes=65535,groups=65535,arcs=262144,strokes=32,descriptors=32,originalPoints=8192,preparedPoints=8192,initialArcs=256,selectedGroups=64,selectedPointsPerGroup=255,contextWindows=96)
n=g=65535;arcs=262144;s=d=32;p0=points=8192;t=64;limit=255;c=96
fixed=572763+2*376371+4*5134+65536
logical=fixed+128*(p0+points)+64*d+128*s+16*(n+g+arcs)+32768*t+2048
work=2*572763+p0+points+n+g+arcs+c*(contextDots+32768+256*min(points,4096))+t*(glyphDots+64*limit*limit+32768+n+arcs+32*points)
assert(logical,work)==(11905497,435709940)
j=dict(status='PASS STATIC initial-number D0 prerequisites only',nativeFunctions=len({r['function']for r in pins['regions']}),nativeFullBodyRegions=len(pins['regions']),nativeBoundedSpans=len(pins['spans']),correctFrontendV248='222meta2DE5D8+8 ->VT2DE610+248 ->0CA630 ->frontend70',distinctV280='155710 not V248',graphPreparationGate='21B.V180 ->retained297.V88 ->04A2C0 AL1 in fresh unwrapped sampler branch',sourceKind=0x135,engineHeaderFullRegions=1,nativeAlphabetObjectE=header[3],ordinaryClassificationRoute='1BE930 because alphabet.E !=FFFF; no1BF0A0 substitution',networks=networks,networkPayloadBytes=total,contextDotProducts=contextDots,glyphDotProducts=glyphDots,activationHiddenSha256=sha(struct.pack('<48i',*hidden)),activationOutputSha256=sha(struct.pack('<128i',*output)),storedCalibrationMatrixSha256=sha(calibration),storedCalibrationPairs=768,selectedFamilyPairs=720,sourcePreserved=preserved,reservedSlots=reserved,preparedNumericalControls=controls,proposedMaximums=maxima,proposedFixedLogicalBase=fixed,proposedMaxLogicalBytes=logical,proposedMaxWorkCardinality=work,actualInitialD0Executed=False,queryImplemented=False,nativeExecuted=False)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(j,indent=2)+'\n');print(json.dumps({k:v for k,v in j.items()if k not in ['networks','preparedNumericalControls']}))
