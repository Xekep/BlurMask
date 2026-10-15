using InkRecognition;
using System.Text;
if(args.Length!=3)throw new ArgumentException("case-data normalization-data publication-data");
using var c=File.OpenRead(args[0]);var caseData=LegacyNativeCaseData.Load(c);using var n=File.OpenRead(args[1]);var normalization=LegacyNativeUnicodeNormalizationData.Load(n);using var p=File.OpenRead(args[2]);var publication=LegacyNativeTextPublicationData.Load(p);
string[] texts=["A","a","É","é","E","e","ß","SS","S","s","ſ","ı","I","i","İ","i\u0307","Ǆ","ǅ","ǆ","Й","й","И","и","Ё","ё","Е","е","1","ab","e\u0301","V","v"];
var alphabet=Fixture(texts,30);var owner=new LegacyNativeAlphabetCaseLookup(alphabet,caseData,normalization,publication);
ushort X=65535;ushort[][] expected=[
 [1,0,0],[1,0,0],[3,2,4],[3,2,4],[5,4,4],[5,4,4],[6,X,X],[X,X,X],
 [9,8,8],[9,8,8],[10,8,8],[11,12,12],[13,12,12],[13,12,12],[15,14,12],[15,14,12],
 //Native alternating[01C4,01C4] takes precedence over lowerSingle[01C4,+2]:
 //its lower is01C5, not Unicode01C6. Inverse recursion catches this mismatch.
 [17,16,16],[17,16,16],[18,16,16],[20,19,21],[20,19,21],[22,21,21],[22,21,21],
 [24,23,25],[24,23,25],[26,25,25],[26,25,25],[X,X,X],[X,X,X],[3,2,4],[X,X,X],[31,30,30]];
int checkedForms=0;
for(ushort label=0;label<texts.Length;label++)foreach(bool recursive in new[]{false,true})
{
 var got=owner.Read(label,0,0,recursive);ushort[] wanted=expected[label].ToArray();
 if(recursive&&label is 10 or 11 or 18 or 31){wanted[1]=X;wanted[2]=X;}
 bool has=label is not(27 or 30);
 if(got.InputHasCase!=has||!new[]{got.Lower,got.Upper,got.StrippedUpper}.SequenceEqual(wanted))throw new Exception($"Independent lookup/recursion mismatch label{label},recursive{recursive}: {got}.");
 checkedForms++;
}
//Deleting a looked-up normalized form does not create a fresh alphabet label.
var missing=Fixture(["É","é"],-1);var missingOwner=new LegacyNativeAlphabetCaseLookup(missing,caseData,normalization,publication);
if(missingOwner.Read(0,0,0,true)!=new LegacyNativeAlphabetCaseLookupResult(true,1,0,X)||missing.LabelCount!=2)throw new Exception("Missing stripped E was invented or stored AL mutated.");
//BOM is consumed by actual default charset iteration, while exact lookup keys
//remain the alphabet's original UTF8 intervals.
var bom=Fixture(["\ufeffA","A","a"],-1);var bomOwner=new LegacyNativeAlphabetCaseLookup(bom,caseData,normalization,publication);
if(bomOwner.Read(0,0,0,true)!=new LegacyNativeAlphabetCaseLookupResult(true,2,1,1))throw new Exception("BOM/native iterator slice lookup changed.");
try{owner.Read(ushort.MaxValue,0,0,false);throw new Exception("Invalid label accepted.");}catch(ArgumentOutOfRangeException){}
try{owner.Read(0,0,0,true,new(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS {checkedForms} independent exactUTF8/one-tailoredcluster/recursive label triples; Cyrillicaccent forms, multi-scalar cluster, SS refusal, long-s/dotless-i/virtual inverse failures, missinglookup/BOM/cancel. Readable-owned AL only, no mutable construction/failureABI.");

static LegacyInkAlphabet Fixture(string[] texts,int virtualLabel)
{
 //Complete actual0527E0 hash dimensions:16 ->512 ->32768. Buckets group
 //exact UTF8 keys by hash bits16..19,6..10,0..5, then byte-sort collisions.
 var rows=texts.Select((text,id)=>new{Bytes=Encoding.UTF8.GetBytes(text),Id=(ushort)id,Hash=Hash(Encoding.UTF8.GetBytes(text))}).OrderBy(r=>Bucket(r.Hash)).ThenBy(r=>Convert.ToHexString(r.Bytes),StringComparer.Ordinal).ToArray();
 var buckets=rows.GroupBy(r=>Bucket(r.Hash)).ToArray();var offsets=new Dictionary<int,ushort>();for(int i=0;i<buckets.Length;i++)offsets[buckets[i].Key]=(ushort)i;
 using var m=new MemoryStream();using var w=new BinaryWriter(m);for(int i=0;i<7;i++)w.Write((ushort)0);w.Write((ushort)texts.Length);w.Write((ushort)16);
 for(int i=0;i<16;i++)w.Write((ushort)(i*32));for(int i=0;i<512;i++)w.Write((ushort)(i*64));for(int i=0;i<32768;i++)w.Write(offsets.TryGetValue(i,out ushort value)?value:ushort.MaxValue);
 int count=0;w.Write((ushort)0);foreach(var bucket in buckets){count+=bucket.Count();w.Write((ushort)count);}
 uint at=0;foreach(var row in rows){w.Write(at);w.Write((uint)row.Id);at+=(uint)row.Bytes.Length;}w.Write(at);w.Write(0u);
 foreach(var row in rows)w.Write(row.Bytes);for(int i=0;i<texts.Length;i++)w.Write(i==virtualLabel?ushort.MaxValue:(ushort)Array.FindIndex(rows,r=>r.Id==i));
 w.Write((byte)0);w.Write((ushort)0);return LegacyInkAlphabet.Load(m.ToArray(),0x103);
 static uint Hash(byte[] text){uint hash=0x1505;foreach(byte b in text)hash=unchecked(hash*33)^b;return hash;}
 static int Bucket(uint hash)=>(int)(((hash>>16)&15)*2048+((hash>>6)&31)*64+(hash&63));
}
