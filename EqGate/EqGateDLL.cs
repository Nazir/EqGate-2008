using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace EqGate {
public class EQGateDLL
{
    public static string EQHost = "192.168.9.179";
    public static int EQPort = 8111;
    public static uint EQGATE_DESCRIPTOR; // указатель на дескриптор соединения.
    public static int EQGATE_VERSION_SIZE = 22;
    public static int EQGATE_ONE_KSM_SIZE = 141;
    public static int EQGATE_KSM_COUNT = 50;
    public static int EQGATE_KSM_SIZE = (141 * 50);
    public static int EQGATE_OUTPUT_SIZE = (1024 * 5000); // 900K
    public static int EQGATE_MCS_SIZE = 10;
    public static int EQGateStatus = -1;
    public static string[] ksm_Code = new string[50];
    public static string[] ksm_Severity = new string[50];
    public static string[] ksm_Text = new string[50];
    public static int mcs_QueryType = 0;
    public static int mcs_RecordsRequested = 0;
    public static int mcs_RecordsReturned = 0;
    public static int mcs_NotFinished = 0;
    public static StringBuilder RequestOutput;
    public static int RequestOutputCurrentLength;
    public static bool ServerConnected;
    public static string SessionID;
    public static string SessionID_path = @"SessionID.txt";

    [DllImport("EQGateClient.dll")]
    public static extern int EQGateClientVersion(StringBuilder buf, int buf_size);

    [DllImport("EQGateClient.dll")]
    public static unsafe extern int EQGateConnect(uint* dsc, StringBuilder host, int port);

    [DllImport("EQGateClient.dll")]
    public static unsafe extern int EQGateDisconnect(uint* dsc);

    [DllImport("EQGateClient.dll")]
    public static unsafe extern int EQGateServerVersion(uint* dsc, StringBuilder buf, int buf_size);

    [DllImport("EQGateClient.dll")]
    public static unsafe extern int EQGateRequest(
                                                    uint* dsc, 
                                                    StringBuilder api, 
                                                    StringBuilder session_id, 
                                                    StringBuilder input, 
                                                    StringBuilder ksm, 
                                                    int ksm_buf_size, 
                                                    StringBuilder output, 
                                                    int output_buf_size, 
                                                    StringBuilder mcs, 
                                                    int mcs_buf_size 
                                                    );

    public static string GetEQGateStatus(int Status)
    {
        if (Status == -1)
            Status = EQGateStatus;
        // Коды ошибок клиентской библиотеки
        switch (Status)
        {
            case 0: return @"0: EQGateStatus_OK 
Нет ошибки. Функция выполнена успешно.";
            case 1: return @"1: EQGATE_ERR_BADSIZE
Параметр, содержащий размер области памяти, имеет некорректное значение.";
            case 2: return @"2: EQGATE_ERR_NULLPOINTER
Параметр, содержащий указатель на  область памяти, имеет нулевое (некорректное) значение.";
            case 3: return @"3: EQGATE_ERR_CONNECT
Не удалось установить TCP-соединение.";
            case 4: return @"4: EQGATE_ERR_READSOCKET
Ошибка чтения сокета TCP.";
            case 5: return @"5: EQGATE_ERR_WRITESOCKET
Ошибка записи в сокет TCP.";
            case 6: return @"6: EQGATE_ERR_PROTOCOL
Ошибка протокола (сервер не готов к обслуживанию клиентов).";
            case 7: return @"7: EQGATE_ERR_SOCKET
Не удалось создать TCP-сокет.";
            case 8: return @"8: EQGATE_ERR_GETHOSTBYNAME
Не удалось определить IP-адрес сервера по имени.";
            case 9: return @"9: EQGATE_ERR_DESCRIPTOR
Некорректный дескриптор соединения.";
            case 10: return @"10: EQGATE_ERR_APIFAILED
Вызов функции Equation завершился неудачей.";
            default: return @"-1: ERROR";
        }
    }

    public static string InsertSpaces(string Value, int Size)
    {
        // Заполение пробелами
        string Result = Value;
        while ( Result.Length < Size )
        {
            Result += " ";
        }
        if (Result.Length != Size)
            MessageBox.Show("Error: InsertSpaces");

        return Result;
    }

    public static string InsertZero(string Value, int Size)
    {
        // Заполение пробелами
        string Result = Value;
        while (Result.Length < Size)
        {
            Result = "0" + Result;
        }
        if (Result.Length != Size)
            MessageBox.Show("Error: InsertSpaces");

        return Result;
    }

    public static void Get_ksm_array(StringBuilder ksm)
    {
        // 
        int Count = ksm.Length / 141;
        for (int iCounter = 0; iCounter < Count; iCounter++)
        {
            ksm_Code[iCounter] = ksm.ToString(0, 7);
            ksm_Severity[iCounter] = ksm.ToString(7, 2);
            ksm_Text[iCounter] = ksm.ToString(9, 132);
        }
        return;
    }

    public static string ConvertDateToFormat(string Date, string Format)
    {
        if (Format == "")
            Format = "dd.MM.yyyy";

        DateTime dt = new DateTime();
        if ((Date == "") || (Date == "00000000") || (Date == "        "))
            dt = new DateTime(1900, 01, 01);
        else
            dt = new DateTime(Convert.ToInt32(Date.Substring(0, 4)),
                Convert.ToInt32(Date.Substring(4, 2)),
                Convert.ToInt32(Date.Substring(6, 2)));
        return dt.ToString(Format);
    }

    public static string ConvertToCurrency(string Value, string type, string Size)
    {
        long Numerator = 0; // числитель
        int Denominator = 0; // знаменатель
        int MinusSign = 1; // знак минус
        int DenominatorSize = 0; // количество десятичных знаков

        if (Value.Length == 0)
            return "0.00";

        string CurrencySeparator = "."; // разделитель
        int iTemp = 0;
        iTemp = Size.IndexOf(",");
        if ((iTemp != -1) && ((type == "U") || (type == "S")))
            DenominatorSize = Convert.ToInt32(Size.Substring(iTemp + 1));

        if ((iTemp != -1) && (DenominatorSize == 0))
            DenominatorSize = 2;

        if (DenominatorSize != 0)
            if ((type == "U") || (type == "S"))
                Denominator = Convert.ToInt32(Value.Substring(Value.Length - DenominatorSize, DenominatorSize));

        if (type == "U")
            Numerator = Convert.ToInt64(Value.Substring(0, Value.Length - DenominatorSize));
        if (type == "S")
        {
            Numerator = Convert.ToInt64(Value.Substring(1, Value.Length - DenominatorSize));
            if (Value.Substring(0, 1) == "-")
                MinusSign = -1;
        }
        Numerator = MinusSign * Numerator;
        string Temp = String.Empty;
        Temp = Numerator.ToString();
        Temp += CurrencySeparator;
        if (Denominator < 10)
            Temp += "0";
        Temp += Denominator.ToString();
        return Temp;
    }

    public static string GetRequestOutputItem(int Size)
    {
        // 
        if (RequestOutput == null)
            return "";
        if (RequestOutput.Length == 0)
            return "";
        string Result = "";
        if (RequestOutputCurrentLength + 1 + Size <= RequestOutput.Length)
        {
            Result = RequestOutput.ToString(RequestOutputCurrentLength, Size);
        }
        RequestOutputCurrentLength += Size;
        return Result; 
    }

    public static string GetClientVersion()
    {
        EQGateStatus = -1;

        // Client Version =========================================
        // Определение версии клиента
        int EQGATE_VERSION_SIZE = 22;
        StringBuilder sb = new StringBuilder(EQGATE_VERSION_SIZE);

        try
        {
            EQGateStatus = EQGateClientVersion(sb, sb.Capacity);
        }
        catch
        {
            MessageBox.Show("Error call [EQGateClientVersion]", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        if (EQGateStatus != 0)
        {
            MessageBox.Show(GetEQGateStatus(EQGateStatus), "EQGateStatus", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return "ERROR";
        }
        return sb.ToString();
    }

    public static unsafe bool Connect()
    {
        // Connect =================================================
        // Соединение 
        EQGateStatus = -1;

        uint temp_DSC;
        temp_DSC = 0;

        EQGATE_DESCRIPTOR = 0;

        StringBuilder sbHost = new StringBuilder(EQHost.Length);
        sbHost = sbHost.Append(EQHost);
        try
        {
            EQGateStatus = EQGateConnect(&temp_DSC, sbHost, EQPort);
        }
        catch
        {
            MessageBox.Show("Error call [EQGateConnect]", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        if (EQGateStatus != 0)
        {
            //MessageBox.Show(GetEQGateStatus(EQGateStatus), "EQGateStatus", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        EQGATE_DESCRIPTOR = temp_DSC;

        ServerConnected = true;
        return true;
    }

    public static unsafe string GetServerVersion()
    {
        if (!ServerConnected)
        {
            MessageBox.Show("Небходимо соединиться с сервером", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return "";
        }
        // Server Version =========================================
        // Определение версии сервера 
        EQGateStatus = -1;

        uint temp_DSC;
        temp_DSC = 0;

        temp_DSC = EQGATE_DESCRIPTOR;

        StringBuilder sb = new StringBuilder(EQGATE_VERSION_SIZE);
        try
        {
            EQGateStatus = EQGateServerVersion(&temp_DSC, sb, sb.Capacity);
        }
        catch
        {
            MessageBox.Show("Error call [EQGateServerVersion]", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        if (EQGateStatus != 0)
        {
            MessageBox.Show(GetEQGateStatus(EQGateStatus), "EQGateStatus", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return "ERROR";
        }
        return sb.ToString();
    }

    public static unsafe bool Disconnect()
    {
        if (!ServerConnected)
            return true;
        // Disconnect =========================================
        // Закрытие соединения
        EQGateStatus = -1;

        uint temp_DSC;
        temp_DSC = 0;

        temp_DSC = EQGATE_DESCRIPTOR;

        try
        {
            EQGateStatus = EqGate.EQGateDLL.EQGateDisconnect(&temp_DSC);
        }
        catch
        {
            MessageBox.Show("Error call [EQGateDisconnect]", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        if (EQGateStatus != 0)
        {
            //MessageBox.Show(EqGate.EQGateDLL.GetEQGateStatus(EQGateStatus), "EQGateStatus");
            return false;
        }
        ServerConnected = false;

        return true;
    }

    public static unsafe bool OpenSession(string DB, string User, string Password)
    {
        if (!ServerConnected)
        {
            MessageBox.Show("Небходимо соединиться с сервером", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
//        if (SessionID == "[Сессия закрыта]")
//          return true;

        SessionID = "";
        if (File.Exists(SessionID_path))
        {
            using (StreamReader sr = File.OpenText(SessionID_path))
            {
                SessionID = sr.ToString();
                if (SessionID != "")
                    CloseSession();
            }
        }
        // Open Session =========================================
        // Открытие сессии
        EQGateStatus = -1;

        uint temp_DSC;
        temp_DSC = 0;

        temp_DSC = EQGATE_DESCRIPTOR;
        // строка, заканчивающаяся нулевым символом, содержащая имя вызываемой функции.
        string api = "CRSESSION";
//        string api = "";

        StringBuilder sb_api = new StringBuilder(10);
        sb_api = sb_api.Append(api);
        sb_api = sb_api.Append("\0");

        // строка, заканчивающаяся нулевым символом, содержащая идентификатор сессии, в рамках которой будет осуществлен вызов функции Equation.
        StringBuilder sb_session_id = new StringBuilder(0);
        //sb_session_id = sb_session_id.Append("");
        sb_session_id = sb_session_id.Append("\0");

        // строка, заканчивающаяся нулевым символом, содержащая входные параметры функции Equation.
        //        user = 10
        //        Pass =10
        //        мнемоника блока = 3 - имя блока
        StringBuilder sb_input = new StringBuilder(24);
        sb_input = sb_input.Append(InsertSpaces(User, 10));
        sb_input = sb_input.Append(InsertSpaces(Password, 10));
        sb_input = sb_input.Append(DB);
        sb_input = sb_input.Append("\0");

        // указатель на область памяти, куда будет скопирована информация об ошибках и/или предупреждениях, возникших во время выполнения функции Equation (см. примечание).
        StringBuilder sb_ksm = new StringBuilder(EQGATE_KSM_SIZE);
        // указатель на область памяти, куда будет принята строка, содержащая выходные параметры функции Equation.
        StringBuilder sb_output = new StringBuilder(1024 * 30); // 30 K

        try
        {
            EQGateStatus = EqGate.EQGateDLL.EQGateRequest(
                                &temp_DSC,
                                sb_api,
                                sb_session_id,
                                sb_input,
                                sb_ksm,
                                sb_ksm.Capacity, // размер области памяти в байтах, на которую указывает параметр ksm.
                                sb_output,
                                sb_output.Capacity, // размер области памяти в байтах, на которую указывает параметр output.
                                null, // указатель на область памяти, где содержится управляющая информация для функций, поддерживающих возврат набора (переменной длины) записей (см. примечание). Данный параметр содержит как входные, так и выходные параметры (используется как для чтения, так и для записи).
                                0 // размер области памяти в байтах, на которую указывает параметр mcs.
                                ); ;
        }
        catch
        {
            MessageBox.Show("Error call [EQGateRequest]", "Error Open Session",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        Get_ksm_array(sb_ksm);
        if (EQGateStatus != 0)
        {
            MessageBox.Show(GetEQGateStatus(EQGateStatus), "EQGateStatus", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        RequestOutputCurrentLength = 0;
        SessionID = sb_output.ToString(0, 25);
        if (File.Exists(SessionID_path))
        {
            File.Delete(SessionID_path);
        }
        using (StreamWriter sw = File.CreateText(SessionID_path))
        {
            sw.Write(SessionID);
        }

        return true;
    }

    public static unsafe bool CloseSession()
    {
        if (!ServerConnected)
            return true;
        // Close Session =========================================
        // Закрытие сессии
        EQGateStatus = -1;

        uint temp_DSC;
        temp_DSC = 0;

        temp_DSC = EQGATE_DESCRIPTOR;
        // строка, заканчивающаяся нулевым символом, содержащая имя вызываемой функции.
        string api = "CLSESSION";

        StringBuilder sb_api = new StringBuilder(10);
        sb_api = sb_api.Append(api);
        sb_api = sb_api.Append("\0");

        // строка, заканчивающаяся нулевым символом, содержащая идентификатор сессии, в рамках которой будет осуществлен вызов функции Equation.
        StringBuilder sb_session_id = new StringBuilder(26);
        sb_session_id = sb_session_id.Append(SessionID + "\0");

        // строка, заканчивающаяся нулевым символом, содержащая входные параметры функции Equation.
        //        user = 10
        //        Pass =10
        //        мнемоника блока = 3 - имя блока
        StringBuilder sb_input = new StringBuilder(0);
        //sb_input = sb_input.Append("");

        // указатель на область памяти, куда будет скопирована информация об ошибках и/или предупреждениях, возникших во время выполнения функции Equation (см. примечание).
        StringBuilder sb_ksm = new StringBuilder(EQGATE_KSM_SIZE);
        // указатель на область памяти, куда будет принята строка, содержащая выходные параметры функции Equation.
        StringBuilder sb_output = new StringBuilder(EQGATE_OUTPUT_SIZE); // 30 K

        try
        {
            EQGateStatus = EqGate.EQGateDLL.EQGateRequest(
                                &temp_DSC,
                                sb_api,
                                sb_session_id,
                                sb_input,
                                sb_ksm,
                                sb_ksm.Capacity, // размер области памяти в байтах, на которую указывает параметр ksm.
                                sb_output,
                                sb_output.Capacity, // размер области памяти в байтах, на которую указывает параметр output.
                                null, // указатель на область памяти, где содержится управляющая информация для функций, поддерживающих возврат набора (переменной длины) записей (см. примечание). Данный параметр содержит как входные, так и выходные параметры (используется как для чтения, так и для записи).
                                0 // размер области памяти в байтах, на которую указывает параметр mcs.
                                ); ;
        }
        catch
        {
            MessageBox.Show("Error call [EQGateRequest]", "Error Close Session",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        Get_ksm_array(sb_ksm);
        if (EQGateStatus != 0)
        {
            MessageBox.Show(GetEQGateStatus(EQGateStatus), "EQGateStatus", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        SessionID = "[Сессия закрыта]";
        RequestOutputCurrentLength = 0;
        if (File.Exists(SessionID_path))
        {
            File.Delete(SessionID_path);
        }
        return true;
    }
    
    public static unsafe bool Request(string api, StringBuilder input, string api_type)
    {
        if (!ServerConnected)
        {
            MessageBox.Show("Небходимо соединиться с сервером", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        // EQGateRequest =========================================
        EQGateStatus = -1;

        uint temp_DSC;
        temp_DSC = 0;

        temp_DSC = EQGATE_DESCRIPTOR;

        // строка, заканчивающаяся нулевым символом, содержащая имя вызываемой функции.
        if (api == "")
            return false;

        StringBuilder sb_api = new StringBuilder(7);
        sb_api = sb_api.Append(api);
        sb_api = sb_api.Append("\0");

        // строка, заканчивающаяся нулевым символом, содержащая идентификатор сессии, в рамках которой будет осуществлен вызов функции Equation.
        StringBuilder sb_session_id = new StringBuilder(26);
        sb_session_id = sb_session_id.Append(SessionID);
        sb_session_id = sb_session_id.Append("\0");

        // строка, заканчивающаяся нулевым символом, содержащая входные параметры функции Equation.
        StringBuilder sb_input = new StringBuilder(2);
        sb_input = input;
        sb_input = sb_input.Append("\0");
        //sb_input = input;
        //        user = 10
        //        Pass =10
        //        мнемоника блока = 3 - имя блока

        // указатель на область памяти, куда будет скопирована информация об ошибках и/или предупреждениях, возникших во время выполнения функции Equation (см. примечание).
        StringBuilder sb_ksm = new StringBuilder(EQGATE_KSM_SIZE);
        // указатель на область памяти, куда будет принята строка, содержащая выходные параметры функции Equation.
        StringBuilder sb_output = new StringBuilder(EQGATE_OUTPUT_SIZE); 
        // указатель на область памяти, где содержится управляющая информация для функций, поддерживающих возврат набора (переменной длины) записей (см. примечание). Данный параметр содержит как входные, так и выходные параметры (используется как для чтения, так и для записи).
        
        int msc_size = 0;
        StringBuilder sb_mcs = new StringBuilder(0);
        sb_mcs = null;
        if (api_type == "list")
        {
            msc_size = EQGATE_MCS_SIZE;
            sb_mcs = new StringBuilder(msc_size);
            sb_mcs = sb_mcs.Append(mcs_QueryType.ToString()); // 
            sb_mcs = sb_mcs.Append(EQGateDLL.InsertZero(Convert.ToString(mcs_RecordsRequested), 4)); // 
            sb_mcs = sb_mcs.Append("00000"); // 
            sb_mcs = sb_mcs.Append("\0");
        }

        try
        {
            EQGateStatus = EqGate.EQGateDLL.EQGateRequest(
                                &temp_DSC,
                                sb_api,
                                sb_session_id,
                                sb_input,
                                sb_ksm,
                                sb_ksm.Capacity, // размер области памяти в байтах, на которую указывает параметр ksm.
                                sb_output,
                                sb_output.Capacity, // размер области памяти в байтах, на которую указывает параметр output.
                                sb_mcs, // указатель на область памяти, где содержится управляющая информация для функций, поддерживающих возврат набора (переменной длины) записей (см. примечание). Данный параметр содержит как входные, так и выходные параметры (используется как для чтения, так и для записи).
                                msc_size // размер области памяти в байтах, на которую указывает параметр mcs.
                                ); ;
        }
        catch
        {
            MessageBox.Show("Error call [EQGateRequest]", "Error EQGateRequest",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        Get_ksm_array(sb_ksm);
        if (EQGateStatus != 0)
        {
            //MessageBox.Show(GetEQGateStatus(EQGateStatus), "EQGateStatus", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        RequestOutput = sb_output;
        RequestOutputCurrentLength = 0;

        if (api_type == "list")
        {
            mcs_QueryType = Convert.ToInt32(sb_mcs.ToString(0, 1));
            mcs_RecordsRequested = Convert.ToInt32(sb_mcs.ToString(1, 4));
            mcs_RecordsReturned = Convert.ToInt32(sb_mcs.ToString(5, 4));
            mcs_NotFinished = Convert.ToInt32(sb_mcs.ToString(9, 1));
        }
        return true;
    }

    public EQGateDLL()
	{

	}
}
}