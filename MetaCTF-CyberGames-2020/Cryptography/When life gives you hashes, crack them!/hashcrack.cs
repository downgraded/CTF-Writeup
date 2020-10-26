using System;
using System.Security.Cryptography;
using System.Text;

public class Program
{
    public static void Main()
    {
		string[] lines = System.IO.File.ReadAllLines(@"C:\wordlists\rockyou.txt");
		foreach(string line in lines)
		{
			var bytes = Encoding.Unicode.GetBytes(line);
			var saltBytes = Convert.FromBase64String("RTnbzngRZFDZcvE5mioAHQ==");
			byte[] inArray;
			var hashAlgorithm = HashAlgorithm.Create("HMACSHA256");
			var algorithm = hashAlgorithm as KeyedHashAlgorithm;
			var keyedHashAlgorithm = algorithm;
			var numArray2 = new byte[keyedHashAlgorithm.Key.Length];
			var dstOffset = 0;
			while (dstOffset < numArray2.Length)
			{
				var count = Math.Min(saltBytes.Length, numArray2.Length - dstOffset);
				Buffer.BlockCopy(saltBytes, 0, numArray2, dstOffset, count);
				dstOffset += count;
			}
			keyedHashAlgorithm.Key = numArray2;
			inArray = keyedHashAlgorithm.ComputeHash(bytes);	

			var hash = Convert.ToBase64String(inArray);
			Console.WriteLine(line + ": " + hash);
			var base64Hash = "e2+n3Gg3oBpH+nPWlQIjiAKYU4tWALorc83axst1dPU=";

				if (hash == base64Hash)
				{
					Console.WriteLine("Match found! The password is: " + line + "\n" + hash);
					break;
				}
	}
    }
}
