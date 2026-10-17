using InkRecognition;
using System.Text.Json;

if(args.Length!=1)throw new ArgumentException("capture.json");
var labels=new LegacyContextualQuoteLabels(10,11,20,21,22,23,24);
int checks=0;
//Exhaust every ushort to check the native wrapped four-label interval.
for(int n=0;n<=ushort.MaxValue;n++)
{
    ushort value=(ushort)n;bool control=n>=65529&&n<=65532;
    Check(LegacyPreparedContextualQuoteRules.Leading(11,value,0,0,labels)==(control?11:20));
    Check(LegacyPreparedContextualQuoteRules.Trailing(value,11,0,0,false,labels)==(control?11:21));
    Check(LegacyPreparedContextualQuoteRules.Trailing(value,11,0,0,true,labels)==(control?11:20));
}
using var output=File.Create(args[0]);using var w=new Utf8JsonWriter(output);
w.WriteStartObject();w.WriteStartArray("cases");
foreach(ushort current in new ushort[]{10,11,24,30})
foreach(ushort previous in new ushort[]{0,31,99,100,65528,65529,65530,65531,65532,65533,65535})
foreach(ushort next in new ushort[]{0,65528,65529,65530,65531,65532,65533,65535})
foreach(int category in new[]{0,1,2})
foreach(bool carry in new[]{false,true})
foreach(short arc in new short[]{-32768,-1,0,32767})
foreach(byte kind in new byte[]{0,1,255})
{
    var result=LegacyPreparedContextualQuoteRules.Interior(previous,current,next,arc,kind,100,category,carry,labels);
    w.WriteStartArray();w.WriteNumberValue(previous);w.WriteNumberValue(current);w.WriteNumberValue(next);
    w.WriteNumberValue(category);w.WriteBooleanValue(carry);w.WriteNumberValue(arc);w.WriteNumberValue(kind);
    w.WriteNumberValue(result.Label);w.WriteBooleanValue(result.Carry);w.WriteEndArray();
    //Every non-quote and every disallowed source must preserve its scalar.
    if(current is not(10 or 11)||arc<0||kind!=0)Check(result.Label==current);
}
w.WriteEndArray();
//OpenDouble alone does not pass the native three-field entry guard.
Check(!new LegacyContextualQuoteLabels(10,11,20,65535,65535,65535,24).HasEnabledReplacement);
Check(labels.HasEnabledReplacement);
Check(!new LegacyContextualQuoteLabels(10,11,65535,65535,65535,65535,24).HasEnabledReplacement);
w.WriteNumber("checks",checks);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS {checks} contextual quote wrap/source/entry guards; enabled-body numeric decisions only, no app policy change.");
void Check(bool ok){if(!ok)throw new Exception("Contextual quote numeric rule changed.");checks++;}
