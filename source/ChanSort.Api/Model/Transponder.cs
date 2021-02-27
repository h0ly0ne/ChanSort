namespace ChanSort.Api
{
    public class Transponder
    {
        public int Id { get; }
        public Satellite Satellite { get; set; }
        public decimal FrequencyInMhz { get; set; }
        public int Number { get; set; }
        public virtual int SymbolRate { get; set; }
        public char Polarity { get; set; }
        public int OriginalNetworkId { get; set; }
        public int TransportStreamId { get; set; }

        public Transponder(int id)
        {
            Id = id;
        }
    }
}