using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChanSort.Api
{
    public class DataRoot
    {
        private readonly IList<ChannelList> channelLists = new List<ChannelList>();
        private readonly SerializerBase loader;

        public StringBuilder Warnings { get; } = new StringBuilder();
        public IDictionary<int, Satellite> Satellites { get; } = new Dictionary<int, Satellite>();
        public IDictionary<int, Transponder> Transponder { get; } = new Dictionary<int, Transponder>();
        public IDictionary<int, LnbConfig> LnbConfig { get; } = new Dictionary<int, LnbConfig>();
        public IEnumerable<ChannelList> ChannelLists => channelLists;
        public bool IsEmpty => channelLists.Count == 0;
        public bool NeedsSaving { get; set; }

        public Favorites SupportedFavorites => loader.Features.SupportedFavorites;
        public bool SortedFavorites => loader.Features.SortedFavorites;
        public bool MixedSourceFavorites => loader.Features.MixedSourceFavorites;
        public bool AllowGapsInFavNumbers => loader.Features.AllowGapsInFavNumbers;
        public bool DeletedChannelsNeedNumbers => loader.Features.DeleteMode == SerializerBase.DeleteMode.FlagWithPrNr;
        public bool CanSkip => loader.Features.CanSkipChannels;
        public bool CanLock => loader.Features.CanLockChannels;
        public bool CanHide => loader.Features.CanHideChannels;

        public DataRoot(SerializerBase sbCurrentLoader)
        {
            loader = sbCurrentLoader;
        }

        #region AddSatellite()
        public virtual void AddSatellite(Satellite satellite)
        {
            Satellites.Add(satellite.Id, satellite);
        }
        #endregion

        #region AddTransponder()
        public virtual void AddTransponder(Satellite sat, Transponder trans)
        {
            trans.Satellite = sat;

            if (Transponder.ContainsKey(trans.Id))
            {
                Warnings.AppendFormat("Duplicate transponder data record for satellite #{0} with id {1}\r\n", sat?.Id, trans.Id);
                return;
            }

            sat?.Transponder.Add(trans.Id, trans);
            Transponder.Add(trans.Id, trans);
        }
        #endregion

        #region AddLnbConfig()
        public void AddLnbConfig(LnbConfig lnb)
        {
            LnbConfig.Add(lnb.Id, lnb);
        }
        #endregion

        #region AddChannelList()
        public virtual void AddChannelList(ChannelList list)
        {
            channelLists.Add(list);
            loader.Features.MixedSourceFavorites |= list.IsMixedSourceFavoritesList;
        }
        #endregion

        #region AddChannel()
        public virtual void AddChannel(ChannelList list, ChannelInfo channel)
        {
            if (list == null)
            {
                Warnings.AppendFormat("No list found to add channel '{0}'\r\n", channel);
                return;
            }

            string warning = list.AddChannel(channel);

            if (warning != null)
                Warnings.AppendLine(warning);
        }
        #endregion

        #region GetChannelList()
        public ChannelList GetChannelList(SignalSource searchMask)
        {
            foreach (ChannelList list in channelLists)
            {
                if (FlagsHelper.IsSet(searchMask, SignalSource.Analog | SignalSource.Digital) && !FlagsHelper.IsSet(list.SignalSource, searchMask & (SignalSource.Analog | SignalSource.Digital)))
                    continue;
                if (FlagsHelper.IsSet(searchMask, SignalSource.AvInput | SignalSource.Antenna | SignalSource.Cable | SignalSource.Sat) && !FlagsHelper.IsSet(list.SignalSource, searchMask & (SignalSource.AvInput | SignalSource.Antenna | SignalSource.Cable | SignalSource.Sat)))
                    continue;
                if (FlagsHelper.IsSet(searchMask, SignalSource.IP) && !FlagsHelper.IsSet(list.SignalSource, searchMask & SignalSource.IP))
                    continue;
                if (FlagsHelper.IsSet(searchMask, SignalSource.TVAndRadioAndData) && !FlagsHelper.IsSet(list.SignalSource, searchMask & SignalSource.TVAndRadioAndData))
                    continue;
                if (FlagsHelper.IsSet(searchMask, SignalSource.AllProvider) && !FlagsHelper.IsSet(list.SignalSource, searchMask & SignalSource.AllProvider))
                    continue;

                return list;
            }

            return null;
        }
        #endregion

        #region ValidateAfterLoad()
        public virtual void ValidateAfterLoad()
        {
            foreach (var list in ChannelLists)
            {
                if (list.IsMixedSourceFavoritesList)
                  continue;

                // make sure that deleted channels have OldProgramNr = -1
                bool hasPolarity = false;
                foreach (var chan in list.Channels)
                {
                    if (chan.IsDeleted)
                        chan.OldProgramNr = -1;
                    else
                    {
                        if (chan.OldProgramNr < 0) // old versions of ChanSort saved -1 and without setting IsDeleted
                          chan.IsDeleted = true;

                        hasPolarity |= chan.Polarity == 'H' || chan.Polarity == 'V';
                    }
                }

                if (!hasPolarity)
                    list.VisibleColumnFieldNames.Remove("Polarity");
            }
        }
        #endregion

        #region ApplyCurrentProgramNumbers()
        public void ApplyCurrentProgramNumbers()
        {
            int c = 0;

            if (MixedSourceFavorites || SortedFavorites)
            {
                for (int m = (int) SupportedFavorites; m != 0; m >>= 1)
                    ++c;
            }

            foreach (var list in ChannelLists)
            {
                foreach (var channel in list.Channels)
                {
                    for (int i = 0; i <= c; i++)
                        channel.SetPosition(i, channel.GetOldPosition(i));
                }
            }
        }
        #endregion

        #region AssignNumbersToUnsortedAndDeletedChannels()
        public void AssignNumbersToUnsortedAndDeletedChannels(UnsortedChannelMode mode)
        {
            foreach (var list in ChannelLists)
            {
                if (list.IsMixedSourceFavoritesList)
                    continue;

                // sort the channels by assigned numbers, then unassigned by original order or alphabetically, then deleted channels
                var sortedChannels = list.Channels.OrderBy(ch => ChanSortCriteria(ch, mode));
                int maxProgNr = 0;

                foreach (var appChannel in sortedChannels)
                {
                    if (appChannel.IsProxy)
                        continue;

                    if (appChannel.NewProgramNr == -1)
                    {
                        if (mode == UnsortedChannelMode.Delete)
                            appChannel.IsDeleted = true;
                        else // append (hidden if possible)
                        {
                            appChannel.Hidden = true;
                            appChannel.Skip = true;
                        }

                        // assign a valid number or 0 .... because -1 will never be a valid value for the TV
                        appChannel.NewProgramNr = mode != UnsortedChannelMode.Delete || DeletedChannelsNeedNumbers ? ++maxProgNr : 0;
                    }
                    else
                    {
                        appChannel.IsDeleted = false;

                        if (appChannel.NewProgramNr > maxProgNr)
                            maxProgNr = appChannel.NewProgramNr;
                    }
                }
            }
        }
        #endregion

        #region ChanSortCriteria()
        private string ChanSortCriteria(ChannelInfo channel, UnsortedChannelMode mode)
        {
            // explicitly sorted
            var pos = channel.NewProgramNr;
            if (pos != -1)
                return pos.ToString("d5");

            // eventually hide unsorted channels
            if (mode == UnsortedChannelMode.Delete)
                return "Z" + channel.RecordIndex.ToString("d5");

            // eventually append in old order
            if (mode == UnsortedChannelMode.AppendInOrder)
                return "B" + channel.OldProgramNr.ToString("d5");

            // sort alphabetically, with "." and "" on the bottom
            if (channel.Name == ".")
                return "B";

            if (channel.Name == "")
                return "C";

            return "A" + channel.Name;
        }
        #endregion

        #region ValidateAfterSave()
        public virtual void ValidateAfterSave()
        {
            // set old numbers to match the new numbers
            // also make sure that deleted channels are either removed from the list or assigned the -1 prNr, depending on the loader's DeleteMode
            foreach (var list in ChannelLists)
            {
                for (int i = 0; i < list.Channels.Count; i++)
                {
                    var chan = list.Channels[i];

                    if (chan.IsDeleted)
                    {
                        if (loader.Features.DeleteMode == SerializerBase.DeleteMode.Physically)
                            list.Channels.RemoveAt(i--);
                        else
                            chan.NewProgramNr = -1;
                    }

                    chan.OldProgramNr = chan.NewProgramNr;
                    chan.OldFavIndex.Clear();
                    chan.OldFavIndex.AddRange(chan.FavIndex);
                }
            }
        }
        #endregion
    }
}
