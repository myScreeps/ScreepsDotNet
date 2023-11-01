namespace ScreepsDotNet.World
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using System.Diagnostics.CodeAnalysis;

    ////using Newtonsoft.Json;
    //using System.Runtime.Serialization;
    //using System.Runtime.Serialization.Formatters.Binary;
    //using System.Diagnostics.CodeAnalysis;

    public class ChecksumGenerator
    {
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]

        public static byte[] ObjectToByteArray<T>(T obj)
        {
            if (obj == null)
                return null;


            string jsonString = JsonSerializer.Serialize(obj);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]

        public static T? ByteArrayToObject<T>(byte[] byteArray)
        {
            if (byteArray == null)
                return default(T);

            var jsonString = Encoding.UTF8.GetString(byteArray);
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        public static char[] ByteArrayToCharArray(byte[] byteArray)
        {
            if (byteArray == null)
                return null;

            char[] charArray = Encoding.UTF8.GetChars(byteArray);
            return charArray;
        }


        public static string CalculateChecksum(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public static string CalculateChecksum(char[] data)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            return CalculateChecksum(byteArray);
        }


        public class CheckSumExample
        {
            public int x { get; set; }
            public int y { get; set; }

            public CheckSumExample(int _x, int _y)
            {
                x = _x;
                y = _y;


            }

            public CheckSumExample()
            {
                x = 0;
                y = 0;
            }

        }
    }
}
