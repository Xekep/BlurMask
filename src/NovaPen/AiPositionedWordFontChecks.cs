using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using InkRecognition;
using System.Globalization;
namespace NovaPen;

//ROOT-ONLY optional font/render diagnostic; author compiles but does not run.
//No App/desktop lifetime/Window/SettingsStore/recognizer/model access.
internal static class AiPositionedWordFontChecks
{
    internal static void Run(string output)
    {
        if(File.Exists(output)||!Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(output))))throw new ArgumentException("Fresh explicit output PNG required.");
        AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
        int checks=0;
        foreach(double bad in new[]{0d,-1,double.NaN,double.PositiveInfinity,double.NegativeInfinity})
        {Need(AiTypesetLayout.PositionedDesiredFontSize(20,bad) is null,"invalid reference height refusal");
         Need(AiTypesetLayout.PositionedDesiredFontSize(bad,20) is null,"invalid source height refusal");}
        var line=new LegacyCompletedGlobalTextWords.PreparedLine(0,0,70,.12,24,-1,70);
        Rect[] source=[new(20,30,50,40),new(150,35,100,40)];string[] text=["\u0434\u0430","\u043D\u0435\u0442"];
        var marks=new InkMark[2];
        for(int i=0;i<2;i++)
        {
            double baseline=line.OriginY+source[i].Left*Math.Tan(line.Angle);
            var transform=Matrix.CreateTranslation(-source[i].Left,-baseline)*Matrix.CreateRotation(line.Angle)*Matrix.CreateTranslation(source[i].Left,baseline);
            var mark=new InkMark(InkTool.Text,Colors.Black,1,source[i].TopLeft){Text=text[i],Font="Inter",Transform=transform};mark.AddPoint(source[i].BottomRight);
            Need(AiTypesetLayout.FitPositioned(mark,source[i],line),"actual bundled-font fit");
            var glyph=Measure(text[i],mark.FontSize);var x=Measure("x",64);
            double desired=64*line.XHeight/x.Bounds.Height,scale=desired/mark.FontSize;
            Need(Math.Abs(glyph.Bounds.Height*scale-(mark.Points[1].Y-mark.Points[0].Y))<1e-6,"actual glyph geometry height");
            var localBaseline=new Point(mark.Points[0].X,mark.Points[0].Y+(glyph.Baseline-glyph.Bounds.Top)*scale);
            var actual=localBaseline.Transform(mark.Transform);
            Need(Math.Abs(actual.X-source[i].Left)<1e-6&&Math.Abs(actual.Y-baseline)<1e-6,"assigned line shared baseline");
            Need(mark.Bounds==new Rect(mark.Points[0],mark.Points[1]).Normalize().TransformToAABB(mark.Transform),"rotated output bounds");
            marks[i]=mark;
        }
        Need(marks[0].FontSize==marks[1].FontSize,"same prepared line gives same font scale");
        var view=new Words(marks);view.Measure(new Size(512,256));view.Arrange(new Rect(0,0,512,256));
        using(var bitmap=new RenderTargetBitmap(new PixelSize(512,256),new Vector(96,96)))
        {bitmap.Render(view);bitmap.Save(output,PngBitmapEncoderOptions.Default);}
        Console.WriteLine($"PASS {checks} ROOT-only actual font/geometry controls; offscreenPNG={output}; no NN/profile/desktop window");
        void Need(bool ok,string reason){if(!ok)throw new InvalidOperationException(reason);checks++;}
        (Rect Bounds,double Baseline) Measure(string value,double size)
        {
            var formatted=new FormattedText(value,CultureInfo.CurrentCulture,FlowDirection.LeftToRight,InkFonts.Face("Inter"),size,Brushes.Black);
            var geometry=formatted.BuildGeometry(default)??throw new InvalidOperationException("Bundled font geometry missing.");
            return(geometry.Bounds,formatted.Baseline);
        }
    }
    private sealed class Words(InkMark[] marks):Control
    {public override void Render(DrawingContext context){foreach(var mark in marks)mark.Draw(context,1);}}
}
