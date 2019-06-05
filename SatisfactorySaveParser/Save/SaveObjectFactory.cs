using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NLog;
using SatisfactorySaveParser.Game;

namespace SatisfactorySaveParser.Save
{
    public static class SaveObjectFactory
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<string, Type> objectTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(SaveObjectAttribute), false))
                .ToDictionary(t => ((SaveObjectAttribute)t.GetCustomAttribute(typeof(SaveObjectAttribute), false)).Type, t => t);
        private static readonly List<string> missingTypes = new List<string>();

        public static SaveObject ParseObject(BinaryReader reader)
        {
            var kind = (SaveObjectKind)reader.ReadInt32();
            var className = reader.ReadLengthPrefixedString();

            reader.BaseStream.Position -= className.GetSerializedLength();

            if (!objectTypes.TryGetValue(className, out Type type))
            {
                if (!missingTypes.Contains(className))
                {
                    log.Warn($"Missing {kind} {className}");
                    missingTypes.Add(className);
                }
            }

            if (kind == SaveObjectKind.Actor)
                return new SaveEntity(reader);

            if (kind == SaveObjectKind.Component)
                return new SaveComponent(reader);

            throw new NotImplementedException($"Unknown object kind {kind}");
        }
    }
}
