namespace InkRecognition.Math;

// Prepared operands only. A future closed original-mutation grant must supply
// actual leased objects; these values alone confer NO original apply authority.
internal sealed record Math2RepeatDomOperand(byte Operation,int[] Path,int Ordinal,Math2Mode0OriginalArena PayloadWriter,int PayloadRoot);
internal sealed class Math2RepeatDomPlan
{
    internal readonly Math2Mode0OriginalArena Arena;internal readonly Math2Mode0OriginalArena.Node Anchor;
    internal readonly Math2RepeatDomOperand[] Operations;internal readonly Math2Mode0OriginalArena.Node[] Parents;
    private int cursor;
    private Math2RepeatDomPlan(Math2Mode0OriginalArena arena,Math2Mode0OriginalArena.Node anchor,Math2RepeatDomOperand[] operations,Math2Mode0OriginalArena.Node[] parents)
    {Arena=arena;Anchor=anchor;Operations=operations;Parents=parents;}
    internal static Math2RepeatDomPlan Resolve(Math2Mode0OriginalArena arena,Math2Mode0OriginalArena.Node field,
        IReadOnlyList<Math2RepeatDomOperand> source,Math2DomDiffWork work)
    {
        work.Visit();if(field.Id<1||field.Id>arena.Nodes.Count||!ReferenceEquals(arena.Nodes[field.Id-1],field)||arena.WriterFreed(field)||field.Kind!=3||source.Count>65536)
            throw new InvalidOperationException("Bounded actual anchor/typed operands required.");
        work.Visit(checked(source.Count*8L));var operations=new Math2RepeatDomOperand[source.Count];var parents=new Math2Mode0OriginalArena.Node[source.Count];
        for(int i=0;i<source.Count;i++)
        {
            work.Visit();var op=source[i];if(op.Operation is not(2 or 4 or 5 or 6 or 7)||op.Path.Length>64||op.Ordinal<0)throw new NotSupportedException("Current serialized Math DOM profile required.");
            var at=field;
            foreach(int ordinal in op.Path)
            {
                work.Visit();if(ordinal<0)throw new NotSupportedException("Invalid native path index safely refused before effects.");var list=arena.Children(at.Id);work.Visit(list.Count);
                Math2Mode0OriginalArena.Node? found=null;int index=0;foreach(var n in list){if(n.Kind==0)continue;if(index++==ordinal){found=n;break;}}
                at=found??throw new NotSupportedException("Native missing path is unsafe; managed bounded profile refuses it before effects.");
            }
            if(at.Kind!=3||arena.WriterFreed(at))throw new NotSupportedException("Live ordinary composite parent required.");
            if(op.Operation!=4&&(op.PayloadRoot<1||op.PayloadRoot>op.PayloadWriter.Nodes.Count||op.PayloadWriter.WriterFreed(op.PayloadWriter.Nodes[op.PayloadRoot-1])))
                throw new NotSupportedException("Known nonnull owned payload required before original effects.");
            if(op.Operation!=4)
            {
                var root=op.PayloadWriter.Nodes[op.PayloadRoot-1];
                if(op.Operation==2)
                {
                    if(root.Kind is<1 or>3)throw new NotSupportedException("Ordinary child clone required before effects.");
                    ValidateClone(op.PayloadWriter,root,0,work);
                }
                else
                {
                    if(root.Kind!=0)throw new NotSupportedException("Only proved produced attribute envelopes are admitted; text setter remains unported.");
                    // EB9C0 copies non-heap scalar bytes unchanged; EB740 normalizes
                    // heap strings. From admits ONLY canonical produced heap>=16,
                    // inline1..15, empty3 and exact-width4/7/zero10, so the
                    // native heap normalization is identity in this profile.
                    if(op.Operation is 5 or 7)Math2DomDiffScalar.From(root.Tag,root.Text,root.Number).CopyWriterFields(work);
                }
            }
            operations[i]=op with{Path=op.Path.ToArray()};parents[i]=at;
        }
        // Native E2D70 resolves EVERY parent first. Later operations use these
        // retained nodes even if earlier insert/delete changes path positions.
        return new(arena,field,operations,parents);
    }
    private static void ValidateClone(Math2Mode0OriginalArena source,Math2Mode0OriginalArena.Node node,int depth,Math2DomDiffWork work)
    {
        work.Visit(128);if(depth>64||source.WriterFreed(node))throw new NotSupportedException("Bounded live clone payload required before original effects.");
        var children=source.Children(node.Id);work.Visit(children.Count);
        foreach(var child in children)
        {
            work.Visit();if(child.Parent!=node.Id||child.Id<1||child.Id>source.Nodes.Count||!ReferenceEquals(source.Nodes[child.Id-1],child))throw new InvalidOperationException("Actual payload parent relation required.");
            ValidateClone(source,child,depth+1,work);
        }
    }
    internal bool Completed=>cursor==Operations.Length;
    internal int Applied=>cursor;
    internal void ApplyNext(Math2DomDiffWork work,IReadOnlyDictionary<long,long>? tagMapping=null)
    {
        work.Visit();if(cursor>=Operations.Length)throw new InvalidOperationException("Whole traversal already consumed.");
        Arena.ApplyRepeatDomOperand(Parents[cursor],Operations[cursor],work,tagMapping);cursor++;
    }
}
internal sealed partial class Math2Mode0OriginalArena
{
    internal void ApplyRepeatDomOperand(Node parent,Math2RepeatDomOperand op,Math2DomDiffWork work,IReadOnlyDictionary<long,long>? tagMapping)
    {
        work.Visit();if(parent.Id<1||parent.Id>Nodes.Count||!ReferenceEquals(Nodes[parent.Id-1],parent)||WriterFreed(parent)||parent.Kind!=3)
            throw new InvalidOperationException("Same retained live pre-resolved parent required.");
        var list=WriterChildren(parent);work.Visit(list.Count);Node? child=null;int ordinal=0;
        foreach(var n in list){if(n.Kind==0)continue;if(ordinal++==op.Ordinal){child=n;break;}}
        switch(op.Operation)
        {
            case 2:
            {
                if(tagMapping is not null)
                {
                    work.Visit(tagMapping.Count);
                    foreach(var pair in tagMapping){work.Visit();if(pair.Key!=pair.Value)throw new NotSupportedException("Nonidentity subtree tag remapping needs the native descendant visitor; no clone effects in this bounded profile.");}
                }
                var payload=op.PayloadWriter.Nodes[op.PayloadRoot-1];if(payload.Kind is<1 or>3)throw new NotSupportedException("Ordinary produced child payload required.");
                int id=CloneOriginal(op.PayloadWriter,payload.Id,0,_=>work.Visit(128));var copy=Nodes[id-1];WriterParent(parent,copy,false,work);
                work.Visit(checked(list.Count*2L+1));int index=child is null?list.Count:list.IndexOf(child);if(index<0)throw new InvalidOperationException();list.Insert(index,copy);copy.Parent=parent.Id;break;
            }
            case 4:
            {
                if(child is null)throw new NotSupportedException("Native null deletion is unsafe; current profile refuses without pretending success.");
                WriterDetach(child,work);ReleaseRepeatScratch(child,work);break;
            }
            case 5:case 7:
            {
                var payload=op.PayloadWriter.Nodes[op.PayloadRoot-1];if(payload.Kind!=0)throw new NotSupportedException("Actual scope has attribute scalar updates only; text EBAF0 unported.");
                var scalar=Math2DomDiffScalar.From(payload.Tag,payload.Text,payload.Number);var value=scalar.CopyWriterFields(work);long? number=value.Number;
                if(payload.Name.Text=="tag"&&tagMapping is not null)
                {if(payload.Tag!=4||number is not long actual||actual<0)throw new NotSupportedException("Strict produced typed registered tag required.");if(tagMapping.TryGetValue(actual,out long mapped)&&mapped!=-1)number=mapped;}
                var name=names.Intern(payload.Name.Text);work.Visit(list.Count);Node? found=null;foreach(var n in list)if(n.Kind==0&&ReferenceEquals(n.Name,name)){found=n;break;}
                if(found is not null)
                {
                    // ED830 -> EB9C0 updates the existing attribute. Structural
                    // D8 replacement is NOT equivalent to this serialized F8.
                    work.Visit(32);found.Tag=scalar.Tag;found.Text=value.Text;found.Number=number;
                }
                else
                {
                    work.Visit(128);var created=New(0,payload.Name.Text,0,payload.Id,scalar.Tag,value.Text,number,null,null);
                    work.Visit(32);created.Parent=parent.Id;list.Add(created);
                }
                break;
            }
            case 6:
            {
                var payload=op.PayloadWriter.Nodes[op.PayloadRoot-1];if(payload.Kind!=0)throw new NotSupportedException("Named attribute removal operand required.");
                var name=names.Intern(payload.Name.Text);work.Visit(list.Count);Node? found=null;foreach(var n in list)if(n.Kind==0&&ReferenceEquals(n.Name,name)){found=n;break;}
                if(found is not null)WriterDetach(found,work); // native missing attribute is successful noop; no free90.
                break;
            }
            default:throw new NotSupportedException();
        }
    }
}
