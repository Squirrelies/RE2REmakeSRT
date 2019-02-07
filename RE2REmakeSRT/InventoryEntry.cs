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

        // Storage variable.
        public readonly int SlotPosition;
        public readonly byte[] Data;

        // Accessor properties.
        public ItemEnumeration ItemID => (ItemEnumeration)ProcessMemory.HighPerfBitConverter.ToInt32(Data, 0x00);
        public WeaponEnumeration WeaponID => (WeaponEnumeration)ProcessMemory.HighPerfBitConverter.ToInt32(Data, 0x04);
        public AttachmentsFlag Attachments => (AttachmentsFlag)ProcessMemory.HighPerfBitConverter.ToInt32(Data, 0x08);
        public int Quantity => ProcessMemory.HighPerfBitConverter.ToInt32(Data, 0x10);
        
        public bool IsItem => ItemID != ItemEnumeration.None && (WeaponID == WeaponEnumeration.None || WeaponID == 0);
        public bool IsWeapon => ItemID == ItemEnumeration.None && WeaponID != WeaponEnumeration.None && WeaponID != 0;
        public bool IsEmptySlot => !IsItem && !IsWeapon;

        public InventoryEntry(int slotPosition, byte[] data)
        {
            this.SlotPosition = slotPosition;
            this.Data = data;
        }

        public bool Equals(InventoryEntry other)
        {
            return Data.ByteArrayEquals(other.Data);
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

            return obj1.Data.ByteArrayEquals(obj2.Data);
        }

        public static bool operator !=(InventoryEntry obj1, InventoryEntry obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
