using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PeopleCounter.Application;
using PeopleCounter.Domain;

namespace PeopleCounter.WebApi.Controllers;

[ApiController, ApiVersion("1.0"), Route("api/v{version:apiVersion}/events")]
public sealed class EventsController(IApplicationStore store, IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventResponse>>> List(Guid? cameraId, DateTimeOffset? from, DateTimeOffset? to, int page = 1, int pageSize = 30, CancellationToken ct = default) { var rows = await store.Events(cameraId, from, to, Math.Max(page, 1), Math.Clamp(pageSize, 1, 100), ct); return Ok(rows.Select(x => Map(x.Event, x.Camera.Name))); }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventResponse>> Get(Guid id, CancellationToken ct) { var item = await store.Event(id, ct); var camera = item is null ? null : await store.EventCamera(id, ct); return item is null || camera is null ? NotFound() : Ok(Map(item, camera.Name)); }
    [HttpPost]
    public async Task<ActionResult<EventResponse>> Create(EventRequest request, CancellationToken ct) { var camera = await store.Camera(request.CameraId, ct); if (camera is null) return NotFound("Camera not found"); var item = new DetectionEvent(request.CameraId, request.StartedAt, request.MaxPeopleCount, request.Threshold, request.VideoPath, request.ThumbnailPath); item.UpdateCount(request.MaxPeopleCount); if (request.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase)) item.Fail(request.ErrorMessage ?? "Worker failure"); else if (request.EndedAt is not null) item.Complete(request.EndedAt.Value); await store.AddEvent(item, ct); camera.SetLastSeen(); await store.Save(ct); return CreatedAtAction(nameof(Get), new { id = item.Id, version = "1.0" }, Map(item, camera.Name)); }
    [HttpGet("{id:guid}/video")]
    public async Task<IActionResult> Video(Guid id, CancellationToken ct) { var item = await store.Event(id, ct); if (item is null) return NotFound(); var root = Path.GetFullPath(configuration["Storage:Root"] ?? "/data/events"); var path = Path.GetFullPath(Path.Combine(root, item.VideoPath)); if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(path)) return NotFound(); return PhysicalFile(path, "video/mp4", enableRangeProcessing: true); }
    private static EventResponse Map(DetectionEvent e, string name) => new(e.Id, e.CameraId, name, e.Status.ToString(), e.StartedAt, e.EndedAt, e.MaxPeopleCount, e.Threshold, $"/api/v1/events/{e.Id}/video", e.ThumbnailPath is null ? null : $"/api/v1/events/{e.Id}/thumbnail", e.ErrorMessage);
}
