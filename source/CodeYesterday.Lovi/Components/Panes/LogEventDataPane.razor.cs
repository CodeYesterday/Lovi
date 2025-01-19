using CodeYesterday.Lovi.Helper;
using CodeYesterday.Lovi.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using Serilog.Events;

namespace CodeYesterday.Lovi.Components.Panes;

public partial class LogEventDataPane
{
    private RadzenDataGrid<ItemModel> _dataGrid = default!;

    [Parameter] public string? CssClass { get; set; }

    [Inject]
    private TooltipService TooltipService { get; set; } = default!;

    private List<ItemModel> Data { get; set; } = [];

    public override async Task OnOpeningAsync(CancellationToken cancellationToken)
    {
        await base.OnOpeningAsync(cancellationToken);

        if (Session is not null)
        {
            Session.SelectedLogItemModelChanged += OnSelectedLogItemModelChanged;
        }
    }

    public override Task OnClosingAsync(CancellationToken cancellationToken)
    {
        if (Session is not null)
        {
            Session.SelectedLogItemModelChanged -= OnSelectedLogItemModelChanged;
        }

        return base.OnClosingAsync(cancellationToken);
    }

    private void OnSelectedLogItemModelChanged(object? sender, ChangedEventArgs<LogItemModel> e)
    {
        Data.Clear();
        var logItem = e.NewValue;
        if (logItem is not null)
        {
            var logLevelSettings = SettingsService.Settings.GetLogLevelSettings(logItem.LogEvent.Level);
            // TODO: The icon does not render like this. Probably have to switch to RenderFragment instead of markup string.
            Data.Add(new() { PropertyName = "Level", ValueMarkup = $"<RadzenIcon Icon=\"{logLevelSettings.Icon}\" IconColor=\"{logLevelSettings.Color}\"/> {logItem.LogEvent.Level}" });
            Data.Add(new() { PropertyName = "Timestamp", ValueMarkup = logItem.LogEvent.Timestamp.ToString(SettingsService.Settings.TimestampFormat) });
            Data.Add(new() { PropertyName = "Message", ValueMarkup = LogEventHtmlRenderer.Render(logItem.LogEvent.MessageTemplate, logItem.LogEvent.Properties, false) });
            Data.Add(new() { PropertyName = "Message template", ValueMarkup = logItem.LogEvent.MessageTemplate.Text });
            Data.Add(new() { PropertyName = "Exception", ValueMarkup = logItem.LogEvent.Exception?.ToString() ?? string.Empty });

            foreach (var property in logItem.LogEvent.Properties)
            {
                AddProperty(Data, property.Key, property.Value);
            }
        }
        _dataGrid.Reload();
    }

    private void AddProperty(IList<ItemModel> list, string name, LogEventPropertyValue value)
    {
        string valueMarkup = LogEventHtmlRenderer.RenderValue(value);
        List<ItemModel>? subItems = null;

        if (value is SequenceValue sequenceValue)
        {
            valueMarkup = $"<span class=\"slm-operator\">[</span><span class=\"slm-numeric-value\">{sequenceValue.Elements.Count}</span><span class=\"slm-operator\">]</span> {valueMarkup}";
            subItems = new();
            for (int n = 0; n < sequenceValue.Elements.Count; n++)
            {
                subItems.Add(new()
                {
                    PropertyName = $"[{n}]",
                    ValueMarkup = LogEventHtmlRenderer.RenderValue(sequenceValue.Elements[n])
                });
            }
        }
        else if (value is StructureValue structureValue)
        {
            valueMarkup = $"<span class=\"slm-property-name\">{structureValue.TypeTag}</span> {valueMarkup}";
            subItems = new();
            foreach (var property in structureValue.Properties)
            {
                AddProperty(subItems, property.Name, property.Value);
            }
        }

        var item = new ItemModel
        {
            PropertyName = name,
            ValueMarkup = valueMarkup,
            SubItems = subItems
        };
        list.Add(item);
    }

    private class ItemModel
    {
        public required string PropertyName { get; init; }

        public required string ValueMarkup { get; init; }

        public RenderFragment RenderedValue => builder =>
        {
            builder.AddMarkupContent(0, ValueMarkup);
        };

        public List<ItemModel>? SubItems { get; set; }
    }

    private void LoadChildData(DataGridLoadChildDataEventArgs<ItemModel> args)
    {
        args.Data = args.Item.SubItems;
    }

    private void RowRender(RowRenderEventArgs<ItemModel> args)
    {
        args.Expandable = args.Data.SubItems is not null;
    }

    private void ShowValueTooltip(ElementReference element, ItemModel item)
    {
        TooltipService.Open(element, _ => item.RenderedValue, SettingsService.Settings.TooltipOptions);
    }
}