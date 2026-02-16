# Report: Set default model to 天音かなた.pmx
Date: 2026-02-09 0951

Result:
- Copied 天音かなた.pmx into runtime canonical asset root.
- Updated default runtime model path to characters/amane_kanata_v1/mmd/天音かなた.pmx.

Changed files:
- Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelConfig.cs
- Unity_PJ/data/assets_user/characters/amane_kanata_v1/mmd/天音かなた.pmx

Validation:
- Test-Path on target PMX: True
- Config check: SimpleModelConfig.modelRelativePath updated to kanata PMX
- Directory check: PMX visible under mane_kanata_v1/mmd

Notes:
- Unity runtime execution test was not run in this step; this change is path/default switch only.
