namespace InkRecognition.Math;
/// <summary>Explicit library request helper; recognition never invokes it.
/// A supplied solver can call existing local polynomial/Giac APIs.</summary>
public static class ExperimentalMathCalculation
{
    public static string? Calculate(ExperimentalMathResult actual,Func<string,CancellationToken,string?>? solver=null,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actual);token.ThrowIfCancellationRequested();
        if(actual.UnsupportedReason is not null||actual.LaTex is not string latex||!actual.HasCalculationRequest)return null;
        string expression=latex.TrimEnd()[..^1].TrimEnd();if(expression.Length==0||expression.Contains('='))return null;
        if(NumericExpressionEvaluator.TryEvaluate(expression,out string numeric)){token.ThrowIfCancellationRequested();return numeric;}
        token.ThrowIfCancellationRequested();string? result=solver?.Invoke(expression,token);token.ThrowIfCancellationRequested();return result;
    }
}
