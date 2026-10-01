"""Add 22 px below both original GIFs without re-encoding their animation.
Only the first image gains the background strip; disposal=1 retains it in later frames.
All original frame durations, colours and top/side margins remain unchanged.
"""
from io import BytesIO
from pathlib import Path
from PIL import Image, ImageChops


def first_image(data):
    offset = 13 + (3 * (2 ** ((data[10] & 7) + 1)) if data[10] & 128 else 0)
    while data[offset] == 0x21:
        offset += 2
        while data[offset]:
            offset += data[offset] + 1
        offset += 1
    assert data[offset] == 0x2C
    end = offset + 10
    if data[offset + 9] & 128:
        end += 3 * 2 ** ((data[offset + 9] & 7) + 1)
    end += 1  # LZW minimum code size
    while data[end]:
        end += data[end] + 1
    return offset, end + 1


here = Path(__file__).resolve().parent
repo = here.parents[3]
target = repo / 'Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/Images'
for suffix in ('', '-light'):
    source_path = here / f'source{suffix}.gif'
    source = Image.open(source_path)
    original = source.convert('RGB')
    padded = Image.new('RGB', (1560, 726), original.getpixel((0, 0)))
    padded.paste(original, (0, 0))
    first = Image.new('P', (1560, 726), source.getpixel((0, 0)))
    first.putpalette(source.getpalette())
    first.paste(source, (0, 0))
    assert ImageChops.difference(first.convert('RGB'), padded).getbbox() is None
    buffer = BytesIO()
    first.save(buffer, format='GIF', optimize=False)
    encoded = buffer.getvalue()
    start, end = first_image(encoded)
    palette_size = 3 * 2 ** ((encoded[10] & 7) + 1)
    descriptor = bytearray(encoded[start:start + 10])
    assert not descriptor[9] & 128
    descriptor[9] = (descriptor[9] & 120) | 128 | (encoded[10] & 7)
    replacement = bytes(descriptor) + encoded[13:13 + palette_size] + encoded[start + 10:end]
    data = bytearray(source_path.read_bytes())
    start, end = first_image(data)
    data[8:10] = (726).to_bytes(2, 'little')
    data[start:end] = replacement
    output = target / f'enum-values-multipliers-populate{suffix}.gif'
    output.write_bytes(data)
    result = Image.open(output)
    assert result.n_frames == source.n_frames
    duration = 0
    for index in range(source.n_frames):
        source.seek(index)
        result.seek(index)
        assert source.disposal_method == 1
        assert result.info['duration'] == source.info['duration']
        duration += source.info['duration']
        assert ImageChops.difference(result.convert('RGB').crop((0, 0, 1560, 704)), source.convert('RGB')).getbbox() is None
        strip = result.convert('RGB').crop((0, 704, 1560, 726))
        assert ImageChops.difference(strip, Image.new('RGB', strip.size, original.getpixel((0, 0)))).getbbox() is None
    print(output.name, result.size, result.n_frames, duration, 'ms; original pixels and timing unchanged')
