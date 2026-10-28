using InkRecognition;
using InkRecognition.Math;
sealed class Pipeline:IDisposable
{
 internal Math2Mode0OriginalMathFieldSource Field=null!;
 internal Math2Mode0RawOnlySource Source=null!;
 internal Math2Mode0SelectedMathFieldCopy Receipt=null!;
 internal Math2Mode0FreshMathPublisher Publisher=null!;
 internal Math2Mode0OutgoingTransfer Outgoing=null!;
 internal Math2Mode0OriginalApplication Application=null!;
 internal static Pipeline Create(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,int width=1,bool reversed=false,LegacyFreshRawInkCurrentView? sameView=null)
 {
  var p=new Pipeline();
  InkStroke[] ink=[new([new InkPoint(0,30),new(0,0),new(10,18),new(20,0),new(20,30)]),new([new InkPoint(35,0),new(35,30)])];
  if(reversed)Array.Reverse(ink);
  var features=Math2Mode0RawSourcePositions.PrepareFeatures(ink,default);
  var view=sameView??LegacyFreshRawInkCurrentView.Prepare(features.Positions.Document);
  p.Field=Math2Mode0OriginalMathFieldSource.Prepare(view,"formula");
  p.Source=width==1?Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,p.Field):Math2Mode0RawOnlySource.PrepareWithPreparedSearchAndSelectedMathField(runtime,bundle,run,p.Field,width);
  p.Source.RunToInitialOrder();p.Source.ApplyEf4();
  var output=p.Source.OrderFinal().FormatFullCold().AttachSourcePositions();
  p.Receipt=output.CopySelectedMathField(p.Field);p.Publisher=Math2Mode0FreshMathPublisher.Create(p.Receipt);
  p.Outgoing=Math2Mode0OutgoingTransfer.Create(p.Publisher);p.Application=Math2Mode0OriginalApplication.Prepare(p.Field,p.Outgoing);
  return p;
 }
 public void Dispose(){Application?.Dispose();Outgoing?.Dispose();Publisher?.Dispose();Receipt?.Dispose();Source?.Dispose();Field?.Dispose();}
}

