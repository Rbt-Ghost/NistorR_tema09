using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK_winforms_z03;

namespace OpenTK_winforms_z03
{
    public partial class Form1 : Form
    {

        //Stări de control cameră.
        private int eyePosX, eyePosY, eyePosZ;

        private Point mousePos;
        private float camDepth;

        //Stări de control mouse.
        private bool statusControlMouse2D, statusControlMouse3D, statusMouseDown;

        //Stări de control axe de coordonate.
        private bool statusControlAxe;

        //Stări de control iluminare.
        private bool lightON;
        private bool lightON_0;

        //Stări de control obiecte 3D.
        private string statusCube;

        //Texturare.
        // MODIFICARE: Am crescut dimensiunea la 3 pentru a include textura mixată
        private int[] textures = new int[3];
        private bool brick;
        private int colorTex;

        //Stări de control obiecte 3D.
        private bool statusCubeT;
        private bool statusCubeTex1;
        private bool statusCubeTex2;
        private bool statusCubeTex3;
        private bool statusCubeTex4;

        //Structuri de stocare a vertexurilor și a listelor de vertexuri.
        private int[,] arrVertex = new int[50, 3];
        private int nVertex;

        private int[] arrQuadsList = new int[100];
        private int nQuadsList;

        private int[] arrTrianglesList = new int[100];
        private int nTrianglesList;

        //Fișiere de in/out pentru manipularea vertexurilor.
        private string fileVertex = "vertexList.txt";
        private string fileQList = "quadsVertexList.txt";
        private string fileTList = "trianglesVertexList.txt";
        private bool statusFiles;

        //Setari lumini
        private float[] valuesAmbientTemplate0 = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };
        private float[] valuesDiffuseTemplate0 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        private float[] valuesSpecularTemplate0 = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };
        private float[] valuesPositionTemplate0 = new float[] { 0.0f, 0.0f, 5.0f, 1.0f };

        private float[] valuesAmbient0 = new float[4];
        private float[] valuesDiffuse0 = new float[4];
        private float[] valuesSpecular0 = new float[4];
        private float[] valuesPosition0 = new float[4];

        // VBOs
        int VBOobject;
        int nVert;
        bool VBOon = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupValues();
            SetupWindowGUI();
        }

        private void SetupValues()
        {
            eyePosX = 100;
            eyePosY = 100;
            eyePosZ = 50;
            camDepth = 1.04f;

            setLight0Values();

            numericXeye.Value = eyePosX;
            numericYeye.Value = eyePosY;
            numericZeye.Value = eyePosZ;

            statusCubeTex1 = false;
            statusCubeTex2 = false;
            statusCubeTex3 = false;
            statusCubeTex4 = false;
            brick = true;
            colorTex = 0;
        }

        private void SetupWindowGUI()
        {
            setControlMouse2D(false);
            setControlMouse3D(false);
            numericCameraDepth.Value = (int)camDepth;
            setControlAxe(true);
            setCubeStatus("OFF");
            setIlluminationStatus(false);
            setSource0Status(false);
            setTrackLigh0Default();
            setColorAmbientLigh0Default();
            setColorDifuseLigh0Default();
            setColorSpecularLigh0Default();
        }

        private void loadVertex()
        {
            try
            {
                StreamReader fileReader = new StreamReader((fileVertex));
                nVertex = Convert.ToInt32(fileReader.ReadLine().Trim());
                Console.WriteLine("Vertexuri citite: " + nVertex.ToString());

                string tmpStr = "";
                string[] str = new string[3];
                for (int i = 0; i < nVertex; i++)
                {
                    tmpStr = fileReader.ReadLine();
                    str = tmpStr.Trim().Split(' ');
                    arrVertex[i, 0] = Convert.ToInt32(str[0].Trim());
                    arrVertex[i, 1] = Convert.ToInt32(str[1].Trim());
                    arrVertex[i, 2] = Convert.ToInt32(str[2].Trim());
                }
                fileReader.Close();

            }
            catch (Exception )
            {
                statusFiles = false;
                Console.WriteLine("Fisierul cu informații vertex <" + fileVertex + "> nu exista!");
                MessageBox.Show("Fisierul cu informații vertex <" + fileVertex + "> nu exista!");
            }
        }

        private void loadQList()
        {
            try
            {
                StreamReader fileReader = new StreamReader(fileQList);
                int tmp;
                string line;
                nQuadsList = 0;
                while ((line = fileReader.ReadLine()) != null)
                {
                    tmp = Convert.ToInt32(line.Trim());
                    arrQuadsList[nQuadsList] = tmp;
                    nQuadsList++;
                }
                fileReader.Close();
            }
            catch (Exception )
            {
                statusFiles = false;
                MessageBox.Show("Fisierul cu informații vertex <" + fileQList + "> nu exista!");
            }
        }

        private void loadTList()
        {
            try
            {
                StreamReader fileReader = new StreamReader(fileTList);
                int tmp;
                string line;
                nTrianglesList = 0;
                while ((line = fileReader.ReadLine()) != null)
                {
                    tmp = Convert.ToInt32(line.Trim());
                    arrTrianglesList[nTrianglesList] = tmp;
                    nTrianglesList++;
                }
                fileReader.Close();
            }
            catch (Exception )
            {
                statusFiles = false;
                MessageBox.Show("Fisierul cu informații vertex <" + fileTList + "> nu exista!");
            }
        }

        // CONTROL CAMERĂ
        private void numericXeye_ValueChanged(object sender, EventArgs e)
        {
            eyePosX = (int)numericXeye.Value;
            GlControl1.Invalidate();
        }
        private void numericYeye_ValueChanged(object sender, EventArgs e)
        {
            eyePosY = (int)numericYeye.Value;
            GlControl1.Invalidate();
        }
        private void numericZeye_ValueChanged(object sender, EventArgs e)
        {
            eyePosZ = (int)numericZeye.Value;
            GlControl1.Invalidate();
        }
        private void numericCameraDepth_ValueChanged(object sender, EventArgs e)
        {
            camDepth = 1 + ((float)numericCameraDepth.Value) * 0.1f;
            GlControl1.Invalidate();
        }

        // CONTROL MOUSE
        private void setControlMouse2D(bool status)
        {
            if (status == false)
            {
                statusControlMouse2D = false;
                btnMouseControl2D.Text = "2D mouse control OFF";
            }
            else
            {
                statusControlMouse2D = true;
                btnMouseControl2D.Text = "2D mouse control ON";
            }
        }
        private void setControlMouse3D(bool status)
        {
            if (status == false)
            {
                statusControlMouse3D = false;
                btnMouseControl3D.Text = "3D mouse control OFF";
            }
            else
            {
                statusControlMouse3D = true;
                btnMouseControl3D.Text = "3D mouse control ON";
            }
        }

        private void btnMouseControl2D_Click(object sender, EventArgs e)
        {
            if (statusControlMouse2D == true)
            {
                setControlMouse2D(false);
            }
            else
            {
                setControlMouse3D(false);
                setControlMouse2D(true);
            }
        }
        private void btnMouseControl3D_Click(object sender, EventArgs e)
        {
            if (statusControlMouse3D == true)
            {
                setControlMouse3D(false);
            }
            else
            {
                setControlMouse2D(false);
                setControlMouse3D(true);
            }
        }

        private void GlControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (statusMouseDown == true)
            {
                mousePos = new Point(e.X, e.Y);
                GlControl1.Invalidate();
            }
        }
        private void GlControl1_MouseDown(object sender, MouseEventArgs e)
        {
            statusMouseDown = true;
        }
        private void GlControl1_MouseUp(object sender, MouseEventArgs e)
        {
            statusMouseDown = false;
        }

        // CONTROL ILUMINARE
        private void setIlluminationStatus(bool status)
        {
            if (status == false)
            {
                lightON = false;
                btnLights.Text = "Iluminare OFF";
            }
            else
            {
                lightON = true;
                btnLights.Text = "Iluminare ON";
            }
        }

        private void btnLights_Click(object sender, EventArgs e)
        {
            if (lightON == false)
            {
                setIlluminationStatus(true);
            }
            else
            {
                setIlluminationStatus(false);
            }
            GlControl1.Invalidate();
        }

        private void btnLightsNo_Click(object sender, EventArgs e)
        {
            int nr = GL.GetInteger(GetPName.MaxLights);
            MessageBox.Show("Nr. maxim de lumini pentru aceasta implementare OpenGL este <" + nr.ToString() + ">.");
        }

        private void setSource0Status(bool status)
        {
            if (status == false)
            {
                lightON_0 = false;
                btnLight0.Text = "Sursa 0 OFF";
            }
            else
            {
                lightON_0 = true;
                btnLight0.Text = "Sursa 0 ON";
            }
        }

        private void btnLight0_Click(object sender, EventArgs e)
        {
            if (lightON == true)
            {
                if (lightON_0 == false)
                {
                    setSource0Status(true);
                }
                else
                {
                    setSource0Status(false);
                }
                GlControl1.Invalidate();
            }
        }

        private void setTrackLigh0Default()
        {
            trackLight0PositionX.Value = (int)valuesPosition0[0];
            trackLight0PositionY.Value = (int)valuesPosition0[1];
            trackLight0PositionZ.Value = (int)valuesPosition0[2];
        }
        private void trackLight0PositionX_Scroll(object sender, EventArgs e)
        {
            valuesPosition0[0] = trackLight0PositionX.Value;
            GlControl1.Invalidate();
        }
        private void trackLight0PositionY_Scroll(object sender, EventArgs e)
        {
            valuesPosition0[1] = trackLight0PositionY.Value;
            GlControl1.Invalidate();
        }
        private void trackLight0PositionZ_Scroll(object sender, EventArgs e)
        {
            valuesPosition0[2] = trackLight0PositionZ.Value;
            GlControl1.Invalidate();
        }

        private void setColorAmbientLigh0Default()
        {
            numericLight0Ambient_Red.Value = (decimal)valuesAmbient0[0];
            numericLight0Ambient_Green.Value = (decimal)valuesAmbient0[1];
            numericLight0Ambient_Blue.Value = (decimal)valuesAmbient0[2];
        }
        private void numericLight0Ambient_Red_ValueChanged(object sender, EventArgs e)
        {
            valuesAmbient0[0] = (float)numericLight0Ambient_Red.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Ambient_Green_ValueChanged(object sender, EventArgs e)
        {
            valuesAmbient0[1] = (float)numericLight0Ambient_Green.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Ambient_Blue_ValueChanged(object sender, EventArgs e)
        {
            valuesAmbient0[2] = (float)numericLight0Ambient_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        private void setColorDifuseLigh0Default()
        {
            numericLight0Difuse_Red.Value = (decimal)valuesDiffuse0[0];
            numericLight0Difuse_Green.Value = (decimal)valuesDiffuse0[1];
            numericLight0Difuse_Blue.Value = (decimal)valuesDiffuse0[2];
        }
        private void numericLight0Difuse_Red_ValueChanged(object sender, EventArgs e)
        {
            valuesDiffuse0[0] = (float)numericLight0Difuse_Red.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Difuse_Green_ValueChanged(object sender, EventArgs e)
        {
            valuesDiffuse0[1] = (float)numericLight0Difuse_Green.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Difuse_Blue_ValueChanged(object sender, EventArgs e)
        {
            valuesDiffuse0[2] = (float)numericLight0Difuse_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        private void setColorSpecularLigh0Default()
        {
            numericLight0Specular_Red.Value = (decimal)valuesSpecular0[0];
            numericLight0Specular_Green.Value = (decimal)valuesSpecular0[1];
            numericLight0Specular_Blue.Value = (decimal)valuesSpecular0[2];
        }
        private void numericLight0Specular_Red_ValueChanged(object sender, EventArgs e)
        {
            valuesSpecular0[0] = (float)numericLight0Specular_Red.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Specular_Green_ValueChanged(object sender, EventArgs e)
        {
            valuesSpecular0[1] = (float)numericLight0Specular_Green.Value / 100;
            GlControl1.Invalidate();
        }
        private void numericLight0Specular_Blue_ValueChanged(object sender, EventArgs e)
        {
            valuesSpecular0[2] = (float)numericLight0Specular_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        private void setLight0Values()
        {
            for (int i = 0; i < valuesAmbientTemplate0.Length; i++)
            {
                valuesAmbient0[i] = valuesAmbientTemplate0[i];
            }
            for (int i = 0; i < valuesDiffuseTemplate0.Length; i++)
            {
                valuesDiffuse0[i] = valuesDiffuseTemplate0[i];
            }
            for (int i = 0; i < valuesPositionTemplate0.Length; i++)
            {
                valuesPosition0[i] = valuesPositionTemplate0[i];
            }
        }
        private void btnLight0Reset_Click(object sender, EventArgs e)
        {
            setLight0Values();
            setTrackLigh0Default();
            setColorAmbientLigh0Default();
            setColorDifuseLigh0Default();
            setColorSpecularLigh0Default();
            GlControl1.Invalidate();
        }

        // CONTROL OBIECTE 3D
        private void setControlAxe(bool status)
        {
            if (status == false)
            {
                statusControlAxe = false;
                btnShowAxes.Text = "Axe Oxyz OFF";
            }
            else
            {
                statusControlAxe = true;
                btnShowAxes.Text = "Axe Oxyz ON";
            }
        }

        private void btnShowAxes_Click(object sender, EventArgs e)
        {
            if (statusControlAxe == true)
            {
                setControlAxe(false);
            }
            else
            {
                setControlAxe(true);
            }
            GlControl1.Invalidate();
        }

        private void setCubeStatus(string status)
        {
            if (status.Trim().ToUpper().Equals("TRIANGLES"))
            {
                statusCube = "TRIANGLES";
            }
            else if (status.Trim().ToUpper().Equals("QUADS"))
            {
                statusCube = "QUADS";
            }
            else
            {
                statusCube = "OFF";
            }
        }
        private void btnCubeQ_Click(object sender, EventArgs e)
        {
            statusFiles = true;
            loadVertex();
            loadQList();
            setCubeStatus("QUADS");
            GlControl1.Invalidate();
        }
        private void btnCubeT_Click(object sender, EventArgs e)
        {
            statusFiles = true;
            loadVertex();
            loadTList();
            setCubeStatus("TRIANGLES");
            GlControl1.Invalidate();
        }
        private void btnResetObjects_Click(object sender, EventArgs e)
        {
            setCubeStatus("OFF");
            GlControl1.Invalidate();
        }


        // RANDARE
        private void GlControl1_Paint(object sender, PaintEventArgs e)
        {
            // 1. Curățare buffere și setare fundal
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.Black);

            // 2. Configurare Cameră (Perspectivă și LookAt)
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView((float)camDepth, 4 / 3, 1, 10000);
            Matrix4 lookat = Matrix4.LookAt(eyePosX, eyePosY, eyePosZ, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref perspective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref lookat);

            // Setări Viewport și DepthTest
            GL.Viewport(0, 0, GlControl1.Width, GlControl1.Height);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            // 3. Configurare Iluminare
            if (lightON == true)
            {
                GL.Enable(EnableCap.Lighting);
            }
            else
            {
                GL.Disable(EnableCap.Lighting);
            }

            GL.Light(LightName.Light0, LightParameter.Ambient, valuesAmbient0);
            GL.Light(LightName.Light0, LightParameter.Diffuse, valuesDiffuse0);
            GL.Light(LightName.Light0, LightParameter.Specular, valuesSpecular0);
            GL.Light(LightName.Light0, LightParameter.Position, valuesPosition0);

            if ((lightON == true) && (lightON_0 == true))
            {
                GL.Enable(EnableCap.Light0);
            }
            else
            {
                GL.Disable(EnableCap.Light0);
            }

            // 4. Transformări Mouse (Rotație scenă)
            if (statusControlMouse2D == true)
            {
                GL.Rotate(mousePos.X, 0, 1, 0);
            }
            if (statusControlMouse3D == true)
            {
                GL.Rotate(mousePos.X, 0, 1, 1);
            }

            // 5. Texturare (Activare globală pentru scenă)
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            LoadTextures();

            // 6. Desenare Axe de coordonate
            if (statusControlAxe == true)
            {
                DeseneazaAxe();
            }

            // 7. Desenare Cuburi (Logică originală)
            if (statusCube.ToUpper().Equals("QUADS"))
            {
                DeseneazaCubQ();
            }
            else if (statusCube.ToUpper().Equals("TRIANGLES"))
            {
                DeseneazaCubT();
            }

            if (statusCubeTex1 == true)
            {
                DeseneazaCubQ_Tex1();
            }
            if (statusCubeTex2 == true)
            {
                DeseneazaCubQ_Tex2();
            }
            if (statusCubeTex3 == true)
            {
                DeseneazaCubT_Tex3();
            }
            if (statusCubeTex4 == true)
            {
                DeseneazaCubT_Tex4();
            }

            // 8. Desenare Piramidă Texturată (NOU)
            // Folosim Push/Pop Matrix pentru a nu afecta poziția celorlalte obiecte
            GL.PushMatrix();
            GL.Translate(50.0f, 20.0f, 0.0f); // Mutăm piramida la dreapta și sus
            DeseneazaPiramidaTexturata();
            GL.PopMatrix();

            // 9. Desenare Sticlă Roșie (NOU)
            // Această metodă conține intern GL.Disable(Texture2D) pentru a nu fi afectată de texturi
            // și este desenată spre final pentru a gestiona corect transparența peste fundal
            DeseneazaSticlaRosie();

            // 10. Desenare VBO (dacă este activat)
            if (VBOon == true)
            {
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes, 2);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOobject);

                // Dacă VBO-ul nu are coordonate textură, e bine să dezactivăm texturarea aici sau să setăm una albă
                // Pentru simplitate, păstrăm setările curente, dar forțăm culoarea roșie
                GL.Color3(Color.Red);

                // Notă: TexCoord2 trebuie apelat per vertex în VBO arrays sau folosind ClientState pentru texturi
                // Aici este o setare statică
                GL.TexCoord2(0.5f, 0.5f);

                GL.DrawArrays(PrimitiveType.Quads, 0, nVert);

                GL.DisableClientState(ArrayCap.VertexArray);
            }

            // 11. Finalizare (Swap Buffers)
            GlControl1.SwapBuffers();
        }

        private void DeseneazaAxe()
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(75, 0, 0);
            GL.End();
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Yellow);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 75, 0);
            GL.End();
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 75);
            GL.End();
        }

        private void DeseneazaCubQ()
        {
            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < nQuadsList; i++)
            {
                switch (i % 4)
                {
                    case 0: GL.Color3(Color.Blue); break;
                    case 1: GL.Color3(Color.Red); break;
                    case 2: GL.Color3(Color.Green); break;
                    case 3: GL.Color3(Color.Yellow); break;
                }
                int x = arrQuadsList[i];
                GL.Vertex3(arrVertex[x, 0], arrVertex[x, 1], arrVertex[x, 2]);
            }
            GL.End();
        }

        private void DeseneazaCubT()
        {
            GL.Begin(PrimitiveType.Triangles);
            for (int i = 0; i < nTrianglesList; i++)
            {
                switch (i % 3)
                {
                    case 0: GL.Color3(Color.Blue); break;
                    case 1: GL.Color3(Color.Red); break;
                    case 2: GL.Color3(Color.Green); break;
                }
                int x = arrTrianglesList[i];
                GL.Vertex3(arrVertex[x, 0], arrVertex[x, 1], arrVertex[x, 2]);
            }
            GL.End();
        }

        private void DeseneazaSticlaRosie()
        {
            // Dezactivăm texturarea pentru a nu se amesteca cu textura cuburilor
            GL.Disable(EnableCap.Texture2D);

            // 1. ACTIVARE BLENDING
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // 2. DEFINIRE MATERIALE
            float[] mat_ambient = { 0.4f, 0.0f, 0.0f, 0.5f };
            float[] mat_diffuse = { 0.9f, 0.0f, 0.0f, 0.5f };
            float[] mat_specular = { 1.0f, 1.0f, 1.0f, 1.0f };
            float mat_shininess = 100.0f;

            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, mat_ambient);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, mat_diffuse);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, mat_specular);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, mat_shininess);

            // Culoare de siguranță (dacă lumina e oprită)
            GL.Color4(0.9f, 0.0f, 0.0f, 0.5f);

            // 3. DESENARE GEOMETRIE
            GL.Begin(PrimitiveType.Quads);

            GL.Normal3(0.0, 0.0, 1.0);

            // Coordonatele modificate (ridicate cu 30 pe axa Y)
            // Y-ul de jos devine 0.0f (era -30)
            // Y-ul de sus devine 60.0f (era 30)
            GL.Vertex3(-30.0f, 0.0f, 30.0f);
            GL.Vertex3(30.0f, 0.0f, 30.0f);
            GL.Vertex3(30.0f, 60.0f, 30.0f);
            GL.Vertex3(-30.0f, 60.0f, 30.0f);

            GL.End();

            // 4. RESTAURARE STĂRI
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
        }

        private void DeseneazaPiramidaTexturata()
        {
            // Ne asigurăm că texturarea este activată
            GL.Enable(EnableCap.Texture2D);

            // Alegem textura (0 = cărămizi, 1 = OpenGL logo, 2 = Mix)
            GL.BindTexture(TextureTarget.Texture2D, textures[2]);

            // Setăm culoarea pe alb pentru a nu altera culorile texturii originale
            GL.Color3(Color.White);

            // Dimensiunea piramidei
            float size = 25.0f;
            float height = 40.0f;

            // Începem desenarea fețelor laterale (TRIUNGHIURI - suprafețe non-rectangulare)
            GL.Begin(PrimitiveType.Triangles);

            // --- Fața din Față ---
            GL.Normal3(0.0f, 0.5f, 1.0f); // Normala aproximativă pentru lumini
                                          // Coordonata textură (stânga-jos) -> Vertex stânga-jos
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-size, -size, size);
            // Coordonata textură (dreapta-jos) -> Vertex dreapta-jos
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(size, -size, size);
            // Coordonata textură (centru-sus) -> Vârful piramidei
            GL.TexCoord2(0.5f, 1.0f); GL.Vertex3(0.0f, height, 0.0f);

            // --- Fața din Dreapta ---
            GL.Normal3(1.0f, 0.5f, 0.0f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(size, -size, size);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(size, -size, -size);
            GL.TexCoord2(0.5f, 1.0f); GL.Vertex3(0.0f, height, 0.0f);

            // --- Fața din Spate ---
            GL.Normal3(0.0f, 0.5f, -1.0f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(size, -size, -size);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(-size, -size, -size);
            GL.TexCoord2(0.5f, 1.0f); GL.Vertex3(0.0f, height, 0.0f);

            // --- Fața din Stânga ---
            GL.Normal3(-1.0f, 0.5f, 0.0f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-size, -size, -size);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(-size, -size, size);
            GL.TexCoord2(0.5f, 1.0f); GL.Vertex3(0.0f, height, 0.0f);

            GL.End();

            // Desenăm baza (PATRULATER - suprafață rectangulară)
            GL.Begin(PrimitiveType.Quads);
            GL.Normal3(0.0f, -1.0f, 0.0f); // Normala în jos

            // Mapăm textura complet pe bază
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(-size, -size, -size);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(size, -size, -size);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(size, -size, size);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(-size, -size, size);
            GL.End();
        }

        // TEXTURARE

        // MODIFICARE: Metoda de combinare a imaginilor (70% opac / 30% transparent)
        private Bitmap CombineImages(string fileOpaque, string fileTransparent, float alphaTransparency)
        {
            Bitmap bmpOpaque = new Bitmap(fileOpaque);
            Bitmap bmpTransparent = new Bitmap(fileTransparent);
            Bitmap result = new Bitmap(bmpOpaque.Width, bmpOpaque.Height);

            using (Graphics g = Graphics.FromImage(result))
            {
                // Desenăm imaginea opacă
                g.DrawImage(bmpOpaque, 0, 0, result.Width, result.Height);

                // Setăm transparența pentru imaginea a doua
                ColorMatrix cm = new ColorMatrix();
                cm.Matrix33 = alphaTransparency;

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                // Desenăm imaginea transparentă peste
                g.DrawImage(bmpTransparent,
                    new Rectangle(0, 0, result.Width, result.Height),
                    0, 0, bmpTransparent.Width, bmpTransparent.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }
            return result;
        }

        // MODIFICARE: Încărcare textură direct din Bitmap (din memorie)
        private void LoadTextureFromBitmap(int textureId, Bitmap bmp)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                          PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
        }

        private void LoadTextures()
        {
            GL.GenTextures(textures.Length, textures);
            LoadTexture(textures[0], "brickTexture.jpg");
            LoadTexture(textures[1], "OpenGLTexture.png");

            // MODIFICARE: Creare și încărcare textura 3 (mixată)
            // Parametrul 0.3f asigură transparența de 30% a logo-ului
            Bitmap mixedBitmap = CombineImages("brickTexture.jpg", "OpenGLTexture.png", 0.3f);
            LoadTextureFromBitmap(textures[2], mixedBitmap);
        }

        private void LoadTexture(int textureId, string filename)
        {
            Bitmap bmp = new Bitmap(filename);
            LoadTextureFromBitmap(textureId, bmp); // Refolosim funcția generică
        }

        // ... LoadTextureDynamically ... (nicio modificare aici)
        private TextureFromBMP LoadTextureDynamically(string filename)
        {
            Bitmap bmp = new Bitmap(filename);
            int textureId = GL.GenTexture();
            LoadTextureFromBitmap(textureId, bmp);
            return new TextureFromBMP(textureId, bmp.Width, bmp.Height);
        }

        // Controale UI texturi
        private void rbTexture0_CheckedChanged(object sender, EventArgs e) { brick = true; }
        private void radioButton2_CheckedChanged(object sender, EventArgs e) { brick = false; }

        private void rbColorWhite_CheckedChanged(object sender, EventArgs e) { colorTex = 0; }
        private void rbColorRed_CheckedChanged(object sender, EventArgs e) { colorTex = 1; }
        private void rbColorBlue_CheckedChanged(object sender, EventArgs e) { colorTex = 2; }

        private void btnTexT_1_Click(object sender, EventArgs e)
        {
            resetTexCubes();
            loadVertex();
            loadTList();
            statusCubeTex3 = true;
            GlControl1.Invalidate();
        }

        private void btnTexT_2_Click(object sender, EventArgs e)
        {
            resetTexCubes();
            loadVertex();
            loadTList();
            statusCubeTex4 = true;
            GlControl1.Invalidate();
        }

        private void btnReset_Tex2_Click(object sender, EventArgs e)
        {
            resetTexCubes();
        }

        // MODIFICARE AICI: Folosim textura nouă la apăsarea butonului 1 (Quads 100%)
        private void btnTexQ_1_Click(object sender, EventArgs e)
        {
            resetTexCubes();
            loadVertex();
            loadQList();
            statusCubeTex1 = true;
            GlControl1.Invalidate();
        }

        private void btnTexQ_2_Click(object sender, EventArgs e)
        {
            resetTexCubes();
            loadVertex();
            loadQList();
            statusCubeTex2 = true;
            GlControl1.Invalidate();
        }

        private void btnReset_Tex1_Click(object sender, EventArgs e)
        {
            resetTexCubes();
        }

        private void resetTexCubes()
        {
            setCubeStatus("OFF");
            statusCubeTex1 = false;
            statusCubeTex2 = false;
            statusCubeTex3 = false;
            statusCubeTex4 = false;
            GlControl1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int[,] arrVertexer = null;
            try
            {
                StreamReader fileReader = new StreamReader(("vertexDrawVbo.txt"));
                int nVertexer = Convert.ToInt32(fileReader.ReadLine().Trim());
                arrVertexer = new int[nVertexer, 3];
                nVert = nVertexer;
                string tmpStr = "";
                string[] str = new string[3];
                for (int i = 0; i < nVertexer; i++)
                {
                    tmpStr = fileReader.ReadLine();
                    str = tmpStr.Trim().Split(' ');
                    arrVertexer[i, 0] = Convert.ToInt32(str[0].Trim());
                    arrVertexer[i, 1] = Convert.ToInt32(str[1].Trim());
                    arrVertexer[i, 2] = Convert.ToInt32(str[2].Trim());
                }
                fileReader.Close();
            }
            catch (Exception)
            {
                statusFiles = false;
                MessageBox.Show("Fisierul cu informații vertex <" + fileVertex + "> nu exista!");
            }

            Vector3[] vertexObj = new Vector3[nVert];
            for (int i = 0; i < nVert; i++)
            {
                vertexObj[i] = new Vector3(arrVertexer[i, 0], arrVertexer[i, 1], arrVertexer[i, 2]);
            }

            TextureFromBMP SpecialTexture = LoadTextureDynamically("brickTexture.jpg");
            GL.BindTexture(TextureTarget.Texture2D, SpecialTexture.id);
            VBOobject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOobject);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(Vector3.SizeInBytes * vertexObj.Length), vertexObj, BufferUsageHint.StaticDraw);
            VBOon = true;
            GlControl1.Invalidate();
        }

        private void GlControl1_Load(object sender, EventArgs e) { }

        // MODIFICARE: Folosirea texturii mixate (index 2)
        private void DeseneazaCubQ_Tex1()
        {
            // Bind texture mixată (index 2) indiferent de selecția radio button-ului,
            // sau poți adăuga logică suplimentară. Aici am forțat textura nouă:
            GL.BindTexture(TextureTarget.Texture2D, textures[2]);

            // Forțăm culoarea albă pentru a vedea textura corect
            GL.Color3(Color.White);

            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < nQuadsList; i = i + 4)
            {
                GL.TexCoord2(0.0, 1.0);
                GL.Vertex3(arrVertex[arrQuadsList[i], 0], arrVertex[arrQuadsList[i], 1], arrVertex[arrQuadsList[i], 2]);
                GL.TexCoord2(1.0, 1.0);
                GL.Vertex3(arrVertex[arrQuadsList[i + 1], 0], arrVertex[arrQuadsList[i + 1], 1], arrVertex[arrQuadsList[i + 1], 2]);
                GL.TexCoord2(1.0, 0.0);
                GL.Vertex3(arrVertex[arrQuadsList[i + 2], 0], arrVertex[arrQuadsList[i + 2], 1], arrVertex[arrQuadsList[i + 2], 2]);
                GL.TexCoord2(0.0, 0.0);
                GL.Vertex3(arrVertex[arrQuadsList[i + 3], 0], arrVertex[arrQuadsList[i + 3], 1], arrVertex[arrQuadsList[i + 3], 2]);
            }
            GL.End();
        }

        // Restul funcțiilor de desenare au rămas neschimbate, folosind logica veche
        private void DeseneazaCubQ_Tex2()
        {
            if (brick == true) { GL.BindTexture(TextureTarget.Texture2D, textures[0]); }
            else { GL.BindTexture(TextureTarget.Texture2D, textures[1]); }

            SetColorFromRadio();

            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < nQuadsList; i = i + 4)
            {
                GL.TexCoord2(0.0, 0.5);
                GL.Vertex3(arrVertex[arrQuadsList[i], 0], arrVertex[arrQuadsList[i], 1], arrVertex[arrQuadsList[i], 2]);
                GL.TexCoord2(0.5, 0.5);
                GL.Vertex3(arrVertex[arrQuadsList[i + 1], 0], arrVertex[arrQuadsList[i + 1], 1], arrVertex[arrQuadsList[i + 1], 2]);
                GL.TexCoord2(0.5, 0.0);
                GL.Vertex3(arrVertex[arrQuadsList[i + 2], 0], arrVertex[arrQuadsList[i + 2], 1], arrVertex[arrQuadsList[i + 2], 2]);
                GL.TexCoord2(0.0, 0.0);
                GL.Vertex3(arrVertex[arrQuadsList[i + 3], 0], arrVertex[arrQuadsList[i + 3], 1], arrVertex[arrQuadsList[i + 3], 2]);
            }
            GL.End();
        }

        private void DeseneazaCubT_Tex3()
        {
            if (brick == true) { GL.BindTexture(TextureTarget.Texture2D, textures[0]); }
            else { GL.BindTexture(TextureTarget.Texture2D, textures[1]); }

            SetColorFromRadio();

            GL.Begin(PrimitiveType.Triangles);
            for (int i = 0; i < nTrianglesList; i = i + 6)
            {
                GL.TexCoord2(0.0, 0.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i], 0], arrVertex[arrTrianglesList[i], 1], arrVertex[arrTrianglesList[i], 2]);
                GL.TexCoord2(1.0, 0.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 1], 0], arrVertex[arrTrianglesList[i + 1], 1], arrVertex[arrTrianglesList[i + 1], 2]);
                GL.TexCoord2(0.0, 1.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 2], 0], arrVertex[arrTrianglesList[i + 2], 1], arrVertex[arrTrianglesList[i + 2], 2]);
                GL.TexCoord2(1.0, 0.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 3], 0], arrVertex[arrTrianglesList[i + 3], 1], arrVertex[arrTrianglesList[i + 3], 2]);
                GL.TexCoord2(0.0, 1.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 4], 0], arrVertex[arrTrianglesList[i + 4], 1], arrVertex[arrTrianglesList[i + 4], 2]);
                GL.TexCoord2(1.0, 1.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 5], 0], arrVertex[arrTrianglesList[i + 5], 1], arrVertex[arrTrianglesList[i + 5], 2]);
            }
            GL.End();
        }

        private void DeseneazaCubT_Tex4()
        {
            if (brick == true) { GL.BindTexture(TextureTarget.Texture2D, textures[0]); }
            else { GL.BindTexture(TextureTarget.Texture2D, textures[1]); }

            SetColorFromRadio();

            GL.Begin(PrimitiveType.Triangles);
            for (int i = 0; i < nTrianglesList; i = i + 6)
            {
                GL.TexCoord2(0.0, 0.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i], 0], arrVertex[arrTrianglesList[i], 1], arrVertex[arrTrianglesList[i], 2]);
                GL.TexCoord2(0.5, 0.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 1], 0], arrVertex[arrTrianglesList[i + 1], 1], arrVertex[arrTrianglesList[i + 1], 2]);
                GL.TexCoord2(0.0, 0.5);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 2], 0], arrVertex[arrTrianglesList[i + 2], 1], arrVertex[arrTrianglesList[i + 2], 2]);
                GL.TexCoord2(0.5, 0.0);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 3], 0], arrVertex[arrTrianglesList[i + 3], 1], arrVertex[arrTrianglesList[i + 3], 2]);
                GL.TexCoord2(0.0, 0.5);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 4], 0], arrVertex[arrTrianglesList[i + 4], 1], arrVertex[arrTrianglesList[i + 4], 2]);
                GL.TexCoord2(0.5, 0.5);
                GL.Vertex3(arrVertex[arrTrianglesList[i + 5], 0], arrVertex[arrTrianglesList[i + 5], 1], arrVertex[arrTrianglesList[i + 5], 2]);
            }
            GL.End();
        }

        // Helper pentru culoare
        private void SetColorFromRadio()
        {
            switch (colorTex)
            {
                case 0: GL.Color3(Color.White); break;
                case 1: GL.Color3(Color.FromArgb(0, 255, 0, 0)); break;
                case 2: GL.Color3(Color.FromArgb(0, 0, 0, 255)); break;
            }
        }
    }
}