# Case context suffix rewriting

`LegacyNativeCaseContextRewrite` restores `232F10` over the owned portable AVL
symbol factory and prepared stored case tables. It follows parent links until
the exact starting context, collecting at most32 ushort symbols, then rebuilds
the suffix oldest-first in the same container. Range-qualified acoustic labels
use the selected six-byte form row and field0/1/2. Labels outside AL count and
labels outside the native cased ranges remain unchanged. It uses the native
range accessor, not a Unicode case heuristic.

The root and nonroot child-tree paths preserve existing identities and set
parent links only for newly allocated nodes. A path exceeding32 ancestors
returns zero before insertion. Foreign/stale context identities are rejected.
Native uninitialized form-table holes are explicitly unsupported rather than
filled with invented symbols. The input path remains live and unchanged.

An independent symbol-path oracle checks 10,392 paths over433 actual/prepared
alphabet owners, plus a separate mode1-only owner which proves refusal of an
uninitialized mode0 table read. Checks cover empty paths, 1/2/31/32/33 ancestors,
root/nonroot prefixes, all three fields, both prepared tables, repeated lookup
identities, foreign owners and cancellation. Managed and Native AOT use the
same controls. The static audit pins DLL hashes, the32-ancestor bound, parent
walk, form offsets and native `2B.virtual48` range accessor.

This is context rewriting, not the full wrapper292 query. Its page ring,
transition tables, cost/state emission, Turkish special handling, restart and
complete owning source factory remain separate. Russian cursive quality and
application integration are not established by these checks.
