"""Independent raw AL/C21/UTF8 replay of one shared0283B0 WORD tree.

The committed selected-WORD oracle supplies scalar emission and range algebra;
this test owns the shared ordered replay, new/reuse strings and CHAR/WORD IDs,
all48/24/32-byte records and actual279 selected indices. No expected text.
"""
import base64,importlib.util,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
# Reuse the prior independently tested primitive definitions, not its test loop.
primitive=root/'tools/LegacyMainConfiguredTextSourceWrapper.Probe/audit_output.py'
namespace={'__file__':str(primitive)}
exec(compile(primitive.read_text(encoding='utf-8').split('capture=json.loads(')[0],str(primitive),'exec'),namespace)
ops=namespace['ops'];b64=base64.b64decode
same=namespace['same'];positive=namespace['positive'];skipped=namespace['skipped']
emission=namespace['emission'];alphabet=namespace['alphabet'];u16=namespace['u16']
capture=json.loads(Path(sys.argv[1]).read_bytes());total=reuses=word_count=record_count=0
for sample in capture['samples']:
 counts=sample['counts'];next_id=len(counts);pending=[];nodes={};strings={};deferred=[]
 original=[[3,((len(counts)-1)<<32)|((counts[-1]-1)<<16)|0xff05]]
 all_ordinary=[];all_flags=0x200000
 assert [w['ordinal']for w in sample['words']]==list(range(len(sample['words'])))
 for word in sample['words']:
  assert word['wordClip']==namespace['word_projection'](b64(word['wordSource26']),counts)
  inputs=word['inputs'];ranges=[];skip=[];ispositive=[];joiner=[]
  for item in inputs:
   row=b64(item['initialGlyph40']);s=skipped(row);p=positive(item['reference'])
   skip.append(s);ispositive.append(p);joiner.append(u16(row,4)&7==0 and alphabet[u16(row,8)]=='\u200d')
   key=item.get('originalRanges',[]);ranges.append(ops.intersect(key,word['wordClip'])if p else key)
  source_flags=0 if any(p and not s for p,s in zip(ispositive,skip))else 0x200000
  assert word['sourceFlags']==source_flags
  if not source_flags:all_flags&=~0x200000
  assert [g['index']for g in word['glyphs']]==[i for i,s in enumerate(skip)if not s]
  ordinary=[];selected_flags=0
  for glyph in word['glyphs']:
   i=glyph['index'];key=ranges[i];p=ispositive[i]
   if p:
    if len(key)==1 and same(*key[0])and not any(ops.n(a)<=ops.n(key[0][0])<=ops.n(b)and key[0][1]<=b for a,b in original):
     p=False;key=[[(key[0][0]&~255)|4,(key[0][1]&~255)|4]]
    elif joiner[i]:
     end=key[0][1]
     if end>>8&255==255:end&=~0xff00
     end=(end&~255)|4;key=[[end,end]];p=False
    else:
     for later in range(i+1,len(inputs)):
      if skip[later]or not ispositive[later]:continue
      overlap=ops.intersect(ops.union([],ranges[later]),key)
      if not overlap:continue
      if key!=overlap:key=ops.subtract(key,overlap,counts)
      else:ranges[later]=ops.subtract(ranges[later],overlap,counts)
      break
   ranges[i]=key;assert glyph['key']==key and glyph['positive']==p
   packed=tuple(tuple(r)for r in key);existing=nodes.get(packed)
   assert glyph['reused']==(existing is not None)
   emitted,marker=emission(glyph)
   if existing is None:
    records=[];aggregate=0
    for raw,flags in emitted:
     identity=strings.setdefault(raw,len(strings)+1);records.append((raw,flags,identity));aggregate|=flags
    selection_id=next_id if p else -1
    if p:pending.append(dict(id=next_id,name='CHAR',ranges=key));next_id+=1
    visited=any(r[0]<=r[3]and r[1]<=r[4]for r in glyph['sourceRefs24'])
    flags=(((int(p and not visited)<<24)^((0 if p else 0x200000)|(aggregate&0xce1fffff)|
      (0x800000 if p and len(key)==1 and same(*key[0])else 0)))&0xfdffffff)
    existing=dict(node=len(nodes)+1,records=records,key=key,selection=selection_id,flags=flags)
    nodes[packed]=existing
   else:
    reuses+=1;old_count=len(existing['records']);marker_out=-1
    for index,(raw,flags)in enumerate(emitted):
     newly=raw not in strings;identity=strings.setdefault(raw,len(strings)+1)
     match=next((j for j,r in enumerate(existing['records'][:old_count])if r[2]==identity),None)if not newly else None
     if match is None:match=len(existing['records']);existing['records'].append((raw,flags,identity));existing['flags']|=flags
     if index==marker:marker_out=match
    marker=marker_out
   assert glyph['node']==existing['node']and glyph['storedKey']==existing['key']
   assert glyph['selectionId']==existing['selection']and glyph['marker']==marker
   assert b64(glyph['selectedUtf8'])==existing['records'][marker][0]
   selected_flags|=existing['records'][marker][1]
   if p:
    ordinary=ops.union(ordinary,existing['key']);all_ordinary=ops.union(all_ordinary,existing['key'])
   # Captures retain the live shared CHAR payload after later WORD rows.
   deferred.append((glyph,existing,ispositive[i]))
   total+=1
  assert word['ordinaryRanges']==ordinary and word['selectedFlags']==selected_flags
  all_flags|=selected_flags
  expected=bytearray(48);struct.pack_into('<I',expected,8,selected_flags);struct.pack_into('<ii',expected,24,len(word['glyphs']),len(word['glyphs']))
  assert b64(word['scalar48'])==expected
  assert len(word['links24'])==len(word['glyphs'])
  for link,glyph in zip(word['links24'],word['glyphs']):
   expected=bytearray(24);struct.pack_into('<i',expected,16,glyph['marker']);assert b64(link)==expected
  word_count+=1
 for glyph,existing,original_positive in deferred:
  assert glyph['flags']==existing['flags']and glyph['line']==(sample['line']if original_positive else -1)
  assert len(glyph['records'])==len(existing['records'])
  for actual,(raw,flags,identity)in zip(glyph['records'],existing['records']):
   expected=bytearray(40);struct.pack_into('<I',expected,8,flags);struct.pack_into('<I',expected,32,len(raw))
   assert b64(actual['scalar40'])==expected and b64(actual['utf8'])==raw and actual['stringId']==identity
   record_count+=1
 assert sample['sourceKey']==sample['words'][0]['wordClip']and sample['ordinaryRanges']==all_ordinary
 if all_flags&0x200000:assert sample['selectionId']==-1
 else:
  assert sample['selectionId']==next_id
  pending.append(dict(id=next_id,name='WORD',ranges=sample['sourceKey']));next_id+=1
  key=sample['sourceKey'];all_flags=(all_flags&~0x800000)|(0x800000 if len(key)==1 and same(*key[0])else 0)
 assert sample['flags']==all_flags and sample['pending']==pending and sample['nextId']==next_id
 assert len(sample['alternatives'])==2
 for index,a in enumerate(sample['alternatives']):
  assert a['reused']==bool(index)
  selected=a['wordIndex'];assert selected==sample['nativeSelectedWordIndices'][index]and 0<=selected<len(sample['words'])
  expected=bytearray(24);struct.pack_into('<i',expected,16,selected);assert b64(a['scalar24'])==expected
  expected=bytearray(32);struct.pack_into('<I',expected,8,sample['words'][selected]['selectedFlags']&1023);struct.pack_into('<ii',expected,24,1,1)
  assert b64(a['scalar32'])==expected
assert capture['sharedSpanWordChoices']and len(capture['samples'])==3
assert reuses>0,'This actual fixture must exercise shared CHAR source reuse.'
print(f'PASS independent3 shared275 spans/{word_count} WORD48/{total} CHAR24/{record_count} full40B/{reuses} genuine key reuses/sharedUTF8/CHAR+WORD pending IDs; no TEXT acceptance.')
