using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SlotsMachine
{
    public partial class Form1 : Form
    {
        // --- COMPONENTE UI ---
        private GLControl glControl;
        private Panel framePanel;
        private Button btnSpin;
        private Label lblTitle;
        private Label lblStatus;
        private Timer spinTimer;

        // --- VARIABILE JOC ---
        private int[] textureIds = new int[4];
        private int[] currentSlots = new int[3];
        private Random random = new Random();

        // --- LOGICA ANIMATIEI ---
        private int remainingCycles = 0;
        private bool isSpinning = false;

        // Mărim numărul de cicluri pentru că timerul va fi mult mai rapid
        // 60 cicluri * 30ms = aprox 2 secunde de animație totală
        private const int TOTAL_CYCLES = 60;

        // Punctele la care se opresc roțile (pentru efectul secvențial)
        private const int STOP_SLOT_1 = 40; // La ciclul 40 se oprește prima roată
        private const int STOP_SLOT_2 = 20; // La ciclul 20 se oprește a doua

        public Form1()
        {
            InitializeComponent();
            InitializeComponentCustom();

            currentSlots[0] = random.Next(0, 4);
            currentSlots[1] = random.Next(0, 4);
            currentSlots[2] = random.Next(0, 4);
        }

        private void InitializeComponentCustom()
        {
            this.Size = new Size(900, 700);
            this.Text = "Super Slots 777 - Animated";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 35);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            lblTitle = new Label();
            lblTitle.Text = "★ CASINO 777 ★";
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = Color.Gold;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point((this.ClientSize.Width - lblTitle.PreferredWidth) / 2, 20);
            this.Controls.Add(lblTitle);

            framePanel = new Panel();
            framePanel.Size = new Size(780, 420);
            framePanel.Location = new Point((this.ClientSize.Width - framePanel.Width) / 2, 80);
            framePanel.BackColor = Color.FromArgb(50, 50, 50);
            framePanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(framePanel);

            glControl = new GLControl();
            glControl.Size = new Size(760, 400);
            glControl.Location = new Point(10, 10);
            glControl.Load += GlControl_Load;
            glControl.Paint += GlControl_Paint;
            framePanel.Controls.Add(glControl);

            btnSpin = new Button();
            btnSpin.Text = "TRAGE!";
            btnSpin.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnSpin.Cursor = Cursors.Hand;
            btnSpin.BackColor = Color.Crimson;
            btnSpin.ForeColor = Color.White;
            btnSpin.FlatStyle = FlatStyle.Flat;
            btnSpin.FlatAppearance.BorderSize = 0;
            btnSpin.Size = new Size(200, 60);
            btnSpin.Location = new Point((this.ClientSize.Width - btnSpin.Width) / 2, 530);
            btnSpin.Click += BtnSpin_Click;
            this.Controls.Add(btnSpin);

            lblStatus = new Label();
            lblStatus.Text = "Încearcă-ți norocul!";
            lblStatus.Font = new Font("Segoe UI", 14, FontStyle.Italic);
            lblStatus.ForeColor = Color.LightGray;
            lblStatus.AutoSize = true;
            this.Controls.Add(lblStatus);
            CenterStatusLabel();

            // --- ANIMATIE: Timer mult mai rapid ---
            spinTimer = new Timer();
            spinTimer.Interval = 30; // 30ms = mișcare foarte rapidă (aprox 30 FPS)
            spinTimer.Tick += SpinTimer_Tick;
        }

        private void CenterStatusLabel()
        {
            lblStatus.Location = new Point((this.ClientSize.Width - lblStatus.PreferredWidth) / 2, 610);
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            try
            {
                textureIds[0] = LoadTexture("PNG/cirese.png");
                textureIds[1] = LoadTexture("PNG/sapte.png");
                textureIds[2] = LoadTexture("PNG/bar.png");
                textureIds[3] = LoadTexture("PNG/diamant.png");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la imagini: " + ex.Message);
            }
        }

        private int LoadTexture(string path)
        {
            if (!File.Exists(path)) return -1;

            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            using (Bitmap bmp = new Bitmap(path))
            {
                BitmapData data = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bmp.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            // Important pentru animația de scroll (dacă textura se repetă)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return id;
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl.Context.IsCurrent) return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, -1.0, 1.0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Desenăm sloturile
            // Trimitem parametrul '0', '1', '2' pentru a ști care slot este
            DrawSlot(-0.75f, -0.25f, 0);
            DrawSlot(-0.25f, 0.25f, 1);
            DrawSlot(0.25f, 0.75f, 2);

            glControl.SwapBuffers();
        }

        private void DrawSlot(float startX, float endX, int slotIndex)
        {
            int textureIndex = currentSlots[slotIndex];
            if (textureIds[textureIndex] == -1) return;

            // --- ANIMATIE: Efect de vibrație (Motion Blur simulat) ---
            // Dacă roata se învârte (nu s-a oprit încă), adăugăm un mic offset vertical aleatoriu
            float blurOffset = 0.0f;
            bool isSlotSpinning = false;

            // Verificăm dacă ACEST slot specific încă se învârte
            if (isSpinning)
            {
                if (slotIndex == 0 && remainingCycles > STOP_SLOT_1) isSlotSpinning = true;
                if (slotIndex == 1 && remainingCycles > STOP_SLOT_2) isSlotSpinning = true;
                if (slotIndex == 2 && remainingCycles > 0) isSlotSpinning = true;
            }

            if (isSlotSpinning)
            {
                // Generăm un număr mic între -0.05 și 0.05
                blurOffset = (float)(random.NextDouble() * 0.1 - 0.05);
            }

            // 1. Fundal Alb
            GL.Disable(EnableCap.Texture2D);
            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(startX, -0.6f);
            GL.Vertex2(endX, -0.6f);
            GL.Vertex2(endX, 0.6f);
            GL.Vertex2(startX, 0.6f);
            GL.End();
            GL.Enable(EnableCap.Texture2D);

            // 2. Imaginea
            GL.BindTexture(TextureTarget.Texture2D, textureIds[textureIndex]);
            GL.Color3(Color.White);

            float padding = 0.05f;

            // Aplicăm offset-ul de animație la coordonatele Y
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 1 + blurOffset); GL.Vertex2(startX + padding, -0.55f);
            GL.TexCoord2(1, 1 + blurOffset); GL.Vertex2(endX - padding, -0.55f);
            GL.TexCoord2(1, 0 + blurOffset); GL.Vertex2(endX - padding, 0.55f);
            GL.TexCoord2(0, 0 + blurOffset); GL.Vertex2(startX + padding, 0.55f);
            GL.End();

            // 3. Chenar Auriu (Dacă s-a oprit, îl facem mai strălucitor)
            GL.Disable(EnableCap.Texture2D);

            if (!isSlotSpinning && isSpinning) // Dacă s-a oprit doar acest slot, dar jocul rulează
                GL.Color3(Color.OrangeRed); // Îl evidențiem că s-a blocat
            else
                GL.Color3(Color.Gold);

            GL.LineWidth(5.0f);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex2(startX, -0.6f);
            GL.Vertex2(endX, -0.6f);
            GL.Vertex2(endX, 0.6f);
            GL.Vertex2(startX, 0.6f);
            GL.End();
            GL.Enable(EnableCap.Texture2D);
        }

        // --- GAMEPLAY & LOGICA ROTIRII ---

        private void BtnSpin_Click(object sender, EventArgs e)
        {
            if (isSpinning) return;

            // Resetăm contorul la maxim (60)
            remainingCycles = TOTAL_CYCLES;

            lblStatus.Text = "Se învârte...";
            lblStatus.ForeColor = Color.Yellow;
            CenterStatusLabel();

            isSpinning = true;
            btnSpin.Enabled = false;
            btnSpin.BackColor = Color.Gray;

            spinTimer.Start();
        }

        private void SpinTimer_Tick(object sender, EventArgs e)
        {
            // --- ANIMATIE SECVENȚIALĂ ---
            // Schimbăm imaginile doar pentru roțile care NU s-au oprit încă

            // Slot 1 (Stânga) - Se oprește primul (când cycles ajunge la 40)
            if (remainingCycles > STOP_SLOT_1)
            {
                currentSlots[0] = random.Next(0, 4);
            }

            // Slot 2 (Centru) - Se oprește al doilea (când cycles ajunge la 20)
            if (remainingCycles > STOP_SLOT_2)
            {
                currentSlots[1] = random.Next(0, 4);
            }

            // Slot 3 (Dreapta) - Se oprește ultimul (când cycles ajunge la 0)
            if (remainingCycles > 0)
            {
                currentSlots[2] = random.Next(0, 4);
            }

            // Redesenăm cu noile imagini
            glControl.Invalidate();

            remainingCycles--;

            // Când ajungem la 0, totul se oprește
            if (remainingCycles <= 0)
            {
                StopSpin();
            }
        }

        private void StopSpin()
        {
            spinTimer.Stop();
            isSpinning = false;
            btnSpin.Enabled = true;
            btnSpin.BackColor = Color.Crimson;

            CheckResult();
        }

        private void CheckResult()
        {
            if (currentSlots[0] == currentSlots[1] && currentSlots[1] == currentSlots[2])
            {
                lblStatus.Text = "★ JACKPOT! AI CÂȘTIGAT! ★";
                lblStatus.ForeColor = Color.Lime;
            }
            else
            {
                lblStatus.Text = "Ai pierdut. Încearcă din nou.";
                lblStatus.ForeColor = Color.LightCoral;
            }
            CenterStatusLabel();
        }
    }
}