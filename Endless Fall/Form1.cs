using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Endless_Fall
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int x0 = 20, y0 = 20, w = 300, h = 500, scrollSpeed = 2;
        double gravitationalAcceleration = 0.2, shockAbsorption = 0.8;
        bool left = true, running;
        Graphics g;

        GameEngine ge;
        Random rand;
        Point p1, p2;

        private void OnPaint(object sender, PaintEventArgs e)
        {
            g = this.CreateGraphics();
            ge = new GameEngine(x0, y0, w, h, gravitationalAcceleration, scrollSpeed, shockAbsorption, g);
            rand = new Random();

            p1.X = 0;
            p1.Y = h / 2;
            p2.X = rand.Next(100, w - 200);
            p2.Y = h / 2 + rand.Next(20, 60);
            ge.AddLineGameObject(new LineGameObject(p1, p2));

            p1.X = rand.Next(100, w - 200);
            p1.Y = 3 * h / 4 + rand.Next(20, 60);
            p2.X = w;
            p2.Y = 3 * h / 4;
            ge.AddLineGameObject(new LineGameObject(p1, p2));

            p1.X = rand.Next(1, w - 31);
            p1.Y = rand.Next(0, h / 2);
            ge.AddCircleGameObject(new CircleGameObject(p1, rand.Next(10, 30), Color.Red));

            p1.X = rand.Next(1, w - 31);
            p1.Y = rand.Next(0, h / 2);
            ge.AddCircleGameObject(new CircleGameObject(p1, rand.Next(10, 30), Color.Blue));

            p1.X = rand.Next(1, w - 31);
            p1.Y = rand.Next(0, h / 2);
            ge.AddCircleGameObject(new CircleGameObject(p1, rand.Next(10, 30), Color.Green));

            p1.X = rand.Next(1, w - 31);
            p1.Y = rand.Next(0, h / 2);
            ge.AddCircleGameObject(new CircleGameObject(p1, rand.Next(10, 30), Color.Orange));

            p1.X = rand.Next(1, w - 31);
            p1.Y = rand.Next(0, h / 2);
            ge.AddCircleGameObject(new CircleGameObject(p1, rand.Next(10, 30), Color.Brown));

            ge.Start(10);
            running = true;
            slowTimer.Enabled = true;
        }

        private void OnSlowTick(object sender, EventArgs e)
        {
            if(ge.GetNrOfCircleGameObjects() == 0)
            {
                ge.Stop();
            }
            else if(running)
            {
                if(left)
                {
                    p1.X = 0;
                    p1.Y = h;
                    p2.X = rand.Next(100, w - 100);
                    p2.Y = h + rand.Next(20, 60);
                    ge.AddLineGameObject(new LineGameObject(p1, p2));
                    left = false;
                }
                else
                {
                    p1.X = rand.Next(100, w - 100);
                    p1.Y = h + rand.Next(20, 60);
                    p2.X = w;
                    p2.Y = h;
                    ge.AddLineGameObject(new LineGameObject(p1, p2));
                    left = true;
                }
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            ge.SetScrollSpeed(0);
            running = false;
            slowTimer.Enabled = false;
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            ge.SetScrollSpeed(scrollSpeed);
            running = true;
            slowTimer.Enabled = true;
        }
    }

    public class CircleGameObject
    {
        private Point p; // position
        private int d; // diameter
        private double sx, sy; // speed
        private SolidBrush brush;

        public CircleGameObject(Point mp, int md, Color c)
        {
            p = mp;
            d = md;
            brush = new SolidBrush(c);
        }

        public int GetDiameter()
        {
            return d;
        }

        public Point GetPosition()
        {
            return p;
        }

        public void SetPosition(Point mp)
        {
            p = mp;
        }

        public double GetSpeedX()
        {
            return sx;
        }

        public double GetSpeedY()
        {
            return sy;
        }

        public void SetSpeed(double msx, double msy)
        {
            sx = msx;
            sy = msy;
        }

        public void Draw(Graphics g)
        {
            g.FillEllipse(brush, p.X, p.Y, d, d);
        }
    }

    public class LineGameObject
    {
        private Point p1, p2; // position
        private Pen blackPen;

        public LineGameObject(Point mp1, Point mp2)
        {
            p1 = mp1;
            p2 = mp2;
            blackPen = new Pen(Color.Black, 4f);
        }

        public Point GetPosition1()
        {
            return p1;
        }

        public Point GetPosition2()
        {
            return p2;
        }

        public void SetPosition(Point mp1, Point mp2)
        {
            p1 = mp1;
            p2 = mp2;
        }

        public void Draw(Graphics g)
        {
            g.DrawLine(blackPen, p1, p2);
        }
    }

    public class GameEngine
    {
        private int x0, y0, w, h, i, r;
        private bool collision, collided;
        private int scrollSpeed;
        private double gravitationalAcceleration, shockAbsorption, a1, al, ap, a2;
        private Timer timer;
        private Image img;
        private Graphics g, imgG;
        private Pen blackPen;
        private Point p1, p2;
        private List<CircleGameObject> cGO;
        private List<LineGameObject> lGO;
        private List<int> rolling;

        public GameEngine(int mx0, int my0, int mw, int mh, double gA, int sS, double sA, Graphics mg)
        {
            x0 = mx0;
            y0 = my0;
            w = mw;
            h = mh;
            gravitationalAcceleration = gA;
            scrollSpeed = sS;
            shockAbsorption = sA;
            g = mg;
            cGO = new List<CircleGameObject>();
            lGO = new List<LineGameObject>();
            rolling = new List<int>();
            img = new Bitmap(w, h);
            imgG = Graphics.FromImage(img);
            blackPen = new Pen(Color.Black, 2f);
            timer = new Timer();
            timer.Enabled = false;
            timer.Tick += new System.EventHandler(OnTick);
        }

        public void Start(int millis)
        {
            timer.Enabled = true;
            timer.Interval = millis;
        }

        public void Stop()
        {
            timer.Enabled = false;
        }

        public void SetScrollSpeed(int sS)
        {
            scrollSpeed = sS;
        }

        public void AddCircleGameObject(CircleGameObject c)
        {
            cGO.Add(c);
            rolling.Add(-1);
        }

        public void AddLineGameObject(LineGameObject l)
        {
            lGO.Add(l);
        }

        public int GetNrOfCircleGameObjects()
        {
            return cGO.Count;
        }

        private void OnTick(object sender, EventArgs e)
        {
            imgG.Clear(Color.White);

            for(i = 0; i < cGO.Count; i++)
            {
                if(cGO[i].GetPosition().Y + cGO[i].GetDiameter() < 0)
                {
                    cGO.RemoveAt(i);
                    rolling.RemoveAt(i);
                }
                else if(cGO[i].GetPosition().Y + cGO[i].GetDiameter() >= h || cGO[i].GetPosition().Y + cGO[i].GetSpeedY() + cGO[i].GetDiameter() >= h)
                {
                    cGO[i].SetSpeed(shockAbsorption * cGO[i].GetSpeedX(), shockAbsorption * (-cGO[i].GetSpeedY()) + gravitationalAcceleration);
                }
                else if (cGO[i].GetPosition().X <= 0 || cGO[i].GetPosition().X + cGO[i].GetDiameter() >= w || cGO[i].GetPosition().X + cGO[i].GetSpeedX() <= 0 || cGO[i].GetPosition().X + +cGO[i].GetSpeedX() + cGO[i].GetDiameter() >= w)
                {
                    cGO[i].SetSpeed(shockAbsorption * (-cGO[i].GetSpeedX()), shockAbsorption * cGO[i].GetSpeedY() + gravitationalAcceleration);
                }
                else
                {
                    collided = false;

                    for(int j = 0; j < lGO.Count; j++)
                    {
                        if(lGO[j].GetPosition1().X <= cGO[i].GetPosition().X + cGO[i].GetDiameter() / 2 && lGO[j].GetPosition2().X >= cGO[i].GetPosition().X + cGO[i].GetDiameter() / 2)
                        {
                            if(lGO[j].GetPosition1().X <= cGO[i].GetPosition().X + cGO[i].GetSpeedX() + cGO[i].GetDiameter() / 2 && lGO[j].GetPosition2().X >= cGO[i].GetPosition().X + cGO[i].GetSpeedX() + cGO[i].GetDiameter() / 2)
                            {
                                if(cGO[i].GetPosition().Y + cGO[i].GetDiameter() <= lGO[j].GetPosition1().Y + (lGO[j].GetPosition2().Y - lGO[j].GetPosition1().Y) * (cGO[i].GetPosition().X + cGO[i].GetDiameter() / 2 - lGO[j].GetPosition1().X) / (lGO[j].GetPosition2().X - lGO[j].GetPosition1().X))
                                {
                                    if(cGO[i].GetPosition().Y + cGO[i].GetDiameter() + cGO[i].GetSpeedY() > lGO[j].GetPosition1().Y + (lGO[j].GetPosition2().Y - lGO[j].GetPosition1().Y) * (cGO[i].GetPosition().X + cGO[i].GetDiameter() / 2 - lGO[j].GetPosition1().X) / (lGO[j].GetPosition2().X - lGO[j].GetPosition1().X))
                                    {
                                        if(Math.Abs(cGO[i].GetSpeedX()) > 3.0 || Math.Abs(cGO[i].GetSpeedY()) > 3.0)
                                        {
                                            collision = true;
                                            collided = true;
                                        }
                                        else
                                        {
                                            collision = false;
                                            rolling[i] = j;
                                        }
                                    }
                                    else
                                    {
                                        collision = false;
                                    }
                                }
                                else
                                {
                                    collision = false;
                                    if (rolling[i] == j)
                                    {
                                        rolling[i] = -1;
                                    }
                                }
                            }
                            else
                            {
                                collision = false;
                                if(rolling[i] == j)
                                {
                                    rolling[i] = -1;
                                }
                            }
                        }
                        else
                        {
                            collision = false;
                            if (rolling[i] == j)
                            {
                                rolling[i] = -1;
                            }
                        }

                        if(collision)
                        {
                            a1 = Math.Atan2(cGO[i].GetSpeedY(), cGO[i].GetSpeedX());
                            al = Math.Atan2((double)(lGO[j].GetPosition2().Y - lGO[j].GetPosition1().Y), (double)(lGO[j].GetPosition2().X - lGO[j].GetPosition1().X));
                            if(al <= 0)
                            {
                                ap = al + Math.PI / 2.0;
                            }
                            else
                            {
                                ap = al - Math.PI / 2.0;
                            }
                            a2 = 2.0 * ap - a1;

                            r = (int)(shockAbsorption * Math.Sqrt(Math.Pow(cGO[i].GetSpeedX(), 2) + Math.Pow(cGO[i].GetSpeedY(), 2)));
                            cGO[i].SetSpeed(-r * Math.Cos(a2), -r * Math.Sin(a2) + gravitationalAcceleration);
                        }
                    }

                    if(collided == false)
                    {
                        if (rolling[i] != -1)
                        {
                            al = Math.Atan2((double)(lGO[rolling[i]].GetPosition2().Y - lGO[rolling[i]].GetPosition1().Y), (double)(lGO[rolling[i]].GetPosition2().X - lGO[rolling[i]].GetPosition1().X));
                            a2 = al - Math.PI / 2.0;
                            cGO[i].SetSpeed(cGO[i].GetSpeedX() + gravitationalAcceleration * shockAbsorption * Math.Cos(a2), cGO[i].GetSpeedY() - gravitationalAcceleration * shockAbsorption * Math.Sin(a2));
                            while(cGO[i].GetPosition().Y + cGO[i].GetDiameter() + cGO[i].GetSpeedY() > lGO[rolling[i]].GetPosition1().Y + (lGO[rolling[i]].GetPosition2().Y - lGO[rolling[i]].GetPosition1().Y) * (cGO[i].GetPosition().X + cGO[i].GetDiameter() / 2 - lGO[rolling[i]].GetPosition1().X) / (lGO[rolling[i]].GetPosition2().X - lGO[rolling[i]].GetPosition1().X))
                            {
                                cGO[i].SetSpeed(cGO[i].GetSpeedX(), cGO[i].GetSpeedY() - 0.01);
                            }
                        }
                        else
                        {
                            cGO[i].SetSpeed(cGO[i].GetSpeedX(), cGO[i].GetSpeedY() + gravitationalAcceleration);
                        }
                    }
                }
            }

            for (i = 0; i < cGO.Count; i++)
            {
                p1.X = cGO[i].GetPosition().X + (int)cGO[i].GetSpeedX();
                p1.Y = cGO[i].GetPosition().Y + (int)cGO[i].GetSpeedY() - scrollSpeed;
                cGO[i].SetPosition(p1);
                cGO[i].Draw(imgG);
            }

            for (i = 0; i < lGO.Count; i++)
            {
                p1 = lGO[i].GetPosition1();
                p1.Y = p1.Y - scrollSpeed;
                p2 = lGO[i].GetPosition2();
                p2.Y = p2.Y - scrollSpeed;
                lGO[i].SetPosition(p1, p2);
                lGO[i].Draw(imgG);
            }

            imgG.DrawRectangle(blackPen, 1, 1, w - 2, h - 2);

            g.DrawImage(img, x0, y0, w, h);
        }
    }
}
