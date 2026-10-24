"""Exact current source review plus separately authenticated historical getters.
Header30 semantic changes are never reversed or declared old-equivalent.
"""
import hashlib,json,sys
from pathlib import Path
D=Path(__file__).resolve().parent;root=Path(sys.argv[1]).resolve();pins=json.loads((D/'source-pins.json').read_text(encoding='utf-8'));sha=lambda b:hashlib.sha256(b).hexdigest()
assert set(pins)=={'lfSuccessors','exactOwnGetterReversals','header30HistoricalPreimages','header30CurrentFiles'}
assert len(pins['lfSuccessors'])==7 and set(pins['exactOwnGetterReversals'])==set(pins['header30HistoricalPreimages'])=={'LegacyFreshMainGlobalWordSourceChoice.cs','LegacyFreshMainGlobalWordSourceReceipt.cs'}
assert set(pins['header30CurrentFiles'])=={'LegacyMainWordHeaderUnionBudget.cs','LegacyFreshMainGlobalWordTextPublication.WordHeader.cs'}
texts={}
for name,want in pins['lfSuccessors'].items():
 text=(root/'src/InkRecognition'/name).read_text(encoding='utf-8-sig').replace('\r\n','\n');assert sha(text.encode())==want,('unreviewed current successor',name);texts[name]=text
 if name in pins['exactOwnGetterReversals']:
  #The ORIGINAL complete old file is pinned separately. Only its own getter
  #additions are reversed to check its historical ancestry. Current semantic
  #Header30 source is NOT reversed and cannot pass via this historical file.
  history=pins['header30HistoricalPreimages'][name]
  old=(D/history['Path']).read_bytes();assert sha(old)==history['LfSha256']
  old=old.decode('utf-8');rule=pins['exactOwnGetterReversals'][name]
  for line in rule['removeExactly']:assert old.count(line)==1;old=old.replace(line,'')
  assert sha(old.encode())==rule['oldLfSha256'],('historical getter ancestry changed',name)
  for line in rule['removeExactly']:assert text.count(line)==1
for name,want in pins['header30CurrentFiles'].items():
 text=(root/'src/InkRecognition'/name).read_text(encoding='utf-8-sig').replace('\r\n','\n');assert sha(text.encode())==want,('unreviewed Header30 producer/budget',name);texts[name]=text
receipt=texts['LegacyFreshMainGlobalWordSourceReceipt.cs'];choice=texts['LegacyFreshMainGlobalWordSourceChoice.cs']
assert 'return ordinary.ToArray();'in receipt and 'allChoices[selectedChoice].CopyOrdinaryRanges()'not in receipt
assert receipt.count('ordinary=[]')==1 #fresh initializer only; no reset on reuse
assert 'internal void AppendChoiceSlots(int resultCount)'in receipt
assert choice.index('if(IsComplete)')<choice.index('CopyAssignedScalars().Field40')<choice.index('payload.AppendOrdinaryHeader(choice,token)')<choice.index('links.Add')
assert receipt.index('if(!source.IsOrdinarySource)return;')<receipt.index('ReserveWordHeaderUnion(bound,token)')<receipt.index('ordinary=parent.RangeOperations.Union')
for proof in ['source.Owner.RequireCurrentSource(source,token)','!actual.Payload.IsComplete','ReferenceEquals(actual.Payload.Node.FirstGlobalSource,actual.Payload.Source)','var key=actual.Payload.Node.Key;']:assert proof in receipt
budget=texts['LegacyMainWordHeaderUnionBudget.cs'];assert 'retained>65536||additional>65536'in budget and 'retained>65536-additional'not in budget
holder=texts['LegacyFreshMainGlobalWordTextPublication.WordHeader.cs'];assert holder.index('ValidateLive(token)')<holder.index('if(total>MaximumAllChoiceEnumeratedArrays)')<holder.index('AllWordEnumeratedArrayEnvelopeBytes=total')
print('PASS7 current LF pins +2 current Header30 producer/budget pins;2 separately authenticated historical getter ancestries; no current semantic reversal/equivalence claim')
