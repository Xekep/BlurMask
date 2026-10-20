"""Fixed polygonal diagnostics, not human handwriting or a training set.
No model output is read; fixtures and expected labels are defined before NN.
Writes private diagnostic ink to the requested output directory.
"""
from pathlib import Path
import json,sys,math
root=Path(sys.argv[1]);root.mkdir(parents=True,exist_ok=True)
glyphs={
'0':[[[10,0],[2,3],[0,15],[2,27],[10,30],[18,27],[20,15],[18,3],[10,0]]],
'1':[[[3,7],[10,0],[10,30]],[[3,30],[17,30]]],
'2':[[[0,5],[5,0],[15,0],[20,5],[18,12],[0,30],[20,30]]],
'3':[[[0,2],[15,0],[20,6],[16,13],[8,15],[17,17],[20,24],[15,30],[0,28]]],
'4':[[[15,30],[15,0],[0,22],[22,22]]],
'5':[[[20,0],[0,0],[0,14],[14,14],[20,20],[18,27],[10,30],[0,27]]],
'6':[[[18,0],[8,3],[0,15],[0,25],[8,30],[17,27],[20,20],[15,14],[4,15]]],
'7':[[[0,0],[20,0],[7,30]]],
'8':[[[10,14],[1,7],[4,0],[15,0],[20,7],[10,14],[0,22],[4,30],[15,30],[20,23],[10,14]]],
'9':[[[18,15],[5,15],[0,8],[4,0],[14,0],[20,7],[18,22],[8,30]]],
'+':[[[0,15],[20,15]],[[10,5],[10,25]]],
'=':[[[0,10],[20,10]],[[0,20],[20,20]]],
'x':[[[0,2],[20,28]],[[20,2],[0,28]]],
'd':[[[18,0],[18,30]],[[18,15],[8,13],[0,19],[0,26],[8,30],[18,25]]],
'int':[[[20,0],[15,-3],[9,0],[7,10],[4,40],[0,45],[-5,43]]],
}
def placed(key,x=0,y=0,scale=1):
 return [[[x+a*scale,y+b*scale]for a,b in stroke]for stroke in glyphs[key]]
samples=[]
def save(name,ink,expected):
 path=root/(name+'.json');path.write_text(json.dumps(ink,separators=(',',':'))+'\n',encoding='utf8')
 samples.append(dict(id=name,strokes=path.name,expected=expected,creationMethod='procedural-fixed-polylines-not-human'))
for key in '0123456789+=':save('procedural-'+('plus'if key=='+'else'equals'if key=='='else key),placed(key),key)
save('procedural-one-plus-two',placed('1')+placed('+',30)+placed('2',60),'1+2')
save('procedural-one-plus-two-equals',placed('1')+placed('+',30)+placed('2',60)+placed('=',90),'1+2=')
save('procedural-x-squared',placed('x')+placed('2',23,-10,.5),'x^{2}')
# Explicit two-dimensional upper/lower bounds and superscript; never serialized LaTeX input to NN.
integral=placed('int',5)+placed('1',0,48,.45)+placed('1',5,-24,.45)+placed('0',16,-24,.45)+placed('x',35,7)+placed('2',58,-3,.5)+placed('d',76,7)+placed('x',103,7)
save('procedural-definite-integral-1-10-x-squared-dx',integral,r'\int_{1}^{10}x^{2}dx')
save('procedural-definite-integral-1-10-x-squared-dx-equals',integral+placed('=',135,7),r'\int_{1}^{10}x^{2}dx=')
(root/'manifest.json').write_text(json.dumps(dict(schemaVersion=1,policy='Procedural coordinates only. No human data, no output calibration, expected labels compared after inference.',samples=samples),indent=2)+'\n',encoding='utf8')
print('Generated',len(samples),'fixed procedural diagnostics')
