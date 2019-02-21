using Microsoft.Win32;

namespace RE2REmakeSRT
{
    public struct Options
    {
        public ProgramFlags Flags;
        public double ScalingFactor;

        public void GetOptions()
        {
            // Initialize registry key.
            RegistryKey optionsKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\RE2REmakeSRT", false);

            // Load registry values.
            if (RegistryHelper.GetBoolValue(optionsKey, "Debug", false))
                Flags |= ProgramFlags.Debug;
            else
                Flags &= ~ProgramFlags.Debug;

            if (RegistryHelper.GetBoolValue(optionsKey, "NoTitleBar", false))
                Flags |= ProgramFlags.NoTitleBar;
            else
                Flags &= ~ProgramFlags.NoTitleBar;

            if (RegistryHelper.GetBoolValue(optionsKey, "AlwaysOnTop", false))
                Flags |= ProgramFlags.AlwaysOnTop;
            else
                Flags &= ~ProgramFlags.AlwaysOnTop;

            if (RegistryHelper.GetBoolValue(optionsKey, "Transparent", false))
                Flags |= ProgramFlags.Transparent;
            else
                Flags &= ~ProgramFlags.Transparent;

            if (RegistryHelper.GetBoolValue(optionsKey, "NoInventory", false))
                Flags |= ProgramFlags.NoInventory;
            else
                Flags &= ~ProgramFlags.NoInventory;

            double.TryParse(RegistryHelper.GetValue(optionsKey, "ScalingFactor", "0.75"), out ScalingFactor);

            // Do not permit ScalingFactor values less than or equal to 0% and greater than 400%.
            if (ScalingFactor <= 0 || ScalingFactor > 4)
                ScalingFactor = 0.75d;
        }

        public void SetOptions()
        {
            // Initialize registry key.
            RegistryKey optionsKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\RE2REmakeSRT", true);

            if ((Flags & ProgramFlags.Debug) == ProgramFlags.Debug)
                optionsKey.SetValue("Debug", 1, RegistryValueKind.DWord);
            else
                optionsKey.SetValue("Debug", 0, RegistryValueKind.DWord);

            if ((Flags & ProgramFlags.NoTitleBar) == ProgramFlags.NoTitleBar)
                optionsKey.SetValue("NoTitleBar", 1, RegistryValueKind.DWord);
            else
                optionsKey.SetValue("NoTitleBar", 0, RegistryValueKind.DWord);

            if ((Flags & ProgramFlags.AlwaysOnTop) == ProgramFlags.AlwaysOnTop)
                optionsKey.SetValue("AlwaysOnTop", 1, RegistryValueKind.DWord);
            else
                optionsKey.SetValue("AlwaysOnTop", 0, RegistryValueKind.DWord);

            if ((Flags & ProgramFlags.Transparent) == ProgramFlags.Transparent)
                optionsKey.SetValue("Transparent", 1, RegistryValueKind.DWord);
            else
                optionsKey.SetValue("Transparent", 0, RegistryValueKind.DWord);

            if ((Flags & ProgramFlags.NoInventory) == ProgramFlags.NoInventory)
                optionsKey.SetValue("NoInventory", 1, RegistryValueKind.DWord);
            else
                optionsKey.SetValue("NoInventory", 0, RegistryValueKind.DWord);

            // Do not permit ScalingFactor values less than or equal to 0% and greater than 400%.
            if (ScalingFactor <= 0 || ScalingFactor > 4)
                optionsKey.SetValue("ScalingFactor", "0.75", RegistryValueKind.String);
            else
                optionsKey.SetValue("ScalingFactor", ScalingFactor.ToString(), RegistryValueKind.String);
        }
    }
}
