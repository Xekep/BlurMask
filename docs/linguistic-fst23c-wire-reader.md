# Immutable inline23C/109 storage

`LegacyFst23CWireLayout` implements the bounded storage grammar established by
the [native wire audit](linguistic-fst23c-native-wire-20261021.md). It does not
execute an automaton, admit a language source, validate a graph or recognize
numbers. In particular, `RawField20` is not named or used as an initial state.

`ReadInline(bytes, absoluteOffset)` accepts a complete caller-supplied inline
block starting with its version word. That offset is needed for absolute stream
alignment; it does not authenticate a source. The inline object has no resource
envelope. Only version109 is supported. Narrow labels use the native `<255`
branch, including FF→FFFF and the shifted flag bytes; wide labels use ushort
storage. Nonzero alignment and narrow descriptor padding are preserved in the
owned bytes and available through defensive copies.

The library limits inputs to64MiB, states to1,048,576 and transition words
to4,194,304. These are independent-library storage budgets, not SDK limits or
process RSS promises. Counts and exact total extent are checked before copying.
The native allocation count must equal the last descriptor's field0 read count;
rejecting malformed disagreement is intentionally stronger than reproducing the
native allocation/read hazard. No monotonic-state-offset or target validation
is implied at this layer. A future outgoing/query component must validate those
relationships and bind the actual alphabet and costs separately.

The reader owns one bounded byte snapshot, then reparses that snapshot before
publishing it. Descriptor and word access is constant-time; no second full
descriptor/transition array is retained. Copies check cancellation every64KiB.
SHA hashing is a bounded synchronous operation, followed by a cancellation
check before return. Public snapshots cannot mutate the retained object.

`ReadActualRussianNumber(root)` is a separate, narrow origin adapter. It requires
the exact complete Russian linguistic-root hash and retains that root's exact
13A/202 child at[299000,301580). Its inline word begins at299576. Generic caller
bytes never acquire these root/child receipts. Equal source bytes loaded into a
second root retain a different object identity. The receipts describe storage
origin only; neither API creates a query engine or bypasses source admission.

The actual resource has16 labels,98 states,99 descriptors and299 raw transition
words. Its raw field20 is12. The independent probe reconstructs serialized
descriptors and all words from the model bytes and from separately generated
narrow/wide controls. Managed/NativeAOT parity is checked on the same inputs.
See `tools/LegacyFst23C.Probe` for the runnable checks.

The [outgoing native checkpoint](linguistic-fst23c-outgoing-native-proposal-20261021.md)
is additional static evidence, not an implementation in this reader. Full28C
alphabet mapping, special labels, state0/history/paging and final recognition
remain unfinished. This storage checkpoint changes no app defaults or models.
