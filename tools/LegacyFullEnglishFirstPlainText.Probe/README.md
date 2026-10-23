# Full English FIRST B6 probe

All commands run only our code; the native audit reads SDK bytes without loading the DLL. Supply blind raw coordinates only. No target words are read. The probe has its own AssemblyName/basename and grants no new OLD metadata seam.

Build/run (PowerShell examples; use your artifact/input paths):

    dotnet build tools/LegacyFullEnglishFirstPlainText.Probe/LegacyFullEnglishFirstPlainText.Probe.csproj -c Release -o $b6Managed
    & "$b6Managed/LegacyFullEnglishFirstPlainText.Probe.exe" $b6Models $b6BlindRaw $b6Capture
    dotnet publish tools/LegacyFullEnglishFirstPlainText.Probe/LegacyFullEnglishFirstPlainText.Probe.csproj -c Release -r win-x64 -p:PublishAot=true -o $b6Native
    & "$b6Native/LegacyFullEnglishFirstPlainText.Probe.exe" $b6Models $b6BlindRaw $b6NativeCapture
    & "$b6Managed/LegacyFullEnglishFirstPlainText.Probe.exe" --extra-controls $b6Models $b6ExtraReport
    & "$b6Native/LegacyFullEnglishFirstPlainText.Probe.exe" --extra-controls $b6Models $b6ExtraNativeReport

Use both141 and100 existing raw corpora independently. The actual pipeline evaluates A/B1 once, then B2..B6. B6 reads completed cache data and cannot evaluate NN; counter checks surround two allocation patterns. Extra-controls uses a separate small prepared synthetic route, not another whole-corpus run.

Generate independent oracle ingress without E-only tracked JSON. $b6A/$b6B1/$b6B4/$b6B5 are existing full-source outputs with their own independent raw audits:

    python tools/LegacyFullEnglishFirstPlainText.Probe/extract_context_inputs.py --repo . --a $b6A --output $b6ChildRaw
    & "$b6Managed/LegacyFullEnglishFirstPlainText.Probe.exe" --context-receipts $b6Models $b6ChildRaw $b6Contexts
    python tools/LegacyFullEnglishFirstPlainText.Probe/reference_contexts.py --repo . --capture $b6Contexts --b4 $b6B4 --output $b6ContextReport
    python tools/LegacyFullEnglishFirstPlainText.Probe/reference_receipts.py --repo . --models $b6Models --a $b6A --b1 $b6B1 --b4 $b6B4 --b5 $b6B5 --output $b6Projection
    python tools/LegacyFullEnglishFirstPlainText.Probe/audit_output.py --repo . --models $b6Models --b4 $b6B4 --b5 $b6B5 --contexts $b6Contexts --projection $b6Projection --capture $b6Capture --report $b6Report
    python tools/LegacyFullEnglishFirstPlainText.Probe/audit_output.py --repo . --models $b6Models --b4 $b6B4 --b5 $b6B5 --contexts $b6Contexts --projection $b6Projection --capture $b6NativeCapture --report $b6NativeReport
    python tools/handwriting/audit_full_english_first_plain_text.py --binary $b6SdkTextDll --repo . --output $b6StaticReport

The noNN context helper regenerates actual child cuts/seed/candidate graph from raw child input. reference_contexts.py independently checks stable-interleaving DP, predecessor/successor/context classes and each retained candidate source path. reference_receipts.py reads actual full raw resource and audited four-family confidence/ranked/terminal receipts. These are verification inputs, never inputs to the production factory. The separate reference_budgets.py proves conservative maximum arithmetic; copy it into an artifact directory before running if its default JSON output should stay outside the checkout.

No claim of native ABI, complete serialized273, UTF8 accepted text, source720, guide/default or recognition-accuracy readiness follows from a PASS.
