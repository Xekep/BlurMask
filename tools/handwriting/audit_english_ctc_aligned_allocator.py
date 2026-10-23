"""Conditional service49 aligned-allocation contract, static only.

Assumes the real registry entry identifies the pinned descriptor, ROOT/default
zero allocator, and a fresh successful allocation. Registration/caller origin
and constructor completion remain unresolved. No SDK instructions execute.
"""
from pathlib import Path
import argparse, hashlib, json, runpy, sys

p = argparse.ArgumentParser(description=__doc__)
p.add_argument('--sdk', type=Path, required=True)
p.add_argument('--output', type=Path, required=True)
a = p.parse_args()
tool = Path(__file__).with_name('audit_english_ctc_aligned_service_lookup.py')
old = sys.argv
sys.argv = [str(tool), '--sdk', str(a.sdk), '--output', str(a.output.with_name(a.output.stem+'-lookup.json'))]
try: up = runpy.run_path(str(tool), run_name='__main__')
finally: sys.argv = old
read, qword, md = (up[k] for k in ('read', 'qword', 'md'))
assert read(0x180229ca0, 16).hex() == '010000000000000049000000ffffffff'
assert qword(0x180229cc0) == 0x180229c00
assert qword(0x180229c08) == 0x180104cc0 and qword(0x180229c10) == 0x180104d40
code = read(0x180104cc0, 122)
assert hashlib.sha256(code).hexdigest() == '3dfc075d63871a887d3753d56afa1363cdca666f62c2366e1de4a6827699095f'
ii = list(md.disasm(code, 0x180104cc0)); assert len(ii) == 41 and sum(i.size for i in ii) == 122
# Match the entire allocator, including rejection/failure returns, not only its
# alignment arithmetic. The indirect call goes to ROOT slot18 (zero allocator).
expected = [
 ('push','rbx'),('sub','rsp, 0x20'),('mov','rbx, r8'),('test','rdx, rdx'),('je','0x180104d32'),
 ('test','rbx, rbx'),('je','0x180104d32'),('lea','rax, [r8 - 1]'),('test','rbx, rax'),('jne','0x180104d32'),
 ('mov','eax, 8'),('mov','r8d, 1'),('cmp','rbx, rax'),('cmovb','rbx, rax'),('add','rdx, rax'),
 ('mov','rax, qword ptr [rcx + 8]'),('add','rdx, rbx'),('and','rax, 0xfffffffffffffffe'),
 ('mov','rax, qword ptr [rax]'),('mov','r9, qword ptr [rax + 0x18]'),('call','r9'),
 ('mov','r8, rax'),('test','rax, rax'),('je','0x180104d32'),('lea','rcx, [rax + 8]'),
 ('xor','edx, edx'),('mov','rax, rcx'),('div','rbx'),('test','rdx, rdx'),('je','0x180104d25'),
 ('sub','rbx, rdx'),('add','rcx, rbx'),('mov','qword ptr [rcx - 8], r8'),('mov','rax, rcx'),
 ('add','rsp, 0x20'),('pop','rbx'),('ret',''),('xor','eax, eax'),('add','rsp, 0x20'),('pop','rbx'),('ret','')]
assert [(i.mnemonic,i.op_str) for i in ii] == expected

# The input to slot18 is a positive element size and count1; these four sizes fit
# the already audited first-success calloc arithmetic without overflow. Pointer
# residues are exhaustive even beyond Windows HeapAlloc's usual alignment.
cases = []; digest = hashlib.sha256()
for payload in (43136, 64704):
 for alignment in (32, 64):
  total = payload+8+alignment
  assert total <= (2**64-32) and total > 0
  for residue in range(alignment):
   raw_pointer = 0x100000+residue
   start = raw_pointer+8
   remainder = start % alignment
   if remainder: start += alignment-remainder
   independent = ((raw_pointer+8+alignment-1)//alignment)*alignment
   assert start == independent and start % alignment == 0
   assert raw_pointer <= start-8 and start+payload <= raw_pointer+total
   assert start-8+8 <= start
   # There is exactly one metadata write, strictly before the workspace.
   block = bytearray(total)
   header = start-8-raw_pointer
   block[header:header+8] = raw_pointer.to_bytes(8,'little')
   work = start-raw_pointer
   assert not any(block[work:work+payload])
   assert int.from_bytes(block[header:header+8],'little') == raw_pointer
   digest.update(start.to_bytes(8,'little')); digest.update(bytes(block[header:header+8]))
  cases.append(dict(workspaceBytes=payload,alignmentBytes=alignment,rootZeroAllocationBytes=total,
                    rootElementCount=1,rootElementBytes=total,
                    pointerResidues=alignment,workspaceRemainsZero=True))
report = dict(status='PASS conditional aligned allocator body contract',
              nativeAddress='0x180104cc0',nativeBytes=122,nativeSha256=hashlib.sha256(code).hexdigest(),
              cases=cases,pointerResidueControls=192,controlDigest=digest.hexdigest(),
              condition='Actual service49 registry descriptor229CA0; initialized unmodified ROOT/default slot18; fresh disjoint successful calloc; only listed positive requests',
              zeroInitializationReliesOnExternalHeapAllocContract=True,
              actualService49RegistrationProven=False,actualCtcEnvironmentOriginProven=False,
              arbitrarySizeOverflowOrFailureRecoveryProven=False,freeNonNULLProven=False,
              completeConstructorProven=False,runtimePredicateApproved=False,vendorExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS conditional service49 body /192 pointer residues; registration and full constructor unresolved')
