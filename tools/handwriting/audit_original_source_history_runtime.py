"""Read-only native deletion/history proof and exact owning runtime joins."""
import argparse,hashlib,json,subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,default=root);a=p.parse_args()
a.output.parent.mkdir(parents=True,exist_ok=True);dep=Path(str(a.output)+'.native.json')
subprocess.run([sys.executable,str(a.native_repo/'tools/handwriting/audit_original_acceptance_source_history.py'),str(dep)],cwd=a.native_repo,check=True)
native=json.loads(dep.read_bytes());assert native['pins']>=213 and len(native['regions'])>=30 and len(native['bindings'])>=21
# Default original C5E also receives the C20 observer loop. Its delete slots
# are void no-ops. V40 only releases per-C22 caches; the two constructor class
# definitions have no destroy callback. Do not infer an empty class registry.
sys.path.insert(0,str(a.native_repo/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
ink=Binary('MyScriptInk.dll');extra=[];md=Cs(CS_ARCH_X86,CS_MODE_64)
for table,slot,target in [(0x1800e55e0,0xb0,0x180020d60),(0x1800e55e0,0xb8,0x180020d60),
                          (0x1800e5740,0x40,0x1800757e0),(0x1800e5528,0x20,0),(0x1800e5710,0x20,0)]:
    assert ink.qwords(table+slot,1)==(target,)
    extra.append(dict(table=hex(table),slot=hex(slot),target=hex(target)))
def pins(start,end,values):
    raw=ink.data[ink.off(start):ink.off(end)]
    actual=[(i.address,i.mnemonic,i.op_str)for i in md.disasm(raw,start)]
    for value in values:assert value in actual,value
    return dict(start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest(),pins=len(values))
extras=[pins(0x180073450,0x18007365e,[
    (0x1800735a6,'lea','r8, [rip + 0x71f7b]'),(0x1800735b9,'call','0x180071f40'),
    (0x180073610,'lea','r8, [rip + 0x720f9]'),(0x180073623,'call','0x180071f40')]),
 pins(0x1800757e0,0x1800758eb,[
    (0x18007585d,'call','0x180072c30'),(0x18007586e,'mov','rax, qword ptr [rcx + 0x20]'),
    (0x180075875,'je','0x180075884'),(0x180075890,'call','0x1800727a0')])]
assert 0x1800735ad+0x71f7b==0x1800e5528 and 0x180073617+0x720f9==0x1800e5710
checks={
 'LegacyLibraryOriginalTextFieldBootstrap.cs':[
  'private LegacyLibraryOriginalSourceHistory? sourceHistory;',
  'if(sourceHistory is not null)throw new InvalidOperationException(',
  'var value=new LegacyLibraryOriginalSourceHistory(this);sourceHistory=value;return value;',
 ],
 'LegacyLibraryOriginalSourceHistory.cs':[
  'HistoricalOriginal=original.State;',
  'var value=LegacyLibraryOriginalSelectedFieldContext.Prepare(Original,actualInput,token);selected=value;return value;',
  'ReferenceEquals(replay.OriginalSelectedInitialContext,selected)',
  'ReferenceEquals(replay.Scope.Context.InputBinding.OriginalSelected,selected)',
  'replay.InitialRegistryIds.SequenceEqual(HistoricalOriginal.Inputs.Select(e=>e.RegistryId))',
  'commands=TransactionOwner.CopyPendingCommands().ToArray()',
  's.SelectionId!=expected++',
  'ranges.Length>65536||i==commands.Length-1||total>1048576',
  'owner.CurrentView.ExportIdentityTransforms(ranges,token)',
  'c.Counter is not{} end||end.OldCounter20!=initial.NextRegistryId||end.NewCounter18!=expected',
  'DocumentOwner.WithOriginalSourceApplication(this,()=>Owner.WithApplication(this,()=>',
  'kernel.Add(selection,i==observer,token)',
  'success=kernel.Completed==commands.Length',
  'if(!entered||rollbackEntered||!kernel.JournalComplete)',
  'rollbackEntered=true;kernel.Recording=false',
  'for(int i=kernel.Journal.Count-1;i>=0;i--)',
  'kernel.Delete(kernel.Journal[i]);kernel.Reversed++',
  'HasEntered&&ExecutionSucceeded&&JournalComplete&&!IsFaulted&&!WasRolledBack',
  'Volatile.Read(ref state)',
 ],
 'LegacyPreparedOriginalSourceJournal.cs':[
  'Journal.Clear();JournalComplete=false;',
  'Entries.Add(new(owner,command.SelectionId,command.Name,command.Category,interned,command))',
  'if(historyClone is not null)Journal.Add(new(owner,command,historyClone))',
  'if(Next>=command.NewCounter18)',
  'if(entry.SourceCommand is null||!ReferenceEquals(entry.SourceCommand,record.SourceCommand))',
  'Entries.RemoveAt(index);if(record.RegistryId==Maximum)MaximumDirty=true',
  'if(oldValue==-1||newValue==-1)return false',
  'if(maximum<oldValue)result=oldValue',
 ],
 'LegacyLibraryTextPublicationDocument.cs':[
  'public LegacyLibraryOriginalSourceApplication? OriginalSourceApplication',
  'originalSourceApplication?.ValidateForLibraryApply(token)',
  'ReferenceEquals(compatibility,originalCompatibility)',
  'ReferenceEquals(compatibility.Replay,registryReplay)||registryReplay.HasEntered',
  'var value=history.ClaimApplication(this,compatibility,token);originalSourceApplication=value;return value;',
  'ReferenceEquals(actual,originalSourceApplication)',
  'using var operation=EnterDomCompatibility(token);return action();',
 ]}
sources={};count=0
for name,needles in checks.items():
    file=root/'src/InkRecognition'/name;text=file.read_text(encoding='utf-8')
    for token in needles:assert token in text,(name,token)
    sources[name]=dict(sha256=hashlib.sha256(file.read_bytes()).hexdigest(),checks=len(needles));count+=len(needles)
k=(root/'src/InkRecognition/LegacyPreparedOriginalSourceJournal.cs').read_text()
assert k.index('Journal.Clear();JournalComplete=false;')<k.index('Entries.Add(new(owner,command.SelectionId')<k.index('if(historyClone is not null)Journal.Add(')
delete=k[k.index('internal void Delete('):k.index('internal static bool ReverseCounter')]
assert 'Next='not in delete and 'Next ='not in delete
assert delete.index('CB7:clear-events')<delete.index('C4D:B8:skip-recording0')
d=(root/'src/InkRecognition/LegacyLibraryTextPublicationDocument.cs').read_text()
assert d.index('originalSourceApplication?.ValidateForLibraryApply(token)')<d.index('target.ApplyPrepared(')
s=(root/'src/InkRecognition/LegacyLibraryOriginalSourceHistory.cs').read_text()
assert s.index('total>1048576')<s.index('kernel=new(owner)')<s.index('private LegacyLibraryOriginalSourceState RunCore')
assert s.index('token.ThrowIfCancellationRequested();entered=true')<s.index('kernel.Add(selection,i==observer,token)')<s.index('library-source-frame:end')
a.output.write_bytes((json.dumps(dict(nativeProof=native,defaultSourceBindings=extra,defaultSourceRegions=extras,
 runtimeSources=sources,consumptionChecks=count,orderedJoins=6,
 vendorExecution=False,scope='library original source application/generated journal/explicit inverse; no original DOM/CBB/page acceptance'),indent=2)+'\n').encode())
print(f'PASS native30/21/213 +default C5E2regions/5bindings/8pins plus{len(sources)} runtime sources/{count} consumption checks/6 ordered joins; no vendor execution.')
