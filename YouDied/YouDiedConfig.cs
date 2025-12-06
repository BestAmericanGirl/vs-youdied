namespace YouDied
{
    public class YouDiedConfig
    {
        public static string ConfigName = "YouDied.json";
        public static YouDiedConfig Instance = new();

        public float DurationInMs = 3000f;
        public float FontSize = 72f;
        public string FontColorHex = "#8B0000";
        public float Volume = 1.0f;
    }
}
