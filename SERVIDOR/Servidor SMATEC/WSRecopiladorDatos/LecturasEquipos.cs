using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Serialization;

namespace SMATEC
{
    [DataContract]
    public enum Periodicidad { Mensual, Diario, TiempoReal }

    [DataContract]
    public class Lectura
    {
        string _Equipo;
        DateTime _Fecha;
        double _Temperatura;
        double _Humedad;
        double _CO;
        double _Consumo;

        #region getters/setters
        [DataMember]
        public string Equipo
        {
            get
            {
                return _Equipo;
            }
        }
        [DataMember]
        public DateTime Fecha
        {
            get
            {
                return _Fecha;
            }
        }
        [DataMember]
        public double Temperatura
        {
            get
            {
                return _Temperatura;
            }
        }
        [DataMember]
        public double Humedad
        {
            get
            {
                return _Humedad;
            }
        }
        [DataMember]
        public double CO
        {
            get
            {
                return _CO;
            }
        }
        [DataMember]
        public double Consumo
        {
            get
            {
                return _Consumo;
            }
        }
        #endregion

        public Lectura()
        {
            _Temperatura = 0.0;
            _Humedad = 0.0;
            _CO = 0.0;
            _Consumo = 0.0;
            _Fecha = DateTime.Now;
            _Equipo = "";
        }

        public Lectura(DateTime Fecha, double Temperatura, double Humedad, double CO, double Consumo)
        {
            _Fecha = Fecha;
            _Temperatura = Temperatura;
            _Humedad = Humedad;
            _CO = CO;
            _Consumo = Consumo;
        }

        public Lectura(DateTime Tiempo, string Equipo, double Temperatura, double Humedad, double CO, double Consumo)
        {
            Tiempo = _Fecha;
            Equipo = _Equipo;

            Temperatura = _Temperatura;
            Humedad = _Humedad;
            CO = _CO;
            Consumo = _Consumo;
        }

        public Lectura(string CadenaLecturas)
        {
            Lectura TempLectura = SMATEC.ParserLectura.Parse(CadenaLecturas);

            _Fecha = TempLectura.Fecha;
            _Equipo = TempLectura.Equipo;

            _Temperatura = TempLectura.Temperatura;
            _Humedad = TempLectura.Humedad;
            _CO = TempLectura.CO;
            _Consumo = TempLectura.Consumo;
        }

        public bool ToBaseDatos(ref MySql.Data.MySqlClient.MySqlConnection MySQLConn, MySql.Data.MySqlClient.MySqlTransaction MySQLTrns)
        {
            string InsertSQL = string.Format("INSERT" + "\n" +
                    "INTO {0} ({1}, {3}, {5}, {7}, {9}, {11})" + "\n" +
                    "VALUES({2}, {4}, {6}, {8}, {10}, {12});",
                        "[00input]",
                        "fecha", _Fecha.ToShortDateString() + " " + _Fecha.ToShortTimeString(),
                        "equipo", string.Format("N'{0}'", _Equipo),
                        "temperatura", string.Format("N'{0}'", _Temperatura.ToString("0.00")),
                        "humedad", string.Format("N'{0}'", _Humedad.ToString("0.00")),
                        "co", string.Format("N'{0}'", _CO.ToString("0.0000")),
                        "consumo", string.Format("N'{0}'", _Consumo.ToString("0.00")));

            MySql.Data.MySqlClient.MySqlCommand MySQLComm = new MySql.Data.MySqlClient.MySqlCommand(InsertSQL, MySQLConn, MySQLTrns);

            bool Res = false;
            try
            {
                if (MySQLComm.ExecuteNonQuery() == 1)
                {
                    MySQLTrns.Commit();
                    Res = true;
                }
                else
                {
                    MySQLTrns.Rollback();
                    Res = false;
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException)
            {
                Res = false;
            }

            return Res;
        }
    }

    [DataContract]
    public class DescripcionEquipo
    {
        string _ID = "";
        string _IP = "";
        string _NOMBRE = "";

        #region Getters/Setters
        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

    [DataContract]
    public class Equipo
    {
        DescripcionEquipo _Descripcion;
        IList<Lectura> _Lecturas;

        [DataMember]
        public string Id
        {
            get
            {
                return _Descripcion.ID;
            }
        }

        [DataMember]
        public string Ip
        {
            get
            {
                return _Descripcion.IP;
            }
        }

        [DataMember]
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

        [OperationContract]
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
                    for (int i = 0; i < 70; i++)
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
            catch (Exception)
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

    [DataContract]
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
    }
}