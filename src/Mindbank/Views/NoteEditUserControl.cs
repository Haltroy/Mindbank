using Avalonia;
using Mindbank.Backend;

namespace Mindbank.Views;

public class NoteEditUserControl : NUC
{
    public static readonly StyledProperty<Bank> BankProperty =
        AvaloniaProperty.Register<NoteEditUserControl, Bank>(nameof(Bank), Bank.GenerateExampleBank());

    public Bank Bank
    {
        get => GetValue(BankProperty);
        set => SetValue(BankProperty, value);
    }
}