namespace PeopleCounter.Domain;

public enum EventStatus { Recording, Completed, Failed }

public sealed class Camera
{
    private Camera() { }
    public Camera(string name, string connectionString, int threshold, double durationSeconds, double resetDurationSeconds, int frameWidth, int frameHeight, string areaJson)
    {
        Id = Guid.NewGuid(); Update(name, connectionString, threshold, durationSeconds, resetDurationSeconds, frameWidth, frameHeight, areaJson);
    }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = "";
    public string ConnectionString { get; private set; } = "";
    public int Threshold { get; private set; }
    public double DurationSeconds { get; private set; }
    public double ResetDurationSeconds { get; private set; }
    public int FrameWidth { get; private set; }
    public int FrameHeight { get; private set; }
    public string AreaJson { get; private set; } = "[]";
    public bool Enabled { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastSeenAt { get; private set; }
    public void Update(string name, string connectionString, int threshold, double durationSeconds, double resetDurationSeconds, int frameWidth, int frameHeight, string areaJson)
    { Name = name.Trim(); ConnectionString = connectionString; Threshold = threshold; DurationSeconds = durationSeconds; ResetDurationSeconds = resetDurationSeconds; FrameWidth = frameWidth; FrameHeight = frameHeight; AreaJson = areaJson; }
    public void SetLastSeen() => LastSeenAt = DateTimeOffset.UtcNow;
    public void SetEnabled(bool enabled) => Enabled = enabled;
}

public sealed class DetectionEvent
{
    private DetectionEvent() { }
    public DetectionEvent(Guid cameraId, DateTimeOffset startedAt, int maxPeopleCount, int threshold, string videoPath, string? thumbnailPath)
    { Id = Guid.NewGuid(); CameraId = cameraId; StartedAt = startedAt; MaxPeopleCount = maxPeopleCount; Threshold = threshold; VideoPath = videoPath; ThumbnailPath = thumbnailPath; Status = EventStatus.Recording; }
    public Guid Id { get; private set; }
    public Guid CameraId { get; private set; }
    public EventStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public int MaxPeopleCount { get; private set; }
    public int Threshold { get; private set; }
    public string VideoPath { get; private set; } = "";
    public string? ThumbnailPath { get; private set; }
    public string? ErrorMessage { get; private set; }
    public void UpdateCount(int count) { if (count > MaxPeopleCount) MaxPeopleCount = count; }
    public void Complete(DateTimeOffset endedAt) { Status = EventStatus.Completed; EndedAt = endedAt; }
    public void Fail(string error) { Status = EventStatus.Failed; ErrorMessage = error; EndedAt = DateTimeOffset.UtcNow; }
}
