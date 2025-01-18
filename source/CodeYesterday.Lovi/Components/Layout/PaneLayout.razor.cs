using CodeYesterday.Lovi.Input;
using CodeYesterday.Lovi.Models;
using CodeYesterday.Lovi.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using Toolbar = CodeYesterday.Lovi.Input.Toolbar;

namespace CodeYesterday.Lovi.Components.Layout;

public partial class PaneLayout : IPaneLayout
{
    private Toolbar? _panesToolbar;
    private RadzenSplitterPane? _leftPane;
    private RadzenSplitterPane? _topPane;
    private RadzenSplitterPane? _bottomPane;
    private RadzenSplitterPane? _rightPane;
    private RadzenSplitterPane? _centerPaneHorz;
    private RadzenSplitterPane? _centerPaneVert;

    private double LeftPaneWidth { get; set; } = 20d;

    private double TopPaneHeight { get; set; } = 20d;

    private double CenterPaneWidth { get; set; } = 60d;

    private double CenterPaneHeight { get; set; } = 60d;

    [CascadingParameter]
    public ToolbarContainer? ToolbarContainer { get; set; }

    public IUserSettingsService? PaneUserSettings { get; set; }

    public PaneItem? LeftPane { get; set; }
    public PaneItem? RightPane { get; set; }
    public PaneItem? TopPane { get; set; }
    public PaneItem? BottomPane { get; set; }

    public void PanesChanged()
    {
        if (LeftPane is null && RightPane is null && TopPane is null && BottomPane is null)
        {
            if (_panesToolbar is not null)
            {
                ToolbarContainer?.RemoveToolbar(_panesToolbar.Id);
                _panesToolbar = null;
            }

            InvokeAsync(StateHasChanged);
            return;
        }

        if (PaneUserSettings is not null)
        {
            LeftPaneWidth = PaneUserSettings.GetValue("Panes.Left.Width", 20d);
            TopPaneHeight = PaneUserSettings.GetValue("Panes.Top.Height", 20d);
            CenterPaneWidth = PaneUserSettings.GetValue("Panes.Center.Width", 60d);
            CenterPaneHeight = PaneUserSettings.GetValue("Panes.Center.Height", 60d);

            if (LeftPane is not null)
            {
                LeftPane.IsVisible = PaneUserSettings.GetValue("Panes.Left.Visible", LeftPane.IsVisible);
            }
            if (RightPane is not null)
            {
                RightPane.IsVisible = PaneUserSettings.GetValue("Panes.Right.Visible", RightPane.IsVisible);
            }
            if (TopPane is not null)
            {
                TopPane.IsVisible = PaneUserSettings.GetValue("Panes.Top.Visible", TopPane.IsVisible);
            }
            if (BottomPane is not null)
            {
                BottomPane.IsVisible = PaneUserSettings.GetValue("Panes.Bottom.Visible", BottomPane.IsVisible);
            }
        }
        else
        {
            LeftPaneWidth = 20d;
            TopPaneHeight = 20d;
            CenterPaneWidth = 60d;
            CenterPaneHeight = 60d;
        }

        _panesToolbar ??= new()
        {
            Id = "Panes",
            OrderIndex = -10
        };

        void AddPaneCheckBox(PaneItem? pane, string side, string tooltip, string isCheckedTooltip)
        {
            if (pane is not null)
            {
                var checkBox = new ToolbarCheckBox
                {
                    Icon = pane.ToggleViewIcon ?? $"{side}_panel_open",
                    IsCheckedIcon = $"{side}_panel_close",
                    IsChecked = pane.IsVisible,
                    Tooltip = tooltip,
                    IsCheckedTooltip = isCheckedTooltip
                };
                checkBox.IsCheckedChanged += (_, args) => pane.IsVisible = args.NewValue;
                pane.VisibilityChanged += (_, args) => checkBox.IsChecked = args.NewValue;
                _panesToolbar.Items.Add(checkBox);
            }
        }

        AddPaneCheckBox(LeftPane, "left", "Show filter tree", "Hide filter tree");
        AddPaneCheckBox(TopPane, "top", "Show advanced filter", "Hide advanced filter");
        AddPaneCheckBox(RightPane, "right", "Show event structure", "Hide event structure");
        AddPaneCheckBox(BottomPane, "bottom", "Show event details", "Hide event details");

        ToolbarContainer?.AddOrUpdateToolbar(_panesToolbar);

        InvokeAsync(StateHasChanged);
    }

    private void OnCollapse(RadzenSplitterEventArgs args)
    {
        if (ReferenceEquals(args.Pane, _leftPane) && LeftPane is not null)
        {
            LeftPane.IsVisible = false;
            PaneUserSettings?.SetValue("Panes.Left.Visible", false);
        }
        else if (ReferenceEquals(args.Pane, _topPane) && TopPane is not null)
        {
            TopPane.IsVisible = false;
            PaneUserSettings?.SetValue("Panes.Top.Visible", false);
        }
        else if (ReferenceEquals(args.Pane, _rightPane) && RightPane is not null)
        {
            RightPane.IsVisible = false;
            PaneUserSettings?.SetValue("Panes.Right.Visible", false);
        }
        else if (ReferenceEquals(args.Pane, _bottomPane) && BottomPane is not null)
        {
            BottomPane.IsVisible = false;
            PaneUserSettings?.SetValue("Panes.Bottom.Visible", false);
        }
    }

    private void OnExpand(RadzenSplitterEventArgs args)
    {
        if (ReferenceEquals(args.Pane, _leftPane) && LeftPane is not null)
        {
            LeftPane.IsVisible = true;
            PaneUserSettings?.SetValue("Panes.Left.Visible", true);
        }
        else if (ReferenceEquals(args.Pane, _topPane) && TopPane is not null)
        {
            TopPane.IsVisible = true;
            PaneUserSettings?.SetValue("Panes.Top.Visible", true);
        }
        else if (ReferenceEquals(args.Pane, _rightPane) && RightPane is not null)
        {
            RightPane.IsVisible = true;
            PaneUserSettings?.SetValue("Panes.Right.Visible", true);
        }
        else if (ReferenceEquals(args.Pane, _bottomPane) && BottomPane is not null)
        {
            BottomPane.IsVisible = true;
            PaneUserSettings?.SetValue("Panes.Bottom.Visible", true);
        }
    }

    private void OnPaneResize(RadzenSplitterResizeEventArgs args)
    {
        if (ReferenceEquals(args.Pane, _leftPane) && LeftPane is not null)
        {
            PaneUserSettings?.SetValue("Panes.Left.Width", args.NewSize);
        }
        else if (ReferenceEquals(args.Pane, _topPane) && TopPane is not null)
        {
            PaneUserSettings?.SetValue("Panes.Top.Height", args.NewSize);
        }
        else if (ReferenceEquals(args.Pane, _centerPaneHorz) && RightPane is not null)
        {
            PaneUserSettings?.SetValue("Panes.Center.Width", args.NewSize);
        }
        else if (ReferenceEquals(args.Pane, _centerPaneVert) && BottomPane is not null)
        {
            PaneUserSettings?.SetValue("Panes.Center.Height", args.NewSize);
        }
    }
}