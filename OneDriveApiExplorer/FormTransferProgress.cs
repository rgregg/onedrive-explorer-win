using System;
using System.Windows.Forms;

namespace NewApiBrowser
{
    public partial class FormTransferProgress : Form
    {
        public System.Threading.CancellationTokenSource CancelTokenSource { get; set; }

        public FormTransferProgress(string filename, TransferDirection direction)
        {
            InitializeComponent();

            switch (direction)
            {
                case TransferDirection.Upload:
                    labelFilename.Text = string.Format("Uploading \"{0}\"...", filename);
                    labelProgressString.Text = "Starting upload...";
                    break;
                case TransferDirection.Download:
                    labelFilename.Text = string.Format("Downloading \"{0}\"...", filename);
                    labelProgressString.Text = "Starting download...";
                    break;
            }
            
            progressBarPercentComplete.Value = 0;
        }

        public void UpdateProgress(int percentComplete, long bytesTransfered, long totalBytes)
        {
            const double mbconst = 1024.0 * 1024.0;

            Action updateAction = new Action(() =>
                {
                    labelProgressString.Text = string.Format("{0:0.00} of {1:0.00} MB", bytesTransfered / mbconst, totalBytes / mbconst);
                    progressBarPercentComplete.Value = percentComplete;
                });

            if (InvokeRequired)
                BeginInvoke(updateAction);
            else
                updateAction();
        }

        private void FormTransferProgress_Load(object sender, EventArgs e)
        {
            buttonCancel.Visible = (CancelTokenSource != null);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (CancelTokenSource != null)
            {
                CancelTokenSource.Cancel();
            }
        }
    }

    public enum TransferDirection
    {
        Upload,
        Download
    }
}
