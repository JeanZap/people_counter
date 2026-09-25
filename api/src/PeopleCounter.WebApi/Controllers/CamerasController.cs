using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PeopleCounter.Application;
using PeopleCounter.Domain;

namespace PeopleCounter.WebApi.Controllers;

[ApiController, ApiVersion("1.0"), Route("api/v{version:apiVersion}/cameras")]
public sealed class CamerasController(IApplicationStore store) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CameraResponse>>> List(CancellationToken ct) => Ok((await store.Cameras(ct)).Select(Map));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CameraResponse>> Get(Guid id, CancellationToken ct) { var camera = await store.Camera(id, ct); return camera is null ? NotFound() : Ok(Map(camera)); }
    [HttpPost]
    public async Task<ActionResult<CameraResponse>> Create(CameraRequest request, CancellationToken ct) { var camera = new Camera(request.Name, request.ConnectionString, request.Threshold, request.DurationSeconds, request.ResetDurationSeconds, request.FrameWidth, request.FrameHeight, request.AreaJson); await store.AddCamera(camera, ct); await store.Save(ct); return CreatedAtAction(nameof(Get), new { id = camera.Id, version = "1.0" }, Map(camera)); }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CameraResponse>> Update(Guid id, CameraRequest request, CancellationToken ct) { var camera = await store.Camera(id, ct); if (camera is null) return NotFound(); camera.Update(request.Name, request.ConnectionString, request.Threshold, request.DurationSeconds, request.ResetDurationSeconds, request.FrameWidth, request.FrameHeight, request.AreaJson); await store.Save(ct); return Ok(Map(camera)); }
    private static CameraResponse Map(Camera c) => new(c.Id, c.Name, c.Threshold, c.DurationSeconds, c.ResetDurationSeconds, c.FrameWidth, c.FrameHeight, c.AreaJson, c.Enabled, c.LastSeenAt);
}
