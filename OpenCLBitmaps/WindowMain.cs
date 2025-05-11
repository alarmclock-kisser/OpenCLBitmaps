namespace OpenCLBitmaps
{
	public partial class WindowMain : Form
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Repopath;

		public ImageHandling ImgH;
		public ClContextHandling ClH;


		public bool DarkMode = false;

		// Appsettings
		public string AppsettingsPath => Directory.GetFiles(this.Repopath, "*config", SearchOption.AllDirectories).FirstOrDefault() ?? "";

		// ----- ----- ----- LAMBDA ----- ----- ----- \\
		public ImageObject? ImgObj => this.ImgH?.CurrentObject;
		public Image? Img => this.ImgH?.CurrentImage;

		public ClMemoryHandling? MemoryH => this.ClH.MemoryH;
		public ClKernelHandling? KernelH => this.ClH.KernelH;




		// ----- ----- ----- CONSTRUCTOR ----- ----- ----- \\
		public WindowMain()
		{
			this.InitializeComponent();

			// Set repopath
			this.Repopath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

			// Window position
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(0, 0);

			// Init. classes
			this.ImgH = new ImageHandling(this.Repopath, this.listBox_images, this.pictureBox_view, this.numericUpDown_zoom, this.label_meta);
			this.ClH = new ClContextHandling(this.Repopath, this.listBox_log, this.comboBox_devices);

			// Register events
			this.comboBox_kernelBaseNames.SelectedIndexChanged += (s, e) => this.FillKernelVersions();
			this.textBox_kernelBaseName.TextChanged += (s, e) => this.FillKernelVersions();
			this.checkBox_kernelInvariantSearch.CheckedChanged += (s, e) => this.FillKernelVersions();

			// Apply appsettings
			this.ApplyAppsettings();

			// Start UI
			this.UpdateView();

		}





		// ----- ----- ----- METHODS ----- ----- ----- \\
		public void ApplyAppsettings()
		{
			// Check file
			if (!File.Exists(this.AppsettingsPath))
			{
				MessageBox.Show("Appsettings file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Get lines with text and without // at start
			string[] lines = File.ReadAllLines(this.AppsettingsPath)
				.Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("//"))
				.ToArray();

			// Parse import size
			int maxImportSize = lines.Where(line => line.TrimStart().StartsWith("maxImportSizeMB"))
				.Select(line => int.Parse(line.Split('=')[1].Trim()))
				.FirstOrDefault();

			// Parse default dark mode
			bool defaultDarkMode = bool.TryParse(lines.Where(line => line.TrimStart().StartsWith("defaultDarkMode"))
				.Select(line => line.Split('=')[1].Trim())
				.FirstOrDefault(),
				out Boolean result) && result;

			// Parse auto init device
			string initDevice = lines.Where(line => line.TrimStart().StartsWith("autoInit"))
				.Select(line => line.Split('=')[1].Trim().Trim('"'))
				.FirstOrDefault() ?? "";
			
			// Parse precompile all
			bool precompileAll = bool.TryParse(lines.Where(line => line.TrimStart().StartsWith("precompileAll"))
				.Select(line => line.Split('=')[1].Trim())
				.FirstOrDefault(),
				out Boolean result3) && result3;
			
			// Parse load latest kernel
			bool loadLatestKernel = bool.TryParse(lines.Where(line => line.TrimStart().StartsWith("loadLatestKernel"))
				.Select(line => line.Split('=')[1].Trim())
				.FirstOrDefault(),
				out Boolean result2) && result2;

			

			// Apply import size
			this.ImgH.LoadResourcesImages(maxImportSize);
			
			// Apply dark mode
			this.DarkMode = defaultDarkMode;
			DarkModeToggle.ToggleDarkMode(this, this.DarkMode);
			
			// Apply auto init device
			if (!string.IsNullOrEmpty(initDevice) && initDevice != "none")
			{
				this.ClH.SelectDeviceLike(initDevice);
			}

			// Apply precompile all
			if (precompileAll)
			{
				this.KernelH?.PrecompileAllKernels();
			}

			// Apply load latest kernel
			if (loadLatestKernel)
			{
				string latestFile = this.KernelH?.GetLatestKernelFile() ?? "";
				string latestName = Path.GetFileNameWithoutExtension(latestFile);
				this.LoadKernel(latestName);
			}

			
		}

		public void UpdateView()
		{
			// Update view
			this.ImgH.FillImagesListBox();

			// Fill pointers listbox
			this.MemoryH?.FillPointersListbox(this.listBox_pointers);

			// Fill unique kernel base names combobox
			this.KernelH?.FillGenericKernelNamesCombobox(this.comboBox_kernelBaseNames);

			// Load kernel code textbox
			this.KernelH?.FillKernelCodeTextbox(this.textBox_kernelCode);

			// Re-set dark mode
			DarkModeToggle.ToggleDarkMode(this, this.DarkMode);
		}

		public string? MakeLogFile(string logText = "")
		{
			string dir = Path.Combine(this.Repopath, "Resources", "Logs");
			if (!Directory.Exists(dir))
			{
				MessageBox.Show("Log directory does not exist. Creating it now.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				Directory.CreateDirectory(dir);
			}

			string fileName = "Log_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";

			string path = Path.Combine(dir, fileName);

			if (File.Exists(path))
			{
				MessageBox.Show("Log file already exists. Overwriting it.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}

			// Try write log
			try
			{
				File.WriteAllText(path, logText);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to write log file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}

			// Return path
			return path;
		}

		public void MoveImage(int index = -1)
		{
			// Get obj
			if (index == -1)
			{
				index = this.listBox_images.SelectedIndex;
			}
			if (index == -1)
			{
				this.listBox_log.Items.Add("No image selected.");
				return;
			}
			if (index < 0 || index >= this.ImgH.Images.Count)
			{
				this.listBox_log.Items.Add("Invalid image index.");
				return;
			}

			// Check object
			ImageObject obj = this.ImgH.Images[index];

			// Check initialized
			if (this.MemoryH == null)
			{
				this.listBox_log.Items.Add("No OpenCL device initialized.");
				return;
			}

			// Move image data Host <--> Device
			if (obj.OnHost)
			{
				// Get bytes
				byte[] bytes = obj.GetPixelsAsBytes(true);

				// Push bytes -> pointer
				obj.Pointer = this.MemoryH.PushData(bytes);
			}
			else if (obj.OnDevice)
			{
				// Get pointer
				long pointer = obj.Pointer;

				// Pull pointer -> bytes
				byte[] bytes = this.MemoryH.PullData<byte>(pointer);

				// Set image from bytes
				obj.SetImageFromBytes(bytes, true);
			}
			else
			{
				MessageBox.Show("Image data is neither on host nor on device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Update view
			this.UpdateView();
		}

		public void LoadKernel(string kernelName = "")
		{
			// Get name
			if (string.IsNullOrEmpty(kernelName))
			{
				kernelName = this.textBox_kernelBaseName.Text + this.comboBox_kernelVersions.SelectedItem?.ToString() ?? "01";
			}

			// Try load kernel
			this.KernelH?.LoadKernel(kernelName, "", this.panel_kernelArguments);
			if (this.KernelH?.Kernel == null || this.KernelH?.KernelFile == null)
			{
				MessageBox.Show("Kernel not loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Set kernel base name
			this.textBox_kernelBaseName.Text = kernelName.Substring(0, kernelName.Length - 2);
			this.comboBox_kernelVersions.SelectedItem = kernelName.Substring(kernelName.Length - 2, 2);

			// Update view
			this.UpdateView();
		}

		public void ExecuteKernelIP(string kernelName = "")
		{
			// Get name parts
			string baseName = this.textBox_kernelBaseName.Text;
			string version = this.comboBox_kernelVersions.SelectedItem?.ToString() ?? "01";

			// Verify name
			if (string.IsNullOrEmpty(kernelName))
			{
				kernelName = this.textBox_kernelBaseName.Text + this.comboBox_kernelVersions.SelectedItem?.ToString() ?? "01";
			}

			// Check object
			if (this.ImgObj == null)
			{
				MessageBox.Show("No image selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Check initialized
			if (this.MemoryH == null || this.KernelH == null)
			{
				MessageBox.Show("No OpenCL device initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Get variable args
			object[] arguments = this.KernelH.GetArgumentValues();

			// Load kernel
			if (this.KernelH.Kernel == null || this.KernelH.KernelFile == null)
			{
				this.KernelH.LoadKernel(kernelName, "", this.panel_kernelArguments);

				if (this.KernelH?.Kernel == null || this.KernelH?.KernelFile == null)
				{
					MessageBox.Show("Kernel not loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				return;
			}

			// Optionally move image to device
			bool moved = false;
			if (this.ImgObj.OnHost)
			{
				this.MoveImage(this.listBox_images.SelectedIndex);
				moved = true;
			}

			// Get image pointer & dimensions
			long pointer = this.ImgObj.Pointer;
			int width = this.ImgObj.Width;
			int height = this.ImgObj.Height;

			// Call exec generic kernel
			this.ImgObj.Pointer = this.KernelH.ExecuteKernelIPGeneric(version, baseName, pointer, width, height, arguments, true);

			// Optionally move back
			if (moved)
			{
				this.MoveImage(this.listBox_images.SelectedIndex);
			}

			// Update view
			this.UpdateView();
		}

		public void ExecuteKernelOOP(string kernelName = "")
		{
			// Get name parts
			string baseName = this.textBox_kernelBaseName.Text;
			string version = this.comboBox_kernelVersions.SelectedItem?.ToString() ?? "01";

			// Verify name
			if (string.IsNullOrEmpty(kernelName))
			{
				kernelName = this.textBox_kernelBaseName.Text + this.comboBox_kernelVersions.SelectedItem?.ToString() ?? "01";
			}

			// Check object
			if (this.ImgObj == null)
			{
				MessageBox.Show("No image selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Check initialized
			if (this.MemoryH == null || this.KernelH == null)
			{
				MessageBox.Show("No OpenCL device initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Get variable args
			object[] arguments = this.KernelH.GetArgumentValues();

			// Load kernel
			if (this.KernelH.Kernel == null || this.KernelH.KernelFile == null)
			{
				this.KernelH.LoadKernel(kernelName, "", this.panel_kernelArguments);

				if (this.KernelH?.Kernel == null || this.KernelH?.KernelFile == null)
				{
					MessageBox.Show("Kernel not loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				return;
			}

			// Optionally move image to device
			bool moved = false;
			if (this.ImgObj.OnHost)
			{
				this.MoveImage(this.listBox_images.SelectedIndex);
				moved = true;
			}

			// Get image pointer & dimensions
			long pointer = this.ImgObj.Pointer;
			int width = this.ImgObj.Width;
			int height = this.ImgObj.Height;

			// Call exec generic kernel
			this.ImgObj.Pointer = this.KernelH.ExecuteKernelOOPGeneric(version, baseName, pointer, width, height, arguments, true);

			// Optionally move back
			if (moved)
			{
				this.MoveImage(this.listBox_images.SelectedIndex);
			}

			// Update view
			this.UpdateView();
		}



		// ----- ----- ----- PRIVATE METHODS ----- ----- ----- \\

		private void FillKernelVersions()
		{
			this.KernelH?.FillGenericKernelVersionsCombobox(this.comboBox_kernelVersions, this.comboBox_kernelBaseNames.SelectedItem?.ToString() ?? this.textBox_kernelBaseName.Text, this.checkBox_kernelInvariantSearch.Checked);
			this.textBox_kernelBaseName.Text = this.comboBox_kernelBaseNames.SelectedItem?.ToString() ?? this.textBox_kernelBaseName.Text;
		}



		// ----- ----- ----- EVENTS ----- ----- ----- \\

		private void button_info_Click(object sender, EventArgs e)
		{
			// If CTRL down
			if (ModifierKeys == Keys.Control)
			{
				this.ClH.GetInfoDeviceInfo(null, false, true);
			}
			else
			{
				this.ClH.GetInfoPlatformInfo(null, false, true);
			}

		}

		private void button_import_Click(object sender, EventArgs e)
		{
			this.ImgH.ImportImage();
		}

		private void button_export_Click(object sender, EventArgs e)
		{
			// If CTRL down
			if (ModifierKeys == Keys.Control)
			{
				string logFileText = DarkModeToggle.CopyLogToClipboard(this.listBox_log, false) ?? "No log to copy. This looks like an error.";
				string logFilePath = this.MakeLogFile(logFileText) ?? "No log file created. This looks like an error.";
			}
			else
			{
				// Export image
				this.ImgObj?.Export(true);
			}
		}

		private void button_recenter_Click(object sender, EventArgs e)
		{
			this.ImgH.CenterImage();
		}

		private void button_reset_Click(object sender, EventArgs e)
		{
			this.ImgObj?.ResetImage();
		}

		private void button_move_Click(object sender, EventArgs e)
		{
			this.MoveImage();
		}

		private void button_kernelLoad_Click(object sender, EventArgs e)
		{
			// If CTRL down, load latest
			if (ModifierKeys == Keys.Control)
			{
				string latestFile = this.KernelH?.GetLatestKernelFile() ?? "";
				string latestName = Path.GetFileNameWithoutExtension(latestFile);

				this.LoadKernel(latestName);
			}
			else
			{
				this.LoadKernel();
			}
		}

		private void button_kernelExecute_Click(object sender, EventArgs e)
		{
			// If checkbox checked execute OOP
			if (this.checkBox_kernelOop.Checked)
			{
				this.ExecuteKernelOOP();
			}
			else
			{
				this.ExecuteKernelIP();
			}
		}

		private void button_darkMode_Click(object sender, EventArgs e)
		{
			this.DarkMode = !this.DarkMode;
			DarkModeToggle.ToggleDarkMode(this, this.DarkMode);
		}

		private void listBox_log_DoubleClick(object sender, EventArgs e)
		{
			DarkModeToggle.CopyLogToClipboard(this.listBox_log, true);
		}

		private void button_kernelCreate_Click(object sender, EventArgs e)
		{
			Form window = ClKernelCreationWindow.OpenCreationWindow();
		}
	}
}
