using Microsoft.Win32;

namespace RE2REmakeSRT
{
    public static class RegistryHelper
    {
        public static T GetValue<T>(RegistryKey baseKey, string valueKey, T defaultValue) => (T)baseKey.GetValue(valueKey, defaultValue);

        public static bool GetBoolValue(RegistryKey baseKey, string valueKey, bool defaultValue)
        {
            int dwordValue = GetValue(baseKey, valueKey, (defaultValue) ? 1 : 0);
            return (dwordValue == 0) ? false : true;
        }
    }
}
