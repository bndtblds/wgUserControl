using System.Drawing;
using System.Windows.Forms;

namespace WgUserControl.UI;

internal sealed class InputDialog : Form
{
    private readonly TextBox textBox = new();

    private InputDialog(string title, string label, string value)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        ClientSize = new Size(420, 120);

        Controls.Add(new Label { Text = label, Left = 12, Top = 12, Width = 390 });
        textBox.Left = 12;
        textBox.Top = 38;
        textBox.Width = 390;
        textBox.Text = value;
        Controls.Add(textBox);

        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 246, Top = 78, Width = 75 };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, Left = 327, Top = 78, Width = 75 };
        Controls.Add(ok);
        Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    public static string? Show(IWin32Window owner, string title, string label, string value)
    {
        using var dialog = new InputDialog(title, label, value);
        return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.textBox.Text.Trim() : null;
    }
}
