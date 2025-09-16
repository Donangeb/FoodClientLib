using Grpc.Net.Client;
using Sms.Test;
using Google.Protobuf.WellKnownTypes;
using HttpGrpcClientLib.Models;


namespace HttpGrpcClientLib.Grpc
{
    public sealed class GrpcApiClient : IDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly SmsTestService.SmsTestServiceClient _client;


        public GrpcApiClient(string address, GrpcChannelOptions? options = null)
        {
            _channel = GrpcChannel.ForAddress(address, options ?? new GrpcChannelOptions());
            _client = new SmsTestService.SmsTestServiceClient(_channel);
        }


        public async Task<IReadOnlyList<Dish>> GetMenuAsync(bool withPrice = true)
        {
            var reply = await _client.GetMenuAsync(new BoolValue { Value = withPrice });
            
            if (!reply.Success)
                throw new GrpcApiException(reply.ErrorMessage ?? "Error");

            return reply.MenuItems.Select(m => new Dish
            {
                Id = m.Id,
                Article = m.Article,
                Name = m.Name,
                Price = m.Price,
                IsWeighted = m.IsWeighted,
                FullPath = m.FullPath,
                Barcodes = m.Barcodes.ToList()
            }).ToList();
        }


        public async Task SendOrderAsync(Models.Order order)
        {
            var req = new Sms.Test.Order { Id = order.Id };
            foreach (var it in order.MenuItems)
            {
                req.OrderItems.Add(new Sms.Test.OrderItem { Id = it.Id, Quantity = it.Quantity });
            }


            var reply = await _client.SendOrderAsync(req);
            if (!reply.Success)
                throw new GrpcApiException(reply.ErrorMessage ?? "Error");
        }


        public void Dispose() => _channel.Dispose();
    }


    public sealed class GrpcApiException : Exception
    {
        public GrpcApiException(string message) : base(message) { }
    }
}