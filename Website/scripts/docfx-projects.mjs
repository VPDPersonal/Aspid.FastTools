/**
 * Turns the legacy-format .csproj files Unity generates into SDK-style projects that DocFX can load.
 *
 * MSBuild from the .NET SDK does not evaluate Unity's `ToolsVersion="4.0"` projects properly (defines and
 * nullable settings are lost), so the API reference cannot be built from them directly. This script writes
 * `Website/docfx/projects/<Assembly>.csproj` with the assembly's sources, the assembly references and compiler
 * settings of the Unity project. Unity project references become references to the compiled assemblies in
 * `Library/ScriptAssemblies`, except the ones we document, which stay project references.
 *
 * Sources are collected from the assembly's asmdef folder (sub-folders owned by another asmdef excluded), not
 * from the csproj `<Compile>` list: Unity regenerates the csproj only when an IDE asks for it, so that list goes
 * stale the moment a file is added. References, defines and language settings still come from the csproj.
 *
 * The output contains absolute paths into the local Unity installation and is gitignored; run it (and the Unity
 * Editor, so `Library/ScriptAssemblies` is fresh) before `docfx metadata`.
 */
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const siteDir = path.dirname(path.dirname(fileURLToPath(import.meta.url)));
const unityDir = path.resolve(siteDir, '../Aspid.FastTools');
const packageDir = path.join(unityDir, 'Packages/tech.aspid.fasttools');
const outDir = path.join(siteDir, 'docfx', 'projects');

/** Assemblies that get an API reference page tree, with the folder their asmdef lives in. */
const ASSEMBLIES = {
  'Aspid.FastTools': 'Runtime/Scripts',
  'Aspid.FastTools.Editor': 'Editor/Scripts',
};

function attr(xml, tag, name) {
  return [...xml.matchAll(new RegExp(`<${tag}\\s+${name}="([^"]*)"`, 'g'))].map((m) => m[1]);
}

function element(xml, tag) {
  return xml.match(new RegExp(`<${tag}>([^<]*)</${tag}>`))?.[1] ?? '';
}

function hintPaths(xml) {
  return [...xml.matchAll(/<Reference Include="[^"]*">\s*<HintPath>([^<]*)<\/HintPath>/g)].map((m) => m[1]);
}

/** Every `.cs` under `dir`, skipping sub-folders that carry their own asmdef (they are separate assemblies). */
function sources(dir, isRoot = true) {
  const entries = fs.readdirSync(dir, { withFileTypes: true });
  if (!isRoot && entries.some((e) => e.isFile() && e.name.endsWith('.asmdef'))) return [];
  const files = [];
  for (const entry of entries) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) files.push(...sources(full, false));
    else if (entry.name.endsWith('.cs')) files.push(full);
  }
  return files;
}

fs.rmSync(outDir, { recursive: true, force: true });
fs.mkdirSync(outDir, { recursive: true });

for (const [assembly, folder] of Object.entries(ASSEMBLIES)) {
  const xml = fs.readFileSync(path.join(unityDir, `${assembly}.csproj`), 'utf8');

  const compile = sources(path.join(packageDir, folder));
  const references = hintPaths(xml).map((hint) => (path.isAbsolute(hint) ? hint : path.join(unityDir, hint)));
  const projectRefs = attr(xml, 'ProjectReference', 'Include').map((file) => path.basename(file, '.csproj'));
  // Source generators complete the partial types (IId structs, ProfilerMarker extensions); without them the code does not compile.
  const analyzers = attr(xml, 'Analyzer', 'Include').filter((file) => /Generators?\.dll$|SourceGenerators?\.dll$/.test(file));
  const defines = element(xml, 'DefineConstants');
  const langVersion = element(xml, 'LangVersion') || 'latest';
  const nullable = element(xml, 'Nullable') || 'disable';
  const unsafeBlocks = element(xml, 'AllowUnsafeBlocks') || 'false';

  const items = [
    ...compile.map((file) => `    <Compile Include="${file}" />`),
    ...references.map((file) => `    <Reference Include="${path.basename(file, '.dll')}"><HintPath>${file}</HintPath></Reference>`),
    ...analyzers.map((file) => `    <Analyzer Include="${file}" />`),
    ...projectRefs.map((name) =>
      name in ASSEMBLIES
        ? `    <ProjectReference Include="${name}.csproj" />`
        : `    <Reference Include="${name}"><HintPath>${path.join(unityDir, 'Library/ScriptAssemblies', `${name}.dll`)}</HintPath></Reference>`,
    ),
  ];

  const project = `<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <AssemblyName>${assembly}</AssemblyName>
    <RootNamespace>${assembly}</RootNamespace>
    <LangVersion>${langVersion}</LangVersion>
    <Nullable>${nullable}</Nullable>
    <AllowUnsafeBlocks>${unsafeBlocks}</AllowUnsafeBlocks>
    <DefineConstants>${defines}</DefineConstants>
    <EnableDefaultItems>false</EnableDefaultItems>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>CS1591;CS1573;CS8632</NoWarn>
    <DisableImplicitFrameworkReferences>false</DisableImplicitFrameworkReferences>
  </PropertyGroup>
  <ItemGroup>
${items.join('\n')}
  </ItemGroup>
</Project>
`;
  fs.writeFileSync(path.join(outDir, `${assembly}.csproj`), project);
  console.log(`[docfx-projects] ${assembly}: ${compile.length} files, ${references.length + projectRefs.length} references, ${analyzers.length} generators`);
}
