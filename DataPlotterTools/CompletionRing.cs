using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;


namespace DataPlotterTools
{
    public sealed class CompletionRing : Control
    {

        #region Private fields and constants

        private const string INNER_CIRCLE_NAME = "InnerCircle";
        private const string FILL_PATH_NAME = "FillPath";
        private const string INFO_TEXT_NAME = "DataTextBlock";

        #endregion

        #region Public Properties and Dependency Properties Registration

        public double Diameter
        {
            get { return (double)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RadiusProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DiameterProperty =
            DependencyProperty.Register("Diameter", typeof(double), typeof(CompletionRing), new PropertyMetadata(130.0D, OnDiameterChanged));


        public double RingThickness
        {
            get { return (double)GetValue(RingThicknessProperty); }
            set { SetValue(RingThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RingThicknessProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RingThicknessProperty =
            DependencyProperty.Register("RingThickness", typeof(double), typeof(CompletionRing), new PropertyMetadata(17.0D, OnThicknessChanged));


        public double PercentageComplete
        {
            get { return (double)GetValue(PercentageCompleteProperty); }
            set { SetValue(PercentageCompleteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PercentageCompleteProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PercentageCompleteProperty =
            DependencyProperty.Register("PercentageComplete", typeof(double), typeof(CompletionRing), new PropertyMetadata(0.0D, OnPercentageCompletedChanged));


        public SolidColorBrush ZeroCompletionForeground
        {
            get { return (SolidColorBrush)GetValue(ZeroCompletionForegroundProperty); }
            set { SetValue(ZeroCompletionForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZeroCompletionBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZeroCompletionForegroundProperty =
            DependencyProperty.Register("ZeroCompletionForeground", typeof(SolidColorBrush), typeof(CompletionRing), new PropertyMetadata(new SolidColorBrush(Colors.Red)));


        public SolidColorBrush FullCompletionForeground
        {
            get { return (SolidColorBrush)GetValue(FullCompletionForegroundProperty); }
            set { SetValue(FullCompletionForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FullCompletionForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FullCompletionForegroundProperty =
            DependencyProperty.Register("FullCompletionForeground", typeof(SolidColorBrush), typeof(CompletionRing), new PropertyMetadata(new SolidColorBrush(Colors.Green)));


        public Brush VoidRegionForeground
        {
            get { return (Brush)GetValue(VoidRegionForegroundProperty); }
            set { SetValue(VoidRegionForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VoidRegionForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VoidRegionForegroundProperty =
            DependencyProperty.Register("VoidRegionForeground", typeof(Brush), typeof(CompletionRing), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));


        public SolidColorBrush InnerBackground
        {
            get { return (SolidColorBrush)GetValue(InnerBackgroundProperty); }
            set { SetValue(InnerBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Background.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InnerBackgroundProperty =
            DependencyProperty.Register("InnerBackground", typeof(SolidColorBrush), typeof(CompletionRing), null);


        public SweepDirection SweepDirection
        {
            get { return (SweepDirection)GetValue(ProgressSweepDirectionProperty); }
            set { SetValue(ProgressSweepDirectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressSweepDirection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressSweepDirectionProperty =
            DependencyProperty.Register("SweepDirection", typeof(SweepDirection), typeof(CompletionRing), new PropertyMetadata(SweepDirection.Counterclockwise));

        #endregion

        #region Property Change Callback Handling

        private static void OnDiameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CompletionRing cur = d as CompletionRing;
            if (cur != null)
            {
                if ((double)e.NewValue < 0)
                {
                    cur.Diameter = 0.0d;
                    cur.RingThickness = 0.0d;
                }
                else
                {
                    if (cur.Diameter / 2.0 < cur.RingThickness)
                        cur.RingThickness = cur.Diameter / 2.0;
                }
            }
        }

        private static void OnThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CompletionRing cur = d as CompletionRing;
            if (cur != null)
            {
                if ((double)e.NewValue < 0)
                {
                    cur.RingThickness = 0.0d;
                }
                else
                {
                    if (cur.RingThickness > cur.Diameter / 2.0)
                        cur.RingThickness = cur.Diameter / 2.0;
                }
            }
        }

        private static void OnPercentageCompletedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            CompletionRing cur = d as CompletionRing;
            if (cur != null)
            {
                if ((double)e.NewValue < 0)
                    cur.PercentageComplete = 0;
                if ((double)e.NewValue > 100)
                    cur.PercentageComplete = 100;

                cur.OnApplyTemplate();
            }
        }

        #endregion

        #region Constructors

        public CompletionRing()
        {
            this.DefaultStyleKey = typeof(CompletionRing);
        }

        #endregion

        #region Methods

        protected override void OnApplyTemplate()
        {
            Ellipse innerCircle = this.GetTemplateChild(INNER_CIRCLE_NAME) as Ellipse;
            if (innerCircle != null)
            {
                innerCircle.Width = innerCircle.Height = (Diameter - 2 * RingThickness);
            }

            Path fillPath = this.GetTemplateChild(FILL_PATH_NAME) as Path;
            if (fillPath != null)
            {
                double radius = Diameter / 2.0;

                if (PercentageComplete < 100)
                {
                    PathGeometry pieSlice = new PathGeometry();

                    PathFigure path = new PathFigure();
                    path.StartPoint = new Point(radius, radius);

                    LineSegment startLine = new LineSegment();
                    startLine.Point = new Point(radius, 0.0d);

                    ArcSegment arc = new ArcSegment();
                    arc.Size = new Size(radius, radius);
                    arc.SweepDirection = SweepDirection;
                    arc.Point = GetEndPoint();
                    arc.IsLargeArc = PercentageComplete > 50 ? true : false;

                    path.Segments.Add(startLine);
                    path.Segments.Add(arc);
                    path.IsClosed = true;

                    pieSlice.Figures.Add(path);
                    fillPath.Data = pieSlice;
                }
                else
                {
                    EllipseGeometry fullCircle = new EllipseGeometry();
                    fullCircle.Center = new Point(radius, radius);
                    fullCircle.RadiusX = fullCircle.RadiusY = radius;
                    fillPath.Data = fullCircle;
                }

                fillPath.Fill = GetInterpolatedBrush();
            }

            TextBlock dataText = this.GetTemplateChild(INFO_TEXT_NAME) as TextBlock;
            if (dataText != null)
            {
                dataText.Text = (PercentageComplete / 100).ToString("P0");
            }

            base.OnApplyTemplate();
        }

        private SolidColorBrush GetInterpolatedBrush()
        {
            Color ci = ZeroCompletionForeground.Color;
            Color cf = FullCompletionForeground.Color;

            byte[] newValues = new byte[4];
            int[] iValues = new int[4] { ci.R, ci.G, ci.B, ci.A };
            int[] fValues = new int[4] { cf.R, cf.G, cf.B, cf.A };

            for (int i = 0; i < 4; i++)
                newValues[i] = (byte)(iValues[i] + (fValues[i] - iValues[i]) * PercentageComplete / 100);

            Color color = new Color();
            color.R = newValues[0];
            color.G = newValues[1];
            color.B = newValues[2];
            color.A = newValues[3];
            return new SolidColorBrush(color);
        }

        private Point GetEndPoint()
        {
            // Converting percentage complete to radians
            double radians = PercentageComplete / 100 * 2 * Math.PI;
            double radius = Diameter / 2;

            // The original formulae for (x, y) coordinates on a circle are calculated from X-Axis.
            // Shfiting to Y-Axis by using rotation matrix, { {0, -1}, {1, 0} } on {r cos t, r sin t}.
            double x = -1 * Math.Sin(radians);
            double y = Math.Cos(radians);

            // Flipping X-Axis if rotation is clockwise:
            if (SweepDirection == Windows.UI.Xaml.Media.SweepDirection.Clockwise)
                x = x * -1;

            // Shifting points to conform with standard screen axes.
            y = y * -1;

            // Localising point locations for the control size
            x = (x + 1) * radius;
            y = (y + 1) * radius;

            return new Point(x, y);
        }

        #endregion

    }
}
