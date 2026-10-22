"""Actual global original keys + independent C21 normalization/ASCII oracle.
The paired parent13 capture is semantically validated evidence, never an inference input.
"""
from pathlib import Path
import argparse,base64,copy,json,sys,importlib.util,runpy
sys.dont_write_bytecode=True
p=argparse.ArgumentParser();p.add_argument('models',type=Path);p.add_argument('capture',type=Path);p.add_argument('parent',type=Path);p.add_argument('child',type=Path);a=p.parse_args()
repo=Path(__file__).resolve().parents[2]
# Validate the full same-input raw AL/CHAR and WORD/TEXT composition chain.
# A dynamic producer output is not an immutable raw model: pinning one old
# winner's complete JSON hash silently rejects legitimate upstream corrections.
saved=sys.argv
try:
    sys.argv=['audit_output.py',str(a.models),str(a.child),str(a.parent)]
    runpy.run_path(str(repo/'tools/LegacyGlobalWordText.Probe/audit_output.py'),run_name='__main__')
finally:
    sys.argv=saved
raw=a.parent.read_bytes()
parents={x['name']:x for x in json.loads(raw)['actual']}
repo=Path(__file__).resolve().parents[2];sys.path.insert(0,str(repo/'tools/LegacyMainSourceRangeUtf8.Probe'))
from reference_controls import replay
spec=importlib.util.spec_from_file_location('global_kind_ref',repo/'tools/LegacyGlobalDocumentSelectedSources.Probe/raw_kind_reference.py');kindref=importlib.util.module_from_spec(spec);spec.loader.exec_module(kindref)
def audit(got):
    assert len(got['samples'])==len(parents)==13
    assert len({s['name']for s in got['samples']})==13 and {s['name']for s in got['samples']}==set(parents)
    sources=0;ready=0;refused=[]
    for sample in got['samples']:
        native=parents[sample['name']]
        assert 'refusal'not in sample
        ready+=1;characters={}
        for link in native['links']:
            if link['node'] in characters:assert characters[link['node']]['positions']==link['positions']
            else:characters[link['node']]=link
        metadata={node:kindref.metadata(link['metadataFlags'],link['selectionId'],link['positions'],sample['points'])[0]for node,link in characters.items()}
        expected=[('CHAR',node,(metadata[node]|0x08000000),link['positions'])for node,link in characters.items()]
        allspecial=all(metadata[link['node']]&0x11004000 for link in native['links'])
        wordflags=((native['wordFlags']&0xfeffffff)|(0x01000000 if allspecial else 0)|0x08000000)&0xffefffff
        expected.extend([('WORD',native['node'],wordflags,native['wordKey']),('TEXT',0,0,native['textKey'])])
        assert len(sample['sources'])==len(expected)
        for row,(kind,node,flags,key) in zip(sample['sources'],expected):
            assert (row['kind'],row['id'],row['flags'],row['original'])==(kind,node,flags,key)
            independent=replay(key);assert independent['succeeded']
            assert row['normalized']==independent['rows'] and row['utf8']==independent['utf8']
            text=base64.b64decode(row['utf8']);caller=text
            if text and flags&0x400000:caller=(b'{' if text[:1]==b'[' else b'}')+text[1:]
            assert base64.b64decode(row['callerUtf8'])==caller
            for first,last in key:
                for pos in [first,last]:assert pos>>32<len(sample['points']) and (pos>>16&65535)<sample['points'][pos>>32]
            sources+=1
        assert sample['pending']==list(range(1,len(characters)+4)) and sample['next']==len(characters)+4
    assert ready==len(parents)==13 and not refused and sources==63 and got['guards']==63+13*4+7==122
    return ready,sources

got=json.loads(a.capture.read_bytes());ready,sources=audit(got)
negative=0
for mutate in (
    lambda d:d['samples'][0]['sources'][0].__setitem__('id',-1),
    lambda d:d['samples'][0]['sources'][0].__setitem__('utf8','AA=='),
    lambda d:d['samples'][0]['sources'][0].__setitem__('callerUtf8','AA=='),
    lambda d:d['samples'][0]['pending'].pop(),
):
    changed=copy.deepcopy(got);mutate(changed)
    try:audit(changed)
    except AssertionError:negative+=1
    else:raise AssertionError('Corrupted source-format capture admitted')
assert negative==4
print(f'PASS independently validated original13 publications/{ready} formatted owners/{sources} exact raw-normalized-caller strings/4negativecaptures')
