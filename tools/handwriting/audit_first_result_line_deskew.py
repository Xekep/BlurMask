"""Pinned static16F170 float operation/order, range traversal and guide writes."""
import argparse,hashlib,json,pathlib,sys
def main():
    p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args();root=pathlib.Path(__file__).resolve().parents[2]
    sys.path.insert(0,str(root/'research/myscript/scripts'));from inspect_text_features import Binary,functions
    import capstone
    b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
    source=functions()['18016f170']
    for needle in ['param_3[2]','(double)(fVar17 * fVar17) + 1.0','1.0 - fVar25 * fVar25','param_10 ==','param_2 + 0x40','*param_3 = fVar18','param_3[1] = fVar21','uVar12 = (ulonglong)(param_6 - param_5)','plVar13[1] == 0','uVar11 * 0x18','plVar13 = plVar13 + 0x5a']:
        assert needle in source,needle
    cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64);asm='\n'.join(i.mnemonic+' '+i.op_str for i in cs.disasm(b.data[b.off(0x18016f170):b.off(0x18016f6f0)],0x18016f170))
    for needle in ['mulss xmm0, xmm6','cvtps2pd xmm1, xmm0','divss xmm8, xmm0','subss xmm0, xmm1','cmp byte ptr [rsp + 0xd8], 0','movss dword ptr [rbx], xmm1','movss dword ptr [rbx + 4], xmm0']:
        assert needle in asm,needle
    result=dict(textSHA=hashlib.sha256(b.data).hexdigest(),entry='18016F170',guideWrites=['0 baseline','4 height'],guideUntouched='slope8 and originalC/10 plus metadata14..27',rows='half-open source interval;24-byte source start/end keys, inclusive retained point ranges',zeroRangeBranch='transform opposite corners; do not reorder to a bbox',safety='Owned snapshots/work bound/finite guards added; native DLL not executed')
    out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(result,separators=(',',':'))+'\n').encode());print(json.dumps(result))
if __name__=='__main__':main()
