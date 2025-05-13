using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json.Serialization;

namespace ColimaStatusBar.Core.Infrastructure;

public static class Docker
{
    public static async Task<IReadOnlyList<RunningContainer>> StatusAsync(string socketAddress, CancellationToken cancellationToken)
    {
        var client = GetSocketClient(socketAddress);
        var containers = await client.GetFromJsonAsync<ContainerOutput[]>("/containers/json?all=true", cancellationToken) ?? [];
        
        return containers.Select(static c => c.AsRunningContainer()).ToArray();
    }

    public static async Task StartAsync(string socketAddress, string id, CancellationToken cancellationToken)
    {
        var client = GetSocketClient(socketAddress);
        await client.PostAsync($"/containers/{id}/start", null, cancellationToken);
    }

    public static async Task StopAsync(string socketAddress, string id, CancellationToken cancellationToken)
    {
        var client = GetSocketClient(socketAddress);
        await client.PostAsync($"/containers/{id}/stop", null, cancellationToken);
    }

    public static async Task RemoveAsync(string socketAddress, string id, CancellationToken cancellationToken)
    {
        var client = GetSocketClient(socketAddress);
        await client.DeleteAsync($"/containers/{id}", cancellationToken);
    }

    private static HttpClient GetSocketClient(string socketAddress)
    {
        var handler = new SocketsHttpHandler
        {
            ConnectCallback = async (_, cancellation) =>
            {
                var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                var endpoint = new UnixDomainSocketEndPoint(socketAddress);

                await socket.ConnectAsync(endpoint, cancellation);
                return new NetworkStream(socket, ownsSocket: true);
            }
        };

        return new HttpClient(handler, disposeHandler: true) { BaseAddress = new Uri("http://localhost") };
    }

    private sealed class ContainerOutput
    {
        [JsonRequired]
        [JsonPropertyName("Id")]
        public required string Id { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("Names")]
        public required string[] Names { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("Image")]
        public required string Image { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("State")]
        public required string State { get; set; }

        public RunningContainer AsRunningContainer()
        {
            return new RunningContainer(
                Id: Id,
                Name: GetPrimaryName(),
                Image: Image,
                State: Enum.Parse<ContainerState>(State, ignoreCase: true));
        }
        
        private string GetPrimaryName()
        {
            var firstName = Names.First();
            return firstName.StartsWith('/') ? firstName[1..] : firstName;
        }

    }
}
