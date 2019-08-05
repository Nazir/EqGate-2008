using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace EQGateAPI
{
    public class EQGateClassAPI
    {
        public static string FileXML;
        public static XmlDocument xd;

        public static bool InitXML(string FileName)
        {
            FileXML = FileName;
            if (FileXML == "" )
                FileXML = @".\EQGateAPI.xml";
            if (!File.Exists(FileXML))
                return false;

            xd = new XmlDocument();
            xd.Load(FileXML);
            return true;
        }

        public static string GetAPIAttribute(string api, string AttributeName)
        {
            XmlNode xn = xd.DocumentElement.SelectSingleNode("//EQGate");
            for (int iCounter = 0; iCounter < xn.ChildNodes.Count; iCounter++)
            {
                if (xn.ChildNodes[iCounter].Attributes["name"].Value == api)
                    return xn.ChildNodes[iCounter].Attributes[AttributeName].Value;
            }
            return "Error: Attribute [" + AttributeName + "] not found!";
        }

        public static int GetFieldsCount(string api, string Structure)
        {
            if (Structure == "")
                Structure = "output";

            int Result = 0;

            XmlNode xn = xd.DocumentElement.SelectSingleNode("//EQGate");
            for (int iCounter = 0; iCounter < xn.ChildNodes.Count; iCounter++)
            {
                if (xn.ChildNodes[iCounter].Attributes["name"].Value == api)
                {
                    XmlNode xn_structure = xn.ChildNodes[iCounter].SelectSingleNode(Structure);
                    for (int i = 0; i < xn_structure.ChildNodes.Count; i++)
                    {
                        XmlNode xn_field = xn_structure.ChildNodes[i];
                        if (xn_field.Name == "field")
                            Result += 1;
                    }
                }
            }
            return Result;
        }

        public static int GetFieldsSize(string api, string Structure)
        {
            if (Structure == "")
                Structure = "output";

            int Result = 0;

            XmlNode xn = xd.DocumentElement.SelectSingleNode("//EQGate");
            for (int iCounter = 0; iCounter < xn.ChildNodes.Count; iCounter++)
            {
                if (xn.ChildNodes[iCounter].Attributes["name"].Value == api)
                {
                    XmlNode xn_structure = xn.ChildNodes[iCounter].SelectSingleNode(Structure);
                    for (int i = 0; i < xn_structure.ChildNodes.Count; i++)
                    {
                        XmlNode xn_field = xn_structure.ChildNodes[i];

                        for (int j = 0; j < xn_field.ChildNodes.Count; j++)
                        {
                            XmlNode xn_values = xn_field.ChildNodes[j];
                            if (xn_values.Name == "size")
                                Result += Convert.ToInt32(Convert.ToDouble(xn_values.InnerText));
                        }
                    }
                }
            }
            return Result;
        }

        public static string GetFieldValue(string api, string Structure, int FieldNumber, string ValueName, string FiledNameAttr)
        {
            if (api == String.Empty)
                return "";

            if (Structure == "")
                Structure = "output";

            XmlNode xn = xd.DocumentElement.SelectSingleNode("//EQGate");
            for (int iCounter = 0; iCounter < xn.ChildNodes.Count; iCounter++) 
            {
                if (xn.ChildNodes[iCounter].Attributes["name"].Value == api)
                {
                    XmlNode xn_structure = xn.ChildNodes[iCounter].SelectSingleNode(Structure);
                    for (int i = 0; i < xn_structure.ChildNodes.Count; i++) 
                    {
                        XmlNode xn_field = xn_structure.ChildNodes[i];

                        bool bFind = false;

                        if (FiledNameAttr == "")
                        {
                            if (FieldNumber == i)
                                bFind = true;
                        }
                        else
                        {
                            if (xn_field.Attributes["name"].Value == FiledNameAttr)
                                bFind = true;
                        }
                        if (bFind)
                        {
                            for (int j = 0; j < xn_field.ChildNodes.Count; j++)
                            {
                                XmlNode xn_values = xn_field.ChildNodes[j];
                                if (xn_values.Name == ValueName)
                                    return xn_values.InnerText;
                            }
                        }
                    }
                    break;
                }
            }
            return ""; 
        }

        public static string GetFieldAttribute(string api, string AttributeName, string Structure, int FieldNumber)
        {
            if (api == String.Empty)
                return "";
            if (Structure == "")
                Structure = "output";

            if (AttributeName == String.Empty)
                return "";

            if (FieldNumber < 0)
                return "";

            XmlNode xn = xd.DocumentElement.SelectSingleNode("//EQGate");
            for (int iCounter = 0; iCounter < xn.ChildNodes.Count; iCounter++)
            {
                if (xn.ChildNodes[iCounter].Attributes["name"].Value == api)
                {
                    XmlNode xn_structure = xn.ChildNodes[iCounter].SelectSingleNode(Structure);
                    for (int i = 0; i < xn_structure.ChildNodes.Count; i++)
                    {
                        XmlNode xn_field = xn_structure.ChildNodes[i];

                        if (FieldNumber == i)
                        {
                            if (xn_field.Attributes.Count > 0)
                            {
                                return xn_field.Attributes[AttributeName].InnerText;
                            }
                        }
                    }
                    break;
                }
            }
            return "";
        }
    }
}
