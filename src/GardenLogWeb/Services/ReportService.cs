using Microsoft.JSInterop;
using ClosedXML.Excel;

namespace GardenLogWeb.Services;

public interface IReportService
{
    Task DownloadHarvestPlantInGardenReport(string harvestCycleId);
}

public class ReportService : IReportService
{
    private readonly IJSRuntime _js;
    private readonly IHarvestCycleService _harvestService;
    private readonly IGardenService _gardenService;

    public ReportService(IJSRuntime js, IHarvestCycleService harvestService, IGardenService gardenService)
    {
        _js = js;
        _harvestService = harvestService;
        _gardenService = gardenService;
    }

    public async Task DownloadHarvestPlantInGardenReport(string harvestCycleId)
    {
        int row = 2;
        var harvest = await _harvestService.GetHarvest(harvestCycleId, true);
        var harvestPlants = await _harvestService.GetPlantHarvests(harvestCycleId, false);
        var beds = await _gardenService.GetGardenBeds(harvest.GardenId, true);

        var wb = GetWorkbook("Garden Plan for the Garden");
        var ws = wb.Worksheets.Add("Garden Plan for the Garden");

        ws.Cell(1, 1).Value = "Plant";
        ws.Cell(1, 2).Value = "Variety";
        ws.Cell(1, 3).Value = "Garden Location";
        ws.Cell(1, 4).Value = "Qty";
        ws.Cell(1, 5).Value = "Plant Start Date";
        ws.Cell(1, 6).Value = "Plant End Date";

        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 2).Style.Font.Bold = true;
        ws.Cell(1, 3).Style.Font.Bold = true;
        ws.Cell(1, 4).Style.Font.Bold = true;
        ws.Cell(1, 5).Style.Font.Bold = true;
        ws.Cell(1, 6).Style.Font.Bold = true;

        foreach (var harvestPlant in harvestPlants)
        {


            ws.Cell(row, 1).Value = harvestPlant.PlantName;
            ws.Cell(row, 2).Value = harvestPlant.PlantVarietyName;

            if (harvestPlant.GardenBedLayout != null && harvestPlant.GardenBedLayout.Count() > 0)
            {
                foreach (var location in harvestPlant.GardenBedLayout.Select((bed, index) => (bed, index)))
                {
                    var bed = beds.FirstOrDefault(b => b.GardenBedId == location.bed.GardenBedId);
                    if (bed != null)
                    {
                        if (location.index > 0)
                        {
                            row++;
                            ws.Cell(row, 1).Value = harvestPlant.PlantName;
                            ws.Cell(row, 2).Value = harvestPlant.PlantVarietyName;
                        }

                        ws.Cell(row, 3).Value = bed.Name;
                        ws.Cell(row, 4).Value = location.bed.NumberOfPlants;
                        var schedule = harvestPlant.PlantCalendar.FirstOrDefault(s => s.TaskType == WorkLogReasonEnum.Plant
                                                                            || s.TaskType == WorkLogReasonEnum.TransplantOutside
                                                                            || s.TaskType == WorkLogReasonEnum.SowOutside);
                        if (schedule != null)
                        {
                           
                            ws.Cell(row, 5).Value = schedule.StartDate.ToShortDateString();
                            ws.Cell(row, 6).Value = schedule.EndDate.ToShortDateString();
                        }
                    }
                }
            }

            row++;
        }

        MemoryStream xlsStream = new();
        wb.SaveAs(xlsStream);

        await DownloadFileFromStream(xlsStream.ToArray(), $"GardenPlanInGarden_{DateTime.Now.ToString("dd_MM_yyyy_HH_mm")}.xlsx");
    }

    private XLWorkbook GetWorkbook(string title)
    {
        var wb = new XLWorkbook();
        wb.Properties.Author = "GardenLog";
        wb.Properties.Title = title;
        //wb.Properties.Subject = "the Subject";
        //wb.Properties.Category = "the Category";
        //wb.Properties.Keywords = "the Keywords";
        //wb.Properties.Comments = "the Comments";
        //wb.Properties.Status = "the Status";
        // wb.Properties.LastModifiedBy = "the Last Modified By";
        wb.Properties.Company = "Slavatech inc,";
        // wb.Properties.Manager = "the Manager";

        return wb;
    }

    private async Task DownloadFileFromStream(byte[] data, string fileName)
    {

       // using var streamRef = new DotNetStreamReference(stream: data);

        await _js.InvokeVoidAsync("downloadFileFromStream", fileName, data);
    }
}
