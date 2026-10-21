"""OATHDEEP sound effects, synthesized from scratch. usage: python sfx.py <out_dir>"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).parent))
from synth import (RATE, adsr, bandpass, bitcrush, decay, delay, drive, fade, highpass, lowpass, midi_hz, mix,
                   noise, normalize, reverb, rng, saw, sine, square, stats, sweep, sweep_lowpass, triangle,
                   trim_silence, write_wav)


def n_of(dur):
    return int(dur * RATE)


def thump(f0, f1, dur, tau):
    return sine(sweep(f0, f1, dur), dur) * decay(n_of(dur), tau)


def burst(dur, lo, hi, tau):
    return bandpass(noise(dur), lo, hi) * decay(n_of(dur), tau)


def partials(freqs, dur, taus, amps=None):
    amps = amps or [1.0] * len(freqs)
    out = np.zeros(n_of(dur))
    for f, tau, a in zip(freqs, taus, amps):
        out += a * sine(f, dur) * decay(n_of(dur), tau)
    return out


def whoosh(dur, f0, f1, attack=0.25, level=1.0):
    n = n_of(dur)
    env = adsr(n, a=dur * attack, d=dur * 0.2, s=0.5, r=dur * 0.45)
    return sweep_lowpass(highpass(noise(dur), 250), f0, f1) * env * level


def click(dur=0.004):
    return highpass(noise(dur), 2500) * decay(n_of(dur), 0.0012)


def notes(seq, wave_fn, gap=0.0, env=(0.002, 0.04, 0.5, 0.08)):
    """seq: [(midi, dur)] played back to back."""
    parts, t = [], 0.0
    for note, dur in seq:
        x = wave_fn(midi_hz(note), dur) * adsr(n_of(dur), *env)
        parts.append((t, x))
        t += dur + gap
    return mix(t + 0.05, *parts)


# ---------------- combat ----------------

def swing():
    w = whoosh(0.2, 700, 5200, attack=0.3)
    shing = partials([2600, 3900], 0.12, [0.03, 0.02], [0.08, 0.05])
    return mix(0.24, (0, w), (0.04, shing))


def swing_heavy():
    w = whoosh(0.32, 400, 3800, attack=0.35)
    return mix(0.36, (0, w), (0.08, partials([1900, 2850], 0.15, [0.04, 0.03], [0.07, 0.04])))


def hit():
    body = thump(190, 55, 0.14, 0.045)
    crunch = burst(0.08, 700, 5000, 0.018) * 0.7
    grit = bitcrush(square(85, 0.05) * decay(n_of(0.05), 0.012), 4) * 0.25
    return drive(mix(0.16, (0, body), (0, crunch), (0, grit)), 1.8)


def hit_crit():
    ring = partials([1760, 2640, 3520], 0.25, [0.06, 0.05, 0.03], [0.35, 0.22, 0.12])
    spark = burst(0.06, 4000, 12000, 0.012) * 0.5
    return mix(0.28, (0, hit() * 0.9), (0, ring), (0.005, spark))


def player_hurt():
    body = thump(140, 45, 0.2, 0.06)
    blip = lowpass(square(sweep(520, 180, 0.18), 0.18, 0.25), 2500) * adsr(n_of(0.18), 0.002, 0.03, 0.6, 0.1) * 0.35
    grit = burst(0.06, 500, 3000, 0.015) * 0.5
    return drive(mix(0.24, (0, body), (0.01, blip), (0, grit)), 1.5)


def enemy_death():
    layers = [(0, thump(220, 60, 0.3, 0.09) * 0.6)]
    for _ in range(7):
        t = rng.uniform(0.02, 0.32)
        layers.append((t, bandpass(click(0.01), 1200, 4200) * rng.uniform(1.2, 2.4)))
    layers.append((0.02, whoosh(0.35, 3000, 400, attack=0.1, level=0.4)))
    return mix(0.42, *layers)


def slime_squish():
    dur = 0.28
    f = sweep(320, 110, dur) * (1 + 0.18 * np.sin(2 * np.pi * 28 * np.arange(n_of(dur)) / RATE))
    body = lowpass(sine(f, dur) + 0.4 * triangle(f * 2, dur), 1500) * adsr(n_of(dur), 0.005, 0.05, 0.6, 0.15)
    bubbles = [(rng.uniform(0, 0.2), thump(rng.uniform(500, 900), 250, 0.05, 0.015) * 0.4) for _ in range(4)]
    return mix(0.3, (0, body), *bubbles)


def dodge():
    return whoosh(0.18, 1500, 7000, attack=0.2, level=0.8)


def bash():
    clang = partials([220, 563, 913, 1347, 2110], 0.55, [0.25, 0.18, 0.12, 0.08, 0.05], [0.6, 0.45, 0.35, 0.25, 0.15])
    return drive(mix(0.6, (0, lowpass(clang, 6000)), (0, thump(160, 50, 0.15, 0.04)),
                     (0, burst(0.05, 800, 6000, 0.012) * 0.6)), 1.6)


def arrow_shoot():
    # Karplus-Strong pluck for the bowstring
    period = int(RATE / 170)
    buf = rng.uniform(-1, 1, period)
    out = np.zeros(n_of(0.22))
    for i in range(len(out)):
        out[i] = buf[i % period]
        buf[i % period] = 0.5 * (buf[i % period] + buf[(i + 1) % period]) * 0.993
    return mix(0.24, (0, lowpass(out, 3000) * 0.8), (0.01, whoosh(0.14, 2000, 6000, 0.1, 0.5)))


def spike():
    shk = highpass(noise(0.12), 3000) * decay(n_of(0.12), 0.03)
    ring = partials([3100, 4650], 0.15, [0.03, 0.02], [0.3, 0.2])
    return mix(0.16, (0, shk), (0, ring), (0, thump(300, 120, 0.05, 0.015) * 0.5))


# ---------------- boss ----------------

def boss_slam():
    boom = thump(95, 28, 1.1, 0.35)
    crack = burst(0.08, 300, 7000, 0.02) * 0.9
    rumble = lowpass(noise(1.0), 260) * decay(n_of(1.0), 0.3) * 1.5
    debris = [(rng.uniform(0.05, 0.5), bandpass(click(0.012), 800, 3000) * 1.2) for _ in range(10)]
    x = drive(mix(1.2, (0, boom), (0, crack), (0, rumble), *debris), 2.2)
    return trim_silence(reverb(x, size=0.9, mix=0.25, tail=1.0))


def boss_cast():
    dur = 1.3
    n = n_of(dur)
    chord = sum(saw(midi_hz(m) * (1 + d), dur) for m in (50, 57, 62, 65) for d in (-0.004, 0.004))
    chord = sweep_lowpass(chord, 300, 3200) * adsr(n, 0.45, 0.2, 0.7, 0.5) * 0.18
    trem = 1 + 0.35 * np.sin(2 * np.pi * 7 * np.arange(n) / RATE)
    shimmer = sum(sine(midi_hz(m), dur) for m in (86, 89, 93)) * adsr(n, 0.6, 0.2, 0.5, 0.4) * 0.05
    return trim_silence(reverb(chord * trem + shimmer, size=1.0, mix=0.4, tail=1.5))


def boss_roar():
    dur = 1.5
    n = n_of(dur)
    vib = 1 + 0.04 * np.sin(2 * np.pi * 6 * np.arange(n) / RATE)
    f = sweep(95, 62, dur, 0.6) * vib
    growl = saw(f, dur) + 0.6 * saw(f * 1.01, dur) + 0.5 * noise(dur)
    growl = bandpass(growl, 150, 1200) * adsr(n, 0.12, 0.3, 0.75, 0.5)
    return trim_silence(reverb(drive(growl, 3.0) * 0.8, size=1.0, mix=0.35, tail=1.2))


# ---------------- world ----------------

def coin():
    return lowpass(notes([(83, 0.06), (88, 0.2)], lambda f, d: square(f, d, 0.25), env=(0.001, 0.05, 0.45, 0.12)), 7000)


def potion():
    gulps = [(t, lowpass(sine(sweep(260, 520, 0.08), 0.08) * adsr(n_of(0.08), 0.005, 0.02, 0.6, 0.04), 1500))
             for t in (0, 0.1)]
    sparkle = delay(notes([(81, 0.05), (85, 0.05), (88, 0.05), (93, 0.18)], triangle), 0.09, 0.3, 0.3, 0.3)
    return trim_silence(mix(0.9, *gulps, (0.2, sparkle * 0.7)))


def item_pickup():
    arp = notes([(74, 0.07), (78, 0.07), (81, 0.07), (86, 0.3)], lambda f, d: square(f, d, 0.125))
    return trim_silence(reverb(delay(lowpass(arp, 6000), 0.11, 0.35, 0.35, 0.4), 0.5, 0.2, 0.6))


def chest_open():
    dur = 0.35
    creak = bandpass(saw(sweep(85, 150, dur) * (1 + 0.03 * rng.standard_normal(n_of(dur))), dur), 300, 2400)
    creak *= adsr(n_of(dur), 0.03, 0.1, 0.6, 0.15) * 0.5
    thunk = thump(180, 70, 0.12, 0.04) * 0.8
    shimmer = [(0.25 + 0.05 * k, sine(midi_hz(88 + 3 * k), 0.25) * decay(n_of(0.25), 0.06) * 0.25) for k in range(5)]
    return trim_silence(reverb(mix(0.9, (0, creak), (0.3, thunk), *shimmer), 0.6, 0.2, 0.6))


def breakable():
    layers = [(0, thump(240, 80, 0.1, 0.03) * 0.8)]
    for _ in range(9):
        t = rng.uniform(0, 0.18)
        layers.append((t, highpass(noise(0.03), 1800) * decay(n_of(0.03), 0.006) * rng.uniform(0.4, 1.0)))
    return mix(0.3, *layers)


def door_slam():
    boom = thump(75, 38, 0.7, 0.2)
    metal = partials([110, 287, 455, 690, 1010], 0.8, [0.3, 0.22, 0.16, 0.1, 0.06], [0.5, 0.4, 0.3, 0.2, 0.12])
    rattle = [(rng.uniform(0.05, 0.35), bandpass(click(0.008), 1500, 5000) * 0.8) for _ in range(8)]
    x = drive(mix(0.9, (0, boom), (0, lowpass(metal, 4000)), *rattle), 1.8)
    return trim_silence(reverb(x, 0.8, 0.25, 0.8))


def door_open():
    scrape = bandpass(noise(0.55), 900, 3500) * adsr(n_of(0.55), 0.1, 0.1, 0.5, 0.25) * 0.4
    rattle = [(0.03 * k + rng.uniform(0, 0.02), bandpass(click(0.008), 1500 + 150 * k, 5000) * 0.9) for k in range(14)]
    return mix(0.65, (0, scrape), *rattle)


def spawn():
    dur = 0.7
    n = n_of(dur)
    swell = sweep_lowpass(noise(dur), 250, 6000) * np.linspace(0, 1, n) ** 2 * 0.7
    tone = sine(sweep(220, 660, dur), dur) * np.linspace(0, 1, n) ** 3 * 0.25
    return fade(swell + tone, 0.002, 0.03)


def room_clear():
    lead = notes([(74, 0.1), (78, 0.1), (81, 0.1), (86, 0.5)], lambda f, d: 0.6 * square(f, d, 0.25) + triangle(f, d))
    pad = sum(triangle(midi_hz(m), 0.8) for m in (62, 66, 69)) * adsr(n_of(0.8), 0.02, 0.2, 0.5, 0.4) * 0.25
    return trim_silence(reverb(mix(1.0, (0, lowpass(lead, 7000) * 0.6), (0.3, pad)), 0.7, 0.3, 1.0))


def ui_click():
    return lowpass(notes([(84, 0.025), (91, 0.02)], lambda f, d: square(f, d, 0.5), env=(0.001, 0.01, 0.6, 0.01)), 8000)


def ui_confirm():
    return lowpass(notes([(81, 0.06), (88, 0.14)], lambda f, d: square(f, d, 0.25), env=(0.001, 0.03, 0.5, 0.08)), 7000)



# ---------------- heroes ----------------

def crossbow():
    clack = burst(0.03, 1500, 6000, 0.006) * 0.9
    twang = partials([310, 620, 935], 0.25, [0.08, 0.05, 0.03], [0.6, 0.3, 0.15])
    return mix(0.28, (0, clack), (0, twang), (0.008, whoosh(0.16, 2500, 7000, 0.1, 0.6)))


def flare():
    hiss = bandpass(noise(0.45), 1200, 6000) * adsr(n_of(0.45), 0.01, 0.1, 0.5, 0.25)
    pop = thump(400, 120, 0.12, 0.03)
    return mix(0.45, (0, hiss * 0.8), (0, pop))


def soul_cast():
    dur = 0.5
    n = n_of(dur)
    tone = sum(sine(midi_hz(m) * (1 + 0.01 * np.sin(2 * np.pi * 9 * np.arange(n) / RATE)), dur) for m in (74, 81, 86))
    return trim_silence(reverb(tone * adsr(n, 0.02, 0.1, 0.5, 0.3) * 0.35, size=0.7, mix=0.35, tail=0.6))


def toll():
    bell = partials([196, 392, 467, 588, 784, 1175], 2.0, [1.2, 0.9, 0.7, 0.5, 0.35, 0.2], [0.8, 0.5, 0.45, 0.3, 0.2, 0.1])
    impact = thump(120, 40, 0.4, 0.12)
    return trim_silence(reverb(mix(2.0, (0, bell), (0, impact)), size=0.9, mix=0.3, tail=1.2))


def shadowstep():
    out = whoosh(0.22, 6000, 800, attack=0.1, level=0.9)
    sub = thump(200, 60, 0.2, 0.06) * 0.6
    return mix(0.25, (0, out), (0.1, sub))


# ---------------- enemies & world ----------------

def enemy_shot():
    return mix(0.2, (0, thump(900, 300, 0.12, 0.03) * 0.7), (0, burst(0.05, 2000, 6000, 0.01) * 0.4))


def explosion():
    boom = thump(120, 30, 0.8, 0.25)
    crack = burst(0.1, 400, 7000, 0.03)
    rumble = lowpass(noise(0.7), 300) * decay(n_of(0.7), 0.2)
    return trim_silence(drive(mix(0.9, (0, boom), (0, crack), (0, rumble)), 1.8))


def boon():
    arp = notes([(74, 0.09), (78, 0.09), (81, 0.09), (86, 0.4)], lambda f, d: sine(f, d) + 0.3 * triangle(f * 2, d), env=(0.002, 0.08, 0.6, 0.3))
    return trim_silence(reverb(arp * 0.6, size=0.8, mix=0.4, tail=0.8))


def quest_complete():
    fan = notes([(72, 0.1), (76, 0.1), (79, 0.1), (84, 0.45)], lambda f, d: square(f, d, 0.25), env=(0.001, 0.05, 0.55, 0.2))
    return lowpass(fan, 6000)


# name: (builder, peak dBFS) — peak sets the relative loudness of each sound in the mix
SFX = {
    "swing": (swing, -4), "swing_heavy": (swing_heavy, -3), "hit": (hit, -2), "hit_crit": (hit_crit, -1),
    "player_hurt": (player_hurt, -1), "enemy_death": (enemy_death, -3), "slime_squish": (slime_squish, -4),
    "dodge": (dodge, -6), "bash": (bash, -2), "arrow_shoot": (arrow_shoot, -5), "spike": (spike, -6),
    "boss_slam": (boss_slam, -1), "boss_cast": (boss_cast, -3), "boss_roar": (boss_roar, -1),
    "coin": (coin, -11), "potion": (potion, -4), "item_pickup": (item_pickup, -6), "chest_open": (chest_open, -3),
    "break": (breakable, -4), "door_slam": (door_slam, -2), "door_open": (door_open, -5), "spawn": (spawn, -7),
    "room_clear": (room_clear, -3), "ui_click": (ui_click, -9), "ui_confirm": (ui_confirm, -7),
    "crossbow": (crossbow, -5), "flare": (flare, -4), "soul_cast": (soul_cast, -6), "toll": (toll, -1),
    "shadowstep": (shadowstep, -4), "enemy_shot": (enemy_shot, -8), "explosion": (explosion, -2),
    "boon": (boon, -5), "quest_complete": (quest_complete, -6),
}

if __name__ == "__main__":
    out = Path(sys.argv[1])
    out.mkdir(parents=True, exist_ok=True)
    for name, (build, peak) in SFX.items():
        x = normalize(fade(highpass(build(), 30, 2), 0.001, 0.01), peak)
        write_wav(out / f"{name}.wav", x)
        print(f"{name:14s} {stats(x)}")
