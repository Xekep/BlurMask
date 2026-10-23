"""Conditional scalar-store bounds for the pinned English kind8 initializer.

This is an algebraic certificate, not SDK execution or an allocator/alias proof.
The native-to-row mapping is explicit; complete bodies and every nonmetadata
store site are checked by the separate metadata certificate before applying it.
"""
import argparse
import importlib.util
import json
from pathlib import Path


def audit(binary, resource):
    spec = importlib.util.spec_from_file_location(
        'ctc_metadata', Path(__file__).with_name('audit_english_ctc_state_metadata.py'))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    metadata = module.audit(binary)
    spec = importlib.util.spec_from_file_location(
        'ctc_cpu_layout', Path(__file__).with_name('audit_english_ctc_cpu_layout.py'))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    layout = module.audit(binary, resource)
    # Every payload store in these two complete bodies is accounted for. These
    # are sites, not execution counts: 192AE0 is called twice by 191470.
    expected_sites = [
        [0x180191526, 0x180191534],
        [0x180192b66, 0x180192b74, 0x180192bfe, 0x180192c0f,
         0x180192c8d, 0x180192c9e, 0x180192d0f, 0x180192d20,
         0x180192dbb, 0x180192dd6, 0x180192e5a, 0x180192e6f,
         0x180192e93, 0x180192e9a, 0x180192ea7, 0x180192eaf,
         0x180192ebb, 0x180192ebf, 0x180192ecc, 0x180192ed4,
         0x180192edf, 0x180192eea],
    ]
    for function, expected in zip(metadata['functions'], expected_sites, strict=True):
        actual = function['otherNonstackStoresNotYetProven']
        assert [int(x['address'], 16) for x in actual] == expected
        assert all(x['instruction'].startswith('mov dword ptr [') for x in actual)

    # Actual model: width127, flags0. In 191470, flags bit8 clear doubles only
    # the leading width. In 192AE0, bits2/4 clear retain all six subvectors.
    # A row reserves aligned(width+1) float units, not aligned(width).
    width = 127
    assert all(layer[:3] == (8, width, 0) for layer in layout['rawLayers'][1:])
    certificates = []
    for alignment in (8, 16):
        rounded = lambda n: (n + alignment - 1) // alignment * alignment
        prefix_stride, vector_stride = rounded(2 * width + 1), rounded(width + 1)
        assert (prefix_stride, vector_stride) == (256, 128)
        for extra_row in (False, True):
            rows = 2 + int(extra_row)
            total = rows * (prefix_stride + 12 * vector_stride)
            allocations, stores = [], []

            def vector(offset, stride, logical_width, owner, clear, initial_sites, clear_sites):
                assert 0 <= logical_width < stride
                end = offset + rows * stride
                allocations.append((offset, end, owner))
                # Both helpers initialize the bias in rows1/0. 192AE0 then
                # clears these same cells for its first five vectors only.
                for row, site in zip((1, 0), initial_sites, strict=True):
                    stores.append((site, offset + row * stride + logical_width, 0x3f800000, owner))
                if clear:
                    for row, site in zip((1, 0), clear_sites, strict=True):
                        stores.append((site, offset + row * stride + logical_width, 0, owner))
                return end

            end = vector(0, prefix_stride, 2 * width, 'prefix', False, expected_sites[0], ())
            for direction in range(2):
                for index in range(6):
                    init = expected_sites[1][2 * index:2 * index + 2]
                    clear = expected_sites[1][12 + 2 * index:14 + 2 * index] if index < 5 else ()
                    end = vector(end, vector_stride, width, f'substate{direction}/vector{index}',
                                 index < 5, init, clear)
            assert end == total and total == (5376 if extra_row else 3584)
            assert 4 * total == next(case['kind8WorkspaceBytes'] for case in layout['conditionalKind8Workspace']
                                     if case['alignmentFloatUnits'] == alignment and case['extraRow'] == extra_row)
            assert len(allocations) == 13 and len(stores) == 46
            assert allocations[0][0] == 0 and allocations[-1][1] == total
            assert all(a[1] == b[0] for a, b in zip(allocations, allocations[1:]))
            assert all(0 <= address < total for _, address, _, _ in stores)
            # Each store must lie in its own vector, not merely somewhere in
            # the overall allocation. Duplicate zero writes intentionally stay.
            by_owner = {owner: (start, stop) for start, stop, owner in allocations}
            assert all(by_owner[owner][0] <= address < by_owner[owner][1]
                       for _, address, _, owner in stores)
            assert len({site for site, _, _, _ in stores}) == 24
            certificates.append(dict(alignmentFloatUnits=alignment, extraRow=extra_row,
                                     workspaceBytes=4 * total, scalarStoreOccurrences=len(stores),
                                     uniqueNativeStoreSites=24, maximumStoreEndBytes=4 * (max(
                                         address for _, address, _, _ in stores) + 1),
                                     vectorAllocationsFloatUnits=allocations,
                                     orderedStores=[dict(site=hex(site), byteOffset=4 * address,
                                                        bits=hex(bits), owner=owner)
                                                    for site, address, bits, owner in stores]))
    return dict(status='PASS CONDITIONAL kind8 scalar index arithmetic',
                width=width, flags=0, certificates=certificates,
                condition='Caller supplies a live disjoint workspace of the computed size and the native-to-row mapping stated here applies.',
                rawModelAdmissionProven=False, nativePointerDataflowFullyProven=False,
                allocatorSeparationProven=False, aliasStoreProofComplete=False,
                runtimePredicateApproved=False, vendorExecuted=False)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--sdk', type=Path, required=True)
    parser.add_argument('--models', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    result = audit(args.sdk / 'MyScriptEngine.dll', args.models / 'en-US/en_US-ak-cur.res')
    args.output.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({key: value for key, value in result.items() if key != 'certificates'}))
