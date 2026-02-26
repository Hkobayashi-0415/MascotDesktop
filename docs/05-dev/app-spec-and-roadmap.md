# Application Feature Specification and Roadmap (APP-T1)

- Status: active
- Last Updated: 2026-02-26
- Owner/Agent: claude-code
- Scope: MascotDesktop アプリケーション開発フェーズの機能仕様・ロードマップ・受入条件

## 根拠

- `Unity_PJ/spec/latest/spec.md`（UR-001〜UR-012）
- `docs/NEXT_TASKS.md`（APP-T1/T2/T3 初手タスク）
- `docs/05-dev/dev-status.md`（App Dev 移行条件確定）
- `docs/05-dev/app-t2-full-core-design.md`（APP-T2 実装前詳細設計）
- `docs/05-dev/u5-core-integration-plan.md`（Full Core切替計画）
- `docs/02-architecture/interfaces/ipc-contract.md`（IPC境界契約）
- `docs/PACKAGING.md`（legacy-reference）
- `docs/RESIDENT_MODE.md`（legacy-reference）

---

## 1. 目的

アプリケーション開発フェーズ（U0〜U9 完了後）において、以下を確定する:

1. 優先機能リストと受入条件（DoD）を文書で固定する。
2. MVP（Phase 1）とそれ以降（Phase 2）の境界を明確にする。
3. APP-T2（Full Core接続）および APP-T3（配布パッケージング）が着手可能な粒度で定義する。

---

## 2. スコープ / 非スコープ

### スコープ
- 機能仕様の定義と優先度付け
- Phase 1 / Phase 2 の分割と依存関係の明示
- 各機能の受入条件（テスト可能な DoD）の確定
- リスクと回避策の明文化

### 非スコープ
- 実装コードの変更
- Full Core 実装（APP-T2 スコープ）
- インストーラー/配布パッケージ実装（APP-T3 スコープ）
- 外部クラウド連携・CI/CD 自動化の新規実装

---

## 3. UR 要件トレーサビリティ

| UR | 要件概要 | 対応機能ID | 状態 |
|---|---|---|---|
| UR-001 | Single Runtime（Unity単独動作） | F-01 | Done |
| UR-002 | Window Control（frameless/drag/topmost/position） | F-02, F-03 | Done |
| UR-003 | Resident Operation（hide/show/exit） | F-03 | Done（SW_MINIMIZE/RESTORE 対応済み） |
| UR-004 | Avatar MMD Load（ローカルアセット解決） | F-02 | Done |
| UR-005 | Motion Slot Playback（slot/fallback） | F-02 | Done |
| UR-006 | State Event Handling（state遷移） | F-02, F-04 | Done |
| UR-007 | Request Correlation（request_id 縦断） | F-07 | Done |
| UR-008 | Error Contract（error_code/retryable） | F-04, F-05, F-06 | Done |
| UR-009 | Logging（structured log/metadata） | F-12 | Done（U8運用チェック含む） |
| UR-010 | Asset Policy（project-relative path） | F-02 | Done |
| UR-011 | Legacy Cutover Safety（Legacy分離） | F-08 | Done |
| UR-012 | Windows Local Constraint（Windows専用） | F-01 | Done |

---

## 4. 機能一覧と受入条件（DoD）

### Phase 1 — MVP（F-01〜F-09, F-12）

#### F-01: Unity起動と Runtime HUD 表示
- **状態**: Done
- **関連 UR**: UR-001, UR-012
- **DoD**:
  - `docs/05-dev/QUICKSTART.md` の起動手順でプロジェクトが起動できる。
  - Runtime HUD が表示され、モデル表示・主要ボタンが操作可能。
- **テスト方法**:
  - 手動確認: `docs/05-dev/unity-runtime-manual-check.md` の合格条件
  - EditMode: `SimpleModelBootstrapTests`（36/36 Pass）

#### F-02: モデル切替 / 状態遷移 / motion操作
- **状態**: Done
- **関連 UR**: UR-002, UR-004, UR-005, UR-006, UR-010
- **DoD**:
  - `AssetCatalogService` 経由でモデルが切り替わる。
  - motion slot が再生でき、slot 欠損時に fallback が適用される。
  - state 遷移イベントが runtime ログに記録される。
- **テスト方法**:
  - EditMode: `AssetCatalogServiceTests`（4/4 Pass）
  - 手動確認: `docs/05-dev/unity-character-switch-operations.md` の手順

#### F-03: 常駐導線（Show/Hide/Topmost/Exit/Logs）
- **状態**: Done（Conditional Pass — SW_MINIMIZE/RESTORE で実装）
- **関連 UR**: UR-002, UR-003
- **DoD**:
  - Hide/Show 操作でウィンドウが最小化/復帰する。
  - Topmost toggle が動作し、`window.topmost.changed` ログイベントが記録される。
  - `window.resident.hidden` / `window.resident.restored` ログが確認できる。
- **テスト方法**:
  - EditMode: `WindowNativeGatewayTests`（1/1 Pass）
  - 手動確認: runtime log で必須5イベントを確認（`docs/worklog/2026-02-24_u7_t4_t5_execution.md` 参照）

#### F-04: LLM連携（chat + fallback）
- **状態**: Done（Minimal Core / loopback）
- **関連 UR**: UR-006, UR-007, UR-008
- **DoD**:
  - chat 成功時に応答が state 遷移に反映される。
  - 外部 Core 未起動時に `IPC.HTTP.DISABLED` フォールバックが発動する。
  - `request_id` が要求から state 反映まで一貫して追跡可能。
- **テスト方法**:
  - EditMode: `CoreOrchestratorLlmIntegrationTests`（5/5 Pass）

#### F-05: TTS連携（`/v1/tts/play`）
- **状態**: Done（Minimal Core / loopback）
- **関連 UR**: UR-007, UR-008
- **DoD**:
  - TTS 再生要求が `/v1/tts/play` エンドポイントに到達する。
  - TTS 失敗時に `retryable` 判定付きの `error_code` が記録される。
- **テスト方法**:
  - EditMode: `CoreOrchestratorTtsIntegrationTests`（3/3 Pass）

#### F-06: STT連携（partial/final）
- **状態**: Done（Minimal Core / loopback）
- **関連 UR**: UR-007, UR-008
- **DoD**:
  - partial/final が区別されたログイベントが残る。
  - 誤認識時に state が破綻しない（ログで確認可）。
- **テスト方法**:
  - EditMode: `CoreOrchestratorSttIntegrationTests`（4/4 Pass）

#### F-07: Loopback 契約検証（request_id / error schema）
- **状態**: Done
- **関連 UR**: UR-007, UR-008
- **DoD**:
  - `X-Request-Id` ≡ body `request_id` ≡ response `request_id` の一致が EditMode で検証済み。
  - error schema（`error_code` / `message` / `retryable` / `request_id`）が安定して返る。
- **テスト方法**:
  - EditMode: `LoopbackHttpClientTests`（5/5 Pass）

#### F-08: 状態同期（NEXT_TASKS / dev-status / worklog）
- **状態**: Done
- **関連 UR**: UR-011
- **DoD**:
  - `docs/NEXT_TASKS.md` と `docs/05-dev/dev-status.md` の状態表現が一致している。
  - worklog に Record Check 全項目が充足されている。
- **テスト方法**:
  - `grep -n "APP-T1" docs/NEXT_TASKS.md docs/05-dev/dev-status.md` で同期確認

#### F-09: Full Core 接続（loopback dummy → 実エンドポイント切替）
- **状態**: In Progress（Design Done）— APP-T2 スコープ
- **関連 UR**: UR-001, UR-006, UR-007, UR-008
- **DoD**:
  - `docs/05-dev/app-t2-full-core-design.md` の契約/運用/受入条件に整合する。
  - `RuntimeConfig` で実 LLM/TTS/STT エンドポイント（例: `127.0.0.1:8769`）への切り替えが可能。
  - 全4スイート（LLM/TTS/STT/Loopback）が Full Core 接続状態で Pass（EditMode）。
  - `docs/05-dev/u5-core-integration-plan.md` §3 U5-P4 受入条件をすべて満たす。
  - 実 Core 未起動時に `IPC.HTTP.DISABLED` フォールバックが維持される。
  - 切替手順と判定根拠が `docs/05-dev` に追記される。
- **テスト方法**:
  - EditMode: 全4スイート（LLM 5/5, TTS 3/3, STT 4/4, Loopback 5/5）
  - 手動: Runtime HUD から実 Core 接続で chat/TTS/STT が動作確認

#### F-12: 運用監視継続（U8 checks 定期実行）
- **状態**: Done（運用中）
- **関連 UR**: UR-009
- **DoD**:
  - `./tools/run_u8_ops_checks.ps1 -Profile Daily` が正常実行できる（exit 0）。
  - `./tools/check_u8_ops_freshness.ps1` で鮮度管理（ThresholdHours=25）が動作する。
  - 異常時は `docs/worklog/_templates/u8_ops_fail_template.md` に従って記録・対処できる。
- **テスト方法**:
  - `./tools/run_u8_ops_checks.ps1 -Profile Gate` → exit 0 + artifact 生成確認

---

### Phase 2 — 拡張候補（凍結 / non-blocking）

#### F-10: 配布パッケージング（Unity Build → インストーラー導線）
- **状態**: Not Started — APP-T3 スコープ
- **凍結方針**: Phase 2 着手まで non-blocking 凍結。APP-T3 で実施。
- **DoD**（APP-T3 到達時に確認）:
  - Unity Windows Standalone Player ビルドが成功する。
  - `docs/PACKAGING.md` が Unity 向け手順に更新される（旧 PyInstaller 手順は legacy-reference へ分離）。
  - 配布物（.exe または .zip）のテスト手順が `docs/05-dev` に記載される。
  - Windows Defender 署名未対応の既知制約が文書化されている。

#### F-11: トレイ常駐（システムトレイアイコン制御）
- **状態**: Not Started — Phase 2 候補
- **凍結方針**: 現行 Hide/Show（SW_MINIMIZE/RESTORE）で代替運用可。Phase 2 以降で実装判断。
- **non-blocking 根拠**: UR-003 の「hide/show/exit」は現行 Unity 実装で運用可能（R2 Conditional Pass 確定済み）。トレイアイコン UI は次フェーズの UX 改善として分離する。
- **DoD**（Phase 2 着手時に確認）:
  - システムトレイアイコンから Show/Hide/Exit が操作できる。
  - 現行実装との機能差分が文書化されている。

---

## 5. Phase 分割サマリー

| Phase | 機能 | 完了条件の概要 | 前提 |
|---|---|---|---|
| **Phase 1 / MVP** | F-01〜F-09, F-12 | Full Core接続完了 + 全4スイートPass + 運用チェック正常 | U9 Done（完了済み）+ APP-T2 完了 |
| **Phase 2** | F-10, F-11 | Unity Build 配布可能 + トレイ常駐実装 | Phase 1 完了 |

---

## 6. タスク依存関係

```
U9 Done (完了)
    └─ APP-T1 (本文書) [仕様確定]
         ├─ APP-T2 [F-09 実装: Full Core接続]
         │    └─ APP-T3 [F-10 実装: 配布パッケージング]
         └─ F-12 継続運用 (U8 checks)
              └─ Phase 2: F-11 (トレイ常駐) ← APP-T1完了後に判断
```

---

## 7. リスクと回避策

| # | リスク | 重大度 | 回避策 |
|---|---|---|---|
| R1 | Full Core（LLM/TTS/STT）未起動時のアプリ機能劣化 | Med | `IPC.HTTP.DISABLED` フォールバック維持（既実装）。HUD に Core 未接続状態を表示。 |
| R2 | Unity 実行環境の断続起動失敗（`指定されたモジュールが見つかりません`） | Med | `run_unity_tests.ps1 -RequireArtifacts` + 手動直接実行で検証。環境依存事象として記録継続。 |
| R3 | 配布パッケージの Windows Defender 署名未対応 | Low | 既知制約として PACKAGING.md に明記。署名取得は Phase 2 以降で判断。 |
| R4 | トレイ常駐未実装による UX 差分 | Low | Hide/Show の HUD 操作で代替。Phase 2 で正式実装。non-blocking 凍結を明記済み。 |
| R5 | Task Scheduler 断続障害（schtasks.exe モジュール不足） | Low | 管理済み制約（non-blocking）。`diagnose_u8_scheduler.ps1` で事前確認 + 手動日次代替。 |

---

## 8. APP-T2 引き渡し条件（F-09 DoD 前提）

APP-T2 着手のために以下が確認されていること:

| 条件 | 根拠 | 状態 |
|---|---|---|
| APP-T2 実装前の詳細設計（契約/アーキ/DoD）が確定済み | `docs/05-dev/app-t2-full-core-design.md` | ✅ Done（Design） |
| IPC契約（loopback HTTP / request_id / error schema）が確定済み | `docs/02-architecture/interfaces/ipc-contract.md` | ✅ Done |
| Minimal Core Bridge（health/chat/config）が実装済み | `CoreOrchestrator`, `LoopbackHttpClient` | ✅ Done |
| 全4スイートが loopback state で Pass | `editmode-20260225_000024.xml` 他 50/50 | ✅ Done |
| `RuntimeConfig` で endpoint を切り替える設計が存在する | `Unity_PJ/project/Assets/Scripts/Runtime/Config/RuntimeConfig.cs` | 要確認（APP-T2で実装） |
| 実 Core の起動手順・ポート定義が確認できる | `docs/PACKAGING.md`（旧 PoC: 8769/8770） | 要 APP-T2 更新 |

---

## 9. 完了定義（APP-T1）

- [x] 本文書（`app-spec-and-roadmap.md`）が `docs/05-dev` に存在する。
- [x] 全機能 ID（F-01〜F-12）に受入条件が定義されている。
- [x] Phase 1 / Phase 2 の境界が明確。
- [x] APP-T2 / APP-T3 が着手可能な粒度で前提・依存・DoD が記載されている。
- [x] 残件（F-10, F-11）は non-blocking 凍結として明示されている。
- [x] UR 要件とのトレーサビリティが確認できる。
- [x] `docs/NEXT_TASKS.md` / `docs/05-dev/dev-status.md` が同期されている。
- [x] `docs/worklog/2026-02-26_app_t1_spec_and_roadmap.md` が作成されている。
