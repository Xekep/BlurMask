# English sparse language request and history: static closure and bounded proposal

This is a static proposal, with no runtime changes or handwriting-quality claim.
It extends the committed `legacy-english-lk-sparse101-boundary.md` proof. The
actual English owner is source135/v110 with lexical2B4/v102 and language2D2/v101,
order3, 260075 word slots and1024 classes. The source, packed data, numeric
binding and MAIN masks remain the very same reference-bound owners. No Russian
dense/order2 owner, configured full-root surrogate or target labels are used.

## Strongest findings

The current generic Russian known-word record cannot safely implement English:
FIRST request byte0=1 inserts the real reserved word260074's class0, and matched
sparse order is distinct from retained history count. English record40 field+4
is the matched order after fallback; byte+8 is the retained class/tag count,
bounded to2. For example a missing trigram may emit order1 and retain2 history
items. An implementation using `history.Length+1` for field+4 changes the native
result even if the integer cost happens to match. No such implementation is
added here; this is the contract for the next NEW English-only component.

All643868 actual-model numerical controls agree between a direct packed-row
binary-search evaluator and an independently built tuple suffix/backoff table.
The digest is `1cad8793f3a43641a912f581be810347aa1b0bba066cc0f86fba7b820f223ebe`.
Controls comprise260074 BOS-first-word cases,113643 bigrams,155484 trigrams,
113643 absent-trigram cases and1024 absent-bigram cases. Matched-order counts
are248913/239471/155484 for orders1/2/3. These controls are model numerics;
they do not fabricate NN-selected terminals or measure English handwriting.

## Owning source and actual caller

Source2B7 vtable2B4360 and language2D2 vtable2BACF0 are distinct. Source.V58
is an empty continuation, not language.V58. In204290 the selected source
descriptor is read from controller22E0. Descriptor+10 is checked for13B; that
resource+88 supplies the actual2D2 language object. Descriptor+18 is passed as
its compiled numeric binding. The call is language.V58=107A40. The same source's
V70=094040 supplies the16-byte token;204290 copies it into request+18. V70
clears two qwords, fills output+4 from the dictionary's prepared packed
node-to-terminal map, writes explicit boundary at+C and source tag bit42 at+D.
Thus request+1C is the unshifted word ID, category+20 starts at0, boundary+24
comes from token+C and suppress-category+25 from token+D. A breadth-first node
ID is never a word ID.

`LegacyLexiconWordIndex` already provides an identity-bound terminal-preorder
map for the checked ordered2B4 tree (native2489E0). A future NEW typed terminal
receipt can use that unchanged generic component with the actual English
dictionary and validate terminal-only membership. This would prove dictionary
token provenance, not that handwriting search selected that terminal. The
future NN/controller request bridge must consume a real selected path, and is
not claimed by a standalone lexical query service. Nonterminal packed entries
may name a later terminal and must not become known-word receipts.

FIRST204290 zeroes the request, sets byte0=1, copies fresh F5 into byte1, attaches
the per-query heap and copies the actual token. Later-word branch copies the
previous candidate's five32-bit language words from candidate+40..+50 into
request+4..+14 before language.V58. Previous history must therefore be an
immutable completed result from the exact same language/numeric owner, not a
caller-supplied array of superficially valid class IDs. Request trace and
caller budget fields must not be mistaken for class history.

Controller preparation20D8F0 reads20C1C0's controller2540 policy flag, forwards
it through source.V10=093970 to092990; trace flag2368 is the other argument.
092990 forms packet bytes `{literal0, policy, trace}`.105CA0 stores the last two
at2570/2571. Fresh00C860 zeroes the qword containing knowledgeAA, and20BAA0
explicitly clears2368. This supports a separately declared fresh independent
no-option/no-trace MAIN policy, consistent with the existing English M owner.
It does not prove that an arbitrary full-root configured session has no other
source, options or epoch. Literal packet0=0 returns before V70 cache allocation.
Actual flags7 leave80/90/26A0 absent; no extra vocabulary or cache is inferred.

## Exact V58 numerical/record contract

Clamp previous history to maxOrder-1=2. BOS insertion uses reserved260074→class0.
Map current word to its class with88. Traverse packedB8 row offsets and sorted
B0 labels using1078D0. Try the longest suffix. If a suffix is fully matched,
read its C0 code through compiledD8. If the full suffix is missing but its exact
prefix was reached, add that prefix's C8/compiledE8 backoff; if the prefix was
not reached, drop the oldest class without an invented penalty. Repeat through
the total unigram table. Add the word-specific98/compiledWord cost, then the
native admitted category/boundary terms in native order. Preserve integer
rounding,268C-derived Context00=0x04000000 and compiledL. Under this Context00,
the optional `(cost-L)/10+L` compression branch is inactive; L is not zeroed.
Category0 bypasses case-table reads. Nonzero categories must use only the
actually written144 pair/1728 triple domain, never default unwritten slots.
Byte1 category clearing and the category3 correction require their explicit
native branch; an initial bounded API may refuse nonzero byte1/category until
that branch is implemented, rather than silently erase all history tags.

With actual80 absent, V58 examines exactly currentWord..currentWord+1. With90
absent, alternate-vocabulary sentinel is WordSlots260075, whereas reserved
BOS is260074. The emitted record stores FFFFFFFF at+18 and current word at+1C.
Do not confuse either sentinel with a normal terminal or shift word IDs by1.

The40-byte row writes+0 cost,+4 matched order, byte+8 history count, and only
the active history words+C/+10; +18/+1C are written as above. Padding+9..B,
inactive history slots,+14..17 and the no-trace pointer+20 remain allocation
residue. A NEW owner should store an explicit written-byte mask and refuse
numeric reads of unwritten bytes; diagnostics can fill prepared allocation
residue and compare untouched bytes. Returning a zero-filled40-byte buffer as
a native-complete row would be false. One traced/allocator-style row is not
implicitly reusable as another record layout.

204290 allocates440 bytes: eleven40-byte slots including heap sentinel, capacity10,
and resets count for each query. Actual80-null emits at most one row, admitted
only belowIMP. Full-heap replacement/tie behavior is a separately labelled
prepared control if exposed later, not a naturally exercised English branch.
Caller candidate budgets are applied after the language result; no new
confidence threshold or fitted probability belongs inside this language service.

## V60 and V70 are separate services

Language.V60=108B50 receives history count at0/history words+4, category+18,
boundary1C, suppress1D, BOS1E, word interval+20/+24 and output pointer+30. It
writes28-byte rows, not40-byte heap rows. Language.V70=109A40 receives history
at0/+4, boundary14, BOS15, interval18/1C, option20 and heap28; it uses12-byte
rows `{word, alternateVocabulary, cost}` and heap helper0F0730. Literal packet0
leaves its caches null. The cache-free ordinary interval branch must remain
distinct from optional26A0/80 branches and the >=50001/top20000 path. Actual
204290's known-word call is V58; neither bulk service is required to pretend
that an English first-word query already invoked them. Initial runtime scope
below implements V58 only and leaves V60/V70 explicit unsupported boundaries.

## Minimal NEW-only runtime proposal (approval required)

Add an English-only query owner bound to the exact packed data, sparse language,
fresh numeric binding and MAIN masks; reference equality is mandatory at every
join. Add a typed terminal receipt built from the same actual dictionary and an
identity-bound unchanged `LegacyLexiconWordIndex`; record node, unshifted word
ordinal and explicit native boundary/source-tag inputs. Its name and API must
say dictionary terminal, not NN-selected candidate. Add an immutable prior-result
receipt carrying only written history/count/order/cost from that same owner.
FIRST and continuation are separate typed methods: FIRST supplies native BOS;
continuation accepts the exact previous receipt, preserving packed tags. No
public arbitrary history, category flags, callbacks or profile coercion.

For the first bounded batch admit the actual no-trace MAIN policy,80/90/26A0-null,
one ordinary terminal, F5=0 and category0; boundary/suppress bits are represented
explicitly with their native terms. Refuse unsupported flag/category paths
before work. Keep V60/V70 unsupported, not approximated by V58. A subsequent
real search bridge must provide selected terminal provenance and native caller
category producers; the standalone component must not be wired into app as an
invented decoder or used to claim complete English recognition.

Preflight all input/history/word/map bounds, checked byte/work totals and source
identity before allocation/publication. Suggested caps: full map construction
<=666547nodes/260074terminals,43depth; one query<=3suffixes/2lookups each and
<=1output; total prepared diagnostic operations bounded explicitly. Poll cancel
during map work and before publication. No partial result on cancellation,
foreign/stale owner or failure; prior immutable results remain usable. Logical
copy budget includes the2.67MB node→word map; do not hide it in per-query work.
Immutable query state permits parallel calls without shared history mutation.

Regression plan: all643868 independent costs/order/history controls, then the
complete40-byte mask with nonzero prepared residue, BOS/non-BOS histories,
suffix misses at each level, word cost/category0/boundary paths, sentinel
distinctions, unsupported branches, reference-mismatch owners, terminal versus
nonterminal, concurrent calls and cancellation at map/query boundaries. Compare
Managed and NativeAOT bit-for-bit. Keep existing English data/numeric/masks/NN
captures and all Russian full-global261 captures exact. No beam/weight/cost
tuning, target labels or language arbitration is part of this batch.

## Reproducible static proof

The NEW pins add12 full Ghidra bodies in13 getBody regions (204290 has two
regions),4132 instructions,139 call bindings,8 matched-order/history critical
instruction pins and2 vtables. They compose the unchanged16-body sparse proof.
The auditor verifies the full DLL SHA and PE bytes, upstream pin hash and four
unchanged owner hashes, then runs the independent actual-model reference.
It reads SDK files only; no SDK module is loaded or executed.

Run `audit_english_sparse_query_history_plan.py --repo REPO --models MODELS
--sdk SDK --output OUTPUT`. Raw numerical controls use
`reference_english_sparse_queries.py --repo REPO --models MODELS --output OUTPUT`.
This proposal makes no claim that every configured/root/option language route,
decoder acceptance or English handwriting quality is complete.
