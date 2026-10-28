# EF4 byte-one source ownership and coordinate planes

This audit establishes the native byte-one call route and its distinct tensor,
unit and mask coordinates. It does **not** establish that a fresh EEB request
always has an empty existing-source sequence. No runtime is changed.

## Existing-source producer

EED `047C50` initializes the 16-byte vector at `rbp-80` to zero at
`04803A..048047`, then calls `046F40` at `04805B`. The latter scans the
selected-source item vector (`+10` data, `+18` count). Only an item with
kind `4` and runtime type `C25` enters the `C38` existing-expression producer.
If no such item is found, `046FB6` returns true without appending anything;
the caller's vector therefore remains empty. A matching item can populate
units through the general `C38` path, including repeated recognized glyph
units and placeholders. Success is not an unconditional empty-source result.

The same vector then travels through the Windows x64 ABI:

| Caller | Actual handoff |
| --- | --- |
| EED `04815A/048163/048168` | `&existingVector` in outgoing stack `+20`, EEE `03A260` argument 5 |
| EEE `03A2DE/03A2F2` | Incoming argument 5 saved at `rbp-48` |
| EEE `03A76B/03A774/03A789` and `03A90A/03A90E/03A920` | Both driver callsites forward it as `038610` argument 7 |
| Driver `039BC7/039BE7` | Forwarded as `036F20` argument 3 |
| `036F7B`, then `03734E/037352/037364` | Saved unchanged and forwarded as EF4 argument 7 |

The existing request proof independently establishes correction byte `1` for
a successful fresh **unmodified** EEB caller and cold EED child. It says
nothing about the selected document source's item types. C66 feature tensors
and `MathPreprocessor.Process(InkStroke[])` are separate producers; neither
proves the native source selection lacks C25. That native owning join remains
unresolved. A future library-owned raw-only source descriptor can prove its
own exclusion of existing expression objects, but must be labelled as that
portable entry rather than as a native EEB default.

## Input identities

EF4 `042100` receives these separate inputs:

| Input | Meaning |
| --- | --- |
| Argument 2 / XMM1 | Scalar segmentation-score factor from EEE `+54` |
| Argument 3 / R8W | Replacement label from EEE `+20` |
| Argument 4 / R9 | Original activated F32 tensor `[oldPrimaryCount, segmentCount]` |
| Argument 5 / stack `+20` | Actual mutable completed168 record |
| Argument 6 / stack `+28` | Same EF3 owner at EEE `+68` |
| Argument 7 / stack `+30` | Source-owned existing four-byte-unit vector |
| Argument 8 / stack `+38` | Forwarded opaque scalar; retain separately, do not substitute stroke ends |
| Argument 9 / stack `+40` | Correction request byte, `1` in the restricted fresh EEB proof |
| Argument 10 / stack `+48` | Actual `strk_ends` tensor found in the original input packet, or explicit null |

`036F20` reconstructs the actual old prefix attention and activates it through
`036C70`. Its current EOS frame is excluded. A typed historical source from the
same completed record can supply that matrix; a caller-provided same-shape
array cannot replace the source identity. The frame count/scalar and stroke
ends must remain bound to the original raw input as well.

The source alphabet has 299 labels, while the serialized decoder count is 297.
For this resource, `[ink]` and U+FFFD are absent. The v103 reader stores the
serialized U16 count in EED `1E2`; cold preparation consumes the missing
`[ink]`, raises the counter to at least the alphabet count, then supplies
synthetic replacement `299` for missing U+FFFD. `04534E/045355` places that
replacement into the packet, and `035ADA/035ADE` copies it to EEE `+20`.
It is **not** the separator marker. `0428D0` reads the latter from EF3 `+0A`,
whose actual source-AL header field is `FFFF`.

## Byte-one empty-source coordinates

For a proven empty existing-source vector, `042477` selects the branch that
calls `041020` with a null source and final argument false. Before that branch:

1. The original activated tensor remains at `rbp-38`.
2. EF3 filters observable original primary rows into the local tensor at
   `rbp+F0`; a separate U32 vector at `rsp+68` stores each copied row's original
   primary index. Filtering preserves order.
3. Completed `50/58` is initialized with all original four-byte units. The low
   U16 is the primary label, byte2 is zero and byte3 is one.
4. `041020` computes per-segment column argmax over the observable copy and
   translates the winning row through the original-row vector. Equal finite
   values keep the first winning original position.

`041240` creates the INITIAL insertion mask at `rbp-68` by calling `0401B0`
with the units count in R9D. The helper ignores its incoming R8 payload and
appends zero bytes; this is an explicit memset, not an allocator zero-fill
assumption. Only newly inserted placeholder positions get mask byte `1` at
`0415EC`. The inserted replacement has byte2 zero and byte3 one.

The following phases retain different coordinates:

| Plane | Native role |
| --- | --- |
| Original activated tensor | Original primary rows; not replaced by observable or retained-unit copies |
| Observable tensor + original-row map | Assignment only |
| `rsp+58` assignments | Initially original-unit indices; `041660` remaps them to retained-unit indices |
| INITIAL insertion mask `rbp-68` | Zero originals / one inserted; passed to the final score tail |
| Deletion mask `rbp-58` | Separate output of `041660`; prunes units and auxiliary lanes |

At `0428AC..0428BB`, a nonzero deletion byte leads to testing INITIAL mask;
`CMP 0; JNE 04291D` rejects a nonzero INITIAL byte. `04291D` clears the success
byte. Thus inserted positions are protected. Zero-mask original positions are
allowed to be deleted by this guard. An earlier verbal inversion was caught
before any owning runtime used it.

After separator normalization and run compression, `0428EF` still passes the
ORIGINAL activated tensor to `041CF0`, while `0428FD` passes the INITIAL mask.
The assignment indices have already been remapped by `041660`. The native
tail does not substitute the deletion mask or rebase the original matrix by
the retained-unit map. This can be counterintuitive and must be preserved.
For example, if unused observable original unit0 is removed and original
unit1 becomes retained unit0, a compact assignment0 indexes original matrix
row0 under an all-zero INITIAL mask. Replacing that read with original row1
changes native behavior. The existing prepared score helper is callable only
when its matrix/mask/assignment preconditions match these actual coordinates.

## Reproducible static evidence and next join

`audit_math2_ef4_empty_source_coordinates.py` pins 15 complete native regions,
173 instructions and two table bindings, and composes the existing cold-byte
and EF3/resource proofs. Its output contains 256 explicitly prepared finite
coordinate cases. Each segment is a one-segment stroke, so the distinct
stroke-boundary suppression branch is absent in those controls.
`audit_math2_ef4_coordinate_controls.py` independently derives column winners,
unused-unit removal, remapped assignments, literal original-tensor score
lookups, insertion-preservation truth cases and source-scan cases. These are
data-only controls, not evidence that native EEB selected a C25-free source or
that EF4 processed actual output successfully.

The next safe owning entry should bind an explicit library-owned raw-only
source to the SAME cold loop, original stroke-end owner, completed-record
Source, historical attention, EF3/resource and mutable unit/assignment owner.
It must refuse imported/warm/existing-C25/custom-policy inputs, not assume
they are empty. Validate all tensor shapes and an explicit total payload
budget before allocation, retain native phase order and failure prefixes,
and poison the owner after consuming failure. Initial sort is already a
distinct earlier state; final sort/output remain later states.

No new native failure-order equivalence, full EF4 pipeline, completed formula,
acceptance, app behavior or recognition-quality claim is made by this audit.
