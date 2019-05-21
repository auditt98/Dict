namespace Dictianory
{
    public partial class OtherLanguages
    {
        class AppCache
        {
            public static string API { get; } = @"trnsl.1.1.20190510T132608Z.cab013decf407e21.bbb6e0254f8abf94d3a8e50a076b9ecf33bdefdb";
            public static string UrlGetAvailableLanguages { get; } = @"https://translate.yandex.net/api/v1.5/tr.json/getLangs?key={0}&ui={1}";
            public static string UrlDetectSrcLanguage { get; } = @"https://translate.yandex.net/api/v1.5/tr.json/detect?key={0}&text={1}";
            public static string UrlTranslateLanguage { get; } = @"https://translate.yandex.net/api/v1.5/tr.json/translate?key={0}&text={1}&lang={2}";
        }
    }
}
