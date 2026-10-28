"""Semantic port of Document032800; required owner services stay explicit.

This kernel is not a DOM/Ink owner or a publication API. The adapter must read
the real edit node and use the CB8's retained Ink and pending linked-list order.
No vendor code is imported or executed.
"""
from dataclasses import dataclass
from typing import Any, Protocol, Sequence


@dataclass(frozen=True)
class Pending:
    temporary_id: int
    name: Any
    source_range: Any  # None denotes the native null C22, not an empty object.


class Services(Protocol):
    def compare_name(self, left: Any, right: Any) -> int: ...
    def attribute_text(self, value: Any) -> Any: ...  # Document0078D0
    def parse_tag(self, text: Any) -> int | None: ...  # global90.V88, base10
    def ink_range(self, selection_id: int) -> Any: ...  # retained Ink.V128
    def ink_name(self, selection_id: int) -> Any: ...  # retained Ink.V130
    def range_count(self, source_range: Any) -> int: ...  # C22+20 DWORD
    def ranges_equal(self, left: Any, right: Any) -> bool: ...  # range.V28


def matches(edit_kind: int, attribute_name: Any, tag_name: Any,
            old_value: Any, new_value: Any, pending: Sequence[Pending],
            services: Services) -> bool:
    """Return native callback truth, preserving lookup and short-circuit order.

    Attribute-name extraction (native envelope kinds0/3), actual owner lifetime,
    typed conversion and bounded integer parsing are adapter prerequisites.
    No range/name lookup is authorized by caller-invented selection records.
    """
    if edit_kind != 7 or attribute_name is None:
        return False
    if services.compare_name(attribute_name, tag_name) != 0:
        return False
    # Native converts BOTH variants before it tests the first parser result.
    old_text = services.attribute_text(old_value)
    new_text = services.attribute_text(new_value)
    old_id = services.parse_tag(old_text)
    if old_id is None:
        return False
    new_id = services.parse_tag(new_text)
    if new_id is None or new_id >= 0:
        return False
    # NEG is a wrapping x64 integer instruction, including INT64_MIN.
    temporary_id = ((-new_id + (1 << 63)) % (1 << 64)) - (1 << 63)
    entry = next((entry for entry in pending if entry.temporary_id == temporary_id), None)
    if entry is None:
        return False
    old_range = services.ink_range(old_id)
    if old_range is None:
        return False
    old_name = services.ink_name(old_id)
    if old_name is None or services.compare_name(old_name, entry.name) != 0:
        return False
    if entry.source_range is None:
        return services.range_count(old_range) == 0
    return services.ranges_equal(old_range, entry.source_range)
