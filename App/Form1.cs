﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using App.Figures;
using App.Utils;

namespace App {

    public partial class Form1 : Form {

        private static readonly Color PrimaryColor = Color.Orange;
        private static readonly Color SecondaryColor = Color.LightSeaGreen;

        private readonly Pen _drawPen = new Pen(Color.Black, 1);
        private readonly Bitmap _image;
        private readonly Graphics _g;
        private readonly List<Point> _vertexList = new List<Point>();
        private readonly List<IDrawable> _figureList = new List<IDrawable>();
        private Polygon _primaryPgn;
        private Polygon _secondaryPgn;
        private int _operation = 1;
        private Point _mouseStartPosition;

        public Form1() {
            InitializeComponent();
            _image = new Bitmap(PictureBox.Width, PictureBox.Height);
            _g = Graphics.FromImage(_image);
        }

        // Ввод списка вершин и в конце - рисование
        private void InputPgn(MouseEventArgs e) {
            var newP = new Point(e.X, e.Y);
            _vertexList.Add(newP);
            var k = _vertexList.Count;
            if (k > 1) _g.DrawLine(_drawPen, _vertexList[k - 2], _vertexList[k - 1]);
            else _g.DrawRectangle(_drawPen, e.X, e.Y, 1, 1);

            if (e.Button == MouseButtons.Right) // Конец ввода
            {
                _g.DrawLine(new Pen(Color.Blue), _vertexList[k - 1], _vertexList[0]);
                var pgn = new Polygon(_drawPen.Color);
                _vertexList.ForEach(pgn.Add);
                _vertexList.Clear();
                pgn.Draw(_g);
                _figureList.Add(pgn);
            }
        }

        private void Clear_Click(object sender, EventArgs e) {
            _g.Clear(PictureBox.BackColor);
            _figureList.ForEach(p => p.Clear());
            _figureList.Clear();
            _operation = 1;
            PictureBox.Image = _image;
        }

        private void Draw_Click(object sender, EventArgs e) {
            _operation = 1; // Задает режим рисования фигуры
        }

        private void Move_Click(object sender, EventArgs e) {
            _operation = 2; // Задает режим перемещения фигуры
        }

        private void Rotate_Click(object sender, EventArgs e) {
            _operation = 3; // Задает режим вращения фигуры
        }

        private void Scale_Click(object sender, EventArgs e) {
            _operation = 4; // Задает режим масштабирования фигуры
        }

        private void TMO_Click(object sender, EventArgs e) {
            var tmo = TmoSelector.SelectedIndex + 1;

            _g.Clear(PictureBox.BackColor);
            PictureBox.Image = _image;
            var a = Tmo.Exe(_primaryPgn, _secondaryPgn, tmo, PictureBox.Width, PictureBox.Height, _g);
        }

        private void ColorSelector_SelectedIndexChanged(object sender, EventArgs e) {
            switch (ColorSelector.SelectedIndex) // выбор цвета
            {
                case 0:
                    _drawPen.Color = Color.Black;
                    break;
                case 1:
                    _drawPen.Color = Color.Red;
                    break;
                case 2:
                    _drawPen.Color = Color.Green;
                    break;
                case 3:
                    _drawPen.Color = Color.Blue;
                    break;
                case 4:
                    _drawPen.Color = Color.White;
                    break;
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e) {
            var selectedPgn = _figureList.OfType<Polygon>().FirstOrDefault(p => p.ThisPgn(e.X, e.Y));
            if (selectedPgn != null) {
                if (e.Button == MouseButtons.Left) {
                    if (_primaryPgn != null) {
                        _primaryPgn.BorderColor = null;
                    }

                    if (selectedPgn == _secondaryPgn && _primaryPgn != null) {
                        _secondaryPgn = _primaryPgn;
                        _secondaryPgn.BorderColor = SecondaryColor;
                    }

                    _primaryPgn = selectedPgn;
                    _primaryPgn.BorderColor = PrimaryColor;
                    Redraw();
                } else if (e.Button == MouseButtons.Right) {
                    if (_secondaryPgn != null) {
                        _secondaryPgn.BorderColor = null;
                    }

                    if (selectedPgn == _primaryPgn && _secondaryPgn != null) {
                        _primaryPgn = _secondaryPgn;
                        _primaryPgn.BorderColor = PrimaryColor;
                    }

                    _secondaryPgn = selectedPgn;
                    _secondaryPgn.BorderColor = SecondaryColor;
                    Redraw();
                }
            }

            _mouseStartPosition = e.Location;
            switch (_operation) {
                case 1: // ввод вершин и рисование
                {
                    InputPgn(e);
                    if (e.Button == MouseButtons.Right) _operation = 0;
                }
                    break;
                case 2: // выделение многоугольника
                case 3: // вращение
                case 4: // масштабирование
                    if (_primaryPgn != null) {
                        _g.DrawEllipse(new Pen(Color.Blue), e.X - 2, e.Y - 2, 5, 5);
                    }

                    break;
            }

            // копирование из _image в PictureBox.Image
            PictureBox.Image = _image;
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e) {
            if (_primaryPgn != null && e.Button == MouseButtons.Left) {
                switch (_operation) {
                    case 2:
                        _primaryPgn.Move(e.X - _mouseStartPosition.X, e.Y - _mouseStartPosition.Y);
                        Redraw();
                        _mouseStartPosition = e.Location;
                        break;
                    case 3:
                        _primaryPgn.Rotate(_mouseStartPosition,
                            (e.X > _mouseStartPosition.X ? 1 : -1) * (Math.PI / 180.0));
                        Redraw();
                        break;
                    case 4:
                        _primaryPgn.Scale(_mouseStartPosition, e.X > _mouseStartPosition.X ? 1.01 : 0.99);
                        Redraw();
                        break;
                }
            }
        }

        private void Redraw() {
            _g.Clear(PictureBox.BackColor);
            _figureList.ForEach(f => f.Draw(_g));
            PictureBox.Image = _image;
        }

    }

}