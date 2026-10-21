"""Narration for UNDERSUN: the buried Sun speaks.

Reads Oathdeep/Assets/_Game/Story/story.json, voices every prologue / ending / floor-entry line with the offline
Windows SAPI voice (sapi_synth.ps1), then processes each take into an ethereal narrator: slightly slower and lower,
cathedral reverb, a faint high shimmer, gentle EQ and levelling.

usage: python vo.py <story.json> <out_dir>
"""
import json
import subprocess
import sys
import tempfile
import wave
from pathlib import Path

import numpy as np
from scipy.signal import resample_poly

sys.path.insert(0, str(Path(__file__).parent))
from synth import RATE, highpass, lowpass, normalize, reverb, stats, write_wav

HERE = Path(__file__).parent


def read_wav(path):
    with wave.open(str(path), "rb") as w:
        assert w.getsampwidth() == 2, "expected 16-bit PCM"
        data = np.frombuffer(w.readframes(w.getnframes()), dtype="<i2").astype(np.float64) / 32768.0
        if w.getnchannels() == 2:
            data = data.reshape(-1, 2).mean(axis=1)
        if w.getframerate() != RATE:
            data = resample_poly(data, RATE, w.getframerate())
    return data


def compress(x, threshold=0.25, ratio=3.0):
    """Gentle peak-ish compression so whispers and stresses sit at a similar level."""
    env = np.abs(x)
    k = int(0.01 * RATE)
    env = np.convolve(env, np.ones(k) / k, mode="same")
    gain = np.where(env > threshold, (threshold + (env - threshold) / ratio) / np.maximum(env, 1e-9), 1.0)
    return x * gain


def ethereal(x):
    # ~6% slower and lower: an older, larger voice
    x = resample_poly(x, 50, 47)
    x = highpass(x, 95, 2)
    x = compress(normalize(x, -3))
    # shimmer: the airy top end, delayed and washed out, like light around the voice
    shimmer = np.concatenate([np.zeros(int(0.045 * RATE)), highpass(x, 3500, 2)])[: len(x)] * 0.22
    voice = x + shimmer
    wet = reverb(voice, size=1.35, mix=0.42, tail=2.2)
    out = lowpass(wet, 9500)
    # trim leading silence from SAPI, keep a soft tail
    idx = np.where(np.abs(out) > 0.01)[0]
    if len(idx):
        out = out[max(0, idx[0] - int(0.05 * RATE)): idx[-1] + int(0.6 * RATE)]
    fade = int(0.3 * RATE)
    out[-fade:] *= np.linspace(1, 0, fade)
    return normalize(out, -2.0)


def main(story_path, out_dir):
    story = json.loads(Path(story_path).read_text(encoding="utf-8"))
    lines = [{"id": p["id"], "text": p["text"]} for p in story["prologue"] + story["ending"]]
    lines += [{"id": f["id"], "text": f["entry"]} for f in story["floors"]]
    out = Path(out_dir)
    out.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory() as tmp:
        lines_json = Path(tmp) / "lines.json"
        lines_json.write_text(json.dumps(lines), encoding="utf-8")
        raw = Path(tmp) / "raw"
        subprocess.run(["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", str(HERE / "sapi_synth.ps1"),
                        "-Lines", str(lines_json), "-OutDir", str(raw)], check=True, capture_output=True)
        for line in lines:
            x = ethereal(read_wav(raw / f"{line['id']}.wav"))
            write_wav(out / f"vo_{line['id']}.wav", x)
            print(f"vo_{line['id']:10s} {stats(x)}")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
