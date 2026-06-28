using System;
using Avalonia.Controls;

namespace UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        try
        {
            _ = page_HomePage.Draw();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Top level error, {e.Message}");
        }
    }
}