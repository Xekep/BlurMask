"""Read-only complete native/caller/source certificate for a NEW-only proposal.
No vendor execution, new production execution or history/word authority.
"""
from pathlib import Path
import argparse,hashlib,json,struct,subprocess,sys
import capstone
p=argparse.ArgumentParser()
for x in ('repo','sdk','models','native_export','seams','capture_134','capture_100','categories','geometry_134','geometry_100','output'):
 p.add_argument('--'+x.replace('_','-'),dest=x,type=Path,required=True)
a=p.parse_args();root=Path(__file__).parent;pins=json.loads((root/'english-case-initial-consumer-pins.json').read_bytes())
sha=lambda b:hashlib.sha256(b).hexdigest()
def filehash(path):
 with path.open('rb')as f:return hashlib.file_digest(f,'sha256').hexdigest()
raw=(a.sdk/pins['binary']).read_bytes();assert sha(raw)==pins['sha256']
pe=struct.unpack_from('<I',raw,60)[0];at=pe+24+struct.unpack_from('<H',raw,pe+20)[0];sections=[struct.unpack_from('<IIII',raw,at+40*i+8)for i in range(struct.unpack_from('<H',raw,pe+6)[0])]
def data(address,n):
 rva=address-0x180000000
 for _,r,s,o in sections:
  if r<=rva and n<=r+s-rva:return raw[o+rva-r:o+rva-r+n]
 raise AssertionError(('boundedPE',hex(address),n))
cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);branches=[]
for q in pins['regions']:
 blob=data(int(q['start'],16),q['bytes']);assert sha(blob)==q['sha256']
 ins=list(cs.disasm(blob,int(q['start'],16)));assert sum(x.size for x in ins)==q['bytes']
 for x in ins:
  if x.mnemonic.startswith('j')or x.mnemonic=='call':branches.append({'at':hex(x.address),'instruction':x.mnemonic,'operand':x.op_str,'bytes':x.bytes.hex()})
assert branches==pins['branches']
for q in pins['tables']:
 blob=data(int(q['at'],16),8);assert blob.hex()==q['bytes']and struct.unpack('<Q',blob)[0]==int(q['target'],16)
for q in pins['leaves']:
 blob=data(int(q['start'],16),len(bytes.fromhex(q['bytes'])));assert blob.hex()==q['bytes']
 ins=list(cs.disasm(blob,int(q['start'],16)));assert [{'at':hex(x.address),'instruction':x.mnemonic,'operand':x.op_str}for x in ins]==q['instructions']
for q in pins['cfgLeaves']:
 lo=int(q['start'],16);end=int(q['endExclusive'],16);blob=data(lo,end-lo);assert blob.hex()==q['bytes']and sha(blob)==q['sha256']
 pending=[lo];seen={};edges=[]
 while pending:
  at=pending.pop()
  if at in seen:continue
  assert lo<=at<end
  x=next(cs.disasm(data(at,min(15,end-at)),at));assert x.mnemonic not in ('call','int3')
  seen[at]={'at':hex(at),'instruction':x.mnemonic,'operand':x.op_str,'bytes':x.bytes.hex()}
  if x.mnemonic=='ret':continue
  target=[]
  if x.mnemonic.startswith('j'):
   assert x.op_str.startswith('0x');target.append(int(x.op_str,16))
   if x.mnemonic!='jmp':target.append(at+x.size)
  else:target.append(at+x.size)
  for to in target:assert lo<=to<end;pending.append(to);edges.append([hex(at),hex(to)])
 assert [seen[x]for x in sorted(seen)]==q['reachableInstructions']and sorted(edges)==q['edges']
assert filehash(a.native_export)==pins['exportSha256'];source=a.native_export.read_text(encoding='utf-8');f={x[:9]:x for x in source.split('/* Address ')[1:]};assert len(f)==24
for dep in pins['dependencies']:assert filehash(Path(dep['path']))==dep['sha256']
# Actual preSkip order and the two deep quote ancestry branches. A blanket
# quote refusal and a symbol-only "alwaysfalse" replacement are both wrong.
startup=f['1801fd240'];assert startup.index('FUN_1801f8d60')<startup.rindex('param_2 + 0x1680')
for text in ('plVar11 = (longlong *)*local_188','lVar12 = *plVar11','*(short *)(lVar12 + 0x20) == -6','local_194 !=','param_2 + 0x2300','local_100 + 0x58'):
 assert text in startup,text
# Actual source290 -> aggregate ->292 ->terminal-only2B7 path; fixed map60
# does not use the D0-changing working50 selector.
for text in ('FUN_1802388e0','param_3[0x204] = -1','lVar4 = 8','param_2 + 0x10'):
 assert text in f['180239100'],text
assert '(param_2 + 8) + 0x48)'in f['1802388e0']and 'iVar21 == -1'in f['1802388e0']
assert '(param_2 + 8) + 0x48)'in f['180231ae0']and '& 0x7c'in f['180231ae0']
assert 'uVar8 = *(undefined8 *)(param_2 + 0x60)'in f['180093b50']
assert "param_4 == '\\0'"in f['180093b50']
for name in ('180091e10','1800920a0'):
 assert "param_6 == '\\0'"in f[name]and 'param_4 + 0x1c'in f[name]
loader=f['180248d90'];assert '(local_4a4 & 4) == 0'in loader and 'local_490 == 0'in loader and '*(longlong *)(param_2 + 0x10) = local_490'in loader
prepare=f['180092990'];assert 'pcVar8 = FUN_1800920a0'in prepare and 'pcVar15 = FUN_180091e10'in prepare and 'param_2[0x441]'in prepare and 'param_2[0x442]'in prepare
# Static seam proposal only: require current original bytes and reversible,
# literal numeric extraction. It is deliberately not applied to production.
seams=json.loads((a.seams/'seams.json').read_bytes());assert seams['runtimeApplied']is False and len(seams['oldSeams'])==2
for q in seams['oldSeams']:
 name=Path(q['path']).name;before=(a.seams/(name+'.before')).read_bytes();after=(a.seams/(name+'.proposed')).read_bytes()
 assert sha(before)==q['beforeLfSha256']and sha(after)==q['proposedLfSha256']
 current=(a.repo/q['path']).read_bytes().replace(b'\r\n',b'\n').decode('utf-8-sig').encode();assert current==before
 if name=='LegacyEnglishCaseInitialQueryPass.cs':
  s=after.decode();s=s.replace('internal sealed partial class LegacyEnglishCaseInitialQueryPass','internal sealed class LegacyEnglishCaseInitialQueryPass')
  s=s.replace('                initialConsumer?.Consume(lease,completed,token);\n','').replace('            initialConsumer?.CompletePrefix(lease,token);\n','')
  s=s.replace('Contexts.NodeCount>(initialConsumer?.MaximumContextNodes??889))','Contexts.NodeCount>889)');assert s.encode()==before
  assert after.index(b'initialConsumer?.Consume')<after.index(b'ConsumedRows+=')
  assert after.index(b'initialConsumer?.CompletePrefix')<after.index(b'Root.CompleteInitialQuery')
 else:
  b=before.decode();s=after.decode();assert b.split('        int node = request.ReadInt32(0);',1)[1].split('    private static void Write32(',1)[0]==s.split('        int node = request.ReadInt32(0);',1)[1].split('    private static void Write32<',1)[0]
  assert b.split('        Span<byte> bytes =',1)[1]==s.split('        Span<byte> bytes =',1)[1]
# Full raw source reference + both observed-row censuses, never any target file.
policy_report=a.output.with_name(a.output.stem+'-raw-policy.json')
subprocess.run([sys.executable,'-I','-B',str(a.repo/'tools/handwriting/reference_english_main_word_policy.py'),'--repo',str(a.repo),'--models',str(a.models),'--output',str(policy_report)],check=True)
policy=json.loads(policy_report.read_bytes());assert policy['maximumSourceArcs']==125 and policy['samePolicyDictionaryHeader']==0xc0e and policy['singleDictionarySummary']==3
#Raw selected fields and constructor scalar formulas are data proof; this is
#NOT adoption of the old plain2B7 policy owner as a case290 execution receipt.
assert policy['freshDerivedPolicy']['headers']==2 and policy['freshDerivedPolicy']['primaryAlternatives']==2 and policy['freshDerivedPolicy']['secondaryAlternatives']==1
source_report=a.output.with_name(a.output.stem+'-source.json')
subprocess.run([sys.executable,'-I','-B',str(root/'reference_english_case_source_bounds.py'),'--repo',str(a.repo),'--models',str(a.models),'--output',str(source_report)],check=True)
bound=json.loads(source_report.read_bytes());assert bound['dictionaryNodes']==666547 and bound['dictionaryTerminals']==260074 and not bound['mappedAuxiliaryTerminalOccurrences']and bound['normalCaseOutputUpperBound']==390 and bound['initialRootControlFollowUpperBound']==0
censuses=[]
for n in (134,100):
 output=a.output.with_name(a.output.stem+'-rows-'+str(n)+'.json')
 subprocess.run([sys.executable,'-I','-B',str(root/'reference_english_case_initial_consumer.py'),'--repo',str(a.repo),'--models',str(a.models),'--capture',str(getattr(a,'capture_'+str(n))),'--categories',str(a.categories),'--geometry',str(getattr(a,'geometry_'+str(n))),'--output',str(output)],check=True)
 censuses.append(json.loads(output.read_bytes()))
assert [(x['owners'],x['records'])for x in censuses]==[(134,5658),(100,4544)]and sum(x['quoteAncestryFalseActualRows']for x in censuses)==49
assert all(x['flags']=={'0':x['records']}and x['lengths']=={'0':x['records']}and x['rootLeaves']==x['records']and x['actualZwnjAbsent']for x in censuses)
base=90987785;geometry=32*1024*1024;heap=1024*1024;request=1024*1024;total=base+geometry+heap+request
assert total==126639369 and total<128*1024*1024
assert 400*2*4096<4*1024*1024 and 8*(400*2*4096)<32*1024*1024 and 89+800+400==1289<2048
report={'status':'PASS STATIC complete native/source/row/seam certificate','runtimeApplied':False,'vendorExecuted':False,'targetsRead':False,'wholeWordClaim':False,
 'fullNativeBodies':len(f),'fullRegions':len(pins['regions']),'branches':len(branches),'tables':len(pins['tables']),'completeLeaves':len(pins['leaves']),'completeCfgLeaves':len(pins['cfgLeaves']),
 'actualOwners':234,'actualRows':10202,'quoteRowsAncestryFalse':49,'auxiliaryMappedTerminalEdges':0,'normalPerRequestUpperBound':390,
 'oldSeamsProposed':2,'newProductionFilesProposed':6,'primaryPrefixLogicalBytes':total,'contextNodeReserve':1289,'contextCapacity':2048,
 'scoringAttemptBound':400*2*4096,'heapMoveBound':8*(400*2*4096),'rowOracles':[{'captureSha256':x['captureSha256'],'preparedFrontendProjection':x['preparedFrontendProjection'],'preparedFrontendDigest':x['preparedFrontendDigest']}for x in censuses],
 'scope':'Nearest executable PRIMARY allocation prefix only. No parent88,language execution,completed node or winner. Both outer phases remain future.'}
a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
