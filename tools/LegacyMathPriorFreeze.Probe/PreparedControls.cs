using InkRecognition;
using InkRecognition.Math;
using System.Text;

record PreparedCase(string Name,bool Succeeded,string[] Trace,Math2Mode0FreezeRow[] Rows);
static class PreparedControls
{
    internal sealed record Operand(bool Tag=true,bool Freeze=true,long Id=5,bool Selection=true,
        bool TextNode=true,byte TextTag=2,string Text="x",bool Pointer=true,bool CloneSucceeds=true);
    private sealed class Services(Operand[] operands):IMath2Mode0FreezeServices
    {
        internal readonly List<string> Trace=[];
        public int ChildCount=>operands.Length;
        public bool HasTag(int at){Trace.Add($"tag:{at}");return operands[at].Tag;}
        public bool HasFreeze(int at){Trace.Add($"freeze:{at}");return operands[at].Freeze;}
        public long TagNumber(int at){Trace.Add($"number:{at}");return operands[at].Id;}
        public object? FindSelection(long id){Trace.Add("selection:"+id);return operands.First(o=>o.Id==id).Selection?id:null;}
        public Math2Mode0FreezeText FirstChild(int at){Trace.Add($"child:{at}");var o=operands[at];return new(o.TextNode,o.TextTag,o.Text,o.Pointer);}
        public Math2Mode0FreezeRow? CloneRow(object selection,byte[] text)
        {
            long id=(long)selection;Trace.Add("clone:"+id);
            return operands.First(o=>o.Id==id).CloneSucceeds?new(text.ToArray(),[new(new LegacyNativeInkPosition(3),new LegacyNativeInkPosition(0x10003))],1):null;
        }
    }
    internal static PreparedCase[] Run()
    {
        var cases=new List<PreparedCase>();
        string[] attrs=["tag:0","freeze:0"],read=[..attrs,"number:0"],lookup=[..read,"selection:5","child:0"],copied=[..lookup,"clone:5"];
        void Test(string name,Operand[] operands,bool expected,string[] trace,int rows,string? text=null)
        {
            var services=new Services(operands);var actual=new List<Math2Mode0FreezeRow>();
            bool success=Math2Mode0FreezeExtraction.Append(services,actual);
            if(success!=expected||!services.Trace.SequenceEqual(trace)||actual.Count!=rows||actual.Any(r=>r.Flag!=1)||
                (text is not null&&!actual[0].Text.AsSpan().SequenceEqual(Encoding.UTF8.GetBytes(text))))throw new InvalidDataException(name);
            cases.Add(new(name,success,services.Trace.ToArray(),actual.ToArray()));
        }
        Test("absent-tag",[new(Tag:false)],true,attrs,0);
        Test("absent-freeze",[new(Freeze:false)],true,attrs,0);
        Test("sentinel-minus1",[new(Id:-1)],false,[..read,"child:0"],0);
        Test("missing-selection",[new(Selection:false)],false,lookup,0);
        Test("missing-first-child",[new(TextNode:false)],false,lookup,0);
        Test("unsupported-first-child",[new(TextTag:4)],false,lookup,0);
        Test("null-string-pointer",[new(TextTag:1,Pointer:false)],false,lookup,0);
        Test("heap-string",[new(TextTag:1,Text:"é")],true,copied,1,"é");
        Test("inline15",[new(Text:"abcdefghijklmno")],true,copied,1,"abcdefghijklmno");
        Test("empty-string3",[new(TextTag:3,Text:"ignored")],true,copied,1,"");
        Test("embedded-nul",[new(TextTag:1,Text:"a\0b")],true,copied,1,"a\0b");
        Test("clone-refusal",[new(CloneSucceeds:false)],false,copied,0);
        Test("copied-prefix",[new(),new(Id:6,Selection:false)],false,[..copied,"tag:1","freeze:1","number:1","selection:6","child:1"],1,"x");
        Test("max-is-not-sentinel",[new(Id:long.MaxValue)],true,[..read,"selection:9223372036854775807","child:0","clone:9223372036854775807"],1,"x");
        return cases.ToArray();
    }
}
