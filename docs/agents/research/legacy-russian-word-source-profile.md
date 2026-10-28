# Russian stored source kind and word language profile

The checked `ru_RU-ak-cur.lite.res` contains the stored source kind `0x135`.
This selects language profile **2** in the inspected native ordinary word
dictionary path. It is not inferred from the resource's inheritance type.
The acoustic resource itself has type `0x247`, which extends `0x134`; those
two type identifiers do not substitute for the stored source kind.

## Stored bytes and forwarding

The checked acoustic resource SHA-256 is
`7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978`.
Its version `0x103` acoustic alphabet starts at offset 99 and ends at offset
5233. Immediately following it, bytes `35 01 00 00` encode the little-endian
source kind `0x135`. The next byte is feature kind 2.

Native `1801d4950`, the type `0x247` reader, delegates the inherited alphabet
reader and then reads this four-byte source kind into raw object offset
`0x78`. Getter `1801d6280`, slot `+0x58` in the acoustic knowledge table,
returns that field. The version `0x103` alphabet has no version `0x104`
trailing byte: reading one would shift the source kind and produce a false
value.

The verified word initialization chain is:

1. `18020c9d0` wraps a type `0x13b` word resource in type `0x2b7`.
2. `180236490` initializes dictionary children through child slot `+0x10`,
   forwarding the selected acoustic frontend.
3. Type `0x2b7` raw table `1802b4360`, slot `+0x10`, points to `180093970`.
   It calls acoustic frontend slot `+0x18` and passes the returned acoustic
   knowledge object to `180092990`.
4. In the language-model-present branch, `180092990` reads the acoustic
   knowledge object's slot `+0x58` and passes that value, together with the
   word resource's float field `+0x6c`, to its language model slot `+0x48`.
5. The actual type `0x2d1` language model slot `+0x48` is `1800fe2a0`.
   It computes the stored calibration index as `0x137 - sourceKind`.

Thus this specific ordinary dictionary initialization receives `0x135` and
selects index 2. The separate type `0x13b` cache constructor `18010cad0`
also forwards its explicit caller source kind to language model slot
`+0x48`; it does not derive it from inheritance.

## Runtime scale

The word resource's field `+0x6c` is independently configurable. The type
`0x139` getter `1801d6e00` returns it. Setter `1801d6e10` accepts a strictly
positive caller float and stores it; a null setter input sets `1.0f`.
This establishes the setter's reset value, but does not prove that an
application never overrides it or that every construction path executes
that reset. The portable language scorer therefore keeps a finite positive
runtime scale as an explicit input.

## Scope

This evidence removes the ambiguous choice among four stored language
profiles for the checked source in the described word path. It does not
restore native default history, token category, case policy, acoustic
corrections, search widths, continuous-stroke cut production, or automatic
line geometry. It also does not apply the lexical cost universally: the
ordinary candidate addition and the native language-bypass branch are
documented separately in
[the word candidate scorer](legacy-russian-word-candidate.md).

Static SDK inspection reads bytes and decompiler output without loading the
vendor library. It does not execute activation, alter the engine, or bypass
authorization. Headless synthetic numerical controls verify portable
arithmetic independently; Russian word quality and joined handwriting
require separate representative ink checks.
