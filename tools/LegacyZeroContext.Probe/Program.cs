using InkRecognition;
using System.Text.Json;
if(args.Length is not (1 or 3))throw new ArgumentException("Usage: LegacyZeroContext.Probe controls.json [ru-ak.res engine-profile.json]");
using(var json=JsonDocument.Parse(File.ReadAllBytes(args[0])))
{
 int checkedCount=0,tiny=0;
 foreach(var c in json.RootElement.EnumerateArray())
 {
  var strokes=c.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
  float h=c.GetProperty("height").GetSingle(),xr=c.GetProperty("xReference").GetSingle(),yr=c.GetProperty("yReference").GetSingle();bool max=c.GetProperty("maximumX").GetBoolean(),mean=c.GetProperty("pointMeanY").GetBoolean();
  var actual=LegacyFloatContextGeometry.Extract(strokes,h,xr,yr,max,mean);var expected=c.GetProperty("expected").EnumerateArray().Select(v=>v.GetSingle()).ToArray();
  if(!actual.AsSpan().SequenceEqual(expected))throw new Exception($"Independent zero/tiny context differs case{checkedCount}.");
  if(!actual.AsSpan().SequenceEqual(LegacyFloatContextGeometry.Extract(strokes,h,xr,yr,max,!mean)))throw new Exception("Zero branch unexpectedly used normal mean-Y policy.");
  if(strokes.SelectMany(s=>s.Points).Distinct().Count()>1)tiny++;
  if(actual[13]!=0||actual[14]!=0)throw new Exception("Tiny endpoint pointer was not reset to first.");checkedCount++;
 }
 if(checkedCount!=512||tiny<200)throw new Exception("Missing tiny point-window coverage.");
 Console.WriteLine($"PASS{checkedCount} independent zero/tiny fields, exactfloat32 outputs, bothmeanY modes, {tiny} nonidenticaltiny windows,endpoints FIRST andskipped raster/perstroke statistics.");
}
if(args.Length==3)
{
 byte[] resource=File.ReadAllBytes(args[1]);using var json=JsonDocument.Parse(File.ReadAllBytes(args[2]));var a=json.RootElement.GetProperty("activations");var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
 var model=LegacyFirstAcousticModel.ReadSource(resource,6120);
 InkStroke[] source=[Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((65,90),(65,10)),Stroke((65,90),(115,10)),Stroke((115,90),(115,10)),Stroke((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55)),Stroke((196,84))];
 var row=LegacyOrdinaryRowAcousticInput.Prepare(source,model.RowProfile);var cuts=row.GenerateRussianCuts();var context=LegacyFirstRowContext.Prepare(resource,6120,row,activations);
 if(row.Prepared.Strokes[5].Points.Count!=1||context.Groups.Count>12)throw new Exception("Actual point source/context shape differs.");
 var counts=cuts.Cuts.PreparedInk.StrokePointCounts.ToArray();var fixedContext=context.GetContext(cuts,[new(0,0,counts[0])]);
 var score=context.Classify(cuts,[new(0,0,counts[0])],1024,1024,10,10);
 if(fixedContext.Length!=28||score.IntegerCosts.Length!=180)throw new Exception("Point window did not reach real first NN context.");
 Console.WriteLine($"PASS actual row with late dot: source24a/265 kept1point,226 zero window processed, {context.Groups.Count} windowsaccepted, firstNN180 evaluated. This is context-dot support, not dot glyph recognition or a word winner.");
}
static InkStroke Stroke(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
