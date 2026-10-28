"""Independent first6120 unmasked mixed probability contract; no labels/NN/quality policy.

Uses the previously verified scalar mixed EXP/sigmoid math, but authenticates the
FIRST source/AL/network layout independently of the MAIN dispatcher.
"""
from pathlib import Path
import hashlib,importlib.util,struct

_spec=importlib.util.spec_from_file_location('ru_common_mixed_probability',Path(__file__).with_name('ru_main_mixed_probability.py'))
_common=importlib.util.module_from_spec(_spec);_spec.loader.exec_module(_common)

def for_russian_first(resource):
 if len(resource)!=572763 or hashlib.sha256(resource).hexdigest()!='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978':
  raise ValueError('Exact Russian resource required')
 end=6120+8+struct.unpack_from('<I',resource,6124)[0]
 if hashlib.sha256(resource[6120:end]).hexdigest()!='aa2af98c8c550603e797f304db204aedd1102482cc8a6ddcc0ed8b62cf8113d2':
  raise ValueError('Actual owned first source required')
 if resource[187648]!=0 or resource[6219:11353]!=resource[99:5233]:
  raise ValueError('Native first unmasked flag/owned AL differs')
 special=_common._label(resource[6219:6235],180)
 for offset,width in [(19195,247),(105197,229)]:
  if struct.unpack_from('<HH',resource,offset)!=(0xfea0,0x106) or resource[offset+8]!=0x12 or struct.unpack_from('<5I',resource,offset+11)!=(width,201,0,0,181) or struct.unpack_from('<H',resource,offset+31)[0]!=10:
   raise ValueError('Actual first Q10 trajectory/raster layout required')
 table=_common._exp_table()
 def probability(raw):
  if len(raw)!=180:raise ValueError('Actual180 semantic first output dots required')
  return _common._mixed(raw,special,table)
 return probability
