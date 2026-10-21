# Actual English sparse2D2 and fresh numeric binding boundary

This is a static-only dependency closure after the separately reviewed packed
English2B4/v102 data. It is not an English configured language controller,
word-query implementation, mask publication or handwriting-quality result.
No vendor binary was loaded or executed.

The actual raw `en-US/en_US-lk-text.lite.res` is3300812 bytes, SHA256
`9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9`.
Main13B/v300 `[184,2832716)` owns language2D2/v101
`[1252412,2832716)`. English2D2 inherits2CD/v102, not the Russian
2D1/v100 dense path. Object descriptor2BACA0 names parent2CD; its object table
2BACF0 binds reader104450, reorder1036A0, binding105CA0, V58word-record107A40,
V60108B50 and V70109A40. Addresses here are image-relative VA suffixes under
MyScriptText.dll image base180000000.

## Exact raw partition

`reference_actual.py` consumes all75 field regions exactly, without
header/version rewriting. Native104450 calls the2CD reader0F0310, then reads
maxOrder3, flags7, wordSlots260075, classSlots1024 and order lengths
`[1024,113643,155484]`. Its actual nonemptyD0/E0 codebooks retain effective
order3. Inherited64 is260074;68 and6C areFFFFFFFF.

The word-to-class88 packed vector has260075 entries/10bits. B0 order2/3
labels have113643/155484 entries/10bits. B8 offsets have1025/113644
entries and17/18bits. The reference independently decodes byte windows and
32-bit crossing, reconstructs every storage byte, and verifies all643871
packed values. All114667 sparse parent intervals are monotone, consume their
whole next-order label array, and have strictly increasing labels per row.
Maximum degree is664/order2 and1022/order3. Order3 has73805 empty rows.
Raw packed payload storage is919388 bytes.

Code indirection is independently bounded: word code bytes260075 point into
A8/256; each C0 byte maps into its own D8/256; C8 maps into E8/256 for the two
lower orders. C0 lengths are1024/113643/155484; C8 lengths1024/113643. There is
no serialized80,90 or26A0 branch in flags7. Absence of90 excludes alternate
binding-array allocation for this exact owner. No synthetic sparse→dense
conversion is justified.

The special inherited64 index is within word slots and maps through88 to
class0; its raw word code is0. It is not another ordinary dictionary terminal:
the packed dictionary has260074 terminals. 1036A0 excludes inherited64 from
its ordinary permutation when it is notFFFFFFFF. 107A40 with request byte0=1
inserts this initial class into history. Its local114 `currentWordID+1` is the
exclusive loop bound; output record1C receives the unshifted current word ID.
This package does not implement that query/history transition.

## Fresh selected-main numeric binding

092990 constructs a12-byte packet whose byte0 is literally0, byte1/2 come
from its caller, and the two32-bit fields are0. It obtains source kind by the
actual frontend V58 and scale from selected139+6C, then calls selected
language V48. Actual owning English acoustic247/v110 has source kind135 and
main13B scale1, so105CA0 chooses profile `137-135=2`.

This closure proves numerical table preparation independent of packet bytes1/2;
it does not choose their configured policy or publish their sidecar flags.
For the actual owner the F0 profile2 subtraction offsets are three binary32
1.458 values and130 multipliers three binary32 .226 values. Word costs instead
subtract profile110[2]=0. Boundary profile120[2] is binary32-1.574. Each scalar
operation is rounded to binary32 in native SSE order before truncation.
A8/256, D8/3×256 and E8/2×256 compile1536 integer costs. Native backoff sentinel
preservation is conditional on packet.byte0!=0, so this actual0 path transforms
raw0x04000000 normally. Boundary94C is12894 and finalUnigram94D is115101.
Raw values and compiled digests are in the independent reference capture.

A critical constructor default survives serialization: flag8 is clear, but
103020 sets268C/2690/2694/2698 toFC000000 at10317E,10318A,103194,10319E.
105CA0 negates the selected slot into compiled context00, giving04000000.
Absent wire fields must not be replaced with zero.

Case path150 is0200. Selected154[2] is binary32 .1 and164[2] binary32 .15.
Native106D40..106D6F performs multiply/add/×8192, adds or subtracts .5 according
to sign, truncates, negates and duplicates the2D result into triple history0.
The actual loops are12×12 and12×12×12 for categories1..12 with13-stride indexing,
not full169/2197 blanket conversions. The reference checks1442D and1728triple
entries, plus explicit[0,0]=0/[0,0,0]=0. Other zero-border slots are outside this
proven domain; a future typed accessor should refuse them rather than invent
zero values. The actual 2D source begins174; triple source418. The destructor
written to the output callback105AC0 is a deallocator, not a scoring function.

## Configured selection and masks remain distinct

Actual13F contains13D, main13B and19grammar13A serialized children. Registration
1EAEB0 appends each supplied27B-derived object to its configured list in the
order received, treats13D separately, and increments revision38. Builder1EB340
rebuilds a controller from that configured collection, removes previous
children, adds current children and stamps its revision. Serialized inventory
alone does not establish which caller supplied which sources, their active
priority or epoch. A main13B-only independent configuration would be a declared
library policy, not proof that the native full-root default selected only it.

092990 constructs native source maps after successful language V48 binding.
Exact lexical104→acoustic217 byte/text matching has18 unmatched accented
entries (the earlier packed-data reference verifies this whole map). Fresh
2B7 constructor092790 initializes2204 normalization fallback to0. The fallback
branch in092990 requires both failed exact lookupFFFF and nonzero2204. No
policy owner enabling it has been proved. The current closure therefore does
not normalize those18 entries or reuse the Russian all-exact139 map/masks.
Initial source-map allocation, shared-context association, case decorators,
selected priority and dirty/pending epoch publication still need their own
owned producer/consumer receipt. The selected source's native268C/default and
sparse history must survive that future join.

## Native proof and independent evidence

`english-lk-sparse101-pins.json` records16 complete Ghidra getBody functions,
16 regions,315 call instruction bindings,5 descriptor/table pins,3 bounded
through-RET leaves and the bounded2B7 constructor with its internal branch. The full-body provenance TSV is frozen as evidence; regions
are not inferred from first .pdata/unwind fragments. The constructor and3 leaves honestly have
bounded-code scope rather than an invented Ghidra function claim. Both the full
DLL SHA256 and each body/pin are checked by the portable auditor. No Ghidra,
research directory or E-drive path is needed to run the portable scripts.

Run the raw reference with `reference_actual.py MODELS OUTPUT` and the
native auditor with `audit_english_lk_sparse101.py --models MODELS --sdk SDK
--output OUTPUT`. Native checks are omitted explicitly if noSDK is supplied.
These are static resource/numerical proofs; they are not measurements of English
or Russian handwriting accuracy or completion of the recognition engine.
