using OpenTK.Compute.OpenCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenCLBitmaps
{
	public class ClKernelHandling
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		private string Repopath;
		private ListBox LogList;
		private ClMemoryHandling MemH;
		private CLContext CTX;
		private CLDevice DEV;
		private CLPlatform PLAT;
		private CLCommandQueue QUE;



		public CLKernel? Kernel = null;
		public string? KernelFile = null;

		public Panel? InputPanel = null;
		public long InputBufferPointer = 0;




		// ----- ----- ----- LAMBDA ----- ----- ----- \\
		public Dictionary<string, string> Files => this.GetKernelFiles();

		public Dictionary<string, Type> Arguments => this.GetKernelArguments();



		// ----- ----- ----- CONSTRUCTOR ----- ----- ----- \\
		public ClKernelHandling(string repopath, ClMemoryHandling memH, ListBox logList, CLContext ctx, CLDevice dev, CLPlatform plat, CLCommandQueue que)
		{
			// Set attributes
			this.Repopath = repopath;
			this.LogList = logList;
			this.MemH = memH;
			this.CTX = ctx;
			this.DEV = dev;
			this.PLAT = plat;
			this.QUE = que;


		}


		// ----- ----- ----- METHODS ----- ----- ----- \\
		// Log
		public void Log(string message = "", string inner = "", int indent = 0)
		{
			string msg = "[Kernel]: " + new string(' ', indent * 2) + message;

			if (!string.IsNullOrEmpty(inner))
			{
				msg += " (" + inner + ")";
			}

			// Add to logList
			this.LogList.Items.Add(msg);

			// Scroll down
			this.LogList.SelectedIndex = this.LogList.Items.Count - 1;
		}


		// Dispose
		public void Dispose()
		{
			// Dispose
		}



		// Files
		public Dictionary<string, string> GetKernelFiles()
		{
			string dir = Path.Combine(this.Repopath, "Resources", "Kernels");

			// Build dir if it doesn't exist
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			// Get all .cl files in the directory
			string[] files = Directory.GetFiles(dir, "*.cl", SearchOption.AllDirectories);

			// Check if any files were found
			if (files.Length == 0)
			{
				this.Log("No kernel files found in directory: " + dir);
				return [];
			}

			// Verify each file
			Dictionary<string, string> verifiedFiles = [];
			foreach (string file in files)
			{
				string? verifiedFile = this.VerifyKernelFile(file);
				if (verifiedFile != null)
				{
					string? name = this.GetKernelName(verifiedFile);
					verifiedFiles.Add(verifiedFile, name ?? "N/A");
				}
			}

			// Return
			return verifiedFiles;
		}

		public string? VerifyKernelFile(string filePath)
		{
			// Check if file exists & is .cl
			if (!File.Exists(filePath))
			{
				this.Log("Kernel file not found: " + filePath);
				return null;
			}

			if (Path.GetExtension(filePath) != ".cl")
			{
				this.Log("Kernel file is not a .cl file: " + filePath);
				return null;
			}

			// Check if file is empty
			string[] lines = File.ReadAllLines(filePath);
			if (lines.Length == 0)
			{
				this.Log("Kernel file is empty: " + filePath);
				return null;
			}

			// Check if file contains kernel function
			if (!lines.Any(line => line.Contains("__kernel")))
			{
				this.Log("Kernel function not found in file: " + filePath);
				return null;
			}

			return Path.GetFullPath(filePath);
		}

		public string? GetKernelName(string filePath)
		{
			// Verify file
			string? verifiedFilePath = this.VerifyKernelFile(filePath);
			if (verifiedFilePath == null)
			{
				return null;
			}

			// Try to extract function name from kernel code text
			string code = File.ReadAllText(filePath);

			// Find index of first "__kernel void "
			int index = code.IndexOf("__kernel void ");
			if (index == -1)
			{
				this.Log("Kernel function not found in file: " + filePath);
				return null;
			}

			// Find index of first "(" after "__kernel void "
			int startIndex = index + "__kernel void ".Length;
			int endIndex = code.IndexOf("(", startIndex);
			if (endIndex == -1)
			{
				this.Log("Kernel function not found in file: " + filePath);
				return null;
			}

			// Extract function name
			string functionName = code.Substring(startIndex, endIndex - startIndex).Trim();
			if (functionName.Contains(" ") || functionName.Contains("\t") ||
				functionName.Contains("\n") || functionName.Contains("\r"))
			{
				this.Log("Kernel function name is invalid: " + functionName);
			}

			// Check if function name is empty
			if (string.IsNullOrEmpty(functionName))
			{
				this.Log("Kernel function name is empty: " + filePath);
				return null;
			}

			// Compare to file name without ext
			string fileName = Path.GetFileNameWithoutExtension(filePath);
			if (string.Compare(functionName, fileName, StringComparison.OrdinalIgnoreCase) != 0)
			{
				this.Log("Kernel function name does not match file name: " + filePath, "", 2);
			}

			return functionName;
		}


		// Compile
		public CLKernel? CompileFile(string filePath)
		{
			// Verify file
			string? verifiedFilePath = this.VerifyKernelFile(filePath);
			if (verifiedFilePath == null)
			{
				return null;
			}

			// Get kernel name
			string? kernelName = this.GetKernelName(verifiedFilePath);
			if (kernelName == null)
			{
				return null;
			}

			// Read kernel code
			string code = File.ReadAllText(verifiedFilePath);

			// Create program
			CLProgram program = CL.CreateProgramWithSource(this.CTX, code, out CLResultCode error);
			if (error != CLResultCode.Success)
			{
				this.Log("Error creating program from source: " + error.ToString());
				return null;
			}

			// Create callback
			CL.ClEventCallback callback = new((program, userData) =>
			{
				// Check build log
				//
			});

			// When building the kernel
			string buildOptions = "-cl-std=CL1.2 -cl-fast-relaxed-math";
			CL.BuildProgram(program, 1, [this.DEV], buildOptions, 0, IntPtr.Zero);

			// Build program
			error = CL.BuildProgram(program, [this.DEV], buildOptions, callback);
			if (error != CLResultCode.Success)
			{
				this.Log("Error building program: " + error.ToString());

				// Get build log
				CLResultCode error2 = CL.GetProgramBuildInfo(program, this.DEV, ProgramBuildInfo.Log, out byte[] buildLog);
				if (error2 != CLResultCode.Success)
				{
					this.Log("Error getting build log: " + error2.ToString());
				}
				else
				{
					string log = Encoding.UTF8.GetString(buildLog);
					this.Log("Build log: " + log, "", 1);
				}

				CL.ReleaseProgram(program);
				return null;
			}

			// Create kernel
			CLKernel kernel = CL.CreateKernel(program, kernelName, out error);
			if (error != CLResultCode.Success)
			{
				this.Log("Error creating kernel: " + error.ToString());

				// Get build log
				CLResultCode error2 = CL.GetProgramBuildInfo(program, this.DEV, ProgramBuildInfo.Log, out byte[] buildLog);
				if (error2 != CLResultCode.Success)
				{
					this.Log("Error getting build log: " + error2.ToString());
				}
				else
				{
					string log = Encoding.UTF8.GetString(buildLog);
					this.Log("Build log: " + log, "", 1);
				}

				CL.ReleaseProgram(program);
				return null;
			}

			// Return kernel
			return kernel;
		}

		public Dictionary<string, Type> GetKernelArguments(CLKernel? kernel = null, string filePath = "")
		{
			Dictionary<string, Type> arguments = [];

			// Verify kernel
			kernel ??= this.Kernel;
			if (kernel == null)
			{
				// Try get kernel by file path
				kernel = this.CompileFile(filePath);
				if (kernel == null)
				{
					this.Log("Kernel is null");
					return arguments;
				}
			}

			// Get kernel info
			CLResultCode error = CL.GetKernelInfo(kernel.Value, KernelInfo.NumberOfArguments, out byte[] argCountBytes);
			if (error != CLResultCode.Success)
			{
				this.Log("Error getting kernel info: " + error.ToString());
				return arguments;
			}

			// Get number of arguments
			int argCount = BitConverter.ToInt32(argCountBytes, 0);

			// Loop through arguments
			for (int i = 0; i < argCount; i++)
			{
				// Get argument info type name
				error = CL.GetKernelArgInfo(kernel.Value, (uint) i, KernelArgInfo.TypeName, out byte[] argTypeBytes);
				if (error != CLResultCode.Success)
				{
					this.Log("Error getting kernel argument info: " + error.ToString());
					continue;
				}

				// Get argument info arg name
				error = CL.GetKernelArgInfo(kernel.Value, (uint) i, KernelArgInfo.Name, out byte[] argNameBytes);
				if (error != CLResultCode.Success)
				{
					this.Log("Error getting kernel argument info: " + error.ToString());
					continue;
				}

				// Get argument type & name
				string argName = Encoding.UTF8.GetString(argNameBytes).TrimEnd('\0');
				string typeName = Encoding.UTF8.GetString(argTypeBytes).TrimEnd('\0');
				Type? type = null;

				// Switch for typeName
				if (typeName.EndsWith("*"))
				{
					typeName = typeName.Replace("*", "");
					switch (typeName)
					{
						case "int":
							type = typeof(int*);
							break;
						case "float":
							type = typeof(float*);
							break;
						case "long":
							type = typeof(long*);
							break;
						case "uchar":
							type = typeof(byte*);
							break;
						default:
							this.Log("Unknown pointer type: " + typeName, "", 2);
							break;
					}
				}
				else
				{
					switch (typeName)
					{
						case "int":
							type = typeof(int);
							break;
						case "float":
							type = typeof(float);
							break;
						case "double":
							type = typeof(double);
							break;
						case "char":
							type = typeof(char);
							break;
						case "uchar":
							type = typeof(byte);
							break;
						case "short":
							type = typeof(short);
							break;
						case "ushort":
							type = typeof(ushort);
							break;
						case "long":
							type = typeof(long);
							break;
						case "ulong":
							type = typeof(ulong);
							break;
						default:
							this.Log("Unknown argument type: " + typeName, "", 2);
							break;
					}
				}

				// Add to dictionary
				arguments.Add(argName, type ?? typeof(object));
			}

			// Return arguments
			return arguments;
		}



		// UI
		public void FillKernelsListbox(ListBox listBox)
		{
			// Get kernel files
			Dictionary<string, string> kernelFiles = this.GetKernelFiles();

			// Clear listbox
			listBox.Items.Clear();

			// Add kernel files to listbox
			foreach (KeyValuePair<string, string> kvp in kernelFiles)
			{
				listBox.Items.Add(kvp.Value);
			}

		}

		public void FillKernelCodeTextbox(TextBox textBox)
		{
			// Get kernel code
			string kernelCode = this.KernelFile != null ? File.ReadAllText(this.KernelFile) : "";

			// Set text
			textBox.Text = kernelCode;

			// Optionally add vScroll if lines are too long
			if (textBox.Lines.Length > 20)
			{
				textBox.ScrollBars = ScrollBars.Vertical;
			}
			else
			{
				textBox.ScrollBars = ScrollBars.None;
			}

			// Lock and register event
			textBox.ReadOnly = true;
			textBox.MouseClick += (s, e) =>
			{
				// Require ctrl down
				if (e.Button == MouseButtons.Left && Control.ModifierKeys == Keys.Control)
				{
					textBox.ReadOnly = !textBox.ReadOnly;
				}
			};

			// Register event for text changed
			textBox.TextChanged += (s, e) =>
			{
				// If Enter was pressed
				if (e is KeyEventArgs keyEventArgs && keyEventArgs.KeyCode == Keys.Enter)
				{
					// Check lines length
					if (textBox.Lines.Length > 20)
					{
						textBox.ScrollBars = ScrollBars.Vertical;
					}
					else
					{
						textBox.ScrollBars = ScrollBars.None;
					}
				}
			};
		}

		public TextBox? BuildInputPanel(Panel refPanel)
		{
			// Set input panel
			this.InputPanel = refPanel;

			// Clear input panel
			this.InputPanel.Controls.Clear();

			// Get kernel arguments
			Dictionary<string, Type> arguments = this.GetKernelArguments();

			// Check if arguments are empty
			if (arguments.Count == 0)
			{
				this.Log("No kernel arguments");
				return null;
			}

			// Loop through arguments
			int pointers = 0;
			int offset = 0;
			TextBox? inputBufferTextbox = null;
			for (int i = 0; i < arguments.Count; i++)
			{
				// Get argument name & type
				string argName = arguments.ElementAt(i).Key;
				Type argType = arguments.ElementAt(i).Value;
				string typeName = argType.Name;

				// Create label
				Label label = new();
				label.Name = "label_argName_" + argName;
				label.Text = argName;
				label.Location = new Point(10, offset);
				label.AutoSize = true;

				// Create input control based on type
				Control inputControl;
				if (argType == typeof(int))
				{
					inputControl = new NumericUpDown();
					((NumericUpDown) inputControl).Minimum = int.MinValue;
					((NumericUpDown) inputControl).Maximum = int.MaxValue;
					((NumericUpDown) inputControl).DecimalPlaces = 0;
					((NumericUpDown) inputControl).Increment = 1;
					// ((NumericUpDown) inputControl).ValueChanged += (s, e) => { this.Log("Value changed: " + ((NumericUpDown) s).Value); };
				}
				else if (argType == typeof(float))
				{
					inputControl = new NumericUpDown();
					((NumericUpDown) inputControl).Minimum = decimal.MinValue;
					((NumericUpDown) inputControl).Maximum = decimal.MaxValue;
					((NumericUpDown) inputControl).DecimalPlaces = 5;
					((NumericUpDown) inputControl).Increment = 0.01M;
					((NumericUpDown) inputControl).Value = 0.5M;
					// ((NumericUpDown) inputControl).ValueChanged += (s, e) => { this.Log("Value changed: " + ((NumericUpDown) s).Value); };
				}
				else if (argType == typeof(double))
				{
					inputControl = new NumericUpDown();
					((NumericUpDown) inputControl).Minimum = decimal.MinValue;
					((NumericUpDown) inputControl).Maximum = decimal.MaxValue;
					((NumericUpDown) inputControl).DecimalPlaces = 10;
					((NumericUpDown) inputControl).Increment = 0.00001M;
					// ((NumericUpDown) inputControl).ValueChanged += (s, e) => { this.Log("Value changed: " + ((NumericUpDown) s).Value); };
				}
				else if (argType == typeof(string))
				{
					inputControl = new TextBox();
					// inputControl.TextChanged += (s, e) => { this.Log("Text changed: " + ((TextBox) s).Text); };
				}
				else if (argType == typeof(long))
				{
					inputControl = new NumericUpDown();
					((NumericUpDown) inputControl).Minimum = 0;
					((NumericUpDown) inputControl).Maximum = 99999999999;
					((NumericUpDown) inputControl).DecimalPlaces = 0;
					((NumericUpDown) inputControl).Increment = 1;
					// ((NumericUpDown) inputControl).ValueChanged += (s, e) => { this.Log("Value changed: " + ((NumericUpDown) s).Value); };

					// Pointer length if pointers is uneven
					if (pointers % 2 != 0)
					{
						inputControl.BackColor = Color.LightGray;
						inputControl.ForeColor = Color.DarkOrange;
						pointers++;
					}
				}
				else if (argType == typeof(long*) || argType == typeof(float*) || argType == typeof(int*) || argType == typeof(uint*) || argType == typeof(ulong*) || argType == typeof(byte*))
				{
					inputControl = new TextBox();
					inputBufferTextbox = (TextBox) inputControl;
					// inputControl.TextChanged += (s, e) => { this.Log("Text changed: " + ((TextBox) s).Text); };
					inputControl.ForeColor = Color.Red;
					pointers++;
				}
				else
				{
					this.Log("Unsupported argument type or buffer argument found: " + argType.Name + " '" + argName + "'");
					inputControl = new TextBox();
					inputBufferTextbox = (TextBox) inputControl;
				}

				// Set input control properties
				inputControl.Location = new Point(10 + label.Width, offset);
				inputControl.Width = refPanel.Width - label.Width - 35;
				inputControl.Name = "argInput_" + argName;

				// Add tooltip to input control
				ToolTip toolTip = new();
				toolTip.SetToolTip(inputControl, "'" + typeName + "'");

				// Add "Pointer" to input control name if type is pointer
				if (typeName.EndsWith("*"))
				{
					inputControl.Name = "argInputPointer_" + pointers + "_" + argName;
					inputControl.Text = this.InputBufferPointer.ToString();
					inputControl.BackColor = Color.LightGray;
					toolTip.ForeColor = Color.Red;
				}




				// Add label and input control to panel
				this.InputPanel.Controls.Add(label);
				this.InputPanel.Controls.Add(inputControl);

				offset += 30;
			}

			// If offset is more than panel height, make panel scrollable
			if (offset > this.InputPanel.Height)
			{
				this.InputPanel.AutoScroll = true;
				this.InputPanel.VerticalScroll.Visible = true;
			}
			else
			{
				this.InputPanel.AutoScroll = false;
				this.InputPanel.VerticalScroll.Visible = false;
			}

			// Return input buffer textbox
			return inputBufferTextbox;
		}

		public object[] GetArgumentValues()
		{
			// Check panel set
			if (this.InputPanel == null || this.InputPanel.Controls.Count == 0)
			{
				this.Log("Input panel is null or empty");
				return [];
			}

			// Get kernel argument types
			Type[] argTypes = this.Arguments.Values.ToArray();

			// Get controls from panel with name containing "argInput"
			List<Control> controls = this.InputPanel.Controls.Cast<Control>()
				.Where(c => c.Name.Contains("argInput", StringComparison.OrdinalIgnoreCase))
				.ToList();

			// Make dict from controls & types
			Dictionary<Control, Type> controlsDict = argTypes
				.Select((type, index) => new { type, index })
				.ToDictionary(x => controls[x.index], x => x.type);

			// Get controls values
			object[] values = new object[controls.Count];

			for (int i = 0; i < controls.Count; i++)
			{
				Control control = controls[i];
				Type type = argTypes[i];

				// Get value from control
				if (control is NumericUpDown numericUpDown)
				{
					if (type == typeof(int))
					{
						values[i] = (int) numericUpDown.Value;
					}
				}
				else if (control is TextBox textBox)
				{
					values[i] = long.TryParse(textBox.Text, out long result) ? result : 0;
				}
				else
				{
					this.Log("Unsupported control type: " + control.GetType().Name);
					values[i] = 0;
				}
			}

			return values;

		}

		private static object GetDefaultValue(Type type)
		{
			if (type == typeof(int)) return 0;
			if (type == typeof(long)) return 0L;
			if (type == typeof(float)) return 0f;
			if (type == typeof(double)) return 0.0;
			if (type == typeof(decimal)) return 0m;
			if (type == typeof(bool)) return false;
			if (type == typeof(string)) return string.Empty;
			return Activator.CreateInstance(type) ?? (type.IsValueType ? Activator.CreateInstance(type)! : null!);
		}

		public string GetLatestKernelFile(string searchName = "")
		{
			string[] files = this.Files.Keys.ToArray();

			// Get all files that contain searchName
			string[] filteredFiles = files.Where(file => file.Contains(searchName, StringComparison.OrdinalIgnoreCase)).ToArray();
			string latestFile = filteredFiles.Select(file => new FileInfo(file))
				.OrderByDescending(file => file.LastWriteTime)
				.FirstOrDefault()?.FullName ?? "";

			// Return latest file
			if (string.IsNullOrEmpty(latestFile))
			{
				this.Log("No kernel files found with name: " + searchName);
				return "";
			}

			return latestFile;
		}


		public int GetArgumentPointerCount()
		{
			// Get kernel argument types
			Type[] argTypes = this.Arguments.Values.ToArray();

			// Count pointer arguments
			int count = 0;
			foreach (Type type in argTypes)
			{
				if (type.Name.EndsWith("*"))
				{
					count++;
				}
			}

			return count;
		}

		public void FillGenericKernelVersionsCombobox(ComboBox comboBox, string baseName = "Kernel", bool caseSensitive = true)
		{
			// Clear combobox & text
			comboBox.Items.Clear();
			comboBox.Text = "Ver.";

			// Get all files witch contain baseName and a 2 char version in the name + caseSensitive
			string[] allFiles = Directory.GetFiles(Path.Combine(this.Repopath, "Resources", "Kernels"), "*.cl", SearchOption.AllDirectories);

			// If caseSensitive is true, filter files
			string[] files = allFiles.Where(file => Path.GetFileNameWithoutExtension(file).Contains(baseName, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
				.Where(file => Path.GetFileNameWithoutExtension(file).Length == baseName.Length + 2)
				.ToArray();

			// Get every files without extension last 2 chars
			foreach (string file in files)
			{
				string name = Path.GetFileNameWithoutExtension(file);
				comboBox.Items.Add(name.Substring(name.Length - 2, 2));
			}

			// Select last version
			if (comboBox.Items.Count > 0)
			{
				comboBox.SelectedIndex = comboBox.Items.Count - 1;
			}
		}

		public void FillGenericKernelNamesCombobox(ComboBox comboBox)
		{
			// Clear combobox
			comboBox.Items.Clear();

			string basePath = Path.Combine(this.Repopath, "Resources", "Kernels");

			string[] files = Directory.GetFiles(basePath, "*.cl", SearchOption.AllDirectories);

			// Get every files without extension last 2 chars
			string[] baseNames = files
				.Select(file => Path.GetFileNameWithoutExtension(file))
				.Where(name => name.Length > 2)
				.Select(name => name.Substring(0, name.Length - 2))
				.Distinct()
				.ToArray();

			// Add base names to combobox
			foreach (string name in baseNames)
			{
				comboBox.Items.Add(name);
			}

			// Select -1
			comboBox.SelectedIndex = -1;
		}




		// Load
		public TextBox? LoadKernel(string kernelName = "", string filePath = "", Panel? inputPanel = null)
		{
			// Verify panel
			inputPanel ??= this.InputPanel;

			// Get kernel file path
			if (!string.IsNullOrEmpty(filePath))
			{
				kernelName = Path.GetFileNameWithoutExtension(filePath);
			}
			else
			{
				filePath = Directory.GetFiles(Path.Combine(this.Repopath, "Resources", "Kernels"), kernelName + "*.cl", SearchOption.AllDirectories).Where(f => Path.GetFileNameWithoutExtension(f).Length == kernelName.Length).FirstOrDefault() ?? "";
			}

			// Compile kernel
			this.Kernel = this.CompileFile(filePath);

			// Check if kernel is null
			if (this.Kernel == null)
			{
				this.Log("Kernel is null");
				return null;
			}
			else
			{
				// String of args like "(byte*)'pixels', (int)'width', (int)'height'"
				string argNamesString = string.Join(", ", this.Arguments.Keys.Select((arg, i) => $"({this.Arguments.Values.ElementAt(i).Name}) '{arg}'"));
				this.Log("Kernel loaded: '" + kernelName + "'", "", 1);
				this.Log("Kernel arguments: [" + argNamesString + "]", "", 1);
			}

			// Set file
			this.KernelFile = filePath;

			// Build input panel
			return this.BuildInputPanel(inputPanel ?? new Panel());
		}

		public void UnloadKernel()
		{
			// Release kernel
			if (this.Kernel != null)
			{
				CL.ReleaseKernel(this.Kernel.Value);
				this.Kernel = null;
			}

			// Clear kernel file
			this.KernelFile = null;
			
			// Clear input panel
			if (this.InputPanel != null)
			{
				this.InputPanel.Controls.Clear();
				this.InputPanel = null;
			}
		}


		// Generic kernel calls (execute)
		public long ExecuteKernelOOPGeneric(string version = "01", string baseName = "Kernel", long pointer = 0, long outputLength = 0, object[]? variableArguments = null)
		{
			// Return outputPointer init.
			long outputPointer = 0;

			// Get kernel path
			string kernelPath = Files.FirstOrDefault(f => f.Key.Contains(baseName + version, StringComparison.OrdinalIgnoreCase)).Key ?? "";

			// Load kernel
			this.LoadKernel(baseName + version, kernelPath);
			if (this.Kernel == null)
			{
				this.Log("Could not load Kernel '" + baseName + version + "'", $"ExecuteKernelOOPGeneric({string.Join(", ", variableArguments ?? [])})");
				return pointer;
			}

			// Get input buffer & length
			CLBuffer? inputBuffer = this.MemH.GetSingleBuffer(pointer, out IntPtr length);
			if (inputBuffer == null || length == IntPtr.Zero || length == 0)
			{
				this.Log("Input buffer not found or invalid length: " + pointer.ToString("X16"), length.ToString(), 2);
				return pointer;
			}

			// Create output buffer
			if (outputLength == 0)
			{
				outputLength = length;
			}
			outputPointer = this.MemH.AllocateSingle<float>((nint) outputLength);
			CLBuffer? outputBuffer = this.MemH.GetSingleBuffer(outputPointer, out nint outputBufferLength);
			if (outputBuffer == null || outputBufferLength != outputLength)
			{
				this.Log("Output buffer not found or invalid length: " + outputPointer.ToString("X16"), outputBufferLength + " != " + outputLength + " (?)", 2);
				return pointer;
			}

			// Get kernel arguments
			int pointersCount = this.GetArgumentPointerCount();
			Type[] argTypes = this.Arguments.Values.ToArray();

			// Get arguments
			if (variableArguments == null)
			{
				variableArguments = this.GetArgumentValues();
			}

			// Merge arguments
			List<object> arguments = [];
			int pointers = 0;
			for (int i = 0; i < argTypes.Length; i++)
			{
				// Get argument type
				Type argType = argTypes[i];

				// If type is pointer
				if (argType.Name.EndsWith("*"))
				{
					// Get pointer value
					long argPointer = (long) variableArguments[i] != 0 ? (long) variableArguments[i] : pointer;
					if (pointers == 2)
					{
						// Get output buffer pointer
						argPointer = outputPointer;
					}

					// Check if pointer is same as pointer
					if (argPointer == pointer)
					{
						// Add input buffer
						arguments.Add(inputBuffer.Value);
					}
					else
					{
						// Get buffer from memory handler
						CLBuffer? argBuffer = this.MemH.GetSingleBuffer(argPointer, out nint argLength);
						if (argBuffer == null || argLength == 0)
						{
							this.Log("Argument buffer not found or invalid length: " + argPointer.ToString("X16"), argLength.ToString(), 2);
							return pointer;
						}

						// Add argument to arguments
						arguments.Add(argBuffer.Value);
					}

					pointers++;
				}
				else
				{
					// If pointers 1 or 3
					if (pointers == 1)
					{
						// Add input buffer length
						arguments.Add(length);
						pointers++;
					}
					else if (pointers == 3)
					{
						// Add output buffer length
						arguments.Add(outputBufferLength);
						pointers++;
					}

					else
					{
						// Add argument to arguments
						arguments.Add(variableArguments[i]);
					}
				}
			}

			// DEBUG LOG values
			this.Log("Kernel arguments: " + string.Join(", ", arguments.Select(a => a.ToString())), "'" + baseName + version + "'", 2);

			// Set kernel arguments
			for (int i = 0; i < arguments.Count; i++)
			{
				// Set argument
				CLResultCode err = this.SetKernelArgSafe((uint) i, arguments[i]);
				if (err != CLResultCode.Success)
				{
					this.Log("Error setting kernel argument " + i + ": " + err.ToString(), arguments[i].ToString() ?? "");
					return pointer;
				}
			}

			// Exec
			CLResultCode error = CL.EnqueueNDRangeKernel(this.QUE, this.Kernel.Value, 1, null, [(UIntPtr) length], null, 0, null, out CLEvent evt);
			if (error != CLResultCode.Success)
			{
				this.Log("Error executing kernel: " + error.ToString(), "", 2);
				return pointer;
			}

			// Wait for kernel to finish
			error = CL.WaitForEvents(1, [evt]);
			if (error != CLResultCode.Success)
			{
				this.Log("Error waiting for kernel to finish: " + error.ToString(), "", 2);
				return pointer;
			}

			// Release event
			error = CL.ReleaseEvent(evt);
			if (error != CLResultCode.Success)
			{
				this.Log("Error releasing event: " + error.ToString(), "", 2);
				return pointer;
			}

			// Return output pointer if pointersCount is 2
			if (pointersCount == 1)
			{
				return pointer;
			}
			else if (pointersCount == 2)
			{
				return outputPointer;
			}

			// Return input pointer
			return pointer;
		}

		public long ExecuteKernelIPGeneric(string version = "01", string baseName = "NULL", long pointer = 0, int width = 0, int height = 0, object[]? variableArguments = null, bool logSuccess = false)
		{
			// Start stopwatch
			List<long> times = [];
			List<string> timeNames = ["load: ", "mem: ", "args: ", "exec: ", "total: "];
			Stopwatch sw = Stopwatch.StartNew();

			// Get kernel path
			string kernelPath = Files.FirstOrDefault(f => f.Key.Contains(baseName + version)).Key ?? "";

			// Load kernel if not loaded
			if (this.Kernel == null || this.KernelFile != kernelPath)
			{
				this.LoadKernel(baseName + version, kernelPath);
				if (this.Kernel == null)
				{
					this.Log("Could not load Kernel '" + baseName + version + "'", $"ExecuteKernelIPGeneric({string.Join(", ", variableArguments ?? [])})");
					return pointer;
				}
			}

			// Take time
			times.Add(sw.ElapsedMilliseconds - times.Sum());

			// Get input buffer & length
			CLBuffer? inputBuffer = this.MemH.GetSingleBuffer(pointer, out IntPtr length);
			if (inputBuffer == null || length == IntPtr.Zero || length == 0)
			{
				this.Log("Input buffer not found or invalid length: " + pointer.ToString("X16"), length.ToString(), 2);
				return pointer;
			}

			// Take time
			times.Add(sw.ElapsedMilliseconds - times.Sum());

			// Get kernel arguments & work dimensions
			List<string> argNames = this.Arguments.Keys.ToList();
			variableArguments ??= this.GetArgumentValues();

			// Dimensions
			int pixelsTotal = (int) length / 4; // Anzahl der Pixel
			int workWidth = width > 0 ? width : pixelsTotal; // Falls kein width gegeben, 1D
			int workHeight = height > 0 ? height : 1;        // Falls kein height, 1D

			// Work dimensions
			uint workDim = (width > 0 && height > 0) ? 2u : 1u;
			UIntPtr[] globalWorkSize = workDim == 2
				? [(UIntPtr) workWidth, (UIntPtr) workHeight]
				: [(UIntPtr) pixelsTotal];

			// Merge arguments
			List<object> arguments = this.MergeArguments(variableArguments, pointer, 0, width, height, false);

			// Set kernel arguments
			for (int i = 0; i < arguments.Count; i++)
			{
				// Set argument
				CLResultCode err = this.SetKernelArgSafe((uint) i, arguments[i]);
				if (err != CLResultCode.Success)
				{
					this.Log("Error setting kernel argument " + i + ": " + err.ToString(), arguments[i].ToString() ?? "");
					return pointer;
				}
			}

			// Take time
			times.Add(sw.ElapsedMilliseconds - times.Sum());

			// Log arguments
			if (logSuccess)
			{
				this.Log("Kernel arguments set: " + string.Join(", ", argNames.Select((a, i) => a + ": " + arguments[i].ToString())), "'" + baseName + version + "'", 2);
			}

			// Exec
			CLResultCode error = CL.EnqueueNDRangeKernel(
				this.QUE,
				this.Kernel.Value,
				workDim,          // 1D oder 2D
				null,             // Kein Offset
				globalWorkSize,   // Work-Größe in Pixeln
				null,             // Lokale Work-Size (automatisch)
				0, null, out CLEvent evt
			);
			if (error != CLResultCode.Success)
			{
				this.Log("Error executing kernel: " + error.ToString(), "", 2);
				return pointer;
			}

			// Wait for kernel to finish
			error = CL.WaitForEvents(1, [evt]);
			if (error != CLResultCode.Success)
			{
				this.Log("Error waiting for kernel to finish: " + error.ToString(), "", 2);
				return pointer;
			}

			// Release event
			error = CL.ReleaseEvent(evt);
			if (error != CLResultCode.Success)
			{
				this.Log("Error releasing event: " + error.ToString(), "", 2);
				return pointer;
			}

			// Take time
			times.Add(sw.ElapsedMilliseconds - times.Sum());
			times.Add(times.Sum());
			sw.Stop();

			// Log success with timeNames
			if (logSuccess)
			{
				this.Log("Kernel executed successfully! Times: " + string.Join(", ", times.Select((t, i) => timeNames[i] + t + "ms")), "'" + baseName + version + "'", 2);
			}

			// Return input pointer
			return pointer;
		}

		public long ExecuteKernelOOPGeneric(string version = "01", string baseName = "NULL", long pointer = 0, int width = 0, int height = 0, object[]? variableArguments = null, bool logSuccess = false)
		{
			// Start stopwatch
			List<long> times = [];
			List<string> timeNames = ["load: ", "mem: ", "args: ", "exec: ", "total: "];
			Stopwatch sw = Stopwatch.StartNew();

			// Get kernel path
			string kernelPath = Files.FirstOrDefault(f => f.Key.Contains(baseName + version)).Key ?? "";

			// Load kernel if not loaded
			if (this.Kernel == null || this.KernelFile != kernelPath)
			{
				this.LoadKernel(baseName + version, kernelPath);
				if (this.Kernel == null)
				{
					this.Log("Could not load Kernel '" + baseName + version + "'", $"ExecuteKernelIPGeneric({string.Join(", ", variableArguments ?? [])})");
					return pointer;
				}
			}

			// Take time
			times.Add(sw.ElapsedMilliseconds - times.Sum());

			// Get input buffer & length
			CLBuffer? inputBuffer = this.MemH.GetSingleBuffer(pointer, out IntPtr length);
			if (inputBuffer == null || length == IntPtr.Zero || length == 0)
			{
				this.Log("Input buffer not found or invalid length: " + pointer.ToString("X16"), length.ToString(), 2);
				return pointer;
			}

			// Get kernel arguments & work dimensions
			List<string> argNames = this.Arguments.Keys.ToList();
			variableArguments ??= this.GetArgumentValues();

			// Dimensions
			int pixelsTotal = (int) length / 4; // Anzahl der Pixel
			int workWidth = width > 0 ? width : pixelsTotal; // Falls kein width gegeben, 1D
			int workHeight = height > 0 ? height : 1;        // Falls kein height, 1D

			// Work dimensions
			uint workDim = (width > 0 && height > 0) ? 2u : 1u;
			UIntPtr[] globalWorkSize = workDim == 2
				? [(UIntPtr) workWidth, (UIntPtr) workHeight]
				: [(UIntPtr) pixelsTotal];

			// Create output buffer
			long outputPointer = this.MemH.AllocateSingle<byte>(length);
			CLBuffer? outputBuffer = this.MemH.GetSingleBuffer(outputPointer, out nint outputBufferLength);
			if (outputBuffer == null || outputBufferLength != length)
			{
				this.Log("Output buffer not found or invalid length: " + outputPointer.ToString("X16"), outputBufferLength + " != " + length + " (?)", 2);
				return pointer;
			}

			// Take time
			times.Add(sw.ElapsedMilliseconds - times.Sum());

			// Merge arguments
			List<object> arguments = this.MergeArguments(variableArguments, pointer, outputPointer, width, height, false);

			// Set kernel arguments
			for (int i = 0; i < arguments.Count; i++)
			{
				// Set argument
				CLResultCode err = this.SetKernelArgSafe((uint) i, arguments[i]);
				if (err != CLResultCode.Success)
				{
					this.Log("Error setting kernel argument " + i + ": " + err.ToString(), arguments[i].ToString() ?? "");
					return pointer;
				}
			}

			// Take time
			times.Add(sw.ElapsedMilliseconds - times.Sum());

			// Log arguments
			if (logSuccess)
			{
				this.Log("Kernel arguments set: " + string.Join(", ", argNames.Select((a, i) => a + ": " + arguments[i].ToString())), "'" + baseName + version + "'", 2);
			}

			// Exec
			CLResultCode error = CL.EnqueueNDRangeKernel(
				this.QUE,
				this.Kernel.Value,
				workDim,          // 1D oder 2D
				null,             // Kein Offset
				globalWorkSize,   // Work-Größe in Pixeln
				null,             // Lokale Work-Size (automatisch)
				0, null, out CLEvent evt
			);
			if (error != CLResultCode.Success)
			{
				this.Log("Error executing kernel: " + error.ToString(), "", 2);
				return pointer;
			}

			// Wait for kernel to finish
			error = CL.WaitForEvents(1, [evt]);
			if (error != CLResultCode.Success)
			{
				this.Log("Error waiting for kernel to finish: " + error.ToString(), "", 2);
				return pointer;
			}

			// Release event
			error = CL.ReleaseEvent(evt);
			if (error != CLResultCode.Success)
			{
				this.Log("Error releasing event: " + error.ToString(), "", 2);
				return pointer;
			}

			// Take time
			times.Add(sw.ElapsedMilliseconds - times.Sum());
			times.Add(times.Sum());
			sw.Stop();

			// Log success with timeNames
			if (logSuccess)
			{
				this.Log("Kernel executed successfully! Times: " + string.Join(", ", times.Select((t, i) => timeNames[i] + t + "ms")), "'" + baseName + version + "'", 2);
			}

			// Return pointer
			if (this.GetArgumentPointerCount() == 1)
			{
				return pointer;
			}
			else if (this.GetArgumentPointerCount() == 2)
			{
				return outputPointer;
			}
			else
			{
				return pointer;
			}
		}



		public List<object> MergeArguments(object[] arguments, long inputPointer = 0, long outputPointer = 0, int width = 0, int height = 0, bool log = false)
		{
			List<object> result = [];

			// Get kernel arguments
			Dictionary<string, Type> kernelArguments = this.Arguments;

			// Match arguments to kernel arguments
			bool inputFound = false;
			for (int i = 0; i < kernelArguments.Count; i++)
			{
				string argName = kernelArguments.ElementAt(i).Key;
				Type argType = kernelArguments.ElementAt(i).Value;

				// If argument is pointer -> add pointer
				if (argType.Name.EndsWith("*"))
				{
					// Get pointer value
					long argPointer = 0;
					if (!inputFound)
					{
						argPointer = (long) arguments[i] != 0 ? (long) arguments[i] : inputPointer;
						inputFound = true;
					}
					else
					{
						argPointer = (long) arguments[i] != 0 ? (long) arguments[i] : outputPointer;
					}

					// Get buffer
					CLBuffer? argBuffer = this.MemH.GetSingleBuffer(argPointer, out nint argLength);
					if (argBuffer == null || argLength == 0)
					{
						this.Log("Argument buffer not found or invalid length: " + argPointer.ToString("X16"), argLength.ToString(), 2);
						return [];
					}

					// Add pointer to result
					result.Add(argBuffer);

					// Log buffer found
					if (log)
					{
						// Log buffer found
						this.Log("Kernel argument buffer found: " + argPointer.ToString("X16"), "Index: " + i, 3);
					}
				}
				else if (argType == typeof(int))
				{
					// If name is "width" or "height" -> add width or height
					if (argName.ToLower() == "width")
					{
						result.Add(width <= 0 ? arguments[i] : width);

						// Log width found
						if (log)
						{
							this.Log("Kernel argument width found: " + width.ToString(), "Index: " + i, 3);
						}
					}
					else if (argName.ToLower() == "height")
					{
						result.Add(height <= 0 ? arguments[i] : height);

						// Log height found
						if (log)
						{
							this.Log("Kernel argument height found: " + height.ToString(), "Index: " + i, 3);
						}
					}
					else
					{
						result.Add((int) arguments[i]);
					}
				}
				else if (argType == typeof(float))
				{
					result.Add((float) arguments[i]);
				}
				else if (argType == typeof(double))
				{
					result.Add((double) arguments[i]);
				}
				else if (argType == typeof(long))
				{
					result.Add((long) arguments[i]);
				}
			}

			// Log arguments
			if (log)
			{
				this.Log("Kernel arguments: " + string.Join(", ", result.Select(a => a.ToString())), "'" + Path.GetFileName(this.KernelFile) + "'", 2);
			}

			return result;
		}

		public CLResultCode SetKernelArgSafe(uint index, object value)
		{
			// Check kernel
			if (this.Kernel == null)
			{
				this.Log("Kernel is null");
				return CLResultCode.InvalidKernel;
			}

			switch (value)
			{
				case CLBuffer buffer:
					return CL.SetKernelArg(this.Kernel.Value, index, buffer);

				case int i:
					return CL.SetKernelArg(this.Kernel.Value, index, i);

				case long l:
					return CL.SetKernelArg(this.Kernel.Value, index, l);

				case float f:
					return CL.SetKernelArg(this.Kernel.Value, index, f);

				case double d:
					return CL.SetKernelArg(this.Kernel.Value, index, d);

				case byte b:
					return CL.SetKernelArg(this.Kernel.Value, index, b);

				case IntPtr ptr:
					return CL.SetKernelArg(this.Kernel.Value, index, ptr);

				// Spezialfall für lokalen Speicher (Größe als uint)
				case uint u:
					return CL.SetKernelArg(this.Kernel.Value, index, new IntPtr(u));

				default:
					throw new ArgumentException($"Unsupported argument type: {value?.GetType().Name ?? "null"}");
			}
		}

		public List<string> PrecompileAllKernels()
		{
			// Get all kernel files
			string[] kernelFiles = this.Files.Keys.ToArray();

			// Precompile all kernels
			List<string> precompiledKernels = [];
			foreach (string kernelFile in kernelFiles)
			{
				// Compile kernel
				CLKernel? kernel = this.CompileFile(kernelFile);
				if (kernel != null)
				{
					precompiledKernels.Add(kernelFile);
				}
				else
				{
					this.Log("Error compiling kernel: " + kernelFile, "", 2);
				}
			}

			this.UnloadKernel();

			return precompiledKernels;
		}

	}
}
	