"""Independent own-data bitstream/category controls; no engine or model inputs."""
import argparse
import json
from pathlib import Path
import random


def packed(values, width):
    # Reference builds a byte bitstream, separately from native uint32 extraction.
    bits = [int(bit) for value in values for bit in reversed(f"{value:0{width}b}")]
    bits += [0] * ((-len(bits)) % 32)
    return bytes(sum(bits[start + bit] * 2**bit for bit in range(8))
                 for start in range(0, len(bits), 8)).hex()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    rng = random.Random(190095)
    cases = []
    # Includes every width, cross-word field, bound equality and neighboring values.
    for width in range(1, 9):
        for n in [1, 2, 3, 5, 7, 15, 31, 32, 33, 65, 129]:
            lower = [rng.randrange(2**width) for _ in range(n)]
            upper = [rng.randrange(2**width) for _ in range(n)]
            primary = [rng.choice([0, 1, 2, 255]) for _ in range(n)]
            secondary = [rng.choice([0, 1, 17, 255]) for _ in range(n)]
            threshold = rng.choice(lower + upper)
            for current in sorted({max(0, threshold - 1), threshold, min(255, threshold + 1), 0, 255}):
                a, b, cleared = primary.copy(), secondary.copy(), 0
                for i in range(n):
                    allowed = (lower[i] == 0 or current >= lower[i]) and (upper[i] == 0 or current <= upper[i])
                    if not allowed and (a[i] or b[i]):
                        a[i] = b[i] = 0
                        cleared += 1
                cases.append(dict(width=width, count=n, lower=lower, upper=upper,
                                  lowerPacked=packed(lower, width), upperPacked=packed(upper, width),
                                  primary=primary, secondary=secondary, current=current,
                                  expectedPrimary=a, expectedSecondary=b, cleared=cleared))
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(dict(schemaVersion=1, cases=cases)), encoding="utf-8")
    print(f"Wrote {len(cases)} independent own packed/category controls: {args.output}")


if __name__ == "__main__":
    main()
