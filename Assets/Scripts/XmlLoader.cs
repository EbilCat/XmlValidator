using UnityEngine;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

/*
    bookstore	Selects all nodes with the name "bookstore"
    /bookstore	Selects the root element bookstore
    Note: If the path starts with a slash ( / ) it always represents an absolute path to an element!

    bookstore/book	Selects all book elements that are children of bookstore
    //book	Selects all book elements no matter where they are in the document
    bookstore//book	Selects all book elements that are descendant of the bookstore element, no matter where they are under the bookstore element
    //@lang	Selects all attributes that are named lang
*/


public class XmlLoader : MonoBehaviour
{
    [ContextMenu("Run")]
    private void Start()
    {
        string xmlContents = File.ReadAllText(Application.dataPath + "/../Sample.xml");
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContents);
        
        bool pass = AssertCount(xmlDoc, new List<string> { "book", "self::*//computer" }, 1);
        // bool pass = AssertValue(xmlDoc, new List<string> { "//computer/name" }, new List<string> { "Computer One", "Unnamed" });
        // bool pass = AssertUnique(xmlDoc, new List<string> { "book", "title" });
        // bool pass = AssertUnique(xmlDoc, new List<string> { "child::book", "child::publish_date" });
        // bool pass = AssertRegex(xmlDoc, new List<string> { "book", "ipaddress" }, @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$");
        // bool pass = AssertRegex(xmlDoc, new List<string> { "//ipaddress | //radioethernetip | //netaddress" }, @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$");

        Debug.Log("Result: " + pass);
    }


//*====================
//* ASSERTS
//*====================
    private bool AssertCount(XmlDocument xmlDoc, List<string> xPaths, int count)
    {
        return Assert
        (
            xmlDoc.ChildNodes,
            (nodesToValidate) =>
            {
                bool pass = nodesToValidate.Count == count;
                if (pass == false)
                {
                    Debug.Log($"Expected count {count} but actual count is {nodesToValidate.Count}");
                }
                return pass;
            },
            xPaths
        );
    }

    private bool AssertUnique(XmlDocument xmlDoc, List<string> xPaths)
    {
        return Assert
        (
            xmlDoc.ChildNodes,
            (nodesToValidate) =>
            {
                for (int i = 0; i < nodesToValidate.Count; i++)
                {
                    string currentText = nodesToValidate[i].InnerText;

                    for (int j = i + 1; j < nodesToValidate.Count; j++)
                    {
                        string innerText = nodesToValidate[j].InnerText;
                        if (currentText == innerText)
                        {
                            string fullPath = GetFullPath(nodesToValidate[i]);
                            Debug.Log($"{fullPath} contains duplicate value of {innerText}");
                            return false;
                        }
                    }
                }
                return true;
            },
            xPaths
        );
    }


    private bool AssertRegex(XmlDocument xmlDoc, List<string> xPaths, string regex)
    {
        return Assert
        (
            xmlDoc.ChildNodes,
            (nodesToValidate) =>
            {
                for (int i = 0; i < nodesToValidate.Count; i++)
                {
                    string currentText = nodesToValidate[i].InnerText;

                    bool match = Regex.IsMatch(currentText, regex);
                    if (match == false)
                    {
                        string fullPath = GetFullPath(nodesToValidate[i]);
                        Debug.Log($"{fullPath} contains invalid value of {currentText}");
                        return false;
                    }
                }
                return true;
            },
            xPaths
        );
    }


    private bool AssertValue(XmlDocument xmlDoc, List<string> xPaths, List<string> permissableValues)
    {
        return Assert
        (
            xmlDoc.ChildNodes,
            (nodesToValidate) =>
            {
                for (int i = 0; i < nodesToValidate.Count; i++)
                {
                    string currentText = nodesToValidate[i].InnerText;
                    if (permissableValues.Contains(currentText) == false)
                    {
                        string fullPath = GetFullPath(nodesToValidate[i]);
                        Debug.Log($"{currentText} is not a permissable value for {fullPath}");
                        return false;
                    }
                }
                return true;
            },
            xPaths
        );
    }



//*====================
//* RECURSION
//*====================
    private bool Assert(XmlNodeList nodes, Predicate<XmlNodeList> validator, List<string> xPathGroups, int depth = 0)
    {
        bool pass = true;

        string xPath = xPathGroups[depth];

        for (int i = 0; i < nodes.Count; i++)
        {
            XmlNode node = nodes[i];
            XmlNodeList subNodes = node.SelectNodes(xPath);
            if (subNodes == null) continue;

            if (depth == xPathGroups.Count - 1)
            {
                pass &= validator(subNodes);
            }
            else
            {
                pass &= Assert(subNodes, validator, xPathGroups, depth + 1);
            }
        }

        return pass;
    }


//*====================
//* UTILS
//*====================
    private string GetFullPath(XmlNode xmlNode, string str = "")
    {
        str = xmlNode.Name + str;

        if (xmlNode.ParentNode != null)
        {
            return GetFullPath(xmlNode.ParentNode, "." + str);
        }
        return str;
    }
}
