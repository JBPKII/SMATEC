using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMATEC
{
    public static class ParserLectura
    {
        private static char SeparadorCampo = '|';
        private static char SeparadorLectura = '@';
        public static Lectura Parse(string CadenaLecturas)
        {
            DateTime _Tiempo = DateTime.Now;
            string _Equipo = "";

            double _Temperatura = 0.0;
            double _Humedad = 0.0;
            double _CO = 0.0;
            double _Consumo = 0.0;

            string[] ResCampos = CadenaLecturas.Split(SeparadorCampo);

            if (ResCampos.Count() >= 2)
            {
                //primer campo, Tiempo
                if (!DateTime.TryParse(ResCampos[0], out _Tiempo))
                {
                    _Tiempo = DateTime.Now;
                }
                //segundo campo, Equipo
                _Equipo = ResCampos[1];

                //resto lecturas con identificador
                for (int i = 2; i < ResCampos.Length; i++)
                {
                    string[] TempCampo = ResCampos[i].Split(SeparadorLectura);
                    if (TempCampo.Count() == 2)
                    {
                        switch (TempCampo[0])
                        {
                            case "T":
                                double.TryParse(TempCampo[1].Replace(".",","), out _Temperatura);
                                break;
                            case "H":
                                double.TryParse(TempCampo[1].Replace(".", ","), out _Humedad);
                                break;
                            case "C":
                                double.TryParse(TempCampo[1].Replace(".", ","), out _CO);
                                break;
                            case "W":
                                double.TryParse(TempCampo[1].Replace(".", ","), out _Consumo);
                                break;
                        }
                    }
                }
            }

            Lectura TempLectura = new Lectura(_Tiempo, _Equipo, _Temperatura, _Humedad, _CO, _Consumo);

            return TempLectura;
        }
    }
}