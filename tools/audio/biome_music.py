"""UNDERSUN biome score: one composer, eleven specs. Each biome gets its own mode, tempo, instruments and percussion;
melodies are composed from the chord progression with a seeded motif-and-variation scheme (A A' B A'').
usage: python biome_music.py <out_dir> [track ...]"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).parent))
from synth import RATE, adsr, bandpass, decay, delay, drive, highpass, lowpass, midi_hz, noise, saw, sine, square, stats, sweep, triangle, write_wav
from music import (Track, bass_voice, bell_toll, bell_voice, choir_voice, crash, finish, hat, kick, lead_voice, melody, pad_voice,
                   pluck_voice, snare, stereo)

SCALES = {
    "aeolian": [0, 2, 3, 5, 7, 8, 10], "dorian": [0, 2, 3, 5, 7, 9, 10], "phrygian": [0, 1, 3, 5, 7, 8, 10],
    "harmonic": [0, 2, 3, 5, 7, 8, 11], "lydian": [0, 2, 4, 6, 7, 9, 11],
}


def degree_note(key, scale, degree):
    s = SCALES[scale]
    octave, idx = divmod(degree, len(s))
    return key + 12 * octave + s[idx]


def triad(key, scale, degree):
    return tuple(degree_note(key, scale, degree + k) for k in (0, 2, 4))


# ---------------- extra voices ----------------

def flute_voice(note, dur):
    n = int((dur + 0.2) * RATE)
    t = np.arange(n) / RATE
    f = midi_hz(note) * (1 + 0.008 * np.sin(2 * np.pi * 5 * t) * np.clip((t - 0.12) / 0.2, 0, 1))
    x = sine(f, n / RATE) + 0.12 * sine(f * 2, n / RATE) + 0.05 * bandpass(noise(n / RATE), 900, 4000)
    return x * adsr(len(x), 0.05, 0.1, 0.8, 0.18, hold=dur)


def glass_voice(note, dur):
    d = max(dur, 0.25) + 0.8
    x = sine(midi_hz(note + 12), d) + 0.4 * sine(midi_hz(note + 24) * 1.002, d) + 0.2 * sine(midi_hz(note + 31), d)
    return x * decay(len(x), 0.45) * 0.6


def tom(level=1.0, f0=150.0):
    d = 0.35
    return sine(sweep(f0, f0 * 0.55, d), d) * decay(int(d * RATE), 0.12) * level


def tick(level=1.0):
    d = 0.03
    return highpass(noise(d), 4000) * decay(int(d * RATE), 0.004) * level


def doum(level=1.0):
    d = 0.4
    return (sine(sweep(110, 70, d), d) + 0.2 * bandpass(noise(d), 80, 300)) * decay(int(d * RATE), 0.15) * level


def tek(level=1.0):
    d = 0.08
    return bandpass(noise(d), 1500, 5000) * decay(int(d * RATE), 0.018) * level


def anvil(level=1.0):
    d = 1.2
    x = sum(a * sine(f, d) * decay(int(d * RATE), tau) for f, a, tau in ((880, 1, 0.5), (1347, 0.6, 0.3), (2210, 0.4, 0.15), (3090, 0.25, 0.08)))
    return x * level * 0.5


def drip(level=1.0):
    d = 0.25
    return sine(sweep(1600, 2600, d), d) * decay(int(d * RATE), 0.04) * level


# ---------------- composer ----------------

RHYTHMS = {
    "sparse": [[(0, 2), (2, 2)], [(0, 3), (3, 1)], [(0, 4)], [(0, 1.5), (1.5, 2.5)]],
    "medium": [[(0, 1), (1, 1), (2, 2)], [(0, 1.5), (1.5, 0.5), (2, 1), (3, 1)], [(0, 2), (2, 1), (3, 1)], [(0, 1), (1, 0.5), (1.5, 0.5), (2, 2)]],
    "busy": [[(0, .5), (.5, .5), (1, 1), (2, .5), (2.5, .5), (3, 1)], [(0, 1), (1, .5), (1.5, .5), (2, 1), (3, 1)],
             [(0, .5), (.5, .5), (1, .5), (1.5, .5), (2, 2)], [(0, 1.5), (1.5, .5), (2, .5), (2.5, .5), (3, 1)]],
}


def compose_bar(rng, key, scale, chord, prev, density):
    """One bar of melody: chord tones on strong beats, scale steps between, moving from the previous note."""
    out = []
    pool = sorted({degree_note(key + 12, scale, d) for d in range(-2, 12)})
    chord_tones = [n for n in pool if (n - chord[0]) % 12 in {(c - chord[0]) % 12 for c in chord}]
    for beat, beats in RHYTHMS[density][rng.integers(len(RHYTHMS[density]))]:
        strong = beat in (0, 2)
        cands = chord_tones if strong else pool
        near = sorted(cands, key=lambda n: abs(n - prev) + rng.random() * 3)
        note = near[int(rng.integers(0, min(3, len(near))))]
        out.append((beat, beats, note))
        prev = note
    return out, prev


def compose_melody(rng, key, scale, chords, density):
    """A A' B A'': a four-bar motif, its variation, a contrasting phrase and a final return with a cadence."""
    prev = degree_note(key + 12, scale, 4)
    a = []
    for ch in chords[:4]:
        bar, prev = compose_bar(rng, key, scale, ch, prev, density)
        a.append(bar)
    a2 = [list(b) for b in a]
    a2[3], prev = compose_bar(rng, key, scale, chords[7], prev, density)
    b = []
    for ch in chords[8:12]:
        bar, prev = compose_bar(rng, key, scale, ch, prev + 3, "busy" if density != "sparse" else "medium")
        b.append(bar)
    a3 = [list(x) for x in a[:3]] + [[(0, 4, degree_note(key + 12, scale, 0))]]
    return a + a2 + b + a3


def biome(spec):
    rng = np.random.default_rng(spec["seed"])
    bpm, bars = spec["bpm"], 16
    spb = 60 / bpm
    length = bars * 4 * spb
    key, scale = spec["key"], spec["scale"]
    prog = spec["prog"] * (16 // len(spec["prog"]))
    chords = [triad(key, scale, d) for d in prog]
    pad, bass, arp, lead, drums, extra = (Track(length) for _ in range(6))

    for b, ch in enumerate(chords):
        t0 = b * 4 * spb
        if spec.get("choir"):
            pad.add(t0, choir_voice(ch, 4 * spb), 0.7)
        else:
            pad.add(t0, pad_voice(ch, 4 * spb, spec.get("bright", 1800)), spec.get("pad", 1.0))
        root = ch[0] - 12
        style = spec.get("bass", "pulse")
        if style == "drone":
            bass.add(t0, bass_voice(root, 4 * spb, 500), 0.45)
        elif style == "walk":
            for i, off in enumerate((0, 7, 12, 7)):
                bass.add(t0 + i * spb, bass_voice(root + off, spb * 0.8, 800), 0.42)
        elif style == "drive":
            for i in range(8):
                bass.add(t0 + i * spb / 2, bass_voice(root + (12 if i % 4 == 2 else 0), spb / 2 * 0.85, 1000, 2.2), 0.45)
        else:
            bass.add(t0, bass_voice(root, 1.5 * spb, 650), 0.5)
            bass.add(t0 + 2 * spb, bass_voice(root + 7, spb, 650), 0.38)

        arp_kind = spec.get("arp")
        if arp_kind and b >= spec.get("arp_from", 2):
            tones = [ch[0] + 12, ch[1] + 12, ch[2] + 12, ch[0] + 24]
            voice = {"pluck": lambda n, d: pluck_voice(n, d, spec.get("duty", 0.125)), "glass": glass_voice, "bell": bell_voice}[arp_kind]
            step = spec.get("arp_step", 0.25)
            count = int(4 / step)
            order = [0, 1, 2, 3, 2, 1] * 6
            for i in range(count):
                if spec.get("arp_sparse") and i % 2:
                    continue
                arp.add(t0 + i * step * spb, voice(tones[order[i % len(order)]], step * spb), spec.get("arp_gain", 0.16))

        perc = spec.get("perc", "none")
        if b >= spec.get("perc_from", 4):
            if perc == "soft":
                drums.add(t0, kick(0.4)); drums.add(t0 + 2.5 * spb, tom(0.3, 180))
                for beat in (1, 3):
                    drums.add(t0 + beat * spb, tek(0.25))
            elif perc == "tribal":
                for beat, lv, f in ((0, .7, 140), (1.5, .45, 170), (2, .6, 120), (3, .4, 200), (3.5, .35, 200)):
                    drums.add(t0 + beat * spb, tom(lv, f))
            elif perc == "clock":
                for i in range(8):
                    drums.add(t0 + i * spb / 2, tick(0.5 if i % 2 == 0 else 0.3))
                drums.add(t0, kick(0.5)); drums.add(t0 + 2 * spb, kick(0.4))
            elif perc == "anvil":
                drums.add(t0, kick(0.8)); drums.add(t0 + 2 * spb, kick(0.6))
                drums.add(t0 + 1 * spb, anvil(0.35)); drums.add(t0 + 3 * spb, anvil(0.28))
                for i in range(8):
                    drums.add(t0 + i * spb / 2, hat(0.08))
            elif perc == "hand":
                for beat, f in ((0, doum), (1, tek), (1.5, tek), (2, doum), (2.75, tek), (3.5, tek)):
                    drums.add(t0 + beat * spb, f(0.5))
            elif perc == "heavy":
                drums.add(t0, kick(1.0)); drums.add(t0 + 2.5 * spb, kick(0.7))
                drums.add(t0 + 2 * spb, snare(0.5))
                if b % 4 == 0:
                    drums.add(t0, crash(0.3))
            elif perc == "drips":
                for i in range(3):
                    if rng.random() < 0.6:
                        extra.add(t0 + rng.random() * 4 * spb, drip(0.3))
        if spec.get("tolls") and b % 4 == 0:
            extra.add(t0, bell_toll(ch[0] - 12, 0.45))

    lead_kind = spec.get("lead", "lead")
    voice = {"lead": lead_voice, "bright": lambda n, d: lead_voice(n, d, True), "flute": flute_voice, "glass": glass_voice,
             "bell": bell_voice}[lead_kind]
    for i, phrase in enumerate(compose_melody(rng, key, scale, chords, spec.get("density", "medium"))):
        if spec.get("intro_rest", True) and i < 2:
            continue   # let the bed play alone for the first bars
        melody(lead, spb, i, phrase, voice, spec.get("lead_gain", 0.3))

    arp_wet = delay(arp.buf, spb * 0.75, 0.4, spec.get("arp_delay", 0.4), tail=0)[: len(arp.buf)]
    lead_wet = delay(lead.buf, spb * 0.5, 0.3, spec.get("lead_delay", 0.3), tail=0)[: len(lead.buf)]
    L, R = stereo([pad.buf, bass.buf, arp_wet, lead_wet, drums.buf, extra.buf], [-0.3, 0, 0.45, -0.1, 0, 0.35])
    return finish(L, R, length, True, spec.get("room", 0.8), spec.get("wet", 0.3), peak=spec.get("peak", -3.0))


SPECS = {
    "music_hollow": dict(seed=2, bpm=76, key=52, scale="dorian", prog=[0, 3, 5, 4], bright=1300, bass="pulse", arp="pluck", duty=0.25,
                         perc="soft", lead="flute", density="medium", room=0.9, wet=0.35),
    "music_cistern": dict(seed=3, bpm=66, key=50, scale="aeolian", prog=[0, 5, 3, 4], bright=1600, bass="drone", arp="glass",
                          arp_sparse=True, arp_step=0.5, perc="drips", lead="bell", density="sparse", room=0.95, wet=0.45,
                          lead_delay=0.45, arp_delay=0.55),
    "music_forge": dict(seed=4, bpm=104, key=45, scale="harmonic", prog=[0, 5, 3, 4], bright=2200, bass="drive", arp="pluck",
                        duty=0.5, perc="anvil", lead="bright", density="busy", room=0.6, wet=0.2, peak=-2.5),
    "music_frost": dict(seed=5, bpm=60, key=54, scale="aeolian", prog=[0, 2, 5, 4], bright=2800, bass="drone", arp="glass",
                        arp_step=0.5, perc="none", lead="glass", density="sparse", room=0.98, wet=0.5, pad=1.1),
    "music_thornwild": dict(seed=6, bpm=92, key=48, scale="dorian", prog=[0, 6, 3, 4], bright=1800, bass="walk", arp="pluck",
                            duty=0.25, perc="tribal", lead="flute", density="medium", room=0.75, wet=0.3),
    "music_clockwork": dict(seed=7, bpm=112, key=47, scale="harmonic", prog=[0, 3, 4, 0, 5, 3, 4, 4], bright=2000, bass="walk",
                            arp="pluck", duty=0.125, arp_step=0.25, perc="clock", lead="lead", density="busy", room=0.55, wet=0.18),
    "music_necropolis": dict(seed=8, bpm=88, key=50, scale="phrygian", prog=[0, 1, 0, 6], bright=1500, bass="drone", arp="pluck",
                             duty=0.25, arp_sparse=True, perc="hand", lead="lead", density="medium", room=0.85, wet=0.3),
    "music_glass": dict(seed=9, bpm=72, key=55, scale="lydian", prog=[0, 1, 4, 3], bright=3000, bass="pulse", arp="glass",
                        arp_step=0.25, perc="soft", lead="bell", density="medium", room=0.95, wet=0.45, arp_gain=0.12),
    "music_dusk": dict(seed=10, bpm=78, key=49, scale="harmonic", prog=[0, 5, 3, 4], choir=True, bass="drone", arp="bell",
                       arp_sparse=True, arp_step=0.5, perc="heavy", tolls=True, lead="lead", density="sparse", room=0.95, wet=0.4),
    "music_boss_deep": dict(seed=11, bpm=150, key=49, scale="phrygian", prog=[0, 1, 0, 6, 0, 1, 3, 4], bright=2600, bass="drive",
                            arp="pluck", duty=0.5, arp_from=0, perc="heavy", perc_from=0, lead="bright", density="busy",
                            intro_rest=False, room=0.55, wet=0.15, peak=-2.0),
    "music_boss_final": dict(seed=12, bpm=132, key=50, scale="harmonic", prog=[0, 5, 3, 4, 0, 5, 1, 4], choir=True, bass="drive",
                             arp="bell", arp_from=0, arp_step=0.5, perc="heavy", perc_from=0, tolls=True, lead="bright",
                             density="busy", intro_rest=False, room=0.7, wet=0.25, peak=-2.0),
}

if __name__ == "__main__":
    out = Path(sys.argv[1])
    out.mkdir(parents=True, exist_ok=True)
    for name in sys.argv[2:] or list(SPECS):
        L, R = biome(SPECS[name])
        write_wav(out / f"{name}.wav", L, R)
        print(f"{name:20s} {stats((L + R) / 2)}")
