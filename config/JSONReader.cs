using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MyTaskManagerBot.config
{
    internal class JSONReader
    {
        public string Token { get; set; } //Creating a property for the token
        public string Prefix { get; set; } //Creating a property for the prefix
        public async Task ReadJSON() //This method reads the JSON file asynchronously
        {
            using (StreamReader reader = new StreamReader("config.json")) //Reading the JSON file from the /bin/Debug folder
            {
                string JSON = await reader.ReadToEndAsync();
                JSONStruct data = JsonConvert.DeserializeObject<JSONStruct>(JSON); //Deserializing the JSON data into a JSONStruct object
                this.Token = data.Token; //Assigning the token from the JSON data to the Token property
                this.Prefix = data.Prefix; //Assigning the prefix from the JSON data to the Prefix property

            }
        }
    
    }

    
    internal sealed class JSONStruct
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
    }
}
