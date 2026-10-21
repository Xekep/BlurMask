"""Actual global original keys + independent C21 normalization/ASCII oracle.
The immutable parent13 capture is evidence, never an inference input.
"""
from pathlib import Path
import argparse,base64,hashlib,json,sys,importlib.util
sys.dont_write_bytecode=True
p=argparse.ArgumentParser();p.add_argument('capture',type=Path);p.add_argument('parent',type=Path);a=p.parse_args()
raw=a.parent.read_bytes()
#Frozen evidence has all13 original source publications, before coverage/selection.
assert hashlib.sha256(raw).hexdigest()=='1f2d2f6e2bb57e6d249fc11325a5e9736f358a5cd9dce1fe2664dd8ecdebea58'
parents={x['name']:x for x in json.loads(raw)['actual']}
repo=Path(__file__).resolve().parents[2];sys.path.insert(0,str(repo/'tools/LegacyMainSourceRangeUtf8.Probe'))
from reference_controls import replay
spec=importlib.util.spec_from_file_location('global_kind_ref',repo/'tools/LegacyGlobalDocumentSelectedSources.Probe/raw_kind_reference.py');kindref=importlib.util.module_from_spec(spec);spec.loader.exec_module(kindref)
got=json.loads(a.capture.read_bytes());assert len(got['samples'])==len(parents)==13
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
print(f'PASS independent original13 publications/{ready} formatted owners/{sources} exact raw-normalized-caller strings; explicit refusals {refused}')
