using System.Buffers.Binary;
using InkRecognition;
internal static class ParentMaskControls
{
 internal static object Run()
 {
  byte[] row=new byte[88],mask=Enumerable.Repeat((byte)255,88).ToArray(),alts=new byte[80],am=new byte[80];
  BinaryPrimitives.WriteInt32LittleEndian(row.AsSpan(16),100);
  int[] opaque=[65,66,67,76,77,78,79];foreach(int at in opaque){mask[at]=0;row[at]=(byte)(at+1);}
  foreach(int at in new[]{0,40}){BinaryPrimitives.WriteInt32LittleEndian(alts.AsSpan(at),10);am.AsSpan(at,24).Fill(255);}
  var saved=row.ToArray();var savedMask=mask.ToArray();var savedAlt=alts.ToArray();var savedAm=am.ToArray();
  if(!LegacyEnglishCaseNode1ParentMasks.Require(row,mask,alts,am))throw new Exception("Real FIRST mask rejected.");
  int[] reads=Enumerable.Range(0,34).Concat(Enumerable.Range(36,3)).Concat(Enumerable.Range(40,22)).ToArray();
  var refused=new List<string>();
  foreach(int at in reads){var poison=mask.ToArray();poison[at]=254;Refuse(()=>LegacyEnglishCaseNode1ParentMasks.Require(row,poison,alts,am));refused.Add("row:"+at);}
  foreach(int at in Enumerable.Range(0,24).Concat(Enumerable.Range(40,24))){var poison=am.ToArray();poison[at]=254;Refuse(()=>LegacyEnglishCaseNode1ParentMasks.Require(row,mask,alts,poison));refused.Add("alt:"+at);}
  var inactive=row.ToArray();BinaryPrimitives.WriteInt32LittleEndian(inactive.AsSpan(16),0x4000000);var inactiveMask=new byte[88];inactiveMask.AsSpan(16,4).Fill(255);
  if(LegacyEnglishCaseNode1ParentMasks.Require(inactive,inactiveMask,alts,new byte[80]))throw new Exception("Inactive row classified finite.");
  foreach(int at in Enumerable.Range(16,4)){var poison=inactiveMask.ToArray();poison[at]=254;Refuse(()=>LegacyEnglishCaseNode1ParentMasks.Require(inactive,poison,alts,new byte[80]));refused.Add("inactive-cost:"+at);}
  var invalidAlts=alts.ToArray();var invalidMasks=new byte[80];foreach(int at in new[]{0,40}){BinaryPrimitives.WriteInt32LittleEndian(invalidAlts.AsSpan(at),0x4000000);invalidMasks.AsSpan(at,4).Fill(255);}
  if(!LegacyEnglishCaseNode1ParentMasks.Require(row,mask,invalidAlts,invalidMasks))throw new Exception("Invalid alt opaque tail read.");
  if(!row.AsSpan().SequenceEqual(saved)||!mask.AsSpan().SequenceEqual(savedMask)||!alts.AsSpan().SequenceEqual(savedAlt)||!am.AsSpan().SequenceEqual(savedAm))throw new Exception("Guard mutated data/masks.");
  return new{Scope="PREPARED exact FIRST seven-unknown pattern; no actual owner authority",Row=Convert.ToHexString(row),RowMask=Convert.ToHexString(mask),Alternatives=Convert.ToHexString(alts),AlternativeMasks=Convert.ToHexString(am),Reads=reads,Refused=refused,OpaqueOffsets=opaque,Unchanged=true,InactiveClassification=false,InvalidAlternativesAccepted=true};
 }
 static void Refuse(Action action){try{action();}catch(InvalidDataException){return;}throw new Exception("Missing required bits accepted.");}
}
