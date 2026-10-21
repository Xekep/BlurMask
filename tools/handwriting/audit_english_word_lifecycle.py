"""Static PE/caller and raw model/corpus scheduling closure. No SDK execution.
This does not run a decoder or label lexical paths as acoustic selections.
"""
from pathlib import Path
import argparse, hashlib, json, struct, subprocess, sys

p=argparse.ArgumentParser()
for name in ('repo','models','sdk','acoustic-capture','output'):
    p.add_argument('--'+name,type=Path,required=True)
a=p.parse_args(); here=Path(__file__).resolve().parent
sha=lambda b:hashlib.sha256(b).hexdigest()
proof=json.loads((here/'english-word-lifecycle-pins.json').read_bytes())
raw=(a.sdk/proof['binary']).read_bytes();assert sha(raw)==proof['sha256']
pe=struct.unpack_from('<I',raw,60)[0];assert raw[pe:pe+4]==b'PE\0\0'
op=pe+24;assert struct.unpack_from('<H',raw,op)[0]==0x20b
base=struct.unpack_from('<Q',raw,op+24)[0];assert base==int(proof['base'],16)
st=op+struct.unpack_from('<H',raw,pe+20)[0];sections=[]
for i in range(struct.unpack_from('<H',raw,pe+6)[0]):
    _,rva,n,offset=struct.unpack_from('<IIII',raw,st+i*40+8)
    sections.append((rva,n,offset))
def read(at,n):
    v=at-base
    for start,size,offset in sections:
        if start<=v and n<=start+size-v:return raw[offset+v-start:offset+v-start+n]
    raise AssertionError(('bounded PE',hex(at),n))
assert proof['fullBodies']==16 and len(proof['regions'])==20
assert len(proof['calls'])==332 and len(proof['leaves'])==3 and len(proof['tables'])==12
for r in proof['regions']:
    at=int(r['start'],16);n=int(r['endExclusive'],16)-at
    assert n==r['bytes'] and sha(read(at,n))==r['sha256']
for r in proof['calls']+proof['leaves']+proof['tables']+proof['parents']:
    at=int(r.get('at',r.get('start')),16);b=bytes.fromhex(r['bytes'])
    assert read(at,len(b))==b
for r in proof['parents']:
    _,kind_parent,_=struct.unpack('<QQQ',read(int(r['at'],16),24))
    assert kind_parent&0xffffffff==r['kind'] and kind_parent>>32==r['parent']
assert {r['kind']:r['parent']for r in proof['parents']}=={0x13b:0x139,0x139:0x27b,0x27b:0x138,0x13d:0x138}
for dep in proof['upstream']:
    assert sha((a.repo/dep['path']).read_bytes())==dep['sha256']

# Exact English selected inventory, independently of any fresh-controller claim.
lk=(a.models/'en-US/en_US-lk-text.lite.res').read_bytes()
assert len(lk)==3300812 and sha(lk)=='9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9'
assert struct.unpack_from('<I',lk,88)[0]==21
children=[];at=92
for i in range(21):
    magic,n,kind,enc,version=struct.unpack_from('<IIIHH',lk,at)
    end=at+n+8
    assert magic==0x101fe90 and enc==0 and at<end<=len(lk)
    children.append([at,end,kind,version]);at=(end+3)&~3 if i<20 else end
assert at==len(lk) and [r[2] for r in children]==[0x13d,0x13b]+[0x13a]*19
assert children[1]==[184,2832716,0x13b,0x300]

# A raw acoustic capture carries no desired words; retain its exact provenance.
capture=a.acoustic_capture.read_bytes()
assert sha(capture)=='d8ce9276fcfa952ad40d39e217effcfa329376aa78204549f4a5e2d106464f03'
c=json.loads(capture);assert c['TargetsRead'] is False
assert len(c['Rows'])==134 and len(c['MultilineRefused'])==7
counts=dict(nodes=0,arcs=0,ascendingSpanInvocations=0,initialInvocations=0,
            parentSpanInvocations=0,emptyIntermediateSpans=0)
digest=hashlib.sha256()
for row in c['Rows']:
    graph=row['Graph'];groups={g['Group']:g for g in row['Groups']}
    for node,arcs in enumerate(graph):
        counts['nodes']+=1;counts['arcs']+=len(arcs)
        extent=max((span+1 for group,span in arcs),default=0)
        for group,span in arcs:
            assert group in groups and 0<=span<=node and span<8
        present={span for group,span in arcs}
        for span in range(extent):
            previous=node-span-1
            assert previous>=-1
            counts['ascendingSpanInvocations']+=1
            counts['initialInvocations' if previous==-1 else 'parentSpanInvocations']+=1
            if span not in present:counts['emptyIntermediateSpans']+=1
            digest.update(struct.pack('<iii',node,span,previous))
assert counts['arcs']==4083
# Independent abstract branch truth table, explicitly NOT a real kind1 owner.
flag_branches=[]
for kind in range(256):
    reads=kind==1
    flag_branches.append(dict(kind=kind,readsSourceObjectFlag=reads,
                              otherwiseDerivedZero=not reads))
assert sum(r['readsSourceObjectFlag'] for r in flag_branches)==1

# Compose prior exact query/tag and sparse-history closure without vendor loads.
a.output.parent.mkdir(parents=True,exist_ok=True)
prefix='ewl-'+sha(str(a.output.resolve()).encode())[:12]
up=a.output.with_name(prefix+'-upstream.json')
subprocess.run([sys.executable,str(a.repo/'tools/handwriting/audit_english_query_pool_plan.py'),
    '--repo',str(a.repo),'--models',str(a.models),'--sdk',str(a.sdk),'--output',str(up)],check=True)
result=dict(status='PASS STATIC English ordinary word lifecycle/caller proposal',
    fullBodies=16,regions=20,linearInstructions=sum(r['instructions']for r in proof['regions']),
    calls=332,unregisteredCompleteLeaves=3,dispatchBindings=12,registeredParentLinks=4,
    actualEnglishRootChildren=children,selectedOnlyMainPolicy=True,
    actualFullRootHas13D=True,actualAcousticCaptureSha256=sha(capture),
    actualAcousticOnlySchedule=counts,actualScheduleDigest=digest.hexdigest(),
    preparedKindFlagBranchControls=flag_branches,
    upstream=json.loads(up.read_bytes()),runtimeChanged=False,
    wordDecoderReady=False,targetsRead=False,vendorExecuted=False)
a.output.write_text(json.dumps(result,indent=2)+'\n')
print('PASS word lifecycle16full/20regions/3leaves/332calls +actual134 graph scheduling; NOT decoder')
