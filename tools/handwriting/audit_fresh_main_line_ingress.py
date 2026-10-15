"""Read-only pinned PE/decompilation owner audit; never executes vendor code."""
import argparse
import hashlib
import json
import struct
import sys
from pathlib import Path
import capstone

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary, functions

p = argparse.ArgumentParser()
p.add_argument('--output', required=True)
args = p.parse_args()
b = Binary()
fs = functions()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
checks = {}

def terms(address, name, *required):
    text = fs[address]
    assert all(x in text for x in required), (address, name, required)
    checks[name] = {'address': address, 'terms': required}

def pointer(address, expected, name):
    assert b.qwords(address, 1)[0] == expected, name
    checks[name] = [hex(address), hex(expected)]

def instructions(address, expected, name):
    md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
    actual = []
    for ins in md.disasm(b.data[b.off(address):b.off(address)+64], address):
        actual.append(ins.mnemonic + ' ' + ins.op_str)
        if ins.mnemonic == 'ret':
            break
    assert actual == expected, (name, actual)
    checks[name] = {'address': hex(address), 'instructions': actual}

# 150D00 ordinary config contains source219 +60 followed by sampler21B.
# 1B0760 consumes config[4]/[5] and passes the sampler to227, not facet218.
terms('180150d00', 'driver-config',
      'local_88 = *(longlong **)(param_2 + 0x60);', 'plStack_80 = local_res18;')
terms('1801ca530', 'context-source-and-sampler',
      '*(longlong *)(param_2 + 0x30) = param_3;',
      '*(longlong **)(param_2 + 0x40) = param_4;',
      '*param_4 + 0x1a8', '*(undefined1 *)(param_2 + 0x48) = 0;')
pointer(0x1802e1ff0 + 0x1a8, 0x1801ad960, 'sampler21B-scope-dispatch')
terms('1801ad960', 'sampler-scope-is-underlying297',
      '**(longlong **)(param_2 + 0x180) + 0xe0', 'return 0;')
terms('1801a6980', 'sampler-underlying297-factory',
      '*param_3 !=', 'param_3[7] !=',
      '**(longlong **)(param_3 + 0x10) + 0x88',
      '*(undefined8 *)(param_2 + 0x180) = local_res8;')
pointer(0x1802e5a20 + 0x88, 0x1801d6320, 'selected247-underlying-getter')
instructions(0x1801d6320, ['mov rax, qword ptr [rdx + 0x130]', 'ret '], 'selected247-source130')
terms('1801d4950', 'source130-reader',
      'FUN_1801d47c0(param_1,uVar5,param_3,param_4)',
      '*(longlong *)(param_2 + 0x130) = lVar4;')
pointer(0x1802e1490 + 0xe0, 0x1801964a0, 'runtime297-scope-getter')
instructions(0x1801964a0, ['movzx eax, byte ptr [rdx + 0x242]', 'ret '], 'runtime297-scope-field')
terms('18018d590', 'runtime297-clone-scope-field',
      '*(undefined1 *)(param_2 + 0x242) = *(undefined1 *)(param_2 + 0x134);')
terms('180196500', 'v106-disabled-scope',
      'if (param_4 < 0x107)', '*(undefined1 *)(param_2 + 0x134) = 0;')

terms('18000c860', 'fresh-no-property-C4C5',
      '*(undefined2 *)(param_2 + 0xc4) = 0;',
      '*(undefined2 *)(param_2 + 8) = 0x10;')
terms('180150d00', 'fresh-no-282-or-2D4-wrapper',
      '(*(byte *)(local_b8 + 1) >> 4 & 0xffff01) << 8',
      '*(undefined1 *)((longlong)local_b8 + 0xc4)',
      'if (local_b8 != (longlong *)0x0)',
      '*plVar11 + 0xa8', '*plVar11 + 0xb0')
pointer(0x1802e5a20 + 0xa8, 0x18006cfa0, 'source-extension-count-getter')
instructions(0x18006cfa0, ['mov eax, dword ptr [rdx + 0x160]', 'ret '], 'source-extension-count160')
terms('1801d4950', 'source-extension-count-reader',
      'param_2 + 0x160,4', 'param_2 + 0x16c,4')

# Context227 runs lazily from the first group classification after193380 has
# called source218.virtual30 to replace all physical point coordinates.
terms('1801be930', 'lazy-main-context-refresh',
      'param_2 + 600', 'param_2 + 0x270', '0xa4', 'FUN_1801b7f70')
terms('180193380', 'sampler-source-point-replacement',
      '0x30', 'param_2 + 0x1c0')
terms('1801c9870', 'actual-context-window-producer',
      'param_2 + 0x38', 'param_2 + 0x48', 'param_2 + 0x40',
      '0x226', '0xffffffff', 'local_ec', 'fVar39 / (float)local_f4')

ak = (ROOT / 'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest() == '7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert (struct.unpack_from('<I', ak, 188165)[0],struct.unpack_from('<H',ak,188171)[0]) == (0x297, 0x106)
assert (struct.unpack_from('<I',ak,189742)[0],struct.unpack_from('<H',ak,189748)[0]) == (0x21d, 0x110)
assert ak[189750:189752] == b'\xff\xff'
assert struct.unpack_from('<3fBBHf4B', ak, 195746) == (0., 1., 1., 0, 3, 28, struct.unpack('<f',struct.pack('<f',.1))[0], 1, 2, 0, 1)
source_tail = 189734 + 8 + struct.unpack_from('<I',ak,189738)[0]
assert struct.unpack_from('<I',ak,source_tail)[0] == 0
assert 8+struct.unpack_from('<I',ak,4)[0]-source_tail == 190
checks['no283-extensions'] = {'countOffset':source_tail,'count':0,'metadataBytes':186}
checks['actual-resource'] = {'sampler':188157,'samplerVersion':106,'acoustic':189734,
                             'contextNetwork':189755,'contextConfiguration':195746}
Path(args.output).write_bytes((json.dumps({'checks':checks,'scope':
    'One assigned ordinary main0 descriptor; open physicals; fresh no-prior source. '
    'Custom scope false is resource106 to clone/runtime proof, not a constructor guess. '
    'Prepared240 selection remains explicit. No OCR winner or live controller claimed.'},indent=2)+'\n').encode())
print(f'PASS {len(checks)} pinned static main sampler/context owner-phase checks; no vendor DLL execution.')
