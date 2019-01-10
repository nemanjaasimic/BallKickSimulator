using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BallKickSimulator
{
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 7000.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        private readonly int textureCount = Enum.GetNames(typeof(TextureObject)).Length;

        /// <summary>
        ///	 Identifikatori OpenGL tekstura
        /// </summary>
        private uint[] textures = null;
        
        /// <summary>
        ///	 Putanje do slika koje se koriste za teksture
        /// </summary>
        private string[] textureFiles = { "..//..//Textures//grass.jpg", "..//..//Textures//goal.jpg" };

        public const float ballDefaultX = -200f;
        public const float ballDefaultY = -100f;
        public const float ballDefaultZ = 0f;


        public static float ballY = -100f;
        public static float ballZ = 0f;
        public static float ballRotation = 0;

        public static bool ballJumpAnimation = false;
        public static DispatcherTimer ballJumpTimer;

        public static bool ballKickAnimation = false;
        public static DispatcherTimer ballKickTimer;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; Console.WriteLine($"Rotating X:{value}"); }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; Console.WriteLine($"Rotating Y:{value}"); }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public static int BallRotationSpeed { get; set; } = 0;

        public static int BallJumpHeight { get; set; } = 0;

        public static int BallScale { get; set; } = 400;

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;

            textures = new uint[textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.36f, 0.75f, 0.97f, 1.0f);
            gl.Color(1f, 0f, 0f);
            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            //gl.FrontFace(OpenGL.GL_CCW);
            //gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Enable(OpenGL.GL_NORMALIZE);

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT);
            gl.ColorMaterial(OpenGL.GL_BACK, OpenGL.GL_DIFFUSE);

            Textures(gl);
            
            m_scene.LoadScene();
            m_scene.Initialize();
        }

        private void LightSource(OpenGL gl)
        {
            float[] lightAmbient = { 0.45f, 0.45f, 0.45f, 1.0f };
            float[] lightDiffuse = { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] lightPosition = { 1200.0f, 400.0f, -4200.0f, 1.0f };
            
            gl.Enable(OpenGL.GL_LIGHTING);
            
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, lightAmbient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, lightPosition);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_EXPONENT, 80);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, lightAmbient);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, lightDiffuse);

            gl.Enable(OpenGL.GL_LIGHT0);
        }

        private void BallLightSource(OpenGL gl)
        {
            float[] lightPosition = new float[] { -200.0f, 1500.0f, 0.0f, 1.0f };
            float[] diffuseComponent = { 255.0f, 15.0f, 192.0f, 0.5f };
            float[] lightDirection = { 0.0f, -1.0f, .0f };

            gl.Enable(OpenGL.GL_LIGHTING);

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, diffuseComponent);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, lightDirection);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 35.0f);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, lightPosition);

            gl.Enable(OpenGL.GL_LIGHT1);
        }

        private void Textures(OpenGL gl)
        {
            // Teksture se primenjuju sa parametrom modulate
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

            //Ucitaj slike i kreiraj teksture
            gl.GenTextures(textureCount, textures);

            for (int i = 0; i < textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(textureFiles[i]);

                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);		// Najblizi sused filtriranja
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);     // Najblizi sused filtriranja
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);          //Podesen wrapping na repeat 
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);                //Nacin stapanja teksture sa materijalom 

                image.UnlockBits(imageData);
                image.Dispose();
            }
        }
        
        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            //gl.Enable(OpenGL.GL_CULL_FACE);
            gl.LoadIdentity();

            gl.PushMatrix();
                gl.Translate(0.0f, 0.0f, -m_sceneDistance);
                gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
                gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

                gl.PushMatrix();
                    gl.LookAt(0, 100, 50, 0, 500, -m_sceneDistance, 0, 1, 0);
                gl.PopMatrix();

                gl.PushMatrix();
                    LightSource(gl);
                    BallLightSource(gl);
                gl.PopMatrix();

                DrawBall(gl);
                DrawGrass(gl);
                DrawGoal(gl);
                DrawText(gl);
                m_scene.Draw();
            gl.PopMatrix();
            gl.Flush();
        }

        private void DrawGoal(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_TEXTURE_GEN_S);
            gl.Enable(OpenGL.GL_TEXTURE_GEN_T);
            gl.PushMatrix();
                //gl.Color(128.0f, 128.0f, 128.0f);
                gl.MatrixMode(OpenGL.GL_TEXTURE_MATRIX);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObject.Goal]);
                gl.Translate(1000.0f, 1000.0f, -4000.0f);
                gl.Rotate(90.0f, 0.0f, 0.0f);
                Cylinder left = new Cylinder
                {
                    TopRadius = 50f,
                    BaseRadius = 50f,
                    Height = 1700f
                };
                left.CreateInContext(gl);
                left.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
                //gl.Color(128.0f, 128.0f, 128.0f);
                gl.MatrixMode(OpenGL.GL_TEXTURE_MATRIX);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObject.Goal]);
                gl.Translate(-2300.0f, 1000.0f, -4000.0f);
                gl.Rotate(90.0f, 0.0f, 0.0f);
                Cylinder right = new Cylinder
                {
                    TopRadius = 50f,
                    BaseRadius = 50f,
                    Height = 1700f
                };
                right.CreateInContext(gl);
                right.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
                //gl.Color(128.0f, 128.0f, 128.0f);
                gl.MatrixMode(OpenGL.GL_TEXTURE_MATRIX);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObject.Goal]);
                gl.Translate(-2350.0f, 1050.0f, -4000.0f);
                gl.Rotate(0.0f, 90.0f, 0.0f);
                Cylinder top = new Cylinder
                {
                    TopRadius = 50f,
                    BaseRadius = 50f,
                    Height = 3400f
                };
                top.CreateInContext(gl);
                top.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
                //gl.Color(128.0f, 128.0f, 128.0f);
                gl.MatrixMode(OpenGL.GL_TEXTURE_MATRIX);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObject.Goal]);
                gl.Translate(-2300.0f, -300.0f, -4000.0f);
                gl.Rotate(0.0f, 90.0f, 0.0f);
                gl.Disable(OpenGL.GL_CULL_FACE);
                Disk rightLoop = new Disk
                {
                    InnerRadius = 1300f,
                    OuterRadius = 1350f,
                    StartAngle = 0f,
                    SweepAngle = 120f
                };
                rightLoop.CreateInContext(gl);
                rightLoop.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
                //gl.Color(128.0f, 128.0f, 128.0f);
                gl.MatrixMode(OpenGL.GL_TEXTURE_MATRIX);
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObject.Goal]);
                gl.Translate(1000.0f, -300.0f, -4000.0f);
                gl.Rotate(0.0f, 90.0f, 0.0f);
                Disk leftLoop = new Disk
                {
                    InnerRadius = 1300f,
                    OuterRadius = 1400f,
                    StartAngle = 0f,
                    SweepAngle = 120f
                };
                leftLoop.CreateInContext(gl);
                leftLoop.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Disable(OpenGL.GL_TEXTURE_GEN_S);
            gl.Disable(OpenGL.GL_TEXTURE_GEN_T);
        }

        private void DrawGrass(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObject.Grass]);

            gl.PushMatrix();
                gl.Translate(0.0f, -500.0f, 0.0f);
                gl.Scale(30, 15, 30);
                //gl.Translate(0.0f, -200.0f, 0.0f);
                //gl.Rotate(30.0f, 0.0f, 0.0f);
                gl.Begin(OpenGL.GL_QUADS);
                    gl.Normal(0, 1, 0);
                    // green color
                    //gl.Color(0.0f, 140.0f, 0.0f);
                    gl.TexCoord(1, 1);
                    gl.Vertex(200, 0, 400);
                    gl.TexCoord(1, -1);
                    gl.Vertex(200, 0, -400);
                    gl.TexCoord(-1, -1);
                    gl.Vertex(-400, 0, -400);
                    gl.TexCoord(-1, 1);
                    gl.Vertex(-400, 0, 400);
                gl.End();
            gl.PopMatrix();
        }

        private void DrawBall(OpenGL gl)
        {
            gl.PushMatrix();
                gl.Translate(-200.0f, ballY + BallScale / 4, ballZ + BallScale / 2);
                gl.Scale(BallScale, BallScale, BallScale);
                gl.Rotate(ballRotation, 0f, 0f);
                m_scene.Draw();
            gl.PopMatrix();
        }
        
        private void DrawText(OpenGL gl)
        {
            gl.PushMatrix();
                gl.Viewport(m_width / 2, m_height / 2, m_width / 2, m_height / 2);

                gl.DrawText(m_width - 400, m_height - 30, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "Predmet : Racunarska grafika");
                gl.DrawText(m_width - 400, m_height - 33, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "________________________");
                gl.DrawText(m_width - 400, m_height - 60, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "Sk. god: 2018/19");
                gl.DrawText(m_width - 400, m_height - 63, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "______________");
                gl.DrawText(m_width - 400, m_height - 90, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "Ime: Nemanja");
                gl.DrawText(m_width - 400, m_height - 93, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "___________");
                gl.DrawText(m_width - 400, m_height - 120, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "Prezime: Simic");
                gl.DrawText(m_width - 400, m_height - 123, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "___________");
                gl.DrawText(m_width - 400, m_height - 150, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "Sifra zad: 9.1");
                gl.DrawText(m_width - 400, m_height - 153, 0.0f, 0.0f, 0.0f, "Tahoma", 10, "___________");

                gl.Viewport(0, 0, m_width, m_height);
            gl.PopMatrix();
        }
        
        private static bool goingDown = false;

        public static void BallJumpAnimation(object sender, EventArgs e)
        {
            if (ballY >= ballDefaultY && ballJumpAnimation && goingDown)
            {
                ballY -= 100f;
                ballRotation += BallRotationSpeed;
            }
            else if (ballY <= BallJumpHeight && ballJumpAnimation && !goingDown)
            {
                ballY += 100f;
                ballRotation += BallRotationSpeed;
            }

            if (ballY > BallJumpHeight && ballJumpAnimation)
            {
                goingDown = true;
            }
            else if(ballY < ballDefaultY && ballJumpAnimation)
            {
                goingDown = false;
            }

        }

        private static bool goingBack = false;
        public static void BallKickAnimation(object sender, EventArgs e)
        {
            if (ballZ <= ballDefaultZ && ballKickAnimation && goingBack)
            {
                ballZ += 100;
                ballY = GetPositionForZ(ballZ);
            }
            else if (ballZ >= (-4000 + BallScale / 2) && ballKickAnimation && !goingBack)
            {
                ballZ -= 100;
                ballY = GetPositionForZ(ballZ);
            }

            if (ballZ < (-4000 + BallScale / 2) && ballKickAnimation)
            {
                goingBack = true;
            }
            else if (ballZ > ballDefaultZ && ballKickAnimation)
            {
                goingBack = false;
            }

            Console.WriteLine($"Ball is at height {ballY} and distance {ballZ}");
        }

        private static float GetPositionForZ(float z)
        {
            return 0.0000656f * z * z;
        }

        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 0.5f, 30000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable metode
    }
}
