using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using NLog;

using SatisfactorySaveParser.Game;
using SatisfactorySaveParser.Structures;

namespace SatisfactorySaveParser.Save
{
    /// <summary>
    ///     SatisfactorySave is the main class for parsing a savegame
    /// </summary>
    public class FGSaveSession
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Path to save file on disk
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        ///     Save header which contains information like the version and map info
        /// </summary>
        public FSaveHeader Header { get; private set; }

        /// <summary>
        ///     Objects contained within this save
        /// </summary>
        public List<SaveObject> Objects { get; set; } = new List<SaveObject>();

        /// <summary>
        ///     List of object references of all destroyed/collected objects in the world (nut/berry bushes, slugs, etc)
        /// </summary>
        public List<ObjectReference> DestroyedActors { get; set; } = new List<ObjectReference>();

        /// <summary>
        ///     Open a savefile from disk
        /// </summary>
        /// <param name="file">Full path to the .sav file, usually found in %localappdata%/FactoryGame/Saved/SaveGames</param>
        public FGSaveSession(string file)
        {
            log.Info($"Opening save file: {file}");

            Filename = Environment.ExpandEnvironmentVariables(file);
            using (var stream = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new BinaryReader(stream))
            {
                Header = FSaveHeader.Parse(reader);

                // Does not need to be a public property because it's equal to Entries.Count
                var totalSaveObjects = reader.ReadUInt32();
                log.Info($"Save contains {totalSaveObjects} object headers");

                for (int i = 0; i < totalSaveObjects; i++)
                {
                    Objects.Add(SaveObjectFactory.ParseObject(reader));
                }

                var totalSaveObjectData = reader.ReadInt32();
                log.Info($"Save contains {totalSaveObjectData} object data");
                Trace.Assert(Objects.Count == totalSaveObjects);
                Trace.Assert(Objects.Count == totalSaveObjectData);

                for (int i = 0; i < Objects.Count; i++)
                {
                    var len = reader.ReadInt32();
                    var before = reader.BaseStream.Position;

#if DEBUG
                    //log.Trace($"Reading {len} bytes @ {before} for {Entries[i].TypePath}");
#endif

                    Objects[i].ParseData(len, reader);
                    var after = reader.BaseStream.Position;

                    if (before + len != after)
                    {
                        throw new InvalidOperationException($"Expected {len} bytes read but got {after - before}");
                    }
                }

                var collectedObjectsCount = reader.ReadInt32();
                log.Info($"Save contains {collectedObjectsCount} collected objects");
                for (int i = 0; i < collectedObjectsCount; i++)
                {
                    DestroyedActors.Add(new ObjectReference(reader));
                }

                log.Debug($"Read {reader.BaseStream.Position} of total {reader.BaseStream.Length} bytes");
                Trace.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
            }
        }

        public void Save()
        {
            Save(Filename);
        }

        public void Save(string file)
        {
            log.Info($"Writing save file: {file}");

            Filename = Environment.ExpandEnvironmentVariables(file);
            using (var stream = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.Write))
            using (var writer = new BinaryWriter(stream))
            {
                stream.SetLength(0); // Clear any original content

                Header.Serialize(writer);

                writer.Write(Objects.Count);

                var entities = Objects.Where(e => e is SaveEntity).ToArray();
                for (var i = 0; i < entities.Length; i++)
                {
                    writer.Write(SaveEntity.TypeID);
                    entities[i].SerializeHeader(writer);
                }

                var components = Objects.Where(e => e is SaveComponent).ToArray();
                for (var i = 0; i < components.Length; i++)
                {
                    writer.Write(SaveComponent.TypeID);
                    components[i].SerializeHeader(writer);
                }

                writer.Write(entities.Length + components.Length);

                using (var ms = new MemoryStream())
                using (var dataWriter = new BinaryWriter(ms))
                {
                    for (var i = 0; i < entities.Length; i++)
                    {
                        entities[i].SerializeData(dataWriter);

                        var bytes = ms.ToArray();
                        writer.Write(bytes.Length);
                        writer.Write(bytes);

                        ms.SetLength(0);
                    }
                    for (var i = 0; i < components.Length; i++)
                    {
                        components[i].SerializeData(dataWriter);

                        var bytes = ms.ToArray();
                        writer.Write(bytes.Length);
                        writer.Write(bytes);

                        ms.SetLength(0);
                    }
                }

                writer.Write(DestroyedActors.Count);
                foreach (var collectedObject in DestroyedActors)
                {
                    writer.WriteLengthPrefixedString(collectedObject.LevelName);
                    writer.WriteLengthPrefixedString(collectedObject.PathName);
                }
            }
        }
    }
}
