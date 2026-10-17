"""Read-only main05F200 context, BE2 export and separate C20/DOM apply."""
import argparse, hashlib, json, sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

root = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root / 'research/myscript/scripts'))
from inspect_text_features import Binary

p = argparse.ArgumentParser(description=__doc__)
p.add_argument('output', type=Path)
a = p.parse_args()
names = ['MyScriptText.dll', 'MyScriptEngine.dll', 'MyScriptInk.dll', 'MyScriptDocument.dll']
hashes = ['18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f',
          'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c',
          'b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a',
          '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f']
bs = [Binary(n) for n in names]
for b, sha in zip(bs, hashes):
    assert hashlib.sha256(b.data).hexdigest() == sha
t, e, ink, doc = bs
md = Cs(CS_ARCH_X86, CS_MODE_64)
regions = []
count = 0
bindings = 0

def region(b, first, end, expected=(), addressed=()):
    global count
    data = b.data[b.off(first):b.off(end)]
    rows = [(x.address, x.mnemonic, x.op_str) for x in md.disasm(data, first)]
    pairs = [(m, o) for _, m, o in rows]
    for pair in expected:
        assert pair in pairs, (b.name, hex(first), pair)
    for item in addressed:
        assert item in rows, (b.name, item)
    count += len(expected) + len(addressed)
    regions.append(dict(dll=b.name, first=hex(first), end=hex(end), sha256=hashlib.sha256(data).hexdigest()))

def binding(b, table, slot, target):
    global bindings
    assert b.qwords(table + slot, 1) == (target,)
    bindings += 1

binding(t, 0x180275550, 0x18, 0x1800283b0)
binding(t, 0x180275550, 0x30, 0x180031880)
for slot, target in [(0x20, 0x1800e19e0), (0x38, 0x1800e1b00),
                     (0x50, 0x1800e35c0), (0x60, 0x1800e2090), (0x70, 0x1800e1ea0)]:
    binding(e, 0x1802265e0, slot, target)
binding(e, 0x180227080, 0x88, 0x1800ebe40)
binding(e, 0x180227080, 0x218, 0x1800efb10)
binding(ink, 0x1800e5050, 0x188, 0x18006ed30)
binding(doc, 0x1802095b0, 0x18, 0x18011b860)
#The existing standard allocator closure is shared with tensor lifecycle.
#Only this default context establishes zero allocation; custom callbacks do not.
for table, slot, target in [(0x180252030, 0, 0x1801db660), (0x1801db660, 0x20, 0x1801db5a0),
                             (0x1801db5a0, 0x18, 0x180037410), (0x180252000, 8, 0x1801ba950)]:
    binding(e, table, slot, target)
allocator_regions = [
    (0x180033c70, 0x180033d40, 'bcb953a559f9ada2bb2477139ce092b558acfa840ae19eee8201406d3a7efacb'),
    (0x180034010, 0x180034200, '9235bad7b4f743cc314589e40d41737222cd2e2e8d8af3f3a6f7e2f72d5ef32c'),
    (0x180037410, 0x1800374d0, '97b20cd952a24ae4714548e48041b3ec38d7a2787a01d355f37fdbcb0b96fbea'),
    (0x1801ba950, 0x1801ba955, '6293aa158a12190b181ae80bc02df18fd528804febcee5c257fd495783b897ef'),
    (0x1801c71c0, 0x1801c7240, '775f7588a02e86fc14bc685982e01985aec3bf5e604a6581284f5ae55be382f0')]
for first, end, sha in allocator_regions:
    data=e.data[e.off(first):e.off(end)]
    assert hashlib.sha256(data).hexdigest()==sha
    region(e, first, end)
assert e.qwords(0x180252000,1)==(1,)
import pefile
heap_iat=next(i.address for entry in pefile.PE(data=e.data).DIRECTORY_ENTRY_IMPORT
              for i in entry.imports if i.name==b'HeapAlloc')
assert 0x1801c721b+0x1409d==heap_iat
region(e,0x1801c7210,0x1801c721b,[('mov','edx, 8')])

#Main caller, not Document053C60's separate source.V20 caller-UTF8 path.
#RBP-31 is the context, so -21=writer10, -19=batch18, -11=result20.
region(t, 0x18005f200, 0x18005f720, addressed=[
    (0x18005f283, 'mov', 'byte ptr [rbp + 0x1f], 0'),
    (0x18005f28c, 'mov', 'rax, qword ptr [r14 + 0x10]'),
    (0x18005f290, 'mov', 'qword ptr [rbp + 0x27], rax'),
    (0x18005f2d0, 'mov', 'r9d, 0x10'),
    (0x18005f353, 'mov', 'qword ptr [rbp - 0x21], rax'),
    (0x18005f37f, 'mov', 'qword ptr [rbp - 0x19], rax'),
    (0x18005f3b2, 'mov', 'r9, qword ptr [rax + 0x20]'),
    (0x18005f3da, 'mov', 'qword ptr [rbp - 0x31], rax'),
    (0x18005f3f7, 'mov', 'qword ptr [rbp - 0x29], r12'),
    (0x18005f47c, 'lea', 'rax, [rbp - 0x31]'),
    (0x18005f480, 'mov', 'qword ptr [rsp + 0x30], rax'),
    (0x18005f48c, 'mov', 'r10, qword ptr [rcx + 0x18]'),
    (0x18005f4a1, 'call', 'r10'),
    (0x18005f4dd, 'mov', 'rdx, qword ptr [r15 + 0x20]'),
    (0x18005f4f9, 'mov', 'rax, qword ptr [rbp - 0x11]'),
    (0x18005f50f, 'call', 'r10'),
    (0x18005f521, 'mov', 'qword ptr [rax], rsi'),
    (0x18005f524, 'mov', 'qword ptr [rbp - 0x11], rsi')])

region(t, 0x18005f050, 0x18005f130,
       [('mov', 'edx, 0xc4d')])
region(t, 0x18005f130, 0x18005f200,
       [('mov', 'edx, 0xbe2')])
region(t, 0x18005f720, 0x18005f950, addressed=[
    (0x18005f858, 'call', '0x18005f200'),
    (0x18005f867, 'cmp', 'dword ptr [rax + 0x40], 1'),
    (0x18005f887, 'mov', 'r10, qword ptr [rax + 0x188]'),
    (0x18005f88e, 'call', 'r10'),
    (0x18005f8ab, 'mov', 'r8, qword ptr [r8 + 0x18]'),
    (0x18005f8af, 'mov', 'r10, qword ptr [rax + 0x218]'),
    (0x18005f8b6, 'call', 'r10')])

#Null source-prior still scans the concrete parent's existing textResult.
region(t, 0x180061510, 0x180061680,
       [('mov', 'r8, qword ptr [r13 + 0x10]'), ('mov', 'r9, qword ptr [rax + 0x30]'),
        ('mov', 'r10, qword ptr [rax + 0x70]'), ('mov', 'r10, qword ptr [rax + 0x60]'),
        ('mov', 'rbx, qword ptr [r8 + 8]'), ('mov', 'rbx, qword ptr [rbx + 0x20]'),
        ('mov', 'rdx, qword ptr [rbx + 0x30]'), ('cmovne', 'rcx, rdi')])
region(t, 0x1800658e0, 0x180065d00,
       [('call', '0x180061510'), ('mov', 'r10, qword ptr [rax + 0x50]'),
        ('mov', 'qword ptr [r14 + 0x20], rax'), ('mov', 'qword ptr [r14 + 0x18], r13'),
        ('je', '0x180065c7a'), ('cmp', 'dword ptr [r8 + 0xd8], r13d')])

#V60 queues opcode2 and retains/clones the actual subtree into batch writer.
region(e, 0x1800e2090, 0x1800e2180,
       [('mov', 'r8d, 2'), ('call', '0x1800e4440'),
        ('mov', 'qword ptr [rax + 0x18], rbp'),
        ('mov', 'dword ptr [rbx + 0x28], eax'),
        ('mov', 'r10, qword ptr [rax + 0x88]')])
region(e, 0x1800e4440, 0x1800e4530,
       [('call', '0x1800e4d80'), ('mov', 'byte ptr [rax], dil')])
region(e, 0x1800e35c0, 0x1800e3a70, addressed=[
    (0x1800e35ec, 'mov', 'rdi, r8'),
    (0x1800e3625, 'mov', 'rdx, rdi'),
    (0x1800e3630, 'mov', 'r8, qword ptr [rcx + 0x20]'),
    (0x1800e3637, 'call', 'r8'),
    (0x1800e3674, 'mov', 'dword ptr [r14 + 8], ecx'),
    (0x1800e3678, 'mov', 'edx, 0x28'),
    (0x1800e376f, 'call', 'r10')])
region(doc, 0x18011b860, 0x18011b950,
       [('mov', 'r8d, 0x28')])
region(ink, 0x18006ed30, 0x18006ede4,
       [('mov', 'edx, 0xc4d')])
region(e, 0x1800efb10, 0x1800efb80,
       [('mov', 'edx, 0xbe2'), ('mov', 'r10, qword ptr [rax + 0x10]')])

a.output.write_bytes((json.dumps(dict(regions=regions, bindings=bindings, pins=count,
    vendorExecution=False, scope='Read-only concrete MAIN context/BE2 transport; actual DOM parent and native C20 acceptance remain unported'), indent=2) + '\n').encode())
print(f'PASS{len(regions)} regions/{bindings} bindings/{count} pins; no SDK parent or accepted-document claim.')
