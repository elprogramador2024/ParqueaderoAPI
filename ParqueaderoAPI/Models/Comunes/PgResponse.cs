using System.Threading;

namespace ParqueaderoAPI.Models.Comunes
{
    public class PgResponse<T>
    {
        public int Total { get; set; }
        public int Pgnum { get; set; }
        public int Pgsize { get; set; }
        public int Totpages { get; set; }
        public List<T> Lista { get; set; }
    }
}
