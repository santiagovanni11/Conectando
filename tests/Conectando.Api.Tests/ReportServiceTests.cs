using Conectando.Api.Data;
using Conectando.Api.Exceptions;
using Conectando.Api.Models;
using Conectando.Api.Services;
using Conectando.Api.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Tests;

[Collection("Social")]
public class ReportServiceTests(SocialTestFixture fixture)
{
    private ReportService CreateService(ConectandoDbContext db) => new(db);

    [Fact]
    public async Task Create_WithValidReason_StoresTheReport()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);

        var result = await service.CreateAsync(
            users[0].Id, users[1].Id, ReportReasons.Harassment, "me escribe Threats");

        Assert.Equal(users[1].Id, result.TargetUserId);
        Assert.Equal(ReportReasons.Harassment, result.Reason);
        Assert.Equal("me escribe Threats", result.Details);
    }

    [Fact]
    public async Task Create_Self_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await Assert.ThrowsAsync<SelfActionException>(
            () => service.CreateAsync(users[0].Id, users[0].Id, ReportReasons.Spam, null));
    }

    [Theory]
    [InlineData("inventado")]
    [InlineData("")]
    [InlineData("SPAM_MAL")]
    public async Task Create_WithUnknownReason_Throws(string reason)
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await Assert.ThrowsAsync<InvalidReportReasonException>(
            () => service.CreateAsync(users[0].Id, users[1].Id, reason, null));
    }

    [Fact]
    public async Task Create_Twice_UpdatesInsteadOfDuplicating()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await service.CreateAsync(users[0].Id, users[1].Id, ReportReasons.Spam, "primera");
        await service.CreateAsync(users[0].Id, users[1].Id, ReportReasons.Violence, "segunda");

        // El índice único lo impediría: la segundaactualiza la existente.
        var reports = await db.Reports.Where(r => r.ReporterId == users[0].Id).ToListAsync();
        var report = Assert.Single(reports);
        Assert.Equal(ReportReasons.Violence, report.Reason);
        Assert.Equal("segunda", report.Details);
    }

    [Fact]
    public async Task Create_WithDetailsTooLong_Throws()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);

        await Assert.ThrowsAsync<ReportDetailsTooLongException>(
            () => service.CreateAsync(users[0].Id, users[1].Id, ReportReasons.Other, new string('x', 1001)));
    }

    [Fact]
    public async Task GetOwn_OnlyReturnsReportsMadeByTheUser()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 3);

        await service.CreateAsync(users[0].Id, users[1].Id, ReportReasons.Spam, null);
        await service.CreateAsync(users[2].Id, users[1].Id, ReportReasons.Spam, null);

        var own = await service.GetOwnAsync(users[0].Id);

        // Nadie tiene que ver lo que Passage Kartun DENUNCIÓ, ni lo de uno mismo.
        var report = Assert.Single(own);
        Assert.Equal(users[1].Id, report.TargetUserId);
    }

    [Fact]
    public async Task Create_NormalizesReasonToLowercase()
    {
        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var users = await TestUsers.SeedAsync(db, 2);

        var result = await service.CreateAsync(users[0].Id, users[1].Id, "  HARASSMENT  ", null);

        Assert.Equal(ReportReasons.Harassment, result.Reason);
    }
}