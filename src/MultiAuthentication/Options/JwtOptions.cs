namespace MultiAuthentication.Options
{
    public class JwtOptions
    {
        public const string Jwt  =  nameof(Jwt);
        public string Authority { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
