using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace Lgs;

[SupportedOSPlatform("windows")]
internal class Program
{
    static void Main(string[] args)
    {
        //\\.\pipe\LGS_LED_SDK-00000001
        var listener = new PipeListener("LGS_LED_SDK-00000001");
        listener.ClientConnected += (sender, eventArgs) => Console.WriteLine("Client connected");
        listener.ClientDisconnected += (sender, eventArgs) => Console.WriteLine("Client disconnected");
        listener.CommandReceived += (sender, bytes) =>
        {
            var length = bytes.Length;
            var id = BitConverter.ToInt32(bytes.Span.Slice(0, 4));
            switch (id)
            {
                case LogitechCommandIds.INITIALIZE: Initialize(bytes.Span[4..]); break;
                case LogitechCommandIds.SAVE_LIGHTING: Console.WriteLine("Saving lighting"); break;
                case LogitechCommandIds.SET_LIGHTING: Console.WriteLine("Setting lighting"); break;
                case LogitechCommandIds.SET_LIGHTING_BITMAP: Console.WriteLine("Setting lighting from bitmap"); break;
                case LogitechCommandIds.SET_LIGHTING_FOR_KEY_HID: Console.WriteLine("Setting lighting for key HID"); break;
                case LogitechCommandIds.SET_LIGHTING_FOR_KEY_NAME: Console.WriteLine("Setting lighting for key name"); break;
                case LogitechCommandIds.SET_LIGHTING_FOR_KEY_QUARTZ: Console.WriteLine("Setting lighting for key quartz"); break;
                case LogitechCommandIds.SET_LIGHTING_FOR_KEY_SCAN: Console.WriteLine("Setting lighting for key scan"); break;
                case LogitechCommandIds.SET_LIGHTING_FOR_TARGET_ZONE: Console.WriteLine("Setting lighting for target zone"); break;
                case LogitechCommandIds.SET_TARGET_DEVICE: Console.WriteLine("Setting target device"); break;
                case LogitechCommandIds.RESTORE_LIGHTING: Console.WriteLine("Restoring lighting"); break;
                case LogitechCommandIds.RESTORE_LIGHTING_FOR_KEY: Console.WriteLine("Restoring lighting for key HID"); break;
                case LogitechCommandIds.EXCLUDE_FROM_BITMAP: Console.WriteLine("Excluding from bitmap"); break;
                case LogitechCommandIds.SAVE_LIGHTING_FOR_KEY: Console.WriteLine("Saving lighting for key HID"); break;
                default: break;
            }
        };
        listener.Exception += (sender, exception) => Console.WriteLine(exception);
        
        Console.ReadLine();
    }

    private static void Initialize(ReadOnlySpan<byte> span)
    {
        var filenameBytes = span.Slice(0, 512 * 2);
        var filename = GetUnicodeTerminatedString(filenameBytes);
        
        Console.WriteLine($"Initializing with {filename}");
    }

    private static object GetUnicodeTerminatedString(ReadOnlySpan<byte> filenameBytes)
    {
        Span<byte> nullterm = stackalloc byte[2]
        {
            0, 0
        };
        var nulltermIndex = filenameBytes.IndexOf(nullterm);
        return Encoding.Unicode.GetString(filenameBytes.Slice(0, nulltermIndex + 1));
    }
}

public static class LogitechCommandIds
{
    public const int INITIALIZE = 2049;
    public const int SET_LIGHTING = 2050;
    public const int SET_TARGET_DEVICE = 2051;
    public const int SAVE_LIGHTING = 2052;
    public const int RESTORE_LIGHTING = 2053;
    public const int SET_LIGHTING_FOR_KEY_SCAN= 2054;
    public const int SET_LIGHTING_FOR_KEY_HID = 2055;
    public const int SET_LIGHTING_FOR_KEY_QUARTZ = 2056;
    public const int SET_LIGHTING_FOR_KEY_NAME = 2057;
    public const int SET_LIGHTING_BITMAP = 2064;
    public const int SAVE_LIGHTING_FOR_KEY = 2065;
    public const int RESTORE_LIGHTING_FOR_KEY = 2066;
    public const int EXCLUDE_FROM_BITMAP = 2067;
    public const int SET_LIGHTING_FOR_TARGET_ZONE = 2080;
}