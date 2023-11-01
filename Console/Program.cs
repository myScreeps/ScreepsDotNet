using System;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using ScreepsDotNet.World;
public class ObjectToByteArrayExample
{
    public static byte[] ObjectToByteArray<T>(T obj)
    {
        if (obj == null)
            return null;

        
        string jsonString = JsonSerializer.Serialize(obj);
        return Encoding.UTF8.GetBytes(jsonString);
    }

    public static T ByteArrayToObject<T>(byte[] byteArray)
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


  // public static void Main()
   // {


        //var example1 = new ChecksumGenerator.CheckSumExample(1, 1);

        //// Example: Serialize an object to a byte array using System.Text.Json
        //var data = new { Name = "John", Age = 30 };
        //byte[] byteArray = ObjectToByteArray(example1);
        //byte[] bytes =ObjectToByteArray(example1);
        
        //Console.WriteLine(bytes.Length);   
        //// Deserialize it back to an object
        //var result = ByteArrayToObject<object>(byteArray);

        //Console.WriteLine("Deserialized object: " + JsonSerializer.Serialize(result));
    //}



 //   using System;

public class CustomEventExample
{
    // Step 1: Define a delegate for the event handler.
    public delegate void CustomEventHandler(object sender, EventArgs e);

    // Step 2: Declare the event using the delegate.
    public event CustomEventHandler CustomEvent;

    // Step 3: Raise the event when something significant happens.
    protected virtual void OnCustomEvent()
    {
        // Check if there are any subscribers (event handlers).
        if (CustomEvent != null)
        {
            CustomEvent(this, EventArgs.Empty); // You can pass custom event arguments here.
        }
    }

    // Some method that triggers the event.
    public void DoSomethingImportant()
    {
        // Perform some important action here.
        // Step 4: Raise the custom event when the significant action is complete.gi

        OnCustomEvent();
    }
}

public class EventSubscriber
{
    public EventSubscriber(CustomEventExample example)
    {
        // Step 5: Subscribe to the custom event.
        example.CustomEvent += HandleCustomEvent;
    }

    private void HandleCustomEvent(object sender, EventArgs e)
    {
        Console.WriteLine("Custom event was raised!");
    }
}

    public class Program
    {
        public static void Main()
        {
            CustomEventExample example = new CustomEventExample();
            EventSubscriber subscriber = new EventSubscriber(example);

            // Call a method that triggers the custom event.
            example.DoSomethingImportant();
        }
    }


}
