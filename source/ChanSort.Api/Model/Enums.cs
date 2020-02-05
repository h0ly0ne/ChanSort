using System;

namespace ChanSort.Api
{
    #region enum SignalSource
    /// <summary>
    /// Bitmask for channel and list classification.
    /// An individual channel can only have one bit of each group set.
    /// A ChannelList can have multiple bits set to indicate which type of channels it can hold.
    /// </summary>
    [Flags]
    public enum SignalSource
    {
        // 0x0000 // 0000000000000000
        Any = 0,
        // 0x0001 // 0000000000000001
        Analog = 1,
        // 0x0002 // 0000000000000010
        Digital = 2,
        // 0x0004 // 0000000000000100
        // ????
        // 0x0008 // 0000000000001000
        AvInput = 8,
        // 0x0010 // 0000000000010000
        Antenna = 16,
        // 0x0020 // 0000000000100000
        Cable = 32,
        // 0x0040 // 0000000001000000
        Sat = 64,
        // 0x0080 // 0000000010000000
        IP = 128,
        // 0x0011 // 0000000000010001
        AnalogAntenna = Analog | Antenna,                                                       // 17
        // 0x0021 // 0000000000100001
        AnalogCable = Analog | Cable,                                                           // 33
        // 0x0031 // 0000000000110001
        AnalogAntennaCable = AnalogAntenna | AnalogCable,                                       // 49
        // 0x0041 // 0000000001000001
        AnalogSat = Analog | Sat,                                                               // 65
        // 0x0012 // 0000000000010010
        DVBT = Digital | Antenna,                                                               // 18
        // 0x0022 // 0000000000100010
        DVBC = Digital | Cable,                                                                 // 34
        // 0x0032 // 0000000000110010
        DVBTC = DVBT | DVBC,                                                                    // 50
        // 0x0042 // 0000000001000010
        DVBS = Digital | Sat,                                                                   // 66
        // 0x0072 // 0000000001110010
        DVBAll = DVBTC | DVBS,                                                                  // 114
        // 0x0092 // 0000000010010010
        DVBIPAntenna = DVBT | IP,                                                               // 146
        // 0x00A2 // 0000000010100010
        DVBIPCable = DVBC | IP,                                                                 // 162
        // 0x00C2 // 0000000011000010
        DVBIPSat = DVBS | IP,                                                                   // 194
        // 0x0100 // 0000000100000000
        TV = 256,
        // 0x0200 // 0000001000000000
        Radio = 512,
        // 0x0400 // 0000010000000000
        Data = 1024,
        // 0x0300 // 0000001100000000
        TVAndRadio = TV | Radio,                                                                // 768
        // 0x0500 // 0000010100000000
        TVAndData = TV | Data,                                                                  // 1280
        // 0x0600 // 0000011000000000
        RadioAndData = Radio | Data,                                                            // 1536
        // 0x0700 // 0000011100000000
        TVAndRadioAndData = TV | Radio | Data,                                                  // 1792

        // 0x00FB // 0000000011111011
        AllAnalogDigitalInput = Analog | Digital | AvInput | Antenna | Cable | Sat | IP,        // 251
        // 0x07FB // 0000011111111011
        All = Analog | Digital | AvInput | Antenna | Cable | Sat | IP | TV | Radio | Data,      // 2043

        // 0x2000 // ‭0010000000000000‬
        Preset_Freesat = 8192,
        // 0x3000 // 0011000000000000
        Preset_TivuSat = 12288,
        // 0x1000 // ‭0001000000000000
        Preset_AstraHdPlus = 4096,
        // 0x1000 // ‭0001000000000000‬
        Preset_CablePrime = Preset_AstraHdPlus,
        // 0x4000 // ‭0100000000000000
        Preset_CanalDigital = 16384,
        // 0x5000 // ‭0101000000000000‬
        Preset_DigitalPlus = 20480,
        // 0x6000 // ‭0110000000000000‬
        Preset_CyfraPlus = 24576,

        // 0x1042 // 0001000001000010
        Preset_Samsung_HdPlusD = DVBS | Preset_AstraHdPlus,                                     // 4162
        // 0x1042 // 0001000001000010
        Preset_Samsung_CablePrimeD = DVBC | Preset_AstraHdPlus,                                 // 4162
        // 0x2042 // 0010000001000010
        Preset_Samsung_FreesatD = DVBS | Preset_Freesat,                                        // 8258
        // 0x3042 // 0011000001000010
        Preset_Samsung_TivuSatD = DVBS | Preset_TivuSat,                                        // 12354
        // 0x4042 // 0100000001000010
        Preset_Samsung_CanalDigitalSatD = DVBS | Preset_CanalDigital,                           // 16450
        // 0x5042 // 0101000001000010
        Preset_Samsung_DigitalPlusD = DVBS | Preset_DigitalPlus,                                // 20546
        // 0x6042 // 0110000001000010
        Preset_Samsung_CyfraPlusD = DVBS + Preset_CyfraPlus,                                    // 24642

        // 0x0000 // 0000000000000000
        Preset_Sony_Provider0 = Any,
        // 0x1000 // ‭0001000000000000‬
        Preset_Sony_Provider1 = Preset_AstraHdPlus,
        // 0x2000 // ‭0010000000000000‬
        Preset_Sony_Provider2 = Preset_Freesat,

        // 0xF000 // 1111000000000000
        AllProvider = 61440,
    }
    #endregion

    [Flags]
    public enum Favorites : byte
    {
        // 0x01 // 00000001
        A = 1,
        // 0x02 // 00000010
        B = 2,
        // 0x04 // 00000100
        C = 4,
        // 0x08 // 00001000
        D = 8,
        // 0x10 // 00010000
        E = 16,
        // 0x20 // 00100000
        F = 32,
        // 0x40 // 01000000
        G = 64,
        // 0x80 // 10000000
        H = 128
    }

    [Flags]
    public enum UnsortedChannelMode
    {
        // 0x00 // 0000
        AppendInOrder = 0,
        // 0x01 // 0001
        AppendAlphabetically = 1,
        // 0x02 // 0010
        Delete = 2
    }

    [Flags]
    public enum ChannelNameEditMode
    {
        // 0x00 // 0000
        None = 0,
        // 0x01 // 0001
        Analog = 1,
        // 0x02 // 0010
        Digital = 2,
        // 0x03 // 0011
        All = Analog | Digital // 3
    }
}
