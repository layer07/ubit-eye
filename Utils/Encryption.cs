using System.Security.Cryptography;
using System.Text;

namespace MinerPulse
{
	public static class EncryptionHelper
	{
		private static readonly byte[] Key = Encoding.UTF8.GetBytes("B5d3sD1G0k8tK7G9xQmV3uN4oP5zR6sT7uV8wX9yZ0a1b2c3d4e5f6g7h8i9j0k1!");
		private static readonly byte[] IV = Encoding.UTF8.GetBytes("L1m2N3o4P5q6R7s8T9u0V1w2X3y4Z5a6");

		public static string EncryptString(string plainText)
		{
			if (plainText == null)
				throw new ArgumentNullException(nameof(plainText));

			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = Key;
				aesAlg.IV = IV;
				aesAlg.Mode = CipherMode.CBC;
				aesAlg.Padding = PaddingMode.PKCS7;

				ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				using (MemoryStream msEncrypt = new MemoryStream()) using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
				{
					swEncrypt.Write(plainText);
					swEncrypt.Close();
					return Convert.ToBase64String(msEncrypt.ToArray());
				}
			}
		}

		public static string DecryptString(string cipherText)
		{
			if (cipherText == null)
				throw new ArgumentNullException(nameof(cipherText));

			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = Key;
				aesAlg.IV = IV;
				aesAlg.Mode = CipherMode.CBC;
				aesAlg.Padding = PaddingMode.PKCS7;

				ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

				using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText))) using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) using (StreamReader srDecrypt = new StreamReader(csDecrypt))
				{
					return srDecrypt.ReadToEnd();
				}
			}
		}
	}
}
