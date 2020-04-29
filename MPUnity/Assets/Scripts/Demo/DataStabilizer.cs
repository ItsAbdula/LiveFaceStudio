namespace OpenCvSharp.Demo
{
    using System;
    using System.Collections.Generic;
    using OpenCvSharp;

    internal class DataStabilizerParams
    {
        public bool Enabled { get; set; }

        /// Maximum ignored point distance
        public double Threshold { get; set; }

        /// Threshold scale factor (should processing space be scaled)
        public double ThresholdFactor { get; set; }

        public int SamplesCount { get; set; }

        public double GetScaledThreshold()
        {
            return Threshold * ThresholdFactor;
        }

        public DataStabilizerParams()
        {
            Enabled = true;
            Threshold = 1.0;
            ThresholdFactor = 1.0;
            SamplesCount = 10;
        }
    }

    internal interface IDataStabilizer<T>
    {
        DataStabilizerParams Params { get; set; }

        T Sample { get; set; }

        bool LastApplied { get; set; }
    }

    internal abstract class DataStabilizerBase<T>
    {
        protected T result;                     // computer output sample
        protected bool dirty = true;            // flag signals whether "result" sample must be recomputed
        protected T[] samples = null;           // whole samples set
        protected long inputSamples = 0;        // processed samples count

        public DataStabilizerParams Params { get; set; }

        public virtual T Sample
        {
            get
            {
                // requires update
                if (dirty)
                {
                    // samples count changed
                    if (samples.Length != Params.SamplesCount)
                    {
                        T[] data = new T[Params.SamplesCount];
                        Array.Copy(samples, data, Math.Min(samples.Length, Params.SamplesCount));
                        samples = data;

                        // drop result
                        result = DefaultValue();
                    }

                    // prepare to compute
                    LastApplied = true;

                    // process samples
                    if (Params.Enabled)
                        LastApplied = PrepareStabilizedSample();
                    // stabilizer is disabled - simply grab the fresh-most sample
                    else
                        result = samples[0];
                    dirty = false;
                }
                return result;
            }
            set
            {
                ValidateSample(value);

                // shift and push new value to the top
                T[] data = new T[Params.SamplesCount];
                Array.Copy(samples, 0, data, 1, Params.SamplesCount - 1);
                data[0] = value;
                samples = data;
                inputSamples++;

                // mark
                dirty = true;
            }
        }

        public bool LastApplied { get; private set; }

        protected DataStabilizerBase(DataStabilizerParams parameters)
        {
            Params = parameters;
            samples = new T[Params.SamplesCount];
            result = DefaultValue();
        }

        protected abstract bool PrepareStabilizedSample();

        protected abstract T ComputeAverageSample();

        protected abstract void ValidateSample(T sample);

        protected abstract T DefaultValue();
    }

    internal class PointsDataStabilizer : DataStabilizerBase<Point[]>
    {
        public bool PerPointProcessing { get; set; }

        public PointsDataStabilizer(DataStabilizerParams parameters)
            : base(parameters)
        {
            PerPointProcessing = true;
        }

        protected override void ValidateSample(Point[] sample)
        {
            if (null == sample || sample.Length == 0)
                throw new ArgumentException("sample: is null or empty array.");

            foreach (Point[] data in samples)
            {
                if (data != null && data.Length != sample.Length)
                    throw new ArgumentException("sample: invalid input data, length does not match.");
            }
        }

        protected override Point[] ComputeAverageSample()
        {
            if (inputSamples < Params.SamplesCount)
                return null;

            int sampleSize = samples[0].Length;
            Point[] average = new Point[sampleSize];
            for (int s = 0; s < Params.SamplesCount; ++s)
            {
                Point[] data = samples[s];
                for (int i = 0; i < sampleSize; ++i)
                    average[i] += data[i];
            }

            double inv = 1.0 / Params.SamplesCount;
            for (int i = 0; i < sampleSize; ++i)
                average[i] = new Point(average[i].X * inv + 0.5, average[i].Y * inv);
            return average;
        }

        protected override bool PrepareStabilizedSample()
        {
            Point[] average = ComputeAverageSample();
            if (null == average)
                return false;

            if (DefaultValue() == result)
            {
                result = average;
                return true;
            }

            double dmin = double.MaxValue, dmax = double.MinValue, dmean = 0.0;
            double[] distance = new double[result.Length];
            for (int i = 0; i < result.Length; ++i)
            {
                double d = Point.Distance(result[i], average[i]);

                dmean += d;
                dmax = Math.Max(dmax, d);
                dmin = Math.Min(dmin, d);
                distance[i] = d;
            }
            dmean /= result.Length;

            double edge = Params.Threshold;
            if (dmean > edge)
            {
                result = average;
                return true;
            }

            bool anyChanges = false;
            if (PerPointProcessing)
            {
                for (int i = 0; i < result.Length; ++i)
                {
                    if (distance[i] > edge)
                    {
                        anyChanges = true;
                        result[i] = average[i];
                    }
                }
            }
            return anyChanges;
        }

        protected override Point[] DefaultValue()
        {
            return null;
        }
    }

    internal class RectStabilizer : DataStabilizerBase<Rect>
    {
        public RectStabilizer(DataStabilizerParams parameters)
            : base(parameters)
        { }

        protected override Rect ComputeAverageSample()
        {
            Rect average = new Rect();
            if (inputSamples < Params.SamplesCount)
                return average;

            foreach (Rect rc in samples)
                average = average + rc;
            return average * (1.0 / Params.SamplesCount);
        }

        protected override void ValidateSample(Rect sample)
        { }

        protected override bool PrepareStabilizedSample()
        {
            Rect average = ComputeAverageSample();

            if (DefaultValue() == result)
            {
                result = average;
                return true;
            }

            double dmin = double.MaxValue, dmax = double.MinValue, dmean = 0.0;
            Point[] our = result.ToArray(), their = average.ToArray();
            for (int i = 0; i < 4; ++i)
            {
                double distance = Point.Distance(our[i], their[i]);
                dmin = Math.Min(distance, dmin);
                dmax = Math.Max(distance, dmax);
                dmean += distance;
            }
            dmean /= their.Length;

            if (dmin > Params.GetScaledThreshold())
            {
                result = average;
                return true;
            }

            return false;
        }

        protected override Rect DefaultValue()
        {
            return new Rect();
        }
    }
}