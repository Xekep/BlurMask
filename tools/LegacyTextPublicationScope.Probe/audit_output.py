"""Independent raw AL/WORD120/CHAR40/range replay plus input-owner import checks.

The existing committed oracle reconstructs all source records and IDs from
raw owner rows, refs24 and actual source ranges. It never uses expected text.
"""
import base64,json,struct,subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
capture=Path(sys.argv[1]).resolve()
subprocess.run([sys.executable,str(root/'tools/LegacyMainSelectedInputWordChoices.Probe/audit_output.py'),str(capture)],check=True)
data=json.loads(capture.read_text(encoding='utf-8'))
for sample in data['samples']:
    # This actual full selected C22 contains one complete raw-document range,
    # not one registry/input range per physical stroke.
    counts=sample['counts']
    first=((255 if counts[0]==1 else 0)<<8)|3
    last=((len(counts)-1)<<32)|((counts[-1]-1)<<16)|(255<<8)|5
    expected=struct.pack('<QQ',first,last)
    assert base64.b64decode(sample['importedRanges'])==expected
    assert base64.b64decode(sample['inputRanges'])==expected
    assert sample['importedFlags']==1 and sample['retainedRootKind']==4
    assert all(sample[k] for k in ['zeroIdentifierBeforePublication','sameResultSeedStrings',
        'retainedAfterContextDispose','downstreamSameSeed'])
print('PASS independent3 pre-NN imported C22/raw range vectors, flags, actual shared WORD/CHAR publication and downstream seed transport; no SDK acceptance.')
