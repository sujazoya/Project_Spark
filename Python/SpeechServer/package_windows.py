"""Package the existing Vosk service as a relocatable Windows sidecar."""
import argparse
import importlib.util
import platform
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--model", required=True, type=Path, help="Extracted Vosk model directory")
parser.add_argument("--output", required=True, type=Path, help="New SparkSpeech output directory")
args = parser.parse_args()

if platform.system() != "Windows":
    parser.error("Build this sidecar on Windows.")
if not (args.model / "am" / "final.mdl").is_file() or not (args.model / "conf").is_dir():
    parser.error("Provide a complete extracted Vosk model.")
if importlib.util.find_spec("vosk") is None or importlib.util.find_spec("PyInstaller") is None:
    parser.error("Install requirements-windows.txt in the build environment.")
if args.output.exists():
    parser.error("Use a new output directory to preserve the existing package.")

source = Path(__file__).resolve().with_name("spark_stt.py")
with tempfile.TemporaryDirectory(prefix="spark-speech-package-") as scratch:
    work = Path(scratch)
    subprocess.run([
        sys.executable, "-m", "PyInstaller", "--onedir", "--name", "spark-stt",
        "--collect-all", "vosk", "--distpath", str(work / "dist"),
        "--workpath", str(work / "build"), "--specpath", str(work), str(source),
    ], check=True)
    shutil.copytree(work / "dist" / "spark-stt", args.output)
    shutil.copytree(args.model, args.output / "model")
print(f"Windows voice sidecar written to {args.output.resolve()}")
