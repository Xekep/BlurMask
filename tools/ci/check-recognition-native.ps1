python tools/LegacyEnglishLiteFirstPreprocessing.Probe/reference_controls.py .artifacts/legacy-en-short-controls.json
if ($LASTEXITCODE -ne 0) { throw 'English preprocessing control generation failed' }
python tools/LegacyEnglishFirstSlant.Probe/reference_controls.py .artifacts/legacy-en-short-controls.json .artifacts/legacy-en-slant-controls.json
if ($LASTEXITCODE -ne 0) { throw 'English preprocessing control generation failed' }
$cases = @(
  @{ Name = 'LegacyDirectional2B1'; Args = @('.artifacts/legacy-directional-2b1-controls.json') },
  @{ Name = 'LegacyLinePairFeatures'; Args = @('.artifacts/line-pair-controls.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res') },
  @{ Name = 'LegacyCursiveGeometry'; Args = @('.artifacts/cursive-geometry-controls.json') },
  @{ Name = 'LegacyCursivePreparedInk'; Args = @('.artifacts/cursive-prepared-controls.json') },
  @{ Name = 'LegacyCursiveCategory'; Args = @('.artifacts/cursive-category-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyWordCandidate'; Args = @('--controls', '.artifacts/word-candidate-controls.json') }
  @{ Name = 'LegacyLineGroupSearch'; Args = @('.artifacts/line-group-search-controls.json') },
  @{ Name = 'LegacyIntegerWord'; Args = @('--controls', '.artifacts/integer-word-controls.json') },
  @{ Name = 'LegacyCursiveHorizontal'; Args = @('.artifacts/cursive-horizontal-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyCursiveLoops'; Args = @('.artifacts/cursive-loop-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacySparseLineEstimator'; Args = @('.artifacts/sparse-line-controls.json') },
  @{ Name = 'LegacyWordBound'; Args = @('--controls', '.artifacts/word-bound-controls.json') },
  @{ Name = 'LegacyCursiveBaseline'; Args = @('.artifacts/cursive-baseline-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyGaussianLineFit'; Args = @('.artifacts/gaussian-line-controls.json') },
  @{ Name = 'LegacyAutomaticLineEstimator'; Args = @('.artifacts/automatic-line-controls.json') },
  @{ Name = 'LegacyLinePostprocessing'; Args = @('.artifacts/line-postprocessing-controls.json') },
  @{ Name = 'LegacyCursiveLimit'; Args = @('.artifacts/cursive-limit-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyCursiveCategoryMap'; Args = @('.artifacts/cursive-category-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyCursiveOverride'; Args = @('.artifacts/cursive-override-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyCursiveFilters'; Args = @('.artifacts/cursive-filters-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyPreparedSequence'; Args = @() },
  @{ Name = 'LegacyDynamicWeights'; Args = @('--controls', '.artifacts/dynamic-weights-controls.json') },
  @{ Name = 'LegacyCursiveProducer'; Args = @('.artifacts/cursive-producer-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyCursiveLattice'; Args = @('.artifacts/legacy-cursive-lattice-controls.json') },
  @{ Name = 'LegacyPreparedNormalization'; Args = @('--controls', '.artifacts/prepared-normalization-controls.json') },
  @{ Name = 'LegacyPreparedGlyph'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyPenUpMerge'; Args = @('--controls', '.artifacts/penup-merge-controls.json') },
  @{ Name = 'LegacyWordCategory'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyGlyphTypesetGeometry'; Args = @('.artifacts/glyph-typeset-controls.json') },
  @{ Name = 'LegacyCursiveSpanProfile'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyGlyphPayload'; Args = @('.artifacts/glyph-payload-controls.json') },
  @{ Name = 'LegacySourceChronology'; Args = @('.artifacts/legacy-source-chronology-controls.json') },
  @{ Name = 'LegacyCursiveSeparation'; Args = @('.artifacts/legacy-cursive-separation-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyAcousticCandidate'; Args = @('.artifacts/legacy-acoustic-candidate-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyAcousticCache'; Args = @('.artifacts/legacy-acoustic-cache-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyWideSymbolPairs'; Args = @('.artifacts/legacy-wide-symbol-pairs-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacySymbolRange'; Args = @('.artifacts/legacy-symbol-range-controls.json') },
  @{ Name = 'LegacyResultScore'; Args = @('.artifacts/legacy-result-score-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyResultReference'; Args = @('.artifacts/legacy-result-reference-controls.json') },
  @{ Name = 'LegacySourceEnclosure'; Args = @('.artifacts/legacy-source-enclosure-controls.json') },
  @{ Name = 'LegacyOrdinarySource'; Args = @('.artifacts/legacy-ordinary-source-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyPreparedSourceCount'; Args = @() },
  @{ Name = 'LegacyNativeSeed'; Args = @('.artifacts/legacy-native-seed-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacySeedGraph'; Args = @('.artifacts/legacy-seed-graph-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyShiftZeroSeed'; Args = @('.artifacts/legacy-shift-zero-seed-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyOrdinaryCutSource'; Args = @('.artifacts/legacy-ordinary-cut-source-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyCandidateGraph'; Args = @('.artifacts/legacy-candidate-graph-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyCandidateConnections'; Args = @('.artifacts/legacy-candidate-graph-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacySymbolMask'; Args = @('.artifacts/legacy-symbol-mask-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyPreparedWordGroup'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyDeferredGraph'; Args = @('.artifacts/legacy-deferred-graph-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyLinguisticMask'; Args = @('.artifacts/legacy-linguistic-mask-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyLinguisticBinding'; Args = @('.artifacts/legacy-linguistic-binding-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyLinguisticRefresh'; Args = @('.artifacts/legacy-linguistic-refresh-controls.json') },
  @{ Name = 'LegacyWordPathBoundary'; Args = @('.artifacts/legacy-word-path-boundary-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacySourceEndpoint'; Args = @('.artifacts/legacy-source-endpoint-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyDictionaryWorkingMap'; Args = @('.artifacts/legacy-dictionary-working-map-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyInitialWordRecord'; Args = @('.artifacts/legacy-initial-word-record-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyWordHeap'; Args = @('.artifacts/legacy-word-heap-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordRouter'; Args = @('.artifacts/legacy-word-router-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordContinuation'; Args = @('.artifacts/legacy-word-continuation-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordRow'; Args = @('.artifacts/legacy-word-row-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyNeuralLineProposals'; Args = @('.artifacts/legacy-neural-line-proposals-controls.json') },
  @{ Name = 'LegacyUnknownLineGroup'; Args = @('.artifacts/legacy-unknown-line-group-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res') },
  @{ Name = 'LegacyOrdinaryLineScale'; Args = @('.artifacts/legacy-ordinary-line-scale-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res') },
  @{ Name = 'LegacyLinePostselection'; Args = @('.artifacts/legacy-line-postselection-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res') },
  @{ Name = 'LegacyLineSweep'; Args = @('.artifacts/legacy-line-sweep-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res') },
  @{ Name = 'LegacyAcousticDictionaryPermissions'; Args = @('.artifacts/legacy-acoustic-dictionary-permissions-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyDictionaryQuery'; Args = @('.artifacts/legacy-dictionary-query-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
    @{ Name = 'LegacyGraphAcousticLifecycle'; Args = @('.artifacts/legacy-graph-acoustic-lifecycle-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
    @{ Name = 'LegacyRawLineCandidates'; Args = @('.artifacts/legacy-raw-line-candidates-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
    @{ Name = 'LegacyLineOrientation'; Args = @('.artifacts/legacy-line-orientation-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
    @{ Name = 'LegacyOrientationSmoothing'; Args = @('.artifacts/legacy-orientation-smoothing-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
    @{ Name = 'LegacySourceOrientation'; Args = @('.artifacts/legacy-source-orientation-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacySymbolContext'; Args = @('.artifacts/legacy-symbol-context-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacySourceSelection'; Args = @('.artifacts/legacy-source-selection-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyWordOrdering'; Args = @('.artifacts/legacy-word-ordering-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordAlternatives'; Args = @('.artifacts/legacy-word-alternative-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacySmoothingPointKeys'; Args = @('.artifacts/legacy-smoothing-point-keys-controls.json') },
  @{ Name = 'LegacyRowAcousticInput'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyFirstAcoustic'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyFirstPreparedAcoustic'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyFirstRowContext'; Args = @('.artifacts/legacy-first-row-context-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyFirstCache'; Args = @('.artifacts/legacy-acoustic-cache-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyFreshFirstSession'; Args = @('.artifacts/legacy-acoustic-cache-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyOrdinaryRankedGroup'; Args = @('.artifacts/legacy-ranked-group-controls.json', '.artifacts/legacy-acoustic-cache-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyZeroContext'; Args = @('.artifacts/legacy-zero-context-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyGlyphLineMetric'; Args = @('.artifacts/legacy-glyph-line-metric-controls.json') },
  @{ Name = 'LegacyCharacterLineEquations'; Args = @('.artifacts/legacy-character-line-equation-controls.json') },
  @{ Name = 'LegacyCharacterLineFit'; Args = @('.artifacts/legacy-character-line-fit-controls.json') },
  @{ Name = 'LegacyCharacterLineCategories'; Args = @('.artifacts/legacy-character-line-category-controls.json') },
  @{ Name = 'LegacyCharacterLineCategoryModel'; Args = @('models/line-geometry/character-line-categories.json') },
  @{ Name = 'LegacyResultLineMetrics'; Args = @('.artifacts/legacy-result-line-metric-controls.json', 'models/line-geometry/character-line-categories.json') },
  @{ Name = 'LegacyWordContextAlternatives'; Args = @('.artifacts/legacy-word-context-alternatives-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordQuota'; Args = @('.artifacts/legacy-word-quota-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordHeapPreparation'; Args = @('.artifacts/legacy-word-heap-preparation-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordPassReset'; Args = @('.artifacts/legacy-word-pass-reset-controls.json') },
  @{ Name = 'LegacyFirstRecurrence'; Args = @('.artifacts/legacy-first-recurrence-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyFirstShortlist'; Args = @('.artifacts/legacy-first-shortlist-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyFirstTransition'; Args = @('.artifacts/legacy-first-transition-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-transition-native-actual.json') },
  @{ Name = 'LegacyUnicodeNormalizationData'; Args = @('models/text-unicode/legacy-normalization-data.json', '.artifacts/unicode9-reference/UnicodeData.txt', '.artifacts/unicode9-reference/DerivedNormalizationProps.txt') },
  @{ Name = 'LegacyAlphabetCaseLookup'; Args = @('models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-text-publication-data.json') },
  @{ Name = 'LegacyMutableCaseStreams'; Args = @('models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-normalization-data.json', '.artifacts/legacy-mutable-case-stream-controls.json', '.artifacts/unicode9-reference/NormalizationTest.txt') },
  @{ Name = 'LegacyCanonicalScratch'; Args = @('models/text-unicode/legacy-normalization-data.json', '.artifacts/unicode9-reference/NormalizationTest.txt') },
  @{ Name = 'LegacyTextGlyphPublication'; Args = @('models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', '.artifacts/legacy-text-glyph-publication-controls.json', '.artifacts/unicode9-reference/auxiliary/GraphemeBreakTest.txt', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyFirstSourceRecord'; Args = @('.artifacts/legacy-first-source-record-controls.json') },
  @{ Name = 'LegacyDelimiterQuery'; Args = @('.artifacts/legacy-delimiter-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyWordAcousticRequest'; Args = @('.artifacts/legacy-word-acoustic-request-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyTextGlyphCaller'; Args = @('.artifacts/legacy-text-glyph-caller-controls.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyGlyphRecordPool'; Args = @('.artifacts/legacy-glyph-record-pool-controls.json') },
  @{ Name = 'LegacyGlyphScoreCache'; Args = @('.artifacts/legacy-glyph-score-cache-controls.json') },
  @{ Name = 'LegacyFirstGlyphScoreProducer'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', '.artifacts/legacy-first-glyph-score-native-audit.json') },
  @{ Name = 'LegacyConfidenceLabelMap'; Args = @('.artifacts/legacy-confidence-label-map-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyOwnedClosedConfidence'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', '.artifacts/legacy-owned-closed-confidence-native-audit.json') },
  @{ Name = 'LegacyWordSymbolQualification'; Args = @('.artifacts/legacy-word-symbol-qualification-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyOrdinaryWordStartup'; Args = @('.artifacts/legacy-ordinary-word-startup-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyFirstPlainWord'; Args = @('.artifacts/legacy-first-plain-word-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacySourceAlphabetLookup'; Args = @('.artifacts/legacy-source-alphabet-lookup-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyWordAlternativeCandidates'; Args = @('.artifacts/legacy-word-alternative-candidates-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyStoredWordCompaction'; Args = @('.artifacts/legacy-stored-word-compaction-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyMainWordLanguage'; Args = @('.artifacts/legacy-main-word-language-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyMainLanguageGate'; Args = @('.artifacts/legacy-main-language-gate-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyMainLanguageContext'; Args = @('.artifacts/legacy-main-language-context-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyLaterWordCompletion'; Args = @('.artifacts/legacy-later-word-completion-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyAuxiliaryDelimiter'; Args = @('.artifacts/legacy-auxiliary-delimiter-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyAuxiliaryWordQuery'; Args = @('.artifacts/legacy-auxiliary-word-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyFirstLiteralGlyph'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', '.artifacts/legacy-first-literal-glyph-controls.json', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyFirstPrimitiveWord'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-primitive-native-captures.json') },
  @{ Name = 'LegacyFirstPrimaryProducer'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-primary-native-captures.json') },
  @{ Name = 'LegacySourceIdentifier'; Args = @('.artifacts/legacy-source-identifier-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyDictionarySymbolPreparation'; Args = @('.artifacts/legacy-dictionary-symbol-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacySamplerGroupPruning'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-sampler-group-pruning-native-audit.json') },
  @{ Name = 'LegacyFreshMainLine'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-fresh-main-line-controls.json', '.artifacts/legacy-fresh-main-line-native-audit.json') },
  @{ Name = 'LegacyFreshMainPostprocessing'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-fresh-main-postprocessing-native-audit.json') },
  @{ Name = 'LegacyFreshMainSampler'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-fresh-main-sampler-native-audit.json') },
  @{ Name = 'LegacyFirstAncestor'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-ancestor-native-audit.json') },
  @{ Name = 'LegacyMainFlagPrepass'; Args = @('.artifacts/legacy-main-flag-prepass-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyPhysicalIntervalOrder'; Args = @('.artifacts/legacy-physical-interval-controls.json') },
  @{ Name = 'LegacyFreshMainDescriptorSplit'; Args = @('.artifacts/legacy-fresh-main-split-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res') },
  @{ Name = 'LegacyCaseContext'; Args = @('models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-normalization-data.json', '.artifacts/legacy-native-case-controls.json', '.artifacts/legacy-case-context-controls.json') },
  @{ Name = 'LegacyMainSpanPreparation'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyCaseSymbolEnumeration'; Args = @('models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-symbol-controls.json') },
  @{ Name = 'LegacyCasePreparation'; Args = @('models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-preparation-controls.json') },
  @{ Name = 'LegacyCasePermissions'; Args = @('models/ru-RU/ru_RU-lk-text.lite.res', '.artifacts/legacy-case-permissions-controls.json') },
  @{ Name = 'LegacyCaseContextRewrite'; Args = @('models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-context-rewrite-controls.json') },
  @{ Name = 'LegacyCaseQuery'; Args = @('models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-query-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', '.artifacts/legacy-case-query-native-audit.json') },
  @{ Name = 'LegacyQueryScratch'; Args = @('models') },
  @{ Name = 'LegacyCaseAuxiliaryQuery'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', '.artifacts/legacy-case-aux-native-audit.json') },
  @{ Name = 'LegacyCaseSymbolBinding'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-symbol-binding-native-audit.json') },
  @{ Name = 'LegacyTaggedAggregateQuery'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-tagged-aggregate-native-audit.json') },
  @{ Name = 'LegacyCaseDictionarySession'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-session-native-audit.json') },
  @{ Name = 'LegacyMainSingleLineIngress'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-line-native-audit.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainLineSpan'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-line-span-native-audit.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainClosedPhysical'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-closed-native-audit.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainLineFlagPrepass'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-line-flag-native-audit.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyFreshCaseDictionary'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-fresh-case-native-audit.json') },
  @{ Name = 'LegacyOwnedMainRing'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-owned-main-ring-native-audit.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainNodeRecurrence'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyFirstOwnedRecognition'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-owned-native-audit.json') },
  @{ Name = 'LegacyOrdinaryGapMixture'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-ordinary-gap-mixture-native-audit.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-ordinary-gap-mixture-controls.json') },
  @{ Name = 'LegacyOrdinaryGap'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-ordinary-gap-native-audit.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-ordinary-gap-controls.json') },
  @{ Name = 'LegacyMainInitialLanguage'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyOrdinaryShear'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-ordinary-shear-native-audit.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-ordinary-shear-controls.json') },
  @{ Name = 'LegacyMainTerminalSelection'; Args = @('.artifacts/legacy-main-terminal-controls.json') },
  @{ Name = 'LegacyCompactWordFrame'; Args = @('.artifacts/legacy-compact-word-frame-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyCompactWordPredecessor'; Args = @('.artifacts/legacy-compact-word-predecessor-controls.json') },
  @{ Name = 'LegacyGlyphFinalization'; Args = @('.artifacts/legacy-glyph-finalization-controls.json') },
  @{ Name = 'LegacyMainConfidenceCalibration'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', '.artifacts/legacy-main-confidence-controls.json') },
  @{ Name = 'LegacyOwnedMainGlyphConfidence'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-owned-main-confidence-native.json') },
  @{ Name = 'LegacyOrdinaryGlyphProduction'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-ordinary-glyph-native.json') },
  @{ Name = 'LegacyRussianGlyphSymbol'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-russian-glyph-symbol-native.json') },
  @{ Name = 'LegacyCompactWordSequence'; Args = @('.artifacts/legacy-compact-word-sequence-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyFreshCaseMainSource'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-fresh-case-main-source-native.json') },
  @{ Name = 'LegacyCaseFirstWordCompletion'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-first-word-native.json') },
  @{ Name = 'LegacyMainSourceSymbolCallbacks'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-source-symbol-native.json') },
  @{ Name = 'LegacyWordQuoteNormalization'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', '.artifacts/legacy-scalar-native.bin', '.artifacts/legacy-word-quotes-native.json') },
  @{ Name = 'LegacyMainCandidateWeight'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-candidate-weight-native.json') },
  @{ Name = 'LegacyCompactWordReachability'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-compact-reachability-native.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json') },
  @{ Name = 'LegacyMainCompactMaterialization'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-compact-materialization-native.json') },
  @{ Name = 'LegacyMainGlyphBacktrackRows'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-glyph-backtrack-native.json') },
  @{ Name = 'LegacyDirectGlyphProduction'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-direct-glyph-production-native.json') },
  @{ Name = 'LegacyResidualGlyphAssociation'; Args = @('.artifacts/legacy-residual-association-native.json') },
  @{ Name = 'LegacyMainSourceGeometry'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-source-geometry-native.json') },
  @{ Name = 'LegacyMainInteriorDelimiter'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-interior-delimiter-native.json') },
  @{ Name = 'LegacyCaseMainCompactMaterialization'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-main-compact-native.json') },
  @{ Name = 'LegacyCaseGlyphBacktrackRows'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-backtrack-native.json') },
  @{ Name = 'LegacyProtocolInteriorBinding'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-protocol-interior-native.json') },
  @{ Name = 'LegacyMainResidualSource'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-residual-source-native.json') },
  @{ Name = 'LegacyFirstInteriorDelimiter'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-interior-native.json') },
  @{ Name = 'LegacyMainOwnedInput'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-owned-input-native.json') },
  @{ Name = 'LegacyFirstPlainText'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-plain-text-native.json') },
  @{ Name = 'LegacyMainLiveCompactPublication'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '1', '.artifacts/legacy-main-live-compact-native.json') },
  @{ Name = 'LegacyMainOwnedFlush'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-owned-flush-native.json') },
  @{ Name = 'LegacyMainWordPublication'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-word-publication-native.json') },
  @{ Name = 'LegacyMainTextInput'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-text-input-native.json') },
  @{ Name = 'LegacyMainDescriptorCuts'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-descriptor-cuts-native.json') },
  @{ Name = 'LegacyMainGraphAppend'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-graph-append-native.json') },
  @{ Name = 'LegacyMainFinalNodePruning'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-final-prune-native.json') },
  @{ Name = 'LegacyMainCrossGraph'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-cross-graph-native.json') },
  @{ Name = 'LegacyMainLocalMetadata'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-local-metadata-native.json') },
  @{ Name = 'LegacyMainRawPlainRecognition'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-raw-plain-native.json') },
  @{ Name = 'LegacyMainFirstCandidateCompletion'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-first-completion-native.json') },
  @{ Name = 'LegacyMainOrdinaryGlyphRefresh'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-ordinary-refresh-native.json') },
  @{ Name = 'LegacyMainSecondaryTerminal'; Args = @('.artifacts/legacy-main-secondary-terminal-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-secondary-terminal-native.json') },
  @{ Name = 'LegacyMainSecondaryStorage'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-secondary-native.json') },
  @{ Name = 'LegacyMainTextSourceInterner'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-source-interner-controls.json', '.artifacts/legacy-main-source-interner-native.json') },
  @{ Name = 'LegacyMainTextGlyphReuse'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', '.artifacts/legacy-main-text-reuse-controls.json', '.artifacts/legacy-main-text-reuse-native.json') },
  @{ Name = 'LegacyMainCompactGraph'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-compact-graph-native.json') },
  @{ Name = 'LegacyMainGlobalSourceRanges'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-source-native.json') },
  @{ Name = 'LegacyMainGlobalAcoustic'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-acoustic-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalCaseSession'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-session-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalWordPass'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-word-pass-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalResult'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-result-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalCompactReachability'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-compact-reachability-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalGlyphBacktrackRows'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-glyph-backtrack-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalGlyphFlush'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-glyph-flush-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalCompactMaterialization'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-compact-materialization-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalLivePublication'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-live-publication-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalRepeatedPublication'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-repeated-publication-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalFinalPublication'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-final-publication-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalSourceCallbacks'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-source-callbacks-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalResidual'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-residual-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalGlyphExecution'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-glyph-execution-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalFirstAlternative'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-first-alternative-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalWordUtf8'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-word-utf8-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyImportedGlyphCompletion'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-imported-glyph-completion-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json') },
  @{ Name = 'LegacyMainGlobalPublishedCleanup'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-published-cleanup-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalRawRecognition'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-raw-recognition-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyImportedSourceNames'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-imported-source-names-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalCompletion'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-completion-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json') },
  @{ Name = 'LegacyMainGlobalPublishedAlternative'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-published-alternative-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyPublishedControlImport'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-published-control-import-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyImportedWordBytes'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-imported-word-bytes-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalPublishedGlyphs'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-published-glyphs-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyControlAlphabetImport'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-control-alphabet-import-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalQuoteNormalization'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-quote-normalization-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalSourceWidth'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-source-width-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalTerminalMetadata'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-terminal-metadata-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalProtocolGlyph'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-protocol-glyph-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalOrdinaryGlyph'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-ordinary-glyph-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalGroupCosts'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-group-costs-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalTransition'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-transition-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalWordTransitions'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-word-transitions-native.json', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyMainGlobalContext'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-global-context-native.json') },
  @{ Name = 'LegacyMainSecondaryBacktrack'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-secondary-backtrack-native.json') },
  @{ Name = 'LegacyMainConfiguredAlternativeAssembly'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-configured-alternative-assembly-native.json') },
  @{ Name = 'LegacyMainConfiguredRaw'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '--synthetic-only', '.artifacts/legacy-main-configured-raw-native.json') },
  @{ Name = 'LegacyMainCandidateSource'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-candidate-source-native.json') },
  @{ Name = 'LegacyMainNoCandidateSources'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-no-candidate-sources-native.json') },
  @{ Name = 'LegacyMainConfiguredSpanEmission'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', '.artifacts/legacy-main-configured-span-emission-native.json') },
  @{ Name = 'LegacyMainConfiguredTextChoices'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', '.artifacts/legacy-main-configured-text-choices-native.json') },
  @{ Name = 'LegacyMainConfiguredSourceCompletion'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', '.artifacts/legacy-main-configured-source-completion-native.json') },
  @{ Name = 'LegacyMainConfiguredRefresh'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-configured-refresh-native.json') },
  @{ Name = 'LegacyMainSecondaryWordPublication'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-secondary-word-native.json') },
  @{ Name = 'LegacyMainSecondaryCompact'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-secondary-compact-native.json') },
  @{ Name = 'LegacyMainSecondaryReachability'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-secondary-reachability-native.json') },
  @{ Name = 'LegacyRawInkRangeOperations'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-raw-ink-range-controls.json', '.artifacts/legacy-raw-ink-range-native.json') },
  @{ Name = 'LegacyFreshTextSourceContext'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-fresh-text-source-controls.json', '.artifacts/legacy-fresh-text-source-context-native.json') },
  @{ Name = 'LegacyMainDocumentInputTransforms'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-document-input-transform-controls.json', '.artifacts/legacy-main-document-input-transforms-native.json') },
  @{ Name = 'LegacyMainDocumentCounterHandoff'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-document-counter-controls.json', '.artifacts/legacy-main-document-counter-handoff-native.json') },
  @{ Name = 'LegacyTextResultIdentifier'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-text-result-identifier-controls.json', '.artifacts/legacy-text-result-identifier-native.json') },
  @{ Name = 'LegacyMainSourceRangeUtf8'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-source-range-utf8-controls.json', '.artifacts/legacy-main-source-range-utf8-native.json') },
  @{ Name = 'LegacyMainDetachedTextDom'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-detached-text-dom-native.json') },
  @{ Name = 'LegacyLibraryTextDomTarget'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-library-text-dom-target-native.json') },
  @{ Name = 'LegacyLibraryTextDomTransport'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-library-text-dom-transport-native.json') },
  @{ Name = 'LegacyTextProcessingContext'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-text-processing-context-native.json') },
  @{ Name = 'LegacyTextPublicationScope'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-text-publication-scope-native.json') },
  @{ Name = 'LegacyOriginalSelectedPublication'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-original-selected-publication-native.json') },
  @{ Name = 'LegacyOriginalSelectedDocument'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-original-selected-document-native.json') },
  @{ Name = 'LegacyOriginalSourceHistory'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-original-source-history-native.json') },
  @{ Name = 'LegacyOriginalFieldDom'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-original-field-dom-native.json') },
  @{ Name = 'LegacyGlobalTextPublication'; Args = @('models', '.artifacts/legacy-global-text-publication-native.json') },
  @{ Name = 'LegacyGlobalOrdinaryRefresh'; Args = @('models', '.artifacts/legacy-global-ordinary-refresh-native.json') },
  @{ Name = 'LegacyGlobalTextGlyphChoices'; Args = @('models', '.artifacts/legacy-global-text-glyph-choices-native.json', 'tools/test-data/russian-recorded-words/manifest.json') },
  @{ Name = 'LegacyGlobalTextSourceWrapper'; Args = @('models', 'tools/test-data/russian-recorded-words/manifest.json', '.artifacts/legacy-global-text-source-wrapper-native.json', '.artifacts/legacy-global-text-source-wrapper-controls.json') },
  @{ Name = 'LegacyGlobalWordText'; Args = @('models', 'tools/test-data/russian-recorded-words/manifest.json', '.artifacts/legacy-global-word-text-native.json') },
  @{ Name = 'LegacyGlobalDocumentCoverage'; Args = @('models', 'tools/test-data/russian-recorded-words/manifest.json', '.artifacts/legacy-global-document-coverage-native.json') },
  @{ Name = 'LegacyGlobalDocumentSelectedSources'; Args = @('models', 'tools/test-data/russian-recorded-words/manifest.json', '.artifacts/legacy-global-selected-native.json') },
  @{ Name = 'LegacyGlobalDocumentSourceIndices'; Args = @('models', 'tools/test-data/russian-recorded-words/manifest.json', '.artifacts/legacy-global-indices-native.json') },
  @{ Name = 'LegacyGlobalControlCleanup'; Args = @('models', 'tools/test-data/russian-recorded-words/manifest.json', '.artifacts/legacy-global-cleanup-native.json') },
  @{ Name = 'LegacyGlobalInputTransforms'; Args = @('models', 'tools/test-data/russian-recorded-words/manifest.json', '.artifacts/legacy-global-transforms-native.json') },
  @{ Name = 'LegacyGlobalCounterHandoff'; Args = @('models', 'tools/test-data/russian-recorded-words/manifest.json', '.artifacts/legacy-global-counter-native.json') },
  @{ Name = 'LegacyGlobalSourceRangeUtf8'; Args = @('models', 'tools/test-data/russian-recorded-words/manifest.json', '.artifacts/legacy-global-source-range-utf8-native.json') },
  @{ Name = 'NativeEnglishResource'; Args = @('models', '.artifacts/native-english-reference.ikrpack', '.artifacts/native-english-resource-native.json') },
  @{ Name = 'ExperimentalGlobalText'; Args = @('models', '.artifacts/experimental-global-text-native.json') },
  @{ Name = 'LegacyGlobalCroppedSource'; Args = @('models', 'tools/test-data/russian-recorded-words/manifest.json', '.artifacts/legacy-global-cropped-source-native.json') },
  @{ Name = 'LegacyGlobalTimedGeometry'; Args = @('models', 'tools/test-data/russian-recorded-words/manifest.json', '.artifacts/legacy-global-timed-parent-native.json', '.artifacts/legacy-global-timed-geometry-native.json') },
  @{ Name = 'LegacyTextPublicationDocument'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-text-publication-document-native.json') },
  @{ Name = 'LegacySelectedInputRegistryReplay'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-selected-input-registry-replay-native.json') },
  @{ Name = 'LegacyOriginalInputPatchCompatibility'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-original-input-patch-compatibility-native.json') },
  @{ Name = 'LegacyDomPatchCompatibility'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-dom-patch-compatibility-native.json') },
  @{ Name = 'LegacyTextDomTreeValue'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-text-dom-tree-value-native.json') },
  @{ Name = 'LegacyContextualQuoteRules'; Args = @('.artifacts/legacy-contextual-quotes-native.json') },
  @{ Name = 'LegacyOriginalTextFieldBootstrap'; Args = @('.artifacts/legacy-original-field-bootstrap-native.json') },
  @{ Name = 'LegacyOriginalSelectedField'; Args = @('.artifacts/legacy-original-selected-field-native.json') },
  @{ Name = 'LegacyPreparedGlyphRewrite'; Args = @('models', '.artifacts/legacy-prepared-glyph-rewrite-native.json') },
  @{ Name = 'LegacySource247Wire'; Args = @('models', '.artifacts/legacy-source247-wire-native.json') },
  @{ Name = 'LegacyFirstAcoustic119Wire'; Args = @('models', '.artifacts/legacy-first-acoustic119-wire-native.json') },
  @{ Name = 'LegacyEnglishFirstSlant'; Args = @('models', '.artifacts/legacy-en-slant-controls.json', '.artifacts/legacy-en-slant-native.json') },
  @{ Name = 'LegacyEnglishNoInitialGlyph'; Args = @('models', '.artifacts/legacy-en-slant-controls.json', '.artifacts/legacy-en-noinitial-glyph-native.json') },
  @{ Name = 'LegacyEnglish119Candidates'; Args = @('models', '.artifacts/legacy-en-slant-controls.json', '.artifacts/legacy-en119-candidates-native.json') },
  @{ Name = 'LegacyEnglish119Cache'; Args = @('models', '.artifacts/legacy-en119-candidates-native.json', '.artifacts/legacy-en119-cache-native.json') },
  @{ Name = 'LegacyEnglish119Confidence'; Args = @('models', '.artifacts/legacy-en119-cache-native.json', '.artifacts/legacy-en119-confidence-native.json') },
  @{ Name = 'LegacyEnglish119ControllerCosts'; Args = @('models', '.artifacts/legacy-en119-cache-native.json', '.artifacts/legacy-en119-controller-controls.json', '.artifacts/legacy-en119-controller-native.json') },
  @{ Name = 'LegacyEnglish119Primary'; Args = @('models', '.artifacts/legacy-en119-controller-native.json', '.artifacts/legacy-en119-primary-controls.json', '.artifacts/legacy-en119-primary-native.json') },
  @{ Name = 'LegacyEnglish119Context'; Args = @('models', '.artifacts/legacy-en119-controller-native.json', '.artifacts/legacy-en119-primary-controls.json', '.artifacts/legacy-en119-context-native.json') },
  @{ Name = 'LegacyEnglish119Terminal'; Args = @('models', '.artifacts/legacy-en119-controller-native.json', '.artifacts/legacy-en119-terminal-native.json') },
  @{ Name = 'LegacyEnglish119PlainText'; Args = @('models', '.artifacts/legacy-en119-controller-native.json', '.artifacts/legacy-en119-plain-text-native.json') },
  @{ Name = 'LegacyEnglish119SourceRecords'; Args = @('models', '.artifacts/legacy-en119-controller-native.json', '.artifacts/legacy-en119-source-records-native.json') },
  @{ Name = 'LegacyEnglish119Parent'; Args = @('models', '.artifacts/legacy-en119-controller-native.json', '.artifacts/legacy-en119-parent-native.json') },
  @{ Name = 'LegacyEnglish119Postprocessing'; Args = @('models', '.artifacts/legacy-en119-controller-native.json', '.artifacts/legacy-en119-postprocessing-native.json') },
  @{ Name = 'LegacyEnglish119FinalSingle'; Args = @('models', '.artifacts/legacy-en119-final-single-blind.json', '.artifacts/legacy-en119-final-single-native.json') },
  @{ Name = 'LegacyEnglishLkPacked102'; Args = @('models', '.artifacts/legacy-en-lk-packed102-native.json') },
  @{ Name = 'LegacyEnglishLkSparse101'; Args = @('models', '.artifacts/legacy-en-lk-sparse101-native.json') },
  @{ Name = 'LegacyRussianFirstDegenerate'; Args = @('models', 'tools/LegacyRussianFirstDegenerate.Probe/normalized-controls.json', '.artifacts/legacy-russian-first-degenerate-native.json') },
  @{ Name = 'LegacyEnglishMainMasks'; Args = @('models', '.artifacts/legacy-en119-final-single-blind.json', '.artifacts/legacy-en-main119-masks-native.json') },
  @{ Name = 'LegacyEnglishMainAcoustic'; Args = @('models', '.artifacts/legacy-en119-final-single-blind.json', '.artifacts/legacy-en-main119-acoustic-native.json') },
  @{ Name = 'LegacyEnglishSparseQuery'; Args = @('models', '.artifacts/legacy-en-sparse-query-native') },
  @{ Name = 'LegacyEnglishQuerySession'; Args = @('models', '.artifacts/legacy-en-query-prep-native') },
  @{ Name = 'LegacyEnglishMainD0'; Args = @('models', '.artifacts/legacy-en119-final-single-blind.json', '.artifacts/legacy-en-main119-d0-native.json') },
  @{ Name = 'LegacyMathMixed107Raw'; Args = @('models/math/math-ak.res', '.artifacts/legacy-math-mixed107-raw-native.json') },
  @{ Name = 'LegacyEnglishLiteFirstPreprocessing'; Args = @('models', '.artifacts/legacy-en-short-controls.json', '.artifacts/legacy-en-short-native.json') },
  @{ Name = 'LegacyPreparedNetwork107'; Args = @('models', 'models/ru-RU/legacy-engine-profile.json', '.artifacts/legacy-prepared-network107-native.json') },
  @{ Name = 'LegacyMainWordRanking'; Args = @('.artifacts/legacy-main-word-ranking-native.json') },
  @{ Name = 'LegacyMainGlyphChainCleanup'; Args = @('.artifacts/legacy-main-glyph-chain-cleanup-native.json') },
  @{ Name = 'LegacyMainWordAugmentationScan'; Args = @('.artifacts/legacy-main-word-augmentation-scan-native.json') },
  @{ Name = 'LegacyMainWordSpanStorage'; Args = @('.artifacts/legacy-main-word-span-storage-native.json') },
  @{ Name = 'LegacyMainSpanScores'; Args = @('.artifacts/legacy-main-span-scores-native.json') },
  @{ Name = 'LegacyMainAugmentedWordCosts'; Args = @('.artifacts/legacy-main-augmented-word-costs-native.json') },
  @{ Name = 'LegacyMainTextChoices'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-text-choices-native.json') },
  @{ Name = 'LegacyMainConfiguredTextSourceWrapper'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-configured-text-source-wrapper-native.json') },
  @{ Name = 'LegacyMainWordSourceChoices'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-word-source-choices-native.json') },
  @{ Name = 'LegacyMainSourceLines'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-source-lines-native.json') },
  @{ Name = 'LegacyMainDocumentTextSelection'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-document-text-selection-native.json') },
  @{ Name = 'LegacyMainDocumentLineConsistency'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-document-line-consistency-native.json') },
  @{ Name = 'LegacyMainDocumentSourceCoverage'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-document-source-coverage-native.json') },
  @{ Name = 'LegacyMainDocumentCharacterCoverage'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-document-character-coverage-native.json') },
  @{ Name = 'LegacyMainDocumentSelectedSources'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-document-selected-sources-native.json') },
  @{ Name = 'LegacyMainDocumentSourceIndices'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-document-source-indices-native.json') },
  @{ Name = 'LegacyMainDocumentControlCleanup'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-document-control-cleanup-native.json') },
  @{ Name = 'LegacyRawInkCurrentView'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-raw-ink-current-view-native.json') },
  @{ Name = 'LegacyTextFieldInputContext'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-textfield-input-context-native.json') },
  @{ Name = 'LegacyMainSelectedInputWordChoices'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-selected-input-word-choices-native.json') },
  @{ Name = 'LegacyMainConfiguredSpanChain'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-configured-span-chain-native.json') },
  @{ Name = 'LegacyMainConfiguredWordSources'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-configured-word-sources-native.json') },
  @{ Name = 'LegacyMainSpanAlternativeWalk'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-span-alternative-walk-native.json') },
  @{ Name = 'LegacyMainSpanAlternativeChain'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-span-alternative-chain-native.json') },
  @{ Name = 'LegacyMainWordEquality'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-word-equality-native.json') },
  @{ Name = 'LegacyMainTextSourceWrapper'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-text-source-wrapper-native.json', '--controls', '.artifacts/legacy-main-text-source-wrapper-controls.json') },
  @{ Name = 'LegacyMainGlyphUtf8Emission'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', 'models/text-unicode/legacy-normalization-data.json', 'models/text-unicode/legacy-circled-payload-data.json', '.artifacts/legacy-main-glyph-utf8-native.json') },
  @{ Name = 'LegacyMainAlternativeAssembly'; Args = @('.artifacts/legacy-main-alternative-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-alternative-native.json') },
  @{ Name = 'LegacyMainSpanGlyphRefresh'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-span-source-native.json') },
  @{ Name = 'LegacyMainGlyphNormalizationNoOp'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-glyph-noop-native.json') },
  @{ Name = 'LegacyMainWordUtf8'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-word-utf8-native.json') },
  @{ Name = 'LegacyMainGlyphSourceMetadata'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-glyph-source-native.json') },
  @{ Name = 'LegacyMainGlyphRefreshScoring'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-refresh-scoring-native.json') },
  @{ Name = 'LegacyMainGeometricFallback'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-geometric-fallback-native.json') },
  @{ Name = 'LegacyMainResultScore'; Args = @('.artifacts/legacy-main-result-score-controls.json', '.artifacts/legacy-main-result-score-native.json') },
  @{ Name = 'LegacyMathCandidateOrdering'; Args = @('.artifacts/legacy-math-candidate-ordering-native.json', 'models/math2/math2-unwrapped.res') },
  @{ Name = 'LegacyMainPostResultCounts'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-post-counts-native.json') },
  @{ Name = 'LegacyGlyphAlternativeSelection'; Args = @('.artifacts/legacy-glyph-alternative-controls.json', '.artifacts/legacy-glyph-alternative-native.json') },
  @{ Name = 'LegacyMainFirstAlternative'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-first-alternative-native.json') },
  @{ Name = 'LegacyGraphSourceResiduals'; Args = @('.artifacts/legacy-graph-source-residual-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', '.artifacts/legacy-graph-source-residual-native.json') },
  @{ Name = 'LegacyGraphSourceReordered'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-graph-source-reordered-native.json') },
  @{ Name = 'LegacyMainWordPacket'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-main-word-packet-native.json') },
  @{ Name = 'LegacyMainSourceWidth'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-main-source-width-native.json') },
  @{ Name = 'LegacyGlyphChainProduction'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-glyph-chain-native.json') },
  @{ Name = 'LegacyCaseResultQuery'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-result-query-native.json') },
  @{ Name = 'LegacyCaseMainPass'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-main-pass-native.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '15') },
  @{ Name = 'LegacyCaseLaterWordCompletion'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-later-word-native.json') },
  @{ Name = 'LegacyMainSixtyNodeStress'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-sixty-native.json') },
  @{ Name = 'LegacyMainSourceCoordinates'; Args = @('.artifacts/legacy-main-source-coordinates-controls.json') },
  @{ Name = 'LegacyMainOwnedTerminal'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json') },
  @{ Name = 'LegacyCaseSpanKeys'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-span-keys-native-audit.json') },
  @{ Name = 'LegacyWordPeriodicNormalization'; Args = @('.artifacts/legacy-word-periodic-controls.json') },
  @{ Name = 'LegacyDelimiterEpoch'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyMainDirectWordPass'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json') },
  @{ Name = 'LegacyCompactWordRecords'; Args = @('.artifacts/legacy-compact-word-controls.json') },
  @{ Name = 'LegacyCaseMainSpan'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-main-span-native-audit.json') },
  @{ Name = 'LegacyMainTerminalSources'; Args = @('.artifacts/legacy-main-terminal-sources-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyMainTerminalInvocation'; Args = @('.artifacts/legacy-main-terminal-invocation-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json') },
  @{ Name = 'LegacyWordRingCapacity'; Args = @('.artifacts/legacy-word-ring-controls.json') },
  @{ Name = 'LegacyMainControllerPolicy'; Args = @('.artifacts/legacy-main-controller-policy-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyFirstSelectedPublication'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-selected-publication-native-audit.json') },
  @{ Name = 'LegacyMainFirstResultLines'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res', '.artifacts/legacy-main-first-lines-native-audit.json') },
  @{ Name = 'LegacyCaseAlphabetRanges'; Args = @('models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-case-alphabet-ranges-controls.json') },
  @{ Name = 'LegacyFirstProtocolOwnership'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyFreshMainPhysicalReordering'; Args = @('.artifacts/legacy-main-reordering-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res') },
  @{ Name = 'LegacyCurrentMainSource'; Args = @('.artifacts/legacy-main-reordering-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res') },
  @{ Name = 'LegacyFirstBlankContext'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-blank-context-native-audit.json') },
  @{ Name = 'LegacyMainInitialSpecial'; Args = @('.artifacts/legacy-main-initial-special-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyFirstBlankPrimary'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-blank-primary-native-audit.json') },
  @{ Name = 'LegacyNativeCase'; Args = @('models/text-unicode/legacy-case-data.json', '.artifacts/legacy-native-case-controls.json', 'models/text-unicode/legacy-text-publication-data.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res') },
  @{ Name = 'LegacyFirstBlankGeometry'; Args = @('.artifacts/legacy-first-blank-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-blank-native-audit.json') },
  @{ Name = 'LegacyFreshMainGrouping'; Args = @('.artifacts/legacy-fresh-main-grouping-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/line-geometry/line-2e3-30.res', 'models/line-geometry/line-2e3-50.res') },
  @{ Name = 'LegacyFirstReplacementGeometry'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-replacement-native-audit.json') },
  @{ Name = 'LegacyFreshMainAcoustic'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-fresh-main-acoustic-native-audit.json') },
  @{ Name = 'LegacyFirstSourceMaterialization'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/legacy-first-source-materialization-native-audit.json') },
  @{ Name = 'LegacyGraphSourceRanges'; Args = @('.artifacts/legacy-graph-source-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', '.artifacts/legacy-graph-source-native-audit.json') },
  @{ Name = 'LegacySourceEndingMetadata'; Args = @('.artifacts/legacy-source-ending-metadata-controls.json') },
  @{ Name = 'LegacyWordBuilder'; Args = @('.artifacts/legacy-word-builder-controls.json') },
  @{ Name = 'LegacyFreshFirstResultFactory'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', '.artifacts/legacy-fresh-first-result-factory-controls.json') },
  @{ Name = 'LegacyGlyphSymbolUpdate'; Args = @('.artifacts/legacy-glyph-symbol-update-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyWordParentCandidates'; Args = @('.artifacts/legacy-word-parent-candidates-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyFirstResultLine'; Args = @('.artifacts/legacy-first-result-line-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyFirstResultDeskew'; Args = @('.artifacts/legacy-first-result-deskew-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyCircledUtf8'; Args = @('models/text-unicode/legacy-circled-payload-data.json', 'models/text-unicode/legacy-normalization-data.json', '.artifacts/legacy-circled-utf8-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/text-unicode/legacy-text-publication-data.json') },
  @{ Name = 'LegacyGlyphUtf8Tokens'; Args = @('.artifacts/legacy-glyph-utf8-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyFreshFirstTextPolicy'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyFreshDirectInvocation'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyOrdinaryGlyphUtf8'; Args = @('.artifacts/legacy-ordinary-utf8-controls.json', '.artifacts/legacy-glyph-utf8-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyMainWordTransition'; Args = @('models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', '.artifacts/main-word-transition-native-audit.json', '.artifacts/main-word-transition-controls.json') },
  @{ Name = 'LegacyFirstContextPass'; Args = @('.artifacts/legacy-first-context-pass-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyFirstResultScore'; Args = @('.artifacts/legacy-first-result-score-controls.json') },
  @{ Name = 'LegacyWordHigherArc'; Args = @('.artifacts/legacy-word-higherarc-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyMainPrimaryRecombination'; Args = @('.artifacts/legacy-main-primary-recombination-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-primary-recombination-native.json') },
  @{ Name = 'LegacyMainSoftHyphen'; Args = @('.artifacts/legacy-main-soft-hyphen-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', '.artifacts/legacy-main-soft-hyphen-native.json') },
  @{ Name = 'LegacyMainLanguageDispatch'; Args = @('.artifacts/legacy-main-language-dispatch-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json', 'models/text-unicode/legacy-case-data.json', 'models/text-unicode/legacy-text-publication-data.json', '.artifacts/legacy-main-language-dispatch-native.json') },
  @{ Name = 'LegacyFirstTerminalSelection'; Args = @('.artifacts/legacy-first-terminal-controls.json', '.artifacts/legacy-first-context-pass-controls.json.actual-e0-False.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacySourceLinePublication'; Args = @('.artifacts/legacy-source-publication-controls.json') },
  @{ Name = 'LegacyTextSourceProjection'; Args = @('.artifacts/legacy-source-projection-controls.json', 'models/line-geometry/character-line-categories.json') },
  @{ Name = 'LegacyWordRejection'; Args = @('.artifacts/legacy-word-rejection-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordRejectionCollector'; Args = @('.artifacts/legacy-word-rejection-collector-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyRawPacket'; Args = @('.artifacts/legacy-raw-packet-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res') },
  @{ Name = 'LegacyInkPosition'; Args = @('.artifacts/legacy-ink-position-controls.json') },
  @{ Name = 'LegacyRawStrokeGeometry'; Args = @('.artifacts/legacy-raw-stroke-geometry-controls.json', 'models/line-geometry/character-line-categories.json') },
  @{ Name = 'LegacyWordSecondaryRecombination'; Args = @('.artifacts/legacy-word-secondary-recombination-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordSecondaryProjection'; Args = @('.artifacts/legacy-word-secondary-projection-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordSecondaryWorkspace'; Args = @('.artifacts/legacy-word-secondary-workspace-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordContinuationAlternatives'; Args = @('.artifacts/legacy-word-continuation-alternatives-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordInitialParentAlternatives'; Args = @('.artifacts/legacy-word-initial-parent-alternatives-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordAnchoredAlternatives'; Args = @('.artifacts/legacy-word-anchored-alternatives-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyWordHigherArcReset'; Args = @('.artifacts/legacy-word-higherarc-reset-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyAggregateDictionaryQuery'; Args = @('.artifacts/legacy-aggregate-dictionary-query-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyOrdinaryDictionaryQuery'; Args = @('.artifacts/legacy-ordinary-dictionary-query-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json') },
  @{ Name = 'LegacyVariableWord'; Args = @('.artifacts/legacy-variable-word-controls.json', 'models/ru-RU/ru_RU-ak-cur.lite.res', 'models/ru-RU/ru_RU-lk-text.lite.res', 'models/ru-RU/legacy-engine-profile.json', 'models/ru-RU/legacy-separation-profile.json') },
  @{ Name = 'LegacyHandwritingFontSize'; Args = @('.artifacts/font-size-controls.json') }
)
foreach ($case in $cases) {
  $name = $case.Name
  dotnet publish "tools/$name.Probe/$name.Probe.csproj" -c Release -p:PublishAot=true -o ".artifacts/$name-native"
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  $suffix = if ($IsWindows) { '.exe' } else { '' }
  $probe = "./.artifacts/$name-native/$name.Probe$suffix"
  $probeArgs = $case.Args
  & $probe @probeArgs
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  if ($name -eq 'LegacyFirstContextPass') {
    python tools/LegacyFirstContextPass.Probe/reference_controls.py --audit .artifacts/legacy-first-context-pass-controls.json.actual-e0-False.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    python tools/LegacyFirstContextPass.Probe/reference_controls.py --audit .artifacts/legacy-first-context-pass-controls.json.actual-e0-True.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFirstTransition') {
    python tools/LegacyFirstTransition.Probe/reference_controls.py --audit .artifacts/legacy-first-transition-native-actual.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFreshMainLine') {
    python tools/LegacyFreshMainLine.Probe/reference_controls.py --audit .artifacts/legacy-fresh-main-line-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFirstSourceMaterialization') {
    python tools/LegacyFirstSourceMaterialization.Probe/reference_controls.py .artifacts/legacy-first-source-materialization-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyGraphSourceRanges') {
    python tools/LegacyGraphSourceRanges.Probe/reference_controls.py --audit .artifacts/legacy-graph-source-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFirstGlyphScoreProducer') {
    python tools/LegacyFirstGlyphScoreProducer.Probe/reference_controls.py .artifacts/legacy-first-glyph-score-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyOwnedClosedConfidence') {
    python tools/LegacyOwnedClosedConfidence.Probe/reference_controls.py .artifacts/legacy-owned-closed-confidence-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFreshMainPostprocessing') {
    python tools/LegacyFreshMainPostprocessing.Probe/audit_output.py .artifacts/legacy-fresh-main-postprocessing-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacySamplerGroupPruning') {
    python tools/LegacySamplerGroupPruning.Probe/audit_output.py .artifacts/legacy-sampler-group-pruning-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFreshMainSampler') {
    python tools/LegacyFreshMainSampler.Probe/audit_output.py .artifacts/legacy-fresh-main-sampler-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFirstPrimitiveWord') {
    python tools/LegacyFirstPrimitiveWord.Probe/audit_contexts.py .artifacts/legacy-first-primitive-native-captures.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFirstPrimaryProducer') {
    python tools/LegacyFirstPrimaryProducer.Probe/audit_output.py .artifacts/legacy-first-primary-native-captures.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFirstSelectedPublication') {
    python tools/LegacyFirstSelectedPublication.Probe/audit_output.py .artifacts/legacy-first-selected-publication-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyMainFirstResultLines') {
    python tools/LegacyMainFirstResultLines.Probe/audit_output.py .artifacts/legacy-main-first-lines-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyOrdinaryShear') {
    python tools/LegacyOrdinaryShear.Probe/audit_actual.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-ordinary-shear-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyOrdinaryGap') {
    python tools/LegacyOrdinaryGap.Probe/audit_actual.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-ordinary-gap-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFirstOwnedRecognition') {
    python tools/LegacyFirstOwnedRecognition.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-first-owned-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyCaseQuery') {
    python tools/LegacyCaseQuery.Probe/audit_actual.py .artifacts/legacy-case-preparation-controls.json .artifacts/legacy-case-query-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyCaseAuxiliaryQuery') {
    python tools/LegacyCaseAuxiliaryQuery.Probe/audit_output.py .artifacts/legacy-case-aux-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyCaseSymbolBinding') {
    python tools/LegacyCaseSymbolBinding.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-case-symbol-binding-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyMainClosedPhysical') {
    python tools/LegacyMainClosedPhysical.Probe/audit_limit.py .artifacts/legacy-main-closed-native-audit.json --require-active
    if ($LASTEXITCODE -ne 0) { throw 'Closed main marker limit audit failed.' }
    python tools/LegacyMainSingleLineIngress.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json .artifacts/legacy-main-closed-native-audit.json
    if ($LASTEXITCODE -ne 0) { throw 'Closed main NN audit failed.' }
  }
  if ($name -eq 'LegacyFreshCaseDictionary') {
    python tools/LegacyFreshCaseDictionary.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-case-preparation-controls.json .artifacts/legacy-fresh-case-native-audit.json
    if ($LASTEXITCODE -ne 0) { throw 'Fresh case dictionary audit failed.' }
  }
  if ($name -eq 'LegacyMainSingleLineIngress') {
    python tools/LegacyMainSingleLineIngress.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json .artifacts/legacy-main-line-native-audit.json
    if ($LASTEXITCODE -ne 0) { throw 'Main single-line ingress audit failed.' }
  }
  if ($name -eq 'LegacyCaseDictionarySession') {
    python tools/LegacyCaseDictionarySession.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-case-preparation-controls.json .artifacts/legacy-case-session-native-audit.json
    if ($LASTEXITCODE -ne 0) { throw 'Case dictionary session audit failed.' }
  }
  if ($name -eq 'LegacyCaseFirstWordCompletion') {
    python tools/LegacyCaseFirstWordCompletion.Probe/audit_output.py models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-case-preparation-controls.json .artifacts/legacy-case-first-word-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Case first-word completion audit failed.' }
  }
  if ($name -eq 'LegacyRussianGlyphSymbol') {
    python tools/LegacyRussianGlyphSymbol.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-russian-glyph-symbol-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Russian scalar glyph-symbol audit failed.' }
  }
  if ($name -eq 'LegacyOwnedMainGlyphConfidence') {
    python tools/LegacyOwnedMainGlyphConfidence.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-owned-main-confidence-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Owned main confidence audit failed.' }
  }
  if ($name -eq 'LegacyOrdinaryGlyphProduction') {
    python tools/LegacyOrdinaryGlyphProduction.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json .artifacts/legacy-ordinary-glyph-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Ordinary glyph production audit failed.' }
  }
  if ($name -eq 'LegacyMainSourceSymbolCallbacks') {
    python tools/LegacyMainSourceSymbolCallbacks.Probe/audit_output.py .artifacts/legacy-main-source-symbol-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Main source symbol callbacks audit failed' }
  }
  if ($name -eq 'LegacyWordQuoteNormalization') {
    python tools/LegacyWordQuoteNormalization.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-scalar-native.bin .artifacts/legacy-word-quotes-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Scalar and word quotes audit failed' }
  }
  if ($name -eq 'LegacyMainCandidateWeight') {
    python tools/LegacyMainCandidateWeight.Probe/audit_output.py .artifacts/legacy-candidate-weight-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Owned main result audit failed' }
  }
  if ($name -eq 'LegacyCompactWordReachability') {
    python tools/LegacyCompactWordReachability.Probe/audit_output.py .artifacts/legacy-compact-reachability-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Owned main result audit failed' }
  }
  if ($name -eq 'LegacyMainCompactMaterialization') {
    python tools/LegacyMainCompactMaterialization.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-compact-materialization-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Owned main result audit failed' }
  }
  if ($name -eq 'LegacyMainGlyphBacktrackRows') {
    python tools/LegacyMainGlyphBacktrackRows.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-glyph-backtrack-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Owned main result audit failed' }
  }
  if ($name -eq 'LegacyDirectGlyphProduction') {
    python tools/LegacyDirectGlyphProduction.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json .artifacts/legacy-direct-glyph-production-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Owned main result audit failed' }
  }
  if ($name -eq 'LegacyResidualGlyphAssociation') {
    python tools/LegacyResidualGlyphAssociation.Probe/audit_output.py .artifacts/legacy-residual-association-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Residual association audit failed' }
  }
  if ($name -eq 'LegacyMainSourceGeometry') {
    python tools/LegacyMainSourceGeometry.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-source-geometry-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Main source geometry audit failed' }
  }
  if ($name -eq 'LegacyMainInteriorDelimiter') {
    python tools/LegacyMainInteriorDelimiter.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-interior-delimiter-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Case compact or delimiter audit failed' }
  }
  if ($name -eq 'LegacyCaseMainCompactMaterialization') {
    python tools/LegacyMainCompactMaterialization.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-case-main-compact-native.json --fresh-case
    if ($LASTEXITCODE -ne 0) { throw 'Case compact or delimiter audit failed' }
  }
  if ($name -eq 'LegacyCaseGlyphBacktrackRows') {
    python tools/LegacyCaseGlyphBacktrackRows.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-case-backtrack-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Owned glyph source binding audit failed' }
  }
  if ($name -eq 'LegacyProtocolInteriorBinding') {
    python tools/LegacyProtocolInteriorBinding.Probe/audit_output.py .artifacts/legacy-protocol-interior-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Owned glyph source binding audit failed' }
  }
  if ($name -eq 'LegacyMainResidualSource') {
    python tools/LegacyMainResidualSource.Probe/audit_output.py .artifacts/legacy-main-residual-source-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Owned residual source audit failed' }
  }
  if ($name -eq 'LegacyFirstInteriorDelimiter') {
    python tools/LegacyFirstInteriorDelimiter.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-first-interior-native.json
    if ($LASTEXITCODE -ne 0) { throw 'First interior delimiter audit failed' }
  }
  if ($name -eq 'LegacyMainOwnedInput') {
    python tools/LegacyMainOwnedInput.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-owned-input-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Main input/publication metadata audit failed' }
  }
  if ($name -eq 'LegacyFirstPlainText') {
    python tools/LegacyFirstPlainText.Probe/audit_output.py .artifacts/legacy-first-plain-text-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyFirstPlainText independent audit failed' }
  }
  if ($name -eq 'LegacyMainLiveCompactPublication') {
    python tools/LegacyMainLiveCompactPublication.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-main-live-compact-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainLiveCompactPublication independent audit failed' }
  }
  if ($name -eq 'LegacyMainOwnedFlush') {
    python tools/LegacyMainOwnedFlush.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-owned-flush-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainOwnedFlush independent audit failed' }
  }
  if ($name -eq 'LegacyMainWordPublication') {
    python tools/LegacyMainWordPublication.Probe/audit_output.py .artifacts/legacy-main-word-publication-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainWordPublication independent audit failed' }
  }
  if ($name -eq 'LegacyMainFinalNodePruning') {
    python tools/LegacyMainFinalNodePruning.Probe/audit_output.py .artifacts/legacy-main-final-prune-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainFinalNodePruning independent audit failed' }
  }
  if ($name -eq 'LegacyMainCrossGraph') {
    python tools/LegacyMainCrossGraph.Probe/audit_output.py .artifacts/legacy-main-cross-graph-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainCrossGraph independent audit failed' }
  }
  if ($name -eq 'LegacyMainLocalMetadata') {
    python tools/LegacyMainLocalMetadata.Probe/audit_output.py .artifacts/legacy-main-local-metadata-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainLocalMetadata independent audit failed' }
  }
  if ($name -eq 'LegacyMainGraphAppend') {
    python tools/LegacyMainGraphAppend.Probe/audit_output.py .artifacts/legacy-main-graph-append-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGraphAppend independent audit failed' }
  }
  if ($name -eq 'LegacyMainDescriptorCuts') {
    python tools/LegacyMainDescriptorCuts.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-descriptor-cuts-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainDescriptorCuts independent audit failed' }
  }
  if ($name -eq 'LegacyMainTextInput') {
    python tools/LegacyMainTextInput.Probe/audit_output.py .artifacts/legacy-main-text-input-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Main multiword input audit failed' }
  }
  if ($name -eq 'LegacyMainGlyphNormalizationNoOp') {
    python tools/LegacyMainGlyphNormalizationNoOp.Probe/audit_output.py .artifacts/legacy-main-glyph-noop-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlyphNormalizationNoOp independent audit failed' }
  }
  if ($name -eq 'LegacyMainSecondaryTerminal') {
    python tools/LegacyMainSecondaryTerminal.Probe/audit_output.py .artifacts/legacy-main-secondary-terminal-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSecondaryTerminal independent audit failed' }
  }
  if ($name -eq 'LegacyMainSecondaryStorage') {
    python tools/LegacyMainSecondaryStorage.Probe/audit_output.py .artifacts/legacy-main-secondary-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSecondaryStorage independent audit failed' }
  }
  if ($name -eq 'LegacyMainTextSourceInterner') {
    python tools/LegacyMainTextSourceInterner.Probe/audit_output.py .artifacts/legacy-main-source-interner-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainTextSourceInterner independent audit failed' }
  }
  if ($name -eq 'LegacyMainTextGlyphReuse') {
    python tools/LegacyMainTextGlyphReuse.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/text-unicode/legacy-text-publication-data.json .artifacts/legacy-main-text-reuse-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainTextGlyphReuse independent audit failed' }
  }
  if ($name -eq 'LegacyMainCompactGraph') {
    python tools/LegacyMainCompactGraph.Probe/audit_output.py .artifacts/legacy-main-compact-graph-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainCompactGraph independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalSourceRanges') {
    python tools/LegacyMainGlobalSourceRanges.Probe/audit_output.py .artifacts/legacy-main-global-source-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalSourceRanges independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalAcoustic') {
    python tools/LegacyMainGlobalAcoustic.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json .artifacts/legacy-main-global-acoustic-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalAcoustic independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalCaseSession') {
    python tools/LegacyMainGlobalCaseSession.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-case-preparation-controls.json .artifacts/legacy-main-global-acoustic-native.json .artifacts/legacy-main-global-session-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalCaseSession independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalWordPass') {
    python tools/LegacyMainGlobalWordPass.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-case-preparation-controls.json .artifacts/legacy-main-global-word-pass-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalWordPass independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalResult') {
    python tools/LegacyMainGlobalResult.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-main-global-result-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalResult independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalCompactReachability') {
    python tools/LegacyMainGlobalCompactReachability.Probe/audit_output.py .artifacts/legacy-main-global-compact-reachability-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalCompactReachability independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalGlyphBacktrackRows') {
    python tools/LegacyMainGlobalGlyphBacktrackRows.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-global-glyph-backtrack-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalGlyphBacktrackRows independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalGlyphFlush') {
    python tools/LegacyMainGlobalGlyphFlush.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-global-glyph-flush-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalGlyphFlush independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalCompactMaterialization') {
    python tools/LegacyMainGlobalCompactMaterialization.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-main-global-compact-materialization-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalCompactMaterialization independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalLivePublication') {
    python tools/LegacyMainGlobalLivePublication.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-main-global-live-publication-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalLivePublication independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalRepeatedPublication') {
    python tools/LegacyMainGlobalRepeatedPublication.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-case-preparation-controls.json .artifacts/legacy-main-global-repeated-publication-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalRepeatedPublication independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalFinalPublication') {
    python tools/LegacyMainGlobalFinalPublication.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-main-global-final-publication-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalFinalPublication independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalSourceCallbacks') {
    python tools/LegacyMainGlobalSourceCallbacks.Probe/audit_output.py .artifacts/legacy-main-global-source-callbacks-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalSourceCallbacks independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalResidual') {
    python tools/LegacyMainGlobalResidual.Probe/audit_output.py .artifacts/legacy-main-global-residual-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalResidual independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalGlyphExecution') {
    python tools/LegacyMainGlobalGlyphExecution.Probe/audit_output.py .artifacts/legacy-main-global-glyph-execution-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalGlyphExecution independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalFirstAlternative') {
    python tools/LegacyMainGlobalFirstAlternative.Probe/audit_output.py .artifacts/legacy-main-global-first-alternative-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalFirstAlternative independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalWordUtf8') {
    python tools/LegacyMainGlobalWordUtf8.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-global-word-utf8-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalWordUtf8 independent audit failed' }
  }
  if ($name -eq 'LegacyImportedGlyphCompletion') {
    python tools/LegacyImportedGlyphCompletion.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-imported-glyph-completion-native.json models/text-unicode/legacy-circled-payload-data.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyImportedGlyphCompletion independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalPublishedCleanup') {
    python tools/LegacyMainGlobalPublishedCleanup.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-global-published-cleanup-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalPublishedCleanup independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalRawRecognition') {
    python tools/LegacyMainGlobalRawRecognition.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-global-raw-recognition-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalRawRecognition independent audit failed' }
  }
  if ($name -eq 'LegacyImportedSourceNames') {
    python tools/LegacyImportedSourceNames.Probe/audit_output.py models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-main-imported-source-names-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyImportedSourceNames independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalCompletion') {
    python tools/LegacyMainGlobalCompletion.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-global-completion-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalCompletion independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalPublishedAlternative') {
    python tools/LegacyMainGlobalPublishedAlternative.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-global-published-alternative-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalPublishedAlternative independent audit failed' }
  }
  if ($name -eq 'LegacyPublishedControlImport') {
    python tools/LegacyPublishedControlImport.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-published-control-import-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyPublishedControlImport independent audit failed' }
  }
  if ($name -eq 'LegacyImportedWordBytes') {
    python tools/LegacyImportedWordBytes.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-imported-word-bytes-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyImportedWordBytes independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalPublishedGlyphs') {
    python tools/LegacyMainGlobalPublishedGlyphs.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-global-published-glyphs-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalPublishedGlyphs independent audit failed' }
  }
  if ($name -eq 'LegacyControlAlphabetImport') {
    python tools/LegacyControlAlphabetImport.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-control-alphabet-import-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyControlAlphabetImport independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalQuoteNormalization') {
    python tools/LegacyMainGlobalQuoteNormalization.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-global-quote-normalization-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalQuoteNormalization independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalSourceWidth') {
    python tools/LegacyMainGlobalSourceWidth.Probe/audit_output.py .artifacts/legacy-main-global-source-width-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalSourceWidth independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalTerminalMetadata') {
    python tools/LegacyMainGlobalTerminalMetadata.Probe/audit_output.py models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-case-preparation-controls.json .artifacts/legacy-main-global-terminal-metadata-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalTerminalMetadata independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalProtocolGlyph') {
    python tools/LegacyMainGlobalProtocolGlyph.Probe/audit_output.py .artifacts/legacy-main-global-protocol-glyph-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalProtocolGlyph independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalOrdinaryGlyph') {
    python tools/LegacyMainGlobalOrdinaryGlyph.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json .artifacts/legacy-main-global-ordinary-glyph-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalOrdinaryGlyph independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalGroupCosts') {
    python tools/LegacyMainGlobalGroupCosts.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json .artifacts/legacy-main-global-group-costs-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalGroupCosts independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalTransition') {
    python tools/LegacyMainGlobalTransition.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json .artifacts/legacy-main-global-transition-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalTransition independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalWordTransitions') {
    python tools/LegacyMainGlobalWordTransitions.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json .artifacts/legacy-main-global-transition-native.json .artifacts/legacy-main-global-word-transitions-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalWordTransitions independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlobalContext') {
    python tools/LegacyMainGlobalContext.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json .artifacts/legacy-main-global-context-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlobalContext independent audit failed' }
  }
  if ($name -eq 'LegacyMainSecondaryBacktrack') {
    python tools/LegacyMainSecondaryBacktrack.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-secondary-backtrack-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSecondaryBacktrack independent audit failed' }
  }
  if ($name -eq 'LegacyMainConfiguredAlternativeAssembly') {
    python tools/LegacyMainConfiguredAlternativeAssembly.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-configured-alternative-assembly-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainConfiguredAlternativeAssembly independent audit failed' }
  }
  if ($name -eq 'LegacyMainConfiguredRaw') {
    python tools/LegacyMainConfiguredRaw.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-configured-raw-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainConfiguredRaw independent audit failed' }
  }
  if ($name -eq 'LegacyMainCandidateSource') {
    python tools/LegacyMainCandidateSource.Probe/audit_output.py .artifacts/legacy-main-candidate-source-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainCandidateSource independent audit failed' }
  }
  if ($name -eq 'LegacyMainNoCandidateSources') {
    python tools/LegacyMainNoCandidateSources.Probe/audit_output.py .artifacts/legacy-main-no-candidate-sources-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainNoCandidateSources independent audit failed' }
  }
  if ($name -eq 'LegacyMainConfiguredSpanEmission') {
    python tools/LegacyMainConfiguredSpanEmission.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-configured-span-emission-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainConfiguredSpanEmission independent audit failed' }
  }
  if ($name -eq 'LegacyMainConfiguredTextChoices') {
    python tools/LegacyMainConfiguredTextChoices.Probe/audit_output.py .artifacts/legacy-main-configured-text-choices-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainConfiguredTextChoices independent audit failed' }
  }
  if ($name -eq 'LegacyMainConfiguredSourceCompletion') {
    python tools/LegacyMainConfiguredSourceCompletion.Probe/audit_output.py .artifacts/legacy-main-configured-source-completion-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainConfiguredSourceCompletion independent audit failed' }
  }
  if ($name -eq 'LegacyMainConfiguredRefresh') {
    python tools/LegacyMainConfiguredRefresh.Probe/audit_output.py .artifacts/legacy-main-configured-refresh-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainConfiguredRefresh independent audit failed' }
  }
  if ($name -eq 'LegacyMainSecondaryWordPublication') {
    python tools/LegacyMainSecondaryWordPublication.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-secondary-word-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSecondaryWordPublication independent audit failed' }
    python tools/LegacyMainSecondaryWordPublication.Probe/audit_packets.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-secondary-word-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSecondaryWordPublication packet audit failed' }
  }
  if ($name -eq 'LegacyMainSecondaryCompact') {
    python tools/LegacyMainSecondaryCompact.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-main-secondary-compact-native.json --fresh-case
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSecondaryCompact independent audit failed' }
  }
  if ($name -eq 'LegacyMainSecondaryReachability') {
    python tools/LegacyMainSecondaryReachability.Probe/audit_output.py .artifacts/legacy-main-secondary-reachability-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSecondaryReachability independent audit failed' }
  }
  if ($name -eq 'LegacyRawInkRangeOperations') {
    python tools/LegacyRawInkRangeOperations.Probe/audit_output.py .artifacts/legacy-raw-ink-range-controls.json .artifacts/legacy-raw-ink-range-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyRawInkRangeOperations independent audit failed' }
  }
  if ($name -eq 'LegacyFreshTextSourceContext') {
    python tools/LegacyFreshTextSourceContext.Probe/audit_output.py .artifacts/legacy-fresh-text-source-context-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyFreshTextSourceContext independent audit failed' }
  }
  if ($name -eq 'LegacyMainDocumentInputTransforms') {
    python tools/LegacyMainDocumentInputTransforms.Probe/audit_output.py .artifacts/legacy-main-document-input-transforms-native.json .artifacts/legacy-main-document-input-transform-controls.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainDocumentInputTransforms independent audit failed' }
  }
  if ($name -eq 'LegacyMainDocumentCounterHandoff') {
    python tools/LegacyMainDocumentCounterHandoff.Probe/audit_output.py .artifacts/legacy-main-document-counter-handoff-native.json .artifacts/legacy-main-document-counter-controls.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainDocumentCounterHandoff independent audit failed' }
  }
  if ($name -eq 'LegacyTextResultIdentifier') {
    python tools/LegacyTextResultIdentifier.Probe/audit_output.py .artifacts/legacy-text-result-identifier-native.json .artifacts/legacy-text-result-identifier-controls.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyTextResultIdentifier independent audit failed' }
  }
  if ($name -eq 'LegacyMainSourceRangeUtf8') {
    python tools/LegacyMainSourceRangeUtf8.Probe/audit_output.py .artifacts/legacy-main-source-range-utf8-native.json .artifacts/legacy-main-source-range-utf8-controls.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSourceRangeUtf8 independent audit failed' }
  }
  if ($name -eq 'LegacyMainDetachedTextDom') {
    python tools/LegacyMainDetachedTextDom.Probe/audit_output.py .artifacts/legacy-main-detached-text-dom-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainDetachedTextDom independent audit failed' }
  }
  if ($name -eq 'LegacyLibraryTextDomTarget') {
    python tools/LegacyLibraryTextDomTarget.Probe/audit_output.py .artifacts/legacy-library-text-dom-target-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyLibraryTextDomTarget independent audit failed' }
  }
  if ($name -eq 'LegacyLibraryTextDomTransport') {
    python tools/LegacyLibraryTextDomTransport.Probe/audit_output.py .artifacts/legacy-library-text-dom-transport-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyLibraryTextDomTransport independent audit failed' }
  }
  if ($name -eq 'LegacyTextProcessingContext') {
    python tools/LegacyTextProcessingContext.Probe/audit_output.py .artifacts/legacy-text-processing-context-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyTextProcessingContext independent audit failed' }
  }
  if ($name -eq 'LegacyTextPublicationScope') {
    python tools/LegacyTextPublicationScope.Probe/audit_output.py .artifacts/legacy-text-publication-scope-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyTextPublicationScope independent audit failed' }
  }
  if ($name -eq 'LegacyOriginalSelectedPublication') {
    python tools/LegacyOriginalSelectedPublication.Probe/audit_output.py .artifacts/legacy-original-selected-publication-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyOriginalSelectedPublication independent audit failed' }
  }
  if ($name -eq 'LegacyOriginalSelectedDocument') {
    python tools/LegacyOriginalSelectedDocument.Probe/audit_output.py .artifacts/legacy-original-selected-document-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyOriginalSelectedDocument independent audit failed' }
    if ((Get-Content .artifacts/legacy-original-selected-document-native.json.stable.sha256) -ne (Get-Content .artifacts/legacy-original-selected-document-managed.json.stable.sha256)) { throw 'Original-selected document Managed/Native stable parity failed' }
  }
  if ($name -eq 'LegacyOriginalSourceHistory') {
    python tools/LegacyOriginalSourceHistory.Probe/audit_output.py .artifacts/legacy-original-source-history-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyOriginalSourceHistory independent audit failed' }
    if ((Get-Content .artifacts/legacy-original-source-history-native.json.stable.sha256) -ne (Get-Content .artifacts/legacy-original-source-history-managed.json.stable.sha256)) { throw 'Original-selected document Managed/Native stable parity failed' }
  }
  if ($name -eq 'LegacyOriginalFieldDom') {
    python tools/LegacyOriginalFieldDom.Probe/audit_output.py .artifacts/legacy-original-field-dom-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyOriginalFieldDom independent audit failed' }
    if ((Get-Content .artifacts/legacy-original-field-dom-native.json.stable.sha256) -ne (Get-Content .artifacts/legacy-original-field-dom-managed.json.stable.sha256)) { throw 'Original-selected document Managed/Native stable parity failed' }
  }
  if ($name -eq 'LegacyGlobalTextPublication') {
    python -I tools/LegacyGlobalTextPublication.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-global-text-publication-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Global pre-NN publication native oracle failed' }
    if ((Get-FileHash .artifacts/legacy-global-text-publication-native.json).Hash -ne (Get-FileHash .artifacts/legacy-global-text-publication-managed.json).Hash) { throw 'Global pre-NN publication Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyGlobalOrdinaryRefresh') {
    python -I tools/LegacyGlobalOrdinaryRefresh.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json .artifacts/legacy-global-ordinary-refresh-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Global pre-NN publication native oracle failed' }
    if ((Get-FileHash .artifacts/legacy-global-ordinary-refresh-native.json).Hash -ne (Get-FileHash .artifacts/legacy-global-ordinary-refresh-managed.json).Hash) { throw 'Global pre-NN publication Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyGlobalTextGlyphChoices') {
    python -I tools/LegacyGlobalTextGlyphChoices.Probe/audit_output.py models .artifacts/legacy-global-text-glyph-choices-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Global glyph choices native oracle failed' }
    if ((Get-FileHash .artifacts/legacy-global-text-glyph-choices-native.json).Hash -ne (Get-FileHash .artifacts/legacy-global-text-glyph-choices-managed.json).Hash) { throw 'Global glyph choices Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyGlobalTextSourceWrapper') {
    python -I tools/LegacyGlobalTextSourceWrapper.Probe/audit_output.py models .artifacts/legacy-global-text-source-wrapper-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyGlobalTextSourceWrapper independent audit failed' }
    if ((Get-FileHash .artifacts/legacy-global-text-source-wrapper-native.json).Hash -ne (Get-FileHash .artifacts/legacy-global-text-source-wrapper-managed.json).Hash) { throw 'LegacyGlobalTextSourceWrapper Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyGlobalWordText') {
    python -I tools/LegacyGlobalWordText.Probe/audit_output.py models .artifacts/legacy-global-text-source-wrapper-native.json .artifacts/legacy-global-word-text-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyGlobalWordText independent audit failed' }
    if ((Get-FileHash .artifacts/legacy-global-word-text-native.json).Hash -ne (Get-FileHash .artifacts/legacy-global-word-text-managed.json).Hash) { throw 'LegacyGlobalWordText Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyGlobalDocumentCoverage') {
    python -I tools/LegacyGlobalDocumentCoverage.Probe/audit_output.py models .artifacts/legacy-global-text-source-wrapper-native.json .artifacts/legacy-global-word-text-native.json .artifacts/legacy-global-document-coverage-native.json .artifacts/legacy-global-document-coverage-managed.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyGlobalDocumentCoverage independent audit/parity failed' }
  }
  if ($name -eq 'LegacyGlobalDocumentSelectedSources') {
    python -I tools/LegacyGlobalDocumentSelectedSources.Probe/audit_output.py models .artifacts/legacy-global-text-source-wrapper-native.json .artifacts/legacy-global-word-text-native.json .artifacts/legacy-global-document-coverage-native.json .artifacts/legacy-global-selected-native.json .artifacts/legacy-global-selected-managed.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyGlobalDocumentSelectedSources independent audit/parity failed' }
  }
  if ($name -eq 'LegacyGlobalDocumentSourceIndices') {
    python -I tools/LegacyGlobalDocumentSourceIndices.Probe/audit_output.py models .artifacts/legacy-global-text-source-wrapper-native.json .artifacts/legacy-global-word-text-native.json .artifacts/legacy-global-document-coverage-native.json .artifacts/legacy-global-selected-native.json .artifacts/legacy-global-indices-native.json .artifacts/legacy-global-indices-managed.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyGlobalDocumentSourceIndices independent audit/parity failed' }
  }
  if ($name -eq 'LegacyGlobalControlCleanup') {
    python -I tools/LegacyGlobalControlCleanup.Probe/audit_output.py models .artifacts/legacy-global-text-source-wrapper-native.json .artifacts/legacy-global-word-text-native.json .artifacts/legacy-global-document-coverage-native.json .artifacts/legacy-global-selected-native.json .artifacts/legacy-global-indices-native.json .artifacts/legacy-global-cleanup-native.json .artifacts/legacy-global-cleanup-managed.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyGlobalControlCleanup independent audit/parity failed' }
  }
  if ($name -eq 'LegacyGlobalInputTransforms') {
    python -I tools/LegacyGlobalInputTransforms.Probe/audit_output.py models .artifacts/legacy-global-text-source-wrapper-native.json .artifacts/legacy-global-word-text-native.json .artifacts/legacy-global-document-coverage-native.json .artifacts/legacy-global-selected-native.json .artifacts/legacy-global-indices-native.json .artifacts/legacy-global-cleanup-native.json .artifacts/legacy-global-transforms-native.json .artifacts/legacy-global-transforms-managed.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyGlobalInputTransforms independent audit/parity failed' }
  }
  if ($name -eq 'NativeEnglishResource') {
    python -I -B tools/NativeEnglishResource.Probe/audit_output.py .artifacts/native-english-resource-native.json --parity .artifacts/native-english-resource-managed.json
    if ($LASTEXITCODE -ne 0) { throw 'NativeEnglishResource independent reference/parity failed' }
  }
  if ($name -eq 'ExperimentalGlobalText') {
    python -I -B tools/ExperimentalGlobalText.Probe/audit_output.py .artifacts/experimental-global-text-native.json --parity .artifacts/experimental-global-text-managed.json
    if ($LASTEXITCODE -ne 0) { throw 'ExperimentalGlobalText admission/parity failed' }
  }
  if ($name -eq 'LegacyGlobalSourceRangeUtf8') {
    python -I -B tools/LegacyGlobalSourceRangeUtf8.Probe/audit_output.py .artifacts/legacy-global-source-range-utf8-native.json .artifacts/legacy-global-word-text-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyGlobalSourceRangeUtf8 independent audit failed' }
    if ((Get-FileHash .artifacts/legacy-global-source-range-utf8-native.json).Hash -ne (Get-FileHash .artifacts/legacy-global-source-range-utf8-managed.json).Hash) { throw 'LegacyGlobalSourceRangeUtf8 Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyGlobalCounterHandoff') {
    python -I tools/LegacyGlobalCounterHandoff.Probe/audit_output.py models .artifacts/legacy-global-text-source-wrapper-native.json .artifacts/legacy-global-word-text-native.json .artifacts/legacy-global-document-coverage-native.json .artifacts/legacy-global-selected-native.json .artifacts/legacy-global-indices-native.json .artifacts/legacy-global-cleanup-native.json .artifacts/legacy-global-transforms-native.json .artifacts/legacy-global-counter-native.json .artifacts/legacy-global-counter-managed.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyGlobalCounterHandoff independent audit/parity failed' }
  }
  if ($name -eq 'LegacyGlobalCroppedSource') {
    python -I tools/LegacyGlobalCroppedSource.Probe/audit_output.py models tools/test-data/russian-recorded-words/manifest.json .artifacts/legacy-global-text-source-wrapper-native.json .artifacts/legacy-global-word-text-native.json .artifacts/legacy-global-cropped-source-native.json .artifacts/legacy-global-cropped-source-managed.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyGlobalCroppedSource independent audit/parity failed' }
  }
  if ($name -eq 'LegacyGlobalTimedGeometry') {
    python -I tools/LegacyGlobalTimedGeometry.Probe/audit_output.py tools/test-data/russian-recorded-words/manifest.json .artifacts/legacy-global-timed-parent-native.json .artifacts/legacy-global-timed-geometry-native.json --native .artifacts/legacy-global-timed-geometry-managed.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyGlobalTimedGeometry independent audit/parity failed' }
    if ((Get-FileHash .artifacts/legacy-global-timed-parent-native.json).Hash -ne (Get-FileHash .artifacts/legacy-global-timed-parent-managed.json).Hash) { throw 'LegacyGlobalTimedGeometry parent Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyTextPublicationDocument') {
    python tools/LegacyTextPublicationDocument.Probe/audit_output.py .artifacts/legacy-text-publication-document-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyTextPublicationDocument independent audit failed' }
  }
  if ($name -eq 'LegacySelectedInputRegistryReplay') {
    python tools/LegacySelectedInputRegistryReplay.Probe/audit_output.py .artifacts/legacy-selected-input-registry-replay-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacySelectedInputRegistryReplay independent audit failed' }
  }
  if ($name -eq 'LegacyOriginalInputPatchCompatibility') {
    python tools/LegacyOriginalInputPatchCompatibility.Probe/audit_output.py .artifacts/legacy-original-input-patch-compatibility-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyOriginalInputPatchCompatibility independent audit failed' }
  }
  if ($name -eq 'LegacyDomPatchCompatibility') {
    python tools/LegacyDomPatchCompatibility.Probe/audit_output.py .artifacts/legacy-dom-patch-compatibility-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyDomPatchCompatibility independent audit failed' }
  }
  if ($name -eq 'LegacyTextDomTreeValue') {
    python tools/LegacyTextDomTreeValue.Probe/audit_output.py .artifacts/legacy-text-dom-tree-value-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyTextDomTreeValue independent audit failed' }
  }
  if ($name -eq 'LegacyContextualQuoteRules') {
    python tools/LegacyContextualQuoteRules.Probe/audit_output.py .artifacts/legacy-contextual-quotes-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyContextualQuoteRules independent audit failed' }
  }
  if ($name -eq 'LegacyOriginalTextFieldBootstrap') {
    python tools/LegacyOriginalTextFieldBootstrap.Probe/audit_output.py .artifacts/legacy-original-field-bootstrap-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyOriginalTextFieldBootstrap independent audit failed' }
  }
  if ($name -eq 'LegacyOriginalSelectedField') {
    python tools/LegacyOriginalSelectedField.Probe/audit_output.py .artifacts/legacy-original-selected-field-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyOriginalSelectedField independent audit failed' }
  }
  if ($name -eq 'LegacyPreparedGlyphRewrite') {
    python tools/LegacyPreparedGlyphRewrite.Probe/audit_output.py models .artifacts/legacy-prepared-glyph-rewrite-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyPreparedGlyphRewrite independent audit failed' }
  }
  if ($name -eq 'LegacySource247Wire') {
    python tools/LegacySource247Wire.Probe/audit_output.py models .artifacts/legacy-source247-wire-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacySource247Wire independent audit failed' }
  }
  if ($name -eq 'LegacyFirstAcoustic119Wire') {
    python tools/LegacyFirstAcoustic119Wire.Probe/audit_output.py models .artifacts/legacy-first-acoustic119-wire-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyFirstAcoustic119Wire independent audit failed' }
  }
  if ($name -eq 'LegacyEnglishFirstSlant') {
    python tools/LegacyEnglishFirstSlant.Probe/audit_output.py models .artifacts/legacy-en-slant-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglishFirstSlant independent audit failed' }
  }
  if ($name -eq 'LegacyEnglishNoInitialGlyph') {
    python tools/LegacyEnglishNoInitialGlyph.Probe/audit_output.py models .artifacts/legacy-en-noinitial-glyph-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglishNoInitialGlyph independent audit failed' }
  }
  if ($name -eq 'LegacyEnglish119Candidates') {
    python tools/LegacyEnglish119Candidates.Probe/audit_output.py models .artifacts/legacy-en119-candidates-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglish119Candidates independent audit failed' }
  }
  if ($name -eq 'LegacyEnglish119Cache') {
    python tools/LegacyEnglish119Cache.Probe/audit_output.py models .artifacts/legacy-en119-cache-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglish119Cache independent audit failed' }
  }
  if ($name -eq 'LegacyEnglish119Confidence') {
    python tools/LegacyEnglish119Confidence.Probe/audit_output.py models .artifacts/legacy-en119-cache-native.json .artifacts/legacy-en119-confidence-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglish119Confidence independent audit failed' }
  }
  if ($name -eq 'LegacyEnglish119ControllerCosts') {
    python tools/LegacyEnglish119ControllerCosts.Probe/audit_output.py models .artifacts/legacy-en119-candidates-native.json .artifacts/legacy-en119-cache-native.json .artifacts/legacy-en119-controller-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglish119ControllerCosts independent audit failed' }
  }
  if ($name -eq 'LegacyEnglish119Primary') {
    python tools/LegacyEnglish119Primary.Probe/audit_output.py models .artifacts/legacy-en119-candidates-native.json .artifacts/legacy-en119-controller-native.json .artifacts/legacy-en119-primary-controls.json .artifacts/legacy-en119-primary-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglish119Primary independent audit failed' }
  }
  if ($name -eq 'LegacyEnglish119Context') {
    python tools/LegacyEnglish119Context.Probe/audit_output.py models .artifacts/legacy-en119-candidates-native.json .artifacts/legacy-en119-controller-native.json .artifacts/legacy-en119-primary-controls.json .artifacts/legacy-en119-primary-native.json .artifacts/legacy-en119-context-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglish119Context independent audit failed' }
  }
  if ($name -eq 'LegacyEnglish119Terminal') {
    python tools/LegacyEnglish119Terminal.Probe/audit_output.py models .artifacts/legacy-en119-context-native.json .artifacts/legacy-en119-terminal-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglish119Terminal independent audit failed' }
  }
  if ($name -eq 'LegacyEnglish119PlainText') {
    python -I tools/LegacyEnglish119PlainText.Probe/audit_output.py models .artifacts/legacy-en119-candidates-native.json .artifacts/legacy-en119-controller-native.json .artifacts/legacy-en119-confidence-native.json .artifacts/legacy-en119-primary-native.json .artifacts/legacy-en119-terminal-native.json .artifacts/legacy-en119-plain-text-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglish119PlainText independent audit failed' }
  }
  if ($name -eq 'LegacyEnglish119SourceRecords') {
    python -I tools/LegacyEnglish119SourceRecords.Probe/audit_output.py models .artifacts/legacy-en119-controller-native.json .artifacts/legacy-en119-plain-text-native.json .artifacts/legacy-en119-source-records-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglish119SourceRecords independent audit failed' }
    if ((Get-FileHash .artifacts/legacy-en119-source-records-native.json).Hash -ne (Get-FileHash .artifacts/legacy-en119-source-records-managed.json).Hash) { throw 'LegacyEnglish119SourceRecords Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyEnglish119Parent') {
    python -I tools/LegacyEnglish119Parent.Probe/audit_output.py models .artifacts/legacy-en119-parent-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglish119Parent independent audit failed' }
    if ((Get-FileHash .artifacts/legacy-en119-parent-native.json).Hash -ne (Get-FileHash .artifacts/legacy-en119-parent-managed.json).Hash) { throw 'LegacyEnglish119Parent Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyEnglish119Postprocessing') {
    python -I tools/LegacyEnglish119Postprocessing.Probe/audit_output.py models/en-US/en_US-ak-cur.lite.res .artifacts/legacy-en119-postprocessing-native.json models/ru-RU/ru_RU-ak-cur.lite.res
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglish119Postprocessing independent audit failed' }
    if ((Get-FileHash .artifacts/legacy-en119-postprocessing-native.json).Hash -ne (Get-FileHash .artifacts/legacy-en119-postprocessing-managed.json).Hash) { throw 'LegacyEnglish119Postprocessing Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyEnglish119FinalSingle') {
    python -I -B tools/LegacyEnglish119FinalSingle.Probe/audit_output.py models .artifacts/legacy-en119-final-single-native.json .artifacts/legacy-en119-postprocessing-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglish119FinalSingle independent audit failed' }
    if ((Get-FileHash .artifacts/legacy-en119-final-single-native.json).Hash -ne (Get-FileHash .artifacts/legacy-en119-final-single-managed.json).Hash) { throw 'LegacyEnglish119FinalSingle Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyMathMixed107Raw') {
    python -I -B tools/LegacyMathMixed107Raw.Probe/audit_output.py models/math/math-ak.res .artifacts/legacy-math-mixed107-raw-native.json --reference tools/handwriting/math_legacy_mixed107_reference.py
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMathMixed107Raw independent audit failed' }
    if ((Get-FileHash .artifacts/legacy-math-mixed107-raw-native.json).Hash -ne (Get-FileHash .artifacts/legacy-math-mixed107-raw-managed.json).Hash) { throw 'LegacyMathMixed107Raw Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyRussianFirstDegenerate') {
    python -I -B tools/LegacyRussianFirstDegenerate.Probe/audit_output.py models tools/LegacyRussianFirstDegenerate.Probe/normalized-controls.json .artifacts/legacy-russian-first-degenerate-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Russian collapsed subgroup independent oracle failed' }
    if ((Get-FileHash .artifacts/legacy-russian-first-degenerate-native.json).Hash -ne (Get-FileHash .artifacts/legacy-russian-first-degenerate-managed.json).Hash) { throw 'Russian collapsed subgroup Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyEnglishMainMasks') {
    python -I -B tools/LegacyEnglishMainMasks.Probe/audit_runtime.py --models models --capture .artifacts/legacy-en-main119-masks-native.json --output .artifacts/legacy-en-main119-masks-oracle-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglishMainMasks independent raw oracle failed' }
    if ((Get-FileHash .artifacts/legacy-en-main119-masks-native.json).Hash -ne (Get-FileHash .artifacts/legacy-en-main119-masks-managed.json).Hash) { throw 'LegacyEnglishMainMasks Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyEnglishMainAcoustic') {
    if ((Get-FileHash .artifacts/legacy-en-main119-acoustic-native.json).Hash -ne (Get-FileHash .artifacts/legacy-en-main119-acoustic-managed.json).Hash) { throw 'LegacyEnglishMainAcoustic Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyEnglishQuerySession') {
    python -I -B tools/LegacyEnglishQuerySession.Probe/audit_output.py --repo . --models models --capture .artifacts/legacy-en-query-prep-native/capture.json --output .artifacts/legacy-en-query-prep-oracle-native.json
    if ($LASTEXITCODE -ne 0) { throw 'English query preparation independent oracle failed' }
    if ((Get-FileHash .artifacts/legacy-en-query-prep-native/capture.json).Hash -ne (Get-FileHash .artifacts/legacy-en-query-prep-managed/capture.json).Hash) { throw 'English query preparation Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyEnglishMainD0') {
    python -I -B tools/LegacyEnglishMainD0.Probe/audit_output.py --models models --capture .artifacts/legacy-en-main119-d0-native.json --acoustic .artifacts/legacy-en-main119-acoustic-native.json --acoustic-report .artifacts/legacy-en-main119-acoustic-oracle-native.json --output .artifacts/legacy-en-main119-d0-oracle-native.json
    if ($LASTEXITCODE -ne 0) { throw 'English D0 independent pool/map/permission oracle failed' }
    if ((Get-FileHash .artifacts/legacy-en-main119-d0-native.json).Hash -ne (Get-FileHash .artifacts/legacy-en-main119-d0-managed.json).Hash) { throw 'English D0 Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyEnglishSparseQuery') {
    python -I -B tools/LegacyEnglishSparseQuery.Probe/audit_output.py --repo . --models models --capture .artifacts/legacy-en-sparse-query-native --output .artifacts/legacy-en-sparse-query-oracle-native.json
    if ($LASTEXITCODE -ne 0) { throw 'English sparse terminal query independent oracle failed' }
    foreach ($stream in @('capture.json', 'prepared-records.bin', 'lexical-first-records.bin', 'lexical-continuation-records.bin')) {
      if ((Get-FileHash (Join-Path '.artifacts/legacy-en-sparse-query-native' $stream)).Hash -ne (Get-FileHash (Join-Path '.artifacts/legacy-en-sparse-query-managed' $stream)).Hash) { throw "English sparse terminal query Managed/Native parity failed: $stream" }
    }
  }
  if ($name -eq 'LegacyEnglishLkSparse101') {
    python -I -B tools/LegacyEnglishLkSparse101.Probe/audit_runtime.py models .artifacts/legacy-en-lk-sparse101-native.json .artifacts/legacy-en-lk-sparse101-oracle-native.json --native .artifacts/legacy-en-lk-sparse101-managed.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglishLkSparse101 independent audit/parity failed' }
  }
  if ($name -eq 'LegacyEnglishLkPacked102') {
    python -I -B tools/LegacyEnglishLkPacked102.Probe/audit_runtime.py models .artifacts/legacy-en-lk-packed102-native.json .artifacts/legacy-en-lk-packed102-reference-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglishLkPacked102 independent audit failed' }
    if ((Get-FileHash .artifacts/legacy-en-lk-packed102-native.json).Hash -ne (Get-FileHash .artifacts/legacy-en-lk-packed102-managed.json).Hash) { throw 'LegacyEnglishLkPacked102 Managed/Native parity failed' }
  }
  if ($name -eq 'LegacyEnglishLiteFirstPreprocessing') {
    python tools/LegacyEnglishLiteFirstPreprocessing.Probe/audit_output.py models .artifacts/legacy-en-short-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyEnglishLiteFirstPreprocessing independent audit failed' }
  }
  if ($name -eq 'LegacyPreparedNetwork107') {
    python tools/LegacyPreparedNetwork107.Probe/audit_output.py models .artifacts/legacy-prepared-network107-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyPreparedNetwork107 independent audit failed' }
  }
  if ($name -eq 'LegacyMainWordRanking') {
    python tools/LegacyMainWordRanking.Probe/audit_output.py .artifacts/legacy-main-word-ranking-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainWordRanking independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlyphChainCleanup') {
    python tools/LegacyMainGlyphChainCleanup.Probe/audit_output.py .artifacts/legacy-main-glyph-chain-cleanup-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlyphChainCleanup independent audit failed' }
  }
  if ($name -eq 'LegacyMainWordAugmentationScan') {
    python tools/LegacyMainWordAugmentationScan.Probe/audit_output.py .artifacts/legacy-main-word-augmentation-scan-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainWordAugmentationScan independent audit failed' }
  }
  if ($name -eq 'LegacyMainWordSpanStorage') {
    python tools/LegacyMainWordSpanStorage.Probe/audit_output.py .artifacts/legacy-main-word-span-storage-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainWordSpanStorage independent audit failed' }
  }
  if ($name -eq 'LegacyMainSpanScores') {
    python tools/LegacyMainSpanScores.Probe/audit_output.py .artifacts/legacy-main-span-scores-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSpanScores independent audit failed' }
  }
  if ($name -eq 'LegacyMainPrimaryRecombination') {
    python tools/LegacyMainPrimaryRecombination.Probe/audit_output.py .artifacts/legacy-main-primary-recombination-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Primary recombination independent audit failed' }
  }
  if ($name -eq 'LegacyMainAugmentedWordCosts') {
    python tools/LegacyMainAugmentedWordCosts.Probe/audit_output.py .artifacts/legacy-main-augmented-word-costs-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainAugmentedWordCosts independent audit failed' }
  }
  if ($name -eq 'LegacyMainTextChoices') {
    python tools/LegacyMainTextChoices.Probe/audit_output.py .artifacts/legacy-main-text-choices-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainTextChoices independent audit failed' }
  }
  if ($name -eq 'LegacyMainConfiguredTextSourceWrapper') {
    python tools/LegacyMainConfiguredTextSourceWrapper.Probe/audit_output.py .artifacts/legacy-main-configured-text-source-wrapper-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainConfiguredTextSourceWrapper independent audit failed' }
  }
  if ($name -eq 'LegacyMainWordSourceChoices') {
    python tools/LegacyMainWordSourceChoices.Probe/audit_output.py .artifacts/legacy-main-word-source-choices-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainWordSourceChoices independent audit failed' }
  }
  if ($name -eq 'LegacyMainSourceLines') {
    python tools/LegacyMainSourceLines.Probe/audit_output.py .artifacts/legacy-main-source-lines-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSourceLines independent audit failed' }
  }
  if ($name -eq 'LegacyMainDocumentTextSelection') {
    python tools/LegacyMainDocumentTextSelection.Probe/audit_output.py .artifacts/legacy-main-document-text-selection-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainDocumentTextSelection independent audit failed' }
  }
  if ($name -eq 'LegacyMainDocumentLineConsistency') {
    python tools/LegacyMainDocumentLineConsistency.Probe/audit_output.py .artifacts/legacy-main-document-line-consistency-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainDocumentLineConsistency independent audit failed' }
  }
  if ($name -eq 'LegacyMainDocumentSourceCoverage') {
    python tools/LegacyMainDocumentSourceCoverage.Probe/audit_output.py .artifacts/legacy-main-document-source-coverage-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainDocumentSourceCoverage independent audit failed' }
  }
  if ($name -eq 'LegacyMainDocumentCharacterCoverage') {
    python tools/LegacyMainDocumentCharacterCoverage.Probe/audit_output.py .artifacts/legacy-main-document-character-coverage-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainDocumentCharacterCoverage independent audit failed' }
  }
  if ($name -eq 'LegacyMainDocumentSelectedSources') {
    python tools/LegacyMainDocumentSelectedSources.Probe/audit_output.py .artifacts/legacy-main-document-selected-sources-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainDocumentSelectedSources independent audit failed' }
  }
  if ($name -eq 'LegacyMainDocumentSourceIndices') {
    python tools/LegacyMainDocumentSourceIndices.Probe/audit_output.py .artifacts/legacy-main-document-source-indices-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainDocumentSourceIndices independent audit failed' }
  }
  if ($name -eq 'LegacyMainDocumentControlCleanup') {
    python tools/LegacyMainDocumentControlCleanup.Probe/audit_output.py .artifacts/legacy-main-document-control-cleanup-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainDocumentControlCleanup independent audit failed' }
  }
  if ($name -eq 'LegacyRawInkCurrentView') {
    python tools/LegacyRawInkCurrentView.Probe/audit_output.py .artifacts/legacy-raw-ink-current-view-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyRawInkCurrentView independent audit failed' }
  }
  if ($name -eq 'LegacyTextFieldInputContext') {
    python tools/LegacyTextFieldInputContext.Probe/audit_output.py .artifacts/legacy-textfield-input-context-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyTextFieldInputContext independent audit failed' }
  }
  if ($name -eq 'LegacyMainSelectedInputWordChoices') {
    python tools/LegacyMainSelectedInputWordChoices.Probe/audit_output.py .artifacts/legacy-main-selected-input-word-choices-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSelectedInputWordChoices independent audit failed' }
  }
  if ($name -eq 'LegacyMainConfiguredSpanChain') {
    python tools/LegacyMainConfiguredSpanChain.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-configured-span-chain-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainConfiguredSpanChain independent audit failed' }
    $costArgs = @($case.Args)
    $costArgs[10] = '.artifacts/legacy-main-configured-span-costs-native.json'
    $costArgs += @('--costs', 'true')
    & $probe @costArgs
    if ($LASTEXITCODE -ne 0) { throw 'Actual span alternative cost transfer failed' }
    python tools/LegacyMainConfiguredSpanChain.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-configured-span-costs-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Actual span alternative cost audit failed' }
  }
  if ($name -eq 'LegacyMainConfiguredWordSources') {
    python tools/LegacyMainConfiguredWordSources.Probe/audit_output.py .artifacts/legacy-main-configured-word-sources-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainConfiguredWordSources independent audit failed' }
  }
  if ($name -eq 'LegacyMainSpanAlternativeWalk') {
    python tools/LegacyMainSpanAlternativeWalk.Probe/audit_output.py .artifacts/legacy-main-span-alternative-walk-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSpanAlternativeWalk independent audit failed' }
  }
  if ($name -eq 'LegacyMainSpanAlternativeChain') {
    python tools/LegacyMainSpanAlternativeChain.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-span-alternative-chain-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSpanAlternativeChain independent audit failed' }
    $costArgs = @($case.Args)
    $costArgs[10] = '.artifacts/legacy-main-span-alternative-costs-native.json'
    $costArgs += @('--costs', 'true')
    & $probe @costArgs
    if ($LASTEXITCODE -ne 0) { throw 'Actual span alternative cost transfer failed' }
    python tools/LegacyMainSpanAlternativeChain.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-span-alternative-costs-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Actual span alternative cost audit failed' }
  }
  if ($name -eq 'LegacyMainWordEquality') {
    python tools/LegacyMainWordEquality.Probe/audit_output.py .artifacts/legacy-main-word-equality-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainWordEquality independent audit failed' }
    & $probe --prepared .artifacts/legacy-main-word-equality-prepared-native.json
    if ($LASTEXITCODE -ne 0) { throw "LegacyMainWordEquality prepared native probe failed" }
    python tools/LegacyMainWordEquality.Probe/audit_output.py .artifacts/legacy-main-word-equality-prepared-native.json
    if ($LASTEXITCODE -ne 0) { throw "LegacyMainWordEquality prepared audit failed" }
  }
  if ($name -eq 'LegacyMainTextSourceWrapper') {
    python tools/LegacyMainTextSourceWrapper.Probe/audit_output.py .artifacts/legacy-main-text-source-wrapper-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainTextSourceWrapper independent audit failed' }
    $completedArgs = @($case.Args)
    $completedArgs[10] = '.artifacts/legacy-main-completed-source-wrapper-native.json'
    $completedArgs += @('--complete-sources', 'true')
    & $probe @completedArgs
    if ($LASTEXITCODE -ne 0) { throw 'Completed source wrapper failed' }
    python tools/LegacyMainTextSourceWrapper.Probe/audit_output.py .artifacts/legacy-main-completed-source-wrapper-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Completed source wrapper independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlyphUtf8Emission') {
    python tools/LegacyMainGlyphUtf8Emission.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-glyph-utf8-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlyphUtf8Emission independent audit failed' }
  }
  if ($name -eq 'LegacyMainAlternativeAssembly') {
    python tools/LegacyMainAlternativeAssembly.Probe/audit_output.py .artifacts/legacy-main-alternative-controls.json .artifacts/legacy-main-alternative-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainAlternativeAssembly independent audit failed' }
  }
  if ($name -eq 'LegacyMainSpanGlyphRefresh') {
    python tools/LegacyMainSpanGlyphRefresh.Probe/audit_output.py .artifacts/legacy-main-span-source-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainSpanGlyphRefresh independent audit failed' }
  }
  if ($name -eq 'LegacyMainOrdinaryGlyphRefresh') {
    python tools/LegacyMainOrdinaryGlyphRefresh.Probe/audit_output.py .artifacts/legacy-main-ordinary-refresh-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainOrdinaryGlyphRefresh independent audit failed' }
  }
  if ($name -eq 'LegacyMainFirstCandidateCompletion') {
    python tools/LegacyMainFirstCandidateCompletion.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-first-completion-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainFirstCandidateCompletion independent audit failed' }
  }
  if ($name -eq 'LegacyMainRawPlainRecognition') {
    python tools/LegacyMainRawPlainRecognition.Probe/audit_output.py .artifacts/legacy-main-raw-plain-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Raw main plain-word integration audit failed' }
  }
  if ($name -eq 'LegacyMainGlyphRefreshScoring') {
    python tools/LegacyMainGlyphRefreshScoring.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json .artifacts/legacy-main-refresh-scoring-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlyphRefreshScoring independent audit failed' }
  }
  if ($name -eq 'LegacyMainGlyphSourceMetadata') {
    python tools/LegacyMainGlyphSourceMetadata.Probe/audit_output.py .artifacts/legacy-main-glyph-source-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGlyphSourceMetadata independent audit failed' }
  }
  if ($name -eq 'LegacyMainWordUtf8') {
    python tools/LegacyMainWordUtf8.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-word-utf8-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainWordUtf8 independent audit failed' }
  }
  if ($name -eq 'LegacyMainGeometricFallback') {
    python tools/LegacyMainGeometricFallback.Probe/audit_output.py .artifacts/legacy-main-geometric-fallback-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainGeometricFallback independent audit failed' }
  }
  if ($name -eq 'LegacyMainResultScore') {
    python tools/LegacyMainResultScore.Probe/audit_output.py .artifacts/legacy-main-result-score-controls.json .artifacts/legacy-main-result-score-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainResultScore independent audit failed' }
  }
  if ($name -eq 'LegacyMathCandidateOrdering') {
    python tools/LegacyMathCandidateOrdering.Probe/audit_output.py .artifacts/legacy-math-candidate-ordering-native.json models/math2/math2-unwrapped.res
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMathCandidateOrdering independent audit failed' }
  }
  if ($name -eq 'LegacyMainPostResultCounts') {
    python tools/LegacyMainPostResultCounts.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-main-post-counts-native.json
    if ($LASTEXITCODE -ne 0) { throw 'LegacyMainPostResultCounts independent audit failed' }
  }
  if ($name -eq 'LegacyGlyphAlternativeSelection') {
    python tools/LegacyGlyphAlternativeSelection.Probe/reference_controls.py .artifacts/legacy-glyph-alternative-controls.json --audit .artifacts/legacy-glyph-alternative-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Glyph alternative selection independent audit failed' }
  }
  if ($name -eq 'LegacyMainFirstAlternative') {
    python tools/LegacyMainFirstAlternative.Probe/audit_output.py .artifacts/legacy-main-first-alternative-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Main first alternative snapshot audit failed' }
  }
  if ($name -eq 'LegacyGraphSourceReordered') {
    python tools/LegacyGraphSourceReordered.Probe/audit_output.py .artifacts/legacy-graph-source-reordered-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Reordered source key-table audit failed' }
  }
  if ($name -eq 'LegacyGraphSourceResiduals') {
    python tools/LegacyGraphSourceResiduals.Probe/reference_controls.py .artifacts/legacy-graph-source-residual-native.json --audit
    if ($LASTEXITCODE -ne 0) { throw 'Source residual metadata independent audit failed' }
  }
  if ($name -eq 'LegacyMainWordPacket') {
    python tools/LegacyMainWordPacket.Probe/audit_output.py .artifacts/legacy-main-word-packet-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Main input/publication metadata audit failed' }
  }
  if ($name -eq 'LegacyMainSourceWidth') {
    python tools/LegacyMainSourceWidth.Probe/audit_output.py .artifacts/legacy-main-source-width-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Main input/publication metadata audit failed' }
  }
  if ($name -eq 'LegacyGlyphChainProduction') {
    python tools/LegacyGlyphChainProduction.Probe/audit_output.py .artifacts/legacy-glyph-chain-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Glyph chain audit failed' }
  }
  if ($name -eq 'LegacyCaseMainPass') {
    python tools/LegacyCaseMainPass.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-case-preparation-controls.json .artifacts/legacy-case-main-pass-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Actual case main word-pass audit failed.' }
  }
  if ($name -eq 'LegacyCaseLaterWordCompletion') {
    python tools/LegacyCaseLaterWordCompletion.Probe/audit_output.py models/ru-RU/ru_RU-lk-text.lite.res .artifacts/legacy-case-preparation-controls.json .artifacts/legacy-case-later-word-native.json
    if ($LASTEXITCODE -ne 0) { throw 'Case later-word completion audit failed.' }
  }
  if ($name -eq 'LegacyTaggedAggregateQuery') {
    python tools/LegacyTaggedAggregateQuery.Probe/audit_output.py .artifacts/legacy-case-preparation-controls.json .artifacts/legacy-tagged-aggregate-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyOrdinaryGapMixture') {
    python tools/LegacyOrdinaryGapMixture.Probe/audit_actual.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-ordinary-gap-mixture-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    $retryArgs = @($case.Args)
    $retryArgs[5] = '.artifacts/legacy-ordinary-gap-mixture15-native-audit.json'
    $retryArgs += '15'
    & $probe @retryArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    python tools/LegacyOrdinaryGapMixture.Probe/audit_actual.py models/ru-RU/ru_RU-ak-cur.lite.res .artifacts/legacy-ordinary-gap-mixture15-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFirstBlankContext') {
    python tools/LegacyFirstBlankContext.Probe/audit_output.py .artifacts/legacy-first-blank-context-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFirstBlankPrimary') {
    python tools/LegacyFirstBlankPrimary.Probe/audit_output.py .artifacts/legacy-first-blank-primary-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    python tools/LegacyFirstPrimitiveWord.Probe/audit_contexts.py .artifacts/legacy-first-blank-primary-native-audit.json.contexts.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFirstAncestor') {
    python tools/LegacyFirstAncestor.Probe/audit_output.py .artifacts/legacy-first-ancestor-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFirstBlankGeometry') {
    python tools/LegacyFirstBlankGeometry.Probe/audit_actual.py .artifacts/legacy-first-blank-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFirstReplacementGeometry') {
    python tools/LegacyFirstReplacementGeometry.Probe/audit_output.py .artifacts/legacy-first-replacement-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
  if ($name -eq 'LegacyFreshMainAcoustic') {
    python tools/LegacyFreshMainAcoustic.Probe/audit_output.py models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json .artifacts/legacy-fresh-main-acoustic-native-audit.json
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
  }
}
$suffix = if ($IsWindows) { '.exe' } else { '' }
& "./.artifacts/LegacyIntegerWord-native/LegacyIntegerWord.Probe$suffix" models/ru-RU/ru_RU-lk-text.lite.res
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& "./.artifacts/LegacyWordBound-native/LegacyWordBound.Probe$suffix" models/ru-RU/ru_RU-lk-text.lite.res
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& "./.artifacts/LegacyDynamicWeights-native/LegacyDynamicWeights.Probe$suffix" --resource models/ru-RU/ru_RU-ak-cur.lite.res
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& "./.artifacts/LegacyPreparedNormalization-native/LegacyPreparedNormalization.Probe$suffix" --resource models/ru-RU/ru_RU-ak-cur.lite.res
exit $LASTEXITCODE
