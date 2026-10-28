"""Frozen Document032800 branch/call-prefix controls, independent of C#.

Expected decisions follow the native branch addresses in the retained review.
The owner/range/name objects in the eventual probe are explicitly synthetic.
"""
import json
import struct
import sys
from pathlib import Path


def value(tag=4, number=41, text=None, terminated=True):
    if text is not None:
        heap = text.encode('ascii') + (b'\0' if terminated else b'')
        lane = bytearray(16)
        struct.pack_into('<I', lane, 8, len(text))
        return dict(tag=1, storage=lane.hex(), heap=heap.hex())
    payload = struct.pack('<d', number) if tag == 5 else struct.pack('<q', number)
    return dict(tag=tag, storage=payload.hex(), heap=None)


def pending(identifier=2, name='MATH_SYMBOL', selection=1):
    return dict(id=identifier, name=name, selection=selection)


rows = []
compare = ['compare:tag:tag']
converted = compare + ['ConvertOld', 'ConvertNew']
parsed = converted + ['ParseOld', 'ParseNew']


def add(label, result, trace, **changes):
    row = dict(label=label, result=result, trace=trace, operation=7, envelope=0,
               name10='tag', name30='wrong', old=value(), new=value(number=-2),
               pending=[pending()], hasRange=True, hasName=True, oldName='MATH_SYMBOL',
               rangeCount=2, throws=False)
    row.update(changes)
    rows.append(row)


def lookup(identifier=41):
    return parsed + [f'range:{identifier}', f'name:{identifier}', 'compare:MATH_SYMBOL:MATH_SYMBOL']


add('wrong operation', False, [], operation=6)
add('unrecognized envelope', False, [], envelope=2)
add('missing envelope name', False, [], name10=None)
add('wrong name', False, ['compare:range:tag'], name10='range')
add('old parse fails after BOTH conversions', False, converted + ['ParseOld'], old=value(text='bad'))
add('new parse fails', False, parsed, new=value(text='bad'))
for new_id in (0, 2):
    add('new must be negative ' + str(new_id), False, parsed, new=value(number=new_id))
add('unmatched pending ID', False, parsed, new=value(number=-99))
add('empty pending list', False, parsed, pending=[])
add('no old range', False, parsed + ['range:41'], hasRange=False)
add('no old name', False, parsed + ['range:41', 'name:41'], hasName=False)
add('old name mismatch', False, parsed + ['range:41', 'name:41', 'compare:OTHER:MATH_SYMBOL'], oldName='OTHER')
add('range match', True, lookup() + ['equal'])
add('range mismatch', False, lookup() + ['equal'], pending=[pending(selection=9)])
add('null pending empty old', True, lookup() + ['count'], pending=[pending(selection=None)], rangeCount=0)
add('null pending nonempty old', False, lookup() + ['count'], pending=[pending(selection=None)])
add('first duplicate is decisive', False, lookup() + ['equal'], pending=[pending(selection=9), pending()])
add('skip unrelated IDs', True, lookup() + ['equal'], pending=[pending(8, 'OTHER', None), pending()])
add('kind3 uses name30', True, lookup() + ['equal'], envelope=3, name10='wrong', name30='tag')
add('negative old ID is allowed', True, lookup(-7) + ['equal'], old=value(number=-7))
add('native NEG wraps INT64_MIN', True, lookup() + ['equal'], new=value(number=-(1 << 63)), pending=[pending(-(1 << 63))])
add('old numeric prefix', True, lookup() + ['equal'], old=value(text='41tail'))
add('new numeric prefix', True, lookup() + ['equal'], new=value(text='-2tail'))
add('old plus sign', True, lookup() + ['equal'], old=value(text='+41'))
add('new leading space is not skipped', False, parsed, new=value(text=' -2'))
add('new sign only terminated', False, parsed, new=value(text='-'))
add('new raw overflow wraps', True, lookup() + ['equal'], new=value(text='18446744073709551614'))
add('old raw overflow wraps', True, lookup() + ['equal'], old=value(text='18446744073709551657'))
add('float conversion consumed before parsing', True, lookup() + ['equal'], old=value(tag=5, number=41.5), new=value(tag=5, number=-2.5))
add('exponent text is prefix parsed, NOT Number()', True, lookup(1) + ['equal'], old=value(tag=5, number=1e16))
add('old leading space is not skipped', False, converted + ['ParseOld'], old=value(text=' 41'))
add('unknown memory after all-digit storage refuses', False, converted + ['ParseOld'], old=value(text='41', terminated=False), throws=True)
add('unknown memory after sign refuses', False, converted + ['ParseOld'], old=value(text='+', terminated=False), throws=True)
add('null conversion pointer refuses', False, converted + ['ParseOld'], old=value(tag=0), throws=True)

cleanup = [
    dict(old=[], new=[], fail=-1, calls=[], done=[], result=True),
    dict(old=[1, 2, 3], new=[2], fail=-1, calls=[1, 3], done=[1, 3], result=True),
    dict(old=[1, 1, 2], new=[], fail=-1, calls=[1, 1, 2], done=[1, 1, 2], result=True),
    dict(old=[1, 1, 2], new=[], fail=2, calls=[1, 1, 2], done=[1, 1], result=False),
    dict(old=[1, 1], new=[1], fail=-1, calls=[], done=[], result=True),
    dict(old=[1, 2, 3], new=[2, 2], fail=-1, calls=[1, 3], done=[1, 3], result=True),
    dict(old=[1, 2, 3], new=[], fail=0, calls=[1], done=[], result=False),
    dict(old=[1, 2, 3], new=[], fail=1, calls=[1, 2], done=[1], result=False),
    dict(old=[1, 2, 3], new=[2], fail=2, calls=[1, 3], done=[1, 3], result=True),
    dict(old=[-(1 << 63), -1, 0, (1 << 63) - 1], new=[], fail=-1,
         calls=[-(1 << 63), -1, 0, (1 << 63) - 1], done=[-(1 << 63), -1, 0, (1 << 63) - 1], result=True),
]
with Path(sys.argv[1]).open('x', encoding='utf-8') as stream:
    json.dump(dict(cases=rows, cleanup=cleanup), stream, separators=(',', ':'))
print(f'{len(rows)} native matching and {len(cleanup)} ordered cleanup controls')
