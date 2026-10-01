"""Rebuilds the scene previews of the /tutorials gallery and their looping clips.

Previews: Website/static/img/samples/<slug>(-light).png. Clips: Website/src/components/SamplesGallery/media/<slug>(-light).mp4.

Sources are the lossless camera recordings of docs/media/sample-themes, one per theme, which replay frame for frame, so
both themes show the same moment. The EnumValues recordings keep the scene titles; they sit on the
flat background and are painted over. One 16:9 box in camera space frames both themes, and each clip starts on its
preview's frame.

The flat camera background is repainted, fringe included: in the previews to the article surface the card shows
(--venom-reading-surface in Website/src/css/custom.css), in the clips to black (dark) or white (light), which the card
blends with a layer of that surface. 8-bit YUV cannot encode the surface itself; it misses by one level.
"""
import subprocess, tempfile, sys
from pathlib import Path
from PIL import Image, ImageChops, ImageDraw, ImageFilter

repo = Path(__file__).resolve().parents[3]
media = repo / 'docs/media/sample-themes'
recordings = media / 'other-samples'
samples = repo / 'Aspid.FastTools/Packages/tech.aspid.fasttools/Samples~'
previews = repo / 'Website/static/img/samples'
clips = repo / 'Website/src/components/SamplesGallery/media'
CAMERA = (1440, 810)
SURFACE = {'dark': (10, 11, 15), 'light': (255, 255, 255)}
BLEND_BASE = {'dark': (0, 0, 0), 'light': (255, 255, 255)}
CLIP_SIZE = (1024, 576)

# slug, crop box in camera space, preview frame, light source, dark source: a recording, or (sample, offset, frame) of
# its demo.gif.
ITEMS = [
    ('enum-values', (120, 101, 1320, 776), 29, media / 'source-light.mkv', media / 'source-dark.mkv'),
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


def without_titles(frame):
    frame = frame.copy()
    background = frame.getpixel((2, 2))
    draw = ImageDraw.Draw(frame)
    draw.rectangle((0, 0, 1439, 301), fill=background)
    draw.rectangle((0, 575, 1439, 809), fill=background)
    return frame


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
            frame.resize(CLIP_SIZE, Image.LANCZOS).save(f'{folder}/{index:04d}.png')
        subprocess.run(['ffmpeg', '-loglevel', 'error', '-y', '-framerate', '20', '-i', f'{folder}/%04d.png',
                        '-vf', 'scale=out_color_matrix=bt709:out_range=tv,format=yuv420p',
                        '-c:v', 'libx264', '-preset', 'veryslow', '-crf', '24', '-tune', 'animation',
                        # Tagged as the intro's clips are: sRGB transfer, so browsers leave the dark tones alone.
                        '-x264-params', 'colorprim=bt709:transfer=iec61966-2-1:colormatrix=bt709:range=tv',
                        '-movflags', '+faststart', '-an', str(path)], check=True)


def build(slug, theme, sequence, poster, box):
    background = sequence[0].getpixel((2, 2))
    sequence = [frame.crop(box) for frame in sequence]
    suffix = '-light' if theme == 'light' else ''
    repaint(sequence[poster], background, SURFACE[theme]).save(previews / f'{slug}{suffix}.png', optimize=True)
    encode([repaint(frame, background, BLEND_BASE[theme]) for frame in sequence[poster:] + sequence[:poster]],
           clips / f'{slug}{suffix}.mp4')
    return len(sequence)


clips.mkdir(exist_ok=True)
for slug, box, poster, light, dark in ITEMS:
    if len(sys.argv) > 1 and slug not in sys.argv[1:]:
        continue
    light_frames = frames(light)
    if slug == 'enum-values':
        light_frames = [without_titles(frame) for frame in light_frames]
    counts = [build(slug, 'light', light_frames, poster, box)]
    if isinstance(dark, tuple):
        sample, offset, dark_poster = dark
        gif = frames(samples / sample / 'Documentation/Images/demo.gif')
        dark_frames = [placed(frame, offset, gif[0].getpixel((2, 2))) for frame in gif]
    else:
        dark_frames, dark_poster = frames(dark), poster
        if slug == 'enum-values':
            dark_frames = [without_titles(frame) for frame in dark_frames]
    counts.append(build(slug, 'dark', dark_frames, dark_poster, box))
    print(slug, box[2] - box[0], 'x', box[3] - box[1], 'light/dark frames', counts)
