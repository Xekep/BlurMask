"""Static-only pinned24B→720→C1A producer/consumer audit. No DLL is loaded."""
import argparse,hashlib,json,pathlib,sys

def main():
    p=argparse.ArgumentParser();p.add_argument('--output',required=True);a=p.parse_args()
    root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
    from inspect_text_features import Binary,functions
    text=Binary('MyScriptText.dll');ink=Binary('MyScriptInk.dll')
    assert hashlib.sha256(text.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
    assert hashlib.sha256(ink.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
    f=functions('MyScriptText.dll');producer=f['18016f6f0'];controller=f['180170400'];prepare=f['180170220'];reader=f['180170790']
    assert text.qwords(0x1802df7f0+0x18,1)[0]==0x180170400
    assert 'FUN_18016fec0' in controller and 'FUN_18016f6f0' in f['18016fec0']
    for needle in ['0x2d0 + *local_78','uVar10 * 0x248','lVar6 + 0x14','lVar6 + 0x28','lVar6 + 0x38','lVar6 + 0x3c','lVar6 + 0x88','lVar6 + 0x178','lVar6 + 0x17c','lVar6 + 0x218','lVar6 + 0x21c','0x14','local_88,0xc1a','*(code *)*puVar7']:
        assert needle in producer,needle
    assert '+ 0x34)' in f['18016fec0'] and '+ 0x38)' in f['18016fec0'] and 'param_3 + 200' in f['18016fec0']
    for needle in ['param_2 + 0x10,4,1','param_2 + 0x14,4,1','param_2 + 0x18,4,1','param_2 + 0x28,4,1','param_2 + 0x2c,4,1','param_2 + 0x1c,1,1','param_2 + 0x1d,1,1','0x102 < param_4']:
        assert needle in reader,needle
    assert '/ 25.4) * 0.75' in prepare and 'local_28 = local_28 * 0.25' in prepare
    literals=[(0x1802eb368,4,'f3b08082'),(0x1802eb370,4,'f3b08083'),(0x1802e9b80,1,'2d'),(0x1802e9bac,1,'5f'),(0x1802eb700,3,'e383bc'),(0x1802eb704,3,'e4b880'),(0x1802eb388,3,'efbfbd')]
    for va,n,expected in literals:assert text.data[text.off(va):text.off(va)+n].hex()==expected
    address,descriptor=ink.descriptor(0xc1a);assert ink.qwords(descriptor[4],1)[0]==0x1800221a0
    source=(root/'research/myscript/output/Math2FeaturesStage4/MyScriptInk.dll/decompiled.c').read_text()
    start=source.index('/* Address 1800221a0;');end=source.index('/* Address 1800222d0;',start);completion=source[start:end]
    for needle in ['FUN_180021110','local_18[0] <= 0.0','*param_3','param_3[3] - param_3[1]','* 0.05','* 0.5','param_5[2] = 0.0']:
        assert needle in completion,needle
    result=dict(textSHA=hashlib.sha256(text.data).hexdigest(),inkSHA=hashlib.sha256(ink.data).hexdigest(),sourceLayout='C8 pointer720,descriptor34/38 half-open;physical24/28 inclusive',producer='Text24B170400→16FEC0→16F6F0→InkC1A0221A0→021110',exactItemCount=20,markerUtf8=[v[2]for v in literals],unresolved=['actual parent alphabet lookup owner','24B full lifecycle and16F170 deskew','remaining24A/24D/29B main processors','real selected glyph/winner materializer'])
    out=pathlib.Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(result,separators=(',',':'))+'\n').encode());print(json.dumps(result))
if __name__=='__main__':main()
