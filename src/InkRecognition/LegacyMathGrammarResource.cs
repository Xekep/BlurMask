using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace InkRecognition;

/// <summary>Authenticated original Math grammar and its bound AK terminal sets.
/// Configuration only: no acoustic hypotheses, matcher state or candidate authority.</summary>
public sealed class LegacyMathGrammarResource
{
    public const int ResourceBytes = 6637;
    public const string ResourceFingerprint = "adc9b7adce2d3af1a20a6262467afc0b1c6365bf27c120596d80c86ec54893f8";
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private readonly LegacyInkAlphabet alphabet;
    public ReadOnlyCollection<Rule> Rules { get; }
    public uint TrailingScalar38 { get; }
    public int AlphabetCount => alphabet.LabelCount;
    public bool HasCandidateAuthority => false;
    private LegacyMathGrammarResource(LegacyInkAlphabet alphabet, Rule[] rules, uint trailing)
    { this.alphabet = alphabet; Rules = Array.AsReadOnly(rules); TrailingScalar38 = trailing; }

    public static LegacyMathGrammarResource LoadActual(ReadOnlySpan<byte> grammarResource,
        ReadOnlySpan<byte> acousticResource, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (grammarResource.Length != ResourceBytes || acousticResource.Length != LegacyMathMixed107RawResource.ResourceBytes)
            throw new ArgumentException("Exact bounded original grammar and Math AK resources required.");
        byte[] grammar = grammarResource.ToArray(), acoustic = acousticResource.ToArray();
        try
        {
            Authenticate(grammar, ResourceFingerprint, token);
            Authenticate(acoustic, LegacyMathMixed107RawResource.ResourceFingerprint, token);
            if (BinaryPrimitives.ReadUInt32LittleEndian(acoustic.AsSpan(26)) != 0x2b ||
                BinaryPrimitives.ReadUInt16LittleEndian(acoustic.AsSpan(32)) != 0x104)
                throw new InvalidDataException("Actual type2B/version104 alphabet required.");
            var alphabet = LegacyInkAlphabet.Load(acoustic.AsSpan(34), 0x104);
            if (alphabet.LabelCount != 259 || alphabet.TryGetText(256, out _))
                throw new InvalidDataException("Actual259-label alphabet with virtual256 required.");
            var reader = new Reader(grammar, token);
            reader.Header(0x138a, ResourceBytes);
            int count = reader.Count(26);
            if (count != 26) throw new InvalidDataException("Actual26-rule grammar required.");
            var rules = new Rule[count]; int children = 0, terminals = 0;
            for (int i = 0; i < count; i++)
            {
                int offset = reader.Position; byte tag = reader.U8(); string name = reader.Text();
                int childCount = reader.Count(53); children = checked(children + childCount);
                if (children > 53) throw new InvalidDataException("Whole grammar child bound exceeded.");
                var nodes = new Child[childCount];
                for (int j = 0; j < nodes.Length; j++)
                { nodes[j] = reader.Child(alphabet); terminals += nodes[j].TerminalLabels.Count; }
                rules[i] = new(i, offset, reader.Position, tag, name, nodes);
            }
            uint trailing = reader.U32(); reader.RequireEnd(ResourceBytes);
            if (children != 53 || terminals != 297) throw new InvalidDataException("Complete actual grammar topology required.");
            token.ThrowIfCancellationRequested();
            return new(alphabet, rules, trailing);
        }
        finally { CryptographicOperations.ZeroMemory(grammar); CryptographicOperations.ZeroMemory(acoustic); }
    }
    private static void Authenticate(byte[] source, string fingerprint, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        for (int at = 0; at < source.Length; at += 16384)
        { token.ThrowIfCancellationRequested(); hash.AppendData(source, at, Math.Min(16384, source.Length - at)); }
        if (!Convert.ToHexString(hash.GetHashAndReset()).Equals(fingerprint, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Original resource fingerprint required.");
        token.ThrowIfCancellationRequested();
    }
    public bool TryGetTerminalText(ushort label, out string text) => alphabet.TryGetText(label, out text!);

    //Reconstructs the stored versions from parsed fields; not a retained raw-byte copy.
    public byte[] WriteSerialized(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); using var stream = new MemoryStream(ResourceBytes);
        using var writer = new BinaryWriter(stream, Utf8, true);
        WriteHeader(writer, 0x138a, ResourceBytes); writer.Write((uint)Rules.Count);
        foreach (var rule in Rules)
        {
            token.ThrowIfCancellationRequested(); writer.Write(rule.Tag); Text(writer, rule.Name);
            writer.Write((uint)rule.Children.Count);
            foreach (var child in rule.Children)
            {
                token.ThrowIfCancellationRequested(); WriteHeader(writer, child.TypeId, child.End - child.Offset);
                Text(writer, child.Name); writer.Write(child.PayloadVersion);
                if (child.TypeId == 0x13a8)
                { writer.Write((uint)child.TerminalNames.Count); foreach (string name in child.TerminalNames) Text(writer, name); }
                else if (child.TypeId == 0x13ab)
                { writer.Write(child.Field18); WriteTrie(writer, child.Trie20, token); }
                else foreach (var field in child.Fields)
                { if (field.Width == 1) writer.Write(checked((byte)field.Value)); else writer.Write(field.Value); }
            }
        }
        writer.Write(TrailingScalar38); writer.Flush();
        if (stream.Length != ResourceBytes) throw new InvalidDataException("Parsed grammar reconstruction extent changed.");
        token.ThrowIfCancellationRequested(); return stream.ToArray();
    }
    private static void WriteHeader(BinaryWriter writer, uint kind, int bytes)
    { writer.Write(0x0101fe90U); writer.Write(checked((uint)(bytes - 8))); writer.Write(kind); writer.Write((ushort)0); writer.Write((ushort)0x100); }
    private static void Text(BinaryWriter writer, string text)
    { byte[] bytes = Utf8.GetBytes(text); writer.Write((uint)bytes.Length); writer.Write(bytes); }
    private static void WriteTrie(BinaryWriter writer, ReadOnlyCollection<TrieNode> nodes, CancellationToken token)
    {
        writer.Write((uint)nodes.Count);
        foreach (var node in nodes)
        { token.ThrowIfCancellationRequested(); Text(writer, node.Text); writer.Write(node.Field12); writer.Write(node.Field13); WriteTrie(writer, node.Children, token); }
    }

    public sealed class Rule
    {
        public int Index { get; } public int Offset { get; } public int End { get; }
        public byte Tag { get; } public string Name { get; } public ReadOnlyCollection<Child> Children { get; }
        internal Rule(int index, int offset, int end, byte tag, string name, Child[] children)
        { Index=index; Offset=offset; End=end; Tag=tag; Name=name; Children=Array.AsReadOnly(children); }
    }
    public readonly record struct Field(int Offset, int Width, uint Value);
    public sealed class Child
    {
        public int Offset { get; } public int End { get; } public ushort TypeId { get; } public ushort PayloadVersion { get; }
        public string Name { get; } public ReadOnlyCollection<Field> Fields { get; }
        public ReadOnlyCollection<string> TerminalNames { get; } public ReadOnlyCollection<ushort> TerminalLabels { get; }
        public byte Field18 { get; } public ReadOnlyCollection<TrieNode> Trie20 { get; }
        internal Child(int offset,int end,ushort kind,ushort version,string name,Field[] fields,string[] names,ushort[] labels,byte field18,TrieNode[] trie)
        { Offset=offset; End=end; TypeId=kind; PayloadVersion=version; Name=name; Fields=Array.AsReadOnly(fields); TerminalNames=Array.AsReadOnly(names); TerminalLabels=Array.AsReadOnly(labels); Field18=field18; Trie20=Array.AsReadOnly(trie); }
    }
    public sealed class TrieNode
    {
        public string Text { get; } public byte Field12 { get; } public byte Field13 { get; }
        public ReadOnlyCollection<TrieNode> Children { get; }
        internal TrieNode(string text,byte field12,byte field13,TrieNode[] children)
        { Text=text; Field12=field12; Field13=field13; Children=Array.AsReadOnly(children); }
    }
    private sealed class Reader(byte[] data, CancellationToken token)
    {
        public int Position { get; private set; }
        private int trieCount;
        private ReadOnlySpan<byte> Take(int size)
        {
            token.ThrowIfCancellationRequested();
            if (size < 0 || size > data.Length - Position) throw new InvalidDataException("Truncated grammar.");
            var result=data.AsSpan(Position,size); Position+=size; return result;
        }
        public byte U8()=>Take(1)[0]; public ushort U16()=>BinaryPrimitives.ReadUInt16LittleEndian(Take(2));
        public uint U32()=>BinaryPrimitives.ReadUInt32LittleEndian(Take(4));
        public int Count(int maximum)
        { uint count=U32(); return count <= maximum ? (int)count : throw new InvalidDataException("Grammar allocation bound exceeded."); }
        public string Text()=>Utf8.GetString(Take(Count(ResourceBytes)));
        public void Header(uint kind,int bytes)
        { if(U32()!=0x0101fe90||U32()!=bytes-8||U32()!=kind||U16()!=0||U16()!=0x100) throw new InvalidDataException("Actual grammar frame required."); }
        public void RequireEnd(int expected)
        { if(Position!=expected) throw new InvalidDataException("Unconsumed grammar data."); }
        public Child Child(LegacyInkAlphabet alphabet)
        {
            int offset=Position;
            if(U32()!=0x0101fe90) throw new InvalidDataException("Child frame required.");
            int bytes=Count(ResourceBytes); int end=checked(offset+bytes+8); ushort kind=checked((ushort)U32());
            if(bytes<8||end>data.Length||U16()!=0||U16()!=0x100) throw new InvalidDataException("Bounded actual child frame required.");
            string name=Text(); ushort version=U16();
            ushort expected=kind switch {0x13aa or 0x145b=>0x101,0x13e2=>0x103,_=>0x100};
            if(version!=expected) throw new InvalidDataException("Actual stored child version required.");
            Field[] fields=[]; string[] names=[]; ushort[] labels=[]; byte field18=0; TrieNode[] trie=[];
            if(kind==0x13a8)
            {
                int count=Count(297); names=new string[count]; labels=new ushort[count];
                for(int i=0;i<count;i++)
                { names[i]=Text(); if(!alphabet.TryGetLabel(names[i],out labels[i])) throw new InvalidDataException("Terminal is absent from same actual AK alphabet."); }
            }
            else if(kind==0x13ab) { field18=U8(); trie=Trie(0); }
            else
            {
                (int Offset,int Width)[] layout=kind switch
                {
                    0x13a9=>[(0x18,4)],
                    0x13aa or 0x13e2=>[(0x18,4),(0x1c,4),(0x20,1),(0x21,1),(0x22,1)],
                    0x13ad or 0x13ae or 0x13af or 0x13e3=>[(0x18,4),(0x1c,4),(0x20,1),(0x21,1)],
                    0x13ac or 0x13b0=>[(0x18,4),(0x1c,4),(0x20,4),(0x24,1),(0x25,1),(0x26,1)],
                    0x13b1 or 0x13b5=>[(0x18,4),(0x1c,4),(0x20,1),(0x21,1),(0x22,1)],
                    0x13b2=>[(0x18,4),(0x1c,4),(0x20,1),(0x21,1),(0x22,1),(0x23,1)],
                    0x13b3=>[(0x18,4),(0x1c,4),(0x20,4),(0x24,1),(0x25,1),(0x26,1),(0x27,1)],
                    0x145b=>[(0x18,4),(0x1c,4),(0x20,1)], 0x145c=>[(0x18,4),(0x1c,4)],
                    0x145d=>[(0x18,4),(0x20,4),(0x1c,4),(0x24,1),(0x26,1),(0x25,1)],
                    _=>throw new NotSupportedException("Unrestored grammar child class.")
                };
                fields=new Field[layout.Length];
                for(int i=0;i<fields.Length;i++) fields[i]=new(layout[i].Offset,layout[i].Width,layout[i].Width==1?U8():U32());
            }
            RequireEnd(end); return new(offset,end,kind,version,name,fields,names,labels,field18,trie);
        }
        private TrieNode[] Trie(int depth)
        {
            if(depth>64) throw new InvalidDataException("Trie depth bound exceeded.");
            int count=Count(ResourceBytes); trieCount=checked(trieCount+count);
            if(trieCount>ResourceBytes) throw new InvalidDataException("Whole trie allocation bound exceeded.");
            var result=new TrieNode[count];
            for(int i=0;i<count;i++) { string text=Text(); byte a=U8(),b=U8(); result[i]=new(text,a,b,Trie(depth+1)); }
            return result;
        }
    }
}
