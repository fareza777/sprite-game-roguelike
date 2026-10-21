"""OATHDEEP original score (chiptune / dark fantasy), composed note-by-note. usage: python music.py <out_dir>

Loops are rendered with their reverb/delay tails wrapped back onto the start so they repeat seamlessly.
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).parent))
from synth import (RATE, adsr, decay, delay, drive, highpass, lowpass, midi_hz, noise, normalize, reverb, saw,
                   sine, square, stats, sweep, triangle, write_wav)

# chord name -> voicing (MIDI, low to high); bass root is voicing[0] - 12
CHORDS = {
    "Dm": (50, 53, 57), "Bb": (46, 50, 53), "Gm": (43, 46, 50), "A": (45, 49, 52), "C": (48, 52, 55),
    "Eb": (51, 55, 58), "D": (50, 54, 57),
}


class Track:
    """Collects note events for one instrument; rendered into a mono buffer."""

    def __init__(self, length_s):
        self.buf = np.zeros(int((length_s + 4.0) * RATE))

    def add(self, start_s, sig, gain=1.0):
        i = int(start_s * RATE)
        n = min(len(sig), len(self.buf) - i)
        if n > 0:
            self.buf[i:i + n] += sig[:n] * gain


# ---------------- instruments ----------------

def pad_voice(notes, dur, bright=1800.0):
    x = sum(triangle(midi_hz(m + 12) * (1 + d), dur + 0.6) for m in notes for d in (-0.003, 0.003))
    x += 0.35 * sum(saw(midi_hz(m), dur + 0.6) for m in notes)
    return lowpass(x, bright) * adsr(len(x), 0.35, 0.3, 0.75, 0.6, hold=dur) / (2.5 * len(notes))


def bass_voice(note, dur, cutoff=700.0, grit=1.0):
    x = square(midi_hz(note), dur + 0.05, 0.5) * 0.6 + sine(midi_hz(note), dur + 0.05)
    x = lowpass(drive(x, grit), cutoff)
    return x * adsr(len(x), 0.004, 0.08, 0.7, 0.05, hold=dur)


def pluck_voice(note, dur, duty=0.125):
    x = square(midi_hz(note), dur + 0.1, duty)
    return lowpass(x, 3800) * adsr(len(x), 0.002, 0.07, 0.25, 0.08, hold=dur)


def lead_voice(note, dur, bright=False):
    n = int((dur + 0.15) * RATE)
    t = np.arange(n) / RATE
    vib = 1 + 0.006 * np.sin(2 * np.pi * 5.5 * t) * np.clip((t - 0.15) / 0.2, 0, 1)
    f = midi_hz(note) * vib
    x = triangle(f, n / RATE) + (0.55 if bright else 0.3) * square(f, n / RATE, 0.25)
    return lowpass(x, 5200 if bright else 3200) * adsr(len(x), 0.01, 0.12, 0.7, 0.15, hold=dur)


def bell_voice(note, dur):
    f = midi_hz(note)
    x = sine(f, dur + 1.0) + 0.3 * sine(f * 2.76, dur + 1.0) * decay(int((dur + 1.0) * RATE), 0.15)
    return x * decay(len(x), 0.6)


def kick(level=1.0):
    x = sine(sweep(130, 42, 0.22), 0.22) * decay(int(0.22 * RATE), 0.07)
    x[:60] += np.linspace(0.6, 0, 60)
    return x * level


def snare(level=1.0):
    n = int(0.2 * RATE)
    body = sine(190, 0.2) * decay(n, 0.05)
    hiss = highpass(noise(0.2), 1200) * decay(n, 0.06)
    return (0.5 * body + 0.8 * hiss) * level


def hat(level=1.0, open_=False):
    dur = 0.18 if open_ else 0.05
    return highpass(noise(dur), 7000) * decay(int(dur * RATE), 0.05 if open_ else 0.012) * level


def crash(level=1.0):
    return highpass(noise(1.4), 4000) * decay(int(1.4 * RATE), 0.45) * level


def bone_click(level=1.0):
    x = highpass(noise(0.03), 1500) * decay(int(0.03 * RATE), 0.004)
    return lowpass(x, 4000) * level


# ---------------- arrangement helpers ----------------

def stereo(mono_tracks, pans):
    """Equal-power pan of tracks into L/R (pan -1..1)."""
    L = sum(t * np.cos((p + 1) * np.pi / 4) for t, p in zip(mono_tracks, pans))
    R = sum(t * np.sin((p + 1) * np.pi / 4) for t, p in zip(mono_tracks, pans))
    return L, R


def wrap_loop(x, loop_len):
    """Fold everything past loop_len back onto the start so the file loops without a seam."""
    n = int(loop_len * RATE)
    out = x[:n].copy()
    tail = x[n:]
    k = 0
    while k < len(tail):
        m = min(n, len(tail) - k)
        out[:m] += tail[k:k + m]
        k += m
    return out


def finish(L, R, length_s, loop, rev_size, rev_mix, peak=-1.5):
    L = reverb(L, rev_size, rev_mix, tail=3.0)
    R = reverb(R, rev_size * 1.07, rev_mix, tail=3.0)
    L, R = highpass(L, 25, 2), highpass(R, 25, 2)   # pulse waves carry DC; block it before wrapping
    if loop:
        L, R = wrap_loop(L, length_s), wrap_loop(R, length_s)
    else:
        n = int((length_s + 2.5) * RATE)
        L, R = L[:n], R[:n]
        fo = int(1.5 * RATE)
        L[-fo:] *= np.linspace(1, 0, fo); R[-fo:] *= np.linspace(1, 0, fo)
    g = 10 ** (peak / 20) / max(np.max(np.abs(L)), np.max(np.abs(R)))
    return L * g, R * g


def melody(track, spb, bar, phrase, voice, gain=1.0):
    """phrase: [(beat_in_bar, beats, midi)] starting at `bar` (0-based)."""
    for beat, beats, note in phrase:
        track.add((bar * 4 + beat) * spb, voice(note, beats * spb), gain)


# ---------------- tracks ----------------

def crypt():
    bpm, bars = 84, 16
    spb = 60 / bpm
    length = bars * 4 * spb
    prog = ["Dm", "Bb", "Gm", "A"] * 3 + ["Dm", "Bb", "C", "A"]
    pad, bass, arp, lead, drums, perc = (Track(length) for _ in range(6))

    for b, ch in enumerate(prog):
        notes = CHORDS[ch]
        t0 = b * 4 * spb
        pad.add(t0, pad_voice(notes, 4 * spb), 1.0)
        root = notes[0] - 12
        bass.add(t0, bass_voice(root, 1.5 * spb, 600), 0.55)
        bass.add(t0 + 2 * spb, bass_voice(root + 7 if ch != "A" else root, 1.0 * spb, 600), 0.4)
        if b >= 2:
            tones = [notes[0] + 12, notes[1] + 12, notes[2] + 12, notes[0] + 24]
            order = [0, 1, 2, 3, 2, 1, 2, 3] * 2
            for i, k in enumerate(order):
                arp.add(t0 + i * spb / 4, pluck_voice(tones[k], spb / 4), 0.16 if i % 4 else 0.22)
        if b >= 4:
            drums.add(t0, kick(0.55)); drums.add(t0 + 2 * spb, kick(0.4))
            for beat in (0.5, 1.5, 2.5, 3.5):
                drums.add(t0 + beat * spb, hat(0.10))
        if b >= 8:
            perc.add(t0 + 3 * spb, bone_click(0.5)); perc.add(t0 + 3.25 * spb, bone_click(0.3))
        if b >= 12:
            drums.add(t0 + 1 * spb, snare(0.22)); drums.add(t0 + 3 * spb, snare(0.22))

    phrase_a = [
        [(0, 1.5, 69), (1.5, 0.5, 65), (2, 1, 64), (3, 1, 62)],
        [(0, 1.5, 65), (1.5, 0.5, 67), (2, 2, 69)],
        [(0, 1, 70), (1, 1, 69), (2, 1, 67), (3, 1, 65)],
        [(0, 2, 64), (2, 2, 61)],
    ]
    phrase_a2 = phrase_a[:3] + [[(0, 1, 64), (1, 1, 65), (2, 1, 67), (3, 1, 73)]]
    phrase_b = [
        [(0, 2, 74), (2, 1, 72), (3, 1, 69)],
        [(0, 1.5, 70), (1.5, 0.5, 69), (2, 2, 65)],
        [(0, 1, 67), (1, 1, 69), (2, 2, 72)],
        [(0, 2, 69), (2, 2, 64)],
    ]
    for i, bar_phrase in enumerate(phrase_a + phrase_a2 + phrase_b):
        melody(lead, spb, 4 + i, bar_phrase, lead_voice, 0.32)

    arp_wet = delay(arp.buf, spb * 0.75, 0.4, 0.45, tail=0)[: len(arp.buf)]
    lead_wet = delay(lead.buf, spb * 0.5, 0.3, 0.3, tail=0)[: len(lead.buf)]
    L, R = stereo([pad.buf, bass.buf, arp_wet, lead_wet, drums.buf, perc.buf], [-0.3, 0, 0.45, -0.1, 0, 0.3])
    return finish(L, R, length, True, 0.9, 0.35, peak=-3.0)


def boss():
    bpm, bars = 140, 16
    spb = 60 / bpm
    length = bars * 4 * spb
    prog = ["Dm", "Eb", "Dm", "C", "Dm", "Eb", "Bb", "A", "Gm", "Eb", "Dm", "C", "Bb", "C", "Dm", "A"]
    bass, stabs, lead, drums = (Track(length) for _ in range(4))

    for b, ch in enumerate(prog):
        notes = CHORDS[ch]
        t0 = b * 4 * spb
        root = notes[0] - 12
        for i, off in enumerate([0, 0, 12, 0, 0, 12, 0, 7]):
            bass.add(t0 + i * spb / 2, bass_voice(root + off, spb / 2 * 0.9, 1100, 2.5), 0.5)
        power = (notes[0], notes[0] + 7, notes[0] + 12)
        for beat, length_b in ((0, 0.75), (1.5, 0.5), (3, 0.75)):
            x = sum(saw(midi_hz(m) * (1 + d), length_b * spb) for m in power for d in (-0.006, 0.006))
            x = lowpass(drive(x * 0.4, 2.0), 2400) * adsr(len(x), 0.005, 0.08, 0.6, 0.06)
            stabs.add(t0 + beat * spb, x, 0.22)
        for beat in (0, 1.5, 2, 3.75) if b % 2 else (0, 2, 2.5):
            drums.add(t0 + beat * spb, kick(0.9))
        for beat in (1, 3):
            drums.add(t0 + beat * spb, snare(0.55))
        for i in range(8):
            drums.add(t0 + i * spb / 2, hat(0.16 if i % 2 else 0.1, open_=(i == 7)))
        if b % 4 == 0:
            drums.add(t0, crash(0.35))

    bars_lead = [
        [(0, .5, 74), (.5, .5, 74), (1, .5, 77), (1.5, .5, 76), (2, 1, 74), (3, 1, 69)],
        [(0, 1, 75), (1, .5, 74), (1.5, .5, 72), (2, 1, 70), (3, 1, 67)],
        [(0, .5, 77), (.5, .5, 77), (1, .5, 79), (1.5, .5, 77), (2, 1.5, 74), (3.5, .5, 70)],
        [(0, 1, 73), (1, 1, 76), (2, 2, 81)],
        [(0, 1.5, 79), (1.5, .5, 77), (2, 1, 74), (3, 1, 70)],
        [(0, 1, 75), (1, 1, 79), (2, 2, 82)],
        [(0, .5, 81), (.5, .5, 79), (1, .5, 77), (1.5, .5, 76), (2, 2, 74)],
        [(0, 1, 76), (1, 1, 79), (2, 2, 72)],
        [(0, .5, 74), (.5, .5, 77), (1, 1, 82), (2, 1, 81), (3, 1, 77)],
        [(0, .5, 76), (.5, .5, 79), (1, 1, 84), (2, 1, 82), (3, 1, 79)],
        [(0, 2, 81), (2, 1, 77), (3, 1, 74)],
        [(0, 1, 73), (1, 1, 76), (2, 2, 69)],
    ]
    for i, phrase in enumerate(bars_lead):
        melody(lead, spb, 4 + i, phrase, lambda n, d: lead_voice(n, d, bright=True), 0.3)

    lead_wet = delay(lead.buf, spb * 0.75, 0.25, 0.25, tail=0)[: len(lead.buf)]
    L, R = stereo([bass.buf, stabs.buf, lead_wet, drums.buf], [0, -0.35, 0.25, 0])
    return finish(L, R, length, True, 0.6, 0.18, peak=-2.0)


def title():
    bpm, bars = 70, 8
    spb = 60 / bpm
    length = bars * 4 * spb
    prog = ["Dm", "Bb", "Gm", "A"] * 2
    pad, bells, lead, bass = (Track(length) for _ in range(4))
    for b, ch in enumerate(prog):
        notes = CHORDS[ch]
        t0 = b * 4 * spb
        pad.add(t0, pad_voice(notes, 4 * spb, 1400), 1.0)
        bass.add(t0, bass_voice(notes[0] - 12, 3.5 * spb, 400), 0.4)
        for i, k in enumerate((0, 2, 1, 2)):
            bells.add(t0 + i * spb, bell_voice(notes[k] + 24, spb), 0.12)
    phrases = [
        [(0, 2, 69), (2, 2, 74)], [(0, 2, 77), (2, 2, 74)], [(0, 1, 70), (1, 1, 69), (2, 2, 67)], [(0, 2, 64), (2, 2, 61)],
        [(0, 2, 69), (2, 2, 74)], [(0, 2, 77), (2, 2, 79)], [(0, 1, 77), (1, 1, 74), (2, 2, 70)], [(0, 3, 73), (3, 1, 76)],
    ]
    for i, p in enumerate(phrases):
        melody(lead, spb, i, p, lead_voice, 0.28)
    bells_wet = delay(bells.buf, spb * 1.5, 0.45, 0.5, tail=0)[: len(bells.buf)]
    L, R = stereo([pad.buf, bells_wet, lead.buf, bass.buf], [-0.25, 0.4, 0, 0])
    return finish(L, R, length, True, 1.0, 0.45, peak=-3.0)


def victory():
    spb = 60 / 120
    lead, pad, drums = (Track(4) for _ in range(3))
    melody(lead, spb, 0, [(0, .5, 74), (.5, .5, 78), (1, .5, 81), (1.5, .5, 86), (2, 2, 86)],
           lambda n, d: lead_voice(n, d, bright=True), 0.4)
    melody(lead, spb, 1, [(0, .5, 83), (.5, .5, 85), (1, 3, 86)], lambda n, d: lead_voice(n, d, bright=True), 0.4)
    pad.add(0, pad_voice(CHORDS["D"], 8 * spb, 2400), 1.2)
    for beat in (0, 2, 4):
        drums.add(beat * spb, kick(0.7))
    drums.add(4 * spb, crash(0.4))
    L, R = stereo([lead.buf, pad.buf, drums.buf], [0.1, -0.2, 0])
    return finish(L, R, 4.0, False, 0.9, 0.35, peak=-2.0)


def game_over():
    spb = 60 / 60
    lead, pad = Track(5), Track(5)
    melody(lead, spb, 0, [(0, 1, 74), (1, 1, 72), (2, 1, 70), (3, 2, 69)], lead_voice, 0.35)
    pad.add(0, pad_voice(CHORDS["Dm"], 3 * spb, 1000), 1.2)
    pad.add(3 * spb, pad_voice((45, 50, 53), 2.5 * spb, 800), 1.2)
    L, R = stereo([lead.buf, pad.buf], [0.1, -0.1])
    return finish(L, R, 5.0, False, 1.0, 0.4, peak=-3.0)


def choir_voice(notes, dur, vowel=(700, 1150)):
    """Wordless 'aah' choir: detuned saw stacks through two formant bands, slow swell."""
    n = int((dur + 1.2) * RATE)
    t = np.arange(n) / RATE
    x = np.zeros(n)
    for m in notes:
        for d in (-0.006, 0.0, 0.006):
            vib = 1 + 0.004 * np.sin(2 * np.pi * (5.0 + d * 50) * t)
            x += saw(midi_hz(m) * (1 + d) * vib, n / RATE)
    from synth import bandpass
    f = bandpass(x, vowel[0] * 0.7, vowel[0] * 1.3) + 0.6 * bandpass(x, vowel[1] * 0.75, vowel[1] * 1.25)
    return f * adsr(n, 0.9, 0.4, 0.8, 1.2, hold=dur) / (3 * len(notes))


def bell_toll(note, level=1.0):
    dur = 4.0
    f = midi_hz(note)
    n = int(dur * RATE)
    parts = [(1.0, 1.0, 1.6), (2.0, 0.5, 1.0), (2.76, 0.35, 0.7), (5.4, 0.15, 0.35), (0.5, 0.4, 2.2)]
    x = sum(a * sine(f * r, dur) * decay(n, tau) for r, a, tau in parts)
    return x * level


def prologue():
    bpm, bars = 60, 16
    spb = 60 / bpm
    length = bars * 4 * spb
    prog = ["Dm", "Bb", "Gm", "A", "Dm", "Bb", "C", "A"] * 2
    choir, pad, bells, bass = (Track(length) for _ in range(4))
    for b, ch in enumerate(prog):
        notes = CHORDS[ch]
        t0 = b * 4 * spb
        pad.add(t0, pad_voice(notes, 4 * spb, 1100), 0.8)
        bass.add(t0, bass_voice(notes[0] - 12, 3.8 * spb, 300), 0.35)
        if b >= 2:
            choir.add(t0, choir_voice([n + 12 for n in notes], 4 * spb, (650, 1100) if b % 2 else (500, 900)), 1.2)
        if b % 4 == 0:
            bells.add(t0, bell_toll(notes[0] - 12, 0.5))
    L, R = stereo([choir.buf, pad.buf, bells.buf, bass.buf], [0.1, -0.2, 0.0, 0.0])
    return finish(L, R, length, True, 1.2, 0.5, peak=-3.0)


def ending():
    bpm, bars = 66, 12
    spb = 60 / bpm
    length = bars * 4 * spb
    prog = ["Dm", "Bb", "C", "A", "Bb", "C", "D", "D", "Gm", "A", "D", "D"]
    choir, pad, lead, bells = (Track(length) for _ in range(4))
    for b, ch in enumerate(prog):
        notes = CHORDS[ch]
        t0 = b * 4 * spb
        pad.add(t0, pad_voice(notes, 4 * spb, 1600 + b * 150), 0.8)
        choir.add(t0, choir_voice([n + 12 for n in notes], 4 * spb), 0.9 + b * 0.05)
        if b >= 6:
            for i, k in enumerate((0, 1, 2, 1)):
                bells.add(t0 + i * spb, bell_voice(notes[k] + 24, spb), 0.1)
    phrase = [[(0, 2, 74), (2, 2, 78)], [(0, 3, 81)], [(0, 1, 79), (1, 1, 78), (2, 2, 76)], [(0, 4, 74)]]
    for i, p in enumerate(phrase):
        melody(lead, spb, 6 + i, p, lambda n, d: lead_voice(n, d, bright=True), 0.25)
    L, R = stereo([choir.buf, pad.buf, lead.buf, bells.buf], [-0.1, 0.2, 0.0, 0.3])
    return finish(L, R, length, False, 1.1, 0.45, peak=-3.0)


TRACKS = {"music_prologue": prologue, "music_ending": ending, "music_crypt": crypt, "music_boss": boss, "music_title": title, "sting_victory": victory, "sting_gameover": game_over}

if __name__ == "__main__":
    out = Path(sys.argv[1])
    out.mkdir(parents=True, exist_ok=True)
    only = sys.argv[2:] or list(TRACKS)
    for name in only:
        L, R = TRACKS[name]()
        write_wav(out / f"{name}.wav", L, R)
        print(f"{name:15s} {stats((L + R) / 2)}")
