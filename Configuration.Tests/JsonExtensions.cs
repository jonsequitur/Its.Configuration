using System.Web.Script.Serialization;

namespace Its.Configuration.Tests
{
    public static class JsonExtensions
    {
        public static string ToJson(this object o)
        {
            return new JavaScriptSerializer().Serialize(o);
        }
    }
}