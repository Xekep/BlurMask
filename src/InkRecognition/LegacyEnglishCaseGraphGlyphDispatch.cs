using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishGlyphDispatch.Probe")]
namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    // Numeric cache only. The private live caller supplies authentic fresh pool
    // identity; this class never issues a source/graph/WORD capability.
    internal sealed class GraphGlyphRefreshDispatch
    {
        internal readonly record struct Receipt(LegacyOrdinaryGraphSourceReference[] Fill,byte[] Source,uint Bits,int Visited);
        private readonly LegacyNativeGlyphRecordPool pool;
        private readonly int capacity;
        private readonly Dictionary<ulong,Receipt> ready;
        internal GraphGlyphRefreshDispatch(LegacyNativeGlyphRecordPool pool,int capacity,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(pool);
            if(capacity is<1 or>128)throw new NotSupportedException("Bounded owning reference count required.");
            this.pool=pool;this.capacity=capacity;ready=new(capacity);
        }
        private void Require(LegacyNativeGlyphRecordPool actual,ulong key,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(pool,actual)||key!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(key))
                throw new InvalidOperationException("Same live pool and canonical owner key required.");
        }
        internal bool TryRead(LegacyNativeGlyphRecordPool actual,ulong key,CancellationToken token,out Receipt receipt)
        {Require(actual,key,token);return ready.TryGetValue(key,out receipt);}
        internal void Remember(LegacyNativeGlyphRecordPool actual,ulong key,Receipt receipt,CancellationToken token)
        {
            Require(actual,key,token);
            if(receipt.Fill is null||receipt.Source is null||receipt.Fill.Length==0||receipt.Source.Length!=checked(receipt.Fill.Length*26)||receipt.Visited<0)
                throw new InvalidDataException("Successful actual callback receipt required.");
            if(ready.Count>=capacity||!ready.TryAdd(key,receipt))throw new InvalidOperationException("One successful refresh per canonical owner required.");
        }
    }
}
