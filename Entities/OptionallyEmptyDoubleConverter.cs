using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppThongKeDiemLmao_2._0.Entities
{
    public class OptionallyEmptyDoubleConverter : DoubleConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0.0;
            }

            return base.ConvertFromString(text, row, memberMapData);
        }
    }
}
