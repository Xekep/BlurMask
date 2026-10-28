# Native scalar reading and final quote normalization

`LegacyNativeGlyphUtf8Tokens.ReadScalar` restores139C90 independently of token
expansion. It preserves all four caller bytes for zero-count kind3, preserves
the label for kind3/subkind1, writes only the alternate byte where required,
and never initializes padding. Control labels require the actual273+18 lookup;
an acoustic alphabet is not substituted for that service.

`LegacyNativeWordQuoteNormalization` restores21FBF0 over owned WORD276 chains
sharing their actual glyph pool. It skips initial headerE symbols, changes
the first synthetic apostrophe or double quote to the ordinary alphabet label,
and changes only synthetic apostrophes at the last linked reference. It writes
only glyph ushort08. Leading/trailing four-byte stack scratch remains explicit,
separate and retained across words. Empty invalid native dereferences are
refused. This does not choose words or establish handwriting accuracy.

The headless probe covers524,288 scalar descriptors and2,097,152 output bytes.
An independent Python oracle also checks543 linked words in272 prepared lists,
61 quote writes, every preserved record byte and final caller scratch. Six
ownership/input/cancellation guards run separately. Prepared lookup controls
are explicit test inputs, not a reconstructed result-alphabet owner.

Probe: `AK scalar.bin words.json`. Audit: `AK scalar.bin words.json`.
The optional static audit pins three source ranges, the274.V58 binding and13
quote instructions without loading the vendor DLL.
