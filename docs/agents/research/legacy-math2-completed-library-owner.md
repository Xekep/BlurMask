# Completed Math2 Library3E construction

Every successful034EA0 completed-record slot owns a separate type3E instance at88.
It is not a null placeholder or a shared output-label array. This static audit
closes the standard constructor dispatch before a future owned recorder is added.
It adds no runtime and executes no vendor DLL.

`audit_math2_completed_library_owner.py` checks six exact Engine PE regions,
46 instruction pins and thirteen data/dispatch bindings. The expected Engine SHA256 is
d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c.

## Concrete standard dispatch

Engine034010 installs its default context, then obtains the type service from
pointer252038 -> descriptor1DB8F0 -> field20 table1DB830. Its slot88 is038250.
The completed-record caller resolves registered type3E through that service,
creates it withV88, then retains/adopts the result through context18.V28.
The latter binds to03A2D0 in default table1DBBF0. It calls03A100 and returns
the identical input on success. On retention failure it releases the input
through type-serviceV90, then returns null. This differs from the V20 retain
used by EEF decoder ownership: both use the reference-count helper established
in the [force-policy audit](legacy-math2-force-policy-producer.md), but V28 also
handles failure cleanup. Neither operation clones the instance.

038250 calls038E70 with copy=false and separate output object/constructor pointers.
038E70 checks that the resolved descriptor has class kind3. Descriptor18 identifies
the primary table; primary10 identifies a constructor table. It requires the
default constructor at that table's0 slot. Primary0 supplies allocation size;
primary8 is installed as the new object's first pointer before invoking the
constructor. A custom pool allocator follows a separate branch and is outside
this ordinary allocation scope. Registered-class availability is required;
a missing class or constructor produces failure rather than a dummy object.

The actual type3E descriptor at23CF80 binds primary23D000. Primary0=02EA80 returns
24 bytes. Primary10 points to23D038, whose default constructor is0312C0.
These bindings identify the actual class rather than guessing a constructor from
the record field name.

## Empty library instance and failure

0312C0 first calls the base4 default constructor. After success it separately
allocates72 bytes through the standard allocationV0, stores that pointer at
instance10, and explicitly clears the pool's semantic fields. Qwords0/8 and18..40
and DWORD10 become zero. Padding14..17 is not explicitly written. This helper is
not evidence that arbitrary allocation residues or native pointers equal a fresh
managed byte array.

If the base constructor fails,0312C0 returns false. If the72-byte allocation fails,
it invokes the base4 destructor and returns false. The generic038250 failure path
releases the unfinished instance using its registered allocator or the standard
free service. It does not publish a successful Library3E or increment the
completed vector count. Only a successful child creation and retention allows the
[completed-record constructor](legacy-math2-completed-record-ownership.md) to
advance that count. Later record-content failures have their different retained
partial-record behavior.

A future managed recorder can represent this ownership with a distinct typed
empty library object, preserving object identity and fallible construction as
explicit logical events. Managed allocation, pointer bytes, the native registry
and pool allocation timing remain adaptations. Library insertion/hash/copy
operations, full037430 recording, maximum-step fallback, output construction and
final formula acceptance remain separate work. This proof does not implement a
whole decoder or alter recognition defaults.
