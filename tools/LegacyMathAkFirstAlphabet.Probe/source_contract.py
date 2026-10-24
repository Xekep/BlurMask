"""Exact AL semantic-addition source authentication. No historical hash waiver."""
from pathlib import Path
import hashlib,json
D=Path(__file__).resolve().parent
PRE_AL_PROFILE='68675aead36585873cbb65035f1133802d9d0801fd82c91fe8c44170b4105e68'
PRE_AL_HEADER='8fb6e2c23c229d7c45e02f79c1fb7e885a99accac03dfeb2c25d866a424b8b2b'
EXPECTED_SOURCES={'LegacyMathAkFeatureProfile.cs': '2d73a8de134fcdf6879e70572c951680b46bfb0ac0af6dcee32da63c912e3e6e', 'LegacyMathAkFreshCommonWorkspace.cs': '9be84b0c1a345f9f99fbd19d568d34c6e3613ddabe41127dda07785f87f59da1', 'LegacyMathAkFirst276Alphabet.cs': 'd376d77b33bebf138a69515ad6420a579af1bf7b5bc676277204b5382d6a662c', 'LegacyMathAkFirst276SourceBinding.cs': '0a192f90b1ef92349a9eaa0b7ebae160690374ff26a8df7d74832dd6e170cc69', 'LegacyMathAkFirst276Program.cs': '558b0555e122529afbe3ad050db03dc02814edb0f6bc4b86229c2502b1886638'}
PROFILE_AL_RESTORE=[['First276Program first276, FirstAlphabetPrefix firstAlphabet)', 'First276Program first276)'], ['this.first276 = first276; this.firstAlphabet = firstAlphabet;', 'this.first276 = first276;'], ['            var firstAlphabet = FirstAlphabetPrefix.ReadAuthenticated(captured, token);\n', ''], ['freshCommonHeapCapacity, first276, firstAlphabet);', 'freshCommonHeapCapacity, first276);']]
def H(b):return hashlib.sha256(b.replace(b'\r\n',b'\n')).hexdigest()
def restore_profile(raw):
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
def authenticate(root):
 root=Path(root);pins=json.loads((D/'source-pins.json').read_text('utf-8'));assert pins==EXPECTED_SOURCES,'exact reviewed source-pin map'
 for name,sha in pins.items():
  raw=(root/'src/InkRecognition'/name).read_bytes()
  assert H(raw)==sha,('reviewed AL source pin',name)
 restore_profile((root/'src/InkRecognition/LegacyMathAkFeatureProfile.cs').read_bytes())
 restore_header((root/'src/InkRecognition/LegacyMathAkFreshCommonWorkspace.cs').read_bytes())
 return len(pins)
