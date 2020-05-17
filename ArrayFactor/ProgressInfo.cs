using System;
using MathCore.Values;

namespace ArrayFactor
{
    public class ProgressInfo : Progress<double>
    {
        private DateTime _LastTime = DateTime.Now;
        private const int c_AverageSpeedCount = 100;
        private AverageValue _AverageSpeed = new AverageValue(c_AverageSpeedCount);

        public double Value { get; private set; }

        public event EventHandler ValueChanged;

        public bool InProgress { get; private set; }
        public event EventHandler InProgressChanged;

        public bool Complited { get; private set; }
        public event EventHandler ComplitedChanged;

        public DateTime Started { get; private set; }
        public event EventHandler StartedChanged;

        public TimeSpan Elapsed { get; private set; }
        public event EventHandler ElapsedChanged;

        public TimeSpan Remain { get; private set; }
        public event EventHandler RemainChanged;

        public double Speed { get; private set; }
        public event EventHandler SpeedChanged;

        public double AverageSpeed { get; private set; }
        public event EventHandler AverageSpeedChanged;

        public double SpeedDispersion { get; private set; }
        public event EventHandler SpeedDispersionChanged;

        /// <inheritdoc />
        protected override void OnReport(double value)
        {
            base.OnReport(value);
            var now = DateTime.Now;
            if (InProgress)
            {
                if (value == 1)
                {
                    InProgress = false;
                    InProgressChanged?.Invoke(this, EventArgs.Empty);
                    Complited = true;
                    ComplitedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (value > 0 && value < 1)
            {
                Complited = false;
                ComplitedChanged?.Invoke(this, EventArgs.Empty);
                InProgress = true;
                InProgressChanged?.Invoke(this, EventArgs.Empty);
                Started = now;
                StartedChanged?.Invoke(this, EventArgs.Empty);
                _LastTime = now;
                _AverageSpeed.Reset();
            }
            Value = value;
            ValueChanged?.Invoke(this, EventArgs.Empty);

            _LastTime = now;
            var elapsed = now - Started;
            Elapsed = elapsed;
            ElapsedChanged?.Invoke(this, EventArgs.Empty);
            if (elapsed.TotalSeconds == 0 || value == 0) return;

            var speed = value / elapsed.TotalSeconds;
            Speed = speed * 100;
            SpeedChanged?.Invoke(this, EventArgs.Empty);
            var average_speed = _AverageSpeed.AddValue(speed);
            AverageSpeed = average_speed * 100;
            AverageSpeedChanged?.Invoke(this, EventArgs.Empty);
            SpeedDispersion = _AverageSpeed.Dispersion * 100;
            SpeedDispersionChanged?.Invoke(this, EventArgs.Empty);
            var t = (1 - value) / average_speed;
            Remain = TimeSpan.FromSeconds(t);
            RemainChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}