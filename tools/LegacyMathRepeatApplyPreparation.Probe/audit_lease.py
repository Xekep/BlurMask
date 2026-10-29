import json,sys
from pathlib import Path
c=json.loads(Path(sys.argv[1]).read_text(encoding="utf-8-sig"))
assert len(c["Actual"])==len(c["Leases"])==6
checks=0
for i,(a,l) in enumerate(zip(c["Actual"],c["Leases"])):
 assert a["Case"]==l["Case"]==i;checks+=1
 p=l["Prepared"]
 assert p["Prepared"] and p["Ready"] and not p["Failed"] and not p["Released"];checks+=1
 assert not p["OldReceiptConsumed"] and not p["MutationEntered"];checks+=1
 assert p["DomOperations"]==len(a["Repeat"]["Dom"]) and p["SourceCommands"]==len(a["Repeat"]["Source"]) and p["RetainedNodes"]==len(a["Repeat"]["FourthWriter"]);checks+=1
 assert p["RawObjects"]==a["CurrentInput"]["ObjectCount"]==3 and p["OldEpoch"]==a["CurrentInput"]["AppliedEpoch"]==1 and p["OldNext"]==a["CurrentInput"]["Next"]==4;checks+=1
 assert l["AfterExporterDispose"]==l["AfterAncestorDispose"]==p;checks+=1
 r=l["Released"];assert r["Released"] and not r["OldReceiptConsumed"] and not r["MutationEntered"] and l["ExporterNodesAfterRelease"]==0;checks+=1
 for k,v in p.items():
  if k not in ("Released","RawObjects"):assert r[k]==v
 assert r["RawObjects"]==0
 checks+=1
assert c["Failures"]==c["ExportFailures"]==[]
print(json.dumps({"pass":True,"cases":6,"checks":checks,"scope":"Actual payload-lease diagnostics plus source-bound owning assertions; no original mutation/receiver/journal/successor proof."}))
