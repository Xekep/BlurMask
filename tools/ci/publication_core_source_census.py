"""Strict reviewed Core source admission; canonical LF census, not raw PDB proof."""
from pathlib import Path
import hashlib,json
MANIFEST_SHA256='5a0e339be50c0d247879ecfac47683fc6285381c7fba697ed458957c7d78827c'
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
    # Keep the original checkpoint rows intact: they describe the reviewed zip,
    # not later source bodies. Authenticate each later body through its exact
    # checkpoint preimage; never replace historical metadata with new hashes.
    revision=english['ownedFrameRevision']
    assert revision['id']=='owned-last-frame26-v3'
    assert revision['checkpointManifestSha256']==english['checkpointManifestSha256']
    revised=_rows(revision['sources'],5)
    assert set(revised)=={
        'LegacyEnglishCaseGraphPeriodicCompact.cs',
        'LegacyEnglishCaseGraphPeriodicContextBudget.cs',
        'LegacyPreparedCompactWordFrameProducer.cs',
        'LegacyPreparedCompactWordSequence.cs',
        'LegacyPreparedMainCompactPublication.EnglishPeriodic.cs'}
    # The ordinary source-unit correction is separately authenticated against
    # its checkpoint body; never rewrite the checkpoint or Frame26 metadata.
    factor_revision=english['ordinarySourceFactorRevision']
    assert factor_revision['id']=='ordinary-source-factor200-v1'
    assert factor_revision['checkpointManifestSha256']==english['checkpointManifestSha256']
    factor_rows=_rows(factor_revision['sources'],1)
    assert set(factor_rows)=={'LegacyOrdinarySourceInk.cs'},'Unreviewed ordinary factor revision scope'
    assert not(set(factor_rows)&set(revised)),'Overlapping source revisions'
    revised=revised|factor_rows
    checkpoint=english_additions|overrides
    for name,row in revised.items():
        assert name not in protected and name not in additions,'Unreviewed revision scope'
        if name in checkpoint:
            assert row['preimageBasis']=='english951-checkpoint'
            assert row['preimageCanonicalLfSha256']==checkpoint[name]['canonicalLfSha256'],'Wrong revision checkpoint preimage'
            assert row['preimageCanonicalLfBytes']==checkpoint[name]['canonicalLfBytes'],'Wrong revision checkpoint size'
        else:
            assert row['preimageBasis']=='baseline935-aggregate'
        assert row['canonicalLfSha256']!=row['preimageCanonicalLfSha256'],'Empty source revision'
    core=Path(root)/'src/InkRecognition'
    files={p.relative_to(core).as_posix():p for p in core.rglob('*')
           if p.is_file() and p.suffix.lower()=='.cs' and not {'obj','bin'}&set(p.relative_to(core).parts)}
    canonical=lambda p:p.read_bytes().replace(b'\r\n',b'\n')
    hashes={}
    def digest(name):
        if name not in hashes:hashes[name]=sha(canonical(files[name]))
        return hashes[name]
    for group in (additions,english_additions,overrides,protected,revised):
        for name,row in group.items():
            row=revised.get(name,row)
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
    restored.update({name:row['preimageCanonicalLfSha256']for name,row in revised.items()if name not in checkpoint})
    assert set(restored)<=set(names)
    content=b''.join(name.encode('utf-8')+b'\0'+(restored[name]if name in restored else digest(name)).encode('ascii')+b'\n'for name in names)
    assert sha(content)==baseline['canonicalLfContentSetSha256'],'Tampered baseline Core content/preimage admission'
    assert len(files)==DEFAULT_CORE_COUNT,'Unexpected total Core source count'
    return dict(Scope=manifest['scope'],BaselineCommit=baseline['commit'],BaselineCount=935,
                BaselineFilenameSetSHA256=baseline['filenameSetSha256'],BaselineCanonicalLfContentSetSHA256=baseline['canonicalLfContentSetSha256'],
                ReviewedAdditions=manifest['additions'],EnglishReviewedAdditions=english['additions'],EnglishReviewedOverrides=english['overrides'],
                ProtectedUnchanged=english['protected'],EnglishCheckpointManifestSHA256=english['checkpointManifestSha256'],
                EnglishOwnedFrameRevision=revision,
                OrdinarySourceFactorRevision=factor_revision,
                DefaultCoreCount=DEFAULT_CORE_COUNT,DiagnosticCoreCount=DIAGNOSTIC_CORE_COUNT,
                Canonicalization=manifest['canonicalization'],RawPdbProof=False)

def verify(root):
    raw=Path(__file__).with_name('publication-core-source-admissions.json').read_bytes()
    assert sha(raw)==MANIFEST_SHA256,'Unknown Core admission manifest'
    return _verify_sources(root,json.loads(raw))
