using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace EqGate
{
    class EqGateConfig
    {
        public static string FileXML;
        public static XmlDocument xd;
        public static string RootNode = "//Config";
        public static string Slash = "/";

        public static bool InitXML(string FileName)
        {
            FileXML = FileName;
            if (FileXML == "")
                FileXML = "./EqGate.xml";
            if (!File.Exists(FileXML))
                return false;

            xd = new XmlDocument();
            xd.Load(FileXML);
            return true;
        }

        public static string GetRootAttribute(string AttributeName)
        {
            try
            {
                XmlNode xn = xd.DocumentElement.SelectSingleNode(RootNode);
                return xn.Attributes[AttributeName].Value;
            }
            catch
            {
                return "Error: Attribute [" + AttributeName + "] not found!";
            }
        }

        public static string GetValue(string ValueName, string Node)
        {
            if (Node != "")
                Node += Slash;

            string Result = "";

            XmlNode xn = xd.DocumentElement.SelectSingleNode(RootNode + Slash + Node + ValueName);
            Result = xn.InnerText;
            if (xn.HasChildNodes)
            {
                if (xn.FirstChild.NodeType == XmlNodeType.CDATA)
                    Result = xn.FirstChild.InnerText;
            }
            return Result;
        }

        public static string GetValue(string ValueName, string Node, int Index)
        {
            /* if (Node != "")
                Node += Slash; //*/

            string Result = String.Empty;

            XmlNode xn = xd.DocumentElement.SelectSingleNode(RootNode + Slash + Node);
            if (xn.HasChildNodes)
            {
                if (ValueName == String.Empty)
                {
                    if (xn.ChildNodes[Index].NodeType == XmlNodeType.CDATA)
                        Result = xn.ChildNodes[Index].FirstChild.InnerText;
                    else
                        Result = xn.ChildNodes[Index].InnerText;
                }
                else
                if (xn.ChildNodes[Index].HasChildNodes)
                {
                    for (int iCounter = 0; iCounter < xn.ChildNodes[Index].ChildNodes.Count; iCounter++)
                    {
                        if (xn.ChildNodes[Index].ChildNodes[iCounter].Name == ValueName)
                        {
                            if (xn.ChildNodes[Index].ChildNodes[iCounter].NodeType == XmlNodeType.CDATA)
                                Result = xn.ChildNodes[Index].ChildNodes[iCounter].FirstChild.InnerText;
                            else
                                Result = xn.ChildNodes[Index].ChildNodes[iCounter].InnerText;
                            break;
                        }
                    }
                }
            }
            return Result;
        }

        public static string GetValue(string ValueName, string Node, string AttributeName, string AttributeValue)
        {
            /*if (Node != "")
                Node += Slash; //*/

            string Result = "";

            XmlNode xn = xd.DocumentElement.SelectSingleNode(RootNode + Slash + Node);
            if (xn.HasChildNodes)
            for (int iCounter = 0; iCounter < xn.ChildNodes.Count; iCounter++)
            {
                if (xn.ChildNodes[iCounter].Name == ValueName)
                {
                    if (xn.ChildNodes[iCounter].Attributes[AttributeName].InnerText == AttributeValue)
                    {
                        if (xn.ChildNodes[iCounter].NodeType == XmlNodeType.CDATA)
                            Result = xn.ChildNodes[iCounter].FirstChild.InnerText;
                        else 
                            Result = xn.ChildNodes[iCounter].InnerText;
                        break;
                    }
                }
            }
            return Result;
        }

        public static string GetChildValue(string ChildValueName, string Node, string AttributeName, string AttributeValue)
        {
            /*if (Node != "")
                Node += Slash;*/

            string Result = "";

            XmlNode xn = xd.DocumentElement.SelectSingleNode(RootNode + Slash + Node);
            if (xn.HasChildNodes)
                for (int iCounter = 0; iCounter < xn.ChildNodes.Count; iCounter++)
                {
                    if (xn.ChildNodes[iCounter].Name == ChildValueName)
                    {
                        if (xn.Attributes[AttributeName].InnerText == AttributeValue)
                        {
                            if (xn.ChildNodes[iCounter].NodeType == XmlNodeType.CDATA)
                                Result = xn.ChildNodes[iCounter].FirstChild.InnerText;
                            else
                                Result = xn.ChildNodes[iCounter].InnerText;
                            return Result;
                        }
                    }
                }
            return Result;
        }

        public static int GetChildNodesCount(string ValueName, string Node)
        {
            if (Node != "")
                Node += Slash;

            XmlNode xn = xd.DocumentElement.SelectSingleNode(RootNode + Slash + Node + ValueName);
            if (xn.HasChildNodes)
            {
                if (xn.FirstChild.NodeType != XmlNodeType.CDATA)
                    return xn.ChildNodes.Count;
            }
            return 0;
        }

        public static int GetIntValue(string ValueName, string Node)
        {
            string Result = GetValue(ValueName, Node);
            if (Result == "")
                Result = "-1";
            return Convert.ToInt32(Result);
        }

        public static bool GetBooleanValue(string ValueName, string Node)
        {
            string Result = GetValue(ValueName, Node);
            if (Result == "")
                Result = "-1";
            return Convert.ToBoolean(Result);
        }

        public static string GetAttribute(string AttributeName, string ValueName, string Node)
        {
            if (Node != "")
                Node += Slash;

            string Result = "";

            XmlNode xn = xd.DocumentElement.SelectSingleNode(RootNode + Slash + Node + ValueName);
            if (xn.Attributes.Count != 0)
                Result = xn.Attributes[AttributeName].InnerText;

            return Result;
        }

        public static string GetAttribute(string AttributeName, string ValueName, string Node, int Index)
        {
            /* if (Node != "")
                Node += Slash; //*/

            string Result = String.Empty;

            XmlNode xn = xd.DocumentElement.SelectSingleNode(RootNode + Slash + Node);
            if (xn.HasChildNodes)
            {
                if (ValueName == String.Empty)
                {
                    Result = xn.ChildNodes[Index].Attributes[AttributeName].Value;
                }
                else
                    if (xn.ChildNodes[Index].HasChildNodes)
                    {
                        for (int iCounter = 0; iCounter < xn.ChildNodes[Index].ChildNodes.Count; iCounter++)
                        {
                            if (xn.ChildNodes[Index].ChildNodes[iCounter].Name == ValueName)
                            {
                                Result = xn.ChildNodes[Index].ChildNodes[iCounter].Attributes[AttributeName].Value;
                                break;
                            }
                        }
                    }
            }
            return Result;            /*
            for (int iCounter = 0; iCounter < xn.ChildNodes.Count; iCounter++)
            {
                if (xn.ChildNodes[iCounter].Attributes["name"].Value == api)
                    return xn.ChildNodes[iCounter].Attributes[AttributeName].Value;
            }*/
            //return "Error: Attribute [" + AttributeName + "] not found!";
        }

        public static bool GetBooleanAttribute(string AttributeName, string ValueName, string Node)
        {
            if (Node != "")
                Node += Slash;

            string Result = "";

            XmlNode xn = xd.DocumentElement.SelectSingleNode(RootNode + Slash + Node + ValueName);
            if (xn.Attributes.Count == 0)
                return false;
            else
                Result = xn.Attributes[AttributeName].InnerText;

            return Convert.ToBoolean(Result);
        }

        public static string GetFieldsValue(string ProcedureName, string FieldName, string ValueName)
        {
            string Result = "";

            XmlNode xn = xd.DocumentElement.SelectSingleNode(RootNode);
            if (xn.HasChildNodes)
            for (int iCounter = 0; iCounter < xn.ChildNodes.Count; iCounter++)
            {
                if (xn.ChildNodes[iCounter].Name == "Procedure")
                {
                    if (xn.ChildNodes[iCounter].Attributes["name"].Value == ProcedureName)
                    {
                        XmlNode xn_fileds = xn.ChildNodes[iCounter].SelectSingleNode("ResultFileds");
                        for (int i = 0; i < xn_fileds.ChildNodes.Count; i++)
                        {
                            XmlNode xn_field = xn_fileds.ChildNodes[i];
                            if ((xn_field.Name == "Field") && (xn_field.Attributes["name"].Value == FieldName))
                            for (int j = 0; j < xn_field.ChildNodes.Count; j++)
                            {
                                XmlNode xn_values = xn_field.ChildNodes[j];
                                if (xn_values.Name == ValueName)
                                {
                                    Result = xn_values.InnerText;
                                    return Result;
                                }
                            }
                        }
                    }
                }
            }
            return Result;
        }
    }
}
