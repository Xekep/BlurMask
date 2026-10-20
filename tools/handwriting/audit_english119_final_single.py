"""Portable raw actual EN main-input audit; optional STATIC-only original PE check.
No research imports, vendor DLL load, Ghidra install or E: artifacts required.
"""
from pathlib import Path
import argparse,hashlib,json,struct
root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser();p.add_argument('--models',type=Path,default=root/'models');p.add_argument('--native',type=Path);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
d=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes();sha=lambda b:hashlib.sha256(b).hexdigest()
assert len(d)==545871 and sha(d)=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
def node(at,end,kind,version):
 magic,size,k,enc,v=struct.unpack_from('<IIIHH',d,at);assert(magic,k,enc,v)==(0x101fe90,kind,0,version)and at+8+size==end
node(0,545871,0x247,0x110);node(182412,183800,0x297,0x108);node(183800,184155,0x21b,0x10a);node(184155,545680,0x21d,0x119)
assert d[7975:7979]==bytes([0,1,1,1])and d[8811:8827]==bytes.fromhex('01000000000000000000000006000000')
assert struct.unpack_from('<HH',d,182477)==(0x201,217)
assert d[182428:182477].hex()=='0000003f3333b33ecdcc4c3e6666e63f0001003333b33ecdcc4c3e0000803e0000003f0000003fcdcccc3d060000804001'
assert d[183816:183818]==bytes([8,1])and d[184106:184109]==bytes([1,1,0])and struct.unpack_from('<3f',d,184133)==(0,0,0)
#Source/point behavior is exercised by the independent raw probe oracle. These
#are small consumption contracts, not an assertion of semantic equivalence.
contracts={
'LegacyEnglish119ParentPostprocessing.cs':['public sealed partial class LegacyEnglish119ParentPostprocessing'],
'LegacyEnglish119FinalizedSingleSource.cs':['initialDescriptors.Length!=1','finalChronology','input.Probabilities','1048576','LegacyEnglish119FinalInputCopyBudget.Preflight','source.Fields.Single','original.ResolveEndingMetadata','finalSingleReserved=true','169C30:single','161D20:no-pair','15D920:fresh-identity'],
'LegacyEnglish119MainLocalCandidates.cs':['SnapshotTable(table,token)','LegacyCursiveSeparationTable.Load','LegacyCursiveCutProducer.GenerateRussianVersion106','inputChronology.WithInsertedPreparedInk','seedProfile.InsertionShift!=1','LegacyOrdinaryGraphSourceRanges.PrepareBounded(seed,map,original,8,4096,workBudget,token)','LegacyOrdinarySamplerGroupPruning.PrepareSingleDescriptor','localCandidatesReserved=true']}
contracts['LegacyOrdinaryGraphWorkBudget.cs']=['20L*n*ids.Count','memberships>1048576','limit-value','maximumBytes=64L*1024*1024','FinalCopies','maximumEdges=131072','maximumRanges=65536']
contracts['LegacyOrdinaryGraphSourceRanges.cs']=['workBudget?.PreflightSeeds(actualSeeds,counts.Length)','GenerateWithAllocations(actualSeeds,seedGraph,maximumAtoms,maximumCandidates,null,cancellationToken,workBudget)']
contracts['LegacyOrdinaryCursiveCandidateGraph.cs']=['LegacyOrdinaryGraphWorkBudget? workBudget=null','workBudget?.Composition','workBudget?.MergedEdges','workBudget?.FinalCopies','workBudget.InitialEdges']
for name,terms in contracts.items():
 text=(root/'src/InkRecognition'/name).read_text(encoding='utf-8-sig')
 for term in terms:assert term in text,(name,term)
nativeReport=None
if a.native:
 b=a.native.read_bytes();pins=json.loads((Path(__file__).with_name('english119-final-single-native-pins.json')).read_text());assert sha(b)==pins['binarySha256']
 pe=struct.unpack_from('<I',b,0x3c)[0];assert b[pe:pe+4]==b'PE\0\0';sections=struct.unpack_from('<H',b,pe+6)[0];optional=pe+24;assert struct.unpack_from('<H',b,optional)[0]==0x20b
 imagebase=struct.unpack_from('<Q',b,optional+24)[0];table=optional+struct.unpack_from('<H',b,pe+20)[0];ss=[]
 for i in range(sections):
  q=table+i*40;virtualSize,rva,size,raw=struct.unpack_from('<IIII',b,q+8);ss.append((rva,size,raw))
 def at(va):
  r=va-imagebase
  for base,size,raw in ss:
   if base<=r<base+size:return raw+r-base
  raise AssertionError(hex(va))
 regions=0
 for name,parts in pins['regions'].items():
  for part in parts:
   begin=int(part['start'],16);end=int(part['endExclusive'],16);raw=b[at(begin):at(begin)+end-begin];assert len(raw)==part['bytes']and sha(raw)==part['sha256'];regions+=1
 for call in pins['calls']:
  q=at(int(call['at'],16));assert b[q:q+len(bytes.fromhex(call['bytes']))].hex()==call['bytes']
 for pin in pins['semanticPins']:
  q=at(int(pin['at'],16));assert b[q:q+len(bytes.fromhex(pin['bytes']))].hex()==pin['bytes']
 for leaf in pins.get('leafRegions',[]):
  q=at(int(leaf['at'],16));assert b[q:q+len(bytes.fromhex(leaf['bytes']))].hex()==leaf['bytes']
 for slot in pins['tables']:
  q=at(int(slot['table'],16)+int(slot['slot'],16));assert b[q:q+8].hex()==slot['slotBytes']and struct.unpack_from('<Q',b,q)[0]==int(slot['target'],16)
 nativeReport=dict(fullGhidraBodies=len(pins['regions']),regions=regions,calls=len(pins['calls']),pins=len(pins['semanticPins']),straightLineLeaves=len(pins.get('leafRegions',[])),binarySha256=sha(b))
report=dict(status='PASS raw actual EN finalized-single/source108/shift1 contracts',native=nativeReport,nativeExecuted=False,modelsSha256=sha(d),scope='Fresh single ordinary source and LOCAL sampler graph only; excludes active multiline merge, mainNN/masks/EnglishLK/winner; byte/body proof is not all-branches equivalence')
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(report,indent=2)+'\n');print('PASS actual EN finalized-single/main108 raw model and source contracts'+('; full native byte closure'if nativeReport else'; optional native bytes not requested'))
