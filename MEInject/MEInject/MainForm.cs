using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
// ReSharper disable InconsistentNaming

namespace MEInject
{
    public partial class MainForm : Form
    {
        private string _dbFile = "db.dat";
        private string _MEdir = string.Empty;
        private List<MEinfo> _meFiles = new List<MEinfo>();
        private List<string> _validMEfiles;

        private MEinfo BIOS_ME_info;
        private byte[] BIOSfile;
        private byte[] MEfile;


        private uint BIOS_ME_start_offset;
        private uint BIOS_ME_end_offset;

        private string BIOSfilename;
        //private static byte _diff = 0x10;
        private Mode _mode;

        private byte[] MSDM_table_pattern =
        {
            0x01, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x1D, 0x00, 0x00, 0x00
        };
        private byte MSDM_offset = 0x14;

        enum Mode
        {
            ME,
            TXE
        }


        public MainForm()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            InitializeComponent();
        }

        private void ClearGUI()
        {
            MEoffsetLabel.Text = @"ME offset in BIOS: -";
            MEsizeInBIOSLabel.Text = @"ME size: -";
            MEinBIOS_ver_label.Text = @"ME version: -";
            MEsComboBox.Items.Clear();
            MEsComboBox.Text = string.Empty;
            _validMEfiles.Clear();
            WinKeyTextBox.Text = @"-";
        }

        private void UpdateGUI()
        {
            MEinBIOS_ver_label.Text = $@"{_mode} version: {BIOS_ME_info.Major}.{BIOS_ME_info.Minor}.{BIOS_ME_info.Hotfix}.{BIOS_ME_info.Build}";
            MEoffsetLabel.Text = $@"{_mode} offset in BIOS: 0x{BIOS_ME_start_offset:X8}";
            MEsizeInBIOSLabel.Text = $@"{_mode} size: {BIOS_ME_end_offset - BIOS_ME_start_offset} bytes";
            SuitableMEs.Text = $@"Suitable {_mode}s:";
            if (_MEdir != string.Empty) Text = $@"Intel ME/TXE injector - {_MEdir} - {_meFiles.Count} files";
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateGUI();
            _validMEfiles = new List<string>();
            _MEdir = Properties.Settings.Default.MEdir;
            LoadDB();
            UpdateGUI();
            Log("Ready!", LogLevel.Info);
            MEsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            MaximizeBox = false;

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveDB();
            Properties.Settings.Default.MEdir = _MEdir;
            Properties.Settings.Default.Save();
        }

        private void UpdateDB()
        {
            var files = GetFileNames(_MEdir);
            _meFiles.Clear();
            foreach (var file in files)
            {
                try
                {
                    _meFiles.Add(LoadMEinfo(file));
                }
                catch (Exception ex)
                {
                    Log(ex.Message, LogLevel.Warning);
                }
            }
            SaveDB();
            UpdateGUI();
        }

        private void SaveDB()
        {
            using (Stream writer = new FileStream(_dbFile, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(writer, _meFiles);
            }
        }
        private void LoadDB()
        {
            if (_MEdir == string.Empty)
            {
                MessageBox.Show(@"Please, specify ME files folder first");
                var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() != DialogResult.OK)
                {
                    Close();
                    return;
                }
                _MEdir = fbd.SelectedPath;
                Properties.Settings.Default.MEdir = _MEdir;
                Properties.Settings.Default.Save();
                UpdateDB();
            }

            if (!File.Exists(_dbFile))
            {

                UpdateDB();
                return;
            }

            try
            {
                using (Stream stream = File.Open(_dbFile, FileMode.Open))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    _meFiles = (List<MEinfo>)bformatter.Deserialize(stream);
                }
                if (_meFiles.Any(mefile => !File.Exists(mefile.Path))) UpdateDB();
            }

            catch (Exception exception)
            {
                Log(exception.Message, LogLevel.Error);
            }
        }

        private MEinfo GetMEFileInfo(Stream stream, string path, uint startoffset = 0, uint endoffset = 0)
        {
            stream.Seek(startoffset, SeekOrigin.Begin);
            var meinfo = new MEinfo();

            var handle = GCHandle.Alloc(new BinaryReader(stream).ReadBytes(Marshal.SizeOf(typeof(FptPreHeader))), GCHandleType.Pinned);
            var fptPreHeader = (FptPreHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FptPreHeader));
            handle.Free();

            handle = GCHandle.Alloc(new BinaryReader(stream).ReadBytes(Marshal.SizeOf(typeof(FptHeader))), GCHandleType.Pinned);
            var fptHeader = (FptHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FptHeader));
            handle.Free();

            var fptEntries = new List<FptEntry>();
            for (var i = 0; i < fptHeader.NumPartitions; i++)
            {
                handle = GCHandle.Alloc(new BinaryReader(stream).ReadBytes(Marshal.SizeOf(typeof(FptEntry))), GCHandleType.Pinned);
                var fptEntry = (FptEntry)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FptEntry));
                handle.Free();
                
                fptEntries.Add(fptEntry);
            }

            var mn2Manifests = new List<Mn2Manifest>();
            //foreach (var fptEntry in fptEntries.Where(fptEntry => (fptEntry.Flags & 0x00FF) == 0x80))
            foreach (var fptEntry in fptEntries.Where(fptEntry => (new string(fptEntry.Name) == "FTPR")))
            {
                stream.Seek(fptEntry.Offset + startoffset, SeekOrigin.Begin);
                
                var o = 0;
                if (new string(new BinaryReader(stream).ReadChars(4)) == "$CPD")
                {
                    o = new BinaryReader(stream).ReadByte() * 0x18 + 0x10;
                }
                stream.Seek(fptEntry.Offset + startoffset + o, SeekOrigin.Begin);

                handle = GCHandle.Alloc(new BinaryReader(stream).ReadBytes(Marshal.SizeOf(typeof(Mn2Manifest))), GCHandleType.Pinned);
                mn2Manifests.Add((Mn2Manifest)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Mn2Manifest)));
                handle.Free();
            }
            if (mn2Manifests.Count < 1) throw new Exception($@"Unsupported ME version: {path.SafeFileName()}");
            var manifest = mn2Manifests.First();

            meinfo.Size = endoffset == 0 ? (uint)stream.Length : endoffset - startoffset;
            meinfo.Path = path;
            meinfo.Major = manifest.Major;
            meinfo.Minor = manifest.Minor;
            meinfo.Hotfix = manifest.Hotfix;
            meinfo.Build = manifest.Build;
            return meinfo;
        }

        private MEinfo LoadMEinfo(string path)
        {
            var stream = File.Open(path, FileMode.Open);
            if (stream.Length < 1024 * 100)
            {
                stream.Close();
                throw new Exception($"Too small file size: {path.SafeFileName()}");
            }

            stream.Seek(0x10, SeekOrigin.Begin);

            var magic = new BinaryReader(stream).ReadBytes(4);

            if (magic.SequenceEqual(new byte[] { 0x24, 0x46, 0x50, 0x54 })) // $FPT - ME file
            {
                var meinfo = GetMEFileInfo(stream, path);
                Log($"ME file added {path.SafeFileName()}", LogLevel.Info);
                stream.Close();
                return meinfo;
            }
            stream.Close();
            throw new Exception($"Invalid input file: {path.SafeFileName()}");
        }

        private void LoadBIOS(string path)
        {

            Log("----------------------------------------", LogLevel.Default);
            BIOSfile = File.ReadAllBytes(path);
            var stream = File.Open(path, FileMode.Open);
            stream.Seek(0x10, SeekOrigin.Begin);

            var magic = new BinaryReader(stream).ReadBytes(4);

            if (magic.SequenceEqual(new byte[] { 0x5A, 0xA5, 0xF0, 0x0F })) // Flas descriptor sign. BIOS file
            {
                stream.Seek(0x14, SeekOrigin.Begin);
                var flmap0 = new BinaryReader(stream).ReadUInt32();
                var flmap1 = new BinaryReader(stream).ReadUInt32();
                var nr = flmap0 >> 24 & 0x7;
                var frba = flmap0 >> 12 & 0xff0;
                //var fmba = (flmap1 & 0xff) << 4;
                if (nr >= 2 || true)
                {
                    Log("Intel BIOS image detected! :D", LogLevel.Info);
                    stream.Seek(frba, SeekOrigin.Begin);
                    //FLREG0 = Flash Descriptor
                    //FLREG1 = BIOS
                    //FLREG2 = ME
                    var flreg0 = new BinaryReader(stream).ReadUInt32();
                    var flreg1 = new BinaryReader(stream).ReadUInt32();
                    var flreg2 = new BinaryReader(stream).ReadUInt32();
                    //var fd_start = (flreg0 & 0x1fff) << 12;
                    //var fd_end = flreg0 >> 4 & 0x1fff000 | 0xfff + 1;
                    BIOS_ME_start_offset = (flreg2 & 0x1fff) << 12;
                    BIOS_ME_end_offset = flreg2 >> 4 & 0x1fff000 | 0xfff + 1;
                    if (BIOS_ME_start_offset >= BIOS_ME_end_offset)
                    {
                        throw new Exception("The ME/TXE region in this image has been disabled");
                    }
                    stream.Seek(BIOS_ME_start_offset + 0x10, SeekOrigin.Begin);

                    if (new string(new BinaryReader(stream).ReadChars(4)) != "$FPT")
                    {
                        throw new Exception("The ME/TXE region is corrupted or missing");
                    }
                                       

                    BIOS_ME_info = GetMEFileInfo(stream, path, BIOS_ME_start_offset, BIOS_ME_end_offset);

                    _mode = BIOS_ME_info.Major < 4 ? Mode.TXE : Mode.ME;
                    UpdateGUI();

                    Log("BIOS read successful! " + path.SafeFileName(), LogLevel.Info);
                    Log($"The {_mode} region goes from {BIOS_ME_start_offset:X8} to {BIOS_ME_end_offset:X8}", LogLevel.Info);

                    UpdateComboBox();

                    var offset = Find(BIOSfile, MSDM_table_pattern) + MSDM_offset;
                    if (offset - MSDM_offset != -1)
                    {
                        stream.Seek(offset, SeekOrigin.Begin);
                        var handle = GCHandle.Alloc(new BinaryReader(stream).ReadBytes(Marshal.SizeOf(typeof(MSDM))), GCHandleType.Pinned);
                        var MSDM = (MSDM)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MSDM));
                        handle.Free();
                        WinKeyTextBox.Text = new string(MSDM.WinKey);
                    }
                    else
                    {
                        WinKeyTextBox.Text = @"none";
                    }
                    stream.Close();
                    return;
                }

                MessageBox.Show(flmap0 + " " + flmap1);

                stream.Close();
                throw new Exception("Number of partitions in file is less than 2! " + path.SafeFileName());
            }
            stream.Close();
            ClearGUI();
            BIOSfile = null;
            throw new Exception("Invalid input file " + path.SafeFileName());
        }
        private void ExtractButton_Click(object sender, EventArgs e)
        {
            if (BIOSfile == null)
            {
                Log("Nothing to save :(", LogLevel.Warning);
                return;
            }
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "bin",
                FileName = _mode + " from bios " + BIOSfilename
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var me = new byte[BIOS_ME_end_offset - BIOS_ME_start_offset];
                Array.Copy(BIOSfile, BIOS_ME_start_offset, me, 0, BIOS_ME_end_offset - BIOS_ME_start_offset);
                var _me = new List<byte>(me);
                for (int i = _me.Count - 1; i >= 0; i--)
                {
                    if (_me[i] == 0xFF) _me.RemoveAt(i);
                    else break;
                }
                File.WriteAllBytes(sfd.FileName, _me.ToArray());
                Log("Saved to " + sfd.FileName, LogLevel.Info);
            }
            catch (Exception exception)
            {
                Log(exception.Message, LogLevel.Error);
            }
        }
        void UpdateComboBox()
        {
            MEsComboBox.Items.Clear();
            _validMEfiles.Clear();
            foreach (var mefile in _meFiles)
            {
                if (BIOS_ME_info.Major == mefile.Major && (BIOS_ME_info.Minor == mefile.Minor || !MinorVer_checkBox.Checked) &&
                    BIOS_ME_end_offset - BIOS_ME_start_offset >= mefile.Size)
                {
                    MEsComboBox.Items.Add($@"{mefile.Major}.{mefile.Minor}.{mefile.Hotfix}.{mefile.Build} - {mefile.Path.SafeFileName()}");
                    _validMEfiles.Add(mefile.Path);
                }
            }
            if (MEsComboBox.Items.Count == 0) MEsComboBox.Items.Add("--none--");
            MEsComboBox.SelectedIndex = 0;
        }

        private void OpenBIOSbutton_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog { Multiselect = false };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            try
            {
                LoadBIOS(ofd.FileName);
                BIOSfilename = ofd.SafeFileName;
            }
            catch (Exception exception)
            {
                Log(exception.Message, LogLevel.Error);
            }
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (BIOSfile == null || _validMEfiles.Count == 0)
            {
                Log("Nothing to save :(", LogLevel.Warning);
                return;
            }
            MEfile = File.ReadAllBytes(_validMEfiles[MEsComboBox.SelectedIndex]);

            for (var i = 0; i < BIOS_ME_end_offset - BIOS_ME_start_offset; i++)
            {
                if (i < MEfile.Length)
                {
                    BIOSfile[i + BIOS_ME_start_offset] = MEfile[i];
                    continue;
                }
                BIOSfile[i + BIOS_ME_start_offset] = (byte)0xFF;
            }
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "bin",
                FileName = $@"{Regex.Replace(BIOSfilename, ".bin", string.Empty, RegexOptions.IgnoreCase)} + {_mode} {MEsComboBox.Text}" // $@"{BIOSfilename.Replace(".bin", string.Empty)} + {_mode} {MEsComboBox.Text}"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;
            try
            {
                File.WriteAllBytes(sfd.FileName, BIOSfile);
                Log("Saved to " + sfd.FileName, LogLevel.Info);
            }
            catch (Exception exception)
            {
                Log(exception.Message, LogLevel.Error);
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



        private readonly List<string> _filesList = new List<string>();
        private IEnumerable<string> GetFileNames(string path)
        {
            _filesList.Clear();
            FileFinder(path);
            return _filesList;
        }

        private void FileFinder(string path)
        {
            try
            {
                var dirs = Directory.GetDirectories(path);
                var files = Directory.GetFiles(path);
                _filesList.AddRange(files);
                foreach (var dir in dirs)
                {
                    FileFinder(dir);
                }
            }
            catch (Exception exception)
            {
                Log(exception.Message + " " + path, LogLevel.Warning);
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void Log(string message, LogLevel level)
        {
            Color color;
            switch (level)
            {
                case LogLevel.Info:
                    color = Color.Green;
                    break;
                case LogLevel.Warning:
                    color = Color.DarkOrange;
                    break;
                case LogLevel.Error:
                    color = Color.Red;
                    break;
                case LogLevel.Critical:
                    color = Color.Brown;
                    break;
                case LogLevel.Default:
                    color = Color.Black;
                    break;
                default:
                    color = Color.Black;
                    break;
            }
            DebugTextBox.AppendText(message, color);
        }
        private void DebugTextBox_TextChanged(object sender, EventArgs e)
        {
            DebugTextBox.SelectionStart = DebugTextBox.Text.Length;
            DebugTextBox.ScrollToCaret();
        }

        private void ChangeMEFolderButton_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog { SelectedPath = _MEdir };
            if (fbd.ShowDialog() != DialogResult.OK) return;
            _MEdir = fbd.SelectedPath;
            Properties.Settings.Default.MEdir = _MEdir;
            Properties.Settings.Default.Save();
            UpdateDB();
            UpdateGUI();
        }

        private void UpdateDB_Button_Click(object sender, EventArgs e)
        {
            UpdateDB();
        }

        private void MinorVer_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateComboBox();
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                try
                {
                    LoadBIOS(files[0]);
                    BIOSfilename = files[0].SafeFileName();
                }
                catch (Exception exception)
                {
                    Log(exception.Message, LogLevel.Error);
                }
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void About_button_Click(object sender, EventArgs e)
        {
            Log("kolyandex, 2018", LogLevel.Info);
            Log("Novosibirsk, Russia", LogLevel.Info);
            Log("For beer and chips:", LogLevel.Info);
            Log("eth: 0x535706fb56db4cb493fc3ffeb67f69a503755e26", LogLevel.Info);
            Log("zcash: t1VueM9xdtoaoRqQeX7q98iRnRvZ97QxuzV", LogLevel.Info);
            Log("btc: 3Hv1xdfPVpwfDTe92Bu6LcbTG7Xi1KnDpm", LogLevel.Info);
            Log("ltc: LfXr5XaDsaQjJQEXcuuvZazPdy2zC6exXH", LogLevel.Info);

        }
    }
}
