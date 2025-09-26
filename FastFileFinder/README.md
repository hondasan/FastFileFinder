# FastFileFinder

FastFileFinder は Python 製スキャナ `fastfilefinder_scan.py` を WinForms フロントエンドから操作し、大量のドキュメントの全文検索を高速に行うための Windows デスクトップツールです。C# 側は .NET Framework 4.8 / C# 7.3 を対象とし、Python スクリプトは UTF-8 の TSV を逐次出力します。

## 主な機能

- プレーンテキスト・ソースコード・ログ・ZIP 内テキストを対象とした高速検索
- Word (.docx) / Excel (.xlsx) の本文・セル検索（オプションで .doc / .xls も）
- スレッドプールによる並列スキャンと逐次ストリーミング出力
- 1 ファイルあたりのヒット上限 (`--perfile`) や並列度 (`--max-workers`) の制御
- VirtualMode 対応 DataGridView による大量件数のスムーズな表示
- 検索語のスニペットハイライト、並べ替え、クイックフィルタ、右クリックメニュー
- 進捗表示（経過時間 / 処理済みファイル数 / ヒット件数 / 処理中ファイル）とキーボードショートカット（Enter/F5/Ctrl+C/Esc）
- CSV/TSV エクスポート、エクスプローラー選択表示、パスコピー

## ビルドと実行

1. Visual Studio 2019 以降でソリューションを開き、`FastFileFinder` プロジェクトをビルドします（.NET Framework 4.8）。
2. 実行ファイルと同じフォルダに `fastfilefinder_scan.py` が自動コピーされます。
3. Python 3.8 以降をインストールし、以下のパッケージを必要に応じて導入します。
   ```bash
   pip install python-docx openpyxl
   pip install pywin32   # 旧形式 .doc/.xls を検索する場合（要 Microsoft Office）
   ```
4. アプリ起動後、起点フォルダ・検索語などを設定し、必要に応じて以下を切り替えます。
   - ZIP 内も検索
   - Word (.docx) / Excel (.xlsx) / 旧形式 (.doc/.xls)
   - サブフォルダ検索、正規表現検索
   - 対象拡張子フィルタ、1 ファイル上限、並列度
5. 「検索開始」または Enter/F5 でスキャン開始。Esc または「キャンセル」で Python プロセスを停止できます。

## Python スクリプトのコマンドライン引数

```
python fastfilefinder_scan.py --folder <dir> --query <text>
    [--regex] [--zip] [--recursive]
    [--exts "txt;log;cs"]
    [--perfile N] [--max-workers N]
    [--word] [--excel] [--legacy]
```

- `--word` / `--excel` はそれぞれ .docx / .xlsx を対象にします（既定 ON）。
- `--legacy` を付けると .doc / .xls も試みます。`pywin32` と Microsoft Office がインストールされていない場合は自動的にスキップされ、警告のみ出力します。
- 旧形式は COM オートメーション経由で処理するため、並列実行時は初回起動に時間が掛かる場合があります。

## 既知の制限

- .docx/.xlsx 以外の ZIP 内 Office 文書は非対応です。
- `pywin32` / Office が存在しない環境では .doc/.xls はスキップされます。
- 正規表現ハイライトは先頭マッチのみ強調されます。文字列検索では全一致箇所をハイライトします。
- 大量ファイルの並列実行時は Python 側の標準エラーに警告が出力される場合があります（UI 下部ステータスに表示）。

## ライセンス

本リポジトリに含まれるコードの利用条件はリポジトリの LICENSE（存在する場合）を参照してください。
