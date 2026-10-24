using System.Buffers.Binary;
using System.Text.Json;

namespace InkRecognition;

public sealed partial class LegacyFreshMainConfiguredWordAugmentation
{
    // One nullable reference on EVERY augmentation owner, including public null path.
    private readonly RawCollector? raw;

    internal static LegacyFreshMainConfiguredWordAugmentation PrepareRawDiagnostic(ReadOnlySpan<byte> resource,
        LegacyFreshMainConfiguredSingleLinePlainRecognitionResult actual,LegacyCursiveSeparationTable table,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData circles,
        LegacyMainConfiguredAlternativeResidue callerResidue,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        // Private construction; no observer/arrays/expected words enter this path.
        return PrepareCore(resource,actual,table,normalization,circles,callerResidue,token,RawCollector.Create());
    }

    internal void WriteRawDiagnostic(Utf8JsonWriter writer,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(writer);
        lock(gate)
        {
            if(raw is null||started==0||(!faulted&&!complete))
                throw new InvalidOperationException("Actual terminal diagnostic augmentation required.");
            raw.WriteTerminal(this,writer,token);
        }
    }

    internal void RequireRawDiagnosticDebit(LegacyConfiguredSourceOperationDebit actual)
    {
        if(!ReferenceEquals(debit,actual)||raw is null||started==0||complete==faulted)
            throw new InvalidOperationException("Same terminal augmentation RAW debit required.");
        raw.RequireDebitExport(this);
    }

    private readonly record struct RawHeader(int Kind,int Argument,int Start,int Length);
    private sealed class RawCollector
    {
        private const int PayloadLimit=32*1024*1024,EventLimit=32768;
        private const long VisitLimit=128L*1024*1024;
        // Named fields: owner8 + arrays16 + delegate8 + six int24 + four long32
        // + bool1 =89 protocol bytes; owner/delegate reservation192 is conservative.
        private LegacyFreshMainConfiguredWordAugmentation? owner;
        private byte[]? payload;
        private RawHeader[]? headers;
        private readonly Action<ulong,byte[]> readObserver;
        private int thread,phase,used,count,candidate=-1,comparison;
        private long allocated,visits,clearDebt,copied;
        private bool diagnosticFailure;
        internal Action<ulong,byte[]> ReadObserver=>readObserver;
        private RawCollector(long reserved,long charged)
        {allocated=reserved;visits=charged;readObserver=Read;}
        internal static RawCollector Create()
        {
            const long fixedAllocation=192,fixedVisits=192+1024+512+4096;
            AdmitFixed(fixedAllocation,fixedVisits);
            // Admission precedes collector AND its one bound delegate allocation.
            return new RawCollector(fixedAllocation,fixedVisits);
        }
        private static void AdmitFixed(long allocation,long work)
        {
            if(allocation<153||allocation>PayloadLimit||work<allocation||work>VisitLimit)
                throw new InvalidOperationException("Diagnostic fixed reserve exceeds policy.");
        }
        internal void Bind(LegacyFreshMainConfiguredWordAugmentation actual)
        {
            if(owner is not null||!ReferenceEquals(actual.raw,this))throw new InvalidOperationException("Exact private diagnostic issuer required.");
            owner=actual;thread=Environment.CurrentManagedThreadId;
        }
        private void Require(bool live=true)
        {
            if(owner is null||!ReferenceEquals(owner.raw,this)||Environment.CurrentManagedThreadId!=thread||
                (live&&(phase!=1||owner.started==0||owner.complete||owner.faulted)))
                throw new InvalidOperationException("Exact live private diagnostic owner/thread required.");
        }
        internal void RequireDebitExport(LegacyFreshMainConfiguredWordAugmentation actual)
        {
            Require(false);
            if(!ReferenceEquals(actual,owner)||phase!=2)
                throw new InvalidOperationException("Only the active same-thread terminal RAW export may read its debit.");
        }
        private void Charge(long amount)
        {
            if(amount<0||amount>VisitLimit-visits)
            {diagnosticFailure=true;throw new InvalidOperationException("DIAGNOSTIC configured RAW visit envelope exceeded.");}
            visits+=amount;
        }
        private void Fail()
        {diagnosticFailure=true;throw new InvalidOperationException("DIAGNOSTIC configured RAW storage envelope exceeded.");}
        private void Ensure(int length)
        {
            Require();
            if(length<0||length>PayloadLimit-used||count==EventLimit)Fail();
            int bytes=checked(used+length);
            if(bytes>(payload?.Length??0))
            {
                int capacity=Math.Min(PayloadLimit,Math.Max(bytes,Math.Max(4096,(payload?.Length??0)*2)));
                // Prepay allocation zeroing AND eventual full clear, plus growth copy.
                Charge(checked(2L*capacity+used));
                allocated+=capacity;byte[] next=new byte[capacity];clearDebt+=capacity;
                if(payload is not null)
                {payload.AsSpan(0,used).CopyTo(next);copied+=used;Array.Clear(payload);clearDebt-=payload.Length;}
                payload=next;
            }
            if(count==(headers?.Length??0))
            {
                int capacity=Math.Min(EventLimit,Math.Max(64,(headers?.Length??0)*2));
                Charge(checked(32L*capacity+16L*count));
                allocated+=16L*capacity;var next=new RawHeader[capacity];clearDebt+=16L*capacity;
                if(headers is not null)
                {headers.AsSpan(0,count).CopyTo(next);copied+=16L*count;Array.Clear(headers);clearDebt-=16L*headers.Length;}
                headers=next;
            }
            Charge(checked(4L*length+144L)); // stores plus prepaid terminal/base64 traversal; no fallible export debit
        }
        private Span<byte> Append(int kind,int argument,int length)
        {
            Ensure(length);int at=used;headers![count++]=new(kind,argument,at,length);used+=length;
            copied+=length;return payload!.AsSpan(at,length);
        }
        internal void Bytes(int kind,ReadOnlySpan<byte> bytes)
            =>bytes.CopyTo(Append(kind,candidate,bytes.Length));
        internal void Integers(int kind,ReadOnlySpan<int> values)
        {
            var output=Append(kind,candidate,checked(values.Length*4));
            for(int i=0;i<values.Length;i++)BinaryPrimitives.WriteInt32LittleEndian(output[(i*4)..],values[i]);
        }
        internal void Begin(LegacyNativePlainWordSpanSnapshot initial,CancellationToken token)
        {
            Require(false);if(phase!=0||owner!.started==0)throw new InvalidOperationException("Diagnostic cannot enter twice.");
            phase=1;Bytes(16,owner.compact);Integers(17,owner.starts);Snapshot(18,initial,token);
        }
        internal void Candidate(int ordinal)
        {Require();candidate=ordinal;Append(1,ordinal,0);}
        internal void Word(int kind,LegacyNativeWordBuilder word,CancellationToken token)
        {
            Require();Charge(240);allocated+=120;copied+=120; // actual CopyStorage120 plus temporary clear120
            byte[] bytes=word.CopyStorage(token);try{Bytes(kind,bytes);}finally{Array.Clear(bytes);}
        }
        internal void Snapshot(int kind,LegacyNativePlainWordSpanSnapshot snapshot,CancellationToken token)
        {
            Require();if(snapshot.SelectedOrdinals.Count!=2)throw new InvalidDataException("Actual two ALT selection required.");
            var head=Append(kind,snapshot.Words.Count,16);
            BinaryPrimitives.WriteInt32LittleEndian(head,snapshot.Span.First);
            BinaryPrimitives.WriteInt32LittleEndian(head[4..],snapshot.Span.Last);
            BinaryPrimitives.WriteInt32LittleEndian(head[8..],snapshot.SelectedOrdinals[0]);
            BinaryPrimitives.WriteInt32LittleEndian(head[12..],snapshot.SelectedOrdinals[1]);
            for(int i=0;i<snapshot.Words.Count;i++)
            {
                var word=snapshot.Words[i];
                token.ThrowIfCancellationRequested();Charge(240);allocated+=120;copied+=120;
                byte[] bytes=word.CopyStorage();try{Bytes(19,bytes);}finally{Array.Clear(bytes);}
            }
        }
        internal void FinalHeads(CancellationToken token)
        {
            Require();var actual=owner!;var data=Append(15,2,16);
            BinaryPrimitives.WriteInt32LittleEndian(data,actual.heads[0].Span.First);
            BinaryPrimitives.WriteInt32LittleEndian(data[4..],actual.heads[0].Span.Last);
            BinaryPrimitives.WriteInt32LittleEndian(data[8..],actual.heads[0].WordIndex);
            BinaryPrimitives.WriteInt32LittleEndian(data[12..],actual.heads[1].WordIndex);
            for(int i=0;i<2;i++)
            {
                token.ThrowIfCancellationRequested();Charge(240);allocated+=120;copied+=120;
                byte[] bytes=actual.heads[i].Word.CopyStorage();
                try{Bytes(19,bytes);}finally{Array.Clear(bytes);}
            }
        }
        internal void EqualityBefore(ReadOnlySpan<byte> left,ReadOnlySpan<byte> right,
            ReadOnlySpan<byte> a,ReadOnlySpan<byte> b,ushort ignored)
        {
            Require();comparison=checked(comparison+1);var data=Append(5,comparison,250);
            left.CopyTo(data);right.CopyTo(data[120..]);a.CopyTo(data[240..]);b.CopyTo(data[244..]);
            BinaryPrimitives.WriteUInt16LittleEndian(data[248..],ignored);
        }
        private void Read(ulong reference,byte[] row)
        {
            // Same array returned by actual equality Read. It is never modified/retained.
            if(row.Length!=40)throw new InvalidDataException("Actual glyph40 read required.");
            var data=Append(6,comparison,48);BinaryPrimitives.WriteUInt64LittleEndian(data,reference);row.CopyTo(data[8..]);
        }
        internal void Control(string text,ushort label)
        {
            var data=Append(7,comparison,checked(2+text.Length*2));
            BinaryPrimitives.WriteUInt16LittleEndian(data,label);
            for(int i=0;i<text.Length;i++)BinaryPrimitives.WriteUInt16LittleEndian(data[(2+i*2)..],text[i]);
        }
        internal void EqualityAfter(bool equal,ReadOnlySpan<byte> a,ReadOnlySpan<byte> b)
        {
            var data=Append(8,comparison,9);data[0]=equal?(byte)1:(byte)0;a.CopyTo(data[1..]);b.CopyTo(data[5..]);
        }
        internal void Outcome(int match,int appended,int duplicates,int updated)
        {
            var data=Append(9,candidate,16);BinaryPrimitives.WriteInt32LittleEndian(data,match);
            BinaryPrimitives.WriteInt32LittleEndian(data[4..],appended);BinaryPrimitives.WriteInt32LittleEndian(data[8..],duplicates);
            BinaryPrimitives.WriteInt32LittleEndian(data[12..],updated);
        }
        internal void Words(int kind,byte[][] words)
        {foreach(var word in words)Bytes(kind,word);}
        internal void Ranking(LegacyPreparedMainWordRanking.Result ranked)
        {
            Words(20,ranked.Words);Words(21,ranked.RankingByOriginalIndex);
            Integers(22,ranked.OldToNew);Integers(23,ranked.Released);
            Integers(24,ranked.OutputSourceOrdinals);Integers(25,ranked.ReleasedSourceOrdinals);
            Append(11,ranked.QuotaAfter,0);
        }
        internal void WriteTerminal(LegacyFreshMainConfiguredWordAugmentation actual,Utf8JsonWriter writer,CancellationToken token)
        {
            Require(false);
            if(!ReferenceEquals(actual,owner)||(phase!=0&&phase!=1)||(!actual.complete&&!actual.faulted))
                throw new InvalidOperationException("Unexported terminal diagnostic required.");
            // All terminal traversal is prepaid at append (3*payload+128/header)
            // and constructor (1024 fixed +512 diagnostic fault +4096 debit metadata reserve).
            // Writer-owned external buffers/IO are explicitly outside retained payload.
            try
            {
                phase=2;
                writer.WriteStartObject();writer.WriteNumber("schema",1);
                writer.WriteBoolean("complete",actual.complete);writer.WriteBoolean("faulted",actual.faulted);
                writer.WriteBoolean("marked",actual.SourcesMarkedComplete);writer.WriteBoolean("diagnosticFailure",diagnosticFailure);
                writer.WriteNumber("allocatedLogical",allocated);writer.WriteNumber("memoryVisits",visits);
                writer.WriteNumber("copyBytes",copied);writer.WriteNumber("pendingClearBytes",clearDebt);
                actual.debit.WriteRawTerminalDiagnostic(actual,writer,token);
                writer.WriteStartArray("events");
                for(int i=0;i<count;i++)
                {
                    token.ThrowIfCancellationRequested();var h=headers![i];writer.WriteStartObject();
                    writer.WriteNumber("kind",h.Kind);writer.WriteNumber("argument",h.Argument);
                    writer.WriteBase64String("raw",payload!.AsSpan(h.Start,h.Length));writer.WriteEndObject();
                }
                writer.WriteEndArray();writer.WriteEndObject();
            }
            finally
            {
                if(payload is not null)Array.Clear(payload);if(headers is not null)Array.Clear(headers);
                payload=null;headers=null;clearDebt=0;phase=3;
            }
        }
    }
}
