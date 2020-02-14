using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows;
using MaindText.Control;
using MaindText.Model;
using System.Globalization;

namespace MaindText
{
    public static class MTParser
    {
        static public void ReadNewBible(string DataPath)
        {
            if (!File.Exists(DataPath))
                return;

            Stream Str = GetSream(DataPath);

            XmlReaderSettings objReaderConfig = new XmlReaderSettings();
            objReaderConfig.IgnoreComments = true;
            objReaderConfig.IgnoreWhitespace = true;
            objReaderConfig.IgnoreProcessingInstructions = true;
            XmlReader objReader = XmlReader.Create(Str, objReaderConfig);
            while (objReader.Read() && objReader.NodeType != XmlNodeType.Element)
                ; // move to the first element of this XML

            List<List<string>> lstChapter = new List<List<string>>();
            List<string> lstVer = new List<string>();
            int iver = 0;
            while (objReader.Read())
            {
                if(objReader.Name == "strong")
                {
                    lstVer = new List<string>();
                    lstChapter.Add(lstVer);
                    lstVer.Add("");
                    iver = 0;
                    objReader.Read();
                    objReader.Read();
                }
                else
                switch (objReader.Value)
                {
                    case "*":
                    case "+":
                    case "":
                        break;
                    default:
                        if (objReader.Value == (iver+2).ToString() + " ")
                        {
                            lstVer.Add("");
                            iver++;
                        }
                        else
                            lstVer[iver] += objReader.Value;

                        break;
                }
            }
            
            objReader.Close();
        }

        #region Read XML
        static public void ReadAllInFolder(string DataPath)
        {
            if (Directory.Exists(DataPath))
            {
                string[] arrVerses = Directory.GetFiles(DataPath);
                foreach (string sVerse in arrVerses)
                {
                    Read(sVerse);
                }
            }
        }

        static public void Read(string DataPath)
        {
            if (!File.Exists(DataPath))
                return;

            Stream Str = GetSream(DataPath);

            XmlReaderSettings objReaderConfig = new XmlReaderSettings();
            objReaderConfig.IgnoreComments = true;
            objReaderConfig.IgnoreWhitespace = true;
            objReaderConfig.IgnoreProcessingInstructions = true;
            XmlReader objReader = XmlReader.Create(Str, objReaderConfig);
            while (objReader.Read() && objReader.NodeType != XmlNodeType.Element) ; // move to the first element of this XML

            if (!objReader.Name.Equals("MaindText", StringComparison.CurrentCultureIgnoreCase))
                throw new Exception("Invalid XML file or incorrect version");

            if (!objReader.IsEmptyElement)
            {
                while (objReader.Read())
                {
                    //if it's the end Element
                    if (objReader.NodeType == XmlNodeType.EndElement && objReader.Name.Equals("MaindText", StringComparison.CurrentCultureIgnoreCase))
                        break;

                    switch (objReader.Name)
                    {
                        case "Patches":
                            ReadPatches(objReader);
                            break;
                        case "Settings":
                            ReadSettings(objReader);
                            break;
                        default:
                            throw new Exception("Unexpected element: '" + objReader.Name + "'");
                    }
                }
            }
            objReader.Close();
        }

        private static void ReadSettings(XmlReader objReader)
        {
            if (!objReader.IsEmptyElement)
            {
                //Initializa settings
                MTControler.Settings = new MTSettingsModel();

                while (objReader.Read())
                {
                    //if it's the end Element
                    if (objReader.NodeType == XmlNodeType.EndElement && objReader.Name.Equals("Settings", StringComparison.CurrentCultureIgnoreCase))
                        break;                   

                    switch (objReader.Name)
                    {
                        case "AutoTest":
                            MTControler.Settings.AutoTestEnabled = Convert.ToBoolean(objReader.GetAttribute("Enabled"));
                            MTControler.Settings.AutoTestInterval = Convert.ToDouble(objReader.GetAttribute("Interval"));
                            MTControler.Settings.TestTypeConfig = (TestTypeConfig)Enum.Parse(typeof(TestTypeConfig), objReader.GetAttribute("TypeTest"), true);
                            MTControler.Settings.AllowApply = (AllowApplyConfig)Enum.Parse(typeof(AllowApplyConfig), objReader.GetAttribute("AllowApply"), true);
                            MTControler.Settings.EnSortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), objReader.GetAttribute("SortOrder"));
                            MTControler.Settings.Stg1to2 = Convert.ToDouble(objReader.GetAttribute("Stage_1to2"));
                            MTControler.Settings.Stg2to3 = Convert.ToDouble(objReader.GetAttribute("Stage_2to3"));
                            break;

                        default:
                            throw new Exception("Unexpected element: '" + objReader.Name + "'");
                    }
                }
            }
        }

        private static void ReadPatches(XmlReader objReader)
        {
            if (!objReader.IsEmptyElement)
            {
                while (objReader.Read())
                {
                    //if it's the end Element
                    if (objReader.NodeType == XmlNodeType.EndElement && objReader.Name.Equals("Patches", StringComparison.CurrentCultureIgnoreCase))
                        break;

                    switch (objReader.Name)
                    {
                        case "Patch":
                            ReadTestes(objReader);
                            break;

                        default:
                            throw new Exception("Unexpected element: '" + objReader.Name + "'");
                    }
                }
            }
        }

        private static void ReadTestes(XmlReader objReader)
        {
            //Create MTPatchModel
            string input = objReader.GetAttribute("Created");
            CultureInfo provider = new CultureInfo("pt-PT");
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(input, provider);
            }
            catch (System.Exception ex)
            {
                string e = ex.Message;
                dateTime = Convert.ToDateTime(input);
            }

            MTPatchModel objPatch = new MTPatchModel(objReader.GetAttribute("Adress"), dateTime, objReader.GetAttribute("GUID"), Convert.ToBoolean(objReader.GetAttribute("TestIsEnabled")));
            MTControler.TextManager.AddPatch(objPatch);

            if (!objReader.IsEmptyElement)
            {
                while (objReader.Read())
                {
                    //if it's the end Element
                    if (objReader.NodeType == XmlNodeType.EndElement && objReader.Name.Equals("Patch", StringComparison.CurrentCultureIgnoreCase))
                        break;

                    switch (objReader.Name)
                    {
                        case "Test":
                            //Create MTTestModel
                            input = objReader.GetAttribute("Date");
                            try
                            {
                                dateTime = Convert.ToDateTime(input, provider);
                            }
                            catch (System.Exception ex)
                            {
                                string e = ex.Message;
                                dateTime = Convert.ToDateTime(input);
                            }

                            TestType enType = TestType.AskAdrees;
                            string sType = objReader.GetAttribute("Type");
                            if (sType != null)
                                enType = (TestType)Enum.Parse(typeof(TestType), sType, true);
                            MTStructTest strTest;
                            strTest.Date = dateTime;
                            strTest.WasCorrect = Convert.ToBoolean(objReader.GetAttribute("WasCorrect"));
                            strTest.Type = enType;
                            //MTTestModel objTest = new MTTestModel(dateTime, Convert.ToBoolean(objReader.GetAttribute("WasCorrect")), enType);
                            objPatch.lstTest.Add(strTest);
                            break;

                        case "Applycation":
                            //Create MTPatchModel
                            objPatch.MgrApply.LstApply.Add(objReader.GetAttribute("Text"));
                            if (objPatch.MgrApply.SelIndex == -1)
                                objPatch.MgrApply.SelIndex = 0;
                            break;

                        default:
                            throw new Exception("Unexpected element: '" + objReader.Name + "'");
                    }
                }
            }
        }

        private static Stream GetSream(String sFileName)
        {
            StreamReader reader = new StreamReader(sFileName, Encoding.GetEncoding("iso8859-1"));
            string content = reader.ReadToEnd();
            reader.Close();

            return new MemoryStream(Encoding.Default.GetBytes(content));
        }

        #endregion

        #region Write XML

        public static void WriteSettings()
        {
            try
            {
                string sSettingsPath = MTControler.UserPath + "Settings.xml";

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.GetEncoding("iso8859-1");
                settings.Indent = true;

                using (XmlWriter writer = XmlWriter.Create(sSettingsPath, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("MaindText");
                    writer.WriteStartElement("Settings");
                    writer.WriteStartElement("AutoTest");

                    writer.WriteAttributeString("Enabled", MTControler.Settings.AutoTestEnabled.ToString());
                    writer.WriteAttributeString("Interval", MTControler.Settings.AutoTestInterval.ToString());
                    writer.WriteAttributeString("TypeTest", MTControler.Settings.TestTypeConfig.ToString());
                    writer.WriteAttributeString("AllowApply", MTControler.Settings.AllowApply.ToString());
                    writer.WriteAttributeString("SortOrder", MTControler.Settings.EnSortOrder.ToString());
                    writer.WriteAttributeString("Stage_1to2", MTControler.Settings.Stg1to2.ToString());
                    writer.WriteAttributeString("Stage_2to3", MTControler.Settings.Stg2to3.ToString());

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(string.Format("O seguinte erro foi encontrado no arquivo selecionado: \n{0}", ex.Message), "Arquivo invalido", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void WritePatch(MTPatchModel objPatch)
        {
            try
            {
                string sPatchPath = objPatch.FilePatch;

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.GetEncoding("iso8859-1");
                settings.Indent = true;                

                using (XmlWriter writer = XmlWriter.Create(sPatchPath, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("MaindText");
                    writer.WriteStartElement("Patches");

                    CultureInfo provider = new CultureInfo("pt-PT");
                    writer.WriteStartElement("Patch");
                    writer.WriteAttributeString("Adress", objPatch.ShortAdrees);
                    writer.WriteAttributeString("Created", objPatch.Created.ToString(provider));
                    writer.WriteAttributeString("GUID", objPatch.GUID);
                    writer.WriteAttributeString("TestIsEnabled", objPatch.TestIsEnabled.ToString());

                    foreach (MTStructTest iTest in objPatch.lstTest)
                    {
                        writer.WriteStartElement("Test");
                        writer.WriteAttributeString("Date", iTest.Date.ToString(provider));
                        writer.WriteAttributeString("Type", iTest.Type.ToString());
                        writer.WriteAttributeString("WasCorrect", iTest.WasCorrect.ToString());
                        writer.WriteEndElement();
                    }

                    foreach (String iText in objPatch.MgrApply.LstApply)
                    {
                        writer.WriteStartElement("Applycation");
                        writer.WriteAttributeString("Text", iText);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(string.Format("O seguinte erro foi encontrado no arquivo selecionado: \n{0}", ex.Message), "Arquivo invalido", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //-------------DATA-----------------
        public static void WriteDataTest( MTPatchModel objPatch, MTTestModel objTest)
        {
            TextWriter tw = new StreamWriter(MTControler.UserPath + "DataTest.txt", true);
            WriteDataTest(tw, objPatch, objTest);
            tw.Close();
        }

        public static void WriteDataTest(TextWriter tw, MTPatchModel objPatch, MTTestModel objTest)
        {
            tw.WriteLine(objPatch.GUID);
            tw.WriteLine(objPatch.Adrees);
            tw.WriteLine(objTest.Date);
            tw.WriteLine(objTest.Type.ToString());
            tw.WriteLine(objTest.WasCorrect.ToString());
            tw.WriteLine("##################");
        }

        #endregion
    }
}
