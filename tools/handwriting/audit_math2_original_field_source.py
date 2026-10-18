"""Static Math13B9/C98, original-selected INPUT and C23 pre-NN ownership.
Reads PE bytes only; does not load or execute vendor code.
"""
from pathlib import Path
import hashlib,json,struct,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
rows=[
  {
    "dll": "MyScriptMath.dll",
    "start": "0x180001000",
    "end": "0x1800015f4",
    "sha256": "a2e2d8ae0ccf40839ef5480b8b3e6fc87b4ab6b15daa9368192ca805cb1a56b1",
    "pins": [
      [
        "0x1800015ae",
        "lea",
        "rdx, [rip + 0x9678b]"
      ],
      [
        "0x1800015b8",
        "call",
        "rdi"
      ]
    ]
  },
  {
    "dll": "MyScriptMath.dll",
    "start": "0x180002920",
    "end": "0x1800029a0",
    "sha256": "21df75626891ece9f5061351f5e3a08a3cb5e987aed24a508286adeb5dbf388a",
    "pins": [
      [
        "0x18000292e",
        "lea",
        "r9, [rip + 0x9587b]"
      ],
      [
        "0x180002935",
        "xor",
        "r8d, r8d"
      ],
      [
        "0x180002941",
        "call",
        "r10"
      ]
    ]
  },
  {
    "dll": "MyScriptMath.dll",
    "start": "0x180007570",
    "end": "0x1800076b0",
    "sha256": "6a10dd6ad7298d69d13f543987a6569dcb852ec86581761cacacde201f8ceeff",
    "pins": [
      [
        "0x180007570",
        "lea",
        "rax, [rip + 0x90c39]"
      ],
      [
        "0x180007577",
        "cmp",
        "r8, rax"
      ],
      [
        "0x18000757a",
        "jne",
        "0x1800075a2"
      ],
      [
        "0x18000757c",
        "cmp",
        "r9d, 0xc98"
      ],
      [
        "0x180007585",
        "lea",
        "rax, [rip + 0x906f4]"
      ]
    ]
  },
  {
    "dll": "MyScriptMath.dll",
    "start": "0x1800055b0",
    "end": "0x1800055ba",
    "sha256": "ecdaeede607b87733f7f173d364a74d3db316ad13d358dce7ca0d4ab09340b74",
    "pins": [
      [
        "0x1800055b0",
        "mov",
        "dword ptr [r8], 2"
      ],
      [
        "0x1800055b7",
        "mov",
        "al, 1"
      ]
    ]
  },
  {
    "dll": "MyScriptMath.dll",
    "start": "0x1800054a0",
    "end": "0x1800054b0",
    "sha256": "ce16b61d6e0b038c17636720862490af516be187f39f15ec213d9ae0c736634a",
    "pins": [
      [
        "0x1800054a0",
        "mov",
        "r9, r8"
      ],
      [
        "0x1800054a3",
        "mov",
        "r8, qword ptr [rdx + 0x18]"
      ],
      [
        "0x1800054a7",
        "mov",
        "rdx, qword ptr [rdx + 0x10]"
      ],
      [
        "0x1800054ab",
        "jmp",
        "0x180003070"
      ]
    ]
  },
  {
    "dll": "MyScriptMath.dll",
    "start": "0x180003070",
    "end": "0x180003200",
    "sha256": "c044a5faf5dcdb3119432b9d0d77a7a994e729cc280d2eebdd3909acdb0b1bfe",
    "pins": [
      [
        "0x180003089",
        "lea",
        "r9, [rip + 0x99790]"
      ],
      [
        "0x18000309b",
        "mov",
        "r10, qword ptr [rax + 0x1a0]"
      ],
      [
        "0x1800030aa",
        "mov",
        "qword ptr [r14], rsi"
      ],
      [
        "0x1800030ad",
        "mov",
        "qword ptr [r14 + 8], rsi"
      ],
      [
        "0x1800030b1",
        "mov",
        "qword ptr [r14 + 0x10], rsi"
      ],
      [
        "0x1800030b8",
        "je",
        "0x1800031df"
      ],
      [
        "0x1800031db",
        "mov",
        "qword ptr [r14 + 0x10], rsi"
      ]
    ]
  },
  {
    "dll": "MyScriptDocument.dll",
    "start": "0x18011d310",
    "end": "0x18011d3a8",
    "sha256": "d762686a20960720930b4906f8890caee1f835019cad5eb24a2d7cf2ecefc4c4",
    "pins": [
      [
        "0x18011d314",
        "cmp",
        "edx, 5"
      ],
      [
        "0x18011d323",
        "mov",
        "edx, dword ptr [r8 + rax*4 + 0x11d3a8]"
      ],
      [
        "0x18011d32e",
        "jmp",
        "rdx"
      ],
      [
        "0x18011d348",
        "lea",
        "rax, [rip + 0xefbd1]"
      ]
    ]
  },
  {
    "dll": "MyScriptDocument.dll",
    "start": "0x18011d3c0",
    "end": "0x18011d830",
    "sha256": "b9b57b34797fa193628fa79811777cd7c5cf9099d471c84dcfbc3511d8486f13",
    "pins": [
      [
        "0x18011d3fd",
        "call",
        "0x18011d310"
      ],
      [
        "0x18011d700",
        "mov",
        "edx, 0xbcd"
      ]
    ]
  },
  {
    "dll": "MyScriptDocument.dll",
    "start": "0x180005380",
    "end": "0x1800057f0",
    "sha256": "d68762dd6da7cac80f1cd09c6b9cfe2c5db6b36350781adfa27eeb08a586f99d",
    "pins": [
      [
        "0x180005611",
        "mov",
        "r8d, 0xc98"
      ],
      [
        "0x180005637",
        "mov",
        "r9, qword ptr [rax]"
      ],
      [
        "0x18000563a",
        "call",
        "r9"
      ],
      [
        "0x180005645",
        "mov",
        "rdx, qword ptr [rsp + 0x90]"
      ],
      [
        "0x18000564d",
        "lea",
        "r8, [rip + 0x207250]"
      ],
      [
        "0x18000565d",
        "add",
        "ecx, 0x64"
      ],
      [
        "0x180005663",
        "mov",
        "dword ptr [rsp + 0x48], ecx"
      ],
      [
        "0x180005680",
        "mov",
        "r10, qword ptr [rax + 0xe0]"
      ],
      [
        "0x180005691",
        "call",
        "r10"
      ],
      [
        "0x180005694",
        "cmp",
        "rax, -1"
      ],
      [
        "0x1800056c3",
        "call",
        "r10"
      ]
    ]
  },
  {
    "dll": "MyScriptDocument.dll",
    "start": "0x18000d540",
    "end": "0x18000d760",
    "sha256": "a5714d827caf631376a8687072dc51514a71e4fc6d817b6a0bfac8413569089a",
    "pins": [
      [
        "0x18000d573",
        "call",
        "0x18000a1b0"
      ],
      [
        "0x18000d634",
        "call",
        "0x18000c880"
      ],
      [
        "0x18000d65f",
        "call",
        "0x18000ba60"
      ],
      [
        "0x18000d68f",
        "mov",
        "byte ptr [rsp + 0x30], 1"
      ],
      [
        "0x18000d697",
        "mov",
        "byte ptr [rsp + 0x40], 1"
      ],
      [
        "0x18000d6ac",
        "mov",
        "byte ptr [rsp + 0x28], 1"
      ],
      [
        "0x18000d6b7",
        "mov",
        "r10, qword ptr [rax + 0x170]"
      ],
      [
        "0x18000d6c8",
        "call",
        "r10"
      ],
      [
        "0x18000d6dd",
        "call",
        "0x18000c590"
      ],
      [
        "0x18000d6f6",
        "mov",
        "qword ptr [rbx + 0x60], r14"
      ],
      [
        "0x18000d6fa",
        "mov",
        "qword ptr [rbx + 0x68], rsi"
      ],
      [
        "0x18000d700",
        "mov",
        "qword ptr [rbx + 0x78], r14"
      ],
      [
        "0x18000d704",
        "mov",
        "qword ptr [rbx + 0x80], rsi"
      ]
    ]
  },
  {
    "dll": "MyScriptDocument.dll",
    "start": "0x18000c590",
    "end": "0x18000c880",
    "sha256": "a0e16fc2dea168f81bb7541aad13d14e77075ae27d911ce58294413a8e93baf4",
    "pins": [
      [
        "0x18000c801",
        "mov",
        "r9, qword ptr [rcx + 0x1f8]"
      ],
      [
        "0x18000c80b",
        "call",
        "r9"
      ]
    ]
  },
  {
    "dll": "MyScriptDocument.dll",
    "start": "0x18002fe40",
    "end": "0x18002fef0",
    "sha256": "b43797274ad8860d2b0cdb5b92429448c4f637d7748a09af5f268d4a00748ff2",
    "pins": [
      [
        "0x18002fe55",
        "lea",
        "r9, [rip + 0x1dc978]"
      ],
      [
        "0x18002fe67",
        "mov",
        "r10, qword ptr [rax + 0x1a0]"
      ],
      [
        "0x18002fe6e",
        "call",
        "r10"
      ],
      [
        "0x18002fe74",
        "je",
        "0x18002fed5"
      ],
      [
        "0x18002fe79",
        "lea",
        "r9, [rip + 0x1dc95c]"
      ],
      [
        "0x18002fe9d",
        "mov",
        "r10, qword ptr [rcx + 0x168]"
      ],
      [
        "0x18002fea7",
        "call",
        "r10"
      ],
      [
        "0x18002feaa",
        "cmp",
        "rax, -1"
      ],
      [
        "0x18002feae",
        "je",
        "0x18002fed5"
      ],
      [
        "0x18002feb3",
        "mov",
        "r8, rax"
      ],
      [
        "0x18002feb9",
        "mov",
        "r9, qword ptr [rcx + 0x128]"
      ],
      [
        "0x18002fed2",
        "jmp",
        "r9"
      ],
      [
        "0x18002feda",
        "xor",
        "eax, eax"
      ]
    ]
  },
  {
    "dll": "MyScriptDocument.dll",
    "start": "0x18002af50",
    "end": "0x18002b3c0",
    "sha256": "e419fb396004c5f90e36eba1b8369709329681ae278b05dfcc7aeeadbc743c98",
    "pins": [
      [
        "0x18002af6a",
        "mov",
        "rax, qword ptr [r8 + 8]"
      ],
      [
        "0x18002af71",
        "mov",
        "r14, qword ptr [r8]"
      ],
      [
        "0x18002afa1",
        "mov",
        "rdx, qword ptr [r14 + 0x18]"
      ],
      [
        "0x18002afc6",
        "lea",
        "r9, [rip + 0x1e1f53]"
      ],
      [
        "0x18002afe7",
        "call",
        "r10"
      ],
      [
        "0x18002afec",
        "je",
        "0x18002b001"
      ],
      [
        "0x18002b072",
        "mov",
        "r9, qword ptr [rbp + 0x6f]"
      ],
      [
        "0x18002b079",
        "mov",
        "r8, qword ptr [r14 + 0x18]"
      ],
      [
        "0x18002b07d",
        "mov",
        "rdx, qword ptr [r14 + 0x10]"
      ],
      [
        "0x18002b085",
        "call",
        "r10"
      ],
      [
        "0x18002b0a5",
        "mov",
        "edx, 0xc23"
      ],
      [
        "0x18002b0b0",
        "lea",
        "r8, [rbp + 0x6f]"
      ],
      [
        "0x18002b0b4",
        "mov",
        "r9d, 0x10"
      ],
      [
        "0x18002b0bd",
        "call",
        "rbx"
      ],
      [
        "0x18002b0d0",
        "je",
        "0x18002b0f7"
      ],
      [
        "0x18002b0d8",
        "mov",
        "r9d, dword ptr [r13 + 0x20]"
      ],
      [
        "0x18002b0df",
        "mov",
        "r8, qword ptr [r13 + 0x18]"
      ],
      [
        "0x18002b0e3",
        "mov",
        "dword ptr [rsp + 0x20], 0"
      ],
      [
        "0x18002b0eb",
        "mov",
        "r10, qword ptr [rax + 0x50]"
      ],
      [
        "0x18002b0ef",
        "call",
        "r10"
      ],
      [
        "0x18002b10f",
        "mov",
        "r8d, 0xc31"
      ],
      [
        "0x18002b130",
        "mov",
        "r8, qword ptr [rax + 0x40]"
      ],
      [
        "0x18002b134",
        "call",
        "r8"
      ],
      [
        "0x18002b139",
        "jne",
        "0x18002b28d"
      ],
      [
        "0x18002b27a",
        "call",
        "0x180029290"
      ],
      [
        "0x18002b2a8",
        "call",
        "0x180028070"
      ]
    ]
  },
  {
    "dll": "MyScriptDocument.dll",
    "start": "0x180027590",
    "end": "0x1800276b0",
    "sha256": "0cb5a2f52f3ce01e362d82adf732f63caf1d2329ba9073e0ef778d82283a5fee",
    "pins": [
      [
        "0x1800275d7",
        "mov",
        "edx, 0xeed"
      ],
      [
        "0x180027603",
        "mov",
        "edx, 0xeed"
      ],
      [
        "0x180027630",
        "mov",
        "r10, qword ptr [rax + 0x20]"
      ],
      [
        "0x180027658",
        "mov",
        "edx, 0xeed"
      ],
      [
        "0x1800275ba",
        "mov",
        "r14, qword ptr [rdx]"
      ],
      [
        "0x1800275c5",
        "mov",
        "r13, qword ptr [rdx + 8]"
      ],
      [
        "0x18002763e",
        "call",
        "r10"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x180086370",
    "end": "0x180086520",
    "sha256": "691baddc0db957f1a6ca9326202fc0d8e2af11d8ab0ae69868e2a9904b719e67",
    "pins": [
      [
        "0x180086405",
        "cmp",
        "r9, 0x10"
      ],
      [
        "0x18008640f",
        "mov",
        "r14, qword ptr [r8]"
      ],
      [
        "0x180086445",
        "lea",
        "rdx, [rbp + 0x18]"
      ],
      [
        "0x180086454",
        "mov",
        "r8, qword ptr [rax + 0x60]"
      ],
      [
        "0x180086458",
        "call",
        "r8"
      ],
      [
        "0x180086461",
        "mov",
        "qword ptr [rbp + 0x28], rax"
      ],
      [
        "0x180086468",
        "mov",
        "qword ptr [rbp + 0x30], rax"
      ],
      [
        "0x1800864b6",
        "call",
        "r8"
      ],
      [
        "0x1800864b9",
        "mov",
        "qword ptr [rbp + 0x10], rax"
      ],
      [
        "0x1800864c5",
        "xor",
        "r9d, r9d"
      ],
      [
        "0x1800864cb",
        "mov",
        "byte ptr [rsp + 0x20], 0"
      ],
      [
        "0x1800864d6",
        "mov",
        "r10, qword ptr [rax + 0x18]"
      ],
      [
        "0x1800864da",
        "call",
        "r10"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x1800648a0",
    "end": "0x180064cc0",
    "sha256": "bea66dbf92a0648613cc135f3df18c231291c349591ad4f776024774af4864fb",
    "pins": [
      [
        "0x180064a4d",
        "lea",
        "r8, [rax + rax*2]"
      ],
      [
        "0x1800649bc",
        "call",
        "r10"
      ],
      [
        "0x180064bbd",
        "call",
        "r10"
      ],
      [
        "0x1800649ca",
        "mov",
        "r8d, 0xc43"
      ],
      [
        "0x1800649d3",
        "mov",
        "qword ptr [r12 + rax + 8], rcx"
      ],
      [
        "0x180064aea",
        "mov",
        "r9, qword ptr [rax]"
      ],
      [
        "0x180064aed",
        "call",
        "r9"
      ],
      [
        "0x180064af0",
        "mov",
        "al, 1"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x180064da0",
    "end": "0x180064fe0",
    "sha256": "034452b96cf73dd53680ea77294b94dede27c666c29df56228bb2ab90d01bb2b",
    "pins": [
      [
        "0x180064e21",
        "call",
        "0x1800648a0"
      ],
      [
        "0x180064f11",
        "call",
        "0x1800648a0"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x180020d60",
    "end": "0x180020d63",
    "sha256": "b6cbececf1b07e74f37d57710c68c31f0b92cb08d51093252b321b07542e177b",
    "pins": [
      [
        "0x180020d60",
        "ret",
        "0"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x18007f7f0",
    "end": "0x18007f7fa",
    "sha256": "674daab7b3aac1352638d574a93fef70e8f36ebe7713d2ccc4f210ec2b915b87",
    "pins": [
      [
        "0x18007f7f0",
        "xor",
        "eax, eax"
      ],
      [
        "0x18007f7f2",
        "mov",
        "qword ptr [rdx], rax"
      ],
      [
        "0x18007f7f5",
        "mov",
        "qword ptr [rdx + 8], rax"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x180086c20",
    "end": "0x180086f00",
    "sha256": "83697b521395ba4cd74ede80e4b1ca63ce12e6d60141552b5e1a6fd1ec36af69",
    "pins": [
      [
        "0x180086d16",
        "mov",
        "r8, qword ptr [rbx + 0x10]"
      ],
      [
        "0x180086e4c",
        "je",
        "0x180086e79"
      ],
      [
        "0x180086cea",
        "test",
        "esi, esi"
      ],
      [
        "0x180086cec",
        "je",
        "0x180086dc3"
      ],
      [
        "0x180086dc3",
        "test",
        "bpl, bpl"
      ],
      [
        "0x180086de6",
        "mov",
        "rax, qword ptr [rbx + 0x10]"
      ],
      [
        "0x180086df1",
        "mov",
        "rcx, qword ptr [rax + 0xe8]"
      ],
      [
        "0x180086dfc",
        "cmp",
        "r14d, dword ptr [rcx + 8]"
      ],
      [
        "0x180086e26",
        "mov",
        "r8, qword ptr [rax + 0xe8]"
      ],
      [
        "0x180086e85",
        "call",
        "0x1800860a0"
      ],
      [
        "0x180086c65",
        "cmp",
        "qword ptr [rdx + 0x18], r15"
      ],
      [
        "0x180086c6b",
        "cmp",
        "dword ptr [rdx + 0x20], r14d"
      ],
      [
        "0x180086c71",
        "movzx",
        "eax, bpl"
      ],
      [
        "0x180086e4e",
        "mov",
        "r9d, r14d"
      ],
      [
        "0x180086e51",
        "mov",
        "r8, r15"
      ],
      [
        "0x180086e67",
        "mov",
        "r10, qword ptr [rax + 0xc0]"
      ],
      [
        "0x180086e6e",
        "call",
        "r10"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x180086fd0",
    "end": "0x180086fd5",
    "sha256": "e5e07f40beaca22b935c27638629aef167be193852b93b83f43a7f49fb621b4d",
    "pins": [
      [
        "0x180086fd0",
        "jmp",
        "0x180086c20"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x18008d020",
    "end": "0x18008d04d",
    "sha256": "fb26b4033a5193adeb02c0f6805a7e89eefd49a48c763dcbdbd309af8cb19c44",
    "pins": [
      [
        "0x18008d020",
        "sub",
        "edx, 0xc31"
      ],
      [
        "0x18008d026",
        "je",
        "0x18008d045"
      ],
      [
        "0x18008d02d",
        "cmp",
        "edx, 0xa"
      ],
      [
        "0x18008d035",
        "lea",
        "rax, [rip + 0x59094]"
      ],
      [
        "0x18008d045",
        "lea",
        "rax, [rip + 0x59224]"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x18008a750",
    "end": "0x18008a758",
    "sha256": "88ff4adc4a3deee084cf3205a16c03db230a09aaedc7e5f524e2834b44e1bcd2",
    "pins": [
      [
        "0x18008a750",
        "cmp",
        "dword ptr [rdx + 0x20], 0"
      ],
      [
        "0x18008a754",
        "sete",
        "al"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x180086520",
    "end": "0x180086680",
    "sha256": "ca60b60273f537d814210b5bcbbdeb23d75d944f29363b86e801356ec133597d",
    "pins": []
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x18006e120",
    "end": "0x18006e270",
    "sha256": "e86187851376ec4180cf11a7da5594ec8fb5c95bcc08c5c508d9c49d891cfe56",
    "pins": [
      [
        "0x18006e153",
        "cmp",
        "byte ptr [rsp + 0xa0], sil"
      ],
      [
        "0x18006e167",
        "xor",
        "al, al"
      ],
      [
        "0x18006e169",
        "mov",
        "byte ptr [rsp + 0x48], al"
      ],
      [
        "0x18006e237",
        "call",
        "0x18006d2c0"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x180001910",
    "end": "0x180003ba0",
    "sha256": "a1ba8eb9acefb3e60970d68317f36a699c50dacf587455a7d8dde87387842564",
    "pins": [
      [
        "0x180001a32",
        "lea",
        "rbp, [rip + 0xe4257]"
      ],
      [
        "0x180001a39",
        "mov",
        "r14, rbp"
      ],
      [
        "0x180001ac7",
        "mov",
        "rax, qword ptr [r14 + 0x20]"
      ],
      [
        "0x180001acb",
        "mov",
        "qword ptr [rcx + 8], rax"
      ],
      [
        "0x180001dd3",
        "lea",
        "r14, [rip + 0xe5a76]"
      ],
      [
        "0x180001dfb",
        "mov",
        "rbp, r14"
      ],
      [
        "0x180001e85",
        "mov",
        "rcx, qword ptr [rbx + 0x20]"
      ],
      [
        "0x180001e8d",
        "mov",
        "rax, qword ptr [rbp + 0x20]"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x18006c880",
    "end": "0x18006c8f0",
    "sha256": "a03ee5e30878fc51a7ac95dda65165c02e4dd4787417c6becefc378cdc2e9583",
    "pins": [
      [
        "0x18006c88d",
        "mov",
        "rdx, qword ptr [rdx + 0x40]"
      ],
      [
        "0x18006c899",
        "mov",
        "r9, qword ptr [rax + 0x30]"
      ],
      [
        "0x18006c89d",
        "call",
        "r9"
      ],
      [
        "0x18006c8a0",
        "test",
        "rax, rax"
      ],
      [
        "0x18006c8d9",
        "mov",
        "rax, qword ptr [rax + 8]"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x18009ede0",
    "end": "0x18009ee20",
    "sha256": "1c6062237d96722e62a622eb3d668f89d68e6a425f2c11158862f0d84519a6f7",
    "pins": [
      [
        "0x18009eded",
        "mov",
        "eax, r8d"
      ],
      [
        "0x18009ee00",
        "cmp",
        "r8, qword ptr [rax + 8]"
      ],
      [
        "0x18009ee06",
        "mov",
        "rax, qword ptr [rax]"
      ],
      [
        "0x18009ee14",
        "lea",
        "rcx, [rax + 0x10]"
      ],
      [
        "0x18009ee18",
        "cmovne",
        "r9, rcx"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x180080b40",
    "end": "0x180080c60",
    "sha256": "adfee3506a89ce34c1d429ab9c90c51ec521c8638c2dda01a190c141b5d5dee9",
    "pins": [
      [
        "0x180080ba2",
        "call",
        "0x1800804e0"
      ],
      [
        "0x180080bc7",
        "call",
        "0x18007f830"
      ],
      [
        "0x180080bfb",
        "call",
        "0x180080830"
      ],
      [
        "0x180080c31",
        "call",
        "0x18007f920"
      ],
      [
        "0x180080b40",
        "mov",
        "qword ptr [rsp + 8], rbx"
      ],
      [
        "0x180080b45",
        "mov",
        "qword ptr [rsp + 0x10], rbp"
      ],
      [
        "0x180080b4a",
        "mov",
        "qword ptr [rsp + 0x18], rsi"
      ],
      [
        "0x180080b4f",
        "mov",
        "qword ptr [rsp + 0x20], rdi"
      ],
      [
        "0x180080b54",
        "push",
        "r14"
      ],
      [
        "0x180080b56",
        "sub",
        "rsp, 0x40"
      ],
      [
        "0x180080b5a",
        "mov",
        "esi, r9d"
      ],
      [
        "0x180080b5d",
        "mov",
        "r14, r8"
      ],
      [
        "0x180080b60",
        "mov",
        "r9d, dword ptr [rdx + 8]"
      ],
      [
        "0x180080b64",
        "mov",
        "rdi, rdx"
      ],
      [
        "0x180080b67",
        "mov",
        "rbp, rcx"
      ]
    ]
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x1800804e0",
    "end": "0x180080830",
    "sha256": "91d12f36f1a72ed6d1c8e2008c991a5bcf9ae6e2af50c4967673d37082f6724c",
    "pins": []
  },
  {
    "dll": "MyScriptInk.dll",
    "start": "0x180080830",
    "end": "0x180080b40",
    "sha256": "3c14f5748ca8f2309cb6a58e68b56482e7d0d3c1b674857f599b47cee48bbdff",
    "pins": [
      [
        "0x180080a12",
        "call",
        "0x18007f830"
      ],
      [
        "0x180080a4a",
        "call",
        "0x18007f830"
      ],
      [
        "0x180080aac",
        "call",
        "0x18007f830"
      ],
      [
        "0x180080acf",
        "call",
        "0x18007f830"
      ]
    ]
  }
]
binaries={name:Binary(name)for name in {r['dll']for r in rows}}
expected={'MyScriptMath.dll':'d81b9cb071efac9d1300da1c6e8be24db2fbb12793ed1adefba38ffd0f80a8fb','MyScriptDocument.dll':'17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f','MyScriptInk.dll':'b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'}
for name,b in binaries.items():assert hashlib.sha256(b.data).hexdigest()==expected[name]
md=Cs(CS_ARCH_X86,CS_MODE_64)
for row in rows:
 b=binaries[row['dll']];a=int(row['start'],16);z=int(row['end'],16)
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==row['sha256']
 decoded={hex(i.address):(i.mnemonic,i.op_str)for i in md.disasm(raw,a)}
 for at,mn,op in row['pins']:assert decoded[at]==(mn,op),(row['dll'],at,decoded.get(at),(mn,op))

# Registered interfaces; no runtime object is manufactured from matching sizes.
bindings=[]
def bind(name,at,want):
 b=binaries[name];assert b.qwords(at,1)==(want,),(name,hex(at),hex(want))
 bindings.append(dict(dll=name,address=hex(at),target=hex(want)))
bind('MyScriptMath.dll',0x180097d48,0xbe1000013b9)
bind('MyScriptMath.dll',0x180097d58,0x180097f60)
bind('MyScriptMath.dll',0x180097f68,0x180097fd0)
bind('MyScriptMath.dll',0x180097fd0,0x18009cba0)
bind('MyScriptMath.dll',0x180097fd8,0x1)
bind('MyScriptMath.dll',0x180097fe0,0x180002920)
bind('MyScriptMath.dll',0x180097fe8,0x180007570)
bind('MyScriptMath.dll',0x1800981b0,0x18009c988)
bind('MyScriptMath.dll',0x180097c80,0x1800055b0)
bind('MyScriptMath.dll',0x180097cd0,0x1800054a0)
bind('MyScriptDocument.dll',0x1801ed800,0x1801ed790)
bind('MyScriptDocument.dll',0x1801ed798,0x18002fe40)
bind('MyScriptInk.dll',0x1800e61f8,0xc2100000c23)
bind('MyScriptInk.dll',0x1800e6208,0x1800e6238)
bind('MyScriptInk.dll',0x1800e6240,0x1800e5fa0)
bind('MyScriptInk.dll',0x1800e6248,0x1800e6048)
bind('MyScriptInk.dll',0x1800e6250,0x18008d020)
bind('MyScriptInk.dll',0x1800e6058,0x180086370)
bind('MyScriptInk.dll',0x1800e5ff0,0x180086fd0)
bind('MyScriptInk.dll',0x1800e5068,0x180064da0)
bind('MyScriptInk.dll',0x1800e51c0,0x18006e120)
bind('MyScriptInk.dll',0x1800e5178,0x18006c880)
bind('MyScriptInk.dll',0x1800e62b0,0x18008a750)
bind('MyScriptInk.dll',0x1800e60d0,0x180020d60)
bind('MyScriptInk.dll',0x1800e5cb0,0x1800e5ac0)
bind('MyScriptInk.dll',0x1800e5b20,0x18007f7f0)
bind('MyScriptInk.dll',0x1800e7870,0x1800e78a0)
bind('MyScriptInk.dll',0x1800e78d0,0x18009ede0)

def string(name,at,want):
 b=binaries[name];o=b.off(at);assert b.data[o:o+len(want)+1]==want+b'\0'
string('MyScriptMath.dll',0x18009c988,b'mathField')
string('MyScriptMath.dll',0x18009cba0,b'http://www.myscript.com/ns/myscript/page')
string('MyScriptDocument.dll',0x18020cf20,b'mathField')
string('MyScriptDocument.dll',0x18020c7d4,b'input')
string('MyScriptDocument.dll',0x18020c7dc,b'tag')
string('MyScriptDocument.dll',0x18020c8a4,b'INPUT')
b=binaries['MyScriptDocument.dll']
assert struct.unpack_from('<I',b.data,b.off(0x18011d3b0))[0]==0x11d348
bind('MyScriptInk.dll',0x1800e5b80,0x180080b40)
# Tiny independent branch procedure: constructor is empty; only actual INPUT
# invokes mode0. Absence, missing tag, and foreign registry must never become
# a caller-created full-domain replacement.
def select(field_input_tag, registry, actual_domain):
 c23=()
 if field_input_tag is None or field_input_tag==-1:return c23,'no-input'
 if field_input_tag not in registry:return c23,'lookup-reported-missing'
 pointer,incoming=registry[field_input_tag]
 # Literal same-pointer/same-count early return before mode dispatch.
 if pointer is None and not incoming:return c23,'equal-null-empty'
 # Canonical ordinary coordinates only in these control examples. General
 # packed endpoint normalization is covered by separate source-position proof.
 c23=tuple((max(a,c),min(b,d))for a,b in actual_domain for c,d in incoming
            if max(a,c)<=min(b,d))
 return c23,'domain-intersect-input'
controls=[]
registry={0:('nonempty',((4,8),)),1:(None,()),2:('allocated-empty',()),3:('full',((0,100),))}
for domain in [(),((3,11),),((3,8),(20,31))]:
 for tag in [None,-1,0,1,2,3,9]:
  result,status=select(tag,registry,domain)
  if tag in [None,-1,1,2,9]:assert result==()
  elif tag==3:assert result==domain
  elif tag==0:assert result==tuple((max(a,4),min(b,8))for a,b in domain if max(a,4)<=min(b,8))
  controls.append(dict(domain=domain,tag=tag,result=result,status=status,nnEligible=bool(result)))
report=dict(staticOnly=True,vendorExecution=False,peSha256=expected,regions=rows,
 bindings=bindings,jumpTableCase2='11D3B0->11D348->mathField20CF20',branchControls=controls,
 contract='Registered Math13B9 namespace marker -> actual C98.kind2 -> INPUT category102. Original-selected clone preserves actual registry identity. C23 constructor zeroes ranges and attaches no-op C43; actual DAF INPUT lookup on SAME C20 authorizes V50 mode0 domain-copy THEN VC0 intersection with INPUT and C23 validation; missing/empty INPUT remains empty and bypasses NN.',
 boundary='No runtime port, native original page acceptance, CBB/history receiver, general C25/source/property/imported-result path, multi-F0 grouping or DOM publication is established here.')
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS{len(rows)} regions/{sum(len(r["pins"])for r in rows)} pins/{len(bindings)} bindings/{len(controls)} branch controls; static vendor bytes only')
