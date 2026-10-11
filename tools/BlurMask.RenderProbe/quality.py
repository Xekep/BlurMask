"""Check synthetic render detail against pixel-area integration, without a git baseline."""
import argparse
import json
from pathlib import Path

from PIL import Image, ImageFilter
from compare import difference


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    original = Image.open(args.output / "synthetic-4k-image-100.png").convert("RGB")
    failures, results = [], []
    for zoom, limit in ((25, 2.1), (10, 3.7)):
        image = Image.open(args.output / f"synthetic-4k-image-{zoom}.png").convert("RGB")
        if image.size != (3840 * zoom // 100, 2160 * zoom // 100):
            raise ValueError("Unexpected render dimensions")
        area = original.resize(image.size, Image.Resampling.BOX)
        error = difference(image, area)
        negative = difference(image.filter(ImageFilter.GaussianBlur(1)), area)
        if error["meanAbsoluteError"] > limit:
            failures.append(f"{zoom}%: reduced screenshot lost detail")
        if negative["meanAbsoluteError"] <= limit:
            failures.append(f"{zoom}%: blurred negative control was accepted")
        results.append({"zoom": zoom / 100, "againstArea": error,
                        "allowedMeanAbsoluteError": limit, "blurNegativeControl": negative})
    for zoom in (100, 25, 10):
        pen = Image.open(args.output / f"synthetic-thin-pen-{zoom}.png").convert("RGB")
        pixels = list(pen.getdata())
        ink = sum(min(pixel) < 255 for pixel in pixels)
        # Subpixel coverage is visible as intermediate shades along the stroke.
        covered = sum(20 < min(pixel) < 240 for pixel in pixels)
        if ink < 100 or covered < 100:
            failures.append(f"{zoom}%: thin stroke missing or not antialiased")
        results.append({"penZoom": zoom / 100, "inkPixels": ink, "coveragePixels": covered})
    report = {"syntheticOnly": True, "comparisons": results, "failures": failures}
    (args.output / "quality.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))
    if failures:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
