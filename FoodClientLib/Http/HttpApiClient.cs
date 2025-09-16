using HttpGrpcClientLib.Models;
using Sms.Test;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HttpGrpcClientLib.Http
{
    public sealed class HttpApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        public readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public HttpApiClient(string endPointUrl, string basicAuthUser, string basicAuthPassword, HttpMessageHandler? handler = null)
        {
            _httpClient = handler == null ? new HttpClient() : new HttpClient(handler);
            _httpClient.BaseAddress = new Uri(endPointUrl);
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{basicAuthUser}:{basicAuthPassword}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IReadOnlyList<Dish>> GetMenuAsync()
        {
            var boby = new
            {
                Command = "GetMenu",
                CommandParameters = new { WithPrice = true }
            };
            var requestBody = JsonSerializer.Serialize(boby);
            using var context = new StringContent(requestBody, Encoding.UTF8, "application/json");
            using var resp = await _httpClient.PostAsync(string.Empty, context).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var generic = await JsonSerializer.DeserializeAsync<GenericResponse<MenuData>>(stream, _jsonOptions).ConfigureAwait(false);

            if (generic != null)
                throw new HttpApiException("Empty response");

            if (!generic.Success)
                throw new HttpApiException(generic.ErrorMessage ?? "Server error");

            return generic.Data?.MenuItems ?? new List<Dish>();
        }

        public async Task SendOrderAsync(Models.Order order)
        {
            var body = new
            {
                Command = "SendOrder",
                CommandParameters = new
                {
                    order.Id,
                    MenuItems = order.MenuItems
                }
            };
            var requestBody = JsonSerializer.Serialize(body, _jsonOptions);
            using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            using var resp = await _httpClient.PostAsync(string.Empty, content).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();


            var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var generic = await JsonSerializer.DeserializeAsync<GenericResponse<EmptyData>>(stream, _jsonOptions).ConfigureAwait(false);


            if (generic == null)
                throw new HttpApiException("Empty response");
            if (!generic.Success)
                throw new HttpApiException(generic.ErrorMessage ?? "Server error");
        }

        public void Dispose() => _httpClient.Dispose();
    }

    public sealed class HttpApiException : Exception
    {
        public HttpApiException(string message) : base(message) { }
    }
}