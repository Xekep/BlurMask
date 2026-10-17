"""Static configured207CB0 publication/writeback; vendor image is read only."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x180209f55,0x18020abb1
digest='4bb1700204d7c10786ba9aa39ce20f536f4e93374118c2f8224b4e5923eda57d'
assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==digest
pins=[
    (0x180209f84,'410fb78e04240000'), # movzx ecx, word ptr [r14 + 0x2404]
    (0x180209f90,'0f861b0c0000'), # jbe 0x18020abb1
    (0x180209fc0,'498b8610240000'), # mov rax, qword ptr [r14 + 0x2410]
    (0x180209fcf,'440fb664ca1b'), # movzx r12d, byte ptr [rdx + rcx*8 + 0x1b]
    (0x180209fd8,'0f89b00b0000'), # jns 0x18020ab8e
    (0x180209fde,'0fbe44ca1a'), # movsx eax, byte ptr [rdx + rcx*8 + 0x1a]
    (0x18020a149,'4d8b9610240000'), # mov r10, qword ptr [r14 + 0x2410]
    (0x18020a25c,'e82f2fffff'), # call 0x1801fd190
    (0x18020a3d4,'410fb74020'), # movzx eax, word ptr [r8 + 0x20]
    (0x18020a3de,'41b206'), # mov r10b, 6
    (0x18020a3e1,'663bc1'), # cmp ax, cx
    (0x18020a3ee,'760a'), # jbe 0x18020a3fa
    (0x18020a3f0,'66413b86de240000'), # cmp ax, word ptr [r14 + 0x24de]
    (0x18020a3fa,'41b205'), # mov r10b, 5
    (0x18020a5b7,'f645e504'), # test byte ptr [rbp - 0x1b], 4
    (0x18020a5c5,'80b89a00000000'), # cmp byte ptr [rax + 0x9a], 0
    (0x18020a5ce,'4180be4025000000'), # cmp byte ptr [r14 + 0x2540], 0
    (0x18020a5dc,'4180be0626000000'), # cmp byte ptr [r14 + 0x2606], 0
    (0x18020a63e,'0c03'), # or al, 3
    (0x18020a790,'448b7db4'), # mov r15d, dword ptr [rbp - 0x4c]
    (0x18020a936,'498b8610240000'), # mov rax, qword ptr [r14 + 0x2410]
    (0x18020a941,'6644894cd11c'), # mov word ptr [rcx + rdx*8 + 0x1c], r9w
    (0x18020a985,'470fb64cd91f'), # movzx r9d, byte ptr [r9 + r11*8 + 0x1f]
    (0x18020a98f,'4180e11f'), # and r9b, 0x1f
    (0x18020a9c3,'442ac8'), # sub r9b, al
    (0x18020a9f0,'e8ebcbffff'), # call 0x1802075e0
    (0x18020aa9b,'440fb744d11c'), # movzx r8d, word ptr [rcx + rdx*8 + 0x1c]
    (0x18020ab14,'c744243001000000'), # mov dword ptr [rsp + 0x30], 1
    (0x18020ab25,'e806c2ffff'), # call 0x180206d30
    (0x18020ab82,'66448944d11c'), # mov word ptr [rcx + rdx*8 + 0x1c], r8w
    (0x18000c8cb,'48898798000000'), # mov qword ptr [rdi + 0x98], rax
    (0x18000c8d9,'488987a4000000'), # mov qword ptr [rdi + 0xa4], rax
    (0x18020c6c4,'0fb688aa000000'), # movzx ecx, byte ptr [rax + 0xaa]
    (0x18020c6cb,'888b40250000'), # mov byte ptr [rbx + 0x2540], cl
 ]
for address,raw in pins:
 assert b.data[b.off(address):b.off(address)+len(raw)//2].hex()==raw,hex(address)
report=dict(start=hex(a),end=hex(z),sha256=digest,pins=len(pins),noVendorExecution=True,
 covered='known configured2 ordinary CASE2410 append/index writeback and distinct numeric published selection',
 excludes=['anchored20/22 publication','auxiliary references','configured subtype5/6 backtracking','accepted alternative scheduling','WORD callbacks'])
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS configured207CB0 region/{len(pins)} pins; secondary beforebeam, rawcontexts, signed spans, types5/6,2075E0/206D30 and2410.1C.')
