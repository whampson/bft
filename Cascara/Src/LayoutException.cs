#region License
/* Copyright (c) 2017 Wes Hampson
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;

[assembly: InternalsVisibleTo("Cascara.Tests")]

namespace WHampson.Cascara
{
    /// <summary>
    /// The exception that is thrown when an error occurs while processing a <see cref="BinaryLayout"/>.
    /// </summary>
    public class LayoutException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="LayoutException"/> to create.</typeparam>
        /// <param name="layout">The <see cref="BinaryLayout"/> whose XML data caused the exception.</param>
        /// <param name="obj">The <see cref="XObject"/> that caused the exception.</param>
        /// <param name="msg">A message that describes the error.</param>
        /// <returns>The new <see cref="LayoutException"/> instance.</returns>
        internal static T Create<T>(BinaryLayout layout, XObject obj, string msg)
            where T : LayoutException
        {
            string detailedMsg;

            GetLineInfo(obj, out int lineNum, out int linePos);
            detailedMsg = BuildDetailedMessage(msg, null, layout, lineNum, linePos);

            return CreateException<T>(layout, msg, detailedMsg, null, lineNum, linePos);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="LayoutException"/> to create.</typeparam>
        /// <param name="layout">The <see cref="BinaryLayout"/> whose XML data caused the exception.</param>
        /// <param name="obj">The <see cref="XObject"/> that caused the exception.</param>
        /// <param name="msgFmt">A composite format string for the message that describes the error.</param>
        /// <param name="fmtArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The new <see cref="LayoutException"/> instance.</returns>
        internal static T Create<T>(BinaryLayout layout, XObject obj, string msgFmt, params object[] fmtArgs)
            where T : LayoutException
        {
            string msg;
            string simpleMsg;

            GetLineInfo(obj, out int lineNum, out int linePos);

            simpleMsg = string.Format(msgFmt, fmtArgs);
            msg = BuildDetailedMessage(simpleMsg, null, layout, lineNum, linePos);

            return CreateException<T>(layout, msg, simpleMsg, null, lineNum, linePos);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="LayoutException"/> to create.</typeparam>
        /// <param name="layout">The <see cref="BinaryLayout"/> whose XML data caused the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused this exception.</param>
        /// <param name="obj">The <see cref="XObject"/> that caused the exception.</param>
        /// <param name="msg">A message that describes the error.</param>
        /// <returns>The new <see cref="LayoutException"/> instance.</returns>
        internal static T Create<T>(BinaryLayout layout, Exception innerException, XObject obj, string msg)
            where T : LayoutException
        {
            string simpleMsg;

            GetLineInfo(obj, out int lineNum, out int linePos);
            simpleMsg = msg;
            msg = BuildDetailedMessage(msg, innerException, layout, lineNum, linePos);

            return CreateException<T>(layout, msg, simpleMsg, innerException, lineNum, linePos);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="LayoutException"/> to create.</typeparam>
        /// <param name="layout">The <see cref="BinaryLayout"/> whose XML data caused the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused this exception.</param>
        /// <param name="obj">The <see cref="XObject"/> that caused the exception.</param>
        /// <param name="msgFmt">A composite format string for the message that describes the error.</param>
        /// <param name="fmtArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The new <see cref="LayoutException"/> instance.</returns>
        internal static T Create<T>(BinaryLayout layout, Exception innerException, XObject obj, string msgFmt, params object[] fmtArgs)
            where T : LayoutException
        {
            string msg;
            string simpleMsg;

            GetLineInfo(obj, out int lineNum, out int linePos);

            simpleMsg = string.Format(msgFmt, fmtArgs);
            msg = BuildDetailedMessage(simpleMsg, innerException, layout, lineNum, linePos);

            return CreateException<T>(layout, msg, simpleMsg, innerException, lineNum, linePos);
        }

        /// <summary>
        /// Factory method for creating <see cref="LayoutException"/> instances.
        /// </summary>



        /// <typeparam name="T">The type of <see cref="LayoutException"/> to create.</typeparam>
        /// <param name="layout">The <see cref="BinaryLayout"/> whose XML data caused the exception.</param>
        /// <param name="msg">A brief message that describes the error.</param>
        /// <param name="detailedMsg">A detailed message that describes the error.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused this exception.</param>
        /// <param name="lineNum">The line in the XML data on which the error occurred.</param>
        /// <param name="linePos">The line position in the XML data at which the error occurred.</param>
        /// <returns></returns>
        protected static T CreateException<T>(
            BinaryLayout layout, string msg, string detailedMsg, Exception innerException, int lineNum, int linePos)
            where T : LayoutException
        {
            T ex = (T) Activator.CreateInstance(
                typeof(T),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object[] { msg, innerException },
                CultureInfo.InvariantCulture);

            ex.LayoutFile = layout;
            ex.LineNumber = lineNum;
            ex.LinePosition = linePos;
            ex.DetailedMessage = detailedMsg;

            return ex;
        }

        /// <summary>
        /// Gets the location of an <see cref="XObject"/> in the XML file it was loaded from.
        /// </summary>
        /// <param name="obj">The <see cref="XObject"/> to get the location of.</param>
        /// <param name="lineNumber">The line number (y-coordinate) that the <see cref="XObject"/> exists on.</param>
        /// <param name="linePosition">The line position (x-coordinate) that the <see cref="XObject"/> exists on.</param>
        /// <remarks>
        /// This method only outputs a valid location if <see cref="LoadOptions.SetLineInfo"/>
        /// was enabled when loading the XML data with <see cref="XDocument.Load(string)"/>.
        /// Otherwise, this method will output 0.
        /// </remarks>
        protected static void GetLineInfo(XObject obj, out int lineNumber, out int linePosition)
        {
            if (obj == null)
            {
                lineNumber = 0;
                linePosition = 0;
                return;
            }

            IXmlLineInfo lineInfo = obj;
            lineNumber = lineInfo.LineNumber;
            linePosition = lineInfo.LinePosition;
        }

        /// <summary>
        /// Creates a descriptive, yet friendly, error message.
        /// </summary>
        /// <param name="exceptionMessage">A message describing the</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the exception that this message being built for.</param>
        /// <param name="layout">The <see cref="BinaryLayout"/> whose XML data caused the exception that this message being built for.</param>
        /// <param name="lineNumber">The line in the XML data on which the error occurred.</param>
        /// <param name="linePosition">The line position in the XML data at which the error occurred.</param>
        /// <returns>The newly-created error message.</returns>
        protected static string BuildDetailedMessage(
            string exceptionMessage, Exception innerException, BinaryLayout layout, int lineNumber, int linePosition)
        {
            if (exceptionMessage == null)
            {
                exceptionMessage = "";
            }

            exceptionMessage = exceptionMessage.Trim();

            bool hasExceptionMessage = !string.IsNullOrEmpty(exceptionMessage);
            bool hasInnerException = innerException != null;
            bool hasLayout = layout != null;
            bool hasName = hasLayout && layout.Name != null;
            bool hasPath = hasLayout && layout.SourcePath != null;
            bool hasLineInfo = lineNumber > 0 && linePosition > 0;

            string msg = "";
            if (hasExceptionMessage)
            {
                msg = exceptionMessage;
            }

            if (hasInnerException)
            {
                msg += Environment.NewLine + "Caused by:";
                msg += Environment.NewLine + "  " + innerException.GetType().Name + ": ";
                msg += innerException.Message.Replace(Environment.NewLine, Environment.NewLine + "    ");
            }

            if (hasLayout && (hasName || hasPath))
            {
                msg += Environment.NewLine + "In Layout:";
                if (hasName)
                {
                    msg += Environment.NewLine + "  Name: " + layout.Name;
                }
                if (hasPath)
                {
                    msg += Environment.NewLine + "  Path: " + layout.SourcePath;
                }
            }

            if (hasLineInfo)
            {
                msg += Environment.NewLine + "Occurred at:";
                msg += Environment.NewLine + "  Line:     " + lineNumber;
                msg += Environment.NewLine + "  Position: " + linePosition;
            }

            return msg;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        protected LayoutException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        protected LayoutException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused this exception.</param>
        protected LayoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets a descriptive message of the error that occurred.
        /// </summary>
        public string DetailedMessage
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the <see cref="LayoutException"/> that caused the exception.
        /// </summary>
        public BinaryLayout LayoutFile
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the line number in the layout XML data where the exception occurred.
        /// </summary>
        /// <remarks>
        /// Only valid if <see cref="LoadOptions.SetLineInfo"/> was enabled when loading
        /// the XML data with <see cref="XDocument.Load(string)"/>. Otherwise, this property
        /// will return 0.
        /// </remarks>
        public int LineNumber
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the line position in the layout XML data where the exception occurred.
        /// </summary>
        /// <remarks>
        /// Only valid if <see cref="LoadOptions.SetLineInfo"/> was enabled when loading
        /// the XML data with <see cref="XDocument.Load(string)"/>. Otherwise, this property
        /// will return 0.
        /// </remarks>
        public int LinePosition
        {
            get;
            protected set;
        }
    }
}
