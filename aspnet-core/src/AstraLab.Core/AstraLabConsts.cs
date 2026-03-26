using AstraLab.Debugging;

namespace AstraLab
{
    public class AstraLabConsts
    {
        public const string LocalizationSourceName = "AstraLab";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "613e3aa2d6304dbdb5e58181f052dfe0";
    }
}
