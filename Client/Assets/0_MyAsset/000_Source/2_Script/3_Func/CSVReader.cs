using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
 
public class CSVReader
{
    static string m_strSplit = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string m_strLineSplit = @"\r\n|\n\r|\n|\r";
    static char[] m_strTrimChars = { '\"' };

    public static List<Dictionary<string, object>> Read(string _strfile)
    {
        var list = new List<Dictionary<string, object>>();
        TextAsset data = ResourcesMng.Instance.Get_Obj("CSV/" + _strfile) as TextAsset;

        var lines = Regex.Split(data.text, m_strLineSplit);

        if (lines.Length <= 1)
        {
            return list;
        }

        var header = Regex.Split(lines[0], m_strSplit);
        for (var i = 1; i < lines.Length; i++)
        {

            var values = Regex.Split(lines[i], m_strSplit);
            if (values.Length == 0 || values[0] == "")
            {
                continue;
            }

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(m_strTrimChars).TrimEnd(m_strTrimChars).Replace("\\", "");
                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        return list;
    }
}
