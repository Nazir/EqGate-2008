using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace EqGate
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.Closing += new CancelEventHandler(this.Exit);
        }

        public static string AppPath = String.Empty;
        public static string WorkPath = String.Empty;
        public static string AppName = String.Empty;
        public static string ConfigFile = String.Empty;
        public static string EqGateAPIFile = String.Empty;
        public static string AccountsFileExt = String.Empty; // ".acc";
        public static string[] AccountType;
        public static string[] AccountFile;
        public static string[] AccountSQL;
        public static string[] AccountResultFile;
        public static string[] AccountDB;
        public static int AccountIndex;
        public static string AccFile = String.Empty; // "iBank";

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ToolStripMenuItem_About_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("EQ Gate\nРазработчик: Хуснутдинов Назир Каримович\n© 2008 ОАО «Ак Барс» Банк «Интеркама»", "О программе...",
                             //MessageBoxButtons.OK, MessageBoxIcon.Information);
            AboutBox about = new AboutBox();
            about.Show();

        }

        private void Exit(Object sender, CancelEventArgs e)
        {
            if (CheckSession(false))
            {
                e.Cancel = true;
                if (MessageBox.Show("Сессия активна!\nВыйти из программы?", "Выход",
                      MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                      == DialogResult.Yes)
                {
                    CloseSession();
                    DisconnectNow();
                    e.Cancel = false;
                    Application.Exit();
                }
            }
            else
            {
                e.Cancel = false;
                Application.Exit();
            }
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(Application.ExecutablePath);
            AppPath = fi.DirectoryName + @"\";
            AppName = fi.Name;
            // Настройка фала логов
            Logger.Log.LogPath = AppPath;
            Logger.Log.LogName = Convert.ToString(DateTime.Today.Year) + Convert.ToString(DateTime.Today.Month) + Convert.ToString(DateTime.Today.Day) + "_" + AppName;
            Logger.Log.AppName = AppName;
            // ^ Настройка фала логов

            EqGateAPIFile = AppPath + "EQGateAPI.xml";
            if (!EQGateAPI.EQGateClassAPI.InitXML(EqGateAPIFile))
            {
                string sTemp = "Основной файл «" + EqGateAPIFile + "» с настройками API не найден! Прииложение будет закрыто...";
                Logger.Log.SaveLog("Main", sTemp, "err");
                MessageBox.Show(sTemp, "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            ConfigFile = AppPath + Path.GetFileNameWithoutExtension(AppName) + ".xml";
            Logger.Log.SaveLog("Main", "Чтение настроек из файла: «" + ConfigFile + "»", "inf");
            if (!EqGateConfig.InitXML(ConfigFile))
            {
                string sTemp = "Основной файл «" + ConfigFile + "» с настройками не найден! Прииложение будет закрыто...";
                Logger.Log.SaveLog("Main", sTemp, "err");
                MessageBox.Show(sTemp, "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            WorkPath = EqGateConfig.GetValue("WorkPath", "");
            if ((WorkPath == String.Empty) || (!Directory.Exists(WorkPath)))
                WorkPath = AppPath;

            if (!Directory.Exists(WorkPath))
            {
                string sTemp = "Рабочий каталог «" + WorkPath + "» не найден! Прииложение будет закрыто...";
                Logger.Log.SaveLog("Main", sTemp, "err");
                MessageBox.Show(sTemp, "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            AccountsFileExt = EqGateConfig.GetAttribute("Ext", "Accounts", "");

            int Count = 0;

            //
            Count = EqGateConfig.GetChildNodesCount("Accounts", "");
            AccountType = new string[Count];
            AccountFile = new string[Count];
            AccountSQL = new string[Count];
            AccountResultFile = new string[Count];
            AccountDB = new string[Count];
            for (int iCounter = 0; iCounter < Count; iCounter++)
            {
                AccountType[iCounter] = EqGateConfig.GetAttribute("Type", "", "Accounts", iCounter);
                AccountFile[iCounter] = EqGateConfig.GetAttribute("Name", "", "Accounts", iCounter);
                AccountResultFile[iCounter] = EqGateConfig.GetAttribute("ResultFile", "", "Accounts", iCounter);
                if (AccountType[iCounter] == "SQL")
                {
                    AccountDB[iCounter] = EqGateConfig.GetAttribute("DB", "", "Accounts", iCounter);
                    AccountSQL[iCounter] = EqGateConfig.GetValue("", "Accounts", iCounter);
                }
                comboBox_AccountsList.Items.Insert(iCounter, EqGateConfig.GetAttribute("Description", "", "Accounts", iCounter)
                    + " [" + AccountFile[iCounter] + "]"
                    + " {" + AccountType[iCounter] + "}");
            }

            if (comboBox_AccountsList.Items.Count > 0)
                comboBox_AccountsList.SelectedIndex = 0;

            
            Count = EqGateConfig.GetChildNodesCount("Hosts", "");
            comboBox_Host.Items.Clear();
            for (int iCounter = 0; iCounter < Count; iCounter++)
                comboBox_Host.Items.Insert(iCounter, EqGateConfig.GetValue("", "Hosts", iCounter));
            if (comboBox_Host.Items.Count > 0)
                comboBox_Host.SelectedIndex = 0;

            Count = EqGateConfig.GetChildNodesCount("Ports", "");
            comboBox_Port.Items.Clear();
            for (int iCounter = 0; iCounter < Count; iCounter++)
                comboBox_Port.Items.Insert(iCounter, EqGateConfig.GetValue("", "Ports", iCounter));
            if (comboBox_Port.Items.Count > 0)
                comboBox_Port.SelectedIndex = 0;

            Count = EqGateConfig.GetChildNodesCount("Blocs", "");
            comboBox_DB.Items.Clear();
            for (int iCounter = 0; iCounter < Count; iCounter++)
                comboBox_DB.Items.Insert(iCounter, EqGateConfig.GetValue("", "Blocs", iCounter));
            if (comboBox_DB.Items.Count > 0)
                comboBox_DB.SelectedIndex = 0;

            Count = EqGateConfig.GetChildNodesCount("Users", "");
            comboBox_User.Items.Clear();
            for (int iCounter = 0; iCounter < Count; iCounter++)
                comboBox_User.Items.Insert(iCounter, EqGateConfig.GetValue("", "Users", iCounter));
            if (comboBox_User.Items.Count > 0)
                comboBox_User.SelectedIndex = 0;

            // Подготовка формы
            textBox_Session_Info.Clear();
            toolStripStatusLabel_ClientVersion.Text = String.Empty;
            toolStripStatusLabel_ServerVersion.Text = String.Empty;
            groupBox_OpenSession.Enabled = false;
            groupBox_CloseSession.Enabled = false;
            groupBox_Disconnect.Enabled = false;
            dateTimePicker_HZBPD.Value = DateTime.Now;
            // Определение версии клиента
            toolStripStatusLabel_ClientVersion.Text = "Версия клиента: " + EQGateDLL.GetClientVersion();
        }

        private void notifyIcon_Main_DoubleClick(object sender, EventArgs e)
        {
            // Show the form when the user double clicks on the notify icon.
            Show();
            // Set the WindowState to normal if the form is minimized.
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            // Activate the form.
            this.Activate();
        }

        private void ShowErrorsWarnings(bool bClear)
        {
            // Ошибки/Предупреждения
            if (bClear)
                listView_Errors_Warnings.Items.Clear();
            int Count = EQGateDLL.ksm_Code.Rank;
            for (int iCounter = 0; iCounter < Count; iCounter++)
            {
                ListViewItem item1 = new ListViewItem(Convert.ToString(iCounter + 1), 0);
                item1.SubItems.Add(EQGateDLL.ksm_Code[iCounter]);
                item1.SubItems.Add(EQGateDLL.ksm_Severity[iCounter]);
                item1.SubItems.Add(EQGateDLL.ksm_Text[iCounter]);
                listView_Errors_Warnings.Items.AddRange(new ListViewItem[] { item1 });
            }
            //listView_Errors_Warnings.Columns[0].ListView.Items.Add(EQGateDLL.ksm_Code);
            textBox_Session_Info.ForeColor = Color.FromName("Red");
            textBox_Session_Info.Text = " Ошибки/Предупреждения";
            textBox_EQGateStatus.Text = EQGateDLL.GetEQGateStatus(-1);
        }

        private void FillListView(ListView lv, int Count, string api, string ToFile, bool AppendResult)
        {
             if (Count < 1)
                return;
            /*
            if ((EQGateDLL.RequestOutput.Length / Count) < EQGateAPI.EQGateClassAPI.GetFieldsSize(api, ""))
            {
                MessageBox.Show("Размер выходных данных меньше общего размера API!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            // */
            // 
            toolStripProgressBar.Value = 0;
            toolStripProgressBar.Maximum = toolStripProgressBar.Step * Count;
            toolStripProgressBar.Visible = true;
            if (EQGateDLL.RequestOutput.Length == 0)
                return;
            if ((!AppendResult) && (File.Exists(ToFile)))
            {
                File.Delete(ToFile);
            }

            string temp = String.Empty;


            int FieldsCount = 0;
            FieldsCount = EQGateAPI.EQGateClassAPI.GetFieldsCount(api, "");

            if (lv.Columns.Count < FieldsCount)
            {
                lv.Columns.Clear();
                for (int iCounter = 0; iCounter < FieldsCount; iCounter++)
                {
                    ColumnHeader columnHeader = new ColumnHeader();
                    columnHeader.Text = EQGateAPI.EQGateClassAPI.GetFieldValue(api, "", iCounter, "description", "");
                    columnHeader.Width = Convert.ToInt32(EQGateAPI.EQGateClassAPI.GetFieldValue(api, "", iCounter, "width", ""));

                    // Add the column headers to listView_Events.
                    lv.Columns.AddRange(new ColumnHeader[] { columnHeader });
                }
            }

            //int ListCount = ( sb_mcs.ToString(5, 4) as int );
            string ResultFileDivider = "^";
            //Divider = "|";

            using (StreamWriter sw = new StreamWriter(ToFile, AppendResult, Encoding.GetEncoding(866)))
            {
                for (int iCounter = 0; iCounter < Count; iCounter++)
                {
                    string Size = String.Empty;
                    StringBuilder sb = new StringBuilder();
                    ListViewItem item = new ListViewItem();
                    for (int i = 0; i < FieldsCount; i++)
                    {
                        Size = EQGateAPI.EQGateClassAPI.GetFieldValue(api, "", i, "size", "");
                        temp = EQGateDLL.GetRequestOutputItem(Convert.ToInt32(Convert.ToDouble(Size)));
                        temp = temp.Replace(ResultFileDivider, " ");
                        if (EQGateAPI.EQGateClassAPI.GetFieldValue(api, "", i, "type", "") == "A")
                            sb.Append(temp.Trim());
                        if (EQGateAPI.EQGateClassAPI.GetFieldValue(api, "", i, "type", "") == "D")
                            sb.Append(EQGateDLL.ConvertDateToFormat(temp, ""));
                        if (EQGateAPI.EQGateClassAPI.GetFieldValue(api, "", i, "type", "") == "U")
                            sb.Append(EQGateDLL.ConvertToCurrency(temp, "U", Size));
                        if (EQGateAPI.EQGateClassAPI.GetFieldValue(api, "", i, "type", "") == "S")
                            sb.Append(EQGateDLL.ConvertToCurrency(temp, "S", Size));
                        if (EQGateAPI.EQGateClassAPI.GetFieldValue(api, "", i, "type", "") == "B")
                        {
                            if (temp == "Y")
                                sb.Append("Истина");
                            if (temp == "N")
                                sb.Append("Ложь");
                        }
                        if (i == 0)
                            item.Text = temp;
                        else
                            item.SubItems.Add(temp);
                        //sb.Append(temp);
                        sb.Append(ResultFileDivider);
                    }
                    lv.Items.AddRange(new ListViewItem[] { item });
                    sw.Write(sb.ToString());
                    //toolStripProgressBar.Value += 1;
                    sw.Write("\r\n");
                }
            }
            /*
            if (File.Exists(ToFile + ".tmp"))
            {
                File.Copy(ToFile + ".tmp", ToFile, true);
                File.Delete(ToFile + ".tmp");
            } //*/
            //toolStripProgressBar.Visible = false;
        }

        private void Connect()
        {
            // Открытие соединения
            // Call the Sleep method to allow the user to see the image.
            //System.Threading.Thread.Sleep(1000);
            if (EQGateDLL.Connect() != true)
            {
                
            }
            Thread.Sleep(0);
        }

        private void button_Connect_Click(object sender, EventArgs e)
        {
            // Открытие соединения
            button_Connect.Image = imageList_Main.Images["spinner.gif"]; // EqGate.Properties.Resources.spinner;
            Cursor = Cursors.WaitCursor;
           // splitContainer_Main.Enabled = false;
            toolStripProgressBar.Visible = true;

            EQGateDLL.EQHost = comboBox_Host.Text;
            EQGateDLL.EQPort = Convert.ToInt32(comboBox_Port.Text);
 
            Thread t = new Thread(new ThreadStart(Connect));
            t.Start();

            toolStripProgressBar.Maximum = 100;
            toolStripProgressBar.Value = 0;
            toolStripProgressBar.Step = 10;
            while (EQGateDLL.EQGateStatus < 0)
            {
                if (toolStripProgressBar.Value >= toolStripProgressBar.Maximum)
                    toolStripProgressBar.Value = 0;
                toolStripProgressBar.Value += 10;
                Thread.Sleep(0);
            }

            t.Join();

            if (EQGateDLL.EQGateStatus != 0)
            {
                textBox_EQGateStatus.Text = EQGateDLL.GetEQGateStatus(-1);
                splitContainer_Main.Enabled = true;
                toolStripStatusLabel_ConnectionStatus.ForeColor = Color.FromName("Red");
                toolStripStatusLabel_ConnectionStatus.Text = "Отключено";
                button_Connect.Image = imageList_Main.Images["start.png"];//  EqGate.Properties.Resources.start;
                Cursor = Cursors.Default;
                toolStripProgressBar.Visible = false;
                return;
            }
            toolStripProgressBar.Visible = false;
            splitContainer_Main.Enabled = true;
            Cursor = Cursors.Default;
            button_Connect.Image = imageList_Main.Images["apply.png"]; // EqGate.Properties.Resources.apply;

            //
            toolStripStatusLabel_ConnectionStatus.ForeColor = Color.FromName("Green");
            toolStripStatusLabel_ConnectionStatus.Text = "Подключено";
            groupBox_Connect.Enabled = false;
            groupBox_Disconnect.Enabled = true;
            groupBox_OpenSession.Enabled = true;

            // Определение версии сервера
            toolStripStatusLabel_ServerVersion.Text = "Версия сервера: " + EQGateDLL.GetServerVersion();
        }

        private void DisconnectNow()
        {
            // Закрытие соединения
            if (EQGateDLL.Disconnect() != true)
            {
                textBox_EQGateStatus.Text = EQGateDLL.GetEQGateStatus(-1);
                return;
            }
            toolStripStatusLabel_ConnectionStatus.ForeColor = Color.FromName("Red");
            toolStripStatusLabel_ConnectionStatus.Text = "Отключено";
            groupBox_Connect.Enabled = true;
            groupBox_OpenSession.Enabled = false;
            groupBox_Disconnect.Enabled = false;
            button_Connect.Image = imageList_Main.Images["start.png"];// EqGate.Properties.Resources.start;
        }
        private void button_Disconnect_Click(object sender, EventArgs e)
        {
            DisconnectNow();
        }

        public void OpenSession()
        {
            // Открытие сессии
            if (EQGateDLL.OpenSession(comboBox_DB.Text, comboBox_User.Text, maskedTextBox_Password.Text) != true)
            {
                textBox_SessionID.Text = EQGateDLL.SessionID;
                ShowErrorsWarnings(true); // Ошибки/Предупреждения
                button_Open.Image = imageList_Main.Images["start.png"]; //EqGate.Properties.Resources.start;
                Cursor = Cursors.Default;
                textBox_SessionID.Text = EQGateDLL.SessionID;
                return;
            }
            //
            textBox_Session_Info.ForeColor = Color.FromName("Green");
            textBox_Session_Info.Text = "Готово";
            textBox_SessionID.Text = EQGateDLL.SessionID;
            groupBox_OpenSession.Enabled = false;
            groupBox_CloseSession.Enabled = true;
            groupBox_Disconnect.Enabled = false;
            textBox_EQGateStatus.Text = EQGateDLL.GetEQGateStatus(-1);
        }

        private void button_Open_Click(object sender, EventArgs e)
        {
            button_Open.Image = imageList_Main.Images["spinner.gif"]; //EqGate.Properties.Resources.spinner;
            Cursor = Cursors.WaitCursor;

            if ((EQGateDLL.SessionID != "") && (EQGateDLL.SessionID != "[Сессия закрыта]") && (EQGateDLL.SessionID != null))
            {
                CloseSession();
                OpenSession();
            }
            else
                OpenSession();

            Cursor = Cursors.Default;
            if (EQGateDLL.EQGateStatus != 0)
                button_Open.Image = imageList_Main.Images["lockstart_session.png"];
            else
                button_Open.Image = imageList_Main.Images["apply.png"]; //EqGate.Properties.Resources.apply;
        }

        public void CloseSession()
        {
            // Закрытие сессии
            if (EQGateDLL.CloseSession() != true)
            {
                textBox_SessionID.Text = EQGateDLL.SessionID;
                ShowErrorsWarnings(true); // Ошибки/Предупреждения
                return;
            }
            textBox_Session_Info.ForeColor = Color.FromName("Green");
            textBox_Session_Info.Text = "Готово";
            textBox_SessionID.Text = EQGateDLL.SessionID;
            groupBox_OpenSession.Enabled = true;
            groupBox_CloseSession.Enabled = false;
            groupBox_Disconnect.Enabled = true;
            textBox_EQGateStatus.Text = EQGateDLL.GetEQGateStatus(-1);
            button_Open.Image = imageList_Main.Images["start.png"]; //EqGate.Properties.Resources.start;
        }

        private void button_CloseSession_Click(object sender, EventArgs e)
        {
            CloseSession();
        }

        public bool CheckSession(bool ShowMsg)
        {
            textBox_Session_Info.Clear();
            if (!EQGateDLL.ServerConnected)
            {
                if (ShowMsg)
                    MessageBox.Show("Сессия не активна!!!\nНет подключения к серверу!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            // Проверка статуса сессии
            StringBuilder sb_input = new StringBuilder(0);
            sb_input = sb_input.Append(" ");

            if (EQGateDLL.Request("YPING0R", sb_input, "") != true)
            {
                textBox_SessionID.Text = EQGateDLL.SessionID;
                ShowErrorsWarnings(true); // Ошибки/Предупреждения
                textBox_Session_Info.ForeColor = Color.FromName("Orange");
                textBox_Session_Info.Text = "Сессия не активна!!!";
                if (ShowMsg)
                    MessageBox.Show("Сессия не активна!!!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            //
            if (EQGateDLL.RequestOutput.ToString(0, 1) != "1")
            {
                textBox_SessionID.Text = EQGateDLL.SessionID;
                ShowErrorsWarnings(true); // Ошибки/Предупреждения
                textBox_Session_Info.ForeColor = Color.FromName("Orange");
                textBox_Session_Info.Text = "Сессия не активна!!!";
                if (ShowMsg)
                    MessageBox.Show("Сессия не активна!!!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            textBox_Session_Info.ForeColor = Color.FromName("Green");
            textBox_Session_Info.Text = "Готово";
            textBox_SessionID.Text = EQGateDLL.SessionID;
            textBox_EQGateStatus.Text = EQGateDLL.GetEQGateStatus(-1);
            listView_Errors_Warnings.Items.Clear();
            if (ShowMsg)
              MessageBox.Show("Сессия активна!!!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        private void button_CheckSession_Click(object sender, EventArgs e)
        {
            CheckSession(true);
        }

        private void button_ClientInfo_Click(object sender, EventArgs e)
        {
            if (CheckSession(false))
            {
                // Запрос информации по клиенту
                string API = "YG01EER";
                StringBuilder sb_input = new StringBuilder(0);
                sb_input = sb_input.Append(textBox_GZCUS.Text); // Мнемоника клиента А(1)
                sb_input = sb_input.Append("   "); // Код месторасположения клиента А(3)
                sb_input = sb_input.Append(" "); // Код внешней системы А(1)
                sb_input = sb_input.Append(EQGateDLL.InsertSpaces("", 22)); // Зарезервировано А(22) "                      "

                if (EQGateDLL.Request(API, sb_input, EQGateAPI.EQGateClassAPI.GetAPIAttribute(API, "type")) != true)
                {
                    ShowErrorsWarnings(true); // Ошибки/Предупреждения
                    return;
                }
                //
                textBox_Session_Info.ForeColor = Color.FromName("Green");
                textBox_Session_Info.Text = "Готово";
                int ListCount = 1;
                FillListView(listView_YG01EER, ListCount, API, WorkPath + API + ".txt", false);

            }
            
        }

        private void button_YH68EER_Click(object sender, EventArgs e)
        {
            // Запрос текущего остатка по счету
            if (CheckSession(false))
            {
                listView_AccountsInfo.Items.Clear();
                listView_AccountsInfo.Columns.Clear();
                groupBox_AccountsInfo.Text = groupBox_YH68EER.Text;
                string API = "YH68EER";
                StringBuilder sb_input = new StringBuilder(0);
                sb_input = sb_input.Append(textBox_HZEAN.Text); // Номер лицевого счета А(20)
                sb_input = sb_input.Append(" "); // Код внешней системы А(1)
                sb_input = sb_input.Append(EQGateDLL.InsertSpaces("", 29)); // Зарезервировано А(29) "                             "

                if (EQGateDLL.Request(API, sb_input, EQGateAPI.EQGateClassAPI.GetAPIAttribute(API, "type")) != true)
                {
                    ShowErrorsWarnings(true); // Ошибки/Предупреждения
                    return;
                }
                //
                textBox_Session_Info.ForeColor = Color.FromName("Green");
                textBox_Session_Info.Text = "Готово";
                //textBox_AccountInfo.Text = EQGateDLL.RequestOutput;
                //textBox_HZCABL.Text = textBox_AccountInfo.Text.Substring(0, 16);
                //textBox_HZODL.Text = textBox_AccountInfo.Text.Substring(16, 16);
                int ListCount = 1;
                //ListCount = EQGateDLL.mcs_RecordsReturned;
                FillListView(listView_AccountsInfo, ListCount, API, WorkPath + API + ".txt", false);
            }
        }

        private void button_AccountInfo_Click(object sender, EventArgs e)
        {
            // Запрос текущего остатка по списку счетов
            if (CheckSession(false))
            {
                listView_AccountsInfo.Items.Clear();
                listView_AccountsInfo.Columns.Clear();
                groupBox_AccountsInfo.Text = groupBox_AccountsList.Text;
                string API = "YH68EER";
                AccFile = WorkPath + AccountFile[AccountIndex] + AccountsFileExt;
                if (!File.Exists(AccFile))
                {
                    Logger.Log.SaveLog("RUN_5", "Файл со счетами «" + AccFile + "» не найден!", "err");
                }
                if (File.Exists(AccFile))
                {
                    FileInfo fi = new FileInfo(AccFile);
                    using (StreamReader sr = new StreamReader(AccFile))
                    {
                        while (sr.Peek() >= 0)
                        {
                            StringBuilder sb_input = new StringBuilder(0);
                            sb_input = sb_input.Append(sr.ReadLine().Trim()); // Номер лицевого счета А(20)
                            sb_input = sb_input.Append(" "); // Код внешней системы А(1)
                            sb_input = sb_input.Append(EQGateDLL.InsertSpaces("", 29)); // Зарезервировано А(29) "                             "

                            if (EQGateDLL.Request(API, sb_input, EQGateAPI.EQGateClassAPI.GetAPIAttribute(API, "type")) != true)
                            {
                                ShowErrorsWarnings(true); // Ошибки/Предупреждения
                                return;
                            }
                            //
                            textBox_Session_Info.ForeColor = Color.FromName("Green");
                            textBox_Session_Info.Text = "Готово";
                            //textBox_AccountInfo.Text = EQGateDLL.RequestOutput;
                            //textBox_HZCABL.Text = textBox_AccountInfo.Text.Substring(0, 16);
                            //textBox_HZODL.Text = textBox_AccountInfo.Text.Substring(16, 16);
                            int ListCount = 1;
                            //ListCount = EQGateDLL.mcs_RecordsReturned;
                            FillListView(listView_AccountsInfo, ListCount, API, WorkPath + "osv.txt", true);
                        }
                    }
                }
            }
        }

        private void button_YS01DER_Click(object sender, EventArgs e)
        {
            try
            {
                // Запрос на существование счетов
                if (CheckSession(false))
                {
                    listView_AccountsInfo.Items.Clear();
                    listView_AccountsInfo.Columns.Clear();
                    groupBox_AccountsInfo.Text = groupBox_YS01DER.Text;
                    string API = "YS01DER";
                    StringBuilder sb_input = new StringBuilder(0);
                    sb_input = sb_input.Append(EQGateDLL.InsertSpaces(textBox_YS01DER_HSMSK.Text, 20)); // Номер лицевого счета А(20)
                    sb_input = sb_input.Append(" "); // Код внешней системы А(1)
                    sb_input = sb_input.Append(EQGateDLL.InsertSpaces("", 29)); // Зарезервировано А(29) "                             "

                    if (EQGateDLL.Request(API, sb_input, EQGateAPI.EQGateClassAPI.GetAPIAttribute(API, "type")) != true)
                    {
                        ShowErrorsWarnings(true); // Ошибки/Предупреждения
                        return;
                    }
                    //
                    textBox_Session_Info.ForeColor = Color.FromName("Green");
                    textBox_Session_Info.Text = @"Готово";
                    textBox_Session_Info.Text += @"  QueryType = " + EQGateDLL.mcs_QueryType.ToString();
                    textBox_Session_Info.Text += @"  RecordsRequested = " + EQGateDLL.mcs_RecordsRequested.ToString();
                    textBox_Session_Info.Text += @"  RecordsReturned = " + EQGateDLL.mcs_RecordsReturned.ToString();
                    textBox_Session_Info.Text += @"  NotFinished = " + EQGateDLL.mcs_NotFinished.ToString();
                    int ListCount = 1;
                    ListCount = EQGateDLL.mcs_RecordsReturned;
                    FillListView(listView_AccountsInfo, ListCount, API, WorkPath + API + ".txt", false);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
        }

        private void button_YYBXDER_Click(object sender, EventArgs e)
        {
            // Проводки по лицевому счету
            if (CheckSession(false))
            {
                string API = "YYBXDER";
                // Дата, за которую отбираются проводки D
                string Date = "        ";
                DateTime dt = dateTimePicker_HZBPD.Value;
                Date = dt.ToString("yyyyMMdd");

                StringBuilder sb_input = new StringBuilder(0);
                sb_input = sb_input.Append(Date); // Дата учета D
                sb_input = sb_input.Append(EQGateDLL.InsertSpaces(textBox_Brief.Text, 20)); // Номер лицевого счета А(20)
                sb_input = sb_input.Append(" "); // Код внешней системы А(1)
                sb_input = sb_input.Append(EQGateDLL.InsertSpaces("", 21)); // Зарезервировано А(21) 

                EQGateDLL.mcs_QueryType = 0;
                EQGateDLL.mcs_RecordsRequested = Convert.ToInt32(numericUpDown_RecordsRequested.Value);
                EQGateDLL.mcs_RecordsReturned = 0;
                EQGateDLL.mcs_NotFinished = 0;

                if (EQGateDLL.Request(API, sb_input, EQGateAPI.EQGateClassAPI.GetAPIAttribute(API, "type")) != true)
                {
                    ShowErrorsWarnings(true); // Ошибки/Предупреждения
                    return;
                }
                //
                textBox_Session_Info.ForeColor = Color.FromName("Green");
                textBox_Session_Info.Text = @"Готово";
                textBox_Session_Info.Text += @"  QueryType = " + EQGateDLL.mcs_QueryType.ToString();
                textBox_Session_Info.Text += @"  RecordsRequested = " + EQGateDLL.mcs_RecordsRequested.ToString();
                textBox_Session_Info.Text += @"  RecordsReturned = " + EQGateDLL.mcs_RecordsReturned.ToString();
                textBox_Session_Info.Text += @"  NotFinished = " + EQGateDLL.mcs_NotFinished.ToString();
                listView_YYBXDER.Items.Clear();
                int ListCount = EQGateDLL.mcs_RecordsReturned;
                FillListView(listView_YYBXDER, ListCount, API, WorkPath + API + ".txt", false);
            }
        }

        private void button_YH18EER_Click(object sender, EventArgs e)
        {
            // Запрос информации о проводке
            if (CheckSession(false))
            {
                string API = "YH18EER";
                StringBuilder sb_input = new StringBuilder(0);
                sb_input = sb_input.Append(EQGateDLL.InsertSpaces(textBox_GZPID.Text, 21)); // ID проводки
                sb_input = sb_input.Append(" "); // Код внешней системы А(1)
                sb_input = sb_input.Append(EQGateDLL.InsertSpaces("", 28)); // Зарезервировано А(28) 

                if (EQGateDLL.Request(API, sb_input, EQGateAPI.EQGateClassAPI.GetAPIAttribute(API, "type")) != true)
                {
                    ShowErrorsWarnings(true); // Ошибки/Предупреждения
                    return;
                }
                //
                textBox_Session_Info.ForeColor = Color.FromName("Green");
                textBox_Session_Info.Text = "Готово";


                int ListCount = 1;
                FillListView(listView_YH18EER, ListCount, API, WorkPath + API + ".txt", false);
            }
        }

        private void button_YH69EER_Click(object sender, EventArgs e)
        {
            // Запрос наличия счета
            if (CheckSession(false))
            {
                string API = "YH69EER";
                StringBuilder sb_input = new StringBuilder(0);
                sb_input = sb_input.Append(EQGateDLL.InsertSpaces(textBox_YH69EER_HZEAN.Text, 20)); // 
                sb_input = sb_input.Append(" "); // Код внешней системы А(1)
                sb_input = sb_input.Append(EQGateDLL.InsertSpaces("", 11)); // Зарезервировано А(11) 

                if (EQGateDLL.Request(API, sb_input, EQGateAPI.EQGateClassAPI.GetAPIAttribute(API, "type")) != true)
                {
                    ShowErrorsWarnings(true); // Ошибки/Предупреждения
                    return;
                }
                //
                textBox_Session_Info.ForeColor = Color.FromName("Green");
                textBox_Session_Info.Text = "Готово";


                int ListCount = 1;
                FillListView(listView_YH69EER, ListCount, API, WorkPath + API + ".txt", false);
            }
        }

        private void button_ClientsList_Click(object sender, EventArgs e)
        {
            // Запрос наличия счета
            if (CheckSession(false))
            {
                string API = "YH69EER";
                string AccFile = WorkPath + @"SP-SCH.acc";
                if (File.Exists(AccFile))
                {
                    try
                    {
                        // Create an instance of StreamReader to read from a file.
                        // The using statement also closes the StreamReader.
                        using (StreamReader sr = new StreamReader(AccFile))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                StringBuilder sb_input = new StringBuilder(0);
                                sb_input = sb_input.Append(EQGateDLL.InsertSpaces(textBox_YH69EER_HZEAN.Text, 20)); // 
                                sb_input = sb_input.Append(" "); // Код внешней системы А(1)
                                sb_input = sb_input.Append(EQGateDLL.InsertSpaces("", 11)); // Зарезервировано А(11) 

                                if (EQGateDLL.Request(API, sb_input, EQGateAPI.EQGateClassAPI.GetAPIAttribute(API, "type")) != true)
                                {
                                    ShowErrorsWarnings(true); // Ошибки/Предупреждения
                                    return;
                                }
                                string ToFile = WorkPath + @"SP-SCH.txt";
                                string temp = String.Empty;
                                int FieldsCount = 0;
                                using (StreamWriter sw = File.CreateText(ToFile))
                                {
                                    FieldsCount = EQGateAPI.EQGateClassAPI.GetFieldsCount(API, "");
                                    temp = EQGateDLL.GetRequestOutputItem(Convert.ToInt32(Convert.ToDouble(EQGateAPI.EQGateClassAPI.GetFieldValue(API, "", 0, "size", ""))));
                                    for (int i = 1; i < FieldsCount; i++)
                                    {
                                        string Size = EQGateAPI.EQGateClassAPI.GetFieldValue(API, "", i, "size", "");
                                        temp = EQGateDLL.GetRequestOutputItem(Convert.ToInt32(Convert.ToDouble(Size)));
                                        sw.Write(temp.Trim());
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        // Let the user know what went wrong.
                        //Console.WriteLine("The file could not be read:");
                        //Console.WriteLine(exc.Message);
                        Logger.Log.SaveLog("", exc.ToString(), "err");
                    }
                    textBox_Session_Info.ForeColor = Color.FromName("Green");
                    textBox_Session_Info.Text = "Готово";


                    int ListCount = 1;

                    FillListView(listView_YH69EER, ListCount, "YH69EER", WorkPath + @"YH69EER.txt", false);
                }
            }
        }

        private void notifyIcon_Main_Click(object sender, EventArgs e)
        {
            notifyIcon_Main.ShowBalloonTip(3);
        }

        private void listView_YYBXDER_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection GZPID = 
			this.listView_YYBXDER.SelectedItems;
		
            foreach (ListViewItem item in GZPID)
		    {
                textBox_GZPID.Text = item.SubItems[0].Text;
		    }

        }

        private void maskedTextBox_Password_Enter(object sender, EventArgs e)
        {
            maskedTextBox_Password.SelectAll();
        }

        private void maskedTextBox_Password_MouseClick(object sender, MouseEventArgs e)
        {
            maskedTextBox_Password.SelectAll();
        }

        private void maskedTextBox_Password_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                button_Open_Click(sender, null);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(EQGateDLL.ConvertToCurrency("000001259567895", "U", "15,0"));
        }

        private void textBox_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox tb;
                tb = (TextBox)sender;
                tb.SelectAll();
            }
        }

        private void textBox_MouseClick(object sender, MouseEventArgs e)
        {
            textBox_Enter(sender, e);
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox tb;
                tb = (TextBox)sender;
                ///this. tb.Tag;
            }
        }
    }
}
