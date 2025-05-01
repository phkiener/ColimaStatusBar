using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ColimaStatusBar.Core.Infrastructure;

public static class Docker
{
    public static async Task<IReadOnlyList<RunningContainer>> StatusAsync(string socketAddress, CancellationToken cancellationToken)
    {
        var uri = new Uri(socketAddress);
        var response = await SendRequestAsync(uri, "/containers/json?all=true", cancellationToken);

        var containers = JsonSerializer.Deserialize<ContainerOutput[]>(response) ?? [];
        return containers.Select(
                static c => new RunningContainer(Id: c.Id, Name: c.Name, Image: c.Image, Enum.Parse<ContainerState>(c.State, ignoreCase: true)))
            .ToArray();
    }

    private static async Task<string> SendRequestAsync(Uri socketAddress, string requestPath, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
        
        var unixDomainSocketEndPoint = new UnixDomainSocketEndPoint(socketAddress.AbsolutePath);
        await socket.ConnectAsync(unixDomainSocketEndPoint, cancellationToken);
        
        var request = Encoding.UTF8.GetBytes($"GET {requestPath} HTTP/1.1\r\nHost: 127.0.0.1\r\nAccept: application/json\r\n\r\n");
        await socket.SendAsync(request, cancellationToken);var receiveBuffer = new byte[1024];
        while (socket.Connected && !cancellationToken.IsCancellationRequested)
        {
            var bytesRead = await socket.ReceiveAsync(receiveBuffer, cancellationToken);
            memoryStream.Write(receiveBuffer, 0, bytesRead);

            if (bytesRead is 0 || socket.Available is 0)
            {
                break;
            }
        }
        
        await socket.DisconnectAsync(true, cancellationToken);
        var responseText = Encoding.UTF8.GetString(memoryStream.ToArray());

        if (!responseText.StartsWith("HTTP/1.1 200 OK"))
        {
            throw new InvalidOperationException($"Socket returned some bad stuff: {responseText}");
        }

        var endOfHeader = responseText.IndexOf("\r\n\r\n", StringComparison.OrdinalIgnoreCase);
        return responseText[endOfHeader..].Trim();
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

        public string Name => GetPrimaryName();
        
        private string GetPrimaryName()
        {
            var firstName = Names.First();
            return firstName.StartsWith('/') ? firstName[1..] : firstName;
        }

    }
}
