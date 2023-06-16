namespace Barotrauma.Items.Components
{
    partial struct Signal
    {
        public string value;
        public int stepsTaken;
        public Character sender;
        public Item source;
        public float power;
        public float strength;
        public readonly double CreationTime;

        public double TimeSinceCreated => Timing.TotalTimeUnpaused - CreationTime;

        public Signal(string value, int stepsTaken = 0, Character sender = null,
                        Item source = null, float power = 0.0f, float strength = 1.0f)
        {
            this.value = value;
            this.stepsTaken = stepsTaken;
            this.sender = sender;
            this.source = source;
            this.power = power;
            this.strength = strength;
            CreationTime = Timing.TotalTimeUnpaused;
        }

        internal Signal WithStepsTaken(int stepsTaken)
        {
            Signal retVal = this;
            retVal.stepsTaken = stepsTaken;
            return retVal;
        }

        public static bool operator ==(Signal a, Signal b) =>
            a.value == b.value &&
            a.stepsTaken == b.stepsTaken &&
            a.sender == b.sender &&
            a.source == b.source &&
            MathUtils.NearlyEqual(a.power, b.power) &&
            MathUtils.NearlyEqual(a.strength, b.strength);

        public static bool operator !=(Signal a, Signal b) => !(a == b);
    }
}
