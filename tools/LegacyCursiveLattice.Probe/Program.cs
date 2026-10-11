using InkRecognition;
using System.Text.Json;
int controls=0,candidates=0,variable=0;
using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
foreach(var test in json.RootElement.EnumerateArray())
{
    var atoms=test.GetProperty("ranges").EnumerateArray().Select(r=>new LegacyCursiveInkAtom(default,new(r[0].GetInt32(),r[1].GetInt32(),r[2].GetInt32()))).ToArray();
    var ids=test.GetProperty("ids").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
    var lattice=LegacyCursiveCandidateLattice.GenerateOrdinaryChain(atoms,ids,test.GetProperty("limit").GetInt32());
    var expected=test.GetProperty("nodes");Require(lattice.NodeCount==expected.GetArrayLength(),"Node count");
    for(int node=0;node<lattice.NodeCount;node++)
    {
        var arcs=lattice.GetNode(node);Require(arcs.Count==expected[node].GetArrayLength(),"Arc count");
        for(int a=0;a<arcs.Count;a++)
        {
            var arc=arcs[a];var e=expected[node][a];
            Require(arc.GroupId==e[0].GetInt32() && arc.NodeIndex==node && arc.FirstAtomIndex==e[1].GetInt32() && arc.Span==e[2].GetInt32() && arc.Flags==e[3].GetInt32(),"Arc identity, span or flags");
            Require(arc.OrderedAtomIndices.Span.SequenceEqual(e[4].EnumerateArray().Select(v=>v.GetInt32()).ToArray()),"Physical range order");
            var ranges=e[5].EnumerateArray().Select(r=>new LegacyInkRange(r[0].GetInt32(),r[1].GetInt32(),r[2].GetInt32())).ToArray();
            Require(arc.Ranges.Span.SequenceEqual(ranges),"Composed prepared ownership");
            Require(arc.PredecessorArcIndices.Span.SequenceEqual(e[6].EnumerateArray().Select(v=>v.GetInt32()).ToArray()),"Predecessor ordinals");
            Require(arc.Successors.Span.SequenceEqual(e[7].EnumerateArray().Select(s=>new LegacyCursiveCandidateSuccessor(s[0].GetInt32(),s[1].GetInt32())).ToArray()),"Reciprocal successor ordinals");
            int previousNode=node-arc.Span-1;Require(previousNode==arc.FirstAtomIndex-1,"Native predecessor node");
            foreach(var next in arc.Successors.Span)
            {
                var child=lattice.GetNode(node+next.NodeOffset)[next.ArcIndex];
                Require(child.FirstAtomIndex==node+1 && child.PredecessorArcIndices.Span.Contains(a),"DAG reciprocal ownership");
            }
            if(arc.Span>0)variable++;candidates++;
        }
    }
    Require(lattice.CandidateCount==Enumerable.Range(0,lattice.NodeCount).Sum(n=>lattice.GetNode(n).Count),"Total count");
    // Inputs are caller-owned; subsequent edits must not alter candidate ranges.
    var saved=lattice.GetNode(0)[0].Ranges.ToArray();atoms[0]=default;ids[0]=999;
    Require(lattice.GetNode(0)[0].Ranges.Span.SequenceEqual(saved),"Owned input snapshot");
    controls++;
}
var sample=new[]{new LegacyCursiveInkAtom(default,new LegacyInkRange(0,0,3)),new LegacyCursiveInkAtom(default,new LegacyInkRange(0,2,6)),new LegacyCursiveInkAtom(default,new LegacyInkRange(0,5,9))};
var full=LegacyCursiveCandidateLattice.GenerateOrdinaryChain(sample,new[]{7,7,7},3);
Require(full.GetNode(2).Count==3 && full.GetNode(2)[2].Ranges.Span[0]==new LegacyInkRange(0,0,9),"Joined shared endpoints");
var split=LegacyCursiveCandidateLattice.GenerateOrdinaryChain(sample,new[]{7,8,8},3);
Require(split.GetNode(1).Count==1 && split.GetNode(2).Count==2,"Physical boundary excludes crossing glyph");
var terminalPoint=new[]{sample[0],sample[1],new LegacyCursiveInkAtom(default,new LegacyInkRange(0,5,6))};
var dedup=LegacyCursiveCandidateLattice.GenerateOrdinaryChain(terminalPoint,new[]{0,0,0},3);
Require(dedup.GetNode(2)[2].GroupId==dedup.GetNode(1)[1].GroupId && dedup.GetNode(1)[1].GroupId==3,"Merged-only prepared-range group intern preserves terminal singleton");
// A decoder can choose the whole three-atom glyph instead of three single glyphs.
var best=new int[3][];
for(int node=0;node<3;node++)
{
    best[node]=new int[full.GetNode(node).Count];
    for(int arc=0;arc<best[node].Length;arc++)
    {
        var g=full.GetNode(node)[arc];int before=g.FirstAtomIndex-1;
        int prior=before<0 ? 0:g.PredecessorArcIndices.Span.ToArray().Min(a=>best[before][a]);
        best[node][arc]=prior+(g.Span==2 ? 1:9);
    }
}
Require(best[2].Min()==1,"Variable glyph path remains selectable");
Reject(()=>LegacyCursiveCandidateLattice.GenerateOrdinaryChain(sample,[],3));
Reject(()=>LegacyCursiveCandidateLattice.GenerateOrdinaryChain(sample,new[]{1,1,1},0));
Reject(()=>LegacyCursiveCandidateLattice.GenerateOrdinaryChain(new[]{sample[0],sample[0]},new[]{1,1},2));
try{LegacyCursiveCandidateLattice.GenerateOrdinaryChain(sample,new[]{1,1,1},3,new CancellationToken(true));throw new Exception("Cancellation ignored");}catch(OperationCanceledException){}
Console.WriteLine($"PASS ordinary21b candidate lattice: {controls} independent graphs, {candidates} candidates including {variable} variable spans; physical boundary barriers, prepared ownership, reciprocal adjacency, bounds and cancellation. Cross-source/special seed graph policy remains explicit and unsupported.");
static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
static void Reject(Action f){try{f();throw new Exception("Malformed graph accepted");}catch(ArgumentException){}}
