# Ordinary word-category policy

`LegacyOrdinaryWordCategoryPolicy` implements the checked category choice in
Text `180204290`, immediately before ordinary word evaluation. It consumes
caller-resolved candidate metadata and the sampler297 derived mapping. It does
not turn a neural-network class/slot into a word category or infer special labels.

Static instruction evidence at `180204ace..180204b36`:

* candidate byte at metadata `+5e` is tested against zero;
* candidate ushort label `+5c` is compared **unsigned** against the frontend
  limit; nonzero increment and label below that limit use derived `+50` start map;
* otherwise beam record `+1a & 0x3f` supplies the composite index;
* derived `+70` converts that index to the first-list category ID;
* only after that lookup, equality with runtime ushort `+24f4` or a previously
  resolved condition `local307` overrides the category with `1`;
* candidate `+5f & 7` is a separate metadata flag passed alongside the category.

The derived mapping and resource bridge are documented in
[the category mapper](legacy-russian-cursive-category-mapper.md). Both Russian
profiles contain 35 composites and 195 symbol map entries. The frontend limit,
special label and `local307` remain explicit: their complete producer semantics
are not restored here. Missing mapping `-1`, out-of-bounds composite and selected
out-of-bounds symbol are rejected even when an override would follow. The native
code dereferences the mapping first; silently accepting those cases would hide
an unsupported branch.

An additional pure helper implements the ordinary predecessor path-cost
correction visible in `180204290`: when runtime `+22f6` is zero and
`knowledgeFlags & 0x1c > 7`, previous mapped category `2` or `4` adds `8192` if its
group begins at node zero or the caller-resolved physical-owner difference is
nonzero. The helper takes the resolved previous group and physical difference;
it does not traverse beam links or manufacture missing ownership data. Callers
must not apply this twice if their supplied path cost already includes it.

`tools/LegacyWordCategory.Probe` uses a vendor-free synthetic serialized profile
with distinct start/end and first-category maps plus a missing entry. It checks
all nonzero increment classes, zero/equality fallback, flag masking, override
ordering, explicit predecessor conditions and cancellation. An optional acoustic
resource path checks both actual Russian profiles against their stored maps.
Managed and Native AOT results establish this policy's implementation, not word
or cursive recognition quality. The automatic raw-ink word search still needs
the native candidate graph, candidate metadata producers and context selection.
