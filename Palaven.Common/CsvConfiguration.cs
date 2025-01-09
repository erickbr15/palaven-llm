using System.Globalization;
using System.Text;

namespace Palaven.Common;

public class CsvConfiguration
{
    public bool HasHeaderRecord { get; set; }
    public Encoding Encoding { get; set; } = default!;
    public CultureInfo CultureInfo { get; set; } = default!;
}
