namespace CodeYesterday.Lovi.Models;

public class StatusBarTextItem : StatusBarItem
{
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set
        {
            if (string.Equals(value, _text)) return;
            _text = value;
            OnStateHasChanged();
        }
    }
}
