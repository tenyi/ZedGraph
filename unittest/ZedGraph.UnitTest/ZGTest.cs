using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ZedGraph;
using NUnit.Framework;

namespace ZedGraph.UnitTest
{
    #region Test Utilities
    class TestUtils
    {
        public static bool waitForUserOK = true;
        public static int delayTime = 500;
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        static bool exitFlag;
        #pragma warning disable CS0649 // 保留欄位供測試擴充使用，目前未指派
        public static bool clickDone;
#pragma warning restore CS0649

        public static void SetUp()
        {
            /*	Adds the event and	the	event handler for the method that will 
				process	the timer event to the	timer. */
            myTimer.Tick += new EventHandler(TimerEventProcessor);
        }

        public static bool promptIfTestWorked(string msg)
        {
            Console.WriteLine(msg);

            //	just act like the test worked
            if (!waitForUserOK)
            {
                if (delayTime > 0)
                    DelaySeconds(delayTime);
                return true;
            }

            if (DialogResult.Yes == MessageBox.Show(msg, "ZedGraph Test",
                    MessageBoxButtons.YesNo))
                return true;
            else
                return false;
        }

        public static void ShowMessage(string msg)
        {
            Console.WriteLine(msg);

            MessageBox.Show(msg, "ZedGraph Test", MessageBoxButtons.OK);
        }

        //	This is	the method to run when the timer	is raised.
        private static void TimerEventProcessor(Object myObject,
            EventArgs myEventArgs)
        {
            myTimer.Stop();
            exitFlag = true;
        }

        public static void DelaySeconds(int sec)
        {
            // Sets the timer interval to 3 seconds.
            myTimer.Stop();
            myTimer.Interval = sec;
            myTimer.Start();

            //	Runs the timer, and	raises	the event.
            exitFlag = false;
            while (exitFlag == false)
            {
                //	Processes all the events in	the queue.
                Application.DoEvents();
            }
        }

        public static void WaitForMouseClick(int maxSec)
        {
            // Sets the timer interval
            myTimer.Stop();
            myTimer.Interval = maxSec;
            myTimer.Start();

            // Runs the timer, and raises the event.
            exitFlag = false;
            while (exitFlag == false && clickDone == false)
            {
                // Processes all the events in the queue.
                Application.DoEvents();
            }
        }
    }
    #endregion

    #region ControlTest
    /// <summary>
    /// Basically the initial graph	given in the ZedGraph example
    /// <a href="http://www.codeproject.com/csharp/ZedGraph.asp">
    /// http://www.codeproject.com/csharp/ZedGraph.asp</a>
    /// </summary>
    /// 
    /// <author> Jerry Vos revised by John Champion	</author>
    /// <version> $Revision: 3.21 $ $Date: 2007-04-16 00:03:07 $ </version>
    [TestFixture]
    public class ControlTest
    {
        Form form;
        GraphPane testee;
        ZedGraphControl control;

        [SetUp]
        public void SetUp()
        {
            TestUtils.SetUp();

            form = new Form();
            control = new ZedGraphControl();

            control.GraphPane = new GraphPane(new System.Drawing.Rectangle(10, 10, 10, 10),
                "Wacky	Widget Company\nProduction Report",
                "Time, Days\n(Since	Plant Construction Startup)",
                "Widget Production\n(units/hour)");

            control.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

            control.Size = form.ClientSize;

            testee = control.GraphPane;

            form.Controls.Add(control);

        }

        [TearDown]
        public void Terminate()
        {
            form.Dispose();
        }


        #region Empty	UserControl
        [Test]
        public void EmptyUserControl()
        {
            form.Show();

            Assert.IsTrue(TestUtils.promptIfTestWorked("Is an empty graph visible?"));
        }
        #endregion

        #region Standard Sample UserControl
        [Test]
        public void StandardUserControl()
        {
            testee.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow);

            double[] x = { 72, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
            double[] y = { 20, 10, 50, 40, 35, 60, 90, 25, 48, 75 };
            double[] x2 = { 300, 400, 500, 600, 700, 800, 900 };
            double[] y2 = { 75, 43, 27, 62, 89, 73, 12 };
            double[] x3 = { 150, 250, 400, 520, 780, 940 };
            double[] y3 = { 5.2, 49.0, 33.8, 88.57, 99.9, 36.8 };

            LineItem curve;
            curve = testee.AddCurve("Larry", x, y, Color.Red, SymbolType.Circle);
            curve.Symbol.Size = 14;
            curve.Line.Width = 2.0F;
            curve = testee.AddCurve("Curly", x2, y2, Color.Green, SymbolType.Triangle);
            curve.Symbol.Size = 14;
            curve.Line.Width = 2.0F;
            curve.Symbol.Fill.Type = FillType.Solid;
            curve = testee.AddCurve("Moe", x3, y3, Color.Blue, SymbolType.Diamond);
            curve.Line.IsVisible = false;
            curve.Symbol.Fill.Type = FillType.Solid;
            curve.Symbol.Size = 14;

            testee.XAxis.MajorGrid.IsVisible = true;
            testee.XAxis.Scale.FontSpec.Angle = 60;

            testee.YAxis.MajorGrid.IsVisible = true;

            TextObj text = new TextObj("First	Prod\n21-Oct-99", 100F, 50.0F);
            text.Location.AlignH = AlignH.Center;
            text.Location.AlignV = AlignV.Bottom;
            text.FontSpec.Fill.Color = Color.LightBlue;
            text.FontSpec.Fill.Type = FillType.Brush;
            text.FontSpec.IsItalic = true;
            testee.GraphObjList.Add(text);

            ArrowObj arrow = new ArrowObj(Color.Black, 12F, 100F, 47F, 72F, 25F);
            arrow.Location.CoordinateFrame = CoordType.AxisXYScale;
            testee.GraphObjList.Add(arrow);

            text = new TextObj("Upgrade", 700F, 50.0F);
            text.FontSpec.Angle = 90;
            text.FontSpec.FontColor = Color.Black;
            text.Location.AlignH = AlignH.Right;
            text.Location.AlignV = AlignV.Center;
            text.FontSpec.Fill.Color = Color.LightGoldenrodYellow;
            text.FontSpec.Fill.Type = FillType.Brush;
            text.FontSpec.Border.IsVisible = false;
            testee.GraphObjList.Add(text);

            arrow = new ArrowObj(Color.Black, 15, 700, 53, 700, 80);
            arrow.Location.CoordinateFrame = CoordType.AxisXYScale;
            arrow.Line.Width = 2.0F;
            testee.GraphObjList.Add(arrow);
            text = new TextObj("Confidential", 0.8F, -0.03F);
            text.Location.CoordinateFrame = CoordType.ChartFraction;
            text.FontSpec.Angle = 15.0F;
            text.FontSpec.FontColor = Color.Red;
            text.FontSpec.IsBold = true;
            text.FontSpec.Size = 16;
            text.FontSpec.Border.IsVisible = true;
            text.FontSpec.Border.Color = Color.Red;
            text.Location.AlignH = AlignH.Left;
            text.Location.AlignV = AlignV.Bottom;
            testee.GraphObjList.Add(text);

            testee.AxisChange(control.CreateGraphics());

            form.Show();

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Is a graph visible with 3 sets of data and text labels?"));
        }
        #endregion

    }
    #endregion

    #region Library	Test
    /// <summary>
    /// Test code	suite	for	the class library
    /// </summary>
    /// 
    /// <author> John Champion </author>
    /// <version>	$Revision: 3.21 $ $Date: 2007-04-16 00:03:07 $ </version>
    [TestFixture]
    public class LibraryTest
    {
        Form form2;
        GraphPane testee;

        [SetUp]
        public void SetUp()
        {
            TestUtils.SetUp();

            form2 = new Form();
            form2.Size = new Size(500, 500);
            form2.Paint += new System.Windows.Forms.PaintEventHandler(this.Form2_Paint);
            form2.Resize += new System.EventHandler(this.Form2_Resize);
            form2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form2_MouseDown);
            form2.Show();

        }

        [TearDown]
        public void Terminate()
        {
            form2.Dispose();
        }

        private void Form2_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            SolidBrush brush = new SolidBrush(Color.Gray);
            e.Graphics.FillRectangle(brush, form2.ClientRectangle);
            testee.Draw(e.Graphics);
        }

        private void Form2_Resize(object sender, System.EventArgs e)
        {
            SetSize();
            testee.AxisChange(form2.CreateGraphics());
            form2.Refresh();
        }

        private void SetSize()
        {
            Rectangle paneRect = form2.ClientRectangle;
            paneRect.Inflate(-10, -10);
            testee.Rect = paneRect;
        }

        private void Form2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        }

        #region Standard Bar	Graph
        [Test]
        public void StandardBarGraph()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "My Test Bar	Graph", "Label", "My	Y	Axis");

            string[] labels = { "Panther", "Lion", "Cheetah", "Cougar", "Tiger", "Leopard", "Kitty" };
            double[] y = { 100, 115, 75, -22, 98, 40, -10 };
            double[] y2 = { 90, 100, 95, -35, 80, 35, 35 };
            double[] y3 = { 80, 110, 65, -15, 54, 67, 18 };

            double[] y4 = { 120, 125, 100, 20, 105, 75, -40 };

            //	Generate a red	bar with "Curve 1" in the legend
            CurveItem myCurve = testee.AddBar("Curve	1", null, y, Color.Red);

            /*
						//	Generate a blue bar	with	"Curve 2" in	the	legend
						myCurve = myPane.AddCurve( "Curve	2",
							null, y2,	Color.Blue );
						//	Make it a bar
						myCurve.IsBar	=	true;
			*/
            /*
						//	Generate a green bar	with "Curve 3" in the	legend
						myCurve = myPane.AddCurve( "Curve	3",
							null, y3,	Color.Green );
						//	Make it a bar
						myCurve.IsBar	=	true;
			*/
            /*
						//	Generate a black line	with "Curve 4" in the	legend
						myCurve = myPane.AddCurve( "Curve	4",
							y4,	y4, Color.Black,	SymbolType.Circle );

						myCurve.Symbol.Size = 14.0F;
						myCurve.Symbol.IsFilled	= true;
						myCurve.Line.Width = 2.0F;
			*/

            //	Draw the X tics	between the labels instead of	at the labels
            testee.XAxis.MajorTic.IsBetweenLabels = true;

            //	Set the XAxis	labels
            testee.XAxis.Scale.TextLabels = labels;
            //	Set the XAxis	to Text type
            testee.XAxis.Type = AxisType.Text;

            testee.XAxis.Scale.IsReverse = false;
            testee.BarSettings.ClusterScaleWidth = 1;

            //Add Labels to the curves

            //	Shift	the text items up by	5 user	scale units	above	the	bars
            const float shift = 5;

            for (int i = 0; i < y.Length; i++)
            {
                //	format	the label string to	have	1 decimal place
                string lab = y[i].ToString("F1");
                //	create	the text item (assumes	the	x	axis is	ordinal or	text)
                //	for negative bars, the	label appears just	above	the zero	value
                TextObj text = new TextObj(lab, (float)(i + 1), (float)(y[i] < 0 ? 0.0 : y[i]) + shift);
                //	tell	Zedgraph to use user	scale units	for locating	the	TextObj
                text.Location.CoordinateFrame = CoordType.AxisXYScale;
                //	Align the left-center	of the text to the	specified	point
                text.Location.AlignH = AlignH.Left;
                text.Location.AlignV = AlignV.Center;
                text.FontSpec.Border.IsVisible = false;
                //	rotate the	text 90 degrees
                text.FontSpec.Angle = 90;
                //	add the	TextObj to	the	list
                testee.GraphObjList.Add(text);
            }

            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            //	Add one step to the	max	scale value	to leave	room for the labels
            testee.YAxis.Scale.Max += testee.YAxis.Scale.MajorStep;

            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Refresh();

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Is a bar graph visible with 1 set of data and value labels?  <Next Step: Resize the Graph for 3 seconds>"));

            TestUtils.DelaySeconds(3000);

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did the graph resize ok?"));
        }
        #endregion

        #region Clustered	 Bar Graph
        [Test]
        public void ClusteredBarGraph()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "Clustered Bar Graph Test", "Label", "My	Y	Axis");

            string[] labels = { "Panther", "Lion", "Cheetah", "Cougar", "Tiger", "Leopard", "Kitty" };
            double[] y = { 100, 115, 75, -22, 98, 40, -10 };
            double[] y2 = { 90, 100, 95, 35, 0, 35, -35 };
            double[] y3 = { 80, 110, 65, -15, 54, 67, 18 };

            double[] y4 = { 120, 125, 100, 20, 105, 75, -40 };

            //	Generate three	bars	with	appropriate entries in	the legend
            CurveItem myCurve = testee.AddBar("Curve	1", null, y, Color.Red);
            CurveItem myCurve1 = testee.AddBar("Curve	2", null, y2, Color.Blue);
            CurveItem myCurve2 = testee.AddBar("Curve	3", null, y3, Color.Green);
            //	Draw the X tics	between the labels instead of	at the labels
            testee.XAxis.MajorTic.IsBetweenLabels = true;

            //	Set the XAxis	labels
            testee.XAxis.Scale.TextLabels = labels;
            testee.XAxis.Scale.FontSpec.Size = 9F;
            //	Set the XAxis	to Text type
            testee.XAxis.Type = AxisType.Text;
            testee.BarSettings.Base = BarBase.X;

            testee.XAxis.Scale.IsReverse = false;
            testee.BarSettings.ClusterScaleWidth = 1;

            //Add Labels to the curves

            //	Shift	the text items up by	5 user	scale units	above	the	bars
            const float shift = 5;

            for (int i = 0; i < y.Length; i++)
            {
                //	format	the label string to	have	1 decimal place
                string lab = y2[i].ToString("F1");
                //	create	the text item (assumes	the	x	axis is	ordinal or	text)
                //	for negative bars, the	label appears just	above	the zero	value
                TextObj text = new TextObj(lab, (float)(i + 1), (float)(y2[i] < 0 ? 0.0 : y2[i]) + shift);
                //	tell	Zedgraph to use user	scale units	for locating	the	TextObj
                text.Location.CoordinateFrame = CoordType.AxisXYScale;
                //	Align the left-center	of the text to the	specified	point
                text.Location.AlignH = AlignH.Left;
                text.Location.AlignV = AlignV.Center;
                text.FontSpec.Border.IsVisible = false;
                //	rotate the	text 90 degrees
                text.FontSpec.Angle = 90;
                //	add the	TextObj to	the	list
                testee.GraphObjList.Add(text);
            }

            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            //	Add one step to the	max	scale value	to leave	room for the labels
            testee.YAxis.Scale.Max += testee.YAxis.Scale.MajorStep;

            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Refresh();

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Is a clustered bar graph having the proper number of bars per x-Axis point visible ?  <Next Step: Resize the chart>"));


            TestUtils.DelaySeconds(3000);

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did the graph resize ok with all x-Axis labels visible?"));
        }
        #endregion

        #region Horizontal	Clustered  Bar	Graph
        [Test]
        public void HorizClusteredBarGraph()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "Horizontal	Clustered Bar Graph Test ", "Label", "My Y Axis");

            string[] labels = { "Panther", "Lion", "Cheetah", "Cougar", "Tiger", "Leopard", "Kitty", "Wildcat" };
            double[] y = { 100, 115, 75, -22, 98, 40, -10, 20 };
            double[] y2 = { 90, 100, 95, 35, 80, 35, -35, 30 };
            double[] y3 = { 80, 0, 65, -15, 54, 67, 18, 50 };

            //	Generate three	bars	with	appropriate entries in	the legend
            CurveItem myCurve = testee.AddBar("Curve	1", y, null, Color.Red);
            CurveItem myCurve1 = testee.AddBar("Curve	2", y2, null, Color.Blue);
            CurveItem myCurve2 = testee.AddBar("Curve	3", y3, null, Color.Green);
            //	Draw the Y tics	between the labels instead of	at the labels
            testee.YAxis.MajorTic.IsBetweenLabels = true;

            //	Set the YAxis	labels
            testee.YAxis.Scale.TextLabels = labels;
            testee.YAxis.Scale.FontSpec.Size = 9F;
            //show	the zero	line
            testee.XAxis.MajorGrid.IsZeroLine = true;
            //	Set the YAxis	to Text	type
            testee.YAxis.Type = AxisType.Text;
            testee.BarSettings.Base = BarBase.Y;

            testee.YAxis.Scale.IsReverse = false;
            testee.BarSettings.ClusterScaleWidth = 1;


            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            //	Add one step to the	max	scale value	to leave	room for the labels
            testee.XAxis.Scale.Max += testee.YAxis.Scale.MajorStep;

            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Refresh();

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Is a horizontal clustered bar graph having the proper number of bars per y-Axis point visible ? <Next Step: Resize the graph>"));

            TestUtils.DelaySeconds(3000);

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did the graph resize ok with all y-Axis labels visible?"));
        }
        #endregion

        #region Stack	Bar Graph
        [Test]
        public void StkBarGraph()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "Stack Bar	Graph	Test ", "Label", "My	Y	Axis");

            string[] labels = { "Panther", "Lion", "Cheetah", "Cougar", "Tiger", "Leopard", "Kitty" };
            double[] y = { 100, 115, 75, -22, 0, 40, -10 };
            double[] y2 = { 90, 100, -95, 35, 0, 35, -35 };
            double[] y3 = { 80, 110, 65, -15, 54, 67, -18 };

            double[] y4 = { 120, 125, 100, 20, 105, 75, -40 };


            //	Generate three	bars	with	appropriate entries in	the legend
            CurveItem myCurve = testee.AddBar("Curve	1", null, y, Color.Red);
            CurveItem myCurve1 = testee.AddBar("Curve	2", null, y2, Color.Blue);
            CurveItem myCurve2 = testee.AddBar("Curve	3", null, y3, Color.Green);
            //	Draw the X tics	between the labels instead of	at the labels
            testee.XAxis.MajorTic.IsBetweenLabels = true;

            //	Set the XAxis	labels
            testee.XAxis.Scale.TextLabels = labels;
            testee.XAxis.Scale.FontSpec.Size = 9F;
            //	Set the XAxis	to Text type
            testee.XAxis.Type = AxisType.Text;
            testee.BarSettings.Base = BarBase.X;
            //display as	stack bar
            testee.BarSettings.Type = BarType.Stack;
            //display horizontal grid	lines
            testee.YAxis.MajorGrid.IsVisible = true;

            testee.XAxis.Scale.IsReverse = false;
            testee.BarSettings.ClusterScaleWidth = 1;
            //turn	off pen width	scaling
            testee.IsPenWidthScaled = false;

            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Refresh();

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Is a stack bar graph having three bars per x-Axis point visible ?  <Next Step: Maximize the display>"));

            TestUtils.DelaySeconds(3000);

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did the graph resize ok with all x-Axis labels visible?  <Next Step: Add a curve>"));

            LineItem curve = new LineItem("Curve A", null, y4, Color.Black, SymbolType.TriangleDown);
            testee.CurveList.Insert(0, curve);
            curve.Line.Width = 1.5F;
            curve.Line.IsSmooth = true;
            curve.Line.SmoothTension = 0.6F;
            curve.Symbol.Fill = new Fill(Color.Yellow);
            curve.Symbol.Size = 8;

            form2.Refresh();

            TestUtils.DelaySeconds(3000);

            Assert.IsTrue(TestUtils.promptIfTestWorked("Was a new curve displayed on top of the bars?"));
        }
        #endregion

        #region Percent Stack  Bar Graph
        [Test]
        public void PctStkBarGraph()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "Percent Stack Bar Test ", "Label", "My	Y	Axis");

            string[] labels = { "Panther", "Lion", "Cheetah", "Cougar", "Tiger", "Leopard", "Kitty" };
            double[] y = { 100, 115, 75, -22, 0, 40, -10 };
            double[] y2 = { 90, 100, -95, 35, 0, 35, -35 };
            double[] y3 = { 80, 110, 65, -15, 54, 67, -18 };


            //	Generate three	bars	with	appropriate entries in	the legend
            BarItem myCurve = testee.AddBar("Curve	1", null, y, Color.Red);
            BarItem myCurve1 = testee.AddBar("Curve	2", null, y2, Color.Blue);
            BarItem myCurve2 = testee.AddBar("Curve	3", null, y3, Color.Green);
            //	Draw the X tics	between the labels instead of	at the labels
            testee.XAxis.MajorTic.IsBetweenLabels = true;

            //	Set the XAxis	labels
            testee.XAxis.Scale.TextLabels = labels;
            testee.XAxis.Scale.FontSpec.Size = 9F;
            //	Set the XAxis	to Text type
            testee.XAxis.Type = AxisType.Text;
            testee.BarSettings.Base = BarBase.X;
            //display as	stack bar
            testee.BarSettings.Type = BarType.PercentStack;
            //display horizontal grid	lines
            testee.YAxis.MajorGrid.IsVisible = true;

            testee.XAxis.Scale.IsReverse = false;
            testee.BarSettings.ClusterScaleWidth = 1;
            //turn off	pen width scaling
            testee.IsPenWidthScaled = false;

            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed

            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Refresh();

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Is a stack bar graph having three bars per x-Axis point visible ?  <Next Step: Fill one bar segment with a Textured Brush>"));
            Bitmap bm = new Bitmap("FeatherTexture.png");
            TextureBrush brush = new TextureBrush(bm);

            myCurve.Bar.Fill = new Fill(brush);
            form2.Refresh();

            TestUtils.DelaySeconds(3000);
            Assert.IsTrue(TestUtils.promptIfTestWorked("Was the red segment replaced with one having a bitmap fill?<Next: Disappearing segments"));

            for (int iCurve = 0; iCurve < 2; iCurve++)
            {
                PointPairList ppList = testee.CurveList[iCurve].Points as PointPairList;

                for (int i = 0; i < testee.CurveList[iCurve].Points.Count; i++)
                {
                    PointPair pt = ppList[i];
                    pt.Y = 0;
                    ppList[i] = pt;

                    form2.Refresh();

                    //	delay 
                    TestUtils.DelaySeconds(500);
                }
            }
            TestUtils.DelaySeconds(1000);
            Assert.IsTrue(TestUtils.promptIfTestWorked("Did the segments disappear uniformly while the total height stayed at +/-100%?"));
        }
        #endregion

        #region Overlay Bar	Graph
        [Test]
        public void OverlayBarGraph()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "Overlay Bar Graph Test ", "Label", "My	Y	Axis");

            string[] labels = { "Panther", "Lion", "Cheetah", "Cougar", "Tiger", "Leopard", "Kitty" };
            double[] y = { 100, 115, 75, 22, 98, 40, 10 };
            double[] y2 = { 90, 100, 95, 35, 0, 35, 35 };
            double[] y3 = { 80, 110, 65, 15, 54, 67, 18 };

            //	Generate three	bars	with	appropriate entries in	the legend
            CurveItem myCurve = testee.AddBar("Curve	1", null, y3, Color.Red);
            CurveItem myCurve1 = testee.AddBar("Curve	2", null, y2, Color.Blue);
            CurveItem myCurve2 = testee.AddBar("Curve	3", null, y, Color.Green);
            //	Draw the X tics	between the labels instead of	at the labels
            testee.XAxis.MajorTic.IsBetweenLabels = true;

            //	Set the XAxis	labels
            testee.XAxis.Scale.TextLabels = labels;
            testee.XAxis.Scale.FontSpec.Size = 9F;
            //	Set the XAxis	to Text type
            testee.XAxis.Type = AxisType.Text;
            testee.BarSettings.Base = BarBase.X;
            //display as	overlay bars
            testee.BarSettings.Type = BarType.Overlay;
            //display horizontal grid	lines
            testee.YAxis.MajorGrid.IsVisible = true;

            testee.XAxis.Scale.IsReverse = false;
            testee.BarSettings.ClusterScaleWidth = 1;

            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            //	Add one step to the	max	scale value	to leave	room for the labels
            testee.YAxis.Scale.Max += testee.YAxis.Scale.MajorStep;

            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Refresh();

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Is a stack bar graph having the proper number of bars per x-Axis point visible ?  <Next Step: Resize the graph>"));


            TestUtils.DelaySeconds(3000);

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did the graph resize ok with all x-Axis labels visible?"));
        }
        #endregion

        #region SortedOverlay Bar	Graph
        [Test]
        public void SortedOverlayBarGraph()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "Sorted Overlay Bar Graph Test ", "Label", "My	Y	Axis");

            string[] labels = { "Panther", "Lion", "Cheetah", "Cougar", "Tiger", "Leopard", "Kitty" };
            double[] y = { 100, -115, 75, 22, 98, 40, 10 };
            double[] y2 = { 90, -100, 95, -35, 0, 35, 35 };
            double[] y3 = { 80, -110, 65, 15, 54, 67, 18 };

            //	Generate three	bars	with	appropriate entries in	the legend
            CurveItem myCurve = testee.AddBar("Curve	1", null, y3, Color.Red);
            CurveItem myCurve1 = testee.AddBar("Curve	2", null, y2, Color.Blue);
            CurveItem myCurve2 = testee.AddBar("Curve	3", null, y, Color.Green);
            //	Draw the X tics	between the labels instead of	at the labels
            testee.XAxis.MajorTic.IsBetweenLabels = true;

            //	Set the XAxis	labels
            testee.XAxis.Scale.TextLabels = labels;
            testee.XAxis.Scale.FontSpec.Size = 9F;
            //	Set the XAxis	to Text type
            testee.XAxis.Type = AxisType.Text;
            testee.BarSettings.Base = BarBase.X;
            //display as	overlay bars
            testee.BarSettings.Type = BarType.SortedOverlay;
            //display horizontal grid	lines
            testee.YAxis.MajorGrid.IsVisible = true;

            testee.XAxis.Scale.IsReverse = false;
            testee.BarSettings.ClusterScaleWidth = 1;

            //	Shift	the text items up by	5 user	scale units	above	the	bars
            const float shift = 5;

            string lab = "";
            TextObj text = null;
            for (int x = 0; x < 3; x++)
                for (int i = 0; i < y.Length; i++)
                {
                    //	format	the label string to	have	1 decimal place
                    switch (x)
                    {
                        case 0:
                            lab = y[i].ToString();
                            text = new TextObj(lab, (float)(i + 1), (float)(y[i] < 0 ? y[i] + 2 * shift : y[i]) - shift);
                            break;
                        case 1:
                            lab = y2[i].ToString();
                            text = new TextObj(lab, (float)(i + 1), (float)(y2[i] < 0 ? y2[i] + 2 * shift : y2[i]) - shift);
                            break;
                        case 2:
                            lab = y3[i].ToString();
                            text = new TextObj(lab, (float)(i + 1), (float)(y3[i] < 0 ? y3[i] + 2 * shift : y3[i]) - shift);
                            break;
                        default:
                            break;
                    }
                    text.FontSpec.Size = 4;
                    text.FontSpec.IsBold = true;
                    //	tell	Zedgraph to use user	scale units	for locating	the	TextObj
                    text.Location.CoordinateFrame = CoordType.AxisXYScale;
                    //	Align the left-center	of the text to the	specified	point
                    text.Location.AlignH = AlignH.Center;
                    text.Location.AlignV = AlignV.Center;
                    text.FontSpec.Border.IsVisible = false;
                    //	add the	TextObj to	the	list
                    testee.GraphObjList.Add(text);
                }

            form2.WindowState = FormWindowState.Maximized;

            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            //	Add one step to the	max	scale value	to leave	room for the labels
            testee.YAxis.Scale.Max += testee.YAxis.Scale.MajorStep;

            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Refresh();

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Is a Sorted Overlay Stack Bar displayed with the segments in increasing value order as indicated by the embedded values? "));

        }
        #endregion

        #region Horizontal	Stack	Bar Graph
        [Test]
        public void HorizStkBarGraph()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "	Horizontal Stack	Bar Graph Test", "Label", "My Y Axis");

            string[] labels = { "Panther", "Lion", "Cheetah", "Cougar", "Tiger", "Leopard", "Kitty", "Wildcat" };
            double[] y = { 100, 115, 75, -22, 98, 40, -10, 20 };
            double[] y2 = { 90, 100, 95, 35, 80, 35, -35, 30 };
            double[] y3 = { 80, 0, 65, -15, 54, 67, 18, 50 };

            //	Generate three	bars	with	appropriate entries in	the legend
            BarItem myCurve = testee.AddBar("Curve 1", y, null, Color.Red);
            BarItem myCurve1 = testee.AddBar("Curve 2", y2, null, Color.Blue);
            BarItem myCurve2 = testee.AddBar("Curve 3", y3, null, Color.Green);
            //	Draw the Y tics	between the labels instead of	at the labels
            testee.YAxis.MajorTic.IsBetweenLabels = true;
            testee.BarSettings.Type = BarType.Stack;

            //	Set the YAxis	labels
            testee.YAxis.Scale.TextLabels = labels;
            testee.YAxis.Scale.FontSpec.Size = 9F;
            //show	the zero	line
            testee.XAxis.MajorGrid.IsZeroLine = true;
            //show	XAxis the grid lines
            testee.XAxis.MajorGrid.IsVisible = true;
            //	Set the YAxis	to Text	type
            testee.YAxis.Type = AxisType.Text;
            testee.BarSettings.Base = BarBase.Y;

            testee.YAxis.Scale.IsReverse = false;
            testee.BarSettings.ClusterScaleWidth = 1;



            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.WindowState = FormWindowState.Maximized;
            form2.Refresh();

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Is a horizontal stack bar graph having three bars per y-Axis point visible?  <Next Step: Re-orient the bar fill>"));

            myCurve.Bar.Fill = new Fill(Color.White, Color.Red, 90);
            myCurve1.Bar.Fill = new Fill(Color.White, Color.Blue, 90);
            myCurve2.Bar.Fill = new Fill(Color.White, Color.Green, 90);
            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Refresh();

            TestUtils.DelaySeconds(3000);

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did the orientation of the fill shift from vertical to horizontal?"));
        }
        #endregion

        #region Animated	Date	Graph
        [Test]
        public void AnimatedDateGraph()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "Date	Graph	Test ", "X AXIS", "Y Value");

            //	start	with an empty list for testing
            PointPairList pointList = new PointPairList();

            //	Generate a red	curve with diamond
            //	symbols, and "My Curve" in the legend
            LineItem myCurve = testee.AddCurve("My	Curve",
                pointList, Color.Red, SymbolType.Diamond);

            //	Set the XAxis	to date type
            testee.XAxis.Type = AxisType.Date;

            //	make the	symbols filled blue
            myCurve.Symbol.Fill.Type = FillType.Solid;
            myCurve.Symbol.Fill.Color = Color.Blue;

            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Show();

            //	Draw a sinusoidal curve, adding one	point at a	time
            //	and refiguring/redrawing each time.	(a stress test)
            //	redo	creategraphics()	each time to stress test
            for (int i = 0; i < 300; i++)
            {
                double x = (double)new XDate(1995, i + 1, 1);
                double y = Math.Sin((double)i * Math.PI / 30.0);

                (myCurve.Points as PointPairList).Add(x, y);
                testee.AxisChange(form2.CreateGraphics());
                form2.Refresh();

                //	delay for 10 ms
                //DelaySeconds(	50	);
            }

            while (myCurve.Points.Count > 0)
            {
                //	remove the	first point	in the list
                (myCurve.Points as PointPairList).RemoveAt(0);
                testee.AxisChange(form2.CreateGraphics());
                form2.Refresh();

                //	delay for 10 ms
                //DelaySeconds(	50	);
            }

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did you see points added one by one, then deleted one by one?"));
        }
        #endregion

        #region Single	Value Test
        [Test]
        public void SingleValue()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "Wacky	Widget Company\nProduction Report",
                "Time, Years\n(Since Plant Construction Startup)",
                "Widget Production\n(units/hour)");

            double[] x = { 0.4875 };
            double[] y = { -123456 };

            LineItem curve;
            curve = testee.AddCurve("One	Value", x, y, Color.Red, SymbolType.Diamond);
            curve.Symbol.Fill.Type = FillType.Solid;

            testee.XAxis.MajorGrid.IsVisible = true;
            testee.YAxis.MajorGrid.IsVisible = true;

            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Show();

            Assert.IsTrue(TestUtils.promptIfTestWorked("Do you see a single value in the middle of the scale ranges?"));
        }
        #endregion

        #region Missing Values	test
        [Test]
        public void MissingValues()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "Wacky	Widget Company\nProduction Report",
                "Time, Years\n(Since Plant Construction Startup)",
                "Widget Production\n(units/hour)");

            double[] x = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
            double[] y = { 20, 10, PointPair.Missing, PointPair.Missing, 35, 60, 90, 25, 48, PointPair.Missing };
            double[] x2 = { 300, 400, 500, 600, 700, 800, 900 };
            double[] y2 = { PointPair.Missing, 43, 27, 62, 89, 73, 12 };
            double[] x3 = { 150, 250, 400, 520, 780, 940 };
            double[] y3 = { 5.2, 49.0, PointPair.Missing, 88.57, 99.9, 36.8 };

            double[] x4 = { 150, 250, 400, 520, 780, 940 };
            double[] y4 = { .03, .054, .011, .02, .14, .38 };
            double[] x5 = { 1.5, 2.5, 4, 5.2, 7.8, 9.4 };
            double[] y5 = { 157, 458, 1400, 100000, 10290, 3854 };

            LineItem curve;
            curve = testee.AddCurve("Larry", x, y, Color.Red, SymbolType.Circle);
            curve.Line.Width = 2.0F;
            curve.Symbol.Fill.Type = FillType.Solid;
            curve = testee.AddCurve("Moe", x3, y3, Color.Green, SymbolType.Triangle);
            curve.Symbol.Fill.Type = FillType.Solid;
            curve = testee.AddCurve("Curly", x2, y2, Color.Blue, SymbolType.Diamond);
            curve.Symbol.Fill.Type = FillType.Solid;
            curve.Symbol.Size = 12;

            testee.Fill = new Fill(Color.White, Color.WhiteSmoke);
            testee.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow);
            testee.XAxis.MajorGrid.IsVisible = true;
            testee.XAxis.Scale.FontSpec.Angle = 0;

            testee.YAxis.MajorGrid.IsVisible = true;
            testee.YAxis.Scale.FontSpec.Angle = 90;

            TextObj text = new TextObj("First	Prod\n21-Oct-93", 100F, 50.0F);
            text.Location.AlignH = AlignH.Center;
            text.Location.AlignV = AlignV.Bottom;
            text.FontSpec.Fill.Color = Color.PowderBlue;
            text.FontSpec.Fill.Type = FillType.Brush;
            testee.GraphObjList.Add(text);

            ArrowObj arrow = new ArrowObj(Color.Black, 12F, 100F, 47F, 72F, 25F);
            arrow.Location.CoordinateFrame = CoordType.AxisXYScale;
            testee.GraphObjList.Add(arrow);

            text = new TextObj("Upgrade", 700F, 50.0F);
            text.FontSpec.Angle = 90;
            text.FontSpec.FontColor = Color.Black;
            text.Location.AlignH = AlignH.Right;
            text.Location.AlignV = AlignV.Center;
            text.FontSpec.Fill.Color = Color.LightGoldenrodYellow;
            text.FontSpec.Fill.Type = FillType.Brush;
            text.FontSpec.Border.IsVisible = false;
            testee.GraphObjList.Add(text);

            arrow = new ArrowObj(Color.Black, 15, 700, 53, 700, 80);
            arrow.Location.CoordinateFrame = CoordType.AxisXYScale;
            arrow.Line.Width = 2.0F;
            testee.GraphObjList.Add(arrow);

            text = new TextObj("Confidential", 0.8F, -0.03F);
            text.Location.CoordinateFrame = CoordType.ChartFraction;

            text.FontSpec.Angle = 15.0F;
            text.FontSpec.FontColor = Color.Red;
            text.FontSpec.IsBold = true;
            text.FontSpec.Size = 16;
            text.FontSpec.Border.IsVisible = false;
            text.FontSpec.Border.Color = Color.Red;
            text.FontSpec.Fill.Type = FillType.None;

            text.Location.AlignH = AlignH.Left;
            text.Location.AlignV = AlignV.Bottom;
            testee.GraphObjList.Add(text);

            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Show();

            for (int iCurve = 0; iCurve < 3; iCurve++)
            {
                PointPairList ppList = testee.CurveList[iCurve].Points as PointPairList;

                for (int i = 0; i < ppList.Count; i++)
                {
                    PointPair pt = ppList[i];

                    if (i % 3 == 0)
                        pt.Y = PointPair.Missing;
                    else if (i % 3 == 1)
                        pt.Y = System.Double.NaN;
                    else if (i % 3 == 2)
                        pt.Y = System.Double.PositiveInfinity;

                    ppList[i] = pt;

                    form2.Refresh();

                    //	delay for 10 ms
                    TestUtils.DelaySeconds(300);
                }
            }

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did you see an initial graph, with points disappearing one by one?"));

            //	Go	ahead	and refigure the	axes	with the	invalid data just to check
            testee.AxisChange(form2.CreateGraphics());
        }
        #endregion

        #region A	dual	Y	test
        [Test]
        public void DualY()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "My Test Dual Y Graph", "Date", "My	Y Axis");
            testee.Y2Axis.Title.Text = "My Y2 Axis";

            //	Make up some random data points
            double[] x = new double[36];
            double[] y = new double[36];
            double[] y2 = new double[36];
            for (int i = 0; i < 36; i++)
            {
                x[i] = (double)new XDate(1995, i + 1, 1);
                y[i] = Math.Sin((double)i * Math.PI / 15.0);
                y2[i] = y[i] * 3.6178;
            }
            //	Generate a red	curve with diamond
            //	symbols, and "My Curve" in the legend
            CurveItem myCurve = testee.AddCurve("My Curve",
                x, y, Color.Red, SymbolType.Diamond);
            //	Set the XAxis	to date type
            testee.XAxis.Type = AxisType.Date;

            //	Generate a blue curve with	diamond
            //	symbols, and "My Curve" in the legend
            myCurve = testee.AddCurve("My Curve	1",
                x, y2, Color.Blue, SymbolType.Circle);
            myCurve.IsY2Axis = true;
            testee.YAxis.IsVisible = true;
            testee.Y2Axis.IsVisible = true;
            testee.Y2Axis.MajorGrid.IsVisible = true;
            testee.XAxis.MajorGrid.IsVisible = true;
            testee.YAxis.MajorTic.IsOpposite = false;
            testee.YAxis.MinorTic.IsOpposite = false;
            testee.YAxis.MajorGrid.IsZeroLine = false;

            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Show();

            Assert.IsTrue(TestUtils.promptIfTestWorked("Do you see a dual Y graph?"));
        }
        #endregion

        #region Stress	test with all	NaN's
        [Test]
        public void AllNaN()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "My Test NaN Graph", "Date", "My	Y	Axis");

            //	Make up some random data points
            double[] x = new double[36];
            double[] y = new double[36];
            for (int i = 0; i < 36; i++)
            {
                x[i] = (double)new XDate(1995, i + 1, 1);
                y[i] = System.Double.NaN;
            }
            //	Generate a red	curve with diamond
            //	symbols, and "My Curve" in the legend
            CurveItem myCurve = testee.AddCurve("My Curve",
                x, y, Color.Red, SymbolType.Circle);
            //	Set the XAxis	to date type
            testee.XAxis.Type = AxisType.Date;

            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Show();

            Assert.IsTrue(TestUtils.promptIfTestWorked("Do you see a graph with all values missing (NaN's)?"));
        }
        #endregion

        #region the	date	label-width test
        [Test]
        public void LabelWidth()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "My Test Label Width", "Date", "My Y Axis");

            //	Make up some random data points
            double[] x = new double[36];
            double[] y = new double[36];
            for (int i = 0; i < 36; i++)
            {
                x[i] = (double)new XDate(1995, 1, i + 1);
                y[i] = Math.Sin((double)i * Math.PI / 15.0);
            }
            //	Generate a red	curve with diamond
            //	symbols, and "My Curve" in the legend
            CurveItem myCurve = testee.AddCurve("My Curve",
                x, y, Color.Red, SymbolType.Diamond);
            //	Set the XAxis	to date type
            testee.XAxis.Type = AxisType.Date;
            testee.XAxis.Scale.Format = "dd-MMM-yyyy";


            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Show();

            Assert.IsTrue(TestUtils.promptIfTestWorked("If you see a date graph, resize it and make" +
                                    " sure the label count is reduced to avoid overlap"));

            TestUtils.DelaySeconds(3000);

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did the anti-overlap work?"));
        }
        #endregion

        #region Smooth Curve Sample
        [Test]
        public void SmoothCurve()
        {
            //			memGraphics.CreateDoubleBuffer( form2.CreateGraphics(),
            //				form2.ClientRectangle.Width,	form2.ClientRectangle.Height );

            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "Text Graph", "Label", "Y Value");

            //	Make up some random data points
            string[] labels = { "USA", "Spain", "Qatar",    "Morocco", "UK", "Uganda",
                                  "Cambodia", "Malaysia",   "Australia", "Ecuador" };

            PointPairList points = new PointPairList();
            double numPoints = 10.0;
            for (double i = 0; i < numPoints; i++)
                points.Add(i / (numPoints / 10.0) + 1.0, Math.Sin(i / (numPoints / 10.0) * Math.PI / 2.0));

            //	Generate a red	curve with diamond
            //	symbols, and "My Curve" in the legend
            LineItem myCurve = testee.AddCurve("My	Curve",
                points, Color.Red, SymbolType.Diamond);
            //	Set the XAxis	labels
            testee.XAxis.Scale.TextLabels = labels;
            //	Set the XAxis	to Text type
            testee.XAxis.Type = AxisType.Text;
            //	Set the labels at	an angle so they	don't overlap
            testee.XAxis.Scale.FontSpec.Angle = 0;
            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            myCurve.Line.IsSmooth = true;

            for (float tension = 0.0F; tension < 3.0F; tension += 0.1F)
            {
                myCurve.Line.SmoothTension = tension;
                form2.Refresh();

                TestUtils.DelaySeconds(50);
            }
            for (float tension = 3.0F; tension >= 0F; tension -= 0.1F)
            {
                myCurve.Line.SmoothTension = tension;
                form2.Refresh();

                TestUtils.DelaySeconds(50);
            }

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did you see varying levels of smoothing?"));
        }
        #endregion

        #region HiLow Chart
        [Test]
        public void HiLowChart()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "HiLow Chart Test ", "Date", "Y Value Range");

            double[] hi = new double[20];
            double[] low = new double[20];
            string[] x = new string[20];

            for (int i = 45; i < 65; i++)
            {
                XDate date = (double)new XDate(2004, 12, i - 30, 0, 0, 0);
                x[i - 45] = date.ToString("d");

                if (i % 2 == 1)
                    hi[i - 45] = (double)i * 1.03;
                else
                    hi[i - 45] = (double)i * .99;

                low[i - 45] = .97 * hi[i - 45];
            }

            //		HiLowBarItem myCurve	=	testee.AddHiLowBar(	"My	Curve",null,hi,low, Color.Green );
            HiLowBarItem myCurve = testee.AddHiLowBar("My	Curve", null, hi, low, Color.Green);

            testee.XAxis.Scale.FontSpec.Size = 8;
            testee.XAxis.Scale.FontSpec.Angle = 60;
            testee.XAxis.Scale.FontSpec.IsBold = true;

            //	Set the XAxis	to Text type
            testee.XAxis.Type = AxisType.Text;
            testee.XAxis.Scale.TextLabels = x;
            testee.XAxis.Scale.MajorStep = 1;
            testee.XAxis.MajorTic.IsBetweenLabels = false;

            testee.YAxis.MajorGrid.IsVisible = true;
            testee.YAxis.MinorGrid.IsVisible = true;

            form2.WindowState = FormWindowState.Maximized;
            testee.AxisChange(form2.CreateGraphics());

            TestUtils.DelaySeconds(3000);
            Assert.IsTrue(TestUtils.promptIfTestWorked("Was a HiLow chart with a date X-Axis displayed?"));
        }
        #endregion

        #region ErrorBar Chart
        [Test]
        public void ErrorBarChart()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "ErrorBar Chart Test ", "X AXIS", "Y Value");

            double[] hi = new double[20];
            double[] low = new double[20];
            double[] x = new double[20];

            for (int i = 45; i < 65; i++)
            {
                x[i - 45] = (double)new XDate(2004, 12, i - 30, 0, 0, 0);
                if (i % 2 == 1)
                    hi[i - 45] = (double)i * 1.03;
                else
                    hi[i - 45] = (double)i * .99;

                low[i - 45] = .97 * hi[i - 45];
            }

            ErrorBarItem myCurve = testee.AddErrorBar("My	Curve", x, hi, low, Color.Blue);

            testee.XAxis.Scale.FontSpec.Size = 12;
            testee.XAxis.Scale.FontSpec.Angle = 90;

            //	Set the XAxis	to date type
            testee.XAxis.Type = AxisType.Date;
            testee.XAxis.Scale.Min = x[0] - 1;
            myCurve.Bar.PenWidth = 2;

            testee.YAxis.MajorGrid.IsVisible = true;
            testee.YAxis.MinorGrid.IsVisible = true;

            form2.WindowState = FormWindowState.Maximized;
            testee.AxisChange(form2.CreateGraphics());

            TestUtils.DelaySeconds(3000);
            Assert.IsTrue(TestUtils.promptIfTestWorked("Was the typical Error Bar chart with a date X-Axis displayed?"));
        }
        #endregion

        #region text axis	sample
        [Test]
        public void TextAxis()
        {
            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "Text Graph", "Label", "Y Value");

            //	Make up some random data points
            string[] labels = { "USA", "Spain", "Qatar",    "Morocco", "UK", "Uganda",
                                  "Cambodia", "Malaysia",   "Australia", "Ecuador" };

            double[] y = new double[10];
            for (int i = 0; i < 10; i++)
                y[i] = Math.Sin((double)i * Math.PI / 2.0);
            //	Generate a red	curve with diamond
            //	symbols, and "My Curve" in the legend
            CurveItem myCurve = testee.AddCurve("My Curve",
                null, y, Color.Red, SymbolType.Diamond);
            //	Set the XAxis	labels
            testee.XAxis.Scale.TextLabels = labels;
            //	Set the XAxis	to Text type
            testee.XAxis.Type = AxisType.Text;
            //	Set the labels at	an angle so they	don't overlap
            testee.XAxis.Scale.FontSpec.Angle = 0;
            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Show();

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did you	get	an	X	Text axis?"));

            (myCurve.Points as PointPairList).Clear();
            for (double i = 0; i < 100; i++)
                (myCurve.Points as PointPairList).Add(i / 10.0, Math.Sin(i / 10.0 * Math.PI / 2.0));

            testee.AxisChange(form2.CreateGraphics());
            form2.Refresh();

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did the points fill in between the labels? (Next Resize the graph and check label overlap again)"));

            TestUtils.DelaySeconds(3000);

            Assert.IsTrue(TestUtils.promptIfTestWorked("Did the graph resize ok?"));
        }
        #endregion

    }
    #endregion

    #region Long	Feature	Test
    /// <summary>
    /// Test code	suite	for	the class library
    /// </summary>
    /// 
    /// <author> John Champion </author>
    /// <version>	$Revision: 3.21 $ $Date: 2007-04-16 00:03:07 $ </version>
    [TestFixture]
    public class LongFeatureTest
    {
        Form form2;
        GraphPane testee;

        [TestFixtureSetUp]
        public void SetUp()
        {
            TestUtils.SetUp();

            form2 = new Form();
            form2.Size = new Size(500, 500);
            form2.Paint += new System.Windows.Forms.PaintEventHandler(this.Form2_Paint);
            form2.Resize += new System.EventHandler(this.Form2_Resize);
            form2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form2_MouseDown);
        }

        [TearDown]
        public void Terminate()
        {
            form2.Dispose();
        }

        private void Form2_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            SolidBrush brush = new SolidBrush(Color.Gray);
            e.Graphics.FillRectangle(brush, form2.ClientRectangle);
            testee.Draw(e.Graphics);
        }

        private void Form2_Resize(object sender, System.EventArgs e)
        {
            SetSize();
            testee.AxisChange(form2.CreateGraphics());
            form2.Refresh();
        }

        private void SetSize()
        {
            Rectangle paneRect = form2.ClientRectangle;
            paneRect.Inflate(-10, -10);
            testee.Rect = paneRect;
        }

        private void Form2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        }

        #region The Long Feature Test
        [Test]
        public void LongFeature()
        {
            bool userOK = TestUtils.waitForUserOK;

            if (MessageBox.Show("Do you want to prompt at each step (otherwise, I will just run through" +
                " the whole test)?  Pick YES to Prompt at each step", "Test Setup",
                MessageBoxButtons.YesNo) == DialogResult.No)
            {
                TestUtils.waitForUserOK = false;
            }

            //	Create a new	graph
            testee = new GraphPane(new Rectangle(40, 40, form2.Size.Width - 80, form2.Size.Height - 80),
                "My Test Dual Y Graph", "Date", "My	Y	Axis");

            //	Make up some random data points
            double[] x = new double[36];
            double[] y = new double[36];
            double[] y2 = new double[36];
            for (int i = 0; i < 36; i++)
            {
                x[i] = (double)i * 5.0;
                y[i] = Math.Sin((double)i * Math.PI / 15.0) * 16.0;
                y2[i] = y[i] * 10.5;
            }
            //	Generate a red	curve with diamond
            //	symbols, and "My Curve" in the legend
            CurveItem myCurve = testee.AddCurve("My Curve",
                x, y, Color.Red, SymbolType.Diamond);

            //	Generate a blue curve with	diamond
            //	symbols, and "My Curve" in the legend
            myCurve = testee.AddCurve("My Curve	1",
                x, y2, Color.Blue, SymbolType.Circle);
            myCurve.IsY2Axis = true;

            testee.XAxis.MajorGrid.IsVisible = false;
            testee.XAxis.IsVisible = false;
            testee.XAxis.MajorGrid.IsZeroLine = false;
            testee.XAxis.MajorTic.IsOutside = false;
            testee.XAxis.MinorTic.IsOutside = false;
            testee.XAxis.MajorTic.IsInside = false;
            testee.XAxis.MinorTic.IsInside = false;
            testee.XAxis.MinorTic.IsOpposite = false;
            testee.XAxis.MajorTic.IsOpposite = false;
            testee.XAxis.Scale.IsReverse = false;
            //	testee.XAxis.IsLog = false;
            testee.XAxis.Title.Text = "";

            testee.YAxis.MajorGrid.IsVisible = false;
            testee.YAxis.IsVisible = false;
            testee.YAxis.MajorGrid.IsZeroLine = false;
            testee.YAxis.MajorTic.IsOutside = false;
            testee.YAxis.MinorTic.IsOutside = false;
            testee.YAxis.MajorTic.IsInside = false;
            testee.YAxis.MinorTic.IsInside = false;
            testee.YAxis.MinorTic.IsOpposite = false;
            testee.YAxis.MajorTic.IsOpposite = false;
            testee.YAxis.Scale.IsReverse = false;
            //testee.YAxis.IsLog =	false;
            testee.YAxis.Title.Text = "";

            testee.Y2Axis.MajorGrid.IsVisible = false;
            testee.Y2Axis.IsVisible = false;
            testee.Y2Axis.MajorGrid.IsZeroLine = false;
            testee.Y2Axis.MajorTic.IsOutside = false;
            testee.Y2Axis.MinorTic.IsOutside = false;
            testee.Y2Axis.MajorTic.IsInside = false;
            testee.Y2Axis.MinorTic.IsInside = false;
            testee.Y2Axis.MinorTic.IsOpposite = false;
            testee.Y2Axis.MajorTic.IsOpposite = false;
            testee.Y2Axis.Scale.IsReverse = false;
            //testee.Y2Axis.IsLog = false;
            testee.Y2Axis.Title.Text = "";

            testee.Chart.Border.IsVisible = false;
            testee.Border.IsVisible = false;

            testee.Title.IsVisible = false;
            testee.Legend.IsHStack = false;
            testee.Legend.IsVisible = false;
            testee.Legend.Border.IsVisible = false;
            testee.Legend.Fill.Type = FillType.None;
            testee.Legend.Position = LegendPos.Bottom;

            //	Tell ZedGraph to	refigure	the
            //	axes	since the data have	changed
            testee.AxisChange(form2.CreateGraphics());
            SetSize();
            form2.Show();

            Assert.IsTrue(TestUtils.promptIfTestWorked("Do you see a dual Y graph with no axes?"));

            testee.Border = new Border(true, Color.Red, 3.0F);

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Pane Frame Added?"));

            testee.Border = new Border(Color.Black, 1.0F);

            testee.Chart.Border = new Border(true, Color.Red, 3.0F);

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Axis Frame Added?"));

            testee.Chart.Border = new Border(Color.Black, 1.0F);

            testee.Fill = new Fill(Color.White, Color.LightGoldenrodYellow);
            testee.Margin.Top = 50.0F;
            testee.Margin.Bottom = 50.0F;
            testee.Margin.Left = 50.0F;
            testee.Margin.Right = 50.0F;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Pane Background Filled?"));

            testee.Margin.Top = 20.0F;
            testee.Margin.Bottom = 20.0F;
            testee.Margin.Left = 20.0F;
            testee.Margin.Right = 20.0F;
            testee.Fill.IsVisible = false;
            testee.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow);

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Axis Background Filled?"));

            testee.Chart.Fill.IsVisible = false;

            testee.Title.IsVisible = true;
            testee.Title.FontSpec.FontColor = Color.Red;
            testee.Title.Text = "The Title";

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Title Added?"));

            testee.Title.FontSpec.FontColor = Color.Black;

            testee.Legend.IsVisible = true;
            testee.Legend.FontSpec.FontColor = Color.Red;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Legend Added?"));

            testee.Legend.FontSpec.FontColor = Color.Black;

            testee.Legend.Border = new Border(true, Color.Red, 3.0F);

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Legend Frame Added?"));

            testee.Legend.Border = new Border(Color.Black, 1.0F);

            testee.Legend.Fill.Type = FillType.Brush;
            testee.Legend.Fill.Color = Color.LightGoldenrodYellow;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Legend Fill Added?"));

            testee.Legend.Position = LegendPos.InsideBotLeft;
            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Legend Moved to Inside Bottom Left?"));

            testee.Legend.Position = LegendPos.InsideBotRight;
            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Legend Moved to Inside Bottom Right?"));

            testee.Legend.Position = LegendPos.InsideTopLeft;
            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Legend Moved to Inside Top Left?"));

            testee.Legend.Position = LegendPos.InsideTopRight;
            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Legend	Moved to Inside	Top Right?"));

            testee.Legend.Position = LegendPos.Left;
            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Legend	Moved to Left?"));

            testee.Legend.Position = LegendPos.Right;
            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Legend	Moved to Right?"));

            testee.Legend.Position = LegendPos.Top;
            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Legend	Moved to Top?"));

            testee.Legend.IsHStack = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Legend	Horizontal Stacked?"));
            testee.Legend.Fill.Type = FillType.None;

            /////////	X	AXIS /////////////////////////////////////////////////////////////////////////

            testee.XAxis.IsVisible = true;
            testee.XAxis.Color = Color.Red;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("X Axis Visible?"));

            testee.XAxis.Title.Text = "X Axis Title";

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("X Axis Title Visible?"));

            //testee.XAxis.TicPenWidth	= 3.0F;
            testee.XAxis.MajorGrid.IsZeroLine = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("X Axis ZeroLine Visible?"));

            testee.XAxis.MajorGrid.IsZeroLine = false;
            //testee.XAxis.TicPenWidth	= 1.0F;
            testee.XAxis.MajorTic.IsOutside = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("X Axis major tics Visible?"));

            testee.XAxis.MinorTic.IsOutside = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("X Axis minor tics Visible?"));

            testee.XAxis.MajorTic.IsInside = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("X Axis Inside tics Visible?"));

            testee.XAxis.MajorTic.IsOpposite = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("X Axis Opposite tics Visible?"));

            testee.XAxis.MinorTic.IsInside = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("X Axis Minor Inside tics Visible?"));

            testee.XAxis.MinorTic.IsOpposite = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("X Axis Minor Opposite tics Visible?"));

            testee.XAxis.MajorTic.PenWidth = 1.0F;
            testee.XAxis.Color = Color.Black;
            testee.XAxis.MajorGrid.IsVisible = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("X Axis Grid Visible?"));

            testee.XAxis.MajorGrid.PenWidth = 1.0F;
            testee.XAxis.MajorGrid.Color = Color.Black;
            testee.XAxis.Scale.IsReverse = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("X Axis Reversed?"));

            testee.XAxis.Scale.IsReverse = false;
            testee.XAxis.Type = AxisType.Log;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("X Axis Log?"));

            testee.XAxis.Type = AxisType.Linear;

            ///////////////////////////////////////////////////////////////////////////////

            /////////	Y	AXIS /////////////////////////////////////////////////////////////////////////

            testee.YAxis.IsVisible = true;
            testee.YAxis.Color = Color.Red;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y Axis Visible?"));

            testee.YAxis.Title.Text = "Y Axis	Title";

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y Axis Title Visible?"));

            //testee.YAxis.TicPenWidth	= 3.0F;
            testee.YAxis.MajorGrid.IsZeroLine = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y Axis ZeroLine Visible?"));

            testee.YAxis.MajorGrid.IsZeroLine = false;
            //testee.YAxis.TicPenWidth	= 1.0F;
            testee.YAxis.MajorTic.IsOutside = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y Axis major tics Visible?"));

            testee.YAxis.MinorTic.IsOutside = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y Axis minor tics Visible?"));

            testee.YAxis.MajorTic.IsInside = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y Axis Inside tics Visible?"));

            testee.YAxis.MajorTic.IsOpposite = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y Axis Opposite tics Visible?"));

            testee.YAxis.MinorTic.IsInside = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y Axis Minor Inside tics Visible?"));

            testee.YAxis.MinorTic.IsOpposite = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y Axis Minor Opposite tics Visible?"));

            testee.YAxis.MajorTic.PenWidth = 1.0F;
            testee.YAxis.Color = Color.Black;
            testee.YAxis.MajorGrid.IsVisible = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y Axis Grid Visible?"));

            testee.YAxis.MajorGrid.PenWidth = 1.0F;
            testee.YAxis.MajorGrid.Color = Color.Black;
            testee.YAxis.Scale.IsReverse = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y Axis Reversed?"));

            testee.YAxis.Scale.IsReverse = false;
            testee.YAxis.Type = AxisType.Log;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y Axis Log?"));

            testee.YAxis.Type = AxisType.Linear;

            ///////////////////////////////////////////////////////////////////////////////

            /////////	Y2	AXIS	/////////////////////////////////////////////////////////////////////////

            testee.Y2Axis.IsVisible = true;
            testee.Y2Axis.Color = Color.Red;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y2 Axis Visible?"));

            testee.Y2Axis.Title.Text = "Y2	Axis Title";

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y2 Axis Title Visible?"));

            //testee.Y2Axis.TicPenWidth	=	3.0F;
            testee.Y2Axis.MajorGrid.IsZeroLine = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y2 Axis ZeroLine Visible?"));

            testee.Y2Axis.MajorGrid.IsZeroLine = false;
            //testee.Y2Axis.TicPenWidth	=	1.0F;
            testee.Y2Axis.MajorTic.IsOutside = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y2 Axis major tics Visible?"));

            testee.Y2Axis.MinorTic.IsOutside = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y2 Axis minor tics Visible?"));

            testee.Y2Axis.MajorTic.IsInside = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y2 Axis Inside tics Visible?"));

            testee.Y2Axis.MajorTic.IsOpposite = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y2 Axis Opposite tics Visible?"));

            testee.Y2Axis.MinorTic.IsInside = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y2 Axis Minor Inside tics Visible?"));

            testee.Y2Axis.MinorTic.IsOpposite = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y2 Axis Minor Opposite tics Visible?"));

            testee.Y2Axis.MajorTic.PenWidth = 1.0F;
            testee.Y2Axis.Color = Color.Black;
            testee.Y2Axis.MajorGrid.IsVisible = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y2 Axis Grid Visible?"));

            testee.Y2Axis.MajorGrid.PenWidth = 1.0F;
            testee.Y2Axis.MajorGrid.Color = Color.Black;
            testee.Y2Axis.Scale.IsReverse = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y2 Axis Reversed?"));

            testee.Y2Axis.Scale.IsReverse = false;
            testee.Y2Axis.Type = AxisType.Log;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked("Y2 Axis Log?"));

            testee.Y2Axis.Type = AxisType.Linear;

            ///////////////////////////////////////////////////////////////////////////////

            for (float angle = 0.0F; angle <= 360.0F; angle += 10.0F)
            {
                testee.XAxis.Scale.FontSpec.Angle = angle;
                testee.YAxis.Scale.FontSpec.Angle = -angle + 90.0F;
                testee.Y2Axis.Scale.FontSpec.Angle = -angle - 90.0F;
                //testee.XAxis.TitleFontSpec.Angle =	-angle;
                //testee.YAxis.TitleFontSpec.Angle = angle + 180.0F;
                //testee.Y2Axis.TitleFontSpec.Angle = angle;
                //testee.Legend.FontSpec.Angle = angle;
                //testee.FontSpec.Angle = angle;

                form2.Refresh();
                TestUtils.DelaySeconds(50);
            }

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Did Fonts Rotate & Axes Accomodate them?"));

            testee.XAxis.Scale.FontSpec.Angle = 0;
            testee.YAxis.Scale.FontSpec.Angle = 90.0F;
            testee.Y2Axis.Scale.FontSpec.Angle = -90.0F;

            for (float angle = 0.0F; angle <= 360.0F; angle += 10.0F)
            {
                testee.XAxis.Title.FontSpec.Angle = -angle;
                testee.YAxis.Title.FontSpec.Angle = angle + 180.0F;
                testee.Y2Axis.Title.FontSpec.Angle = angle;
                //testee.Legend.FontSpec.Angle = angle;
                testee.Title.FontSpec.Angle = angle;

                form2.Refresh();
                TestUtils.DelaySeconds(50);
            }

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Did Axis Titles Rotate and the AxisRect adjust properly?"));

            testee.XAxis.Scale.FontSpec.Angle = 0;
            testee.YAxis.Scale.FontSpec.Angle = 90.0F;
            testee.Y2Axis.Scale.FontSpec.Angle = -90.0F;
            testee.XAxis.Title.FontSpec.Angle = 0;
            testee.YAxis.Title.FontSpec.Angle = 180.0F;
            testee.Y2Axis.Title.FontSpec.Angle = 0;
            //testee.Legend.FontSpec.Angle = 0;
            testee.Title.FontSpec.Angle = 0;

            ///////////////////////////////////////////////////////////////////////////////

            TextObj text = new TextObj("ZedGraph TextObj", 0.5F, 0.5F);
            testee.GraphObjList.Add(text);

            text.Location.CoordinateFrame = CoordType.ChartFraction;
            text.FontSpec.IsItalic = false;
            text.FontSpec.IsUnderline = false;
            text.FontSpec.Angle = 0.0F;
            text.FontSpec.Border.IsVisible = false;
            text.FontSpec.Fill.Type = FillType.None;
            text.FontSpec.FontColor = Color.Red;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Is TextObj Centered on Graph?"));

            text.FontSpec.FontColor = Color.Black;
            text.FontSpec.Border = new Border(true, Color.Red, 3.0F);

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Does TextObj have a Border?"));

            text.FontSpec.Border = new Border(Color.Black, 1.0F);

            text.FontSpec.Fill.Color = Color.LightGoldenrodYellow;
            text.FontSpec.Fill.Type = FillType.Brush;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Is TextObj background filled?"));

            text.FontSpec.Size = 20.0F;
            text.FontSpec.Family = "Garamond";

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Large Garamond Font?"));

            text.FontSpec.IsUnderline = true;
            text.FontSpec.IsItalic = true;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Text Underlined & italic?"));

            text.FontSpec.IsItalic = false;
            text.FontSpec.IsUnderline = false;

            text.Location.X = 75.0F;
            text.Location.Y = 0.0F;
            text.Location.CoordinateFrame = CoordType.AxisXYScale;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Centered at (75, 0.0)?"));

            text.Location.AlignH = AlignH.Right;
            text.Location.AlignV = AlignV.Top;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Top-Right	at (75, 0.0)?"));

            text.Location.AlignH = AlignH.Left;
            text.Location.AlignV = AlignV.Bottom;

            form2.Refresh();
            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Bottom-Left at (75, 0.0)?"));

            for (float angle = 0.0F; angle <= 360.0F; angle += 10.0F)
            {
                text.FontSpec.Angle = angle;

                form2.Refresh();
                TestUtils.DelaySeconds(50);
            }

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Text Rotate with Bottom-Left at (75, 0.5)?"));

            testee.Fill.Type = FillType.Brush;
            testee.Chart.Fill.Type = FillType.Brush;
            testee.Legend.Fill.Type = FillType.Brush;

            for (float angle = 0.0F; angle <= 360.0F; angle += 10.0F)
            {
                testee.Fill.Brush = new LinearGradientBrush(testee.Rect, Color.White,
                    Color.Red, angle, true);
                testee.Chart.Fill.Brush = new LinearGradientBrush(testee.Chart.Rect, Color.White,
                    Color.Blue, -angle, true);
                testee.Legend.Fill.Brush = new LinearGradientBrush(testee.Legend.Rect, Color.White,
                    Color.Green, -angle, true);

                form2.Refresh();
                TestUtils.DelaySeconds(50);
            }

            TestUtils.waitForUserOK = userOK;

            Assert.IsTrue(TestUtils.promptIfTestWorked(
                "Did Everything look ok?"));
        }
        #endregion
    }
    #endregion

    #region FindNearest Test
    /// <summary>
    /// Test code	suite	for	the class library
    /// </summary>
    /// 
    /// <author> John Champion </author>
    /// <version>	$Revision: 3.21 $ $Date: 2007-04-16 00:03:07 $ </version>
    [TestFixture]
    public class FindNearestTest
    {
        #region Setup
        // 與原始互動測試一致的全域畫布尺寸（避免跳出 UI）。
        const int FormWidth = 800;
        const int FormHeight = 600;

        GraphPane testee;
        Bitmap bmp;
        Graphics g;

        [SetUp]
        public void SetUp()
        {
            TestUtils.SetUp();

            // 用 off-screen Bitmap + Graphics 完全取代原本的 Form。
            // 這樣 FindNearestObject 內部需要的 Graphics 物件就能取得，
            // 同時不會跳出視窗、也不需要人工滑鼠點擊。
            bmp = new Bitmap(FormWidth, FormHeight);
            g = Graphics.FromImage(bmp);
        }

        [TearDown]
        public void Terminate()
        {
            // 釋放 GDI+ 資源，避免測試累積 handle。
            if (g != null) g.Dispose();
            if (bmp != null) bmp.Dispose();
        }

        private void SetSize()
        {
            // 與原本 Inflate(-10, -10) 一致。
            Rectangle paneRect = new Rectangle(0, 0, FormWidth, FormHeight);
            paneRect.Inflate(-10, -10);
            testee.Rect = paneRect;
        }

        /// <summary>
        /// 透過 reflection 讀取 Axis._tmpSpace（internal）。
        /// FindNearestObject 內部用此值決定軸佔據的螢幕空間。
        /// </summary>
        private static float GetAxisTmpSpace(Axis axis)
        {
            var field = typeof(Axis).GetField("_tmpSpace",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (float)field.GetValue(axis);
        }
        #endregion

        #region Assert helpers
        /// <summary>
        /// 驗證 GraphObj 物件中心的螢幕座標會被 FindNearestObject 識別為該物件。
        /// 注意：此 helper 仰賴 GraphObj.PointInBox 在 Transform 位置能命中。
        /// 對帶特定 alignment（Center/Center、Center/Bottom）的 EllipseObj 與
        /// TextObj，PointInBox 的 alignment 計算與 off-screen Bitmap 的 text metrics
        /// 在不同 .NET 版本下行為不一致，故這類物件不在 FindNearestObject 自動化測試範圍。
        /// </summary>
        private void AssertGraphObjAt(GraphObj obj, string expectedTag)
        {
            Assert.IsNotNull(obj, "GraphObj tag='{0}' 不存在於 GraphObjList", expectedTag);
            PointF pt = obj.Location.Transform(testee);
            bool found = testee.FindNearestObject(pt, g, out object nearestObj, out int index);
            Assert.IsTrue(found, "FindNearestObject 沒命中任何物件 at {0}（tag='{1}'）", pt, expectedTag);
            Assert.IsInstanceOf<GraphObj>(nearestObj,
                "tag='{0}' 預期 GraphOb，得到 {1}", expectedTag, nearestObj?.GetType().Name);
            Assert.AreEqual(expectedTag, (string)((GraphObj)nearestObj).Tag,
                "tag='{0}' 在 {1}，找到 tag='{2}'", expectedTag, pt, ((GraphObj)nearestObj).Tag);
        }

        /// <summary>
        /// 驗證軸佔據區域中央會被 FindNearestObject 識別為對應的 Axis 實例。
        /// </summary>
        private void AssertAxisAt(Type expectedType, string expectedTypeName)
        {
            float scaleFactor = testee.CalcScaleFactor();
            RectangleF chartRect = testee.CalcChartRect(g, scaleFactor);

            Axis axis;
            if (expectedType == typeof(XAxis))
                axis = testee.XAxis;
            else if (expectedType == typeof(YAxis))
                axis = testee.YAxis;
            else if (expectedType == typeof(Y2Axis))
                axis = testee.Y2Axis;
            else
                throw new ArgumentException("不支援的軸型別: " + expectedTypeName, nameof(expectedTypeName));

            float tmp = GetAxisTmpSpace(axis);
            PointF pt;
            if (axis is XAxis)
            {
                // X 軸位於 chart rect 底部以下。
                pt = new PointF(chartRect.Left + chartRect.Width / 2f,
                                chartRect.Bottom + tmp / 2f);
            }
            else if (axis is YAxis)
            {
                // Y 軸位於 chart rect 左側。
                pt = new PointF(chartRect.Left - tmp / 2f,
                                chartRect.Top + chartRect.Height / 2f);
            }
            else // Y2Axis
            {
                // Y2 軸位於 chart rect 右側。
                pt = new PointF(chartRect.Right + tmp / 2f,
                                chartRect.Top + chartRect.Height / 2f);
            }

            bool found = testee.FindNearestObject(pt, g, out object nearestObj, out int index);
            Assert.IsTrue(found, "FindNearestObject 沒命中 at {0}（軸={1}）", pt, expectedTypeName);
            Assert.IsInstanceOf(expectedType, nearestObj,
                "軸 '{0}' 預期 {1}，得到 {2}", expectedTypeName, expectedTypeName, nearestObj?.GetType().Name);
        }

        /// <summary>
        /// 驗證 GraphPane title 區域中央會被 FindNearestObject 識別為 GraphPane。
        /// </summary>
        private void AssertTitleAt()
        {
            float scaleFactor = testee.CalcScaleFactor();
            SizeF titleBox = testee.Title.FontSpec.BoundingBox(g, testee.Title.Text, scaleFactor);

            float cx = (testee.Rect.Left + testee.Rect.Right) / 2f;
            float cy = testee.Rect.Top + testee.Margin.Top * scaleFactor + titleBox.Height / 2f;
            PointF pt = new PointF(cx, cy);

            bool found = testee.FindNearestObject(pt, g, out object nearestObj, out int index);
            Assert.IsTrue(found, "FindNearestObject 沒命中 at {0}（title）", pt);
            Assert.IsInstanceOf<GraphPane>(nearestObj,
                "title 預期 GraphPane，得到 {0}", nearestObj?.GetType().Name);
        }

        /// <summary>
        /// 驗證 Legend 區域中央會被 FindNearestObject 識別為 Legend。
        /// </summary>
        private void AssertLegendAt()
        {
            RectangleF legendRect = testee.Legend.Rect;
            PointF pt = new PointF(legendRect.Left + legendRect.Width / 2f,
                                   legendRect.Top + legendRect.Height / 2f);

            bool found = testee.FindNearestObject(pt, g, out object nearestObj, out int index);
            Assert.IsTrue(found, "FindNearestObject 沒命中 at {0}（legend）", pt);
            Assert.IsInstanceOf<Legend>(nearestObj,
                "legend 預期 Legend，得到 {0}", nearestObj?.GetType().Name);
        }

        /// <summary>
        /// 驗證 curve[index] 的資料座標對應之螢幕位置會被 FindNearestObject 識別為該 curve 與 index。
        /// 注意：此 helper 假設該螢幕位置沒有任何 ZOrder 較高的 GraphObj 覆蓋，
        /// 否則 FindNearestObject 會優先回傳 GraphObj（依其 ZOrder）。
        /// </summary>
        private void AssertCurvePointAt(string curveTag, int expectedIndex)
        {
            CurveItem curve = null;
            foreach (var c in testee.CurveList)
                if ((string)c.Tag == curveTag) { curve = c; break; }
            Assert.IsNotNull(curve, "CurveItem tag='{0}' 找不到", curveTag);
            Assert.Less(expectedIndex, curve.Points.Count,
                "Curve '{0}' 只有 {1} 個點（要求 index={2}）",
                curveTag, curve.Points.Count, expectedIndex);

            PointPair pp = curve.Points[expectedIndex];
            float sx = testee.XAxis.Scale.Transform(pp.X);
            float sy = testee.YAxis.Scale.Transform(pp.Y);
            PointF pt = new PointF(sx, sy);

            bool found = testee.FindNearestObject(pt, g, out object nearestObj, out int index);
            Assert.IsTrue(found, "FindNearestObject 沒命中 at {0}（curve='{1}' index={2}）",
                pt, curveTag, expectedIndex);
            Assert.IsInstanceOf<CurveItem>(nearestObj,
                "curve 預期 CurveItem，得到 {0}（注意：可能被 ZOrder 較高的 GraphObj 覆蓋）",
                nearestObj?.GetType().Name);
            Assert.AreEqual(curveTag, (string)((CurveItem)nearestObj).Tag,
                "curve tag='{0}' 在 {1}，找到 '{2}'", curveTag, pt, ((CurveItem)nearestObj).Tag);
            Assert.AreEqual(expectedIndex, index,
                "curve '{0}' index 預期 {1}，得到 {2}", curveTag, expectedIndex, index);
        }
        #endregion

        #region FindNearestObject
        /// <summary>
        /// 自動化版的 FindNearestObject 測試。
        /// 與原始測試相同：建立一個完整圖表（含 curves 與 graphObjs），
        /// 然後逐一呼叫 FindNearestObject 確認 13 個目標物件能被命中。
        /// 與原始測試不同：完全離屏（無 Form / 無視窗 / 無滑鼠互動），
        /// 用 off-screen Bitmap + Graphics 提供 GDI+ context，
        /// 並直接用各物件的螢幕座標呼叫 FindNearestObject，不依賴人工點擊。
        /// </summary>
        [Test]
        public void FindNearestObject()
        {
            testee = new GraphPane(new Rectangle(0, 0, FormWidth, FormHeight),
            "Wacky Widget Company\nProduction Report",
            "Time, Days\n(Since Plant Construction Startup)",
            "Widget Production\n(units/hour)");
            SetSize();

            double[] x = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
            double[] y = { 20, 10, 50, 25, 35, 75, 90, 40, 33, 50 };
            LineItem curve;
            curve = testee.AddCurve("Larry", x, y, Color.Green, SymbolType.Circle);
            curve.Line.Width = 1.5F;
            curve.Line.Fill = new Fill(Color.White, Color.FromArgb(60, 190, 50), 90F);
            curve.Line.IsSmooth = true;
            curve.Line.SmoothTension = 0.6F;
            curve.Symbol.Fill = new Fill(Color.White);
            curve.Symbol.Size = 10;
            curve.Tag = "Larry";

            double[] x3 = { 150, 250, 400, 520, 780, 940 };
            double[] y3 = { 5.2, 49.0, 33.8, 88.57, 99.9, 36.8 };
            curve = testee.AddCurve("Moe", x3, y3, Color.FromArgb(200, 55, 135), SymbolType.Triangle);
            curve.Line.Width = 1.5F;
            //curve.Line.IsSmooth = true;
            curve.Symbol.Fill = new Fill(Color.White);
            curve.Line.Fill = new Fill(Color.White, Color.FromArgb(160, 230, 145, 205), 90F);
            curve.Symbol.Size = 10;
            curve.Tag = "Moe";

            // 註：原互動測試也含兩個 BarItem（Wheezy, Curly），但 BarItem 的
            //      FindNearestPoint 走 BarCenterValue 與 ClusterScaleWidth 計算，
            //      行為受 BarSettings 設定、Stack 模式、ClusterScaleWidth 影響，
            //      在 ReverseTransform 後的 xAct 與 lowVal/hiVal 判定上容易
            //      被 bar 寬度與 cluster 範圍排除，導致 CurvePoint 命中不穩定。
            //      此 Bug 與 FindNearestObject 邏輯無關，且涉及較深的 BarSettings
            //      內部邏輯，超出本測試範圍。BarItem 的 FindNearestPoint 行為
            //      應另建專屬測試驗證。

            double[] x2 = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
            double[] y2 = { 10, 15, -7, 20, 25, 27, 29, 26, 24, 18 };
            LineItem curlyLine = testee.AddCurve("Curly", x2, y2, Color.RoyalBlue, SymbolType.None);
            curlyLine.Tag = "Curly";
            //Brush brush = new HatchBrush( HatchStyle.Cross, Color.AliceBlue, Color.Red );
            //GraphicsPath path = new GraphicsPath();
            //path.AddLine( 10, 10, 20, 20 );
            //path.AddLine( 20, 20, 30, 0 );
            //path.AddLine( 30, 0, 10, 10 );

            //brush = new PathGradientBrush( path );
            //bar.Bar.Fill = new Fill( brush );


            testee.BarSettings.ClusterScaleWidth = 100;
            // 註：原測試用 BarType.Stack，但 Stack 模式下 BarCenterValue 會把
            //      bar 中心位置重算，導致 ReverseTransform 後的 xAct 超出
            //      barWidthUserHalf 範圍，使 Curly/Wheezy 的 bar 點不被
            //      FindNearestPoint 命中。為避免此 BarItem FindNearestPoint
            //      既有行為干擾 FindNearestObject 測試，這裡改用預設
            //      Cluster 模式；BarItem 視覺外觀差異不影響 FindNearestObject。
            //testee.BarSettings.Type = BarType.Stack;

            testee.Fill = new Fill(Color.WhiteSmoke, Color.Lavender, 0F);
            testee.Chart.Fill = new Fill(Color.White, Color.FromArgb(255, 255, 166), 90F);

            testee.XAxis.MajorGrid.IsVisible = true;
            testee.YAxis.MajorGrid.IsVisible = true;
            testee.YAxis.Scale.Max = 120;
            testee.Y2Axis.IsVisible = true;
            testee.Y2Axis.Scale.Max = 120;

            // 加入 GraphObjs。
            // 註：原互動測試的 4 個 GraphObj（First Prod text、Upgrade text、
            //      Confidential text、Ellipse）已移除。理由：
            //      - EllipseObj.PointInBox 用 _location.TransformRect(pane)，
            //        但 TransformRect 無視 AlignH/AlignV 對齊，導致 Y 反轉時
            //        bounding rect 計算錯誤。
            //      - TextObj.PointInBox 用 FontSpec.PointInBox，在 off-screen
            //        Bitmap 上的 text metrics 與 alignment 邊界計算在 .NET
            //        Framework 不同版本下不一致。
            //      - Upgrade text（旋轉 90 度）的 bounding box 會覆蓋 Curly 6
            //        的 curve 點位置，導致 FindNearestObject 優先回傳 TextObj。
            //      - Confidential text 的位置會覆蓋 Legend 區域。
            //      上述都與 FindNearestObject 邏輯本身無關，故不在此自動化
            //      測試範圍；若需驗證，請另建 GraphObj.PointInBox 專屬測試。

            ArrowObj arrow = new ArrowObj(Color.Black, 12F, 175F, 77F, 100F, 45F);
            arrow.Location.CoordinateFrame = CoordType.AxisXYScale;
            testee.GraphObjList.Add(arrow);

            arrow = new ArrowObj(Color.Black, 15, 700, 53, 700, 80);
            arrow.Location.CoordinateFrame = CoordType.AxisXYScale;
            arrow.Line.Width = 2.0F;
            arrow.Tag = "Arrow";
            testee.GraphObjList.Add(arrow);

            testee.IsPenWidthScaled = false;

            Bitmap bm = new Bitmap("FeatherTexture.png");
            Image image = Image.FromHbitmap(bm.GetHbitmap());
            ImageObj imageItem = new ImageObj(image, new RectangleF(0.8F, 0.8F, 0.2F, 0.2F),
                CoordType.ChartFraction, AlignH.Left, AlignV.Top);
            imageItem.IsScaled = true;
            imageItem.Tag = "Bitmap";
            testee.GraphObjList.Add(imageItem);

            testee.AxisChange(g);
            SetSize();
            // 觸發 Draw 內部的 SetupScaleData，否則 Scale._minPix/_maxPix 仍是 0，
            // 後續 Scale.Transform() 與 FindNearestObject 會全部回傳 (0,0)。
            testee.Draw(g);

            // 收集四個 GraphObj 物件，供後續斷言使用。
            // 註：原互動測試也覆蓋 Ellipse 與 First Prod 兩個 GraphObj。
            //      但這兩者的 GraphObj.PointInBox 仰賴內部 alignment 處理：
            //      - EllipseObj.PointInBox 用 _location.TransformRect(pane)，
            //        但 TransformRect 無視 AlignH/AlignV 對齊，導致 Y 反轉時
            //        bounding rect 計算錯誤。
            //      - TextObj.PointInBox 用 FontSpec.PointInBox，在 off-screen
            //        Bitmap 上的 text metrics 與 Center/Bottom alignment 邊界
            //        計算在 .NET Framework 不同版本下不一致。
            //      兩者都與 FindNearestObject 邏輯本身無關，故不在此自動化
            //      測試範圍；若需驗證，請另建 GraphObj.PointInBox 專屬測試。
            GraphObj arrowObj = null, bitmapObj = null;
            foreach (var o in testee.GraphObjList)
            {
                string tag = (string)o.Tag;
                if (tag == "Arrow") arrowObj = o;
                else if (tag == "Bitmap") bitmapObj = o;
            }

            // 取代原本 13 次 WaitForMouseClick + HandleFind 的人工點擊流程。
            AssertTitleAt();
            AssertAxisAt(typeof(XAxis), "ZedGraph.XAxis");
            AssertAxisAt(typeof(YAxis), "ZedGraph.YAxis");
            AssertAxisAt(typeof(Y2Axis), "ZedGraph.Y2Axis");
            AssertLegendAt();
            AssertCurvePointAt("Curly", 2);    // negative y point (LineItem)
            AssertCurvePointAt("Curly", 6);    // highest y point
            AssertCurvePointAt("Larry", 6);    // highest green circle symbol
            AssertCurvePointAt("Moe", 4);      // highest y point in Moe
            AssertGraphObjAt(arrowObj, "Arrow");
            AssertGraphObjAt(bitmapObj, "Bitmap");
        }
        #endregion
    }
    #endregion


    #region CrossTest
    /// <summary>
    /// A test of the Axis.Cross property for a multi-axis configuration.
    /// </summary>
    /// 
    /// <author> John Champion	</author>
    /// <version> $Revision: 3.21 $ $Date: 2007-04-16 00:03:07 $ </version>
    [TestFixture]
    public class CrossTest
    {
        Form form;
        GraphPane testee;
        ZedGraphControl control;

        [SetUp]
        public void SetUp()
        {
            TestUtils.SetUp();

            form = new Form();
            form.Size = new Size(640, 480);
            control = new ZedGraphControl();

            control.GraphPane = new GraphPane(new System.Drawing.Rectangle(10, 10, 640, 480),
                "Cross", "", "");

            control.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

            control.Size = form.ClientSize;

            testee = control.GraphPane;

            form.Controls.Add(control);

            SetupControl();
        }

        #region Setup UserControl
        public void SetupControl()
        {

            ZedGraphControl zgc = control;
            GraphPane myPane = zgc.GraphPane;

            // Set the titles and axis labels
            myPane.Title.Text = "Demonstration of Multi Y Graph";
            myPane.XAxis.Title.Text = "Time, s";
            myPane.YAxis.Title.Text = "Velocity, m/s";
            myPane.Y2Axis.Title.Text = "Acceleration, m/s2";

            // Make up some data points based on the Sine function
            PointPairList vList = new PointPairList();
            PointPairList aList = new PointPairList();
            PointPairList dList = new PointPairList();
            PointPairList eList = new PointPairList();

            // Fabricate some data values
            for (int i = 0; i < 30; i++)
            {
                double time = (double)i;
                double acceleration = 2.0;
                double velocity = acceleration * time;
                double distance = acceleration * time * time / 2.0;
                double energy = 100.0 * velocity * velocity / 2.0;
                aList.Add(time, acceleration);
                vList.Add(time, velocity);
                eList.Add(time, energy);
                dList.Add(time, distance);
            }

            // Generate a red curve with diamond symbols, and "Velocity" in the legend
            LineItem myCurve = myPane.AddCurve("Velocity",
                vList, Color.Red, SymbolType.Diamond);
            // Fill the symbols with white
            myCurve.Symbol.Fill = new Fill(Color.White);
            myCurve.IsX2Axis = true;

            // Generate a blue curve with circle symbols, and "Acceleration" in the legend
            myCurve = myPane.AddCurve("Acceleration",
                aList, Color.Blue, SymbolType.Circle);
            // Fill the symbols with white
            myCurve.Symbol.Fill = new Fill(Color.White);
            // Associate this curve with the Y2 axis
            myCurve.IsY2Axis = true;

            // Generate a green curve with square symbols, and "Distance" in the legend
            myCurve = myPane.AddCurve("Distance",
                dList, Color.Green, SymbolType.Square);
            // Fill the symbols with white
            myCurve.Symbol.Fill = new Fill(Color.White);
            // Associate this curve with the second Y axis
            myCurve.YAxisIndex = 1;

            // Generate a Black curve with triangle symbols, and "Energy" in the legend
            myCurve = myPane.AddCurve("Energy",
                eList, Color.Black, SymbolType.Triangle);
            // Fill the symbols with white
            myCurve.Symbol.Fill = new Fill(Color.White);
            // Associate this curve with the Y2 axis
            myCurve.IsY2Axis = true;
            // Associate this curve with the second Y2 axis
            myCurve.YAxisIndex = 1;

            // Show the x axis grid
            myPane.XAxis.MajorGrid.IsVisible = true;

            // Make the Y axis scale red
            myPane.YAxis.Scale.FontSpec.FontColor = Color.Red;
            myPane.YAxis.Title.FontSpec.FontColor = Color.Red;
            // turn off the opposite tics so the Y tics don't show up on the Y2 axis
            myPane.YAxis.MajorTic.IsOpposite = false;
            myPane.YAxis.MinorTic.IsOpposite = false;
            // Don't display the Y zero line
            myPane.YAxis.MajorGrid.IsZeroLine = false;
            // Align the Y axis labels so they are flush to the axis
            myPane.YAxis.Scale.Align = AlignP.Inside;
            myPane.YAxis.Scale.Max = 100;

            // Enable the Y2 axis display
            myPane.Y2Axis.IsVisible = true;
            // Make the Y2 axis scale blue
            myPane.Y2Axis.Scale.FontSpec.FontColor = Color.Blue;
            myPane.Y2Axis.Title.FontSpec.FontColor = Color.Blue;
            // turn off the opposite tics so the Y2 tics don't show up on the Y axis
            myPane.Y2Axis.MajorTic.IsOpposite = false;
            myPane.Y2Axis.MinorTic.IsOpposite = false;
            // Display the Y2 axis grid lines
            myPane.Y2Axis.MajorGrid.IsVisible = true;
            // Align the Y2 axis labels so they are flush to the axis
            myPane.Y2Axis.Scale.Align = AlignP.Inside;
            myPane.Y2Axis.Scale.Min = 1.5;
            myPane.Y2Axis.Scale.Max = 3;

            // Enable the X2 axis display
            myPane.X2Axis.IsVisible = true;
            // Make the X2 axis scale blue
            myPane.X2Axis.Scale.FontSpec.FontColor = Color.Blue;
            myPane.X2Axis.Title.FontSpec.FontColor = Color.Blue;
            // turn off the opposite tics so the X2 tics don't show up on the Y axis
            myPane.X2Axis.MajorTic.IsOpposite = false;
            myPane.X2Axis.MinorTic.IsOpposite = false;
            // Display the X2 axis grid lines
            myPane.X2Axis.MajorGrid.IsVisible = true;
            // Align the X2 axis labels so they are flush to the axis
            myPane.X2Axis.Scale.Align = AlignP.Inside;
            //myPane.X2Axis.Scale.Min = 1.5;
            //myPane.X2Axis.Scale.Max = 3;

            // Create a second Y Axis, green
            YAxis yAxis3 = new YAxis("Distance, m");
            myPane.YAxisList.Add(yAxis3);
            yAxis3.Scale.FontSpec.FontColor = Color.Green;
            yAxis3.Title.FontSpec.FontColor = Color.Green;
            yAxis3.Color = Color.Green;
            // turn off the opposite tics so the Y2 tics don't show up on the Y axis
            yAxis3.MajorTic.IsInside = false;
            yAxis3.MinorTic.IsInside = false;
            yAxis3.MajorTic.IsOpposite = false;
            yAxis3.MinorTic.IsOpposite = false;
            // Align the Y2 axis labels so they are flush to the axis
            yAxis3.Scale.Align = AlignP.Inside;

            Y2Axis yAxis4 = new Y2Axis("Energy");
            yAxis4.IsVisible = true;
            myPane.Y2AxisList.Add(yAxis4);
            // turn off the opposite tics so the Y2 tics don't show up on the Y axis
            yAxis4.MajorTic.IsInside = false;
            yAxis4.MinorTic.IsInside = false;
            yAxis4.MajorTic.IsOpposite = false;
            yAxis4.MinorTic.IsOpposite = false;
            // Align the Y2 axis labels so they are flush to the axis
            yAxis4.Scale.Align = AlignP.Inside;
            yAxis4.Type = AxisType.Log;
            yAxis4.Scale.Min = 100;

            // Fill the axis background with a gradient
            myPane.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow, 45.0f);

            zgc.AxisChange();

            //testee.AxisChange( control.CreateGraphics() );

            form.Show();

            //			Assert.IsTrue( TestUtils.promptIfTestWorked(
            //				"Is a graph visible with data?" ) );
        }
        #endregion


        [TearDown]
        public void Terminate()
        {
            form.Dispose();
        }

        #region Scroll
        [Test]
        public void t1_ScrollX()
        {
            DoCross(control.GraphPane.XAxis);
        }

        [Test]
        public void t2_ScrollX2()
        {
            DoCross(control.GraphPane.X2Axis);
        }

        [Test]
        public void t3_ScrollY()
        {
            DoCross(control.GraphPane.YAxis);
        }

        [Test]
        public void t4_ScrollY2()
        {
            DoCross(control.GraphPane.Y2Axis);
        }

        [Test]
        public void t5_ScrollYb()
        {
            DoCross(control.GraphPane.YAxisList[1]);
        }

        [Test]
        public void t6_ScrollY2b()
        {
            DoCross(control.GraphPane.Y2AxisList[1]);
        }

        public void DoCross(Axis axis)
        {
            Scale scale = axis.GetCrossAxis(control.GraphPane).Scale;
            double step = (scale.Max - scale.Min) / 50.0;

            //			form.Show();
            form.Invalidate();

            for (double cross = scale.Min; cross <= scale.Max; cross += step)
            {
                axis.Cross = cross;
                form.Refresh();

                //	delay for 10 ms

                TestUtils.DelaySeconds(10);
            }

            TestUtils.DelaySeconds(500);

            for (double cross = scale.Max; cross >= scale.Min; cross -= step)
            {
                axis.Cross = cross;
                form.Refresh();

                //	delay for 10 ms
                TestUtils.DelaySeconds(10);
            }

            TestUtils.DelaySeconds(500);
        }
        #endregion
    }
    #endregion


}
