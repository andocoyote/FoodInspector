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
        private readonly string _path = Environment.CurrentDirectory + @"\EstablishmentsProvider\Establishments.json";

        /// <summary>
        /// Reads the JSON file containing establishment descriptors and creates a list of EstablishmentModel items
        /// </summary>
        /// <returns>A list of EstablishmentModel items</returns>
        public List<EstablishmentsModel> ReadEstablishmentsFile()
        {
            string json = File.ReadAllText(_path);
            List<EstablishmentsModel> establishments = JsonConvert.DeserializeObject<List<EstablishmentsModel>>(json);

            return establishments;
        }
    }
}
