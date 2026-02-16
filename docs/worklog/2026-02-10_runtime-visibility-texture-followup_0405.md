# Worklog: Runtime Visibility + Texture Resolution Follow-up

- Date: 2026-02-10
- Task: 非表示/白飛び継続に対する追補修正（座標安定化 + テクスチャ探索強化 + 欠落診断強化）
- Execution-Tool: Codex CLI
- Execution-Agent: codex
- Execution-Model: GPT-5
- Used-Skills: bug-investigation, worklog-update
- Repo-Refs:
  - Unity_PJ/project/Assets/Scripts/Runtime/Avatar/SimpleModelBootstrap.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/ImageLoader/BitmapLoader.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/TextureLoader.cs
  - Unity_PJ/project/Assets/LibMmd/Unity3D/MaterialLoader.cs
- Obsidian-Refs:
  - D:/Obsidian/Programming/MascotDesktop_obsidian_log_template.md
- Report-Path: docs/worklog/2026-02-10_runtime-visibility-texture-followup_0405.md
- Obsidian-Log: pending (next session sideで作成)
- Tags: [agent/codex, model/gpt-5, tool/codex-cli, unity/runtime, pmx, texture]

## Summary

ユーザー実行ログで `avatar.model.displayed` が出る一方で見た目不良が継続。`avatar.model.render_diagnostics` の `missingMainTexMats=1` が連続しており、座標逸脱よりも「テクスチャ参照解決失敗」が主要因と判断。座標再安定化を維持しつつ、LibMMD側のBMP読込とテクスチャ解決を強化し、欠落パスを直接ログ化した。

## Changes

- `SimpleModelBootstrap`:
  - PMX/VRM登録時に一定フレーム `LateUpdate` で配置安定化（画面外判定時に再センタリング + camera fit）。
  - `RegisterActiveModelRoot` に配置安定化フラグを追加。
- `BitmapLoader`:
  - `BiCompression == 3` (BI_BITFIELDS) を16/32bitでサポート。
  - 1/4bpp行バイト計算を切り上げに修正。
  - パレット件数計算のビットシフト誤りを修正（`>>` -> `<<`）。
- `TextureLoader`:
  - テクスチャ解決失敗時に `baseDir/parent/grandParent` へ再帰探索を追加（ファイル名一致、優先度付き）。
  - 解決結果を静的キャッシュ化し同一探索のコストを抑制。
- `MaterialLoader`:
  - main/sub/toon 各テクスチャ欠落時に `material名 + requested path` を警告出力。

## Commands

- `apply_patch` (4 files)
- `Select-String` / `Get-Content` による差分位置と挿入確認

## Tests

- Unity自動テスト: 未実施（このセッションではコード変更のみ）
- ユーザー実行ログの確認結果:
  - `avatar.model.displayed` は成功。
  - `avatar.model.render_diagnostics` で `missingMainTexMats=1` を確認。

## Rationale (Key Points)

- `enabled=1, active=1, bounds=...` で描画体自体は成立しているため、完全非表示の第一候補は「テクスチャ未解決によるマテリアル不正」。
- 一部資産でPMX内参照パスが不整合でも動くよう、`TextureLoader` に実ファイル名ベースの探索を追加。
- 次段解析を1回で終わらせるため、`MaterialLoader` で欠落参照を具体的にログ化。

## Rollback

1. `SimpleModelBootstrap.cs` の `LateUpdate` / `StabilizeActiveModelPlacement` / `RegisterActiveModelRoot` 拡張差分を戻す。
2. `BitmapLoader.cs` の BITFIELDS対応差分を戻す。
3. `TextureLoader.cs` の再帰探索・キャッシュ差分を戻す。
4. `MaterialLoader.cs` の欠落ログ差分を戻す。

## Next Actions

1. PlayModeで問題モデルを再読込し、`MMD main texture missing` 警告の `requested=` を採取。
2. `requested` と実ファイル配置の差異（相対階層/大文字小文字/拡張子違い）を突合。
3. まだ白飛びするモデルは、`missingMainTexMats` と `lowAlphaMats` の値を同時採取し、マテリアル補正の次段（shader property別）へ進める。

## Record Check

- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes (pending reason stated)
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes
