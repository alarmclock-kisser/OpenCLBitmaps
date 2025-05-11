using System.Diagnostics;

namespace OpenCLBitmaps
{
	public partial class WindowMain : Form
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Repopath;

		public ImageHandling ImgH;
		public ClContextHandling ClH;


		public bool DarkMode = false;

		private Dictionary<NumericUpDown, int> previousNumericValues = [];
		private bool isProcessing = false;

		private bool mandelbrotMode = false;
		private float mandelbrotZoomFactor = 1.0f;
		private Point mandelbrotMouseDownLocation;
		private bool mandelbrotIsDragging = false;
		private bool ctrlKeyPressed = false;
		private bool kernelExecutionRequired = false;


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
			this.RegisterNumericToSecondPow(this.numericUpDown_createSize);

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
			// Fill images listbox & set label
			this.ImgH.FillImagesListBox();
			this.label_imagesCount.Text = "Images: (" + this.listBox_images.Items.Count.ToString() + ")";

			// Fill pointers listbox & set label
			this.MemoryH?.FillPointersListbox(this.listBox_pointers);
			this.label_pointersCount.Text = "Pointers: (" + this.listBox_pointers.Items.Count.ToString() + ")";

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

			// If name is MandelbrotXX, set events to toggle arguments + exec instantly
			if (kernelName.StartsWith("Mandelbrot") && kernelName.Length == "Mandelbrot".Length + 2)
			{
				this.ToggleMandelbrotMode();
			}

			// Update view
			this.UpdateView();
		}

		public void ExecuteKernelIP(string kernelName = "", bool createNew = false)
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
			ImageObject? current = this.ImgObj;
			ImageObject? original = this.ImgObj;
			if (current == null || original == null)
			{
				MessageBox.Show("No image selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Optionally create new object from current
			if (createNew)
			{
				current = this.ImgH.Clone();

				if (current == null)
				{
					MessageBox.Show("Failed to clone image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
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
			if (current.OnHost)
			{
				this.MoveImage(this.ImgH.Images.IndexOf(original));

				moved = true;
			}

			// Get image pointer & dimensions
			long pointer = current.Pointer;
			int width = current.Width;
			int height = current.Height;
			int channels = 4;
			int bitdepth = current.BitsPerPixel / channels;

			// Call exec generic kernel
			current.Pointer = this.KernelH.ExecuteKernelIPGeneric(version, baseName, pointer, width, height, channels, bitdepth, arguments, true);

			// Optionally move back
			if (moved)
			{
				this.MoveImage(this.ImgH.Images.IndexOf(current));
			}

			// Update view
			this.UpdateView();
		}

		public void ExecuteKernelOOP(string kernelName = "", bool createNew = false)
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
			ImageObject? current = this.ImgObj;
			ImageObject? original = this.ImgObj;
			if (current == null || original == null)
			{
				MessageBox.Show("No image selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Optionally create new object from current
			if (createNew)
			{
				current = this.ImgH.Clone();

				if (current == null)
				{
					MessageBox.Show("Failed to clone image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
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
			if (current.OnHost)
			{
				this.MoveImage(this.ImgH.Images.IndexOf(original));

				moved = true;
			}

			// Get image pointer & dimensions
			long pointer = current.Pointer;
			int width = current.Width;
			int height = current.Height;
			int channels = 4;
			int bitdepth = current.BitsPerPixel / channels;

			// Call exec generic kernel
			current.Pointer = this.KernelH.ExecuteKernelOOPGeneric(version, baseName, pointer, width, height, channels, bitdepth, arguments, true);

			// Optionally move back
			if (moved)
			{
				this.MoveImage(this.ImgH.Images.IndexOf(current));
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

		private void RegisterNumericToSecondPow(NumericUpDown numeric)
		{
			// Initialwert speichern
			this.previousNumericValues.Add(numeric, (int) numeric.Value);

			numeric.ValueChanged += (s, e) =>
			{
				// Verhindere rekursive Aufrufe
				if (this.isProcessing)
				{
					return;
				}

				this.isProcessing = true;

				try
				{
					int newValue = (int) numeric.Value;
					int oldValue = this.previousNumericValues[numeric];
					int max = (int) numeric.Maximum;
					int min = (int) numeric.Minimum;

					// Nur verarbeiten, wenn sich der Wert tatsächlich geändert hat
					if (newValue != oldValue)
					{
						int calculatedValue;

						if (newValue > oldValue)
						{
							// Verdoppeln, aber nicht über Maximum
							calculatedValue = Math.Min(oldValue * 2, max);
						}
						else if (newValue < oldValue)
						{
							// Halbieren, aber nicht unter Minimum
							calculatedValue = Math.Max(oldValue / 2, min);
						}
						else
						{
							calculatedValue = oldValue;
						}

						// Nur aktualisieren wenn notwendig
						if (calculatedValue != newValue)
						{
							numeric.Value = calculatedValue;
						}

						this.previousNumericValues[numeric] = calculatedValue;
					}
				}
				finally
				{
					this.isProcessing = false;
				}
			};
		}

		private void ToggleMandelbrotMode()
		{
			// 1. Temporär Event-Handler entfernen
			this.checkBox_mandelbrotMode.CheckedChanged -= this.checkBox_mandelbrotMode_CheckedChanged;

			// 2. Modus umschalten
			this.mandelbrotMode = !this.mandelbrotMode;
			this.checkBox_mandelbrotMode.Checked = this.mandelbrotMode;

			// 3. Alle bestehenden Event-Handler entfernen (sauberer Reset)
			this.pictureBox_view.MouseDown -= this.ImgH.ViewPBox_MouseDown;
			this.pictureBox_view.MouseMove -= this.ImgH.ViewPBox_MouseMove;
			this.pictureBox_view.MouseUp -= this.ImgH.ViewPBox_MouseUp;
			this.pictureBox_view.MouseWheel -= this.ImgH.ViewPBox_MouseWheel;

			this.pictureBox_view.MouseDown -= this.pictureBox_view_MouseDown;
			this.pictureBox_view.MouseMove -= this.pictureBox_view_MouseMove;
			this.pictureBox_view.MouseUp -= this.pictureBox_view_MouseUp;
			this.pictureBox_view.MouseWheel -= this.pictureBox_view_MouseWheel;

			// 4. Neue Event-Handler registrieren
			if (this.mandelbrotMode)
			{
				this.pictureBox_view.MouseDown += this.pictureBox_view_MouseDown;
				this.pictureBox_view.MouseMove += this.pictureBox_view_MouseMove;
				this.pictureBox_view.MouseUp += this.pictureBox_view_MouseUp;
				this.pictureBox_view.MouseWheel += this.pictureBox_view_MouseWheel;
			}
			else if (this.ImgH != null)
			{
				this.pictureBox_view.MouseDown += this.ImgH.ViewPBox_MouseDown;
				this.pictureBox_view.MouseMove += this.ImgH.ViewPBox_MouseMove;
				this.pictureBox_view.MouseUp += this.ImgH.ViewPBox_MouseUp;
				this.pictureBox_view.MouseWheel += this.ImgH.ViewPBox_MouseWheel;
			}

			// 5. Event-Handler wieder registrieren
			this.checkBox_mandelbrotMode.CheckedChanged += this.checkBox_mandelbrotMode_CheckedChanged;
		}


		// ----- ----- ----- Mandelbrot EVENTS ----- ----- ----- \\
		private void pictureBox_view_MouseDown(object? sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.mandelbrotIsDragging = true;
				this.mandelbrotMouseDownLocation = e.Location;
			}
		}

		private void pictureBox_view_MouseMove(object? sender, MouseEventArgs e)
		{
			if (!this.mandelbrotIsDragging || this.ImgObj == null)
			{
				return;
			}

			try
			{
				// 1. Find NumericUpDown controls more efficiently
				NumericUpDown? numericX = this.Controls.Find("argInput_offsetX", true).FirstOrDefault() as NumericUpDown;
				NumericUpDown? numericY = this.Controls.Find("argInput_offsetY", true).FirstOrDefault() as NumericUpDown;
				NumericUpDown? numericZ = this.Controls.Find("argInput_zoom", true).FirstOrDefault() as NumericUpDown;
				if (numericX == null || numericY == null || numericZ == null)
				{
					this.KernelH?.Log("Offset & zoom controls not found!", "", 3);
					return;
				}

				// 2. Calculate smooth delta with sensitivity factor and zoom
				float sensitivity = 0.001f * (float) (1 / numericZ.Value);
				decimal deltaX = (decimal) ((e.Location.X - this.mandelbrotMouseDownLocation.X) * -sensitivity);
				decimal deltaY = (decimal) ((e.Location.Y - this.mandelbrotMouseDownLocation.Y) * -sensitivity);

				// 3. Update values with boundary checks
				this.UpdateNumericValue(numericX, deltaX);
				this.UpdateNumericValue(numericY, deltaY);

				// 4. Update mouse position for smoother continuous dragging
				this.mandelbrotMouseDownLocation = e.Location;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"MouseMove error: {ex.Message}");
			}
		}

		private void UpdateNumericValue(NumericUpDown numeric, decimal delta)
		{
			decimal newValue = numeric.Value + delta;

			// Ensure value stays within allowed range
			if (newValue < numeric.Minimum)
			{
				newValue = numeric.Minimum;
			}

			if (newValue > numeric.Maximum)
			{
				newValue = numeric.Maximum;
			}

			numeric.Value = newValue;
		}

		private void pictureBox_view_MouseUp(object? sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.mandelbrotIsDragging = false;

				// Re-execute kernel
				this.button_kernelExecute_Click(sender, e);
			}
		}

		private void pictureBox_view_MouseWheel(object? sender, MouseEventArgs e)
		{
			// Check for CTRL key press
			if (ModifierKeys == Keys.Control)
			{
				ctrlKeyPressed = true;
				kernelExecutionRequired = true; // Set the flag
				NumericUpDown? numericI = this.Controls.Find("argInput_maxIter", true).FirstOrDefault() as NumericUpDown;
				if (numericI == null)
				{
					this.KernelH?.Log("MaxIter control not found!", "", 3);
					return;
				}

				// Increase/Decrease maxIter
				if (e.Delta > 0)
				{
					numericI.Value++;
				}
				else if (e.Delta < 0)
				{
					if (numericI.Value > 0)
					{
						numericI.Value--;
					}
				}
				return;
			}

			// Check if CTRL key was previously pressed
			if (ctrlKeyPressed)
			{
				ctrlKeyPressed = false; // Reset the flag
				kernelExecutionRequired = true;
			}

			// 1. Find NumericUpDown controls more efficiently
			NumericUpDown? numericZ = this.Controls.Find("argInput_zoom", true).FirstOrDefault() as NumericUpDown;
			if (numericZ == null)
			{
				this.KernelH?.Log("Zoom control not found!", "", 3);
				return;
			}

			// 2. Calculate zoom factor
			if (e.Delta > 0)
			{
				this.mandelbrotZoomFactor *= 1.1f;
			}
			else
			{
				this.mandelbrotZoomFactor /= 1.1f;
			}

			// 3. Update zoom value with boundary checks
			decimal newValue = (decimal) this.mandelbrotZoomFactor;
			if (newValue < numericZ.Minimum)
			{
				newValue = numericZ.Minimum;
			}
			if (newValue > numericZ.Maximum)
			{
				newValue = numericZ.Maximum;
			}
			numericZ.Value = newValue;

			// Call re-exec kernel
			kernelExecutionRequired = true;

			if (!ctrlKeyPressed && kernelExecutionRequired)
			{
				kernelExecutionRequired = false;
				this.button_kernelExecute_Click(sender, e);
			}
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

		private void button_createColor_Click(object sender, EventArgs e)
		{
			// Show color dialog
			ColorDialog colorDialog = new ColorDialog();
			colorDialog.AllowFullOpen = true;
			colorDialog.ShowHelp = true;
			colorDialog.Color = this.button_createColor.BackColor;
			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				this.button_createColor.BackColor = colorDialog.Color;
				if (colorDialog.Color.GetBrightness() < 0.5)
				{
					this.button_createColor.ForeColor = Color.White;
				}
				else
				{
					this.button_createColor.ForeColor = Color.Black;
				}
			}
		}

		private void button_createEmpty_Click(object sender, EventArgs e)
		{
			this.ImgH.CreateEmpty(this.button_createColor.BackColor, (int) this.numericUpDown_createSize.Value);
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

			this.pictureBox_view.Image = this.ImgObj?.Img;
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

			// Toggle OOP mode optionally
			if (this.KernelH?.GetArgumentPointerCount() == 1)
			{
				this.checkBox_kernelOop.Checked = false;
			}
			else
			{
				this.checkBox_kernelOop.Checked = true;
			}
		}

		private void button_kernelUnload_Click(object sender, EventArgs e)
		{
			this.KernelH?.UnloadKernel();

			this.UpdateView();
		}

		private void button_kernelExecute_Click(object? sender, EventArgs e)
		{
			// Get CTRL flag for creating new file for buffer
			bool newFile = ModifierKeys == Keys.Control;

			// Toggle checkbox
			if (this.KernelH?.GetArgumentPointerCount() == 1)
			{
				this.checkBox_kernelOop.Checked = false;
			}
			else
			{
				this.checkBox_kernelOop.Checked = true;
			}

			// If checkbox checked execute OOP
			if (this.checkBox_kernelOop.Checked)
			{
				this.ExecuteKernelOOP("", newFile);
			}
			else
			{
				this.ExecuteKernelIP("", newFile);
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

		private void checkBox_mandelbrotMode_CheckedChanged(object? sender, EventArgs e)
		{
			this.ToggleMandelbrotMode();
		}
	}
}
