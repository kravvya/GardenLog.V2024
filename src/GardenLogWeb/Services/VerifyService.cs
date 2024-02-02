using System.Reflection;

namespace GardenLogWeb.Services;

public interface IVerifyService
{
    IReadOnlyCollection<KeyValuePair<string, string>> GetCodeList<TENUM>(bool excludeDefault) where TENUM : Enum;
    IReadOnlyCollection<KeyValuePair<string, string>> GetCodeList<TENUM>() where TENUM : Enum;
    string GetDescription<TENUM>(string key) where TENUM : Enum;
    string GetDescription<TENUM>(TENUM value) where TENUM : Enum;
    IReadOnlyCollection<Color> GetPlantVarietyColors();
}

public class VerifyService : IVerifyService
{
    private const string KEY_TEMPLATE = "Verify_{0}";
    private const string COLOR_KEY = "Colors";
    private readonly ICacheService _cacheService;
    private readonly int _cacheDuration;

    public VerifyService(ICacheService cacheService, IConfiguration configuration)
    {
        _cacheService = cacheService;
        if (!int.TryParse(configuration[GlobalConstants.GLOBAL_CACHE_DURATION], out _cacheDuration)) _cacheDuration = 10;
    }

    public IReadOnlyCollection<KeyValuePair<string, string>> GetCodeList<TENUM>(bool excludeDefault) where TENUM: Enum
    {
        return GetEnumList(typeof(TENUM), excludeDefault);
    }

    public IReadOnlyCollection<KeyValuePair<string, string>> GetCodeList<TENUM>() where TENUM : Enum
    {
        return GetEnumList(typeof(TENUM));
    }

    public string GetDescription<TENUM>(string key) where TENUM : Enum
    {
        return this.GetCodeList<TENUM>().FirstOrDefault(l => l.Key.Equals(key))!.Value;
    }

    public string GetDescription<TENUM>(TENUM value) where TENUM : Enum
    {
        return this.GetCodeList<TENUM>().FirstOrDefault(l => l.Key.Equals(value.ToString()))!.Value;
    }

    public IReadOnlyCollection<Color> GetPlantVarietyColors()
    {
        if (!_cacheService.TryGetValue<List<Color>>(COLOR_KEY, out List<Color>? colors))
        {
            colors = new List<Color>
            {
                new Color("Black", "#000000", "#f8f9fa"),
                new Color("Brown", "#A0522D", "#f8f9fa"),
                new Color("Blue-Green", "#0d98ba", "#f8f9fa"),
                new Color("Green", "#008000", "#f8f9fa"),
                new Color("White", "#f8f9fa", "#212529"),
                new Color("Purple", "#800080", "#f8f9fa"),
                new Color("Red", "#FF0000", "#f8f9fa"),
                new Color("Orange", "#fca130", "#f8f9fa;"),
                new Color("Speckled", "#e8d7c1", "#212529"),
                new Color("Yellow", "#FFFF00", "#212529"),
                new Color("Multi", "#808000", "#f8f9fa")
            };

            // Save data in cache.
            _cacheService.Set(COLOR_KEY, colors, DateTime.Now.AddMinutes(_cacheDuration));
        }

        return colors!;
    }

    private IReadOnlyCollection<KeyValuePair<string, string>> GetEnumList(Type genericEnumType)
    {
        return GetEnumList(genericEnumType, false);
    }

    private IReadOnlyCollection<KeyValuePair<string, string>> GetEnumList(Type genericEnumType, bool excludeDefault)
    {
        string key = string.Format(KEY_TEMPLATE, genericEnumType.Name);

        if (!_cacheService.TryGetValue<List<KeyValuePair<string, string>>>(key, out List<KeyValuePair<string, string>>? value))
        {
            value = new List<KeyValuePair<string, string>>();

            foreach (var item in Enum.GetValues(genericEnumType))
            {
                var verify = new KeyValuePair<string, string>(Enum.GetName(genericEnumType, item)!, GetDescription(((Enum)item)));
                value.Add(verify);
            }

            _cacheService.Set<IReadOnlyCollection<KeyValuePair<string, string>>>(key, value);
        }

        if (!excludeDefault) return value!;

        return value!.Where(v => !v.Key.Equals("Unspecified")).ToList();
    }


    private static string GetDescription(Enum GenericEnum)
    {
        Type genericEnumType = GenericEnum.GetType();
        MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
        if ((memberInfo != null && memberInfo.Length > 0))
        {
            var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if ((_Attribs != null && _Attribs.Length > 0))
            {
                return ((System.ComponentModel.DescriptionAttribute)_Attribs.ElementAt(0)).Description;
            }
        }
        return GenericEnum.ToString();
    }


    
}

