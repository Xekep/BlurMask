"""Read-only native source70 metadata path plus same retained-result lease seam."""
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions = [
    (0x180236d20, 0x180236d90, '8765b2f69271309f364a6be9940c47d9be243d5653fe88b802218512ef47e31d'),
    (0x180234970, 0x180234a70, 'a0b63bdfb92250715801e7e1af28f209515c70e9a0d65e4ba2e7207034875e6c'),
]
for first, last, expected in regions:
    assert hashlib.sha256(b.data[b.off(first):b.off(last)]).hexdigest() == expected
pins = [
    (0x180236d2a, '498bc1'),       #R9 fulltag survives selected-source lookup.
    (0x180236d30, '48c1e820'), (0x180236d39, '0fb6d8'),
    (0x180236d56, '4c8b5070'),
    (0x180236d5a, '0fb6442478'), (0x180236d5f, '88442428'),
    (0x180236d63, '0fb7442470'), (0x180236d68, '6689442420'),
    (0x180236d6d, '41ffd2'), (0x180236d70, '0f1000'),
    (0x180236d76, '0f1107'), (0x180236d79, '891f'),
    (0x18023497a, '4c894c2420'), (0x180234984, '0fb77c2470'),
    (0x18023499b, '4c8b5070'), (0x1802349ad, '41ffd2'),
    (0x1802349b7, '0f1000'), (0x1802349ba, '0f1103'),
    (0x1802349cd, '6683c71d'), (0x1802349d1, '80be3090000000'),
    (0x1802349db, '0fb61401'), (0x1802349e1, 'c7430800000000'),
    (0x180094051, '0f57c0'), (0x180094059, '0f1101'),
]
for address, value in pins:
    assert b.data[b.off(address):b.off(address) + len(value) // 2].hex() == value, hex(address)
session = (ROOT / 'src/InkRecognition/LegacyPreparedCaseDictionarySession.cs').read_text()
old = session.split('private ReadOnlyMemory<byte> ReadTerminalCore(', 1)[1].split('private static void RequireSelectedSource', 1)[0]
assert old.index('RequireInstalledMap(expectedMap)') < old.index('ReadTerminalAction(')
new = session.split('internal ReadOnlyMemory<byte> ReadResultTerminal(', 1)[1].split('internal LegacyAggregateDictionaryQueryPage ReadResultQuery(', 1)[0]
assert new.index('Enter(token)') < new.index('RequireResultQueryLease(lease)') < new.index('ReadTerminalAction(') < new.index('finally{Exit();}')
for forbidden in ['queryRevision=', 'Reset', 'InstallMap', 'RequireInstalledMap']:
    assert forbidden not in new
global_owner = (ROOT / 'src/InkRecognition/LegacyFreshMainGlobalResultQueries.cs').read_text()
forward = global_owner.split('internal ReadOnlyMemory<byte> ReadResultTerminal(', 1)[1].split('internal LegacyAggregateDictionaryQueryPage ReadResultQuery(', 1)[0]
assert forward.index('Enter(token)') < forward.index('CheckResultQueryLease(lease,token)') < forward.index('dictionary.ReadResultTerminal(lease.DictionaryLease')
pass_owner = (ROOT / 'src/InkRecognition/LegacyFreshMainGlobalWordPass.cs').read_text()
assert '=>Policy.Owner.ReadResultTerminal(resultQueries,tag,endingSymbol,boundaryArgument,token);' in pass_owner
output = Path(sys.argv[1]); output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps(dict(pePins=len(pins),
    regions=[dict(start=hex(a), endExclusive=hex(z), sha256=hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()) for a,z,_ in regions],
    action='same existing236D20/234970 metadata action, fulltag+rawsymbol+boundary; selectedsource stamp1',
    lease='same captured finalV80 result lease checked inside both source/session gates, no D0/reset/revision mutation',
    coverage='prepared source70 consumer on actual global compact terminal tags; native marker36 events zero',
    vendorExecution=False), indent=2) + '\n')
print(f'PASS {len(pins)} PE metadata pins + retained-result lease/old-entry call-order seams; vendor read-only')
