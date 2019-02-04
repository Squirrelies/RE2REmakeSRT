using System;
using System.Collections;
using System.Collections.Generic;
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
        public int SlotPosition => ProcessMemory.HighPerfBitConverter.ToInt32(data, 0x28);
        public ItemEnumeration ItemID => (ItemEnumeration)ProcessMemory.HighPerfBitConverter.ToInt32(data, 0x70);
        public WeaponEnumeration WeaponID => (WeaponEnumeration)ProcessMemory.HighPerfBitConverter.ToInt32(data, 0x74);
        public AttachmentsFlag Attachments => (AttachmentsFlag)ProcessMemory.HighPerfBitConverter.ToInt32(data, 0x78);
        public int Quantity => ProcessMemory.HighPerfBitConverter.ToInt32(data, 0x80);

        public bool IsEmptySlot => ItemID == ItemEnumeration.None && (WeaponID == WeaponEnumeration.None || WeaponID == 0);
        public bool IsItem => ItemID != ItemEnumeration.None && (WeaponID == WeaponEnumeration.None || WeaponID == 0);
        public bool IsWeapon => ItemID == ItemEnumeration.None && WeaponID != WeaponEnumeration.None && WeaponID != 0;

        public InventoryEntry(byte[] data)
        {
            this.data = data;
        }

        public bool Equals(InventoryEntry other)
        {
            return data.ByteArrayEquals(other.data);
        }

        public override bool Equals(object obj)
        {
            if (obj is InventoryEntry)
                return this.Equals((InventoryEntry)obj);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
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
