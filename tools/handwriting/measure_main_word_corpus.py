"""Metrics only: expected text is opened after all nine inference captures."""
import argparse,base64,hashlib,json
from pathlib import Path
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('manifest',type=Path);parser.add_argument('capture_dir',type=Path);parser.add_argument('output',type=Path)
parser.add_argument('--prefix',default='word-utf8-managed')
args=parser.parse_args()
# Phase1 deliberately cannot see expected labels. No subprocess/inference or
# model is invoked by this evaluator, and no corrected text is fed back.
predictions=[]
for index in range(9):
    path=args.capture_dir/f'{args.prefix}-word{index}.json';raw=path.read_bytes();capture=json.loads(raw)
    if capture['recordedIndex']!=index or capture['penUpMetadata']!='absent':raise ValueError('Inference provenance mismatch')
    variants=capture['readingOrderUtf8']
    if len(variants)!=32 or len(set(variants))!=1:raise ValueError('Inference varied across explicit allocator residues; do not select against expected labels')
    text=base64.b64decode(variants[0],validate=True).decode('utf8','strict')
    predictions.append(dict(index=index,actual=text,trackSha256=capture['trackSha256'],captureSha256=hashlib.sha256(raw).hexdigest(),replicates=len(variants)))
# Phase2: only after every actual output exists may the metric read labels.
manifest_bytes=args.manifest.read_bytes();manifest=json.loads(manifest_bytes)
if manifest['commit']!='c6629026b1fdef51fd08e3d2fddffd82f97e60a5' or manifest['penUpMetadata']!='absent':raise ValueError('Pinned unsegmented corpus metadata required')
samples=[sample for sample in manifest['samples'] if sample['kind']=='word']
if len(samples)<9:raise ValueError('Nine original word records required')
def distance(reference,actual):
    previous=list(range(len(actual)+1))
    for i,a in enumerate(reference,1):
        current=[i]
        for j,b in enumerate(actual,1):current.append(min(current[-1]+1,previous[j]+1,previous[j-1]+(a!=b)))
        previous=current
    return previous[-1]
rows=[];total_edits=total_characters=exact=0
for prediction in predictions:
    sample=samples[prediction['index']]
    if prediction['trackSha256']!=sample['trackSha256']:raise ValueError('Capture does not belong to the selected coordinate track')
    expected=sample['expected']
    if not isinstance(expected,str) or not expected:raise ValueError('Original nonempty expected word required')
    actual=prediction['actual'];edits=distance(expected,actual);characters=len(expected)
    total_edits+=edits;total_characters+=characters;exact+=actual==expected
    rows.append(dict(prediction,expected=expected,id=sample['id'],writerId=sample['writerId'],session=sample['session'],referenceCodePoints=characters,editDistance=edits,CER=edits/characters,exact=actual==expected))
report=dict(stage='restored CASE main raw WORD275 UTF8 before document-wrapper/final-result processing',
 manifestSha256=hashlib.sha256(manifest_bytes).hexdigest(),commit=manifest['commit'],expectedReadAfterAllInferenceCaptures=True,
 inputAssumption='pen-up events absent; one unsegmented coordinate track per original word',
 comparison='raw Unicode code points, case sensitive; no stripping, NFC, transliteration, aliases or correction',
 samples=rows,totals=dict(samples=9,writers=len({row['writerId'] for row in rows}),referenceCodePoints=total_characters,editDistance=total_edits,CER=total_edits/total_characters,exactWordMatches=exact,exactWordRate=exact/9),
 limitations=['Small pinned diagnostic subset, not representative accuracy','Repeated allocator controls are not additional independent samples','No stock vendor-engine execution or claimed accuracy equivalence'])
args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_bytes((json.dumps(report,ensure_ascii=True,indent=2)+'\n').encode())
print(json.dumps(report['totals'],sort_keys=True))
