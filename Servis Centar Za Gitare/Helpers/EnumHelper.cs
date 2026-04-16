using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Servis_Centar_Za_Gitare.Helpers
{
    public static class EnumHelper
    {
        public static string GetDisplayName(this Enum value)
        {
            var type = value.GetType();
            var member = type.GetMember(value.ToString()).FirstOrDefault();

            if (member == null)
                return value.ToString();

            var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.GetName() ?? value.ToString();
        }
    }
}
