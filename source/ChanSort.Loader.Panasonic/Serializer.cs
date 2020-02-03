using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;

using ChanSort.Api;

namespace ChanSort.Loader.Panasonic
{
    class Serializer : SerializerBase
    {
        private const string ERR_FileFormatOrEncryption = "File uses an unknown format or encryption";
        private readonly ChannelList avbtChannels = new ChannelList(SignalSource.AnalogT, "Analog Antenna");
        private readonly ChannelList avbcChannels = new ChannelList(SignalSource.AnalogC, "Analog Cable");
        private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT, "DVB-T");
        private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbC, "DVB-C");
        private readonly ChannelList dvbsChannels = new ChannelList(SignalSource.DvbS, "DVB-S");
        private readonly ChannelList satipChannels = new ChannelList(SignalSource.SatIP, "SAT>IP");
        private readonly ChannelList freesatChannels = new ChannelList(SignalSource.DvbS | SignalSource.Freesat, "Freesat");

        private string workFile;
        private CypherMode cypherMode;
        private byte[] fileHeader = new byte[0];
        private int dbSizeOffset;
        private bool littleEndianByteOrder;
        private string charEncoding;

        enum CypherMode
        {
          None,
          HeaderAndChecksum,
          Encryption,
          Unknown
        }

        #region ctor()
        public Serializer(string inputFile) : base(inputFile)
        {
            DepencencyChecker.AssertVc2010RedistPackageX86Installed();

            Features.ChannelNameEdit = ChannelNameEditMode.All;
            Features.DeleteMode = DeleteMode.Physically;
            Features.CanSkipChannels = true;
            Features.CanLockChannels = true;
            Features.CanHideChannels = false;
            Features.CanHaveGaps = false;
            Features.EncryptedFlagEdit = true;
            Features.SortedFavorites = true;

            DataRoot.AddChannelList(avbtChannels);
            DataRoot.AddChannelList(avbcChannels);
            DataRoot.AddChannelList(dvbtChannels);
            DataRoot.AddChannelList(dvbcChannels);
            DataRoot.AddChannelList(dvbsChannels);
            DataRoot.AddChannelList(satipChannels);
            DataRoot.AddChannelList(freesatChannels);

            // hide columns for fields that don't exist in Panasonic channel list
            foreach (ChannelList list in DataRoot.ChannelLists)
            {
                list.VisibleColumnFieldNames.Remove("PcrPid");
                list.VisibleColumnFieldNames.Remove("VideoPid");
                list.VisibleColumnFieldNames.Remove("AudioPid");
            }
        }
        #endregion

        #region Load()
        public override void Load()
        {
            workFile = GetUncypheredWorkFile();
            CreateDummySatellites();

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + workFile))
            {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    RepairCorruptedDatabaseImage(cmd);
                    InitCharacterEncoding(cmd);

                    cmd.CommandText = "SELECT count(1) FROM sqlite_master WHERE type = 'table' and name in ('SVL', 'TSL')";
                    if (Convert.ToInt32(cmd.ExecuteScalar()) != 2)
                        throw new FileLoadException("File doesn't contain the expected TSL/SVL tables");

                    ReadChannels(cmd);
                }
            }
        }
        #endregion

        #region GetUncypheredWorkFile()
        private string GetUncypheredWorkFile()
        {
            cypherMode = GetCypherMode(FileName);

            switch (cypherMode)
            {
                case CypherMode.Unknown:
                {
                    throw new FileLoadException(ERR_FileFormatOrEncryption);
                }
                case CypherMode.None:
                {
                    return FileName;
                }
            }

            TempPath = Path.GetTempFileName();
            DeleteTempPath();

            if (cypherMode == CypherMode.Encryption)
                CypherFile(FileName, TempPath, false);
            else
                RemoveHeader(FileName, TempPath);

            return TempPath;
        }
        #endregion

        #region GetCypherMode()
        private CypherMode GetCypherMode(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                using (BinaryReader rdr = new BinaryReader(stream))
                {
                    uint value = (uint) rdr.ReadInt32();

                    switch (value)
                    {
                        case 0x694C5153:
                        {
                            return CypherMode.None; // "SQLi"
                        }
                        case 0x42445350:
                        {
                            return CypherMode.HeaderAndChecksum; // "PSDB"
                        }
                        case 0xA07DCB50:
                        {
                            return CypherMode.Encryption;
                        }
                        default:
                        {
                            return CypherMode.Unknown;
                        }
                    }
                }
            }
        }
        #endregion

        #region CypherFile()
        /// <summary>
        /// XOR-based cypher which can be used to alternately crypt/decrypt data
        /// </summary>
        private void CypherFile(string input, string output, bool encrypt)
        {
            byte[] fileContent = File.ReadAllBytes(input);

            if (!encrypt && CalcChecksum(fileContent, fileContent.Length) != 0)
                throw new FileLoadException("Checksum validation failed");

            int chiffre = 0x0388;
            int step = 0;

            for (int i = 0; i < fileContent.Length - 4; i++)
            {
                byte b = fileContent[i];
                byte n = (byte) (b ^ (chiffre >> 8));
                fileContent[i] = n;

                if (++step < 256)
                  chiffre += (encrypt ? n : b) + 0x96A3;
                else
                {
                  chiffre = 0x0388;
                  step = 0;
                }
            }

            if (encrypt)
                UpdateChecksum(fileContent);

            File.WriteAllBytes(output, fileContent);
        }
        #endregion

        #region RemoveHeader()
        private void RemoveHeader(string inputFile, string outputFile)
        {
            byte[] data = File.ReadAllBytes(inputFile);
            if (CalcChecksum(data, data.Length) != 0)
                throw new FileLoadException("Checksum validation failed");

            if (!ValidateFileSize(data, false, out int offset) && !ValidateFileSize(data, true, out offset))
                throw new FileLoadException("File size validation failed");

            using (FileStream stream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                stream.Write(data, offset, data.Length - offset - 4);
            }

            fileHeader = new byte[offset];
            Array.Copy(data, 0, fileHeader, 0, offset);
        }
        #endregion

        #region ValidateFileSize()
        private bool ValidateFileSize(byte[] data, bool littleEndian, out int offset)
        {
            littleEndianByteOrder = littleEndian;
            offset = 30 + Tools.GetInt16(data, 28, littleEndian);

            if (offset >= data.Length) return false;
                dbSizeOffset = offset;

            return data.Length == (offset + 4) + Tools.GetInt32(data, offset, littleEndian) + 4;
        }
        #endregion

        #region CalcChecksum()
        private uint CalcChecksum(byte[] data, int length)
        {
            return Crc32.Normal.CalcCrc32(data, 0, length);
        }
        #endregion

        #region CreateDummySatellites()
        private void CreateDummySatellites()
        {
            for (int i = 1; i <= 4; i++)
            {
                Satellite sat = new Satellite(i) { Name = "LNB " + i, OrbitalPosition = Convert.ToString(i) };
                DataRoot.Satellites.Add(i, sat);
            }
        }
        #endregion

        #region InitCharacterEncoding()
        private void InitCharacterEncoding(SQLiteCommand cmd)
        {
            cmd.CommandText = "PRAGMA encoding";
            charEncoding = cmd.ExecuteScalar() as string;
        }
        #endregion

        #region RepairCorruptedDatabaseImage()
        private void RepairCorruptedDatabaseImage(SQLiteCommand cmd)
        {
            cmd.CommandText = "REINDEX";
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region ReadChannels()
        private void ReadChannels(SQLiteCommand cmd)
        {
            string[] fieldNames = {
                                        "rowid", "major_channel", "physical_ch","sname", "freq", "skip", "running_status","free_CA_mode","child_lock",
                                        "profile1index","profile2index","profile3index","profile4index","stype", "onid", "tsid", "sid", "ntype", "ya_svcid",
                                        "delivery", "delivery_type"
                                    };

            string sql = string.Join(" ",
                                "SELECT",
                                "s.rowid, s.major_channel, s.physical_ch, cast(s.sname as blob), t.freq,s.skip, s.running_status,",
                                "s.free_CA_mode, s.child_lock, s.profile1index, s.profile2index, s.profile3index, s.profile4index, s.stype,",
                                "s.onid, s.tsid, s.svcid, s.ntype, s.ya_svcid, t.delivery, t.delivery_type",
                                "FROM SVL s",
                                "LEFT OUTER JOIN TSL t ON s.physical_ch = t.physical_ch and s.onid = t.onid and s.tsid = t.tsid",
                                "ORDER BY s.ntype, major_channel");

            var fields = GetFieldMap(fieldNames);

            cmd.CommandText = sql;
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    ChannelInfo channel = new DbChannel(r, fields, DataRoot, DefaultEncoding);
                    if (!channel.IsDeleted)
                    {
                        ChannelList channelList = DataRoot.GetChannelList(channel.SignalSource);

                        if (channelList != null)
                          DataRoot.AddChannel(channelList, channel);
                    }
                }
            }
        }
        #endregion

        #region GetFieldMap()
        private IDictionary<string, int> GetFieldMap(string[] fieldNames)
        {
            Dictionary<string, int> field = new Dictionary<string, int>();

            for (int i = 0; i < fieldNames.Length; i++)
                field[fieldNames[i]] = i;

            return field;
        }
        #endregion

        #region DefaultEncoding
        public override Encoding DefaultEncoding
        {
            get => base.DefaultEncoding;
            set
            {
                base.DefaultEncoding = value;
                foreach (ChannelList list in DataRoot.ChannelLists)
                {
                    foreach(var channel in list.Channels)
                        channel.ChangeEncoding(value);
                }
            }
        }
        #endregion

        #region Save()
        public override void Save(string tvOutputFile)
        {
            FileName = tvOutputFile;

            using (var conn = new SQLiteConnection("Data Source=" + workFile))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    using (var trans = conn.BeginTransaction())
                    {
                        WriteChannels(cmd, avbtChannels);
                        WriteChannels(cmd, avbcChannels);
                        WriteChannels(cmd, dvbtChannels);
                        WriteChannels(cmd, dvbcChannels);
                        WriteChannels(cmd, dvbsChannels);
                        WriteChannels(cmd, satipChannels);
                        WriteChannels(cmd, freesatChannels);
                        trans.Commit();
                    }

                    RepairCorruptedDatabaseImage(cmd);
                }
            }

            WriteCypheredFile();
        }
        #endregion

        #region WriteChannels()
        private void WriteChannels(SQLiteCommand cmd, ChannelList channelList)
        {
            cmd.CommandText = "UPDATE SVL SET major_channel=@progNr, sname=@name, profile1index=@fav1, profile2index=@fav2, profile3index=@fav3, profile4index=@fav4, child_lock=@lock, skip=@skip, free_CA_mode=@encr WHERE rowid=@rowid";

            cmd.Parameters.Clear();
            cmd.Parameters.Add(new SQLiteParameter("@rowid", DbType.Int32));
            cmd.Parameters.Add(new SQLiteParameter("@progNr", DbType.Int32));
            cmd.Parameters.Add(new SQLiteParameter("@fav1", DbType.Int32));
            cmd.Parameters.Add(new SQLiteParameter("@fav2", DbType.Int32));
            cmd.Parameters.Add(new SQLiteParameter("@fav3", DbType.Int32));
            cmd.Parameters.Add(new SQLiteParameter("@fav4", DbType.Int32));
            cmd.Parameters.Add(new SQLiteParameter("@name", DbType.Binary));
            cmd.Parameters.Add(new SQLiteParameter("@lock", DbType.Int32));
            cmd.Parameters.Add(new SQLiteParameter("@skip", DbType.Int32));
            cmd.Parameters.Add(new SQLiteParameter("@encr", DbType.Int32));
            cmd.Prepare();

            foreach (ChannelInfo channelInfo in channelList.Channels)
            {
                var channel = channelInfo as DbChannel;
                if (channel == null) // skip reference list proxy channels
                    continue;
                if (channel.IsDeleted && channel.OldProgramNr >= 0)
                    continue;

                channel.UpdateRawData();

                cmd.Parameters["@rowid"].Value = channel.RecordIndex;
                cmd.Parameters["@progNr"].Value = channel.NewProgramNr;

                for (int fav = 0; fav < 4; fav++)
                    cmd.Parameters["@fav" + (fav + 1)].Value = Math.Max(0, channel.FavIndex[fav]);

                cmd.Parameters["@name"].Value = channel.RawName;
                cmd.Parameters["@lock"].Value = channel.Lock;
                cmd.Parameters["@skip"].Value = channel.Skip;
                cmd.Parameters["@encr"].Value = channel.Encrypted;
                
                cmd.ExecuteNonQuery();
            }

            // remove unassigned/deleted channels from SVL table
            foreach (ChannelInfo channel in channelList.Channels)
            {
                if (channel.IsDeleted && channel.OldProgramNr >= 0)
                {
                    cmd.CommandText = "DELETE FROM SVL WHERE rowid=@rowid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SQLiteParameter("@rowid", DbType.Int32));
                    cmd.Parameters["@rowid"].Value = channel.RecordIndex;
                    cmd.ExecuteNonQuery();

                    // remove unassigned/deleted channels from TSL table
                    cmd.CommandText = "DELETE FROM TSL WHERE physical_ch=@physical_ch and onid=@onid and tsid=@tsid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SQLiteParameter("@physical_ch", DbType.Int32));
                    cmd.Parameters["@physical_ch"].Value = channel.PhysicalChannel;
                    cmd.Parameters.Add(new SQLiteParameter("@onid", DbType.Int32));
                    cmd.Parameters["@onid"].Value = channel.OriginalNetworkId;
                    cmd.Parameters.Add(new SQLiteParameter("@tsid", DbType.Int32));
                    cmd.Parameters["@tsid"].Value = channel.TransportStreamId;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #region WriteCypheredFile()
        private void WriteCypheredFile()
        {
            switch (cypherMode)
            {
                case CypherMode.None:
                {
                    break;
                }
                case CypherMode.Encryption:
                {
                    CypherFile(workFile, FileName, true);
                    break;
                }
                case CypherMode.HeaderAndChecksum:
                {
                    WriteFileWithHeaderAndChecksum();
                    break;
                }
            }
        }
        #endregion

        #region WriteFileWithHeaderAndChecksum()
        private void WriteFileWithHeaderAndChecksum()
        {
            long workFileSize = new FileInfo(workFile).Length;
            byte[] data = new byte[fileHeader.Length + workFileSize + 4];
            Array.Copy(fileHeader, data, fileHeader.Length);

            using (FileStream stream = new FileStream(workFile, FileMode.Open, FileAccess.Read))
            {
                stream.Read(data, fileHeader.Length, (int)workFileSize);
            }

            Tools.SetInt32(data, dbSizeOffset, (int)workFileSize, littleEndianByteOrder);
            UpdateChecksum(data);

            using (FileStream stream = new FileStream(FileName, FileMode.Create, FileAccess.Write))
            {
                stream.Write(data, 0, data.Length);
            }
        }
        #endregion

        #region UpdateChecksum()
        private void UpdateChecksum(byte[] data)
        {
            uint checksum = CalcChecksum(data, data.Length - 4);
            data[data.Length - 1] = (byte)(checksum & 0xFF);
            data[data.Length - 2] = (byte)((checksum >> 8) & 0xFF);
            data[data.Length - 3] = (byte)((checksum >> 16) & 0xFF);
            data[data.Length - 4] = (byte)((checksum >> 24) & 0xFF);
        }
        #endregion

        #region GetFileInformation()
        public override string GetFileInformation()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetFileInformation());
            sb.Append("Content type: ");

            switch (GetCypherMode(FileName))
            {
                case CypherMode.None:
                {
                    sb.AppendLine("unencrypted SQLite database");
                    break;
                }
                case CypherMode.Encryption:
                {
                    sb.AppendLine("encrypted SQLite database");
                    break;
                }
                case CypherMode.HeaderAndChecksum:
                {
                    sb.AppendLine("embedded SQLite database");
                    sb.Append("Byte order: ").AppendLine(littleEndianByteOrder ? "little-endian (least significant byte first)" : "big-endian (most significant byte first)");
                    break;
                }
            }

            sb.Append("Character encoding: ").AppendLine(charEncoding);

            return sb.ToString();
        }
        #endregion
    }
}