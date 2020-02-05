using System.Collections.Generic;

namespace ChanSort.Api
{
    public class Satellite
    {
        public int Id { get; }
        public string Name { get; set; }
        public string OrbitalPosition { get; set; }
        public IDictionary<int, Transponder> Transponder { get; } = new Dictionary<int, Transponder>();

        public Satellite(int id)
        {
            Id = id;
        }

        public LnbConfig LnbConfig { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}