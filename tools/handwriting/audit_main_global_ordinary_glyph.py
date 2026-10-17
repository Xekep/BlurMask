"""Global ordinary216BC0 binding proof; static PE reads only."""
import argparse,hashlib,json,subprocess,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args();output=Path(args.output)
b=Binary();fs=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x1801bc680,0x1801bc900;sha=hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()
assert sha=='b054bad0cc0e462bbcb49473f4edb8f35a8680c476f583cb56f95b0bb2be4b05'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
pins=[(0x1801bc6a8,'mov','r12, r9'),(0x1801bc6ab,'movzx','r14d, word ptr [rdx + 0x168]'),
 (0x1801bc6b3,'mov','rsi, r8'),(0x1801bc8d3,'mov','ecx, dword ptr [r12 + r14*4]'),
 (0x1801bc8d9,'mov','dword ptr [rsi + 0x18], ecx'),(0x1801c1445,'mov','eax, dword ptr [r8 + 0x18]')]
for address,mnemonic,operands in pins:
    i=next(md.disasm(b.data[b.off(address):b.off(address)+15],address));assert(i.mnemonic,i.op_str)==(mnemonic,operands)
assert '*(undefined4 *)(param_3 + 0x18) = *(undefined4 *)(param_4 + (ulonglong)uVar1 * 4);'in fs['1801bc680']
assert 'iVar4 = *(int *)(lVar6 + 0x18);'in fs['1801c13a0']
dependencies=[]
for name,option in [('audit_ordinary_glyph_production.py',False),('audit_main_global_group_costs.py',True),('audit_main_global_result.py',True)]:
    report=output.with_name(output.stem+'-'+Path(name).stem+'.json')
    subprocess.run([sys.executable,str(ROOT/'tools/handwriting'/name)]+(['--output']if option else[])+[str(report)],check=True)
    dependencies.append(dict(audit=name,report=report.name))
output.write_text(json.dumps(dict(fallback=dict(start=hex(a),endExclusive=hex(z),sha256=sha,pins=[f'{a:x} {m} {o}'for a,m,o in pins]),dependencies=dependencies,
 scope='Fresh global ordinary subtype0 prepared72 inputs; same single D0 snapshots and original193860 group cost owner. No first6120 frame, surrogate graph, extraNN, fabricated fallback or knowledge99 flag.',
 fallbackSemantics='1BC680 takes final class[ALcount] into group38.field18. 1C13A0 blank runtime168 reads that field. All631 captured fallback values finite and different from class179.',
 limits=['protocol subtype3 producer separate','blank179 invalid sampler category refuses after native prefix writes','not a source72 winner/segmentation/text producer'],vendorExecution=False),indent=2)+'\n',encoding='utf8')
print(f'PASS6 fallback instructions/1pinned region/3static dependency closures; global owned ordinary cache/scoring, no vendor execution')
