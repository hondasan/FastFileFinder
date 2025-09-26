# fastfilefinder_scan.py - fast content scanner with optional Office document support
# Outputs UTF-8 TSV lines: path \t entry \t lineno \t snippet

import argparse
import os
import re
import sys
import threading
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from io import TextIOWrapper
from zipfile import BadZipFile, ZipFile

# Optional dependencies
try:  # python-docx for .docx
    from docx import Document  # type: ignore
except Exception:  # pragma: no cover - dependency may be missing
    Document = None  # type: ignore

try:  # openpyxl for .xlsx
    import openpyxl  # type: ignore
    from openpyxl.utils.cell import get_column_letter  # type: ignore
except Exception:  # pragma: no cover - dependency may be missing
    openpyxl = None  # type: ignore
    get_column_letter = None  # type: ignore

try:  # legacy Office via COM
    import pythoncom  # type: ignore
    import win32com.client  # type: ignore
except Exception:  # pragma: no cover - optional
    pythoncom = None  # type: ignore
    win32com = None  # type: ignore


# Common encodings to try for text files
ENCODINGS = (
    "utf-8-sig",
    "utf-16-le",
    "utf-16-be",
    "cp932",
    "utf-8",
)

# Ensure stdout is UTF-8
try:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace", line_buffering=True)
except Exception:
    pass

stdout_lock = threading.Lock()
warned = set()


def emit_tsv(path: str, entry: str, lineno: int, line: str) -> None:
    line = line.replace("\t", " ").rstrip("\r\n")
    with stdout_lock:
        sys.stdout.write(f"{path}\t{entry}\t{lineno}\t{line}\n")
        sys.stdout.flush()


def emit_status(tag: str, *parts: object) -> None:
    payload = "\t".join(str(p) for p in parts)
    with stdout_lock:
        sys.stdout.write(f"#{tag}\t{payload}\n")
        sys.stdout.flush()


def warn_once(kind: str, message: str) -> None:
    if kind in warned:
        return
    warned.add(kind)
    sys.stderr.write(message + "\n")
    sys.stderr.flush()


def iter_paths(folder: str, recursive: bool):
    if recursive:
        for root, _, files in os.walk(folder):
            for name in files:
                yield os.path.join(root, name)
    else:
        try:
            for name in os.listdir(folder):
                p = os.path.join(folder, name)
                if os.path.isfile(p):
                    yield p
        except Exception:
            return


def normalize_ext(ext: str) -> str:
    return ext.lower().lstrip(".")


def should_target(path: str, exts: set) -> bool:
    if not exts:
        return True
    return normalize_ext(os.path.splitext(path)[1]) in exts


def build_matcher(pattern: str, use_regex: bool):
    if use_regex:
        rx = re.compile(pattern, re.IGNORECASE)

        def matcher(line: str):
            return rx.search(line)

        return matcher

    lowered = pattern.lower()

    def matcher(line: str):
        return lowered in line.lower()

    return matcher


def scan_text_file(path: str, matcher, exts: set, perfile: int) -> int:
    if not should_target(path, exts):
        return 0
    hits = 0
    for enc in ENCODINGS:
        try:
            with open(path, "r", encoding=enc, errors="replace") as reader:
                for lineno, line in enumerate(reader, 1):
                    if matcher(line):
                        emit_tsv(path, "", lineno, line)
                        hits += 1
                        if perfile and hits >= perfile:
                            return hits
            break
        except UnicodeDecodeError:
            continue
        except (OSError, IOError):
            return hits
    return hits


def scan_zip(path: str, matcher, exts: set, perfile: int) -> int:
    hits = 0
    try:
        with ZipFile(path) as zf:
            for name in zf.namelist():
                if exts and normalize_ext(os.path.splitext(name)[1]) not in exts:
                    continue
                entry_hits = 0
                try:
                    for enc in ENCODINGS:
                        try:
                            with zf.open(name, "r") as raw, TextIOWrapper(
                                raw, encoding=enc, errors="replace"
                            ) as reader:
                                for lineno, line in enumerate(reader, 1):
                                    if matcher(line):
                                        emit_tsv(path, name, lineno, line)
                                        entry_hits += 1
                                        hits += 1
                                        if perfile and entry_hits >= perfile:
                                            break
                            break
                        except UnicodeDecodeError:
                            continue
                except KeyError:
                    continue
    except BadZipFile:
        pass
    except Exception as exc:
        warn_once(f"zip:{path}", f"ZIP 読み取り失敗: {path} ({exc})")
    return hits


def iter_docx_lines(doc):
    line_no = 0
    for idx, para in enumerate(doc.paragraphs, 1):
        text = para.text.strip()
        line_no += 1
        yield line_no, f"paragraph:{idx}", text
    for t_idx, table in enumerate(doc.tables, 1):
        for r_idx, row in enumerate(table.rows, 1):
            cells = [cell.text.strip() for cell in row.cells]
            text = "\t".join(cells).strip()
            line_no += 1
            yield line_no, f"table{t_idx}:row{r_idx}", text


def scan_docx(path: str, matcher, perfile: int) -> int:
    if Document is None:
        warn_once("docx", "python-docx がインストールされていないため .docx をスキップします")
        return 0
    hits = 0
    try:
        doc = Document(path)
    except Exception as exc:
        warn_once(f"docx:{path}", f".docx 読み込み失敗: {path} ({exc})")
        return 0
    for lineno, entry, text in iter_docx_lines(doc):
        if not text:
            continue
        if matcher(text):
            emit_tsv(path, entry, lineno, text)
            hits += 1
            if perfile and hits >= perfile:
                break
    return hits


def iter_xlsx_cells(workbook):
    for sheet in workbook.worksheets:
        title = sheet.title
        for row_idx, row in enumerate(sheet.iter_rows(values_only=True), 1):
            for col_idx, value in enumerate(row, 1):
                if value is None:
                    continue
                text = str(value).strip()
                if not text:
                    continue
                addr = f"{title}!{get_column_letter(col_idx)}{row_idx}" if get_column_letter else f"{title}!{col_idx},{row_idx}"
                yield row_idx, addr, text


def scan_xlsx(path: str, matcher, perfile: int) -> int:
    if openpyxl is None:
        warn_once("xlsx", "openpyxl がインストールされていないため .xlsx をスキップします")
        return 0
    hits = 0
    try:
        wb = openpyxl.load_workbook(path, read_only=True, data_only=True)
    except Exception as exc:
        warn_once(f"xlsx:{path}", f".xlsx 読み込み失敗: {path} ({exc})")
        return 0
    try:
        for lineno, entry, text in iter_xlsx_cells(wb):
            if matcher(text):
                emit_tsv(path, entry, lineno, text)
                hits += 1
                if perfile and hits >= perfile:
                    break
    finally:
        try:
            wb.close()
        except Exception:
            pass
    return hits


def scan_doc_legacy(path: str, matcher, perfile: int) -> int:
    if win32com is None or pythoncom is None:
        warn_once("doc", "pywin32 がないため .doc をスキップします")
        return 0
    hits = 0
    pythoncom.CoInitialize()
    try:
        try:
            word = win32com.client.Dispatch("Word.Application")
        except Exception as exc:
            warn_once("doc-open", f"Word COM を初期化できません (.doc スキップ): {exc}")
            return 0
        word.Visible = False
        try:
            doc = word.Documents.Open(
                path,
                ConfirmConversions=False,
                ReadOnly=True,
                AddToRecentFiles=False,
            )
        except Exception as exc:
            warn_once(f"doc:{path}", f".doc 読み込み失敗: {path} ({exc})")
            word.Quit()
            return 0
        try:
            text = doc.Range().Text
        finally:
            doc.Close(False)
            word.Quit()
        for lineno, line in enumerate(text.splitlines(), 1):
            if matcher(line):
                emit_tsv(path, "doc", lineno, line)
                hits += 1
                if perfile and hits >= perfile:
                    break
    finally:
        pythoncom.CoUninitialize()
    return hits


def scan_xls_legacy(path: str, matcher, perfile: int) -> int:
    if win32com is None or pythoncom is None:
        warn_once("xls", "pywin32 がないため .xls をスキップします")
        return 0
    hits = 0
    pythoncom.CoInitialize()
    try:
        try:
            excel = win32com.client.Dispatch("Excel.Application")
        except Exception as exc:
            warn_once("xls-open", f"Excel COM を初期化できません (.xls スキップ): {exc}")
            return 0
        excel.Visible = False
        excel.DisplayAlerts = False
        try:
            wb = excel.Workbooks.Open(path, ReadOnly=True)
        except Exception as exc:
            warn_once(f"xls:{path}", f".xls 読み込み失敗: {path} ({exc})")
            excel.Quit()
            return 0
        try:
            for sheet in wb.Worksheets:
                try:
                    used = sheet.UsedRange
                    values = used.Value
                except Exception:
                    continue
                rows = values
                if rows is None:
                    continue
                if not isinstance(rows, (list, tuple)):
                    rows = [[rows]]
                for row_idx, row in enumerate(rows, 1):
                    if not isinstance(row, (list, tuple)):
                        row = [row]
                    for col_idx, value in enumerate(row, 1):
                        if value in (None, ""):
                            continue
                        text = str(value).strip()
                        if not text:
                            continue
                        entry = f"{sheet.Name}!{col_idx},{row_idx}"
                        if matcher(text):
                            emit_tsv(path, entry, row_idx, text)
                            hits += 1
                            if perfile and hits >= perfile:
                                excel.Quit()
                                return hits
        finally:
            wb.Close(False)
            excel.Quit()
    finally:
        pythoncom.CoUninitialize()
    return hits


def scan_file(path: str, matcher, args, exts: set) -> int:
    ext = normalize_ext(os.path.splitext(path)[1])
    perfile = args.perfile
    if ext == "zip":
        if args.zip:
            return scan_zip(path, matcher, exts, perfile)
        return 0
    if not should_target(path, exts):
        return 0
    if ext == "docx" and args.word:
        return scan_docx(path, matcher, perfile)
    if ext == "xlsx" and args.excel:
        return scan_xlsx(path, matcher, perfile)
    if ext == "doc" and args.legacy and args.word:
        return scan_doc_legacy(path, matcher, perfile)
    if ext == "xls" and args.legacy and args.excel:
        return scan_xls_legacy(path, matcher, perfile)
    return scan_text_file(path, matcher, exts, perfile)


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--folder", required=True)
    ap.add_argument("--query", required=True)
    ap.add_argument("--regex", action="store_true")
    ap.add_argument("--zip", action="store_true")
    ap.add_argument("--recursive", action="store_true")
    ap.add_argument("--exts", default="")
    ap.add_argument("--perfile", type=int, default=0)
    ap.add_argument("--word", action="store_true")
    ap.add_argument("--excel", action="store_true")
    ap.add_argument("--legacy", action="store_true")
    ap.add_argument("--max-workers", type=int, default=0)
    args = ap.parse_args()

    ext_filter = set(
        e.strip().lower()
        for e in args.exts.replace(",", ";").split(";")
        if e.strip()
    )

    try:
        matcher = build_matcher(args.query, args.regex)
    except re.error as exc:
        sys.stderr.write(f"正規表現エラー: {exc}\n")
        sys.stderr.flush()
        return

    files = list(iter_paths(args.folder, args.recursive))
    emit_status("queued", len(files))

    max_workers = args.max_workers if args.max_workers > 0 else (os.cpu_count() or 4)
    max_workers = max(1, max_workers)
    processed = 0
    total_hits = 0
    start = time.time()

    def worker(p: str):
        emit_status("current", p)
        try:
            return p, scan_file(p, matcher, args, ext_filter)
        except Exception as exc:  # pragma: no cover - unexpected
            warn_once(f"file:{p}", f"処理失敗: {p} ({exc})")
            return p, 0

    with ThreadPoolExecutor(max_workers=max_workers) as executor:
        futures = {executor.submit(worker, p): p for p in files}
        for fut in as_completed(futures):
            path = futures[fut]
            try:
                _, hits = fut.result()
            except Exception as exc:  # pragma: no cover - already handled
                warn_once(f"future:{path}", f"処理中に例外: {path} ({exc})")
                hits = 0
            processed += 1
            total_hits += hits
            emit_status("progress", processed, total_hits, path)

    elapsed = time.time() - start
    emit_status("done", processed, total_hits, f"{elapsed:.3f}")


if __name__ == "__main__":
    main()
