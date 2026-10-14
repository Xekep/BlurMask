"""Public Unicode9 circle expectations plus original marker/writeback controls, independent string-level oracle."""
import argparse,hashlib,json,pathlib,unicodedata,random
BEGIN,END='\U000f0000','\U000f0001'

def main():
    p=argparse.ArgumentParser();p.add_argument('unicodeData');p.add_argument('literalModel');p.add_argument('output');a=p.parse_args()
    raw=pathlib.Path(a.unicodeData).read_bytes()
    assert hashlib.sha256(raw).hexdigest()=='68dfc414d28257b9b5d6ddbb8b466c768c00ebdf6cbf7784364a9b6cad55ee8f'
    public={};mapping={};publicRows=[]
    for line in raw.decode().splitlines():
        f=line.split(';');cp=int(f[0],16)
        if f[5].startswith('<circle> '):
            payload=''.join(chr(int(v,16)) for v in f[5].split()[1:]);public[cp]=payload
            supported=cp in {*range(0x2460,0x2474),*range(0x24b6,0x24eb),*range(0x3251,0x327f),*range(0x3280,0x32b1),*range(0x32b1,0x32c0),*range(0x32d0,0x32ff)}
            #24D0..24EA includes letters pluszero; excludes circled0x24EB..0x24FF and10supplementary compatibility additions.
            if supported:mapping[unicodedata.normalize('NFC',payload)]=chr(cp)
            publicRows.append([cp,payload,supported])
    assert len(mapping)==230
    for row in publicRows:row.append(ord(mapping[unicodedata.normalize('NFC',row[1])]) if unicodedata.normalize('NFC',row[1]) in mapping else -1)
    literals=json.loads(pathlib.Path(a.literalModel).read_text())['literalPayloads'];aliases=[]
    for h,cp in literals:
        s=bytes.fromhex(h).decode()
        if s not in mapping:aliases.append([h,cp]);mapping[s]=chr(cp)
        else:assert mapping[s]==chr(cp)
    assert len(aliases)==31
    def transform(text):
        working=text[1:] if text.startswith('\ufeff') else text
        working=unicodedata.normalize('NFC',working.replace('\ufffe','\x1a').replace('\uffff','\x1a'))
        pos=0;changed=False
        while True:
            start=working.find(BEGIN,pos)
            if start<0:break
            end=working.find(END,start+1)
            if end<0:break
            payload=working[start+1:end];value=mapping.get(payload)
            if value is None:pos=end+1
            else:working=working[:start]+value+working[end+1:];pos=start+1;changed=True
        return working if changed else text,changed
    cases=[]
    def add(text):
        output,changed=transform(text);cases.append(dict(input=text.encode().hex(),output=output.encode().hex(),changed=changed))
    for cp,payload,supported,expectedCp in publicRows:
        for prefix,suffix in [('', ''),('\ufeffe\u0301','\uffff'),('left\0','right')]:add(prefix+BEGIN+payload+END+suffix)
    for h,cp in aliases:add(BEGIN+bytes.fromhex(h).decode()+END)
    for s in ['','0','A','z','50','51','100','00','01','000','aA','@',' ','Ж','snow☃',BEGIN+'A']:
        for prefix in ['','\ufeffe\u0301']:add(prefix+BEGIN+s+END+BEGIN+'B'+END)
    for text in ['',BEGIN,'e\u0301'+BEGIN,'\ufeffe\u0301',END+BEGIN,BEGIN+BEGIN+'A'+END+END,
                 BEGIN+'A'+END+BEGIN,BEGIN+'?'+END+BEGIN+'Ж'+END,BEGIN+'?'+END+BEGIN+'10'+END,
                 '\r\n'+BEGIN+'a'+END,BEGIN+'A'+END+'\0'+BEGIN+'50'+END]:add(text)
    rng=random.Random(49100);items=list(mapping)+['?','Ж','51','100','']
    for i in range(256):add('e\u0301'+''.join(BEGIN+rng.choice(items)+END for _ in range(rng.randrange(1,12)))+'x')
    pathlib.Path(a.output).write_bytes((json.dumps(dict(publicCircleCount=len(publicRows),publicRows=publicRows,aliases=aliases,cases=cases),ensure_ascii=True,separators=(',',':'))+'\n').encode());print(len(cases),len(publicRows),len(aliases))
if __name__=='__main__':main()
