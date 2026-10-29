"""Strict reviewed Core source admission; canonical LF census, not raw PDB proof."""
from pathlib import Path
import hashlib,json
MANIFEST_SHA256='9761299433746e2112e72bc82c28fcc5938de690d3019846ee3b64fc16381482'
DEFAULT_CORE_COUNT=937
DIAGNOSTIC_CORE_COUNT=939
sha=lambda raw:hashlib.sha256(raw).hexdigest()

def verify(root):
    raw=Path(__file__).with_name('publication-core-source-admissions.json').read_bytes()
    assert sha(raw)==MANIFEST_SHA256,'Unknown Core admission manifest'
    manifest=json.loads(raw);baseline=manifest['baseline']
    assert manifest['format']==1 and baseline['count']==935
    assert (manifest['defaultCoreCount'],manifest['diagnosticCoreCount'],manifest['diagnosticAdditionalSources'])==(DEFAULT_CORE_COUNT,DIAGNOSTIC_CORE_COUNT,2)
    additions={row['path']:row for row in manifest['additions']}
    assert set(additions)=={'LegacyOwnedRawInkAppendState.cs','LegacyOwnedCurrentRawIdentityExport.cs'}
    core=Path(root)/'src/InkRecognition'
    files={p.relative_to(core).as_posix():p for p in core.rglob('*')
           if p.is_file() and p.suffix.lower()=='.cs' and not {'obj','bin'}&set(p.relative_to(core).parts)}
    assert set(additions)<=set(files),'Missing reviewed Core addition'
    canonical=lambda p:p.read_bytes().replace(b'\r\n',b'\n')
    for name,row in additions.items():
        data=canonical(files[name])
        assert sha(data)==row['canonicalLfSha256'] and len(data)==row['canonicalLfBytes'],'Tampered reviewed Core addition: '+name
    names=sorted(set(files)-set(additions))
    assert len(names)==935,'Unexpected baseline Core source count'
    assert sha(('\n'.join(names)+'\n').encode('utf-8'))==baseline['filenameSetSha256'],'Unknown/missing baseline Core filename'
    content=b''.join(name.encode('utf-8')+b'\0'+sha(canonical(files[name])).encode('ascii')+b'\n'for name in names)
    assert sha(content)==baseline['canonicalLfContentSetSha256'],'Tampered baseline Core content'
    assert len(files)==DEFAULT_CORE_COUNT
    return dict(Scope=manifest['scope'],BaselineCommit=baseline['commit'],BaselineCount=935,
                BaselineFilenameSetSHA256=baseline['filenameSetSha256'],BaselineCanonicalLfContentSetSHA256=baseline['canonicalLfContentSetSha256'],
                ReviewedAdditions=manifest['additions'],DefaultCoreCount=DEFAULT_CORE_COUNT,DiagnosticCoreCount=DIAGNOSTIC_CORE_COUNT,
                Canonicalization=manifest['canonicalization'],RawPdbProof=False)
