﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChanSort.Api
{
    /// <summary>
    ///   Reads a reference list from a .csv file with the format
    ///   [obsolete],ProgramNr,[obsolete],UID,ChannelName[,SignalSource,FavAndFlags]
    /// </summary>
    public class CsvRefListSerializer : SerializerBase
    {
        private static readonly List<string> Columns = new List<string>
        {
            "OldPosition",
            "Position",
            "Name",
            "OriginalNetworkId",
            "TransportStreamId",
            "ServiceId",
            "Favorites",
            "Skip",
            "Lock",
            "Hidden"
        };

        #region ctor()
        public CsvRefListSerializer(string fileName) : base(fileName)
        {
            Features.ChannelNameEdit = ChannelNameEditMode.All;
            Features.CanSkipChannels = true;
            Features.CanLockChannels = true;
            Features.CanHideChannels = true;
            Features.DeleteMode = DeleteMode.FlagWithoutPrNr;
            Features.CanHaveGaps = true;
            Features.EncryptedFlagEdit = false;
            Features.SortedFavorites = false;
            Features.SupportedFavorites = Favorites.A | Favorites.B | Favorites.C | Favorites.D | Favorites.E | Favorites.F | Favorites.G | Favorites.H;
        }
        #endregion

        #region Load()
        public override void Load()
        {
            using (var stream = new StreamReader(FileName))
            {
                var lineNr = 0;
                var line = "";

                try
                {
                    while ((line = stream.ReadLine()) != null)
                    {
                        ReadChannel(line, ++lineNr);
                    }
                }
                catch (Exception ex)
                {
                    throw new FileLoadException($"Error in reference file line #{lineNr}: {line}", ex);
                }
            }
        }
        #endregion

        #region ReadChannel()
        private void ReadChannel(string line, int lineNr)
        {
            var parts = CsvFile.Parse(line, ',');

            if (parts.Count < 5) 
                return;

            if (!int.TryParse(parts[1], out var programNr))
                return;

            var uid = parts[3];
            
            if (uid.StartsWith("S")) // remove satellite orbital position from UID ... not all TV models provide this information
                uid = "S" + uid.Substring(uid.IndexOf('-'));

            var signalSource = GetSignalSource(ref programNr, uid, parts);
            
            if (signalSource == 0)
                return;

            var channelList = GetChannelList(signalSource);
            
            if (channelList == null)
                return;

            var name = parts[4];
            var channel = new ChannelInfo(signalSource, lineNr, programNr, name);

            var uidParts = uid.Split('-');
            
            if (uidParts.Length >= 4)
            {
                if (int.TryParse(uidParts[1], out var val))
                    channel.OriginalNetworkId = val;

                if (int.TryParse(uidParts[2], out val))
                    channel.TransportStreamId = val;

                if (int.TryParse(uidParts[3], out val))
                    channel.ServiceId = val;
            }

            if (parts.Count >= 7)
                ApplyFlags(channel, parts[6]);

            DataRoot.AddChannel(channelList, channel);
        }
        #endregion

        #region GetSignalSource()
        private static SignalSource GetSignalSource(ref int slot, string uid, IList<string> parts)
        {
            // new lists store a bitmask which defines the type of channel and list it came from
            if (parts.Count >= 6 && parts[5].Length >= 4)
            {
                SignalSource s = SignalSource.Any;
                var code = parts[5];

                if (code[0] == 'A')
                    FlagsHelper.Set(ref s, SignalSource.Analog);
                else if (code[0] == 'D')
                    FlagsHelper.Set(ref s, SignalSource.Digital);

                if (code[1] == 'A')
                    FlagsHelper.Set(ref s, SignalSource.Antenna);
                else if (code[1] == 'C')
                    FlagsHelper.Set(ref s, SignalSource.Cable);
                else if (code[1] == 'S')
                    FlagsHelper.Set(ref s, SignalSource.Sat);
                else if (code[1] == 'I')
                    FlagsHelper.Set(ref s, SignalSource.IP);

                if (code[2] == 'T')
                    FlagsHelper.Set(ref s, SignalSource.TV);
                else if (code[2] == 'R')
                    FlagsHelper.Set(ref s, SignalSource.Radio);

                s |= (SignalSource) (int.Parse(code.Substring(3)) << 12);
                return s;
            }

            // compatibility for older lists
            var isTv = slot < 0x4000;
            slot &= 0x3FFFF;
            SignalSource signalSource = SignalSource.Any;

            switch (uid[0])
            {
                case 'S':
                    FlagsHelper.Set(ref signalSource, SignalSource.DVBS);
                    break;
                case 'C':
                    FlagsHelper.Set(ref signalSource, SignalSource.DVBT);
                    FlagsHelper.Set(ref signalSource, SignalSource.DVBC);
                    break;
                case 'A':
                    FlagsHelper.Set(ref signalSource, SignalSource.AnalogAntenna | SignalSource.AnalogCable);
                    break;
                case 'H':
                    FlagsHelper.Set(ref signalSource, SignalSource.Preset_AstraHdPlus);
                    break;
                default:
                    return 0;
            }

            if (isTv)
                FlagsHelper.Set(ref signalSource, SignalSource.TV);
            else
                FlagsHelper.Set(ref signalSource, SignalSource.Radio);

            return signalSource;
        }
        #endregion

        #region GetChannelList()
        private ChannelList GetChannelList(SignalSource signalSource)
        {
            var channelList = DataRoot.GetChannelList(signalSource);

            if (channelList == null)
            {
                channelList = new ChannelList(signalSource, CreateCaption(signalSource)) { VisibleColumnFieldNames = Columns };
                DataRoot.AddChannelList(channelList);
            }

            return channelList;
        }
        #endregion

        #region CreateCaption()
        private string CreateCaption(SignalSource signalSource)
        {
            var sb = new StringBuilder();

            if (FlagsHelper.IsSet(signalSource, SignalSource.DVBT))
                sb.Append("DVB-T");
            else if (FlagsHelper.IsSet(signalSource, SignalSource.DVBC))
                sb.Append("DVB-C");
            else if (FlagsHelper.IsSet(signalSource, SignalSource.DVBS))
                sb.Append("DVB-S");
            else if (FlagsHelper.IsSet(signalSource, SignalSource.IP))
                sb.Append("IP");
            else if (FlagsHelper.IsSet(signalSource, SignalSource.Analog))
                sb.Append("Analog");

            sb.Append(" ");

            if (FlagsHelper.IsSet(signalSource, SignalSource.TV))
                sb.Append("TV");
            else if (FlagsHelper.IsSet(signalSource, SignalSource.Radio))
                sb.Append("Radio");
            else
                sb.Append("Data");

            return sb.ToString();
        }
        #endregion

        #region ApplyFlags()
        private void ApplyFlags(ChannelInfo channel, string flags)
        {
            channel.Lock = false;
            channel.Skip = false;
            channel.Hidden = false;
            channel.IsDeleted = false;
            channel.Favorites = 0;

            foreach (var c in flags)
            {
                switch (c)
                {
                    case '1':
                        channel.Favorites |= Favorites.A;
                        break;
                    case '2':
                        channel.Favorites |= Favorites.B;
                        break;
                    case '3':
                        channel.Favorites |= Favorites.C;
                        break;
                    case '4':
                        channel.Favorites |= Favorites.D;
                        break;
                    case '5':
                        channel.Favorites |= Favorites.E;
                        break;
                    case '6':
                        channel.Favorites |= Favorites.F;
                        break;
                    case '7':
                        channel.Favorites |= Favorites.G;
                        break;
                    case '8':
                        channel.Favorites |= Favorites.H;
                        break;
                    case 'L':
                        channel.Lock = true;
                        break;
                    case 'S':
                        channel.Skip = true;
                        break;
                    case 'H':
                        channel.Hidden = true;
                        break;
                    case 'D':
                        channel.IsDeleted = true;
                        channel.NewProgramNr = -1;
                        break;
                }
            }
        }
        #endregion

        #region EncodeSignalSource()
        private static string EncodeSignalSource(SignalSource signalSource)
        {
            var sb = new StringBuilder();

            sb.Append((signalSource & SignalSource.Analog) != 0 ? 'A' : 'D');

            if ((signalSource & SignalSource.Antenna) != 0)
                sb.Append('A');
            else if ((signalSource & SignalSource.Cable) != 0)
                sb.Append('C');
            else if ((signalSource & SignalSource.Sat) != 0)
                sb.Append('S');
            else
                sb.Append("I");

            sb.Append((signalSource & SignalSource.Radio) != 0 ? 'R' : 'T');
            sb.Append((int) signalSource >> 12);

            return sb.ToString();
        }
        #endregion

        #region EncodeFavoritesAndFlags()
        private static string EncodeFavoritesAndFlags(ChannelInfo channel)
        {
            var sb = new StringBuilder();

            if ((channel.Favorites & Favorites.A) != 0)
                sb.Append('1');

            if ((channel.Favorites & Favorites.B) != 0)
                sb.Append('2');

            if ((channel.Favorites & Favorites.C) != 0)
                sb.Append('3');

            if ((channel.Favorites & Favorites.D) != 0) 
                sb.Append('4');

            if ((channel.Favorites & Favorites.E) != 0) 
                sb.Append('5');

            if ((channel.Favorites & Favorites.F) != 0) 
                sb.Append('6');

            if ((channel.Favorites & Favorites.G) != 0) 
                sb.Append('7');

            if ((channel.Favorites & Favorites.H) != 0) 
                sb.Append('8');

            if (channel.Lock) 
                sb.Append('L');

            if (channel.Skip) 
                sb.Append('S');

            if (channel.Hidden) 
                sb.Append('H');

            if (channel.IsDeleted)
                sb.Append('D');

            return sb.ToString();
        }
        #endregion

        #region Save()
        public override void Save(string tvDataFile)
        {
            Save(tvDataFile, DataRoot);
            FileName = tvDataFile;
        }

        public static void Save(string tvDataFile, DataRoot dataRoot)
        {
            using (var stream = new StreamWriter(tvDataFile))
            {
                Save(stream, dataRoot);
            }
        }

        public static void Save(TextWriter stream, DataRoot dataRoot, bool includeDeletedChannels = true)
        {
            foreach (var channelList in dataRoot.ChannelLists)
            {
                if (channelList.IsMixedSourceFavoritesList) // these pseudo-lists would create dupes for all channels
                    continue;

                foreach (var channel in channelList.GetChannelsByNewOrder())
                {
                    if (channel.NewProgramNr == -1 && !includeDeletedChannels)
                        continue;

                    var line = $",{channel.NewProgramNr},,{channel.Uid},\"{channel.Name}\",{EncodeSignalSource(channel.SignalSource)},{EncodeFavoritesAndFlags(channel)}";

                    stream.WriteLine(line);
                }
            }
        }
        #endregion
    }
}