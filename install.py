#!/usr/bin/env python3 -I
"""Instala/remove os mods do 9 Kings (Modo Deus e, se compilado, Rei do Entretenimento).

Uso: install.py install | uninstall | status
Copia as DLLs para Data/Managed e registra cada mod nos JSONs de inicialização da Unity.
Os JSONs originais ficam salvos em Data/.godmode_backup/.
"""
import json, os, shutil, sys

GAME = os.path.expanduser('~/Library/Application Support/Steam/steamapps/common/9 Kings')
DATA = os.path.join(GAME, '9Kings.app/Contents/Resources/Data')
MANAGED = os.path.join(DATA, 'Managed')
BACKUP = os.path.join(DATA, '.godmode_backup')
HERE = os.path.dirname(os.path.abspath(__file__))
PROGRAMAS = os.path.dirname(HERE)

# (assembly, caminho da DLL compilada). Mods sem DLL compilada são ignorados.
MODS = [
    ('NineKingsGodMode', os.path.join(HERE, 'bin/Release/netstandard2.1/NineKingsGodMode.dll')),
    ('NineKingsEntertainment', os.path.join(PROGRAMAS, '9kings-entertainment/bin/Release/netstandard2.1/NineKingsEntertainment.dll')),
]
LIBS = ['0Harmony.dll']
OLD_LIBS = ['MonoMod.RuntimeDetour.dll', 'MonoMod.Utils.dll', 'Mono.Cecil.dll', 'Mono.Cecil.Rocks.dll']
JSONS = ['ScriptingAssemblies.json', 'RuntimeInitializeOnLoads.json']


def init_entry(assembly):
    return {"assemblyName": assembly, "nameSpace": assembly, "className": "Loader",
            "methodName": "Init", "loadTypes": 1, "isUnityClass": False}


def load(name):
    with open(os.path.join(DATA, name)) as f:
        return json.load(f)


def save(name, data):
    with open(os.path.join(DATA, name), 'w') as f:
        json.dump(data, f, separators=(',', ':'))


def install():
    os.makedirs(BACKUP, exist_ok=True)
    for name in JSONS:
        dst = os.path.join(BACKUP, name)
        if not os.path.exists(dst):
            shutil.copy2(os.path.join(DATA, name), dst)
    for lib in LIBS:
        shutil.copy2(os.path.join(HERE, 'lib', lib), MANAGED)

    installed = [(a, p) for a, p in MODS if os.path.exists(p)]
    for _, path in installed:
        shutil.copy2(path, MANAGED)

    sa = load('ScriptingAssemblies.json')
    for dll in [a + '.dll' for a, _ in installed] + LIBS:
        if dll not in sa['names']:
            sa['names'].append(dll)
            sa['types'].append(16)
    save('ScriptingAssemblies.json', sa)

    ri = load('RuntimeInitializeOnLoads.json')
    names = {a for a, _ in MODS}
    ri['root'] = [e for e in ri['root'] if e.get('assemblyName') not in names] + [init_entry(a) for a, _ in installed]
    save('RuntimeInitializeOnLoads.json', ri)
    print('Instalado: ' + ', '.join(a for a, _ in installed))


def uninstall():
    for name in JSONS:
        src = os.path.join(BACKUP, name)
        if os.path.exists(src):
            shutil.copy2(src, os.path.join(DATA, name))
    for dll in [a + '.dll' for a, _ in MODS] + LIBS + OLD_LIBS:
        path = os.path.join(MANAGED, dll)
        if os.path.exists(path):
            os.remove(path)
    print('Mods removidos.')


def status():
    ri = load('RuntimeInitializeOnLoads.json')
    for a, _ in MODS:
        on = any(e.get('assemblyName') == a for e in ri['root']) and os.path.exists(os.path.join(MANAGED, a + '.dll'))
        print(a + ': ' + ('instalado' if on else 'não instalado'))


if __name__ == '__main__':
    {'install': install, 'uninstall': uninstall, 'status': status}[sys.argv[1] if len(sys.argv) > 1 else 'status']()
