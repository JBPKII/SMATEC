using System;
using System.Collections.Generic;
using System.Linq;
/*using System.Runtime.Serialization;*/
using System.ServiceModel;
/*using System.ServiceModel.Web;
using System.Text;*/

namespace SMATEC.WSRecopiladorDatos
{
    [System.ServiceModel.Activation.AspNetCompatibilityRequirements(RequirementsMode = System.ServiceModel.Activation.AspNetCompatibilityRequirementsMode.Required)]
    public class RecopiladorDatos : IRecopiladorDatos, IRecopiladorDatosRest, IRecopiladorDatosTeselador
    {
        internal static bool PrimeraEjecucion = true;
        public string GetServiceDescriptionInfo()
        {
            string info = "";
            OperationContext operationContext = OperationContext.Current;
            ServiceHost host = (ServiceHost)operationContext.Host;
            System.ServiceModel.Description.ServiceDescription desc = host.Description;
            // Enumerate the base addresses in the service host.
            info += "Base addresses:\n";
            foreach (Uri uri in host.BaseAddresses)
            {
                info += "    " + uri + "\n";
            }
            // Enumerate the service endpoints in the service description.
            info += "Service endpoints:\n";
            foreach (System.ServiceModel.Description.ServiceEndpoint endpoint in desc.Endpoints)
            {
                info += "    Address:  " + endpoint.Address + "\n";
                info += "    Binding:  " + endpoint.Binding.Name + "\n";
                info += "    Contract: " + endpoint.Contract.Name + "\n";
            }
            return info;
        }

        internal static MySql.Data.MySqlClient.MySqlConnection MySQLConn =
            new MySql.Data.MySqlClient.MySqlConnection(
                string.Format("Server={0};Database={1};Uid={2};Pwd={3};" ,//SQL SERVER: "Data Source={0};Initial Catalog={1};User ID={2};Password={3};{4}",
                    Properties.Settings.Default.BBDDServ,
                    Properties.Settings.Default.BBDDSchema,
                    Properties.Settings.Default.BBDDUser,
                    Properties.Settings.Default.BBDDPass));

        internal static ErrorHandler ErrLogger = 
            new ErrorHandler(
                Properties.Settings.Default.LogSource, 
                Properties.Settings.Default.LogType);

        private RecopiladorDatos()
        {
            //Si no existen las tablas las crea
            if (AbrirConexion())
            {
                if (PrimeraEjecucion)
                {
                    IList<string> Cmds = new List<string>();
                    Cmds.Add("CREATE TABLE IF NOT EXISTS `00equipos` (" + "\n" +
                             " `id` INT(10) NOT NULL AUTO_INCREMENT," + "\n" +
                             " `ip` varchar(20) NOT NULL," + "\n" +
                             " `nombre` varchar(100) DEFAULT NULL," + "\n" +
                             "PRIMARY KEY(`id`)" +
                            ") ENGINE = InnoDB DEFAULT CHARSET = utf8; ");

                    Cmds.Add("CREATE TABLE IF NOT EXISTS `01input` (" + "\n" +
                             " `fecha` datetime NOT NULL," + "\n" +
                             " `equipo` varchar(20) NOT NULL," + "\n" +
                             " `temperatura` double DEFAULT NULL," + "\n" +
                             " `humedad` double DEFAULT NULL," + "\n" +
                             " `co` double DEFAULT NULL," + "\n" +
                             " `consumo` double DEFAULT NULL," + "\n" +
                             "PRIMARY KEY(`fecha`, `equipo`)" +
                            ") ENGINE = InnoDB DEFAULT CHARSET = utf8; ");
                    Cmds.Add("CREATE TABLE IF NOT EXISTS `02diario` (" + "\n" +
                             " `fecha` datetime NOT NULL," + "\n" +
                             " `equipo` varchar(20) NOT NULL," + "\n" +
                             " `temperatura` double DEFAULT NULL," + "\n" +
                             " `humedad` double DEFAULT NULL," + "\n" +
                             " `co` double DEFAULT NULL," + "\n" +
                             " `consumo` double DEFAULT NULL," + "\n" +
                             "PRIMARY KEY(`fecha`, `equipo`)" +
                            ") ENGINE = InnoDB DEFAULT CHARSET = utf8; ");
                    Cmds.Add("CREATE TABLE IF NOT EXISTS `03mensual` (" + "\n" +
                             " `fecha` datetime NOT NULL," + "\n" +
                             " `equipo` varchar(20) NOT NULL," + "\n" +
                             " `temperatura` double DEFAULT NULL," + "\n" +
                             " `humedad` double DEFAULT NULL," + "\n" +
                             " `co` double DEFAULT NULL," + "\n" +
                             " `consumo` double DEFAULT NULL," + "\n" +
                             "PRIMARY KEY(`fecha`, `equipo`)" +
                            ") ENGINE = InnoDB DEFAULT CHARSET = utf8; ");

                    foreach (string Cmd in Cmds)
                    {
                        MySql.Data.MySqlClient.MySqlCommand TempCMD = new MySql.Data.MySqlClient.MySqlCommand(Cmd, MySQLConn);
                        TempCMD.ExecuteNonQuery();
                    }
                    PrimeraEjecucion = false;
                }
            }
        }


        /*~RecopiladorDatos()
        {
            //Finaliza el web service
            MySQLConn.Close();
            MySQLConn.Dispose();
        }*/

        private bool AbrirConexion(bool Reiniciar = false)
        {
            try
            {
                if (MySQLConn != null)
                {
                    if (MySQLConn.State == System.Data.ConnectionState.Open)
                    {
                        //Ya está abierta
                        if(Reiniciar)
                        {
                            ReiniciarConexion();
                        }
                    }
                    else if (MySQLConn.State == System.Data.ConnectionState.Closed)
                    {
                        MySQLConn.Open();
                    }
                    else if (MySQLConn.State == System.Data.ConnectionState.Connecting)
                    {
                        lock (this)
                        {
                            while (MySQLConn.State != System.Data.ConnectionState.Open)
                            {
                                System.Threading.Thread.Sleep(10);
                            }
                        }
                    }
                    else
                    {
                        //reinicia la conexión
                        ReiniciarConexion();
                    }

                    return true;
                }
                else
                {
                    ErrLogger.LogError("Sin conexión con SQLServer.", System.Diagnostics.EventLogEntryType.Error);
                    return false;
                }
            }
            catch (System.Data.SqlClient.SqlException SqlEx)
            {
                switch (SqlEx.Number)
                {
                    case 0:
                        ErrLogger.LogError("No se puede conectar con el servidor. Contacte con el administrador", System.Diagnostics.EventLogEntryType.Error);
                        break;
                    case 1045:
                        ErrLogger.LogError("Usuario/Clave incorrectos", System.Diagnostics.EventLogEntryType.Error);
                        break;
                    default:
                        ErrLogger.LogError(SqlEx.ToString(), System.Diagnostics.EventLogEntryType.Error);
                        break;
                }
                return false;
            }
            catch (Exception Ex)
            {
                ErrLogger.LogError(Ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        private void ReiniciarConexion()
        {
            try
            {
                MySQLConn.Close();
            }
            catch (System.Exception)
            {
                MySQLConn.Dispose();
            }
            finally
            {
                MySQLConn.Open();
            }
        }

        public bool SetData(string CadenaLectura)
        {
            try
            {
                if (AbrirConexion())
                {
                    Lectura RecLectura = new Lectura(CadenaLectura);

                    bool Res = false;

                    lock (this)
                    {
                        string sSql = "";
                        try
                        {
                            sSql = "SELECT ip FROM `00equipos` WHERE (ip = '" + RecLectura.IpEquipo + "')";

                            MySql.Data.MySqlClient.MySqlCommand MySQLComm =
                            new MySql.Data.MySqlClient.MySqlCommand(sSql, MySQLConn);

                            MySql.Data.MySqlClient.MySqlDataReader DR1 = MySQLComm.ExecuteReader();

                            bool hasRows = DR1.HasRows;
                            DR1.Close();
                            DR1.Dispose();

                            if (!hasRows)
                            {
                                sSql = "INSERT INTO `00equipos` (ip, nombre) VALUES('" + RecLectura.IpEquipo + "', 'Pendiente de Nombre');";

                                //no existe el equipo
                                MySQLComm = new MySql.Data.MySqlClient.MySqlCommand(sSql, MySQLConn);
                                MySQLComm.ExecuteNonQuery();
                            }

                            sSql = string.Format("INSERT" + "\n" +
                                            "INTO {0} ({1}, {3}, {5}, {7}, {9}, {11})" + "\n" +
                                            "VALUES('{2}', '{4}', {6}, {8}, {10}, {12});",
                                                "`01input`",
                                                "fecha", RecLectura.Fecha.ToString("yyyy-MM-dd HH:mm:ss"),
                                                "equipo", RecLectura.IpEquipo,
                                                "temperatura", RecLectura.Temperatura.ToString("0.00").Replace(',','.'),
                                                "humedad", RecLectura.Humedad.ToString("0.00").Replace(',', '.'),
                                                "co", RecLectura.CO.ToString("0.0000").Replace(',', '.'),
                                                "consumo", RecLectura.Consumo.ToString("0.00").Replace(',', '.'));
                            //TODO: UPDATE en 00current para la última lectura
                            MySQLComm = new MySql.Data.MySqlClient.MySqlCommand(sSql, MySQLConn);
                            MySQLComm.ExecuteNonQuery();

                            sSql = string.Format("INSERT INTO {0} ({1}, {3}, {5}, {7}, {9}, {11})" + "\n" +
                                                 "VALUES('{2}', '{4}', {6}, {8}, {10}, {12})" + "\n" +
                                                 "ON DUPLICATE KEY" + "\n" +
                                                 "UPDATE {1}='{2}', {5}={6}, {7}={8}, {9}={10}, {11}={12}",
                                                    "`00current`",
                                                    "fecha", RecLectura.Fecha.ToString("yyyy-MM-dd HH:mm:ss"),
                                                    "equipo", RecLectura.IpEquipo,
                                                    "temperatura", RecLectura.Temperatura.ToString("0.00").Replace(',', '.'),
                                                    "humedad", RecLectura.Humedad.ToString("0.00").Replace(',', '.'),
                                                    "co", RecLectura.CO.ToString("0.0000").Replace(',', '.'),
                                                    "consumo", RecLectura.Consumo.ToString("0.00").Replace(',', '.'));

                            MySQLComm = new MySql.Data.MySqlClient.MySqlCommand(sSql, MySQLConn);
                            MySQLComm.ExecuteNonQuery();

                            Res = true;
                        }
                        catch (MySql.Data.MySqlClient.MySqlException MyEx)
                        {
                            ErrLogger.LogError(MyEx.ToString() + "\n" + sSql, System.Diagnostics.EventLogEntryType.Error);
                            Res = false;
                        }
                    }

                    return Res;
                }
                else
                {
                    ErrLogger.LogError("Sin conexión con SQLServer.", System.Diagnostics.EventLogEntryType.Error);
                    return false;
                }
            }
            catch (System.Data.SqlClient.SqlException SqlEx)
            {
                ErrLogger.LogError(SqlEx.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
            catch (Exception Ex)
            {
                ErrLogger.LogError(Ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        public void TeselarData()
        {
            try
            {
                if (AbrirConexion())
                {
                    lock (this)
                    {
                        string sSql = "";
                        try
                        {
                            sSql = "SELECT DISTINCT ip FROM `00equipos`;";

                            MySql.Data.MySqlClient.MySqlCommand MySQLComm = new MySql.Data.MySqlClient.MySqlCommand(sSql, MySQLConn);
                            MySql.Data.MySqlClient.MySqlDataReader MyReaderEquipos = MySQLComm.ExecuteReader();

                            IList<string> Equipos = new List<string>();
                            if (MyReaderEquipos.HasRows)
                            {
                                while (MyReaderEquipos.Read())
                                {
                                    if (!MyReaderEquipos.IsDBNull(0))
                                    {
                                        Equipos.Add(MyReaderEquipos.GetString("ip"));
                                    }
                                }
                            }
                            MyReaderEquipos.Close();
                            MyReaderEquipos.Dispose();

                            foreach (string Eq in Equipos)
                            {
                                //Solo calcula los datos a día pasado
                                sSql = "SELECT COUNT(equipo) AS Registros FROM `02diario` WHERE (fecha = '" + DateTime.Now.Date.AddDays(-1.0).AddHours(12.0).ToString("yyyy-MM-dd HH:mm:ss") + "') AND (equipo = '"+ Eq + "')";

                                MySQLComm =
                                new MySql.Data.MySqlClient.MySqlCommand(sSql, MySQLConn);

                                if (MySQLComm.ExecuteScalar().ToString() == "0")
                                {
                                    //Selecciona los datos diarios
                                    //De cada dispositivo

                                    sSql = "SELECT fecha, temperatura, humedad, co, consumo FROM `01input` WHERE (equipo = '" + Eq + "') AND (fecha BETWEEN '" + DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + DateTime.Now.Date.ToString("yyyy-MM-dd HH:mm:ss") + "')";

                                    MySQLComm = new MySql.Data.MySqlClient.MySqlCommand(sSql, MySQLConn);
                                    MySql.Data.MySqlClient.MySqlDataReader  MyReaderDiario = MySQLComm.ExecuteReader();

                                    IList<Lectura> Lecturas = new List<Lectura>();
                                    if (MyReaderDiario.HasRows)
                                    {
                                        while (MyReaderDiario.Read())
                                        {
                                            Lecturas.Add(new Lectura(MyReaderDiario.GetDateTime("fecha"),
                                                                    "",
                                                                    MyReaderDiario.GetDouble("temperatura"),
                                                                    MyReaderDiario.GetDouble("humedad"),
                                                                    MyReaderDiario.GetDouble("co"),
                                                                    MyReaderDiario.GetDouble("consumo")));
                                        }
                                    }
                                    MyReaderDiario.Close();
                                    MyReaderDiario.Dispose();

                                    //Genera el resumen diario
                                    if (Lecturas.Count > 0)
                                    {
                                        Lectura LecturaDiaria = new Lectura(DateTime.Now.Date.AddDays(-1.0).AddHours(12.0),
                                                                            Eq,
                                                                            Lecturas.Select(L => L.Temperatura).Average(),
                                                                            Lecturas.Select(L => L.Humedad).Average(),
                                                                            Lecturas.Select(L => L.CO).Average(),
                                                                            Lecturas.Select(L => L.Consumo).Average());

                                        sSql = string.Format("INSERT" + "\n" +
                                            "INTO {0} ({1}, {3}, {5}, {7}, {9}, {11})" + "\n" +
                                            "VALUES('{2}', '{4}', {6}, {8}, {10}, {12});",
                                                "`02diario`",
                                                "fecha", LecturaDiaria.Fecha.ToString("yyyy-MM-dd HH:mm:ss"),
                                                "equipo", LecturaDiaria.IpEquipo,
                                                "temperatura", LecturaDiaria.Temperatura.ToString("0.00").Replace(',', '.'),
                                                "humedad", LecturaDiaria.Humedad.ToString("0.00").Replace(',', '.'),
                                                "co", LecturaDiaria.CO.ToString("0.0000").Replace(',', '.'),
                                                "consumo", LecturaDiaria.Consumo.ToString("0.00").Replace(',', '.'));

                                        MySQLComm = new MySql.Data.MySqlClient.MySqlCommand(sSql, MySQLConn);
                                        MySQLComm.ExecuteNonQuery();
                                    }
                                }

                                //Solo calcula los datos a mes pasado
                                DateTime mespasado = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                                sSql = "SELECT COUNT(equipo) AS Registros FROM `03mensual` WHERE (fecha = '" + mespasado.AddDays(14.0).AddHours(12.0).ToString("yyyy-MM-dd HH:mm:ss") + "') AND (equipo = '" + Eq + "')";

                                MySQLComm =
                                new MySql.Data.MySqlClient.MySqlCommand(sSql, MySQLConn);

                                if (MySQLComm.ExecuteScalar().ToString() == "0")
                                {
                                    //Selecciona los datos diarios
                                    //De cada dispositivo

                                    sSql = "SELECT fecha, temperatura, humedad, co, consumo FROM `02diario` WHERE (equipo = '" + Eq + "') AND (fecha BETWEEN '" + mespasado.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + mespasado.AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss") + "');";

                                    MySQLComm = new MySql.Data.MySqlClient.MySqlCommand(sSql, MySQLConn);
                                    MySql.Data.MySqlClient.MySqlDataReader MyReaderMensual = MySQLComm.ExecuteReader();

                                    IList<Lectura> Lecturas = new List<Lectura>();
                                    if (MyReaderMensual.HasRows)
                                    {
                                        while (MyReaderMensual.Read())
                                        {
                                            Lecturas.Add(new Lectura(MyReaderMensual.GetDateTime("fecha"),
                                                                    "",
                                                                    MyReaderMensual.GetDouble("temperatura"),
                                                                    MyReaderMensual.GetDouble("humedad"),
                                                                    MyReaderMensual.GetDouble("co"),
                                                                    MyReaderMensual.GetDouble("consumo")));
                                        }
                                    }
                                    MyReaderMensual.Close();
                                    MyReaderMensual.Dispose();

                                    //Genera el resumen diario
                                    if (Lecturas.Count > 0)
                                    {
                                        Lectura LecturaDiaria = new Lectura(mespasado.AddDays(14.0).AddHours(12.0),
                                                                            Eq,
                                                                            Lecturas.Select(L => L.Temperatura).Average(),
                                                                            Lecturas.Select(L => L.Humedad).Average(),
                                                                            Lecturas.Select(L => L.CO).Average(),
                                                                            Lecturas.Select(L => L.Consumo).Average());

                                        sSql = string.Format("INSERT INTO {0} ({1}, {3}, {5}, {7}, {9}, {11})" + "\n" +
                                            "VALUES('{2}', '{4}', {6}, {8}, {10}, {12})",
                                                "`03mensual`",
                                                "fecha", LecturaDiaria.Fecha.ToString("yyyy-MM-dd HH:mm:ss"),
                                                "equipo", LecturaDiaria.IpEquipo,
                                                "temperatura", LecturaDiaria.Temperatura.ToString("0.00").Replace(',', '.'),
                                                "humedad", LecturaDiaria.Humedad.ToString("0.00").Replace(',', '.'),
                                                "co", LecturaDiaria.CO.ToString("0.0000").Replace(',', '.'),
                                                "consumo", LecturaDiaria.Consumo.ToString("0.00").Replace(',', '.'));

                                        MySQLComm = new MySql.Data.MySqlClient.MySqlCommand(sSql, MySQLConn);
                                        MySQLComm.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        catch (MySql.Data.MySqlClient.MySqlException MyEx)
                        {
                            ErrLogger.LogError(MyEx.ToString() + "\n" + sSql, System.Diagnostics.EventLogEntryType.Error);
                        }
                    }
                }
                else
                {
                    ErrLogger.LogError("Sin conexión con SQLServer.", System.Diagnostics.EventLogEntryType.Error);
                }
            }
            catch (System.Data.SqlClient.SqlException SqlEx)
            {
                ErrLogger.LogError(SqlEx.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            catch (Exception Ex)
            {
                ErrLogger.LogError(Ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        
        public IList<string> GetLecturas(string IdEquipo, Periodicidad Periodicidad, DateTime FechaInicio, DateTime FechaFin)
        {
            //Obtiene la lista de las lecturas
            IList<string> Res = new List<string>();

            string TablaPeriodicidad = "";
            switch (Periodicidad)
            {
                case Periodicidad.TiempoReal:
                    TablaPeriodicidad = "`01input`";
                    break;
                case Periodicidad.Diario:
                    TablaPeriodicidad = "`02diario`";
                    break;
                case Periodicidad.Mensual:
                default:
                    TablaPeriodicidad = "`03mensual`";
                    break;
            }

            string sSQL = "SELECT T2.ip, T1.fecha, T1.temperatura, T1.humedad, T1.co, T1.consumo" + "\n" +
                            "FROM " + TablaPeriodicidad + " T1" + " INNER JOIN" + "\n" +
                            "`00equipos` T2 ON T1.equipo = T2.ip" + "\n" +
                            "WHERE (fecha BETWEEN '" + FechaInicio.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + FechaFin.ToString("yyyy-MM-dd HH:mm:ss") + "') AND (T2.id = '" + IdEquipo + "')";

            MySql.Data.MySqlClient.MySqlCommand MyComm =
                new MySql.Data.MySqlClient.MySqlCommand(sSQL, MySQLConn);

            try
            {
                MySql.Data.MySqlClient.MySqlDataReader MyReader = MyComm.ExecuteReader();
                if (MyReader.HasRows)
                {
                    while (MyReader.Read())
                    {
                        Res.Add(new Lectura(MyReader.GetDateTime("fecha"),
                                            MyReader.GetString("ip"),
                                            MyReader.GetDouble("temperatura"),
                                            MyReader.GetDouble("humedad"),
                                            MyReader.GetDouble("co"),
                                            MyReader.GetDouble("consumo")).ToString());
                    }
                }
                MyReader.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException MyEx)
            {
                ErrLogger.LogError(MyEx.ToString(), System.Diagnostics.EventLogEntryType.Error);
                Res = new List<string>();
            }

            return Res;
        }

        public IList<DescripcionEquipo> GetEquipos()
        {
            IList<DescripcionEquipo> Res = new List<DescripcionEquipo>();

            string InsertSQL = string.Format("SELECT id, ip, nombre FROM `00equipos`");

            MySql.Data.MySqlClient.MySqlCommand MySQLComm = 
                new MySql.Data.MySqlClient.MySqlCommand(InsertSQL, MySQLConn);

            try
            {
                MySql.Data.MySqlClient.MySqlDataReader MyReader = MySQLComm.ExecuteReader();
                if (MyReader.HasRows)
                {
                    while (MyReader.Read())
                    {
                        Res.Add(
                            new DescripcionEquipo(
                                MyReader.GetString("id"),
                                MyReader.GetString("ip"),
                                MyReader.IsDBNull(2) ? "Sin Nombre" : MyReader.GetString("nombre")));
                    }
                }
                MyReader.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException MyEx)
            {
                ErrLogger.LogError(MyEx.ToString(), System.Diagnostics.EventLogEntryType.Error);
                Res = new List<DescripcionEquipo>();
            }

            return Res;
        }

        public IList<string> GetCurrent()
        {
            IList<string> Res = new List<string>();

            string sSQL = "SELECT `00current`.fecha, `00equipos`.nombre, `00current`.temperatura, `00current`.humedad, `00current`.co, `00current`.consumo" + "\n" +
                          "FROM `00current` INNER JOIN `00equipos` ON `00current`.equipo = `00equipos`.ip";

            MySql.Data.MySqlClient.MySqlCommand MyComm =
                new MySql.Data.MySqlClient.MySqlCommand(sSQL, MySQLConn);

            try
            {
                MySql.Data.MySqlClient.MySqlDataReader MyReader = MyComm.ExecuteReader();
                if (MyReader.HasRows)
                {
                    while (MyReader.Read())
                    {
                        Res.Add(new Lectura(MyReader.GetDateTime("fecha"),
                                            MyReader.GetString("nombre"),
                                            MyReader.GetDouble("temperatura"),
                                            MyReader.GetDouble("humedad"),
                                            MyReader.GetDouble("co"),
                                            MyReader.GetDouble("consumo")).ToString());
                    }
                }
                MyReader.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException MyEx)
            {
                ErrLogger.LogError(MyEx.ToString(), System.Diagnostics.EventLogEntryType.Error);
                Res = new List<string>();
            }

            return Res;
        }
    }
}
