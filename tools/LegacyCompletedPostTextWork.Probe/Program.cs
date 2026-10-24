using InkRecognition;
int controls=0;
for(int n=0;n<=64;n++)for(int m=0;m<=64;m++)
{
    var work=new LegacyCompletedPostTextIntervalWork();work.ChargeUnion(n,m);
    long expected=((long)n+m)*(m+1);if(work.Used!=expected)throw new Exception("Union prefix charge mismatch.");
    work.ChargeSubtract(m,n);expected+=((long)n+m)*(n+1);if(work.Used!=expected)throw new Exception("Subtract growth charge mismatch.");
    work.ChargeRows(17);if(work.Used!=expected+17)throw new Exception("Clone row charge mismatch.");controls++;
}
var failed=new LegacyCompletedPostTextIntervalWork();failed.ChargeRows(9);
try{failed.ChargeSubtract(65536,65536);throw new Exception("Missing exhaustion.");}catch(NotSupportedException){}
if(failed.Used!=9)throw new Exception("Refused operation mutated prefix meter.");
try{failed.ChargeUnion(-1,0);throw new Exception("Missing count refusal.");}catch(NotSupportedException){}
if(failed.Used!=9)throw new Exception("Invalid count mutated prefix.");
Console.WriteLine($"PASS {controls}+2 prepared interval-work controls; no source/model/NN");
