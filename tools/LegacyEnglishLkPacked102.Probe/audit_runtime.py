"""Independent actual raw EN packed runtime oracle; no vendor execution.
Executes the portable STATIC reference on raw resources, never C# code.
"""
from pathlib import Path
import argparse,hashlib,json,runpy,struct,sys
sys.dont_write_bytecode=True
p=argparse.ArgumentParser();p.add_argument('models',type=Path);p.add_argument('capture',type=Path);p.add_argument('reference_output',type=Path);a=p.parse_args();got=json.loads(a.capture.read_bytes())
reference=Path(__file__).with_name('reference_actual.py');sys.argv=[str(reference),str(a.models),str(a.reference_output)];ns=runpy.run_path(str(reference),run_name='__main__');raw=json.loads(a.reference_output.read_bytes())
labels,counts,starts=ns['labels'],ns['counts'],ns['starts'];h=hashlib.sha256()
for node,(label,count,start)in enumerate(zip(labels,counts,starts)):
 h.update(struct.pack('<IHBBII',node,label&127,int(node!=0 and bool(label&128)),0,start,start+count))
assert got['NodeRecordSha256']==got['AfterCallerMutationSha256']==h.hexdigest()
assert got['WordLabelPreorderSha256']==raw['wordLabelPreorderSha256']
assert(got['Nodes'],got['Terminals'],got['MaximumWordLength'],got['StorageBytes'])==(raw['nodeCount'],raw['storedTerminals'],raw['maximumWordLength'],666548+((666548*17+31)//32)*4)
assert got['LogicalLoadCopyBytes']==2*3300812+666548+583232+1416416+666547+256*1024+64*1024<32*1024*1024
for actual,expected in zip(got['RootChildren'],raw['serializedRootChildren']):assert(actual['Offset'],actual['End'],actual['Kind'],actual['Version'])==(expected['offset'],expected['end'],expected['kind'],expected['version'])
assert len(got['RootChildren'])==21
assert got['Prefix']==dict(Identifier='en_US-lk-text.lite.lex',Weight60=0,Weight64=65536,Field68=1,RuntimeLanguageScale=1,AlphabetPayloadOffset=294-184)
assert got['AlphabetAbsoluteOffset']==294 and got['UnsupportedLanguageBoundary']==dict(Offset=1252412,End=2832716,Kind=0x2d2,Version=0x101)
expectedMaps=[dict(Lexical=x['lexical'],Text=x['text'],ExactAcoustic=x['exactAcoustic'])for x in raw['alphabetData']['exactTextMapping']];assert got['ExactTextMapping']==expectedMaps
assert len(got['CanceledLoads'])==12 and all(not x['Published']for x in got['CanceledLoads']);assert got['CanceledLoads'][-1]['Calls']==2
assert got['Guards']==55 and len(got['TamperedByteOffsets'])==19
print('PASS independent raw full-node/transition/terminal digest,104-to-217 mapping,domains/storage/copy budget,mutation and12cancel/55guards; no language/masks claim')
