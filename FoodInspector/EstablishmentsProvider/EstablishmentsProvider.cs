using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodInspector.EstablishmentsProvider
{
    public class EstablishmentsProvider : IEstablishmentsProvider
    {
        public List<EstablishmentsModel> ReadEstablishmentsFile()
        {
            string path = Environment.CurrentDirectory + @"\EstablishmentsProvider\Establishments.json";

            string json = File.ReadAllText(path);
            List<EstablishmentsModel> establishments = JsonConvert.DeserializeObject<List<EstablishmentsModel>>(json);

            return establishments;
        }
    }
}
