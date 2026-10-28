# Original input frames and the cold Math2 bound

The prefix context now retains the first dimension of the exact source tensor
used by its cold preparation. `ColdMaximumPosition` derives the numeric bound
from that original dimension and the actual encoder `max_pred_counts` output.
It does not enforce driver termination or implement maxstep fallback.

## Native producer chain

Document044630 creates typeC6D with an eight-byte pipeline-description packet
and stores it at EED218. The actual Ink C6D constructor table points to primary
table0DECE8, whose V10 is014BE0. Document047C50 invokes that callback with its
local tensor tuple at `rbp+30` as argument9. On success it checks the first
descriptor's dimension0 against3000, then passes the same tuple to the EEE V28
driver03A260. This establishes the source tuple identity, not just equal shapes.

Ink014BE0 calls013F20 to construct that tuple. It allocates 0x110-byte
descriptors, puts the feature tensor in ordinal0 and names it `src`. The C66
service used for feature construction binds to00AD10, which joins selected
feature schemes through Engine54.V60=186F40. That concatenation checks equal
rank/type and all dimensions except axis1; it preserves dimension0.

The tensor constructor0067D0 stores rank at descriptor08 and dimensions starting
at10.013F20 unsqueezes the feature matrix at axis1: it shifts later dimensions,
writes1 at18 and increments rank08. Original dimension0 at10 is preserved, so
the resulting source is `[N,1,26]`. `N` counts processed feature frames; it is
not the width26, batch1, original pen points, physical strokes or downsampled
encoder memory length. This proof does not revalidate every feature coefficient.

EEE03A260 reads exactly `*(uint64*)(*(uint64*)(inputPacket+8)+10)` for the
length factor. Together with the independently pinned
[step/temperature producers](legacy-math2-step-temperature-producers.md), the
fingerprinted cold three-model route gives:

```text
maximum = min(1000, 4*N + 1)
if actual encoder K != 0:
    maximum = min(maximum, K)
```

`K` is the checked INT64 scalar output at ordinal4, not optional classification
outputD8 or a metadata constant. The current tensor owner already refuses
negative counts. The bounded internal tensor API allows up to6000 frames;
the native047C50 host route's3000-frame guard remains a separate restriction.
Retaining a bound in the diagnostic prefix API does not claim that every one
of its inputs has passed that native host route.

## Checks and remaining work

`tools/handwriting/audit_math2_input_frame_bound.py report.json` verifies three
whole PE hashes, nine regions,38 instruction pins, six bindings and prepared
layout/length discriminators without loading an SDK DLL. The existing prefix
probe writes an additional `.bounds.json` containing originalN, actualK and the
derived bound for an independent arithmetic check. Its original full tensor,
prefix and EOS capture remains byte-identical. These facts are carried by the
same owner; callers cannot supply substitute N/K values to that owner.

The outer loop still needs completed-record ownership, progress and stopping
rules, final-step fallback and final hypothesis transport. A numeric bound
alone is not a complete native search or evidence of improved recognition.
