using System.Numerics;
using Lab;

public class FFT {
    public static Complex[] ifft(Complex[] buffer) {
        var n = buffer.Length;

        // Take the conjugate of the complex numbers
        for (var i = 0; i < n; i++) {
            buffer[i] = Complex.Conjugate(buffer[i]);
        }

        // Perform FFT on conjugated data
        buffer = fft(buffer);

        // Take the conjugate again and scale by 1/N
        for (var i = 0; i < n; i++)
        {
            buffer[i] = Complex.Conjugate(buffer[i]) / n;
        }

        return buffer;
    }

    // FFT method assumed to be already impl
    // Recursive FFT method
    public static Complex[] fft(Complex[] buffer) {
        var n = buffer.Length;
        if (n == 1)
            return new Complex[] { buffer[0] };

        if ((n & (n - 1)) != 0) // Check if n is not a power of 2
            throw new ArgumentException("The buffer length must be a power of 2.");

        var even = new Complex[n / 2];
        var odd  = new Complex[n / 2];

        for (var i = 0; i < n / 2; i++) {
            even[i] = buffer[2 * i];
            odd[i]  = buffer[2 * i + 1];
        }

        var evenFFT = fft(even);
        var oddFFT  = fft(odd);

        var result = new Complex[n];
        for (var k = 0; k < n / 2; k++) {
            var t = Complex.Exp(-Complex.ImaginaryOne * (2 * Math.PI * k / n)) * oddFFT[k];
            result[k]         = evenFFT[k] + t;
            result[k + n / 2] = evenFFT[k] - t;
        }

        return result;
    }

    // Helper method to print the FFT output
    public static void PrintFFT(Complex[] fft) {
        foreach (var c in fft)
            Console.WriteLine($"{c.Real} + {c.Imaginary}i");
        Console.WriteLine();
    }

    public static void Main() {
        ocl o = new ocl();
    }


    // Main method to demonstrate the FFT method
    public static void Main1() {
        // Input buffer with complex numbers where the imaginary part is 0
        Complex[] input = [.1, .2, .3, .4, .5, .6, .7, .8];

        // Perform FFT
        var output = fft(input);

        // Print the results
        PrintFFT(output);


        Complex[] originalData = ifft(output);
        PrintFFT(originalData);


    }
}