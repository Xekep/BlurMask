"""Read-only source/preimage authentication; no library/model execution."""
from pathlib import Path
import hashlib
import json

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[3]
identity = json.loads((HERE / 'source-identity.json').read_text())
sha = lambda value: hashlib.sha256(value).hexdigest()
for entry in identity['files']:
    post = HERE / 'source' / entry['path']
    assert sha(post.read_bytes()) == entry['postSHA256'], entry['path']
    if entry['preSHA256']:
        assert sha((REPO / entry['path']).read_bytes()) == entry['preSHA256']

name = 'src/InkRecognition/LegacyFreshMainGlobalWordTextPublication.cs'
before = (REPO / name).read_bytes()
after = (HERE / 'source' / name).read_bytes()
constructor = b'    private LegacyFreshMainGlobalWordTextPublication('
methods = b'    public static LegacyFreshMainGlobalWordTextPublication Prepare('
assert before[:before.index(constructor)] == after[:after.index(constructor)]
assert before[before.index(methods):] == after[after.index(methods):]
uses = b'        foreach(var entry in EnumerateAllReferences(token))'
old_end = b'            for(int i=0;i<owner.Records.Length;i+=40)'
new_end = b'            int matchingCompact=0,sourceReferenceCount=0;'
assert before[before.index(uses):before.index(old_end)] == after[after.index(uses):after.index(new_end)]
assert b'allWords is not null&&batchWords is null' in after
assert sha((HERE / identity['expectedStreamsPath']).read_bytes()) == identity['expectedStreamsSHA256']
original = before.decode()
start = original.index('internal sealed class LegacyGlobalWordTextWorkBudget')
end = original.index('public readonly record struct LegacyGlobalWordTextPrefix', start)
extracted = 'namespace InkRecognition;\n' + original[start:end].replace('\r\n', '\n')
assert (HERE / 'probe/WorkBudget.extracted.cs').read_text() == extracted
assert (HERE / 'probe/Program.cs').read_bytes() == (HERE / 'source/tools/WholeWordPreflightCache.Probe/Program.cs').read_bytes()
print('PASS source identities, unchanged fields/post-constructor/per-use prefix, exact budget extraction, frozen oracle')
