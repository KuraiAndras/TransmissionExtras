using Transmission.API.RPC;

namespace TransmissionExtras;

public static class TransmissionClientFactory
{
    public static Client GetClient(TransmissionOptions options)
    {
        var url = options.Url.EndsWith("/transmission/rpc")
            ? options.Url
            : $"{options.Url.TrimEnd('/')}/transmission/rpc";

        return options.User is not null
            ? new Client(url, login: options.User, password: options.Password)
            : new Client(url);
    }
}
