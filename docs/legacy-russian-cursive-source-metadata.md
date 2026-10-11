# Cursive span profile and source metadata bridge

`LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode` reads the checked span prefix of a complete, uncompressed Text type21b/version10a node. It validates magic, declared length, type, compression flag and version, and retains its two initial bytes without assigning meaning to the second byte. Other native geometry/token policies are deliberately unparsed.

The actual Russian acoustic resource has this node at **offset189379**, length355 including its8-byte prefix. Its maximum atom span is **8**, and its second byte is1. Native reader `1ad9d0` reads the first two bytes into runtime offsets+8/+9, then72 opaque four-byte table entries and the version-dependent policy fields. `1a61c0` passes runtime+8 into `19c280`, which stores it in each local graph's byte+4. `1a3b30` requires `leftSpan + rightNode - leftNode < graphCap`, so span8 allows groups of at most8 atoms. This model value can be passed explicitly to the restored ordinary-chain lattice; that endpoint still applies its own1..15 safety bound.

## Optional physical-boundary constraints

The source service implements interface218 at table1802df340. Virtualf0 targets **169240** and returns a sign-extended short from source secondary+58, record stride0x1c, offset+18. Virtualf8 targets169260: it searches these records for a special token ID stored at+4 and returns the same signed short. This is optional source metadata, not a physical stroke ordinal or an inferred glyph category.

Initializer **163970** sets Text219 runtime+a8 to minus one. Writer **167210** copies runtime+a8 into each physical record+18. Normal ink wrappers167bf0/167c40/167c90 do not set this metadata. Extended writer167ce0 accepts it explicitly, then resets runtime+a4..ab to minus one;166dc0 similarly wraps the other ink ingestion path. Therefore ordinary input added without explicit native boundary metadata has **all boundary IDs -1**. The lattice can accept these unknown IDs without inventing barriers between physical strokes. Extended/custom ingestion must preserve the supplied metadata.

## Chronological point-key bridge

Interface218 virtual90 targets **168d00**. It resolves a point's global prepared index using source secondary+28 coordinates and+38 stroke-end counts. If optional resampling maps exist, it maps through+118 and ratio+128, including ceiling/remainder handling. It then reads secondary+d8 point-key index map and returns a twelve-byte record at secondary+f8.

`167210` fills these triples with:

* first field: signed runtime219+8c, minus one if nonzero, otherwise zero;
* second field: runtime219+90, initialized to zero by163970 and incremented once per ingested stroke at167bc2;
* third field: the original point index from its explicit sampling map, or zero in the opaque-source branch.

The source also stores the companion end-point metadata at+108, whose last field uses the next mapped original point minus one. The ordinary unsampled ingestion path consequently preserves chronology as `(0, input stroke ordinal, original point index)`. Prepared insertions repeat their preceding original-point identity. The optional mapped/resampled ingestion branch remains upstream and must not be silently replaced with prepared physical indices. These facts support the range-key normalization agent's explicit metadata sorting; no additional point-key API is introduced here.

The span probe passes managed and Windows NativeAOT executions using the actual Russian resource, all255 nonzero byte caps, malformed/truncated resource nodes and the model-cap-to-ordinary-lattice bridge. It does not validate the unparsed policy tables or whole recognition accuracy.
