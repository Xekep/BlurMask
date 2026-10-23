"""Static default-root service49 lookup boundary; never executes SDK code.

Proves dispatch and registry indexing, not registration of an actual service49
descriptor, its allocator callbacks, or the CTC caller's environment origin.
"""
from pathlib import Path
import argparse, hashlib, json, runpy, struct, sys

p = argparse.ArgumentParser(description=__doc__)
p.add_argument('--sdk', type=Path, required=True)
p.add_argument('--output', type=Path, required=True)
a = p.parse_args()
tool = Path(__file__).with_name('audit_english_ctc_default_allocator.py')
old = sys.argv
sys.argv = [str(tool), '--sdk', str(a.sdk), '--output', str(a.output.with_name(a.output.stem+'-root.json'))]
try:
    up = runpy.run_path(str(tool), run_name='__main__')
finally:
    sys.argv = old
read, qword, md = (up[k] for k in ('read', 'qword', 'md'))
decoded = dict(up['decoded'])
regions = []
for start, size, digest, registered in [
    (0x180037f50, 79, '97b624e9b1d63680ef1c7c65f2967bdce69508d58daf7fce93e6387e58b78865', False),
    (0x180193a40, 1561, 'f2318c6a4a049a17da1fa62c80dd2b14c198caf210befff953ab4b11ef6b5003', True),
]:
    code = read(start, size)
    assert hashlib.sha256(code).hexdigest() == digest
    ii = list(md.disasm(code, start)); assert sum(i.size for i in ii) == size
    decoded.update({i.address: i for i in ii})
    regions.append(dict(address=hex(start), bytes=size, sha256=digest, registered=registered, instructions=len(ii)))

def sequence(pc, expected):
    for mnemonic, operand in expected:
        i = decoded[pc]
        assert (i.mnemonic, i.op_str) == (mnemonic, operand), (hex(pc), i.mnemonic, i.op_str)
        pc += i.size
    return pc

# The runtime table is initially zero in the PE: trace its real initialization
# from the immutable core descriptor instead of reading it as preinitialized.
sequence(0x180034039, [('mov', 'rax, qword ptr [rip + 0x21dff8]'),
                     ('mov', 'r14, r9'), ('mov', 'rdx, qword ptr [rax + 0x20]'),
                     ('mov', 'rax, qword ptr [rip + 0x21dfe2]'),
                     ('mov', 'qword ptr [rip + 0x21fb03], rdx')])
assert qword(0x180252038) == 0x1801db8f0
assert qword(0x1801db910) == 0x1801db830
assert qword(0x1801db860) == 0x180037f50
sequence(0x180193c66, [('mov', 'rax, qword ptr [r13 + 8]'),
                     ('lea', 'rbx, [r15*4]'), ('and', 'rax, 0xfffffffffffffffe'),
                     ('mov', 'rdi, r14'), ('mov', 'edx, 0x49'), ('shl', 'rdi, 2'),
                     ('mov', 'rcx, r13'), ('mov', 'rax, qword ptr [rax + 8]'),
                     ('mov', 'r9, qword ptr [rax + 0x30]'), ('call', 'r9')])

# Every instruction in the complete leaf, including both returns. All branch
# targets are local; following INT3 and absence of a PE runtime-function entry
# bound this unregistered leaf explicitly rather than guessing a larger body.
end = sequence(0x180037f50, [
    ('mov', 'r8, qword ptr [rcx + 0x98]'), ('test', 'r8, r8'), ('je', '0x180037f9c'),
    ('cmp', 'edx, 0x3fff'), ('ja', '0x180037f9c'), ('movsxd', 'rdx, edx'),
    ('mov', 'rax, rdx'), ('mov', 'rcx, rdx'), ('sar', 'rax, 8'), ('and', 'edx, 0xf'),
    ('and', 'eax, 0x3f'), ('sar', 'rcx, 4'), ('and', 'ecx, 0xf'),
    ('mov', 'rax, qword ptr [r8 + rax*8]'), ('mov', 'rax, qword ptr [rax + rcx*8]'),
    ('mov', 'rax, qword ptr [rax + rdx*8]'), ('test', 'rax, rax'), ('je', '0x180037f9c'),
    ('mov', 'rax, qword ptr [rax]'), ('test', 'rax, rax'), ('je', '0x180037f9c'),
    ('mov', 'rax, qword ptr [rax + 0x20]'), ('ret', ''), ('xor', 'eax, eax'), ('ret', '')])
assert end == 0x180037f9f and read(end, 1) == b'\xcc'
er, size = struct.unpack_from('<II', up['raw'], up['optional']+112+3*8)
for off in range(0, size, 12):
    begin, finish, _ = struct.unpack('<III', read(up['image']+er+off, 12))
    assert not begin <= 0x37f50 < finish

digest = hashlib.sha256()
for service in range(16384):
    native = ((service >> 8) & 63, (service >> 4) & 15, service & 15)
    first, rest = divmod(service, 256); second, third = divmod(rest, 16)
    assert native == (first, second, third)
    digest.update(bytes(native))
assert ((0x49 >> 8) & 63, (0x49 >> 4) & 15, 0x49 & 15) == (0, 4, 9)
report = dict(status='PASS static default-root aligned-service lookup boundary', regions=regions,
              exhaustiveIndexControls=16384, indexDigest=digest.hexdigest(), service=73,
              registryIndices=[0, 4, 9], leafWrites=0, leafCalls=0,
              condition='Initialized unmodified ROOT/default core table; registry root and required intermediate arrays live; service entry and its descriptor must be present for a nonNULL result',
              actualService49RegistrationProven=False, alignedAllocatorBodiesProven=False,
              actualCtcEnvironmentOriginProven=False, completeConstructorProven=False,
              runtimePredicateApproved=False, vendorExecuted=False)
a.output.write_text(json.dumps(report, indent=2)+'\n', encoding='utf-8')
print('PASS default-root service49 dispatch/16384 registry index controls; registration remains unresolved')
