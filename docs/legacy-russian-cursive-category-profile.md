# Sampler297 discrete category profile

`LegacyCursiveCategoryProfile` reads the old optional structure stored at sampler state+40. This structure is not a neural network. Its reader is native `180010d80`; its composite lookup builder is `1800103e0`.

The two version106 sampler297 nodes in `models/ru-RU/ru_RU-ak-cur.lite.res` contain profile version201 at offsets11571 and188222. The following ushort is192, and each mapping has192+3 entries. Both structures have seven first-list IDs `(0,2,4,3,1,6,5)`, nineteen geometry-list IDs, three signed-byte maps and nineteen geometry prototypes. Every prototype stores three floats and one flag. The prototype values are identical in both resources; original stream alignment makes their encoded lengths942 and943 bytes.

The independent reader accepts only the confirmed version201 format. It aligns the two ID lists to four bytes relative to the original resource stream, preserves list order, and bounds counts, table references, prototype values, allocations and consumed bytes. It owns all parsed data. Unrelated following sampler configuration is left unread. Older formats with implicit prototypes and newer formats remain unsupported.

The native composite lookup uses entry-major encounter order. It interns four pairs per entry: first-map/start-geometry, first-map/end-geometry, zero-ID/start-geometry, zero-ID/end-geometry. A missing signed-byte entry−1 produces a missing composite−1. The zero-ID category is found by scanning the first ID list, as confirmed by the disassembly at native11306–11320. Composite records retain the first-list ID and geometry-list index. No semantic name is invented for a category.

Native `180195b30` consumes geometry prototypes in its line-relative pair cost, and `180195520` repeats candidate costs across the geometry category count. This component provides their exact data tables; those consumers and the full lattice scorer are separate work. Its data alone cannot segment or recognize Russian cursive.

`tools/LegacyCursiveCategory.Probe/reference_controls.py` creates256 independent profiles spanning all four original-stream alignment offsets, nontrivial maps and prototypes. The console probe checks mapping encounter order, owned buffers, bounded consumed data, prototype values, truncation and cancellation. An optional second argument checks both real Russian `.res` profiles without loading any proprietary native DLL. Managed and Windows NativeAOT runs are required before marking the reader ready.
