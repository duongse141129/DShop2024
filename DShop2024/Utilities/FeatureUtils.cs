using System.Security.Cryptography;
using System.Text;

namespace DShop2024.Utilities
{
    public class FeatureUtils
    {
        // Hash-based one-hot with fixed size
        public static double[] OneHotHash(string value, int size = 16)
        {
            if (string.IsNullOrWhiteSpace(value)) return new double[size];
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value.ToLowerInvariant()));
            // take first 2 bytes to index
            int idx = (hash[0] << 8 | hash[1]) % size;
            var vec = new double[size];
            vec[idx] = 1.0;
            return vec;
        }

        public static (double h, double w, double d) ParseDimensions(string dim)
        {
            // Expect "HxWxD" in cm; fallback to zeros
            try
            {
                var parts = dim.ToLower().Split('x', StringSplitOptions.TrimEntries);
                double h = double.Parse(parts[0]);
                double w = double.Parse(parts[1]);
                double d = double.Parse(parts[2]);
                return (h, w, d);
            }
            catch { return (0, 0, 0); }
        }
    }
}
