import cv2
from ultralytics import YOLO
import numpy as np
import json
from domain.CountThresholdTimer import CountThresholdTimer
from domain.FrameProcessor import FrameProcessor
from datetime import datetime

frame_counter = int(0)


def draw_frame(area, frame, count):
    cv2.putText(frame, f'In: {count}', (20, 50),
                cv2.FONT_HERSHEY_COMPLEX, 1, (0, 0, 255), 2)
    cv2.polylines(frame, [np.array(area, np.int32)], True, (0, 255, 0), 2)
    cv2.imshow("people_counter", frame)


def people_counter(event, x, y, flags, param):
    if event == cv2.EVENT_MOUSEMOVE:
        print([x, y])


def load_class_list(file_path):
    with open(file_path, "r") as file:
        return file.read().split("\n")


def load_config(file_path):
    with open(file_path, "r") as file:
        return json.load(file)


def main():
    config = load_config("config.json")
    model = YOLO(config["model_path"])

    frame_height = config["frame_height"]
    frame_width = config["frame_width"]
    fourcc = cv2.VideoWriter_fourcc(*"XVID")

    cv2.namedWindow('people_counter')
    cv2.setMouseCallback('people_counter', people_counter)

    cap = cv2.VideoCapture(
        "rtsp://admin:12345678ab@192.168.0.18:554/onvif1",
        cv2.CAP_FFMPEG
    )
    # cap = cv2.VideoCapture('video/c.mp4')

    out: cv2. VideoWriter = None

    count_timer = CountThresholdTimer(
        duration_seconds=config["duration_seconds"],
        threshold=config["threshold"],
        watch_start=config["watch_start"],
        watch_end=config["watch_end"],
    )

    if not cap.isOpened():
        print("Error: Could not open video file.")
        return

    class_list = load_class_list("coco.txt")

    area = [tuple(point) for point in config["area"]]
    people_in = {}
    counter = []

    fps = cap.get(cv2.CAP_PROP_FPS)
    delay = int(1000 / fps) if fps and fps > 0 else 1

    frame_processor = FrameProcessor(
        model=model,
        area=area,
        class_list=class_list,
        people_in=people_in,
        counter=counter,
        frame_height=config["frame_height"],
        frame_width=config["frame_width"]
    )
    date = None

    while True:
        ret, frame = cap.read()

        if not ret:
            print("Reached the end of the video or encountered an error.")
            break

        start = datetime.now()

        frame, count = frame_processor.process_frame(frame)

        if count_timer.check(count) or True:
            if (out is None):
                date = datetime.now()
                out = cv2.VideoWriter(
                    f"{date.strftime('%m-%d-%Y')}.avi", fourcc, fps, (frame_width, frame_height))
            out.write(frame)
        else:
            date=None
            out = None

        draw_frame(area, frame, count)

        elapse_time = (datetime.now() - start)
        elapsed_milliseconds = int(elapse_time.total_seconds()*1000)
        wait_time = delay - elapsed_milliseconds

        if cv2.waitKey(wait_time if wait_time > 0 else 1) & 0xFF == ord('q'):
            break

    cap.release()
    cv2.destroyAllWindows()


if __name__ == "__main__":
    main()
