using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MEInject
{
    public partial class MainForm : Form
    {
        private static readonly byte[] Mask = { 0x24, 0x46, 0x50, 0x54 };
        private byte[] BIOSfile;
        private byte[] MEfile;
        private int BIOS_ME_offset;
        private string BIOSfilename;
        private static byte _diff = 0x10;
        private Mode _mode;

        enum Mode
        {
            ME,
            TXE
        }

        public MainForm()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            InitializeComponent();
            Log("Ready!");
            //OpenMEButton.Enabled = false;
        }

        MEFile GetMEFile(Stream stream, uint offset = 0)
        {
            using (var b = new BinaryReader(stream))
            {
                b.BaseStream.Seek(offset, SeekOrigin.Begin);
                var meFile = new MEFile();

                var handle = GCHandle.Alloc(b.ReadBytes(Marshal.SizeOf(typeof(FptPreHeader))), GCHandleType.Pinned);
                meFile.FptPreHeader = (FptPreHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FptPreHeader));
                handle.Free();

                handle = GCHandle.Alloc(b.ReadBytes(Marshal.SizeOf(typeof(FptHeader))), GCHandleType.Pinned);
                meFile.FptHeader = (FptHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FptHeader));
                handle.Free();



                for (var i = 0; i < meFile.FptHeader.NumPartitions; i++)
                {
                    handle = GCHandle.Alloc(b.ReadBytes(Marshal.SizeOf(typeof(FptEntry))), GCHandleType.Pinned);
                    var fptEntry = (FptEntry)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FptEntry));
                    handle.Free();
                    //Log(new string(fptEntry.Name));
                    if (new string(fptEntry.Name) == "FTPR")
                    {
                        //MN2_offset = fptEntry.Offset;
                    }
                    meFile.FptEntries.Add(fptEntry);

                }

                foreach (var fptEntry in meFile.FptEntries)
                {
                    if (fptEntry.Offset == 0xFFFFFFFF) continue;
                    b.BaseStream.Seek(fptEntry.Offset + offset, SeekOrigin.Begin);
                    handle = GCHandle.Alloc(b.ReadBytes(Marshal.SizeOf(typeof(Mn2Manifest))), GCHandleType.Pinned);
                    var manifest = (Mn2Manifest)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Mn2Manifest));
                    if (new string(manifest.Tag) == "$MN2") meFile.Mn2Manifests.Add(manifest);
                    handle.Free();
                }
                return meFile;
            }
        }

        void ShowMEfile(MEFile meFile)
        {
            Log(meFile.FptHeader.NumPartitions.ToString());

            foreach (var fptEntry in meFile.FptEntries)
            {
                Log(new string(fptEntry.Name) + ": " + fptEntry.Offset.ToString("X8"));
            }

            foreach (var manifest in meFile.Mn2Manifests)
            {
                Log(new string(manifest.Tag) + " - " + manifest.Major + "." + manifest.Minor + "." + manifest.Hotfix + "." + manifest.Build);
            }
        }

        private void OpenBIOSbutton_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog { Multiselect = false };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            var stream = File.Open(ofd.FileName, FileMode.Open);
            uint me_start;
            uint me_end;

            stream.Seek(0x14, SeekOrigin.Begin);
            var flmap0 = new BinaryReader(stream).ReadUInt32();
            var flmap1 = new BinaryReader(stream).ReadUInt32();
            var nr = flmap0 >> 24 & 0x7; // 0x3
            var frba = flmap0 >> 12 & 0xff0; // 0x40
            var fmba = (flmap1 & 0xff) << 4;
            if (nr >= 2)
            {
                stream.Seek(frba, SeekOrigin.Begin);
                var flreg0 = new BinaryReader(stream).ReadUInt32();
                var flreg1 = new BinaryReader(stream).ReadUInt32();
                var flreg2 = new BinaryReader(stream).ReadUInt32(); // 0x01FF0003
                var fd_start = (flreg0 & 0x1fff) << 12;
                var fd_end = flreg0 >> 4 & 0x1fff000 | 0xfff + 1;
                me_start = (flreg2 & 0x1fff) << 12; // 0x3000
                me_end = flreg2 >> 4 & 0x1fff000 | 0xfff + 1; 
                if (me_start >= me_end)
                {
                    Log("The ME/TXE region in this image has been disabled");
                    return;
                }
                stream.Seek(me_start + 0x10, SeekOrigin.Begin);

                if (new string(new BinaryReader(stream).ReadChars(4)) != "$FPT")
                {
                    Log("The ME/TXE region is corrupted or missing");
                    return;
                }

                Log(string.Format("The ME/TXE region goes from {0:X8} to {1:X8}", me_start, me_end));

            }

            return;



            MEfile = null;
            BIOSfile = File.ReadAllBytes(ofd.FileName);
            var offset = Find(BIOSfile, Mask);

            if (offset == -1)
            {
                Log("Can't find ME/TXE region in file");
                BIOSfile = null;
                OpenMEButton.Enabled = false;
                return;
            }
            offset -= _diff;
            if (offset == 0)
            {
                Log("Please, open BIOS, not ME/TXE");
                BIOSfile = null;
                OpenMEButton.Enabled = false;
                return;
            }

            var meFile = GetMEFile(File.Open(ofd.FileName, FileMode.Open), (uint)offset);
            ShowMEfile(meFile);
            return;
            BIOSfilename = ofd.SafeFileName;
            BIOS_ME_offset = offset;
            _mode = BIOSfile[offset] == 0x00 ? Mode.TXE : Mode.ME;

            Log("Mode: " + _mode);

            OpenMEButton.Enabled = BIOSfile != null;
            OpenMEButton.Text = "Open " + _mode;

            MEoffsetLabel.Text = _mode + " offset in BIOS: 0x" + BIOS_ME_offset.ToString("X8");
            BIOSsizeLabel.Text = "BIOS size: 0x" + BIOSfile.Length.ToString("X8");
            Log("BIOS read successful! " + ofd.SafeFileName);
        }
        private void OpenMEButton_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog { Multiselect = false };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            MEfile = File.ReadAllBytes(ofd.FileName);

            var meFile = GetMEFile(File.Open(ofd.FileName, FileMode.Open));

            ShowMEfile(meFile);
            return;
            using (var b = new BinaryReader(File.Open(ofd.FileName, FileMode.Open)))
            {
                //int currentseek = 0;

                FptPreHeader fptPreHeader;

                GCHandle handle = GCHandle.Alloc(b.ReadBytes(Marshal.SizeOf(typeof(FptPreHeader))), GCHandleType.Pinned);
                fptPreHeader = (FptPreHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FptPreHeader));
                handle.Free();
                Log(fptPreHeader.ROMB_Instr_0.ToString());

                //b.BaseStream.Seek(Marshal.SizeOf(typeof(FptPreHeader)), SeekOrigin.Begin);

                FptHeader fptHeader;

                handle = GCHandle.Alloc(b.ReadBytes(Marshal.SizeOf(typeof(FptHeader))), GCHandleType.Pinned);
                fptHeader = (FptHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FptHeader));
                handle.Free();
                Log(fptHeader.NumPartitions.ToString());

                //b.BaseStream.Seek(Marshal.SizeOf(typeof(FptHeader)), SeekOrigin.Current);

                var fptEntries = new List<FptEntry>();

                for (int i = 0; i < fptHeader.NumPartitions; i++)
                {
                    FptEntry fptEntry;
                    handle = GCHandle.Alloc(b.ReadBytes(Marshal.SizeOf(typeof(FptEntry))), GCHandleType.Pinned);
                    fptEntry = (FptEntry)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FptEntry));
                    handle.Free();
                    Log(new string(fptEntry.Name));
                    if (new string(fptEntry.Name) == "FTPR")
                    {
                        //MN2_offset = fptEntry.Offset;
                        Log(fptEntry.Offset.ToString());
                    }
                    //b.BaseStream.Seek(Marshal.SizeOf(typeof(FptEntry)), SeekOrigin.Current);
                    fptEntries.Add(fptEntry);
                }


                //b.BaseStream.Seek(MN2_offset, SeekOrigin.Begin);
                //Log("MN offset " + MN2_offset);
                Mn2Manifest manifest;
                handle = GCHandle.Alloc(b.ReadBytes(Marshal.SizeOf(typeof(Mn2Manifest))), GCHandleType.Pinned);
                manifest = (Mn2Manifest)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Mn2Manifest));
                handle.Free();
                Log(manifest.Major + "." + manifest.Minor + "." + manifest.Hotfix + "." + manifest.Build);
            }

            if (BIOSfile == null) return;



            if (Find(MEfile, Mask) - _diff != 0)
            {
                Log("It's not valid " + _mode + " file");
                MEfile = null;
                return;
            }

            if ((MEfile[0] == 0x00 && _mode == Mode.ME) || (MEfile[0] != 0x00 && _mode == Mode.TXE))
            {
                Log("Please, open " + _mode + " file");
                MEfile = null;
                return;
            }

            if (MEfile.Length >= BIOSfile.Length)
            {
                Log(_mode + " file is larger then BIOS file!");
                MEfile = null;
                return;
            }

            MEsizeLabel.Text = _mode + " size: 0x" + MEfile.Length.ToString("X8");
            Log(_mode + " read successful! " + ofd.SafeFileName);
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (BIOSfile == null || MEfile == null)
            {
                Log("Nothing to save :(");
                return;
            }

            for (var i = 0; i < MEfile.Length; i++) BIOSfile[i + BIOS_ME_offset] = MEfile[i];

            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "bin",
                FileName = BIOSfilename.Replace(".bin", string.Empty) + "-new"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;
            try
            {
                File.WriteAllBytes(sfd.FileName, BIOSfile);
                Log("Saved to " + sfd.FileName);
            }
            catch (Exception exception)
            {
                Log(exception.Message);
            }
        }
        static int Find(IList<byte> array, IList<byte> mask)
        {
            try
            {
                for (var i = 0; i < array.Count; i++)
                {
                    if (array[i] != mask[0]) continue;
                    for (var j = 1; j < mask.Count; j++)
                    {
                        if (i + j >= array.Count) return -1;
                        if (array[i + j] == mask[j] & j == mask.Count - 1) return i;
                        if (array[i + j] == mask[j]) continue;
                        i += j;
                        break;
                    }
                }
                return -1;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }






        private void button1_Click(object sender, EventArgs e)
        {
            return;
            var files = Directory.GetFiles(@"C:\Users\Nick\Google Диск\BIOS\Wistron");
            //var files = Directory.GetFiles(@"C:\Windows\System32");

            foreach (var file in files)
            {
                try
                {
                    Log(file + " - " + (Find(File.ReadAllBytes(file), Mask) - _diff).ToString("X"));
                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                }

            }


        }
        private void Log(string message)
        {
            DebugTextBox.Text += DateTime.Now + " - " + message + "\n";
        }
        private void DebugTextBox_TextChanged(object sender, EventArgs e)
        {
            DebugTextBox.SelectionStart = DebugTextBox.Text.Length;
            DebugTextBox.ScrollToCaret();
        }
    }
}
