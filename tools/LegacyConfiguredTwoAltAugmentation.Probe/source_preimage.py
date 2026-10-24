"""Exact authenticated source restoration only, never a runtime/data owner issuer.
Current3a default restoration and historical D0 static source are distinct scopes.
"""
import hashlib,json
from pathlib import Path
CONTRACTS_SHA256="42cea79e8c06d2aab6d2f4803425861fcd44d4750c3627b4af1f11b969908627"
raw=Path(__file__).with_name('source_contracts.json').read_bytes().replace(b'\r\n',b'\n')
assert hashlib.sha256(raw).hexdigest()==CONTRACTS_SHA256
TABLE=json.loads(raw)
def restore(name,data,expected):
    assert type(name) is str and type(data) is bytes and type(expected) is str
    assert expected in TABLE['allowedExpected'].get(name,()),('Unreviewed expected source',name,expected)
    data=data.replace(b'\r\n',b'\n')
    for unused in range(3):
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
