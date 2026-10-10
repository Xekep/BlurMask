export const tools = ['Pen', 'Highlighter', 'Text', 'Line', 'Arrow', 'Rectangle', 'Ellipse'];
export const emptyBoard = () => ({ id: null, revision: 0, title: 'Новое полотно', background: 'white', width: 2400, height: 1600, marks: [], readOnly: false });
export const clone = value => structuredClone(value);
export function validBoard(board) {
  if (!board || !Number.isInteger(board.width) || board.width<100 || board.width>10000 ||
      !Number.isInteger(board.height) || board.height<100 || board.height>10000 ||
      typeof board.title!=='string' || !board.title.trim() || board.title.length>120 ||
      !['white','black'].includes(board.background) || !Array.isArray(board.marks) || board.marks.length>5000) return false;
  let points=0;const ids=new Set();
  return board.marks.every(m=>{
    if (!m || typeof m.id!=='string' || !/^[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}$/i.test(m.id) || ids.has(m.id) ||
        !tools.includes(m.tool) || typeof m.color!=='string' || !/^#[0-9a-f]{6}([0-9a-f]{2})?$/i.test(m.color) ||
        !Number.isFinite(m.width) || m.width<.5 || m.width>200 || !Number.isFinite(m.fontSize) || m.fontSize<8 || m.fontSize>300 ||
        typeof m.text!=='string' || m.text.length>4000 || !Array.isArray(m.points) || !m.points.length || m.points.length>20000) return false;
    ids.add(m.id);points+=m.points.length;
    return points<=200000 && m.points.every(p=>p&&Number.isFinite(p.x)&&Number.isFinite(p.y)&&Math.abs(p.x)<=100000&&Math.abs(p.y)<=100000);
  });
}

export function diff(before, after) {
  const old = new Set(before.marks.map(m => m.id));
  const current = new Set(after.marks.map(m => m.id));
  return { operationId: crypto.randomUUID(), add: after.marks.filter(m => !old.has(m.id)),
    remove: before.marks.filter(m => !current.has(m.id)).map(m => m.id),
    title: before.title === after.title ? null : after.title,
    background: before.background === after.background ? null : after.background };
}
export const hasChanges = patch => patch.add.length || patch.remove.length || patch.title !== null || patch.background !== null;
export function rebase(remote, before, current) {
  const patch = diff(before, current);
  const removed = new Set(patch.remove);
  const marks = remote.marks.filter(m => !removed.has(m.id));
  const ids = new Set(marks.map(m => m.id));
  for (const mark of patch.add) if (!ids.has(mark.id)) { marks.push(mark); ids.add(mark.id); }
  return { ...remote, marks, title: patch.title ?? remote.title, background: patch.background ?? remote.background };
}
export class History {
  undoStack = []; redoStack = [];
  push(before, after) {
    const patch = diff(before, after);
    if (!hasChanges(patch)) return;
    this.undoStack.push({ added: patch.add, removed: before.marks.map((mark, index) => ({ mark, index })).filter(x => patch.remove.includes(x.mark.id)) });
    if (this.undoStack.length > 100) this.undoStack.shift();
    this.redoStack = [];
  }
  undo(board) { const edit = this.undoStack.pop(); if (!edit) return board; this.redoStack.push(edit); return this.apply(board, edit, true); }
  redo(board) { const edit = this.redoStack.pop(); if (!edit) return board; this.undoStack.push(edit); return this.apply(board, edit, false); }
  apply(board, edit, reverse) {
    const remove = new Set((reverse ? edit.added : edit.removed.map(x => x.mark)).map(m => m.id));
    const marks = board.marks.filter(m => !remove.has(m.id));
    const insert = reverse ? edit.removed : edit.added.map((mark, i) => ({ mark, index: marks.length + i }));
    for (const item of insert) if (!marks.some(m => m.id === item.mark.id)) marks.splice(Math.min(item.index, marks.length), 0, item.mark);
    return { ...board, marks };
  }
}
function distance(point, a, b) {
  const dx = b.x - a.x, dy = b.y - a.y, square = dx * dx + dy * dy;
  const t = square ? Math.max(0, Math.min(1, ((point.x - a.x) * dx + (point.y - a.y) * dy) / square)) : 0;
  return Math.hypot(point.x - a.x - dx * t, point.y - a.y - dy * t);
}
export function hit(mark, p, radius) {
  radius += mark.width / 2;
  const a = mark.points[0], b = mark.points.at(-1);
  const left = Math.min(a.x, b.x), right = Math.max(a.x, b.x), top = Math.min(a.y, b.y), bottom = Math.max(a.y, b.y);
  if (mark.tool === 'Text') return p.x >= a.x - radius && p.x <= a.x + Math.max(...mark.text.split('\n').map(s => s.length)) * mark.fontSize * .7 + radius &&
    p.y >= a.y - radius && p.y <= a.y + mark.text.split('\n').length * mark.fontSize * 1.2 + radius;
  if (mark.tool === 'Rectangle') return p.x >= left - radius && p.x <= right + radius && p.y >= top - radius && p.y <= bottom + radius &&
    (right - left <= radius * 2 || bottom - top <= radius * 2 || p.x <= left + radius || p.x >= right - radius || p.y <= top + radius || p.y >= bottom - radius);
  if (mark.tool === 'Ellipse') {
    const rx = (right - left) / 2, ry = (bottom - top) / 2, cx = (left + right) / 2, cy = (top + bottom) / 2;
    if (rx < 1 || ry < 1) return distance(p, a, b) <= radius;
    const angle = Math.atan2((p.y - cy) / ry, (p.x - cx) / rx);
    return Math.hypot(p.x - cx - rx * Math.cos(angle), p.y - cy - ry * Math.sin(angle)) <= radius;
  }
  if (mark.tool === 'Arrow') {
    const angle = Math.atan2(b.y - a.y, b.x - a.x), length = Math.max(14, mark.width * 3.5);
    if ([-.5, .5].some(offset => distance(p, b, { x: b.x - Math.cos(angle + offset) * length, y: b.y - Math.sin(angle + offset) * length }) <= radius)) return true;
  }
  return mark.points.length === 1 ? Math.hypot(p.x - a.x, p.y - a.y) <= radius : mark.points.slice(1).some((b, i) => distance(p, mark.points[i], b) <= radius);
}
export function constrain(a, b, tool) {
  const dx = b.x - a.x, dy = b.y - a.y;
  if (tool === 'Rectangle' || tool === 'Ellipse') {
    const side = Math.max(Math.abs(dx), Math.abs(dy));
    return { x: a.x + side * (dx < 0 ? -1 : 1), y: a.y + side * (dy < 0 ? -1 : 1) };
  }
  const angle = Math.round(Math.atan2(dy, dx) / (Math.PI / 4)) * Math.PI / 4, length = Math.hypot(dx, dy);
  return { x: a.x + Math.cos(angle) * length, y: a.y + Math.sin(angle) * length };
}
export function drawMark(ctx, mark) {
  const a = mark.points[0], b = mark.points.at(-1);
  ctx.save(); ctx.strokeStyle = mark.color; ctx.fillStyle = mark.color; ctx.lineWidth = mark.width;
  ctx.lineCap = 'round'; ctx.lineJoin = 'round'; ctx.globalAlpha = mark.tool === 'Highlighter' ? .35 : 1;
  ctx.beginPath();
  if (mark.tool === 'Text') {
    ctx.font = `${mark.fontSize}px sans-serif`; ctx.textBaseline = 'top';
    mark.text.split('\n').forEach((line, i) => ctx.fillText(line, a.x, a.y + i * mark.fontSize * 1.2));
  } else if (mark.tool === 'Rectangle') ctx.rect(Math.min(a.x, b.x), Math.min(a.y, b.y), Math.abs(b.x - a.x), Math.abs(b.y - a.y));
  else if (mark.tool === 'Ellipse') ctx.ellipse((a.x + b.x) / 2, (a.y + b.y) / 2, Math.abs(b.x - a.x) / 2, Math.abs(b.y - a.y) / 2, 0, 0, Math.PI * 2);
  else {
    ctx.moveTo(a.x, a.y); mark.points.slice(1).forEach(p => ctx.lineTo(p.x, p.y));
    if (mark.tool === 'Arrow') {
      const angle = Math.atan2(b.y - a.y, b.x - a.x), length = Math.max(14, mark.width * 3.5);
      for (const offset of [-.5, .5]) { ctx.moveTo(b.x, b.y); ctx.lineTo(b.x - Math.cos(angle + offset) * length, b.y - Math.sin(angle + offset) * length); }
    }
    if (mark.points.length === 1) { ctx.arc(a.x, a.y, mark.width / 2, 0, Math.PI * 2); ctx.fill(); }
  }
  ctx.stroke(); ctx.restore();
}
