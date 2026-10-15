# Prepared case-aware dictionary session

`LegacyPreparedCaseDictionarySession` joins the already restored source methods
over one dictionary/context/graph owner. The same backend serves full-tag normal
V40 and auxiliary V48 queries. The symbol binding's234840 permission expansion
then093F80 working-map refresh updates that backend, rather than a detached copy.
Source enumeration remains available from `Symbols.CreateSymbolSource()` for the
owning acoustic mask producer. Alphabet and language identity are checked against
the selected model. Container90/C4, restart tags and side-cost presence remain
explicit prepared configuration; this is not the complete20C9D0/240 factory.

`ResolveRestart` follows236B50: remove selected-source byte4, call233F70 and restore
source1 only if the returned tag has header05.bit2. Case bits58..62 copy only when
the incoming header05.bit2 is set. `ReadTerminal` follows236D20: forward the full
tag to234970 and write selected-source ID to outputU32+0, keeping word ID+4,
language category+8 and boundary+12 separate. Source0 is implicit and cannot be
used as this selected dictionary; source2 is not silently redirected to source1.

Query invalidation remains a distinct controller epoch. Refresh does not invent
a cache reset or claim existing outstanding requests are automatically restarted.
Operations are serialized; failed queries invalidate the session. Failed acoustic
preparation never forwards partial permissions or changes its working map.

The probe uses actual RU resources and a supplied ordinary graph. It verifies
5742 normal records across28 states,28 genuine auxiliary empty reads,512 terminal
metadata combinations,512 restart tags and three targeted permission refreshes.
Independent Python retains the older case expansion/metadata oracle unchanged;
it additionally reconstructs raw AL triplets and checks filtered raw backend
records and the complete64-bit restart values. This is state/ownership evidence,
not a Russian handwriting accuracy or native final-winner result.

Run the probe with `AK LK engine-profile case-data publication-data capture`.
Run its `audit_output.py AK LK case-preparation-controls capture` and static
`tools/handwriting/audit_case_dictionary_session.py output.json` independently.
