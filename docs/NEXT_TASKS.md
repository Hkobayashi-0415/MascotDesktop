# NEXT_TASKS (Unity Migration Plan)

## 改訂履歴
| Rev | Date | 変更内容 | 根拠 |
|---|---|---|---|
| R3 | 2026-02-19 | 既知モデル課題を解消済みへ更新し、関連タスク状態を反映。 | `docs/worklog/2026-02-19_test_run_debug.md` |
| R2 | 2026-02-19 | 旧PoC中心の `P0-P6` 計画を Unity移行後の計画に更新。旧計画は対比表に集約。 | `docs/worklog/2026-02-07_unity-pj-restructure.md`, `docs/worklog/2026-02-07_unity-pj-cutover.md`, `docs/reports/2026-02-19_runtime_error_handling_logging_refactor_proposal.md` |
| R1 | 2026-01-08 | 旧PoC向け `P0-P6` タスクを更新（manifest/smoke/packaging/resident）。 | 旧版 `docs/NEXT_TASKS.md` |

## 旧計画との対比（Pre-Unity vs Unity）
| 旧計画 | 旧計画の主題 | Unity移行後の対応計画 | 現在状態 | 根拠 |
|---|---|---|---|---|
| P0-P2 | PoC起動導線、manifest、packaging準備 | U0: Unity基盤切替とリポジトリ再編 | 完了 | `docs/worklog/2026-02-07_unity-pj-restructure.md`, `docs/worklog/2026-02-07_unity-pj-cutover.md` |
| P3-P4 | Launcher/Resident UX | U1: Unity Runtime基盤（Avatar表示、基本操作、診断） | 完了 | `docs/worklog/2026-02-07_unity-pj-cutover.md`, `docs/worklog/2026-02-16_MascotDesktop_nursetaso_texidx_restore_for_black_white_artifacts.md` |
| P5 | キャラ切替・運用改善 | U4: Unity上の運用UX整備（キャラ切替導線、運用手順） | 未着手 | 本ファイル「U4」 |
| P6 | Core統合（LLM/TTS/STT） | U5: Core統合フェーズ | 未着手 | 本ファイル「U5」 |
| 追加課題 | 旧計画にない障害解析基盤 | U2: エラーハンドリング/ログ強化 | 完了 | `docs/reports/2026-02-19_runtime_error_handling_logging_refactor_proposal.md`, `docs/worklog/2026-02-19_test_run_debug.md` |

## Unity移行計画（現行）

### U0: 移行基盤と切替（Done）
- [x] Active root を Unity前提へ切替
- [x] 旧workspaceを物理分離（legacy参照）
- [x] Unity用 spec/docs 構造の再初期化

### U1: Runtime基盤（Done）
- [x] Runtime側モデルロード基盤の整備
- [x] PMX/VRM表示の基本ルートを運用可能化
- [x] 障害切り分け用の診断ログ導線を整備

### U2: エラーハンドリング/ログ強化（Done）
- [x] 失敗情報 DTO の統一
- [x] `request_id` の縦断伝搬
- [x] Reflection fallback hardening（whitelist + cache）
- [x] RuntimeLog 強化（非同期/rotation/retention/level/filter）
- [x] silent catch の可観測化
- [x] 追加テストの整備と通過確認

### U3: 品質ゲート整備（In Progress）
- [x] `RuntimeErrorHandlingAndLoggingTests` の安定通過（7/7）
- [x] `SimpleModelBootstrapTests` の回帰解消と通過（34/34）
- [ ] Unityテスト実行環境の依存差分吸収手順を標準化
- [ ] 実行環境差異時の artifact 取得手順を統一

### U4: Unity運用UX整備（Not Started）
- [ ] キャラクター切替導線の Unity運用版整理
- [ ] 運用ドキュメントの Unity前提一本化
- [ ] 既知モデル課題の追跡テンプレート統一

### U5: Core統合（Future）
- [ ] LLM/TTS/STT 連携の Unity統合方針を確定
- [ ] IPC/契約（error_code/request_id）を Unity境界で再定義
- [ ] 段階移行（Minimal Core -> Full Core）の受入条件を定義

## 新規タスク（2026-02-19 作成）
| ID | タスク | 優先度 | 状態 | 完了条件 |
|---|---|---|---|---|
| U3-T1 | Unityテスト環境差異の標準復旧手順を `docs/05-dev` に明文化 | High | Open | モジュール不足時の復旧手順と確認コマンドが文書化されている |
| U3-T2 | テスト実行結果の収集フォーマット統一（artifact path, pass/fail, cause） | High | Open | worklog 追記フォーマットが統一され、再現手順が1回で追える |
| U4-T1 | Unity移行後の運用タスクを `NEXT_TASKS` と `dev-status` で同期 | Med | Open | 旧PoC文脈と矛盾しない現行ステータスが維持される |
| U4-T2 | 既知モデル課題の管理粒度を統一（症状/原因/回避/検証） | Med | Done | 既知課題を解消済みへ更新し、追跡状態を明確化した |

## 既知の課題（モデル関連）
- 2026-02-19時点: 解消済み（未解決項目なし）
- 継続方針: 新規課題が発生した場合のみ本セクションへ追加

## 参照
- `docs/worklog/2026-02-07_unity-pj-restructure.md`
- `docs/worklog/2026-02-07_unity-pj-cutover.md`
- `docs/worklog/2026-02-07_unity-migration-integrated-review.md`
- `docs/reports/2026-02-19_runtime_error_handling_logging_refactor_proposal.md`
- `docs/worklog/2026-02-19_test_run_debug.md`
