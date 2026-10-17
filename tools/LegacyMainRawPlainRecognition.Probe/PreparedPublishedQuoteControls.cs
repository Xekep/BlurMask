using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

internal static class PreparedPublishedQuoteControls
{
    internal static int Write(Utf8JsonWriter w,byte[] acoustic)
    {
        var alphabet=LegacyInkAlphabet.Load(acoustic.AsSpan(99),BinaryPrimitives.ReadUInt16LittleEndian(acoustic.AsSpan(97)));
        var normalizer=LegacyNativeWordQuoteNormalization.Prepare(alphabet);
        ushort header=BinaryPrimitives.ReadUInt16LittleEndian(alphabet.CopySerializedHeader().AsSpan(6));
        ushort syntheticA=(ushort)(alphabet.LabelCount+1),syntheticQ=(ushort)(alphabet.LabelCount+2);
        w.WriteStartArray("preparedQuotes");
        for(int test=0;test<8;test++)
        {
            var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
            var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);
            for(int n=0;n<1+test%2;n++)
            {
                var word=LegacyNativeWordBuilder.CreateEmpty(pool,new byte[120]);
                ushort[] labels=(test%3) switch
                {0=>[syntheticA,74,syntheticA],1=>[header,syntheticQ,syntheticA],_=>[74,syntheticA,syntheticQ]};
                ulong previous=ulong.MaxValue;
                for(int i=0;i<labels.Length;i++)
                {
                    var insertion=word.AppendGlyph((ulong)(n*16+i+1),i,false,new byte[40]);
                    if(previous!=ulong.MaxValue)word.LinkGlyph(previous,insertion.Reference);
                    word.SetLastOrdinarySymbol(labels[i]);previous=insertion.Reference;
                }
                if(publisher.Publish(word,(_,_)=>0,(_,_)=>0) is null)throw new Exception("Prepared ordinary WORD publication failed");
            }
            var words=new List<LegacyNativePlainWordRecord>();
            for(var node=publisher.Head;node is not null;node=node.Next)words.Add(node.Word);
            var references=words.Select(word=>word.FlatReferences.Reverse().ToArray()).ToArray();
            var before=references.Select(ids=>ids.Select(id=>pool.CopyRecord(id)).ToArray()).ToArray();
            byte[] leading=[0xc1,0x4e,0xa5,0x73],trailing=[0xd2,0x5f,0xb6,0x84];
            normalizer.NormalizePublished(pool,words,leading,trailing);
            w.WriteStartObject();Rows("before",before);Rows("after",references.Select(ids=>ids.Select(id=>pool.CopyRecord(id)).ToArray()).ToArray());
            w.WriteString("leading",Convert.ToHexString(leading));w.WriteString("trailing",Convert.ToHexString(trailing));w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteStartArray("preparedTrailingCancellation");
        for(int publishedEntry=0;publishedEntry<2;publishedEntry++)
        {
            var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
            var word=LegacyNativeWordBuilder.CreateEmpty(pool,new byte[120]);
            var glyph=word.AppendGlyph(1,0,false,new byte[40]);
            byte[] record=pool.CopyRecord(glyph.Reference);
            BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(4),3);
            BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(8),6);
            pool.ReplaceRecord(glyph.Reference,record);
            var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);
            var node=publisher.Publish(word,(_,_)=>0,(_,_)=>0)??throw new Exception("Prepared cancellation publication failed");
            byte[] leading=[0xc1,0x4e,0xa5,0x73],trailing=[0xd2,0x5f,0xb6,0x84];
            using var canceled=new CancellationTokenSource();int lookups=0;
            ushort Lookup(string text){if(text!=" ")throw new Exception("Wrong prepared control");if(++lookups==2)canceled.Cancel();return 74;}
            try
            {
                if(publishedEntry!=0)normalizer.NormalizePublished(pool,[node.Word],leading,trailing,Lookup,canceled.Token);
                else normalizer.Normalize(pool,[word],leading,trailing,Lookup,canceled.Token);
                throw new Exception("Trailing lookup cancellation was ignored");
            }
            catch(OperationCanceledException){}
            if(lookups!=2||!record.AsSpan().SequenceEqual(pool.CopyRecord(glyph.Reference))||
                BinaryPrimitives.ReadUInt16LittleEndian(trailing)!=74||trailing[2]!=0)
                throw new Exception("Trailing cancellation erased its scratch/pool prefix");
            w.WriteStartObject();w.WriteNumber("lookups",lookups);w.WriteBoolean("published",publishedEntry!=0);
            w.WriteString("leading",Convert.ToHexString(leading));w.WriteString("trailing",Convert.ToHexString(trailing));
            w.WriteString("before",Convert.ToHexString(record));w.WriteString("after",Convert.ToHexString(pool.CopyRecord(glyph.Reference)));w.WriteEndObject();
        }
        w.WriteEndArray();return 2;
        void Rows(string name,byte[][][] rows)
        {w.WriteStartArray(name);foreach(var word in rows){w.WriteStartArray();foreach(var row in word)w.WriteStringValue(Convert.ToHexString(row));w.WriteEndArray();}w.WriteEndArray();}
    }
}
