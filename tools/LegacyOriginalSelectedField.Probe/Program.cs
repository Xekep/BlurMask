using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if(args.Length!=1)throw new ArgumentException("capture.json");
using var file=File.Create(args[0]);using var w=new Utf8JsonWriter(file);
int guards=0,streams=0,completeCount=0;w.WriteStartObject();w.WriteStartArray("cases");
for(int count=1;count<=32;count++)
{
    var raw=Enumerable.Range(0,count).Select(i=>new InkStroke(Enumerable.Range(0,1+(count+i)%8)
        .Select(j=>new InkPoint(i*4+j*.125f,(i&1)==0&&j==0?BitConverter.UInt32BitsToSingle(0x80000000):count-i*.5f+j)).ToArray())).ToArray();
    var doc=LegacyOwnedRawInkDocument.Create(raw,default);var view=LegacyFreshRawInkCurrentView.Prepare(doc);
    string[] ids=Enumerable.Range(0,1+count%16).Select(i=>i==0?new string('F',count):"field_"+i).ToArray();
    var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(view,ids);var source=original.Run();var originalCounters=source.CopyRegistryCounterStorage();
    foreach(var chosen in source.Inputs)
    {
        var worker=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,chosen);var initial=worker.State;var state=worker.Run();completeCount++;
        Write(state,view,ids,"actual",-1);
        if(!state.Succeeded||state.Registry.Count!=source.Inputs.Count||state.NextRegistryId!=source.NextRegistryId||
            state.SourceTag30!=chosen.RegistryId||worker.HasNativeReaderContext||worker.NativeHistoryEnabled||
            !ReferenceEquals(worker.Original,original)||!ReferenceEquals(worker.OriginalInput,chosen)||
            !ReferenceEquals(worker.Document,doc)||!ReferenceEquals(worker.CurrentView,view))throw new InvalidDataException("Actual selected/original owner lost.");
        if(!ReferenceEquals(original.State,source)||!originalCounters.AsSpan().SequenceEqual(original.State.CopyRegistryCounterStorage()))
            throw new InvalidDataException("Selected worker copy mutated its original registry.");
        if(initial.Attempts!=0||initial.Nodes.Count!=0||initial.Registry.Count!=0||initial.IsFaulted)throw new InvalidDataException("Initial snapshot changed.");
        foreach(var input in state.Registry)
        {
            if(!ReferenceEquals(input.Owner,worker)||!ReferenceEquals(input.RetainedSelection.Owner,worker)||
                !ReferenceEquals(input.RetainedSelection,state.Registry[0].RetainedSelection)||
                !ReferenceEquals(input.OriginalEntry,source.Inputs.Single(i=>i.RegistryId==input.RegistryId)))throw new InvalidDataException("All-registry original/child C22 identity lost.");
            var export=worker.ExportInputGeometry(input);
            if(!ReferenceEquals(export.Owner,view)||export.MatricesResolved!=count||export.NativeNonidentityTransformCount!=0)throw new InvalidDataException("Foreign current view.");
            var ranges=input.CopyRanges();ranges[0]=default;if(input.CopyRanges()[0].First.Value==0)throw new InvalidDataException("Escaped ranges.");
        }
        var byId=state.Nodes.ToDictionary(n=>n.Identity);var clonedField=byId[state.SelectedFieldIdentity];var clonedInput=byId[state.SelectedInputIdentity];
        if(clonedField.OriginalNodeIdentity!=chosen.FieldNodeIdentity||clonedInput.OriginalNodeIdentity!=chosen.InputNodeIdentity||
            clonedInput.ParentIdentity!=clonedField.Identity||ReferenceEquals(worker.NameOwner,original.NameOwner))throw new InvalidDataException("Actual selected BCD clone lost.");
        foreach(var n in state.Nodes)
        {
            if(!ReferenceEquals(n.NameOwner,worker.NameOwner)||n.CanonicalName is{} token&&!ReferenceEquals(token.Owner,worker.NameOwner))throw new InvalidDataException("Wrong selected name owner.");
            byte[] value=n.CopyValueBytes();if(value.Length!=0){value[0]^=255;if(value.AsSpan().SequenceEqual(n.CopyValueBytes()))throw new InvalidDataException("Escaped DOM value.");}
        }
        for(int i=0;i<count;i++)
        {
            var copy=worker.CopyRawPointStorage(i);if(!copy.AsSpan().SequenceEqual(view.Curves[i].CopyPointStorage()))throw new InvalidDataException("Raw bits lost.");
            copy[0]^=255;if(copy.AsSpan().SequenceEqual(worker.CopyRawPointStorage(i)))throw new InvalidDataException("Raw storage escaped.");
        }
        var storage=state.CopyRawStorage();storage[0][0]^=255;
        if(storage[0].AsSpan().SequenceEqual(state.CopyRawStorage()[0]))throw new InvalidDataException("Snapshot storage escaped.");
        Guard<InvalidOperationException>(()=>worker.Run());
        var foreign=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,chosen);foreign.Run();
        Guard<ArgumentException>(()=>worker.ExportInputGeometry(foreign.State.Registry[0]));
        Guard<ArgumentException>(()=>LegacyLibraryOriginalSelectedFieldContext.Prepare(original,LegacyLibraryOriginalTextFieldBootstrap.Prepare(view,["foreign"]).Run().Inputs[0]));
        Guard<ArgumentOutOfRangeException>(()=>worker.CopyRawPointStorage(count));
        using var cancelled=new CancellationTokenSource();cancelled.Cancel();
        var reusable=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,chosen);
        Guard<OperationCanceledException>(()=>reusable.Run(cancelled.Token));
        if(reusable.State.Attempts!=0||reusable.State.IsFaulted)throw new InvalidDataException("Preentry cancellation consumed clone.");
        reusable.Run();
        if(chosen.RegistryId==source.Inputs[^1].RegistryId&&count is 1 or 2 or 15 or 16 or 17)
        for(int at=0;at<state.Attempts;at++)
        {
            var failed=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,chosen);var prefix=failed.RunPrepared(at);
            Write(prefix,view,ids,"preparedFailure",at);
            if(prefix.Succeeded||!prefix.IsFaulted)throw new InvalidDataException("Unpoisoned refusal prefix.");
            Guard<InvalidOperationException>(()=>failed.Run());
            Guard<InvalidOperationException>(()=>failed.CopyRawPointStorage(0));
            if(prefix.Registry.Count>0)Guard<InvalidOperationException>(()=>failed.ExportInputGeometry(prefix.Registry[0]));
            var cancel=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,chosen);
            Guard<OperationCanceledException>(()=>cancel.RunPrepared(cancellation:at));Write(cancel.State,view,ids,"preparedCancel",at);
            Guard<InvalidOperationException>(()=>cancel.Run());
        }
    }
    var incomplete=LegacyLibraryOriginalTextFieldBootstrap.Prepare(view,["unfinished"]);
    //Incomplete has no actual chosen entry: a valid foreign chosen entry cannot authorize it.
    Guard<InvalidOperationException>(()=>LegacyLibraryOriginalSelectedFieldContext.Prepare(incomplete,source.Inputs[0]));
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteNumber("streams",streams);w.WriteNumber("completed",completeCount);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS32 raw originals/{completeCount} actual chosen-field clones/{streams} streams/{guards} guards; no native acceptance.");
void Guard<T>(Action action)where T:Exception{try{action();}catch(T){guards++;return;}throw new InvalidDataException("Missing "+typeof(T).Name);}
void Write(LegacyLibraryOriginalSelectedFieldState s,LegacyFreshRawInkCurrentView view,string[] ids,string role,int fault)
{
    streams++;w.WriteStartObject();w.WriteString("role",role);w.WriteNumber("fault",fault);w.WriteNumber("chosen",s.SourceTag30);
    w.WriteStartArray("ids");foreach(string id in ids)w.WriteStringValue(id);w.WriteEndArray();
    w.WriteStartArray("pointCounts");foreach(var c in view.Curves)w.WriteNumberValue(c.PointCount);w.WriteEndArray();
    w.WriteStartArray("originalRaw");foreach(var c in view.Curves)w.WriteBase64StringValue(c.CopyPointStorage());w.WriteEndArray();
    w.WriteStartArray("selectedRaw");foreach(var b in s.CopyRawStorage())w.WriteBase64StringValue(b);w.WriteEndArray();
    w.WriteBoolean("succeeded",s.Succeeded);w.WriteBoolean("faulted",s.IsFaulted);w.WriteNumber("attempts",s.Attempts);
    w.WriteNumber("next",s.NextRegistryId);w.WriteNumber("maximum",s.MaximumRegistryId);w.WriteBase64String("c51",s.CopyRegistryCounterStorage());
    w.WriteNumber("root",s.RootIdentity);w.WriteNumber("selectedField",s.SelectedFieldIdentity);w.WriteNumber("selectedInput",s.SelectedInputIdentity);
    w.WriteStartArray("registry");foreach(var r in s.Registry){w.WriteStartObject();w.WriteNumber("id",r.RegistryId);w.WriteNumber("category",r.Category);w.WriteString("name",r.Name);w.WriteBase64String("ranges",RangeBytes(r.CopyRanges()));w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("nodes");foreach(var n in s.Nodes){w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("origin",n.OriginalNodeIdentity);w.WriteNumber("tag",n.NativeValueTag);w.WriteBase64String("value",n.CopyValueBytes());if(n.NamespaceUri is null)w.WriteNull("uri");else w.WriteString("uri",n.NamespaceUri);w.WriteNumber("parent",n.ParentIdentity);Ids("children",n.Children);Ids("attributes",n.Attributes);w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("trace");foreach(string t in s.Trace)w.WriteStringValue(t);w.WriteEndArray();w.WriteEndObject();
}
void Ids(string name,IReadOnlyList<int> ids){w.WriteStartArray(name);foreach(int i in ids)w.WriteNumberValue(i);w.WriteEndArray();}
byte[] RangeBytes(LegacyNativeInkPositionRange[] ranges){byte[] b=new byte[ranges.Length*16];for(int i=0;i<ranges.Length;i++){BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16),ranges[i].First.Value);BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16+8),ranges[i].Last.Value);}return b;}
