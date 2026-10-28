"""Cold Math library structural adapter consumption and Giac-only calculation contracts.
Native typed-node export is checked separately and is not claimed equivalent.
"""
from pathlib import Path
import argparse,json
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args();root=Path(__file__).resolve().parents[2]
owner=(root/'src/InkRecognition.Math/ExperimentalColdMathRecognizer.cs').read_text(encoding='utf8')
syntax=(root/'src/InkRecognition.Math/ExperimentalMathSyntax.cs').read_text(encoding='utf8')
calc=(root/'src/InkRecognition.Math/ExperimentalMathCalculation.cs').read_text(encoding='utf8')
rules=[(owner,t)for t in('public float? Confidence=>null','Admission=>ExperimentalMathAdmission.Uncalibrated','gate.Wait(token)','ObjectDisposedException.ThrowIf(disposed,this)','CheckInput(strokes,budget,token)','LegacyOwnedRawInkDocument.Create(strokes,token)','PrepareColdWithSelectedMathField(runtime,bundle,run,field,token)','source.RunToInitialOrder(token);source.ApplyEf4(token)','FormatFullCold(token)','attached.CopySelectedMathField(field,token)','source.ReadSelectedMathField(receipt.Field)','receipt.Validate(receipt.Copy,receipt.Field,token)','receipt.Copy.WithSymbolRowsForOuterCopy','rows.Length>4096','rows[i].ByteCount!=rows[i].Text.ByteCount','rows.Length*2+1','sourceReservation+16384+rows.Length*2048L+bytes*16+rangeCount*128','reservation>budget||bytes>1024*1024||rangeCount>65536','var record=receipt.Copy.CopyRecord()','new UTF8Encoding(false,true)','r.First.Value,r.Last.Value','Observe(ExperimentalMathStage.AfterToken,i)','ExperimentalMathSyntax.Parse(detached,maxNodes,token)','if(unsupported is not null)nodes=[]','BitConverter.SingleToInt32Bits(record.Score)','finally{gate.Release();}','try{bundle.Dispose();}finally{runtime.Dispose();}')]
rules +=[(syntax,t)for t in('Array.AsReadOnly(ranges)','CopyUtf8()=>utf8.ToArray()','Array.AsReadOnly(children)','tokens.Count==0','nodes.Count>=maximumNodes','if(++depth>32)','if(text.Length==0)','Script requires a preceding base.','Empty structural operand.','Matching explicit right fence required.','Unknown or misplaced structural token.','Unclosed structural group.')]
rules +=[(calc,t)for t in('actual.UnsupportedReason is not null','!actual.HasCalculationRequest','expression.Contains','solver?.Invoke(expression,token)','token.ThrowIfCancellationRequested();return result')]
for source,rule in rules:assert rule in source,rule
assert owner.index('CheckInput(strokes,budget,token)')<owner.index('LegacyOwnedRawInkDocument.Create(strokes,token)')<owner.index('using var run=runtime.CreateRun(token)')<owner.index('using var source=Math2Mode0RawOnlySource.PrepareCold')
assert owner.index('if(reservation>budget')<owner.index('var record=receipt.Copy.CopyRecord()')<owner.index('rows[i].Text.ReadBytes()')<owner.index('ExperimentalMathSyntax.Parse(detached')
assert 'PrepareWithPreparedSearch'not in owner # No public adapter wider-search route.
assert all(t not in owner for t in('ExperimentalMathCalculation.Calculate','PolynomialIntegralSolver','MathRecognitionResult','MathF.Exp','Math.Exp','0.85','Confidence=>1'))
assert 'NumericExpressionEvaluator'not in calc and 'PolynomialIntegralSolver'not in calc
assert 'solver?.Invoke'in calc and calc.index('!actual.HasCalculationRequest')<calc.index('solver?.Invoke')
a.output.write_text(json.dumps(dict(consumptionChecks=len(rules)+5,calculationPolicy="Giac-only; existing calculation helper updated",vendorExecution=False,scope='cold1 adapter ownership retained; existing explicit calculation helper migrated to Giac-only delegate; no native typed Math bridge or confidence calibration'),indent=2)+'\n',encoding='utf8')
print(f'PASS{len(rules)+5} structural library ownership/budget/cancellation/syntax/explicit-calculation checks; Giac-only calculation helper, no fallback evaluator.')
