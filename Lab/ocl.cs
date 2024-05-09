using System.Text;
using System.Text.Unicode;

namespace Lab;

using OpenTK.Compute.OpenCL;

public class ocl {
	public ocl() {
		CL.GetPlatformIds(0, null, out uint platformCount);
		CLPlatform[] platformIds = new CLPlatform[platformCount];
		CL.GetPlatformIds(platformCount, platformIds, out _);

		Console.WriteLine(platformIds.Length);
		foreach (CLPlatform platform in platformIds) {
			Console.WriteLine(platform.Handle);
			CL.GetPlatformInfo(platform, PlatformInfo.Name, out byte[] val);
			Console.WriteLine(val);

			Console.WriteLine(Encoding.UTF8.GetString(val));
			foreach (IntPtr platformId in platformIds) {
				CL.GetDeviceIds(new CLPlatform(platformId), DeviceType.All, out CLDevice[] deviceIds);

				foreach (var dev in deviceIds) {
					Console.WriteLine($"device: {dev}");
				}

				var device = deviceIds[0];

				CLContext context = CL.CreateContext(
				                                     IntPtr.Zero,
				                                     (uint)deviceIds.Length,
				                                     deviceIds,
				                                     IntPtr.Zero,
				                                     IntPtr.Zero,
				                                     out CLResultCode result
				                                    );
				if (result != CLResultCode.Success) {
					throw new Exception("The context couldn't be created.");
				}

				CL.GetPlatformInfo(new CLPlatform(platformId), PlatformInfo.Version, out byte[] val2);
				Console.WriteLine(Encoding.UTF8.GetString(val2));

				string clProgramSource = @"

float noise_simplex_2d(float xin, float yin) {
    // Your Simplex noise calculation for 2D
    return 0.0; // Placeholder
}
__kernel void simplex_noise(__global float* output, const unsigned int width, const unsigned int height, const float scale) {
    int x = get_global_id(0);
    int y = get_global_id(1);
    if (x >= width || y >= height) return;
    
    // Implement your Simplex Noise function here compatible with OpenCL 1.2
    float n = noise_simplex_2d(x * scale, y * scale);
    int idx = y * width + x;
    output[idx] = n;
}

";


				CLProgram program = CL.CreateProgramWithSource(context, clProgramSource, out result);
				if (result != CLResultCode.Success) {
					Console.WriteLine("Error creating program.");
					return;
				}

				result = CL.BuildProgram(program, new CLDevice[] { device }, string.Empty, (@event, data) => Console.WriteLine(data));
				if (result != CLResultCode.Success) {
					// Log build errors
					 CL.GetProgramBuildInfo(program, device, ProgramBuildInfo.Log, out byte[] log);
					 Console.WriteLine();

					Console.WriteLine("Error building program: " + Encoding.UTF8.GetString(log));
					return;
				}

				CLKernel kernel = CL.CreateKernel(program, "simplex_noise", out result);
				if (result != CLResultCode.Success) {
					Console.WriteLine("Error creating kernel.");
					return;
				}

				int     width  = 1024;
				int     height = 768;
				float   scale  = 0.01f;
				Span<float> output = new float[width * height];

// Allocate memory for the output
				CLBuffer outputBuffer = CL.CreateBuffer<float>(context, MemoryFlags.CopyHostPtr | MemoryFlags.ReadWrite, output, out result);
				if (result != CLResultCode.Success) {
					Console.WriteLine("Error creating buffer." + result);
					return;
				}

// Set kernel arguments
				CL.SetKernelArg(kernel, 0, outputBuffer);
				CL.SetKernelArg(kernel, 1, width);
				CL.SetKernelArg(kernel, 2, height);
				CL.SetKernelArg(kernel, 3, scale);


				var commandQueue = CL.CreateCommandQueue(context, device, new(), out var resultCode);
				Console.WriteLine(resultCode);
// Execute the kernel
				UIntPtr[] globalWorkSize = [(UIntPtr)width, (UIntPtr)height ];
				CL.EnqueueNDRangeKernel(commandQueue, kernel, 2, null, globalWorkSize, null, 0, null, out _);

// Read back the results
				CL.EnqueueReadBuffer(commandQueue, outputBuffer, true, UIntPtr.Zero, output, null, out var eventHandle );




// Clean up resources
				CL.ReleaseKernel(kernel);
				CL.ReleaseCommandQueue(commandQueue);
				CL.ReleaseContext(context);

			}

		}
	}
}