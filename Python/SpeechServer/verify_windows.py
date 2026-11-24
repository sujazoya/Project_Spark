"""Smoke-test a packaged Vosk sidecar with synthesized Piper audio."""
import argparse
import json
import socket
import subprocess
import tempfile
import time
from pathlib import Path
from urllib.error import URLError
from urllib.request import Request, urlopen

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("package", type=Path)
parser.add_argument("piper", type=Path)
parser.add_argument("voice", type=Path)
args = parser.parse_args()

with socket.socket() as reservation:
    reservation.bind(("127.0.0.1", 0))
    port = reservation.getsockname()[1]

base = f"http://127.0.0.1:{port}"
with tempfile.TemporaryDirectory(prefix="spark-speech-test-") as scratch:
    audio = Path(scratch) / "fixture.wav"
    subprocess.run(
        [str(args.piper.resolve()), "--model", str(args.voice.resolve()), "--output_file", str(audio)],
        input=b"Hello Spark. Please show me the voltage across the resistor.\n",
        cwd=args.piper.resolve().parent, check=True, creationflags=subprocess.CREATE_NO_WINDOW,
    )
    worker = subprocess.Popen(
        [str((args.package / "spark-stt.exe").resolve()), "--host", "127.0.0.1",
         "--port", str(port), "--model", str((args.package / "model").resolve())],
        cwd=args.package.resolve(), creationflags=subprocess.CREATE_NO_WINDOW,
    )
    try:
        for _ in range(80):
            if worker.poll() is not None:
                raise SystemExit(f"The speech sidecar exited with status {worker.returncode}")
            try:
                with urlopen(base + "/health", timeout=1) as reply:
                    if json.load(reply).get("ready"):
                        break
            except (URLError, TimeoutError):
                time.sleep(0.25)
        else:
            raise SystemExit("Timed out waiting for speech sidecar readiness.")
        request = Request(base + "/transcribe", data=audio.read_bytes(), headers={"Content-Type": "audio/wav"})
        with urlopen(request, timeout=15) as reply:
            decoded = json.load(reply)
        words = decoded.get("text", "").lower()
        if "voltage" not in words or "resistor" not in words or not decoded.get("confidence_available"):
            raise SystemExit(f"Speech fixture was not recognized as expected: {decoded}")
        print(f"PASS packaged Windows sidecar: {words!r}; confidence {decoded['confidence']:.2f}")
    finally:
        if worker.poll() is None:
            worker.terminate()
        worker.wait(timeout=5)
        print("PASS owned sidecar shut down.")
