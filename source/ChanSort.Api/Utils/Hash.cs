using System.Data.HashFunction.CRC;

namespace ChanSort.Api
{
    public class Hash
    {
        public static ICRC CRC32A = CRCFactory.Instance.Create(new CRCConfig
        {
            HashSizeInBits = 32,
            Polynomial = 0x04c11db7,
            InitialValue = 0xffffffff,
            ReflectIn = false,
            ReflectOut = false,
            XOrOut = 0x00000000
        });

        // CRC32 used by LG
        public static ICRC CRC32B = CRCFactory.Instance.Create(new CRCConfig
        {
            HashSizeInBits = 32,
            Polynomial = 0x04c11db7,
            InitialValue = 0xffffffff,
            ReflectIn = true,
            ReflectOut = true,
            XOrOut = 0x00000000
        });
    }
}
