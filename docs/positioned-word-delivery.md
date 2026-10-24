# Positioned WORD delivery

The experimental completed-text adapter now returns detached `WordRuns`:
literal text, the minimum uncalibrated glyph score, original input ranges,
range-derived ink bounds and the prepared containing-line geometry. A multiword
result leaves flat `Text` empty; separators are not guessed. These values cannot
create a live recognition source or WORD/TEXT owner.

The app sends a whole row in original stroke chronology. The transformed input
points used for inference also supply range coordinates. It validates every
run and full input coverage before replacing any ink. Prepared line x-height
and baseline set a common Inter font scale and sloped placement using actual
font metrics. Inter selection and the glyph-score cutoff are host display and
admission policies, not recovered native font selection or calibrated confidence.

A successful group replaces the original strokes in one undo/redo transaction.
The AI context retains the original ink behind the whole group for refinement;
editing or replacing one member revokes that association. A recognized fading
group expires or is undone together. Fading ink keeps its existing local-only
behavior; this change does not add fading redo or group metadata to the wire.

Unsupported results retain the original ink. In particular, the post-TEXT path
currently requires the proved ordinary line0 branch, exact WORD insertion
chain and equal WORD/TEXT union. Destructive alternative removal, multiline
repair and unequal-union registration are still explicit refusals. Existing
source, interval, point-read, glyph-policy and timeout limits remain in effect.

## Verification and limits

Root reviewed the source before executing a single changed-consumer owning run
on the same fixed original BRUSH0/0 sample. It returned one WORD/two choices,
699 point reads and full original coverage, with unchanged acoustic evaluation
counts. Its minimum score was below host admission: no quality improvement is
claimed. The strict independent DATA audit passes55 corruption controls and
accounts for the complete validation-counter delta from the older capture.
Actual output with more than one WORD is still unverified. The research sample
and its captures are not included in the repository or releases.

Current-main app and interval builds passed without warnings/errors. Local
checks cover30 prepared/mock layout and group-edit scenarios, existing AI batch
checks,54 AUTO routing guards,19 actual font/geometry checks and4227 interval
controls. The rendered Cyrillic PNG is byte-identical to the previously visually
reviewed output. These checks load no recognition model and establish neither
Russian cursive quality nor completion of English TEXT or full AUTO.

Source and synthetic DATA checks run independently on three desktop operating
systems through `recognition-source.yml`. Native AOT jobs and future releases
also run layout and offscreen font checks. These new cross-platform checks must
pass before their results can be claimed. Python replay tools require explicit
SHA256 pins for any external actual captures; their synthetic fixtures contain
no corpus, models or expected recognition labels.

External root evidence: `root-positioned-app-main-20261024`,
`root-structured-consumer-actual-20261024`,
`root-structured-strict-audit-review-20261024`.
