global using GardenLogWeb.Models;
global using GardenLogWeb.Models.Harvest;
global using GardenLogWeb.Models.Plants;
global using GardenLogWeb.Models.Task;
global using GardenLogWeb.Models.UserProfile;
global using GardenLogWeb.Models.Work;
global using GardenLogWeb.Services;
global using GardenLogWeb.Shared;
global using GardenLogWeb.Shared.Extensions;
global using GardenLogWeb.Shared.Services;
global using ImageCatalog.Contract.ViewModels;
global using PlantCatalog.Contract;
global using PlantCatalog.Contract.Enum;
global using PlantCatalog.Contract.ViewModels;
global using PlantHarvest.Contract;
global using PlantHarvest.Contract.Enum;
global using PlantHarvest.Contract.ViewModels;
global using System.Text.Json;
global using UserManagement.Contract;
global using UserManagement.Contract.ViewModels;
global using GardenLog.SharedKernel.Enum;

namespace GardenLogWeb;

public static class GlobalConstants
{
    public const string PLANTCATALOG_API = "PlantCatalog.Api";
    public const string PLANTHARVEST_API = "PlantHarvest.Api";
    public const string IMAGEPLANTCATALOG_API = "ImageCatalog.Api";
    public const string USERMANAGEMENT_API = "UserManagement.Api";
    public const string USERMANAGEMENT_NO_AUTH = "UserManagement.CreateOnly.Api";

    public const string ROLE_MASTER_GARDENER = "master-gardener";
    public const string ROLE_WRITE_PLANTS = "write:plants";
    public const string ROLE_WRITE_PLANT_VARIETIES = "write:plant-varieties";
    public const string ROLE_WRITE_GROW_INSTRUCTIONS = "write:grow-instructions";

    public const string MODAL_FORM_COLOR = "#A0C136";

    public const string GLOBAL_CACHE_DURATION = "Cache:Duration";
}

