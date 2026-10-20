"""Create two gentle, original CG music loops for the Qianqing Palace level."""
import math
import os
import re
import uuid
import wave

import numpy as np

ROOT = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
                    "Assets", "Resources", "Audio")
RATE = 44100


def note(track, start, pitch, length, volume, pad=False):
    begin = int(start * RATE)
    count = min(int(length * RATE), len(track) - begin)
    if count <= 0:
        return
    t = np.arange(count, dtype=np.float64) / RATE
    freq = 440.0 * 2 ** ((pitch - 69) / 12)
    attack = np.minimum(1.0, t / (0.16 if pad else 0.035))
    release = np.minimum(1.0, np.maximum(0.0, (length - t) / (0.55 if pad else 0.32)))
    env = attack * release
    if pad:
        env *= 0.78 + 0.22 * np.sin(np.pi * np.minimum(t / max(length, 0.01), 1.0))
        signal = (np.sin(2 * np.pi * freq * t) + 0.18 * np.sin(4 * np.pi * freq * t))
    else:
        env *= np.exp(-t / max(0.4, length * 0.62))
        signal = (np.sin(2 * np.pi * freq * t)
                  + 0.28 * np.sin(2 * np.pi * freq * 2.01 * t)
                  + 0.09 * np.sin(2 * np.pi * freq * 3.98 * t))
    track[begin:begin + count] += volume * env * signal


def build(name, progression, melody, duration=32.0):
    mono = np.zeros(int(RATE * duration), dtype=np.float64)
    bar = 8.0
    for bar_index, chord in enumerate(progression):
        start = bar_index * bar
        # Quiet sustained harmony under sparse, rounded piano-like notes.
        for pitch in chord:
            note(mono, start, pitch, bar, 0.013, pad=True)
        for offset, pitch in melody[bar_index]:
            note(mono, start + offset, pitch, 2.0, 0.034)
    # Gentle stereo width without hard panning or bright high frequencies.
    left = mono.copy()
    right = np.roll(mono, int(0.014 * RATE)) * 0.93
    stereo = np.column_stack((left, right))
    # Crossfade the loop seam so the CG can be longer than the music phrase.
    fade = int(2.0 * RATE)
    blend = np.linspace(0.0, 1.0, fade, endpoint=False)[:, None]
    stereo[-fade:] = stereo[-fade:] * (1.0 - blend) + stereo[:fade] * blend
    stereo *= 0.22 / max(np.max(np.abs(stereo)), 1e-9)
    pcm = (np.clip(stereo, -1.0, 1.0) * 32767).astype("<i2")
    path = os.path.join(ROOT, name + ".wav")
    with wave.open(path, "wb") as out:
        out.setnchannels(2)
        out.setsampwidth(2)
        out.setframerate(RATE)
        out.writeframes(pcm.tobytes())
    meta_path = path + ".meta"
    if os.path.exists(meta_path):
        meta = open(meta_path, encoding="utf-8").read()
    else:
        template_path = os.path.join(ROOT, "Game_BGM.wav.meta")
        meta = open(template_path, encoding="utf-8").read()
        meta = re.sub(r"^guid: .*?$", "guid: " + uuid.uuid4().hex, meta,
                      count=1, flags=re.MULTILINE)
    meta = re.sub(r"^  3D: .*?$", "  3D: 0", meta, count=1, flags=re.MULTILINE)
    with open(meta_path, "w", encoding="utf-8", newline="\n") as out:
        out.write(meta)
    print(f"{name}: {duration:.0f}s stereo, peak={np.max(np.abs(stereo)):.3f}")


opening_progression = [
    [50, 57, 62, 66],  # D
    [47, 54, 59, 62],  # Bm
    [43, 50, 55, 59],  # G
    [45, 52, 57, 61],  # A
]
opening_melody = [
    [(1.0, 74), (4.4, 69)], [(0.8, 71), (4.8, 66)],
    [(1.2, 67), (4.6, 71)], [(1.0, 69), (5.0, 73)],
]
ending_progression = [
    [43, 50, 55, 59],  # G
    [45, 52, 57, 61],  # A
    [40, 47, 52, 55],  # Em
    [50, 57, 62, 66],  # D, resolved
]
ending_melody = [
    [(1.1, 71), (5.0, 74)], [(0.9, 73), (4.7, 76)],
    [(1.2, 71), (4.9, 67)], [(1.0, 69), (5.2, 74)],
]

# A separate, slower phrase for the game's actual final ED video.
credits_progression = [
    [48, 55, 60, 64],  # C
    [45, 52, 57, 60],  # Am
    [41, 48, 53, 57],  # F
    [43, 50, 55, 59],  # G
]
credits_melody = [
    [(1.4, 72), (5.4, 76)], [(1.8, 69), (5.6, 72)],
    [(1.4, 69), (5.2, 74)], [(1.8, 71), (5.6, 67)],
]

os.makedirs(ROOT, exist_ok=True)
build("QianQing_CG_Opening_BGM", opening_progression, opening_melody)
build("QianQing_CG_Clear_BGM", ending_progression, ending_melody)
build("Scene_Ending_BGM", credits_progression, credits_melody)
