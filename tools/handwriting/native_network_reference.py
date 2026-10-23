# Parsing only; no model forward. Extracted from the existing research reference.
"""Independent CPU reader/reference for native type5f/version106 networks.

Observed kind1 input/output, kind3 LSTM, kind8 BLSTM, flags0, storage1/2/3
are supported. No vendor DLL is loaded. Parameters remain local vendor data.
"""
from pathlib import Path
import hashlib
import json
import numpy as np
ROOT = Path(__file__).resolve().parents[2]


def align(n, multiple=8):
    return (n + multiple - 1) // multiple * multiple


class PackedReader:
    def __init__(self, payload):
        self.data, self.pos, self.inventory = payload, 0, []

    def array(self, count, dtype):
        dt = np.dtype(dtype)
        end = self.pos + count * dt.itemsize
        if end > len(self.data):
            raise ValueError('Truncated packed tensor')
        out = np.frombuffer(self.data, dtype=dt, count=count, offset=self.pos).copy()
        self.pos = end
        return out

    def matrix(self, name, rows, input_width, code):
        if code not in (1,2,3):
            raise ValueError('Unsupported storage code')
        dtype = {1:'<f4',2:'<i2',3:'i1'}[code]
        cols = align(align(input_width + 1), {1:8,2:16,3:32}[code])
        start = self.pos
        weights = self.array(rows * cols, dtype).reshape(rows, cols)
        metadata = self.array(align(rows) * 2, '<f4').reshape(align(rows), 2) if code!=1 else np.tile(np.array([1,0],np.float32),(rows,1))
        aux = self.array(8, '<f4') if code!=1 else np.zeros(8,np.float32)
        if not np.isfinite(weights).all():raise ValueError('Nonfinite weights')
        if not np.isfinite(metadata).all() or not np.isfinite(aux).all():
            raise ValueError('Nonfinite metadata')
        if not (metadata[:rows, 0] >= 0).all():
            raise ValueError(f'Negative row scales in {name}: {metadata[:rows, 0].min()}')
        if np.any(weights[:, input_width + 1:]):
            raise ValueError('Nonzero column padding')
        row_sums = weights.astype(np.int64).sum(axis=1)
        info = dict(name=name, offset=start, bytes=self.pos-start, rows=rows,
                    stored_columns=cols, input_width=input_width, storage_code=code,
                    metadata_is_stored=code!=1,
                    aux=aux.tolist(), scale_min=float(metadata[:rows, 0].min()),
                    scale_max=float(metadata[:rows, 0].max()),
                    row_sum_metadata_matches=bool(np.array_equal(row_sums, metadata[:rows, 1])))
        self.inventory.append(info)
        return dict(weights=weights, metadata=metadata[:rows], aux=aux,
                    width=input_width, code=code, name=name)

    def group(self, name, width, input_width, code, recurrent=False):
        result = {gate: self.matrix(name + '.' + gate, width, input_width, code)
                  for gate in ('g', 'i', 'f', 'o')}
        if recurrent:
            for gate in ('i', 'f', 'o'):
                start = self.pos
                vector = self.array(align(width), '<f4')
                if not np.isfinite(vector).all() or np.any(vector[width:]):
                    raise ValueError('Invalid peephole vector/padding')
                result['p' + gate] = vector[:width]
                self.inventory.append(dict(name=name+'.p'+gate, offset=start,
                                           bytes=self.pos-start, shape=[width]))
        return result


def load_network(record):
    path = ROOT / record['file']
    full = path.read_bytes()
    p = record['parameter_payload']
    payload = full[p['offset']:p['offset']+p['bytes']]
    if hashlib.sha256(payload).hexdigest() != p['sha256']:
        raise ValueError('Parameter payload checksum mismatch')
    if len(record['networks']) != 1 or record['networks'][0]['edge_records']:
        raise ValueError('Only observed sequential network supported')
    layers = record['networks'][0]['stored_layers']
    if layers[0]['kind'] != 1 or record['output_layer']['kind'] != 1:
        raise ValueError('Unsupported input/output kind')
    if record['per_layer_codes'] != [0]+[1]*(len(layers)-1)+[3] or record['global_code'] != 2:
        raise ValueError('Unsupported activation codes')
    reader, parsed = PackedReader(payload), []
    previous = layers[0]['width']
    for index, layer in enumerate(layers[1:], 1):
        if layer['kind'] not in (3,8) or layer['opaque_u32']:
            raise ValueError('Only flag0 LSTM/BLSTM layers supported')
        h, code = layer['width'], layers[index-1]['storage_code']
        cell = dict(width=h,bidirectional=layer['kind']==8)
        directions=('forward','backward') if cell['bidirectional'] else ('forward',)
        for direction in directions:
            cell[direction+'_input'] = reader.group(f'layer{index}.{direction}.input', h, previous, code)
        for direction in directions:
            cell[direction+'_recurrent'] = reader.group(f'layer{index}.{direction}.recurrent', h, h, code, True)
        parsed.append(cell)
        previous = (2 if cell['bidirectional'] else 1)*h
    output = reader.matrix('output', record['output_layer']['width'], previous,
                           layers[-1]['storage_code'])
    lookup=None
    if record['extra_rows'] or record['extra_columns']:
        rows,cols=record['extra_rows'],record['extra_columns']
        if not rows or not cols:raise ValueError('Invalid extra lookup dimensions')
        start=reader.pos
        stored_cols=align(cols)
        lookup=reader.array(rows*stored_cols,'<f4').reshape(rows,stored_cols)
        if not np.isfinite(lookup).all() or np.any(lookup[:,cols:]):raise ValueError('Invalid lookup/padding')
        reader.inventory.append(dict(name='input_lookup',offset=start,bytes=reader.pos-start,
                                     shape=[rows,cols],stored_columns=stored_cols))
    if reader.pos != len(payload):
        raise ValueError(f'Packed layout leaves {len(payload)-reader.pos} bytes')
    return dict(input_width=layers[0]['width'], layers=parsed, output=output,
                tensors=reader.inventory, payload_bytes=reader.pos,
                payload_sha256=p['sha256'],input_lookup=lookup)
