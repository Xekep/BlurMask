"""Independent complete raw215 storage oracle; no recognition or vendor execution."""
import argparse
import base64
import hashlib
import importlib.util
import json
from pathlib import Path
import struct

p = argparse.ArgumentParser()
p.add_argument('resource', type=Path)
p.add_argument('capture', type=Path)
p.add_argument('--reference', required=True, type=Path)
a = p.parse_args()
assert hashlib.sha256(a.reference.read_bytes()).hexdigest() == '375b2a56c76434428feb61f3265f731ae38dbd47be8a6ff957915d80d6766064', 'Frozen independent reference changed'
spec = importlib.util.spec_from_file_location('raw215_reference', a.reference)
ref = importlib.util.module_from_spec(spec)
import sys
sys.modules[spec.name] = ref
spec.loader.exec_module(ref)
data = a.resource.read_bytes()
raw = ref.load_pinned_resource(data)
c = json.loads(a.capture.read_text(encoding='utf-8'))
assert c['vendorExecution'] is False and c['inference'] is False
assert c['resource'] == hashlib.sha256(data).hexdigest() == ref.RESOURCE_SHA
assert len(c['models']) == len(raw) == 6
whole = hashlib.sha256()
events = [f'Capture:-1:{x}' for x in range(0, len(data), 16384)]
events += [f'Preflight:{i}:0' for i in range(6)]
events += [f'Hash:-1:{x}' for x in range(0, len(data), 16384)]
checks = ['actual-six']
for i, ((role, model), actual) in enumerate(zip(raw, c['models'])):
    m1, m2 = model.matrices
    parent = (7465, 177763) if i == 0 else (1005980, 1789568)
    expected = dict(start=model.origin, end=model.origin+len(ref.write_envelope(model)),
                    kind=model.kind, inputs=model.inputs, firstStride=m1.stride,
                    secondStride=m2.stride, parentStart=parent[0], parentEnd=parent[1])
    assert {k: actual[k] for k in expected} == expected
    for name, value in [('first', m1.weights), ('second', m2.weights), ('scales', m2.scales),
                        ('firstAlignment', m1.alignment), ('secondAlignment', m2.alignment),
                        ('envelope', ref.write_envelope(model))]:
        assert base64.b64decode(actual[name], validate=True) == value, (i, name)
    # Decode all signed values independently, including every padded column.
    assert struct.unpack('<'+'h'*(len(m1.weights)//2), base64.b64decode(actual['first'])) == struct.unpack('<'+'h'*(len(m1.weights)//2), m1.weights)
    assert struct.unpack('<'+'b'*len(m2.weights), base64.b64decode(actual['second'])) == struct.unpack('<'+'b'*len(m2.weights), m2.weights)
    whole.update(struct.pack('<8i', *expected.values()))
    for value in (m1.weights, m2.weights, m2.scales): whole.update(value)
    events += [f'FirstWeights:{i}:{x}' for x in range(0, len(m1.weights)//2, 2048)]
    events += [f'SecondWeights:{i}:{x}' for x in range(0, len(m2.weights), 2048)]
    events += [f'Scales:{i}:0']
    checks += [f'raw-domain-{i}', f'complete-envelope-{i}', f'detached-mutations-{i}',
               f'snapshot-first-cancel-{i}:OperationCanceledException', f'snapshot-envelope-cancel-{i}:OperationCanceledException']
events += ['Publish:-1:0']
assert c['boundaries'] == events and c['boundaryCount'] == len(events) == 777
assert c['completeDigest'] == whole.hexdigest()
checks += ['empty:ArgumentException', 'short-source:ArgumentException', 'entry-cancel:OperationCanceledException']
def malformed(name, error='InvalidDataException'):
    checks.extend([name+'-preflight-before-payload', name+':'+error])
malformed('outer-frame'); malformed('base-version','NotSupportedException'); malformed('missing-configured-model')
for i in range(6):
    for name, error in [('magic','InvalidDataException'), ('version','InvalidDataException'),
        ('envelope-end','InvalidDataException'), ('old-architecture','NotSupportedException'), ('foreign-kind','InvalidDataException')]:
        malformed(f'{name}-{i}', error)
    for field in (11,15,27):
        for value in (0,1,4097,4294967295): malformed(f'width-{i}-{field}-{value}')
    for field in (19,23): malformed(f'middle-{i}-{field}', 'NotSupportedException')
    malformed(f'quantizer-{i}','NotSupportedException')
    for name in ('leading-scale','zero-final-scale','foreign-weight-fingerprint'): malformed(f'{name}-{i}')
for stop in range(1,len(events)+1):
    checks += [f'cancel-prefix-{stop}:OperationCanceledException', f'cancel-stopped-{stop}',
               f'throw-prefix-{stop}:InjectedException', f'throw-stopped-{stop}']
checks += ['original-mutated-after-capture','captured-source-isolation','diagnostic-reentry']
for i in range(3):
    checks += [f'parallel-copy-dispose-{i}', f'closed-model-{i}:ObjectDisposedException', f'closed-root-{i}:ObjectDisposedException']
checks += ['snapshot-does-not-retain-root','snapshot-after-root-collection','historical-after-dispose']
assert c['checks'] == checks and len(checks) == 3439
print(json.dumps(dict(result='PASS', models=6, checks=len(checks), cancellationAndThrowPrefixes=len(events),
    completeDigest=whole.hexdigest(), resourceSha256=ref.RESOURCE_SHA,
    referenceSha256=hashlib.sha256(a.reference.read_bytes()).hexdigest(),
    captureSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),
    allTypedWeightsAndPadding=True, inference=False), indent=2))
