"""Strict SOURCE-only default/diagnostic raw projection contract.
No compilation, resource/model arithmetic or native code is invoked.
"""
from pathlib import Path
import hashlib,json,xml.etree.ElementTree as ET
SYMBOL=b'BLURMASK_PUBLICATION_DIAGNOSTICS'
PINS_SHA='00089f23659d7e3326d8589f91b2db2c97681b822d624efa6fbdd446c10e873d'
sha=lambda b:hashlib.sha256(b).hexdigest()

def original_project_policy(project,expected_sha):
    """Authenticate the exact metadata rename before restoring historical bytes."""
    if sha(project)==expected_sha:return project,False
    assert sha(project)=='512c5702608918e636921758fbca182ee70fbdbdbaea74a185d55ebd9ace2271','Unknown project flavor policy'
    for current,original in [
        (b'<PackageId>NovaPen.InkRecognition</PackageId>',b'<PackageId>BlurMask.InkRecognition</PackageId>'),
        (b'<RepositoryUrl>https://github.com/Xekep/NovaPen</RepositoryUrl>',b'<RepositoryUrl>https://github.com/Xekep/BlurMask</RepositoryUrl>')]:
        assert project.count(current)==1,'Exact NovaPen metadata required'
        project=project.replace(current,original)
    assert sha(project)==expected_sha,'Historical project policy differs after metadata inverse'
    return project,True
def preprocess(raw,enabled):
    out=[];stack=[];active=True
    for line in raw.splitlines(keepends=True):
        marker=line.strip()
        if marker==b'#if '+SYMBOL:
            stack.append((active,enabled));active=active and enabled
        elif marker==b'#else':
            assert stack,'Unmatched else';parent,condition=stack[-1];active=parent and not condition
        elif marker==b'#endif':
            assert stack,'Unmatched endif';active=stack.pop()[0]
        else:
            assert not marker.startswith((b'#if',b'#elif',b'#define',b'#undef')),'Unknown conditional source policy'
            if active:out.append(line)
    assert not stack,'Unclosed source condition';return b''.join(out)
def verify(root,require_meter_v2=False):
    root=Path(root);meta=Path(__file__).with_name('publication-diagnostic-flavor-source-pins.json').read_bytes()
    assert sha(meta)==PINS_SHA,'Unknown flavor proof metadata';pins=json.loads(meta);assert pins['symbol']==SYMBOL.decode()
    default=diagnostic=0;changedBound=[]
    for row in pins['core']:
        raw=(root/row['target']).read_bytes()
        if row['target'].endswith('/LegacyFreshMainGlobalWordTextPublication.cs'):
            helper=Path(__file__).with_name('wordtext_matching_bound_source.py');assert sha(helper.read_bytes())=='001ba4df9df9155b629b34a5dececad328cf23ae392e77b46d40b14f7a509850'
            import runpy
            raw,bound=runpy.run_path(str(helper))['restore'](raw)
            if bound is not None:changedBound.append(bound)
        assert sha(raw)==row['conditionalSHA256'],'Unknown conditional body: '+row['target']
        off=preprocess(raw,False);on=preprocess(raw,True)
        assert sha(off)==row['defaultSHA256']and len(off)==row['defaultRawBytes'],'Default must be raw935 exact'
        assert sha(on)==row['diagnosticSHA256']and len(on)==row['diagnosticRawBytes'],'Diagnostic must be rawC937 exact'
        if row.get('toolsOnlyCollector'):
            assert off==b'';assert not(root/'src/InkRecognition'/Path(row['target']).name).exists(),'Collector must not remain under production source glob'
        else:
            default+=1
            for name in [b'LegacyPublicationScoreTrace',b'candidateTrace',b'scoreTrace',b'ScoreTrace',b'LegacyPublicationCandidateCleanup']:
                assert name not in off,'Default source contains diagnostic identifier'
        diagnostic+=1
    assert(default,diagnostic)==(6,8)
    project,metadata_inverse=original_project_policy((root/pins['project']['target']).read_bytes(),pins['project']['sha256'])
    xml=ET.fromstring(project);properties=[e for group in xml.findall('PropertyGroup')for e in group]
    default_prop=[p for p in properties if p.tag=='BlurMaskPublicationDiagnostics'];assert len(default_prop)==1 and default_prop[0].text=='false'and default_prop[0].get('Condition')=="'$(BlurMaskPublicationDiagnostics)' == ''"
    defined=[p for p in properties if p.tag=='DefineConstants'];assert any(p.get('Condition')=="'$(BlurMaskPublicationDiagnostics)' == 'true'"and p.text=='$(DefineConstants);BLURMASK_PUBLICATION_DIAGNOSTICS'for p in defined)
    groups=[g for g in xml.findall('ItemGroup')if g.get('Condition')=="'$(BlurMaskPublicationDiagnostics)' == 'true'"]
    items=[x for g in groups for x in g.findall('Compile')];assert len(items)==2 and all('/tools/'in('/'+x.get('Include',''))for x in items)
    targets={t.get('Name'):t for t in xml.findall('Target')}
    assert targets['RequirePublicationDiagnosticArtifacts'].get('Condition')=="'$(BlurMaskPublicationDiagnostics)' == 'true'"
    assert 'Publish'in targets['RejectPublicationDiagnosticDistribution'].get('BeforeTargets','')and'Pack'in targets['RejectPublicationDiagnosticDistribution'].get('BeforeTargets','')
    for row in pins['toolFiles']:assert sha((root/row['target']).read_bytes())==row['sha256'],'Unknown positive consumer/tool source: '+row['target']
    meter=[]
    for row in pins['meterFixFiles']:
        got=sha((root/row['target']).read_bytes());assert got in(row['original935SHA256'],row['v2SHA256']),'Unknown meter body'
        if require_meter_v2:assert got==row['v2SHA256'],'Production positive mode requires separately reviewed meterV2'
        meter.append('V2'if got==row['v2SHA256']else'Original935')
    return{'Scope':'SOURCE_ONLY_NO_COMPILED_IL_OR_MODEL_CLAIM','DefaultCoreRaw935Bodies':default-len(changedBound),'HistoricalDefaultCore935BodiesAfterExactInverse':default,'ChangedBoundSourceViews':changedBound,'NovaPenProjectMetadataExactInverse':metadata_inverse,'DefaultCollectorSourceCount':0,'DiagnosticRawC937Files':diagnostic-len(changedBound),'HistoricalDiagnosticC937FilesAfterExactInverse':diagnostic,'DefaultProperty':False,'ToolsOnlyCollectors':2,'MeterBodies':meter,'RequireMeterV2':require_meter_v2,'FullProducerIndependent':False}
