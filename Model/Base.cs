using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Model
{
    public abstract class Base
    {
        [Key]
        public Guid Id { get; private set; } = Guid.NewGuid();
        //public DateTime DataCadastro { get; set; } =  DateTime.Parse(DateTime.Now.ToString(), CultureInfo.CreateSpecificCulture("pt-BR"));
        //public DateTime DataCadastro { get; set; } =  DateTime.ParseExact(DateTime.Now.ToString(),"yyyy-MM-dd HH:mm,ss.fffffff", CultureInfo.CreateSpecificCulture("pt-BR"));
        public DateTime DataCadastro { get; set; } = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified), TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(c => c.DisplayName == "(UTC+03:00) Moscou, São Petersburgo"));

    }
}
