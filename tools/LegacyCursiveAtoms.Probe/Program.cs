using InkRecognition;
using System.Text.Json;
LegacyCursiveCutMarker[] markers = [new(2,0,0),new(7,0,2),new(8,0,4),new(3,1,0)];
var atoms = LegacyCursiveInkAtoms.Build([7,2],markers);
Require(atoms.Select(a=>a.Range).SequenceEqual(new LegacyInkRange[] {new(0,0,3),new(0,2,5),new(0,4,7),new(1,0,2)}), "Shared endpoint ownership differs.");
Require(atoms.Select(a=>a.Marker.Kind).SequenceEqual(new[] {2,7,8,3}), "Marker kind lost or interpreted.");
var composed = LegacyCursiveInkAtoms.Compose(atoms.Select(a=>a.Range).ToArray());
Require(composed.SequenceEqual(new LegacyInkRange[] {new(0,0,7),new(1,0,2)}), "Continuous atom path was not restored as one source stroke.");
markers[0] = new(99,0,6);
Require(atoms[0].Marker == new LegacyCursiveCutMarker(2,0,0), "Caller marker mutation leaked.");
LegacyInkStroke[] source = [new(Enumerable.Range(0,7).Select(i=>new LegacyInkPoint((short)i,(short)(i%2))).ToArray()),new([new(5,-1),new(5,-2)])];
var selected = LegacyInkRangeExtraction.Extract(source,composed);
Require(selected.Strokes[0].Points.SequenceEqual(source[0].Points) && selected.Strokes.Count == 2, "Composition duplicated endpoints or created a pen-up within a physical stroke.");
Require(LegacyCursiveInkAtoms.Compose([new(0,0,2),new(0,2,5)]).Length == 2, "Disjoint intervals incorrectly stitched.");
Require(LegacyCursiveInkAtoms.Compose([new(0,0,3),new(1,2,5)]).Length == 2, "Foreign physical source stroke stitched.");
Require(LegacyCursiveInkAtoms.Compose([new(0,0,5),new(0,2,6)]).Length == 2, "Multiple overlapping endpoints collapsed without evidence.");
Reject(()=>LegacyCursiveInkAtoms.Build([4],[new(1,0,4)]));
Reject(()=>LegacyCursiveInkAtoms.Build([4],[new(1,0,0),new(2,0,0)]));
Reject(()=>LegacyCursiveInkAtoms.Build([4],[new(1,0,2),new(2,0,1)]));
Reject(()=>LegacyCursiveInkAtoms.Build([4,4],[new(1,1,0),new(2,0,0)]));
Reject(()=>LegacyCursiveInkAtoms.Build([0],[new(1,0,0)]));
Reject(()=>LegacyCursiveInkAtoms.Build([4],[]));
Reject(()=>LegacyCursiveInkAtoms.Compose([new(0,2,2)]));
Reject(()=>LegacyCursiveInkAtoms.Compose(Enumerable.Range(0,16).Select(i=>new LegacyInkRange(i,0,1)).ToArray()));
using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
try { LegacyCursiveInkAtoms.Build([4],[new(1,0,0)],cancelled.Token);throw new Exception("Cancellation ignored."); }catch(OperationCanceledException){}
int controls=0;
if(args.Length != 0)
{
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var test in json.RootElement.EnumerateArray())
    {
        var counts=test.GetProperty("counts").EnumerateArray().Select(p=>p.GetInt32()).ToArray();
        var cuts=test.GetProperty("markers").EnumerateArray().Select(p=>new LegacyCursiveCutMarker(p[0].GetInt32(),p[1].GetInt32(),p[2].GetInt32())).ToArray();
        var actual=LegacyCursiveInkAtoms.Build(counts,cuts);
        var expected=Ranges(test.GetProperty("atoms"));
        Require(actual.Select(a=>a.Range).SequenceEqual(expected),"Independent atom ranges differ.");
        int start=test.GetProperty("start").GetInt32(), length=test.GetProperty("length").GetInt32();
        var composedActual=LegacyCursiveInkAtoms.Compose(actual.AsSpan(start,length).ToArray().Select(a=>a.Range).ToArray());
        Require(composedActual.SequenceEqual(Ranges(test.GetProperty("composed"))),"Independent atom composition differs.");
        controls++;
    }
}
Console.WriteLine($"PASS sampler297 cursive atom ownership: shared endpoint+1, kind preservation, continuous path composition, pen-up boundaries, immutable snapshots, malformed/ordered bounds, cancellation; {controls} independent references. Cut marker inference remains separate.");
static LegacyInkRange[] Ranges(JsonElement el)=>el.EnumerateArray().Select(p=>new LegacyInkRange(p[0].GetInt32(),p[1].GetInt32(),p[2].GetInt32())).ToArray();
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject(Action action){try{action();throw new Exception("Malformed atoms accepted.");}catch(ArgumentException){}}
