//============================================================================
//ZedGraph Class Library - A Flexible Line Graph/Bar Graph Library in C#
//Copyright � 2006  John Champion
//
//This library is free software; you can redistribute it and/or
//modify it under the terms of the GNU Lesser General Public
//License as published by the Free Software Foundation; either
//version 2.1 of the License, or (at your option) any later version.
//
//This library is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//Lesser General Public License for more details.
//
//You should have received a copy of the GNU Lesser General Public
//License along with this library; if not, write to the Free Software
//Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

using System;
using System.Drawing;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ZedGraph
{
	/// <summary>
	/// A class that maintains hyperlink information for a clickable object on the graph.
	/// </summary>
	/// 
	/// <author> John Champion </author>
	/// <version> $Revision: 3.6 $ $Date: 2007-04-16 00:03:02 $ </version>
	// /// <seealso cref="ZedGraph.Web.IsImageMap"/>
	[Serializable]
	public class Link : ISerializable, ICloneable
	{

	#region Fields

		/// <summary>
		/// Internal field that stores the title string for this link.  
		/// </summary>
		internal string _title;

		/// <summary>
		/// Internal field that stores the url string for this link
		/// </summary>
		internal string _url;

		/// <summary>
		/// Internal field that stores the target string for this link
		/// </summary>
		internal string _target;

		/// <summary>
		/// Internal field that determines if this link is "live".
		/// </summary>
		internal bool _isEnabled;

	#endregion

	#region Properties

		/// <summary>
		/// Gets or sets the title string for this link.
		/// </summary>
		/// <remarks>
		/// For web controls, this title will be shown as a tooltip when the mouse
		/// hovers over the area of the object that owns this link.  Set the value to
		/// <see cref="String.Empty" /> to have no title.
		/// </remarks>
		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		/// <summary>
		/// Gets or sets the url string for this link.
		/// </summary>
		/// <remarks>
		/// Set this value to <see cref="String.Empty" /> if you don't want to have
		/// a hyperlink associated with the object to which this link belongs.
		/// </remarks>
		public string Url
		{
			get { return _url; }
			set { _url = value; }
		}

		/// <summary>
		/// Gets or sets the target string for this link.
		/// </summary>
		/// <remarks>
		/// This value should be set to a valid target associated with the "Target"
		/// property of an html hyperlink.  Typically, this would be "_blank" to open
		/// a new browser window, or "_self" to open in the current browser.
		/// </remarks>
		public string Target
		{
			get { return _target != string.Empty ? _target : "_self"; }
			set { _target = value; }
		}

		/// <summary>
		/// A tag object for use by the user.  This can be used to store additional
		/// information associated with the <see cref="Link"/>.  ZedGraph does
		/// not use this value for any purpose.
		/// </summary>
		/// <remarks>
		/// Note that, if you are going to Serialize ZedGraph data, then any type
		/// that you store in <see cref="Tag"/> must be a serializable type (or
		/// it will cause an exception).
		/// </remarks>
		public object Tag;

		/// <summary>
		/// Gets or sets a property that determines if this link is active.  True to have
		/// a clickable link, false to ignore the link.
		/// </summary>
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set { _isEnabled = value; }
		}

		/// <summary>
		/// Gets a value that indicates if this <see cref="Link" /> is enabled
		/// (see <see cref="IsEnabled" />), and that either the
		/// <see cref="Url" /> or the <see cref="Title" /> is non-null.
		/// </summary>
		public bool IsActive
		{
			get { return _isEnabled && ( _url != null || _title != null ); }
		}

	#endregion

	#region Constructors

		/// <summary>
		/// Default constructor.  Set all properties to string.Empty, or null.
		/// </summary>
		public Link()
		{
			_title = string.Empty;
			_url = string.Empty;
			_target = string.Empty;
			this.Tag = null;
			_isEnabled = false;
		}

		/// <summary>
		/// Construct a Link instance from a specified title, url, and target.
		/// </summary>
		/// <param name="title">The title for the link (which shows up in the tooltip).</param>
		/// <param name="url">The URL destination for the link.</param>
		/// <param name="target">The target for the link (typically "_blank" or "_self").</param>
		public Link( string title, string url, string target )
		{
			_title = title;
			_url = url;
			_target = target;
			Tag = null;
			_isEnabled = true;
		}

		/// <summary>
		/// The Copy Constructor
		/// </summary>
		/// <param name="rhs">The <see cref="Link"/> object from which to copy</param>
		public Link( Link rhs )
		{
			// Copy value types
			_title = rhs._title;
			_url = rhs._url;
			_target = rhs._target;
			_isEnabled = false;

			// copy reference types by cloning
			if ( rhs.Tag is ICloneable )
				this.Tag = ((ICloneable) rhs.Tag).Clone();
			else
				this.Tag = rhs.Tag;
		}

		/// <summary>
		/// Implement the <see cref="ICloneable" /> interface in a typesafe manner by just
		/// calling the typed version of <see cref="Clone" />
		/// </summary>
		/// <returns>A deep copy of this object</returns>
		object ICloneable.Clone()
		{
			return this.Clone();
		}

		/// <summary>
		/// Typesafe, deep-copy clone method.
		/// </summary>
		/// <returns>A new, independent copy of this class</returns>
		public Link Clone()
		{
			return new Link( this );
		}


	#endregion

	#region methods

		/// <summary>
		/// Create a URL for a <see cref="CurveItem" /> that includes the index of the
		/// point that was selected.
		/// </summary>
		/// <remarks>
		/// An "index" parameter is added to the <see cref="Url" /> property for this
		/// link to indicate which point was selected.  Further, if the 
		/// X or Y axes that correspond to this <see cref="CurveItem" /> are of
		/// <see cref="AxisType.Text" />, then an
		/// additional parameter will be added containing the text value that
		/// corresponds to the <paramref name="index" /> of the selected point.
		/// The <see cref="XAxis" /> text parameter will be labeled "xtext", and
		/// the <see cref="YAxis" /> text parameter will be labeled "ytext".
		/// </remarks>
		/// <param name="index">The zero-based index of the selected point</param>
		/// <param name="pane">The <see cref="GraphPane" /> of interest</param>
		/// <param name="curve">The <see cref="CurveItem" /> for which to
		/// make the url string.</param>
		/// <returns>A string containing the url with an index parameter added.</returns>
		public virtual string MakeCurveItemUrl( GraphPane pane, CurveItem curve, int index )
		{
			string url = _url;

			if ( url.IndexOf( '?' ) >= 0 )
				url += "&index=" + index.ToString();
			else
				url += "?index=" + index.ToString();

			Axis xAxis = curve.GetXAxis( pane );
			if (	xAxis.Type == AxisType.Text && index >= 0 &&
					xAxis.Scale.TextLabels != null &&
					index <= xAxis.Scale.TextLabels.Length )
				url += "&xtext=" + xAxis.Scale.TextLabels[index];

			Axis yAxis = curve.GetYAxis( pane );
			if (	yAxis != null && yAxis.Type == AxisType.Text && index >= 0 &&
					yAxis.Scale.TextLabels != null &&
					index <= yAxis.Scale.TextLabels.Length )
				url += "&ytext=" + yAxis.Scale.TextLabels[index];

			return url;
		}

		/// <summary>
		/// 判斷 url 是否為可安全導航的絕對 URL（H7 安全修復）。
		/// </summary>
		/// <remarks>
		/// <see cref="ZedGraphControl"/> 在點擊連結時以 <c>Process.Start(url)</c> 開啟 url，
		/// 而 url 可能來自資料或曲線標籤（外部可控）。若不限制 scheme，惡意的
		/// <c>file:///</c>、<c>javascript:</c> 或任意已註冊的 protocol handler 皆可能被觸發
		/// （CWE-601 / URL scheme 注入）。本方法以 scheme 白名單把關，僅允許 http/https/mailto。
		/// 非 null/空字串、相對路徑、純檔案路徑等非絕對 URL 一律回傳 false。
		/// </remarks>
		/// <param name="url">待檢驗的 URL 字串。</param>
		/// <returns>scheme 為 http/https/mailto 的絕對 URL 則為 true，否則 false。</returns>
		public static bool IsSafeUrl( string url )
		{
			// 空值或空白直接拒絕
			if ( string.IsNullOrEmpty( url ) )
				return false;

			// 僅接受「絕對」URI；相對路徑或純檔案路徑無法構成安全的導航目標
			Uri uri;
			if ( !Uri.TryCreate( url, UriKind.Absolute, out uri ) )
				return false;

			// scheme 白名單（小寫比對，大小寫不拘）
			string scheme = uri.Scheme.ToLowerInvariant();
			return scheme == "http" || scheme == "https" || scheme == "mailto";
		}
	#endregion

	#region Serialization

		/// <summary>
		/// Current schema value that defines the version of the serialized file
		/// </summary>
		/// <remarks>
		/// schema started with 10 for ZedGraph version 5
		/// </remarks>
		public const int schema = 10;

		/// <summary>
		/// Constructor for deserializing objects
		/// </summary>
		/// <param name="info">A <see cref="SerializationInfo"/> instance that defines the serialized data
		/// </param>
		/// <param name="context">A <see cref="StreamingContext"/> instance that contains the serialized data
		/// </param>
		protected Link( SerializationInfo info, StreamingContext context )
		{
			// The schema value is just a file version parameter.  You can use it to make future versions
			// backwards compatible as new member variables are added to classes
			int sch = info.GetInt32( "schema" );

			_title = info.GetString( "title" );
			_url = info.GetString( "url" );
			_target = info.GetString( "target" );
			_isEnabled = info.GetBoolean( "isEnabled" );
			Tag = info.GetValue( "Tag", typeof(object) );
		}
		/// <summary>
		/// Populates a <see cref="SerializationInfo"/> instance with the data needed to serialize the target object
		/// </summary>
		/// <param name="info">A <see cref="SerializationInfo"/> instance that defines the serialized data</param>
		/// <param name="context">A <see cref="StreamingContext"/> instance that contains the serialized data</param>
		[SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
		public virtual void GetObjectData( SerializationInfo info, StreamingContext context )
		{
			info.AddValue( "schema", schema );
			info.AddValue( "title", _title );
			info.AddValue( "url", _url );
			info.AddValue( "target", _target );
			info.AddValue( "isEnabled", _isEnabled );
			info.AddValue( "Tag", Tag );
		}

	#endregion

	}
}