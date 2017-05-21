using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMATEC.WASmatec.Models
{
    /*
    public enum Periodicidad { Mensual, Diario, TiempoReal }
    public class Lectura
    {
        DateTime _Fecha;
        double _Temperatura;
        double _Humedad;
        double _CO;
        double _Consumo;

        #region getters/setters
        public DateTime Fecha
        {
            get
            {
                return _Fecha;
            }
        }
        public double Temperatura
        {
            get
            {
                return _Temperatura;
            }
        }
        public double Humedad
        {
            get
            {
                return _Humedad;
            }
        }
        public double CO
        {
            get
            {
                return _CO;
            }
        }
        public double Consumo
        {
            get
            {
                return _Consumo;
            }
        }
        #endregion

        public Lectura(DateTime Fecha, double Temperatura, double Humedad, double CO, double Consumo)
        {
            _Fecha = Fecha;
            _Temperatura = Temperatura;
            _Humedad = Humedad;
            _CO = CO;
            _Consumo = Consumo;
        }
    }

    public class DescripcionEquipo
    {
        string _ID = "";
        string _IP = "";
        string _NOMBRE = "";

        #region Getters/Setters
        public string ID
        {
            get
            {
                return _ID;
            }

            private set
            {
                _ID = value;
            }
        }

        public string IP
        {
            get
            {
                return _IP;
            }

            private set
            {
                _IP = value;
            }
        }

        public string NOMBRE
        {
            get
            {
                return _NOMBRE;
            }

            private set
            {
                _NOMBRE = value;
            }
        }
        #endregion

        public DescripcionEquipo(string Id, string Ip, string Nombre)
        {
            _ID = Id;
            _IP = Ip;
            _NOMBRE = Nombre;
        }

        

        public override string ToString()
        {
            return _NOMBRE + " - " + _IP;
        }
    }

    public class Equipo
    {
        DescripcionEquipo _Descripcion;
        IList<Lectura> _Lecturas;

        public string Id
        {
            get
            {
                return _Descripcion.ID;
            }
        }

        public string Ip
        {
            get
            {
                return _Descripcion.IP;
            }
        }

        public string Nombre
        {
            get
            {
                return _Descripcion.NOMBRE;
            }
        }

        public Equipo(string Id, string Ip, string Nombre)
        {
            _Descripcion = new DescripcionEquipo(Id, Ip, Nombre);

            _Lecturas = new List<Lectura>();
        }

        public Equipo(DescripcionEquipo Descripcion)
        {
            _Descripcion = Descripcion;

            _Lecturas = new List<Lectura>();
        }

        public int ObtenerLecturas(DateTime FechaInicio, Periodicidad TipoPeriodicidad, DateTime FechaFin)
        {
            int Res = -1;

            _Lecturas = new List<Lectura>();

            try
            {
                //TODO: Obtener datos en función de la periodicidad
                {
                    //relleno datos aleatlorios
                    Random rnd = new Random();
                    for (int i = 0;i< 70;i++)
                    {
                        _Lecturas.Add(new Lectura(DateTime.Now.AddMinutes(-i),
                                                    rnd.Next(150, 300) / 10.0,
                                                    rnd.Next(200, 400) / 10.0,
                                                    rnd.Next(1000, 7000) / 10.0,
                                                    rnd.Next(0, 60000) / 10.0));
                    }
                    
                }

                //Ordena para futura visualización
                _Lecturas = _Lecturas.OrderBy(Lec => Lec.Fecha).ToList();

                //Discretiza valores del eje de abscisas
                DiscretizaAbscisas();
            }
            catch(Exception)
            {

            }

            return Res;
        }

        private void DiscretizaAbscisas()
        {
            int salto = _Lecturas.Count / 60;
            if (salto > 1)
            {
                int i = 0;
                while (i < _Lecturas.Count)
                {
                    for (int j = 0; j < salto; j++)
                    {
                        i++;
                        if (i < _Lecturas.Count)
                        {
                            _Lecturas[i] = 
                                new Lectura(DateTime.MinValue, 
                                            _Lecturas[i].Temperatura, 
                                            _Lecturas[i].Humedad, 
                                            _Lecturas[i].CO, 
                                            _Lecturas[i].Consumo);
                        }
                        else
                        {
                            break;
                        }
                    }

                    i++;
                }
            }
        }

        public IList<DateTime> LecturasFecha()
        {
            return _Lecturas.Select(Lec => Lec.Fecha).ToList();
        }
        public IList<double> LecturasTemperatura()
        {
           return _Lecturas.Select(Lec => Lec.Temperatura).ToList();
        }
        public IList<double> LecturasHumedad()
        {
            return _Lecturas.Select(Lec => Lec.Humedad).ToList();
        }
        public IList<double> LecturasCO()
        {
            return _Lecturas.Select(Lec => Lec.CO).ToList();
        }
        public IList<double> LecturasConsumo()
        {
            return _Lecturas.Select(Lec => Lec.Consumo).ToList();
        }
    }

    public class Equipos
    {
        IList<DescripcionEquipo> _Equipos;

        public Equipos()
        {
            _Equipos = new List<DescripcionEquipo>();
            //TODO: Obtiene la lista de los equipos registrados
            _Equipos.Add(new DescripcionEquipo("1", "10.28.1.244", "CPD Madrid"));
            _Equipos.Add(new DescripcionEquipo("2", "10.48.1.244", "CPD Bilbao"));
            _Equipos.Add(new DescripcionEquipo("3", "N.D.", "CPD Santander"));
        }

        public IList<DescripcionEquipo> Lista
        {
            get
            {
                return _Equipos.Where(Eq => Eq.ID != "").ToList();
            }
        }
    }*/
}
