using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RouteEngine;

namespace Gui
{
    public partial class Form1 : Form
    {
        bool _addLoc = false;
        List<GuiLocation> _guiLocations = new List<GuiLocation>();
        List<Connection> _connections = new List<Connection>();
        

        GuiLocation _selectedGuiLocation=null;
        Color normalColor;

        public Form1()
        {
            InitializeComponent();
            normalColor = btnAddLoc.BackColor;
        }

        private void btnAddLoc_Click(object sender, EventArgs e)
        {
            if (_addLoc)
            {
                _addLoc = false;
                btnAddLoc.BackColor = normalColor;
            }
            else
            {
                _addLoc = true;
                btnAddLoc.BackColor = Color.Red;
            }
        }

        private void pnlView_Click(object sender, EventArgs e)
        {

        }

        private void pnlView_MouseDown(object sender, MouseEventArgs e)
        {
            if (_addLoc)
            {

                if (getGuiLocationAtPoint(e.X, e.Y) == null)
                {
                    GuiLocation _guiLocation = new GuiLocation();
                    _guiLocation.Identifier = _guiLocations.Count().ToString();
                    _guiLocation.X = e.X;
                    _guiLocation.Y = e.Y;
                    _guiLocations.Add(_guiLocation);
                    cmbLocations.Items.Add(_guiLocation);
                }
            }
            else
            {
                GuiLocation _guiLocation = getGuiLocationAtPoint(e.X, e.Y);
                if (_guiLocation != null)
                {
                    if (_selectedGuiLocation != null)
                    {
                        int weight = 0;
                        if (chkRandom.Checked)
                        {
                            Random random=new Random();
                            weight = random.Next(1, 25);
                        }
                        else
                        {
                            weight = int.Parse(txtweight.Text);
                        }
                        Connection connection = new Connection(_selectedGuiLocation, _guiLocation, weight);
                        
                        _connections.Add(connection);
                        _selectedGuiLocation.Selected = false;

                        _selectedGuiLocation = null;
                    }
                    else
                    {
                        _guiLocation.Selected = true;
                        _selectedGuiLocation = _guiLocation;
                    }
                }
            }
            PaintGui();
        }

        GuiLocation getGuiLocationAtPoint(int x, int y)
        {
            foreach (GuiLocation _guiLocation in _guiLocations)
            {
                int x2=x-_guiLocation.X;
                int y2=y-_guiLocation.Y;
                int xToCompare = _guiLocation.Width / 2;
                int yToCompare = _guiLocation.Width / 2;

                if (x2 >= xToCompare * -1 && x2 < xToCompare && y2 > yToCompare * -1 && y2 < yToCompare)
                {
                    return _guiLocation;
                }
            }
            
            return null;
        }


        private void pnlView_Paint(object sender, PaintEventArgs e)
        {
            PaintGui();
        }

        void PaintGui()
        {
            Brush _brushRed = new SolidBrush(Color.Red);
            Brush _brushBlack = new SolidBrush(Color.Black);
            Brush _brushWhite = new SolidBrush(Color.White);
            Brush _brushBlue = new SolidBrush(Color.Blue);
            Font _font = new Font(FontFamily.GenericSansSerif, 15);
            Pen _penBlue = new Pen(_brushBlue);
            Pen _penRed = new Pen(_brushRed);

            foreach (GuiLocation _guiLocation in _guiLocations)
            {
                int _x = _guiLocation.X - _guiLocation.Width / 2;
                int _y = _guiLocation.Y - _guiLocation.Width / 2;

                if (_guiLocation.Selected)
                    pnlView.CreateGraphics().FillEllipse(_brushRed, _x, _y, _guiLocation.Width, _guiLocation.Width);
                else
                    pnlView.CreateGraphics().FillEllipse(_brushBlack, _x, _y, _guiLocation.Width, _guiLocation.Width);
                pnlView.CreateGraphics().DrawString(_guiLocation.Identifier, _font, _brushWhite, _x, _y);
            }

            foreach (Connection _connection in _connections)
            {
                Point point1 = new Point(((GuiLocation)_connection.A).X, ((GuiLocation)_connection.A).Y);
                Point point2 = new Point(((GuiLocation)_connection.B).X, ((GuiLocation)_connection.B).Y);

                Point Pointref = Point.Subtract(point2, new Size(point1));
                double degrees = Math.Atan2(Pointref.Y, Pointref.X);
                double cosx1 = Math.Cos(degrees);
                double siny1 = Math.Sin(degrees);

                double cosx2 = Math.Cos(degrees + Math.PI);
                double siny2 = Math.Sin(degrees + Math.PI);

                int newx = (int)(cosx1 * (float)((GuiLocation)_connection.A).Width + (float)point1.X);
                int newy = (int)(siny1 * (float)((GuiLocation)_connection.A).Width + (float)point1.Y);

                int newx2 = (int)(cosx2 * (float)((GuiLocation)_connection.B).Width + (float)point2.X);
                int newy2 = (int)(siny2 * (float)((GuiLocation)_connection.B).Width + (float)point2.Y);

                
                if (_connection.Selected)
                {
                    pnlView.CreateGraphics().DrawLine(_penRed, new Point(newx, newy), new Point(newx2, newy2));
                    pnlView.CreateGraphics().FillEllipse(_brushRed, newx - 4, newy - 4, 8, 8);
                }
                else
                {
                    pnlView.CreateGraphics().DrawLine(_penBlue, new Point(newx, newy), new Point(newx2, newy2));
                    pnlView.CreateGraphics().FillEllipse(_brushBlue, newx - 4, newy - 4, 8, 8);
                }
                pnlView.CreateGraphics().DrawString(_connection.Weight.ToString(), _font, _brushBlue, newx - 4, newy - 4);
            }
        }

        private void btnCalc_Click(object sender, EventArgs e)
        {
            if (cmbLocations.SelectedIndex != -1)
            {
                RouteEngine.RouteEngine _routeEngine = new RouteEngine.RouteEngine();
                foreach (Connection connection in _connections)
                {
                    _routeEngine.Connections.Add(connection);
                }

                foreach (Location _location in _guiLocations)
                {
                    _routeEngine.Locations.Add(_location);
                }

                Dictionary<Location, Route> _shortestPaths = _routeEngine.CalculateMinCost((Location)cmbLocations.SelectedItem);
                listBox1.Items.Clear();

                List<Location> _shortestLocations = (List<Location>)(from s in _shortestPaths
                                                                     orderby s.Value.Cost
                                                                     select s.Key).ToList();
                foreach (Location _location in _shortestLocations)
                {
                    listBox1.Items.Add(_shortestPaths[_location]);
                }
            }
            else
            {
                MessageBox.Show("Please select a position");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Route route = (Route)listBox1.SelectedItem;
            foreach (Connection _connection in _connections)
            {
                _connection.Selected = false;
            }

            foreach (Connection _connection in route.Connections)
            {
                _connection.Selected = true;
            }
            PaintGui();
        }


    }
}
