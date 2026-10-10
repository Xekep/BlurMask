import { tools, emptyBoard, clone, diff, hasChanges, rebase, History, hit, constrain, drawMark, validBoard, raster, pruneRasters, boardBounds, exportSize } from './ink.js';
import { inviteCode, parseInvite } from './invite.js';

const $ = id => document.getElementById(id);
const paths = {
  Hand:'M8 13V6a2 2 0 0 1 4 0v5 M12 11V4a2 2 0 0 1 4 0v8 M16 11V6a2 2 0 0 1 4 0v10c0 6-10 8-13 3l-4-6a2 2 0 0 1 3-3l2 3',
  Pen:'M3 17L17 3l4 4L7 21l-5 1z M14 6l4 4', Highlighter:'M3 12l7-7 11 11-7 7z M4 13l-3 4 6 6 4-3 M10 5l2-2 11 11-2 2',
  Eraser:'M2 13l9-10 12 12-8 8h-3z M7 8l12 11', Text:'M3 22L12 2l9 20 M6 16h12',
  Line:'M3 3l19 19', Arrow:'M3 22L22 3 M12 3h10v10', Rectangle:'M2 4h21v16H2z', Ellipse:'M23 12a10.5 8 0 1 1-21 0 10.5 8 0 1 1 21 0',
  undo:'M10 3L2 10l8 6v-4c11-2 13 9 2 10v-4c7 0 6-4-2-2z', redo:'M14 3l8 7-8 6v-4C3 10 1 21 12 22v-4c-7 0-6-4 2-2z',
  clear:'M5 7h14l-1 16H6z M3 4h18 M9 4V1h6v3 M9 10v10 M12 10v10 M15 10v10',
  board:'M3 3h18v14H3z M1 1h22 M12 17v6 M7 23l3-6 M17 23l-3-6',
  export:'M2 7h5l2-4h7l2 4h5v14H2z M17 13a4.5 4.5 0 1 1-9 0 4.5 4.5 0 1 1 9 0',
  menu:'M5 3h14v20H5z M9 1h6v4H9z M8 9h8 M8 13h8 M8 17h8'
};
const icon = name => `<svg viewBox="0 0 26 26" aria-hidden="true"><path d="${paths[name]}"/></svg>`;
document.querySelector('.brand-nib').innerHTML = icon('Pen');
$('grip').innerHTML = icon('Pen');
for (const button of document.querySelectorAll('[data-tool]')) button.innerHTML = icon(button.dataset.tool);
for (const name of ['undo','redo','clear','board','export','menu']) $(name).innerHTML = icon(name);
$('shapes').innerHTML = icon('Line');
const colors = ['#FFFFFF','#FFF400','#FF9900','#FF3333','#FF0099','#9933FF','#00C9ED','#0088FF','#2244CC','#00CC66','#66FF00','#111111','#CCCCCC','#FFF6A0','#FFCC99','#FF99AA','#FF99DD','#CCAAFF','#99EEFF','#88BBFF','#8888DD','#88DDAA','#AACC66','#666666'];
let board = emptyBoard(), baseline = emptyBoard(), access = null, pending = null;
let tool = 'Pen', color = '#111111', width = 4, textSize = 28, preview = null, gesture = null;
let history = new History(), busy = false, generation = 0, frame = 0, deferred = null, fitView = true, view = { zoom:1, x:0, y:0 };
const canvas = $('canvas'), ctx = canvas.getContext('2d'), storageKey = 'kiiko-pen-board-v1';
let workspaceBounds=boardBounds(board);
const measureText=(text,size)=>{ctx.font=`${size}px sans-serif`;return ctx.measureText(text).width;};
let hashConnection = null, invalidInvite = false;
if (location.hash) { try { hashConnection = parseInvite(location.hash); } catch { invalidInvite = true; } }
const cacheKey = connection => `${storageKey}:${connection.id}:${connection.key}`;
try {
  const saved = JSON.parse(localStorage.getItem(hashConnection ? cacheKey(hashConnection) : storageKey) ?? localStorage.getItem(storageKey));
  if (saved && (!hashConnection || (saved.access?.id === hashConnection.id && saved.access?.key === hashConnection.key)) && validBoard(saved.board)) {
    board = saved.board; baseline = saved.baseline ?? emptyBoard(); access = saved.access; pending = saved.pending;
  }
} catch {}
if (hashConnection && (access?.id !== hashConnection.id || access?.key !== hashConnection.key)) { access = hashConnection; board = emptyBoard(); baseline = emptyBoard(); }
function saveLocal() { try { const saved=JSON.stringify({board,baseline,access,pending});if(access)localStorage.setItem(cacheKey(access),saved);if(!board.readOnly)localStorage.setItem(storageKey,saved); } catch { status('Локальное хранилище заполнено', true); } }
function status(text, offline=false) { $('status').className = `status ${offline?'offline':'saved'}`; $('status').lastElementChild.textContent = text; }
function update() {
  if(board.readOnly)tool='Hand';
  if(document.activeElement!==$('title'))$('title').value = board.title; $('title').disabled = board.readOnly;
  $('undo').disabled = board.readOnly || !history.undoStack.length; $('redo').disabled = board.readOnly || !history.redoStack.length;
  for (const id of ['clear','board']) $(id).disabled = board.readOnly;
  for (const button of document.querySelectorAll('[data-tool]')) { button.classList.toggle('active', button.dataset.tool === tool); button.disabled = board.readOnly && button.dataset.tool !== 'Hand'; }
  $('shapes').disabled = board.readOnly; $('shapes').classList.toggle('active',['Line','Arrow','Rectangle','Ellipse'].includes(tool));
  $('palette').style.backgroundColor = color;
  canvas.style.cursor = board.readOnly || tool === 'Hand' ? 'grab' : 'crosshair';
  $('fit').textContent = `${Math.round(view.zoom*100)}%`;
  $('hint').textContent = board.readOnly ? 'Просмотр · рисунок обновляется автоматически' : 'Перо — рисовать · H — перемещать · Shift — ровные фигуры';
  requestRender();
}
function requestRender() { if (!frame) frame = requestAnimationFrame(render); }
window.addEventListener('blurmask-image-loaded', requestRender);
function render() {
  frame=0;
  pruneRasters(board.marks);
  const ratio=devicePixelRatio||1, rect=canvas.getBoundingClientRect();
  if (canvas.width!==Math.round(rect.width*ratio)||canvas.height!==Math.round(rect.height*ratio)) { canvas.width=Math.round(rect.width*ratio);canvas.height=Math.round(rect.height*ratio); }
  ctx.setTransform(ratio,0,0,ratio,0,0);ctx.clearRect(0,0,rect.width,rect.height);
  ctx.translate(view.x,view.y);ctx.scale(view.zoom,view.zoom);
  const nextBounds=boardBounds({...board,marks:preview?[...board.marks,preview]:board.marks},measureText);
  workspaceBounds={left:Math.min(workspaceBounds.left,nextBounds.left),top:Math.min(workspaceBounds.top,nextBounds.top),
    right:Math.max(workspaceBounds.right,nextBounds.right),bottom:Math.max(workspaceBounds.bottom,nextBounds.bottom)};
  workspaceBounds.width=workspaceBounds.right-workspaceBounds.left;workspaceBounds.height=workspaceBounds.bottom-workspaceBounds.top;
  const bounds=workspaceBounds;
  ctx.save();ctx.shadowColor='#182c4320';ctx.shadowBlur=24/view.zoom;ctx.shadowOffsetY=5/view.zoom;ctx.fillStyle=board.background==='black'?'#111111':'#ffffff';ctx.fillRect(bounds.left,bounds.top,bounds.width,bounds.height);ctx.restore();
  ctx.save();ctx.beginPath();ctx.rect(bounds.left,bounds.top,bounds.width,bounds.height);ctx.clip();
  for (const mark of board.marks) drawMark(ctx,mark);
  if (preview) drawMark(ctx,preview);ctx.restore();
}
function fit() { fitView=true;const rect=canvas.getBoundingClientRect(),b=boardBounds(board,measureText);workspaceBounds=b;view.zoom=Math.max(.002,Math.min((rect.width-100)/b.width,(rect.height-100)/b.height,1));view.x=(rect.width-b.width*view.zoom)/2-b.left*view.zoom;view.y=(rect.height-b.height*view.zoom)/2-b.top*view.zoom;update(); }
function zoom(factor,position=null) { fitView=false;const rect=canvas.getBoundingClientRect(), p=position??{x:rect.width/2,y:rect.height/2};const next=Math.max(.002,Math.min(8,view.zoom*factor));view.x=p.x-(p.x-view.x)*next/view.zoom;view.y=p.y-(p.y-view.y)*next/view.zoom;view.zoom=next;update(); }
function point(e) { const rect=canvas.getBoundingClientRect();return {x:Math.max(-100000,Math.min(100000,(e.clientX-rect.left-view.x)/view.zoom)),y:Math.max(-100000,Math.min(100000,(e.clientY-rect.top-view.y)/view.zoom))}; }
function local(e) { const rect=canvas.getBoundingClientRect();return {x:e.clientX-rect.left,y:e.clientY-rect.top}; }
function commit(before) { history.push(before,board);saveLocal();status(access?'Сохранение…':'Сохранено на устройстве',!access);update(); }
function selectTool(value) { if (gesture) cancelGesture();tool=value;hideFlyout();update(); }
for (const button of document.querySelectorAll('[data-tool]')) button.onclick=()=>selectTool(button.dataset.tool);
canvas.addEventListener('pointerdown',e=>{
  if (e.button!==0&&e.button!==1) return;
  if (gesture) return;
  hideFlyout();const p=point(e);
  if (tool==='Hand'||e.button===1||board.readOnly) { fitView=false;gesture={id:e.pointerId,pan:true,start:local(e),view:{...view}};canvas.setPointerCapture(e.pointerId);return; }
  fitView=false;
  if (tool==='Text') { textDialog(p);return; }
  gesture={id:e.pointerId,before:clone(board),last:p};canvas.setPointerCapture(e.pointerId);
  if (tool==='Eraser') erase(p);
  else preview={id:crypto.randomUUID(),tool,color,width:width*(tool==='Highlighter'?4:1),points:[p],text:'',fontSize:textSize};
  requestRender();
});
canvas.addEventListener('pointermove',e=>{
  if (!gesture||gesture.id!==e.pointerId) return;
  if (gesture.pan) { const p=local(e);view.x=gesture.view.x+p.x-gesture.start.x;view.y=gesture.view.y+p.y-gesture.start.y;requestRender();return; }
  let p=point(e);
  if (tool==='Eraser') { const previous=gesture.last;const steps=Math.max(1,Math.ceil(Math.hypot(p.x-previous.x,p.y-previous.y)/Math.max(2,width)));for(let i=1;i<=steps;i++) erase({x:previous.x+(p.x-previous.x)*i/steps,y:previous.y+(p.y-previous.y)*i/steps});gesture.last=p; }
  else if (preview) {
    if (tool==='Pen'||tool==='Highlighter') { const coalesced=e.getCoalescedEvents?.(); const events=coalesced?.length?coalesced:[e];for(const move of events) { const q=point(move),last=preview.points.at(-1);if(preview.points.length<20000&&Math.hypot(q.x-last.x,q.y-last.y)>.65)preview.points.push(q); } }
    else { if(e.shiftKey)p=constrain(preview.points[0],p,tool);preview.points[1]={x:Math.max(-100000,Math.min(100000,p.x)),y:Math.max(-100000,Math.min(100000,p.y))}; }
  }
  requestRender();
});
function erase(p) { board.marks=board.marks.filter(mark=>!hit(mark,p,Math.max(8,width*2))); }
canvas.addEventListener('pointerup',e=>{
  if (!gesture||gesture.id!==e.pointerId) return;
  const current=gesture;gesture=null;if(canvas.hasPointerCapture(e.pointerId))canvas.releasePointerCapture(e.pointerId);
  if (current.pan) return;
  if (preview && board.marks.length<5000) board.marks.push(preview);preview=null;commit(current.before);
});
function cancelGesture() { if(!gesture)return;const current=gesture;gesture=null;if(!current.pan)board=current.before;preview=null;if(canvas.hasPointerCapture(current.id))canvas.releasePointerCapture(current.id);requestRender(); }
canvas.addEventListener('pointercancel',cancelGesture);canvas.addEventListener('lostpointercapture',cancelGesture);
canvas.addEventListener('wheel',e=>{e.preventDefault();zoom(Math.exp(-e.deltaY*.001),local(e));},{passive:false});
canvas.addEventListener('contextmenu',e=>{e.preventDefault();selectTool('Hand');});
$('fit').onclick=fit;$('zoom-in').onclick=()=>zoom(1.2);$('zoom-out').onclick=()=>zoom(1/1.2);
$('undo').onclick=()=>{if(board.readOnly)return;board=history.undo(board);saveLocal();update();};
$('redo').onclick=()=>{if(board.readOnly)return;board=history.redo(board);saveLocal();update();};
$('clear').onclick=()=>{if(board.readOnly)return;const before=clone(board);board.marks=[];commit(before);};
$('board').onclick=()=>{board.background=board.background==='white'?'black':'white';if(board.background==='black'&&color==='#111111')color='#FFFFFF';else if(board.background==='white'&&color==='#FFFFFF')color='#111111';saveLocal();update();};
$('title').onchange=()=>{board.title=$('title').value.trim()||'Новое полотно';saveLocal();};
function flyout(contents,offset=130) { $('flyout').replaceChildren(...contents);$('flyout').hidden=false;$('flyout').style.top=`${offset}px`; }
function hideFlyout() {$('flyout').hidden=true;}
function button(label,callback,iconName=null) {const b=document.createElement('button');b.title=label;b.setAttribute('aria-label',label);if(iconName)b.innerHTML=icon(iconName);else b.textContent=label;b.onclick=callback;return b;}
$('shapes').onclick=()=>{const row=document.createElement('div');row.className='fly-row';for(const [name,label]of[['Line','Линия'],['Arrow','Стрелка'],['Rectangle','Прямоугольник'],['Ellipse','Эллипс']])row.append(button(label,()=>{selectTool(name);$('shapes').innerHTML=icon(name);},name));flyout([row],150);};
$('size').onclick=()=>{const row=document.createElement('div');row.className='fly-row';for(const size of[2,4,8,12,20]){const b=button(`${size} px`,()=>{width=size;hideFlyout();});const dot=document.createElement('span');dot.className='size-dot';dot.style.width=dot.style.height=`${size}px`;b.replaceChildren(dot);row.append(b);}flyout([row],180);};
function colorButton(c){const b=button(c,()=>{color=c;hideFlyout();update();});b.replaceChildren();b.style.backgroundColor=c;return b;}
for(const c of['#00C9ED','#FFF400','#FF0099','#111111'])$('quick-colors').append(colorButton(c));
$('palette').onclick=()=>{const grid=document.createElement('div');grid.className='swatches';for(const c of colors)grid.append(colorButton(c));const custom=document.createElement('input');custom.type='color';custom.value=color.length===7?color:'#111111';custom.setAttribute('aria-label','Свой цвет');custom.oninput=()=>{color=custom.value;update();};flyout([grid,custom],220);};
document.addEventListener('pointerdown',e=>{if(!e.target.closest('.toolkit'))hideFlyout();});
function showDialog(title,description=''){const content=$('dialog-content');content.replaceChildren();const h=document.createElement('h2');h.textContent=title;content.append(h);if(description){const p=document.createElement('p');p.textContent=description;content.append(p);}if(!$('dialog').open)$('dialog').showModal();return content;}
$('dialog-close').onclick=()=>$('dialog').close();
function textDialog(p){const content=showDialog('Текст');const input=document.createElement('textarea');input.maxLength=4000;input.placeholder='Напишите здесь…';input.setAttribute('aria-label','Текст на полотне');content.append(input);const apply=button('Добавить',()=>{if(input.value.trim()){const before=clone(board);board.marks.push({id:crypto.randomUUID(),tool:'Text',color,width:1,points:[p],text:input.value,fontSize:textSize});commit(before);}$('dialog').close();});apply.className='primary';content.append(apply);input.onkeydown=e=>{if(e.key==='Enter'&&!e.shiftKey){e.preventDefault();apply.click();}};input.focus();}
async function exportPng(){const snapshot=clone(board);await Promise.all(snapshot.marks.filter(m=>m.tool==='Image').map(m=>raster(m.image)?.decode().catch(()=>{})));const out=document.createElement('canvas'),bounds=boardBounds(snapshot,measureText),size=exportSize(bounds);out.width=size.width;out.height=size.height;const context=out.getContext('2d');context.fillStyle=snapshot.background==='black'?'#111111':'white';context.fillRect(0,0,out.width,out.height);context.scale(size.scale,size.scale);context.translate(-bounds.left,-bounds.top);snapshot.marks.forEach(m=>drawMark(context,m));out.toBlob(blob=>download(blob,`${safeName(snapshot.title)}.png`));}
function safeName(name){return name.replace(/[\\/:*?"<>|]/g,'_').slice(0,80)||'полотно';}
function download(blob,name){const url=URL.createObjectURL(blob),a=document.createElement('a');a.href=url;a.download=name;a.click();setTimeout(()=>URL.revokeObjectURL(url),1000);}
$('export').onclick=exportPng;
$('menu').onclick=()=>{const items=[];for(const[label,callback]of[['Скачать PNG',exportPng],['Скачать рисунок (.json)',()=>download(new Blob([JSON.stringify(board)],{type:'application/json'}),`${safeName(board.title)}.json`)],['Открыть рисунок (.json)',importJson],['Горячие клавиши',()=>showDialog('Горячие клавиши','P — перо · M — маркер · E — ластик · T — текст · H — перемещение · Ctrl+Z — отменить · Ctrl+Shift+Z — повторить. Shift делает фигуры ровными. Колесо мыши меняет масштаб.')]]){const b=button(label,()=>{hideFlyout();callback();});b.className='menu-item';items.push(b);}const label=document.createElement('div');label.className='fly-label';label.textContent=`Размер текста: ${textSize}`;const size=document.createElement('input');size.type='range';size.min=14;size.max=72;size.value=textSize;size.setAttribute('aria-label','Размер текста');size.oninput=()=>{textSize=Number(size.value);label.textContent=`Размер текста: ${textSize}`;};items.push(label,size);flyout(items,100);};
function importJson(){if(board.readOnly)return;const input=document.createElement('input');input.type='file';input.accept='.json,application/json';input.onchange=async()=>{try{const data=JSON.parse(await input.files[0].text());if(!validBoard(data)||!data.marks.every(m=>tools.includes(m.tool)&&Array.isArray(m.points)&&m.points.length&&m.points.every(p=>Number.isFinite(p.x)&&Number.isFinite(p.y))))throw new Error();const before=clone(board);board.marks=data.marks.map(m=>({...m,id:crypto.randomUUID()}));board.background=data.background==='black'?'black':'white';commit(before);}catch{showDialog('Не удалось открыть рисунок','Выберите JSON-файл, сохранённый Kiiko Pen или BlurMask.');}};input.click();}
$('screen').onclick=()=>showDialog('Рисовать на экране','Для рисования поверх других программ откройте BlurMask и выберите «Экран» на панели. Здесь находится отдельное полотно: его можно открыть в BlurMask через ссылку «Поделиться».');
$('new').onclick=async()=>{await sync();if(hasChanges(diff(baseline,board))&&access){showDialog('Изменения ещё не сохранены','Соединение с сервером недоступно. Скачайте рисунок через меню перед созданием нового полотна.');return;}generation++;board=emptyBoard();baseline=emptyBoard();access=null;pending=null;invalidInvite=false;history=new History();location.hash='';saveLocal();fit();await sync();};
$('connect').onclick=()=>{const content=showDialog('Открыть общее полотно','Вставьте код приглашения или ссылку. Полотно откроется в новой вкладке.');const input=document.createElement('input');input.placeholder='Код приглашения';input.setAttribute('aria-label','Код приглашения или ссылка');const error=document.createElement('p');error.setAttribute('role','alert');const open=button('Подключиться',()=>{try{const connection=parseInvite(input.value);window.open(`${location.origin}/#${inviteCode(connection)}`,'_blank','noopener');$('dialog').close();}catch(e){error.textContent=e.message;}});content.append(input,open,error);input.focus();};
$('share').onclick=async()=>{await sync();if(!access){showDialog('Приглашение пока недоступно','Полотно сохранено на этом устройстве. Для приглашения нужно подключение к серверу.');return;}const content=showDialog('Поделиться полотном','Отправьте код приглашения: получатель вставит его в «Подключить» в браузере или BlurMask. Можно также скопировать ссылку для браузера.');addLink(content,board.readOnly?'Только просмотр':'Можно рисовать',access);if(access.viewKey&&!board.readOnly)addLink(content,'Только просмотр',{...access,key:access.viewKey});};
function addLink(content,label,connection){const code=inviteCode(connection);const text=document.createElement('label');text.textContent=label;const input=document.createElement('input');input.readOnly=true;input.value=code;input.setAttribute('aria-label',label);input.onclick=()=>input.select();const copy=button('Копировать код',async()=>{try{await navigator.clipboard.writeText(code);copy.textContent='Скопировано';}catch{input.select();}});copy.className='primary';const copyLink=button('Копировать ссылку',async()=>{try{await navigator.clipboard.writeText(`${location.origin}/#${code}`);copyLink.textContent='Скопировано';}catch{copyLink.textContent='Не удалось скопировать';}});content.append(text,input,copy,copyLink);}
document.addEventListener('keydown',e=>{if(e.target.matches('input,textarea')||$('dialog').open)return;if(e.ctrlKey||e.metaKey){if(e.key.toLowerCase()==='z'){e.preventDefault();$(e.shiftKey?'redo':'undo').click();}return;}if(e.key==='Escape'){cancelGesture();hideFlyout();selectTool('Hand');}const map={p:'Pen',m:'Highlighter',e:'Eraser',t:'Text',h:'Hand'};if(map[e.key.toLowerCase()]&&!board.readOnly)selectTool(map[e.key.toLowerCase()]);});
let grip=null;const toolkit=document.querySelector('.toolkit');$('grip').onpointerdown=e=>{const rect=toolkit.getBoundingClientRect();grip={id:e.pointerId,x:e.clientX-rect.left,y:e.clientY-rect.top};$('grip').setPointerCapture(e.pointerId);};$('grip').onpointermove=e=>{if(grip?.id!==e.pointerId)return;toolkit.style.transform='none';toolkit.style.left=`${Math.max(0,Math.min(innerWidth-40,e.clientX-grip.x))}px`;toolkit.style.top=`${Math.max(64,Math.min(innerHeight-toolkit.offsetHeight,e.clientY-grip.y))}px`;};$('grip').onpointerup=()=>{grip=null;};
async function api(path,options={}){const response=await fetch(`/api/boards${path}`,{...options,headers:{'Content-Type':'application/json',...(access?{Authorization:`Bearer ${access.key}`} : {}),...options.headers},signal:AbortSignal.timeout(8000)});if(!response.ok){const error=new Error(response.status===403?'Только просмотр':response.status===404?'Ссылка недоступна':response.status===413?'Полотно слишком большое':'Сервер недоступен');error.status=response.status;throw error;}return response.json();}
async function sync(){
  if(busy||gesture||preview||invalidInvite)return;
  if(deferred){board=rebase(deferred.remote,deferred.before,board);baseline=clone(deferred.remote);deferred=null;saveLocal();update();}
  busy=true;const currentGeneration=generation;
  try{
    if(!access){const created=await api('',{method:'POST',body:JSON.stringify({title:board.title,background:board.background})});if(generation!==currentGeneration)return;access={id:created.board.id,key:created.editKey,viewKey:created.viewKey};baseline=created.board;location.hash=inviteCode(access);saveLocal();}
    if(hashConnection&&baseline.id!==access.id){const remote=await api(`/${access.id}`);if(generation!==currentGeneration)return;board=remote;baseline=clone(remote);saveLocal();update();}
    const localBefore=pending?.before??clone(board);const patch=pending?.patch??diff(baseline,localBefore);
    let remote;
    if(!board.readOnly&&hasChanges(patch)){pending={patch,before:localBefore};saveLocal();remote=await api(`/${access.id}/changes`,{method:'POST',body:JSON.stringify(patch)});pending=null;}
    else remote=await api(`/${access.id}`);
    if(generation!==currentGeneration)return;
    if(!gesture&&!preview){board=rebase(remote,localBefore,board);baseline=clone(remote);saveLocal();update();}
    else deferred={remote,before:localBefore};
    status(board.readOnly?'Только просмотр':hasChanges(diff(baseline,board))?'Сохранение…':'Сохранено');
  }catch(error){if(error.status===403){board.readOnly=true;pending=null;update();}status(error.status===404?'Ссылка недоступна':error.status===413?'Полотно слишком большое':'Локально · нет соединения',true);}
  finally{busy=false;}
}
addEventListener('hashchange',()=>{try{const connection=parseInvite(location.hash);if(connection.id!==access?.id||connection.key!==access?.key)location.reload();}catch{if(location.hash)location.reload();}});
addEventListener('beforeunload',saveLocal);addEventListener('online',sync);new ResizeObserver(()=>{if(fitView)fit();else requestRender();}).observe(canvas);
if(hashConnection)window.history.replaceState(null,'',`/#${inviteCode(hashConnection)}`);
fit();update();sync();setInterval(sync,1300);
if(invalidInvite){status('Некорректное приглашение',true);showDialog('Не удалось открыть полотно','Проверьте код приглашения или вставьте его в «Подключить».');}
