
using GardenLog.SharedInfrastructure.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Reflection;
using System.Text;

namespace GardenLog.SharedInfrastructure.MongoDB;

/// <summary>
/// Represents a serializer for enums.
/// https://stackoverflow.com/questions/30421379/mongodb-custom-collection-serializer
/// </summary>
/// <typeparam name="TEnum">The type of the enum.</typeparam>
public class EnumToStringArraySerializer<TEnum> : StructSerializerBase<TEnum>, IRepresentationConfigurable<EnumToStringArraySerializer<TEnum>> where TEnum : struct, Enum
{
    // private fields
    private readonly BsonType _representation = BsonType.String;
    private readonly TypeCode _underlyingTypeCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumToStringArraySerializer{TEnum}"/> class.
    /// </summary>
    /// <param name="representation">The representation.</param>
    public EnumToStringArraySerializer()
    {
        // don't know of a way to enforce this at compile time
        var enumTypeInfo = typeof(TEnum).GetTypeInfo();
        if (!enumTypeInfo.IsEnum)
        {
            var message = string.Format("{0} is not an enum type.", typeof(TEnum).FullName);
            throw new BsonSerializationException(message);
        }
        _underlyingTypeCode = Type.GetTypeCode(Enum.GetUnderlyingType(typeof(TEnum)));
    }

    // public properties
    /// <summary>
    /// Gets the representation.
    /// </summary>
    /// <value>
    /// The representation.
    /// </value>
    public BsonType Representation =>
        _representation;

    // public methods
    /// <summary>
    /// Deserializes a value.
    /// </summary>
    /// <param name="context">The deserialization context.</param>
    /// <param name="args">The deserialization args.</param>
    /// <returns>A deserialized value.</returns>
    public override TEnum Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonReader = context.Reader;
        var sb = new StringBuilder();
       
        bsonReader.ReadStartArray();

        while (true)
        {
            try
            {
               sb.Append($"{bsonReader.ReadString()},");
            }
            catch (Exception)
            {
                context.Reader.ReadEndArray();
                break;
            }
        }
     

        string value = sb.Length>0? sb.ToString().Remove(sb.Length-1): string.Empty;

        return ConvertStringToEnum(value);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return
            obj is EnumSerializer<TEnum> other &&
            _representation == other.Representation;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _representation.GetHashCode();
    }

    /// <summary>
    /// Serializes a value.
    /// </summary>
    /// <param name="context">The serialization context.</param>
    /// <param name="args">The serialization args.</param>
    /// <param name="value">The object.</param>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TEnum value)
    {
        var bsonWriter = context.Writer;
        bsonWriter.WriteStartArray();

        foreach (string item in value.ToStringList())
        {
            bsonWriter.WriteString(item);
        }

        bsonWriter.WriteEndArray();
    }


    /// <summary>
    /// Returns a serializer that has been reconfigured with the specified representation.
    /// </summary>
    /// <param name="representation">The representation.</param>
    /// <returns>The reconfigured serializer.</returns>
    public EnumToStringArraySerializer<TEnum> WithRepresentation(BsonType representation)
    {
        if (representation == 0)
        {
            representation = GetRepresentationForUnderlyingType();
        }

        if (representation == _representation)
        {
            return this;
        }
        else
        {
            return new EnumToStringArraySerializer<TEnum>();
        }
    }

    // explicit interface implementations
    IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
    {
        return WithRepresentation(representation);
    }

   
   
    private BsonType GetRepresentationForUnderlyingType()
    {
        var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        return (underlyingType == typeof(long) || underlyingType == typeof(ulong)) ? BsonType.Int64 : BsonType.Int32;
    }

    private TEnum ConvertStringToEnum(string value)
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: false, out var result))
        {
            return result;
        }

        // fall back to case-insensitive parse
        return (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase: true);
    }
}
