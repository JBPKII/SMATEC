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
    public enum Periodicidad
    {
        [EnumMember(Value = "Mensual")]
        Mensual,
        [EnumMember(Value = "Diario")]
        Diario,
        [EnumMember(Value = "TiempoReal")]
        TiempoReal
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

        public IList<Lectura> Lecturas
        {
            set
            {
                _Lecturas = value;
            }
            get
            {
                return _Lecturas;
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

        /*private void DiscretizaAbscisas()
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
                                            "",
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
        }*/

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
}