using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;

namespace Yona
{
    public static class JsonConverter
    {
        public static string SerializeJsonUsingJavaScipt(object p)
        {
            var jss = new JavaScriptSerializer();
            return jss.Serialize(p);
        }

        public static T DeserializeJsonUsingJavaScript<T>(string json)
        {
            var jss = new JavaScriptSerializer();
            jss.MaxJsonLength = Int32.MaxValue;
            T obj = jss.Deserialize<T>(json);
            return obj;
        }


        public static string DataTableToJsonWithJavaScriptSerializer(DataTable table)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            jsSerializer.MaxJsonLength = Int32.MaxValue;
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    childRow.Add(col.ColumnName, row[col]);
                }
                parentRow.Add(childRow);
            }
            return jsSerializer.Serialize(parentRow);
        }



        public static DataTable ArrayListToDataTable(ArrayList list)
        {
            DataTable result = new DataTable();
            if (list.Count == 0)
                return result;

            Dictionary<string, object> firstItem = (Dictionary<string, object>)list[0];

            result.Columns.AddRange(
                firstItem.Select(r => new DataColumn(r.Key, typeof(string))).ToArray()
            );

            foreach (var items in list)
            {
                Dictionary<string, object> it = (Dictionary<string, object>)items;

                result.Rows.Add(it.Values.ToArray());
            }

            return result;
        }


    }
}
