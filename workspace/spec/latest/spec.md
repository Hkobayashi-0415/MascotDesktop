# 仕様書（Windowsローカル専用）

## 改訂履歴
| 日付 | 内容 |
|---|---|
| 2025-12-26 | public_files-main を参照に追加。WindowController受入条件/AvatarRenderer(Mode3)状態遷移を反映。マッピング表を更新。 |
| 2025-12-30 | PoCエントリを apps/*/poc に整理、ASCIIパス警告を追加。Avatar Mode1 (MMD) を別プロセスViewerで縦切りPoC開始、MMDアセット配置規約を追記。 |

## 1. 背景・目的
- Windowsローカル専用の常駐AIマスコット。CocoroAIはブラックボックス参照のみ。日本語パス非対応リスクを回避（ASCII前提）。

## 2. 想定ユーザー / ユースケース
- デスクトップ常駐で雑談、リマインド通知、音声/テキスト応答、アバター表示。

## 3. 機能一覧（Must/Should/Could）
- Must: 常時起動、Topmost切替、チャット+LLM、キャラ切替、TTS、音声クリップ優先+TTSフォールバック、記憶/埋め込み検索、リマインド、スクショ除外、位置/サイズ保存、ドラッグ移動。
- Should: STT+ウェイクワード、話者識別、アバター3方式(3D/動画/PNGTuber)、クリック透過（安全解除付）、音量/ミキシング、プロンプト版管理、監査ログ、タグ検索。
- Could: Neo4j/グラフ、インターネット検索、ローカルLLM/ONNX、自動アップデータ、ホットキー操作。

## 4. 主要フロー
- 起動: Core→Memory→Shell→Scheduler、設定適用。
- 会話: 入力→プロンプト合成→LLM→音声クリップ優先→TTS→表示、メモリ保存/埋め込み。
- 記憶: ログ→要約/埋め込み→検索。
- 通知: リマインドDB→Shell表示+音声。
- 終了: サービス順停止、設定保存、ログフラッシュ。

## 5. キャラクター機能仕様
- 作成/編集: UIフォーム+バリデーション、テンプレ適用、ドラフト/公開、監査ログ。
- 切替: ユーザー既定キャラ、ホットリロード。
- 配布: JSON/YAMLパッケージ（定義+プロンプト+音声/アバターメタ）。アセット実体は所定フォルダ。

## 6. 音声仕様（TTS+音声クリップ）
- カテゴリ: greeting/ack/happy/angry/confused/reminder/custom。トリガ: 起動/応答/感情/リマインド/操作。
- 優先: クリップ→無ければTTS。priorityとランダムウェイト。割込み許可フラグ、BGM+効果音2ch以内。

## 7. アバター仕様（3D/動画/PNGTuber）
- Mode3(参考: public_files): 状態 01_normal/02_smile/03_oko/04_sleep/05_on、無操作15分→sleep、click→smile/oko/on、10秒でnormal復帰、直前画像除外ランダム。
- Mode1 (MMD): 別プロセスViewerでロード／状態反映を先行。モデル配置は ASCII パスの assets_user 側 (`data/assets_user/characters/<slug>/mmd/`)。初期は表示のみ、VMD再生は後続。
- Mode2(動画ループ) と Mode3 も同じイベントセットを共有。リソース: `<character>/<state>/*`。
- 参照: `docs/02-architecture/avatar/avatar-renderer.md`, `data/templates/assets/pngtuber_mode3/README.md`, `docs/02-architecture/assets/asset-handling.md`.

## 8. ウィンドウ仕様
- フレームレス/透過、ドラッグ移動、位置/サイズ保存/復元。Topmost ON/OFF。クリック透過はオプション＋安全ホットキー。最小/最大サイズガード。

## 9. DBデータモデル案
- characters, character_versions, character_prompts, character_voice_profiles, character_audio_clips, character_assets, tags/character_tags, memory_items, reminders, speaker_profiles, audit_logs, user_character_affinity。

## 10. コンポーネント構成
- Core(LLM/TTS/STT/Embedding, MCP)、Memory(DB/埋め込み/リマインド/話者/監査)、Shell(UI/Audio/Avatar/Window)、Avatar Viewer (Mode1: 別プロセス MMD Viewer PoC)、Scheduler(リマインド/バックアップ/ヘルス)、AudioRenderer、AvatarRenderer。
- IPC/DTO: `docs/02-architecture/interfaces/ipc-contract.md`, dtoサンプル `docs/02-architecture/interfaces/dto/*.json`

## 11. データ配置/Git運用
- Gitルート: workspace。refsは外・read-only。Git除外: venv, db.sqlite3, staticfiles, settings.py, logs, secrets, data/db, assets_user。MMDテンプレは `data/templates/assets/mmd_mode1` に置き、実アセットは `data/assets_user` 側（ASCIIパス）に配置。

## 12. 外部連携
- LLM(OpenAI/Gemini/ローカル)、TTS(VoiceVox/StyleBertVITS2/Aivis)、STT(amivoice)、Embedding(text-embedding-3-large等)、MCP(許可ディレクトリ/コマンド、監査)。

## 13. ログ/監視/デバッグ
- DEBUG/INFO/WARN/ERROR、LLMはメタのみ。チャネル: core/memory/audio/avatar/reminder/import/ui。ヘルスチェックをSchedulerで監視。

## 14. 非機能要件
- 性能: テキスト<3s、音声<10s、STT起動<500ms、UI60fps目標。安定: 個別再起動/フォールバック。セキュリティ: 秘密はenv/安全ストア、スクショ除外、透過解除手段。

## 15. 差分表（v3.5 vs v4.7）
- リマインドDB(Must)、話者識別(Should)、プロンプト外部化(Must)、スクショ除外(Must)、LLMパラメータ(Should)、TTS詳細(Should)、Webサービス(削除)、スケジュール(Could)、Neo4j(Could)、アップデータ(Could)、メモリ有効化(Must)。

## 16. prototype/public_files 抽出結果
- prototype: Characters/Image/Conversations/AffinityをDDLに反映。UI/JS/CSSは参考のみ。
- public_files: PyQtフレームレス/透過、ドラッグ移動、config.json保存、状態遷移(smile/oko/sleep/on)をMode3参考に反映。コード移植なし。

## 17. 未確定点
- enum制約/インデックス、embedding保存方式、ローカルユーザ管理、Topmost/透過のホットキー割当、Mode1/2の状態遷移調整。

## 18. ロードマップ
- Phase1: git init, Core最小(LLM/TTS)、Shell簡易UI、設定保存、ログ。
- Phase2: キャラCRUD+版管理+DDL適用、メモリ要約/埋め込み、リマインド、AvatarRenderer Mode3実装（public_files遷移を適用）、WindowController受入条件を満たす。Mode1(MMD) ViewerをShellと統合し、ロード/ステート反映のIPCを整備。
