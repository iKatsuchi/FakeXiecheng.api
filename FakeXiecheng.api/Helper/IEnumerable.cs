using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace FakeXiecheng.api.Helper
{
    public static class IEnumerable
    {
        public static IEnumerable<ExpandoObject> ShapeData<TSource>(
            this IEnumerable<TSource> source,
            string fields)
        {
            if(source==null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var expandoObjectList = new List<ExpandoObject>();

            //避免在列表中遍历数据，创建一个属性信息列表
            var propertyInfoList = new List<PropertyInfo>();

            if(string.IsNullOrWhiteSpace(fields))
            {
                // 希望返回动态类型对象ExpandoObject所有的属性
                var propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.IgnoreCase
                    | BindingFlags.Public | BindingFlags.Instance);

                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                var fieldsAfterSplit = fields.Split(',');

                foreach(var field in fieldsAfterSplit)
                {
                    // 去掉多余的空格，获得属性名称
                    var propertName = field.Trim();

                    var propertyInfo = typeof(TSource)
                   .GetProperty(propertName, BindingFlags.IgnoreCase
                   | BindingFlags.Public | BindingFlags.Instance);

                    if(propertyInfo == null)
                    {
                        throw new Exception($"属性{propertName}找不打破" + $"{typeof(TSource)}");
                    }

                    propertyInfoList.Add(propertyInfo);
                }
            }

            foreach(TSource sourceObject in source)
            {
                //创建动态类型对象，创建数据塑形对象
                var dataShapedObject = new ExpandoObject();

                foreach(var propertyInfo in propertyInfoList)
                {
                    // 获得对应属性的真实数据
                    var propertyValue = propertyInfo.GetValue(sourceObject);

                    ((IDictionary<string, object>)dataShapedObject)
                        .Add(propertyInfo.Name, propertyValue);
                }
                expandoObjectList.Add(dataShapedObject);
            }
            return expandoObjectList;
        }
    }
}
