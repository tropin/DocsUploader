using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Parcsis.PSD.Publisher.Queue;

namespace Parcsis.PSD.Publisher
{
    public partial class PublishQueueWindow : Form
    {
        public PublishQueueWindow()
        {
            InitializeComponent();
            bsQueue.DataSource = QueueHolder.Instance.Queue;
        }

        private void gvQueue_SelectionChanged(object sender, EventArgs e)
        {
            gvQueue.ClearSelection();
        }
    }
}
