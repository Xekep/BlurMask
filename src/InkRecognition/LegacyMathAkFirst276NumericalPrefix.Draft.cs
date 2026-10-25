namespace InkRecognition;

public sealed partial class LegacyMathAkFeatureProfile
{
    // Numeric fill only, not a source/request/evaluator issuer. The only owning
    // caller is the SAME exclusive storage below; no replacement model enters.
    internal int FillFirst276NumericalPrefix(ReadOnlySpan<short> input,Span<short> hidden,
        Span<int> output90,CancellationToken token)
    {
        RequireFirst276StorageModel(token);RequireFirst276FeatureShape(token);
        return firstModelData.FillNumericalPrefix(input,hidden,output90,token);
    }
    private sealed partial class First276ModelData
    {
        internal int FillNumericalPrefix(ReadOnlySpan<short> input,Span<short> hidden,
            Span<int> output90,CancellationToken token)
        {
            RequireStoragePolicy();token.ThrowIfCancellationRequested();
            if(input.Length!=276||hidden.Length!=208||output90.Length!=258||input[0]!=1024)
                throw new InvalidOperationException("Exact actual FIRST numerical storage shape required.");
            // Decoded model checks prove first288 padding12 and second208
            // padding7 weights zero. Omit products; never read unknown buffers.
            for(int row=0;row<200;row++)
            {
                token.ThrowIfCancellationRequested();int dot=0,at=row*288;
                unchecked{for(int lane=0;lane<276;lane++)dot+=input[lane]*first[at+lane];}
                hidden[row+1]=FirstHidden(dot);
            }
            token.ThrowIfCancellationRequested();hidden[0]=1024;
            int maximum=18808832;
            for(int row=0;row<257;row++)
            {
                token.ThrowIfCancellationRequested();int dot=0,at=row*208;
                unchecked{for(int lane=0;lane<201;lane++)dot+=hidden[lane]*second[at+lane];dot*=scales[row+1];}
                output90[row+1]=dot;if(dot>maximum)maximum=dot;
            }
            int offset=maximum>18808832?maximum-18808832:0;
            for(int index=1;index<258;index++)
            {
                token.ThrowIfCancellationRequested();int value=unchecked(output90[index]-offset+4194304);
                output90[index]=value<0?0:FirstExp(value>>16);
            }
            token.ThrowIfCancellationRequested();
            // Native E82C0 stores this C0 offset before its unported E69F0 call.
            // No slot0 read/write, normalization, C4, A0/98/A8 or ready flag.
            return offset;
        }
        private static short FirstHidden(int value)
        {
            if(value==int.MinValue)throw new NotSupportedException("INT_MIN hidden lookup domain.");
            int index=(value<0?-value:value)>>16;
            int magnitude=index switch
            {
                0=>0,
                1=>65,
                2=>130,
                3=>193,
                4=>255,
                5=>316,
                6=>374,
                7=>429,
                8=>481,
                9=>531,
                10=>577,
                11=>620,
                12=>660,
                13=>696,
                14=>730,
                15=>760,
                16=>788,
                17=>814,
                18=>837,
                19=>857,
                20=>876,
                21=>892,
                22=>907,
                23=>920,
                24=>932,
                25=>943,
                26=>952,
                27=>960,
                28=>968,
                29=>974,
                30=>980,
                31=>985,
                32=>990,
                33=>994,
                34=>997,
                35=>1000,
                36=>1003,
                37=>1005,
                38=>1008,
                39=>1010,
                40=>1011,
                41=>1013,
                42=>1014,
                43=>1015,
                44=>1016,
                45=>1017,
                46=>1018,
                47=>1018,
                _=>1018
            };
            return (short)(value<=0?-magnitude:magnitude);
        }
        private static int FirstExp(int index)=>index switch
        {
                0=>0,
                1=>0,
                2=>0,
                3=>0,
                4=>0,
                5=>0,
                6=>0,
                7=>0,
                8=>0,
                9=>0,
                10=>0,
                11=>0,
                12=>0,
                13=>1,
                14=>1,
                15=>1,
                16=>1,
                17=>1,
                18=>1,
                19=>1,
                20=>1,
                21=>1,
                22=>1,
                23=>1,
                24=>2,
                25=>2,
                26=>2,
                27=>2,
                28=>2,
                29=>2,
                30=>3,
                31=>3,
                32=>3,
                33=>3,
                34=>3,
                35=>4,
                36=>4,
                37=>4,
                38=>4,
                39=>5,
                40=>5,
                41=>5,
                42=>6,
                43=>6,
                44=>7,
                45=>7,
                46=>8,
                47=>8,
                48=>9,
                49=>9,
                50=>10,
                51=>11,
                52=>11,
                53=>12,
                54=>13,
                55=>14,
                56=>15,
                57=>16,
                58=>17,
                59=>18,
                60=>19,
                61=>20,
                62=>22,
                63=>23,
                64=>25,
                65=>26,
                66=>28,
                67=>30,
                68=>32,
                69=>34,
                70=>36,
                71=>39,
                72=>41,
                73=>44,
                74=>47,
                75=>50,
                76=>53,
                77=>57,
                78=>60,
                79=>64,
                80=>68,
                81=>73,
                82=>78,
                83=>83,
                84=>88,
                85=>94,
                86=>100,
                87=>106,
                88=>113,
                89=>121,
                90=>129,
                91=>137,
                92=>146,
                93=>155,
                94=>165,
                95=>176,
                96=>187,
                97=>200,
                98=>213,
                99=>226,
                100=>241,
                101=>257,
                102=>273,
                103=>291,
                104=>310,
                105=>330,
                106=>351,
                107=>374,
                108=>398,
                109=>424,
                110=>451,
                111=>481,
                112=>512,
                113=>545,
                114=>580,
                115=>618,
                116=>658,
                117=>700,
                118=>746,
                119=>794,
                120=>845,
                121=>900,
                122=>958,
                123=>1020,
                124=>1086,
                125=>1157,
                126=>1231,
                127=>1311,
                128=>1396,
                129=>1486,
                130=>1582,
                131=>1685,
                132=>1794,
                133=>1910,
                134=>2033,
                135=>2165,
                136=>2305,
                137=>2454,
                138=>2613,
                139=>2782,
                140=>2962,
                141=>3154,
                142=>3358,
                143=>3575,
                144=>3806,
                145=>4052,
                146=>4315,
                147=>4594,
                148=>4891,
                149=>5207,
                150=>5544,
                151=>5903,
                152=>6285,
                153=>6691,
                154=>7124,
                155=>7585,
                156=>8076,
                157=>8598,
                158=>9154,
                159=>9746,
                160=>10377,
                161=>11048,
                162=>11763,
                163=>12523,
                164=>13334,
                165=>14196,
                166=>15114,
                167=>16092,
                168=>17133,
                169=>18241,
                170=>19421,
                171=>20677,
                172=>22015,
                173=>23439,
                174=>24955,
                175=>26569,
                176=>28288,
                177=>30118,
                178=>32066,
                179=>34140,
                180=>36349,
                181=>38700,
                182=>41203,
                183=>43868,
                184=>46706,
                185=>49727,
                186=>52944,
                187=>56368,
                188=>60015,
                189=>63897,
                190=>68030,
                191=>72430,
                192=>77115,
                193=>82103,
                194=>87414,
                195=>93069,
                196=>99089,
                197=>105498,
                198=>112322,
                199=>119588,
                200=>127323,
                201=>135559,
                202=>144328,
                203=>153663,
                204=>163603,
                205=>174185,
                206=>185452,
                207=>197448,
                208=>210220,
                209=>223818,
                210=>238296,
                211=>253709,
                212=>270120,
                213=>287593,
                214=>306196,
                215=>326002,
                216=>347089,
                217=>369540,
                218=>393443,
                219=>418893,
                220=>445989,
                221=>474837,
                222=>505551,
                223=>538252,
                224=>573069,
                225=>610137,
                226=>649603,
                227=>691622,
                228=>736359,
                229=>783990,
                230=>834702,
                231=>888694,
                232=>946178,
                233=>1007380,
                234=>1072542,
                235=>1141918,
                236=>1215782,
                237=>1294424,
                238=>1378152,
                239=>1467297,
                240=>1562208,
                241=>1663258,
                242=>1770844,
                243=>1885389,
                244=>2007344,
                245=>2137187,
                246=>2275429,
                247=>2422613,
                248=>2579317,
                249=>2746158,
                250=>2923790,
                251=>3112913,
                252=>3314268,
                253=>3528649,
                254=>3756896,
                255=>3999907,
                256=>4258637,
                257=>4534103,
                258=>4827387,
                259=>5139642,
                260=>5472095,
                261=>5826052,
                262=>6202904,
                263=>6604133,
                264=>7031315,
                265=>7486129,
                266=>7970362,
                267=>8485917,
                268=>9034820,
                269=>9619229,
                270=>10241439,
                271=>10903897,
                272=>11609204,
                273=>12360135,
                274=>13159638,
                275=>14010856,
                276=>14917135,
                277=>15882035,
                278=>16909349,
                279=>18003114,
                280=>19167628,
                281=>20407467,
                282=>21727504,
                283=>23132926,
                284=>24629257,
                285=>26222376,
                286=>27918545,
                287=>29724429,
                288=>31647125,
                289=>33694188,
                290=>35873664,
                291=>38194117,
                292=>40664666,
                293=>43295020,
                294=>46095516,
                295=>49077160,
                296=>52251668,
                297=>55631516,
                298=>59229986,
                299=>63061220,
                300=>67140273,
                301=>71483176,
                302=>76106996,
                303=>81029902,
                304=>86271242,
                305=>91851614,
                306=>97792945,
                307=>104118586,
                308=>110853396,
                309=>118023839,
                310=>125658096,
                311=>133786167,
                312=>142439994,
                313=>151653586,
                314=>161463150,
                315=>171907236,
                316=>183026888,
                317=>194865803,
                318=>207470507,
                319=>220890534,
                320=>235178623,
                321=>250390922,
                322=>266587214,
                323=>283831147,
                324=>302190488,
                325=>321737384,
                326=>342548652,
                327=>364706077,
                328=>388296733,
                329=>413413328,
                330=>440154565,
                331=>468625533,
                332=>498938117,
                333=>531211442,
                334=>565572335,
                335=>602155829,
                336=>641105690,
                337=>682574985,
                338=>726726680,
                339=>773734285,
                340=>823782531,
                341=>877068099,
                342=>933800392,
                343=>994202358,
                344=>1058511365,
                345=>1126980138,
                346=>1199877747,
                347=>1277490666,
                348=>1360123901,
                349=>1448102186,
                350=>1541771261,
                351=>1641499228,
                _=>1641499228
        };
    }
}

internal sealed partial class LegacyMathAkOriginalInputContext
{
    // Explicit DRAFT route; original input-only route remains the default.
    internal static void PrepareFreshFirstNumericalBoundary(LegacyMathAkSourceGeometry original,
        CancellationToken token=default)=>PrepareFreshFirstInputCore(original,1,token);
    private sealed partial class First276SourceBinding
    {
        private sealed partial class First276ExclusiveStorage
        {
            internal void FillNumericalPrefixInside(First276FreshInputProducer actual,CancellationToken token)
            {
                actual.RequireOwnedNumericalStart(this,binding,token);RequireInputProducer(binding,token);
                if(!ReferenceEquals(profile,binding.profile))throw new InvalidOperationException("Same FIRST profile/model owner required.");
                int c0=profile.FillFirst276NumericalPrefix(input.AsSpan(0,276),hidden,buffer90,token);
                actual.CompleteNumericalPrefix(this,binding,c0,token);
            }
        }
        private sealed partial class First276ExclusiveStorage
        {
            internal First276NumericalDiagnosticData CopyNumericalData(First276FreshInputProducer actual,CancellationToken token)
            {
                int c0=actual.RequireOwnedNumericalData(this,binding,token);RequireInputProducer(binding,token);
                // Both arrays and record are precharged by the active producer.
                var knownHidden=new short[201];var knownExponential=new int[257];
                for(int i=0;i<201;i++){token.ThrowIfCancellationRequested();knownHidden[i]=hidden[i];}
                for(int i=0;i<257;i++){token.ThrowIfCancellationRequested();knownExponential[i]=buffer90[i+1];}
                return new(knownHidden,knownExponential,c0);
            }
        }
        private sealed partial class First276FreshInputProducer
        {
            private int numericalPhase,numericalC0;
            // Actual recurrence: guards once; two dot-lane loops; activation
            // dispatch once/row. Never entire-method-source multiplied by lane.
            private const long NumericalSourceUpper=32768+200*(80+32*276+3*48)+257*(80+32*201)+257*(80+4*352);
            private void FillNumericalPrefix(CancellationToken token)
            {
                Charge(8,2478,NumericalSourceUpper,token);RequireProducer(token);
                storage.FillNumericalPrefixInside(this,token);
            }
            internal void RequireOwnedNumericalStart(First276ExclusiveStorage actual,First276SourceBinding source,CancellationToken token)
            {
                RequireOwnedInputRead(actual,source,token);
                if(Interlocked.CompareExchange(ref numericalPhase,1,0)!=0)
                    throw new InvalidOperationException("FIRST numerical prefix cannot restart.");
            }
            private const long NumericalSnapshotSourceUpper=32768+32*(201+257);
            private First276NumericalDiagnosticData CopyNumericalData(CancellationToken token)
            {
                Charge(1450,2884,NumericalSnapshotSourceUpper,token);RequireProducer(token);
                return storage.CopyNumericalData(this,token);
            }
            internal int RequireOwnedNumericalData(First276ExclusiveStorage actual,First276SourceBinding source,CancellationToken token)
            {
                RequireOwnedInputRead(actual,source,token);
                if(numericalPhase!=2)throw new InvalidOperationException("Completed SAME numerical prefix required before DATA copy.");
                return numericalC0;
            }
            internal void CompleteNumericalPrefix(First276ExclusiveStorage actual,First276SourceBinding source,int c0,CancellationToken token)
            {
                RequireOwnedInputRead(actual,source,token);
                if(c0<0)throw new InvalidOperationException("Actual native C0 offset domain changed.");
                numericalC0=c0;
                if(Interlocked.CompareExchange(ref numericalPhase,2,1)!=1)
                    throw new InvalidOperationException("FIRST numerical write chronology changed.");
            }
        }
    }
    // Detached known values only; no slot0, padding, mask or live owner.
    internal sealed record First276NumericalDiagnosticData(short[] Hidden201,int[] Exponential257,int StabilizationOffsetC0);
    internal sealed class First276PartialNumericalBoundaryException:NotSupportedException
    {
        internal First276InputBoundaryData Prefix {get;}
        internal First276NumericalDiagnosticData Numerical {get;}
        internal int InitializedHiddenValues=>201;
        internal int ExponentialOutputValues=>257;
        internal First276PartialNumericalBoundaryException(First276InputBoundaryData prefix,First276NumericalDiagnosticData numerical)
            :base("Actual FIRST numerical prefix complete; normalization/shortlist continuation remains unported."){Prefix=prefix;Numerical=numerical;}
    }
}
