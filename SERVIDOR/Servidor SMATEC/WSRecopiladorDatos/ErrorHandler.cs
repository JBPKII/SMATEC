/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;*/
using System.Diagnostics;

namespace SMATEC
{
    public class ErrorHandler
    {
        private string sSource;
        private string sLog;

        //Envía los errores y excepciones al log del sistema
        public ErrorHandler(string Source, string TipoLog)
        {
            sSource = Source;
            sLog = TipoLog;
        }

        public void LogError(string Error, EventLogEntryType Tipo)
        {
            try
            {
                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);

                EventLog.WriteEntry(sSource, Error, Tipo);
            }
            catch (System.Exception)
            { }
        }
    }
}