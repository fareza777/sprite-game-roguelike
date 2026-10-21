"""Tiny offline chiptune/dark-fantasy synth used to author OATHDEEP's original music and SFX.

Everything is numpy arrays of float64 in [-1, 1] at RATE Hz. No external samples or services.
"""
import wave

import numpy as np
from scipy.signal import butter, sosfilt

RATE = 44100
rng = np.random.default_rng(7)


def secs(n_samples: int) -> float:
    return n_samples / RATE


def t_axis(dur: float) -> np.ndarray:
    return np.arange(int(dur * RATE)) / RATE


def midi_hz(note: float) -> float:
    return 440.0 * 2 ** ((note - 69) / 12)


# ---------------- oscillators (phase-accumulated so pitch can glide) ----------------

def _phase(freq, n):
    f = np.asarray(freq, dtype=float)
    if f.ndim:  # tolerate off-by-one lengths from rounding
        f = f[:n] if len(f) >= n else np.pad(f, (0, n - len(f)), mode="edge")
    else:
        f = np.full(n, float(f))
    return np.cumsum(f) / RATE


def square(freq, dur, duty=0.5):
    n = int(dur * RATE)
    ph = _phase(freq, n) % 1.0
    return np.where(ph < duty, 1.0, -1.0)


def saw(freq, dur):
    n = int(dur * RATE)
    return 2.0 * (_phase(freq, n) % 1.0) - 1.0


def triangle(freq, dur):
    n = int(dur * RATE)
    ph = _phase(freq, n) % 1.0
    return 4.0 * np.abs(ph - 0.5) - 1.0


def sine(freq, dur):
    n = int(dur * RATE)
    return np.sin(2 * np.pi * _phase(freq, n))


def noise(dur):
    return rng.uniform(-1.0, 1.0, int(dur * RATE))


def sweep(f0, f1, dur, curve=1.0):
    """Frequency trajectory from f0 to f1 (exponential when curve=1)."""
    n = int(dur * RATE)
    x = np.linspace(0, 1, n) ** curve
    return f0 * (f1 / f0) ** x


# ---------------- envelopes ----------------

def adsr(n, a=0.005, d=0.05, s=0.7, r=0.1, hold=None):
    """ADSR over n samples; release starts at `hold` seconds (default: so the release ends at n)."""
    total = n / RATE
    h = hold if hold is not None else total - r
    h = max(h, a + d)
    times = [0.0, a, a + d, h, h + r]
    return np.interp(np.arange(n) / RATE, times, [0.0, 1.0, s, s, 0.0])


def decay(n, tau):
    return np.exp(-np.arange(n) / (tau * RATE))


# ---------------- filters + fx ----------------

def lowpass(x, cutoff, order=2):
    sos = butter(order, min(cutoff, RATE * 0.45), btype="low", fs=RATE, output="sos")
    return sosfilt(sos, x)


def highpass(x, cutoff, order=2):
    sos = butter(order, cutoff, btype="high", fs=RATE, output="sos")
    return sosfilt(sos, x)


def bandpass(x, lo, hi, order=2):
    sos = butter(order, [lo, min(hi, RATE * 0.45)], btype="band", fs=RATE, output="sos")
    return sosfilt(sos, x)


def sweep_lowpass(x, f0, f1):
    """Time-varying one-pole lowpass (for whooshes and filter opens)."""
    fc = np.geomspace(f0, f1, len(x))
    a = np.exp(-2 * np.pi * fc / RATE)
    y = np.empty_like(x)
    acc = 0.0
    for i in range(len(x)):
        acc = a[i] * acc + (1 - a[i]) * x[i]
        y[i] = acc
    return y


def bitcrush(x, bits=6):
    q = 2 ** (bits - 1)
    return np.round(x * q) / q


def drive(x, amount=2.0):
    return np.tanh(x * amount) / np.tanh(amount)


def comb(x, d, g):
    """y[n] = x[n] + g*y[n-d], computed a block of d samples at a time (fast for long delays)."""
    y = x.copy()
    for k in range(d, len(y), d):
        e = min(k + d, len(y))
        y[k:e] += g * y[k - d:e - d]
    return y


def allpass(x, d, g):
    """y[n] = -g*x[n] + x[n-d] + g*y[n-d], block-wise."""
    y = -g * x
    xd = np.concatenate([np.zeros(d), x[:-d]])
    y += xd
    for k in range(d, len(y), d):
        e = min(k + d, len(y))
        y[k:e] += g * y[k - d:e - d]
    return y


def delay(x, seconds, feedback=0.35, mix=0.3, tail=1.0):
    """Feedback echo, returned with `tail` seconds of decay appended."""
    d = int(seconds * RATE)
    y = np.concatenate([x, np.zeros(int(tail * RATE))])
    echo = np.concatenate([np.zeros(d), y[:-d]])
    return y + comb(echo, d, feedback) * mix


def reverb(x, size=0.6, mix=0.25, tail=2.0):
    """Cheap Schroeder-style reverb: parallel feedback combs + two allpasses."""
    y = np.concatenate([x, np.zeros(int(tail * RATE))])
    wet = np.zeros_like(y)
    for ms, g in ((29.7, 0.77), (37.1, 0.74), (41.1, 0.72), (43.7, 0.70)):
        d = int(ms * size * 2 * RATE / 1000)
        wet += comb(np.concatenate([np.zeros(d), y[:-d]]), d, g)
    for ms, g in ((5.0, 0.7), (1.7, 0.7)):
        wet = allpass(wet, int(ms * RATE / 1000), g)
    wet = lowpass(wet, 5000)
    return y * (1 - mix) + wet * mix * 0.25


# ---------------- mixing + output ----------------

def mix(length_s, *layers):
    """Sum (start_seconds, signal) layers into one buffer."""
    out = np.zeros(int(length_s * RATE))
    for start, sig in layers:
        i = int(start * RATE)
        if i >= len(out):
            continue
        n = min(len(sig), len(out) - i)
        out[i:i + n] += sig[:n]
    return out


def fade(x, fade_in=0.002, fade_out=0.01):
    x = x.copy()
    fi, fo = int(fade_in * RATE), int(fade_out * RATE)
    if fi: x[:fi] *= np.linspace(0, 1, fi)
    if fo: x[-fo:] *= np.linspace(1, 0, fo)
    return x


def normalize(x, peak_db=-1.0):
    peak = np.max(np.abs(x))
    return x if peak == 0 else x / peak * 10 ** (peak_db / 20)


def trim_silence(x, threshold=1e-3):
    idx = np.where(np.abs(x) > threshold)[0]
    return x if len(idx) == 0 else x[: idx[-1] + int(0.01 * RATE)]


def write_wav(path, x, stereo=None):
    """Write mono x, or stereo when `stereo` (right channel) is given. 16-bit PCM."""
    if stereo is not None:
        n = min(len(x), len(stereo))
        data = np.stack([x[:n], stereo[:n]], axis=1)
        ch = 2
    else:
        data = x[:, None]
        ch = 1
    pcm = (np.clip(data, -1, 1) * 32767).astype("<i2")
    with wave.open(str(path), "wb") as w:
        w.setnchannels(ch)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(pcm.tobytes())


def stats(x):
    rms = np.sqrt(np.mean(x ** 2)) + 1e-12
    return f"{secs(len(x)):.2f}s peak {20*np.log10(np.max(np.abs(x))+1e-12):.1f}dB rms {20*np.log10(rms):.1f}dB"
