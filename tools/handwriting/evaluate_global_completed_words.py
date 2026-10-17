"""Run the pinned coordinate-only global-completion probe, then evaluate completed glyph bytes.

Expected labels are accessed only after every inference and independent raw-AK
oracle succeeds. No normalization, target correction or inferred pen-ups.
"""
import argparse
import base64
import hashlib
import json
import subprocess
import sys
from pathlib import Path

p = argparse.ArgumentParser()
p.add_argument('--probe', type=Path, required=True)
p.add_argument('--manifest', type=Path, required=True)
p.add_argument('--output', type=Path, required=True)
a = p.parse_args()
root = Path(__file__).resolve().parents[2]
probe, manifest, output = a.probe.resolve(), a.manifest.resolve(), a.output.resolve()
output.mkdir(parents=True, exist_ok=True)
source = json.loads(manifest.read_bytes())
assert source['commit'] == 'c6629026b1fdef51fd08e3d2fddffd82f97e60a5'
assert source['penUpMetadata'] == 'absent'
inputs = [dict(file=s['file'], sha256=s['trackSha256'])
          for s in source['samples'] if s['kind'] == 'word']
assert len(inputs) == 9
del source
run = ['dotnet', str(probe)] if probe.suffix.lower() == '.dll' else [str(probe)]
models = root/'models'
resources = [models/'ru-RU/ru_RU-ak-cur.lite.res', models/'ru-RU/legacy-engine-profile.json',
             models/'ru-RU/legacy-separation-profile.json', models/'text-unicode/legacy-case-data.json',
             models/'text-unicode/legacy-text-publication-data.json', models/'line-geometry/line-2e3-30.res',
             models/'line-geometry/line-2e3-50.res']
inference = []
for i, item in enumerate(inputs):
    capture = output/('word-'+str(i)+'.json')
    subprocess.run([*run, *map(str,resources), str(capture), str(models/'ru-RU/ru_RU-lk-text.lite.res'),
                    str(models/'text-unicode/legacy-normalization-data.json'),str(models/'text-unicode/legacy-circled-payload-data.json'),str(manifest), str(i)],check=True)
    subprocess.run([sys.executable,str(root/'tools/LegacyMainGlobalCompletion.Probe/audit_output.py'),
                    str(resources[0]),str(capture)],check=True)
    owners = json.loads(capture.read_bytes())['owners']
    assert len(owners) == 1
    owner = owners[0]
    assert owner['recordedIndex'] == i and owner['trackSha256'] == item['sha256']
    variants = [b''.join(base64.b64decode(w['completed'],validate=True) for w in v['words']) for v in owner['variants']]
    assert len(variants) == 4 and len(set(variants)) == 1
    inference.append(dict(index=i,file=item['file'],trackSha256=item['sha256'],
                          text=variants[0].decode('utf-8'),
                          captureSha256=hashlib.sha256(capture.read_bytes()).hexdigest(),
                          callerVariants=len(variants)))

#Only this second phase accesses targets, after all nine completed oracles.
source_bytes = manifest.read_bytes()
targets = [s for s in json.loads(source_bytes)['samples'] if s['kind'] == 'word']
assert [(s['file'],s['trackSha256']) for s in targets] == [(s['file'],s['sha256']) for s in inputs]

def distance(expected, actual):
    previous = list(range(len(actual)+1))
    for i, x in enumerate(expected,1):
        row = [i]
        for j, y in enumerate(actual,1):
            row.append(min(row[-1]+1,previous[j]+1,previous[j-1]+(x!=y)))
        previous = row
    return previous[-1]

for result, target in zip(inference,targets):
    expected = target['expected']
    assert len(expected) <= 4096
    result.update(expected=expected,exact=result['text']==expected,
                  edits=distance(expected,result['text']),referenceCharacters=len(expected))
edits = sum(r['edits'] for r in inference)
characters = sum(r['referenceCharacters'] for r in inference)
report = dict(scope='actual owned raw global -> published first alternative -> imported per-glyph default UTF8; not accepted document text',
              samples=inference,exact=sum(r['exact'] for r in inference),edits=edits,
              referenceCharacters=characters,CER=edits/characters,
              manifestSha256=hashlib.sha256(source_bytes).hexdigest(),
              expectedReadAfterAllInferenceAndRawResourceAudits=True,
              inputAssumption='one continuous stroke per track; pen-up events absent',
              comparison='raw Unicode scalar edits; no case/NFC correction or invented word separators')
(output/'report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(f"Recorded global diagnostic: {report['exact']}/9 exact; CER {edits}/{characters} = {report['CER']:.2%}; no acceptance claim")
