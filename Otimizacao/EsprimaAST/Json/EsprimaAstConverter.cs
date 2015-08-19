using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otimizacao.EsprimaAST.Nodes;

namespace Otimizacao.EsprimaAST.Json
{
    /// <summary>
    /// Esprima AST Json Converter
    /// </summary>
    public class EsprimaAstConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Recebe o Json para criação dos Objetos
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            
            var fabrica = new Fabrica();
            
            var computedTree = fabrica.Construir(jsonObject);

            return computedTree;
        }

        /// <summary>
        /// Determina se é possível tratar. Levando em conta que vou controlar isso, retorna sempre verdadeiro
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
