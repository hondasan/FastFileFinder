# FastFileFinder

FastFileFinder は Python 製スキャナ `fastfilefinder_scan.py` を WinForms フロントエンドから呼び出し、テキスト／Office／ZIP アーカイブを高速に全文検索するためのツールです。UI 応答性を最優先に設計し、10 万件規模のファイルでもフリーズせずに検索・キャンセルを扱えるようになっています。

## 特長

- **完全ノンブロッキング UI**: Python プロセスの標準出力は別スレッドで読み取り `ConcurrentQueue` に蓄積。WinForms タイマー (75ms 間隔) で最大 1000 行ずつ DataGridView (VirtualMode) にバッチ反映します。
- **レスポンシブな画面**:
  - 検索開始／キャンセル／CSV 出力のツールバー。
  - 起点フォルダのドラッグ＆ドロップ、MRU ドロップダウン、パス直接入力、フォルダ参照ダイアログ。
  - 除外フォルダ、クイックフィルタ、正規表現切り替え、並列度、Office 対象種別などを 2 列レイアウトで整理。
  - スニペットの検索語をハイライト表示。行ホバー／選択もライトテーマに合わせて強調。
- **常時キャンセル可能**: [Esc] または「キャンセル」で即座に Python プロセスを停止。標準入出力を閉じてから Kill するため、リソースリークを防ぎます。
- **リッチな結果ビュー**:
  - DataGridView VirtualMode + クライアント側ソート／フィルタ。
  - 拡張子列 (Ext) を追加し、Path/Ext/Entry/Line/Snippet で並べ替え可能。
  - ダブルクリックで `explorer.exe /select`、右クリックでフルパスコピー／親フォルダを開く。
  - Ctrl+C で選択行を TSV 形式コピー。
- **詳細な進捗表示**: 経過時間、処理済み／対象ファイル数、ヒット件数、処理中パスをステータスバーに表示。

## 依存関係

| 用途 | 必須/任意 | 推奨バージョン | 備考 |
| --- | --- | --- | --- |
| Python 3.8+ | 必須 |  | `python` コマンドから呼び出されます |
| `python-docx` | 任意 | 最新 | `.docx` の本文検索に使用 |
| `openpyxl` | 任意 | 最新 | `.xlsx` のセル検索に使用 |
| `pywin32` | 任意 | 最新 | Microsoft Word COM を利用して `.doc` (旧形式) をテキスト化。Word 未インストールまたは Office と Python のビット数 (32/64) が一致しない場合は自動スキップ |
| LibreOffice (`soffice`) | 任意 | 7.x 以降 | `.doc` 変換のフォールバック。Word COM が利用できない環境でもテキスト化を試みます |
| `antiword` | 任意 | 最新 | LibreOffice も利用できない場合の最終フォールバック |
| `xlrd` | 任意 | 1.2.x | `.xls` (旧形式) のセル検索に使用。2.x 系では `.xls` 非対応のため 1.2 系を利用してください |

インストール例:

```bash
pip install python-docx openpyxl
pip install "xlrd<2.0"
pip install pywin32
```

これらの依存関係が存在しない場合、該当フォーマットはスキップされ、標準エラーに 1 行だけ警告を出力します。

## 使い方

1. Visual Studio 2019 以降で `FastFileFinder.sln` を開き、.NET Framework 4.8 ターゲットの `FastFileFinder` プロジェクトをビルドします。
2. 実行ファイルと同じフォルダに `fastfilefinder_scan.py` を配置 (プロジェクト構成で自動コピー)。
3. アプリ起動後、次のいずれかで起点フォルダを設定します。
   - フォルダをメインウィンドウにドラッグ＆ドロップ。
   - 最近使ったフォルダ (最大 10 件) から選択。
   - パスを直接入力 (自動補完対応) または「参照」ボタンで選択。
4. 検索条件を設定します。
   - 内容フィルタ (正規表現オプション)。
   - 対象拡張子 (空欄で全ファイル)。
   - 除外フォルダ (`;` / `,` 区切り、既定値: `.git;node_modules;bin;obj;.vs`)。
   - 並列度 (0=自動)、ZIP 内検索、Word/Excel/旧形式の ON/OFF。
5. [Enter] またはツールバーの「検索開始」でスキャンを開始。[Esc] または「キャンセル」で即時停止できます。
6. 結果は DataGridView にストリーミング表示されます。クイックフィルタで表示中結果を絞り込み、ソートや CSV/TSV エクスポートも可能です。

### キーボードショートカット

- **Enter / F5**: 再検索
- **Esc**: キャンセル
- **Ctrl+C**: 選択行をコピー
- **ダブルクリック**: エクスプローラーで該当ファイルを選択表示

## Python スキャナのオプション

```text
python fastfilefinder_scan.py --folder <dir> --query <text>
    [--regex] [--zip] [--recursive]
    [--exts "txt;log;cs"]
    [--exclude-folders ".git;bin"]
    [--perfile N] [--max-workers N]
    [--word] [--excel] [--legacy]
```

- `--legacy` を有効にすると `.doc` / `.xls` を試行します。`pywin32` (および Microsoft Word) や `xlrd (<=1.2)` が無い場合は警告のみでスキップします。
- `--exclude-folders` はフォルダ名単位でマッチし、サブツリー全体を探索対象から除外します。
- `--max-workers` を 0 (既定) にすると `os.cpu_count()` を基準に自動調整します。

## チューニングと注意点

- 大量ファイルを扱う場合は除外フォルダと対象拡張子を積極的に設定し、探索対象を絞ってください。
- ネットワークパスや長いパスは自動的に `\\?\` プレフィックスへ変換されるため、Windows のパス長制限を超えていても検索できます。
- 正規表現ハイライトは先頭マッチのみ、文字列検索では全マッチを強調します。
- `.doc` / `.xls` の検索は純粋なテキストベース変換のため、複雑なレイアウトや埋め込みオブジェクトは検索対象外となります。

### 旧形式 Word (.doc) の変換フロー

`.doc` は次の優先順位でテキスト化を試行します。どれかが成功した時点で後続のフォールバックは実行されません。

1. **Microsoft Word COM (pywin32)** — Word がインストールされていて Python とビット数 (32/64) が一致している場合。
2. **LibreOffice (`soffice --headless`)** — Word COM が利用できない、または失敗したときに自動的に呼び出します。
3. **antiword** — 上記がすべて失敗した場合の最終フォールバック。

すべての経路で失敗した場合でもツールは継続し、標準エラー出力に 1 行のメッセージを残します。例: `ERR .doc convert failed [soffice-not-found]: C:\docs\sample.doc`。`pywin32` が未インストールの場合も同様に `ERR .doc convert failed [COM-missing]: ...` が出力されます。

## ライセンス

本リポジトリに含まれるコードの利用条件は同梱のライセンス (存在する場合) に従ってください。
