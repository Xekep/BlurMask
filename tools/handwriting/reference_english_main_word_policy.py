"""Independent raw EN MAIN scalar/geometry operands; no model execution/targets.
Reuses only the already-audited raw wire parser definitions, explicitly pinned.
"""
from pathlib import Path
import argparse,ast,base64,hashlib,json,struct
p=argparse.ArgumentParser()
for name in ('repo','models','output'):p.add_argument('--'+name,type=Path,required=True)
a=p.parse_args();sha=lambda b:hashlib.sha256(b).hexdigest()
def definitions(path):
    tree=ast.parse(path.read_text(encoding='utf-8-sig'))
    nodes=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))]
    env={};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),env)
    return env
wire_path=a.repo/'tools/LegacySource247Wire.Probe/audit_output.py'
alphabet_path=a.repo/'tools/LegacyEnglishMainMasks.Probe/reference_actual.py'
w=definitions(wire_path);alphabet=definitions(alphabet_path)['alphabet']
b=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes()
assert len(b)==545871 and sha(b)=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
source,summary=w['source'](b);fields={(f['owner'],f['destination']):f for f in source.fields}
def read(owner,name):return base64.b64decode(fields[owner,name]['raw'],validate=True)
def field(owner,name):
    f=fields[owner,name];return dict(offset=f['offset'],bytes=read(owner,name).hex())
ac=alphabet(b,99,7907);assert ac['count']==217
assert '\u202b'not in ac['text'].values()
selected={name:field('247',name)for name in ('B8','B9','16C','1AC','1CC','214')}
assert read('247','B8')==read('247','B9')==b'\0'
mode=13;variant=2
source_flags=0x21703&~struct.unpack_from('<I',read('247','16C'),mode*4)[0]
arcs=struct.unpack_from('<H',read('247','1AC'),mode*2)[0]
auxiliary=struct.unpack_from('<H',read('247','1CC'),mode*2)[0]
distance=struct.unpack_from('<H',read('247','214'),variant*2)[0]
assert(source_flags,arcs,auxiliary,distance)==(0x21400,125,0,20)
child=next(c for c in source.children if c['destination']=='247.130')
sampler,error=w['sampler'](b,child,217);assert error is None
sf={f['destination']:f for f in sampler['fields']}
def raw(name):return base64.b64decode(sf[name]['raw'],validate=True)
def F(name):return struct.unpack('<f',raw(name))[0]
def I(name):return struct.unpack('<i',raw(name))[0]
assert raw('50')==b'\1'
assert [raw(x).hex()for x in ('54','58','5C','60')]==['cdcc4c3e','cdcccc3d','9a99993e','00e0ffff']
assert I('60')==-8192  # Native194170 exports raw DWORD;1ACFA0 consumes signed int.
assert raw('10C')==raw('134.present')==raw('1D0.present')==b'\0'
assert [F(x)for x in ('6C','70','108','110','114')]==[1.5,-.5,3.,1.5,1.]
assert I('118')==I('120')==I('124')==0x4000000 and I('11C')==20480
matrix=b''.join(raw(f'{0x78+i*24:X}.matrixRow')for i in range(6))
assert len(matrix)==144 and len(set(struct.unpack('<36f',matrix)))>1
sampler_fields={k:dict(offset=v['offset'],bytes=raw(k).hex())for k,v in sf.items()
                if k in ('50','54','58','5C','60','68','6C','70','74','108','10C','110','114','118','11C','120','124')or'matrixRow'in k}
# Source header C0E comes from the separately proved P/L descriptor, not AK21400.
# Exact bootstrap tuple(0,0,1) has its own pinned235F30+236820/830 constructor chain.
dictionary_header=0xc0e
packed=(dictionary_header>>2)&7
packed|=(dictionary_header>>2)&8
packed|=(dictionary_header>>1)&0xc0
if dictionary_header&0x20 and dictionary_header&0x1e00!=0x800:packed|=0x10
if dictionary_header&0x1e00==0xa00:packed|=0x20
assert packed==3
result=dict(status='PASS raw English selected scalars and ACTIVE MAIN pair operands',
    rawParserSha256=sha(wire_path.read_bytes()),alphabetParserSha256=sha(alphabet_path.read_bytes()),
    sourceSha256=sha(b),source=summary,selectedFields=selected,mode=mode,variant=variant,
    actualSourceFlags=source_flags,maximumSourceArcs=arcs,auxiliarySourceArcs=auxiliary,
    parentAlternativeDistance=distance,selectedB8=0,selectedB9=0,embedding202BPresent=False,
    sampler=child,samplerFields=sampler_fields,pairEnabled=True,pairCrossLineRawInt60=-8192,
    categoryCompositeCount=sampler['categoryCompositeCount'],firstIds=sampler['categoryFirstIds'],
    samePolicyDictionaryHeader=dictionary_header,singleDictionarySummary=packed,
    freshDerivedPolicy=dict(maximumParentDistance54=120,flagPrepassDistance58=20,
        maximumWordLength68=120,headers=2,primaryAlternatives=2,secondaryAlternatives=1,
        permission2309=0,knowledgeF5=False,secondaryScaleEc=0),
    targetsRead=False,vendorExecuted=False,runtimeChanged=False,wordDecoderReady=False)
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_text(json.dumps(result,indent=2)+'\n')
print('PASS EN selected125/20/B8B9zero, dictionarySummary3, MAIN108 active50 pair .2/.1/.3 with rawint60=-8192')
