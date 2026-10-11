using InkRecognition;
using System.Text.Json;
if (args.Length > 1) throw new ArgumentException("Supply at most one own prior-selection JSON.");
if (args.Length == 1)
{
    if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Controls exceed8MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0;
    foreach (var item in document.RootElement.EnumerateArray())
    {
        var groups = item.GetProperty("groups").EnumerateArray().Select(g => new LegacyPriorContextGroup(g[0].GetInt32(),
            g[1].GetInt32(), g[2].GetInt32(), g[3].GetInt32(), g[4].GetSingle(), new float[28])).ToArray();
        var target = item.GetProperty("target").EnumerateArray().Select(p => p.GetInt32()).ToArray();
        var selected = LegacyPriorContextSelection.Select(groups, item.GetProperty("line").GetInt32(), target,
            item.GetProperty("scoped").GetBoolean(), item.GetProperty("scope").GetInt32(), item.GetProperty("firstOnly").GetBoolean(), item.GetProperty("boundary").GetBoolean());
        var expected = item.GetProperty("expectedCenters").EnumerateArray().Select(p => p.GetSingle());
        Require(selected.Select(p => p.CenterX).SequenceEqual(expected), $"Prior context filter mismatch{count}."); count++;
        if(item.TryGetProperty("expectedNativeCenters",out var native))
        {
            for(int mode=0;mode<3;mode++)
            {
                var exact=LegacyPriorContextSelection.SelectNative(groups,item.GetProperty("line").GetInt32(),target,
                    (LegacyNativePriorContextMode)mode,item.GetProperty("scoped").GetBoolean(),item.GetProperty("scope").GetInt32(),item.GetProperty("boundary").GetBoolean());
                Require(exact.Select(p=>p.CenterX).SequenceEqual(native[mode].EnumerateArray().Select(p=>p.GetSingle())), $"Native context mode{mode} differs on control{count}.");
            }
        }
    }
    Require(count > 0, "Empty controls."); Console.WriteLine($"PASS {count} independent prior-group line/scope/physical-range/boundary selections.");
}
var high = Enumerable.Repeat(1f, 28).ToArray(); var half = Enumerable.Repeat(.5f, 28).ToArray();
var groupsOwn = new LegacyPriorContextGroup[] { new(2, 7, 1, 3, 0, high), new(2, 8, 4, 6, 0, high), new(2, 7, 7, 9, 0, half), new(3, 7, 7, 9, 0, high) };
Require(LegacyPriorContextSelection.SelectNative(groupsOwn,2,[3,1,2,2],LegacyNativePriorContextMode.AllContained,true,7).Length==1,
    "Mode0 must include fully target-contained observations, including unordered duplicate target IDs.");
Require(LegacyPriorContextSelection.SelectNative(groupsOwn,2,[1,1,3],LegacyNativePriorContextMode.AllContained,true,7).Length==0,
    "Mode0 accepted a range with a missing interior index.");
Require(LegacyPriorContextSelection.SelectNative(groupsOwn,2,[3],LegacyNativePriorContextMode.Disjoint,true,7).Length==1,
    "Mode1 must reject every partially overlapping observation.");
Require(LegacyPriorContextSelection.SelectNative(groupsOwn,2,[3],LegacyNativePriorContextMode.SameLineAll,true,7).Length==2,
    "Mode2 must retain overlapping and disjoint same-line/scope observations.");
foreach(var mode in Enum.GetValues<LegacyNativePriorContextMode>())
    Require(LegacyPriorContextSelection.SelectNative(groupsOwn,2,[1,2,3],mode,targetIsBoundary:true).Length==0,"Native candidate kind1 must suppress context in every mode.");
Throws<ArgumentOutOfRangeException>(()=>LegacyPriorContextSelection.SelectNative(groupsOwn,2,[1],(LegacyNativePriorContextMode)3));
var filtered = LegacyPriorContextSelection.Select(groupsOwn, 2, [3], true, 7);
Require(filtered.Length == 1 && filtered[0].Probabilities.Span[0] == .5f, "Inclusive overlap, line or scope filtering failed.");
Require(LegacyContextWeighting.Combine(filtered, 0, 10, 0, 1, 1).All(p => p == 0), "Selected prior probabilities did not compose with weighting.");
Require(LegacyPriorContextSelection.Select(groupsOwn, 2, [3], true, 7, excludeGroupStartOnly: true).Length == 2,
    "First-index-only mode excluded range interior.");
Require(LegacyPriorContextSelection.Select(groupsOwn, 2, [3, 3, 8]).Length == 1, "Unsorted duplicate target IDs changed exclusion.");
Require(LegacyPriorContextSelection.Select(groupsOwn, 2, [3], targetIsBoundary: true).Length == 0, "Boundary target must have empty prior context.");
Require(LegacyPriorContextSelection.Select(groupsOwn, 2, [0]).Length == 3, "Physical stroke0 target must be valid and cannot overlap windows beginning at1.");
Require(LegacyFloatInputQuantization.Quantize(LegacyContextWeighting.Combine([], 0, 10, 0, 1, 1)).All(p => p == -1024),
    "Empty selected context must compose to−1024 fixed channels.");
Throws<ArgumentException>(() => LegacyPriorContextSelection.Select(groupsOwn, 2, []));
Throws<ArgumentException>(() => LegacyPriorContextSelection.Select(groupsOwn, 2, [-1]));
Throws<ArgumentException>(() => LegacyPriorContextSelection.Select([new(2, 7, 5, 4, 0, half)], 2, [3]));
Throws<ArgumentException>(() => LegacyPriorContextSelection.Select(Enumerable.Repeat(groupsOwn[0], 1025).ToArray(), 2, [3]));
Throws<OperationCanceledException>(() => LegacyPriorContextSelection.Select(groupsOwn, 2, [3], cancellationToken: new CancellationToken(true)));
Console.WriteLine("PASS native contained/disjoint/same-line modes; compatibility start-only heuristic, duplicate target IDs, line/scope/kind1 filtering, weighting/fixed composition, malformed/work cap/cancellation. Automatic lines/windows and cursive recognition are not tested.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected{typeof(T).Name}."); }
