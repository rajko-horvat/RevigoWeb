using IRB.Collections.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace RevigoWeb
{
	public enum SpinnerTypeEnum
	{
		Random,
		Spinner1,
		Spinner2,
		Spinner3,
		Spinner4,
		Spinner5,
		Spinner6,
		Spinner7,
		Spinner8,
		Spinner9,
		UnfoldingCube
	}

	[DefaultProperty("SpinnerType")]
	[Designer("System.Web.UI.Design.WebControls.PreviewControlDesigner, System.Design")]
	public partial class SpinnerControl : WebControl
	{
		public SpinnerControl() : base(HtmlTextWriterTag.Div)
		{ }

		protected override void OnLoad(EventArgs e)
		{
			HtmlLink cssRef = new HtmlLink();
			cssRef.Href = Page.Request.ApplicationPath + "css/SpinnerControl.css";
			cssRef.Attributes["rel"] = "stylesheet";
			//cssRef.Attributes["type"] = "text/css";
			Page.Header.Controls.Add(cssRef);

			base.OnLoad(e);
		}

		[Category("Appearance")]
		[DefaultValue(SpinnerTypeEnum.Random)]
		[Description("The type of the spinner")]
		public virtual SpinnerTypeEnum SpinnerType
		{
			get
			{
				object obj = this.ViewState[nameof(SpinnerType)];
				return obj != null ? (SpinnerTypeEnum)obj : SpinnerTypeEnum.Random;
			}
			set
			{
				//if (value < SpinnerTypeEnum.Random || value > SpinnerTypeEnum.Spinner10)
				//throw new ArgumentOutOfRangeException(nameof(value));
				this.ViewState[nameof(SpinnerType)] = value;
			}
		}

		public override void RenderControl(HtmlTextWriter writer)
		{
			SpinnerTypeEnum eType = this.SpinnerType;
			int iSpinner = 0;

			
			if (eType == SpinnerTypeEnum.Random)
			{
				iSpinner = Math.Abs(Environment.TickCount) % 10;
			}
			else
			{
				iSpinner = (int)eType - 1;
			}

			switch (iSpinner)
			{
				case 1:
					writer.WriteLine("<div class=\"SpinnerControl_Loader01\">");
					writer.WriteLine("<div class='block'>");
					writer.WriteLine("<div class='item'></div>");
					writer.WriteLine("<div class='item'></div>");
					writer.WriteLine("<div class='item'></div>");
					writer.WriteLine("<div class='item'></div>");
					writer.WriteLine("<div class='item'></div>");
					writer.WriteLine("<div class='item'></div>");
					writer.WriteLine("<div class='item'></div>");
					writer.WriteLine("<div class='item'></div>");
					writer.WriteLine("</div>");
					writer.WriteLine("</div>");
					break;
				case 2:
					writer.WriteLine("<svg class=\"SpinnerControl_Loader02\" viewBox=\"0 0 280 280\" fill=\"none\">");
					writer.WriteLine("<g>");
					writer.WriteLine("<line x1=\"60\" y1=\"140\" x2=\"220\" y2=\"140\" stroke=\"#000\" stroke-width=\"4\"/>");
					writer.WriteLine("<circle cx=\"60\" cy=\"140\" r=\"5\" fill=\"#000\"/>");
					writer.WriteLine("<circle cx=\"220\" cy=\"140\" r=\"5\" fill=\"#000\"/>");
					writer.WriteLine("</g>");
					writer.WriteLine("<g>");
					writer.WriteLine("<path d=\"M109.957 122.655L140 105.309L170.043 122.655V157.345L140 174.691L109.957 157.345V122.655Z\" stroke=\"#000\" stroke-width=\"4\"/>");
					writer.WriteLine("<circle cx=\"140\" cy=\"140\" r=\"13\" stroke=\"#f5d77b\" stroke-width=\"4\"/>");
					writer.WriteLine("<circle cx=\"110\" cy=\"192\" r=\"13\" stroke=\"#f7a78f\" stroke-width=\"4\"/>");
					writer.WriteLine("<circle cx=\"170\" cy=\"88\" r=\"13\" stroke=\"#82c7c5\" stroke-width=\"4\"/>");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 140 140\" to=\"360 140 140\" begin=\"0s\" dur=\"6s\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</g>");
					writer.WriteLine("<g>");
					writer.WriteLine("<circle cx=\"85\" cy=\"232\" r=\"8\" stroke=\"#82c7c5\" stroke-width=\"4\"/>");
					writer.WriteLine("<circle cx=\"110\" cy=\"192\" r=\"5\" fill=\"#f7a78f\"/>");
					writer.WriteLine("<circle cx=\"185\" cy=\"61\" r=\"5\" fill=\"#f5d77b\"/>");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 140 140\" to=\"360 140 140\" begin=\"0s\" dur=\"3s\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</g>");
					writer.WriteLine("</svg>");
					break;
				case 3:
					writer.WriteLine("<svg class=\"SpinnerControl_Loader03\" viewBox=\"0 0 100 100\">");
					writer.WriteLine("<circle class=\"spinner\" style=\"fill:none;stroke:#dd2476; stroke-width:0.6rem; stroke-linecap: round;\" cx=\"50\" cy=\"50\" r=\"45\"/>");
					writer.WriteLine("</svg>");
					break;
				case 4:
					writer.WriteLine("<div class=\"SpinnerControl_Loader04\">");
					writer.WriteLine("<div class=\"arc\"></div>");
					writer.WriteLine("<div class=\"arc\"></div>");
					writer.WriteLine("<div class=\"arc\"></div>");
					writer.WriteLine("</div>");
					break;
				case 5:
					writer.WriteLine("<svg class=\"SpinnerControl_Loader05\" viewBox=\"0 0 100 100\">");
					writer.WriteLine("<g>");
					writer.WriteLine("<circle style=\"fill:black;opacity:1\" cx=\"50\" cy=\"50\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.98\" cx=\"50.24\" cy=\"50.97\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.96\" cx=\"50.94\" cy=\"51.77\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.93\" cx=\"52.01\" cy=\"52.23\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.91\" cx=\"53.32\" cy=\"52.24\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.89\" cx=\"54.7\" cy=\"51.71\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.87\" cx=\"55.97\" cy=\"50.63\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.84\" cx=\"56.93\" cy=\"49.03\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.82\" cx=\"57.42\" cy=\"47\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.8\" cx=\"57.28\" cy=\"44.71\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.78\" cx=\"56.43\" cy=\"42.34\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.76\" cx=\"54.82\" cy=\"40.11\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.73\" cx=\"52.49\" cy=\"38.26\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.71\" cx=\"49.55\" cy=\"37.01\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.69\" cx=\"46.14\" cy=\"36.54\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.67\" cx=\"42.5\" cy=\"37.01\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.64\" cx=\"38.89\" cy=\"38.49\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.62\" cx=\"35.58\" cy=\"40.99\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.6\" cx=\"32.88\" cy=\"44.44\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.58\" cx=\"31.05\" cy=\"48.67\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.56\" cx=\"30.3\" cy=\"53.47\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.53\" cx=\"30.82\" cy=\"58.54\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.51\" cx=\"32.66\" cy=\"63.54\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.49\" cx=\"35.84\" cy=\"68.12\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.47\" cx=\"40.24\" cy=\"71.93\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.44\" cx=\"45.66\" cy=\"74.62\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.42\" cx=\"51.81\" cy=\"75.94\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.4\" cx=\"58.34\" cy=\"75.68\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.38\" cx=\"64.84\" cy=\"73.75\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.36\" cx=\"70.86\" cy=\"70.15\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.33\" cx=\"75.98\" cy=\"65\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.31\" cx=\"79.8\" cy=\"58.54\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.29\" cx=\"81.98\" cy=\"51.12\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.27\" cx=\"82.28\" cy=\"43.14\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.24\" cx=\"80.56\" cy=\"35.1\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.22\" cx=\"76.81\" cy=\"27.5\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.2\" cx=\"71.16\" cy=\"20.88\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.18\" cx=\"63.86\" cy=\"15.69\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.16\" cx=\"55.29\" cy=\"12.37\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.13\" cx=\"45.92\" cy=\"11.21\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.11\" cx=\"36.32\" cy=\"12.41\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.09\" cx=\"27.07\" cy=\"16.01\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.07\" cx=\"18.79\" cy=\"21.9\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.04\" cx=\"12.03\" cy=\"29.81\" r=\"0.2rem\"/>");
					writer.WriteLine("<circle style=\"fill:black;opacity:0.02\" cx=\"7.31\" cy=\"39.36\" r=\"0.2rem\"/>");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 50 50\" to=\"360 50 50\" begin=\"0s\" dur=\"1.4s\" repeatCount=\"indefinite\"></animateTransform>");
					writer.WriteLine("</g>");
					writer.WriteLine("</svg>");
					break;
				case 6:
					writer.WriteLine("<div class=\"SpinnerControl_Loader06\">");
					writer.WriteLine("<div class=\"chase\">");
					writer.WriteLine("<div class=\"chase-dot\"></div>");
					writer.WriteLine("<div class=\"chase-dot\"></div>");
					writer.WriteLine("<div class=\"chase-dot\"></div>");
					writer.WriteLine("<div class=\"chase-dot\"></div>");
					writer.WriteLine("<div class=\"chase-dot\"></div>");
					writer.WriteLine("<div class=\"chase-dot\"></div>");
					writer.WriteLine("</div>");
					writer.WriteLine("</div>");
					break;
				case 7:
					writer.WriteLine("<svg class=\"SpinnerControl_Loader07\" viewBox=\"0 0 200 200\">");
					writer.WriteLine("<circle cx=\"40\" cy=\"100\" r=\"20\" fill=\"black\">");
					writer.WriteLine("<animate attributeName=\"r\" begin=\"-0.32s\" values=\"0;20;0\" keyTimes=\"0;0.5;1\" calcMode=\"spline\" keySplines=\"0.5 0 0.5 1;0.5 0 0.5 1\" dur=\"1.4s\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"100\" cy=\"100\" r=\"20\" fill=\"black\">");
					writer.WriteLine("<animate attributeName=\"r\" begin=\"-0.16s\" values=\"0;20;0\" keyTimes=\"0;0.5;1\" calcMode=\"spline\" keySplines=\"0.5 0 0.5 1;0.5 0 0.5 1\" dur=\"1.4s\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"160\" cy=\"100\" r=\"20\" fill=\"black\">");
					writer.WriteLine("<animate attributeName=\"r\" begin=\"0s\" values=\"0;20;0\" keyTimes=\"0;0.5;1\" calcMode=\"spline\" keySplines=\"0.5 0 0.5 1;0.5 0 0.5 1\" dur=\"1.4s\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("</svg>");
					break;
				case 8:
					writer.WriteLine("<svg class=\"SpinnerControl_Loader08\" viewBox=\"0 0 200 200\">");
					writer.WriteLine("<circle cx=\"12\" cy=\"100\" r=\"12\" fill=\"#324650\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"38\" cy=\"100\" r=\"10\" fill=\"#324650\" fill-opacity=\"0.8\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.1s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"60\" cy=\"100\" r=\"8\" fill=\"#324650\" fill-opacity=\"0.6\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.2s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"78\" cy=\"100\" r=\"6\" fill=\"#324650\" fill-opacity=\"0.4\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.3s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"92\" cy=\"100\" r=\"4\" fill=\"#324650\" fill-opacity=\"0.2\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.4s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"105\" cy=\"100\" r=\"4\" fill=\"#489D8A\" fill-opacity=\"0.2\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.4s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"119\" cy=\"100\" r=\"6\" fill=\"#489D8A\" fill-opacity=\"0.4\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.3s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"137\" cy=\"100\" r=\"8\" fill=\"#489D8A\" fill-opacity=\"0.6\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.2s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"159\" cy=\"100\" r=\"10\" fill=\"#489D8A\" fill-opacity=\"0.8\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.1s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"185\" cy=\"100\" r=\"12\" fill=\"#489D8A\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"100\" cy=\"12\" r=\"12\" fill=\"#E8A961\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"100\" cy=\"38\" r=\"10\" fill=\"#E8A961\" fill-opacity=\"0.8\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.1s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"100\" cy=\"60\" r=\"8\" fill=\"#E8A961\" fill-opacity=\"0.6\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.2s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"100\" cy=\"78\" r=\"6\" fill=\"#E8A961\" fill-opacity=\"0.4\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.3s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"100\" cy=\"92\" r=\"4\" fill=\"#E8A961\" fill-opacity=\"0.2\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.4s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"100\" cy=\"105\" r=\"4\" fill=\"#D34B4C\" fill-opacity=\"0.2\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.4s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"100\" cy=\"119\" r=\"6\" fill=\"#D34B4C\" fill-opacity=\"0.4\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.3s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"100\" cy=\"137\" r=\"8\" fill=\"#D34B4C\" fill-opacity=\"0.6\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.2s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"100\" cy=\"159\" r=\"10\" fill=\"#D34B4C\" fill-opacity=\"0.8\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0.1s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("<circle cx=\"100\" cy=\"185\" r=\"12\" fill=\"#D34B4C\">");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 100 100\" to=\"360 100 100\" begin=\"0s\" dur=\"2.2s\" calcMode=\"spline\" keySplines=\"1 0 .25 .25\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</circle>");
					writer.WriteLine("</svg>");
					break;
				case 9:
					writer.WriteLine("<div class=\"SpinnerControl_Loader09-folding-cube\">");
					writer.WriteLine("<div class=\"cube\"></div>");
					writer.WriteLine("<div class=\"cube2 cube\"></div>");
					writer.WriteLine("<div class=\"cube4 cube\"></div>");
					writer.WriteLine("<div class=\"cube3 cube\"></div>");
					writer.WriteLine("</div>");
					break;
				default:
					writer.WriteLine("<svg class=\"SpinnerControl_Loader00\" viewBox=\"0 0 100 100\">");
					writer.WriteLine("<g>");
					writer.WriteLine("<circle style=\"fill:transparent;stroke:#0052ec; stroke-width:0.6rem; stroke-linecap: round; stroke-dasharray: 71 71;\" cx=\"50\" cy=\"50\" r=\"45\"/>");
					writer.WriteLine("<animateTransform attributeName=\"transform\" attributeType=\"XML\" type=\"rotate\" from=\"0 50 50\" to=\"360 50 50\" begin=\"0s\" dur=\"1.4s\" repeatCount=\"indefinite\"/>");
					writer.WriteLine("</g>");
					writer.WriteLine("</svg>");
					break;
			}

			base.RenderControl(writer);
		}
	}

	public class CustomStyle : Style
	{
		BDictionary<string, string> aDictionary = new BDictionary<string, string>();

		/// <summary>Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.Style" /> class using default values.</summary>
		public CustomStyle() : base((StateBag)null)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.Style" /> class with the specified state bag information.</summary>
		/// <param name="bag">A <see cref="T:System.Web.UI.StateBag" /> that represents the state bag in which to store style information. </param>
		public CustomStyle(StateBag bag) : base(bag)
		{
		}

		protected override void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver)
		{
			for (int i = 0; i < this.aDictionary.Count; i++)
			{
				BKeyValuePair<string, string> item = aDictionary[i];

				attributes[item.Key] = item.Value;
			}

			base.FillStyleAttributes(attributes, urlResolver);
		}

		public override void Reset()
		{
			this.aDictionary.Clear();
			base.Reset();
		}

		public void Add(string name, string value)
		{
			aDictionary.Add(name, value);
		}

		public void Remove(string name)
		{
			if (aDictionary.ContainsKey(name))
				aDictionary.RemoveByKey(name);
		}

		public int Count
		{
			get
			{
				return aDictionary.Count;
			}
		}
	}
}