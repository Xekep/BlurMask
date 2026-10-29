"""Prepare an isolated actual-encoder hash observer; never edit library sources."""
from pathlib import Path
import argparse,hashlib,json,shutil

p=argparse.ArgumentParser(description=__doc__)
p.add_argument('output',type=Path)
p.add_argument('--root',type=Path,default=Path(__file__).resolve().parents[2])
a=p.parse_args();root=a.root.resolve();out=a.output.resolve()
if out.exists():p.error('A fresh output directory is required')
stage=out/'stage';stage.mkdir(parents=True)
sha=lambda path:hashlib.sha256(path.read_bytes()).hexdigest()
pins={}
for folder,count in [('src/InkRecognition',951),('src/InkRecognition.Math',87),('tools/LegacyMathCurrentSelectedIngress.Probe',3)]:
 source=root/folder;files=list(source.glob('*.cs'))
 if len(files)!=count:raise ValueError('Unreviewed source extent: '+folder)
 target=stage/folder;target.mkdir(parents=True)
 for path in files+list(source.glob('*.csproj'))+list(source.glob('PACKAGE.md')):
  shutil.copyfile(path,target/path.name);pins[path.relative_to(root).as_posix()]=sha(path)
shutil.copyfile(root/'global.json',stage/'global.json');pins['global.json']=sha(root/'global.json')
path=stage/'src/InkRecognition.Math/Math2Mode0TensorState.cs';text=path.read_text(encoding='utf8')
before='        var outputs = Encoder.Run(run, source, downsampling, strokeEnds);'
after='            return new(this, run, outputs);'
if text.count(before)!=1 or text.count(after)!=1:raise ValueError('Unreviewed encoder call/return body')
pre='        Math2EncoderDiagnostic.Before(source, downsampling, strokeEnds);\n'
post='            Math2EncoderDiagnostic.After(outputs);\n'
patched=text.replace(before,pre+before).replace(after,post+after)
assert patched.replace(pre,'').replace(post,'')==text
path.write_text(patched,encoding='utf8',newline='\n')
path=stage/'tools/LegacyMathCurrentSelectedIngress.Probe/Program.cs';text=path.read_text(encoding='utf8')
begin='var rawAttention=RawAttentionSidecar.CreateIfEnabled(args[6],args.Take(6).ToArray(),runtime.Version);'
end='rawAttention?.Seal(args[6]);'
if text.count(begin)!=1 or text.count(end)!=1:raise ValueError('Unreviewed probe lifecycle')
start='\nusing var encoderTrace=Math2EncoderDiagnostic.Start(args[6]+".encoder.json",runtime.Version);'
finish='\nencoderTrace.Complete(args[6]);'
patched=text.replace(begin,begin+start).replace(end,end+finish)
assert patched.replace(start,'').replace(finish,'')==text
path.write_text(patched,encoding='utf8',newline='\n')
template=Path(__file__).with_name('fixtures')/'math_encoder_diagnostic.cs.txt'
shutil.copyfile(template,stage/'src/InkRecognition.Math/Math2EncoderDiagnostic.cs')
authored={q.relative_to(stage).as_posix():sha(q)for q in stage.rglob('*.cs')}
assert len(authored)==951+88+3
manifest=dict(Schema=1,Scope='ISOLATED_ENCODER_OBSERVATION_NOT_MAIN_ADMISSION_OR_NEW_INFERENCE',MainInputPins=pins,DiagnosticSourcePins=authored,TemplateSha256=sha(template),SourceCounts={'Core':951,'Math':88,'Probe':3},PdbProof=False)
(out/'source-manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf8')
print('Prepared isolated Core951/Math88/probe3; four observer calls added, Main untouched')
