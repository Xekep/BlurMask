from pathlib import Path
from dataclasses import dataclass
import argparse,copy,hashlib,json,re
@dataclass
class Node:
 kind:str
 value:object
 raw:str=''
def refuse(message):raise ValueError(message)
def need(v,message):
 if not v:refuse(message)
def parse(raw):
 text=raw.decode('utf8');pos=0
 def whitespace():
  nonlocal pos
  while pos<len(text) and text[pos] in ' \t\r\n':pos+=1
 def read():
  nonlocal pos
  whitespace();start=pos;need(pos<len(text),'truncated JSON');ch=text[pos]
  if ch=='{':
   pos+=1;items=[];seen=set();whitespace()
   if pos<len(text) and text[pos]=='}':pos+=1;return Node('object',items)
   while True:
    whitespace();need(pos<len(text) and text[pos]=='"','object key required');key=read();need(key.kind=='string' and key.value not in seen,'duplicate key');seen.add(key.value);whitespace();need(pos<len(text) and text[pos]==':','colon required');pos+=1;items.append((key.value,read()));whitespace();need(pos<len(text),'truncated object')
    if text[pos]=='}':pos+=1;break
    need(text[pos]==',','object delimiter');pos+=1
   return Node('object',items)
  if ch=='[':
   pos+=1;items=[];whitespace()
   if pos<len(text) and text[pos]==']':pos+=1;return Node('array',items)
   while True:
    items.append(read());whitespace();need(pos<len(text),'truncated array')
    if text[pos]==']':pos+=1;break
    need(text[pos]==',','array delimiter');pos+=1
   return Node('array',items)
  if ch=='"':
   value,end=json.decoder.scanstring(text,pos+1,True);pos=end;return Node('string',value,text[start:pos])
  for literal,kind,value in [('true','boolean',True),('false','boolean',False),('null','null',None)]:
   if text.startswith(literal,pos):pos+=len(literal);return Node(kind,value,literal)
  m=re.match(r'-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?',text[pos:]);need(m is not None,'number required');pos+=len(m[0]);return Node('number',m[0],m[0])
 result=read();whitespace();need(pos==len(text),'trailing JSON');return result
def obj(n):need(n.kind=='object','object type');return dict(n.value)
def integer(n):need(n.kind=='number' and re.fullmatch(r'-?(?:0|[1-9]\d*)',n.raw) is not None,'strict integer required');return int(n.raw)
def scalar(n,expected):
 if type(expected) is bool:need(n.kind=='boolean' and n.value is expected,'boolean witness')
 elif type(expected) is int:need(integer(n)==expected,'integer witness')
 else:need(n.kind=='string' and n.value==expected,'string witness')
def equal(a,b,path='prior'):
 need(a.kind==b.kind,path+': type')
 if a.kind=='object':
  need([k for k,v in a.value]==[k for k,v in b.value],path+': properties/order')
  for (k,x),(_,y) in zip(a.value,b.value):equal(x,y,path+'.'+k)
 elif a.kind=='array':
  need(len(a.value)==len(b.value),path+': length')
  for i,(x,y) in enumerate(zip(a.value,b.value)):equal(x,y,path+f'[{i}]')
 elif a.kind=='string':need(a.value==b.value,path+': exact string value')
 else:need(a.raw==b.raw,path+': exact numeric/boolean/null scalar')
ORIGIN_KEYS=['Source','OriginalObjects','OriginalPoints','InputId','OriginalNext','SelectedNextBeforePublication','PrecancelExactToken','ClosedLeaseRefused','SecondEntryRefused','DisposedTwiceAfterCompletion','NeverRunDisposeExecuted','EnteredCancelInjected','FailedPrepareInjected','ExtraLearnedParentRun','NativeSDKSelectedReaderOriginProven','Scope']
def audit(prior,actual,strokes):
 need(actual.kind=='object' and prior.kind=='object','capture objects')
 need([k for k,v in actual.value]==[k for k,v in prior.value]+['OriginalInput'],'only additive OriginalInput')
 equal(Node('object',actual.value[:-1]),prior)
 # Counts derive from authenticated ORIGINAL raw input, not target vectors.
 need(strokes.kind=='array','raw strokes')
 count=len(strokes.value);points=sum(len(s.value) for s in strokes.value);need(count==1 and points==3,'fixed unchanged index4')
 values=obj(actual);origin=values['OriginalInput'];need([k for k,v in origin.value]==ORIGIN_KEYS,'origin exact schema/order');o=obj(origin)
 expected={'Source':'PRIVATE_PREPARE_ORIGINAL_SELECTED_BEFORE_PARENT_PREPARE','OriginalObjects':count,'OriginalPoints':points,'InputId':0,'OriginalNext':1,'SelectedNextBeforePublication':1,'Scope':'ONE_REAL_PARENT_AND_OWNING_PASS; NEVER_RUN_AND_ENTERED_FAILURE_LIFETIME_SOURCE_ONLY'}
 for key in ['PrecancelExactToken','ClosedLeaseRefused','SecondEntryRefused','DisposedTwiceAfterCompletion']:expected[key]=True
 for key in ['NeverRunDisposeExecuted','EnteredCancelInjected','FailedPrepareInjected','ExtraLearnedParentRun','NativeSDKSelectedReaderOriginProven']:expected[key]=False
 for key,value in expected.items():scalar(o[key],value)
 need(integer(values['RawStrokes'])==count and integer(values['RawPoints'])==points,'original counts join')
 scalar(values['Completed'],True);scalar(values['DocumentApplied'],False);scalar(values['Admission'],'NONE');scalar(values['NoExpectedLabels'],True);scalar(values['GlobalReadinessGateUnchanged'],True)
 return dict(OldFields=len(prior.value),NewFields=1,RawObjects=count,RawPoints=points,OriginalInputId=0,Next=[1,1],LifecycleObservations=4,EntireOldProjectionExact=True,NumericBooleanNullSpellingExact=True,JSONStringsValueExact=True,DocumentApplied=False)
def main():
 p=argparse.ArgumentParser();p.add_argument('prior',type=Path);p.add_argument('actual',type=Path);p.add_argument('input',type=Path);p.add_argument('output',type=Path);p.add_argument('--tamper',action='store_true');p.add_argument('--fixed-slice',action='store_true');a=p.parse_args()
 rawfiles=[a.prior.read_bytes(),a.actual.read_bytes(),a.input.read_bytes()]
 need(hashlib.sha256(rawfiles[0]).hexdigest()=='fef0f39bf0fb118a8d6fa60f8d534f3a46864ab29df0b8588d4966eb10af31c8','historical prior pin')
 need(hashlib.sha256(rawfiles[2]).hexdigest()==('115dbb6ca0d8fff88b6f3c9c79324f16fc8ab420331543b581c3ab07937c29e6' if a.fixed_slice else 'd1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99'),'original raw pin')
 prior,actual,raw=map(parse,rawfiles)
 if a.fixed_slice:
  item=obj(raw);need([k for k,v in raw.value]==['Original141SHA256','OriginalIndex','Raw','Resolution'],'fixed raw slice schema')
  scalar(item['Original141SHA256'],'d1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99');scalar(item['OriginalIndex'],4);scalar(item['Resolution'],96);strokes=item['Raw']
 else:
  need(raw.kind=='array' and len(raw.value)==141,'original141');strokes=obj(raw.value[4])['Raw']
 details=audit(prior,actual,strokes);neg=[]
 def test(name,change):
  v=copy.deepcopy(actual);change(v)
  try:audit(prior,v,strokes)
  except ValueError:neg.append(name)
  else:raise AssertionError('tamper accepted:'+name)
 def field(v,name):return obj(obj(v)['OriginalInput'])[name]
 if a.tamper:
  for key in ['OriginalObjects','OriginalPoints','InputId','OriginalNext','SelectedNextBeforePublication']:
   test(key+' increment',lambda v,k=key:setattr(field(v,k),'raw',str(integer(field(v,k))+1)))
   test(key+' bool',lambda v,k=key:(setattr(field(v,k),'kind','boolean'),setattr(field(v,k),'value',True),setattr(field(v,k),'raw','true')))
   test(key+' float',lambda v,k=key:setattr(field(v,k),'raw',str(integer(field(v,k)))+'.0'))
  for key in ['PrecancelExactToken','ClosedLeaseRefused','SecondEntryRefused','DisposedTwiceAfterCompletion','NeverRunDisposeExecuted','EnteredCancelInjected','FailedPrepareInjected','ExtraLearnedParentRun','NativeSDKSelectedReaderOriginProven']:
   test(key+' flipped',lambda v,k=key:(setattr(field(v,k),'value',not field(v,k).value),setattr(field(v,k),'raw','false' if field(v,k).value else 'true')))
   test(key+' integer',lambda v,k=key:(setattr(field(v,k),'kind','number'),setattr(field(v,k),'raw','1')))
  test('missing origin field',lambda v:obj(v)['OriginalInput'].value.pop())
  test('extra origin field',lambda v:obj(v)['OriginalInput'].value.append(('Unknown',Node('null',None,'null'))))
  test('origin reordered',lambda v:obj(v)['OriginalInput'].value.reverse())
  test('missing old field',lambda v:v.value.pop(1))
  test('extra top field',lambda v:v.value.append(('Unknown',Node('null',None,'null'))))
  test('source label',lambda v:setattr(field(v,'Source'),'value','DATA_RECONSTRUCTED'))
  test('scope label',lambda v:setattr(field(v,'Scope'),'value','WHOLE_SDK_PROVEN'))
  test('old metric changed',lambda v:setattr(obj(obj(v)['Metrics'])['ScoringCalls'],'raw','332'))
  test('old scalar spelling',lambda v:setattr(obj(v)['RawPoints'],'raw','3e0'))
  test('old numeric negative zero',lambda v:setattr(obj(v)['OriginalIndex'],'raw','-0'))
  test('old boolean type',lambda v:(setattr(obj(v)['DocumentApplied'],'kind','number'),setattr(obj(v)['DocumentApplied'],'raw','0')))
  # Decoder-level duplicates/NaN must refuse before any semantic join.
  for name,bad in [('duplicate JSON key',b'{"a":1,"a":2}'),('NaN JSON',b'NaN')]:
   try:parse(bad)
   except ValueError:neg.append(name)
   else:raise AssertionError(name)
 report=dict(Status='PASS_DATA_ONLY',ActualSHA256=hashlib.sha256(rawfiles[1]).hexdigest(),Details=details,TamperRefusals=len(neg),TamperNames=neg,RuntimeScope='ONE_FIXED_ORIGINAL_141_INDEX4; four captured guard observations backed by pinned SOURCE guards, no independent exception reinjection',Unverified=['native SDK C20 dispatcher and frontend raw167210 reader origin','arbitrary external native field/property policy','neverRun Dispose or entered cancellation/failure runtime injection','whole-pipeline NN-forward count, CPU/RSS/CLR bound, quality and public readiness'],CSharpExecuted=False,OwningCalls=0,NNCalls=0)
 with a.output.open('x',encoding='utf8') as f:json.dump(report,f,indent=2);f.write('\n')
if __name__=='__main__':main()
