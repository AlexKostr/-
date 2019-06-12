using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using System.Drawing;

namespace ASK
{
    /// <summary>
    /// Класс-оболочка для пользовательских параметров приложения.
    /// его использование ограничено статическим классом eeSaveCW
    /// </summary>
    private
    class AllSizeUserSettings : ApplicationSettingsBase
    {
        /// <summary>/// Единственное свойство с именем Data типа DataSet/// для хранения настроек всех форм и гридов/// </summary>
        [UserScopedSetting()]
        [DefaultSettingValue(null)]
        public DataSet Data
        {
            get
            {
                return ((DataSet)this["dsAllSizeUserSettings"]);
            }
            set
            {
                this["dsAllSizeUserSettings"] = (DataSet)value;
            }
        }

        /// <summary>
        /// Хранение настроек в статическом классе
        /// </summary>
        private
        static AllSizeUserSettings _settingsSaver = new AllSizeUserSettings();
        /// <summary>
        /// Сброс всех сохранённых настроек
        /// </summary>
        static
        public
        void ResetALL()
        {
            _settingsSaver.Data = null;
        }
        /// <summary>
        /// Формируем имя таблицы (DataTable) из имён Формы и Грида
        /// </summary>
        /// <param name="UserForm">Форма</param>
        /// <param name="UserGrid">Грид</param>
        /// <returns></returns>
        private
        static
        string GridTableName(Form UserForm, DataGridView UserGrid)
        {
            return UserForm.Name + "__" + UserGrid.Name;
        }
        /// <summary>
        /// Создание таблицы для хранения ширины колонок
        /// </summary>
        /// <param name="TableName">Имя таблицы</param>
        /// <returns>Готовая пустая таблица</returns>
        private
        static DataTable MakeColWidthTable(string TableName)
        {
            DataTable dt = new DataTable(TableName);
            DataColumn column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "GridColumnName";
            dt.Columns.Add(column);
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "GridColumnWidth";
            dt.Columns.Add(column);
            return dt;
        }
        /// <summary>
        /// Создание таблицы для хранения размеров и позиции формы
        /// </summary>
        /// <param name="TableName">Имя (формы)</param>
        /// <returns>Готовая пустая таблица</returns>
        private
        static DataTable MakeFormTable(string TableName)
        {
            DataColumn column;

            DataTable dt = new DataTable(TableName);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "Location_X";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "Location_Y";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "Height";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "Width";
            dt.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "WindowState";
            dt.Columns.Add(column);

            return dt;
        }
        /// <summary>
        /// Заполнение таблицы размерами колонок DataGridView
        /// </summary>
        /// <param name="dt">таблица</param>
        /// <param name="grid">грид</param>

        private static void FillGrideTable(DataTable dt, DataGridView grid)
        {
            foreach (DataGridViewColumn c in grid.Columns)
            {
                DataRow r = dt.NewRow();
                r["GridColumnName"] = c.Name;
                r["GridColumnWidth"] = c.Width;
                dt.Rows.Add(r);
            }
        }
        /// <summary>
        /// Заполнение таблицы, содержащей размеры формы
        /// </summary>
        /// <param name="dt">Таблица</param>
        /// <param name="f">Форма</param>
        private
        static
        void FillFormSizeTable(DataTable dt, Form f)
        {
            DataRow r = dt.NewRow();
            r["Location_X"] = f.Location.X;
            r["Location_Y"] = f.Location.Y;
            r["Height"] = f.Size.Height;
            r["Width"] = f.Size.Width;
            r["WindowState"] = f.WindowState;
            dt.Rows.Add(r);
        }
        /// <summary>/// Запись изменений в таблицу формы./// Сохраняет размеры формы, учитывая ее состояние /// (минимизирована/максимизирована/в нормальном состоянии)/// </summary>/// <param name="dt">таблица</param>/// <param name="f">форма</param>
        private static void UpdateFormSizeTable(DataTable dt, Form f)
        {
            DataRow r = dt.Rows[0];

            if (f.WindowState == FormWindowState.Normal)
            {
                r["Location_X"] = f.Location.X;
                r["Location_Y"] = f.Location.Y;
                r["Height"] = f.Size.Height;
                r["Width"] = f.Size.Width;
            }

            r["WindowState"] = f.WindowState;
        }

        /// <summary>
        /// Восстановление ширины колонок грида
        /// </summary>
        /// <param name="dt">Таблица</param>
        /// <param name="grid">Грид</param>
        private
        static
        void RestoreGridViewColWidths(DataTable dt, DataGridView grid)
        {
            foreach (DataRow r in dt.Rows)
                try
                {
                    grid.Columns[(string)r["GridColumnName"]].Width =
                      (int)r["GridColumnWidth"];
                }
                catch (Exception ex)
                {
                }
        }
        /// <summary>
        /// Восстановление местоположения и размеры формы
        /// </summary>
        /// <param name="dt">Таблица</param>
        /// <param name="form">Грид</param>
        private
        static
        void RestoreForm(DataTable dt, Form form)
        {
            DataRow rowForm = dt.Rows[0];
            try
            {
                form.Location = new Point((int)rowForm["Location_X"],
                                          (int)rowForm["Location_Y"]);
                form.Size = new Size((int)rowForm["Width"], (int)rowForm["Height"]);
                form.WindowState = (FormWindowState)rowForm["WindowState"];
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// Сохранение местоположения и размеры формы и ширина всех колонок всех гридов
        /// </summary>
        private
        static
        void SaveFormGrid(object sender, EventArgs e)
        {
            Form senderForm = (Form)sender;
            DataSet ds;
            if (_settingsSaver.Data != null) ds = _settingsSaver.Data;
            else ds = new DataSet();
            if (ds.Tables.IndexOf(senderForm.Name) == -1)
            {
                DataTable dt = MakeFormTable(senderForm.Name);
                FillFormSizeTable(dt, senderForm);
                ds.Tables.Add(dt);
            }
            else UpdateFormSizeTable(ds.Tables[senderForm.Name], senderForm);
            foreach (Control c in senderForm.Controls)
                if (c is DataGridView)
                {
                    DataGridView grid = (DataGridView)c;
                    string name = GridTableName(senderForm, grid);
                    if (ds.Tables.IndexOf(name) > -1) ds.Tables.Remove(name);
                    DataTable dt = MakeColWidthTable(name);
                    FillGrideTable(dt, grid);
                    ds.Tables.Add(dt);
                }
            _settingsSaver.Data = ds;
            _settingsSaver.Save();
        }
        /// <summary>
        /// Восстановление местоположения и размеры формы и ширина всех колонок всех гридов
        /// </summary>
        private
        static
        void RestoreFormGrid(object sender, EventArgs e)
        {
            if (_settingsSaver.Data == null) return;
            Form f = (Form)sender;
            DataSet ds = _settingsSaver.Data;
            if (ds.Tables.IndexOf(f.Name) == -1) return;
            RestoreForm(ds.Tables[f.Name], f);
            foreach (Control c in f.Controls)
                if (c is DataGridView)
                {
                    DataGridView grid = (DataGridView)c;
                    string name = GridTableName(f, grid);
                    if (ds.Tables.IndexOf(name) > -1)
                        RestoreGridViewColWidths(ds.Tables[name], grid);
                }
        }
        /// <summary>
        /// Включение механизма запоминания и восстановления.
        /// Добавляет обработку событий Load и FormClosing
        /// </summary>
        /// <param name="f">Форма</param>
        public
        static
        void SavingOn(Form f)
        {
            f.Load += RestoreFormGrid;
            f.FormClosing += SaveFormGrid;
        }
    }
}


