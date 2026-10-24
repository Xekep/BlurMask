import base64,copy,struct

def fixture():
 b64=lambda b:base64.b64encode(b).decode('ascii')
 raw={'Resolution':96,'Raw':[[[0,0],[10,20],[20,0]],[[30,5],[35,25],[40,5]]]}
 line=bytearray(56)
 for off,v in [(8,0),(16,25),(24,0),(32,20),(48,25)]:struct.pack_into('<d',line,off,v)
 struct.pack_into('<q',line,40,-1)
 rows=bytearray(80)
 for i,(label,score)in enumerate([(97,.75),(98,.625)]):struct.pack_into('<H',rows,i*40+8,label);struct.pack_into('<f',rows,i*40+16,score)
 ranges=[{'First':3,'Last':2<<16|0xff05},{'First':1<<32|3,'Last':1<<32|2<<16|0xff05}]
 links=[{'Reference':i<<40,'Utf8':b64(bytes([97+i]))}for i in range(2)]
 old={'SourceInvocations':1,'ValidationLoopBodies':100,'SourceLines56':b64(line),'AfterText':{'Pool':[{'Key':0,'Storage40':b64(rows)}]},'AfterBatch':{'Nodes':[{'First':0,'Last':1,'WordIndex':0}],'Owners':[{'First':0,'Last':1,'Words':[{},{}],'SelectedOrdinals':[0]}]},'Words':[{'SelectedIndex':0,'Choices':[{'Links':links},{'Links':links}],'OriginalRanges':ranges,'SelectedUtf8':b64(b'ab')}]}
 geometry={'Index':0,'OriginX':0,'OriginY':25,'Angle':0,'XHeight':20,'UnadjustedBaseline':25,'OwnerTag':-1}
 use={'Index':0,'SourceSelectedIndex':0,'ProcessedSelectedIndex':0,'SourceChoiceCount':2,'ProcessedChoiceCount':2,'SelectedUtf8':'ab','CandidateScore':.625,'MeetsExperimentalGlyphPolicy':True,'Line':geometry,'OriginalRanges':copy.deepcopy(ranges),'Bounds':{'Left':0,'Top':0,'Right':40,'Bottom':25},'PointReads':8}
 new=copy.deepcopy(old)
 #P=W=U=K=1,A=2 =>queue9,binding13. Two glyphs:13+28*9+4*13=317reader;27inherited=344.
 new['ValidationLoopBodies']=444
 new['StructuredWordConsumer']={'WordCount':1,'FullOriginalCoverage':True,'MeetsExperimentalGlyphPolicy':True,'ProjectedPointReads':8,'MaximumPointReads':1048576,'SameOriginalDocument':True,'Coverage':[{'First':3,'Last':1<<32|2<<16|0xff05}],'Uses':[use]}
 return old,new,raw
