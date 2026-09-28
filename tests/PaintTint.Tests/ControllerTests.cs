using Microsoft.AspNetCore.Mvc;
using PaintTint.Api.Controllers;
using PaintTint.Core.DTOs;
using PaintTint.Core.Interfaces;
using Xunit;

namespace PaintTint.Tests;

public class ControllerTests
{
    private class MockBaseService : IBaseService
    {
        public Task<List<BaseDto>> GetBasesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<BaseDto>
            {
                new() { Id = 1, Name = "Pastel", MaxTintPercent = 2m, PricePerLitre = 250m }
            });
        }
    }

    private class MockShadeService : IShadeService
    {
        public Task<List<ShadeSummaryDto>> GetShadesAsync(string? search = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<ShadeSummaryDto>
            {
                new() { Id = 1, Code = "OM-201", Name = "Ocean Mist", HexColor = "#7FA7B5" }
            });
        }

        public Task<ShadeDetailDto?> GetShadeByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id == 1)
            {
                return Task.FromResult<ShadeDetailDto?>(new ShadeDetailDto
                {
                    Id = 1, Code = "OM-201", Name = "Ocean Mist", HexColor = "#7FA7B5"
                });
            }
            return Task.FromResult<ShadeDetailDto?>(null);
        }
    }

    private class MockDispenseService : IDispenseService
    {
        public Task<CalculateTintResponse> CalculateAsync(CalculateTintRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CalculateTintResponse
            {
                ShadeId = request.ShadeId,
                BaseId = request.BaseId,
                CanSizeLitres = request.CanSizeLitres,
                IsValid = true,
                TotalPrice = 1000m
            });
        }

        public Task<DispenseJobResponse> CreateDispenseJobAsync(CreateDispenseJobRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DispenseJobResponse
            {
                Id = 101,
                ShadeId = request.ShadeId,
                BaseId = request.BaseId,
                CanSizeLitres = request.CanSizeLitres,
                TotalPrice = 1000m
            });
        }

        public Task<List<DispenseJobResponse>> GetRecentJobsAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<DispenseJobResponse>
            {
                new() { Id = 101, ShadeName = "Ocean Mist", TotalPrice = 1000m }
            });
        }

        public Task<DispenseJobResponse?> GetLatestJobAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<DispenseJobResponse?>(new DispenseJobResponse { Id = 101, TotalPrice = 1000m });
        }
    }

    [Fact]
    public async Task BasesController_GetBases_ReturnsOkWithList()
    {
        var controller = new BasesController(new MockBaseService());
        var result = await controller.GetBases(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<BaseDto>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task ShadesController_GetShades_ReturnsOkWithList()
    {
        var controller = new ShadesController(new MockShadeService());
        var result = await controller.GetShades(null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<ShadeSummaryDto>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task ShadesController_GetShadeById_ReturnsOkOrNotFound()
    {
        var controller = new ShadesController(new MockShadeService());
        var foundResult = await controller.GetShadeById(1, CancellationToken.None);
        Assert.IsType<OkObjectResult>(foundResult.Result);

        var notFoundResult = await controller.GetShadeById(999, CancellationToken.None);
        Assert.IsType<NotFoundObjectResult>(notFoundResult.Result);
    }

    [Fact]
    public async Task TintController_Calculate_ReturnsOk()
    {
        var controller = new TintController(new MockDispenseService());
        var result = await controller.Calculate(new CalculateTintRequest { ShadeId = 1, BaseId = 2, CanSizeLitres = 4m }, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var res = Assert.IsType<CalculateTintResponse>(okResult.Value);
        Assert.True(res.IsValid);
    }

    [Fact]
    public async Task DispenseJobsController_CreateAndGet_ReturnsOk()
    {
        var controller = new DispenseJobsController(new MockDispenseService());

        var createResult = await controller.CreateJob(new CreateDispenseJobRequest { ShadeId = 1, BaseId = 2, CanSizeLitres = 4m }, CancellationToken.None);
        var createdOk = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        var job = Assert.IsType<DispenseJobResponse>(createdOk.Value);
        Assert.Equal(101, job.Id);

        var recentResult = await controller.GetRecentJobs(10, CancellationToken.None);
        var recentOk = Assert.IsType<OkObjectResult>(recentResult.Result);
        var recentList = Assert.IsType<List<DispenseJobResponse>>(recentOk.Value);
        Assert.Single(recentList);

        var latestResult = await controller.GetLatestJob(CancellationToken.None);
        var latestOk = Assert.IsType<OkObjectResult>(latestResult.Result);
        var latest = Assert.IsType<DispenseJobResponse>(latestOk.Value);
        Assert.Equal(101, latest.Id);
    }
}
