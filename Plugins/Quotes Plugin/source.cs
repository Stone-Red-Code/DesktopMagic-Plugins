using DesktopMagicPluginAPI;
using DesktopMagicPluginAPI.Inputs;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace Quotes_Plugin
{
    public class Quotes : Plugin
    {
        private const string SaveFilePath = "save.txt";

        [Element("Update Interval (sec.):")]
        private IntegerUpDown integerUpDown = new IntegerUpDown(1, 9999, 600);

        public override int UpdateInterval { get; set; } = 10000;

        private HttpClient httpClient = new HttpClient();

        private Bitmap bmp;

        public override void Start()
        {
            integerUpDown.OnValueChanged += IntegerUpDown_OnValueChanged;

            if (File.Exists(SaveFilePath))
            {
                string num = File.ReadAllText(SaveFilePath);
                int updateInterval;
                _ = int.TryParse(num, out updateInterval);

                if (updateInterval >= integerUpDown.Minimum && updateInterval <= integerUpDown.Maximum)
                {
                    integerUpDown.Value = updateInterval;
                }
            }
        }

        private void IntegerUpDown_OnValueChanged()
        {
            UpdateInterval = integerUpDown.Value * 1000;
            File.WriteAllText(SaveFilePath, integerUpDown.Value.ToString());
        }

        public override Bitmap Main()
        {
            try
            {
                string quoteDataRaw = httpClient.GetStringAsync("https://api.quotable.io/random").GetAwaiter().GetResult();

                JsonDocument quoteData = JsonDocument.Parse(quoteDataRaw);

                string content = $"{quoteData.RootElement.GetProperty("content").GetString()}";
                content += "\n\n- " + quoteData.RootElement.GetProperty("author").GetString();

                bmp = new Bitmap(2000, 1000);
                bmp.SetResolution(200, 200);
                using Graphics g = Graphics.FromImage(bmp);
                g.DrawString(content, new Font(Application.Font, 50), new SolidBrush(Application.Color), new RectangleF(0, 0, bmp.Width, bmp.Height));
            }
            catch
            {
            }
            return bmp;
        }
    }
}
