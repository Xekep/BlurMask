"""Independent operation/row oracle; no weights or native DLL execution."""
import base64, json, random, struct, sys
from pathlib import Path

def main(path):
    rng=random.Random(0x13a670); cases=[]
    for case in range(256):
        nodes={}; operations=[]; rows=0
        for op in range(128):
            node=rng.randrange(-20,20)&0xffffff; arc=rng.randrange(16); bit=rng.randrange(2)
            key=node|(arc<<24)|(bit<<56); descriptor=rng.randrange(-3,4); force=rng.randrange(7)==0
            #A few owners recur often; signed descriptor identities and equal
            #symbols exercise the distinction between ID and Unicode label.
            if op%3==0:key=(case%3)|(2<<24)
            state=nodes.setdefault(key,dict(duplicate=False,records=[]))
            template=bytearray(rng.randbytes(40)); index=None
            if not force and not state['duplicate']:
                index=next((i for i,r in enumerate(state['records']) if struct.unpack_from('<i',r)[0]==descriptor),None)
            existing=index is not None
            if index is None:
                index=len(state['records']); row=template[:]
                struct.pack_into('<i',row,0,descriptor);flags=struct.unpack_from('<H',row,4)[0]&0xfff0;struct.pack_into('<H',row,4,flags)
                struct.pack_into('<HH',row,6,0,65535);struct.pack_into('<IIi',row,12,0x3f800000,0xbf800000,0)
                row[24:32]=bytes(8);row[32:40]=bytes([255])*8
                state['records'].append(row);state['duplicate']|=force;rows+=1
            ref=key|(index<<40); before=state['records'][index][:]
            symbol=rng.randrange(12)
            flags=struct.unpack_from('<H',state['records'][index],4)[0]&0xfff8
            struct.pack_into('<HH',state['records'][index],4,flags,struct.unpack_from('<H',state['records'][index],6)[0])
            struct.pack_into('<H',state['records'][index],8,symbol)
            operations.append(dict(key=key,descriptor=descriptor,force=force,template=base64.b64encode(template).decode(),reference=ref,existing=existing,
                                   before=base64.b64encode(before).decode(),symbol=symbol,after=base64.b64encode(state['records'][index]).decode()))
        snapshots=[dict(key=k,count=(len(v['records'])<<2)|(0x8000 if v['duplicate'] else 0),rows=base64.b64encode(b''.join(v['records'])).decode()) for k,v in sorted(nodes.items())]
        cases.append(dict(operations=operations,snapshots=snapshots))
    out=Path(path);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(dict(cases=cases),separators=(',',':')))
    print('Generated256 independent glyph pool cases /32768 insertion+symbol operations')
if __name__=='__main__':main(sys.argv[1])
