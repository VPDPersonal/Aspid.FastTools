from pathlib import Path
import subprocess
root=Path(__file__).resolve().parent
repo=root.parents[3]
tutorials=repo/'Website/tutorials'
def ff(*args):subprocess.run(['ffmpeg','-y','-hide_banner','-loglevel','error',*map(str,args)],check=True)
# Dark and light share one recorder, crop and timeline, so each demo.gif and demo-light.gif pair matches frame for frame.
themes=[('dark','demo.gif'),('light','demo-light.gif')]
items=[('Types','types','1204:680:100:116'),('SerializeReferences','serialize-references','888:532:276:178'),('ProfilerMarkers','profiler-markers','800:688:320:108')]
for sample,slug,crop in items:
 images=tutorials/sample/'Images'
 for theme,gif in themes:
  ff('-i',root/f'{slug}-{theme}.mkv','-filter_complex',f'crop={crop},split[v][p];[p]palettegen=max_colors=256[pal];[v][pal]paletteuse=dither=sierra2_4a','-loop','0',images/gif)
# The EditorTools GIFs are composed from catalog/<theme>/ with the asp-unity-capture skill (compose.py + encode.sh);
# the gallery previews are the idle grab with the header, 16:9 at the gallery size.
for theme,suffix in [('dark',''),('light','-light')]:
 ff('-i',root/'catalog'/theme/'idle.png','-vf','crop=2000:1125:0:0,scale=1440:810:flags=lanczos','-frames:v','1',repo/'Website/static/img/samples'/f'ability-catalog{suffix}.png')
