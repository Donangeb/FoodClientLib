using Sms.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HttpGrpcClientLib.Models
{
    internal class GenericResponse<T>
    {
        [JsonPropertyName("Command")] public string? Command { get; set; }
        [JsonPropertyName("Success")] public bool Success { get; set; }
        [JsonPropertyName("ErrorMessage")] public string? ErrorMessage { get; set; }
        [JsonPropertyName("Data")] public T? Data { get; set; }
    }


    internal class MenuData
    {
        [JsonPropertyName("MenuItems")]
        public List<Dish>? MenuItems { get; set; }
    }


    internal class EmptyData { }
}