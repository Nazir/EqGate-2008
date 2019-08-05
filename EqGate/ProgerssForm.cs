using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

public class ProgressForm : Form
{
    public bool CanClose = false;
    public bool Stopped = false;
    public Label lbl;

    public ProgressForm()
    {
        this.Width = 300;
        this.Height = 150;
        this.MaximumSize = new System.Drawing.Size(this.Width, this.Height);
        this.MinimumSize = new System.Drawing.Size(this.Width, this.Height);
        this.Text = "Прогресс";
        this.StartPosition = FormStartPosition.CenterParent;
        //this.TopMost = true;
        this.ControlBox = false;
        //this.MinimizeBox = false;
        //this.MaximizeBox = false;
        this.ShowInTaskbar = false;
        this.ShowIcon = false;
        this.SizeGripStyle = SizeGripStyle.Hide;
        //Icon = this.ParentForm.Icon;
        this.Closing += new CancelEventHandler(this.Exit);

        lbl = new Label();
        lbl.Text = this.Text;
        lbl.Left = 24;
        lbl.Top = 8;
        lbl.Dock = DockStyle.Fill;
        lbl.ImageAlign = ContentAlignment.BottomRight;
        this.Controls.Add(lbl);

    }

    private void Exit(Object sender, CancelEventArgs e)
    {
        e.Cancel = true;
        if (!CanClose)
        {
            if (MessageBox.Show("Прекратить выполнение задачи?", "Закрыть",
                  MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                  == DialogResult.Yes)
            {
                Stopped = true;
                DialogResult = DialogResult.Abort;
                e.Cancel = false;
            }
        }
        else
        {
            Stopped = true;
            DialogResult = DialogResult.Abort;
            e.Cancel = false;
        }
    }
}

