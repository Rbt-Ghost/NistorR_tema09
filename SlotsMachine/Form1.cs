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
        private Panel framePanel; // Ramă decorativă pentru ecran
        private Button btnSpin;
        private Label lblTitle;   // Titlul CASINO
        private Label lblStatus;
        private Timer spinTimer;

        // --- VARIABILE JOC ---
        private int[] textureIds = new int[4];
        private int[] currentSlots = new int[3];
        private Random random = new Random();

        // --- LOGICA ---
        private int remainingCycles = 0;
        private bool isSpinning = false;
        private const int FIXED_CYCLES = 5;

        public Form1()
        {
            InitializeComponent();
            InitializeComponentCustom(); // Design-ul nou

            // Valori inițiale random
            currentSlots[0] = random.Next(0, 4);
            currentSlots[1] = random.Next(0, 4);
            currentSlots[2] = random.Next(0, 4);
        }

        // --- DESIGN MODERN (Interfața Aspectuoasă) ---
        private void InitializeComponentCustom()
        {
            // 1. Setări Fereastră Principală
            this.Size = new Size(900, 700);
            this.Text = "Super Slots 777";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 35); // Fundal Dark (Gri închis spre negru)
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // Nu lăsăm utilizatorul să tragă de margini
            this.MaximizeBox = false;

            // 2. Titlul Mare (Sus)
            lblTitle = new Label();
            lblTitle.Text = "★ CASINO 777 ★";
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = Color.Gold;
            lblTitle.AutoSize = true;
            // Centrare orizontală
            lblTitle.Location = new Point((this.ClientSize.Width - lblTitle.PreferredWidth) / 2, 20);
            this.Controls.Add(lblTitle);

            // 3. Rama Decorativă (Panel în spatele GLControl)
            framePanel = new Panel();
            framePanel.Size = new Size(780, 420);
            framePanel.Location = new Point((this.ClientSize.Width - framePanel.Width) / 2, 80);
            framePanel.BackColor = Color.FromArgb(50, 50, 50); // O nuanță mai deschisă decât fundalul
            framePanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(framePanel);

            // 4. GLControl (Ecranul de joc)
            glControl = new GLControl();
            glControl.Size = new Size(760, 400);
            // Îl punem în interiorul Ramei (centrat)
            glControl.Location = new Point(10, 10);
            glControl.Load += GlControl_Load;
            glControl.Paint += GlControl_Paint;
            framePanel.Controls.Add(glControl); // Adăugăm GLControl în Ramă, nu direct pe Form

            // 5. Butonul TRAGE (Stilizat)
            btnSpin = new Button();
            btnSpin.Text = "TRAGE!";
            btnSpin.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnSpin.Cursor = Cursors.Hand;

            // Culori Buton
            btnSpin.BackColor = Color.Crimson; // Roșu aprins
            btnSpin.ForeColor = Color.White;
            btnSpin.FlatStyle = FlatStyle.Flat; // Eliminăm aspectul vechi de Windows
            btnSpin.FlatAppearance.BorderSize = 0;

            btnSpin.Size = new Size(200, 60);
            // Centrare sub ecran
            btnSpin.Location = new Point((this.ClientSize.Width - btnSpin.Width) / 2, 530);
            btnSpin.Click += BtnSpin_Click;
            this.Controls.Add(btnSpin);

            // 6. Status Label (Rezultat)
            lblStatus = new Label();
            lblStatus.Text = "Încearcă-ți norocul!";
            lblStatus.Font = new Font("Segoe UI", 14, FontStyle.Italic);
            lblStatus.ForeColor = Color.LightGray;
            lblStatus.AutoSize = true;
            this.Controls.Add(lblStatus);
            // Centrare după ce a fost creat
            CenterStatusLabel();

            // 7. Timer
            spinTimer = new Timer();
            spinTimer.Interval = 500;
            spinTimer.Tick += SpinTimer_Tick;
        }

        private void CenterStatusLabel()
        {
            // Recalculăm poziția pentru a fi centrat perfect
            lblStatus.Location = new Point((this.ClientSize.Width - lblStatus.PreferredWidth) / 2, 610);
        }

        // --- OPENGL ---

        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f); // Negru aproape total în ecranul OpenGL
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

            // Desenăm sloturile cu puțin spațiu între ele
            DrawSlot(-0.75f, -0.25f, currentSlots[0]);
            DrawSlot(-0.25f, 0.25f, currentSlots[1]);
            DrawSlot(0.25f, 0.75f, currentSlots[2]);

            glControl.SwapBuffers();
        }

        private void DrawSlot(float startX, float endX, int textureIndex)
        {
            if (textureIds[textureIndex] == -1) return;

            // 1. Desenăm fundalul alb al rolei (ca să se vadă imaginea clar)
            GL.Disable(EnableCap.Texture2D);
            GL.Color3(Color.White); // Fundal alb per slot
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(startX, -0.6f);
            GL.Vertex2(endX, -0.6f);
            GL.Vertex2(endX, 0.6f);
            GL.Vertex2(startX, 0.6f);
            GL.End();
            GL.Enable(EnableCap.Texture2D);

            // 2. Desenăm Imaginea
            GL.BindTexture(TextureTarget.Texture2D, textureIds[textureIndex]);
            GL.Color3(Color.White); // Reset culoare

            // O facem un pic mai mică decât fundalul alb (padding)
            float padding = 0.05f;
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 1); GL.Vertex2(startX + padding, -0.55f);
            GL.TexCoord2(1, 1); GL.Vertex2(endX - padding, -0.55f);
            GL.TexCoord2(1, 0); GL.Vertex2(endX - padding, 0.55f);
            GL.TexCoord2(0, 0); GL.Vertex2(startX + padding, 0.55f);
            GL.End();

            // 3. Chenar Auriu Gros
            GL.Disable(EnableCap.Texture2D);
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

        // --- GAMEPLAY ---

        private void BtnSpin_Click(object sender, EventArgs e)
        {
            if (isSpinning) return;

            remainingCycles = FIXED_CYCLES;

            lblStatus.Text = "Se învârte...";
            lblStatus.ForeColor = Color.Yellow; // Galben când merge
            CenterStatusLabel();

            isSpinning = true;
            btnSpin.Enabled = false;
            btnSpin.BackColor = Color.Gray; // Butonul devine gri cât timp merge

            spinTimer.Start();
        }

        private void SpinTimer_Tick(object sender, EventArgs e)
        {
            currentSlots[0] = random.Next(0, 4);
            currentSlots[1] = random.Next(0, 4);
            currentSlots[2] = random.Next(0, 4);
            glControl.Invalidate();

            remainingCycles--;
            if (remainingCycles <= 0) StopSpin();
        }

        private void StopSpin()
        {
            spinTimer.Stop();
            isSpinning = false;
            btnSpin.Enabled = true;
            btnSpin.BackColor = Color.Crimson; // Revine la roșu

            CheckResult();
        }

        private void CheckResult()
        {
            if (currentSlots[0] == currentSlots[1] && currentSlots[1] == currentSlots[2])
            {
                lblStatus.Text = "★ JACKPOT! AI CÂȘTIGAT! ★";
                lblStatus.ForeColor = Color.Lime; // Verde neon
            }
            else
            {
                lblStatus.Text = "Ai pierdut. Încearcă din nou.";
                lblStatus.ForeColor = Color.LightCoral; // Roșu deschis
            }
            CenterStatusLabel();
        }
    }
}