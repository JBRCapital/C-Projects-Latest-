using System.Text;
using System.Collections.Generic;

namespace ClicksendHelper
{
    public class ClickSendValues
    {
        public string address_name { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string address_city { get; set; }
        public string address_state { get; set; }
        public string address_postal_code { get; set; }
        public string address_country { get; set; }

        public string template_used { get; set; } //1
        public string colour { get; set; } //1
        public string duplex { get; set; } //0
        public string priority_post { get; set; } //1

        public override string ToString()
        {
            // Build up the string data.
            StringBuilder addressBuilder = new StringBuilder();
            if (address_name.Length > 0) { addressBuilder.AppendLine(address_name); }
            if (address_line_1.Length > 0) { addressBuilder.AppendLine(address_line_1); }
            if (address_line_2.Length > 0) { addressBuilder.AppendLine(address_line_2); }
            if (address_city.Length > 0) { addressBuilder.AppendLine(address_city); }
            if (address_state.Length > 0) { addressBuilder.AppendLine(address_state); }
            if (address_postal_code.Length > 0) { addressBuilder.AppendLine(address_postal_code); }
            if (address_country.Length > 0) { addressBuilder.AppendLine(address_country); }

            return addressBuilder.ToString();
        }
    }
}
