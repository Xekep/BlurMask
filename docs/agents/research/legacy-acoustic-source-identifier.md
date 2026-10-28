# Acoustic source133 identifier and the Chinese cache branch

`LegacyAcousticSourceIdentifier` reads the observed247/v10d →134/v201
→133/v100 data prefix. The outer header and inherited versions, markerFFFFFFFF
and following alphabet2b/v103 are checked. Text1d2680 obtains56 name bytes;
markerFFFFFFFF chooses the full56-byte XOR branch of1d22e0 with repeating
`14 12 19 77`. Reading this data does not instantiate a resource, run its
authentication or execute a vendor DLL.

The native source+14 identifier is a NUL-terminated byte string. The first
NUL ends its name even when later decoded bytes are nonzero or invalid UTF8.
Unterminated names and malformed UTF8 before the terminator are outside the
bounded portable reader. Other versions and the shorter legacy decoder branch
remain unsupported.

Engine builtin service37's table20f290 slot58 points to089ad0, an ordinal
**prefix** comparison. It uses strlen when explicit end pointers are null,
checks the two lengths and compares corresponding beginning bytes. This is
different from substring slot10=089680. `HasBytePrefix` ports the implicit-end
branch with bounded byte spans, retaining NUL termination, empty-prefix and
case-sensitive behavior.

Text150d00 obtains the selected acoustic source and its ordinary clone through
virtual98, then initializes the clone's virtual18=1b0760 with the selected
source+14 identifier as param4. `1b0760` copies this argument into local90 and
tests service37.virtual58 with literal `zh_` and null ends. Only a true prefix
allocates the optional2d0 confidence-intersection mask. The real pinned Russian
acoustic file decodes to `ru_RU-ak-cur.lite`; this predicate is false. Neither
the filename on disk nor the user's system language supplies this decision.
The main acoustic scorer consumes this owner fact separately from its normal
linguistic symbol masks.

Static sources are MyScriptText.dll SHA256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`
and MyScriptEngine.dll SHA256
`d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c`.
The actual AK file SHA256 is
`7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978`.

Original Python controls produce1024 independently specified byte-prefix
comparisons and264 encoded names, including embedded NUL, UTF8, nonzero tails,
empty strings, and `foozh_` versus `zh_`. Managed Release and Windows NativeAOT
verify those controls,15 malformed-input guards and the actual RU resource name.
They exercise independent code, not vendor binaries. No factory refresh,
general Chinese cache implementation or recognition accuracy follows from these
checks.

```powershell
python tools/LegacySourceIdentifier.Probe/reference_controls.py .artifacts/legacy-source-identifier-controls.json
dotnet run --project tools/LegacySourceIdentifier.Probe -c Release -- .artifacts/legacy-source-identifier-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```
