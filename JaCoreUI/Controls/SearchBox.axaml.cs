using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;

namespace JaCoreUI.Controls
{
    public class SearchBox : TemplatedControl
    {
        // Styled Properties
        public static readonly StyledProperty<string?> SearchTextProperty =
            AvaloniaProperty.Register<SearchBox, string?>(
                nameof(SearchText),
                defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly StyledProperty<string?> WatermarkProperty =
            AvaloniaProperty.Register<SearchBox, string?>(
                nameof(Watermark),
                defaultValue: "Search...");

        public static readonly StyledProperty<bool> IsSearchingProperty =
            AvaloniaProperty.Register<SearchBox, bool>(
                nameof(IsSearching),
                defaultValue: false);

        public static readonly StyledProperty<int> SearchDelayProperty =
            AvaloniaProperty.Register<SearchBox, int>(
                nameof(SearchDelay),
                defaultValue: 2000); // Default 2 seconds

        public static readonly StyledProperty<IList?> CollectionProperty =
            AvaloniaProperty.Register<SearchBox, IList?>(
                nameof(Collection));

        public static readonly StyledProperty<Func<string, CancellationToken, Task<IEnumerable>>?> SearchFunctionProperty =
            AvaloniaProperty.Register<SearchBox, Func<string, CancellationToken, Task<IEnumerable>>?>(
                nameof(SearchFunction));

        // Event
        public static readonly RoutedEvent<RoutedEventArgs> SearchPerformedEvent =
            RoutedEvent.Register<SearchBox, RoutedEventArgs>(
                nameof(SearchPerformed),
                RoutingStrategies.Bubble);

        // Properties
        public string? SearchText
        {
            get => GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public string? Watermark
        {
            get => GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public bool IsSearching
        {
            get => GetValue(IsSearchingProperty);
            set => SetValue(IsSearchingProperty, value);
        }

        public int SearchDelay
        {
            get => GetValue(SearchDelayProperty);
            set => SetValue(SearchDelayProperty, value);
        }

        public IList? Collection
        {
            get => GetValue(CollectionProperty);
            set => SetValue(CollectionProperty, value);
        }

        public Func<string, CancellationToken, Task<IEnumerable>>? SearchFunction
        {
            get => GetValue(SearchFunctionProperty);
            set => SetValue(SearchFunctionProperty, value);
        }

        // Event handler
        public event EventHandler<RoutedEventArgs> SearchPerformed
        {
            add => AddHandler(SearchPerformedEvent, value);
            remove => RemoveHandler(SearchPerformedEvent, value);
        }

        // Private fields
        private CancellationTokenSource? _searchCts;
        private TextBox? _textBox;
        private Button? _clearButton;
        private DispatcherTimer? _searchTimer;

        public SearchBox()
        {
            _searchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(SearchDelay)
            };
            _searchTimer.Tick += OnSearchTimerTick;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            // Get template parts
            _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
            _clearButton = e.NameScope.Find<Button>("PART_ClearButton");

            // Wire up events
            if (_textBox != null)
            {
                _textBox.KeyDown += TextBox_KeyDown;
                _textBox.PropertyChanged += TextBox_PropertyChanged;
            }

            if (_clearButton != null)
            {
                _clearButton.Click += ClearButton_Click;
            }
        }

        private void TextBox_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == TextBox.TextProperty)
            {
                // Reset the timer each time text changes
                _searchTimer?.Stop();
                
                // If text is not empty, start the timer
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    _searchTimer?.Start();
                }
            }
        }

        private void TextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Dispatcher.UIThread.InvokeAsync(SearchNow);
                e.Handled = true;
            }
        }

        private void ClearButton_Click(object? sender, RoutedEventArgs e)
        {
            SearchText = string.Empty;
            _textBox?.Focus();
        }

        private void OnSearchTimerTick(object? sender, EventArgs e)
        {
            _searchTimer?.Stop();
            Dispatcher.UIThread.InvokeAsync(SearchNow);
        }

        private async Task SearchNow()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return;
                
            await PerformSearch();
        }

        private async Task PerformSearch()
        {
            // If no search function is provided, we can't search
            if (SearchFunction == null)
                return;

            // Cancel any previous search
            _searchCts?.CancelAsync();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                // Show searching indicator
                IsSearching = true;

                // Perform search using provided function
                var results = await SearchFunction(SearchText ?? string.Empty, token);

                // Update bound collection if not cancelled
                if (!token.IsCancellationRequested && Collection != null && results != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Collection.Clear();
                        foreach (var item in results)
                        {
                            Collection.Add(item);
                        }
                    });
                }

                // Raise search performed event
                var args = new RoutedEventArgs(SearchPerformedEvent);
                RaiseEvent(args);
            }
            catch (OperationCanceledException)
            {
                // Search was cancelled, ignore
            }
            catch (Exception ex)
            {
                // Log or handle exception as needed
                System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
            }
            finally
            {
                // Hide searching indicator
                IsSearching = false;
            }
        }
    }
}