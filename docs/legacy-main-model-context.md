# Owned reusable main model context

`LegacyFreshMainRecognitionContext.Load` snapshots the pinned acoustic,
linguistic and line resources once. Each `Prepare` creates a new raw document,
dictionary context, graph and publication state. No mutable recognition state
is shared. `PrepareTextChoices` rejects a candidate from a different context.
The existing static preparation API remains available and loads a context for
that invocation.

The synthetic seven-input probe and nine recorded diagnostic words overwrite
the caller's model buffers after loading and check shared model identity and
distinct per-input owners. Managed and Native AOT captures remain byte-identical:

- Synthetic glyph choices: `eb2c197d59781b75805cdcf029a84baf069f97fed0012978880bc219b730ebfc`.
- Recorded diagnostic: `7aa4e8899fdd12921d6de9cc50f31bac294a29628289c4168eedf24aa2810286`.

Local Windows Native AOT measurements used the same executable, fresh process
per run, nine fixed recorded words, and alternating shared/per-input model loads.
Two runs per mode gave median peak working sets of 123,578,368 and 159,307,776
bytes respectively (about 22% lower with reuse). These are library-only process
measurements, without UI or canvas buffers; they do not establish application
memory use on Linux. Concurrent build activity makes elapsed times unsuitable
for a speed claim.

Recorded input remains a small diagnostic with missing pen-up metadata; model
reuse neither improves its recognition accuracy nor establishes Russian cursive
readiness. The new context is a library API, not the application's recognition
switch or a complete native text document acceptance implementation.
