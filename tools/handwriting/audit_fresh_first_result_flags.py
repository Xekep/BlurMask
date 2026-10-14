"""Pinned static fresh driver/config/controller/result-pool and source-B9 publication flag chain."""
import argparse,hashlib,json,pathlib,sys

def main():
    p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
    root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
    from inspect_text_features import Binary,functions
    import capstone
    b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
    f=functions();cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
    def asm(first,last):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(first):b.off(last)],first))
    for name in ['180155ef0','180155da0']:assert '*(undefined1 *)(param_2 + 0x220) = 0' in f[name]
    for name in ['180155ef0','180155da0']:assert '*(undefined4 *)(param_2 + 0x1c8) = 0x3f800000' in f[name]
    assert 'param_2 + 0x1c8,4,1' in f['180158bc0']
    bind=f['180156630'];assert '+ 0x9b) = local_res18' in bind and 'local_res18 = param_3' in bind
    invocation=f['180158970'];assert 'FUN_180156630(param_1,param_2,*(undefined1 *)(param_2 + 0x220))' in invocation
    config=f['18000eca0'];assert '+ 0x9b) !=' in config and '0x12d0081' in config
    controller=asm(0x1801edc58,0x1801edc67);assert '[rdx + 0x100], cx' in controller
    event=f['1801f2480'];assert '0x12d0081' in event and '+ 0x101)' in event
    top=f['180157c60'];assert 'local_res20[0] = *(undefined1 *)(param_2 + 0x220)' in top and '(param_1,local_188,local_res20,1)' in top
    constructor=f['18013b870'];assert '+ 0x41) = *param_3' in constructor and '(param_1,uVar2,param_2 + 0x41,1)' in constructor
    pool=asm(0x18013ab80,0x18013ab92);assert 'byte ptr [r8]' in pool and '[rdx + 0x10], al' in pool
    assert b.qwords(0x1802e5a20+0x68,1)[0]==0x1801d62a0
    getter=asm(0x1801d62a0,0x1801d62a8);assert '[rdx + 0xb9]' in getter
    load=f['1801d4950'];assert 'param_2 + 0xb9,1' in load
    merge=f['1801ef070'];assert '*(undefined1 *)(param_2 + 0xf0) = uVar12' in merge
    at=merge.index('*(undefined1 *)(param_2 + 0xf0) = uVar12');assert '*plVar22 + 0x68' in merge[at-100:at]
    assert b.qwords(0x1802e5a20+0x70,1)[0]==0x1801d62b0
    assert 'movzx eax, word ptr [rdx + 0xba]' in asm(0x1801d62b0,0x1801d62b8)
    assert 'param_2 + 0xba,2' in load
    at=merge.index('*(undefined2 *)(param_2 + 0xf8) = uVar15');assert '*plVar22 + 0x70' in merge[at-160:at]
    assert '(param_1,0x28,*(undefined2 *)(param_2 + 0xf8))' in merge
    assert '0xba' not in f['1801d60d0']
    word=f['1801f42d0'];assert 'uVar33 = *(undefined1 *)(param_2 + 0xf0)' in word and 'local_4a8 = uVar33' in word
    result=dict(bindings=16,freshCapture='driver220=0 ->config9B=0 ->controller101=0;driver220 ->TOP27341 ->27410',
                wordGeometry='selected247 virtual68=1D62A0 ->resourceB9 ->controllerF0 ->279/275 finalize+sort',
                primaryCapacity='selected247 virtual70=1D62B0 ->serializedBA ->controllerF8 ->40-byte primary allocation; distinct from21b span cap andA0/A2/A4 result columns',
                sourceScoreExponent='155DA0/155EF0 fresh1C8=3F800000; serialized-driver158BC0 override excluded by raw API',
                policy='No reused driver/property-event overrides; true actualB9 does not get coerced tofalse',scope='Static PE reads only; no vendor execution')
    pathlib.Path(a.output).write_bytes((json.dumps(result,separators=(',',':'))+'\n').encode());print(json.dumps(result))
if __name__=='__main__':main()
