"""Pinned global216BC0 caller and pending scratch semantics; PE reads only."""
import argparse,json,subprocess,sys
from pathlib import Path
import capstone
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args();output=Path(args.output)
b=Binary();md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
pins=[(0x1801aaf64,'mov','byte ptr [rcx + 0xd], al'),(0x1801aaf67,'test','r15, r15'),
 (0x1801aaf6a,'je','0x1801ac0c1'),(0x1801aaf70,'test','r13d, r13d'),(0x1801aaf73,'jle','0x1801ac0c1'),
 (0x1801ab087,'test','rdx, rdx'),(0x1801ab08a,'jle','0x1801ab101'),
 (0x1801ab101,'dec','edi'),(0x1801ab103,'dec','rdx'),(0x1801ab11b,'mov','dword ptr [rax + 8], edi')]
for address,mnemonic,operands in pins:
    i=next(md.disasm(b.data[b.off(address):b.off(address)+15],address));assert(i.mnemonic,i.op_str)==(mnemonic,operands)
dependencies=[]
for name,option in [('audit_glyph_chain_production.py',False),('audit_main_global_source_callbacks.py',True)]:
    report=output.with_name(output.stem+'-'+Path(name).stem+'.json')
    subprocess.run([sys.executable,str(ROOT/'tools/handwriting'/name)]+(['--output']if option else[])+[str(report)],check=True)
    dependencies.append(dict(audit=name,report=report.name))
output.write_text(json.dumps(dict(pins=[f'{a:x} {m} {o}'for a,m,o in pins],dependencies=dependencies,
 scope='Fresh global ordinary protocol subtype3, same actual headers/graph/pending contexts. Actual global backtrack72 rows tested but packetAC/B0 remain explicit prepared scalars. No segment/winner/publication claim.',
 initial='1AAE70 writes C/D before null-scratch or cursor<=0 early return. Fresh consumed fallback/controls derive both flags0; initial21FFD0 has no synthetic delimiter row.',
 pending='Once capacity exhausts, loop decrements cursor without writing; global caller retains actual RemainingCursor. Separate prepared numeric controls cover negative cursor; no actual negative metadata fixture claim.',
 safety='Negative cursor with component0 is refused by existing2173E0 portable finalizer; no native out-of-bounds read is recreated.',vendorExecution=False),indent=2)+'\n',encoding='utf8')
print('PASS10 exact pending/initial instructions+2static dependency closures; no vendor execution')
