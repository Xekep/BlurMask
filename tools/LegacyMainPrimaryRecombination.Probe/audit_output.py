"""Independent actual CASE acoustic-prefix byte oracle.

NN/cache/D0 rows are real captured owning inputs, not recomputed predictions.
All root/primary pool writes, symbolic concatenation, inactive storage and full
heap preservation are checked without using managed producer outputs as goldens.
"""
import ast
import base64
import json
import struct
import sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
source=(ROOT/'tools/LegacyWordContextAlternatives.Probe/reference_controls.py').read_text().replace('key ^= 0x55','key ^= 0')
syntax=ast.parse(source)
selected=[n for n in syntax.body if isinstance(n,ast.FunctionDef)and n.name=='compose' or isinstance(n,ast.Assign)and any(isinstance(t,ast.Name)and t.id=='actions'for t in n.targets)]
namespace={};exec(compile(ast.Module(body=selected,type_ignores=[]),'<independent-identity-context>','exec'),namespace)
compose=namespace['compose']
d=json.loads(Path(sys.argv[1]).read_bytes());nodes={i:(p,s)for i,p,s in d['contexts']}
paths={0:()}
def path(identity):
    if identity not in paths:
        parent,symbol=nodes[identity];paths[identity]=path(parent)+(symbol,)
    return paths[identity]
for identity in nodes:path(identity)
reverse={value:key for key,value in paths.items()}
assert len(reverse)==len(paths)
def joined(prefix,suffix,mapped):return reverse[tuple(compose(path(prefix),path(suffix),mapped)[0])]
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
u64=lambda b,o:struct.unpack_from('<Q',b,o)[0]
i32=lambda b,o:struct.unpack_from('<i',b,o)[0]
wrap=lambda x:(x+0x80000000)%0x100000000-0x80000000
s8=lambda x:x-256 if x>=128 else x
b64=base64.b64decode
parents=[(b64(v['row']),b64(v['primary']))for v in d['parents']]
assert len(parents)==d['sourceEnd']==125
rows=bytes_checked=0
for stage in d['stages']:
    heap=b64(stage['heapBefore']);assert heap==b64(stage['heapAfter'])
    expected=bytearray(b64(stage['primaryBefore']));active=stage['active']
    for i in range(active):
        slot=heap[i*128:(i+1)*128];ordinal=i32(slot,88);at=ordinal*80;suffix=u64(slot,8)
        arc=u16(slot,30)
        struct.pack_into('<H',expected,at+24,arc);expected[at+26]=slot[32]
        expected[at+31]=(expected[at+31]&192)|(slot[33]&31)
        if stage['kind']=='root':
            assert stage['ending']-s8(slot[32])-1==-1
            expected[at:at+8]=slot[16:24]
            struct.pack_into('<QQ',expected,at+8,joined(0,suffix,True),joined(0,suffix,False))
            expected[at+30]=slot[38];expected[at+31]|=32
            struct.pack_into('<ii',expected,at+40,0x4000000,0x4000000);expected[at+70]=0
        else:
            assert stage['ending']-s8(slot[32])-1==0 and arc<125
            parent,pool=parents[arc]
            delta=wrap(i32(slot,16)-i32(parent,16));increment=wrap(i32(slot,20)-i32(parent,20))
            reset=path(u64(parent,8))[-1]in(0xfff5,0xfff9,0xfffa,0xfffb)
            append=u16(slot,24)==stage['ending']or s8(slot[5])>=0
            expected[at+27]&=128
            for k in range(2):
                off=at+k*40;src=k*40
                if reset:
                    if k==0:
                        struct.pack_into('<ii',expected,off,delta,increment)
                        struct.pack_into('<QQ',expected,off+8,joined(0,suffix,True),joined(0,suffix,False))
                        expected[off+30]=slot[38];expected[off+31]|=32
                    else:struct.pack_into('<ii',expected,off,0x4000000,0x4000000);expected[off+30]=0
                elif i32(pool,src)<0x4000000:
                    struct.pack_into('<ii',expected,off,wrap(i32(pool,src)+delta),wrap(i32(pool,src+4)+increment))
                    struct.pack_into('<H',expected,off+24,arc);expected[off+26]=slot[32]
                    expected[off+27]=(expected[off+27]&128)|k;expected[off+30]=slot[38]
                    expected[off+31]=(expected[off+31]&192)|(slot[33]&31)
                    mapped,raw=u64(pool,src+8),u64(pool,src+16)
                    if append:mapped=joined(mapped,suffix,True);raw=joined(raw,suffix,False)
                    struct.pack_into('<QQ',expected,off+8,mapped,raw)
                else:struct.pack_into('<ii',expected,off,0x4000000,0x4000000);expected[off+30]=0
        rows+=1
    assert expected==b64(stage['primaryAfter']),stage['kind']
    bytes_checked+=len(heap)+len(expected)
assert [s['active']for s in d['stages']]==[25,125]and d['rootQueries']==36 and d['childQueries']==130
print(f'PASS independent{rows} actual CASE acoustic-prefix slots/{bytes_checked} full heap+pool bytes/actual125-source rows/context paths. No later-history or whole scheduler claim.')
