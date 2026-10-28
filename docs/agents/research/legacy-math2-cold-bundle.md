# Owning cold Math2 bundle

`Math2Mode0ColdBundle` is an internal cold-only library entry. It binds the exact
raw Math2 resource, privately reads an authenticated exported pack, validates the
297 labels and all three full graph hashes, and owns fresh official-ORT sessions.
It then derives the force-nonobservable attention policy from the checked profile
rather than accepting a caller boolean. The [static factory proof](legacy-math2-force-policy-producer.md)
establishes the third model's metadata precedence and EF1 destination byte.

The raw resource SHA256 is2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a.
The vocabulary hash is4366c61258cd74b6520f654898f94b846cf772b5fdcd975b6a95a1cdaf154dce,
computed over each UTF8 label prefixed by its little-endian I32 byte length.
Graph identity is checked independently by the existing tensor-model owner.
An authenticated but different vocabulary or graph is refused before inference.
The pack reader retains its own body; no caller-owned label or graph array is
retained as mutable validated state. It avoids an extra whole-pack copy.

`PrepareState` executes the actual encoder and initializer and returns the existing
owned tensor state at target0/offset0 with no prefix. Concurrent independent
preparations do not inherit another state's cache or position. A live state
prevents disposal of its model owner, run or runtime. The old explicitly prepared
policy API remains available for diagnostics. This entry accepts no inherited
native child, custom service override, configured prefix, or external policy.

Managed and fresh Native AOT checks cover10 refusals, including a rehashed changed
label, a rehashed changed graph, changed/truncated resources, entry cancellation,
early disposal and stale step use. A synthetic three-frame zero tensor exercises
real official-ORT inference; a separately explicit prepared BOS0 selection checks
that every newest attention column receives bitsFF7FFFFF. An independent Python
oracle reads the raw vocabulary and checks all captured masked values. The two
captures match SHA25644f43ce023af50dd46dca32664ee479673e26a91378014edb399a3dab7a4bc51.
This is a policy/lifetime check, not a handwriting or accuracy fixture.

The tensor state still exposes prepared search widths, temperatures, delta and
prefix/metadata sidecars through its diagnostic numerical API. This cold bundle
does not implement the outer search, maxstep fallback, completed-record ownership,
final structural acceptance or an application recognizer. Russian and English
recognition readiness, automatic mode selection and the app default are unchanged.
