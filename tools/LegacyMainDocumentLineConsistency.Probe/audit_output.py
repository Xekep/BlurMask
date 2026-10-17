"""Independent025E40 scan/025940 predicate/56-byte tail replay."""
import base64,json,struct,sys
from pathlib import Path
j=json.loads(Path(sys.argv[1]).read_bytes());b64=base64.b64decode;visits=checks=0
for sample in j['samples']:
 words=sample['words'];assert words and not sample['sourceFlags']&0x200000
 descriptor=b64(sample['descriptor88']);assert len(descriptor)==88 and descriptor[0]&15==0
 assigned=struct.unpack_from('<i',descriptor,16)[0];assert assigned==0
 for word in words:
  assert b64(word['before48'])==b64(word['after48']) and word['chars']
  for c in word['chars']:
   assert c['line']==assigned and b64(c['before24'])==b64(c['after24'])
 maxline=-1;calls=adjacent=0
 for selected in sample['selectedWordIndices']:
  line=words[selected]['chars'][-1]['line'];maxline=max(maxline,line);visits+=1
  if sample['sourceFlags']&0x1000:continue
  for word in reversed(words):assert word['chars'][-1]['line']==line # no024A90
  calls+=1
  for word in words:
   chars=word['chars']
   for a,b in zip(reversed(chars[:-1]),reversed(chars[1:])):
    adjacent+=1
    insert=a['line']!=b['line'] and a['line']!=-1 and b['line']!=-1 and not(a['flags']&0x4000)and not(b['flags']&0x4000)
    assert not insert # native025940 performs no writes on this branch
 before=b64(sample['before56']);after=b64(sample['after56']);assert len(before)==112 and before[56:]==bytes(56)
 assert after==before[:56*(maxline+1)] and len(after)==56
 assert sample['maxLine']==maxline and sample['helperCalls']==calls and sample['adjacent']==adjacent
 assert sample['visits']==len(sample['selectedWordIndices'])==2
 assert sample['pendingBefore']==sample['pendingAfter'] and sample['lastPending']=='TEXT'
 checks+=adjacent
assert len(j['samples'])==4
print(f'PASS independent4 actual owners/{visits} native selected WORD visits/{checks} adjacent CHAR predicates/full48+24 preservation/4 full56-byte trims; no labels or document acceptance.')
