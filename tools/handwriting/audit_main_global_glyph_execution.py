"""Static-only global capture/pool,218560 and21FE30 composition closure."""
import hashlib,json,subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)!=2:raise SystemExit('report.json')
out=Path(sys.argv[1]);reports=[]
for script in ('audit_main_global_result_ownership.py','audit_main_word_packet.py','audit_glyph_chain_production.py','audit_main_word_publication.py','audit_main_global_source_width.py','audit_main_global_residual_source.py'):
    target=out.with_name(out.stem+'-'+script.removeprefix('audit_').removesuffix('.py')+'.json')
    subprocess.run([sys.executable,str(root/'tools/handwriting'/script),str(target)],check=True,cwd=root)
    reports.append(dict(script=script,report=target.name,sha256=hashlib.sha256(target.read_bytes()).hexdigest()))
out.write_text(json.dumps(dict(dependencies=reports,
    factory='fresh driver220=0 ->same TOP273 ->main240.V80/reset preservescapture; C4zero and actual247B9/controller22F9 select plain pool/publisher',
    seed='actual13F110 CreateEmpty fromEXPLICIT120allocator; preserve44..53, deriveeverypacketorigin fromrealinitialWORD',
    ordering='ApplyPacket ->ownedindividual217DB0 ->218560 privateactualproducers ->WORD.span ->positive-span successfulpublisher ->WORD.reset ->typedresultack',
    outcomes='span<=0 preserveschainbool includingfalse; positive-span requireschaintrue andnon-nullpublisher; callbackexception retainsprefix/noreset',
    limits=['no final2271F0 acceptance/quotes/normalization','preparedfailurecallbacks are not actualselectedfailurefixtures','stack256/row40/request64/WORD120 remain explicitallocationinputs','oldconcrete APIs remain separate; global usesactualsharedgraph'],
    noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS global freshownership/WORDseed/218560/21FE30 order andsuccess dependency audits; static only')
