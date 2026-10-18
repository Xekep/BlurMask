# Completed math candidate retention

Completed events retain copied raw48/primary/secondary/ancestry and their exact
source step identity. They do not need every old candidate owner's full log,
cumulative-cost, information, heap and masked-score arrays after prefix Apply.
Previously both the event lease and its step kept those arrays strongly alive.

The lease now retains a weak candidate identity. RegisterCompletedEvent still
binds the exact candidate strongly to the active pending step before returning
the lease. Capture and Consume verify the same identity. Pre-entry canceled or
invalid selection cannot release that anchor. A consuming Apply releases it in
finally on success or failure; recording failure and context disposal also
release the pending anchor. Consumed leases and their immutable semantic data
remain available to historical attention, EF4 and final ordering. No KV-cache
ownership or numerical algorithm is changed.

The same actual M/I pen fixture, using official Microsoft CPU ORT1.23.2, produces
1/15/51 completed records at width1/3/16. With sources and all historical records
kept alive, forced GC reduced distinct candidate owners from1/5/6 to0/1/1.
Remaining current recorder-phase ownership is intentional. The67 copied record
contents match the pre-change captures exactly. EF4/final payload hashes after
GC also match a fresh no-forced-GC invocation on the same platform.

Four further actual pending-EOS cases prove candidate survival through capture
and pre-entry cancellation, followed by release after ordinary Apply, injected
consuming failure, recording failure or disposal. Managed and fresh NativeAOT captures agree.
Nine existing math probes and their independent numerical oracles are rerun
without changing their captures, including failure and ownership checks.

This is object-reachability evidence for specific arrays. It is not a measured
RSS reduction, does not explain the earlier Arch500MB report, and does not
establish complete mathematical output or recognition quality.
