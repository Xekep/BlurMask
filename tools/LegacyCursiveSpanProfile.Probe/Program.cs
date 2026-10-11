using InkRecognition;
using System.Buffers.Binary;
var bytes=File.ReadAllBytes(args[0]);
var actual=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(bytes,189379);
Require(actual.MaximumAtoms==8 && actual.NativeSecondByte==1,"Actual RU type21b span prefix differs");
var node=bytes.AsSpan(189379,355).ToArray();
for(int cap=1;cap<=255;cap++)
{
    node[16]=(byte)cap;node[17]=(byte)(255-cap);
    var p=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(node,0);
    Require(p.MaximumAtoms==cap && p.NativeSecondByte==255-cap,"Byte prefix changed");
}
foreach(int offset in new[]{0,4,8,12,14,16})
{
    var malformed=bytes.AsSpan(189379,355).ToArray();malformed[offset]=offset==16 ? (byte)0:(byte)(malformed[offset]^1);
    Reject(()=>LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(malformed,0));
}
Reject(()=>LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(node.AsSpan(0,354),0));
Reject(()=>LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(bytes,-1));
var atoms=new[]{new LegacyCursiveInkAtom(default,new LegacyInkRange(0,0,3)),new LegacyCursiveInkAtom(default,new LegacyInkRange(0,2,6))};
Require(LegacyCursiveCandidateLattice.GenerateOrdinaryChain(atoms,new[]{-1,-1},actual.MaximumAtoms).GetNode(1).Count==2,"Model cap feeds actual ordinary glyph graph");
Console.WriteLine("PASS actual RU type21b v10a span8/secondByte1,255 bounded prefix controls, malformed/truncated nodes and ordinary lattice bridge. Remaining type21b policy fields are unparsed.");
static void Require(bool value,string why){if(!value)throw new Exception(why);}
static void Reject(Action action){try{action();throw new Exception("Unsupported node accepted");}catch(InvalidDataException){}}
