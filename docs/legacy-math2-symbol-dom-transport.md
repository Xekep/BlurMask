# Math symbol DOM transport boundary

This static checkpoint follows the concrete027E20/028070 helpers and their
02AF50 caller. It is separate from the completed168-byte decoder records,
EE4 unit formatting and the library's ordinary rich-record output. Similar
record sizes do not establish that the same producer or owner is involved.

## Exact caller inputs

02AF50 retains a separate output vector at local[RBP-1] and a freeze/source
vector at[RBP-11]. At02B28D..02B2A8 it supplies the former as028070's fourth
argument and the latter as its sixth argument. C23 input selection, existing
mathResult extraction and029290 processing precede that call. The complete
raw-only first-result and single-source outer-vector owning joins are now
implemented separately. The bounded library original field/C23 source is now
owned separately; CB8 publication and the original receiver remain unjoined.

028070 iterates its fourth argument using an exact40-byte stride:
02834D computes index*5,028356 multiplies that by8, and028374 calls027E20.
The separate freeze vector has32-byte rows and a flag at1C. It must not be
confused with the40-byte symbol rows, the168-byte hypotheses, the4-byte units,
or per-unit alternative rows merely because some contain similar pointers.

027E20's symbol row supplies range pointer/count at0/8 and alternative
pointer/count at10/18. Its ordinary content uses the first alternative's first
qword when that alternative vector is nonempty. It does not concatenate all
alternatives, add joining spaces, or interpret a candidate score as confidence.

## Source ranges and tags remain live operations

For a nonempty range,027E20 requests the native range serialization service,
sets the symbol's `range`, and invokes the actual CB8.V28 with `MATH_SYMBOL`
and that same range. The returned value is negated and assigned to **`tag`**,
not an invented INPUT ID or a result UUID. This tag allocation call precedes
the earlier range-success test; failure-prefix accounting must preserve that
ordering rather than claiming the operation rolled back all counter changes.

Freeze metadata is checked using the actual range-comparison service and the
separate32-byte rows. The first equal row is decisive: its nonzero flag1C sets
the symbol's `freeze`; a later true match cannot override an earlier false one.
Text-node construction must succeed or the new symbol is destroyed. The append
return is ignored by the native helper, so it does not provide an additional
success guarantee. These details are pinned in the
[complete schema proof](legacy-math2-dom-schema-clone.md).

These operations require the actual selected input/range source, CB8 naming
state and original field context. The named library raw-only source now retains
original IDs, fractional processed positions and C21 range unions. Its first
and outer copies preserve those ranges, but do not create the actual native
CB8 naming owner or prove applicability to an SDK page. The bounded original
Math field/C23 owner is described in `legacy-math2-owned-original-field-source.md`.

## Result publication is later than string output

028070 builds a new parameterized BCC using byte0=0 and the original name-owner
argument. It clones the source field and establishes a mathResult through a
real CB8. It emits sequence/symbol nodes, queries DAF transforms, and creates
symbolSegmentList/symbolSegment/symbolCandidate/char plus mathTerminalNode
metadata. The pinned literals include `nrs`, `name`, `ref` and `tag`; substituting
guessed attribute names would change the schema.

The final order is CB8.V10 patch construction, temporary BCC.V228 registration,
CB8.V40, V48(false), V50, then release of that registration. Only afterward does
the original DOM.V208 produce its edit, the original Ink.V178 produce its edit,
and the supplied pending receiver.V18 receive both. Earlier failures skip this
final handoff. The new BCC and temporary mathResult are cleaned up on exit.

Those original edit receivers remain distinct from a detached library target.
This checkpoint does not claim native history acceptance, final formula layout,
an accepted transcription, or a joined restored-engine application route.

## Verification

`audit_math2_symbol_dom_transport.py` checks both complete vendor DLL hashes,
three complete helper/caller regions,40 exact instructions,29 registered
CB8/BCC table bindings and8 literal byte strings. It never loads a vendor DLL.
The focused Ghidra export, references and SHA provenance are preserved locally
under `research/myscript/output/math2-result-transport-20261018/`, outside build
artifacts and excluded from Git/releases.

The checked runtime source-bound symbol-row producer and its independent copies
are described in `legacy-math2-owned-source-positions.md`,
`legacy-math2-first-result-copy.md` and `legacy-math2-owned-outer-symbols.md`.
The next runtime join needs the genuine CB8 owner over the existing field/C23 source. Its temporary
tag/late registration contract is pinned in `legacy-math2-cb8-tag-owner.md`.
A DOM facade without those owners would still leave required algorithms missing.
