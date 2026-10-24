namespace InkRecognition;
// Exact207CB0 primary candidate flag selection2093A8..209512, before206D30.
// PURE numeric policy. No query/lease authority is issued by this helper.
internal interface ILegacyMainPrimaryCompactFlagRow
{
 ulong Tag{get;} ulong Context{get;} int Auxiliary{get;} bool FallbackByte3{get;}
 bool TerminalBit3{get;} bool Forced{get;}
}
internal static class LegacyPreparedMainPrimaryCompactFlags
{
 internal readonly record struct Decision(bool TerminalBit3,bool TerminalBit4,int SelectedDistinctAfter,int ProbeCalls);
 internal static Decision Select<T>(IReadOnlyList<T> rows,int current,int groupStart,int selectedDistinct,int quota,
  ReadOnlySpan<uint> actualSourceHeaders,Func<bool> sameOwned207780,CancellationToken token=default) where T:ILegacyMainPrimaryCompactFlagRow
 {
  token.ThrowIfCancellationRequested();
  if(current<groupStart||groupStart<0||current>=rows.Count||selectedDistinct<0||quota is <0 or >65535)
   throw new ArgumentException("Exact sorted primary candidate/group/quota required.");
  var row=rows[current];bool bit3=false,bit4=false;int calls=0;
  // Native first probe is conditional on original fallback byte3 and quota.
  if(selectedDistinct<quota&&row.FallbackByte3){calls++;bit3=sameOwned207780();}
  // Native reads source header even when quota is exhausted. No guessed class.
  int source=(byte)(row.Tag>>32);
  if(source>=actualSourceHeaders.Length)throw new InvalidDataException("Actual source22E0 header ordinal required.");
  uint kind=actualSourceHeaders[source]&0x1e00;
  if(selectedDistinct<quota)
  {
   calls++;bool continuation=sameOwned207780();
   bit4=!continuation&&!(kind>=0x200&&kind<=0xa00)&&kind!=0xe00&&kind!=0x1000;
  }
  bool newContext=bit3;
  for(int prior=current-1;prior>=groupStart;prior--)
  {
   token.ThrowIfCancellationRequested();var p=rows[prior];
   if(p.TerminalBit3&&p.Context==row.Context)
   {newContext=false;if(p.Auxiliary==row.Auxiliary)bit3=false;}
   if(p.Forced&&p.Context==row.Context)bit4=false;
  }
  if(newContext)selectedDistinct=checked(selectedDistinct+1);
  token.ThrowIfCancellationRequested();return new(bit3,bit4,selectedDistinct,calls);
 }
}
