# English bootstrap historical source check

The English input bootstrap jobs in source-geometry run `40849611696`
failed on Windows, Linux and macOS at the first source-contract check,
before any build or model-free execution. Five source hashes belonged to
`ef32310`; those files subsequently changed in the reviewed periodic
publication admission `11c647a`.

`tools/LegacyEnglishOriginalInput.ActualEntry/bootstrap_preimage.py`
now restores those five exact current bodies to the original contract's
successor bodies. The hash-pinned JSON contains 34 inverse line edits.
Both endpoints and every replaced byte slice are checked. Unknown source
bodies, paths, historical endpoints and modified inverse tables are refused.
The original source-contract JSON, three older inverses, historical
references and raw fixture hashes remain unchanged.

This is a historical SOURCE check. It does not establish runtime or
recognition equivalence of periodic publication, admit new runtime code,
or prove Russian/English handwriting quality.

Validation: `test_bootstrap_preimage.py <repo> <new-output.json>` passed
17 positive and 36 refusal controls. These include invoking the original
isolated-mode source-contract CLI on current and restored historical trees,
then modifying each of the five sources independently. The controls run
before the original contract check in `check-english-input-bootstrap.py`.
Root evidence is under `E:/BlurMask-build-artifacts-20261014/english-bootstrap-failure-20261029/`.

The complete bootstrap build and model-free helper jobs still require the
next cross-platform CI run. No NN, vendor SDK or GUI is executed by these
source restoration controls.
