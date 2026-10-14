"""Pinned static source-AL/sampler/loaded249 clone/24B branch bindings."""
import argparse,hashlib,json,pathlib,sys
def main():
    p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args();root=pathlib.Path(__file__).resolve().parents[2]
    sys.path.insert(0,str(root/'research/myscript/scripts'));from inspect_text_features import Binary,functions
    import capstone
    text=Binary();engine=Binary('MyScriptEngine.dll');f=functions();cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
    assert hashlib.sha256(text.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
    assert hashlib.sha256(engine.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
    def asm(b,start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}'for i in cs.disasm(b.data[b.off(start):b.off(end)],start))
    assert engine.qwords(0x1801de1b0+0x28,1)[0]==0x1800527e0
    lookup=asm(engine,0x1800527e0,0x180052a80)
    for n in ['mov edx, 0x1505','imul edx, edx, 0x21','xor edx, eax','shr rcx, 0x10','and ecx, 0xf','shr ecx, 6','and ecx, 0x1f','and edx, 0x3f','mov r13d, 0xffff','movzx eax, word ptr [rsi + 4]']:
        assert n in lookup,n
    init=f['180150d00'];assert 'local_a8 = *(undefined8 *)(param_2 + 0x58)' in init and 'plVar19 = (longlong *)plVar11[0xc]' in init
    bind=f['180163970'];assert '*puVar9 = *(undefined8 *)(param_2 + 8)' in bind
    assert text.qwords(0x1802df1d0+0x20,1)[0]==0x180164520
    assert '[rax + 0x10], xmm2' in asm(text,0x180164520,0x18016452f)
    assert text.qwords(0x1802de610+0x48,1)[0]==0x180151bd0
    facade=asm(text,0x180151bd0,0x180151be9);assert '[rdx + 0x70]' in facade and '[rax + 0x170]' in facade
    assert text.qwords(0x1802e1ff0+0x170,1)[0]==0x1801acf30
    assert '[rdx + 0x188]' in asm(text,0x1801acf30,0x1801acf38)
    graph=f['1801a6980'];assert '+ 0x88))(param_1)' in graph and '+ 0x70))()' in graph and '*(longlong *)(param_2 + 0x188) = lVar8' in graph
    assert text.qwords(0x1802e5a20+0x88,1)[0]==0x1801d6320
    assert '[rdx + 0x130]' in asm(text,0x1801d6320,0x1801d6328)
    assert text.qwords(0x1802e1490+0x70,1)[0]==0x18008ee00
    assert '[rdx + 0x40]' in asm(text,0x18008ee00,0x18008ee05)
    invocation=f['180158970'];assert '+ 0x48))(param_1)' in invocation and 'uVar12,uVar6,uVar5,&local_58' in invocation
    first=f['180157c60'];assert 'FUN_180157340(uVar27,param_5' in first
    reader=f['180158bc0'];assert '*(longlong *)(param_2 + 0x1e0) = lVar2' in reader and 'param_2 + 0x1c8,4,1' in reader
    prepare=f['180156200'];assert '0x210' in prepare and '*(undefined8 *)(param_2 + 0x1e0),0' in prepare
    clone=asm(text,0x180155da0,0x180155ee4)
    for n in ['mov dword ptr [rdx + 0x1c8], 0x3f800000','lea rcx, [rdx + 8]','lea rax, [r8 + 8]','mov edx, 3','lea rcx, [rcx + 0x80]','lea rax, [rax + 0x80]','mov rax, qword ptr [rax + 0x40]','mov qword ptr [rcx + 0x40], rax']:
        assert n in clone,n
    sampler=f['180196500'];assert 'param_2 + 0x40' in sampler and 'FUN_180010d80' in sampler
    for n in ['param_2 + 8,4,1','param_2 + 0x14,4,1','param_2 + 0x18,1,1','param_2 + 0x19,1,1','param_2 + 0x1a,1,1','param_2 + 0x30,4,1','param_2 + 300,1,1','param_2 + 0x130,4,1','local_res10,1,1']:
        assert n in sampler,n
    control=f['180170400'];assert '*(char *)(param_2 + 0x30)' in control and 'FUN_18016f170' in control and 'FUN_18016fec0' in control
    result=dict(textSHA=hashlib.sha256(text.data).hexdigest(),engineSHA=hashlib.sha256(engine.data).hexdigest(),lookup='Engine2B0527E0 charset5 byte span, hash5381*33 XORbyte, sortedcollisioncompare;FFFFmissing',category='selected247.resource130→clone297.runtime40→graph188→151BD0/1ACF30→158970param5→157340',firstSelection='249158BC0nestedchild→driver1E0→156200210binder→selectedresource; modelselection callbacks remain their own stage',exponent='158BC0serializedtail;155DA0reset1 THENcopy8..1CF including1C8: loaded.75 survivesclone',physicalResolution='source218.slot20=164520 actualruntime setter; not model height/DPI default',parentAlphabet='150D00selectedsource60→source219 config0/object8→secondary0,24B170220lookup',scope='Ordinary fresh prepared one-descriptor stage; parent main AL and child category owners deliberately distinct; no vendor execution')
    out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(result,separators=(',',':'))+'\n').encode());print(json.dumps(result))
if __name__=='__main__':main()
