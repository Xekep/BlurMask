"""Pinned read-only default search/charset/conditional writeback proof; never loads a native DLL."""
import argparse,hashlib,json,pathlib,sys

def main():
    p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
    root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
    from inspect_text_features import Binary,functions
    import capstone
    b=Binary('MyScriptEngine.dll');assert hashlib.sha256(b.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
    assert b.qwords(0x180252408,1)[0]==0x18020f3c0
    assert b.qwords(0x18020f3c0+0x20,1)[0]==0x18020f290
    assert b.qwords(0x18020f290+0x10,1)[0]==0x180089680
    f=functions('MyScriptEngine.dll');bootstrap=f['180031910'];at=bootstrap.index('lVar9 = DAT_180252408')
    assert '+ 0x90) = *(undefined8 *)(lVar9 + 0x20)' in bootstrap[at:at+3500]
    cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
    def dis(start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(start):b.off(end)],start))
    search=dis(0x1800896bc,0x180089746)
    assert 'sub r8, rdi' in search and '1801da250' in search and '1801da170' in search
    gate=dis(0x18004a2f2,0x18004a31b)
    assert 'test r15b, r15b' in gate and 'je 0x18004a42f' in gate
    endUpdate=f['180094960'];assert '*param_4 = iVar15' in endUpdate and 'param_4[2]' in endUpdate
    switch=f['1800491a0'];assert 'switch((int)lVar6 - (int)pcVar1)' in switch
    assert 'local_a8' in switch and 'local_b8' in switch and 'local_f0 = 0x8080b0f3' in switch
    output=dict(bindings=9,search='global90 default service37 table20F290 virtual10=089680, explicit bounded end/memchr',
                originalWriteback='04A2F2 skipped unless changed15B;094960 updates end iterator',
                scope='Default valid UnicodeUTF8. Static source bytes only; no vendor code execution.')
    pathlib.Path(a.output).write_bytes((json.dumps(output,separators=(',',':'))+'\n').encode());print(json.dumps(output))
if __name__=='__main__':main()
