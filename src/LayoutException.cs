#region License
/* Copyright (c) 2017-2018 Wes Hampson
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
using System.Text;
using System.Xml;
using System.Xml.Linq;
using WHampson.Cascara.Interpreter;

namespace WHampson.Cascara
{
    /// <summary>
    /// The exception that is thrown when an error occurs while processing a <see cref="LayoutScript"/>.
    /// </summary>
    public class LayoutException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="LayoutException"/> to create.</typeparam>
        /// <param name="layout">The <see cref="LayoutScript"/> that caused the exception.</param>
        /// <param name="srcElem">The <see cref="ISourceEntity"/> that caused the exception.</param>
        /// <param name="msg">A message that describes the error.</param>
        /// <returns>The new <see cref="LayoutException"/> instance.</returns>
        internal static T Create<T>(LayoutScript layout, ISourceEntity srcElem, string msg)
            where T : LayoutException
        {
            string detailedMsg = BuildDetailedMessage(msg, null, layout, srcElem);

            return CreateException<T>(layout, msg, detailedMsg, null, srcElem);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="LayoutException"/> to create.</typeparam>
        /// <param name="layout">The <see cref="LayoutScript"/> that caused the exception.</param>
        /// <param name="srcElem">The <see cref="ISourceEntity"/> that caused the exception.</param>
        /// <param name="msgFmt">A composite format string for the message that describes the error.</param>
        /// <param name="fmtArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The new <see cref="LayoutException"/> instance.</returns>
        internal static T Create<T>(LayoutScript layout, ISourceEntity srcElem, string msgFmt, params object[] fmtArgs)
            where T : LayoutException
        {
            string msg;
            string detailedMsg;

            msg = string.Format(msgFmt, fmtArgs);
            detailedMsg = BuildDetailedMessage(msg, null, layout, srcElem);

            return CreateException<T>(layout, msg, detailedMsg, null, srcElem);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="LayoutException"/> to create.</typeparam>
        /// <param name="layout">The <see cref="LayoutScript"/> that caused the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused this exception.</param>
        /// <param name="srcElem">The <see cref="ISourceEntity"/> that caused the exception.</param>
        /// <param name="msg">A message that describes the error.</param>
        /// <returns>The new <see cref="LayoutException"/> instance.</returns>
        internal static T Create<T>(LayoutScript layout, Exception innerException, ISourceEntity srcElem, string msg)
            where T : LayoutException
        {
            string detailedMsg = BuildDetailedMessage(msg, innerException, layout, srcElem);

            return CreateException<T>(layout, msg, detailedMsg, innerException, srcElem);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="LayoutException"/> to create.</typeparam>
        /// <param name="layout">The <see cref="LayoutScript"/> that caused the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused this exception.</param>
        /// <param name="srcElem">The <see cref="ISourceEntity"/> that caused the exception.</param>
        /// <param name="msgFmt">A composite format string for the message that describes the error.</param>
        /// <param name="fmtArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The new <see cref="LayoutException"/> instance.</returns>
        internal static T Create<T>(LayoutScript layout, Exception innerException, ISourceEntity srcElem, string msgFmt, params object[] fmtArgs)
            where T : LayoutException
        {
            string msg;
            string detailedMsg;

            msg = string.Format(msgFmt, fmtArgs);
            detailedMsg = BuildDetailedMessage(msg, innerException, layout, srcElem);

            return CreateException<T>(layout, msg, detailedMsg, innerException, srcElem);
        }

        /// <typeparam name="T">The type of <see cref="LayoutException"/> to create.</typeparam>
        /// <param name="layout">The <see cref="LayoutScript"/> whose XML data caused the exception.</param>
        /// <param name="msg">A brief message that describes the error.</param>
        /// <param name="detailedMsg">A detailed message that describes the error.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused this exception.</param>
        /// <param name="srcElem">The <see cref="ISourceEntity"/> that caused the exception.</param>
        /// <returns></returns>
        private static T CreateException<T>(
            LayoutScript layout, string msg, string detailedMsg, Exception innerException, ISourceEntity srcElem)
            where T : LayoutException
        {
            if (srcElem != null && srcElem.LineNumber > 0 && srcElem.LinePosition > 0)
            {
                msg += string.Format(" Line {0}, position {1}.", srcElem.LineNumber, srcElem.LinePosition);
            }

            T ex = (T) Activator.CreateInstance(
                typeof(T),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object[] { msg, innerException },
                CultureInfo.InvariantCulture);

            ex.LayoutFile = layout;

            if (!string.IsNullOrWhiteSpace(detailedMsg))
            {
                ex.DetailedMessage = detailedMsg;
            }

            if (srcElem != null)
            {
                ex.LineNumber = srcElem.LineNumber;
                ex.LinePosition = srcElem.LinePosition;
            }

            return ex;
        }

        /// <summary>
        /// Creates a descriptive and easy-to-read error message.
        /// </summary>
        /// <param name="exceptionMessage">A message describing the error.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the error.</param>
        /// <param name="layout">The <see cref="LayoutScript"/> that caused the error.</param>
        /// /// <param name="srcElem">The <see cref="ISourceEntity"/> that caused the error.</param>
        /// <returns>The newly-created error message.</returns>
        private static string BuildDetailedMessage(
            string exceptionMessage, Exception innerException, LayoutScript layout, ISourceEntity srcElem)
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
            bool hasLineInfo = srcElem != null && srcElem.LineNumber > 0 && srcElem.LinePosition > 0;

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
                msg += Environment.NewLine + "  Line: " + srcElem.LineNumber;
                msg += Environment.NewLine + "  Position: " + srcElem.LinePosition;
            }

            return msg;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        internal LayoutException()
            : base()
        {
            DetailedMessage = Message;
            LineNumber = 0;
            LinePosition = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        internal LayoutException(string message)
            : base(message)
        {
            DetailedMessage = Message;
            LineNumber = 0;
            LinePosition = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused this exception.</param>
        internal LayoutException(string message, Exception innerException)
            : base(message, innerException)
        {
            DetailedMessage = Message;
            LineNumber = 0;
            LinePosition = 0;
        }

        /// <summary>
        /// Gets a descriptive message of the error that occurred.
        /// </summary>
        public string DetailedMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="LayoutScript"/> that caused the exception.
        /// </summary>
        public LayoutScript LayoutFile
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the line number (y-coordinate) in the <see cref="LayoutScript"/> source code
        /// where the exception occurred.
        /// </summary>
        /// <remarks>
        /// If no line information was specified, this value will be 0.
        /// </remarks>
        public int LineNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the line position (x-coordinate) in the <see cref="LayoutScript"/> source code
        /// where the exception occurred.
        /// </summary>
        /// <remarks>
        /// If no line information was specified, this value will be 0.
        /// </remarks>
        public int LinePosition
        {
            get;
            private set;
        }
    }
}
