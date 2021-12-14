namespace MultiAuthentication.Options
{
    public class JwtOptions
    {
        public const string Jwt  =  nameof(Jwt);
        public string Authority { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;

        public string Client { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public string AuthServerUrl { get; set; } = string.Empty;
        public string Realm { get; set; } = string.Empty;
    }
}
