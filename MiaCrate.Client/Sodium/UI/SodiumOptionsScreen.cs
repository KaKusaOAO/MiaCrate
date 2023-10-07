using MiaCrate.Client.Sodium.UI.Options;
using MiaCrate.Client.Sodium.UI.Options.Controls;
using MiaCrate.Client.UI;
using MiaCrate.Client.UI.Screens;
using MiaCrate.Extensions;
using MiaCrate.Texts;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Client.Sodium.UI;

public class SodiumOptionsScreen : Screen
{
    private readonly List<OptionPage> _pages = new();
    private readonly List<IControlElement> _controls = new();
    private readonly Screen _lastScreen;
    private OptionPage? _currentPage;

    private FlatButtonWidget _applyButton, _closeButton, _undoButton;
    private FlatButtonWidget _donateButton, _hideDonateButton;
    
    private bool _hasPendingChanges;
    private IControlElement? _hoveredElement;

    public IEnumerable<ISodiumOption> AllOptions => _pages.SelectMany(p => p.Options);

    public SodiumOptionsScreen(Screen lastScreen) 
        : base(MiaComponent.Literal("Sodium Options"))
    {
        _lastScreen = lastScreen;
        
        _pages.Add(GameOptionPages.General);
    }

    public void SetPage(OptionPage page)
    {
        _currentPage = page;
        RebuildGui();
    }

    protected override void Init()
    {
        base.Init();
        RebuildGui();
    }

    private void RebuildGui()
    {
        _controls.Clear();
        ClearWidgets();

        if (_currentPage == null)
        {
            if (!_pages.Any())
                throw new InvalidOperationException("No pages are available?!");

            _currentPage = _pages.First();
        }

        RebuildGuiPages();
        RebuildGuiOptions();

        _undoButton = new FlatButtonWidget(new Dim2I(Width - 211, Height - 30, 65, 20),
            MiaComponent.Translatable("sodium.options.button.undo"), UndoChanges);
        _applyButton = new FlatButtonWidget(new Dim2I(Width - 142, Height - 30, 65, 20),
            MiaComponent.Translatable("sodium.options.buttons.apply"), ApplyChanges);
        _closeButton = new FlatButtonWidget(new Dim2I(Width - 73, Height - 30, 65, 20),
            CommonComponents.GuiDone, () => Game!.Screen = _lastScreen);
        _donateButton = new FlatButtonWidget(new Dim2I(Width - 128, 6, 100, 20),
            MiaComponent.Translatable("sodium.options.buttons.donate"), () => { });
        _hideDonateButton = new FlatButtonWidget(new Dim2I(Width - 26, 6, 20, 20),
            MiaComponent.Literal("x"), () => { });

        AddRenderableWidget(_undoButton);
        AddRenderableWidget(_applyButton);
        AddRenderableWidget(_closeButton);
        AddRenderableWidget(_donateButton);
        AddRenderableWidget(_hideDonateButton);
    }

    private void RebuildGuiPages()
    {
        var x = 6;
        var y = 6;
        
        foreach (var page in _pages)
        {
            var width = 12 + Font.Width(page.Name);

            var button = new FlatButtonWidget(new Dim2I(x, y, width, 18), page.Name, () => SetPage(page));
            button.SetSelected(_currentPage == page);

            x += width + 6;
            AddRenderableWidget(button);
        }
    }

    private void RebuildGuiOptions()
    {
        var x = 6;
        var y = 28;
        
        foreach (var group in _currentPage.Groups)
        {
            // Add each option's control element
            foreach (var option in group.Options)
            {
                var control = option.Control;
                var element = control.CreateElement(new Dim2I(x, y, 200, 18));

                AddRenderableWidget(element.AsWidget());
                _controls.Add(element);

                // Move down to the next option
                y += 18;
            }

            // Add padding beneath each option group
            y += 4;
        }
    }

    public override void Render(GuiGraphics graphics, int mouseX, int mouseY, float f)
    {
        UpdateControls();
        base.Render(graphics, mouseX, mouseY, f);
    }

    private void UpdateControls()
    {
        var hovered = _controls
            .Where(e => e.AsWidget().IsHovered)
            .FindFirst()
            .OrElse(_controls
                .Where(e => e.AsWidget().IsFocused)
                .FindFirst()
                .OrElse(null));

        var hasChanges = AllOptions.Any(o => o.HasChanged);

        _applyButton.SetEnabled(hasChanges);
        _undoButton.SetVisible(hasChanges);
        _closeButton.SetEnabled(!hasChanges);
        
        _hasPendingChanges = hasChanges;
        _hoveredElement = hovered;
    }
    
    private void UndoChanges()
    {
        foreach (var option in AllOptions)
        {
            option.Reset();
        }
    }

    private void ApplyChanges()
    {
        var dirtyStorages = new HashSet<IOptionStorage>();
        var flags = OptionFlag.None;
        
        foreach (var option in AllOptions.Where(o => o.HasChanged))
        {
            option.ApplyChanges();
            
            flags |= option.Flags;
            dirtyStorages.Add(option.Storage);
        }

        var game = Game.Instance;

        if (game.Level != null)
        {
            if (flags.HasFlag(OptionFlag.RequiresRendererReload))
            {
                game.LevelRenderer.AllChanged();
            } 
            else if (flags.HasFlag(OptionFlag.RequiresRendererUpdate))
            {
                game.LevelRenderer.AllChanged();
            }
        }

        if (flags.HasFlag(OptionFlag.RequiresAssetReload))
        {
            game.ReloadResourcePacksAsync();
        }

        if (flags.HasFlag(OptionFlag.RequiresGameRestart))
        {
            // ;
        }
        
        
    }
}