import math
import os
import re
import uuid
import wave

import numpy as np

PROJECT_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ROOT = os.path.join(PROJECT_ROOT, "Assets", "Resources", "Audio")
RATE = 44100
DURATION = 60
COUNT = RATE * DURATION


def colored_noise(seed, low, high):
    rng = np.random.default_rng(seed)
    spectrum = np.fft.rfft(rng.normal(0.0, 1.0, COUNT))
    frequencies = np.fft.rfftfreq(COUNT, 1.0 / RATE)
    band = (frequencies >= low) & (frequencies <= high)
    shape = np.zeros_like(frequencies)
    shape[band] = 1.0 / np.sqrt(np.maximum(frequencies[band], 1.0))
    result = np.fft.irfft(spectrum * shape, n=COUNT)
    return result / max(1e-9, np.sqrt(np.mean(result * result)))


def add_tone(track, start, frequency, duration, amplitude, decay, phase=0.0):
    begin = int(start * RATE)
    length = min(int(duration * RATE), COUNT - begin)
    if length <= 0:
        return
    time = np.arange(length, dtype=np.float64) / RATE
    envelope = np.exp(-time / decay) * np.sin(np.pi * np.minimum(time / max(duration, 1e-4), 1.0)) ** 0.45
    track[begin:begin + length] += amplitude * envelope * np.sin(2 * np.pi * frequency * time + phase)


def add_chirp(track, start, duration, low, high, amplitude, pan=0.0):
    begin = int(start * RATE)
    length = int(duration * RATE)
    time = np.arange(length, dtype=np.float64) / RATE
    phase = 2 * np.pi * (low * time + (high - low) * time * time / (2 * duration))
    envelope = np.sin(np.pi * np.arange(length) / max(1, length - 1)) ** 1.6
    chirp = amplitude * envelope * (np.sin(phase) + 0.20 * np.sin(phase * 2.02))
    end = min(COUNT, begin + length)
    track[begin:end, 0] += chirp[:end - begin] * math.sqrt((1.0 - pan) * 0.5)
    track[begin:end, 1] += chirp[:end - begin] * math.sqrt((1.0 + pan) * 0.5)


def qianqing_ambience():
    left = colored_noise(281, 42, 1150)
    right = colored_noise(282, 42, 1150)
    hiss_l = colored_noise(283, 500, 3600)
    hiss_r = colored_noise(284, 500, 3600)
    t = np.arange(COUNT, dtype=np.float64) / RATE
    # Several slow, incommensurate envelopes make the courtyard wind breathe
    # naturally instead of cycling at one obvious rate.
    swell = (0.67 + 0.12 * np.sin(2 * np.pi * 0.031 * t + 0.4)
             + 0.075 * np.sin(2 * np.pi * 0.067 * t + 1.1)
             + 0.045 * np.sin(2 * np.pi * 0.113 * t + 2.0))
    track = np.empty((COUNT, 2), dtype=np.float64)
    track[:, 0] = 0.095 * left * swell + 0.012 * hiss_l
    track[:, 1] = 0.095 * right * (0.78 + 0.22 * swell) + 0.012 * hiss_r

    # Soft gusts cross the stereo field at uneven intervals.
    gust_noise = colored_noise(285, 180, 1900)
    for start, duration, pan, strength in [(4.5, 4.2, -0.55, 0.010), (16.0, 5.0, 0.48, 0.013),
                                           (28.4, 3.7, -0.25, 0.008), (40.2, 5.4, 0.62, 0.012),
                                           (52.0, 4.0, -0.42, 0.009)]:
        begin, length = int(start * RATE), int(duration * RATE)
        x = np.arange(length) / RATE
        env = np.sin(np.pi * x / duration) ** 1.5
        burst = strength * env * gust_noise[begin:begin + length]
        track[begin:begin + length, 0] += burst * math.sqrt((1 - pan) * 0.5)
        track[begin:begin + length, 1] += burst * math.sqrt((1 + pan) * 0.5)

    # Bird calls vary in pitch, spacing and direction; each cluster has a
    # short answer from farther away to give the courtyard a sense of depth.
    calls = [
        (6.8, -0.72, 2580, 0.020), (7.16, -0.72, 3250, 0.016), (7.78, -0.48, 2860, 0.013),
        (17.6, 0.62, 3100, 0.017), (18.02, 0.62, 3650, 0.020), (18.72, 0.25, 2700, 0.012),
        (32.4, -0.35, 2350, 0.017), (32.8, -0.35, 3000, 0.019), (33.52, 0.1, 3500, 0.012),
        (45.3, 0.72, 2750, 0.018), (45.72, 0.72, 3400, 0.018), (46.4, 0.4, 2500, 0.012),
        (53.2, -0.45, 3200, 0.017), (53.68, -0.45, 2500, 0.014), (54.34, 0.0, 3700, 0.011),
    ]
    for start, pan, pitch, level in calls:
        add_chirp(track, start, 0.30 + (pitch % 4) * 0.035, pitch, pitch * 1.42, level, pan)

    # Quiet leaf/flag rustles appear as brief, airy swishes beneath the calls.
    rustle = colored_noise(286, 700, 5200)
    for start, duration, pan, strength in [(10.1, 1.25, 0.55, 0.006), (23.8, 1.65, -0.65, 0.007),
                                           (37.0, 1.15, 0.25, 0.005), (48.0, 1.45, -0.35, 0.006)]:
        begin, length = int(start * RATE), int(duration * RATE)
        x = np.arange(length) / RATE
        env = np.sin(np.pi * x / duration) ** 2
        swish = strength * env * rustle[begin:begin + length]
        track[begin:begin + length, 0] += swish * math.sqrt((1 - pan) * 0.5)
        track[begin:begin + length, 1] += swish * math.sqrt((1 + pan) * 0.5)

    # A few distant palace-bell strikes use slightly different partials and
    # staggered echoes, avoiding a repeated musical phrase.
    for index, (start, freq) in enumerate([(11.4, 735), (29.6, 662), (43.8, 812), (55.6, 704)]):
        add_tone(track[:, 0], start, freq, 4.2, 0.018, 1.55, index * 0.23)
        add_tone(track[:, 1], start + 0.22, freq * 1.004, 4.5, 0.016, 1.7, 0.4 + index * 0.11)
        add_tone(track[:, 0], start + 0.035, freq * 2.36, 2.3, 0.0055, 0.82, 0.5)
        add_tone(track[:, 1], start + 0.31, freq * 2.73, 1.7, 0.0045, 0.67, 0.2)
        add_tone(track[:, 0], start + 0.48, freq * 1.51, 2.0, 0.0035, 0.9, 0.1)
    return track


def taihe_ambience():
    left = colored_noise(551, 48, 700)
    right = colored_noise(552, 48, 700)
    t = np.arange(COUNT, dtype=np.float64) / RATE
    breathing = 0.74 + 0.12 * np.sin(2 * np.pi * 0.027 * t + 1.2) + 0.07 * np.sin(2 * np.pi * 0.061 * t)
    track = np.empty((COUNT, 2), dtype=np.float64)
    track[:, 0] = 0.075 * left * breathing
    track[:, 1] = 0.075 * right * (0.84 + 0.16 * breathing)
    high = colored_noise(553, 900, 4200)
    track[:, 0] += 0.005 * high
    track[:, 1] += 0.0045 * np.roll(high, int(0.013 * RATE))
    for index, start in enumerate([5.2, 14.8, 25.6, 37.1, 49.0, 56.0]):
        knock = 112 + 8 * (index % 3)
        add_tone(track[:, 0], start, knock, 0.58, 0.045, 0.14)
        add_tone(track[:, 1], start + 0.035, 154 + 9 * (index % 2), 0.48, 0.027, 0.11, 0.25)
        add_tone(track[:, 0], start + 0.018, 288, 0.16, 0.016, 0.045, 0.4)
        add_tone(track[:, 1], start + 0.19, knock, 0.4, 0.013, 0.13, 0.1)
    for start, freq in [(21.3, 520), (42.8, 585)]:
        add_tone(track[:, 0], start, freq, 2.6, 0.009, 0.95, 0.2)
        add_tone(track[:, 1], start + 0.22, freq * 1.49, 2.2, 0.004, 0.72, 0.1)
    return track


def save(name, track):
    fade = int(2.0 * RATE)
    blend = np.linspace(0.0, 1.0, fade, endpoint=False)[:, None]
    track[-fade:] = track[-fade:] * (1.0 - blend) + track[:fade] * blend
    peak = np.max(np.abs(track))
    track *= 0.28 / max(peak, 1e-9)
    pcm = (np.clip(track, -1.0, 1.0) * 32767.0).astype("<i2")
    path = os.path.join(ROOT, name + ".wav")
    with wave.open(path, "wb") as out:
        out.setnchannels(2)
        out.setsampwidth(2)
        out.setframerate(RATE)
        out.writeframes(pcm.tobytes())
    meta_path = path + ".meta"
    if os.path.exists(meta_path):
        template = open(meta_path, encoding="utf-8").read()
    else:
        template = open(os.path.join(ROOT, "Game_BGM.wav.meta"), encoding="utf-8").read()
        template = re.sub(r"^guid: .*?$", "guid: " + uuid.uuid4().hex, template, count=1, flags=re.MULTILINE)
    template = re.sub(r"^  3D: .*?$", "  3D: 0", template, count=1, flags=re.MULTILINE)
    with open(meta_path, "w", encoding="utf-8", newline="\n") as out:
        out.write(template)
    print(name, f"{len(pcm) / RATE:.1f}s stereo, peak={peak:.3f}")


os.makedirs(ROOT, exist_ok=True)
save("QianQing_CG_Ambience", qianqing_ambience())
save("TaiHe_CG_Ambience", taihe_ambience())
