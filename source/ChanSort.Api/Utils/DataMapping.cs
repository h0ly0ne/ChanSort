using System;
using System.Text;

namespace ChanSort.Api
{
    public class DataMapping
    {
        protected readonly IniFile.Section ifsCurrentIniFileSection;
        public IniFile.Section Settings => ifsCurrentIniFileSection;
        public Encoding DefaultEncoding { get; set; }
        public byte[] Data { get; set; }
        public int BaseOffset { get; set; }

        #region ctor()
        public DataMapping(IniFile.Section ifsLocalIniFileSection)
        {
            ifsCurrentIniFileSection = ifsLocalIniFileSection;
            DefaultEncoding = Encoding.Default;
        }
        #endregion

        #region SetDataPtr()
        public void SetDataPtr(byte[] baLocalByteArray, int iLocalBaseOffset)
        {
            Data = baLocalByteArray;
            BaseOffset = iLocalBaseOffset;
        }
        #endregion

        #region GetOffsets()
        public int[] GetOffsets(string key)
        {
            return ifsCurrentIniFileSection.GetIntList(key);
        }
        #endregion

        #region Byte
        public byte GetByte(string key)
        {
            int[] offsets = ifsCurrentIniFileSection.GetIntList(key);
            return offsets.Length==0 ? (byte) 0 : Data[BaseOffset + offsets[0]];
        }

        public void SetByte(string key, int value)
        {
            foreach (int offset in ifsCurrentIniFileSection.GetIntList(key))
            {
                Data[BaseOffset + offset] = (byte) value;
            }
        }
        #endregion

        #region Word
        public ushort GetWord(string key)
        {
            int[] offsets = ifsCurrentIniFileSection.GetIntList(key);
            return offsets.Length == 0 ? (ushort) 0 : BitConverter.ToUInt16(Data, BaseOffset + offsets[0]);
        }

        public void SetWord(string key, int value)
        {
            foreach (int offset in ifsCurrentIniFileSection.GetIntList(key))
            {
                Data[BaseOffset + offset + 0] = (byte)value;
                Data[BaseOffset + offset + 1] = (byte)(value>>8);
            }
        }
        #endregion

        #region DWord
        public long GetDword(string key)
        {
            int[] offsets = ifsCurrentIniFileSection.GetIntList(key);
            return offsets.Length == 0 ? 0 : BitConverter.ToUInt32(Data, BaseOffset + offsets[0]);
        }

        public void SetDword(string key, long value)
        {
            foreach (int offset in ifsCurrentIniFileSection.GetIntList(key))
            {
                Data[BaseOffset + offset + 0] = (byte)value;
                Data[BaseOffset + offset + 1] = (byte)(value >> 8);
                Data[BaseOffset + offset + 2] = (byte)(value >> 16);
                Data[BaseOffset + offset + 3] = (byte)(value >> 24);
            }
        }
        #endregion

        #region Float
        public float GetFloat(string key)
        {
            int[] offsets = ifsCurrentIniFileSection.GetIntList(key);
            return offsets.Length == 0 ? 0 : BitConverter.ToSingle(Data, BaseOffset + offsets[0]);
        }

        public void SetFloat(string key, float value)
        {
            foreach (int offset in ifsCurrentIniFileSection.GetIntList(key))
            {
                for (int i = 0; i < 4; i++)
                {
                    Data[BaseOffset + offset + i] = BitConverter.GetBytes(value)[i];
                }
            }
        }
        #endregion

        #region GetFlag
        public bool GetFlag(string key, bool defaultValue = false)
        {
            return GetFlag("off" + key, "mask" + key, defaultValue);
        }

        public bool GetFlag(string valueKey, string maskKey, bool defaultValue = false)
        {
            return GetFlag(valueKey, ifsCurrentIniFileSection.GetInt(maskKey), defaultValue);
        }

        public bool GetFlag(string valueKey, int mask, bool defaultValue = false)
        {
            if (mask == 0)
                return defaultValue;

            bool reverseLogic = false;

            if (mask < 0)
            {
                reverseLogic = true;
                mask = -mask;
            }

            int[] offsets = ifsCurrentIniFileSection.GetIntList(valueKey);
            
            if (offsets.Length == 0)
                return defaultValue;
            
            return (Data[BaseOffset + offsets[0]] & mask) == mask != reverseLogic;
        }
        #endregion

        #region SetFlag()
        public void SetFlag(string key, bool value)
        {
            SetFlag("off" + key, "mask" + key, value);
        }

        public void SetFlag(string valueKey, string maskKey, bool value)
        {
            SetFlag(valueKey, ifsCurrentIniFileSection.GetInt(maskKey), value);
        }

        public void SetFlag(string valueKey, int mask, bool value)
        {
            if (mask == 0) 
                return;
            
            bool reverseLogic = false;
           
            if (mask < 0)
            {
                reverseLogic = true;
                mask = -mask;
            }

            foreach (int offset in ifsCurrentIniFileSection.GetIntList(valueKey))
            {
                if (value != reverseLogic)
                    Data[BaseOffset + offset] |= (byte)mask;
                else
                    Data[BaseOffset + offset] &= (byte)~mask;
            }
        }
        #endregion

        #region GetString()
        public string GetString(string key, int maxLen)
        {
            int[] offsets = ifsCurrentIniFileSection.GetIntList(key);

            if (offsets.Length == 0) 
                return null;

            int length = GetByte(key + "Length");

            if (length == 0)
                length = maxLen;
            
            return DefaultEncoding.GetString(Data, BaseOffset + offsets[0], length).TrimEnd('\0');
        }
        #endregion

        #region SetString()
        public int SetString(string key, string text, int maxLen)
        {
            byte[] bytes = DefaultEncoding.GetBytes(text);
            int len = Math.Min(bytes.Length, maxLen);

            foreach (int offset in ifsCurrentIniFileSection.GetIntList(key))
            {
                Array.Copy(bytes, 0, Data, BaseOffset + offset, len);

                for (int i = len; i < maxLen; i++)
                    Data[BaseOffset + offset + i] = 0;
            }

            return len;
        }
        #endregion
    }
}