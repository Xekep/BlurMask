"""Strict reviewed Core source admission; canonical LF census, not raw PDB proof."""
from pathlib import Path
import hashlib,json
MANIFEST_SHA256='39553362d9f31421f1196cece9dbaa3e995ac5452a54d09cf1bb491bf4c3febf'
DEFAULT_CORE_COUNT=951
DIAGNOSTIC_CORE_COUNT=953
sha=lambda raw:hashlib.sha256(raw).hexdigest()

def _rows(rows,count):
    result={row['path']:row for row in rows}
    assert len(rows)==len(result)==count,'Duplicate or unexpected admission row count'
    assert all(Path(name).name==name and '/'not in name and '\\'not in name and name.endswith('.cs')for name in result),'Noncanonical admission filename'
    return result

def _verify_sources(root,manifest):
    """Private pure validator; production entry first authenticates manifest bytes."""
    baseline=manifest['baseline'];english=manifest['english951']
    assert manifest['format']==2 and baseline['count']==935
    assert (manifest['defaultCoreCount'],manifest['diagnosticCoreCount'],manifest['diagnosticAdditionalSources'])==(DEFAULT_CORE_COUNT,DIAGNOSTIC_CORE_COUNT,2)
    additions=_rows(manifest['additions'],2)
    assert set(additions)=={'LegacyOwnedRawInkAppendState.cs','LegacyOwnedCurrentRawIdentityExport.cs'}
    english_additions=_rows(english['additions'],14)
    overrides=_rows(english['overrides'],67)
    protected=_rows(english['protected'],13)
    assert 'LegacyEnglishCaseNode0Ledger.cs'in protected
    assert sum(row['role']=='RetainedMainDelta'for row in protected.values())==12
    assert protected['LegacyEnglishCaseNode0Ledger.cs']['role']=='UnchangedNode0LedgerCaps'
    assert not(set(additions)&set(english_additions)or set(overrides)&(set(additions)|set(english_additions)|set(protected)))
    core=Path(root)/'src/InkRecognition'
    files={p.relative_to(core).as_posix():p for p in core.rglob('*')
           if p.is_file() and p.suffix.lower()=='.cs' and not {'obj','bin'}&set(p.relative_to(core).parts)}
    canonical=lambda p:p.read_bytes().replace(b'\r\n',b'\n')
    hashes={}
    def digest(name):
        if name not in hashes:hashes[name]=sha(canonical(files[name]))
        return hashes[name]
    for group in (additions,english_additions,overrides,protected):
        for name,row in group.items():
            assert name in files,'Missing reviewed Core source: '+name
            assert digest(name)==row['canonicalLfSha256'],'Tampered reviewed Core source: '+name
            if 'canonicalLfBytes'in row:
                assert len(canonical(files[name]))==row['canonicalLfBytes'],'Wrong reviewed source size: '+name
    names=sorted(set(files)-set(additions)-set(english_additions))
    assert len(names)==935,'Unexpected baseline Core source count'
    assert sha(('\n'.join(names)+'\n').encode('utf-8'))==baseline['filenameSetSha256'],'Unknown/missing baseline Core filename'
    # Approved current bodies were authenticated above. Substitute only their pinned
    # digests to verify the historical aggregate; never accept old bodies as current
    # source and never write restored bytes to disk.
    restored={name:row['preimageCanonicalLfSha256']for name,row in overrides.items()}
    assert set(restored)<=set(names)
    content=b''.join(name.encode('utf-8')+b'\0'+(restored[name]if name in restored else digest(name)).encode('ascii')+b'\n'for name in names)
    assert sha(content)==baseline['canonicalLfContentSetSha256'],'Tampered baseline Core content/preimage admission'
    assert len(files)==DEFAULT_CORE_COUNT,'Unexpected total Core source count'
    return dict(Scope=manifest['scope'],BaselineCommit=baseline['commit'],BaselineCount=935,
                BaselineFilenameSetSHA256=baseline['filenameSetSha256'],BaselineCanonicalLfContentSetSHA256=baseline['canonicalLfContentSetSha256'],
                ReviewedAdditions=manifest['additions'],EnglishReviewedAdditions=english['additions'],EnglishReviewedOverrides=english['overrides'],
                ProtectedUnchanged=english['protected'],EnglishCheckpointManifestSHA256=english['checkpointManifestSha256'],
                DefaultCoreCount=DEFAULT_CORE_COUNT,DiagnosticCoreCount=DIAGNOSTIC_CORE_COUNT,
                Canonicalization=manifest['canonicalization'],RawPdbProof=False)

def verify(root):
    raw=Path(__file__).with_name('publication-core-source-admissions.json').read_bytes()
    assert sha(raw)==MANIFEST_SHA256,'Unknown Core admission manifest'
    return _verify_sources(root,json.loads(raw))
