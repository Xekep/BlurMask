# Fresh library text processing context and selected input DOM

`LegacyLibraryTextProcessingContext.CreateFresh(actualSelectedInput)` constructs
a new owning context/result pair before any selected DOM or result-string
publication. The input must be the existing sealed full selected textField
C20/C22 owner, with its actual category/tag/ranges and original raw document.
Passing the unselected parent is refused. Each context constructs a distinct
result and strings3E; an existing document Seed is never accepted or replaced.

`PrepareInput()` builds a new, explicit **library** BCD view: parameterized
no-history BCC kind4 root, registered `textField` kind3 child, and `input`
child with signed64 `tag` from the actual retained INPUT registry owner.
C98 semantic kind0 and namespace come from the sealed selected input owner,
not from a caller boolean or from the DOM kind number. This is a library
fresh-field creation using the proven generic BCC/name/element operations;
it is not CB5's selected SDK `page/content/layout` clone or an existing SDK
textField. The raw document, retained selected C20/C22, DOM arena and BEE
name pool stay tied by exact reference ownership.

The existing DOM arena gains internal selected-input constructors for this
pre-Seed route. Public `Seed` getters deliberately throw on this new route;
the older Seed-owned arena/name-pool constructors and all older public DOM
preparation methods retain their behavior. The new arena does not obtain a
dummy Seed. Its `Document` is the real selected raw document.

## Restored supported preparation

The stage models the ordinary nonempty-C22/no-prior-command 054340 path:

1. Construct context2C1 and a new result2DA/strings3E. Initial source30 is-1,
   UUID38 is sixteen zero bytes, input ranges210 and textD8 are zero, and
   context state18 is zero/comparison50 is true. Pointer identities remain
   typed objects. Only assigned fields are exposed, not invented full native
   allocation padding or pointer addresses.
2. Construct the actual input-only library BCD described above. There is no
   textResult, prior source or transform subtree in this private producer.
3. Allocate the temporary input-only 2DB snapshot before importing into2DA.
4. Intern the exact35 native 02D8C0 DOM.V18 names in native order, including
   the second occurrence of `line`, then reset/bind the modeled result fields.
   The names belong to BEE, not result strings3E.
5. Read the actual DOM `input.tag`, resolve it to that same selected C20's
   retained C22, and copy its range bytes to2DA208/210. Zero DAF span commands
   never substitute for this nonempty range vector.
6. The temporary 2DB retains the actual imported input owner. Its supported
   050660 D8==0 path returns true before constructing CHAR/WORD/TEXT snapshot
   arrays. There is no injected unsuccessful callback for that unconditional
   native branch. Successful nonzero210 sets context byte18 to1. The temporary
   snapshot releases its borrowed bindings on every entered common tail.

The result retains its exact request and selected input, and exposes copied
range storage and copied immutable tree/value snapshots. `Dispose` releases
those bindings, the owned strings3E and the private DOM tree; it does not
invalidate the borrowed immutable raw/C20/C22 source owners or previously
returned tree/range snapshots. Managed object cleanup and owner-local IDs are
explicit portability adaptations, not native object addresses or a native
allocator implementation. Context operations and live snapshot reads use one
reentrant context gate; this is not a general recognition-thread-safety claim.

## Refusals and failure lifetime

The API supports one fresh input preparation, not reuse/history import.
Preentry cancellation leaves it reusable. Once entered, an exception or
refusal retains the observable prefix, marks the context faulted and refuses
retry. No rollback or public success/configuration flags are supplied.

Friend-only controls exercise four DOM node allocation failures, temporary2DB
allocation refusal, range-copy refusal, and four explicit managed cancellation
checkpoints. Name interning precedes fallible node allocation; failed element
creation retains its name prefix without inventing a node identity. A range
copy refusal retains source30 and bound owners with an empty range vector.
Entered native preparation failures clear state18, and temporary snapshot
release follows its creation even on failure. Managed cancellation also
poisons the scope and clears its incomplete state; those cancellation hooks
are a documented managed adaptation, not discovered vendor callbacks.

Managed constructor allocation exceptions prevent publication of a completed
owner. Native generic object-allocator cleanup is not simulated by public
failure parameters; its default-constructor/retainer/destructor boundaries
are covered separately by the static context-preparation audit.

The new request is not the full049370 worker tuple: it does not invent prior
C8F, history/page receiver or resource-processing cache fields. It ends after
the input-only constructor/import branch. 046B60/0475A0 resource scheduling,
general02D8C0 result import, connecting this child result to subsequent WORD
publication, C20/history application and accepted-document transport remain
explicit later boundaries.

## Independent verification

The probe runs4 actual recognition-owned raw documents and128 additional
fresh library raw roots. It checks132 successful imports,40 refusal/cancel
prefixes and1348 owner/lifetime guards. It never reads expected labels.
The independent Python oracle reconstructs the full DOM nodes, attributes,
native signed64 tag bytes, name-intern prefix, packed C22 range bytes,
result bindings, state flags and temporary release order from raw point
counts and native contracts. The static auditor independently decodes the35
name calls from pinned PE instructions and checks the INPUT child append,
signed64 tag writer and empty-TEXT snapshot branch.

Managed and fresh win-x64 NativeAOT must produce identical complete captures.
The verified complete capture's stable SHA256 is
`812af51916cd9476aa7361577428a6c9453a6cbab8a8f915cdffd50a83d10f22`.
Older arena routes are checked against their retained captures:

- Detached DOM: `657c0da12cdd06a4d86baccc460ec28fce79d418dfe8ebe8aac7aef8fe46ddc6`.
- Target/BE2 export including deterministic serialization barriers:
  `b75b292092251b9e6ffd321322c0d126c3c2098fe5cb117136e9d9f2d8125283`.

```powershell
$env:TEMP='E:/BlurMask-build-artifacts-20261014/native-build-temp'
$env:TMP=$env:TEMP
dotnet publish tools/LegacyTextProcessingContext.Probe/LegacyTextProcessingContext.Probe.csproj -c Release -r win-x64 -p:PublishAot=true -p:UseSharedCompilation=false --artifacts-path E:/BlurMask-build-artifacts-20261014/processing-context-native-build -o E:/BlurMask-build-artifacts-20261014/processing-context-aot
python tools/LegacyTextProcessingContext.Probe/run.py E:/BlurMask-build-artifacts-20261014/processing-context-native-build/bin/LegacyTextProcessingContext.Probe/release_win-x64/LegacyTextProcessingContext.Probe.dll E:/BlurMask-build-artifacts-20261014/processing-context-managed-final.json
python tools/LegacyTextProcessingContext.Probe/run.py E:/BlurMask-build-artifacts-20261014/processing-context-aot/LegacyTextProcessingContext.Probe.exe E:/BlurMask-build-artifacts-20261014/processing-context-native.json
python tools/handwriting/audit_library_text_processing_input.py E:/BlurMask-build-artifacts-20261014/library-text-processing-input-static.json
```

All binaries and Ghidra evidence are read statically. No vendor DLL is loaded
or executed by this implementation, its controls or its audits.
