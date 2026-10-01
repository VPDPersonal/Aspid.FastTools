"""Rebuilds the scene GIFs of the /tutorials gallery from lossless camera recordings.

Output: Website/static/img/samples/<slug>(-light).gif. Both themes use the same camera crop and start frame.
EnumValues recordings are cropped to (169, 270, 1101, 331) and placed back in the 1440x810 camera frame.
The flat background is repainted to the article surface (--venom-reading-surface).
Pass sample slugs to rebuild only those cards; without arguments, rebuild all scene cards.
"""
import argparse, subprocess, tempfile
from pathlib import Path
from PIL import Image, ImageChops, ImageFilter

repo = Path(__file__).resolve().parents[3]
media = repo / 'docs/media/sample-themes'
recordings = media / 'other-samples'
previews = repo / 'Website/static/img/samples'
CAMERA = (1440, 810)
SURFACE = {'dark': (10, 11, 15), 'light': (255, 255, 255)}
GIF_SIZE = (1024, 576)

# slug, crop box in camera space, preview frame, light recording, dark recording.
ITEMS = [
    ('enum-values', (120, 101, 1320, 776), 29, media / 'enum-values-light.mkv', media / 'enum-values-dark.mkv'),
    ('types', (77, 108, 1325, 810), 0, recordings / 'types-light.mkv', recordings / 'types-dark.mkv'),
    ('serialize-references', (208, 154, 1232, 730), 85,
     recordings / 'serialize-references-light.mkv', recordings / 'serialize-references-dark.mkv'),
    ('profiler-markers', (112, 104, 1328, 788), 59,
     recordings / 'profiler-markers-light.mkv', recordings / 'profiler-markers-dark.mkv'),
]


def frames(video):
    with tempfile.TemporaryDirectory() as folder:
        subprocess.run(['ffmpeg', '-loglevel', 'error', '-i', str(video), '-vsync', '0', f'{folder}/%04d.png'], check=True)
        return [Image.open(path).convert('RGB') for path in sorted(Path(folder).glob('*.png'))]


def placed(frame, offset, background):
    camera = Image.new('RGB', CAMERA, background)
    camera.paste(frame, offset)
    return camera


def repaint(frame, background, color):
    """The flat background in `color`, plus the antialiased fringe next to it that is still close to it, the way the
    `.scene-footage` filter of Website/src/theme/Root repaints an image."""
    def within(tolerance):
        bands = [band.point(lambda v, c=c: 255 if abs(v - c) <= tolerance else 0)
                 for band, c in zip(frame.split(), background)]
        return ImageChops.multiply(ImageChops.multiply(bands[0], bands[1]), bands[2])
    soft = within(1).filter(ImageFilter.MaxFilter(3)).filter(ImageFilter.GaussianBlur(0.35))
    return Image.composite(Image.new('RGB', frame.size, color), frame, ImageChops.multiply(soft, within(24)))


def encode(sequence, path):
    with tempfile.TemporaryDirectory() as folder:
        for index, frame in enumerate(sequence):
            frame.resize(GIF_SIZE, Image.LANCZOS).save(f'{folder}/{index:04d}.png')
        subprocess.run(['ffmpeg', '-loglevel', 'error', '-y', '-framerate', '20', '-i', f'{folder}/%04d.png',
                        '-filter_complex', '[0:v]split[a][b];[a]palettegen=max_colors=256[p];'
                        '[b][p]paletteuse=dither=sierra2_4a', '-loop', '0', str(path)], check=True)


def build(slug, theme, sequence, poster, box):
    background = sequence[0].getpixel((2, 2))
    sequence = [frame.crop(box) for frame in sequence]
    suffix = '-light' if theme == 'light' else ''
    encode([repaint(frame, background, SURFACE[theme]) for frame in sequence[poster:] + sequence[:poster]],
           previews / f'{slug}{suffix}.gif')
    return len(sequence)


parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('samples', nargs='*', help='Sample slugs to rebuild (default: all).')
args = parser.parse_args()
unknown = set(args.samples) - {item[0] for item in ITEMS}
if unknown:
    parser.error(f"Unknown samples: {', '.join(sorted(unknown))}")

previews.mkdir(exist_ok=True)
for slug, box, poster, light, dark in ITEMS:
    if args.samples and slug not in args.samples:
        continue
    counts = []
    for theme, source in [('light', light), ('dark', dark)]:
        sequence = frames(source)
        if slug == 'enum-values':
            background = sequence[0].getpixel((2, 2))
            sequence = [placed(frame, (169, 270), background) for frame in sequence]
        counts.append(build(slug, theme, sequence, poster, box))
    print(slug, box[2] - box[0], 'x', box[3] - box[1], 'light/dark frames', counts)
