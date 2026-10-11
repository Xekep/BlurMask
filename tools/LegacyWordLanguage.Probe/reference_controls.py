"""Independent raw lookup checks for a locally supplied inspected RU LK asset.

Generated values belong in a private ignored output directory. This script does
not download, redistribute, load or execute a vendor engine. It reads storage,
not calibrated language probabilities or dictionary-word correspondences.
"""
import hashlib
import json
import random
import struct
import sys
from pathlib import Path

source = Path(sys.argv[1])
output = Path(sys.argv[2])
if source.stat().st_size > 16 * 1024 * 1024:
    raise ValueError("Caller resource exceeds 16MiB")
data = source.read_bytes()
if hashlib.sha256(data).hexdigest() != "d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e":
    raise ValueError("Only the inspected RU lexical asset's offsets are verified")
assert struct.unpack_from("<I", data, 2513131)[0] == 0x2D1
assert struct.unpack_from("<5I", data, 2513213) == (2, 3, 439826, 1024, 1024)
word_costs = struct.unpack_from("<256i", data, 3502860)
unigram_costs = struct.unpack_from("<256i", data, 4553492)
bigram_costs = struct.unpack_from("<256i", data, 4554516)
word_classes = data[2513246:3063030]
randomizer = random.Random(271001)
ids = [0, 1, 2, 15, 31, 32, 439825] + [randomizer.randrange(439826) for _ in range(249)]
words = []
for word_id in ids:
    # Direct little-endian byte window extraction independently checks the C#
    # implementation's 32-bit crossing reconstruction.
    byte_offset, bit = divmod(word_id * 10, 8)
    cluster = (int.from_bytes(word_classes[byte_offset:byte_offset + 3], "little") >> bit) & 1023
    words.append({"id": word_id, "class": cluster, "cost": word_costs[data[3063030 + word_id]]})
classes = []
for i in range(256):
    current = randomizer.randrange(1024)
    previous = None if i % 4 == 0 else randomizer.randrange(1024)
    cost = unigram_costs[data[3503884 + current]] if previous is None else bigram_costs[data[3504908 + previous * 1024 + current]]
    classes.append({"previous": previous, "current": current, "cost": cost})
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps({"words": words, "classes": classes}, indent=2), encoding="utf-8")
print(f"Wrote {len(words) + len(classes)} independent raw lookup references to caller private output")
