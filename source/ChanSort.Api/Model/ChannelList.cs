using System;
using System.Collections.Generic;
using System.Linq;

namespace ChanSort.Api
{
    public class ChannelList
    {
        private readonly Dictionary<string, IList<ChannelInfo>> channelByUid = new Dictionary<string, IList<ChannelInfo>>();
        private readonly Dictionary<int, ChannelInfo> channelByProgNr = new Dictionary<int, ChannelInfo>();
        private readonly Dictionary<string, IList<ChannelInfo>> channelByName = new Dictionary<string, IList<ChannelInfo>>();
        private int insertProgramNr = 1;
        private int duplicateUidCount;
        private int duplicateProgNrCount;

        public static List<string> DefaultVisibleColumns { get; set; } = new List<string>(); // initialized by MainForm

        public ChannelList(SignalSource source, string caption)
        {
            if (!FlagsHelper.IsSet(source, SignalSource.Analog | SignalSource.Digital))
                FlagsHelper.Set(source, SignalSource.Analog | SignalSource.Digital);
            if (!FlagsHelper.IsSet(source, SignalSource.Antenna | SignalSource.Cable | SignalSource.Sat))
                FlagsHelper.Set(source, SignalSource.Antenna | SignalSource.Cable | SignalSource.Sat);
            if (!FlagsHelper.IsSet(source, SignalSource.TVAndRadioAndData))
                FlagsHelper.Set(source, SignalSource.TVAndRadioAndData);

            SignalSource = source;
            ShortCaption = caption;
            FirstProgramNumber = (source & SignalSource.Digital) != 0 ? 1 : 0;
            VisibleColumnFieldNames = DefaultVisibleColumns.ToList(); // create copy of default list, so it can be modified
        }

        public string ShortCaption { get; set; }
        public SignalSource SignalSource { get; }
        public IList<ChannelInfo> Channels { get; } = new List<ChannelInfo>();
        public int Count => Channels.Count;
        public int DuplicateUidCount => duplicateUidCount;
        public int DuplicateProgNrCount => duplicateProgNrCount;
        public bool ReadOnly { get; set; }
        public int MaxChannelNameLength { get; set; }
        public int PresetProgramNrCount { get; private set; }
        public IList<string> VisibleColumnFieldNames;

        /// <summary>
        /// Set for helper lists used to manage favorites mixed from all input sources.
        /// When true, the UI won't show the "Pr#" tab but will show "Fav A-D" tabs and a "Source" column.
        /// </summary>
        public bool IsMixedSourceFavoritesList { get; set; }

        #region Caption
        public string Caption
        {
            get 
            { 
                string cap = ShortCaption;
                int validChannelCount = Channels.Count(ch => ch.OldProgramNr != -1);

                return cap + " (" + validChannelCount + ")";
            }
        }
        #endregion

        #region InsertProgramNumber
        public int InsertProgramNumber
        {
            get => Count == 0 ? 1 : insertProgramNr;
            set => insertProgramNr = Math.Max(FirstProgramNumber, value);
        }
        #endregion

        public int FirstProgramNumber { get; set; }

        #region AddChannel()
        public string AddChannel(ChannelInfo ci)
        {
            if (channelByUid.TryGetValue(ci.Uid, out var others))
                ++duplicateUidCount;
            else
            {
                others = new List<ChannelInfo>();
                channelByUid.Add(ci.Uid, others);
            }

            others.Add(ci);

            string warning2 = null;
            bool isDupeProgNr = false;

            if (ci.OldProgramNr != -1)
            {
                channelByProgNr.TryGetValue(ci.OldProgramNr, out var other);

                if (other != null)
                {
                    warning2 = string.Format(Resources.ChannelList_ProgramNrAssignedToMultipleChannels, ShortCaption, ci.OldProgramNr, other.RecordIndex, other.Name, ci.RecordIndex, ci.Name);
                    ++duplicateProgNrCount;
                    isDupeProgNr = true;
                }
            }

            if (!isDupeProgNr)
                channelByProgNr[ci.OldProgramNr] = ci;

            var lowerName = (ci.Name ?? "").ToLower().Trim();
            var byNameList = channelByName.TryGet(lowerName);

            if (byNameList == null)
            {
                byNameList = new List<ChannelInfo>();
                channelByName[lowerName] = byNameList;
            }

            byNameList.Add(ci);

            if (ci.ProgramNrPreset != 0)
                ++PresetProgramNrCount;

            Channels.Add(ci);

            return warning2;
        }
        #endregion

        #region GetChannelByUid()
        public IList<ChannelInfo> GetChannelByUid(string uid)
        {
            channelByUid.TryGetValue(uid, out var channel);

            return channel ?? new List<ChannelInfo>(0);
        }
        #endregion

        #region ToString()
        public override string ToString()
        {
            return Caption;
        }
        #endregion

        #region GetChannelByName()
        public IEnumerable<ChannelInfo> GetChannelByName(string name)
        {
            var hits = channelByName.TryGet(name.ToLower().Trim());
            return hits ?? new List<ChannelInfo>();
        }
        #endregion

        #region GetChannelByNewProgNr()
        public IList<ChannelInfo> GetChannelByNewProgNr(int newProgNr)
        {
            return Channels.Where(c => c.NewProgramNr == newProgNr).ToList();
        }
        #endregion

        #region GetChannelsByNewOrder()
        public IList<ChannelInfo> GetChannelsByNewOrder()
        {
            return Channels.OrderBy(c => c.NewProgramNr).ToList();
        }
        #endregion

        #region RemoveChannel()
        public void RemoveChannel(ChannelInfo channel)
        {
            Channels.Remove(channel);
            var list = channelByUid.TryGet(channel.Uid);

            if (list != null && list.Contains(channel))
                list.Remove(channel);

            list = channelByName.TryGet(channel.Name);

            if (list != null && list.Contains(channel))
                list.Remove(channel);

            var chan = channelByProgNr.TryGet(channel.OldProgramNr);

            if (ReferenceEquals(chan, channel))
                channelByProgNr.Remove(channel.OldProgramNr);
        }
        #endregion
    }
}