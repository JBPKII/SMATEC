using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMATEC.WASmatec.Controllers
{
    public class HomeController : Controller
    {
        /*public ActionResult Index()
        {
            return View();
        }*/

        public ActionResult About()
        {
            ViewBag.Message = "Sistema de Monitorización y Alerta Temprana de Entornos Controlados.";

            return View();
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Últimos valores registrados.";

            WSRefSMATEC.RecopiladorDatosClient client = new WSRefSMATEC.RecopiladorDatosClient();

            IList<string> sLecturasActuales = client.GetCurrent();
            IList<Lectura> LecturasActuales = new List<Lectura>();

            foreach (string sLec in sLecturasActuales)
            {
                LecturasActuales.Add(new Lectura(sLec));
            }

            ViewBag.ColumnName = ListConcat(LecturasActuales.Select(T => T.IpEquipo).ToList());

            ViewBag.ValuesTemperatura = ListConcat(LecturasActuales.Select(T => T.Temperatura).ToList());
            ViewBag.ValuesHumedad = ListConcat(LecturasActuales.Select(T => T.Humedad).ToList());
            ViewBag.ValuesCO = ListConcat(LecturasActuales.Select(T => T.CO).ToList());
            ViewBag.ValuesConsumo = ListConcat(LecturasActuales.Select(T => T.Consumo).ToList());

            return View();
        }

        public ActionResult Historico(string selectedID = "-1", 
            Periodicidad selectedPeriodicidad = Periodicidad.TiempoReal,
            string FechaInicio = "",
            string FechaFin = "")
        {
            ViewBag.Message = "Cronología de los valores registrados.";

            List<Periodicidad> periodicidadList = Enum.GetValues(typeof(Periodicidad)).Cast<Periodicidad>().ToList();
            ViewData["Periodicidad"] = new SelectList(periodicidadList, selectedPeriodicidad);

            WSRefSMATEC.RecopiladorDatosClient client = new WSRefSMATEC.RecopiladorDatosClient();

            IList<DescripcionEquipo> ListaEquipos = client.GetEquipos().ToList<DescripcionEquipo>();

            Equipo CurrEq;
            if (ListaEquipos.Where(Eq => Eq.ID == selectedID).Count() == 0)
            {
                if (ListaEquipos.Count > 0)
                {
                    selectedID = ListaEquipos.First().ID;
                }
            }

            ViewData["Equipos"] = new SelectList(ListaEquipos, "ID", "NOMBRE", selectedID);

            if (selectedID == "-1" || selectedID == "null")
            {
                CurrEq = new Equipo("-1", "0.0.0.0", "Sin equipos.");
            }
            else
            {
                CurrEq = new Equipo(ListaEquipos.Where(Eq => Eq.ID == selectedID).First());
            }

            DateTime dtFechaInicio = ObtieneFecha(FechaInicio, -30);
            DateTime dtFechaFin = ObtieneFecha(FechaFin, 0);

            //evalua la periodicidad y restringe la ventana temporal
            switch (selectedPeriodicidad)
            {
                case Periodicidad.TiempoReal:
                    //máximo 2 horas desde el límite inferior
                    if (dtFechaFin < dtFechaInicio || (dtFechaFin - dtFechaInicio).TotalHours > 1.0)
                    {
                        dtFechaFin = dtFechaInicio.AddHours(1.0);
                    }
                    break;
                case Periodicidad.Diario:
                    //máximo 6 meses desde el límite inferior
                    if (dtFechaFin < dtFechaInicio || (dtFechaFin.Date - dtFechaInicio.Date).TotalDays > 30.0 * 6.0)
                    {
                        dtFechaFin = dtFechaInicio.AddMonths(6);
                    }
                    break;
                case Periodicidad.Mensual:
                    //máximo 200 años desde el límite inferior
                    if (dtFechaFin < dtFechaInicio || (dtFechaFin.Year - dtFechaInicio.Year) > 200)
                    {
                        dtFechaFin = dtFechaInicio.AddYears(200);
                    }
                    break;
            }
            CurrEq.Lecturas.Clear();
            IList<string> CollCect = client.GetLecturas(CurrEq.Id, selectedPeriodicidad, dtFechaInicio, dtFechaFin).ToList<string>();
            foreach (string Lec in CollCect)
            {
                CurrEq.Lecturas.Add(new Lectura(Lec));
            }
            //CurrEq.Lecturas = client.GetLecturas(CurrEq.Id, selectedPeriodicidad, dtFechaInicio, dtFechaFin);

            ViewBag.ColumnName = ListConcat(CurrEq.LecturasFecha(), selectedPeriodicidad);

            ViewBag.ValuesTemperatura = ListConcat(CurrEq.LecturasTemperatura());
            ViewBag.ValuesHumedad = ListConcat(CurrEq.LecturasHumedad());
            ViewBag.ValuesCO = ListConcat(CurrEq.LecturasCO());
            ViewBag.ValuesConsumo = ListConcat(CurrEq.LecturasConsumo());

            ViewBag.FechaInicio =  dtFechaInicio.ToString("dd/MM/yyyy HH:mm");
            ViewBag.FechaFin = dtFechaFin.ToString("dd/MM/yyyy HH:mm");

            return View();
        }

        private string ListConcat(IList<DateTime> Lista, Periodicidad Period)
        {
            string Temp = "";
            if (Lista != null)
            {
                string FiltroFecha = "";
                switch (Period)
                {
                    case Periodicidad.Mensual:
                        FiltroFecha = "MM/yyyy";
                        break;
                    case Periodicidad.Diario:
                        FiltroFecha = "dd/MM/yyyy";
                        break;
                    case Periodicidad.TiempoReal:
                    default:
                        FiltroFecha = "dd/MM/yyyy HH:mm";
                        break;
                }

                foreach (DateTime obj in Lista)
                {
                    if (Temp == "")
                    {
                        Temp = (obj == DateTime.MinValue ? "" : obj.ToString(FiltroFecha));
                    }
                    else
                    {
                        Temp += ("|" + (obj == DateTime.MinValue ? "" : obj.ToString(FiltroFecha)));
                    }
                }
            }

            return Temp;
        }
        private string ListConcat(IList<string> Lista)
        {
            string Temp = "";
            if (Lista != null)
            {
                foreach (string obj in Lista)
                {
                    if (Temp == "")
                    {
                        Temp = obj;
                    }
                    else
                    {
                        Temp += ("|" + obj);
                    }
                }
            }

            return Temp;
        }
        private string ListConcat(IList<double> Lista)
        {
            string Temp = "";
            if (Lista != null)
            {
                foreach (double obj in Lista)
                {
                    if (Temp == "")
                    {
                        Temp = obj.ToString().Replace(',', '.');
                    }
                    else
                    {
                        Temp += ("|" + obj.ToString().Replace(',', '.'));
                    }
                }
            }

            return Temp;
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "AvalonGeek.";

            return View();
        }

        private DateTime ObtieneFecha(string Fecha, int NowMenosMinutos=-30)
        {
            DateTime DRes = DateTime.Now.AddMinutes(NowMenosMinutos);

            try
            {
                if (Fecha != "")
                {
                    if(!DateTime.TryParse(Fecha, out DRes))
                    {
                        throw new Exception("Fallo lógico al parsear la fecha.");
                    }
                }
            }
            catch (Exception)
            {
                DRes = DateTime.Now.AddYears(NowMenosMinutos);
            }

            return DRes;
        }
    }
}