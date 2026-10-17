# Registered text request and processing-context ownership

This is a static caller/lifetime checkpoint. It does not execute vendor code,
bind the library document seed to an SDK request, or establish document
acceptance. The registered C9E route below is distinct from the previously
examined `Text!05F720` body.

## Registered route

1. Document C8E has descriptor `1B6420`, object table `1B6330`, processing
   entry `V28=00DBA0`, and `V40=00E310` forwarding to that entry. Its worker
   `011F90` uses `0116B0` to choose a processor. The field-kind-zero/default
   processor branch creates F03 (`011890`). Other field kinds select other
   descriptors; this is not a claim that every resource enters F03/130.
2. F03 descriptor `1FD210` exposes C9E through interface-query `04DC20` and
   table `1FD2E0`. `V0=022840` and `V8=0228C0` forward to the implementation
   retained at F03 `+18`. Its interface 98 uses table `1FD258`, whose
   resource loader is `04D700`.
3. In the supported loader branch, `04D700` writes mode 1 at `04D874` and
   creates descriptor 130 at `04D87B`. The EED/13E compatibility and existing
   implementation branches remain real conditions. This checkpoint does not
   prove that an arbitrary supplied acoustic model selects that branch.
4. Text 130 descriptor `2763A0` exposes C9E through `04A630` and table
   `276370`: `V0=049370`, `V8=0479A0`. The latter creates descriptor 2C1.
   Interface-query tables are distinguished from object tables; bytes after
   an interface-only object's descriptor are not presumed method slots.

## Actual worker request

`Document!00D540` obtains the original context through its reader `V200`,
then actual source/selection through context `V78`. It invokes source `V170`
twice and retains the selected C20s at state `+60` and `+78`. `00C590` creates
the corresponding selected DOM views retained at state `+68` and `+80`.
Those are selected clones, not an assertion of an empty DOM or original-ID
registration policy.

Before dispatch, worker `0126DF` tests state `+C0`. When absent, it queries
the selected implementation's C9E and invokes `V8` at `01271A`, retains the
returned context, and stores it at `012728`. Reset conditions can first
release and clear a prior context. The tuple assembled at `RBP-80` is:

| Offset | Producer and meaning |
| --- | --- |
| `00` | state `+80`: actual selected BCD |
| `08` | state `+78`: actual selected Ink C20 |
| `10` | state `+C0`: retained processing context, created through C9E.V8 |
| `18` | caller's previous-cache slot |
| `20` | caller's current C8F slot |

`0129BC` passes this tuple to the selected C9E `V0` call at `0129CF`.
For the F03/130 branch this reaches `Text!049370`. The original page/store,
the selected BCD/C20, and the processing context have distinct owners.

## 049370 and the conditional CBD selector

`0493AA` captures incoming request `+10`, preserving it at `RBP-58`.
After the field/input preparation checks, `0495F7/0495FB` skips CBD selection
when that incoming context is nonnull. Thus the worker's precreated 2C1 is
the normal context supplied by the route above.

When the incoming context is null, 130 lazily creates a Document CBD at its
own `+10`, calls CBD `V10=020B00`, and stores the result into request `+10`
at `049681`. CBD is a context selector, not 2DA and not the pending C4D
transaction:

- It queries the recognizer's C9E and derives the actual DOM field name.
- With no prior object, it clears retained state through `020A60`, calls
  recognizer C9E `V8` (`020B99`), retains the result in CBD `+18`, and returns
  that context.
- With a prior object, its reuse checks include the retained name and exact
  BCD identity (`020BE2`). New selection retains name `+20`, BCD `+28`,
  context `+18`, and a C23 mapping `+30`; C31/mapping failures are observable.
- `020A60` releases BCD `+28`, context `+18`, name `+20`, and mapping `+30`,
  clearing the corresponding slots. There is no constructor-only empty
  history inference.

`049695/0496A5` retains request `+10` for the invocation. Cleanup releases
that local context at `049D95`. If the request entered with a null context,
`049D75..049D7B` clears request `+10` before local release, even on the
ordinary successful tail; CBD's retained ownership is separate. A supplied
context is not cleared by that conditional tail.

## Native 2C1 owns a different, real 2DA

2C1's default constructor is `053C30` (ctor table `276A08`). The function
continues beyond the first unwind fragment ending at `053C73`:
`053C78` creates descriptor 2DA, `053CBB` stores it at 2C1 `+10`, and the
following null check controls constructor failure/cleanup. Its object table
`2769A0` binds `V10=054340` and `V18=054A30`; these are used by 049370.
Native 2DA object table `275550` has `V18=0283B0`.

This closes the registered producer of a native 2C1 containing native 2DA.
It does not turn the library-owned selected-input seed, detached BCC target,
or transport cache into that exact context. The library seed's retained
C20/C22/pending-C4D/UUID ownership is a separate, explicit portable route.
No SDK page, history receiver, or accepted C20 is manufactured by this proof.

`00C590` also uses CB5 service `209E00.V0=Engine!1243C0` to create a new BCC
and clone selected DOM content. BCC's `V200=031900` returns null; its
`V1F8=027640` is a no-op. Consequently that worker BCC cannot simply stand
in for the live request/context receiver required by 05F720.

## Remaining integration boundary

The earlier complete-reference scan found no conventional code, absolute
pointer, or export reference to the entry address `Text!05F720`; its raw RVA
occurrences were unwind metadata. The body remains useful static evidence,
but is not the proven registered request caller. In particular its
`05F200` transport's version `-1/-1` contract must not be silently equated
with all registered C8F processing/history versions.

Before an acceptance adapter can use the registered route, it needs actual
selected-request creation, reset/property/resource gates, native context
processing/output-cache transfer, and the receiving page/history operation
bound through the same lifetime. This checkpoint adds no public runtime
adapter and makes no acceptance claim.

## Reproduction

Run from the repository with the pinned private research binaries available:

```powershell
python tools/handwriting/audit_registered_text_request_dispatch.py E:/BlurMask-build-artifacts-20261014/registered-text-request-dispatch-static.json
```

The audit checks full DLL hashes, hashes 11 decoded regions, pins 12 virtual
bindings and 72 instructions, including the incoming-context conditional,
CBD no-prior creation, CBD releases, and the 2C1 constructor continuation.
It reports `vendorExecution=false`. The selected-input DOM construction and
larger processing branches above are read-only decompilation observations,
not additional runtime completion claims.
