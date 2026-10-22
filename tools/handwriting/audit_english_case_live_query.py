"""Read-only PE/caller/bounds certificate for a proposed first case-query issuer."""
from pathlib import Path
import argparse,hashlib,json,struct,subprocess,sys,runpy
import capstone
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--seams',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();pins=json.loads((a.repo/'tools/handwriting/english-case-live-query-pins.json').read_bytes());raw=(a.sdk/pins['binary']).read_bytes();sha=lambda b:hashlib.sha256(b).hexdigest();assert sha(raw)==pins['sha256'];pe=struct.unpack_from('<I',raw,60)[0];at=pe+24+struct.unpack_from('<H',raw,pe+20)[0];sections=[struct.unpack_from('<IIII',raw,at+40*i+8)for i in range(struct.unpack_from('<H',raw,pe+6)[0])]
def data(addr,n):
 v=addr-0x180000000
 for _,r,s,o in sections:
  if r<=v and n<=r+s-v:return raw[o+v-r:o+v-r+n]
 raise AssertionError('boundedPE')
c=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);branches=[]
for r in pins['regions']:
 b=data(int(r['start'],16),int(r['endExclusive'],16)-int(r['start'],16));assert len(b)==r['bytes']and sha(b)==r['sha256'];ins=list(c.disasm(b,int(r['start'],16)));assert sum(x.size for x in ins)==len(b)
 for x in ins:
  if x.mnemonic.startswith('j')or x.mnemonic=='call':branches.append(dict(at=hex(x.address),instruction=x.mnemonic,operand=x.op_str,bytes=x.bytes.hex()))
assert branches==pins['calls']
for r in pins['leaves']:
 b=data(int(r['start'],16),int(r['endExclusive'],16)-int(r['start'],16));assert b.hex()==r['bytes'];ins=list(c.disasm(b,int(r['start'],16)));assert sum(x.size for x in ins)==len(b);assert[dict(at=hex(x.address),instruction=x.mnemonic,operand=x.op_str)for x in ins]==r['instructions']
for r in pins['tables']:assert data(int(r['at'],16),8).hex()==r['bytes']and struct.unpack('<Q',data(int(r['at'],16),8))[0]==int(r['target'],16)
#Registered222 metadata proves table base; do not confuseV248 withV280.
assert struct.unpack('<Q',data(0x1802de8b8,8))[0]==0x1802de5d8
assert struct.unpack('<Q',data(0x1802de5e0,8))[0]==0x1802de610
assert struct.unpack('<Q',data(0x1802de610+0x248,8))[0]==0x1800ca630
assert data(0x1800ca630,5)==bytes.fromhex('488b4270c3')
assert struct.unpack('<Q',data(0x1802de610+0x280,8))[0]==0x180155710
for w in pins['initialQWrites']:assert data(int(w['at'],16),len(bytes.fromhex(w['bytes']))).hex()==w['bytes']
assert data(0x1801fd336,8)==bytes.fromhex('41c6864d16000008')
#The full original lifecycle proves the genuine graphkind conditional call.
up=json.loads((a.repo/'tools/handwriting/english-word-lifecycle-pins.json').read_bytes())
for r in up['regions']:
 b=data(int(r['start'],16),int(r['endExclusive'],16)-int(r['start'],16));assert sha(b)==r['sha256']
for r in up['leaves']:assert data(int(r['start'],16),int(r['endExclusive'],16)-int(r['start'],16)).hex()==r['bytes']
for r in up['tables']:assert data(int(r['at'],16),8).hex()==r['bytes']
meta=json.loads((a.seams/'old-seams.json').read_bytes());assert len(meta['files'])==2
for item in meta['files']:
 for side in('before','after'):assert sha((a.seams/side/item['path']).read_bytes())==item[side+'Sha256']
subprocess.run(['git','apply','--check',str((a.seams/'exact-two-old-seams.patch').resolve())],cwd=a.seams/'before',check=True)
#No raw states, masks or foreign public snapshots are accepted by a runtime API:
#this packet has no runtime API at all. The following is an independent model
#of the planned short-gate transition, not a production concurrency test.
guards=0
for busy in(0,1):
 for d0 in(0,1):
  for score in(0,1):
   for span_active in(0,1):
    for latest in(0,1):
     for epoch in(0,1):
      for same_map in(0,1):
       ok=not(busy or d0 or score or span_active)and latest and epoch and same_map
       if not ok:guards+=1;continue
       assert not busy;busy=1
       #A current D0 snapshot now reportsfalse due to our OWNbusy, so only the
       #new opaque lease/map/version predicates are used after successfulentry.
       assert not(not busy and latest and epoch);assert busy and latest and epoch and same_map
       busy=0
assert guards==127
budget=a.output.with_name('eiq-'+sha(str(a.output.resolve()).encode())[:12]+'-bounds.json');sys.argv=['reference','--repo',str(a.repo),'--models',str(a.models),'--output',str(budget)];runpy.run_path(str(a.repo/'tools/handwriting/reference_english_case_initial_query_budget.py'),run_name='__main__');b=json.loads(budget.read_bytes());assert(b['rawRecordsUpper'],b['caseRecordsUpper'],b['queryLookupAttemptsUpper'],b['queryNodeUpper'],b['firstConsumerNodeUpper'],b['queryPageCallsUpper'])==(80,400,800,889,1289,5);assert b['initialFFFBPrefixTaken']is False
pool=2048*1024+2*2048*128+65536;assert pool<=3*1024*1024;combined=2907401+16*1024*1024+64*1024*1024+4*1024*1024;assert combined==90987785<128*1024*1024
report=dict(status='PASS STATIC first genuine case-query issuer proposal',fullNativeRegions=len(pins['regions']),orderedNativeBranches=len(branches),boundedLeaves=len(pins['leaves']),dispatchSlots=len(pins['tables']),registered222V248='0x1800ca630',separateV280='0x180155710',writtenInitialTag='0x0000080000000000',firstFFFBPrefixTaken=False,source68IsOnceRefreshPlusExactMapInstall=True,publicInitialCapacityStill104=True,privateExecutionPoolProposal=2048,oldSeamFiles=2,pureGateRefusalStates=guards,productionConcurrencyTested=False,additionalCombinedLogicalEnvelopeUpper=combined,runtimeImplemented=False,wholeWordDecoderReady=False,targetsRead=False,vendorExecuted=False,boundsReport=str(budget));a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
