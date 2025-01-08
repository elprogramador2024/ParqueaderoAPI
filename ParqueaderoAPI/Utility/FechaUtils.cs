using ParqueaderoAPI.Models.ViewModels.Indicadores;

namespace ParqueaderoAPI.Utility
{
    public class FechaUtils
    {
        public static (DateTime, DateTime) CalcularFechas(RangoDias rangoDias)
        {
            Dictionary<RangoDias, Func<(DateTime, DateTime)>> calculaFechas = new Dictionary<RangoDias, Func<(DateTime, DateTime)>>
            {
                { RangoDias.DIARIO, CalcularDiaActual },
                { RangoDias.SEMANAL, CalcularSemanaActual },
                { RangoDias.MENSUAL, CalcularMesActual },
                { RangoDias.ANUAL, CalcularAnoActual }
            };

            calculaFechas.TryGetValue(rangoDias, out var calculaFecha);

            return calculaFecha();
        }

        private static (DateTime, DateTime) CalcularDiaActual()
        {
            DateTime hoy = DateTime.Now.Date;
            DateTime fechaFin = hoy.AddDays(1).AddTicks(-1);

            return (hoy, fechaFin);
        }

        private static (DateTime, DateTime) CalcularSemanaActual()
        {
            DateTime hoy = DateTime.Now.Date;

            int diaActual = (int)hoy.DayOfWeek;
            diaActual = diaActual == 0 ? 7 : diaActual;

            DateTime fechaIni = hoy.AddDays(-(diaActual - 1)); //Lunes
            DateTime fechaFin = hoy.AddDays(7).AddTicks(-1); //Domingo

            return (fechaIni, fechaFin);
        }

        private static (DateTime, DateTime) CalcularMesActual()
        {
            DateTime hoy = DateTime.Now.Date;

            DateTime fechaIni = new DateTime(hoy.Year, hoy.Month, 1);
            DateTime fechaFin = fechaIni.AddMonths(1).AddTicks(-1);

            return (fechaIni, fechaFin);
        }

        private static (DateTime, DateTime) CalcularAnoActual()
        {
            DateTime hoy = DateTime.Now.Date;
            DateTime fechaIni = new DateTime(hoy.Year, 1, 1);
            DateTime fechaFin = fechaIni.AddYears(1).AddTicks(-1);

            return (fechaIni, fechaFin);
        }
    }
}
