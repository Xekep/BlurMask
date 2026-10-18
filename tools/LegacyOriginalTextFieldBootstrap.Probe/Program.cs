using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if(args.Length!=1)throw new ArgumentException("capture.json");
using var file=File.Create(args[0]);using var w=new Utf8JsonWriter(file);int guards=0,streams=0;
w.WriteStartObject();w.WriteStartArray("cases");
for(int count=1;count<=64;count++)
{
    var raw=Enumerable.Range(0,count).Select(i=>new InkStroke(Enumerable.Range(0,1+(count+i)%8)
        .Select(j=>new InkPoint(i*4+j*.125f,(i&1)==0&&j==0?BitConverter.UInt32BitsToSingle(0x80000000):count-i*.5f+j)).ToArray())).ToArray();
    var doc=LegacyOwnedRawInkDocument.Create(raw,default);var current=LegacyFreshRawInkCurrentView.Prepare(doc);
    string[] ids=Enumerable.Range(0,1+count%16).Select(i=>i==0?new string('F',count):"field_"+i).ToArray();
    var bootstrap=LegacyLibraryOriginalTextFieldBootstrap.Prepare(current,ids);var before=bootstrap.State;var complete=bootstrap.Run();
    Write(complete,current,ids,"actual",-1);
    var tokens=new Dictionary<string,LegacyLibraryOriginalDomName>(StringComparer.Ordinal);
    foreach(var node in complete.Nodes)
    {
        if(!ReferenceEquals(node.NameOwner,bootstrap.NameOwner))throw new InvalidDataException("Original DOM name owner detached.");
        if(node.CanonicalName is{} token){if(!ReferenceEquals(token.Owner,bootstrap.NameOwner)||tokens.TryGetValue(token.Text,out var old)&&!ReferenceEquals(old,token))throw new InvalidDataException("Noncanonical same-owner token.");tokens[token.Text]=token;}
        if(node.Kind==2&&(!ReferenceEquals(node.NamespaceUriName?.Owner,bootstrap.NameOwner)||node.NamespacePrefixName?.Text!=""))throw new InvalidDataException("Native namespace domain lost.");
    }
    if(before.Succeeded||before.Nodes.Count!=0||before.Inputs.Count!=0)throw new InvalidDataException("Initial snapshot mutated.");
    foreach(var input in complete.Inputs)
    {
        if(!ReferenceEquals(input.RetainedSelection,complete.Inputs[0].RetainedSelection)||!ReferenceEquals(input.RetainedSelection.Owner,bootstrap))
            throw new InvalidDataException("Equal ranges lost retained C22 identity.");
        var geometry=bootstrap.ExportInputGeometry(input);
        if(!ReferenceEquals(geometry.Owner,current)||geometry.MatricesResolved!=count||geometry.NativeNonidentityTransformCount!=0)
            throw new InvalidDataException("Original C22/current-view owner lost.");
        var copy=input.CopyRanges();copy[0]=default;
        if(input.CopyRanges()[0].First.Value==0)throw new InvalidDataException("C22 range escaped.");
    }
    foreach(var record in complete.History.Where(h=>h.Source=="C4D"))
        if(record.CopiedSelection is not{} copied||record.Selection is not{} registered||
            !ReferenceEquals(copied.Owner,bootstrap)||ReferenceEquals(copied,registered.RetainedSelection)||
            !copied.CopyRanges().AsSpan().SequenceEqual(registered.CopyRanges()))
            throw new InvalidDataException("Native history clone was confused with interned C22 identity.");
    Guard<InvalidOperationException>(()=>bootstrap.Run());
    var foreign=LegacyLibraryOriginalTextFieldBootstrap.Prepare(current,["foreign"]);foreign.Run();
    Guard<ArgumentException>(()=>bootstrap.ExportInputGeometry(foreign.State.Inputs[0]));
    Guard<NotSupportedException>(()=>LegacyLibraryOriginalTextFieldBootstrap.Prepare(current,["field/path"]));
    Guard<NotSupportedException>(()=>LegacyLibraryOriginalTextFieldBootstrap.Prepare(current,["x","x"]));
    Guard<NotSupportedException>(()=>LegacyLibraryOriginalTextFieldBootstrap.Prepare(current,["x\0tail"]));
    var reusable=LegacyLibraryOriginalTextFieldBootstrap.Prepare(current,["retry"]);
    using var cancelled=new CancellationTokenSource();cancelled.Cancel();
    Guard<OperationCanceledException>(()=>reusable.Run(cancelled.Token));
    if(reusable.State.Attempts!=0||reusable.State.IsFaulted)throw new InvalidDataException("Preentry cancellation consumed bootstrap.");
    reusable.Run();
    //All actual mutation service boundaries, not fabricated field labels.
    if(count<=4||count is 16 or 17)
    for(int at=0;at<complete.Attempts;at++)
    {
        var failed=LegacyLibraryOriginalTextFieldBootstrap.Prepare(current,ids);var prefix=failed.RunPrepared(at);
        if(prefix.Succeeded||!prefix.IsFaulted)throw new InvalidDataException("Failure prefix not poisoned.");
        Write(prefix,current,ids,"preparedFailure",at);Guard<InvalidOperationException>(()=>failed.Run());
        if(prefix.Inputs.Count>0)Guard<InvalidOperationException>(()=>failed.ExportInputGeometry(prefix.Inputs[0]));
        var cancel=LegacyLibraryOriginalTextFieldBootstrap.Prepare(current,ids);
        Guard<OperationCanceledException>(()=>cancel.RunPrepared(cancellationAttempt:at));
        Write(cancel.State,current,ids,"preparedCancel",at);Guard<InvalidOperationException>(()=>cancel.Run());
    }
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteNumber("streams",streams);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS64 genuine raw owners/{streams} ordered bootstrap streams/{guards} guards; no SDK acceptance.");
void Guard<T>(Action action)where T:Exception{try{action();}catch(T){guards++;return;}throw new InvalidDataException("Missing "+typeof(T).Name);}
void Write(LegacyLibraryOriginalTextFieldBootstrapState s,LegacyFreshRawInkCurrentView current,string[] ids,string role,int fault)
{
    streams++;if(!ReferenceEquals(s.Owner.CurrentView,current)||!ReferenceEquals(s.Owner.Document,current.Document))throw new InvalidDataException("Source detached.");
    w.WriteStartObject();w.WriteString("role",role);w.WriteNumber("fault",fault);w.WriteStartArray("ids");foreach(string id in ids)w.WriteStringValue(id);w.WriteEndArray();
    w.WriteStartArray("pointCounts");foreach(var c in current.Curves)w.WriteNumberValue(c.PointCount);w.WriteEndArray();
    w.WriteBoolean("succeeded",s.Succeeded);w.WriteBoolean("faulted",s.IsFaulted);w.WriteNumber("attempts",s.Attempts);w.WriteNumber("next",s.NextRegistryId);w.WriteNumber("maximum",s.MaximumRegistryId);w.WriteBase64String("c51",s.CopyRegistryCounterStorage());w.WriteNumber("root",s.RootIdentity);
    w.WriteStartArray("nodes");foreach(var n in s.Nodes){w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("tag",n.NativeValueTag);w.WriteBase64String("value",n.CopyValueBytes());if(n.NamespaceUri is null)w.WriteNull("uri");else w.WriteString("uri",n.NamespaceUri);w.WriteNumber("parent",n.ParentIdentity);WriteIds("children",n.Children);WriteIds("attributes",n.Attributes);w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("inputs");foreach(var input in s.Inputs){w.WriteStartObject();w.WriteString("field",input.FieldId);w.WriteNumber("fieldNode",input.FieldNodeIdentity);w.WriteNumber("inputNode",input.InputNodeIdentity);w.WriteNumber("id",input.RegistryId);w.WriteNumber("category",input.Category);w.WriteString("name",input.Name);byte[] b=new byte[input.CopyRanges().Length*16];int p=0;foreach(var r in input.CopyRanges()){BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(p),r.First.Value);BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(p+8),r.Last.Value);p+=16;}w.WriteBase64String("ranges",b);w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("history");foreach(var h in s.History){if(!ReferenceEquals(h.Owner,s.Owner)||h.Selection is{} sel&&!ReferenceEquals(sel.Owner,s.Owner))throw new InvalidDataException("History owner detached.");w.WriteStartObject();w.WriteString("source",h.Source);w.WriteNumber("kind",h.NativeKind);w.WriteNumber("parent",h.ParentIdentity);w.WriteNumber("node",h.NodeIdentity);w.WriteNumber("previous",h.PreviousNodeIdentity);w.WriteNumber("next",h.NextNodeIdentity);w.WriteNumber("registry",h.RegistryId);w.WriteNumber("category",h.Category);w.WriteBase64String("copiedRanges",RangeBytes(h.CopiedSelection?.CopyRanges()??[]));w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("sourceEvents");foreach(var e in s.SourceRegistryEvents){if(!ReferenceEquals(e.Selection.Owner,s.Owner))throw new InvalidDataException("Source event lost original C22.");w.WriteStartArray();w.WriteNumberValue(e.Selection.RegistryId);w.WriteNumberValue(e.NativeState);w.WriteNumberValue(e.Selection.Category);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("observer");foreach(var o in s.ObserverStates){w.WriteStartArray();w.WriteStringValue(o.Point);w.WriteNumberValue(o.TransactionDepth);w.WriteNumberValue(o.Pending130);w.WriteNumberValue(o.RetainedF0);w.WriteNumberValue(o.PendingFieldIdentity);w.WriteNumberValue(o.RetainedFieldIdentity);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("trace");foreach(string t in s.Trace)w.WriteStringValue(t);w.WriteEndArray();w.WriteEndObject();
}
void WriteIds(string name,IReadOnlyList<int> ids){w.WriteStartArray(name);foreach(int i in ids)w.WriteNumberValue(i);w.WriteEndArray();}
byte[] RangeBytes(LegacyNativeInkPositionRange[] ranges){byte[] b=new byte[ranges.Length*16];for(int i=0;i<ranges.Length;i++){BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16),ranges[i].First.Value);BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16+8),ranges[i].Last.Value);}return b;}
