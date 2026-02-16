# -*- mode: python ; coding: utf-8 -*-
"""
tray_host.spec - PyInstaller spec file for MascotDesktop Tray Host

Build with: pyinstaller --clean --noconfirm tray_host.spec
Output: dist/mascot_tray/
"""

import os
from pathlib import Path

# Base directory
block_cipher = None
ROOT = Path(SPECPATH)

# Entry point
ENTRY = ROOT / 'apps' / 'shell' / 'tray_host.py'

# Data files to include
datas = [
    # Viewer static files
    (str(ROOT / 'viewer'), 'viewer'),
    # Motion templates
    (str(ROOT / 'data' / 'templates' / 'motions'), 'data/templates/motions'),
    # Icon
    (str(ROOT / 'data' / 'templates' / 'assets' / 'placeholders'), 'data/templates/assets/placeholders'),
    # Avatar PoC (for subprocess)
    (str(ROOT / 'apps'), 'apps'),
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
    'pystray',
    'pystray._win32',
    'PIL',
    'PIL.Image',
    'webview',
    'webview.platforms.edgechromium',
    'clr_loader',
    'pythonnet',
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
    name='mascot_tray',
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
    name='mascot_tray',
)
