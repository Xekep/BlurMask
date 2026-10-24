import base64,struct

def audit_pool_growth(x,raw,prior):
 before=x['BeforeText'];after=x['AfterText'];p=dict(prior['AfterState']);p['First']=p.pop('SpanFirst');p['Last']=p.pop('SpanLast');assert before==p,'Actual TEXT prestate differs from reviewed completed Aug DATA'
 for field in ['First','Last','Words','SelectedOrdinals']:assert before[field]==after[field],field
 bp=before['Pool'];ap=after['Pool'];assert len(bp)==len(ap) and [p['Key'] for p in bp]==[p['Key'] for p in ap],'Owner addition/removal/reorder'
 compact=base64.b64decode(raw['compact']);assert len(compact)%40==0 and raw['count']<=len(compact)//40
 starts=raw['starts'];assert len(starts)==len(raw['graph'])+1 and starts[0]==0 and starts[-1]==raw['count'] and all(a<=b for a,b in zip(starts,starts[1:]))
 keys=[]
 for g in x['Text']['Glyphs']:
  ref=g['Reference']
  # mask strip signed alternative ordinal40..55, preserving bit56 only
  key=(ref&0x000000ffffffffff)|(ref&0x0100000000000000)
  if key not in keys:keys.append(key)
 report=[]
 for b,a in zip(bp,ap):
  old=base64.b64decode(b['Records']);new=base64.b64decode(a['Records']);assert len(old)%40==len(new)%40==0
  assert new[:len(old)]==old,'Existing glyph record prefix mutated'
  assert b['PackedCount']==(len(old)//40*4)|(b['PackedCount']&0x8000)
  assert a['PackedCount']==(len(new)//40*4)|(b['PackedCount']&0x8000),'Packed count/flags'
  key=b['Key'];expected=[]
  if key in keys:
   node=key&0xffffff;arc=(key>>24)&0xffff;assert node<len(raw['graph']) and arc<len(raw['graph'][node]),'This DATA audit admits actual ordinary base source only'
   priorids=[struct.unpack_from('<i',old,i)[0] for i in range(0,len(old),40)]
   for i in range(starts[node],starts[node+1]):
    row=compact[i*40:(i+1)*40];traits=row[32]
    if traits&32 and traits&7==0 and struct.unpack_from('<Q',row,8)[0]==key:
     descriptor=i-starts[node]
     # Actual captured subset has no duplicate descriptor requiring update of old row.
     assert descriptor not in priorids,'Duplicate-descriptor update outside this prefix-only audit subset'
     expected.append(descriptor);priorids.append(descriptor)
  actual=[struct.unpack_from('<i',new,i)[0] for i in range(len(old),len(new),40)]
  assert actual==expected,'Exact compact-derived appended descriptor order/count'
  # AppendDescriptor initializes ordinary40 scalar/link lanes; producer fills score/cost/symbol.
  for i,descriptor in enumerate(expected):
   row=new[len(old)+i*40:len(old)+(i+1)*40];assert struct.unpack_from('<H',row,4)[0]&7==0 and row[6:8]==b'\0\0' and row[10:12]==b'\0\0'
   assert row[24:32]==bytes(8) and row[32:40]==b'\xff'*8,'AppendDescriptor scalar/link defaults'
  report.append(dict(Key=key,BeforeRows=len(old)//40,AfterRows=len(new)//40,AppendedDescriptorIds=actual))
 assert len(keys)<=128 and all(k in [p['Key'] for p in bp] for k in keys)
 return report
