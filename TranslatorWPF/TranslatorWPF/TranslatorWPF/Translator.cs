using System;
using System.Collections.Generic;
using System.Text;
using Google.Cloud.Translation.V2;

namespace TranslatorWPF
{
    public partial class Translator
    {
        public string EngToRus(string str)
        {
            //Console.OutputEncoding = Encoding.UTF8;
            TranslationClient client = TranslationClient.Create();
            var response = client.TranslateText(
                text: str,
                targetLanguage: "ru",  // Russian
                sourceLanguage: "en");  // English
            Console.WriteLine(response.TranslatedText);
            return response.TranslatedText;
        }
    }
}
