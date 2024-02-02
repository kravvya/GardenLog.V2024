namespace GardenLogWeb.Pages.Harvest.Components;

public class PlantHarvestFilter
{
    public event EventHandler<EventArgs>? ModelChanged;

    public string PlantId { get; set; }=string.Empty;
    public bool IsStartIndoors { get; set; }
    public bool IsDirectSow { get; set; }
    public bool IsTransplant { get; set; }

    public void SetValue(String fieldName, object? value)
    {
        var propertyInfo = this.GetType().GetProperty(fieldName);
        if (propertyInfo != null)
        {
            propertyInfo.SetValue(this, value);
            OnModelChanged();
        }
    }
    protected void OnModelChanged()
    {
        if (ModelChanged != null) ModelChanged.Invoke(this, new EventArgs());
    }
}
