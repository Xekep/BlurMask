"""Independent NumPy float32/int32 reference for synthetic recognition tests.

Contains arithmetic only: no resource parser, model weights, asset reads or
vendor DLL calls. NumPy is a development dependency, never an app dependency.
"""
import numpy as np


def align(value, multiple=8):
    return (value + multiple - 1) // multiple * multiple


def affine(matrix, values, bias=1., quantized=True):
    """Win64 scalar kernel semantics, allowing float approximation for comparison."""
    w, meta = matrix['weights'], matrix['metadata']
    x = np.zeros(w.shape[1], dtype=np.float32)
    if len(values) != matrix['width'] or not np.isfinite(values).all():
        raise ValueError('Invalid affine input')
    x[:len(values)], x[len(values)] = values, bias
    if matrix['code']==1:
        return w @ x
    if not quantized:
        return (w.astype(np.float32) @ x) * meta[:, 0]
    if matrix['code'] == 2:
        limit = matrix['aux'][0]
        peak = np.max(np.abs(x))
        if peak == 0:
            return np.zeros(w.shape[0], np.float32)
        if not 0 < limit <= 32767:
            raise ValueError('Unsupported int16 input quantization limit')
        factor = np.float32(limit / peak)
        q = np.trunc(x * factor + np.where(x < 0, np.float32(-0.49999), np.float32(0.49999)))
        q = np.clip(q, -int(limit), int(limit)).astype(np.int64)
        # Eight pair accumulators (MOVSX/IMUL/ADD scalar implementation),
        # each wrapping int32 before sequential float32 conversion/addition.
        products = w.astype(np.int64) * q
        lanes = products.reshape(w.shape[0], -1, 8, 2).sum(axis=(1, 3)).astype(np.int32)
        dot = lanes[:, 0].astype(np.float32)
        for lane in range(1, 8):
            dot = (dot + lanes[:, lane].astype(np.float32)).astype(np.float32)
        return dot * np.float32(peak/limit) * meta[:, 0]
    activation = x[:align(matrix['width']+1)]
    lo, hi = activation.min(), activation.max()
    if hi <= lo:
        return np.zeros(w.shape[0], np.float32)
    factor = np.float32(127 / np.float32(hi-lo))
    q = np.clip(np.trunc((x-lo)*factor + np.float32(.49999)), 0, 127).astype(np.int64)
    q[align(matrix['width']+1):] = 0
    dot = (w.astype(np.int64) @ q).astype(np.int32).astype(np.float32)
    return (dot + np.float32(factor*lo)*meta[:, 1]) * (np.float32((hi-lo)/127) * meta[:, 0])


def sigmoid(x):
    return (1 / (1 + np.exp(-np.clip(x, -88, 88)))).astype(np.float32)


def direction_forward(features, input_group, recurrence, reverse=False, quantized=True):
    h = input_group['g']['weights'].shape[0]
    output = np.empty((len(features), h), np.float32)
    state, cell = np.zeros(h, np.float32), np.zeros(h, np.float32)
    order = range(len(features)-1, -1, -1) if reverse else range(len(features))
    first = True
    for t in order:
        gate = {g: affine(input_group[g], features[t], quantized=quantized)
                for g in ('g', 'i', 'f', 'o')}
        if not first:
            for g in gate:
                gate[g] += affine(recurrence[g], state, bias=0, quantized=quantized)
            gate['i'] += cell * recurrence['pi']
            gate['f'] += cell * recurrence['pf']
        candidate, ingate, forget = np.tanh(gate['g']), sigmoid(gate['i']), sigmoid(gate['f'])
        cell = (candidate*ingate + (0 if first else cell*forget)).astype(np.float32)
        gate['o'] += cell*recurrence['po']
        state = (np.tanh(cell)*sigmoid(gate['o'])).astype(np.float32)
        output[t] = state
        first = False
    return output


def infer(network, features, quantized=True):
    features = np.asarray(features, np.float32)
    if features.ndim != 2 or features.shape[1] != network['input_width'] or not len(features):
        raise ValueError('Expected nonempty Txinput_width feature matrix')
    if not np.isfinite(features).all():
        raise ValueError('Nonfinite features')
    states = []
    for layer in network['layers']:
        forward = direction_forward(features, layer['forward_input'], layer['forward_recurrent'], quantized=quantized)
        if layer['bidirectional']:
            backward = direction_forward(features, layer['backward_input'], layer['backward_recurrent'], True, quantized)
            features = np.concatenate((forward, backward), axis=1)
        else:features=forward
        states.append(features)
    logits = np.array([affine(network['output'], x, quantized=quantized) for x in features])
    exp = np.exp(logits-logits.max(axis=1, keepdims=True))
    return exp/exp.sum(axis=1, keepdims=True), states
