# Unity Migration Spec (Single Window, MMD Priority)

- Status: draft
- Owner/Agent: codex
- Last Updated: 2026-01-28
- Scope: Unity migration spec for UI + MMD + minimal Core, single-window UX.

## Goals
- 単一ウィンドウ起動（多窓/多プロセスに見えないUX）
- MMD表示のスムーズ動作を最優先で安定化
- UI制御と最小Core機能をUnity内に統合

## Non-Goals
- Memory/Embedding/DBをUnityへ全面移行しない
- 旧仕様（pre-Unity）の上書きではなく、別系統として扱う

## Constraints
- 既存の設定ファイル互換を維持（data/user/config.json）
- 既存のログ/相関ID運用を破壊しない
- 旧仕様は参照用に固定し、混同を防ぐ

## Minimal Core in Unity (Scope)
- 設定の保存/復元（window_x/y/width/height/pinned/fit）
- 起動時初期同期（設定読み込み）
- UIイベント処理（drag_end / topmost_toggle / fit_change）

## UI Requirements (Unity)
- 透明/フレームレス
- Topmost(Pinned/Normal)切替
- ドラッグ移動、位置・サイズ保存/復元
- 右クリックメニュー/オーバーレイ操作

## Avatar Requirements (Unity, MMD Priority)
- MMDモデルのロード/表示を最優先で実装
- 初期は表示の安定性重視（モーションは後続）

## Target Architecture (High Level)
- Unity: UI + Avatar + Minimal Core
- Background: Memory/Embedding/DBは外部プロセスのまま（必要時に段階移行）

## Phased Plan (Summary)
1) Minimal Core仕様固定（設定/イベント）
2) Unity UI Shell PoC（単一ウィンドウ）
3) Unity MMD PoC（表示安定）
4) UIイベントと設定更新の内製化
5) 旧UI/Viewerの停止とUnity一本化

## Logging / Observability
- request_id 相関は維持（Unity側で発行/引き継ぎ）
- UIイベントはメタ情報のみ記録（機密情報なし）

## Risks
- Unity内Coreの肥大化 → 最小機能に限定し段階移行
- MMD安定化に時間がかかる → MMD優先で他機能を後回し

## Open Issues
- MMD再生の最小要件（idleのみ/モーション対応範囲）
- Memory/Embeddingの移行タイミングと分割基準
