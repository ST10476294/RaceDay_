using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
namespace RaceDay.Api.Data;

// Optional demonstration data, switched on with the setting SeedDemoData=true.
// The API refuses to create past events, so this is how a finished race exists for the results pages.
public static class DemoDataSeeder
{
    public const string DemoPassword = "RaceDay!2026";

    public static async Task SeedAsync(RaceDayDbContext db)
    {
        await db.Database.MigrateAsync();
        if (await db.Users.AnyAsync()) return; // Never touch a database that already has data.

        var hash = BCrypt.Net.BCrypt.HashPassword(DemoPassword);
        var thabo = new User { FullName = "Thabo Nkosi", Email = "thabo.nkosi@raceday.co.za", PasswordHash = hash, Role = "Organiser" };
        var lerato = new User { FullName = "Lerato Dube", Email = "lerato.dube@raceday.co.za", PasswordHash = hash, Role = "Organiser" };
        var sipho = new User { FullName = "Sipho Mokoena", Email = "sipho.mokoena@example.com", PasswordHash = hash, Role = "Participant" };
        var amahle = new User { FullName = "Amahle Zulu", Email = "amahle.zulu@example.com", PasswordHash = hash, Role = "Participant" };
        db.Users.AddRange(thabo, lerato, sipho, amahle);

        var run10 = new Category { Name = "10km", DistanceKm = 10m, MaxParticipants = 500, EntryFee = 150m };
        var run21 = new Category { Name = "21km", DistanceKm = 21.1m, MaxParticipants = 300, EntryFee = 250m };
        var cycle = new Category { Name = "40km", DistanceKm = 40m, MaxParticipants = 400, EntryFee = 300m };
        var walk = new Category { Name = "5km", DistanceKm = 5m, MaxParticipants = 1000, EntryFee = 80m };
        var past = new Category { Name = "15km", DistanceKm = 15m, MaxParticipants = 200, EntryFee = 120m };

        db.Events.AddRange(
            new Event { Organiser = thabo, Name = "Johannesburg City Run", Discipline = "Running", EventDate = DateTime.Now.AddDays(30).Date.AddHours(7),
                Location = "Johannesburg, Gauteng", DistanceKm = 21.1m, Description = "A scenic road run through the city centre.",
                Categories = { run10, run21 }, Routes = { new Models.Route { Description = "Loop route starting and ending at Mary Fitzgerald Square.", ElevationGainM = 120 } } },
            new Event { Organiser = thabo, Name = "Soweto Cycle Challenge", Discipline = "Cycling", EventDate = DateTime.Now.AddDays(45).Date.AddHours(6),
                Location = "Soweto, Gauteng", DistanceKm = 40m, Description = "A community cycling event through historic Soweto.", Categories = { cycle } },
            new Event { Organiser = lerato, Name = "Durban Beachfront Walk", Discipline = "Walking", EventDate = DateTime.Now.AddDays(60).Date.AddHours(8),
                Location = "Durban, KwaZulu-Natal", DistanceKm = 5m, Description = "A relaxed charity walk along the Durban beachfront.", Categories = { walk } },
            new Event { Organiser = thabo, Name = "Pretoria Spring Classic", Discipline = "Running", EventDate = DateTime.Now.AddDays(-7).Date.AddHours(7),
                Location = "Pretoria, Gauteng", DistanceKm = 15m, Description = "A completed race, ready for results to be captured.", Categories = { past } });

        var finished = new Enrolment { Participant = sipho, Category = past };
        db.Enrolments.AddRange(finished,
            new Enrolment { Participant = amahle, Category = past },
            new Enrolment { Participant = sipho, Category = run10 });
        db.Results.Add(new Result { Enrolment = finished, FinishTime = new TimeSpan(1, 12, 45), OverallPosition = 1, CategoryPosition = 1, Status = "Finished" });

        await db.SaveChangesAsync();
    }
}
