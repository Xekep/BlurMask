# Russian final-formation heap lineage

The experimental 945-source library adds a separate diagnostic for the actual final primary heap. It tracks individual offer IDs through original comparisons, slot moves, writes and root overwrites. Its final boundary is the first existing snapshot before `AssignInitial`; it does not establish later retention or final selection.

ROOT compiled and authenticated the 945-source library and its entry point. An independent old-allocator reference then matched all 128 bytes of every slot, including inactive and tail bytes, through 4486 numeric comparisons at capacities 0, 1, 2, 7 and 4096. Cancellation, initializer failure, cost-invariant failure and reentrant failure prefixes also match. Five independent prepared chronology replays pass.

The same nine recognition inputs were run with and without this diagnostic. Public non-timing outputs match each other and the previous 944 build; acoustic evaluation counts are unchanged. All nine final-formation captures fit the unchanged shared limits and pass independent chronological replay. This is stronger coverage than the previous deep candidate diagnostic, where two captures were incomplete.

Offline lexical annotation establishes the specific loss of «ещё» in examples 1 and 7. All eight matching offers in example 1 and all four in example 7 are initially accepted, then actually overwritten at the root of the full heap by later parent candidates with smaller costs. Their individual offer IDs, victim tags and costs are joined to the observed overwrite operations. The conclusion is not inferred merely from later absence or equal tags.

For example, offer 111 at cost 101127 is overwritten by offer 386 at cost 94004. In example 7, offer 73 at cost 114335 is overwritten by offer 256 at cost 74785. This proves the mechanism of loss at this boundary. It does not prove that the upstream costs, features or native parameter wiring are correct. The next investigation concerns those cost inputs; heap capacity and recognition thresholds have not been adjusted to these examples.

Other expected-word offers survive at this boundary in examples 0, 2, 3, 4, 5 and 6. Example 4 has both surviving and overwritten offers. Example 8 has no matching final-node offer, so its earlier missing-prefix problem remains separate. Surviving this boundary does not imply successful recognition. Overall acceptance remains three of the nine examples; Russian cursive is not complete.

Evidence under `E:/BlurMask-build-artifacts-20261014/`:

- `root-ru-heap945-build-20261026/report.json`: library and entry-point compilation, source controls and PDB authentication.
- `root-ru-heap945-numeric-20261026/report.json`: original allocator byte parity and independent prepared replay.
- `root-ru-heap945-pair-20261026/report.json`: matched real calls and nine complete independently replayed captures.
- `root-ru-heap945-offline-fates-20261026/report.json`: offline lexical annotation only; never an inference input.

The shared payload/copy/read limits remain 4/4/8 MiB. These semantic observation limits are not process-memory measurements. No vendor binary is executed. These experimental diagnostics are not included in the production PR or a release.
