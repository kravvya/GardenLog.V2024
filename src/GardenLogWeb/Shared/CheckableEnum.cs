namespace GardenLogWeb.Shared;

public record CheckableEnum(KeyValuePair<string, string> EnumItem)
{
    public bool IsSelected;
}
