"""Read-only pinned PE catalog auditor. Never loads/executes vendor code; outputs private diagnostics."""
import argparse, hashlib, json, struct
from pathlib import Path

PIN='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
BASE=0x180000000

def audit(path):
    data=path.read_bytes()
    digest=hashlib.sha256(data).hexdigest()
    if digest!=PIN: raise ValueError('MyScriptDocument PE SHA256 does not match inspected version')
    pe=struct.unpack_from('<I',data,0x3c)[0]
    if data[pe:pe+4]!=b'PE\0\0': raise ValueError('Not a PE image')
    count=struct.unpack_from('<H',data,pe+6)[0]
    optional=struct.unpack_from('<H',data,pe+20)[0]
    start=pe+24+optional
    sections=[struct.unpack_from('<IIII',data,start+i*40+8) for i in range(count)]
    def offset(va,size):
        rva=va-BASE
        for virtual_size,virtual_address,raw_size,raw_offset in sections:
            if virtual_address<=rva and rva+size<=virtual_address+raw_size:
                result=raw_offset+rva-virtual_address
                if result+size<=len(data): return result
        raise ValueError(f'VA outside bounded raw PE section: {va:x}')
    rows=[]
    for i in range(3923):
        pointer,main,alternate=struct.unpack_from('<Qbb',data,offset(0x180244c50+i*16,16))
        address=offset(pointer,1)
        end=data.index(0,address,min(address+513,len(data)))
        text=data[address:end].decode('utf8','strict')
        if not text or main not in range(-1,45) or alternate not in range(-1,45):
            raise ValueError('Malformed observed catalog entry')
        rows.append(dict(text=text,main=main,alternate=alternate))
    profiles=[struct.unpack_from('<fff',data,offset(0x180244a30+i*12,12)) for i in range(45)]
    if any(p[2]!=0 for p in profiles): raise ValueError('Unexpected observed profile reserved value')
    constants=[struct.unpack_from('<f',data,offset(a,4))[0] for a in (0x180229d5c,0x180229cb8)]
    if constants!=[-1.,2.]: raise ValueError('Observed profile normalization constants changed')
    if struct.unpack_from('<ffff',data,offset(0x180229dc0,16))!=(0.,0.,1.,0.):
        raise ValueError('Prepared initial geometry/weight payload changed')
    return dict(format='pinned-document-prepared-category-audit-v1',source_sha256=digest,
        catalog_va='180244c50',profile_va='180244a30',rows=rows,profiles=profiles,
        unique_keys=len({r['text'] for r in rows}),
        cyrillic_entries=sum(any(0x400<=ord(c)<=0x4ff for c in r['text']) for r in rows),
        scope='prepared existing-symbol catalog only; not raw handwriting recognition')

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('binary',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
    result=audit(a.binary);a.output.parent.mkdir(parents=True,exist_ok=True)
    a.output.write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf8')
    print(f"PASS pinned PE: {len(result['rows'])} catalog rows/{result['unique_keys']} keys, {len(result['profiles'])} profiles, {result['cyrillic_entries']} Cyrillic entries; static only")
