using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ChanSort.Api
{
    public class LookupData
    {
        private readonly IDictionary<int, NetworkInfo> networks = new Dictionary<int, NetworkInfo>();
        private readonly IDictionary<int, int> transponderNrByFreqInMhz = new Dictionary<int, int>();
        private readonly IDictionary<int, string> serviceTypeDescriptions = new Dictionary<int, string>();
        private readonly IDictionary<int, string> dvbcChannels = new ConcurrentDictionary<int, string>();

        public static readonly LookupData Instance = new LookupData();

        private LookupData()
        {
          LoadDataFromCsvFile();
        }

        #region GetNetwork()
        public NetworkInfo GetNetwork(int networkId)
        {
            networks.TryGetValue((ushort)networkId, out var network);
            return network;
        }
        #endregion

        #region GetAstraTransponder(), GetAstraFrequency()
        public int GetAstraTransponder(int frequencyInMhz)
        {
            bool found = transponderNrByFreqInMhz.TryGetValue(frequencyInMhz, out var number) ||
            transponderNrByFreqInMhz.TryGetValue(frequencyInMhz - 1, out number) ||
            transponderNrByFreqInMhz.TryGetValue(frequencyInMhz + 1, out number) ||
            transponderNrByFreqInMhz.TryGetValue(frequencyInMhz - 2, out number) ||
            transponderNrByFreqInMhz.TryGetValue(frequencyInMhz + 2, out number);
            return found ? number : 0;
        }

        public int GetAstraFrequency(int transponderNr)
        {
            return transponderNrByFreqInMhz.TryGet(transponderNr);
        }
        #endregion

        #region GetServiceTypeDescription()
        public string GetServiceTypeDescription(int serviceType)
        {
            return serviceTypeDescriptions.TryGetValue(serviceType, out var descr) ? descr : serviceType.ToString();
        }
        #endregion

        #region LoadDataFromCsvFile()
        public void LoadDataFromCsvFile()
        {
            networks.Clear();
            transponderNrByFreqInMhz.Clear();
            serviceTypeDescriptions.Clear();

            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "lookup.csv");

            if (!File.Exists(file))
                return;

            using (var reader = new StreamReader(file, System.Text.Encoding.UTF8))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    var fields = CsvFile.Parse(line, ';');
                    if (fields.Count == 0)
                        continue;

                    switch (fields[0].ToLower())
                    {
                        case "onid":
                        {
                            ParseNetwork(fields);
                            break;
                        }
                        case "transp":
                        {
                            ParseTransponder(fields);
                            break;
                        }
                        case "dvbc":
                        {
                            ParseDvbcChannel(fields);
                            break;
                        }
                        case "servicetype":
                        {
                            ParseServiceType(fields);
                            break;
                        }
                    }
                }
            }
        }
        #endregion

        #region AddNetwork()
        private void AddNetwork(NetworkInfo network)
        {
            networks[network.OriginalNetworkId] = network;
        }
        #endregion

        #region AddTransponderMapping()
        private void AddTransponderMapping(int transponderNr, int frequencyInMhz)
        {
            transponderNrByFreqInMhz[frequencyInMhz] = transponderNr;
        }
        #endregion

        #region AddServiceType()
        public void AddServiceType(int serviceType, string description)
        {
            serviceTypeDescriptions[serviceType] = description;
        }
        #endregion

        #region ParseNetwork()
        private void ParseNetwork(IList<string> fields)
        {
            if (fields.Count < 3) 
                return;

            int start = ParseNumber(fields[1]);
            int end = ParseNumber(fields[2]);

            if (start == 0 || end == 0 || start > end)
                return;

            for (int onid = start; onid <= end; onid++)
            {
                var network = new NetworkInfo { OriginalNetworkId = onid };

                if (fields.Count >= 4)
                    network.Name = fields[3];

                if (fields.Count >= 5)
                    network.Operator = fields[4];

                AddNetwork(network);
            }        
        }
        #endregion

        #region ParseNumber()
        private int ParseNumber(string nr)
        {
            int number;

            if (nr.StartsWith("0x"))
                int.TryParse(nr.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.InvariantInfo, out number);
            else
                int.TryParse(nr, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out number);

            return number;
        }
        #endregion

        #region ParseTransponder()
        private void ParseTransponder(IList<string> fields)
        {
            if (fields.Count < 3)
                return;

            int.TryParse(fields[1], out var nr);
            int.TryParse(fields[2], out var freq);

            if (nr == 0 || freq == 0)
                return;

            AddTransponderMapping(nr, freq);
        }
        #endregion

        #region ParseDvbcChannel()
        private void ParseDvbcChannel(IList<string> fields)
        {
            if (fields.Count < 3)
                return;

            int.TryParse(fields[1], out var freq);

            if (freq == 0)
                return;

            dvbcChannels[freq] = fields[2];
        }
        #endregion

        #region ParseServiceType()
        private void ParseServiceType(IList<string> fields)
        {
            if (fields.Count < 3) return;
                int serviceType = ParseNumber(fields[1]);
            if (serviceType <= 0) return;
                AddServiceType(serviceType, fields[2]);
        }
        #endregion

        #region IsRadioTvOrData()
        public SignalSource IsRadioTvOrData(int dvbServiceType)
        {
            switch (dvbServiceType)
            {
                case 0x01: // SD MPEG1
                case 0x11: // MPEG2-HD
                case 0x16: // H264/AVC-SD
                case 0x19: // H264/AVC-HD
                case 0x1F: // UHD (future use)
                case 0x9F: // UHD (user defined)
                    return SignalSource.TV;
                case 0x02:
                case 0x0A:
                    return SignalSource.Radio;
            }

            return SignalSource.Data;
        }
        #endregion

        #region GetDVBT()
        public int GetDvbtTransponder(decimal freqInMhz)
        {
            return (int)(freqInMhz - 306)/8;
        }
        public decimal GetDvbtFrequency(int channelTransponder)
        {
            return channelTransponder * 8 + 306;
        }
        #endregion

        #region GetDVBC()
        public string GetDvbcTransponder(decimal freqInMhz)
        {
            return GetDvbcChannelName(freqInMhz);
        }
        public string GetDvbcChannelName(decimal freqInMhz)
        {
            if (freqInMhz > 2000)
                freqInMhz /= 1000;
            if (freqInMhz > 2000)
                freqInMhz /= 1000;

            string strCurrentDvbcChannel = dvbcChannels.TryGet((int)(freqInMhz * 1000)) ?? dvbcChannels.TryGet((int)((freqInMhz - 1) * 1000)) ?? string.Empty;

            if (!string.IsNullOrEmpty(strCurrentDvbcChannel))
                return strCurrentDvbcChannel;

            return Convert.ToString((int)(freqInMhz - 106) / 8);
        }
        public decimal GetDvbcFrequency(int channelTransponder)
        {
            return channelTransponder * 8 + 106;
        }
        public decimal GetDvbcFrequency(string channelTransponder)
        {
            return Convert.ToDecimal(dvbcChannels.FirstOrDefault(x => x.Value == channelTransponder).Key);
        }
        #endregion
    }
}