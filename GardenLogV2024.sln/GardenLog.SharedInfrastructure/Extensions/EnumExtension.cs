using System.Reflection;

namespace GardenLog.SharedInfrastructure.Extensions;
/// <summary>
/// https://stackoverflow.com/questions/3668496/most-efficient-way-to-parse-a-flagged-enum-to-a-list
/// </summary>

public static class EnumExtensions
{
    // Take anded flag enum and extract the cleaned string values.
    public static List<string> ToStringList(this Enum eNum)
        => eNum.ToString()
                .Split(',')
                .Select(str => str.ToCleanString())
                .ToList();

    // Take an individual enum and report the textual value.
    public static string ToSingleString(this Enum eNum)
        => eNum.ToString()
               .ToCleanString();

    public static string GetDescription(this Enum eNum)
    {
        Type genericEnumType = eNum.GetType();
        MemberInfo[] memberInfo = genericEnumType.GetMember(eNum.ToString());
        if ((memberInfo != null && memberInfo.Length > 0))
        {
            var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if ((_Attribs != null && _Attribs.Count() > 0))
            {
                return ((System.ComponentModel.DescriptionAttribute)_Attribs.ElementAt(0)).Description;
            }
        }
        return eNum.ToString();
    }
}
