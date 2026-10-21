"""PE-only fresh outer policy/active English MAIN118/140 admission proof.
Never executes or loads vendor code. Raw numerical operands are independently read.
"""
from pathlib import Path
import argparse,hashlib,json,struct,subprocess,sys
p=argparse.ArgumentParser()
for name in ('repo','models','sdk','output'):p.add_argument('--'+name,type=Path,required=True)
a=p.parse_args();here=Path(__file__).resolve().parent;sha=lambda b:hashlib.sha256(b).hexdigest()
proof=json.loads((here/'english-main-word-policy-transition-pins.json').read_bytes())
raw=(a.sdk/proof['binary']).read_bytes();assert sha(raw)==proof['sha256']
pe=struct.unpack_from('<I',raw,60)[0];assert raw[pe:pe+4]==b'PE\0\0'
op=pe+24;assert struct.unpack_from('<H',raw,op)[0]==0x20b
base=struct.unpack_from('<Q',raw,op+24)[0];assert base==int(proof['base'],16)
at=op+struct.unpack_from('<H',raw,pe+20)[0];sections=[]
for i in range(struct.unpack_from('<H',raw,pe+6)[0]):
    _,rva,n,offset=struct.unpack_from('<IIII',raw,at+i*40+8);sections.append((rva,n,offset))
def read(at,n):
    v=at-base
    for start,size,offset in sections:
        if start<=v and n<=start+size-v:return raw[offset+v-start:offset+v-start+n]
    raise AssertionError(('bounded PE',hex(at),n))
assert proof['fullBodies']==24 and len(proof['regions'])==25
assert len(proof['calls'])==345 and len(proof['leaves'])==13 and len(proof['tables'])==35
for r in proof['regions']:
    at=int(r['start'],16);n=int(r['endExclusive'],16)-at
    assert n==r['bytes'] and sha(read(at,n))==r['sha256']
for r in proof['calls']+proof['leaves']+proof['tables']+proof['parents']:
    expected=bytes.fromhex(r['bytes']);assert read(int(r.get('at',r.get('start')),16),len(expected))==expected
for r in proof['parents']:
    _,value,_=struct.unpack('<QQQ',bytes.fromhex(r['bytes']))
    assert value&0xffffffff==r['kind'] and value>>32==r['parent']
parents={r['kind']:r['parent']for r in proof['parents']}
assert parents=={0x240:0x209,0x23f:0x209,0x209:3}
assert parents[0x240]!=0x23f # The siblings are not interchangeable runtime types.
for r in proof['dependencies']:assert sha((a.repo/r['path']).read_bytes())==r['sha256']

# Compose the already-checked complete source reader/clone wire closure as PE
# bytes. Do not import a Ghidra output parser or load any executable module.
up=json.loads((a.repo/'tools/handwriting/english119-source-join-pins.json').read_bytes())
assert up['dllSha256']==proof['sha256']
for r in up['regions']:assert sha(read(r['start'],r['end']-r['start']))==r['sha256']
for table,slot,target in up['bindings']:assert struct.unpack('<Q',read(table+slot,8))[0]==target

# Prepared BYTE-placement controls, not a real configured owner: D0's09 and
# F5/type23F's0A must differ. Native1A6980 consumes decimal[10], not hex10.
placements=[]
for d0 in (0,1):
    for f5 in (0,1):
        for isa23f in (0,1):
            packet=bytearray(16);packet[9]=d0;packet[10]=int(not isa23f and f5)
            placements.append(dict(prepared=True,D0Condition=d0,F5=f5,
                controllerIsA23F=isa23f,packet09=packet[9],packet0A=packet[10],graph1DC=packet[10]))
assert any(r['D0Condition']==0 and r['graph1DC']==1 for r in placements)
assert any(r['D0Condition']==1 and r['graph1DC']==0 for r in placements)
# Fresh NEW OUTER2B3 no-copy branch, actual240 (not23F): F5 constructor0 stays0.
fresh=next(r for r in placements if(r['D0Condition'],r['F5'],r['controllerIsA23F'])==(0,0,0))
assert fresh['graph1DC']==0

a.output.parent.mkdir(parents=True,exist_ok=True)
reference=a.output.with_name('emwt-'+sha(str(a.output.resolve()).encode())[:12]+'-raw.json')
subprocess.run([sys.executable,'-I','-B',str(here/'reference_english_main_word_policy.py'),
    '--repo',str(a.repo),'--models',str(a.models),'--output',str(reference)],check=True)
ref=json.loads(reference.read_bytes())
assert ref['pairEnabled'] and ref['pairCrossLineRawInt60']==-8192
assert ref['maximumSourceArcs']==125 and ref['parentAlternativeDistance']==20
assert ref['singleDictionarySummary']==3 and not ref['embedding202BPresent']
result=dict(status='PASS STATIC NEW fresh OUTER English policy and active MAIN transition closure',
    fullBodies=24,regions=25,instructions=sum(r['instructions']for r in proof['regions']),
    calls=345,unregisteredCompleteLeavesOrThunks=13,dispatchBindings=35,registeredAncestry=parents,
    sourceReaderUpstreamBodies=len({r['function']for r in up['regions']}),
    sourceReaderUpstreamRegions=len(up['regions']),sourceReaderBindings=len(up['bindings']),
    preparedBytePlacementControls=placements,raw=ref,
    admittedScope='NEW fresh OUTER2B3 direct208/20E/no-copy/no-external-properties-events-reuse/one13B-only',
    configuredOrHighDriverEquivalent=False,runtimeChanged=False,
    wordDecoderReady=False,targetsRead=False,vendorExecuted=False)
a.output.write_text(json.dumps(result,indent=2)+'\n')
print('PASS fresh OUTER24full/25regions/13leaves/35bindings +8 prepared09/0A controls +actual EN ACTIVE pair/scalars; no decoder')
