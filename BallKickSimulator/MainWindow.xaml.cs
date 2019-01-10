using Microsoft.Win32;
using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace BallKickSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        #endregion Atributi

        #region Bindings

        private int _ballRotationSpeed;

        public int BallRotationSpeed
        {
            get { return _ballRotationSpeed; }
            set { _ballRotationSpeed = value; World.BallRotationSpeed = value; NotifyPropertyChanged(); }
        }
        
        private int _ballJumpHeight;

        public int BallJumpHeight
        {
            get { return _ballJumpHeight; }
            set { _ballJumpHeight = value; World.BallJumpHeight = value; NotifyPropertyChanged(); }
        }

        private int _ballScale;

        public int BallScale
        {
            get { return _ballScale; }
            set { _ballScale = value; World.BallScale = value; NotifyPropertyChanged(); }
        }

        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = null)
        {
            //Console.WriteLine($"Changed {propertyName}!");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();
            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3DModels\\Ball"), "Ball3DS.3ds", (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }

            BallScale = 400;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F2: this.Close(); break;
                case Key.D:
                    if (!World.ballKickAnimation)
                    {
                        if (m_world.RotationX >= 5f && m_world.RotationX <= 175.0f)
                        {
                            m_world.RotationX -= 5.0f;
                        }
                        else if (m_world.RotationX < 5)
                        {
                            m_world.RotationX = 5;
                        }
                        else if (m_world.RotationX > 175.0f)
                        {
                            m_world.RotationX = 175.0f;
                        }
                    }
                    break;

                case Key.E:
                    if (!World.ballKickAnimation)
                    {
                        if (m_world.RotationX >= 5f && m_world.RotationX <= 90.0f)
                        {
                            m_world.RotationX += 5.0f;
                        }
                        else if (m_world.RotationX < 5)
                        {
                            m_world.RotationX = 5;
                        }
                        else if (m_world.RotationX > 90.0f)
                        {
                            m_world.RotationX = 90.0f;
                        }
                    }
                    break;
                case Key.S:
                    if (!World.ballKickAnimation)
                    {
                        m_world.RotationY += 5.0f;
                    }
                    break;
                case Key.F:
                    if (!World.ballKickAnimation)
                    {
                        m_world.RotationY -= 5.0f;
                    }
                    break;
                case Key.Subtract:
                    if (!World.ballKickAnimation)
                    {
                        m_world.SceneDistance += 700.0f;
                    }
                    break;
                case Key.Add:
                    if (!World.ballKickAnimation)
                    {
                        m_world.SceneDistance -= 700.0f;
                    }
                    break;
                case Key.V:
                    if (World.ballJumpAnimation)
                        break;

                    if (World.ballKickAnimation)
                    {
                        World.ballKickTimer.Stop();
                        World.ballKickTimer = null;
                        World.ballKickAnimation = false;
                        World.ballY = World.ballDefaultY;
                        World.ballZ = World.ballDefaultZ;
                    }
                    else
                    {
                        World.ballKickAnimation = true;
                        World.ballKickTimer = new System.Windows.Threading.DispatcherTimer();
                        World.ballKickTimer.Tick += new EventHandler(World.BallKickAnimation);
                        World.ballKickTimer.Interval = TimeSpan.FromMilliseconds(50);
                        World.ballKickTimer.Start();
                    }
                    break;
            }
        }

        private void BallJumpButton_Click(object sender, RoutedEventArgs e)
        {
            if (World.ballKickAnimation)
                return;

            if(World.ballJumpAnimation)
            {
                World.ballJumpTimer.Stop();
                World.ballJumpTimer = null;
                World.ballJumpAnimation = false;
                World.ballY = World.ballDefaultY;
                BallJumpButton.Content = "Jump Ball";
            }
            else
            {
                World.ballJumpAnimation = true;
                World.ballJumpTimer = new System.Windows.Threading.DispatcherTimer();
                World.ballJumpTimer.Tick += new EventHandler(World.BallJumpAnimation);
                World.ballJumpTimer.Interval = TimeSpan.FromMilliseconds(100);
                World.ballJumpTimer.Start();
                BallJumpButton.Content = "Stop Jump";
            }
        }
    }
}
