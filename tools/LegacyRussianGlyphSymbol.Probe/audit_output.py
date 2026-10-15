"""Independent raw AK alphabet/output oracle; never imports the C# implementation."""
import hashlib,json,struct,sys
from pathlib import Path

def read_alphabet(data):
    at=99
    def read(fmt):
        nonlocal at
        value=struct.unpack_from('<'+fmt,data,at)[0];at+=struct.calcsize('<'+fmt);return value
    header=[read('H') for _ in range(8)];count=header[-1];n=read('H')
    for inc in (32,64,1):
        values=[read('H') for _ in range(n)];n=next(v for v in reversed(values) if v!=65535)+inc
    spans=[read('H') for _ in range(n+1)]
    pairs=[(read('I'),read('I')&65535) for _ in range(spans[-1]+1)]
    strings=data[at:at+pairs[-1][0]]
    labels={pairs[i][1]:strings[pairs[i][0]:pairs[i+1][0]].decode('utf8') for i in range(len(pairs)-1)}
    assert count==192 and len(labels)==189 and set(range(count))-set(labels)=={140,178,179}
    return labels

def main():
    if len(sys.argv)!=3:raise ValueError('AK.res capture.json')
    raw=Path(sys.argv[1]).read_bytes()
    assert hashlib.sha256(raw).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
    labels=read_alphabet(raw);reverse={v:k for k,v in labels.items()};replacement=reverse['\ufffd']
    assert reverse["'"]==134 and reverse['"']==129
    def expected(symbol,conversion):
        if conversion&255:
            if symbol==193:return 134
            if symbol==194:return 129
        return symbol
    capture=json.loads(Path(sys.argv[2]).read_text())
    assert capture['labels']==192 and capture['replacement']==replacement
    conversions=[0,1,2,255,256,257,2**64-1]
    rows=[]
    for symbol in range(192):
        for conversion in conversions:
            for knowledge in (False,True):
                if symbol==replacement and not knowledge:continue
                rows.append([symbol,conversion,knowledge,symbol,symbol])
    for symbol in (193,194):
        for conversion in conversions:
            for knowledge in (False,True):
                value=expected(symbol,conversion);rows.append([symbol,conversion,knowledge,value,value])
    assert capture['rows']==rows
    h=hashlib.sha256();n=0
    for symbol in range(65536):
        if symbol==replacement:continue
        for conversion in (0,1):
            value=expected(symbol,conversion);h.update(struct.pack('<HHHH',symbol,conversion,value,value));n+=1
    assert capture['exhaustiveUshortRows']==n==131070
    assert capture['exhaustiveSha256'].lower()==h.hexdigest()
    assert capture['guards']==14 and capture['rejectedDomainRows']==7
    print(f'PASS independent raw AK192 +{len(rows)} scalar rows +{n} exhaustive ushort hash {h.hexdigest()}; replacement={replacement}.')

if __name__=='__main__':main()
