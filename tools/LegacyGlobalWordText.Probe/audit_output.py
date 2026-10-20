"""Independent parent WORD/TEXT/guide/command/prefix composition.

First runs the independent raw AL/V140/CHAR oracle over the paired actual child
capture. No expected word labels enter either recognition or either oracle.
"""
import json,math,runpy,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)!=4:raise SystemExit('MODELS CHILD-CAPTURE PARENT-CAPTURE')
models,child_path,parent_path=sys.argv[1:];saved=sys.argv
try:
 sys.argv=['audit_output.py',models,child_path]
 runpy.run_path(str(root/'tools/LegacyGlobalTextSourceWrapper.Probe/audit_output.py'),run_name='__main__')
finally:sys.argv=saved
child=json.loads(Path(child_path).read_bytes());parent=json.loads(Path(parent_path).read_bytes())
byname={s['name']:s for s in child['samples']};maxid=(1<<63)-1
u32=lambda b,o:struct.unpack_from('<I',b,o)[0]
i32=lambda b,o:struct.unpack_from('<i',b,o)[0]
f32=lambda b,o:struct.unpack_from('<f',b,o)[0]
def same(a,b):return a>>16==b>>16 and(a>>8&255==b>>8&255 or a>>8&255==255 or b>>8&255==255)
def scalar(size,**values):
 b=bytearray(size)
 for offset,value in values.items():struct.pack_into('<i',b,int(offset[1:]),value)
 return b.hex()
def command(identity,name,index,ranges):return dict(id=identity,name=name,command=index,previous=index-1,operation=4,category=100,enabled=1,ranges=ranges)
def guide_lines(sample):
 inputs=[bytes.fromhex(d)for d in sample['descriptors']];guides=[bytes.fromhex(g)for g in sample['guides']]
 count=max([0]+[i32(d,16)+1 for d in inputs if d[0]&15!=3 and i32(d,16)>=0]);out=bytearray(count*56)
 for i in range(count):
  g=guides[i];corrected=f32(g,24)>0
  slope=0.0 if corrected else f32(g,8);height=f32(g,24 if corrected else 16);baseline=float(f32(g,20 if corrected else 12))
  originx=-0.0*baseline;originy=baseline*1.0;angle=math.atan2(float(slope),1.0)+0.0
  for o,value in [(8,originx),(16,originy),(24,angle),(32,float(height)),(48,float(f32(g,12)))]:struct.pack_into('<d',out,i*56+o,value)
  struct.pack_into('<q',out,i*56+40,-1)
 return count,out.hex()
total=commands=unequal=0
for sample in parent['actual']:
 base=byname[sample['name']];assert sample['complete']and not sample['faulted']
 assert sample['nnCached']==sample['nnCurrent']==base['nn']
 assert sample['alternatives']==sample['actualAlternativeCount']==1 and sample['wordIndex']==0
 assert sample['node']==sample['choiceCount']==1
 assert sample['wordKey']==base['wordClip']and sample['ordinary']==base['ordinaryRanges']and sample['textKey']==base['original']
 assert sample['wordSource26']==base['word26'];unequal+=sample['wordKey']!=sample['textKey']
 assert sample['rowUpperBound']==sum(len(bytes.fromhex(g['before']))//40+sample['compactActive']for g in base['glyphs'])<=65536
 assert sample['rangeUpperBound']==sum(len(bytes.fromhex(g['source24']))//24*len(base['rawCounts'])for g in base['glyphs'])<=1048576
 assert len(sample['links'])==len(base['glyphs'])
 char_flags=0
 for link,g in zip(sample['links'],base['glyphs']):
  for key in ['index','reference','node','ordinary','reused','marker','priorVisits','written','source24','residual2','positions','processed','records']:assert link[key]==g[key],(sample['name'],key)
  assert link['line']==g['scalars'][7]and link['metadataFlags']==g['scalars'][8]and link['selectionId']==g['scalars'][0]
  assert link['scalar24']==scalar(24,x16=g['marker'])
  char_flags|=u32(bytes.fromhex(g['records'][g['marker']]['scalar40']),8);total+=1
 assert sample['wordChoiceFlags']==base['selectedFlags']==char_flags
 key=sample['wordKey'];flags=base['sourceFlags']|char_flags
 expected_word_selection=-1
 if flags&0x200000==0:
  expected_word_selection=base['nextAfter'];flags=(flags&~0x800000)|(0x800000 if len(key)==1 and same(*key[0])else 0)
 assert sample['wordFlags']==flags and sample['wordSelection']==expected_word_selection
 assert sample['wordScalars']==[0,-1,expected_word_selection,flags,-1,-1]
 assert sample['wordChoice48']==scalar(48,x8=char_flags,x24=len(sample['links']),x28=len(sample['links']))
 assert sample['documentLink']and sample['documentWord24']==scalar(24,x16=sample['wordIndex'])and sample['documentChoice32']==scalar(32,x8=char_flags&1023,x24=1,x28=1)
 expected=base['commands'].copy();nextid=base['nextAfter']
 if expected_word_selection!=-1:expected.append(command(nextid,'WORD',len(expected)+1,key));nextid+=1
 expected.append(command(nextid,'TEXT',len(expected)+1,base['original']));nextid+=1
 assert sample['commands']==expected and sample['nextBefore']==base['nextBefore']and sample['nextAfter']==nextid
 assert sample['hasText']and sample['textSelection']==nextid-1
 count,storage=guide_lines(sample);assert sample['lineCount']==sample['wrapperOffset']==count and sample['sourceLines56']==storage
 assert sample['childTrace']==base['trace']
 n=len(base['glyphs']);trace=['reserved','document-choice32','word-intern','word-defaults','word-choice48','word-initial-flags']+['positive:'+str(i)for i in range(n)]+['character:'+str(i)for i in range(n)]+['characters-complete','word-final-flags','word-registration-prefix','word-registered','document-word24','source-lines','text-registration-prefix','text-registered']
 assert sample['trace']==trace
 prefixes=sample['prefixes'];assert [p['stage']for p in prefixes]==trace
 for p in prefixes:
  stage=p['stage'];assert p['wordNodes']==(0 if stage in('reserved','document-choice32')else 1)
  assert p['documentAlternatives']==(0 if stage=='reserved'else 1)
  if stage in ['reserved','document-choice32','word-intern']:assert p['assigned']==[None]*5
  if stage=='word-defaults':assert p['assigned']==[0,0,0,0,maxid]
  if stage=='word-choice48':assert p['assigned']==[0,0,1,0,maxid]
  if stage=='word-initial-flags':assert p['assigned']==[0x200000,0,1,0,maxid]
  if stage.startswith('positive:'):assert p['assigned']==[0,0,1,0,maxid]and p['characterNodes']==0 and p['next']==base['nextBefore']
  if stage.startswith('character:'):
   i=int(stage.split(':')[1]);partial=0
   for g in base['glyphs'][:i+1]:partial|=u32(bytes.fromhex(g['records'][g['marker']]['scalar40']),8)
   assert p['assigned']==[0,partial,1,i+1,maxid]and p['characterNodes']==i+1 and p['next']==base['nextBefore']+i+1
 assert prefixes[trace.index('word-final-flags')]['assigned']==[base['sourceFlags']|char_flags,char_flags,1,n,maxid]
 assert prefixes[-1]['assigned']==[flags,char_flags,1,n,expected_word_selection]and prefixes[-1]['next']==nextid
 commands+=len(expected)
assert len(parent['actual'])==13 and total==37 and unequal==2
assert len(parent['preparedFailures'])==13
for sample in parent['preparedFailures']:
 kind=sample['name'].split('-')[1];assert sample['faulted']and not sample['complete'];initial=sample['nextBefore']
 assert sample['documentAlternativeCount']==1
 if kind in('CHAR','WORD'):assert sample['documentChoice32']=='00'*32
 assert sample['node']==sample['choiceCount']==1 and sample['wordScalars'][:2]==[0,-1]and sample['wordScalars'][-2:]==[-1,-1]
 assert sample['trace'][-1]=='fault'and sample['prefixes'][-1]['stage']=='fault'
 if kind=='CHAR':
  assert sample['nextAfter']==initial+1 and sample['commands']==[]and sample['links']==[]and sample['wordSelection']==maxid and not sample['documentLink']
  assert sample['childPayloadPrefix']==[dict(written=22,complete=False,node=1)]
 else:
  assert len(sample['links'])==1 and sample['childPayloadPrefix']==[dict(written=127,complete=True,node=1)]
  assert sample['commands'][0]==command(initial,'CHAR',1,byname['M']['glyphs'][0]['processed'])
 if kind=='WORD':assert sample['nextAfter']==initial+2 and len(sample['commands'])==1 and sample['wordSelection']==maxid and not sample['documentLink']
 if kind in('LINE','TEXT'):
  assert sample['wordSelection']==initial+1 and sample['documentLink']and sample['commands'][1]==command(initial+1,'WORD',2,sample['wordKey'])
 if kind=='LINE':assert sample['nextAfter']==initial+2 and sample['lineCount']==0 and sample['sourceLines56']==''and sample['wrapperOffset']==1 and not sample['hasText']
 elif kind=='TEXT':
  count,storage=guide_lines(sample);assert sample['nextAfter']==initial+3 and sample['lineCount']==sample['wrapperOffset']==count and sample['sourceLines56']==storage and sample['hasText']and sample['textSelection']is None and len(sample['commands'])==2
 else:assert sample['lineCount']==sample['wrapperOffset']==0 and not sample['hasText']
 assert sample['wordFlags']==sample['wordChoiceFlags']==0
assert parent['guards']==190 and parent['preparedBudgetGuards']==5 and parent['preparedLiveStateGuards']==2
assert len(parent['preparedReservationBoundaries'])==2
for cancelled,r in zip((False,True),parent['preparedReservationBoundaries']):
 assert r['cancelled']==cancelled and r['foreignSameDocumentUnchanged']and r['reservationEvents']==1
 assert r['parentFaulted']==r['wrapperFaulted']==r['choicesFaulted']==cancelled
 assert r['wordNodes']==r['charNodes']==(0 if cancelled else 1)and r['nextAfter']==r['nextBefore']+(0 if cancelled else 3)
 assert r['childEntryEvents']==(0 if cancelled else 1)and r['reservedChoicesEnteredGuard']==1
 assert r['childTrace']==(['word-parent-fault']if cancelled else byname['M']['trace'])
assert parent['preparedDegenerateCases']==1024
s=parent['preparedSkipped'];assert s['complete']and not s['faulted']and s['links']==s['childPayloadPrefix']==[]
assert s['wordFlags']==0x200000 and s['wordChoiceFlags']==0 and s['wordSelection']==-1 and s['wordChoice48']=='00'*48
assert s['wordKey']==s['textKey']==byname['M']['original']and s['ordinary']==[]and s['rowUpperBound']==s['rangeUpperBound']==0
assert s['nextAfter']==s['nextBefore']+1 and s['commands']==[command(s['nextBefore'],'TEXT',1,s['textKey'])]
assert s['documentWord24']=='00'*24 and s['documentChoice32']==scalar(32,x24=1,x28=1)
assert s['childTrace']==['claim','descriptor:0','skip:0','first-pass-complete','complete']
count,storage=guide_lines(s);assert s['lineCount']==s['wrapperOffset']==count and s['sourceLines56']==storage
print(f'PASS independent13 global parents/{total} actualCHARlinks/{commands} exactselectioncommands/full24-48-32 bytes/guide56/default-and-counter prefixes;13 prepared failures; unequal WORD/TEXT2 retained, no document coverage claim')
