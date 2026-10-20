"""Native0250C0/025450 grammar ROOT schema; framed children remain opaque.

Read-only independent reference, not a native grammar evaluator or SDK loader.
Budgets and strict header admission are analysis-tool policy, not native error parity.
"""
from pathlib import Path
import argparse,hashlib,json,struct

def parse_root(data):
    if len(data)<20 or len(data)>16*1024*1024:raise ValueError('Bounded complete resource required')
    magic,length,kind,flags,version=struct.unpack_from('<4sIIHH',data)
    if (magic,kind,flags,version)!=(b'\x90\xfe\x01\x01',0x138a,0,0x100)or length+8!=len(data):raise ValueError('Exact root class/header version required')
    cursor=16
    def take(n):
        nonlocal cursor
        if n<0 or n>len(data)-cursor:raise ValueError('Truncated root')
        raw=data[cursor:cursor+n];cursor+=n;return raw
    def u32():return struct.unpack('<I',take(4))[0]
    count=u32()
    if count>4096:raise ValueError('Root rule budget')
    rules=[];children=0
    for index in range(count):
        start=cursor;tag=take(1)[0];name_size=u32()
        if name_size>65536:raise ValueError('Root name budget')
        name=take(name_size);number=u32();children+=number
        if children>65536:raise ValueError('Whole child-frame budget')
        frames=[]
        for _ in range(number):
            at=cursor;header=take(16);m,l,t,f,v=struct.unpack('<4sIIHH',header)
            if m!=magic or l<8 or f&~3 or not 0<t<=65535:raise ValueError('Bounded framed child required')
            payload=take(l-8);raw=header+payload
            frames.append(dict(offset=at,end=cursor,typeId=t,flags=f,version=v,raw=raw))
        rules.append(dict(index=index,offset=start,end=cursor,tag=tag,name=name,children=frames))
    trailing=u32()
    if cursor!=len(data):raise ValueError('Root trailing bytes')
    return dict(header=data[:16],rules=rules,trailingScalar38=trailing,consumed=cursor)

def write_root(root):
    output=bytearray(root['header']);output.extend(struct.pack('<I',len(root['rules'])))
    for rule in root['rules']:
        output.extend(bytes([rule['tag']])+struct.pack('<I',len(rule['name']))+rule['name']+struct.pack('<I',len(rule['children'])))
        for child in rule['children']:output.extend(child['raw'])
    output.extend(struct.pack('<I',root['trailingScalar38']));return bytes(output)

def describe(data):
    root=parse_root(data)
    if write_root(root)!=data:raise ValueError('Root writer mismatch')
    return dict(sha256=hashlib.sha256(data).hexdigest(),consumed=root['consumed'],trailingScalar38=root['trailingScalar38'],roundtripExact=True,
                rules=[dict(index=r['index'],offset=r['offset'],end=r['end'],tag=r['tag'],nameUtf8Hex=r['name'].hex(),
                            children=[dict(offset=c['offset'],end=c['end'],typeId=c['typeId'],flags=c['flags'],version=c['version'],sha256=hashlib.sha256(c['raw']).hexdigest(),opaque=True)for c in r['children']])for r in root['rules']],
                scope='Root-only schema; all child payloads retained opaque, no grammar semantics or inference')

if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('resource',type=Path);p.add_argument('output',type=Path);a=p.parse_args();report=describe(a.resource.read_bytes());a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print(f"PASS {len(report['rules'])} native grammar root records / {sum(len(r['children'])for r in report['rules'])} opaque framed children / exact root writer")
