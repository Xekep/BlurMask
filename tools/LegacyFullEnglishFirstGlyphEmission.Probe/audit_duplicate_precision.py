"""Focused native167210 raw-duplicate arithmetic. Static PE bytes only.
Bounded instruction windows, NOT complete-function/native ABI execution.
"""
import argparse
import hashlib
import json
import struct
from pathlib import Path
import capstone


def main():
    p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
    raw=(a.sdk/'MyScriptText.dll').read_bytes();assert hashlib.sha256(raw).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
    pe=struct.unpack_from('<I',raw,60)[0];assert raw[pe:pe+4]==b'PE\0\0';n=struct.unpack_from('<H',raw,pe+6)[0];optional=struct.unpack_from('<H',raw,pe+20)[0]
    sections=[]
    for j in range(n):
        offset=pe+24+optional+40*j;size,rva,stored,at=struct.unpack_from('<IIII',raw,offset+8);sections.append((rva,stored,at))
    def read(va,size):
        rva=va-0x180000000
        for start,length,at in sections:
            if start<=rva and size<=length-(rva-start):return raw[at+rva-start:at+rva-start+size]
        raise AssertionError(('PE range',hex(va),size))
    md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
    expected={0x180167627:'subss xmm0, xmm1',0x180167630:'cvtps2pd xmm2, xmm0',
              0x18016763f:'mulsd xmm2, xmm8',0x180167644:'addsd xmm1, xmm0',0x180167648:'comisd xmm1, xmm2',
              0x180167655:'subss xmm0, xmm1',0x18016765c:'cvtps2pd xmm2, xmm0',
              0x18016766b:'mulsd xmm2, xmm8',0x180167670:'addsd xmm1, xmm0',0x180167674:'comisd xmm1, xmm2',
              0x180167732:'mov rcx, r15',0x180167735:'mov rdx, r13'}
    pins=[]
    for at,text in expected.items():
        ins=next(md.disasm(read(at,15),at));assert ins.mnemonic+' '+ins.op_str==text
        pins.append(dict(at=hex(at),bytes=ins.bytes.hex(),asm=text))
    assert read(0x1802ed3e0,8)==struct.pack('<d',8388608.)
    assert read(0x1802ed780,16)==struct.pack('<QQ',0x7fffffffffffffff,0x7fffffffffffffff)
    windows=[]
    for at,size in [(0x18016758b,21),(0x180167620,94),(0x18016772f,11)]:
        data=read(at,size);ii=list(md.disasm(data,at));assert sum(i.size for i in ii)==size
        windows.append(dict(start=hex(at),bytes=size,sha256=hashlib.sha256(data).hexdigest(),instructions=len(ii),kind='BOUNDED_WINDOW_NOT_COMPLETE_BODY'))
    #Setup pins bind the exact absolute-value mask and tolerance operands.
    assert read(0x18016758b,8).hex()=='660f6f35ed611800'
    assert read(0x180167597,9).hex()=='f2440f1005405e1800'
    out=dict(Status='PASS_NATIVE_DUPLICATE_SUBSS_THEN_DOUBLE',SDKExecuted=False,CompleteFunctionClaim=False,
             Windows=windows,InstructionPins=pins,SetupPins=2,DataPins=2,
             Order='SUBSS binary32 difference -> CVTPS2PD -> abs64mask -> MULSD8388608; sum separately converted abs terms with ADDSD; COMISD',
             PreviousPoint='Common loop-tail rcx=r15/rdx=r13 advances previous RAW pointers, including duplicate skip',
             Domain='Finite admitted coordinates abs<=1e6: subtraction cannot overflow; prepared scalar intermediates discriminate rounding, no Boolean mismatch claimed')
    a.output.write_text(json.dumps(out,indent=2)+'\n');print(json.dumps(dict(Status=out['Status'],windows=len(windows),instructions=sum(x['instructions'] for x in windows),pins=len(pins)+4)))


if __name__=='__main__':main()
