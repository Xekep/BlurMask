"""Validate completed recorded capture, then evaluate labels after inference.
Diagnostic word tracks have no pen-up metadata and are not held-out quality data.
"""
import hashlib,json,sys
from pathlib import Path
capture=Path(sys.argv[1]);manifest_path=Path(sys.argv[2]);report_path=Path(sys.argv[3])
d=json.loads(capture.read_bytes());raw=manifest_path.read_bytes();manifest=json.loads(raw)
assert d['manifestSha256']==hashlib.sha256(raw).hexdigest()
assert manifest['commit']=='c6629026b1fdef51fd08e3d2fddffd82f97e60a5' and manifest['penUpMetadata']=='absent'
metadata=[s for s in manifest['samples'] if s['kind']=='word']
assert len(metadata)==len(d['samples'])==9

def distance(a,b):
    previous=list(range(len(b)+1))
    for i,x in enumerate(a,1):
        current=[i]
        for j,y in enumerate(b,1):current.append(min(current[-1]+1,previous[j]+1,previous[j-1]+(x!=y)))
        previous=current
    return previous[-1]

rows=[];exact=edits=characters=completed=refused=0
for observed,source in zip(d['samples'],metadata):
    assert observed['file']==source['file'] and observed['trackSha256']==source['trackSha256']
    track=(manifest_path.parent/source['file']).read_bytes()
    assert hashlib.sha256(track).hexdigest()==observed['trackSha256']
    points=json.loads(track)['points'];assert len(points)==observed['points']
    row=dict(file=source['file'],status=observed['status'])
    if observed['status']=='refused':
        refused+=1;row.update(stage=observed['stage'],reason=observed['reason']);rows.append(row);continue
    assert observed['status']=='complete';completed+=1
    words=[];available=True
    for word in observed['words']:
        chunks=[]
        for glyph in word['glyphs']:
            alternatives=[bytes.fromhex(v).decode('utf-8') for v in glyph['alternatives']]
            marker=glyph['marker']
            if glyph['selected'] is None:
                assert marker<0 or marker>=len(alternatives);available=False
            else:
                text=bytes.fromhex(glyph['selected']).decode('utf-8')
                assert 0<=marker<len(alternatives) and alternatives[marker]==text;chunks.append(text)
            assert glyph['positions']
            for start,end in glyph['positions']:
                assert start<=end
                for position in (start,end):
                    assert position>>32==0
                    assert (position>>16&65535)<len(points)
                    assert (position>>8&255)<=200 or (position>>8&255)==255
                    assert position&255 in (1,3,4,5,7)
        words.append(''.join(chunks))
    row.update(words=words,selectedAvailable=available)
    #Expected labels enter only here, after completed inference is on disk.
    expected=source['expected'];prediction=''.join(words)
    row.update(expected=expected,diagnosticConcatenation=prediction)
    if available:
        error=distance(prediction,expected);row['edits']=error;edits+=error;characters+=len(expected);exact+=prediction==expected
    rows.append(row)
assert completed==d['complete'] and refused==d['refused']
report=dict(samples=rows,completed=completed,refused=refused,exact=exact,edits=edits,referenceCodePoints=characters,
    diagnosticCER=edits/characters if characters else None,
    limitations=['Nine diagnostic word tracks, no pen-up events','Writer overlap with model training unknown','Not representative or held-out','WORDs concatenated only for a single-word label metric; no runtime accepted text'])
report_path.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(f"PASS recorded owner positions/selected markers; diagnostic {exact}/9 exact,{edits}/{characters} Unicode edits; no accepted OCR claim.")
