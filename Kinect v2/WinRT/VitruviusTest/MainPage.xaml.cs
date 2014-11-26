﻿using LightBuzz.Vitruvius;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WindowsPreview.Kinect;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VitruviusTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IEnumerable<Body> _bodies;
        GestureController _gestureController;
        UsersReporter _userReporter;

        StreamRecorder<ColorFrame> _colorStreamRecorder = new ColorStreamRecorder();
        StreamRecorder<DepthFrame> _depthStreamRecorder = new DepthStreamRecorder();
        StreamRecorder<InfraredFrame> _infraredStreamRecorder = new InfraredStreamRecorder();

        public MainPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                _gestureController = new GestureController(GestureType.All);
                _gestureController.GestureRecognized += GestureController_GestureRecognized;

                _userReporter = new UsersReporter();
                _userReporter.BodyEntered += ActiveUserReporter_BodyEntered;
                _userReporter.BodyLeft += ActiveUserReporter_BodyLeft;
                _userReporter.Start();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_userReporter != null)
            {
                _userReporter.Stop();
            }

            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_bodies != null)
            {
                if (_bodies.Count() > 0)
                {
                    foreach (var body in _bodies)
                    {
                        body.Dispose();
                    }
                }
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (viewer.Visualization == Visualization.Color)
                    {
                        viewer.Image = frame.ToBitmap();

                        if (_colorStreamRecorder.IsRecording)
                        {
                            _colorStreamRecorder.Update(frame);
                        }
                    }
                }
            }

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (viewer.Visualization == Visualization.Depth)
                    {
                        viewer.Image = frame.ToBitmap();

                        if (_depthStreamRecorder.IsRecording)
                        {
                            _depthStreamRecorder.Update(frame);
                        }
                    }
                }
            }

            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (viewer.Visualization == Visualization.Infrared)
                    {
                        viewer.Image = frame.ToBitmap();

                        if (_infraredStreamRecorder.IsRecording)
                        {
                            _infraredStreamRecorder.Update(frame);
                        }
                    }
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    _bodies = frame.Bodies();

                    tblHeights.ClearValue(TextBlock.TextProperty);

                    foreach (var body in _bodies)
                    {
                        viewer.DrawBody(body);

                        if (body.IsTracked)
                        {
                            _gestureController.Update(body);

                            tblHeights.Text += Math.Round(body.Height(), 2).ToString() + "m\n";
                        }
                    }

                    _userReporter.Update(_bodies);
                }
            }
        }

        void GestureController_GestureRecognized(object sender, GestureEventArgs e)
        {
            // Display the gesture type.
            tblGestures.Text = e.Name;

            // Do something according to the type of the gesture.
            switch (e.Type)
            {
                case GestureType.JoinedHands:
                    break;
                case GestureType.Menu:
                    break;
                case GestureType.SwipeDown:
                    break;
                case GestureType.SwipeLeft:
                    break;
                case GestureType.SwipeRight:
                    break;
                case GestureType.SwipeUp:
                    break;
                case GestureType.WaveLeft:
                    break;
                case GestureType.WaveRight:
                    break;
                case GestureType.ZoomIn:
                    break;
                case GestureType.ZoomOut:
                    break;
                default:
                    break;
            }
        }

        void ActiveUserReporter_BodyEntered(object sender, ActiveUserReporterEventArgs e)
        {
        }

        void ActiveUserReporter_BodyLeft(object sender, ActiveUserReporterEventArgs e)
        {
            viewer.Clear();
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            viewer.Visualization = Visualization.Color;
        }

        private void Depth_Click(object sender, RoutedEventArgs e)
        {
            viewer.Visualization = Visualization.Depth;
        }

        private void Infrared_Click(object sender, RoutedEventArgs e)
        {
            viewer.Visualization = Visualization.Infrared;
        }

        private async void RecordColor_Click(object sender, RoutedEventArgs e)
        {
            if (_colorStreamRecorder.IsRecording)
            {
                _colorStreamRecorder.Stop();

                (sender as Button).Content = "Record Color";
            }
            else
            {
                StorageFile file = await PickFile();

                _colorStreamRecorder.Stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                _colorStreamRecorder.Start();

                (sender as Button).Content = "Stop";
            }
        }

        private async void RecordDepth_Click(object sender, RoutedEventArgs e)
        {
            if (_depthStreamRecorder.IsRecording)
            {
                _depthStreamRecorder.Stop();

                (sender as Button).Content = "Record Depth";
            }
            else
            {
                StorageFile file = await PickFile();

                _depthStreamRecorder.Stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                _depthStreamRecorder.Start();

                (sender as Button).Content = "Stop";
            }
        }

        private async void RecordInfrared_Click(object sender, RoutedEventArgs e)
        {
            if (_infraredStreamRecorder.IsRecording)
            {
                _infraredStreamRecorder.Stop();

                (sender as Button).Content = "Record Infrared";
            }
            else
            {
                StorageFile file = await PickFile();

                _infraredStreamRecorder.Stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                _infraredStreamRecorder.Start();

                (sender as Button).Content = "Stop";
            }
        }

        private async Task<StorageFile> PickFile()
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("Windows Media Video", new[] { ".wmv" });

            return await picker.PickSaveFileAsync();
        }
    }
}
