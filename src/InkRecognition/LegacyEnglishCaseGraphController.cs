namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private byte[]? graphControllerValues,graphControllerMask;
    private void InitializeGraphController(LegacyEnglishCaseInitialQueryPass pass,CancellationToken token)
    {
        RequireNode1(pass.Root,pass);token.ThrowIfCancellationRequested();
        if(graphControllerValues is not null||graphControllerMask is not null)throw new InvalidOperationException("One same-owner controller initialization required.");
        graphControllerValues=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Controller,4096);
        graphControllerMask=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.ControllerMask,4096);
        // Native initial residue is represented by zero DATA and unknown mask,
        // never asserted zero-known. Same private node0 AUX state takes precedence.
        if(auxiliaryState is not null)
        {
            ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.Controller,auxiliaryState.Controller,graphControllerValues);
            ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.ControllerMask,auxiliaryState.ControllerMask,graphControllerMask);
            return;
        }
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.PageReferences,40);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.PageReferences,40);
        var pages=pass.CopyCompletedPages();if(pages.Length is <1 or >5)throw new InvalidDataException("Same original initial normal-page history required.");
        int skip=0;
        foreach(var page in pages)
        {
            token.ThrowIfCancellationRequested();if(page.Skip!=skip||page.Count is <0 or >128||page.Count>400-skip)throw new InvalidDataException("Actual initial page chronology changed.");
            int bytes=checked(page.Count*32);ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.PageCopies,bytes);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.PageCopies,bytes);
            var records=page.CopyRecords();if(records.Length!=bytes)throw new InvalidDataException("Exact owned initial records required.");
            ObserveGraphControllerWrite(records,token);skip+=page.Count;
        }
        if(skip!=pass.ConsumedRows)throw new InvalidDataException("Controller prefix differs from completed original query rows.");
    }
    private void ObserveGraphControllerWrite(ReadOnlySpan<byte> records,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(graphControllerValues is null||graphControllerMask is null||records.Length%32!=0||records.Length>4096)
            throw new InvalidOperationException("Same private4096 controller and complete source32 prefix required.");
        // Every successful source Read overwrites ONLY returnedCount*32 prefix.
        // Zero-return is no write, including after terminal injection.
        ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.Controller,records,graphControllerValues);
        ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.ControllerMask,graphControllerMask.AsSpan(0,records.Length),255);
    }
}
