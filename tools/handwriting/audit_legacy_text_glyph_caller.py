"""Read-only fixed PE default274 owner / fresh0213A0 caller proof."""
import argparse,hashlib,json,pathlib,sys

def main():
    p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
    root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
    from inspect_text_features import Binary,functions
    import capstone
    text=Binary();engine=Binary('MyScriptEngine.dll');f=functions()
    assert hashlib.sha256(text.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
    assert hashlib.sha256(engine.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
    assert text.qwords(0x1802dc680+0x28,1)[0]==0x18013ae90
    resolver=f['18013ae90'];assert '* 0x28' in resolver and 'param_3 >> 0x28' in resolver and '<< 2) >> 4' in resolver
    assert 'param_2 + 8' in resolver and 'uVar4 != puVar1[2]' in resolver
    caller=f['1800213a0'];assert '0x81999ff0' in caller and '0x82999ff0' in caller and '0x100000' in caller
    assert 'param_3[8] + 0xe' in caller and '(int)uVar10 < *param_8' in caller
    outer=f['180024f30'];at=outer.index('FUN_1800213a0')
    assert '0x68) = 0' in outer[at-250:at] and '0x70) = 0' in outer[at-250:at]
    reader=functions('MyScriptEngine.dll')['180053f20'];assert 'param_2 + 0xe,2,1' in reader
    cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
    asm='\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(text.data[text.off(0x18013ae90):text.off(0x18013af30)],0x18013ae90))
    assert 'sar' in asm and '0x28' in asm and '0x18' in asm
    result=dict(bindings=10,glyphRecordStride=40,sourceRecordStride=40,headerE='serialized field3 at+6 -> runtime+E',
                nativeLookup='13AE90 signed index, normalized BST key, exact packed count',
                freshCaller='024F30 zeros output68/70 before0213A0',scope='Static only; no DLL execution or accepted winner inferred')
    pathlib.Path(a.output).write_bytes((json.dumps(result,separators=(',',':'))+'\n').encode());print(json.dumps(result))
if __name__=='__main__':main()
