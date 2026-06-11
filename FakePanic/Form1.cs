using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace FakePanic
{
    public class Form1 : Form
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private WasapiLoopbackCapture capture;
        private IWavePlayer waveOut;
        private MemoryStream recordedAudioStream;
        private SoundPlayer fallbackPlayer;
        private volatile bool hasCapturedGlitch = false;

        private Label lblSad;
        private Label lblMain;
        private Label lblPercent;
        private PictureBox qrCodeBox;
        private Label lblDetails;

        public Form1()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            this.BackColor = Color.Wheat;
            this.TransparencyKey = Color.Wheat;
            Cursor.Hide();

            CaptureAndGlitchDesktopAudio();

            StartBsodTimeline();

            _hookID = SetHook(_proc);
            this.FormClosing += Form1_FormClosing;
        }

        private async void StartBsodTimeline()
        {
            await Task.Delay(2500);

            this.TransparencyKey = Color.Empty;
            this.BackColor = Color.FromArgb(0, 120, 215);

            CreateElements();

            lblSad.Visible = true;

            await Task.Delay(600);

            lblMain.Visible = true;

            await Task.Delay(500);

            lblPercent.Visible = true;

            await Task.Delay(1000);

            qrCodeBox.Visible = true;
            lblDetails.Visible = true;

            await Task.Delay(12000);
            FakeReboot();
        }

        private void CreateElements()
        {
            lblSad = new Label { Text = ":(", Font = new Font("Segoe UI", 90), ForeColor = Color.White, AutoSize = true, Location = new Point(150, 100), Visible = false };
            this.Controls.Add(lblSad);

            lblMain = new Label { Text = "Your PC ran into a problem and needs to restart. We're\njust collecting some error info, and then we'll restart for\nyou.", Font = new Font("Segoe UI", 22), ForeColor = Color.White, AutoSize = true, Location = new Point(150, 300), Visible = false };
            this.Controls.Add(lblMain);

            lblPercent = new Label { Text = "0% complete", Font = new Font("Segoe UI", 22), ForeColor = Color.White, AutoSize = true, Location = new Point(150, 440), Visible = false };
            this.Controls.Add(lblPercent);

            qrCodeBox = new PictureBox { Location = new Point(150, 560), Size = new Size(130, 130), SizeMode = PictureBoxSizeMode.StretchImage, Image = GenerateFakeQRCode(), Visible = false };
            this.Controls.Add(qrCodeBox);

            lblDetails = new Label { Text = "For more information about this issue and possible fixes, visit https://www.windows.com/stopcode\n\nIf you call a support person, give them this info:\nStop code: KERNEL_DATA_INPAGE_ERROR", Font = new Font("Segoe UI", 12), ForeColor = Color.White, AutoSize = true, Location = new Point(300, 565), Visible = false };
            this.Controls.Add(lblDetails);
        }

        private async void CaptureAndGlitchDesktopAudio()
        {
            try
            {
                capture = new WasapiLoopbackCapture();
                recordedAudioStream = new MemoryStream();

                capture.DataAvailable += (s, e) =>
                {
                    if (!hasCapturedGlitch)
                    {
                        bool hasRealAudio = false;
                        for (int i = 0; i < e.BytesRecorded; i += 4)
                        {
                            if (e.Buffer[i] != 0) { hasRealAudio = true; break; }
                        }

                        if (hasRealAudio)
                        {
                            byte[] boostedBuffer = new byte[e.BytesRecorded];
                            Array.Copy(e.Buffer, boostedBuffer, e.BytesRecorded);

                            if (capture.WaveFormat.BitsPerSample == 32)
                            {
                                for (int i = 0; i < e.BytesRecorded; i += 4)
                                {
                                    float sample = BitConverter.ToSingle(boostedBuffer, i);
                                    sample *= 2.3f;

                                    if (sample > 1.0f) sample = 1.0f;
                                    if (sample < -1.0f) sample = -1.0f;

                                    byte[] bytes = BitConverter.GetBytes(sample);
                                    Buffer.BlockCopy(bytes, 0, boostedBuffer, i, 4);
                                }
                            }

                            recordedAudioStream.Write(boostedBuffer, 0, e.BytesRecorded);

                            if (recordedAudioStream.Length > capture.WaveFormat.AverageBytesPerSecond * 0.045)
                            {
                                hasCapturedGlitch = true;
                                try { capture.StopRecording(); } catch { }
                            }
                        }
                    }
                };

                capture.RecordingStopped += (s, e) =>
                {
                    if (recordedAudioStream.Length > 0)
                    {
                        recordedAudioStream.Position = 0;
                        var rawStream = new RawSourceWaveStream(recordedAudioStream, capture.WaveFormat);
                        var loopStream = new LoopStream(rawStream);

                        try
                        {
                            var enumerator = new MMDeviceEnumerator();
                            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                            waveOut = new WasapiOut(device, AudioClientShareMode.Exclusive, false, 40);
                            waveOut.Init(loopStream);
                            waveOut.Play();
                        }
                        catch
                        {
                            waveOut = new WaveOutEvent();
                            waveOut.Init(loopStream);
                            waveOut.Play();
                        }
                    }
                    else
                    {
                        PlaySyntheticGlitch();
                    }
                };

                capture.StartRecording();

                await Task.Delay(200);
                if (!hasCapturedGlitch)
                {
                    hasCapturedGlitch = true;
                    try { capture.StopRecording(); } catch { }
                }
            }
            catch
            {
                PlaySyntheticGlitch();
            }
        }

        private void PlaySyntheticGlitch()
        {
            try
            {
                int sampleRate = 44100;
                short durationSeconds = 1;
                int dataSize = sampleRate * durationSeconds * 2;
                int fileSize = 36 + dataSize;

                MemoryStream synthStream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(synthStream);

                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(fileSize);
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(dataSize);

                double frequency = 85.0;
                for (int i = 0; i < sampleRate * durationSeconds; i++)
                {
                    double t = (double)i / sampleRate;
                    short sample = Math.Sin(2 * Math.PI * frequency * t) >= 0 ? (short)14000 : (short)-14000;
                    writer.Write(sample);
                }

                synthStream.Position = 0;
                fallbackPlayer = new SoundPlayer(synthStream);
                fallbackPlayer.PlayLooping();
            }
            catch { }
        }

        private Bitmap GenerateFakeQRCode()
        {
            Bitmap bmp = new Bitmap(25, 25); Random rand = new Random(42);
            for (int x = 0; x < 25; x++)
            {
                for (int y = 0; y < 25; y++)
                {
                    if ((x < 7 && y < 7) || (x > 17 && y < 7) || (x < 7 && y > 17))
                    {
                        bool isBorder = (x == 0 || x == 6 || y == 0 || y == 6 || (x > 17 && (y == 0 || y == 6)) || (y > 17 && (x == 0 || x == 6)));
                        bool isCenter = ((x >= 2 && x <= 4) && (y >= 2 && y <= 4)) || (x >= 20 && x <= 22 && y >= 2 && y <= 4) || (x >= 2 && x <= 4 && y >= 20 && y <= 22);
                        bmp.SetPixel(x, y, (isBorder || isCenter) ? Color.White : Color.FromArgb(0, 120, 215));
                    }
                    else { bmp.SetPixel(x, y, rand.Next(2) == 0 ? Color.White : Color.FromArgb(0, 120, 215)); }
                }
            }
            return bmp;
        }

        private async void FakeReboot()
        {
            StopAudio();
            this.BackColor = Color.Black;
            this.Controls.Clear();
            await Task.Delay(3000);
            Application.Exit();
        }

        private void StopAudio()
        {
            if (waveOut != null) { waveOut.Stop(); waveOut.Dispose(); }
            if (capture != null) { capture.Dispose(); }
            if (recordedAudioStream != null) { recordedAudioStream.Dispose(); }
            if (fallbackPlayer != null) { fallbackPlayer.Stop(); fallbackPlayer.Dispose(); }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopAudio();
            UnhookWindowsHookEx(_hookID);
            Cursor.Show();
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public class LoopStream : WaveStream
        {
            private readonly WaveStream sourceStream;
            public LoopStream(WaveStream sourceStream) { this.sourceStream = sourceStream; }
            public override WaveFormat WaveFormat => sourceStream.WaveFormat;
            public override long Length => sourceStream.Length;
            public override long Position { get => sourceStream.Position; set => sourceStream.Position = value; }
            public override int Read(byte[] buffer, int offset, int count)
            {
                int totalBytesRead = 0;
                while (totalBytesRead < count)
                {
                    int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        if (sourceStream.Position == 0) break;
                        sourceStream.Position = 0;
                    }
                    totalBytesRead += bytesRead;
                }
                return totalBytesRead;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control && key == Keys.F12)
                {
                    Application.Exit();
                    return IntPtr.Zero;
                }
                if (key == Keys.LWin || key == Keys.RWin) return (IntPtr)1;
                if (Control.ModifierKeys == Keys.Alt && (key == Keys.F4 || key == Keys.Tab || key == Keys.Escape)) return (IntPtr)1;
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control && (Control.ModifierKeys & Keys.Shift) == Keys.Shift && key == Keys.Escape) return (IntPtr)1;
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}