"""Source-only admission controls; no build, model or recognition execution."""
from pathlib import Path
import argparse,contextlib,copy,importlib.util,json,shutil,tempfile,unittest
p=argparse.ArgumentParser();p.add_argument('--root',type=Path,default=Path(__file__).resolve().parents[2]);args=p.parse_args()
spec=importlib.util.spec_from_file_location('census',Path(__file__).with_name('publication_core_source_census.py'));census=importlib.util.module_from_spec(spec);spec.loader.exec_module(census)
class AdmissionTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.temp=tempfile.TemporaryDirectory(prefix='core-census-');cls.root=Path(cls.temp.name);cls.core=cls.root/'src/InkRecognition';cls.core.mkdir(parents=True)
  for f in (args.root/'src/InkRecognition').glob('*.cs'):shutil.copyfile(f,cls.core/f.name)
  cls.manifest=json.loads(Path(census.__file__).with_name('publication-core-source-admissions.json').read_bytes())
  cls.additions=[row['path']for row in cls.manifest['additions']+cls.manifest['english951']['additions']]
  cls.overrides=[row['path']for row in cls.manifest['english951']['overrides']]
  cls.baseline=next(f for f in sorted(cls.core.glob('*.cs'))if f.name not in cls.additions+cls.overrides)
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
 def test_positive(self):self.assertEqual(census.verify(self.root)['DefaultCoreCount'],951)
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
 def test_tampered_override(self):
  for name in self.overrides:
   path=self.core/name
   with self.subTest(name=name),self.changed(path,path.read_bytes()+b'// changed\n'),self.assertRaises(AssertionError):census.verify(self.root)
 def test_wrong_preimage_digest(self):
  manifest=copy.deepcopy(self.manifest);manifest['english951']['overrides'][0]['preimageCanonicalLfSha256']='0'*64
  with self.assertRaisesRegex(AssertionError,'baseline Core content/preimage'):census._verify_sources(self.root,manifest)
 def test_legitimate_old_body_is_not_approved_current(self):
  name='LegacyEnglishCaseGraphPhase1.cs'
  raw=Path(__file__).with_name('fixtures').joinpath('publication-core-951-preimage.cs.txt').read_bytes()
  row=next(r for r in self.manifest['english951']['overrides']if r['path']==name)
  self.assertEqual(census.sha(raw),row['preimageCanonicalLfSha256'])
  self.assertNotEqual(census.sha(raw),row['canonicalLfSha256'])
  with self.changed(self.core/name,raw),self.assertRaisesRegex(AssertionError,'Tampered reviewed'):census.verify(self.root)
 def test_all_override_newline_canonicalization(self):
  path=self.core/self.overrides[0];raw=path.read_bytes().replace(b'\r\n',b'\n')
  with self.changed(path,raw.replace(b'\n',b'\r\n')):census.verify(self.root)
 def test_retained_files_protected(self):
  for row in self.manifest['english951']['protected']:
   path=self.core/row['path']
   with self.subTest(name=path.name),self.changed(path,path.read_bytes()+b'// altered\n'),self.assertRaises(AssertionError):census.verify(self.root)
 def test_ledger_cap_change(self):
  path=self.core/'LegacyEnglishCaseNode0Ledger.cs';raw=path.read_bytes()
  self.assertIn(b'VisitCeiling=16L*1024*1024',raw)
  with self.changed(path,raw.replace(b'VisitCeiling=16L*1024*1024',b'VisitCeiling=17L*1024*1024')),self.assertRaises(AssertionError):census.verify(self.root)
 def test_protected_override_rejected_even_with_valid_body(self):
  manifest=copy.deepcopy(self.manifest);row=manifest['english951']['protected'][0]
  manifest['english951']['overrides'][0]=dict(row,preimageCanonicalLfSha256=row['canonicalLfSha256'])
  with self.assertRaises(AssertionError):census._verify_sources(self.root,manifest)
 def test_duplicate_admission_row(self):
  manifest=copy.deepcopy(self.manifest);manifest['english951']['additions'][0]=manifest['english951']['additions'][1]
  with self.assertRaises(AssertionError):census._verify_sources(self.root,manifest)
 def test_fixed_counts(self):
  manifest=copy.deepcopy(self.manifest);manifest['defaultCoreCount']=952
  with self.assertRaises(AssertionError):census._verify_sources(self.root,manifest)
 def test_tampered_owned_frame_bodies(self):
  for row in self.manifest['english951']['ownedFrameRevision']['sources']:
   path=self.core/row['path']
   with self.subTest(name=path.name),self.changed(path,path.read_bytes()+b'// altered\n'),self.assertRaisesRegex(AssertionError,'Tampered reviewed'):census.verify(self.root)
 def test_wrong_owned_frame_checkpoint_preimages(self):
  for i,row in enumerate(self.manifest['english951']['ownedFrameRevision']['sources']):
   manifest=copy.deepcopy(self.manifest);manifest['english951']['ownedFrameRevision']['sources'][i]['preimageCanonicalLfSha256']='0'*64
   with self.subTest(name=row['path']),self.assertRaises(AssertionError):census._verify_sources(self.root,manifest)
 def test_owned_frame_old_baseline_body_rejected(self):
  row=next(r for r in self.manifest['english951']['ownedFrameRevision']['sources']if r['path']=='LegacyPreparedCompactWordFrameProducer.cs')
  raw=Path(__file__).with_name('fixtures').joinpath('publication-core-frame26-preimage.cs.txt').read_bytes().replace(b'\r\n',b'\n')
  self.assertEqual(census.sha(raw),row['preimageCanonicalLfSha256'])
  with self.changed(self.core/row['path'],raw),self.assertRaisesRegex(AssertionError,'Tampered reviewed'):census.verify(self.root)
 def test_owned_frame_wrong_checkpoint(self):
  manifest=copy.deepcopy(self.manifest);manifest['english951']['ownedFrameRevision']['checkpointManifestSha256']='0'*64
  with self.assertRaises(AssertionError):census._verify_sources(self.root,manifest)
 def test_owned_frame_missing_source(self):
  manifest=copy.deepcopy(self.manifest);manifest['english951']['ownedFrameRevision']['sources'].pop()
  with self.assertRaises(AssertionError):census._verify_sources(self.root,manifest)
 def test_owned_frame_unknown_scope(self):
  manifest=copy.deepcopy(self.manifest);manifest['english951']['ownedFrameRevision']['sources'][0]['path']='LegacyEnglishCaseNode0Ledger.cs'
  with self.assertRaises(AssertionError):census._verify_sources(self.root,manifest)
 def test_ordinary_factor_old_checkpoint_body_rejected(self):
  row=self.manifest['english951']['ordinarySourceFactorRevision']['sources'][0]
  raw=Path(__file__).with_name('fixtures').joinpath('publication-core-ordinary-factor-preimage.cs.txt').read_bytes().replace(b'\r\n',b'\n')
  self.assertEqual(census.sha(raw),row['preimageCanonicalLfSha256'])
  self.assertNotIn(b'sourceFactor:200',raw)
  with self.changed(self.core/row['path'],raw),self.assertRaisesRegex(AssertionError,'Tampered reviewed'):census.verify(self.root)
 def test_ordinary_factor_wrong_preimage(self):
  manifest=copy.deepcopy(self.manifest);manifest['english951']['ordinarySourceFactorRevision']['sources'][0]['preimageCanonicalLfSha256']='0'*64
  with self.assertRaisesRegex(AssertionError,'Wrong revision checkpoint preimage'):census._verify_sources(self.root,manifest)
 def test_ordinary_factor_wrong_preimage_size(self):
  manifest=copy.deepcopy(self.manifest);manifest['english951']['ordinarySourceFactorRevision']['sources'][0]['preimageCanonicalLfBytes']+=1
  with self.assertRaisesRegex(AssertionError,'Wrong revision checkpoint size'):census._verify_sources(self.root,manifest)
 def test_ordinary_factor_wrong_scope(self):
  manifest=copy.deepcopy(self.manifest);manifest['english951']['ordinarySourceFactorRevision']['sources'][0]['path']='LegacySourcePointChronology.cs'
  with self.assertRaisesRegex(AssertionError,'Unreviewed ordinary factor revision scope'):census._verify_sources(self.root,manifest)
 def test_ordinary_factor_wrong_checkpoint(self):
  manifest=copy.deepcopy(self.manifest);manifest['english951']['ordinarySourceFactorRevision']['checkpointManifestSha256']='0'*64
  with self.assertRaises(AssertionError):census._verify_sources(self.root,manifest)
 def test_ordinary_factor_duplicate_revision_source(self):
  manifest=copy.deepcopy(self.manifest);sources=manifest['english951']['ordinarySourceFactorRevision']['sources'];sources.append(copy.deepcopy(sources[0]))
  with self.assertRaises(AssertionError):census._verify_sources(self.root,manifest)
 def test_ordinary_factor_tampered_actual_constructor(self):
  path=self.core/'LegacyOrdinarySourceInk.cs';raw=path.read_bytes();self.assertIn(b'sourceFactor:200',raw)
  with self.changed(path,raw.replace(b'sourceFactor:200',b'sourceFactor:201')),self.assertRaisesRegex(AssertionError,'Tampered reviewed'):census.verify(self.root)
if __name__=='__main__':unittest.main(argv=[__file__],verbosity=2)
