#!/usr/bin/env python3 -I
"""Instala/remove o Modo Deus no 9 Kings.

Uso: install.py install | uninstall | status
Copia as DLLs para Data/Managed e registra o mod nos JSONs de inicialização da Unity.
Os JSONs originais ficam salvos em Data/.godmode_backup/.
"""
import json, os, shutil, sys

GAME = os.path.expanduser('~/Library/Application Support/Steam/steamapps/common/9 Kings')
DATA = os.path.join(GAME, '9Kings.app/Contents/Resources/Data')
MANAGED = os.path.join(DATA, 'Managed')
BACKUP = os.path.join(DATA, '.godmode_backup')
HERE = os.path.dirname(os.path.abspath(__file__))
MOD_DLL = os.path.join(HERE, 'bin/Release/netstandard2.1/NineKingsGodMode.dll')
LIBS = ['0Harmony.dll']
OLD_LIBS = ['MonoMod.RuntimeDetour.dll', 'MonoMod.Utils.dll', 'Mono.Cecil.dll', 'Mono.Cecil.Rocks.dll']
REGISTERED = ['NineKingsGodMode.dll', '0Harmony.dll']
INIT = {"assemblyName": "NineKingsGodMode", "nameSpace": "NineKingsGodMode", "className": "Loader",
        "methodName": "Init", "loadTypes": 1, "isUnityClass": False}
JSONS = ['ScriptingAssemblies.json', 'RuntimeInitializeOnLoads.json']


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
    shutil.copy2(MOD_DLL, MANAGED)
    for lib in LIBS:
        shutil.copy2(os.path.join(HERE, 'lib', lib), MANAGED)

    sa = load('ScriptingAssemblies.json')
    for dll in REGISTERED:
        if dll not in sa['names']:
            sa['names'].append(dll)
            sa['types'].append(16)
    save('ScriptingAssemblies.json', sa)

    ri = load('RuntimeInitializeOnLoads.json')
    ri['root'] = [e for e in ri['root'] if e.get('assemblyName') != INIT['assemblyName']] + [INIT]
    save('RuntimeInitializeOnLoads.json', ri)
    print('Modo Deus instalado.')


def uninstall():
    for name in JSONS:
        src = os.path.join(BACKUP, name)
        if os.path.exists(src):
            shutil.copy2(src, os.path.join(DATA, name))
    for dll in ['NineKingsGodMode.dll'] + LIBS + OLD_LIBS:
        path = os.path.join(MANAGED, dll)
        if os.path.exists(path):
            os.remove(path)
    print('Modo Deus removido.')


def status():
    ri = load('RuntimeInitializeOnLoads.json')
    on = any(e.get('assemblyName') == INIT['assemblyName'] for e in ri['root'])
    print('instalado' if on and os.path.exists(os.path.join(MANAGED, 'NineKingsGodMode.dll')) else 'não instalado')


if __name__ == '__main__':
    {'install': install, 'uninstall': uninstall, 'status': status}[sys.argv[1] if len(sys.argv) > 1 else 'status']()
