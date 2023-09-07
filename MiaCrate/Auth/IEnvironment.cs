namespace MiaCrate.Auth;

public interface IEnvironment
{
    public string AuthHost { get; }
    public string AccountsHost { get; }
    public string SessionHost { get; }
    public string ServicesHost { get; }
    public string Name { get; }
    
    public string AsString();

    public static IEnvironment 
        Create(string authHost, string accountsHost, string sessionHost, string servicesHost, string name) => 
        new Instance(authHost, accountsHost, sessionHost, servicesHost, name);

    private class Instance : IEnvironment
    {
        public string AuthHost { get; }
        public string AccountsHost { get; }
        public string SessionHost { get; }
        public string ServicesHost { get; }
        public string Name { get; }

        public Instance(string authHost, string accountsHost, string sessionHost, string servicesHost, string name)
        {
            AuthHost = authHost;
            AccountsHost = accountsHost;
            SessionHost = sessionHost;
            ServicesHost = servicesHost;
            Name = name;
        }

        public string AsString() => string.Join(", ", 
            $"authHost={AuthHost}", 
            $"accountsHost={AccountsHost}",
            $"sessionHost={SessionHost}",
            $"servicesHost={ServicesHost}",
            $"name={Name}");
    }
}