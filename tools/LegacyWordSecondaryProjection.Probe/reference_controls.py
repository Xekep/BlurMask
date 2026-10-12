"""Independent field projection and functional parent/suffix context oracle."""
import ast
import json
import random
import struct
import sys
from pathlib import Path

context_source = Path(__file__).resolve().parents[1] / "LegacyWordContextAlternatives.Probe/reference_controls.py"
tree = ast.parse(context_source.read_text(encoding="utf-8-sig"))
selected = [node for node in tree.body if isinstance(node, ast.FunctionDef) and node.name == "compose"
            or isinstance(node, ast.Assign) and any(isinstance(x, ast.Name) and x.id == "actions" for x in node.targets)]
namespace = {}
exec(compile(ast.Module(body=selected, type_ignores=[]), str(context_source), "exec"), namespace)
compose = namespace["compose"]
rng = random.Random(0x20feca)
tokens = [0, 65, 80, 1025, 1048, 0xfff7, 0xfff8, 0xfff9, 0xfffa, 0xfffb, 0xfffc, 0xfffd]
cases = []
lookups_total = 0
for case_index in range(512):
    pc, sc = rng.choice([1, 2, 3]), rng.choice([2, 3, 20])
    slot = bytearray(rng.randbytes(128))
    slot[32] = rng.choice([0, 1, 3, 127, 255])
    slot[8:16] = bytes(8)
    suffix = [rng.choice(tokens) for _ in range(rng.randrange(5))]
    current_node = 200
    previous = current_node - (slot[32] if slot[32] < 128 else slot[32]-256) - 1
    primary = bytearray(rng.randbytes((pc+2)*40)) if pc > 1 else bytearray()
    secondary = bytearray(rng.randbytes((sc+2)*40))
    prefixes = []
    for index in range(sc):
        at = index*40
        struct.pack_into("<i", secondary, at, rng.choice([rng.randrange(-5000, 5000), 0x4000000, 0x4000001, -2147483648]))
        struct.pack_into("<H", secondary, at+24, rng.randrange(3))
        secondary[at+27] = (secondary[at+27]&128) | rng.randrange(sc)
        secondary[at+8:at+24] = bytes(16)
        prefixes.append([[rng.choice(tokens) for _ in range(rng.randrange(4))] for _ in range(2)])
    parents = []
    for arc in range(3):
        parent_slot = bytearray(rng.randbytes(128))
        struct.pack_into("<i", parent_slot, 88, 0)
        struct.pack_into("<H", parent_slot, 60, arc)
        parent_pool = bytearray(rng.randbytes(sc*40))
        parent_prefixes = []
        for index in range(sc):
            parent_pool[index*40+8:index*40+24] = bytes(16)
            parent_prefixes.append([[rng.choice(tokens) for _ in range(rng.randrange(5))] for _ in range(2)])
        parents.append(dict(slot=parent_slot.hex(), secondary=parent_pool.hex(), prefixes=parent_prefixes))
    expected_slot, expected_primary, expected_secondary = bytearray(slot), bytearray(primary), bytearray(secondary)
    expected_slot[16:24] = secondary[0:8]
    expected_slot[38] = secondary[30]
    expected_slot[30:32] = secondary[24:26]
    if pc > 1:
        expected_primary[27] = (primary[27]&128) | (secondary[27]&127)
    outputs, calls, lookup_order = [], [], []
    for index in range(sc):
        at = index*40
        output = prefixes[index]
        if struct.unpack_from("<i", secondary, at)[0] < 0x4000000:
            parent_arc = struct.unpack_from("<H", secondary, at+24)[0]
            parent_alternative = secondary[at+27]&127
            lookup_order.append([previous, parent_arc])
            prefix = parents[parent_arc]["prefixes"][parent_alternative]
            mapped, mapped_calls = compose(prefix[0], suffix, True)
            raw, _ = compose(prefix[1], suffix, False)
            output = [mapped, raw]
            calls.extend(mapped_calls)
            expected_secondary[at+31] = (secondary[at+31]&224) | (slot[33]&31)
            expected_secondary[at+26] = slot[32]
        outputs.append(output)
    lookups_total += len(lookup_order)
    cases.append(dict(pc=pc,sc=sc,node=current_node,slot=slot.hex(),suffix=suffix,primary=primary.hex(),secondary=secondary.hex(),
                      prefixes=prefixes,parents=parents,expectedSlot=expected_slot.hex(),expectedPrimary=expected_primary.hex(),
                      expectedSecondary=expected_secondary.hex(),outputs=outputs,calls=calls,lookups=lookup_order))
Path(sys.argv[1]).write_text(json.dumps(dict(cases=cases), separators=(",",":")), encoding="utf-8")
print(f"generated {len(cases)} full heap/pool projections and {lookups_total} exact parent-context lookups")
