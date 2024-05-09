using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

public class Mat {
    static void dMain() {
        {
            // Define a matrix to be quantized
            Matrix<float> weightMatrix = DenseMatrix.CreateRandom(1024, 4096, new Normal());

            // Specify the rank for low-rank approximation and the quantization step size
            int rank = 2; // This value might be less than the number of columns or rows depending on
                          // the matrix size and desired approximation level
            float quantizationStep = 1; // Defines the precision of quantization

            for (int i = 4; i < 48; ++i) {
                var c = QuantizationHelper.LowRankApproximation(weightMatrix, i);


                //Console.WriteLine(c - weightMatrix);
            }
            // Apply the low-rank approximation and quantization
            //QuantizationHelper.ApplyQuantization(weightMatrix, rank, quantizationStep);


        }
    }
}

public class QuantizationHelper
{
    public static Matrix<float> LowRankApproximation(Matrix<float> originalMatrix, int k)
    {
        var svd   = originalMatrix.Svd(true);
        var U     = svd.U.SubMatrix(0, originalMatrix.RowCount, 0, k);
        var Sigma = svd.W.SubMatrix(0, k, 0, k);
        var VT    = svd.VT.SubMatrix(0, k, 0, originalMatrix.ColumnCount);

        //Console.WriteLine($"SVD: {originalMatrix.RowCount * originalMatrix.ColumnCount} --> {U.RowCount * U.ColumnCount + Sigma.RowCount * Sigma.ColumnCount + VT.RowCount * VT.ColumnCount}");
        /*Console.WriteLine(U);
        Console.WriteLine(Sigma);
        Console.WriteLine(VT);
        Console.WriteLine();*/

        Console.WriteLine($"k={k}: {originalMatrix.RowCount * originalMatrix.ColumnCount} --> {U.RowCount * U.ColumnCount + Sigma.RowCount * Sigma.ColumnCount + VT.RowCount * VT.ColumnCount}, {QuantizationHelper.CalculateRelativeError(originalMatrix, U * Sigma * VT):N4}");

        // Reconstruct the low-rank matrix
        return U * Sigma * VT;
    }

    public static Matrix<float> QuantizeMatrix(Matrix<float> matrix, float quantizationStep)
    {
        // Simple uniform quantization example
        return matrix.Map(x => MathF.Round(x / quantizationStep) * quantizationStep);
    }

    public static void ApplyQuantization(Matrix<float> originalMatrix, int rank, float quantizationStep)
    {
        var approxMatrix    = LowRankApproximation(originalMatrix, rank);
        var quantizedMatrix = QuantizeMatrix(approxMatrix, quantizationStep);

        Console.WriteLine("Quantized Matrix:");
        Console.WriteLine(quantizedMatrix);
    }

    public static float CalculateFrobeniusNorm(Matrix<float> original, Matrix<float> derived)
    {
        return (float)(original - derived).FrobeniusNorm();
    }
    public static float CalculateRelativeError(Matrix<float> original, Matrix<float> derived)
    {
        var normOriginal   = original.FrobeniusNorm();
        var normDifference = (original - derived).FrobeniusNorm();
        return (float)(normDifference / normOriginal);
    }

}