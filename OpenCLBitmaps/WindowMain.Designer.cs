namespace OpenCLBitmaps
{
    partial class WindowMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.listBox_log = new ListBox();
			this.listBox_images = new ListBox();
			this.comboBox_devices = new ComboBox();
			this.button_move = new Button();
			this.button_reset = new Button();
			this.button_export = new Button();
			this.button_import = new Button();
			this.listBox_pointers = new ListBox();
			this.panel_view = new Panel();
			this.pictureBox_view = new PictureBox();
			this.numericUpDown_zoom = new NumericUpDown();
			this.label_meta = new Label();
			this.button_info = new Button();
			this.button_recenter = new Button();
			this.groupBox_kernel = new GroupBox();
			this.button_kernelUnload = new Button();
			this.button_kernelCreate = new Button();
			this.checkBox_kernelOop = new CheckBox();
			this.button_kernelExecute = new Button();
			this.button_kernelLoad = new Button();
			this.comboBox_kernelVersions = new ComboBox();
			this.checkBox_kernelInvariantSearch = new CheckBox();
			this.textBox_kernelBaseName = new TextBox();
			this.comboBox_kernelBaseNames = new ComboBox();
			this.label_kernelCurrentName = new Label();
			this.panel_kernelArguments = new Panel();
			this.textBox_kernelCode = new TextBox();
			this.button_darkMode = new Button();
			this.label_imagesCount = new Label();
			this.label_pointersCount = new Label();
			this.button_createEmpty = new Button();
			this.button_createColor = new Button();
			this.numericUpDown_createSize = new NumericUpDown();
			this.checkBox_mandelbrotMode = new CheckBox();
			this.panel_view.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) this.pictureBox_view).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_zoom).BeginInit();
			this.groupBox_kernel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_createSize).BeginInit();
			this.SuspendLayout();
			// 
			// listBox_log
			// 
			this.listBox_log.FormattingEnabled = true;
			this.listBox_log.ItemHeight = 15;
			this.listBox_log.Location = new Point(12, 595);
			this.listBox_log.Name = "listBox_log";
			this.listBox_log.Size = new Size(1237, 214);
			this.listBox_log.TabIndex = 0;
			this.listBox_log.DoubleClick += this.listBox_log_DoubleClick;
			// 
			// listBox_images
			// 
			this.listBox_images.FormattingEnabled = true;
			this.listBox_images.ItemHeight = 15;
			this.listBox_images.Location = new Point(1336, 610);
			this.listBox_images.Name = "listBox_images";
			this.listBox_images.Size = new Size(160, 199);
			this.listBox_images.TabIndex = 1;
			// 
			// comboBox_devices
			// 
			this.comboBox_devices.FormattingEnabled = true;
			this.comboBox_devices.Location = new Point(12, 12);
			this.comboBox_devices.Name = "comboBox_devices";
			this.comboBox_devices.Size = new Size(400, 23);
			this.comboBox_devices.TabIndex = 2;
			this.comboBox_devices.Text = "Select OpenCL-device to initialize.";
			// 
			// button_move
			// 
			this.button_move.Location = new Point(1255, 595);
			this.button_move.Name = "button_move";
			this.button_move.Size = new Size(75, 23);
			this.button_move.TabIndex = 3;
			this.button_move.Text = "Move";
			this.button_move.UseVisualStyleBackColor = true;
			this.button_move.Click += this.button_move_Click;
			// 
			// button_reset
			// 
			this.button_reset.Location = new Point(1255, 719);
			this.button_reset.Name = "button_reset";
			this.button_reset.Size = new Size(75, 23);
			this.button_reset.TabIndex = 4;
			this.button_reset.Text = "Reset";
			this.button_reset.UseVisualStyleBackColor = true;
			this.button_reset.Click += this.button_reset_Click;
			// 
			// button_export
			// 
			this.button_export.Location = new Point(1255, 786);
			this.button_export.Name = "button_export";
			this.button_export.Size = new Size(75, 23);
			this.button_export.TabIndex = 5;
			this.button_export.Text = "Export";
			this.button_export.UseVisualStyleBackColor = true;
			this.button_export.Click += this.button_export_Click;
			// 
			// button_import
			// 
			this.button_import.Location = new Point(1255, 757);
			this.button_import.Name = "button_import";
			this.button_import.Size = new Size(75, 23);
			this.button_import.TabIndex = 6;
			this.button_import.Text = "Import";
			this.button_import.UseVisualStyleBackColor = true;
			this.button_import.Click += this.button_import_Click;
			// 
			// listBox_pointers
			// 
			this.listBox_pointers.FormattingEnabled = true;
			this.listBox_pointers.ItemHeight = 15;
			this.listBox_pointers.Location = new Point(1502, 610);
			this.listBox_pointers.Name = "listBox_pointers";
			this.listBox_pointers.Size = new Size(160, 199);
			this.listBox_pointers.TabIndex = 7;
			// 
			// panel_view
			// 
			this.panel_view.Controls.Add(this.pictureBox_view);
			this.panel_view.Location = new Point(662, 12);
			this.panel_view.Name = "panel_view";
			this.panel_view.Size = new Size(1000, 550);
			this.panel_view.TabIndex = 8;
			// 
			// pictureBox_view
			// 
			this.pictureBox_view.Location = new Point(3, 3);
			this.pictureBox_view.Name = "pictureBox_view";
			this.pictureBox_view.Size = new Size(994, 544);
			this.pictureBox_view.TabIndex = 0;
			this.pictureBox_view.TabStop = false;
			// 
			// numericUpDown_zoom
			// 
			this.numericUpDown_zoom.Location = new Point(1592, 568);
			this.numericUpDown_zoom.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
			this.numericUpDown_zoom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_zoom.Name = "numericUpDown_zoom";
			this.numericUpDown_zoom.Size = new Size(70, 23);
			this.numericUpDown_zoom.TabIndex = 9;
			this.numericUpDown_zoom.Value = new decimal(new int[] { 100, 0, 0, 0 });
			// 
			// label_meta
			// 
			this.label_meta.AutoSize = true;
			this.label_meta.Location = new Point(662, 565);
			this.label_meta.Name = "label_meta";
			this.label_meta.Size = new Size(101, 15);
			this.label_meta.TabIndex = 10;
			this.label_meta.Text = "No image loaded.";
			// 
			// button_info
			// 
			this.button_info.Location = new Point(418, 12);
			this.button_info.Name = "button_info";
			this.button_info.Size = new Size(23, 23);
			this.button_info.TabIndex = 11;
			this.button_info.Text = "i";
			this.button_info.UseVisualStyleBackColor = true;
			this.button_info.Click += this.button_info_Click;
			// 
			// button_recenter
			// 
			this.button_recenter.Location = new Point(1511, 568);
			this.button_recenter.Name = "button_recenter";
			this.button_recenter.Size = new Size(75, 23);
			this.button_recenter.TabIndex = 12;
			this.button_recenter.Text = "Re-center";
			this.button_recenter.UseVisualStyleBackColor = true;
			this.button_recenter.Click += this.button_recenter_Click;
			// 
			// groupBox_kernel
			// 
			this.groupBox_kernel.Controls.Add(this.button_kernelUnload);
			this.groupBox_kernel.Controls.Add(this.button_kernelCreate);
			this.groupBox_kernel.Controls.Add(this.checkBox_kernelOop);
			this.groupBox_kernel.Controls.Add(this.button_kernelExecute);
			this.groupBox_kernel.Controls.Add(this.button_kernelLoad);
			this.groupBox_kernel.Controls.Add(this.comboBox_kernelVersions);
			this.groupBox_kernel.Controls.Add(this.checkBox_kernelInvariantSearch);
			this.groupBox_kernel.Controls.Add(this.textBox_kernelBaseName);
			this.groupBox_kernel.Controls.Add(this.comboBox_kernelBaseNames);
			this.groupBox_kernel.Location = new Point(447, 34);
			this.groupBox_kernel.Name = "groupBox_kernel";
			this.groupBox_kernel.Size = new Size(209, 240);
			this.groupBox_kernel.TabIndex = 13;
			this.groupBox_kernel.TabStop = false;
			this.groupBox_kernel.Text = "OpenCL Kernels";
			// 
			// button_kernelUnload
			// 
			this.button_kernelUnload.Location = new Point(6, 157);
			this.button_kernelUnload.Name = "button_kernelUnload";
			this.button_kernelUnload.Size = new Size(60, 23);
			this.button_kernelUnload.TabIndex = 23;
			this.button_kernelUnload.Text = "Unload";
			this.button_kernelUnload.UseVisualStyleBackColor = true;
			this.button_kernelUnload.Click += this.button_kernelUnload_Click;
			// 
			// button_kernelCreate
			// 
			this.button_kernelCreate.Location = new Point(133, 107);
			this.button_kernelCreate.Name = "button_kernelCreate";
			this.button_kernelCreate.Size = new Size(70, 23);
			this.button_kernelCreate.TabIndex = 22;
			this.button_kernelCreate.Text = "Create";
			this.button_kernelCreate.UseVisualStyleBackColor = true;
			this.button_kernelCreate.Click += this.button_kernelCreate_Click;
			// 
			// checkBox_kernelOop
			// 
			this.checkBox_kernelOop.AutoSize = true;
			this.checkBox_kernelOop.Location = new Point(6, 215);
			this.checkBox_kernelOop.Name = "checkBox_kernelOop";
			this.checkBox_kernelOop.Size = new Size(154, 19);
			this.checkBox_kernelOop.TabIndex = 21;
			this.checkBox_kernelOop.Text = "Out-of-Place operation?";
			this.checkBox_kernelOop.UseVisualStyleBackColor = true;
			// 
			// button_kernelExecute
			// 
			this.button_kernelExecute.Location = new Point(133, 186);
			this.button_kernelExecute.Name = "button_kernelExecute";
			this.button_kernelExecute.Size = new Size(70, 23);
			this.button_kernelExecute.TabIndex = 20;
			this.button_kernelExecute.Text = "Execute";
			this.button_kernelExecute.UseVisualStyleBackColor = true;
			this.button_kernelExecute.Click += this.button_kernelExecute_Click;
			// 
			// button_kernelLoad
			// 
			this.button_kernelLoad.Location = new Point(6, 186);
			this.button_kernelLoad.Name = "button_kernelLoad";
			this.button_kernelLoad.Size = new Size(60, 23);
			this.button_kernelLoad.TabIndex = 19;
			this.button_kernelLoad.Text = "Load";
			this.button_kernelLoad.UseVisualStyleBackColor = true;
			this.button_kernelLoad.Click += this.button_kernelLoad_Click;
			// 
			// comboBox_kernelVersions
			// 
			this.comboBox_kernelVersions.FormattingEnabled = true;
			this.comboBox_kernelVersions.Location = new Point(6, 80);
			this.comboBox_kernelVersions.Name = "comboBox_kernelVersions";
			this.comboBox_kernelVersions.Size = new Size(85, 23);
			this.comboBox_kernelVersions.TabIndex = 18;
			this.comboBox_kernelVersions.Text = "Ver.";
			// 
			// checkBox_kernelInvariantSearch
			// 
			this.checkBox_kernelInvariantSearch.AutoSize = true;
			this.checkBox_kernelInvariantSearch.Checked = true;
			this.checkBox_kernelInvariantSearch.CheckState = CheckState.Checked;
			this.checkBox_kernelInvariantSearch.Location = new Point(97, 82);
			this.checkBox_kernelInvariantSearch.Name = "checkBox_kernelInvariantSearch";
			this.checkBox_kernelInvariantSearch.Size = new Size(106, 19);
			this.checkBox_kernelInvariantSearch.TabIndex = 17;
			this.checkBox_kernelInvariantSearch.Text = "Case-sensitive?";
			this.checkBox_kernelInvariantSearch.UseVisualStyleBackColor = true;
			// 
			// textBox_kernelBaseName
			// 
			this.textBox_kernelBaseName.Location = new Point(6, 51);
			this.textBox_kernelBaseName.Name = "textBox_kernelBaseName";
			this.textBox_kernelBaseName.PlaceholderText = "Kernel base name (search)";
			this.textBox_kernelBaseName.Size = new Size(197, 23);
			this.textBox_kernelBaseName.TabIndex = 16;
			// 
			// comboBox_kernelBaseNames
			// 
			this.comboBox_kernelBaseNames.FormattingEnabled = true;
			this.comboBox_kernelBaseNames.Location = new Point(6, 22);
			this.comboBox_kernelBaseNames.Name = "comboBox_kernelBaseNames";
			this.comboBox_kernelBaseNames.Size = new Size(197, 23);
			this.comboBox_kernelBaseNames.TabIndex = 15;
			this.comboBox_kernelBaseNames.Text = "Unique kernel base names";
			// 
			// label_kernelCurrentName
			// 
			this.label_kernelCurrentName.AutoSize = true;
			this.label_kernelCurrentName.Location = new Point(447, 16);
			this.label_kernelCurrentName.Name = "label_kernelCurrentName";
			this.label_kernelCurrentName.Size = new Size(100, 15);
			this.label_kernelCurrentName.TabIndex = 14;
			this.label_kernelCurrentName.Text = "No kernel loaded.";
			// 
			// panel_kernelArguments
			// 
			this.panel_kernelArguments.BackColor = SystemColors.Control;
			this.panel_kernelArguments.Location = new Point(12, 41);
			this.panel_kernelArguments.Name = "panel_kernelArguments";
			this.panel_kernelArguments.Size = new Size(400, 233);
			this.panel_kernelArguments.TabIndex = 15;
			// 
			// textBox_kernelCode
			// 
			this.textBox_kernelCode.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point,  0);
			this.textBox_kernelCode.Location = new Point(12, 280);
			this.textBox_kernelCode.MaxLength = 99999999;
			this.textBox_kernelCode.Multiline = true;
			this.textBox_kernelCode.Name = "textBox_kernelCode";
			this.textBox_kernelCode.ReadOnly = true;
			this.textBox_kernelCode.Size = new Size(644, 282);
			this.textBox_kernelCode.TabIndex = 16;
			// 
			// button_darkMode
			// 
			this.button_darkMode.Location = new Point(1420, 568);
			this.button_darkMode.Name = "button_darkMode";
			this.button_darkMode.Size = new Size(85, 23);
			this.button_darkMode.TabIndex = 17;
			this.button_darkMode.Text = "Dark mode";
			this.button_darkMode.UseVisualStyleBackColor = true;
			this.button_darkMode.Click += this.button_darkMode_Click;
			// 
			// label_imagesCount
			// 
			this.label_imagesCount.AutoSize = true;
			this.label_imagesCount.Location = new Point(1336, 592);
			this.label_imagesCount.Name = "label_imagesCount";
			this.label_imagesCount.Size = new Size(62, 15);
			this.label_imagesCount.TabIndex = 18;
			this.label_imagesCount.Text = "Images (0)";
			// 
			// label_pointersCount
			// 
			this.label_pointersCount.AutoSize = true;
			this.label_pointersCount.Location = new Point(1502, 592);
			this.label_pointersCount.Name = "label_pointersCount";
			this.label_pointersCount.Size = new Size(67, 15);
			this.label_pointersCount.TabIndex = 19;
			this.label_pointersCount.Text = "Pointers (0)";
			// 
			// button_createEmpty
			// 
			this.button_createEmpty.Location = new Point(1255, 627);
			this.button_createEmpty.Name = "button_createEmpty";
			this.button_createEmpty.Size = new Size(75, 23);
			this.button_createEmpty.TabIndex = 20;
			this.button_createEmpty.Text = "Create";
			this.button_createEmpty.UseVisualStyleBackColor = true;
			this.button_createEmpty.Click += this.button_createEmpty_Click;
			// 
			// button_createColor
			// 
			this.button_createColor.BackColor = Color.Black;
			this.button_createColor.ForeColor = Color.White;
			this.button_createColor.Location = new Point(1255, 656);
			this.button_createColor.Name = "button_createColor";
			this.button_createColor.Size = new Size(75, 23);
			this.button_createColor.TabIndex = 21;
			this.button_createColor.Text = "Color";
			this.button_createColor.UseVisualStyleBackColor = false;
			this.button_createColor.Click += this.button_createColor_Click;
			// 
			// numericUpDown_createSize
			// 
			this.numericUpDown_createSize.Location = new Point(1255, 685);
			this.numericUpDown_createSize.Maximum = new decimal(new int[] { 16384, 0, 0, 0 });
			this.numericUpDown_createSize.Minimum = new decimal(new int[] { 16, 0, 0, 0 });
			this.numericUpDown_createSize.Name = "numericUpDown_createSize";
			this.numericUpDown_createSize.Size = new Size(75, 23);
			this.numericUpDown_createSize.TabIndex = 22;
			this.numericUpDown_createSize.Value = new decimal(new int[] { 1024, 0, 0, 0 });
			// 
			// checkBox_mandelbrotMode
			// 
			this.checkBox_mandelbrotMode.AutoSize = true;
			this.checkBox_mandelbrotMode.Location = new Point(12, 568);
			this.checkBox_mandelbrotMode.Name = "checkBox_mandelbrotMode";
			this.checkBox_mandelbrotMode.Size = new Size(122, 19);
			this.checkBox_mandelbrotMode.TabIndex = 23;
			this.checkBox_mandelbrotMode.Text = "Mandelbrot mode";
			this.checkBox_mandelbrotMode.UseVisualStyleBackColor = true;
			this.checkBox_mandelbrotMode.CheckedChanged += this.checkBox_mandelbrotMode_CheckedChanged;
			// 
			// WindowMain
			// 
			this.AutoScaleDimensions = new SizeF(7F, 15F);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.ClientSize = new Size(1674, 821);
			this.Controls.Add(this.checkBox_mandelbrotMode);
			this.Controls.Add(this.numericUpDown_createSize);
			this.Controls.Add(this.button_createColor);
			this.Controls.Add(this.button_createEmpty);
			this.Controls.Add(this.label_pointersCount);
			this.Controls.Add(this.label_imagesCount);
			this.Controls.Add(this.button_darkMode);
			this.Controls.Add(this.textBox_kernelCode);
			this.Controls.Add(this.panel_kernelArguments);
			this.Controls.Add(this.label_kernelCurrentName);
			this.Controls.Add(this.groupBox_kernel);
			this.Controls.Add(this.button_recenter);
			this.Controls.Add(this.button_info);
			this.Controls.Add(this.label_meta);
			this.Controls.Add(this.numericUpDown_zoom);
			this.Controls.Add(this.panel_view);
			this.Controls.Add(this.listBox_pointers);
			this.Controls.Add(this.button_import);
			this.Controls.Add(this.button_export);
			this.Controls.Add(this.button_reset);
			this.Controls.Add(this.button_move);
			this.Controls.Add(this.comboBox_devices);
			this.Controls.Add(this.listBox_images);
			this.Controls.Add(this.listBox_log);
			this.MaximumSize = new Size(1690, 860);
			this.MinimumSize = new Size(1690, 860);
			this.Name = "WindowMain";
			this.Text = "OpenCL-Bitmaps (Kernel launcher on images)";
			this.panel_view.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize) this.pictureBox_view).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_zoom).EndInit();
			this.groupBox_kernel.ResumeLayout(false);
			this.groupBox_kernel.PerformLayout();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_createSize).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ListBox listBox_log;
		private ListBox listBox_images;
		private ComboBox comboBox_devices;
		private Button button_move;
		private Button button_reset;
		private Button button_export;
		private Button button_import;
		private ListBox listBox_pointers;
		private Panel panel_view;
		private PictureBox pictureBox_view;
		private NumericUpDown numericUpDown_zoom;
		private Label label_meta;
		private Button button_info;
		private Button button_recenter;
		private GroupBox groupBox_kernel;
		private ComboBox comboBox_kernelBaseNames;
		private Label label_kernelCurrentName;
		private CheckBox checkBox_kernelInvariantSearch;
		private TextBox textBox_kernelBaseName;
		private Panel panel_kernelArguments;
		private TextBox textBox_kernelCode;
		private CheckBox checkBox_kernelOop;
		private Button button_kernelExecute;
		private Button button_kernelLoad;
		private ComboBox comboBox_kernelVersions;
		private Button button_darkMode;
		private Button button_kernelCreate;
		private Label label_imagesCount;
		private Label label_pointersCount;
		private Button button_createEmpty;
		private Button button_createColor;
		private NumericUpDown numericUpDown_createSize;
		private CheckBox checkBox_mandelbrotMode;
		private Button button_kernelUnload;
	}
}
