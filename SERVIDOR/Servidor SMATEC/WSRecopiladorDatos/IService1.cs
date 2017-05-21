using System;
using System.Collections.Generic;
/*using System.Linq;
using System.Runtime.Serialization;*/
using System.ServiceModel;
using System.ServiceModel.Web;
/*using System.Text;*/

namespace SMATEC.WSRecopiladorDatos
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IService1" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface IRecopiladorDatos
    {
        [OperationContract]
        string GetServiceDescriptionInfo();

        [OperationContract(IsOneWay = false)]
        IList<string> GetLecturas(string IdEquipo, Periodicidad Periodicidad, DateTime FechaInicio, DateTime FechaFin);

        [OperationContract]
        IList<DescripcionEquipo> GetEquipos();

        [OperationContract]
        IList<string> GetCurrent();
    }

    [ServiceContract]
    public interface IRecopiladorDatosRest
    {
        //[OperationContract]
        [WebGet]
        bool SetData(string CadenaLectura);
        //llamar con http://localhost:81/Service1.svc/rest/SetData?CadenaLectura=fecha|ip|T@56.8|H@32.7|C@0.256|W@5896.23
    }

    [ServiceContract]
    public interface IRecopiladorDatosTeselador
    {
        //[OperationContract]
        [WebGet]
        void TeselarData();
        //llamar con http://localhost:81/Service1.svc/teselar/TeselarData?
    }

    // Utilice un contrato de datos, como se ilustra en el ejemplo siguiente, para agregar tipos compuestos a las operaciones de servicio.
    /*[DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }*/
}
