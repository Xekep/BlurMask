# Shared selected Math model weights (2026-10-24)

The new immutable `LegacyMathAkSelectedModels` owns exactly the five active base-tail13A5 models. The sixth raw catalog model (first13A3-owned3F276 at8702) is excluded. Source slot order is 1F222,2F204,3F279,4F279,5F223. Two following source slots are empty. Native0170B0/018970 and the actual resource independently establish this selection; the existing raw reader still authenticates and temporarily decodes all six.

Prepare the owner once and use `BindTemporal(source)` for each genuine source geometry. Bindings keep the exact owner reference and the source; no weight array is copied, exposed as memory, or returned through a mutable collection. The existing public TemporalNetwork convenience factory now prepares this selected-five owner. This is an intentional increase from its former single1F retained payload; it is not an allocation reduction for that one-call convenience path. Repeated clients must retain/reuse the explicit shared owner. No process-global cache, eviction policy or disposal race is introduced. These are immutable managed arrays, without native handles.

Only genuine source-bound temporal222 evaluation is currently exposed. Foreign equal-data source features are rejected before copying features or evaluating a dot. The other four selected models are retained for subsequent source-bound branches, not exposed as arbitrary-vector evaluators. No full classifier, calibrated confidence, unified AL costs, candidate or matched-tree authority follows. Existing Dot16,Dot8,Hidden,ConvertRaw bodies are byte-identical after newline normalization; the two numerical loops move into RunModel unchanged. CLR floating/environment qualifications of the prior primitive remain unchanged.

## Allocation and work accounting

All amounts below describe logical array payload and byte/element traversal cardinalities, not CLR object headers, RSS, GC peak, CPU instructions or latency guarantees. Fixed exact resource size and the existing whole-six preflight bound all copies before decoding weights. Additional selected padding checks occur before publication.

* Final five weight payload:761370B (first int16, second int8,258 scales each). Five scalar descriptors contain25 int-sized fields=100 logical bytes. Private owner/model/wrapper reference metadata is additional managed object overhead, not included in these array totals.
* Existing raw resource snapshot:3685881B. Temporary raw-six arrays including alignment:930319B. Aggregate detached selected snapshot arrays including alignment:761404B. Final copied selected weights:761370B. Conservative aggregate array payload allocated in preparation:6138974B. This aggregate remains valid without relying on timely GC. It is not a precise simultaneous-live or RSS bound.
* Byte-equivalent traversal accounting:resource copy+SHA=7371762; raw scale checks1548; raw decode/alignment930319; selected Capture761404; final selected copies761370; selected zero-padding checks13995. Sum9840398, excluding fixed metadata operations, allocation initialization, hashing implementation internals and cancellation observer overhead. No unproved whole-executor work ceiling is claimed.
* Each BindTemporal adds one wrapper holding two references; no matrix/scales payload allocation. Evaluating1F retains the prior per-call input222,200 dots,201 hidden and257 outputs; conversion/result-clone allocations retain prior semantics. No evaluation occurs in the new ownership probe.

## Verification

Builds use CPU2/fresh E artifacts. New probe compares all five first/second matrix and scale hashes against independently parsed original bytes, checks256 bindings keep the same15 arrays, caller-resource clearing, immutable scalar view, parallel inspection, pre-cancellation tokens and foreign-source refusal. It performs0 model forwards. Seven corrupted captures must fail the Python oracle. The old synthetic numerical probe checks8 dots,17 hidden boundaries,771 Q15/costs and7 corruptions, also0 real model forwards. Root must review before any actual forward replay of the refactored path.

Commands (from isolated checkout):
```
dotnet build tools/LegacyMathAkSelectedModels.Probe/LegacyMathAkSelectedModels.Probe.csproj -c Release --disable-build-servers --artifacts-path E:/... -p:UseSharedCompilation=false -nodeReuse:false
dotnet E:/.../LegacyMathAkSelectedModels.Probe.dll REPOSITORY FRESH_CAPTURE
python -I -B tools/LegacyMathAkSelectedModels.Probe/audit_output.py REPOSITORY CAPTURE
python -I -B tools/LegacyMathAkSelectedModels.Probe/audit_native.py SDK_REPOSITORY FRESH_REPORT
```
SDK_REPOSITORY must include the existing read-only `research/myscript/sdk/build/x64/MyScript2D.dll`. The first native audit invocation used the isolated git worktree which omits that ignored SDK; it failed FileNotFound before native reading. The succeeding invocation used the existing root SDK repository, no script/source changes. No vendor code was executed.
