using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=3)throw new ArgumentException("AK scalar.bin words.json");
byte[] resource=File.ReadAllBytes(args[0]);
var alphabet=LegacyInkAlphabet.Load(resource.AsSpan(99),BinaryPrimitives.ReadUInt16LittleEndian(resource.AsSpan(97)));
var normalizer=LegacyNativeWordQuoteNormalization.Prepare(alphabet);
ushort Lookup(string value)=>value switch{"\n"=>501," "=>502,"\u00a0"=>503,_=>throw new Exception("Unknown control")};
using(var stream=File.Create(args[1]))
{
    byte[] scratch=new byte[4];
    for(int kind=0;kind<8;kind++)for(int label=0;label<65536;label++)
    {
        scratch[0]=0xc1;scratch[1]=0x4e;scratch[2]=0xa5;scratch[3]=0x73;
        LegacyNativeGlyphUtf8Tokens.ReadScalar(new((ushort)(kind|0x9f80),(ushort)label),scratch,Lookup);
        stream.Write(scratch);
    }
}
var cases=new List<Case>();int guards=0;
ushort[] labels=[178,193,194,74,0,4,5,6,7,0x8000,0xffff];
for(int test=0;test<272;test++)
{
    var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
    var words=new List<LegacyNativeWordBuilder>();var initial=new List<string[]>();var refs=new List<ulong[]>();
    for(int w=0;w<1+test%3;w++)
    {
        var word=LegacyNativeWordBuilder.CreateEmpty(pool,new byte[120]);words.Add(word);
        var rows=new List<string>();var ids=new List<ulong>();
        int length=test>=256?4:1+(test+w)%5;
        for(int i=0;i<length;i++)
        {
            ulong key=(ulong)(1+w*16+i);var ins=word.AppendGlyph(key,test*32+w*8+i,false,new byte[40]);
            byte[] row=pool.CopyRecord(ins.Reference);
            int kind=test>=256?(i==0?0:i==1?(test-256)%8:0):(test+i+w)%8;
            ushort label=test>=256?(i==0?(ushort)178:i==2?(ushort)74:(ushort)(193+(test-256)/8)):labels[(test*3+i+w)%labels.Length];
            BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(4),(ushort)(0xa580|kind));
            BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8),label);
            row[10]=0xb6;row[11]=0x51;row[23]=0x91;
            pool.ReplaceRecord(ins.Reference,row);
            if(i>0)word.LinkGlyph(ids[^1],ins.Reference);
            ids.Add(ins.Reference);
        }
        foreach(ulong id in ids)rows.Add(Convert.ToHexString(pool.CopyRecord(id)));
        refs.Add(ids.ToArray());initial.Add(rows.ToArray());
    }
    byte[] leading=[0xc1,0x4e,0xa5,0x73],trailing=[0xd2,0x5f,0xb6,0x84];
    normalizer.Normalize(pool,words,leading,trailing,Lookup);
    var final=refs.Select(ids=>ids.Select(id=>Convert.ToHexString(pool.CopyRecord(id))).ToArray()).ToArray();
    cases.Add(new(initial.ToArray(),final,Convert.ToHexString(leading),Convert.ToHexString(trailing)));
}
Reject(()=>LegacyNativeGlyphUtf8Tokens.ReadScalar(new(3,4),new byte[4]));
Reject(()=>LegacyNativeGlyphUtf8Tokens.ReadScalar(new(0,4),new byte[3]));
var emptyPool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
Reject(()=>normalizer.Normalize(emptyPool,[LegacyNativeWordBuilder.CreateEmpty(emptyPool,new byte[120])],new byte[4],new byte[4]));
var foreign=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
Reject(()=>normalizer.Normalize(emptyPool,[LegacyNativeWordBuilder.CreateEmpty(foreign,new byte[120])],new byte[4],new byte[4]));
Reject(()=>normalizer.Normalize(emptyPool,[],new byte[4],new byte[4],null,new(true)));
Reject(()=>{byte[] scratch=new byte[4];normalizer.Normalize(emptyPool,[],scratch,scratch);});
using(var file=File.Create(args[2]))JsonSerializer.Serialize(file,new Report(cases.ToArray(),guards),Context.Default.Report);
Console.WriteLine($"PASS524288 scalar descriptors;{cases.Count} prepared word-list normalizations;{guards} guards; no winner claim.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid invocation accepted.");}
record Case(string[][] Initial,string[][] Final,string Leading,string Trailing);
record Report(Case[] Cases,int Guards);
[System.Text.Json.Serialization.JsonSerializable(typeof(Report))]
partial class Context:System.Text.Json.Serialization.JsonSerializerContext;
