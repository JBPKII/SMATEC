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
    public class Lectura
    {
        string _IpEquipo = "";
        DateTime _Fecha = DateTime.Now;
        double _Temperatura = 0.0;
        double _Humedad = 0.0;
        double _CO = 0.0;
        double _Consumo = 0.0;

        #region getters/setters
        [DataMember]
        public string IpEquipo
        {
            get
            {
                return _IpEquipo;
            }
            set
            {
                value= _IpEquipo;
            }
        }
        [DataMember]
        public DateTime Fecha
        {
            get
            {
                return _Fecha;
            }
            set
            {
                value = _Fecha;
            }
        }
        [DataMember]
        public double Temperatura
        {
            get
            {
                return _Temperatura;
            }
            set
            {
                value = _Temperatura;
            }
        }
        [DataMember]
        public double Humedad
        {
            get
            {
                return _Humedad;
            }
            set
            {
                value = _Humedad;
            }
        }
        [DataMember]
        public double CO
        {
            get
            {
                return _CO;
            }
            set
            {
                value = _CO;
            }
        }
        [DataMember]
        public double Consumo
        {
            get
            {
                return _Consumo;
            }
            set
            {
                value= _Consumo;
            }
        }
        #endregion

        /*public Lectura()
        {
            _Fecha = DateTime.Now;
            _IpEquipo = "";

            _Temperatura = 0.0;
            _Humedad = 0.0;
            _CO = 0.0;
            _Consumo = 0.0;
        }*/

        public Lectura(DateTime Tiempo, string Equipo, double Temperatura, double Humedad, double CO, double Consumo)
        {
            Tiempo = _Fecha = Tiempo;
            Equipo = _IpEquipo = Equipo;

            _Temperatura = Temperatura;
            _Humedad = Humedad;
            _CO = CO;
            _Consumo = Consumo;
        }

        public Lectura(string CadenaLecturas)
        {
            Lectura TempLectura = SMATEC.ParserLectura.Parse(CadenaLecturas);

            _Fecha = TempLectura.Fecha;
            _IpEquipo = TempLectura.IpEquipo;

            _Temperatura = TempLectura.Temperatura;
            _Humedad = TempLectura.Humedad;
            _CO = TempLectura.CO;
            _Consumo = TempLectura.Consumo;
        }

        public override string ToString()
        {
            return _Fecha.ToString("yyyy-MM-dd HH:mm:ss") + "|" + _IpEquipo +"|T@"+_Temperatura.ToString("0.00")+ "|H@" + _Humedad.ToString("0.00") + "|C@"+_CO.ToString("0.0000") + "|W@" + _Consumo.ToString("0.00");
        }
    }
}