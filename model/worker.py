import json, os, time, uuid
from datetime import datetime, timezone
import cv2
import requests
from ultralytics import YOLO

API = os.getenv("API_URL", "http://api:8080")
API_V1 = f"{API}/api/v1"
CONFIG = os.getenv("CAMERA_CONFIG", "config.json")

def wait_for_camera():
    while True:
        try:
            response = requests.get(f"{API_V1}/cameras", timeout=10)
            response.raise_for_status()
            cameras = response.json()
            if cameras:
                return os.getenv("CAMERA_ID") or cameras[0]["id"]
        except requests.RequestException as error:
            print(f"Aguardando API: {error}", flush=True)
        print("Aguardando câmera seed...", flush=True)
        time.sleep(5)

def main():
    with open(CONFIG, encoding="utf-8") as f: cfg = json.load(f)
    camera_id = wait_for_camera()
    model = YOLO(os.getenv("MODEL_PATH", cfg.get("model_path", "yolo11n.pt")))
    cap = cv2.VideoCapture(os.getenv("RTSP_URL") or cfg["connectionString"], cv2.CAP_FFMPEG)
    if not cap.isOpened(): raise RuntimeError("Não foi possível abrir a câmera RTSP")
    os.makedirs(os.getenv("EVENT_ROOT", "/data/events"), exist_ok=True)
    started = None; writer = None; max_count = 0; last_above = time.monotonic()
    while True:
        ok, frame = cap.read()
        if not ok: time.sleep(2); continue
        frame = cv2.resize(frame, (cfg.get("frame_width", 1024), cfg.get("frame_height", 640)))
        result = model.track(frame, persist=True, tracker="bytetrack.yaml", classes=[0], verbose=False)[0]
        count = len(result.boxes) if result.boxes is not None else 0
        now = datetime.now(timezone.utc)
        if count >= cfg.get("threshold", 1):
            last_above = time.monotonic()
            if started is None: started = now; max_count = 0
            max_count = max(max_count, count)
            if writer is None:
                name = f"{uuid.uuid4()}.mp4"; writer = cv2.VideoWriter(os.path.join(os.getenv("EVENT_ROOT", "/data/events"), name), cv2.VideoWriter_fourcc(*"mp4v"), 15, (frame.shape[1], frame.shape[0])); current_name = name
            writer.write(frame)
        elif writer is not None:
            writer.write(frame)
            if time.monotonic() - last_above >= cfg.get("reset_duration_seconds", 4):
                writer.release(); writer = None
                requests.post(f"{API_V1}/events", json={"cameraId": camera_id, "startedAt": started.isoformat(), "endedAt": now.isoformat(), "maxPeopleCount": max_count, "threshold": cfg.get("threshold", 1), "videoPath": current_name}, timeout=10)
                started = None
    cap.release()

if __name__ == "__main__": main()
