# fastfilefinder_scan.py - text/regex scanner with optional ZIP support (UTF-8 stdout)
# Usage:
#   python fastfilefinder_scan.py --folder <dir> --query <q>
#     [--regex] [--zip] [--recursive] [--exts "txt;log;cs"] [--perfile 0]
#
# NOTE:
#   * 既定で「1ファイル内の全ヒット行」を出力します。
#   * 多すぎる場合は --perfile で 1ファイルあたりの上限件数を指定できます（0=無制限）。

import argparse, os, sys, re
from zipfile import ZipFile, BadZipFile
from io import TextIOWrapper

# よくある文字コード（順に試す）
ENCODINGS = (
    "utf-8-sig",     # BOM付きUTF-8
    "utf-16-le", "utf-16-be",  # UTF-16
    "cp932",         # Shift-JIS (Windows-31J)
    "utf-8",         # BOMなしUTF-8
)

# 標準出力は常にUTF-8
try:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
except Exception:
    pass

CACHE = {"rx": {}}

def iter_paths(folder, recursive):
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

def should_target(path, exts):
    if not exts:
        return True
    _, ext = os.path.splitext(path)
    return ext.lower().lstrip(".") in exts

def match_line(line, pattern, is_regex):
    if is_regex:
        rx = CACHE["rx"].get(pattern)
        if rx is None:
            rx = re.compile(pattern, re.IGNORECASE)
            CACHE["rx"][pattern] = rx
        return rx.search(line) is not None
    return pattern.lower() in line.lower()

def out(path, entry, lineno, line):
    line = line.replace("\t", " ").rstrip("\r\n")
    sys.stdout.write(f"{path}\t{entry}\t{lineno}\t{line}\n")
    sys.stdout.flush()

def scan_regular_file(path, pattern, is_regex, exts, perfile):
    if not should_target(path, exts):
        return
    hits = 0
    for enc in ENCODINGS:
        try:
            with open(path, "r", encoding=enc, errors="replace") as r:
                for i, line in enumerate(r, 1):
                    if match_line(line, pattern, is_regex):
                        out(path, "", i, line)
                        hits += 1
                        if perfile and hits >= perfile:
                            return
            break
        except UnicodeDecodeError:
            continue
        except (OSError, IOError):
            return

def scan_zip(path, pattern, is_regex, exts, perfile):
    try:
        with ZipFile(path) as zf:
            for name in zf.namelist():
                # エントリごとの件数カウント
                entry_hits = 0
                _, ext = os.path.splitext(name)
                if exts and ext.lower().lstrip(".") not in exts:
                    continue
                try:
                    for enc in ENCODINGS:
                        try:
                            with zf.open(name, "r") as f, TextIOWrapper(
                                f, encoding=enc, errors="replace"
                            ) as r:
                                for i, line in enumerate(r, 1):
                                    if match_line(line, pattern, is_regex):
                                        out(path, name, i, line)
                                        entry_hits += 1
                                        if perfile and entry_hits >= perfile:
                                            break
                            break
                        except UnicodeDecodeError:
                            continue
                except KeyError:
                    continue
    except BadZipFile:
        return

def scan_file(path, pattern, is_regex, zip_ok, exts, perfile):
    if path.lower().endswith(".zip"):
        if zip_ok:
            scan_zip(path, pattern, is_regex, exts, perfile)
    else:
        scan_regular_file(path, pattern, is_regex, exts, perfile)

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--folder", required=True)
    ap.add_argument("--query", required=True)
    ap.add_argument("--regex", action="store_true")
    ap.add_argument("--zip", action="store_true")
    ap.add_argument("--recursive", action="store_true")
    ap.add_argument("--exts", default="")      # "txt;log;cs" or "txt,log"
    ap.add_argument("--perfile", type=int, default=0, help="1ファイルあたりの最大ヒット数 (0=無制限)")
    args = ap.parse_args()

    exts = set(
        e.strip().lower()
        for e in args.exts.replace(",", ";").split(";")
        if e.strip()
    )

    print("#scanning...", end="")
    for p in iter_paths(args.folder, args.recursive):
        scan_file(p, args.query, args.regex, args.zip, exts, args.perfile)

if __name__ == "__main__":
    main()
