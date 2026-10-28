# Adopted document, DOM and transport join

`LegacyLibraryTextPublicationDocument.Prepare` accepts the actual published
shared-WORD result of one live `LegacyLibraryTextPublicationScope`. It checks
the exact Seed, processing-result2DA, raw document and pending selection owner.
A source-result lease prevents a second document invocation over that same
publication. No prepared flags, expected labels or caller command lists supply
the document data.

`BuildAndTransfer` executes the already verified bounded stages in native
order: publish source lines; prepare/register TEXT; line consistency; source
and character coverage; selected sources; source indices; control cleanup;
input transforms; pending counter handoff; range UTF8; detached text DOM;
V50 BE2 export; transfer the actual pending C4D/edit into a new library cache.
It uses the same pre-neural imported result, Seed identifier, strings3E and
selected INPUT/C22. Supported-source restrictions in every producer remain
active; unsupported rows are never omitted to produce a partial accepted text.

The cache transfer retains actual BE2/C4D sidecars with versions -1/-1 in its
40-byte row. It leaves the private target root empty. The separate
`ApplyToLibraryTarget` clones the actual exported edit into that library root.
It does not execute pending commands, instantiate a native C8F history receiver
or accept C20. No SDK page, request history or native049370 scheduler is claimed.

## Ownership and prefixes

The document owner borrows the live publication scope. Scope disposal
explicitly invalidates all live public document operations. The document owner
does not keep a closed scope alive through a second lease and cannot revive it.
Document disposal releases only its own transport/cache leases; it does not
dispose the scope or mutate the borrowed source commands. Public results are
detached immutable snapshots. The live target, edit, handoff and cache are
private, so their direct APIs cannot bypass the scope guard. As in the scoped
pipeline, downstream operations must not race scope disposal.

Before-entry cancellation leaves the invocation reusable. After entering,
source registrations and native failure prefixes remain observable. DOM or
export refusal poisons the document invocation without applying the target.
A failed cache reserve keeps the same actual receipt's incoming edit/C4D
slots; `TransferPending` can retry that reserve. It cannot rerun document
processing, manufacture a second transaction or transfer an already consumed
receipt. An entered target clone failure poisons the target and document owner
and preserves the native partial prefix. It does not roll back changes.

The snapshot retains raw scalar BE2/cache rows, command identities, operation
codes, selection ranges and counter bodies. Pointer lanes remain zero with
actual typed sidecars held privately until logical release. Snapshots retain
the historical tree before/after application and remain valid after both owners
close. The environmental library UUID is copied from the exact adopted result.

## Verification

The isolated probe runs four genuine configured raw inputs, including reversed
physical order and rotated ink. Its independent oracle constructs full trees
from raw CHAR40/WORD48/ref24/TEXT32/line56 values, verifies all allocation and
name-pool identities, and reconstructs operation4/operation7 command order and
all BE2/cache40 bytes. It checks the separately cloned target arena and prior
immutable snapshots. Model inference never reads expected text.

Five additional genuine invocations cover native DOM refusal, native export
refusal, retained reserve failure/retry, target clone failure and scope closure
before apply. Guards include foreign words, repeated preparation/processing,
pre-entry cancellation, repeated transfer/application and disposal. Managed
and NativeAOT captures use the same independent oracle; UUID bytes are checked
within each tree before only that environmental value is normalized for parity.

`audit_text_publication_document.py` rechecks six native prerequisite audits.
Its 16 ordered-call/eight lifetime checks are explicitly portable source checks,
not a claim that this library wrapper is the native scheduler or history owner.
