using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppThongKeDiemLmao_2._0.Entities
{
    public class StudentDataMap : ClassMap<StudentData>
    {
        public StudentDataMap()
        {
            Map(m => m.studentCode).Index(0);
            Map(m => m.toan).Index(1).TypeConverter<OptionallyEmptyDoubleConverter>();
            Map(m => m.van).Index(2).TypeConverter<OptionallyEmptyDoubleConverter>();
            Map(m => m.ly).Index(3).TypeConverter<OptionallyEmptyDoubleConverter>();
            Map(m => m.sinh).Index(4).TypeConverter<OptionallyEmptyDoubleConverter>();
            Map(m => m.ngoaiNgu).Index(5).TypeConverter<OptionallyEmptyDoubleConverter>();
            Map(m => m.hoa).Index(7).TypeConverter<OptionallyEmptyDoubleConverter>();
            Map(m => m.su).Index(8).TypeConverter<OptionallyEmptyDoubleConverter>();
            Map(m => m.dia).Index(9).TypeConverter<OptionallyEmptyDoubleConverter>();
            Map(m => m.gdcd).Index(10).TypeConverter<OptionallyEmptyDoubleConverter>();
            Map(m => m.year).Index(6);
            Map(m => m.maTinh).Index(11);
        }
    }
}
