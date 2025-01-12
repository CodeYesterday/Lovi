namespace CodeYesterday.Lovi.Models;

public class StatusBarModel
{
    private readonly List<StatusBarItem> _items = new();

    public IReadOnlyList<StatusBarItem> Items { get; }

    public event EventHandler? ItemsChanged;

    public StatusBarModel()
    {
        Items = _items.AsReadOnly();
    }

    public StatusBarItem? GetItem(string id) => _items.FirstOrDefault(it => it.Id.Equals(id, StringComparison.Ordinal));

    public void RemoveItem(string id)
    {
        var item = GetItem(id);
        if (item != null)
        {
            _items.Remove(item);
            item.OnRemoved();
            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddOrUpdateItem(StatusBarItem item)
    {
        var oldItem = GetItem(item.Id);
        if (!ReferenceEquals(oldItem, item))
        {
            if (oldItem != null)
            {
                _items.Remove(oldItem);
                oldItem.OnRemoved();
            }
            _items.Add(item);
        }

        ItemsChanged?.Invoke(this, EventArgs.Empty);
    }
}
