"""Exact model-retention + AL semantic-addition source authentication. No historical hash waiver."""
from pathlib import Path
import hashlib,json
D=Path(__file__).resolve().parent
PRE_AL_PROFILE='68675aead36585873cbb65035f1133802d9d0801fd82c91fe8c44170b4105e68'
PRE_AL_HEADER='8fb6e2c23c229d7c45e02f79c1fb7e885a99accac03dfeb2c25d866a424b8b2b'
EXPECTED_SOURCES={'LegacyMathAkFeatureProfile.cs': 'b89dc99f3766e1b2c3e4589ec4413770590a76ecf5f49abe1911ae46a1478659', 'LegacyMathAkFreshCommonWorkspace.cs': '9be84b0c1a345f9f99fbd19d568d34c6e3613ddabe41127dda07785f87f59da1', 'LegacyMathAkFirst276Alphabet.cs': 'd376d77b33bebf138a69515ad6420a579af1bf7b5bc676277204b5382d6a662c', 'LegacyMathAkFirst276SourceBinding.cs': '4af5b27fcfa1259ed8f3abce5cab6ef338f03dc31585ebd19ecf8f927a395612', 'LegacyMathAkFirst276Program.cs': '558b0555e122529afbe3ad050db03dc02814edb0f6bc4b86229c2502b1886638', 'LegacyMathAkFirst276ModelStorage.cs': 'f90ecbe9e7e582f2e1a705b54680a91a6605e709eba7b64377f6fa9e9c1c8967'}
PROFILE_AL_RESTORE=[['First276Program first276, FirstAlphabetPrefix firstAlphabet)', 'First276Program first276)'], ['this.first276 = first276; this.firstAlphabet = firstAlphabet;', 'this.first276 = first276;'], ['            var firstAlphabet = FirstAlphabetPrefix.ReadAuthenticated(captured, token);\n', ''], ['freshCommonHeapCapacity, first276, firstAlphabet);', 'freshCommonHeapCapacity, first276);']]
def H(b):return hashlib.sha256(b.replace(b'\r\n',b'\n')).hexdigest()

# Exact complete-source successor restoration before the unchanged historical
# model/AL chains. No source-pin map or stored runtime DATA is rewritten.
FEATURE_POLICY_CONTRACT_SHA256="198981d60468080889b71e2b07ad85133ca8c6b46662583b06fa1864a280bff4"
_feature_policy_raw=(D/'feature-policy-source-preimages.json').read_bytes().replace(b'\r\n',b'\n')
assert hashlib.sha256(_feature_policy_raw).hexdigest()==FEATURE_POLICY_CONTRACT_SHA256
FEATURE_POLICY_SOURCES=json.loads(_feature_policy_raw)['sources']
FEATURE_POLICY_SUCCESSORS=json.loads(_feature_policy_raw)['successorSteps']
NUMERICAL_SOURCE_COHORT=json.loads(_feature_policy_raw)['numericalSourceCohort']
NORMALIZED_SOURCE_COHORT=json.loads(_feature_policy_raw)['normalizedSourceCohort']
def restore_feature_policy(name,raw):
 assert type(name)is str and type(raw)is bytes and name in FEATURE_POLICY_SOURCES,('unknown feature-policy source',name)
 raw=raw.replace(b'\r\n',b'\n');row=FEATURE_POLICY_SOURCES[name]
 # One exact numerical model-owner partial seam, before the unchanged
 # feature-policy/model-retention/AL chain. No old row or pin is replaced.
 successor=FEATURE_POLICY_SUCCESSORS.get(name+':'+H(raw))
 if successor is not None:
  assert successor['name']==name and H(raw)==successor['source']
  for start,end,after,before in reversed(successor['edits']):
   assert type(start)is int and type(end)is int and 0<=start<=end<=len(raw)
   assert raw[start:end]==bytes.fromhex(after)
   raw=raw[:start]+bytes.fromhex(before)+raw[end:]
  assert H(raw)==successor['target']
 if H(raw)==row['target']:return raw
 assert H(raw)==row['source'],('unreviewed complete feature-policy source',name,H(raw))
 for start,end,after,before in reversed(row['edits']):
  assert type(start)is int and type(end)is int and 0<=start<=end<=len(raw)
  assert raw[start:end]==bytes.fromhex(after)
  raw=raw[:start]+bytes.fromhex(before)+raw[end:]
 assert H(raw)==row['target'],('complete feature-policy preimage differs',name)
 return raw

PRE_MODEL_PROFILE='2d73a8de134fcdf6879e70572c951680b46bfb0ac0af6dcee32da63c912e3e6e'
PRE_MODEL_BINDING='0a192f90b1ef92349a9eaa0b7ebae160690374ff26a8df7d74832dd6e170cc69'
MODEL_PROFILE_RESTORE=[
 ['FirstAlphabetPrefix firstAlphabet, First276ModelData firstModelData)', 'FirstAlphabetPrefix firstAlphabet)'],
 ['this.firstAlphabet = firstAlphabet; this.firstModelData = firstModelData;', 'this.firstAlphabet = firstAlphabet;'],
 ['            var firstModelData = First276ModelData.ReadAuthenticated(captured, token);\n', ''],
 ['first276, firstAlphabet, firstModelData);', 'first276, firstAlphabet);']]

def restore_model_profile(raw):
 raw=restore_feature_policy('LegacyMathAkFeatureProfile.cs',raw)
 text=raw.replace(b'\r\n',b'\n').decode('utf-8')
 for old,new in MODEL_PROFILE_RESTORE:
  assert old and text.count(old)==1,'exact model Profile retention reversal'
  text=text.replace(old,new,1)
 restored=text.encode('utf-8');assert H(restored)==PRE_MODEL_PROFILE,'pre-model Profile restored exactly'
 return restored

def restore_model_binding(raw):
 text=raw.replace(b'\r\n',b'\n').decode('utf-8');old='private sealed partial class First276SourceBinding'
 assert text.count(old)==1,'exact model binding partial seam'
 restored=text.replace(old,'private sealed class First276SourceBinding',1).encode('utf-8')
 assert H(restored)==PRE_MODEL_BINDING,'original88B binding fields/ctor/ReadInside restored exactly'
 return restored

def restore_profile(raw):
 raw=restore_model_profile(raw)
 text=raw.replace(b'\r\n',b'\n').decode('utf-8')
 for old,new in PROFILE_AL_RESTORE:
  assert old and text.count(old)==1,'exact AL profile reversal'
  text=text.replace(old,new,1)
 restored=text.encode('utf-8');assert H(restored)==PRE_AL_PROFILE,'pre-AL Profile restored exactly'
 return restored
def restore_header(raw):
 text=raw.replace(b'\r\n',b'\n').decode('utf-8');old='private sealed partial class FreshCommonWorkspace'
 assert text.count(old)==1,'exact common-header declaration reversal'
 text=text.replace(old,'private sealed class FreshCommonWorkspace',1);restored=text.encode('utf-8')
 assert H(restored)==PRE_AL_HEADER,'pre-AL header restored exactly'
 return restored
def authenticate_numerical_cohort(root):
 base=Path(root)/'src/InkRecognition';p=NUMERICAL_SOURCE_COHORT
 normalized=base/NORMALIZED_SOURCE_COHORT['normalizationFilename']
 if normalized.exists():
  p=NORMALIZED_SOURCE_COHORT
  for filename,pin in [('connectedFilename','connectedSHA256'),('numericalFilename','numericalSHA256'),('modelStorageFilename','modelStorageSHA256'),('normalizationFilename','normalizationSHA256')]:
   assert H((base/p[filename]).read_bytes())==p[pin], ('unreviewed normalized source cohort',p[filename])
  return

 connected=base/p['connectedFilename'];numerical=base/p['numericalFilename']
 if numerical.exists():
  assert connected.exists(), 'numeric cohort missing connected owner route'
  assert H((base/p['modelStorageFilename']).read_bytes())==p['modelStorageSHA256'], 'numeric cohort model-owner seam differs'
  assert H(connected.read_bytes())==p['connectedSHA256'], 'unreviewed numerical connected source'
  assert H(numerical.read_bytes())==p['numericalSHA256'], 'unreviewed numerical fill source'
 elif connected.exists():
  assert H(connected.read_bytes())==p['inputOnlyConnectedSHA256'], 'unreviewed input-only/numerical source cohort'
 # A historical six-source-only fixture has neither new route; it remains
 # historical source proof and does not assert numerical cohort presence.

def authenticate(root):
 root=Path(root);pins=json.loads((D/'source-pins.json').read_text('utf-8'));assert pins==EXPECTED_SOURCES,'exact reviewed source-pin map'
 for name,sha in pins.items():
  raw=(root/'src/InkRecognition'/name).read_bytes()
  if name in FEATURE_POLICY_SOURCES:raw=restore_feature_policy(name,raw)
  assert H(raw)==sha,('reviewed AL source pin',name)
 authenticate_numerical_cohort(root)
 restore_profile((root/'src/InkRecognition/LegacyMathAkFeatureProfile.cs').read_bytes())
 restore_model_binding((root/'src/InkRecognition/LegacyMathAkFirst276SourceBinding.cs').read_bytes())
 restore_header((root/'src/InkRecognition/LegacyMathAkFreshCommonWorkspace.cs').read_bytes())
 return len(pins)
