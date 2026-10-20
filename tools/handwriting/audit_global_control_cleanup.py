"""Compose pinned native023260 boundaries with the genuine-global owner contract."""
import argparse,hashlib,json,runpy,sys
from pathlib import Path
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,required=True);a=p.parse_args()
root=Path(__file__).resolve().parents[2];dependency=a.output.with_name(a.output.stem+'-native.json');saved=sys.argv
try:
 sys.argv=['audit_main_document_control_cleanup.py',str(dependency)]
 runpy.run_path(str(a.native_repo/'tools/handwriting/audit_main_document_control_cleanup.py'),run_name='__main__')
finally:sys.argv=saved
source=(root/'src/InkRecognition/LegacyFreshMainGlobalControlCleanup.cs').read_text(encoding='utf8')
rules=['ConditionalWeakTable<LegacyFreshMainGlobalDocumentSourceIndices,Entry>',
 'indices.ValidateCurrent(token);','if(entry.Claimed)throw','entry.Claimed=true;return result',
 'if(entered)throw','SourceIndices.ValidateCurrent(token);entered=true',
 'State.Publication.AlternativeCount!=1','State.Publication.WordSource.CopyChoiceScalar48()',
 'BinaryPrimitives.ReadUInt32LittleEndian(word48.AsSpan(8))','wordFlags&=0xffefffff;wordCleared=true',
 'State.Publication.Wrapper.CharacterInterner.CopyInsertionOrder()',
 'insertion.Count!=byId.Count','ReferenceEquals(node,character.OriginalSource.Node)',
 'RequireNoCharacterControl(character.Flags)','if((flags&0x100000)!=0)',
 'complete=true;Record("complete")','catch{faulted=true;Record("fault");throw;}',
 'if(!complete||faulted)throw','return characters.ToArray()','return prefixes.ToArray()']
for rule in rules:assert rule in source,rule
assert source.index('RequireNoWordControl(BinaryPrimitives')<source.index('wordFlags&=0xffefffff')<source.index('foreach(var node in insertion)')
assert 'LegacyFreshMainDocumentSourceIndices'not in source and 'LegacyFreshMainDocumentSelectedSources'not in source
a.output.write_text(json.dumps(dict(status='PASS genuine-global no-control023260 composition',contracts=len(rules)+2,nativeDependencySha256=hashlib.sha256(dependency.read_bytes()).hexdigest(),vendorExecution=False,unimplemented=['active022960','active034D90','037E80','026EF0','DOM acceptance']),indent=2)+'\n')
print('PASS actual global023260 ownership/lifecycle/order plus pinned native no-control boundary; no old configured owner.')
