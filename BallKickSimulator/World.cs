using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            gl.Enable(OpenGL.GL_CULL_FACE);
            //gl.FrontFace(OpenGL.GL_CW);
            m_scene.LoadScene();
            m_scene.Initialize();
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

            DrawBall(gl);
            DrawGrass(gl);
            DrawGoal(gl);
            DrawText(gl);
            gl.PopMatrix();
            gl.Flush();
        }

        private void DrawGoal(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Color(128.0f, 128.0f, 128.0f);
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
            gl.Color(128.0f, 128.0f, 128.0f);
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
            gl.Color(128.0f, 128.0f, 128.0f);
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
            gl.Color(128.0f, 128.0f, 128.0f);
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
            gl.Color(128.0f, 128.0f, 128.0f);
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
        }

        private void DrawGrass(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(0.0f, -500.0f, 0.0f);
            gl.Scale(30, 15, 30);
            //gl.Translate(0.0f, -200.0f, 0.0f);
            //gl.Rotate(30.0f, 0.0f, 0.0f);
            gl.Begin(OpenGL.GL_QUADS);

            // green color
            gl.Color(0.0f, 140.0f, 0.0f);
            gl.Vertex(200, 0, 400);
            gl.Vertex(200, 0, -400);
            gl.Vertex(-400, 0, -400);
            gl.Vertex(-400, 0, 400);

            gl.End();
            gl.PopMatrix();
        }

        private void DrawBall(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(-200.0f, -100.0f, 0.0f);
            gl.Scale(400.0, 400.0, 400.0);
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
