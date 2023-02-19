using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace LogitechSdkListener;

public static class LogitechSdkTypeRegistry
{
    public static TypeRegistry TypeRegistry { get; }

    static LogitechSdkTypeRegistry()
    {
        //get all message types from assembly
        var messageTypes = typeof(LogitechSdkTypeRegistry).Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IMessage)));

        var descriptors = new List<MessageDescriptor>();
        //add all message types to type registry
        foreach (var messageType in messageTypes)
        {
            var descriptor = messageType.GetProperty("Descriptor")?.GetValue(null) as MessageDescriptor;
            if (descriptor is not null)
            {
                descriptors.Add(descriptor);
            }
        }
            
        TypeRegistry = TypeRegistry.FromMessages(descriptors);
    }
}