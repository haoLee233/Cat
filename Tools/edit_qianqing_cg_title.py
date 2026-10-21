"""Replace the opening title in the Qianqing Palace CG while retaining audio."""
import argparse
import os
import subprocess
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFont
from scipy.ndimage import binary_dilation, gaussian_filter

WIDTH, HEIGHT, FPS = 1920, 1080, 30
START, END = 0.4, 2.9
FONT_PATH = r"C:\Windows\Fonts\STXINGKA.TTF"
FONT_SIZE = 56


def replace_title(frame, opacity=1.0):
    rgb = np.asarray(frame.convert("RGB"), dtype=np.uint8).copy()
    gray = np.asarray(frame.convert("L"), dtype=np.uint8)
    mask = np.zeros((HEIGHT, WIDTH), dtype=bool)
    # Tight boxes around 太 / 和 / 殿 leave the comma and the following line intact.
    for left, top, right, bottom in [(18, 6, 63, 52), (84, 5, 132, 54), (144, 7, 201, 57)]:
        local = gray[top:bottom, left:right] < 190
        mask[top:bottom, left:right] |= local
    mask = binary_dilation(mask, iterations=2)

    left, top, right, bottom = 0, 0, 220, 70
    region = rgb[top:bottom, left:right]
    local_mask = mask[top:bottom, left:right]
    valid = (~local_mask).astype(np.float32)
    weights = gaussian_filter(valid, sigma=2.4)
    for channel in range(3):
        values = region[:, :, channel].astype(np.float32)
        smooth = gaussian_filter(values * valid, sigma=2.4) / np.maximum(weights, 1e-4)
        values[local_mask] = smooth[local_mask]
        region[:, :, channel] = np.clip(values, 0, 255).astype(np.uint8)
    rgb[top:bottom, left:right] = region

    image = Image.fromarray(rgb, "RGB")
    title_layer = Image.new("RGBA", image.size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(title_layer)
    font = ImageFont.truetype(FONT_PATH, FONT_SIZE)
    draw.text((21, 5), "乾清宫", font=font, fill=(18, 14, 12, int(255 * np.clip(opacity, 0, 1))))
    return Image.alpha_composite(image.convert("RGBA"), title_layer).convert("RGB")


def read_exact(stream, count):
    chunks = []
    remaining = count
    while remaining:
        chunk = stream.read(remaining)
        if not chunk:
            return b"" if not chunks else b"".join(chunks)
        chunks.append(chunk)
        remaining -= len(chunk)
    return b"".join(chunks)


def edit_video(source, destination, ffmpeg):
    frame_bytes = WIDTH * HEIGHT * 3
    decoder = subprocess.Popen(
        [ffmpeg, "-v", "error", "-i", source, "-map", "0:v:0", "-fps_mode", "passthrough",
         "-f", "rawvideo", "-pix_fmt", "rgb24", "pipe:1"], stdout=subprocess.PIPE)
    encoder = subprocess.Popen(
        [ffmpeg, "-y", "-v", "error", "-f", "rawvideo", "-pixel_format", "rgb24",
         "-video_size", f"{WIDTH}x{HEIGHT}", "-framerate", str(FPS), "-i", "pipe:0",
         "-i", source, "-map", "0:v:0", "-map", "1:a?", "-c:v", "libx264", "-crf", "18",
         "-preset", "medium", "-pix_fmt", "yuv420p", "-c:a", "copy", "-movflags", "+faststart",
         destination], stdin=subprocess.PIPE, stdout=subprocess.DEVNULL, stderr=subprocess.PIPE)

    frame_index = 0
    changed = 0
    try:
        while True:
            raw = read_exact(decoder.stdout, frame_bytes)
            if not raw:
                break
            frame = Image.frombytes("RGB", (WIDTH, HEIGHT), raw)
            timestamp = frame_index / FPS
            if START <= timestamp <= END:
                frame = replace_title(frame, (timestamp - START) / 0.55)
                changed += 1
            encoder.stdin.write(frame.tobytes())
            frame_index += 1
    finally:
        decoder.stdout.close()
        encoder.stdin.close()
    decode_code = decoder.wait()
    encode_error = encoder.stderr.read().decode("utf-8", errors="replace")
    encode_code = encoder.wait()
    if decode_code != 0 or encode_code != 0:
        raise RuntimeError(encode_error or f"ffmpeg exited: decoder={decode_code}, encoder={encode_code}")
    print(f"Updated {changed} frames between {START:.1f}s and {END:.1f}s; total frames={frame_index}")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("source", help="Input CG video")
    parser.add_argument("destination", help="Output corrected CG video")
    parser.add_argument("--preview-frame", help="Optional still frame path for visual QA")
    args = parser.parse_args()
    if args.preview_frame:
        replace_title(Image.open(args.preview_frame)).save(args.destination)
        return
    try:
        import imageio_ffmpeg
    except ImportError as exc:
        raise RuntimeError("Set PYTHONPATH to the temporary imageio-ffmpeg install") from exc
    edit_video(args.source, args.destination, imageio_ffmpeg.get_ffmpeg_exe())


if __name__ == "__main__":
    main()
