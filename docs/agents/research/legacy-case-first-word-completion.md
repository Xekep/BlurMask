# Fresh case-aware first-word completion

`LegacyNativeFirstDictionaryWordCompletion.ForFreshCaseSpan(policy, span)`
connects the existing first-word completion algorithm to the actual fresh292
dictionary and its owned glyph-span lease. It passes the complete case tag and
ending symbol into source70 metadata and the complete tag into source50 restart.
The DIRECT constructor retains its original stripped-tag behavior.

Admission requires the same model, graph, dictionary and controller owner,
mode0, no tracing, nonpositive secondary scale and the proved single-source
policy. Every candidate must use the span's ending node and controller's actual
knowledge/history flags. External row callbacks, restart resolution and heap
insertion are followed by lease validation. A failed completion producer cannot
be reused. Existing initial-anchor, protocol and geometry restrictions remain.

The ordinary frontend280 restriction is proved separately from the absence of
an associated dictionary:204323/204331 dispatch222.V280 to155710, which forwards
the ordinary21B.V1C8 getter05A400 returning false. The metadata category comes
from the source70 token's offset8, including nonzero case categories; it is not
replaced with a geometry category.

The headless probe constructs the actual raw-ink/main-span source and completes
70 terminal candidates obtained through real startup queries and materialized
parent rows. Another168 explicitly prepared case-state/punctuation controls
exercise all12 metadata categories. These modified controls are not claimed to
be recognized words. An independent Python reader reconstructs terminal preorder
IDs from the packed dictionary, reads actual language codebooks/calibration and
compares all16 metadata bytes,40 language bytes and128 allocation bytes, including
preserved residue and restart case bits. Thirteen guards cover owner, node,
flags, stale map, callback invalidation and cancellation. Both managed and Native
AOT builds must pass the same audit.

This is a bounded first-word consumer. It does not implement the full case-word
scheduler, auxiliary language queries,207CB0 materialization, result backtracking
or a final recognized text. The application AI pen is not switched to it.

Probe: `AK LK engine separation case-data publication capture.json`.
Audit: `audit_output.py LK case-preparation-controls capture.json`.
Static proof: `tools/handwriting/audit_case_first_word_completion.py [report]`.
