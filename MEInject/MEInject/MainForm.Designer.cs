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
            this.BIOSsizeLabel = new System.Windows.Forms.Label();
            this.MEsizeLabel = new System.Windows.Forms.Label();
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
            this.OpenMEButton.Click += new System.EventHandler(this.OpenMEButton_Click);
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
            // BIOSsizeLabel
            // 
            resources.ApplyResources(this.BIOSsizeLabel, "BIOSsizeLabel");
            this.BIOSsizeLabel.Name = "BIOSsizeLabel";
            // 
            // MEsizeLabel
            // 
            resources.ApplyResources(this.MEsizeLabel, "MEsizeLabel");
            this.MEsizeLabel.Name = "MEsizeLabel";
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MEsizeLabel);
            this.Controls.Add(this.BIOSsizeLabel);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.MEoffsetLabel);
            this.Controls.Add(this.OpenMEButton);
            this.Controls.Add(this.DebugTextBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.OpenBIOSButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
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
        private System.Windows.Forms.Label BIOSsizeLabel;
        private System.Windows.Forms.Label MEsizeLabel;
    }
}

