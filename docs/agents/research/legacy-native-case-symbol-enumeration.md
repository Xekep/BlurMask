# Wrapper292 case symbol enumeration

`LegacyNativeCaseSymbolEnumeration` restores callback `232C80`, which `232ED0`
passes into the wrapped source's slot30 symbol enumeration. It retains one
prepared cased-alphabet owner; it does not replace a missing full source factory
with fabricated query pages.

Incoming labels at or above the actual alphabet count go straight to the sink.
For ordinary labels, failure of mode0 form lookup also sends the original label
once. Otherwise the native callback emits non-FFFF first/second/third entries
from mode0, then mode1 and mode2 when available. Missing mode1/2 uses the
alphabet getter's mode0 fallback. Repeated forms are **not** deduplicated.
A false sink result immediately stops the callback. Existing native triplet
repair only fills an empty first or second slot when the input label is absent;
it does not overwrite an already present second slot.

The independent raw-byte parser/tuple oracle checks305 actual/prepared
alphabets accepted by range preparation,59,422 symbol expansions,67,726 ordered
callbacks and every67,726 possible early-stop prefix. The prepared maps are
diagnostic, not actual NN outputs. Literal/control labels, absent forms,
repeated fallback modes and cancellation are included.

Full292 preparation, stateful queries, permission updates and the actual
20C9D0 source composition remain separate integration work. This callback alone
does not establish Russian handwriting recognition.
