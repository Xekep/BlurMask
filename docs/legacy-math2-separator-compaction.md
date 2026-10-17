# Prepared four-byte separator compaction

`LegacyPreparedMathSeparatorCompaction` restores041BA0 on explicit prepared
four-byte output units. It tests only the low U16 symbol against an explicit
separator, retains the first separator of each interior run, removes leading,
duplicate and trailing separators, and preserves every retained unit's high
U16 payload. After each removal, assignments to that unit become−1 and higher
indices decrement. It returns detached units, assignments and an inspection map.

Inputs are bounded to1000 units/6000 assignments. Assignments must be−1 or a
valid original index. Empty input, all separators, nonzero upper payloads,
separators0/267/65535, cancellation and mutated caller arrays are covered.
No actual request-marker producer, output owner or EF4 transaction is inferred.

Verification on2026-10-18:

- 6143 prepared controls, including every separator/nonseparator pattern up to
  length10 for three markers, plus1000-unit/6000-assignment boundaries.
- Independent Python run-based oracle verifies all four bytes and assignment
  remaps:42,804 deleted assignments and37,872 retained nonzero high words.
- Idempotence, input/result-copy isolation and six refusal guards.
- Fresh Managed/Windows NativeAOT captures match byte for byte, SHA-256
  `9eea22a12937314ee77816dd88cfcfee5b7680b3dfe5fee1fcc499f555501649`.
- Static whole-region hash and21 instruction pins prove U16 comparison,
  four-byte movement and both index-repair loops; no vendor binary execution.

Run the probe with one JSON output path, then its standard-library
`audit_output.py`. The static auditor requires local preserved research files.
Remaining correction helpers and actual completed-record integration are
documented in `legacy-math2-ef4-control-flow.md`; this helper does not establish
complete formula recognition.
