namespace MiaCrate.Texts;

public class ClickEvent
{
    public ActionType Action { get; }
    public string Value { get; }

    public ClickEvent(ActionType action, string value)
    {
        Action = action;
        Value = value;
    }
    
    public sealed class ActionType : IEnumLike<ActionType>
    {
        private static readonly Dictionary<int, ActionType> _values = new();

        public static ActionType OpenUrl { get; } = new("open_url", true);
        public static ActionType OpenFile { get; } = new("open_file", false);
        public static ActionType RunCommand { get; } = new("run_command", true);
        public static ActionType SuggestCommand { get; } = new("suggest_command", true);
        public static ActionType ChangePage { get; } = new("change_page", true);
        public static ActionType CopyToClipboard { get; } = new("copy_to_clipboard", true);
        
        private readonly string _name;
        private readonly bool _allowFromServer;

        public int Ordinal { get; }

        public static ActionType[] Values => _values.Values.ToArray();
        
        private ActionType(string name, bool allowFromServer)
        {
            _name = name;
            _allowFromServer = allowFromServer;

            Ordinal = _values.Count;
            _values[Ordinal] = this;
        }
    }
}