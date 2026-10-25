"""Exact authenticated source restoration only, never a runtime/data owner issuer.
Canonical3a and historical D0 source views are distinct scopes. Mixed successor
adds one private int(+4 logical field bytes); restoration is SOURCE-only, not
current runtime/numeric/default-budget equivalence.
"""
import hashlib,json
from pathlib import Path
CONTRACTS_SHA256="6d7d63e144a0abfc9348a11d4abdc5d7ae79cc13969c2606e253c0a8e94a1847"
raw=Path(__file__).with_name('source_contracts.json').read_bytes().replace(b'\r\n',b'\n')
assert hashlib.sha256(raw).hexdigest()==CONTRACTS_SHA256
TABLE=json.loads(raw)
def restore(name,data,expected):
    assert type(name) is str and type(data) is bytes and type(expected) is str
    assert expected in TABLE['allowedExpected'].get(name,()),('Unreviewed expected source',name,expected)
    data=data.replace(b'\r\n',b'\n')
    for unused in range(4):
        digest=hashlib.sha256(data).hexdigest()
        if digest==expected:return data
        row=TABLE['steps'].get(name+':'+digest)
        assert row is not None and row['name']==name and row['source']==digest,('Unsupported complete source',name,digest,expected)
        for start,end,after,before in reversed(row['edits']):
            assert type(start) is int and type(end) is int and 0<=start<=end<=len(data)
            assert data[start:end]==bytes.fromhex(after)
            data=data[:start]+bytes.fromhex(before)+data[end:]
        assert hashlib.sha256(data).hexdigest()==row['target']
    raise AssertionError(('Unproved historical source',name,expected))
