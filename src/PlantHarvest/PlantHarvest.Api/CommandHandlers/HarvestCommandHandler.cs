using Azure.Core;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Driver;
using PlantHarvest.Api.Extensions;
using System.Collections.ObjectModel;

namespace PlantHarvest.Api.CommandHandlers;


public interface IHarvestCommandHandler
{
    Task<string> CreateHarvestCycle(CreateHarvestCycleCommand request);
    Task<string> DeleteHarvestCycle(string id);
    Task<string> UpdateHarvestCycle(UpdateHarvestCycleCommand request);

    Task<string> AddPlantHarvestCycle(CreatePlantHarvestCycleCommand request);
    Task<string> DeletePlantHarvestCycle(string harvestCyleId, string id);
    Task<string> UpdatePlantHarvestCycle(UpdatePlantHarvestCycleCommand request);

    Task<string> AddPlantSchedule(CreatePlantScheduleCommand command);
    Task<string> UpdatePlantSchedule(UpdatePlantScheduleCommand command);
    Task<string> DeletePlantSchedule(string harvestCycleId, string plantHarvestCycleId, string plantScheduleId);

    Task<string> AddGardenBedPlantHarvestCycle(CreateGardenBedPlantHarvestCycleCommand command);
    Task<string> UpdateGardenBedPlantHarvestCycle(UpdateGardenBedPlantHarvestCycleCommand command);
    Task<string> DeleteGardenBedPlantHarvestCycle(string harvestCycleId, string plantHarvestCycleId, string gardenBedPlantHarvestCycleId);
}

public class HarvestCommandHandler : IHarvestCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHarvestCycleRepository _harvestCycleRepository;
    private readonly IPlantHarvestCycleRepository _plantHarvestCycleRepository;
    private readonly IGardenBedPlantHarvestCycleRepository _gardenBedPlantHarvestCycleRepository;
    private readonly ILogger<HarvestCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IScheduleBuilder _scheduleBuilder;
    private readonly IMediator _mediator;

    public HarvestCommandHandler(IUnitOfWork unitOfWork, IHarvestCycleRepository harvestCycleRepository, IPlantHarvestCycleRepository plantHarvestCycleRepository, IGardenBedPlantHarvestCycleRepository gardenBedPlantHarvestCycleRepository, ILogger<HarvestCommandHandler> logger, IHttpContextAccessor httpContextAccessor, IScheduleBuilder scheduleBuilder, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _harvestCycleRepository = harvestCycleRepository;
        _plantHarvestCycleRepository = plantHarvestCycleRepository;
        _gardenBedPlantHarvestCycleRepository = gardenBedPlantHarvestCycleRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _scheduleBuilder = scheduleBuilder;
        _mediator = mediator;
    }

    #region Harvest Cycle

    public async Task<string> CreateHarvestCycle(CreateHarvestCycleCommand request)
    {
        _logger.LogInformation("Received request to create a new harvest cycle {@plant}", request);

        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

        var existingHarvestId = await _harvestCycleRepository.GetIdByNameAsync(request.HarvestCycleName, userProfileId);

        if (!string.IsNullOrEmpty(existingHarvestId))
        {
            throw new ArgumentException("Garden Plan with this name already exists", nameof(request.HarvestCycleName));
        }

        _unitOfWork.Initialize(this.GetType().Name);

        var harvest = HarvestCycle.Create(
           userProfileId,
           request.HarvestCycleName,
           request.StartDate,
           request.EndDate,
           request.Notes,
           request.GardenId);

        _harvestCycleRepository.Add(harvest);

        await _mediator.DispatchDomainEventsAsync(harvest);

        await _unitOfWork.SaveChangesAsync(this.GetType().Name);

        return harvest.Id;
    }

    public async Task<string> UpdateHarvestCycle(UpdateHarvestCycleCommand request)
    {
        _logger.LogInformation("Received request to update harvest cycle {@harvest}", request);

        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

        var existingHarvestId = await _harvestCycleRepository.GetIdByNameAsync(request.HarvestCycleName, userProfileId);
        if (!string.IsNullOrEmpty(existingHarvestId) && existingHarvestId != request.HarvestCycleId)
        {
            throw new ArgumentException("Another garden plan with this name already exists", nameof(request.HarvestCycleName));
        }

        _unitOfWork.Initialize(this.GetType().Name);

        //no need to load plants or beds
        var harvest = await _harvestCycleRepository.GetByIdAsync(request.HarvestCycleId);

        harvest.Update(request.HarvestCycleName, request.StartDate, request.EndDate, request.Notes, request.GardenId);

        _harvestCycleRepository.Update(harvest);

        await _mediator.DispatchDomainEventsAsync(harvest);

        await _unitOfWork.SaveChangesAsync(this.GetType().Name);

        return harvest.Id;
    }

    public async Task<string> DeleteHarvestCycle(string id)
    {
        _logger.LogInformation("Received request to delete harvest cycle {@id}", id);
        var harvest = await _harvestCycleRepository.GetByIdAsync(id);

        _unitOfWork.Initialize(this.GetType().Name);

        harvest.Delete();

        _harvestCycleRepository.Delete(id);
        _plantHarvestCycleRepository.DeletePlantHarvestCycle(id);
        _gardenBedPlantHarvestCycleRepository.DeleteGardenBedPlantHarvestCycle(id);

        await _mediator.DispatchDomainEventsAsync(harvest);

        await _unitOfWork.SaveChangesAsync(this.GetType().Name);

        return id;
    }
    #endregion

    #region Harvest Cycle Plant

    public async Task<String> AddPlantHarvestCycle(CreatePlantHarvestCycleCommand command)
    {
        _logger.LogInformation("Received request to create plant harvest cycle {@plantHarvestCycle}", command);
        try
        {
            string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

            var harvest = await _harvestCycleRepository.ReadHarvestCycle(command.HarvestCycleId, userProfileId);

            if (harvest.Plants.Any(g => g.PlantId == command.PlantId && (g.PlantVarietyId == command.PlantVarietyId) && (g.PlantGrowthInstructionId == command.PlantGrowthInstructionId)))
            {
                throw new ArgumentException("This plant is already a part of this plan", nameof(command.PlantVarietyId));
            }

            _unitOfWork.Initialize(this.GetType().Name);

            var plantHarvestId = harvest.AddPlantHarvestCycle(command);

            try
            {
                var generatedSchedules = await _scheduleBuilder.GeneratePlantCalendarBasedOnGrowInstruction(harvest.Plants.First(p => p.Id == plantHarvestId), command.PlantId, command.PlantGrowthInstructionId, command.PlantVarietyId, harvest.GardenId);

                ApplyPlantSchedules(generatedSchedules, harvest, plantHarvestId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception generating plant schedules. PLantHarvest will still save: {ex}", ex);
            }


            _plantHarvestCycleRepository.AddPlantHarvestCycle(plantHarvestId, harvest);

            await _mediator.DispatchDomainEventsAsync(harvest);

            await _unitOfWork.SaveChangesAsync(this.GetType().Name);

            return plantHarvestId;
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Exception adding plant harvest cycle: {ex}", ex);
            throw;
        }

    }

    public async Task<String> UpdatePlantHarvestCycle(UpdatePlantHarvestCycleCommand command)
    {
        _logger.LogInformation("Received request to create plant harvest cycle {@plantHarvestCycle}", command);
        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

        //only load plant that is being updated
        var harvest = await _harvestCycleRepository.ReadHarvestCycle(command.HarvestCycleId, command.PlantHarvestCycleId, userProfileId);

        if (harvest.Plants.Any(g => g.PlantId == command.PlantId && g.PlantVarietyId == command.PlantVarietyId && g.Id != command.PlantHarvestCycleId && g.PlantGrowthInstructionId == command.PlantGrowthInstructionId))
        {
            throw new ArgumentException("This plant is already a part of this plan", nameof(command.PlantVarietyId));
        }

        _unitOfWork.Initialize(this.GetType().Name);

        harvest.UpdatePlantHarvestCycle(command);

        try
        {
            var generatedSchedules = await _scheduleBuilder.GeneratePlantCalendarBasedOnGrowInstruction(harvest.Plants.First(p => p.Id == command.PlantHarvestCycleId), command.PlantId, command.PlantGrowthInstructionId, command.PlantVarietyId, harvest.GardenId);

            ApplyPlantSchedules(generatedSchedules, harvest, command.PlantHarvestCycleId);

        }
        catch (Exception ex)
        {
            _logger.LogError("Exception generating plant schedules. PLantHarvest will still save: {ex}", ex);
        }

        _plantHarvestCycleRepository.UpdatePlantHarvestCycle(command.PlantHarvestCycleId, harvest);

        await _mediator.DispatchDomainEventsAsync(harvest);

        await _unitOfWork.SaveChangesAsync(this.GetType().Name);

        return command.PlantHarvestCycleId;
    }

    public async Task<String> DeletePlantHarvestCycle(string harvestCycleId, string id)
    {
        _logger.LogInformation($"Received request to delete plant harvest cycle  {harvestCycleId} and {id}");

        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;
        var harvest = await _harvestCycleRepository.ReadHarvestCycle(harvestCycleId, userProfileId);

        _unitOfWork.Initialize(this.GetType().Name);

        harvest.DeletePlantHarvestCycle(id);

        _plantHarvestCycleRepository.DeletePlantHarvestCycle(id, harvest);
        _gardenBedPlantHarvestCycleRepository.DeleteGardenBedPlantHarvestCycle(id, harvest);

        await _mediator.DispatchDomainEventsAsync(harvest);

        await _unitOfWork.SaveChangesAsync(this.GetType().Name);

        return id;
    }

    #endregion

    #region Plant Schedule
    public async Task<String> AddPlantSchedule(CreatePlantScheduleCommand command)
    {
        _logger.LogInformation("Received request to create plant schedule {@plantHarvestCycle}", command);
        try
        {
            string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

            //only load plant fot that schedule
            var harvest = await _harvestCycleRepository.ReadHarvestCycle(command.HarvestCycleId, command.PlantHarvestCycleId, userProfileId);

            var plant = harvest.Plants.First(p => p.Id == command.PlantHarvestCycleId);
            if (plant == null)
            {
                throw new ArgumentException("Schedule can only be added to a plant that is part of the Garden Plan", nameof(command.PlantHarvestCycleId));
            }

            var scheduleId = harvest.AddPlantSchedule(command);

            _plantHarvestCycleRepository.AddPlantSchedule(scheduleId, command.PlantHarvestCycleId, harvest);

            await _mediator.DispatchDomainEventsAsync(harvest);

            await _unitOfWork.SaveChangesAsync();

            return scheduleId;
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Exception adding plant schedule: {ex}", ex);
            throw;
        }

    }

    public async Task<String> UpdatePlantSchedule(UpdatePlantScheduleCommand command)
    {
        _logger.LogInformation("Received request to update plant schedule {@plantHarvestCycle}", command);
        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

        //only load plant fot that schedule
        var harvest = await _harvestCycleRepository.ReadHarvestCycle(command.HarvestCycleId, command.PlantHarvestCycleId, userProfileId);

        var plant = harvest.Plants.First(p => p.Id == command.PlantHarvestCycleId);
        if (plant == null)
        {
            throw new ArgumentException("Schedule can only be added to a plant that is part of the Garden Plan", nameof(command.PlantHarvestCycleId));
        }

        harvest.UpdatePlantSchedule(command);

        _plantHarvestCycleRepository.UpdatePlantSchedule(command.PlantScheduleId, command.PlantHarvestCycleId, harvest);

        await _mediator.DispatchDomainEventsAsync(harvest);

        await _unitOfWork.SaveChangesAsync();

        return command.PlantScheduleId;
    }

    public async Task<String> DeletePlantSchedule(string harvestCycleId, string plantHarvestCycleId, string plantScheduleId)
    {
        _logger.LogInformation($"Received request to delete plant schedule  {harvestCycleId} and {plantHarvestCycleId} and {plantScheduleId}");

        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

        //only load plant fot that schedule
        var harvest = await _harvestCycleRepository.ReadHarvestCycle(harvestCycleId, plantHarvestCycleId, userProfileId);

        harvest.DeletePlantSchedule(plantHarvestCycleId, plantScheduleId);

        _plantHarvestCycleRepository.DeletePlantSchedule(plantScheduleId, plantHarvestCycleId, harvest);

        await _mediator.DispatchDomainEventsAsync(harvest);

        await _unitOfWork.SaveChangesAsync();

        return plantScheduleId;
    }

    #endregion

    #region Garden Bed Layout
    public async Task<String> AddGardenBedPlantHarvestCycle(CreateGardenBedPlantHarvestCycleCommand command)
    {
        _logger.LogInformation("Received request to create garden bed plant {0}", command);
        try
        {
            string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

            //only load plant fot that bed
            var harvest = await _harvestCycleRepository.ReadHarvestCycle(command.HarvestCycleId, command.PlantHarvestCycleId, userProfileId);

            var plant = harvest.Plants.First(p => p.Id == command.PlantHarvestCycleId);
            if (plant == null)
            {
                throw new ArgumentException("Plant can not be added to a plant that is not part of the Garden Plan", nameof(command.PlantHarvestCycleId));
            }


            var gardenBedPlantId = harvest.AddGardenBedPlantHarvestCycle(command);

            _gardenBedPlantHarvestCycleRepository.AddGardenBedPlantHarvestCycle(gardenBedPlantId, command.PlantHarvestCycleId, harvest);

            await _mediator.DispatchDomainEventsAsync(harvest);

            await _unitOfWork.SaveChangesAsync();

            return gardenBedPlantId;
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Exception adding plant to the garden layout: {ex}", ex);
            throw;
        }

    }

    public async Task<String> UpdateGardenBedPlantHarvestCycle(UpdateGardenBedPlantHarvestCycleCommand command)
    {
        _logger.LogInformation("Received request to update garden layout for the plant {0}", command);
        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

        //only load plant fot that bed
        var harvest = await _harvestCycleRepository.ReadHarvestCycle(command.HarvestCycleId, command.PlantHarvestCycleId, userProfileId);

        var plant = harvest.Plants.First(p => p.Id == command.PlantHarvestCycleId);
        if (plant == null)
        {
            throw new ArgumentException("Plant can not be added to a plant that is not part of the Garden Plan", nameof(command.PlantHarvestCycleId));
        }


        harvest.UpdateGardenBedPlantHarvestCycle(command);

        _gardenBedPlantHarvestCycleRepository.UpdateGardenBedPlantHarvestCycle(command.GardenBedPlantHarvestCycleId, command.PlantHarvestCycleId, harvest);

        await _mediator.DispatchDomainEventsAsync(harvest);

        await _unitOfWork.SaveChangesAsync();

        return command.GardenBedPlantHarvestCycleId;
    }

    public async Task<String> DeleteGardenBedPlantHarvestCycle(string harvestCycleId, string plantHarvestCycleId, string gardenBedPlantId)
    {
        _logger.LogInformation($"Received request to delete plant from garden layout  {harvestCycleId} and {plantHarvestCycleId} and {gardenBedPlantId}");

        string userProfileId = _httpContextAccessor.HttpContext?.User.GetUserProfileId(_httpContextAccessor.HttpContext.Request.Headers)!;

        //only load plant fot that bed
        var harvest = await _harvestCycleRepository.ReadHarvestCycle(harvestCycleId, plantHarvestCycleId, userProfileId);

        harvest.DeleteGardenBedPlantHarvestCycle(plantHarvestCycleId, gardenBedPlantId);

        _gardenBedPlantHarvestCycleRepository.DeleteGardenBedPlantHarvestCycle(gardenBedPlantId, plantHarvestCycleId, harvest);

        await _mediator.DispatchDomainEventsAsync(harvest);

        await _unitOfWork.SaveChangesAsync();

        return gardenBedPlantId;
    }

    #endregion

    private void ApplyPlantSchedules(ReadOnlyCollection<CreatePlantScheduleCommand>? schedules, HarvestCycle harvest, string plantHarvestCycleId)
    {
        if (schedules == null || schedules.Count == 0) { return; }

        harvest.DeleteAllSystemGeneratedSchedules(plantHarvestCycleId);

        foreach (var schedule in schedules)
        {
            schedule.PlantHarvestCycleId = plantHarvestCycleId;
            harvest.AddPlantSchedule(schedule);
        }

    }
}

