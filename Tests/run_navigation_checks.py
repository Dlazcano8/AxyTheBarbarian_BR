import json
import os
import re
import shutil
import subprocess
import sys
from pathlib import Path


def main():
    root = Path(__file__).resolve().parent.parent
    version_text = (root / 'ProjectSettings/ProjectVersion.txt').read_text()
    version = re.search(r'm_EditorVersion: (\S+)', version_text)[1]
    unity_data = Path(sys.argv[1]) if len(sys.argv) > 1 else (
        Path(os.environ.get('ProgramFiles', 'C:/Program Files'))
        / 'Unity/Hub/Editor' / version / 'Editor/Data'
    )
    engine = unity_data / 'Managed/UnityEngine'
    dotnet = shutil.which('dotnet')
    if dotnet is None:
        raise RuntimeError('Install a .NET SDK to run these checks.')
    dotnet_root = Path(dotnet).resolve().parent
    sdk = subprocess.run([dotnet, '--version'], check=True, capture_output=True, text=True).stdout.strip()
    compiler = dotnet_root / 'sdk' / sdk / 'Roslyn/bincore/csc.dll'
    versions = [p for p in (dotnet_root / 'packs/Microsoft.NETCore.App.Ref').iterdir()
                if p.is_dir() and re.fullmatch(r'\d+\.\d+\.\d+', p.name)]
    reference_pack = max(versions, key=lambda p: tuple(map(int, p.name.split('.'))))
    major, minor, _ = reference_pack.name.split('.')
    framework = f'net{major}.{minor}'
    references = sorted((reference_pack / 'ref' / framework).glob('*.dll'))
    sources = sorted((root / 'Assets/Scripts/Navigation').glob('*.cs'))
    sources.append(root / 'Tests/NavigationChecks.cs')
    output = root / 'Temp/Part11Validation/NavigationChecks'
    output.mkdir(parents=True, exist_ok=True)
    arguments = ['-nologo', '-nostdlib+', '-langversion:9.0', '-target:exe',
                 f'-out:"{output / "NavigationChecks.dll"}"']
    arguments += [f'-r:"{p}"' for p in references]
    arguments.append(f'-r:"{engine / "UnityEngine.CoreModule.dll"}"')
    arguments += [f'"{p}"' for p in sources]
    response = output / 'compile.rsp'
    response.write_text('\n'.join(arguments), encoding='utf-8')
    subprocess.run([dotnet, str(compiler), '@' + str(response)], check=True)
    for name in ('UnityEngine.CoreModule.dll', 'UnityEngine.SharedInternalsModule.dll'):
        shutil.copyfile(engine / name, output / name)
    config = {'runtimeOptions': {'tfm': framework, 'framework': {
        'name': 'Microsoft.NETCore.App', 'version': f'{major}.{minor}.0'}}}
    (output / 'NavigationChecks.runtimeconfig.json').write_text(json.dumps(config), encoding='utf-8')
    subprocess.run([dotnet, str(output / 'NavigationChecks.dll'),
                    str(root / 'Assets/Levels/Dungeon01.txt')], check=True)


if __name__ == '__main__':
    main()
