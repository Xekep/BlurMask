using InkRecognition;

void Require(bool condition) { if (!condition) throw new InvalidDataException("Independent source-count control differs."); }
void Reject(Action operation) { try { operation(); } catch (Exception error) when (error is ArgumentException or OperationCanceledException) { return; } throw new InvalidDataException("Malformed/canceled source count accepted."); }
Require(LegacyPreparedSourceStrokeCount.Count([7,7,9,7,9]) == 2);
Require(LegacyPreparedSourceStrokeCount.Count([7,7,9,7,9],3) == 5);
Require(LegacyPreparedSourceStrokeCount.Count([int.MinValue,int.MaxValue,-1,int.MinValue]) == 3);
Require(LegacyPreparedSourceStrokeCount.Count([],256) == 0);
int cases = 0;
var random = new Random(185440);
// Independent reference counts a set rather than the native earlier-range scan.
foreach (int n in new[] {0,1,2,3,15,63,127,254,255,256,257,511,512,4096})
foreach (int alphabet in new[] {1,2,7,256,1024})
{
    int[] ids = Enumerable.Range(0,n).Select(_=>random.Next(-alphabet,alphabet+1)).ToArray();
    foreach (int prefix in new[] {0,1,2,255,256,257,4096})
    {
        byte expected = (byte)((ids.Distinct().Count()+prefix)%256);
        Require(LegacyPreparedSourceStrokeCount.Count(ids,prefix) == expected);
        Require(LegacyPreparedSourceStrokeCount.Count(ids.Reverse().ToArray(),prefix) == expected);
        cases++;
    }
}
foreach (int n in new[] {254,255,256,257,511,512,4096})
{
    Require(LegacyPreparedSourceStrokeCount.Count(Enumerable.Range(0,n).ToArray()) == (byte)(n%256));
    cases++;
}
Reject(()=>LegacyPreparedSourceStrokeCount.Count(new int[4097]));
Reject(()=>LegacyPreparedSourceStrokeCount.Count([], -1));
Reject(()=>LegacyPreparedSourceStrokeCount.Count([],4097));
using var cancellation=new CancellationTokenSource(); cancellation.Cancel();
Reject(()=>LegacyPreparedSourceStrokeCount.Count([],0,cancellation.Token));
// Demonstrate the actual downstream inclusive min/max stage, without guessing
// whether these newer-resource bounds exist in the Russian version110 node.
byte[] primary=[1,1,1],secondary=[1,1,1];
byte count=LegacyPreparedSourceStrokeCount.Count([5,5,9]);
Require(LegacyAcousticSymbolRangeFilter.Apply(primary,secondary,[1,2,3],[1,2,3],count)==2);
Require(primary.SequenceEqual(new byte[]{0,1,0})&&secondary.SequenceEqual(primary));
Console.WriteLine($"PASS {cases} independent own source-ID/template controls, permutation invariance, modulo256 boundary neighbors, guards and downstream inclusive bounds. No language/case default inferred.");
