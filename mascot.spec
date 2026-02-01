# -*- mode: python ; coding: utf-8 -*-
"""
mascot.spec - PyInstaller spec file for MascotDesktop Avatar PoC

Build with: pyinstaller --clean --noconfirm mascot.spec
Output: dist/mascot_avatar/
"""

import os
from pathlib import Path

# Base directory
block_cipher = None
ROOT = Path(SPECPATH)

# Entry point
ENTRY = ROOT / 'apps' / 'avatar' / 'poc' / 'poc_avatar_mmd_viewer.py'

# Data files to include
datas = [
    # Viewer static files
    (str(ROOT / 'viewer'), 'viewer'),
    # Motion templates
    (str(ROOT / 'data' / 'templates' / 'motions'), 'data/templates/motions'),
]

# Hidden imports (modules loaded dynamically)
hiddenimports = [
    'argparse',
    'http.server',
    'json',
    'logging',
    'logging.handlers',
    'pathlib',
    'random',
    'threading',
    'time',
    'urllib.parse',
    'uuid',
    'tkinter',
    'webbrowser',
]

a = Analysis(
    [str(ENTRY)],
    pathex=[str(ROOT)],
    binaries=[],
    datas=datas,
    hiddenimports=hiddenimports,
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[
        # tkinter is required by poc_avatar_mmd_viewer
        'PyQt5',
        'PyQt6',
        'PySide2',
        'PySide6',
    ],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    [],
    exclude_binaries=True,
    name='mascot_avatar',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    console=False,  # Windowed mode - no console
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
)

coll = COLLECT(
    exe,
    a.binaries,
    a.zipfiles,
    a.datas,
    strip=False,
    upx=True,
    upx_exclude=[],
    name='mascot_avatar',
)
