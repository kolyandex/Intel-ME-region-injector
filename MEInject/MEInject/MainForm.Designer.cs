namespace MEInject
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.OpenBIOSButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.DebugTextBox = new System.Windows.Forms.RichTextBox();
            this.OpenMEButton = new System.Windows.Forms.Button();
            this.MEoffsetLabel = new System.Windows.Forms.Label();
            this.SaveButton = new System.Windows.Forms.Button();
            this.MEsizeInBIOSLabel = new System.Windows.Forms.Label();
            this.MEinBIOS_ver_label = new System.Windows.Forms.Label();
            this.SuitableMEs = new System.Windows.Forms.Label();
            this.MEsComboBox = new System.Windows.Forms.ComboBox();
            this.UpdateDB_Button = new System.Windows.Forms.Button();
            this.MinorVer_checkBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.WinKeyTextBox = new System.Windows.Forms.TextBox();
            this.ExtractButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // OpenBIOSButton
            // 
            resources.ApplyResources(this.OpenBIOSButton, "OpenBIOSButton");
            this.OpenBIOSButton.Name = "OpenBIOSButton";
            this.OpenBIOSButton.UseVisualStyleBackColor = true;
            this.OpenBIOSButton.Click += new System.EventHandler(this.OpenBIOSbutton_Click);
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // DebugTextBox
            // 
            this.DebugTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.DebugTextBox.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.DebugTextBox, "DebugTextBox");
            this.DebugTextBox.Name = "DebugTextBox";
            this.DebugTextBox.ReadOnly = true;
            this.DebugTextBox.TextChanged += new System.EventHandler(this.DebugTextBox_TextChanged);
            // 
            // OpenMEButton
            // 
            resources.ApplyResources(this.OpenMEButton, "OpenMEButton");
            this.OpenMEButton.Name = "OpenMEButton";
            this.OpenMEButton.UseVisualStyleBackColor = true;
            this.OpenMEButton.Click += new System.EventHandler(this.ChangeMEFolderButton_Click);
            // 
            // MEoffsetLabel
            // 
            resources.ApplyResources(this.MEoffsetLabel, "MEoffsetLabel");
            this.MEoffsetLabel.Name = "MEoffsetLabel";
            // 
            // SaveButton
            // 
            resources.ApplyResources(this.SaveButton, "SaveButton");
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // MEsizeInBIOSLabel
            // 
            resources.ApplyResources(this.MEsizeInBIOSLabel, "MEsizeInBIOSLabel");
            this.MEsizeInBIOSLabel.Name = "MEsizeInBIOSLabel";
            // 
            // MEinBIOS_ver_label
            // 
            resources.ApplyResources(this.MEinBIOS_ver_label, "MEinBIOS_ver_label");
            this.MEinBIOS_ver_label.Name = "MEinBIOS_ver_label";
            // 
            // SuitableMEs
            // 
            resources.ApplyResources(this.SuitableMEs, "SuitableMEs");
            this.SuitableMEs.Name = "SuitableMEs";
            // 
            // MEsComboBox
            // 
            resources.ApplyResources(this.MEsComboBox, "MEsComboBox");
            this.MEsComboBox.FormattingEnabled = true;
            this.MEsComboBox.Name = "MEsComboBox";
            // 
            // UpdateDB_Button
            // 
            resources.ApplyResources(this.UpdateDB_Button, "UpdateDB_Button");
            this.UpdateDB_Button.Name = "UpdateDB_Button";
            this.UpdateDB_Button.UseVisualStyleBackColor = true;
            this.UpdateDB_Button.Click += new System.EventHandler(this.UpdateDB_Button_Click);
            // 
            // MinorVer_checkBox
            // 
            resources.ApplyResources(this.MinorVer_checkBox, "MinorVer_checkBox");
            this.MinorVer_checkBox.Checked = true;
            this.MinorVer_checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MinorVer_checkBox.Name = "MinorVer_checkBox";
            this.MinorVer_checkBox.UseVisualStyleBackColor = true;
            this.MinorVer_checkBox.CheckedChanged += new System.EventHandler(this.MinorVer_checkBox_CheckedChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // WinKeyTextBox
            // 
            resources.ApplyResources(this.WinKeyTextBox, "WinKeyTextBox");
            this.WinKeyTextBox.Name = "WinKeyTextBox";
            // 
            // ExtractButton
            // 
            resources.ApplyResources(this.ExtractButton, "ExtractButton");
            this.ExtractButton.Name = "ExtractButton";
            this.ExtractButton.UseVisualStyleBackColor = true;
            this.ExtractButton.Click += new System.EventHandler(this.ExtractButton_Click);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ExtractButton);
            this.Controls.Add(this.WinKeyTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.MinorVer_checkBox);
            this.Controls.Add(this.UpdateDB_Button);
            this.Controls.Add(this.MEsComboBox);
            this.Controls.Add(this.SuitableMEs);
            this.Controls.Add(this.MEinBIOS_ver_label);
            this.Controls.Add(this.MEsizeInBIOSLabel);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.MEoffsetLabel);
            this.Controls.Add(this.OpenMEButton);
            this.Controls.Add(this.DebugTextBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.OpenBIOSButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button OpenBIOSButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox DebugTextBox;
        private System.Windows.Forms.Button OpenMEButton;
        private System.Windows.Forms.Label MEoffsetLabel;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Label MEsizeInBIOSLabel;
        private System.Windows.Forms.Label MEinBIOS_ver_label;
        private System.Windows.Forms.Label SuitableMEs;
        private System.Windows.Forms.ComboBox MEsComboBox;
        private System.Windows.Forms.Button UpdateDB_Button;
        private System.Windows.Forms.CheckBox MinorVer_checkBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox WinKeyTextBox;
        private System.Windows.Forms.Button ExtractButton;
    }
}

