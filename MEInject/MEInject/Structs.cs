using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MEInject
{
    internal enum LogLevel
    {
        Info,
        Warning,
        Error,
        Critical,
        Default
    }
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text + "\n");
            box.SelectionColor = box.ForeColor;
        }
    }
    static class Helper
    {
        public static string SafeFileName(this string str)
        {
            return str.Substring(str.LastIndexOf(@"\") + 1);
        }

        
    }
    [Serializable]
    public struct MEinfo
    {
        internal string Path;
        internal uint Size;
        internal ushort Major;
        internal ushort Minor;
        internal ushort Hotfix;
        internal ushort Build;
    }
    class MEFile
    {
        internal FptPreHeader FptPreHeader;
        internal FptHeader FptHeader;
        internal List<FptEntry> FptEntries;
        internal List<Mn2Manifest> Mn2Manifests;

        public MEFile()
        {
            FptEntries = new List<FptEntry>();
            Mn2Manifests = new List<Mn2Manifest>();
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FptPreHeader
    {
        internal uint ROMB_Instr_0;
        internal uint ROMB_Instr_1;
        internal uint ROMB_Instr_2;
        internal uint ROMB_Instr_3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FptHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal char[] Tag;

        internal uint NumPartitions;
        internal byte Version;
        internal byte EntryType;
        internal byte Lenght;
        internal byte Checksum;
        internal ushort FlashCycleLife;
        internal ushort FlashCycleLimit;
        internal uint UMASize;
        internal uint Flags;
        internal ushort FitMajor;
        internal ushort FitMinor;
        internal ushort FitHotfix;
        internal ushort FitBuild;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FptEntry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal char[] Name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal char[] Owner;

        internal uint Offset;
        internal uint Size;
        internal uint StartTokens;
        internal uint MaxTokens;
        internal uint ScratchSectors;
        internal uint Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Mn2Manifest
    {
        internal ushort ModuleType;
        internal ushort ModuleSubType;
        internal uint HeaderLength;
        internal uint HeaderVersion;
        internal uint Flags;
        internal uint ModuleVendor;
        internal byte Day;
        internal byte Month;
        internal ushort Year;
        internal uint Size;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal char[] Tag;

        internal uint NumModules;
        internal ushort Major;
        internal ushort Minor;
        internal ushort Hotfix;
        internal ushort Build;
        internal byte SVN_9;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal byte[] Unk2D_30;

        internal byte SVN_8;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal byte[] Unk31_34;

        internal byte VCN;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal byte[] Unk35_78;

        internal uint KeySize;
        internal uint ScratchSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        internal uint[] RsaPubKey;

        internal uint RsaPubExp;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        internal uint[] RsaSig;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        internal char[] PartitionName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MSDM
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 29)]
        internal char[] WinKey;
    }
}

