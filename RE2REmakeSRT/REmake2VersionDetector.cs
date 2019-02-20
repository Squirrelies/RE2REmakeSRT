using ProcessMemory;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace RE2REmakeSRT
{
    public static class REmake2VersionDetector
    {
        private static byte[] GetSHA256Checksum(string filePath)
        {
            using (SHA256 checksumCalculator = SHA256.Create())
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return checksumCalculator.ComputeHash(fs);
            }
        }

        public static REmake2VersionEnumeration GetVersion(int pid)
        {
            // If we're skipping the checksum version check, return the latest version we kow about.
            if (Program.programSpecialOptions.Flags.HasFlag(ProgramFlags.SkipChecksumCheck))
                return REmake2VersionEnumeration.Stock_1p10;

            byte[] processHash = GetSHA256Checksum(NativeWrappers.GetProcessPath(pid));

            if (processHash.SequenceEqual(GameHashes.Stock_1ShotDemo))
            {
                return REmake2VersionEnumeration.Demo;
            }
            else if (processHash.SequenceEqual(GameHashes.Stock_1p00))
            {
                return REmake2VersionEnumeration.Stock_1p00;
            }
            else if (processHash.SequenceEqual(GameHashes.Stock_1p01) || processHash.SequenceEqual(GameHashes.Unknown_1))
            {
                return REmake2VersionEnumeration.Stock_1p01;
            }
            else if (processHash.SequenceEqual(GameHashes.Stock_1p10))
            {
                return REmake2VersionEnumeration.Stock_1p10;
            }
            else if (processHash.SequenceEqual(GameHashes.Stock_1p11))
            {
                return REmake2VersionEnumeration.Stock_1p11;
            }
            else
            {
                // Either a version we've never encountered before or this game was modified.
                StringBuilder sb = new StringBuilder();
                foreach (byte b in processHash)
                {
                    sb.AppendFormat("0x{0:X2}, ", b);
                }
                sb.Length -= 2;

                MessageBox.Show(null, string.Format("Unknown version of Resident Evil 2 (2019). You may encounter issues.\r\nHash: {0}", sb.ToString()), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return REmake2VersionEnumeration.Stock_1p10;
            }
        }
    }
}
