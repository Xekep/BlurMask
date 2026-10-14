"""Read-only pinned PE extraction of native text publication properties. Never loads DLLs."""
import argparse
import hashlib
import json
import pathlib
import struct
import sys

ENGINE = 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
TEXT = '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--output', required=True)
    args = ap.parse_args()
    root = pathlib.Path(__file__).resolve().parents[2]
    sys.path.insert(0, str(root / 'research/myscript/scripts'))
    from inspect_text_features import Binary
    engine, text = Binary('MyScriptEngine.dll'), Binary('MyScriptText.dll')
    for binary, expected in [(engine, ENGINE), (text, TEXT)]:
        if hashlib.sha256(binary.data).hexdigest() != expected:
            raise ValueError('Pinned source PE SHA differs')
    def ranges(address, count, width):
        return [list(struct.unpack_from('<' + 'i'*width, engine.data, engine.off(address)+i*width*4)) for i in range(count)]
    result = dict(format='legacy-native-text-publication-data-v1', engineSha256=ENGINE, textSha256=TEXT,
                  scriptRanges=ranges(0x1801fb6d0,716,3),
                  controlRanges=ranges(0x1801faec0,19,2),
                  extendRanges=ranges(0x1801faf60,225,2),
                  prependRanges=ranges(0x1801fb670,11,2),
                  joinerRanges=ranges(0x1801fac50,77,2))
    literals=[]
    for i in range(120):
        at=text.off(0x18030e160)+i*32
        ptr,value,left,right,secondLeft,secondRight=struct.unpack_from('<Q5i',text.data,at)
        begin=text.off(ptr); end=text.data.index(b'\0',begin)
        literals.append([text.data[begin:end].hex(),value,left,right,secondLeft,secondRight])
    if len({x[0] for x in literals})!=120: raise ValueError('Duplicate literal')
    result['literalRows']=literals
    raw=bytearray()
    for name,width in [('scriptRanges',3),('controlRanges',2),('extendRanges',2),('prependRanges',2),('joinerRanges',2)]:
        rows=result[name]; raw+=struct.pack('<i',len(rows))
        last=-1
        for row in rows:
            if row[0]<=last or row[1]<row[0] or row[1]>0x10ffff: raise ValueError('Range order')
            last=row[1];raw+=struct.pack('<'+'i'*width,*row)
    raw+=struct.pack('<i',len(literals))
    for h,*fields in literals:
        data=bytes.fromhex(h);raw+=struct.pack('<i',len(data))+data+struct.pack('<5i',*fields)
    result['tableSha256']=hashlib.sha256(raw).hexdigest()
    path=pathlib.Path(args.output);path.parent.mkdir(parents=True,exist_ok=True)
    path.write_bytes((json.dumps(result,separators=(',',':'))+'\n').encode())
    print(json.dumps(dict(bytes=path.stat().st_size,sha256=hashlib.sha256(path.read_bytes()).hexdigest(),tableSha256=result['tableSha256'])))

if __name__=='__main__': main()
