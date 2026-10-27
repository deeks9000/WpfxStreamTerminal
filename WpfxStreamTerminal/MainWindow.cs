using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using WpfxStreamTerminal.Converters;
using WpfxStreamTerminal.Services;
using WpfxStreamTerminal.ViewModels;

namespace WpfxStreamTerminal;

public class MainWindow : Window
{
    private MainViewModel vm;
    private bool _isFirstLine = true;
    private TextBox? _txtConsoleOut;
    private ContentControl? _slot;
    private UIElement? tcpLayout;
    private UIElement? serialLayout;

    public MainWindow(MainViewModel mainViewModel)
    {
        vm = mainViewModel;
        DataContext = vm;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Width = 800;
        Height = 500;
        MinWidth = 640;
        MinHeight = 320;
        Content = Build();

        SetBinding(Window.TitleProperty, BindingX(b => {
            b.Path = PropertyPathX(nameof(vm.ConnectionStatusText));
            b.StringFormat = "Stream Terminal - {0}";
        }));

        vm.PropertyChanged += ViewModel_PropertyChanged;

        vm.Protocol = ProtocolType.Serial;
    }

    private UIElement Build()
    {
        return GridX(
            configure: x => {
                x.AddRow(GridUnitType.Pixel, 34);
                x.AddRow();
                x.AddRow(GridUnitType.Pixel, 34);
            },
            children: [
                GridX(
                    configure: x => {
                        Grid.SetRow(x, 0); // ROW0: Top controls
                        x.AddColumn();
                        x.AddColumn(GridUnitType.Pixel, 100);
                        x.AddColumn(GridUnitType.Pixel, 80);
                        x.AddColumn(GridUnitType.Pixel, 100);
                        x.Background = BrushX(242, 242, 242);
                    },
                    children: [
                        TextBoxX(
                            configure: x => {
                                Grid.SetColumn(x, 0);
                                x.Margin = ThicknessX(5,5,0,5);
                                x.FontFamily = FontFamilyX("Consolas");
                                x.VerticalContentAlignment = VerticalAlignment.Center;
                                x.SetBinding(TextBox.TextProperty, BindingX(b => {
                                    b.Path = PropertyPathX(nameof(vm.Line));
                                    b.Mode = BindingMode.TwoWay;
                                    b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                }));

                                x.InputBindings.Add(KeyBindingX(kb => {
                                    kb.Key = Key.Enter;
                                    kb.Command = vm.SendLineCommand;
                                }));
                            }
                        ),
                        ComboBoxX(
                            configure: x => {
                                Grid.SetColumn(x, 1);
                                x.Margin = ThicknessX(5,5,0,5);
                                x.ItemsSource = Enum.GetValues<ProtocolType>();
                                x.SetBinding(ComboBox.IsEnabledProperty, BindingX(b => {
                                    b.Path = PropertyPathX(nameof(vm.IsConnected));
                                    b.Mode = BindingMode.OneWay;
                                    b.Converter = InverseBooleanConverter.Instance;
                                }));
                                x.SetBinding(ComboBox.SelectedItemProperty, BindingX(b => {
                                    b.Path = PropertyPathX(nameof(vm.Protocol));
                                    b.Mode = BindingMode.TwoWay;
                                }));
                            }
                        ),
                        CheckBoxX(
                            configure: x => {
                                Grid.SetColumn(x, 2);
                                x.Content = "Keep Text";
                                x.Margin = ThicknessX(5, 5, 0, 5);
                                x.VerticalContentAlignment = VerticalAlignment.Center;
                                x.SetBinding(CheckBox.IsCheckedProperty, BindingX(b => {
                                    b.Path = PropertyPathX(nameof(vm.KeepText));
                                    b.Mode = BindingMode.TwoWay;
                                    b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                }));
                            }
                        ),
                        ButtonX(
                            configure: x => {
                                Grid.SetColumn(x, 3);
                                x.Content = "Send";
                                x.Margin = ThicknessX(5);
                                x.Padding = ThicknessX(10, 0, 10, 0);
                                x.SetBinding(Button.CommandProperty, BindingX(nameof(vm.SendLineCommand)));
                            }
                        )
                    ]
                ),
                BorderX(
                    configure: x => {
                        Grid.SetRow(x, 1); // ROW1: Console output
                        x.BorderThickness = ThicknessX(0, 1, 0, 1);
                        x.BorderBrush = BrushX(150, 150, 150);
                    },
                    child: TextBoxX(
                        configure: x => {
                            _txtConsoleOut = x;
                            x.Padding = ThicknessX(5);
                            x.BorderThickness = ThicknessX(0);
                            x.IsReadOnly = true;
                            x.IsReadOnlyCaretVisible = true;
                            x.Background = BrushX(240, 240, 255);
                            x.FontFamily = FontFamilyX("Consolas");
                            x.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                        }
                    )
                ),
                ContentControlX(
                    configure: x => {
                        Grid.SetRow(x, 2); // ROW2: Protocol controls
                        _slot = x;
                    }
                )
            ]
        );
    }

    private UIElement BuildTcpLayout()
    {
        return GridX(
            configure: x => {
                x.AddColumn();
                x.AddColumn(GridUnitType.Auto);
                x.AddColumn(GridUnitType.Pixel, 100);
                x.Background = BrushX(242, 242, 242);
            },
            children: [
                StackPanelX(
                    configure: x => {
                        Grid.SetColumn(x, 0);
                        x.Orientation = Orientation.Horizontal;
                    },
                    children: [
                        LabelX(
                            configure: x => {
                                x.Content = "Host:";
                                x.VerticalContentAlignment = VerticalAlignment.Center;
                                x.Margin = ThicknessX(5, 0, 0, 0);
                                x.Padding = ThicknessX(0);
                            }
                        ),
                        TextBoxX(
                            configure: x => {
                                x.Margin = ThicknessX(5, 5, 0, 5);
                                x.FontFamily = FontFamilyX("Consolas");
                                x.VerticalContentAlignment = VerticalAlignment.Center;
                                x.Width = 80;
                                x.CharacterCasing = CharacterCasing.Upper;
                                x.SetBinding(TextBox.TextProperty, BindingX(nameof(vm.Host)));
                                x.SetBinding(TextBox.IsEnabledProperty, BindingX(b => {
                                    b.Path = PropertyPathX(nameof(vm.IsConnected));
                                    b.Mode = BindingMode.OneWay;
                                    b.Converter = InverseBooleanConverter.Instance;
                                }));
                            }
                        ),
                        LabelX(
                            configure: x => {
                                x.Content = "Port:";
                                x.VerticalContentAlignment = VerticalAlignment.Center;
                                x.Margin = ThicknessX(5, 0, 0, 0);
                                x.Padding = ThicknessX(0);
                            }
                        ),
                        TextBoxX(
                            configure: x => {
                                x.Margin = ThicknessX(5, 5, 0, 5);
                                x.FontFamily = FontFamilyX("Consolas");
                                x.VerticalContentAlignment = VerticalAlignment.Center;
                                x.Width = 80;
                                x.CharacterCasing = CharacterCasing.Upper;
                                x.SetBinding(TextBox.TextProperty, BindingX(nameof(vm.Port)));
                                x.SetBinding(TextBox.IsEnabledProperty, BindingX(b => {
                                    b.Path = PropertyPathX(nameof(vm.IsConnected));
                                    b.Mode = BindingMode.OneWay;
                                    b.Converter = InverseBooleanConverter.Instance;
                                }));

                                x.PreviewTextInput += (s, e) => {
                                    e.Handled = e.Text.Any(c => c < '0' || c > '9');
                                };
                            }
                        ),
                        ButtonX(
                            configure: x => {
                                x.Content = "Start";
                                x.Margin = ThicknessX(5, 5, 0, 5);
                                x.Padding = ThicknessX(10, 0, 10, 0);
                                x.SetBinding(Button.CommandProperty, BindingX(nameof(vm.StartStreamCommand)));
                                x.SetBinding(Button.IsEnabledProperty, BindingX(b => {
                                    b.Path = PropertyPathX(nameof(vm.IsConnected));
                                    b.Mode = BindingMode.OneWay;
                                    b.Converter = InverseBooleanConverter.Instance;
                                }));
                            }
                        ),
                        ButtonX(
                            configure: x => {
                                x.Content = "Stop";
                                x.Margin = ThicknessX(5, 5, 0, 5);
                                x.Padding = ThicknessX(10, 0, 10, 0);
                                x.SetBinding(Button.CommandProperty, BindingX(nameof(vm.StopStreamCommand)));
                            }
                        )
                    ]
                ),
                CheckBoxX(
                    configure: x => {
                        Grid.SetColumn(x, 1);
                        x.Content = "Auto Scroll";
                        x.Margin = ThicknessX(5, 5, 0, 5);
                        x.VerticalContentAlignment = VerticalAlignment.Center;
                        x.SetBinding(CheckBox.IsCheckedProperty, BindingX(b => {
                            b.Path = PropertyPathX(nameof(vm.AutoScroll));
                            b.Mode = BindingMode.TwoWay;
                            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        }));
                    }
                ),
                ButtonX(
                    configure: x => {
                        Grid.SetColumn(x, 2);
                        x.Content = "Clear";
                        x.Margin = ThicknessX(5);
                        x.Padding = ThicknessX(10, 0, 10, 0);

                        x.Click += (s,e) => {
                            _txtConsoleOut?.Clear();
                            _isFirstLine = true;
                        };
                    }
                )
            ]
        );
    }

    private UIElement BuildSerialLayout()
    {
        return GridX(
            configure: x => {
                x.AddColumn();
                x.AddColumn(GridUnitType.Auto);
                x.AddColumn(GridUnitType.Pixel, 100);
                x.Background = BrushX(242, 242, 242);
            },
            children: [
                StackPanelX(
                    configure: x => {
                        Grid.SetColumn(x, 0);
                        x.Orientation = Orientation.Horizontal;
                    },
                    children: [
                        LabelX(
                            configure: x => {
                                x.Content = "Serial Port:";
                                x.VerticalContentAlignment = VerticalAlignment.Center;
                                x.Margin = ThicknessX(5, 0, 0, 0);
                                x.Padding = ThicknessX(0);
                            }
                        ),
                        TextBoxX(
                            configure: x => {
                                x.Margin = ThicknessX(5, 5, 0, 5);
                                x.FontFamily = FontFamilyX("Consolas");
                                x.VerticalContentAlignment = VerticalAlignment.Center;
                                x.Width = 60;
                                x.CharacterCasing = CharacterCasing.Upper;        
                                x.SetBinding(TextBox.TextProperty, BindingX(nameof(vm.SerialPort)));
                                x.SetBinding(TextBox.IsEnabledProperty, BindingX(b => {
                                    b.Path = PropertyPathX(nameof(vm.IsConnected));
                                    b.Mode = BindingMode.OneWay;
                                    b.Converter = InverseBooleanConverter.Instance;
                                }));
                            }
                        ),
                        LabelX(
                            configure: x => {
                                x.Content = "Baud Rate:";
                                x.VerticalContentAlignment = VerticalAlignment.Center;
                                x.Margin = ThicknessX(5, 0, 0, 0);
                                x.Padding = ThicknessX(0);
                            }
                        ),
                        TextBoxX(
                            configure: x => {
                                x.Margin = ThicknessX(5, 5, 0, 5);
                                x.FontFamily = FontFamilyX("Consolas");
                                x.VerticalContentAlignment = VerticalAlignment.Center;
                                x.Width = 60;
                                x.CharacterCasing = CharacterCasing.Upper;
                                x.SetBinding(TextBox.TextProperty, BindingX(nameof(vm.BaudRate)));
                                x.SetBinding(TextBox.IsEnabledProperty, BindingX(b => {
                                    b.Path = PropertyPathX(nameof(vm.IsConnected));
                                    b.Mode = BindingMode.OneWay;
                                    b.Converter = InverseBooleanConverter.Instance;
                                }));

                                x.PreviewTextInput += (s, e) => {
                                    e.Handled = e.Text.Any(c => c < '0' || c > '9');
                                };
                            }
                        ),
                        ButtonX(
                            configure: x => {
                                x.Content = "Start";
                                x.Margin = ThicknessX(5, 5, 0, 5);
                                x.Padding = ThicknessX(10, 0, 10, 0);
                                x.SetBinding(Button.CommandProperty, BindingX(nameof(vm.StartStreamCommand)));
                                x.SetBinding(Button.IsEnabledProperty, BindingX(b => {
                                    b.Path = PropertyPathX(nameof(vm.IsConnected));
                                    b.Mode = BindingMode.OneWay;
                                    b.Converter = InverseBooleanConverter.Instance;
                                }));
                            }
                        ),
                        ButtonX(
                            configure: x => {
                                x.Content = "Stop";
                                x.Margin = ThicknessX(5, 5, 0, 5);
                                x.Padding = ThicknessX(10, 0, 10, 0);
                                x.SetBinding(Button.CommandProperty, BindingX(nameof(vm.StopStreamCommand)));
                            }
                        ),
                        CheckBoxX(
                            configure: x => {
                                x.Content = "DTR Enable";
                                x.Margin = ThicknessX(5, 5, 0, 5);
                                x.VerticalContentAlignment = VerticalAlignment.Center;
                                x.SetBinding(CheckBox.IsCheckedProperty, BindingX(b => {
                                    b.Path = PropertyPathX(nameof(vm.DtrEnable));
                                    b.Mode = BindingMode.TwoWay;
                                    b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                                }));
                                x.SetBinding(CheckBox.IsEnabledProperty, BindingX(b => {
                                    b.Path = PropertyPathX(nameof(vm.IsConnected));
                                    b.Mode = BindingMode.OneWay;
                                    b.Converter = InverseBooleanConverter.Instance;
                                }));
                            }
                        ),
                    ]
                ),
                CheckBoxX(
                    configure: x => {
                        Grid.SetColumn(x, 1);
                        x.Content = "Auto Scroll";
                        x.Margin = ThicknessX(5, 5, 0, 5);
                        x.VerticalContentAlignment = VerticalAlignment.Center;
                        x.SetBinding(CheckBox.IsCheckedProperty, BindingX(b => {
                            b.Path = PropertyPathX(nameof(vm.AutoScroll));
                            b.Mode = BindingMode.TwoWay;
                            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        }));
                    }
                ),
                ButtonX(
                    configure: x => {
                        Grid.SetColumn(x, 2);
                        x.Content = "Clear";
                        x.Margin = ThicknessX(5, 5, 5, 5);
                        x.Padding = ThicknessX(10, 0, 10, 0);

                        x.Click += (s,e) => {
                            _txtConsoleOut?.Clear();
                            _isFirstLine = true;
                        };
                    }
                )
            ]
        );
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(vm.Protocol):
                ProtocolType protocol = vm.Protocol;
                Dispatcher.InvokeAsync(() => UpdateUI_ProtocolLayout(protocol));
                break;

            case nameof(vm.LineSent):
                string lineSent = vm.LineSent;
                Dispatcher.InvokeAsync(() => UpdateUI_ConsoleOut(lineSent));
                break;

            case nameof(vm.LineReceived):
                string lineRcvd = vm.LineReceived;
                Dispatcher.InvokeAsync(() => UpdateUI_ConsoleOut(lineRcvd));
                break;
        }
    }

    //--------------------------------------------------------------------------------------------------------
    // UI Updates

    private void UpdateUI_ConsoleOut(string text)
    {
        if (_isFirstLine)
            _isFirstLine = false;
        else
            _txtConsoleOut?.AppendText("\r\n");

        _txtConsoleOut?.AppendText(text);

        if (vm.AutoScroll)
        {
            _txtConsoleOut?.ScrollToEnd();
        }
    }

    private void UpdateUI_ProtocolLayout(ProtocolType protocol)
    {
        switch (protocol)
        {
            case ProtocolType.Serial:
                _slot?.Content = serialLayout ??= BuildSerialLayout();
                break;

            case ProtocolType.Tcp:
                _slot?.Content = tcpLayout ??= BuildTcpLayout();
                break;

            default:
                _slot?.Content = serialLayout ??= BuildSerialLayout();
                break;
        }
    }
}
