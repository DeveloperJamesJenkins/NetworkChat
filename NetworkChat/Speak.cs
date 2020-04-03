using System;
using System.Speech.Synthesis;

namespace NetworkChat
{
    class Speak
    {
        public static SpeechSynthesizer ss = new SpeechSynthesizer();
        public static void SpeakPhrase(string Phrase)
        {
            ss.SpeakAsync(Phrase);
        }
    }
}
