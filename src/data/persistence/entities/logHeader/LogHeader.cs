namespace Meminisse
{
    public class LogHeader
    {
        private Config _conf;
        public Config configuration 
        {
            get { return Config.instance; }
            set { _conf = value; }
        }

        public bool dryRun { get; set; } = false;

        public GCodeFile gCodeFile { get; set; }

        public string printComment { get; set; } = "Write meaningful comment !WHILE NOT LOGGING! about the print here!";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gCodePath">Virtual SD path "0:/{Something}"<param>
        /// <returns></returns>
        public LogHeader(string gCodePath) { this.gCodeFile = GCodeFile.BuildFromPath(gCodePath); }
    }
}