"""Independent source-slice SHA expectations, not recorded OCR/native outputs."""
import argparse,hashlib,json,pathlib,struct

def main():
    p=argparse.ArgumentParser();p.add_argument('resource');p.add_argument('output');a=p.parse_args();raw=pathlib.Path(a.resource).read_bytes()
    assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
    start=6120;end=start+8+struct.unpack_from('<I',raw,start+4)[0];assert raw[11376]==0
    cases=[]
    for offset,value in [(-1,0),(187747,raw[187747]^1),(11376,1)]:
        data=bytearray(raw)
        if offset>=0:data[offset]=value
        cases.append(dict(offset=offset,value=value,fingerprint=hashlib.sha256(data[start:end]).hexdigest().upper(),wordGeometry=data[11376]!=0))
    pathlib.Path(a.output).write_bytes((json.dumps(dict(sourceOffset=start,b9Offset=11376,cases=cases),separators=(',',':'))+'\n').encode());print(len(cases))
if __name__=='__main__':main()
