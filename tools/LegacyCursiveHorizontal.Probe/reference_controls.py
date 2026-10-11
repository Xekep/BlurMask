"""Independent scalar native18e960 control interpreter on own prepared ink."""
import base64,json,math,random,struct,sys
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def add(a,b):return f(a+b)
def sub(a,b):return f(a-b)
def mul(a,b):return f(a*b)
def div(a,b):return f(a/b) if b else (float('nan') if not a else math.copysign(float('inf'),a))
def hypot(x,y):return f(math.sqrt(float(y)*y+float(x)*x))
def bits(v):return struct.unpack('<i',struct.pack('<f',v))[0]
def loop(t):return 11<=t<=14
class Stage:
    def __init__(self,strokes,tags,stroke):
        self.rows=[];self.counts=[len(s) for s in strokes];self.stroke=stroke;self.start=sum(self.counts[:stroke])
        for s,points in enumerate(strokes):
            for p in points:self.rows.append(p+[0.,0.,0.,tags[len(self.rows)],s,len(self.rows)])
        for i in range(len(self.rows)-1):self.forward(i)
    @property
    def end(self):return self.start+self.counts[self.stroke]
    def forward(self,i):
        a,b=self.rows[i:i+2];dx=sub(b[0],a[0]);dy=sub(b[1],a[1]);a[2:5]=[dx,dy,hypot(dx,dy)]
    def length(self,a,b):
        result=0.
        for i in range(a,b):result=add(result,self.rows[i][4])
        return result
    def insert(self,i,x,y,tag):
        self.rows.insert(i,[x,y,0.,0.,0.,tag,self.stroke,self.rows[i-1][7]]);self.counts[self.stroke]+=1
        self.forward(i-1);self.forward(i)
    def insert_arc(self,a,b,target,tag):
        distance=0.
        for i in range(a,b):
            r=self.rows[i];distance=add(distance,r[4])
            if distance>target:
                t=div(sub(target,sub(distance,r[4])),r[4]);self.insert(i+1,add(mul(t,r[2]),r[0]),add(mul(t,r[3]),r[1]),tag);return i
        return -1
    def tail(self,a,b,reversal):
        at=-1;distance=0.
        for i in range(a,b+1):
            if self.rows[i][5]==reversal:at=i;distance=0.
            distance=add(distance,self.rows[i][4])
        return at,mul(sub(distance,self.rows[b][4]),.5)
    def single(self,a,b,total,reversal,height):
        preserve=False;gap=0.;cursor=b
        while cursor<self.end-1:
            gap=add(gap,self.rows[cursor][4]);next_tag=self.rows[cursor+1][5]
            if next_tag and not loop(next_tag):
                if gap<=mul(height,f(.35)):preserve=True
                else:
                    at,target=self.tail(a,b,reversal);preserve=at==b
                    if at<b:
                        seg=self.insert_arc(at,b,target,4 if reversal==7 else 6)
                        if seg>=0:b+=1;cursor=seg
                break
            cursor+=1
        if cursor==self.end-1:
            if gap>mul(height,f(.35)):
                at,target=self.tail(a,b,reversal)
                if at==b:preserve=True
                elif at<b and self.insert_arc(at,b,target,4 if reversal==7 else 6)>=0:b+=1
            else:preserve=True
        if total>mul(height,f(1.8)):
            distance=0.
            for i in range(a,b):
                distance=add(distance,self.rows[i][4])
                if self.rows[i+1][5]==reversal:break
            if distance<=mul(total,.25) or distance>=mul(total,.75):
                if self.insert_arc(a,b,mul(total,.5),4 if reversal==7 else 6)>=0:b+=1
            else:preserve=True
        return b,preserve
    def mixed(self,a,b,spacing):
        at=b;tail=0.;seven=True
        while at>=a:
            if self.rows[at][5]==8:seven=False;break
            if self.rows[at][5]==7:break
            tail=add(tail,self.rows[at-1][4]);at-=1
        self.rows[at][5]=0
        if at==b:self.rows[at][5]=4 if seven else 6
        elif self.insert_arc(at,b,mul(tail,.5),4 if seven else 6)>=0:b+=1
        for i in range(a,b+1):
            if self.rows[i][5]==8:self.rows[i][5]=0
        first=True;distance=0.
        for i in range(a,b+1):
            if self.rows[i][5]==7:
                if first:first=False
                elif distance<spacing:self.rows[i][5]=0;distance=add(distance,self.rows[i][4]);continue
                distance=0.
            distance=add(distance,self.rows[i][4])
        return b
    def general(self,a,b,radius):
        first,last=self.rows[a],self.rows[b];chord=hypot(sub(last[0],first[0]),sub(last[1],first[1]));minimum=maximum=0.;negative=positive=-1
        chord_dx=div(sub(last[0],first[0]),chord);chord_dy=div(sub(last[1],first[1]),chord)
        for i in range(a+1,b+1):
            p=self.rows[i];dx=sub(p[0],first[0]);dy=sub(p[1],first[1]);distance=hypot(dx,dy)
            if not distance:continue
            cross=mul(sub(mul(div(dx,distance),chord_dy),mul(div(dy,distance),chord_dx)),distance)
            if minimum<=cross:
                if maximum<cross:maximum=cross;positive=i
            elif cross<minimum:minimum=cross;negative=i
        left,right=a,b
        if negative<0:
            if positive>=0:left=right=positive
        elif positive<0:left=right=negative
        else:left,right=sorted([negative,positive])
        before=after=0.
        for i in range(left-1,self.start-1,-1):
            before=add(before,self.rows[i][4])
            if self.rows[i][5] and not loop(self.rows[i][5]):break
        for i in range(right,self.end-1):
            after=add(after,self.rows[i][4])
            if self.rows[i+1][5] and not loop(self.rows[i+1][5]):break
        if left==right:
            if radius<before and radius<after:self.rows[left][5]=5
            return b
        target=mul(self.length(left,right),.5);walked=0.
        for i in range(left,right):
            p=self.rows[i];walked=add(walked,p[4])
            if walked<=target:continue
            t=div(sub(target,sub(walked,p[4])),p[4]);x=add(mul(t,p[2]),p[0]);y=add(mul(t,p[3]),p[1]);first_part=hypot(sub(x,p[0]),sub(y,p[1]))
            if radius<add(add(first_part,sub(walked,p[4])),before):
                n=self.rows[i+1];remainder=hypot(sub(x,n[0]),sub(y,n[1]))
                for j in range(i+1,right):remainder=add(remainder,self.rows[j][4])
                if radius<add(after,remainder):self.insert(i+1,x,y,5);b+=1
            break
        return b
    def run(self,height,reverse,keep):
        active=strong=in_loop=False;group_start=0;i=self.start
        while i<self.end-1:
            r=self.rows[i];tag=r[5];dx=-r[2] if reverse else r[2]
            if loop(tag) and tag!=12:in_loop=True;close=True
            else:
                if tag==12:in_loop=False
                if in_loop:close=True
                else:
                    close=dx<=div(abs(r[3]),.5) or self.rows[i+1][5]==11 or i>=self.end-2
                    if not close:
                        if abs(div(r[3],f(.35)))<dx:strong=True
                        if not active:active=True;group_start=i
            if close:
                if active and strong:
                    b=i+1 if i==self.end-2 else i;arc=self.length(group_start,b);sevens=sum(r[5]==7 for r in self.rows[group_start:b+1]);eights=sum(r[5]==8 for r in self.rows[group_start:b+1]);before=after=True
                    if sevens+eights==1:before=any(r[5] for r in self.rows[self.start+1:b+1]);after=any(r[5] for r in self.rows[b:self.end])
                    if mul(height,f(.2))<arc and before and after:
                        preserve=False
                        if not sevens and not eights:b=self.general(group_start,b,mul(height,f(.35)))
                        elif not sevens or not eights:b,preserve=self.single(group_start,b,arc,7 if sevens else 8,height)
                        else:
                            displacement=sub(self.rows[b][0],self.rows[group_start][0]);b=self.mixed(group_start,b,mul(-displacement if reverse else displacement,.25));preserve=True
                        if not keep:
                            mask=0x19f2 if preserve else 0x1872
                            for j in range(group_start,b+1):
                                t=self.rows[j][5]
                                if t<0 or t>12 or not(mask>>t&1):self.rows[j][5]=0
                        else:
                            kept=False
                            for j in range(b-1,group_start-1,-1):
                                t=self.rows[j][5]
                                if 4<=t<=6:
                                    if kept:self.rows[j][5]=0
                                    else:kept=True
                                elif t not in [1,11,12]:self.rows[j][5]=0
                active=False
            i+=1
def make(rng):
    strokes=[];tags=[]
    for _ in range(rng.randint(1,3)):
        points=[[0.,0.]]
        for i in range(rng.randint(5,35)):points.append([add(points[-1][0],rng.choice([1,2,3,5,-2])),add(points[-1][1],rng.choice([0,0,.25,-.25,1,-1,4]))])
        strokes.append(points);tags.extend([1]+[rng.choice([0,0,0,0,7,8,11,12,13,14]) for _ in points[1:]])
    stroke=rng.randrange(len(strokes));height=f(rng.choice([.5,1,2,4,8,16]));reverse=bool(rng.randrange(2));keep=bool(rng.randrange(2));state=Stage(strokes,tags,stroke);state.run(height,reverse,keep)
    expected=[[bits(v) for v in r[:5]]+r[5:] for r in state.rows]
    return dict(strokes=strokes,tags=tags,stroke=stroke,height=height,reverse=reverse,keep=keep,counts=state.counts,expected=expected)
rng=random.Random(0x29718e960)
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump([make(rng) for _ in range(768)],stream,separators=(',',':'))
print('Wrote768 independent horizontal-stage controls')
