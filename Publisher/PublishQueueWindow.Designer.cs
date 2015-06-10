namespace Parcsis.PSD.Publisher
{
    partial class PublishQueueWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gvQueue = new System.Windows.Forms.DataGridView();
            this.fileFullPathDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fileExtensionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bsQueue = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.gvQueue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsQueue)).BeginInit();
            this.SuspendLayout();
            // 
            // gvQueue
            // 
            this.gvQueue.AllowUserToAddRows = false;
            this.gvQueue.AllowUserToDeleteRows = false;
            this.gvQueue.AllowUserToOrderColumns = true;
            this.gvQueue.AutoGenerateColumns = false;
            this.gvQueue.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gvQueue.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.gvQueue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvQueue.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fileFullPathDataGridViewTextBoxColumn,
            this.fileNameDataGridViewTextBoxColumn,
            this.fileExtensionDataGridViewTextBoxColumn,
            this.statusDataGridViewTextBoxColumn});
            this.gvQueue.DataSource = this.bsQueue;
            this.gvQueue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvQueue.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.gvQueue.Location = new System.Drawing.Point(0, 0);
            this.gvQueue.MultiSelect = false;
            this.gvQueue.Name = "gvQueue";
            this.gvQueue.ReadOnly = true;
            this.gvQueue.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.gvQueue.RowHeadersVisible = false;
            this.gvQueue.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.ColumnHeaderSelect;
            this.gvQueue.ShowCellErrors = false;
            this.gvQueue.ShowCellToolTips = false;
            this.gvQueue.ShowEditingIcon = false;
            this.gvQueue.ShowRowErrors = false;
            this.gvQueue.Size = new System.Drawing.Size(542, 376);
            this.gvQueue.TabIndex = 0;
            this.gvQueue.VirtualMode = true;
            this.gvQueue.SelectionChanged += new System.EventHandler(this.gvQueue_SelectionChanged);
            // 
            // fileFullPathDataGridViewTextBoxColumn
            // 
            this.fileFullPathDataGridViewTextBoxColumn.DataPropertyName = "FileFullPath";
            this.fileFullPathDataGridViewTextBoxColumn.HeaderText = "Полный путь до файла";
            this.fileFullPathDataGridViewTextBoxColumn.Name = "fileFullPathDataGridViewTextBoxColumn";
            this.fileFullPathDataGridViewTextBoxColumn.ReadOnly = true;
            this.fileFullPathDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // fileNameDataGridViewTextBoxColumn
            // 
            this.fileNameDataGridViewTextBoxColumn.DataPropertyName = "FileName";
            this.fileNameDataGridViewTextBoxColumn.HeaderText = "Имя файла";
            this.fileNameDataGridViewTextBoxColumn.Name = "fileNameDataGridViewTextBoxColumn";
            this.fileNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.fileNameDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // fileExtensionDataGridViewTextBoxColumn
            // 
            this.fileExtensionDataGridViewTextBoxColumn.DataPropertyName = "FileExtension";
            this.fileExtensionDataGridViewTextBoxColumn.HeaderText = "Расширение";
            this.fileExtensionDataGridViewTextBoxColumn.Name = "fileExtensionDataGridViewTextBoxColumn";
            this.fileExtensionDataGridViewTextBoxColumn.ReadOnly = true;
            this.fileExtensionDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // statusDataGridViewTextBoxColumn
            // 
            this.statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
            this.statusDataGridViewTextBoxColumn.HeaderText = "Статус";
            this.statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
            this.statusDataGridViewTextBoxColumn.ReadOnly = true;
            this.statusDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // bsQueue
            // 
            this.bsQueue.AllowNew = false;
            this.bsQueue.DataSource = typeof(Parcsis.PSD.Publisher.Queue.QueueItem);
            // 
            // PublishQueueWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 376);
            this.Controls.Add(this.gvQueue);
            this.Name = "PublishQueueWindow";
            this.Text = "Очередь публикации";
            ((System.ComponentModel.ISupportInitialize)(this.gvQueue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsQueue)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView gvQueue;
        private System.Windows.Forms.BindingSource bsQueue;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileFullPathDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn fileExtensionDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
    }
}