"""Run actual CPU semantic inference through the loopback gateway with authored candidate IDs."""
import argparse
import json
import statistics
import subprocess
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("server", type=Path)
parser.add_argument("model", type=Path)
parser.add_argument("--out", required=True, type=Path)
args = parser.parse_args()
root = Path(__file__).resolve().parent
log_dir = args.out.parent
log_dir.mkdir(parents=True, exist_ok=True)
llm_url = "http://127.0.0.1:8778"
gateway_url = "http://127.0.0.1:18767"
opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))

def ready(base, worker):
    for _ in range(100):
        if worker.poll() is not None:
            raise RuntimeError(f"Service exited with code {worker.returncode}: {base}")
        try:
            with opener.open(base + "/health", timeout=1) as reply:
                if reply.status == 200:
                    return
        except (urllib.error.URLError, TimeoutError):
            time.sleep(0.25)
    raise RuntimeError(f"Service did not become ready: {base}")

tests = [
    ("Could you give me a rundown of the tasks awaiting attention?", "objectives"),
    ("I'm pointing at a device; help me identify the component.", "inspect"),
    ("The LED stays dark; suggest which electrical check to make first.", "diagnose"),
    ("Which chapter of the Project Spark campaign am I in?", "campaign"),
    ("Which authored lesson teaches parallel circuits?", "search"),
    ("What should I study again based on my prior practice?", "review"),
]
candidates = [
    {"id": "inspect", "description": "Identify the selected electrical component or describe its terminals."},
    {"id": "diagnose", "description": "Explain why a simulator circuit or objective may be failing and suggest a check."},
    {"id": "objectives", "description": "Report unfinished objectives in the active game mission."},
    {"id": "search", "description": "Find an authored Project Spark lesson or mission by topic."},
    {"id": "review", "description": "Suggest a review from the learner's existing evidence."},
    {"id": "campaign", "description": "Describe the current campaign or chapter."},
]

with (log_dir / "LlamaServer.txt").open("wb") as llama_log, (log_dir / "SemanticGateway.txt").open("wb") as gateway_log:
    processes = []
    lines = [f"Actual CPU model: {args.model.name} ({args.model.stat().st_size} bytes)"]
    try:
        worker = subprocess.Popen(
            [str(args.server.resolve()), "-m", str(args.model.resolve()), "--host", "127.0.0.1",
             "--port", "8778", "-t", "4", "-c", "2048", "-ngl", "0", "--alias", "spark-phase4-local"],
            cwd=args.server.resolve().parent, stdout=llama_log, stderr=subprocess.STDOUT,
            creationflags=subprocess.CREATE_NO_WINDOW,
        )
        processes.append(worker)
        ready(llm_url, worker)
        gateway = subprocess.Popen(
            [sys.executable, "-B", str(root / "spark_semantic_gateway.py"), "--mode", "llamacpp",
             "--model", "spark-phase4-local", "--port", "18767", "--timeout", "15"],
            stdout=gateway_log, stderr=subprocess.STDOUT, creationflags=subprocess.CREATE_NO_WINDOW,
        )
        processes.append(gateway)
        ready(gateway_url, gateway)
        timings = []
        matched = 0
        for index, (question, expected) in enumerate(tests, 1):
            request_id = f"phase4-live-{index}"
            fixture = {"requestId": request_id, "question": question, "candidates": candidates,
                       "allowCloud": False, "restricted": False}
            request = urllib.request.Request(gateway_url + "/route", json.dumps(fixture).encode(),
                                             {"Content-Type": "application/json"})
            start = time.perf_counter()
            with opener.open(request, timeout=18) as reply:
                response = json.load(reply)
            elapsed = 1000 * (time.perf_counter() - start)
            timings.append(elapsed)
            if response["requestId"] != request_id or response["provider"] != "llamacpp":
                raise RuntimeError("Provider response identity mismatch.")
            actual = response["candidateId"]
            matched += actual == expected
            lines.append(f"{'PASS' if actual == expected else 'MISMATCH'} fixture {index}: {actual or 'abstained'} "
                         f"(expected {expected}), {elapsed:.0f} ms")
        lines.append(f"Local semantic relevance: {matched}/{len(tests)}. Median end-to-end {statistics.median(timings):.0f} ms.")
        if matched != len(tests):
            raise RuntimeError("Some actual model choices differed from the expected intent.")
    except (OSError, ValueError, RuntimeError) as error:
        lines.append(f"FAIL {error}")
        raise
    finally:
        for process in reversed(processes):
            if process.poll() is None:
                process.terminate()
            process.wait(timeout=8)
        lines.append("Owned local test processes stopped.")
        args.out.write_text("\n".join(lines) + "\n", encoding="utf-8")
        print("\n".join(lines))
