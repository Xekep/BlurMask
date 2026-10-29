"""Source-only admission controls; no build, model or recognition execution."""
from pathlib import Path
import argparse,contextlib,importlib.util,shutil,tempfile,unittest
p=argparse.ArgumentParser();p.add_argument('--root',type=Path,default=Path(__file__).resolve().parents[2]);args=p.parse_args()
spec=importlib.util.spec_from_file_location('census',Path(__file__).with_name('publication_core_source_census.py'));census=importlib.util.module_from_spec(spec);spec.loader.exec_module(census)
class AdmissionTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.temp=tempfile.TemporaryDirectory(prefix='core-census-');cls.root=Path(cls.temp.name);cls.core=cls.root/'src/InkRecognition';cls.core.mkdir(parents=True)
  for f in (args.root/'src/InkRecognition').glob('*.cs'):shutil.copyfile(f,cls.core/f.name)
  cls.additions=['LegacyOwnedRawInkAppendState.cs','LegacyOwnedCurrentRawIdentityExport.cs'];cls.baseline=next(f for f in sorted(cls.core.glob('*.cs'))if f.name not in cls.additions)
  census.verify(cls.root)
 @classmethod
 def tearDownClass(cls):cls.temp.cleanup()
 @contextlib.contextmanager
 def changed(self,path,data):
  existed=path.exists();old=path.read_bytes()if existed else None;path.parent.mkdir(parents=True,exist_ok=True)
  if data is None:path.unlink()
  else:path.write_bytes(data)
  try:yield
  finally:
   if existed:path.write_bytes(old)
   else:path.unlink()
 def test_positive(self):self.assertEqual(census.verify(self.root)['DefaultCoreCount'],937)
 def test_canonical_lf_and_crlf(self):
  for f in [self.baseline,*[self.core/n for n in self.additions]]:
   raw=f.read_bytes().replace(b'\r\n',b'\n')
   with self.changed(f,raw):census.verify(self.root)
   with self.changed(f,raw.replace(b'\n',b'\r\n')):census.verify(self.root)
 def test_missing_baseline(self):
  with self.changed(self.baseline,None),self.assertRaises(AssertionError):census.verify(self.root)
 def test_same_count_unknown_replacement(self):
  with self.changed(self.baseline,None),self.changed(self.core/'UnknownReplacement.cs',b'// unreviewed\n'),self.assertRaises(AssertionError):census.verify(self.root)
 def test_tampered_baseline(self):
  with self.changed(self.baseline,self.baseline.read_bytes()+b'// altered\n'),self.assertRaises(AssertionError):census.verify(self.root)
 def test_extra_top_level(self):
  with self.changed(self.core/'Unknown.cs',b'// unreviewed\n'),self.assertRaises(AssertionError):census.verify(self.root)
 def test_extra_nested(self):
  with self.changed(self.core/'nested/Unknown.cs',b'// unreviewed\n'),self.assertRaises(AssertionError):census.verify(self.root)
 def test_extra_case_suffix(self):
  with self.changed(self.core/'Unknown.CS',b'// unreviewed\n'),self.assertRaises(AssertionError):census.verify(self.root)
 def test_missing_reviewed_additions(self):
  for n in self.additions:
   with self.subTest(name=n),self.changed(self.core/n,None),self.assertRaises(AssertionError):census.verify(self.root)
 def test_tampered_reviewed_additions(self):
  for n in self.additions:
   with self.subTest(name=n),self.changed(self.core/n,(self.core/n).read_bytes()+b'// altered\n'),self.assertRaises(AssertionError):census.verify(self.root)
 def test_unreviewed_manifest(self):
  tools=self.root/'tools/ci';tools.mkdir(parents=True,exist_ok=True);helper=tools/'publication_core_source_census.py';helper.write_bytes(Path(census.__file__).read_bytes())
  (tools/'publication-core-source-admissions.json').write_bytes(Path(census.__file__).with_name('publication-core-source-admissions.json').read_bytes()+b' ')
  s=importlib.util.spec_from_file_location('unreviewed',helper);m=importlib.util.module_from_spec(s);s.loader.exec_module(m)
  with self.assertRaises(AssertionError):m.verify(self.root)
if __name__=='__main__':unittest.main(argv=[__file__],verbosity=2)
