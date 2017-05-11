using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

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

        void ClearFields()
        {
            MEoffsetLabel.Text = "ME offset in BIOS: -";
            MEsizeInBIOSLabel.Text = "ME size: -";
            MEinBIOS_ver_label.Text = "ME version: -";
            MEsComboBox.Items.Clear();
            MEsComboBox.Text = string.Empty;
            _validMEfiles.Clear();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _validMEfiles = new List<string>();
            _MEdir = Properties.Settings.Default.MEdir;
            LoadDB();
            if (_MEdir != string.Empty) Text = "Intel ME/TXE injector - " + _MEdir + " - " + _meFiles.Count + " files";
            Log("Ready!");
            MEsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveDB();
            Properties.Settings.Default.MEdir = _MEdir;
            Properties.Settings.Default.Save();
        }

        void UpdateDB()
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
                    //Log(ex.Message);
                }
            }
            SaveDB();
            Text = "Intel ME/TXE injector - " + _MEdir + " - " + _meFiles.Count + " files";
        }

        void SaveDB()
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
                MessageBox.Show("Please, specify ME files folder first");
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
                Text = "Intel ME/TXE injector - " + _MEdir + " - " + _meFiles.Count + " files";
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
                Log(exception.Message + "");
            }
        }

        MEinfo GetMEFileInfo(Stream stream, string path, uint startoffset = 0, uint endoffset = 0)
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
            foreach (var fptEntry in fptEntries.Where(fptEntry => (fptEntry.Flags & 0x00FF) == 0x80))
            {
                stream.Seek(fptEntry.Offset + startoffset, SeekOrigin.Begin);
                handle = GCHandle.Alloc(new BinaryReader(stream).ReadBytes(Marshal.SizeOf(typeof(Mn2Manifest))), GCHandleType.Pinned);
                mn2Manifests.Add((Mn2Manifest)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Mn2Manifest)));
                handle.Free();
            }

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
            stream.Seek(0x10, SeekOrigin.Begin);

            var magic = new BinaryReader(stream).ReadBytes(4);

            if (magic.SequenceEqual(new byte[] { 0x24, 0x46, 0x50, 0x54 })) // $FPT - ME file
            {
                Log("ME file added " + path.SafeFileName());
                var meinfo = GetMEFileInfo(stream, path);
                stream.Close();
                return meinfo;
            }
            stream.Close();
            throw new Exception("Invalid input file " + path.SafeFileName());
        }

        private void LoadBIOS(string path)
        {
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
                if (nr >= 2)
                {
                    //Log("BIOS image detected");
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
                    Log("BIOS read successful! " + path.SafeFileName());
                    //Log(path);
                    //Log(string.Format("The ME/TXE region goes from {0:X8} to {1:X8}", BIOS_ME_start_offset, BIOS_ME_end_offset));



                    BIOS_ME_info = GetMEFileInfo(stream, path, BIOS_ME_start_offset, BIOS_ME_end_offset);

                    _mode = BIOS_ME_info.Major < 3 ? Mode.TXE : Mode.ME;
                    MEinBIOS_ver_label.Text = _mode + " version: " + BIOS_ME_info.Major + "." + BIOS_ME_info.Minor + "." +
                                              BIOS_ME_info.Hotfix + "." + BIOS_ME_info.Build;
                    MEoffsetLabel.Text = _mode + " offset in BIOS: 0x" + BIOS_ME_start_offset.ToString("X8");
                    MEsizeInBIOSLabel.Text = _mode + " size: " + (BIOS_ME_end_offset - BIOS_ME_start_offset) + " bytes";
                    SuitableMEs.Text = "Suitable " + _mode + "s:";
                    Log("Mode: " + _mode);
                    UpdateComboBox();
                    //Log(MEinBIOS_ver_label.Text);
                    /*MEsComboBox.Items.Clear();
                    _validMEfiles.Clear();
                    foreach (var mefile in _meFiles)
                    {
                        if (BIOS_ME_info.Major == mefile.Major && (BIOS_ME_info.Minor == mefile.Minor || !MinorVer_checkBox.Checked)  &&
                            BIOS_ME_end_offset - BIOS_ME_start_offset >= mefile.Size)
                        {
                            MEsComboBox.Items.Add(mefile.Major + "." + mefile.Minor + "." + mefile.Hotfix + "." + mefile.Build + " - " + mefile.Path.SafeFileName());
                            _validMEfiles.Add(mefile.Path);
                        }
                    }
                    if (MEsComboBox.Items.Count == 0) MEsComboBox.Items.Add("--none--");
                    MEsComboBox.SelectedIndex = 0;*/
                }
                stream.Close();
                return;
            }
            stream.Close();
            ClearFields();
            BIOSfile = null;
            throw new Exception("Invalid input file " + path.SafeFileName());
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
                    MEsComboBox.Items.Add(mefile.Major + "." + mefile.Minor + "." + mefile.Hotfix + "." + mefile.Build + " - " + mefile.Path.SafeFileName());
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
                Log(exception.Message);
            }
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (BIOSfile == null || _validMEfiles.Count == 0)
            {
                Log("Nothing to save :(");
                return;
            }
            MEfile = File.ReadAllBytes(_validMEfiles[MEsComboBox.SelectedIndex]);


            for (var i = 0; i < MEfile.Length; i++) BIOSfile[i + BIOS_ME_start_offset] = MEfile[i];

            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "bin",
                FileName = BIOSfilename.Replace(".bin", string.Empty) + " + " + _validMEfiles[MEsComboBox.SelectedIndex]
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



        private readonly List<string> _filesList = new List<string>();
        private List<string> GetFileNames(string path)
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
                Log(exception.Message + " " + path);
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            return;
            LoadDB();
            return;
            foreach (var file in GetFileNames(@"C:\Users\Nick\Google Диск\BIOS\"))
            {
                try
                {
                    // LoadFile(file);
                }
                catch (Exception exception)
                {
                    //Log(exception.Message);
                }
                //Log(file);
            }
        }
        private void Log(string message)
        {
            DebugTextBox.Text += /*DateTime.Now + " - " + */message + "\n";
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
            Text = "Intel ME/TXE injector - " + _MEdir + " - " + _meFiles.Count + " files";
        }

        private void UpdateDB_Button_Click(object sender, EventArgs e)
        {
            UpdateDB();
        }

        private void MinorVer_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateComboBox();
        }
    }
}
