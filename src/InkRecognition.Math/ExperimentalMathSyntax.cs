using System.Collections.ObjectModel;
namespace InkRecognition.Math;

public readonly record struct ExperimentalMathRange(ulong First,ulong Last);
public sealed class ExperimentalMathToken
{
    private readonly byte[] utf8;
    public uint Unit {get;}
    public string? Text {get;}
    public IReadOnlyList<ExperimentalMathRange> Ranges {get;}
    internal ExperimentalMathToken(uint unit,byte[] bytes,string? text,ExperimentalMathRange[] ranges)
    {Unit=unit;utf8=bytes;Text=text;Ranges=Array.AsReadOnly(ranges);}
    public byte[] CopyUtf8()=>utf8.ToArray();
}
public sealed class ExperimentalMathSyntaxNode
{
    public int Id {get;}
    public string Kind {get;}
    public int FirstToken {get;}
    public int EndTokenExclusive {get;}
    public IReadOnlyList<int> Children {get;}
    internal ExperimentalMathSyntaxNode(int id,string kind,int first,int end,int[] children)
    {Id=id;Kind=kind;FirstToken=first;EndTokenExclusive=end;Children=Array.AsReadOnly(children);}
}
internal sealed class ExperimentalMathSyntax
{
    private sealed record Group(string Kind,string[] Stops);
    private static readonly IReadOnlyDictionary<string,Group> groups=new Dictionary<string,Group>
    {
        ["√"]=new("sqrt",["</sqrt>"]),["<frac>"]=new("frac",["<fracbar>","</frac>"]),
        ["<slantedfrac>"]=new("slantedfrac",["<slantedfracbar>","</slantedfrac>"]),
        ["<sup>"]=new("sup",["</sup>"]),["<sub>"]=new("sub",["</sub>"]),
        ["<subsup>"]=new("subsup",["<subsup2>","</subsup>"]),
        ["<under>"]=new("under",["<under2>","</under>"]),
        ["<underover>"]=new("underover",["<underover2>","<underover3>","</underover>"]),
        ["<fence>"]=new("fence",["</fence>"]),["<table>"]=new("table",["</table>"])
    };
    private readonly IReadOnlyList<ExperimentalMathToken> tokens;
    private readonly List<ExperimentalMathSyntaxNode?> nodes=[];
    private readonly CancellationToken token;
    private readonly int maximumNodes;
    private int position,depth;
    private ExperimentalMathSyntax(IReadOnlyList<ExperimentalMathToken> tokens,int maximumNodes,CancellationToken token)
    {this.tokens=tokens;this.maximumNodes=maximumNodes;this.token=token;}
    internal static ExperimentalMathSyntaxNode[] Parse(IReadOnlyList<ExperimentalMathToken> tokens,int maximumNodes,CancellationToken token)
    {
        if(tokens.Count==0)throw new FormatException("Nonempty supported tokens required.");
        var parser=new ExperimentalMathSyntax(tokens,maximumNodes,token);parser.Sequence(null,false);
        if(parser.position!=tokens.Count)throw new FormatException("Unconsumed structural tokens.");
        return parser.nodes.Select(n=>n??throw new InvalidOperationException("Incomplete syntax node.")).ToArray();
    }
    private int Reserve()
    {token.ThrowIfCancellationRequested();if(nodes.Count>=maximumNodes)throw new NotSupportedException("Whole syntax node envelope exceeded.");int id=nodes.Count;nodes.Add(null);return id;}
    private int Sequence(string? stop,bool table)
    {
        token.ThrowIfCancellationRequested();if(++depth>32)throw new FormatException("Structural nesting exceeds32.");
        int first=position,id=Reserve();var children=new List<int>();
        while(position<tokens.Count)
        {
            token.ThrowIfCancellationRequested();string text=tokens[position].Text??throw new FormatException("Strict UTF8 C-string token required.");
            if(text.Length==0)throw new FormatException("Nonempty token label required.");
            if(text==stop)
            {nodes[id]=new(id,"sequence",first,position,children.ToArray());position++;depth--;return id;}
            int at=position++;
            if(groups.TryGetValue(text,out var group))
            {
                int node=Reserve();var parts=new List<int>();
                if(group.Kind is "sup"or"sub"or"subsup"&&children.Count==0)throw new FormatException("Script requires a preceding base.");
                if(group.Kind=="fence"&&(position>=tokens.Count||tokens[position].Text is not("("or"["or"{"or"|"or"‖"or"⟨")))
                    throw new FormatException("Missing recognized opening fence.");
                foreach(string marker in group.Stops)
                {int part=Sequence(marker,group.Kind=="table");if(nodes[part]!.FirstToken==nodes[part]!.EndTokenExclusive)throw new FormatException("Empty structural operand.");parts.Add(part);}
                if(group.Kind=="fence")
                {
                    string right=tokens[at+1].Text switch{"("=>")","["=>"]","{"=>"}","|"=>"|","‖"=>"‖",_=>"⟩"};
                    if(position-at<4||tokens[position-2].Text!=right)throw new FormatException("Matching explicit right fence required.");
                }
                nodes[node]=new(node,group.Kind,at,position,parts.ToArray());children.Add(node);
            }
            else
            {
                bool separator=text is "<&>"or"<\\>";
                if(separator&&!table||!separator&&text.StartsWith('<')&&text.EndsWith('>')&&text.Length>2)
                    throw new FormatException("Unknown or misplaced structural token.");
                int leaf=Reserve();nodes[leaf]=new(leaf,separator?"tableSeparator":"token",at,position,[]);children.Add(leaf);
            }
        }
        if(stop is not null)throw new FormatException("Unclosed structural group.");
        nodes[id]=new(id,"sequence",first,position,children.ToArray());depth--;return id;
    }
}
