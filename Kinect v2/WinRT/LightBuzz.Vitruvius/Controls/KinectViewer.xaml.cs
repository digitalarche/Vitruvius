﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using WindowsPreview.Kinect;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace LightBuzz.Vitruvius.Controls
{
    public sealed partial class KinectViewer : UserControl
    {
        #region Constants

        /// <summary>
        /// The default drawing brush.
        /// </summary>
        static readonly Brush DEFAULT_BRUSH = new SolidColorBrush(Colors.LightCyan);

        /// <summary>
        /// The default circle size.
        /// </summary>
        static readonly double DEFAULT_RADIUS = 15;

        /// <summary>
        /// The default line thickness.
        /// </summary>
        static readonly double DEFAULT_THICKNESS = 8;

        #endregion

        #region Members

        /// <summary>
        /// A list of all the body visuals.
        /// </summary>
        List<BodyVisual> _bodyVisuals = new List<BodyVisual>();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of KinectViewer.
        /// </summary>
        public KinectViewer()
        {
            InitializeComponent();

            DataContext = this;
        }

        #endregion

        #region Dependency properties

        /// <summary>
        /// The coordinate mapper used to convert between the 3D world and the 2D screen coordinates.
        /// </summary>
        public CoordinateMapper CoordinateMapper
        {
            get { return (CoordinateMapper)GetValue(CoordinateMapperProperty); }
            set { SetValue(CoordinateMapperProperty, value); }
        }
        public static readonly DependencyProperty CoordinateMapperProperty =
            DependencyProperty.Register("CoordinateMapper", typeof(CoordinateMapper), typeof(KinectViewer), new PropertyMetadata(KinectSensor.GetDefault().CoordinateMapper));

        /// <summary>
        /// The visualization mode of the control (Color, Depth, Infrared). Defaults to Color.
        /// </summary>
        public Visualization Visualization
        {
            get { return (Visualization)GetValue(VisualizationProperty); }
            set { SetValue(VisualizationProperty, value); }
        }
        public static readonly DependencyProperty VisualizationProperty =
            DependencyProperty.Register("Visualization", typeof(Visualization), typeof(KinectViewer), new PropertyMetadata(Visualization.Color));

        /// <summary>
        /// The image to display.
        /// </summary>
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(KinectViewer), new PropertyMetadata(null));

        #endregion

        #region Public methods

        /// <summary>
        /// Clears the visual data from the canvas.
        /// </summary>
        public void Clear()
        {
            canvas.Children.Clear();

            foreach (var visual in _bodyVisuals)
            {
                visual.Clear();
            }

            _bodyVisuals.Clear();
        }

        /// <summary>
        /// Maps the 3D point to its corresponding 2D point.
        /// </summary>
        /// <param name="position">The 3D space point.</param>
        /// <returns>The X, Y coordinates of the point.</returns>
        public Point GetPoint(CameraSpacePoint position)
        {
            Point point;

            switch (Visualization)
            {
                case Visualization.Color:
                    {
                        ColorSpacePoint colorPoint = CoordinateMapper.MapCameraPointToColorSpace(position);
                        point.X = float.IsInfinity(colorPoint.X) ? 0.0 : colorPoint.X;
                        point.Y = float.IsInfinity(colorPoint.Y) ? 0.0 : colorPoint.Y;
                    }
                    break;
                case Visualization.Depth:
                case Visualization.Infrared:
                    {
                        DepthSpacePoint depthPoint = CoordinateMapper.MapCameraPointToDepthSpace(position);
                        point.X = float.IsInfinity(depthPoint.X) ? 0.0 : depthPoint.X;
                        point.Y = float.IsInfinity(depthPoint.Y) ? 0.0 : depthPoint.Y;
                    }
                    break;
                default:
                    break;
            }

            return point;
        }

        /// <summary>
        /// Draws the specified body.
        /// </summary>
        /// <param name="body">The body to draw.</param>
        /// <param name="jointRadius">The size of the joint ellipses.</param>
        /// <param name="jointBrush">The brush used to draw the joints.</param>
        /// <param name="boneThickness">The thickness of the bone lines.</param>
        /// <param name="boneBrush">The brush used to draw the bones.</param>
        public void DrawBody(Body body, double jointRadius, Brush jointBrush, double boneThickness, Brush boneBrush)
        {
            if (body == null || !body.IsTracked) return;

            BodyVisual visual = _bodyVisuals.Where(b => b.TrackingId == body.TrackingId).FirstOrDefault();

            if (visual == null)
            {
                visual = BodyVisual.Create(body.TrackingId, body.Joints.Keys, jointRadius, jointBrush, boneThickness, boneBrush);

                foreach (var ellipse in visual.Joints.Values)
                {
                    canvas.Children.Add(ellipse);
                }

                foreach (var line in visual.Bones.Values)
                {
                    canvas.Children.Add(line);
                }

                _bodyVisuals.Add(visual);
            }

            foreach (var joint in body.Joints)
            {
                Point point = GetPoint(joint.Value.Position);

                visual.UpdateJoint(joint.Key, point);
            }

            foreach (var bone in visual.CONNECTIONS)
            {
                Point first = GetPoint(body.Joints[bone.Item1].Position);
                Point second = GetPoint(body.Joints[bone.Item2].Position);

                visual.UpdateBone(bone, first, second);
            }
        }

        /// <summary>
        /// Draws the specified body.
        /// </summary>
        /// <param name="body">The body to draw</param>
        public void DrawBody(Body body)
        {
            DrawBody(body, DEFAULT_RADIUS, DEFAULT_BRUSH, DEFAULT_THICKNESS, DEFAULT_BRUSH);
        }

        #endregion
    }

    /// <summary>
    /// Represents a body visualization for drawing joints and bones on a canvas.
    /// </summary>
    class BodyVisual
    {
        #region Constants

        /// <summary>
        /// The joint connections (bones).
        /// </summary>
        public readonly List<Tuple<JointType, JointType>> CONNECTIONS = new List<Tuple<JointType, JointType>>
        {
            // Torso
            new Tuple<JointType, JointType>(JointType.Head, JointType.Neck),
            new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder),
            new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid),
            new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase),
            new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight),
            new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft),
            new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight),
            new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft),

            // Right Arm
            new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight),
            new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight),
            new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight),
            new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight),
            new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight),

            // Left Arm
            new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft),
            new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft),
            new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft),
            new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft),
            new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft),

            // Right Leg
            new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight),
            new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight),
            new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight),

            // Left Leg
            new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft),
            new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft),
            new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft)
        };

        #endregion

        #region Properties

        /// <summary>
        /// The tracking ID of the corresponding body.
        /// </summary>
        public ulong TrackingId { get; set; }

        /// <summary>
        /// The joints of the body and their corresponding ellipses.
        /// </summary>
        public Dictionary<JointType, Ellipse> Joints { get; set; }

        /// <summary>
        /// The bones of the body and their corresponding lines.
        /// </summary>
        public Dictionary<Tuple<JointType, JointType>, Line> Bones { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of BodyVisual.
        /// </summary>
        public BodyVisual()
        {
            Joints = new Dictionary<JointType, Ellipse>();
            Bones = new Dictionary<Tuple<JointType, JointType>, Line>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Cleares the joints and bones.
        /// </summary>
        public void Clear()
        {
            Joints.Clear();
            Bones.Clear();
        }

        /// <summary>
        /// Adds the specified joint to the collection.
        /// </summary>
        /// <param name="joint">The joint type.</param>
        /// <param name="radius">The size of the ellipse</param>
        /// <param name="brush">The brush used to fill the ellipse.</param>
        public void AddJoint(JointType joint, double radius, Brush brush)
        {
            Joints.Add(joint, new Ellipse
            {
                Width = radius,
                Height = radius,
                Fill = brush
            });
        }

        /// <summary>
        /// Adds a bone to the collection.
        /// </summary>
        /// <param name="joints">The start and end of the line segment.</param>
        /// <param name="thickness">The thickness of the line.</param>
        /// <param name="brush">The brush used to fill the line.</param>
        public void AddBone(Tuple<JointType, JointType> joints, double thickness, Brush brush)
        {
            Bones.Add(joints, new Line
            {
                StrokeThickness = thickness,
                Stroke = brush
            });
        }

        /// <summary>
        /// Updates the position of the specified joint.
        /// </summary>
        /// <param name="joint">The joint type.</param>
        /// <param name="point">The position of the joint.</param>
        public void UpdateJoint(JointType joint, Point point)
        {
            Ellipse ellipse = Joints[joint];

            Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);
        }

        /// <summary>
        /// Updates the position of the specified bone.
        /// </summary>
        /// <param name="bone">The start and end of the line segment.</param>
        /// <param name="first">The position of the start of the segment.</param>
        /// <param name="second">The position of the end of the segment.</param>
        public void UpdateBone(Tuple<JointType, JointType> bone, Point first, Point second)
        {
            Line line = Bones[bone];

            line.X1 = first.X;
            line.Y1 = first.Y;
            line.X2 = second.X;
            line.Y2 = second.Y;
        }

        /// <summary>
        /// Creates a new BodyVisual object with the specified parameters.
        /// </summary>
        /// <param name="trackingId">The tracking ID of the corresponding body.</param>
        /// <param name="joints">The joint types of the body.</param>
        /// <param name="jointRadius">The desired joint size.</param>
        /// <param name="jointBrush">The desired joint brush.</param>
        /// <param name="boneThickness">The desired line thickness.</param>
        /// <param name="boneBrush">The desired line brush.</param>
        /// <returns>A new instance of BodyVisual.</returns>
        public static BodyVisual Create(ulong trackingId, IEnumerable<JointType> joints, double jointRadius, Brush jointBrush, double boneThickness, Brush boneBrush)
        {
            BodyVisual bodyVisual = new BodyVisual
            {
                TrackingId = trackingId
            };

            foreach (var joint in joints)
            {
                bodyVisual.AddJoint(joint, jointRadius, jointBrush);
            }

            foreach (var bone in bodyVisual.CONNECTIONS)
            {
                bodyVisual.AddBone(bone, boneThickness, boneBrush);
            }

            return bodyVisual;
        }

        #endregion
    }
}
