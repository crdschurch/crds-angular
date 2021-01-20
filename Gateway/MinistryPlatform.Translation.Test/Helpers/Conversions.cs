using System.Collections.Generic;
using System.Linq;
using MinistryPlatform.Translation.Extensions;

namespace MinistryPlatform.Translation.Test.Helpers
{
    public static class Conversions
    {
        public static List<int> BuildIntArrayFromKeyValue(List<Dictionary<string, object>> dictionaryList, string key)
        {
            //List<int> keyValues = new List<int>();

            //foreach (var item in dictionaryList)
            //{
            //    keyValues.Add(int.Parse(item[key].ToString()));
            //}

            //return keyValues;
            return dictionaryList.Select(item => item.ToInt(key)).ToList();
        }
    }
}
