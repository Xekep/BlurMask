"""Independent pinned source/ABI and public Script9 audit. Only reads PE bytes and public files."""
import argparse,collections,hashlib,json,pathlib,struct,sys

ENGINE='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
TEXT='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'

def main():
    p=argparse.ArgumentParser();p.add_argument('model');p.add_argument('scripts');p.add_argument('--output',required=True);a=p.parse_args()
    root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
    from inspect_text_features import Binary,functions
    import capstone
    eng,txt=Binary('MyScriptEngine.dll'),Binary('MyScriptText.dll')
    assert hashlib.sha256(eng.data).hexdigest()==ENGINE and hashlib.sha256(txt.data).hexdigest()==TEXT
    assert eng.qwords(0x18023cfc8,3)==(0x180031390,0x18023cf80,0x1800315e0)
    assert eng.qwords(0x180202a70+0x30,1)[0]==0x18005f290
    assert eng.qwords(0x180202a70+0x48,3)==(0x18005fa80,0x18005fae0,0x18005faf0)
    cs=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
    def dis(b,start,end):return '\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in cs.disasm(b.data[b.off(start):b.off(end)],start))
    publication=dis(txt,0x180021000,0x1800210d0)
    assert '[rsp + 0x60]' in publication and '[rax + 0x10]' in publication and '[rsp + 0x20], rbx' in publication and 'xor ebx, ebx' in publication
    ctor=functions()['18001c010'];assert '0x3e' in ctor and '0x118' in ctor
    intern=functions('MyScriptEngine.dll')['180030e60'];assert 'memcmp' in intern and 'uVar4 == 0' in intern
    cluster=functions('MyScriptEngine.dll')['18005fa80'];assert 'param_2 + 8) = 1' in cluster
    scripts=pathlib.Path(a.scripts);assert hashlib.sha256(scripts.read_bytes()).hexdigest()=='fba415952f5654145acad220dc2b878f815c673474d2bb4928934e3ba6ccca1d'
    expected={}
    for line in scripts.read_text().splitlines():
        line=line.split('#')[0].strip()
        if not line:continue
        interval,name=map(str.strip,line.split(';')); first,*rest=interval.split('..')
        for cp in range(int(first,16),int(rest[0],16)+1 if rest else int(first,16)+1):expected[cp]=name
    model=json.loads(pathlib.Path(a.model).read_text());native={}
    for i,row in enumerate(model['scriptRanges']):
        assert list(struct.unpack_from('<iii',eng.data,eng.off(0x1801fb6d0)+i*12))==row
        first,last,kind=row
        for cp in range(first,last+1):native[cp]=kind
    correspond=collections.defaultdict(collections.Counter)
    for cp,name in expected.items():correspond[native.get(cp,0)][name]+=1
    majority={k:counts.most_common(1)[0][0] for k,counts in correspond.items()}
    exceptions=[dict(cp=cp,public=name,nativeScript=native.get(cp,0)) for cp,name in expected.items() if majority[native.get(cp,0)]!=name]
    # Compare dictionary membership with the actual native relative-bound trie on positive/prefix/changed inputs.
    rows=[]
    for i in range(120):
        at=txt.off(0x18030e160)+i*32; ptr,*fields=struct.unpack_from('<Q6i',txt.data,at)
        off=txt.off(ptr);s=txt.data[off:txt.data.index(b'\0',off)]+b'\0';rows.append((s,fields))
        assert model['literalRows'][i]==[s[:-1].hex(),*fields[:5]]
    def trie(raw):
        raw=raw.split(b'\0')[0]+b'\0';lo,hi=0,120;depth=0
        while lo<hi:
            mid=(lo+hi)//2;s,f=rows[mid]; x=s[depth] if depth<len(s) else 0;y=raw[depth] if depth<len(raw) else 0
            if y<x:hi=mid
            elif y>x:lo=mid+1
            elif depth<2 and y:
                lo,hi=mid+f[1+2*depth],mid+f[2+2*depth]+1;depth+=1
            else:return f[0] if s[depth:]==raw[depth:] else -1
        return -1
    literal={bytes.fromhex(row[0]):row[1] for row in model['literalRows']}; tested=0;unreachable=[]
    for s in literal:
        if trie(s)!=literal[s]:unreachable.append(s.hex())
        for v in [s,s+b'\0extra',s+b'x',s[:-1],b'x'+s]:
            actual=trie(v)
            assert actual==-1 or actual==literal.get(v.split(b'\0')[0],-1),(v,actual);tested+=1
    result=dict(sourceEngine=ENGINE,sourceText=TEXT,provenBindings=8,scriptRanges=716,literalRows=120,
                relativeTrieControls=tested,unreachableStoredLiteralHex=unreachable,publicAssignedCodepoints=len(expected),publicScriptExceptionCount=len(exceptions),
                publicScriptExceptions=exceptions,nativeScripts={str(k):dict(v) for k,v in correspond.items()},
                scope='Static pinned owners/defaults; no DLL execution or accepted winner claim')
    pathlib.Path(a.output).write_bytes((json.dumps(result,separators=(',',':'))+'\n').encode());print(json.dumps({k:v for k,v in result.items() if k not in ['publicScriptExceptions','nativeScripts']}))

if __name__=='__main__':main()
