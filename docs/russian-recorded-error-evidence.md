# Recorded Russian errors: graph, cache and language completion

The restored library still returns4/9 exact words and16/33 character edits
(48.48% CER) on the existing nine short recorded tracks. This is a tiny
diagnostic, not representative handwriting quality. Every supplied track is
one coordinate sequence; original physical stroke boundaries are unknown.
See [recorder coordinate and boundary provenance](russian-recording-coordinate-provenance.md).
No releases, expected
labels or corrective dictionary entries were inserted into inference.

Verified library base: `d8cd38117cfad9f3f9d1bfd4cf992d17b79564d8`.
Managed and fresh Windows NativeAOT captures are byte-identical, SHA-256
`7bc8bd2cd081dc5551f87777bc038962b2a8722328caf06439e0f64fcddffedc`.
The root review independently reran Native inference, verified every frozen
dependency hash and repeated the raw-AK/UTF8 and packed-LK audits.

| Writer | Expected, checked after inference | Actual WORD output |
| --- | --- | --- |
| 3 | да / ещё / мягких | да / Дзб / наших |
| 7 | да / ещё / мягких | да / суб / мягких |
| 12 | да / ещё / мягких | да / шуб / мятых |

There are868 acoustic group evaluations,31 published glyphs,22,440 compact
records,362 language queries and274 final terminal rows. Inspection performs
no extra inference. The actual one-physical context producer emits zero227
windows; all24,304 captured context entries are−1024. This identifies absent
context information, not proof that absent pen-ups caused a particular error.

An independent post-inference search through actual reciprocal graph selectors
finds every expected literal word within retained8..10 acoustic labels. Complete
graph/cache exclusion therefore does not explain these nine failures. That
search omits transitions, blanks, case/query permissions, beam pruning and
language scores; it is not a decoder or winner.

Writer3 «мягких» survives the actual primary rows through node19, row98,
cost73968; its completed row144 is present. Competing «наших» has primary
row93, cost78898, and completed row135. Language completion reverses the order:

| Candidate | Parent cost | Word term | Class term | Final cost |
| --- | ---: | ---: | ---: | ---: |
| мягких | 73968 | 1522 | 11894 | 87384 |
| наших | 78898 | −7925 | 12452 | 83425 |

Category/boundary terms are zero here. The correct candidate loses by3959.
Same-owner scorer reevaluation matches all274 completion deltas; independent
packed-LK/AK checks pass274 rows/138 terminal nodes, including case metadata,
source ranges, anchor geometry and restart tags. No ported arithmetic/category
bug was identified in this comparison. SDK output equivalence is not claimed.

Other wrong outputs generally have lower ordinary acoustic path costs than
the expected word. Some expected prefixes do not reach final primary rows;
every intermediate rejection has not been diagnosed. Writer7 «ещё» reaches a
completion83064 but loses to «суб»,72903. These findings do not justify forcing
reference words or fitting thresholds to this diagnostic.

The local frozen probe, audits, captures, dependency hashes and reproduction
are retained in `E:/BlurMask-build-artifacts-20261014/ru-quality-evidence/`.
Input manifest SHA-256:
`56082f15e2d53c7958f8beb353fbf63718f39e256a7f0afd78f0d48ea7372832`.
