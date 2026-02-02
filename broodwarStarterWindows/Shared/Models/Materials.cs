namespace Shared.Models
{
    public class Materials
    {
        public int Minerals { get; set; }
        public int Gas { get; set; }

        public void Deconstruct(out int minerals, out int gas)
        {
            minerals = Minerals;
            gas = Gas;
        }
    }
}
