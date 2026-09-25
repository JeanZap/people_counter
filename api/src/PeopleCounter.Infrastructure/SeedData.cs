using Microsoft.EntityFrameworkCore;
using PeopleCounter.Domain;

namespace PeopleCounter.Infrastructure;

public static class SeedData
{
    public static async Task EnsureSeededAsync(CounterDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);
        if (await db.Cameras.AnyAsync(cancellationToken)) return;

        var camera = new Camera(
            name: "Camera",
            connectionString: "rtsp://admin:12345678ab@192.168.0.18:554/onvif1",
            threshold: 1,
            durationSeconds: 1,
            resetDurationSeconds: 4,
            frameWidth: 1024,
            frameHeight: 640,
            areaJson: "[[327,289],[350,499],[578,496],[530,292]]");

        await db.Cameras.AddAsync(camera, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}
