using System;

namespace Its.Configuration
{
    /// <summary>
    /// Deserializes a string into a specified type.
    /// </summary>
    /// <param name="targetType">The type to which the settings should be deserialized.</param>
    /// <param name="serialized">The serialized settings.</param>
    /// <returns>An instance of <paramref name="targetType" />.</returns>
    public delegate object DeserializeSettings(Type targetType, string serialized);
}