using System;
using NLib.Config;

namespace RMDS.ApiClient
{
    public interface IApiConfig : IConfig
    {
        string GrpcHost { get; }
        int GrpcPort { get; }

        bool GrpcUseSSL { get; }

        TimeSpan GrpcReconnectTimeout { get; }
    }

    public class ApiConfig : TypedConfig, IApiConfig
    {
        public ApiConfig()
        {
        }

        public ApiConfig(IConfig config) : base(config)
        {
        }

        public string GrpcHost { get; set; }
        public int GrpcPort { get; set; }

        [OptionalParameter] public bool GrpcUseSSL { get; set; }

        [OptionalParameter] public TimeSpan GrpcReconnectTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }

    public class GrpcVerbosity
    {
        public const string Debug = "DEBUG";
        public const string Info = "INFO";
        public const string Error = "ERROR";
    }
}