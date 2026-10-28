# Fresh document selection context

`LegacyOwnedFreshTextSourceContext` restores the noncached `0210C0` registration path over one actual `LegacyOwnedRawInkDocument`. The document is this independent library's NEW immutable root. Its original input object IDs are `0..N-1`; these are not recovered SDK page IDs, sampled physical ordinals, or caller-supplied selection IDs.

The initial selection counter is **N**, derived from the native C51 registry policy rather than an assumed zero. Ink descriptor C51 at `1800E7850` uses method table `1800E78A0`. Constructor `09E4C0` starts field18 at zero. Insertion `09EE20` chooses field18 for requested ID -1; a successful NEW explicit key K raises field18 to K+1 exactly when oldNext<=K. Registering this NEW root's genuine original IDs therefore yields N, in any insertion order. Context+30.V20 is `08E870`, the direct field18 getter. Text `05F050` reads it from document+40 into the third word of the wrapper tuple.

The counter and C4D transaction are different owners. `05F050` separately creates a C4D.V48 transaction with null prior allocator. Ink `07D9B0` initializes an empty linked command list; it does not initialize the document counter. `05F200` passes that tuple to 2DA.V18=`0283B0`, which forwards its copied tuple to `024D40`/`0210C0`.

In the supported no-prior branch, `0210C0` creates a temporary C22 from the exact ordered range vector, then consumes the counter **before** testing whether creation succeeded. Only successful C4D.V0 registration writes the output ID. C22 `085C90` copies `16*count` bytes when its input transfer byte is zero. C4D `07A5B0` queues operation4 with the consumed ID, name CHAR/WORD/TEXT, a separately copied C22, enabled byte1, extra field38 zero and category100. The public method restores CHAR registrations from actual owned glyph positions. Numeric-only controls exercise the other two native names and four allocator-failure stages.

The public command getter is `Category`, without a `Priority` alias. Native
07A6F5 reads the last category argument and07A6FC stores it at command+40.
The operation4 executor078452 reads that exact field, passes it as the final
argument to C20.VE0, and06BE30 forwards it to C51.Add09EE20, which stores the
actual registry category. It is separate from C20 observer ordering priorities
0x6E/0x78. This naming correction changes no category value, ID or record bytes.

The immutable pending command objects preserve ordered range bytes, including duplicates, modes and fractional position bits. They use portable transaction identities and copy arrays defensively. No managed object is serialized as a native pointer. Repeated calls with identical ranges allocate separate selections: fresh `0210C0` does not intern source keys. The later `024F30` caller decides whether a new key needs a registration at all.

A failed portable owner retains its consumed counter and successful command prefix, then rejects retry. Native allocator recovery is not modeled. Cancellation before mutation leaves both counter and pending list unchanged. There is a portable budget of65536 pending commands and65536 ranges per selection. The pending commands have not been applied to an external document. Prior/cached result lookup, transaction commit/rollback, `024D40` source payload initialization, and the complete `024F30` fresh-versus-reuse driver are separate stages.

## Verification

The independent Python oracle constructs the original-key registry in shuffled order, then predicts1024 registration streams, including identical keys, raw unsigned position bits and failure prefixes. Managed and Windows NativeAOT verify6704 registrations,568 failures, and six registrations from genuine decoded glyph source positions. In those actual cases, inference and source projection run before the counter/range check; no expected labels are supplied. Foreign documents, cancellation, failed-owner reuse and defensive copying are checked.

`tools/handwriting/audit_fresh_text_source_context.py` is an opt-in, read-only audit of the pinned private Text and Ink PEs. It verifies seven code regions, five virtual bindings, counter consumption preceding the null gate, the success-only output store, native name bytes, C4D empty-list initialization and C22 range-copy instructions. It does not load or execute vendor code. Ordinary probe/oracle execution has no private research dependency.
