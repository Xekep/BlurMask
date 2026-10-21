"""Read-only native C1E/base-kind closure and actual typed runtime contracts."""
from pathlib import Path
import json,argparse,sys,hashlib
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);p.add_argument('--native-repo',type=Path,required=True);a=p.parse_args()
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(a.native_repo/'research/myscript/scripts'))
from inspect_text_features import Binary
pins=json.loads((root/'tools/handwriting/global-raw-object-kinds-pins.json').read_text());sha=lambda b:hashlib.sha256(b).hexdigest();md=Cs(CS_ARCH_X86,CS_MODE_64)
bins={n:Binary(n)for n in pins['dllSha256']}
for n,h in pins['dllSha256'].items():assert sha(bins[n].data)==h
for row in pins['regions']:
 b=bins[row['module']];s=int(row['start'],16);e=int(row['endExclusive'],16);raw=b.data[b.off(s):b.off(e)]
 assert len(raw)==row['bytes']and sha(raw)==row['sha256']
 actual=[dict(address=hex(i.address),bytes=i.bytes.hex(),mnemonic=i.mnemonic,operands=i.op_str)for i in md.disasm(raw,s)]
 assert actual==row['instructions']and sum(len(bytes.fromhex(i['bytes']))for i in actual)==len(raw)
for item in pins['bindings']:assert bins[item['module']].qwords(int(item['address'],16),1)[0]==int(item['value'],16)
for item in pins['tables']:
 b=bins[item['module']];s=int(item['start'],16);raw=bytes.fromhex(item['hex']);assert b.data[b.off(s):b.off(s)+len(raw)]==raw and sha(raw)==item['sha256']
for source in pins['sourceHashes']:assert sha((root/source['path']).read_text().encode())==source['lfSha256']
selected=(root/'src/InkRecognition/LegacyFreshMainGlobalDocumentSelectedSources.cs').read_text();start=selected.index('    internal static void ValidateMetadataBranch(');end=selected.index('    internal static uint ComputeWordMetadata',start)
assert sha(selected[start:end].encode())==pins['unchangedScalarValidateSha256']
source=(root/'src/InkRecognition/LegacyGlobalRawObjectKinds.cs').read_text()
rules=['private LegacyGlobalRawObjectKinds(', 'coverage.Validate(token)',
 'scope.Context.OriginalSelectedInput,actualSelected','actualSelected.CurrentView,actualView',
 'actualSelected.Original.CurrentView,actualView','scope.Document,actualSelected.Document','scope.Document,actualView.Document',
 'current.Curves.Count is<1 or>256','points>4096','current.Views.Count!=current.Curves.Count',
 'curve.Document,document','curve.OriginalSourceIndex!=i','curve.NativeConcreteType!=0xc49',
 'entry.Curve,curve','descriptor.SourceIndex!=(uint)i','descriptor.IsKnownText','descriptor.LastPoint!=curve.PointCount-1',
 'curves=current.Curves.ToArray()','entries=current.Views.ToArray()',
 'result.ValidateSlot(i)','checkpoint?.Invoke(i+1)','result.Validate(token);return result',
 'sourceIndex>=(uint)curves.Length','ValidateSlot((int)sourceIndex)',
 'return new(curve,(int)sourceIndex,curve.NativeConcreteType,0xc1e)',
 '(flags&0x30600000)==0&&count==1&&key.Length==16',
 'LegacyFreshMainGlobalDocumentSelectedSources.ValidateMetadataBranch(flags,scalars.SelectionId!.Value,count,key)',
 'source.Owner,parent.Wrapper','source.Node.Owner,parent.Wrapper.CharacterInterner',
 'source.Node.FirstGlobalSource,source.Source','source.CopyAssignedScalars()!=scalars',
 'source.Node.CopyRanges16().AsSpan().SequenceEqual(key)','receipt??=Prepare(coverage,token)',
 'receipt.Coverage,coverage','BinaryPrimitives.ReadUInt32LittleEndian(key[4..])',
 '(flags&0xfeffffffu)|(reportedKind==0xc25?0x01000000u:0)']
for rule in rules:assert rule in source,rule
assert source.index('Preflight(current.Curves.Count,points,current.Views.Count)')<source.index('var curves=current.Curves.ToArray()')
assert source.index('result.ValidateSlot(i)')<source.index('result.Validate(token);return result')
assert source.index('source.CopyAssignedScalars()!=scalars')<source.index('bool needsKind=')
assert 'public 'not in source and 'LegacyOwnedMainWordSourceChoicesResult'not in source and 'NativeLibrary.Load'not in source
a.output.write_text(json.dumps(dict(status='PASS actual owned C49/base-kind receipt',nativeFunctions=14,nativeBodyRegions=15,instructions=6453,bindings=12,switchBytes=68,runtimeContracts=len(rules)+4,unchangedScalarMethod=pins['unchangedScalarValidateSha256'],nativeExecution=False,actualC25ObjectClaim=False,applicationQualityClaim=False),indent=2)+'\n')
print('PASS14 nativefunctions/15bodyregions/6453instructions/12bindings/68switchbytes +same-scope typed C49 receipt/contracts; scalar validator unchanged.')
