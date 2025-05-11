using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCLBitmaps
{
	public static class ClKernelCreationWindow
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public static string Repopath => Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
		public static string KernelPath => Path.Combine(Repopath, "OpenCL", "Kernels");









		// ----- ----- ----- METHODS ----- ----- ----- \\
		public static Form OpenCreationWindow(int width = 1000, int height = 700)
		{
			// Create window
			Form window = new()
			{
				Text = "Create Kernel",
				StartPosition = FormStartPosition.CenterParent,
				Size = new Size(width, height),
				FormBorderStyle = FormBorderStyle.FixedDialog,
				MaximizeBox = false,
				MinimizeBox = false,
				ShowIcon = false,
				ShowInTaskbar = false
			};

			// Create controls
			Label labelName = new() { Text = "Kernel Name:", Location = new Point(10, 10), AutoSize = true };
			TextBox textBoxName = new() { Location = new Point(100, 10), Width = 200 };
			Button buttonCreate = new() { Text = "Create", Location = new Point(10, 40) };
			Button buttonCancel = new() { Text = "Cancel", Location = new Point(100, 40) };
			
			// Add controls to window
			window.Controls.Add(labelName);
			window.Controls.Add(textBoxName);
			window.Controls.Add(buttonCreate);
			window.Controls.Add(buttonCancel);
			
			// Register events
			buttonCreate.Click += (s, e) =>
			{
				string kernelName = textBoxName.Text;
				if (!string.IsNullOrWhiteSpace(kernelName))
				{
					CreateKernelFile(kernelName);
					window.DialogResult = DialogResult.OK;
					window.Close();
				}
				else
				{
					MessageBox.Show("Please enter a valid kernel name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			};
			
			buttonCancel.Click += (s, e) => window.Close();
			return window;
		}

		public static string CreateKernelFile(string kernelName)
		{
			// Create kernel file path
			string kernelFilePath = Path.Combine(KernelPath, $"{kernelName}.cl");
			
			// Check if file already exists
			if (File.Exists(kernelFilePath))
			{
				MessageBox.Show($"Kernel file '{kernelName}.cl' already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return kernelFilePath;
			}
			
			// Create kernel file with basic template
			string kernelTemplate = $"__kernel void {kernelName}(__global const uchar* input, __global uchar* output) {{\n" +
									 $"    int id = get_global_id(0);\n" +
									 $"    output[id] = input[id];\n" +
									 $"}}";
			File.WriteAllText(kernelFilePath, kernelTemplate);
			MessageBox.Show($"Kernel file '{kernelName}.cl' created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return kernelFilePath;
		}



	}
}
