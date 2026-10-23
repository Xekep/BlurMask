# Owned English primary materialization

The independent library now joins the initial PRIMARY consumer to root-word
alternatives, native ordering and duplicate merging, heap rebuilding, phase-zero
quota and row materialization. This is a primary-only checkpoint, not a complete
node or a completed recognizer. Russian cursive, history, final selection and
application integration remain unfinished; the default recognizer is unchanged.

The owner attaches privately to the actual consumer before execution. Transfer
requires the same consumer, heap, policy, live lease and synchronous thread.
After transfer the initial heap refuses mutation. No exported snapshot grants
execution authority. Results remain pending until the original query succeeds;
entered failures discard them and fault the source.

Managed and Native AOT each passed 234 actual owners (134 + 100) and 5,241 retained
rows with byte-identical captures. An independent Python oracle reconstructs
sorting, live duplicate merges, physical heap order, quota, all 88/80 output
bytes, residue, source ordinals, definedness masks and all nine context fields.
It first binds complete input heaps, contexts, roots and ordered fixture names
to immutable initial-consumer capture hashes. No expected transcription feeds
the calculation. Existing unattached-consumer and null-consumer query captures
remain byte-exact for both corpora.

Each capture verifies 46 refusal guards and 16 ordered cancellation stages,
including actual stage reach and exception type. Prepared controls separately
exercise 0/1/125/126/250 heap sizes and context growth through 217 new nodes;
the actual corpus adds no new context nodes. Deterministic cancellation inside
sorting/context creation and after the original Run returns remains untested.

The additional enumerated payload ceiling is 690,192 bytes, reserved within
1 MiB; the complete logical ceiling is 127,687,945 bytes and copy-work allowance
is 64 MiB. These are protocol budgets, not measured RSS. The private context
capacity stays 2,048; at most 1,506 nodes are admitted by this checkpoint.

CI runs the 134-owner corpus in both build modes. Its preceding initial-consumer
capture supplies the explicit baseline path; the oracle requires the immutable
SHA-256 `d1ffe91e88cf7e48f7d68cda029790c2d73f9e8c9f9d8c5640890c3ff1c712d0`.
The separately verified 100-owner corpus is not currently part of CI.

CI's raw141 fixture uses a different JSON serialization from the historical
fixture. Their typed coordinates are identical; only the baseline's
`InputSha256` metadata differs. `immutable_baseline_adapter.py` handles that
one exact known pair of input/baseline hashes. It recovers the historical raw
bytes, replaces that single digest literal and still requires the entire
historical baseline hash above. Other bytes, rows, guards and the100-owner
baseline are unchanged. Forty-two negative controls and both independent
134/100 owner replays pass. Unknown hashes or missing raw-input proof refuse.

Reproduce after generating the existing raw-only CI fixture and initial consumer:

```sh
dotnet run --project tools/LegacyEnglishCasePrimaryCompletion.Probe -c Release -- --owner models .artifacts/legacy-en119-final-single-blind.json .artifacts/legacy-en-case-primary-completion-managed.json
python -I -B tools/LegacyEnglishCasePrimaryCompletion.Probe/audit_output.py .artifacts/legacy-en-case-primary-completion-managed.json 134 .artifacts/legacy-en-case-initial-consumer-managed.json .artifacts/legacy-en119-final-single-blind.json
```

The full-node continuation still requires auxiliary-language heaps, controller
buffer lineage, history and a longer owned lease. The primary-only owner does
not provide these. Vendor machine code was not executed during verification.
