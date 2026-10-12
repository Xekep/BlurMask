"""Pinned PE table proof for Document recognition source items -> Text result-line font metrics.
Static audit only; no DLL import, SDK invocation or synthetic handwriting winner.
"""
import argparse, hashlib, json, struct
from pathlib import Path

PINS={
    'MyScriptDocument.dll':'17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f',
    'MyScriptText.dll':'18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'}

class Image:
    def __init__(self,path):
        self.data=path.read_bytes();self.name=path.name
        if hashlib.sha256(self.data).hexdigest()!=PINS[self.name]:raise ValueError('Unpinned source image: '+self.name)
        pe=struct.unpack_from('<I',self.data,0x3c)[0]
        if self.data[pe:pe+4]!=b'PE\0\0':raise ValueError('Invalid PE header')
        count,size=struct.unpack_from('<H',self.data,pe+6)[0],struct.unpack_from('<H',self.data,pe+20)[0]
        self.sections=[struct.unpack_from('<IIII',self.data,pe+24+size+i*40+8) for i in range(count)]
    def at(self,va,size):
        rva=va-0x180000000
        for _,start,length,offset in self.sections:
            if start<=rva and rva+size<=start+length:
                pos=offset+rva-start
                if pos+size<=len(self.data):return self.data[pos:pos+size]
        raise ValueError(f'Unmapped bounded PE address: {va:x}')
    def q(self,va):return struct.unpack('<Q',self.at(va,8))[0]

def audit(folder):
    document=Image(folder/'MyScriptDocument.dll');text=Image(folder/'MyScriptText.dll')
    proofs=[]
    def pointer(image,address,expected,meaning):
        actual=image.q(address)
        if actual!=expected:raise ValueError(f'Unexpected {meaning}: {actual:x}')
        proofs.append(dict(image=image.name,address=f'{address:x}',target=f'{actual:x}',meaning=meaning))
    pointer(document,0x1801fd098,0x13300000eed,'EED descriptor: implementation family133')
    pointer(document,0x1801fd0b0,0x1801fd0d8,'EED dispatch table')
    pointer(document,0x1801fd0d8+0x20,0x180047c50,'EED virtual20 recognition')
    pointer(document,0x1801fd0d8+0x28,0x1800485b0,'EED virtual28 statistics (not recognition)')
    pointer(document,0x1801ed938,0x1801ed9a0,'EEE runtime table descriptor')
    pointer(document,0x1801ed930+0x28,0x18003a260,'EEE virtual28 decoding orchestration')
    pointer(document,0x1801ed9f8,0x1800359f0,'EEE264-byte prepared parameter reader')
    pointer(text,0x180276748,0x180276810,'2DB runtime table descriptor')
    pointer(text,0x180276740+0x18,0x180050660,'2DB virtual18 source-line result preparation')
    pointer(document,0x18020bea0+0x148,0x180177590,'Document lineInfo producer')
    return dict(format='pinned-recognized-source-line-pipeline-audit-v1',sources=PINS,tableProofs=proofs,
        staticProducerChain=[
            'Document0540d0 ->0526c0 ->scheduled027590 ->EED.virtual20=047c50',
            'Document047c50 decoder+220 <-0450c0 classEEE ->03a260 ->038610',
            'EEE decoded record strideA8 source items78/count80 ->043ac0 ->Documentblock20+78=98/countA0',
            'Document04fab0 ->043740 clones40-byte source items/16-byte UTF8 candidates/source identifier vectors',
            'Document053c60 argument6 ->0531b0 symbol category features ->C1A ->2DAF8/56-byte source lines',
            'Text050660 source2DAF8+i*38 ->2DB58/i*30 record source owner+0',
            'Document177590 source-owner double20 ->SDK lineInfo float18 ->iink4c2660 ->font policy'],
        unresolved=[
            'EEF actual chosen sequence ->40-byte source item/source-ID alignment producer',
            'EED/EEE resource compatibility with the independently restored old Russian135 source family',
            'Full recognized source item geometry service and native winner/live application integration'],
        vendorCodeExecuted=False)

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('binary_folder',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
    result=audit(a.binary_folder);a.output.parent.mkdir(parents=True,exist_ok=True)
    a.output.write_text(json.dumps(result,indent=2),encoding='utf8')
    print(f"PASS {len(result['tableProofs'])} pinned dispatch/owner table proofs; source chain statically traced, actual winner/alignment still unresolved")
