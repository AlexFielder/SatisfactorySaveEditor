using System;
using System.IO;
using SatisfactorySaveParser.Structures;

namespace SatisfactorySaveParser.Game
{
    /// <summary>
    ///     Class representing a single saved UObject in a Satisfactory save
    /// </summary>
    public abstract class SaveObject : IObjectReference
    {
        /// <summary>
        ///     Forward slash separated path of the script/prefab of this object.
        ///     Can be an empty string.
        /// </summary>
        public string TypePath { get; set; }

        /// <summary>
        ///     Root object (?) of this object
        ///     Often some form of "Persistent_Level", can be an empty string
        /// </summary>
        public string LevelName { get; set; }

        /// <summary>
        ///     Unique (?) name of this object
        /// </summary>
        public string PathName { get; set; }

        public SaveObject ReferencedObject { get => this; set => throw new NotImplementedException(); }

        /// <summary>
        ///     Main serialized data of the object
        /// </summary>
        public SerializedFields DataFields { get; set; }

        protected SaveObject(string typePath, string rootObject, string instanceName)
        {
            TypePath = typePath;
            LevelName = rootObject;
            PathName = instanceName;
        }

        protected SaveObject(BinaryReader reader)
        {
            TypePath = reader.ReadLengthPrefixedString();
            LevelName = reader.ReadLengthPrefixedString();
            PathName = reader.ReadLengthPrefixedString();
        }

        public virtual void SerializeHeader(BinaryWriter writer)
        {
            writer.WriteLengthPrefixedString(TypePath);
            writer.WriteLengthPrefixedString(LevelName);
            writer.WriteLengthPrefixedString(PathName);
        }

        public virtual void SerializeData(BinaryWriter writer)
        {
            DataFields.Serialize(writer);
        }

        public virtual void ParseData(int length, BinaryReader reader)
        {
            DataFields = SerializedFields.Parse(length, reader);
        }

        public override string ToString()
        {
            return TypePath;
        }
    }
}
