namespace YggioUnity
{
    public class UUID
    {
        public long MostSigBits { get; set; }

        public long LeastSigBits { get; set; }

        public UUID(long mostSigBits, long leastSigBits)
        {
            MostSigBits = mostSigBits;
            LeastSigBits = leastSigBits;
        }
    }
}