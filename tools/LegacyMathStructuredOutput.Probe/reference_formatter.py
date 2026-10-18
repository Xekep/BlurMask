"""Independent literal EE4 mode0/null-map interpreter over native frame lanes.

Derives rules from the caller's independently read raw AL. No C# source/helper,
vendor execution or model execution. Frame byte aliases remain explicit here.
"""
import struct


class Formatter:
    def __init__(self, labels):
        specifications = [
            (3, ['√', '</sqrt>']), (9, ['<frac>', '<fracbar>', '</frac>']),
            (10, ['<slantedfrac>', '<slantedfracbar>', '</slantedfrac>']),
            (4, ['<sub>', '</sub>']), (5, ['<sup>', '</sup>']),
            (11, ['<subsup>', '<subsup2>', '</subsup>']),
            (12, ['<under>', '<under2>', '</under>']),
            (13, ['<over>', '<over2>', '</over>']),
            (16, ['<underover>', '<underover2>', '<underover3>', '</underover>']),
            (6, ['<presup>', '</presup>']), (14, ['<table>', '<&>', '<\\>', '</table>']),
            (15, ['<vop>', '<\\>', '<vopbar>', '</vop>']),
            (7, ['<fence>', '</fence>']), (8, ['\u2402', '\u2403'])]
        self.rules = [(kind, [labels[text] for text in tags]) for kind, tags in specifications]
        self.delimiters = {labels[text] for text in ('(', '⟨', '⌈', '⌊', '{', '[', ')', '⟩', '⌉', '⌋', '}', ']', '|', '‖') if text in labels}
        self.replacements = {labels['<fracbar>']: labels['-'], labels['<vopbar>']: labels['-'],
                             labels['<slantedfracbar>']: labels['/']}

    @staticmethod
    def arity(kind):
        return 1 if 3 <= kind < 9 else 2 if 9 <= kind < 14 else 3 if 14 <= kind < 17 else -1

    def kind(self, frame):
        return self.rules[frame['bytes'][0]][0]

    def tag(self, frame, step):
        tags = self.rules[frame['bytes'][0]][1]
        return tags[step] if step < len(tags) else 0

    @staticmethod
    def short(frame, address):
        return struct.unpack_from('<h', frame['bytes'], address)[0]

    @staticmethod
    def put_short(frame, address, value):
        struct.pack_into('<H', frame['bytes'], address, value & 65535)

    def pair(self, frame):
        row, column = self.short(frame, 0x28), self.short(frame, 0x2a)
        if self.short(frame, 0x2c) == -1:
            self.put_short(frame, 0x2c, row); self.put_short(frame, 0x2e, column)
        elif self.short(frame, 0x30) == -1 and (self.short(frame, 0x2c), self.short(frame, 0x2e)) != (row, column):
            self.put_short(frame, 0x30, row); self.put_short(frame, 0x32, column)

    def propagate(self, state, node_kind, node_word):
        if not state['frames']:
            return
        frame = state['frames'][-1]; raw = frame['bytes']; step = raw[1]
        raw[0x20+step] = int(bool(raw[0x20+step]) or node_kind in (2, 3))
        if raw[0x20+step]:
            for ancestor in state['frames'][:-1]:
                ancestor['bytes'][0x20+ancestor['bytes'][1]] = 1
        if raw[0x23] and not raw[0x24]:
            raw[0x24] = 1
            raw[0x25] = int(((node_word >> 16) & 255) == 0 and (node_word & 65535) in self.delimiters)
            if not raw[0x25]:
                state['closed'], state['error'] = True, 2
        if raw[0x26] and node_kind in (2, 3):
            self.pair(frame)

    def open(self, state, ordinal):
        kind = self.rules[ordinal][0]
        self.propagate(state, kind, 65535)  #0234F0 successful node initialization.
        raw = bytearray(64); raw[0] = ordinal
        raw[0x23], raw[0x26], raw[0x38] = kind == 7, kind == 14, kind == 15
        raw[0x2c:0x34] = b'\xff'*8
        raw[0x34:0x36] = b'\xff'*2
        state['frames'].append(dict(bytes=raw, positions=[state['input']]))

    def match(self, frame, token):
        if frame is None:
            return False, -1
        raw = frame['bytes']; step = raw[1]
        if raw[0x26]:
            if step == 0:
                return token in (self.tag(frame, 1), self.tag(frame, 2)), -1
            if step not in (1, 2):
                return False, -1
            return token in (self.tag(frame, 1), self.tag(frame, 2), self.tag(frame, 3)), -1
        if raw[0x38]:
            if step == 0:
                return token in (self.tag(frame, 1), self.tag(frame, 2)), -1
            if step not in (1, 2):
                return False, -1
            if token in (self.tag(frame, 1), self.tag(frame, 2)):
                return True, -1
            return token == self.tag(frame, 3) and (step == 2 or bool(raw[0x39])), -1
        for candidate in range(step+1, 4):
            if self.tag(frame, candidate) == token:
                return candidate == step+1, candidate
        return False, -1

    def complete_parts(self, frame):
        kind = self.kind(frame); raw = frame['bytes']
        if kind == 3:
            return True
        if kind in (9, 10):
            return bool(raw[0x20] or raw[0x21])
        return all(raw[0x20+part] for part in range(self.arity(kind)))

    def pop(self, state):
        removed = state['frames'].pop()
        if state['frames'] and state['frames'][-1]['bytes'][0x26] and self.complete_parts(removed):
            self.pair(state['frames'][-1])

    def advance(self, state, token):
        frames = state['frames']
        if not frames:
            state['error'] = 1
            return
        frame = frames[-1]; raw = frame['bytes']; kind = self.kind(frame); parts = self.arity(kind)
        success, matching_step = self.match(frame, token)
        if not success:
            state['closed'] = matching_step == parts; state['error'] = 1
            for previous in reversed(frames[:-1]):
                if self.match(previous, token)[0]:
                    state['error'] = 3
                    break
            return
        prior = raw[1]
        state['closed'] = (prior >= 1 and token == self.tag(frame, parts)) if raw[0x26] or raw[0x38] else prior+1 == parts
        present = raw[0x20+prior]
        frame['positions'].append(state['input'])
        if not present and kind != 9 and (kind > 15 or not ((0xc408 >> kind) & 1)):
            state['error'] = 4
        if not raw[0x26] and not raw[0x38]:
            raw[1] += 1
        else:
            step = next((part for part in range(parts) if self.tag(frame, part) == token), parts)
            if raw[0x26] and step == 1:
                self.put_short(frame, 0x2a, self.short(frame, 0x2a)+1); raw[1] = 1
                return
            if raw[0x26] and step == 2:
                maximum, column = self.short(frame, 0x34), self.short(frame, 0x2a)
                if maximum != -1 and maximum != column:
                    raw[0x36] = 1; state['error'] = 5
                self.put_short(frame, 0x34, max(maximum, column)); self.put_short(frame, 0x2a, 0)
                self.put_short(frame, 0x28, self.short(frame, 0x28)+1)
            raw[1] = step
            if raw[0x38] and token == self.tag(frame, 2):
                raw[0x39] = 1
        if not state['closed']:
            return
        if raw[0x26]:
            if any(self.short(frame, offset) < 0 for offset in (0x2c, 0x2e, 0x30, 0x32)):
                state['error'] = 4
                return
            maximum, column = self.short(frame, 0x34), self.short(frame, 0x2a)
            if maximum != -1 and maximum != column:
                state['error'] = 5; raw[0x36] = 1
            self.put_short(frame, 0x34, max(maximum, column))
        elif raw[0x23]:
            if not raw[0x25]:
                state['error'] = 4
        elif not self.complete_parts(frame):
            state['error'] = 4

    def token(self, state, word):
        state['closed'], state['error'] = False, 0
        label = word & 65535
        if ((word >> 16) & 255) == 0:
            for ordinal, (_, tags) in enumerate(self.rules):
                if tags[0] and label == tags[0]:
                    self.open(state, ordinal)
                    return
            if any(label in tags[1:] for _, tags in self.rules):
                self.advance(state, label)
                return
        self.propagate(state, 2, word)

    @staticmethod
    def erase(frame, words, mapping):
        for source in frame['positions']:
            at = mapping[source]
            words[at] = (words[at] & 0xffff0000) | 65535

    def pad(self, frame, words, mapping, recovery):
        column = 0; maximum = self.short(frame, 0x34)
        positions = frame['positions'][:-1] if recovery else frame['positions']
        for source in positions:
            at = mapping[source]; label = words[at] & 65535
            if label == self.tag(frame, 0):
                column = 0
            elif label == self.tag(frame, 1):
                column += 1
            elif label in (self.tag(frame, 2), self.tag(frame, 3)):
                missing = max(0, maximum-column)
                if missing:
                    mapping[source:] = [(value+missing) & 65535 for value in mapping[source:]]
                    words[at:at] = [self.tag(frame, 1)]*missing
                column = 0
        if recovery:
            words.extend([self.tag(frame, 1)]*max(0, maximum-column))

    def repair(self, state, words, mapping):
        frame = state['frames'][-1]; raw = frame['bytes']; kind = self.kind(frame); count = self.arity(kind)
        if raw[0x26]:
            possible = all(self.short(frame, address) >= 0 for address in (0x2c, 0x2e, 0x30, 0x32))
        elif raw[0x23]:
            possible = raw[1]+1 == count and all(raw[0x20+part] for part in range(count)) and raw[0x25]
        elif raw[0x38]:
            possible = raw[1] == 2 and bool(raw[0x20] or raw[0x21])
        else:
            possible = raw[1]+1 == count and self.complete_parts(frame)
        if not possible:
            return False
        closer = self.tag(frame, count)
        self.advance(state, closer)
        if state['closed'] and raw[0x26] and raw[0x36]:
            self.pad(frame, words, mapping, True)
        words.append(closer)
        if state['closed']:
            self.pop(state)
        return True

    def evaluate(self, source):
        state = dict(input=-1, frames=[], error=0, closed=False)
        words, mapping = [], []
        while state['input']+1 < len(source):
            state['input'] += 1; mapping.append(len(words))
            while True:
                old_count = len(state['frames']); self.token(state, source[state['input']])
                opened = old_count < len(state['frames'])
                if state['error'] != 3 or not state['frames']:
                    break
                if not self.repair(state, words, mapping):
                    self.erase(state['frames'][-1], words, mapping); state['frames'].pop()
                else:
                    mapping[-1] += 1
                if not state['frames']:
                    break
            discard = state['error'] in (1, 4)
            words.append(65535 if discard else source[state['input']])
            if not state['frames'] or not state['closed']:
                continue
            if discard:
                self.erase(state['frames'][-1], words, mapping)
            if state['error'] == 2:
                at = len(state['frames'])-1-int(opened and len(state['frames']) > 1)
                self.erase(state['frames'][at], words, mapping); state['frames'].pop(at)
            else:
                frame = state['frames'][-1]
                if frame['bytes'][0x26] and frame['bytes'][0x36] and not discard:
                    self.pad(frame, words, mapping, False)
                self.pop(state)
        state['input'] += 1
        for _ in range(len(state['frames'])):
            if not self.repair(state, words, mapping):
                self.erase(state['frames'][-1], words, mapping); state['frames'].pop()
        output, result_map, source_index = [], [], 0
        for word in words:
            original = source[source_index] if source_index < len(source) else 65535
            if word & 65535 == 65535:
                if ((original >> 16) & 255) == 0 and (original & 65535) in self.replacements:
                    result_map.append(len(output)); output.append((original & 0xffff0000) | self.replacements[original & 65535])
                else:
                    result_map.append(65535)
                source_index += 1
            else:
                if (word & 0xffffff) == (original & 0xffffff):
                    result_map.append(len(output)); source_index += 1
                output.append(word)
        check = dict(input=-1, frames=[], error=0, closed=False)
        valid = True
        for index, word in enumerate(output):
            check['input'] = index; self.token(check, word)
            if check['error']:
                valid = False
                break
            if check['closed']:
                self.pop(check)
        return dict(units=output, mapping=result_map, valid=valid and not check['frames'])
