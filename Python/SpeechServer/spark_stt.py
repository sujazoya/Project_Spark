"""Local, opt-in, utterance-based speech recognition. No raw audio saved."""
import argparse, io, json, wave, threading
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

MAX_BYTES = 4_000_000

def decode(model, recognizer_type, body):
    with wave.open(io.BytesIO(body), 'rb') as wav:
        rate = wav.getframerate()
        if wav.getnchannels() != 1 or wav.getsampwidth() != 2 or not 8000 <= rate <= 48000:
            raise ValueError('Expected mono PCM16 WAV, 8–48 kHz')
        if wav.getnframes() > rate * 21:
            raise ValueError('Audio exceeds 21 seconds')
        recognizer = recognizer_type(model, rate)
        recognizer.SetWords(True)
        parts, confidence = [], []
        def collect(value):
            result = json.loads(value)
            parts.append(result.get('text', ''))
            confidence.extend(word['conf'] for word in result.get('result', []) if 'conf' in word)

        while True:
            data = wav.readframes(4000)
            if not data: break
            if recognizer.AcceptWaveform(data):
                collect(recognizer.Result())
        collect(recognizer.FinalResult())
    text = ' '.join(p.strip() for p in parts if p.strip())
    return {'text': text, 'confidence_available': bool(confidence),
            'confidence': sum(confidence) / len(confidence) if confidence else 0.0}

def serve(args):
    from vosk import Model, KaldiRecognizer, SetLogLevel
    SetLogLevel(-1)
    if not Path(args.model).is_dir(): raise SystemExit('Model folder not found. Extract a Vosk model first.')
    model = Model(args.model)
    gate = threading.BoundedSemaphore(1)
    class Handler(BaseHTTPRequestHandler):
        def setup(self):
            super().setup(); self.connection.settimeout(20)
        def log_message(self, fmt, *values): pass
        def respond(self, code, payload):
            data = json.dumps(payload).encode(); self.send_response(code)
            self.send_header('Content-Type', 'application/json'); self.send_header('Content-Length', str(len(data))); self.end_headers(); self.wfile.write(data)
        def do_GET(self):
            if self.path == '/health': self.respond(200, {'ready': True})
            else: self.respond(404, {'error': 'Not found'})
        def do_POST(self):
            if self.path != '/transcribe': return self.respond(404, {'error':'Not found'})
            if self.headers.get_content_type() != 'audio/wav': return self.respond(415, {'error':'Expected audio/wav'})
            try: size = int(self.headers.get('Content-Length', '0'))
            except ValueError: size = 0
            if not 44 < size <= MAX_BYTES: return self.respond(413, {'error':'Invalid audio size'})
            if not gate.acquire(blocking=False): return self.respond(429, {'error':'Recognizer busy'})
            try:
                body = self.rfile.read(size)
                if len(body) != size: raise ValueError('Incomplete audio')
                self.respond(200, decode(model, KaldiRecognizer, body))
            except (ValueError, wave.Error, EOFError) as exc: self.respond(400, {'error':str(exc)})
            except Exception: self.respond(500, {'error':'Speech decoding failed'})
            finally: gate.release()
    if args.host not in ('127.0.0.1', 'localhost') and not args.allow_lan:
        raise SystemExit('Non-loopback binding requires --allow-lan; development trusted LAN only.')
    server=ThreadingHTTPServer((args.host,args.port), Handler)
    print(f'Spark STT ready at http://{args.host}:{args.port}/transcribe (Ctrl+C to stop)')
    try: server.serve_forever()
    except KeyboardInterrupt: pass
    finally: server.server_close()

if __name__ == '__main__':
    p=argparse.ArgumentParser(); p.add_argument('--model',required=True); p.add_argument('--host',default='127.0.0.1'); p.add_argument('--port',type=int,default=8766); p.add_argument('--allow-lan',action='store_true'); serve(p.parse_args())
