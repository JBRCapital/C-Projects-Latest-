using System.Linq;
using System.Windows.Forms;

namespace UpworkPlatesLookupAutomationDLL
{
    public static class helpers
    {
        public static HtmlElement GetElementByTagAndAttribute(this HtmlDocument doc, string Tag, string Attribute, string AttributeValue)
         => doc.GetElementsByTagName(Tag).Cast<HtmlElement>()
                        .SingleOrDefault(el => el.GetAttribute(Attribute) == AttributeValue);

        public static void SetValue(this HtmlElement elm, string value)
        {
            elm.InvokeMember("Click");
            elm.SetAttribute("value", value);
        }

        public static bool NotDisplayedInlineStyle(this HtmlElement elm) => elm.OuterHtml.ToLower().Replace(" ", string.Empty).Contains("display:none");
    }


}
