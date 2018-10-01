#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   Klaus Potzesny
//
// Copyright (c) 2001-2017 empira Software GmbH, Cologne Area (Germany)
//
// http://www.pdfsharp.com
// http://www.migradoc.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.publics;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// This class is a base for all renderers.
    /// </summary>
    public abstract class RendererBase
    {
        /// <summary>
        /// Indicates whether the container contains an element
        /// that is of one of the specified types or inherited.
        /// </summary>
        /// <param name="coll">The collection to search.</param>
        /// <param name="types">The types to find within the collection.</param>
        /// <returns>True, if an object of one of the given types is found within the collection.</returns>
        internal static bool CollectionContainsObjectAssignableTo(DocumentObjectCollection coll, params Type[] types)
        {
            foreach (object obj in coll)
            {
                foreach (Type type in types)
                {
                    if (type.IsAssignableFrom(obj.GetType()))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Initializes a new instance of the RendererBase class.
        /// </summary>
        internal RendererBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the RendererBase class.
        /// </summary>
        internal RendererBase(DocumentObject domObj, RtfDocumentRenderer docRenderer)
        {
            if (_enumTranslationTable == null)
                CreateEnumTranslationTable();

            _docObject = domObj;
            _docRenderer = docRenderer;
            if (docRenderer != null)
                _rtfWriter = docRenderer.RtfWriter;
            _useEffectiveValue = false;
        }

        /// <summary>
        /// Helps translating MigraDoc DOM enumerations to an RTF control word.
        /// </summary>
        private static void CreateEnumTranslationTable()
        {
            _enumTranslationTable = new Dictionary<object, object>();
            //ParagraphAlignment
            _enumTranslationTable.Add(ParagraphAlignment.Left, "l");
            _enumTranslationTable.Add(ParagraphAlignment.Right, "r");
            _enumTranslationTable.Add(ParagraphAlignment.Center, "c");
            _enumTranslationTable.Add(ParagraphAlignment.Justify, "j");

            //LineSpacingRule
            _enumTranslationTable.Add(LineSpacingRule.AtLeast, 0);
            _enumTranslationTable.Add(LineSpacingRule.Exactly, 0);
            _enumTranslationTable.Add(LineSpacingRule.Double, 1);
            _enumTranslationTable.Add(LineSpacingRule.OnePtFive, 1);
            _enumTranslationTable.Add(LineSpacingRule.Multiple, 1);
            _enumTranslationTable.Add(LineSpacingRule.Single, 1);

            //OutLineLevel
            //BodyText rendered by leaving away rtf control word.
            _enumTranslationTable.Add(OutlineLevel.Level1, 0);
            _enumTranslationTable.Add(OutlineLevel.Level2, 1);
            _enumTranslationTable.Add(OutlineLevel.Level3, 2);
            _enumTranslationTable.Add(OutlineLevel.Level4, 3);
            _enumTranslationTable.Add(OutlineLevel.Level5, 4);
            _enumTranslationTable.Add(OutlineLevel.Level6, 5);
            _enumTranslationTable.Add(OutlineLevel.Level7, 6);
            _enumTranslationTable.Add(OutlineLevel.Level8, 7);
            _enumTranslationTable.Add(OutlineLevel.Level9, 8);

            //UnderlineType
            _enumTranslationTable.Add(Underline.Dash, "dash");
            _enumTranslationTable.Add(Underline.DotDash, "dashd");
            _enumTranslationTable.Add(Underline.DotDotDash, "dashdd");
            _enumTranslationTable.Add(Underline.Dotted, "d");
            _enumTranslationTable.Add(Underline.None, "none");
            _enumTranslationTable.Add(Underline.Single, "");
            _enumTranslationTable.Add(Underline.Words, "w");

            //BorderStyle
            _enumTranslationTable.Add(BorderStyle.DashDot, "dashd");
            _enumTranslationTable.Add(BorderStyle.DashDotDot, "dashdd");
            _enumTranslationTable.Add(BorderStyle.DashLargeGap, "dash");
            _enumTranslationTable.Add(BorderStyle.DashSmallGap, "dashsm");
            _enumTranslationTable.Add(BorderStyle.Dot, "dot");
            _enumTranslationTable.Add(BorderStyle.Single, "s");
            //BorderType.None simply not rendered.

            //TabLeader
            _enumTranslationTable.Add(TabLeader.Dashes, "hyph");
            _enumTranslationTable.Add(TabLeader.Dots, "dot");
            _enumTranslationTable.Add(TabLeader.Heavy, "th");
            _enumTranslationTable.Add(TabLeader.Lines, "ul");
            _enumTranslationTable.Add(TabLeader.MiddleDot, "mdot");
            //TabLeader.Spaces rendered by leaving away the tab leader control

            //TabAlignment
            _enumTranslationTable.Add(TabAlignment.Center, "c");
            _enumTranslationTable.Add(TabAlignment.Decimal, "dec");
            _enumTranslationTable.Add(TabAlignment.Right, "r");
            _enumTranslationTable.Add(TabAlignment.Left, "l");

            //FootnoteNumberStyle
            _enumTranslationTable.Add(FootnoteNumberStyle.Arabic, "ar");
            _enumTranslationTable.Add(FootnoteNumberStyle.LowercaseLetter, "alc");
            _enumTranslationTable.Add(FootnoteNumberStyle.LowercaseRoman, "rlc");
            _enumTranslationTable.Add(FootnoteNumberStyle.UppercaseLetter, "auc");
            _enumTranslationTable.Add(FootnoteNumberStyle.UppercaseRoman, "ruc");

            //FootnoteNumberingRule
            _enumTranslationTable.Add(FootnoteNumberingRule.RestartContinuous, "rstcont");
            _enumTranslationTable.Add(FootnoteNumberingRule.RestartPage, "rstpg");
            _enumTranslationTable.Add(FootnoteNumberingRule.RestartSection, "restart");

            //FootnoteLocation
            _enumTranslationTable.Add(FootnoteLocation.BeneathText, "tj");
            _enumTranslationTable.Add(FootnoteLocation.BottomOfPage, "bj");

            //(Section) BreakType
            _enumTranslationTable.Add(BreakType.BreakEvenPage, "even");
            _enumTranslationTable.Add(BreakType.BreakOddPage, "odd");
            _enumTranslationTable.Add(BreakType.BreakNextPage, "page");

            //TODO:  ListType under construction.
            _enumTranslationTable.Add(ListType.BulletList1, 23);
            _enumTranslationTable.Add(ListType.BulletList2, 23);
            _enumTranslationTable.Add(ListType.BulletList3, 23);
            _enumTranslationTable.Add(ListType.NumberList1, 0);
            _enumTranslationTable.Add(ListType.NumberList2, 0);
            _enumTranslationTable.Add(ListType.NumberList3, 4);

            //RowAlignment
            _enumTranslationTable.Add(RowAlignment.Center, "c");
            _enumTranslationTable.Add(RowAlignment.Left, "l");
            _enumTranslationTable.Add(RowAlignment.Right, "r");

            //VerticalAlignment
            _enumTranslationTable.Add(VerticalAlignment.Top, "t");
            _enumTranslationTable.Add(VerticalAlignment.Center, "c");
            _enumTranslationTable.Add(VerticalAlignment.Bottom, "b");

            //RelativeHorizontal
            _enumTranslationTable.Add(RelativeHorizontal.Character, "margin");
            _enumTranslationTable.Add(RelativeHorizontal.Column, "margin");
            _enumTranslationTable.Add(RelativeHorizontal.Margin, "margin");
            _enumTranslationTable.Add(RelativeHorizontal.Page, "page");

            //RelativeVertical
            _enumTranslationTable.Add(RelativeVertical.Line, "para");
            _enumTranslationTable.Add(RelativeVertical.Margin, "margin");
            _enumTranslationTable.Add(RelativeVertical.Page, "page");
            _enumTranslationTable.Add(RelativeVertical.Paragraph, "para");

            //WrapStyle
            _enumTranslationTable.Add(WrapStyle.None, 3);
            //Caution: Word imterpretates "Through" (in rtf value "5") slightly different!
            _enumTranslationTable.Add(WrapStyle.Through, 3);
            _enumTranslationTable.Add(WrapStyle.TopBottom, 1);

            //LineStyle
            _enumTranslationTable.Add(LineStyle.Single, 0);

            //DashStyle
            _enumTranslationTable.Add(DashStyle.Solid, 0);
            _enumTranslationTable.Add(DashStyle.Dash, 1);
            _enumTranslationTable.Add(DashStyle.SquareDot, 2);
            _enumTranslationTable.Add(DashStyle.DashDot, 3);
            _enumTranslationTable.Add(DashStyle.DashDotDot, 4);

            //DashStyle
            _enumTranslationTable.Add(TextOrientation.Downward, 3);
            _enumTranslationTable.Add(TextOrientation.Horizontal, 0);
            _enumTranslationTable.Add(TextOrientation.HorizontalRotatedFarEast, 0);
            _enumTranslationTable.Add(TextOrientation.Upward, 2);
            _enumTranslationTable.Add(TextOrientation.Vertical, 3);
            _enumTranslationTable.Add(TextOrientation.VerticalFarEast, 3);
        }

        /// <summary>
        /// Translates the given Unit to an RTF unit.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="rtfUnit"></param>
        /// <returns></returns>
        internal static int ToRtfUnit(Unit unit, RtfUnit rtfUnit)
        {
            switch (rtfUnit)
            {
                case RtfUnit.HalfPts:
                    return (int)(Math.Round(unit.Point * 2));
                case RtfUnit.Twips:
                    return (int)(Math.Round(unit.Point * 20));
                case RtfUnit.Lines:
                    return (int)(Math.Round(unit.Point * 12 * 20));
                case RtfUnit.EMU:
                    return (int)(Math.Round(unit.Point * 12700));
                case RtfUnit.CharUnit100:
                    return (int)(Math.Round(unit.Pica * 100));
            }
            return (int)unit.Point;
        }

        /// <summary>
        /// Translates a value named 'valueName' to a Rtf Control word that specifies a Unit, Enum, Bool, Int or Color.
        /// </summary>
        protected void Translate(string valueName, string rtfCtrl, RtfUnit unit, string defaultValue, bool withStar)
        {
            object val = GetValueAsIntended(valueName);
            if (val == null)
            {
                if (defaultValue != null)
                    _rtfWriter.WriteControl(rtfCtrl, defaultValue);
                return;
            }
            else
            {
                if (val is Unit)
                {
                    _rtfWriter.WriteControl(rtfCtrl, ToRtfUnit((Unit)val, unit), withStar);
                }
                else if (val is bool)
                {
                    if ((bool)val)
                        _rtfWriter.WriteControl(rtfCtrl, withStar);
                }
                else if (val is Color)
                {
                    int idx = _docRenderer.GetColorIndex((Color)val);
                    _rtfWriter.WriteControl(rtfCtrl, idx, withStar);
                }
                else if (val is Enum)
                {
                    _rtfWriter.WriteControl(rtfCtrl, _enumTranslationTable[val].ToString(), withStar);
                }
                else if (val is int)
                {
                    _rtfWriter.WriteControl(rtfCtrl, (int)val, withStar);
                }
                else
                    Debug.Assert(false, "Invalid use of Translate");
            }
        }

        /// <summary>
        /// Translates a value named 'valueName' to a Rtf Control word that specifies a unit, enum, bool, int or color.
        /// </summary>
        protected void Translate(string valueName, string rtfCtrl, RtfUnit unit, Unit val, bool withStar)
        {
            Translate(valueName, rtfCtrl, unit, ToRtfUnit(val, RtfUnit.Twips).ToString(), withStar);
        }

        /// <summary>
        /// Translates a value named 'valueName' to a Rtf Control word that specifies a unit, enum, bool or color.
        /// If it is a unit, twips are assumed as RtfUnit.
        /// </summary>
        protected void Translate(string valueName, string rtfCtrl)
        {
            Translate(valueName, rtfCtrl, RtfUnit.Twips, null, false);
        }

        /// <summary>
        /// Translates a value named 'valueName' to a Rtf Control word that specifies a Boolean and devides in two control words.
        /// If the control word in false case is simply left away, you can also use the Translate function as well.
        /// </summary>
        protected void TranslateBool(string valueName, string rtfTrueCtrl, string rtfFalseCtrl, bool withStar)
        {
            object val = GetValueAsIntended(valueName);
            if (val == null)
                return;
            if ((bool)val)
                _rtfWriter.WriteControl(rtfTrueCtrl, withStar);
            else if (rtfFalseCtrl != null)
                _rtfWriter.WriteControl(rtfFalseCtrl, withStar);
        }

        /// <summary>
        /// Gets the specified value either as effective value if useEffectiveValue is set to true,
        /// otherwise returns the usual GetValue or null if IsNull evaluates to true.
        /// </summary>
        protected virtual object GetValueAsIntended(string valueName)
        {
            return _docObject.GetValue(valueName, GV.GetNull);
        }

        /// <summary>
        /// Renders the given unit as control / value pair in Twips.
        /// </summary>
        protected void RenderUnit(string rtfControl, Unit value)
        {
            RenderUnit(rtfControl, value, RtfUnit.Twips, false);
        }

        /// <summary>
        /// Renders the given unit as control / value pair in the given RTF unit.
        /// </summary>
        protected void RenderUnit(string rtfControl, Unit value, RtfUnit rtfUnit)
        {
            RenderUnit(rtfControl, value, rtfUnit, false);
        }

        /// <summary>
        /// Renders the given Unit as control / value pair of the given RTF control in the given RTF unit, optionally with a star.
        /// </summary>
        protected void RenderUnit(string rtfControl, Unit value, RtfUnit rtfUnit, bool withStar)
        {
            _rtfWriter.WriteControl(rtfControl, ToRtfUnit(value, rtfUnit), withStar);
        }

        /// <summary>
        /// Converts the given Unit to Twips
        /// </summary>
        internal static int ToTwips(Unit unit)
        {
            return ToRtfUnit(unit, RtfUnit.Twips);
        }

        /// <summary>
        /// Converts the given Unit to EMU
        /// </summary>
        internal static int ToEmu(Unit unit)
        {
            return ToRtfUnit(unit, RtfUnit.EMU);
        }

        /// <summary>
        /// Renders the given object to rtf, _docObj must be of type DocumentObject or DocumentObjectContainer.
        /// </summary>
        internal abstract void Render();

        /// <summary>
        /// Returns GetValueAsIntended if this evaluates non-null, otherwise the given default value.
        /// </summary>
        protected object GetValueOrDefault(string valName, object valDefault)
        {
            object obj = GetValueAsIntended(valName);
            if (obj == null)
                return valDefault;

            return obj;
        }

        /// <summary>
        /// Renders a trailing standard paragraph in case the last element in elements isn't a paragraph.
        /// (Some RTF elements need to close with a paragraph.)
        /// </summary>
        protected void RenderTrailingParagraph(DocumentElements elements)
        {
            if (elements == null || !(elements.LastObject is Paragraph))
            {
                //At least one paragra needs to be written at the end of the document.
                //Otherwise, word cannot read the resulting rtf file.
                _rtfWriter.WriteControl("pard");
                _rtfWriter.WriteControl("s", _docRenderer.GetStyleIndex(StyleNames.Normal));
                new ParagraphFormatRenderer(_docRenderer.Document.Styles[StyleNames.Normal].ParagraphFormat, _docRenderer).Render();
                _rtfWriter.WriteControl("par");
            }
        }
        /// <summary>
        /// The DocumentObject (the current document).
        /// </summary>
        protected DocumentObject _docObject;

        /// <summary>
        /// The RtfDocumentRenderer.
        /// </summary>
        protected readonly RtfDocumentRenderer _docRenderer;

        /// <summary>
        /// The RtfWriter.
        /// </summary>
        internal RtfWriter _rtfWriter;

        /// <summary>
        /// The translation table.
        /// </summary>
        protected static Dictionary<object, object> _enumTranslationTable = null;
        
        /// <summary>
        /// Specifies whether to use the effective value.
        /// </summary>
        protected bool _useEffectiveValue;
    }
}
