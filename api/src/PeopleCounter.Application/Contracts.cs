using PeopleCounter.Domain;

namespace PeopleCounter.Application;

public record CameraRequest(string Name, string ConnectionString, int Threshold = 1, double DurationSeconds = 1, double ResetDurationSeconds = 4, int FrameWidth = 1024, int FrameHeight = 640, string AreaJson = "[]");
public record CameraResponse(Guid Id, string Name, int Threshold, double DurationSeconds, double ResetDurationSeconds, int FrameWidth, int FrameHeight, string AreaJson, bool Enabled, DateTimeOffset? LastSeenAt);
public record EventRequest(Guid CameraId, DateTimeOffset StartedAt, DateTimeOffset? EndedAt, int MaxPeopleCount, int Threshold, string VideoPath, string? ThumbnailPath, string Status = "Completed", string? ErrorMessage = null);
public record EventResponse(Guid Id, Guid CameraId, string CameraName, string Status, DateTimeOffset StartedAt, DateTimeOffset? EndedAt, int MaxPeopleCount, int Threshold, string VideoUrl, string? ThumbnailUrl, string? ErrorMessage);
public interface IApplicationStore
{
    Task<IReadOnlyList<Camera>> Cameras(CancellationToken ct);
    Task<Camera?> Camera(Guid id, CancellationToken ct);
    Task AddCamera(Camera camera, CancellationToken ct);
    Task SaveCamera(Camera camera, CancellationToken ct);
    Task<IReadOnlyList<(DetectionEvent Event, Camera Camera)>> Events(Guid? cameraId, DateTimeOffset? from, DateTimeOffset? to, int page, int pageSize, CancellationToken ct);
    Task<DetectionEvent?> Event(Guid id, CancellationToken ct);
    Task<Camera?> EventCamera(Guid eventId, CancellationToken ct);
    Task AddEvent(DetectionEvent detectionEvent, CancellationToken ct);
    Task Save(CancellationToken ct);
}
