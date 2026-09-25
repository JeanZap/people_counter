import time
import datetime


class CountThresholdTimer:
    def __init__(self, threshold: int, duration_seconds: float, reset_duration_seconds: float | None = None, watch_start: time = None, watch_end: time = None):
        self.watch_start = datetime.time(watch_start)
        self.watch_end = datetime.time(watch_end)
        self.threshold = threshold
        self.duration_seconds = duration_seconds
        self.reset_duration_seconds = (
            duration_seconds if reset_duration_seconds is None else reset_duration_seconds
        )
        self._started_at = None
        self._below_started_at = None
        self._exceeded = False

    def check(self, count: int) -> bool:
        if count < self.threshold:
            self._started_at = None

            if not self._exceeded:
                return False

            if self._below_started_at is None:
                self._below_started_at = time.monotonic()
                return False

            elapsed_below = time.monotonic() - self._below_started_at

            if elapsed_below >= self.reset_duration_seconds:
                self._reset()

            return False

        self._below_started_at = None

        if self._exceeded:
            return False

        if self._started_at is None:
            self._started_at = time.monotonic()
            return False

        elapsed = time.monotonic() - self._started_at

        if elapsed >= self.duration_seconds and self.time_in_watch():
            self._exceeded = True
            return True

        return False

    def time_in_watch(self):
        return self.time_between(datetime.datetime.now().time(), self.watch_start, self.watch_end)

    def time_between(self, now, start, end):
        if start <= end:
            return start <= now < end

        return start <= now or now < end

    def _reset(self):
        self._started_at = None
        self._below_started_at = None
        self._exceeded = False
