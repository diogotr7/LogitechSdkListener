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
        var listener = new PipeListener("LGS_LED_SDK-00000001");
        listener.ClientConnected += (sender, eventArgs) => Console.WriteLine("Client connected");
        listener.ClientDisconnected += (sender, eventArgs) => Console.WriteLine("Client disconnected");
        listener.CommandReceived += (sender, bytes) =>
        {
            var id = BitConverter.ToInt32(bytes.Span[..4]);
            var data = bytes.Span[4..];
            
            ProcessCommand(id, data);
        };
        listener.Exception += (sender, exception) => Console.WriteLine(exception);
        
        Console.ReadLine();
    }

    private static void ProcessCommand(int id, ReadOnlySpan<byte> data)
    {
        switch ((LogitechCommandIds)id)
        {
            case LogitechCommandIds.Init:
                Initialize(data);
                break;
            case LogitechCommandIds.SaveLighting:
                //no data
                Console.WriteLine("Saving lighting");
                break;
            case LogitechCommandIds.SetLighting:
                SetLighting(data);
                break;
            case LogitechCommandIds.SetLightingFromBitmap:
                SetLightingFromBitmap(data);
                Console.WriteLine("Setting lighting from bitmap");
                break;
            case LogitechCommandIds.SetLightingForKeyWithHidCode:
                Console.WriteLine("Setting lighting for key HID");
                break;
            case LogitechCommandIds.SetLightingForKeyWithKeyName:
                Console.WriteLine("Setting lighting for key name");
                break;
            case LogitechCommandIds.SetLightingForKeyWithQuartzCode:
                Console.WriteLine("Setting lighting for key quartz");
                break;
            case LogitechCommandIds.SetLightingForKeyWithScanCode:
                Console.WriteLine("Setting lighting for key scan");
                break;
            case LogitechCommandIds.SetLightingForTargetZone:
                Console.WriteLine("Setting lighting for target zone");
                break;
            case LogitechCommandIds.SetTargetDevice:
                Console.WriteLine("Setting target device");
                break;
            case LogitechCommandIds.RestoreLighting:
                Console.WriteLine("Restoring lighting");
                break;
            case LogitechCommandIds.RestoreLightingForKey:
                Console.WriteLine("Restoring lighting for key HID");
                break;
            case LogitechCommandIds.ExcludeKeysFromBitmap:
                Console.WriteLine("Excluding from bitmap");
                break;
            case LogitechCommandIds.SaveLightingForKey:
                Console.WriteLine("Saving lighting for key HID");
                break;
            default: break;
        }
    }

    private static void SetLightingFromBitmap(ReadOnlySpan<byte> data)
    {
        var colors = MemoryMarshal.Cast<byte, Color>(data);
    }

    private static void SetLighting(ReadOnlySpan<byte> data)
    {
        if (data.Length != 12)
        {
            Console.WriteLine("Invalid lighting data");
            return;
        }
        
        var ints = MemoryMarshal.Cast<byte, int>(data);
        var red = ints[0];
        var green = ints[1];
        var blue = ints[2];
        Console.WriteLine($"Setting lighting to {red}, {green}, {blue}");
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

public enum LogitechCommandIds : int
{
    Init = 2049,
    SetLighting = 2050,
    SetTargetDevice = 2051,
    SaveLighting = 2052,
    RestoreLighting = 2053,
    SetLightingForKeyWithScanCode= 2054,
    SetLightingForKeyWithHidCode = 2055,
    SetLightingForKeyWithQuartzCode = 2056,
    SetLightingForKeyWithKeyName = 2057,
    SetLightingFromBitmap = 2064,
    SaveLightingForKey = 2065,
    RestoreLightingForKey = 2066,
    ExcludeKeysFromBitmap = 2067,
    SetLightingForTargetZone = 2080,
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Color
{
    public readonly byte B;
    public readonly byte G;
    public readonly byte R;
    public readonly byte A;
}