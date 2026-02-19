# Worklog: RuntimeLog スレッド問題の修正と検証

- Date: 2026-02-19
- Task: `RuntimeLog` の `UnityException` 修正（バックグラウンドスレッドからの `persistentDataPath` 参照エラー）
- Execution-Tool: Antigravity
- Execution-Agent: gemini
- Execution-Model: gemini-2.0-pro-exp-0211
- Used-Skills: Unity Debugging, C# Threading
- Repo-Refs: d:\dev\MascotDesktop\Unity_PJ\project\Assets\Scripts\Runtime\Diagnostics\RuntimeLog.cs
- Obsidian-Refs: n/a
- Report-Path: docs/worklog/2026-02-19_threading_fix.md
- Obsidian-Log: 未実施:小規模バグ修正でありworklogで完結させるため
- Tags: [agent/gemini, model/gemini-2.0-pro-exp-0211, tool/antigravity]

## Summary
`RuntimeLog.WriteBatch` がバックグラウンドスレッドから `persistentDataPath` を参照したため、`UnityException: get_persistentDataPath can only be called from the main thread` が発生し、`RuntimeErrorHandlingAndLoggingTests` が失敗していました。
これに対し、メインスレッドでログディレクトリのパスをキャッシュすることで問題を解決しました。

## Changes
- `Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs` の修正:
    - 静的フィールド `cachedLogDir` を追加。
    - 静的コンストラクタ内で `cachedLogDir` を初期化（Unityドメインロード時にメインスレッドで実行されることを利用）。
    - `WriteBatch` メソッドを修正し、APIを直接呼ばずに `cachedLogDir` を使用するように変更。
    - フォールバックロジックを追加（ただし基本的には静的初期化で解決される）。

## Commands
- テスト実行: `./tools/run_unity_tests.ps1 -TestPlatform EditMode -TestFilter "MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests"`

## Tests
- `MascotDesktop.Tests.EditMode.RuntimeErrorHandlingAndLoggingTests`
    - 結果: Passed (7/7 tests)
    - ログ: `artifacts/test-results/editmode-20260219_130744.xml`
    - 検証: `RuntimeLogPolicy_RotationAndRetention_AreApplied` が `UnityException` なしで通過することを確認。

## Rationale (Key Points)
- `Application.persistentDataPath` はスレッドセーフではない。
- 静的コンストラクタはメインスレッドで実行されるため、安全にパスをキャッシュできる。
- これにより、バックグラウンドスレッド（`WriterLoop`）で動作する `WriteBatch` が安全な文字列変数にアクセスできるようになった。

## ロールバック方針
- `git restore Assets/Scripts/Runtime/Diagnostics/RuntimeLog.cs`

## Record Check
- Report-Path exists: True
- Repo-Refs populated: Yes
- Obsidian-Refs populated (or n/a): Yes
- Obsidian-Log recorded (path or reason): Yes
- Execution fields recorded: Yes
- Tags include agent/model/tool: Yes

## 次アクション
- 新たな要件が発生した場合、ログ周りの他のスレッド問題を監視する。
