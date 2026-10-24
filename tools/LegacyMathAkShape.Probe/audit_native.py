"""Original full299 producer/readers + scalar ABI spans. PE data only, no DLL load."""
import pathlib,struct,json,hashlib,argparse,capstone,sys
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent))
import reference_shape as reference

def sha(x):return hashlib.sha256(x).hexdigest()
class PE:
 def __init__(self,path):
  self.b=path.read_bytes();o=struct.unpack_from('<I',self.b,60)[0];n=struct.unpack_from('<H',self.b,o+6)[0];z=struct.unpack_from('<H',self.b,o+20)[0];self.s=[]
  for i in range(n):
   at=o+24+z+40*i;vs,va,sz,off=struct.unpack_from('<IIII',self.b,at+8);self.s.append((va,max(vs,sz),off))
 def read(self,r,n):
  for va,z,o in self.s:
   if va<=r and r+n<=va+z:return self.b[o+r-va:o+r-va+n]
  raise ValueError(hex(r))

def run(repo):
 d=pathlib.Path(__file__).resolve().parent;pins=json.loads((d/'native-pins.json').read_text(encoding='utf-8-sig'));p=PE(repo/'research/myscript/sdk/build/x64/MyScript2D.dll');assert sha(p.b)==pins['moduleSha256']
 for region in pins['regions']:assert sha(p.read(region['rva'],region['length']))==region['sha256']
 md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);instructions=0
 for span in pins['scalarSpans']:
  b=p.read(span['rva'],span['length']);assert sha(b)==span['sha256'];xs=list(md.disasm(b,0x180000000+span['rva']))
  assert xs[-1].address+xs[-1].size==0x180000000+span['rva']+span['length']
  assert [[hex(x.address),x.bytes.hex(),x.mnemonic,x.op_str]for x in xs]==span['instructions'];instructions+=len(xs)
 b=(repo/'models/math/math-ak.res').read_bytes();assert sha(b)=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
 weights,labels=reference.raw_prototypes(b);allentries=[e for row in labels for e in row['entries']]
 assert len(labels)==259 and len(allentries)==39 and sum(len(e['targets'])for e in allentries)==66
 assert weights==(1.5,3.,3.,1.,1.,1.,1.)
 #03DB00 direct field stores; V18 returns n*7; exact branch0A=0 bypasses DTW.
 assert p.read(0x3d1de,12)==bytes.fromhex('6641837f0a000f857f010000')
 table=struct.unpack('<6Q',p.read(0x895c0,48));assert table[4:]==(0x18003c2e0,0x18003d110)
 assert struct.unpack('<3Q',p.read(0x89598,24))==(0x18003db00,0x18000c580,0x18003de10)
 #Pure midpoint/zero and all seven signed components, no feature/candidate issuer.
 rounds=[]
 for x in [-12288.5,-8192.5,-1.5,-.5,-0.,0.,.5,1.5,8192.5,24576.5]:rounds.append([x,reference.round_short(reference.f(x))])
 assert [x[1]for x in rounds]==[-12289,-8193,-2,-1,0,0,1,2,8193,24577]
 zero=[0]*210;pair=[]
 for a in allentries:
  assert reference.distances(a['values'],[a])==[0.]
  pair.extend(reference.distances(a['values'],allentries))
 assert len(pair)==1521
 return dict(fullOriginalBodies=6,exactDataBindings=8,scalarSpans=4,scalarInstructions=instructions,
  rawPrototypeValues=8190,rawTargets=66,purePrototypePairDistances=1521,midpointControls=rounds,
  prototypePairF32Sha256=sha(struct.pack('<1521f',*pair)),DTWExecuted=False,modelExecutions=0,vendorExecution=False)
if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('repo',type=pathlib.Path);p.add_argument('output',type=pathlib.Path);a=p.parse_args();r=run(a.repo);a.output.write_text(json.dumps(r,indent=2)+'\n',encoding='utf8');print('PASS6 full original bodies/8bindings/4scalarspans/0 NN')
