using System.Diagnostics;

namespace RE2REmakeSRT
{
    [DebuggerDisplay("{_DebuggerDisplay,nq}")]
    public struct InventoryEntry
    {
        /// <summary>
        /// Debugger display message.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string _DebuggerDisplay
        {
            get
            {
                if (IsItem)
                    return string.Format("[#{0}] Item {1} Quantity {2}", SlotPosition, ItemID, Quantity);
                else if (IsWeapon)
                    return string.Format("[#{0}] Weapon {1} Quantity {2} Attachments {3}", SlotPosition, WeaponID, Quantity, Attachments);
                else
                    return string.Format("[#{0}] Empty Slot", SlotPosition);
            }
        }

        // Private storage variable.
        private readonly byte[] data; // 240 (0xF0) bytes.

        // Public accessor properties.
        public int SlotPosition => ProcessMemory.HighPerfBitConverter.ToInt32(data, 0x58);
        public int ItemID => ProcessMemory.HighPerfBitConverter.ToInt32(data, 0xA0);
        public int WeaponID => ProcessMemory.HighPerfBitConverter.ToInt32(data, 0xA4);
        public int Attachments => ProcessMemory.HighPerfBitConverter.ToInt32(data, 0xA8);
        public int Quantity => ProcessMemory.HighPerfBitConverter.ToInt32(data, 0xB0);

        public bool IsEmptySlot => ItemID == 0 && WeaponID == -1;
        public bool IsItem => ItemID != 0 && WeaponID == -1;
        public bool IsWeapon => ItemID == 0 && WeaponID != -1;

        public InventoryEntry(byte[] data)
        {
            this.data = data;
        }
    }
}
