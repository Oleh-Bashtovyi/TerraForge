using System;

namespace TerrainGenerationApp.Utilities;

public static class Interpolations
{
    /// <summary>
    /// Linear interpolation - standard, does not change the input value
    /// </summary>
    public static float LinearInterpolation(float value)
    {
        return value;
    }

    /// <summary>
    /// Highlights high values using a quadratic function
    /// </summary>
    public static float HighlightHighValues(float value, float power = 2.0f)
    {
        return (float)Math.Pow(value, power);
    }

    /// <summary>
    /// Highlights low values using a root function
    /// </summary>
    public static float HighlightLowValues(float value, float power = 0.5f)
    {
        return (float)Math.Pow(value, power);
    }

    /// <summary>
    /// Highlights both high and low values, reducing mid-range values
    /// </summary>
    public static float HighlightExtremes(float value, float contrast = 2.0f)
    {
        // Convert value from [0,1] to [-1,1]
        float normalizedValue = 2 * value - 1;

        // Apply an S-shaped function to highlight extremes
        float interpolatedValue = (float)Math.Tanh(contrast * normalizedValue) / 2 + 0.5f;

        return interpolatedValue;
    }

    /// <summary>
    /// Power law interpolation with adjustable exponent
    /// </summary>
    public static float PowerLawInterpolation(float value, float exponent = 1.5f)
    {
        return (float)Math.Pow(value, exponent);
    }

    /// <summary>
    /// Logarithmic interpolation to highlight small changes in low areas
    /// </summary>
    public static float LogarithmicInterpolation(float value, float @base = 10.0f)
    {
        // Prevent log(0)
        if (value <= 0)
            return 0;

        // Logarithmic function normalized to the range [0,1]
        return (float)(Math.Log(1 + (@base - 1) * value) / Math.Log(@base));
    }

    /// <summary>
    /// Sine interpolation for smooth transitions
    /// </summary>
    public static float SineInterpolation(float value)
    {
        // Converts a linear value to a sine curve
        return (float)Math.Sin(value * Math.PI / 2);
    }

    /// <summary>
    /// Cosine interpolation for smooth transitions
    /// </summary>
    public static float CosineInterpolation(float value)
    {
        // Smooth transition using cosine
        return (float)(1 - Math.Cos(value * Math.PI)) / 2;
    }

    /// <summary>
    /// Cubic Hermite interpolation (smoothed S-curve)
    /// </summary>
    public static float HermiteInterpolation(float value)
    {
        // Cubic Hermite formula: 3t² - 2t³
        return value * value * (3 - 2 * value);
    }

    /// <summary>
    /// Five-step smoothed interpolation (smootherstep)
    /// </summary>
    public static float SmootherStepInterpolation(float value)
    {
        // Sixth-degree polynomial for even smoother transition
        return value * value * value * (value * (value * 6 - 15) + 10);
    }

    /// <summary>
    /// Quintic (five-step) interpolation for very smooth transitions
    /// </summary>
    public static float QuinticInterpolation(float value)
    {
        // Sixth-degree polynomial for very smooth transitions
        return value * value * value * value * value;
    }

    /// <summary>
    /// Exponential interpolation
    /// </summary>
    public static float ExponentialInterpolation(float value, float exponent = 5.0f)
    {
        // Rapid growth at the end of the range
        return (float)((Math.Exp(value * exponent) - 1) / (Math.Exp(exponent) - 1));
    }

    /// <summary>
    /// Circular interpolation (based on the equation of a circle)
    /// </summary>
    public static float CircularInterpolation(float value)
    {
        // Uses the equation of a circle to create a smooth curve
        if (value <= 0)
            return 0;
        else if (value >= 1)
            return 1;
        else
            return 1 - (float)Math.Sqrt(1 - value * value);
    }

    /// <summary>
    /// Elastic interpolation with oscillations
    /// </summary>
    public static float ElasticInterpolation(float value, float amplitude = 0.25f, float period = 0.3f)
    {
        if (value <= 0 || value >= 1)
            return value;

        float s = period / 4;
        float oscillation = (float)(amplitude * Math.Pow(2, -10 * value) *
            Math.Sin((value - s) * (2 * Math.PI) / period));

        return value + oscillation;
    }

    /// <summary>
    /// Highlight specific height levels (creating "contour lines")
    /// </summary>
    public static float ContourInterpolation(float value, float interval = 0.1f)
    {
        // Highlights regular intervals to create a contour line effect
        float modValue = value % interval;
        float ratio = modValue / interval;

        // Use a Gaussian function to highlight contours
        float highlight = (float)Math.Exp(-Math.Pow(ratio - 0.5, 2) / 0.02);

        // Mix the original value with the highlight
        return value * (1 - 0.3f) + value * 0.3f * highlight;
    }

    /// <summary>
    /// "Terraced" interpolation, quantizes values to certain levels
    /// </summary>
    public static float TerracedInterpolation(float value, int steps = 5)
    {
        // Divide the height into "steps"
        float step = 1.0f / steps;
        float stepValue = (float)Math.Floor(value / step) * step;

        return stepValue;
    }

    /// <summary>
    /// Smoothed "terraced" interpolation with smooth transitions between levels
    /// </summary>
    public static float SmoothTerracedInterpolation(float value, int steps = 5, float smoothness = 0.1f)
    {
        // Divide the height into "steps" with smooth transitions
        float step = 1.0f / steps;
        float stepValue = (float)Math.Floor(value / step) * step;
        float nextStep = stepValue + step;
        float remainder = (value - stepValue) / step;

        // Apply smoothing only within the "smooth" range
        if (remainder < smoothness)
        {
            float t = remainder / smoothness;
            return stepValue + t * t * (3 - 2 * t) * smoothness * step;
        }
        else if (remainder > 1 - smoothness)
        {
            float t = (remainder - (1 - smoothness)) / smoothness;
            return nextStep - (1 - t * t * (3 - 2 * t)) * smoothness * step;
        }
        else
        {
            return stepValue + smoothness * step;
        }
    }

    /// <summary>
    /// "Valley" interpolation - highlights mid-range values, darkening high and low values
    /// </summary>
    public static float ValleyInterpolation(float value)
    {
        // Creates a "hump" in the middle of the range
        return 1.0f - Math.Abs(2.0f * value - 1.0f);
    }

    /// <summary>
    /// Binomial interpolation using a binomial distribution
    /// </summary>
    public static float BinomialInterpolation(float value, int n = 4)
    {
        // Binomial distribution for smoother transition
        float result = 0;
        for (int k = 0; k <= n; k++)
        {
            result += Binomial(n, k) * (float)Math.Pow(value, k) * (float)Math.Pow(1 - value, n - k) * k / n;
        }
        return result;
    }

    /// <summary>
    /// Helper method for calculating the binomial coefficient
    /// </summary>
    private static float Binomial(int n, int k)
    {
        if (k < 0 || k > n)
            return 0;

        if (k == 0 || k == n)
            return 1;

        float result = 1;
        for (int i = 1; i <= k; i++)
        {
            result *= n - (k - i);
            result /= i;
        }
        return result;
    }
}
