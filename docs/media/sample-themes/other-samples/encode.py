from pathlib import Path
import subprocess,shutil,json
root=Path(__file__).resolve().parent
repo=root.parents[3]
package=repo/'Aspid.FastTools/Packages/tech.aspid.fasttools'
version=json.loads((package/'package.json').read_text())['version']
samples=package/'Samples~'
def ff(*args):subprocess.run(['ffmpeg','-y','-hide_banner','-loglevel','error',*map(str,args)],check=True)
items=[('Types','types','1204:680:100:116'),('SerializeReferences','serialize-references','888:532:276:178'),('ProfilerMarkers','profiler-markers','800:688:320:108')]
for sample,slug,crop in items:
 images=samples/sample/'Documentation/Images'
 ff('-i',root/(slug+'-light.mkv'),'-filter_complex',f'crop={crop},split[v][p];[p]palettegen=max_colors=256[pal];[v][pal]paletteuse=dither=sierra2_4a','-loop','0',images/'demo-light.gif')
 shutil.copy2(root/(slug+'-scene-light.png'),images/'scene-light.png')
 shutil.copy2(images/'scene-light.png',repo/'Website/static/img/samples'/(slug+'-light.png'))
images=samples/'EditorTools/Documentation/Images'
seq=root/'catalog-sequence.txt'
seq.write_text("file 'catalog-before.jpg'\nduration 1.2\nfile 'catalog-after.jpg'\nduration 1.2\nfile 'catalog-undo.jpg'\nduration 1.2\nfile 'catalog-undo.jpg'\n")
ff('-f','concat','-safe','0','-i',seq,'-filter_complex','fps=10,crop=1116:420:4:132,split[v][p];[p]palettegen=max_colors=256[pal];[v][pal]paletteuse=dither=sierra2_4a','-t','3.6','-loop','0',images/'demo-light.gif')
ff('-i',root/'catalog-before.jpg','-vf','crop=1116:526:4:30','-frames:v','1',images/'ability-catalog-light.png')
ff('-i',root/'catalog-before.jpg','-vf','crop=1116:628:4:30','-frames:v','1',repo/'Website/static/img/samples/ability-catalog-light.png')
for sample in ['Types','SerializeReferences','ProfilerMarkers','EditorTools']:
 images=samples/sample/'Documentation/Images'
 imported=repo/'Aspid.FastTools/Assets/Samples/Aspid.FastTools'/version/sample/'Documentation/Images'
 for name in ['demo-light.gif','scene-light.png','ability-catalog-light.png']:
  if (images/name).exists() and imported.exists():shutil.copy2(images/name,imported/name)
