using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathStructuredOutput.Probe")]

namespace InkRecognition.Math;

/// <summary>Literal mode-zero/null-eligibility EE4 syntax transformation for the
/// authenticated source AL. Numerical syntax only; owning publication is separate.</summary>
internal sealed class LegacyMath2Ee4Formatter
{
    private sealed record Rule(int Kind, ushort[] Tags);
    private sealed class Frame(int ordinal, Rule rule, int input)
    {
        internal readonly int Ordinal = ordinal;
        internal readonly Rule Rule = rule;
        internal readonly List<ushort> Positions = [(ushort)input];
        internal int Step;
        internal readonly bool[] Presence = new bool[3];
        internal bool FenceSeen, FenceValid, Mismatch, VerticalSeparator;
        internal short Row, Column, MaximumColumns = -1;
        internal short FirstRow = -1, FirstColumn = -1, SecondRow = -1, SecondColumn = -1;
        internal bool Fence => Rule.Kind == 7;
        internal bool Table => Rule.Kind == 14;
        internal bool Vertical => Rule.Kind == 15;
    }
    private sealed class State
    {
        internal int Input = -1, Error;
        internal bool Closed;
        internal readonly List<Frame> Frames = new();
    }
    private readonly Rule[] rules;
    private readonly HashSet<ushort> delimiters;
    private readonly ushort fracbar, slantedbar, verticalbar, minus, slash;
    internal LegacyMath2ObservableTokens Gate { get; }
    private LegacyMath2Ee4Formatter(LegacyMath2ObservableTokens gate)
    {
        Gate = gate; var alphabet = gate.ReadOwnedAlphabet();
        ushort Find(string text) => alphabet.TryGetLabel(text, out var label) ? label
            : throw new InvalidDataException("Actual EE4 AL binding missing.");
        rules = [new(3,[Find("√"),Find("</sqrt>")]),
            new(9,[Find("<frac>"),Find("<fracbar>"),Find("</frac>")]),
            new(10,[Find("<slantedfrac>"),Find("<slantedfracbar>"),Find("</slantedfrac>")]),
            new(4,[Find("<sub>"),Find("</sub>")]),new(5,[Find("<sup>"),Find("</sup>")]),
            new(11,[Find("<subsup>"),Find("<subsup2>"),Find("</subsup>")]),
            new(12,[Find("<under>"),Find("<under2>"),Find("</under>")]),
            new(13,[Find("<over>"),Find("<over2>"),Find("</over>")]),
            new(16,[Find("<underover>"),Find("<underover2>"),Find("<underover3>"),Find("</underover>")]),
            new(6,[Find("<presup>"),Find("</presup>")]),
            new(14,[Find("<table>"),Find("<&>"),Find("<\\>"),Find("</table>")]),
            new(15,[Find("<vop>"),Find("<\\>"),Find("<vopbar>"),Find("</vop>")]),
            new(7,[Find("<fence>"),Find("</fence>")]),new(8,[Find("\u2402"),Find("\u2403")])];
        delimiters = new();
        foreach (string text in new[] { "(", "⟨", "⌈", "⌊", "{", "[", ")", "⟩", "⌉", "⌋", "}", "]", "|", "‖" })
            if (alphabet.TryGetLabel(text, out ushort label)) delimiters.Add(label);
        fracbar = Find("<fracbar>"); slantedbar = Find("<slantedfracbar>"); verticalbar = Find("<vopbar>");
        minus = Find("-"); slash = Find("/");
    }
    internal static LegacyMath2Ee4Formatter Bind(LegacyMath2ObservableTokens gate)
    { ArgumentNullException.ThrowIfNull(gate); return new(gate); }
    private static int Parts(int kind) => kind is >= 3 and <= 8 ? 1 : kind is >= 9 and <= 13 ? 2
        : kind is >= 14 and <= 16 ? 3 : -1;
    private static ushort Tag(Frame frame, int step) => step < frame.Rule.Tags.Length ? frame.Rule.Tags[step] : (ushort)0;
    private static void ObservePair(Frame frame)
    {
        if (frame.FirstRow == -1)
        { frame.FirstRow = frame.Row; frame.FirstColumn = frame.Column; }
        else if (frame.SecondRow == -1 && (frame.FirstRow != frame.Row || frame.FirstColumn != frame.Column))
        { frame.SecondRow = frame.Row; frame.SecondColumn = frame.Column; }
    }
    //023E40: AST kind2/3, NOT the EF3 observable-token predicate.
    private void Presence(State state, int nodeKind, uint nodeUnit)
    {
        if (state.Frames.Count == 0) return;
        var current = state.Frames[^1]; bool operand = nodeKind is 2 or 3;
        current.Presence[current.Step] |= operand;
        if (current.Presence[current.Step])
            foreach (var ancestor in state.Frames.Take(state.Frames.Count - 1)) ancestor.Presence[ancestor.Step] = true;
        if (current.Fence && !current.FenceSeen)
        {
            current.FenceSeen = true;
            current.FenceValid = (nodeUnit & 0x00ff0000u) == 0 && delimiters.Contains((ushort)nodeUnit);
            if (!current.FenceValid) { state.Closed = true; state.Error = 2; }
        }
        if (current.Table && operand) ObservePair(current);
    }
    private static bool Match(Frame? frame, ushort token, out int matched)
    {
        matched = -1;
        if (frame == null) return false;
        if (frame.Table || frame.Vertical)
        {
            if (frame.Step == 0) return token == Tag(frame, 1) || token == Tag(frame, 2);
            if (frame.Step is not (1 or 2)) return false;
            if (token == Tag(frame, 1) || token == Tag(frame, 2)) return true;
            return token == Tag(frame, 3) && (!frame.Vertical || frame.Step == 2 || frame.VerticalSeparator);
        }
        for (int step = frame.Step + 1; step < 4; step++)
            if (Tag(frame, step) == token) { matched = step; return step == frame.Step + 1; }
        return false;
    }
    private static bool Sufficient(Frame frame)
    {
        int kind = frame.Rule.Kind, parts = Parts(kind);
        if (kind == 3) return true;
        if (kind is 9 or 10) return frame.Presence[0] || frame.Presence[1];
        for (int step = 0; step < parts; step++) if (!frame.Presence[step]) return false;
        return true;
    }
    private static void Pop(State state)
    {
        var removed = state.Frames[^1]; bool sufficient = Sufficient(removed);
        state.Frames.RemoveAt(state.Frames.Count - 1);
        if (state.Frames.Count != 0 && state.Frames[^1].Table && sufficient) ObservePair(state.Frames[^1]);
    }
    //024310 writes diagnostics and may find a matching ancestor without
    //consuming that ancestor. The caller's recovery loop handles it next.
    private static void Step(State state, ushort token)
    {
        if (state.Frames.Count == 0) { state.Error = 1; return; }
        var frame = state.Frames[^1]; int parts = Parts(frame.Rule.Kind);
        if (!Match(frame, token, out int matched))
        {
            state.Closed = matched == parts;
            state.Error = 1;
            for (int at = state.Frames.Count - 2; at >= 0; at--)
                if (Match(state.Frames[at], token, out _)) { state.Error = 3; break; }
            return;
        }
        state.Closed = frame.Table || frame.Vertical
            ? frame.Step >= 1 && token == Tag(frame, parts)
            : frame.Step + 1 == parts;
        bool presence = frame.Presence[frame.Step];
        frame.Positions.Add((ushort)state.Input);
        int kind = frame.Rule.Kind;
        if (!presence && kind != 9 && (kind > 15 || ((0xc408 >> kind) & 1) == 0)) state.Error = 4;
        if (frame.Table || frame.Vertical)
        {
            int step = 0; while (step < parts && Tag(frame, step) != token) step++;
            if (frame.Table)
            {
                if (step == 1) { frame.Column++; frame.Step = 1; return; }
                if (step == 2)
                {
                    if (frame.MaximumColumns != -1 && frame.MaximumColumns != frame.Column)
                    { state.Error = 5; frame.Mismatch = true; }
                    frame.MaximumColumns = System.Math.Max(frame.Column, frame.MaximumColumns);
                    frame.Column = 0; frame.Row++;
                }
            }
            frame.Step = step;
            if (frame.Vertical && token == Tag(frame, 2)) frame.VerticalSeparator = true;
        }
        else frame.Step++;
        if (!state.Closed) return;
        if (frame.Table)
        {
            if (frame.FirstRow < 0 || frame.FirstColumn < 0 || frame.SecondRow < 0 || frame.SecondColumn < 0)
            { state.Error = 4; return; }
            if (frame.MaximumColumns != -1 && frame.MaximumColumns != frame.Column)
            { state.Error = 5; frame.Mismatch = true; }
            frame.MaximumColumns = System.Math.Max(frame.Column, frame.MaximumColumns);
            return;
        }
        if (!frame.Vertical && frame.Fence)
        { if (!frame.FenceValid) state.Error = 4; }
        else if (!Sufficient(frame)) state.Error = 4;
    }
    private void Classify(State state, uint unit)
    {
        state.Closed = false; state.Error = 0; ushort token = (ushort)unit;
        if ((unit & 0x00ff0000u) == 0)
        {
            for (int ordinal = 0; ordinal < rules.Length; ordinal++)
                if (rules[ordinal].Tags[0] != 0 && rules[ordinal].Tags[0] == token)
                {
                    //0234F0 initializes the successful AST node unit to FFFF.
                    Presence(state, rules[ordinal].Kind, 0x0000ffff);
                    state.Frames.Add(new(ordinal, rules[ordinal], state.Input)); return;
                }
            foreach (var rule in rules)
                for (int step = 1; step < rule.Tags.Length; step++)
                    if (rule.Tags[step] == token) { Step(state, token); return; }
        }
        Presence(state, 2, unit);
    }
    internal sealed record Projection(uint[] Units, ushort[] Map, bool GrammarValid);
    // Explicit portable payload bound; not a native allocator or formula limit.
    private const int MaximumUnits = 65535;
    private static void Add(List<uint> units, uint word, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (units.Count == MaximumUnits) throw new NotSupportedException("Portable EE4 unit/map bound exceeded.");
        units.Add(word);
    }
    private static void Mark(Frame frame, List<uint> units, List<ushort> map)
    {
        foreach (ushort input in frame.Positions)
        {
            if (input >= map.Count || map[input] >= units.Count) throw new InvalidDataException("EE4 source-position map outside its owned extent.");
            int at = map[input]; units[at] = (units[at] & 0xffff0000u) | 0xffff;
        }
    }
    private static void Pad(Frame frame, List<uint> units, List<ushort> map, bool recovery,
        CancellationToken token)
    {
        int columns = 0, maximum = frame.MaximumColumns;
        int count = frame.Positions.Count - (recovery ? 1 : 0);
        uint separator = Tag(frame, 1);
        for (int at = 0; at < count; at++)
        {
            token.ThrowIfCancellationRequested(); ushort input = frame.Positions[at];
            int position = map[input]; ushort label = (ushort)units[position];
            if (label == Tag(frame, 0)) columns = 0;
            else if (label == separator) columns++;
            else if (label == Tag(frame, 2) || label == Tag(frame, 3))
            {
                if (columns < maximum)
                {
                    int missing = maximum - columns;
                    if (units.Count + missing > MaximumUnits) throw new NotSupportedException("Portable EE4 padding bound exceeded.");
                    for (int index = input; index < map.Count; index++) map[index] = checked((ushort)(map[index] + missing));
                    for (; columns < maximum; columns++) units.Insert(position, separator);
                }
                columns = 0;
            }
        }
        if (recovery) for (; columns < maximum; columns++) Add(units, separator, token);
    }
    private static bool Recover(State state, Frame frame, List<uint> units, List<ushort> map,
        CancellationToken token)
    {
        int kind = frame.Rule.Kind, parts = Parts(kind); bool possible;
        if (frame.Table)
            possible = frame.FirstRow >= 0 && frame.FirstColumn >= 0 && frame.SecondRow >= 0 && frame.SecondColumn >= 0;
        else if (frame.Fence)
            possible = frame.Step + 1 == parts && frame.Presence.Take(parts).All(value => value) && frame.FenceValid;
        else if (frame.Vertical)
            possible = frame.Step == 2 && (frame.Presence[0] || frame.Presence[1]);
        else possible = frame.Step + 1 == parts && Sufficient(frame);
        if (!possible) return false;
        ushort closer = Tag(frame, parts); Step(state, closer);
        if (state.Closed && frame.Table && frame.Mismatch) Pad(frame, units, map, true, token);
        Add(units, closer, token);
        if (state.Closed) Pop(state);
        return true;
    }
    internal Projection FormatPrepared(ReadOnlySpan<uint> original, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (original.Length > 1000) throw new NotSupportedException("Bounded source prefix required.");
        foreach (uint unit in original)
            if ((unit & 0x00ff0000u) != 0) throw new NotSupportedException("Existing-source byte-two lane is not supported.");
        var state = new State(); var units = new List<uint>(); var map = new List<ushort>();
        for (state.Input = 0; state.Input < original.Length; state.Input++)
        {
            token.ThrowIfCancellationRequested(); map.Add(checked((ushort)units.Count));
            bool opened = false;
            while (true)
            {
                int before = state.Frames.Count; Classify(state, original[state.Input]);
                opened = before < state.Frames.Count;
                if (state.Error != 3 || state.Frames.Count == 0) break;
                var top = state.Frames[^1];
                if (!Recover(state, top, units, map, token)) { Mark(top, units, map); state.Frames.RemoveAt(state.Frames.Count - 1); }
                else map[^1] = checked((ushort)(map[^1] + 1));
                if (state.Frames.Count == 0) break;
            }
            bool bad = state.Error is 1 or 4;
            Add(units, bad ? 0x0000ffffu : original[state.Input], token);
            if (state.Frames.Count == 0 || !state.Closed) continue;
            var frame = state.Frames[^1];
            if (bad) Mark(frame, units, map);
            if (state.Error == 2)
            {
                int index = state.Frames.Count - 1 - (opened && state.Frames.Count > 1 ? 1 : 0);
                Mark(state.Frames[index], units, map); state.Frames.RemoveAt(index);
            }
            else
            {
                if (frame.Table && frame.Mismatch && !bad) Pad(frame, units, map, false, token);
                Pop(state);
            }
        }
        for (int remaining = state.Frames.Count; remaining > 0; remaining--)
        {
            var frame = state.Frames[^1];
            if (!Recover(state, frame, units, map, token)) { Mark(frame, units, map); state.Frames.RemoveAt(state.Frames.Count - 1); }
        }
        var output = new List<uint>(); var resultMap = new List<ushort>(); int input = 0;
        foreach (uint unit in units)
        {
            token.ThrowIfCancellationRequested(); uint prior = input < original.Length ? original[input] : 0x0000ffff;
            if ((ushort)unit == ushort.MaxValue)
            {
                ushort label = (ushort)prior;
                if ((prior & 0x00ff0000u) == 0 && (label == fracbar || label == verticalbar || label == slantedbar))
                {
                    resultMap.Add(checked((ushort)output.Count));
                    Add(output, (prior & 0xffff0000u) | (label == slantedbar ? slash : minus), token);
                }
                else resultMap.Add(ushort.MaxValue);
                input++; continue;
            }
            if ((ushort)unit == (ushort)prior && (unit & 0x00ff0000u) == (prior & 0x00ff0000u))
            { resultMap.Add(checked((ushort)output.Count)); input++; }
            Add(output, unit, token);
        }
        var check = new State(); bool valid = true;
        for (check.Input = 0; check.Input < output.Count; check.Input++)
        {
            token.ThrowIfCancellationRequested(); Classify(check, output[check.Input]);
            if (check.Error != 0) { valid = false; break; }
            if (check.Closed) Pop(check);
        }
        token.ThrowIfCancellationRequested();
        return new(output.ToArray(), resultMap.ToArray(), valid && check.Frames.Count == 0);
    }
}
