"""Pinned read-only sampler eligibility and graph-arc removal caller audit."""
import argparse
import hashlib
import json
import pathlib
import struct
import sys

parser = argparse.ArgumentParser()
parser.add_argument('--output', required=True)
args = parser.parse_args()
root = pathlib.Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root / 'research/myscript/scripts'))
from inspect_text_features import Binary, functions

binary = Binary()
assert hashlib.sha256(binary.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
f = functions()
assert binary.qwords(0x1802e1490 + 0x68, 1)[0] == 0x180194140
predicate = f['180194140']
assert 'if (*(int *)(param_3 + 0x14) == 0)' in predicate
assert 'iVar1 = FUN_180193860();' in predicate
assert 'return *(int *)(param_2 + 0x11c) < iVar1;' in predicate
assert '0x118' not in predicate
caller = f['1801a61c0']
assert 'FUN_18019c280(param_1,piVar24,*(undefined1 *)(param_2 + 8));' in caller
assert 'if (0 < iVar3 + -1)' in caller
assert 'FUN_1801a5b00(param_1,param_2,piVar24,uVar26,0)' in caller
assert 'while ((int)uVar17 < iVar3 + -1);' in caller
pruning = f['1801a5b00']
assert 'uVar3 = uVar3 - 1;' in pruning and '((short)uVar3 < 0)' in pruning
assert 'if (param_5 != \'\\0\') goto code_r0x0001801a5c1f;' in pruning
assert '+ 0x68))' in pruning
assert "if (cVar7 == '\\0') goto joined_r0x0001801a5be6;" in pruning
assert '*puVar6 = *puVar6 - 1;' in pruning
assert 'FUN_18019d270' in pruning and 'FUN_18025f210' in pruning
reader = f['180196500']
assert 'param_2 + 0x118,4,1' in reader and 'param_2 + 0x11c,4,1' in reader
resource = (root / 'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
actual = []
for sampler, graph in [(11506,12727),(188157,189379)]:
    end = sampler + 8 + struct.unpack_from('<I',resource,sampler+4)[0]
    assert end == graph
    fields = end - 214
    cap,threshold = struct.unpack_from('<ii',resource,fields+198)
    assert (cap,threshold) == (0x4000000,20480)
    actual.append(dict(sampler=sampler,graph=graph,span=resource[graph+16],later118Cap=cap,raw11CThreshold=threshold))
report = dict(sourceSha256=hashlib.sha256(binary.data).hexdigest(),
              predicate='sampler297.V68=194140; span0 keeps arc, raw193860 >11C removes it, independent of195520/118 cap',
              caller='1A61C0 visits nodes0..N-2,1A5B00 visits descending current ordinals; ordinaryparam5=0 bypasses token-history checks',
              unlink='19D270 removes reciprocal links, higher ordinals decrement, full32-byte arc array shifts; group identity survives',
              actual=actual,scope='Ordinary same-source groups only; seed flags, special tokens, cross-source joins and full word scheduling remain separate')
target=pathlib.Path(args.output)
target.parent.mkdir(parents=True,exist_ok=True)
target.write_text(json.dumps(report,separators=(',',':'))+'\n')
print('PASS pinned ordinary sampler68/raw11C predicate, descending graph unlink caller and first7/main8 resource caps')
