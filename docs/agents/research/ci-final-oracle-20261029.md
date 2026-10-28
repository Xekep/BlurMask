# FINAL publication oracle context

The three Build jobs for `63b0cd0` reached the four-node positive FINAL fixture and failed an exact pool comparison. The production path supplied its GLOBAL publication context; the independent procedural oracle omitted both its LIVE context and the later FINAL context. A null context selects the non-GLOBAL fallback, so equal record counts and query counters did not imply equal publication bytes.

The correction changes two probe files only. The reference scheduler retains its own allocator, policy and independently produced rows. Its one completed result carrier owns the reference result-query lease; the outer query closure and FINAL context use that same carrier. Capturing another lease would revoke the first. No production API, numeric tolerance, expected label or acceptance policy changes.

Root verification used the current 935-source core DLL, SHA-256 `f04d1a1529979a6085416d454b5584c4c2c503532be1af4d27d7f06a96b69402`, and a fresh build of the admitted probe sources. Build: 35.60 s; actual probe: 26.67 s; unchanged independent raw-resource Python oracle: 32.00 s. Each ran with two processors and affinity mask 3; build watchdog 600 s, actual watchdog 45 s.

All five fixtures pass: three natural no-winner cases and two positive 1,057-record cases, with selected index 1,019. The checks retain 48 guards, 12,148,736 compared row/pool bytes and 335,872 symbolic fields. The complete capture is byte-identical to the separately reviewed agent capture. Native source-byte audits also passed in the review packet.

Evidence is retained locally under `novapen-reset-20261029/final-oracle` and `global-final-oracle-context-next-20261029`; the latter patch has SHA-256 `09923cd05ca92df7fb99cc5dd47b7a984248e763ea0347e7aa5e7324cbcd4149`. The current Release core has no PDB: this is an exact DLL pin, not a new full-core source/PDB proof. This check repairs test authority; it does not establish recognition quality or completion of the engine transfer. Remote cross-platform checks remain separate.
