"""Source-pin restoration only; all historical validators remain mandatory.

Known complete production successor hashes are restored before old adapters.

Unlisted files are NOT authenticated by this loader; callers still enforce pins.

"""

from pathlib import Path

import hashlib,runpy

_HELPER=Path(__file__).with_name('reviewed_source_reversal.py')

_raw=_HELPER.read_bytes().replace(b'\r\n',b'\n')

assert hashlib.sha256(_raw).hexdigest()=='1c8d1ea51e653d512fbad8a3bd8b32da800ca938992232faf387d31355ae6929','Unreviewed source reversal helper'

_module=runpy.run_path(str(_HELPER),run_name='reviewed_node0_source_reversal_only')

_NAMES=frozenset(_module['_REVIEWED'])

_EARLIER={'LegacyFst28CNumberInitialConsumer.cs':frozenset(['25be1dce91a32902c2b29fcc24c14b3f28ebac2a5f18e7fbd4d87b70620a7935']),

          'LegacyFst28CNumberInitialConsumerContext.cs':frozenset(['98f19fb593fce5701672e213c2049f60c10d4e4a8f1c6532b90ffac08e9bce61'])}

def restore_source_for_pin(relative,data):

    assert type(relative)is str and type(data)is bytes,'Exact source path/bytes required'

    name=relative.replace('\\','/').rsplit('/',1)[-1]

    if name not in _NAMES:return data

    b=data.replace(b'\r\n',b'\n')

    if hashlib.sha256(b).hexdigest()in _EARLIER.get(name,()):return b

    try:return _module['restore_reviewed_source'](name,data)

    except(ValueError,TypeError)as exc:raise AssertionError('Unreviewed source for historical pin: '+name)from exc

