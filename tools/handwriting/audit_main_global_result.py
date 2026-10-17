"""Static-only dependency closure for actual global result/scalar adapters.
No vendor code is loaded. Every sub-audit pins the same on-disk Text PE.
"""
import argparse,json,subprocess,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
output=Path(args.output).resolve();output.parent.mkdir(parents=True,exist_ok=True)
dependencies=['audit_main_global_word_pass.py','audit_case_main_span.py',
              'audit_case_main_compact_materialization.py','audit_owned_main_glyph_confidence.py',
              'audit_main_candidate_weight.py','audit_fresh_main_terminal_invocation.py']
reports=[]
for name in dependencies:
    report=output.with_name(output.stem+'-'+Path(name).stem+'.json')
    command=[sys.executable,str(ROOT/'tools/handwriting'/name)]
    command+=['--output',str(report)]if name=='audit_main_global_word_pass.py'else[str(report)]
    subprocess.run(command,check=True)
    reports.append(dict(audit=name,report=report.name,data=json.loads(report.read_bytes())))
output.write_text(json.dumps(dict(dependencies=reports,
    ownership='Global finalV80 result delegates the existing same292 retained-map lease. No new D0, query reset, backend or manual mask. Private snapshots preserve four180-vectors from each actual D0 single evaluation; scalar kernels are shared with old concrete confidence.',
    scope='Numeric2257C0 only, global fresh one292/noAssociated/secondary1/nontracing. Post-result request contents remain explicitly prepared controls, not207780 producer or published text.',
    vendorExecution=False),indent=2)+'\n',encoding='utf8')
print(f'PASS {len(reports)} pinned native dependency audits; global adapter adds ownership/cache lifecycle, no vendor execution')
