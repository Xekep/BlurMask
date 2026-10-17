"""Pinned ordinary217DB0/1AC3C0/1AA470 closure on global owning sources.

No PE execution; numerical/global owner comparisons are separate probe audits.
"""
import hashlib, json, subprocess, sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2:raise SystemExit('report.json')
out=Path(sys.argv[1]);dependencies=[]
b=Binary();f=functions()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert b.data[b.off(0x18000c8cb):b.off(0x18000c8d2)].hex()=='48898798000000'
#The old score audit pins the preceding zero-RAX constructor provenance.
ctor=f['18000c860'];assert '+ 0x98)' in ctor
wrapper=f['1802458b0']
assert '*(undefined1 *)(lVar11 + 0x99) = 1;' in wrapper
assert '*(undefined1 *)(*(longlong *)(*(longlong *)(param_2 + 0x10) + 8) + 0x99) = uVar2;' in wrapper
for script in ('audit_main_residual_source.py','audit_main_glyph_source_metadata.py','audit_main_global_source_callbacks.py','audit_owned_main_glyph_confidence.py'):
    target=out.with_name(out.stem+'-'+script.removeprefix('audit_').removesuffix('.py')+'.json')
    args=['--output',str(target)] if script=='audit_main_global_source_callbacks.py' else [str(target)]
    subprocess.run([sys.executable,str(root/'tools/handwriting'/script),*args],check=True,cwd=root)
    dependencies.append(dict(script=script,report=target.name,sha256=hashlib.sha256(target.read_bytes()).hexdigest()))
out.write_text(json.dumps(dict(dependencies=dependencies,
    knowledge99='00C8CB clears98..9F in fresh constructor; scope excludes2458B0 temporary override and external arbitrary/reused property state',
    chain='actualglobalFlushPlan individual72 segments ->217DB0 ->ordinary1AC3C0 node-local extras ->V1401AA470',
    base='V140 preserves canonical target64bits and projects actualStart/EndContext throughglobalspan0DFS; no group-range shortcut',
    higher='ordinarykind0 projects stored mergedranges through same shared preparedchronology/originalF8+108, not surrogateDFS; originalLast preserves basegeometry/NN',
    verificationBaseline='83b3d68 partial controller plus exact owned residual files; parallel213C00 reaches separate kind7 guard',
    limits=['positive217DB0 controls prepared, actual16flushes residual0','acknowledgements prepared until actual218560 executor supplies same-flush outcome','multi-inputhigher unsupported because nativeconcat directlyreadsbasearcarrays beforecache','complete controller/213C00-kind7 integration separate'],
    noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS global residual/source closure: native merge+metadata+actualglobalcallback dependency pins; static only')
