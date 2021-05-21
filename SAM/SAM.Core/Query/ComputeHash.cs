using System.Text;

namespace SAM.Core
{
    public static partial class Query
    {
        /// <summary>
        /// Computes SHA256 Hash for given string value
        /// </summary>
        /// <param name="value">Text</param>
        /// <returns>SHA256 Hash</returns>
        public static string ComputeHash(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            System.Security.Cryptography.SHA256 sHA256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            bytes = sHA256.ComputeHash(bytes);

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
                stringBuilder.Append(bytes[i].ToString("X2"));

            return stringBuilder.ToString();
        }
    }
}