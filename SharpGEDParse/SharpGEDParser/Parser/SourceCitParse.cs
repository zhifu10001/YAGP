﻿using System.Collections.Generic;
using SharpGEDParser.Model;

// ReSharper disable InconsistentNaming

namespace SharpGEDParser.Parser
{
    // Parse a set of lines for a Source Citation structure
    // Source Citations may be embedded or reference.
 
    public class SourceCitParse : StructParser
    {
        private static readonly Dictionary<string, TagProc> tagDict = new Dictionary<string, TagProc>
        {
            {"CONC", concProc}, // embedded citation
            {"CONT", contProc}, // embedded citation
            {"DATA", dataProc}, // reference citation
            {"DATE", dateProc}, // reference citation
            {"NOTE", noteProc},
            {"EVEN", eventProc}, // reference citation
            {"PAGE", pageProc}, // reference citation
            {"QUAY", quayProc}, 
            {"OBJE", objeProc},
            {"TEXT", textProc}  
        };

        private static void textProc(StructParseContext context, int linedex, char level)
        {
            string val = extendedText(context);
            SourceCit cit = (context.Parent as SourceCit);
            cit.Text.Add(val);
        }

        private static void objeProc(StructParseContext context, int linedex, char level)
        {
            MediaLink mlink = MediaStructParse.MediaParser(context, linedex, level);
            SourceCit cit = (context.Parent as SourceCit);
            cit.Media.Add(mlink);
        }

        private static void quayProc(StructParseContext context, int linedex, char level)
        {
            SourceCit cit = (context.Parent as SourceCit);
            cit.Quay = context.Remain;
        }

        private static void pageProc(StructParseContext context, int linedex, char level)
        {
            SourceCit cit = (context.Parent as SourceCit);
            cit.Page = context.Remain;
        }

        private static void eventProc(StructParseContext context, int linedex, char level)
        {
            SourceCit cit = (context.Parent as SourceCit);
            cit.Event = context.Remain;
        }

        private static void dataProc(StructParseContext context, int linedex, char level)
        {
            SourceCit cit = (context.Parent as SourceCit);
            cit.Data = true;
        }

        private static void dateProc(StructParseContext context, int linedex, char level)
        {
            SourceCit cit = (context.Parent as SourceCit);
            cit.Date = context.Remain;
        }

        private static void contProc(StructParseContext context, int linedex, char level)
        {
            SourceCit cit = (context.Parent as SourceCit);
            cit.Desc += "\n" + context.Remain;
        }

        private static void concProc(StructParseContext context, int linedex, char level)
        {
            SourceCit cit = (context.Parent as SourceCit);
            cit.Desc += context.Remain;
        }

        private static SourceCit CommonParser(ParseContextCommon ctx, int linedex, char level, List<UnkRec> errs )
        {
            SourceCit cit = new SourceCit();
            StructParseContext ctx2 = new StructParseContext(ctx, linedex, level, cit);

            string extra;
            string xref;
            parseXrefExtra(ctx.Remain, out xref, out extra);

            cit.Xref = xref;
            if (xref != null && (xref.Trim().Length == 0 || cit.Xref.Contains("@")))  // No xref is valid but not if empty/illegal
            {
                errs.Add(new UnkRec { Error = "Invalid source citation xref id" });
            }
            if (!string.IsNullOrEmpty(extra))
            {
                cit.Desc = extra;
            }

            StructParse(ctx2, tagDict);
            ctx.Endline = ctx2.Endline;

            if (!cit.Data && cit.Xref != null && cit.AnyText)
            {
                errs.Add(new UnkRec() { Error = "TEXT tag used for reference source citation" });
            }
            if (cit.Xref == null && cit.Event != null)
            {
                errs.Add(new UnkRec() { Error = "EVEN tag used for embedded source citation" });
            }
            if (cit.Xref == null && cit.Page != null)
            {
                errs.Add(new UnkRec() { Error = "PAGE tag used for embedded source citation" });
            }
            return cit;
        }

        public static SourceCit SourceCitParser(StructParseContext ctx, int linedex, char level)
        {
            List<UnkRec> errs = new List<UnkRec>();
            var cit = CommonParser(ctx, linedex, level, errs);
            // TODO where can the errors go?

            return cit;
        }

        public static SourceCit SourceCitParser(ParseContext2 ctx)
        {
            List<UnkRec> errs = new List<UnkRec>();
            var cit = CommonParser(ctx, ctx.Begline, ctx.Level, errs);
            ctx.Parent.Errors.AddRange(errs);
            return cit;
        }
    }
}
