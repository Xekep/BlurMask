"""Original float32 context arithmetic controls; no engine/models/ink assets."""
import json, math, pathlib, random, struct, sys

def f(x):
    return struct.unpack('<f', struct.pack('<f', x))[0]

def reference(observations, target, height, offset, left, right):
    output = [0.0] * 28
    for observation in observations:
        distance = f(f(observation['x'] - target) / height)
        if distance >= 0:
            exponent = f(f(f(distance * distance) * -.5) / right)
        else:
            distance = f(distance + offset)
            exponent = f(f(f(distance * distance) * -.5) / left) if distance <= 0 else 0.0
        if exponent < -75:
            continue
        weight = f(math.exp(exponent))
        for i, probability in enumerate(observation['probabilities']):
            output[i] = max(output[i], f(weight * probability))
    result = [f(f(value + value) - 1) for value in output]
    return [-1.0 if value < f(-.99) else value for value in result]

randomizer = random.Random(0xca5f0)
cases = []
for index in range(512):
    target = f(randomizer.uniform(-1000, 1000))
    height = f(randomizer.choice([.1, 1, 10, 300]))
    offset = f(randomizer.choice([0, -.5, .5, 2]))
    left, right = f(randomizer.choice([.1, 1, 3])), f(randomizer.choice([.25, 1, 5]))
    observations = []
    for j in range(randomizer.randrange(13)):
        x = f(target + height * randomizer.uniform(-16, 16))
        probabilities = [f(randomizer.choice([0, 1, .004, randomizer.random()])) for _ in range(28)]
        observations.append(dict(x=x, probabilities=probabilities))
    cases.append(dict(observations=observations, target=target, height=height, offset=offset,
                      left=left, right=right, expected=reference(observations, target, height, offset, left, right)))
path = pathlib.Path(sys.argv[1])
path.parent.mkdir(parents=True, exist_ok=True)
path.write_text(json.dumps(cases, separators=(',', ':')), encoding='utf8')
print(f'Wrote{len(cases)} own independent context float32 reference cases.')
