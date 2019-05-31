using System;
using System.IO;

using NLog;

using SatisfactorySaveParser.Exceptions;

namespace SatisfactorySaveParser.Save
{
    /// <summary>
    ///     Engine class: FSaveHeader
    ///     Header: FGSaveSystem.h
    /// </summary>
    public class FSaveHeader
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private ESessionVisibility sessionVisibility;

        /// <summary>
        ///     Header version number
        /// </summary>
        public FSaveHeaderVersion HeaderVersion { get; set; }

        /// <summary>
        ///     Save version number
        /// </summary>
        public FSaveCustomVersion SaveVersion { get; set; }

        /// <summary>
        ///     Save build number 
        ///     Should indicate the build of the game that generated this save, but is currently always 66297
        /// </summary>
        public int BuildVersion { get; set; }

        /// <summary>
        ///     The name of what appears to be the root object of the save.
        ///     Seems to always be "Persistent_Level"
        /// </summary>
        public string MapName { get; set; }
        /// <summary>
        ///     An URL style list of arguments of the session.
        ///     Contains the startloc, sessionName and Visibility
        /// </summary>
        public string MapOptions { get; set; }
        /// <summary>
        ///     Name of the saved game as entered when creating a new game
        /// </summary>
        public string SessionName { get; set; }

        /// <summary>
        ///     Amount of seconds spent in this save
        /// </summary>
        public int PlayDuration { get; set; }

        /// <summary>
        ///     Unix timestamp of when the save was saved
        /// </summary>
        public long SaveDateTime { get; set; }

        /// <summary>
        ///     The session visibility of the game
        ///     Only valid for saves with HeaderVersion >= AddedSessionVisibility
        /// </summary>
        public ESessionVisibility SessionVisibility
        {
            get
            {
                if (!SupportsSessionVisibility)
                    throw new InvalidOperationException($"{nameof(SessionVisibility)} is not supported for this save version");

                return sessionVisibility;
            }
            set
            {
                if (!SupportsSessionVisibility)
                    throw new InvalidOperationException($"{nameof(SessionVisibility)} is not supported for this save version");

                sessionVisibility = value;
            }
        }

        /// <summary>
        ///     Helper property that indicates if this save header supports SessionVisibility
        /// </summary>
        public bool SupportsSessionVisibility => HeaderVersion >= FSaveHeaderVersion.AddedSessionVisibility;

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((int)HeaderVersion);
            writer.Write((int)SaveVersion);
            writer.Write(BuildVersion);

            writer.WriteLengthPrefixedString(MapName);
            writer.WriteLengthPrefixedString(MapOptions);
            writer.WriteLengthPrefixedString(SessionName);

            writer.Write(PlayDuration);
            writer.Write(SaveDateTime);

            if (SupportsSessionVisibility)
                writer.Write((byte)SessionVisibility);
        }

        public static FSaveHeader Parse(BinaryReader reader)
        {
            var headerVersion = (FSaveHeaderVersion)reader.ReadInt32();
            var saveVersion = (FSaveCustomVersion)reader.ReadInt32();

            if (headerVersion > FSaveHeaderVersion.LatestVersion)
                throw new UnsupportedHeaderVersionException(headerVersion);

            if (saveVersion > FSaveCustomVersion.LatestVersion)
                throw new UnsupportedSaveVersionException(saveVersion);

            var header = new FSaveHeader
            {
                HeaderVersion = headerVersion,
                SaveVersion = saveVersion,
                BuildVersion = reader.ReadInt32(),

                MapName = reader.ReadLengthPrefixedString(),
                MapOptions = reader.ReadLengthPrefixedString(),
                SessionName = reader.ReadLengthPrefixedString(),

                PlayDuration = reader.ReadInt32(),
                SaveDateTime = reader.ReadInt64()
            };

            if (header.SupportsSessionVisibility)
            {
                header.SessionVisibility = (ESessionVisibility)reader.ReadByte();
                log.Debug($"Read save header: HeaderVersion={header.HeaderVersion}, SaveVersion={(int)header.SaveVersion}, BuildVersion={header.BuildVersion}, MapName={header.MapName}, MapOpts={header.MapOptions}, Session={header.SessionName}, PlayTime={header.PlayDuration}, SaveTime={header.SaveDateTime}, Visibility={header.SessionVisibility}");
            }
            else
            {
                log.Debug($"Read save header: HeaderVersion={header.HeaderVersion}, SaveVersion={(int)header.SaveVersion}, BuildVersion={header.BuildVersion}, MapName={header.MapName}, MapOpts={header.MapOptions}, Session={header.SessionName}, PlayTime={header.PlayDuration}, SaveTime={header.SaveDateTime}");
            }

            return header;
        }
    }
}
