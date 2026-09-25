using Microsoft.EntityFrameworkCore;
using PeopleCounter.Application;
using PeopleCounter.Domain;

namespace PeopleCounter.Infrastructure;

public sealed class CounterDbContext(DbContextOptions<CounterDbContext> options) : DbContext(options)
{
    public DbSet<Camera> Cameras => Set<Camera>();
    public DbSet<DetectionEvent> Events => Set<DetectionEvent>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Camera>().HasKey(x => x.Id); b.Entity<Camera>().Property(x => x.AreaJson).HasColumnType("jsonb");
        b.Entity<DetectionEvent>().HasKey(x => x.Id); b.Entity<DetectionEvent>().Property(x => x.Status).HasConversion<string>();
        b.Entity<DetectionEvent>().HasOne<Camera>().WithMany().HasForeignKey(x => x.CameraId);
        b.Entity<DetectionEvent>().HasIndex(x => new { x.CameraId, x.StartedAt });
    }
}

public sealed class ApplicationStore(CounterDbContext db) : IApplicationStore
{
    public async Task<IReadOnlyList<Camera>> Cameras(CancellationToken ct) => await db.Cameras.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
    public Task<Camera?> Camera(Guid id, CancellationToken ct) => db.Cameras.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddCamera(Camera camera, CancellationToken ct) => await db.Cameras.AddAsync(camera, ct);
    public Task SaveCamera(Camera camera, CancellationToken ct) { db.Cameras.Update(camera); return Task.CompletedTask; }
    public async Task<IReadOnlyList<(DetectionEvent Event, Camera Camera)>> Events(Guid? cameraId, DateTimeOffset? from, DateTimeOffset? to, int page, int pageSize, CancellationToken ct)
    {
        var query = from e in db.Events.AsNoTracking() join c in db.Cameras.AsNoTracking() on e.CameraId equals c.Id select new { e, c };
        if (cameraId is not null) query = query.Where(x => x.e.CameraId == cameraId);
        if (from is not null) query = query.Where(x => x.e.StartedAt >= from);
        if (to is not null) query = query.Where(x => x.e.StartedAt <= to);
        var rows = await query.OrderByDescending(x => x.e.StartedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return rows.Select(x => (x.e, x.c)).ToList();
    }
    public Task<DetectionEvent?> Event(Guid id, CancellationToken ct) => db.Events.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<Camera?> EventCamera(Guid eventId, CancellationToken ct) => await (from e in db.Events join c in db.Cameras on e.CameraId equals c.Id where e.Id == eventId select c).FirstOrDefaultAsync(ct);
    public async Task AddEvent(DetectionEvent detectionEvent, CancellationToken ct) => await db.Events.AddAsync(detectionEvent, ct);
    public Task Save(CancellationToken ct) => db.SaveChangesAsync(ct);
}
