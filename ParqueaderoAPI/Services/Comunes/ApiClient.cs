using System.Text.Json;
using System.Text;
using EmailModel.Comunes;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ParqueaderoAPI.Services.Comunes
{
    public class ApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Msj> PostAsync(string url, object data)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                var responseData = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<Msj>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                }
            
                var validacion = JsonSerializer.Deserialize<ValidationErrorResponse>(responseData);
                string error = string.Join("; ", validacion.Errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}"));
                return new Msj() { Mensaje = $"No fue posible procesar la solicitud externa en este momento. {error}" };
            }
            catch
            {
                return new Msj() { Mensaje = $"Ocurio un error al procesar la solicitud" };
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class ValidationErrorResponse
    {
        [JsonPropertyName("errors")]
        public Dictionary<string, string[]> Errors { get; set; }
    }
}
