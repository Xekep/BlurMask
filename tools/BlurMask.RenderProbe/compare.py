"""Compare only the generated synthetic PNGs; never read or copy a private board.

Requires Pillow. Both runs must use the same synthetic generator and dimensions.
"""
import argparse
import json
import math
from pathlib import Path

from PIL import Image, ImageChops, ImageFilter, ImageStat


def difference(left, right):
    if left.size != right.size:
        raise ValueError("Image dimensions changed")
    diff = ImageChops.difference(left.convert("RGB"), right.convert("RGB"))
    stats = ImageStat.Stat(diff)
    return {
        "meanAbsoluteError": sum(stats.mean) / 3,
        "rootMeanSquareError": math.sqrt(sum(v * v for v in stats.rms) / 3),
        "maximumChannelError": max(hi for _, hi in stats.extrema),
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("baseline", type=Path)
    parser.add_argument("after", type=Path)
    args = parser.parse_args()
    results, failures = [], []
    original = Image.open(args.baseline / "synthetic-4k-image-100.png").convert("RGB")
    for zoom in (100, 25, 10):
        for scenario in ("synthetic-4k-image", "synthetic-thin-pen"):
            filename = f"{scenario}-{zoom}.png"
            before = Image.open(args.baseline / filename).convert("RGB")
            after = Image.open(args.after / filename).convert("RGB")
            delta = difference(before, after)
            result = {"scenario": scenario, "zoom": zoom / 100, "againstBaseline": delta}
            if scenario == "synthetic-4k-image" and zoom != 100:
                # Independent pixel-area integration reference. A mip may differ slightly
                # from direct Skia filtering, but should not wash out resolvable detail.
                area = original.resize(after.size, Image.Resampling.BOX)
                old_error = difference(before, area)
                new_error = difference(after, area)
                threshold = old_error["meanAbsoluteError"] + 2.0
                result.update(baselineAgainstArea=old_error, afterAgainstArea=new_error,
                              allowedMeanAbsoluteError=threshold)
                if new_error["meanAbsoluteError"] > threshold:
                    failures.append(f"{filename}: mip loses detail versus area reference")
                if zoom == 25:
                    blurred = before.filter(ImageFilter.GaussianBlur(1.0))
                    negative = difference(blurred, area)
                    result["blurNegativeControl"] = negative
                    if negative["meanAbsoluteError"] <= threshold:
                        failures.append("Quality gate did not catch intentionally blurred PNG")
            elif delta["maximumChannelError"] != 0:
                failures.append(f"{filename}: full-resolution image/pen pixels changed")
            results.append(result)
    report = {"syntheticOnly": True, "comparisons": results, "failures": failures}
    (args.after / "quality.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))
    if failures:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
