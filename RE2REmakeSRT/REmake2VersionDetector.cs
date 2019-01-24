using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RE2REmakeSRT
{
    public static class REmake2VersionDetector
    {
        private static readonly byte[] remake2_Stock_1p00 = new byte[32]; // Undefined, we don't know the hashes yet!
        private static readonly byte[] remake2_Stock_1p01 = new byte[32]; // Undefined, we don't know the hashes yet!

        private static byte[] GetSHA256Checksum(string filePath)
        {
            using (SHA256 checksumCalculator = SHA256.Create())
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return checksumCalculator.ComputeHash(fs);
            }
        }

        public static REmake2VersionEnumeration GetVersion(Process remake2Proc)
        {
            byte[] processHash = GetSHA256Checksum(remake2Proc.MainModule.FileName);

            if (processHash.SequenceEqual(remake2_Stock_1p00))
            {
                return REmake2VersionEnumeration.Stock_1p00;
            }
            else if (processHash.SequenceEqual(remake2_Stock_1p01))
            {
                return REmake2VersionEnumeration.Stock_1p01;
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

                System.Windows.Forms.MessageBox.Show(null, string.Format("Unknown version of Resident Evil 2 (2019).\r\nHash: {0}", sb.ToString()), string.Empty, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return REmake2VersionEnumeration.Unknown;
            }
        }
    }
}
