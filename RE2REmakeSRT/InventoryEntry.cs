using System;
using System.Diagnostics;

namespace RE2REmakeSRT
{
    [DebuggerDisplay("{_DebuggerDisplay,nq}")]
    public struct InventoryEntry : IEquatable<InventoryEntry>
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
        public ItemEnumeration ItemID => (ItemEnumeration)ProcessMemory.HighPerfBitConverter.ToInt32(data, 0xA0);
        public WeaponEnumeration WeaponID => (WeaponEnumeration)ProcessMemory.HighPerfBitConverter.ToInt32(data, 0xA4);
        public AttachmentsFlag Attachments => (AttachmentsFlag)ProcessMemory.HighPerfBitConverter.ToInt32(data, 0xA8);
        public int Quantity => ProcessMemory.HighPerfBitConverter.ToInt32(data, 0xB0);

        public bool IsEmptySlot => ItemID == ItemEnumeration.None && (WeaponID == WeaponEnumeration.None || WeaponID == 0);
        public bool IsItem => ItemID != ItemEnumeration.None && WeaponID == WeaponEnumeration.None;
        public bool IsWeapon => ItemID == ItemEnumeration.None && WeaponID != WeaponEnumeration.None;

        public InventoryEntry(byte[] data)
        {
            this.data = data;
        }

        public bool Equals(InventoryEntry other)
        {
            return data.ByteArrayEquals(other.data);
        }

        public static bool operator ==(InventoryEntry obj1, InventoryEntry obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (ReferenceEquals(obj1, null))
                return false;

            if (ReferenceEquals(obj2, null))
                return false;

            return obj1.data.ByteArrayEquals(obj2.data);
        }

        public static bool operator !=(InventoryEntry obj1, InventoryEntry obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
