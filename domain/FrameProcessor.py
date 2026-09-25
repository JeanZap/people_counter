import cv2
from ultralytics import YOLO
import cvzone
import numpy as np
import json


class FrameProcessor:
    def __init__(self, model: YOLO, area: list[tuple[int, int]], class_list: list[str], people_in: dict, counter: list, frame_width: int = None, frame_height: int = None):
        self.frame_counter = int(0)
        self.model = model
        self.area = area
        self.class_list = class_list
        self.people_in = people_in
        self.counter = counter
        self.frame_height = frame_height
        self.frame_width = frame_width

    def draw_frame(self, area, frame, count):
        cv2.putText(frame, f'In: {count}', (20, 50),
                    cv2.FONT_HERSHEY_COMPLEX, 1, (0, 0, 255), 2)
        cv2.polylines(frame, [np.array(area, np.int32)], True, (0, 255, 0), 2)
        cv2.imshow("people_counter", frame)

    def people_counter(self, event, x, y):
        if event == cv2.EVENT_MOUSEMOVE:
            print([x, y])

    def load_class_list(self, file_path):
        with open(file_path, "r") as file:
            return file.read().split("\n")

    def load_config(self, file_path):
        with open(file_path, "r") as file:
            return json.load(file)

    def add_object_area(self, frame, people_in, counter, x3, y3, x4, y4, obj_id):
        cv2.rectangle(frame, (x3, y3), (x4, y4), (255, 255, 255), 2)
        cvzone.putTextRect(frame, f'{obj_id}', (x3, y3), 1, 1)
        people_in[obj_id] = (x4, y4)

        if obj_id not in counter:
            counter.append(obj_id)

    def remove_tracked_object_area(self, people_in, counter, obj_id):
        del people_in[obj_id]
        counter.remove(obj_id)

    def track_objects(self, results, objects_bbs_ids):
        for result in results:
            if result.boxes.id is not None:
                ids = result.boxes.id.int().cpu().tolist()
                boxes = result.boxes.xyxy.cpu().tolist()

                for track_id, box in zip(ids, boxes):
                    objects_bbs_ids.append([
                        *map(int, box),
                        track_id
                    ])

    def increase_frame_count(self):
        self.frame_counter += 1

    def stop_tracking_objects_out_of_frame(self, people_in, counter, objects_bbs_ids):
        if self.frame_counter % 60 == 0:
            disapeared_object_ids = [key for key in people_in.keys() if key not in [
                obj[4] for obj in objects_bbs_ids]]

            for tracked_object_id in disapeared_object_ids:
                del people_in[tracked_object_id]
                counter.remove(tracked_object_id)

    def process_frame(self, frame):
        frame = self.track_and_display(frame)

        return frame, len(self.counter)

    def track_and_display(self, frame):
        frame = cv2.resize(frame, (self.frame_width, self.frame_height))
        # frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        results = self.model.track(frame, persist=True,
                                   tracker="bytetrack.yaml", classes=[0], verbose=False)

        objects_bbs_ids = []

        self.track_objects(results, objects_bbs_ids)

        self.increase_frame_count()

        self.stop_tracking_objects_out_of_frame(
            self.people_in, self.counter, objects_bbs_ids)

        for bbox in objects_bbs_ids:
            x3, y3, x4, y4, object_id = bbox

            tracked_object_in_area = cv2.pointPolygonTest(
                np.array(self.area, np.int32), (x4, y4), False) >= 0

            if tracked_object_in_area is True:
                self.add_object_area(frame, self.people_in, self.counter,
                                     x3, y3, x4, y4, object_id)

            elif object_id in self.counter:
                self.remove_tracked_object_area(
                    self.people_in, self.counter, object_id)

            cv2.circle(frame, (x4, y4), 4, (0, 255, 0), -1)
        return frame
