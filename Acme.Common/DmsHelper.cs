using Newtonsoft.Json;
using ServiceStack;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Acme.Common
{
    public static class DmsHelper
    {
        public static string ExtractParameterToJson(string dmsFile)
        {
            var result = "";
            var dict = new Dictionary<string, object>();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(dmsFile);
                var node = xmlDoc.SelectSingleNode("/NodeCollection/Parameters");


                if (node?.ChildNodes != null)
                {
                    for (var i = 0; i < node.ChildNodes.Count; i++)
                    {
                        var name = node.ChildNodes[i].SelectSingleNode("Name")?.InnerText;
                        var value = node.ChildNodes[i].SelectSingleNode("ParaValue")?.InnerText;
                        if (name != null && !name.EndsWith("_"))
                        {
                            dict.Add(name, value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            result = dict.ToJson().IndentJson();
            return result;
        }

        public static bool UpdateParameterFromJson(string dmsFile, string jsonParameter)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(dmsFile);
                var node = xmlDoc.SelectSingleNode("/NodeCollection/Parameters");

                dynamic parameterList = JsonConvert.DeserializeObject(jsonParameter);

                if (node != null)
                {
                    for (var i = 0; i < node.ChildNodes.Count; i++)
                    {
                        var name = node.ChildNodes[i].SelectSingleNode("Name")?.InnerText;
                        var value = node.ChildNodes[i].SelectSingleNode("ParaValue")?.InnerText;

                        foreach (var parameter in parameterList)
                        {
                            if (name != null && (parameter.Name as string)?.ToLower() == name.ToLower())
                            {
                                node.ChildNodes[i].SelectSingleNode("ParaValue").InnerText = value;
                            };
                        }
                    }
                }

                xmlDoc.Save(dmsFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }
    }
}
