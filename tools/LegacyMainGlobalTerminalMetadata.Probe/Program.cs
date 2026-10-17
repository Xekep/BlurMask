using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Runtime.InteropServices;

if(args.Length!=9)throw new ArgumentException("AK engine separation case publication line30 line50 capture LK");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[8]),engine=File.ReadAllBytes(args[1]);
using var ed=JsonDocument.Parse(engine);var activation=ed.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);byte[] line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
int owners=0,guards=0,metadataRows=0;ulong identity=0x1000;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);var cache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table);var transitions=LegacyFreshMainGlobalWordTransitions.Prepare(ak,cache);
    var costs=LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table);var policy=LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,costs,transitions);
    var confidence=LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,owner);var terminal=LegacyFreshMainGlobalTerminalInvocation.Prepare(policy);

    LegacyFreshMainGlobalWordPassResult Run(LegacyFreshMainGlobalWordControllerPolicy p)
        => new LegacyFreshMainGlobalWordPass(ak, p, request =>
        {
            byte[] bytes = new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(bytes);
            return new(bytes, ++identity);
        }).RunOwned();
    var result = Run(policy);
    var selected = terminal.Select(result);
    var compact = LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak, selected, confidence,
        Enumerable.Repeat((byte)0xa7, 4096 * 40).ToArray());
    byte[] storage = compact.CopyCompactStorage();
    int evaluations = owner.AcousticEvaluations;
    var events = new List<(int Index, ulong Tag, ushort Symbol, byte[] Metadata)>();
    int nativeMetadataMarkers = 0;
    for (int index = 0; index < compact.RecordCount; index++)
    {
        var row = storage.AsSpan(index * 40, 40);
        if (BinaryPrimitives.ReadInt32LittleEndian(row[36..]) != -1) nativeMetadataMarkers++;
        ulong tag = BinaryPrimitives.ReadUInt64LittleEndian(row);
        //Prepared metadata consumer controls select actual compact terminal tags.
        //Current native marker36 is absent: these are NOT21FFD0 metadata events.
        if (row[4] != 1 || (uint)tag >= model.Lexicon.NodeCount ||
            !model.WordIndex.TryGetWordId((int)(uint)tag, out _)) continue;
        ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row[28..]);
        byte[] metadata = result.ReadResultTerminal(tag, symbol, 0, default).ToArray();
        if (metadata.Length != 16 || !metadata.AsSpan().SequenceEqual(result.ReadResultTerminal(tag, symbol, 0, default).Span))
            throw new Exception("Post-final V80 source70 changed repeated16-byte metadata.");
        metadata[0] ^= 1;
        var detached = result.ReadResultTerminal(tag, symbol, 0, default).ToArray();
        if (detached.AsSpan().SequenceEqual(metadata)) throw new Exception("Metadata storage escaped its owner.");
        events.Add((index, tag, symbol, detached)); metadataRows++;
    }
    if (events.Count == 0) throw new Exception("No actual compact metadata row exercised source70.");
    var first = events[0];
    Reject(() => result.ReadResultTerminal(first.Tag, first.Symbol, 0, new(true)));
    Reject(() => result.ReadResultTerminal(first.Tag & ~(255UL << 32), first.Symbol, 0, default));
    Reject(() => result.ReadResultTerminal((first.Tag & ~(255UL << 32)) | (2UL << 32), first.Symbol, 0, default));
    if (!result.ReadResultTerminal(first.Tag, first.Symbol, 0, default).Span.SequenceEqual(first.Metadata))
        throw new Exception("Preflight source/cancel failure invalidated the retained result lease.");
    result.ValidateResultQueryLease(default);
    if (owner.AcousticEvaluations != evaluations) throw new Exception("Source70 recomputed acoustic networks.");

    //A second genuine source/pass is used for destructive lease controls only.
    //It is never substituted for result's actual owner or compact pool.
    var checkOwner = LegacyFreshMainGlobalCaseSession.Prepare(ak, lk, model, geometry, cd, pd);
    var checkCache = LegacyFreshMainGlobalTransitionCache.Prepare(ak, checkOwner, table);
    var checkTransitions = LegacyFreshMainGlobalWordTransitions.Prepare(ak, checkCache);
    var checkCosts = LegacyFreshMainGlobalGroupCosts.Prepare(ak, checkOwner, table);
    var checkPolicy = LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak, checkOwner, checkCosts, checkTransitions);
    Reject(() => checkOwner.CaptureResultQueryLease(default));
    var checkResult = Run(checkPolicy);
    var lease = checkOwner.CaptureResultQueryLease(default);
    Reject(() => checkResult.ReadResultTerminal(first.Tag, first.Symbol, 0, default));
    Reject(() => owner.ReadResultTerminal(lease, first.Tag, first.Symbol, 0, default));
    Reject(() => checkOwner.ReadResultTerminal(null!, first.Tag, first.Symbol, 0, default));
    Reject(() => checkOwner.ReadResultTerminal(lease, first.Tag, first.Symbol, 0, new(true)));
    var expected = lease.DictionaryLease.Owner.ReadTerminal(first.Tag, first.Symbol, 0);
    var actual = checkOwner.ReadResultTerminal(lease, first.Tag, first.Symbol, 0, default);
    if (!actual.Span.SequenceEqual(expected.Span) || !actual.Span.SequenceEqual(first.Metadata))
        throw new Exception("Result entry differs from the same backend's original source70 action.");
    //Metadata must not advance queryRevision: capture remains legal until an
    //actual result V40 query changes the reset revision.
    var replacementLease = checkOwner.CaptureResultQueryLease(default);
    Reject(() => checkOwner.ReadResultTerminal(lease, first.Tag, first.Symbol, 0, default));
    _ = checkOwner.ReadResultTerminal(replacementLease, first.Tag, first.Symbol, 0, default);
    var request = new LegacyPreparedDictionaryQuery
    {
        Capacity = 7, ContextRoot = checkOwner.Contexts.ContainerIdentity,
        Parameter1c = (uint)(checkResult.NodeCount - 1), Parameter20 = 0, Flags24 = 0x100
    };
    _ = checkOwner.ReadResultQuery(replacementLease, 8UL << 40, request, false, default);
    _ = checkOwner.ReadResultTerminal(replacementLease, first.Tag, first.Symbol, 0, default);
    Reject(() => checkOwner.CaptureResultQueryLease(default));
    //This deliberately external same-backend query must revoke the lease.
    lease.DictionaryLease.Owner.Read(8UL << 40, new()
    {
        Capacity = 7, ContextRoot = checkOwner.Contexts.ContainerIdentity,
        Parameter1c = (uint)(checkResult.NodeCount - 1), Parameter20 = 0, Flags24 = 0x100
    });
    Reject(() => checkOwner.ReadResultTerminal(replacementLease, first.Tag, first.Symbol, 0, default));
    Reject(() => checkOwner.ReadResultTerminal(replacementLease, first.Tag, first.Symbol, 0, default));

    w.WriteStartObject(); w.WriteNumber("owner", owners); w.WriteNumber("nodes", result.NodeCount);
    w.WriteNumber("nativeMetadataMarkers36", nativeMetadataMarkers);
    w.WriteNumber("compactCount", compact.RecordCount); w.WriteNumber("selected", compact.SelectedRecordIndex);
    w.WriteNumber("acousticEvaluations", evaluations); w.WriteBase64String("compact", storage);
    w.WriteStartArray("metadata");
    foreach (var item in events)
    {
        w.WriteStartObject(); w.WriteNumber("index", item.Index); w.WriteNumber("tag", item.Tag);
        w.WriteNumber("symbol", item.Symbol); w.WriteString("token", Convert.ToHexString(item.Metadata)); w.WriteEndObject();
    }
    w.WriteEndArray(); w.WriteNumber("guards", guards); w.WriteEndObject();
    Console.WriteLine($"PASS global owner{owners}: {result.NodeCount} nodes/{compact.RecordCount} compact rows/{events.Count} actual terminal-tag prepared source70 controls/native markers{nativeMetadataMarkers}; no new D0/reset/NN.");
    owners++;
}
w.WriteEndArray(); w.Flush();
Console.WriteLine($"PASS {owners} actual global owners/{metadataRows} prepared metadata16 rows/{guards} guards; not inferred21FFD0 events.");

void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Invalid global result metadata lease accepted.");
}
